using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace StockpileSelector
{
    [StaticConstructorOnStartup]
    public static class StockpileSelectorStartup
    {
        static StockpileSelectorStartup()
        {
            var harmony = new Harmony("com.ishot.stockpileselector");
            harmony.PatchAll();
            Log.Message("Stockpile Selector mod initialized");
        }
    }

    // Store the linked stockpile data
    public class StockpileReference : IExposable
    {
        public Zone_Stockpile stockpile;

        public void ExposeData()
        {
            Scribe_References.Look(ref stockpile, "linkedStockpile");
        }
    }

    // Patch the Bill class to add our stockpile reference
    [HarmonyPatch(typeof(Bill), "ExposeData")]
    public static class Bill_ExposeData_Patch
    {
        private static readonly Dictionary<Bill, StockpileReference> stockpileRefs = new Dictionary<Bill, StockpileReference>();

        public static StockpileReference GetStockpileRef(this Bill bill)
        {
            StockpileReference reference;
            if (!stockpileRefs.TryGetValue(bill, out reference))
            {
                reference = new StockpileReference();
                stockpileRefs[bill] = reference;
            }
            return reference;
        }

        [HarmonyPostfix]
        public static void Postfix(Bill __instance)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                var stockpileRef = __instance.GetStockpileRef();
                stockpileRef.ExposeData();
            }
            else if (Scribe.mode == LoadSaveMode.Saving)
            {
                StockpileReference stockpileRef;
                if (stockpileRefs.TryGetValue(__instance, out stockpileRef))
                {
                    stockpileRef.ExposeData();
                }
            }
        }
    }

    // Patch the Bill_Production.DoConfigInterface method to add our UI controls
    [HarmonyPatch(typeof(Bill_Production), "DoConfigInterface")]
    public static class Bill_Production_DoConfigInterface_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Bill_Production __instance, Rect baseRect, Color baseColor)
        {
            var filter = StockpileFilter.GetFilterForBill(__instance);

            // Create a button to select a stockpile
            Rect stockpileRect = new Rect(baseRect.x, baseRect.yMax + 4f, baseRect.width, 24f);
            string stockpileLabel = filter.allowedStockpiles.Any()
                ? "Pull from: " + string.Join(", ", filter.allowedStockpiles.Select(s => s.label))
                : "Select stockpile to pull from";
            
            if (Widgets.ButtonText(stockpileRect, stockpileLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                
                options.Add(new FloatMenuOption("Any stockpile (default)", delegate
                {
                    filter.enabled = false;
                    filter.allowedStockpiles.Clear();
                }));
                
                foreach (Zone_Stockpile stockpile in Find.CurrentMap.zoneManager.AllZones.OfType<Zone_Stockpile>())
                {
                    string checkMark = filter.allowedStockpiles.Contains(stockpile) ? "✓ " : "";
                    options.Add(new FloatMenuOption(checkMark + stockpile.label, delegate
                    {
                        if (filter.allowedStockpiles.Contains(stockpile))
                            filter.allowedStockpiles.Remove(stockpile);
                        else
                        {
                            filter.enabled = true;
                            filter.allowedStockpiles.Add(stockpile);
                        }
                    }));
                }
                
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }

    // Patch the WorkGiver_DoBill.TryFindBestBillIngredients method
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("TryFindBestBillIngredients")]
    public class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
    {
        public static bool Prefix(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients, ref bool __result)
        {
            // Get the StockpileFilter for this bill
            var filter = StockpileFilter.GetFilterForBill(bill);
            if (filter == null || !filter.enabled) return true;

            // Get all stockpiles that are allowed
            var allowedStockpiles = filter.allowedStockpiles;
            if (allowedStockpiles == null || allowedStockpiles.Count == 0) return true;

            // Get all things from allowed stockpiles
            var relevantThings = allowedStockpiles
                .SelectMany(s => s.AllContainedThings)
                .Where(t => bill.recipe.ingredients.Any(i => i.filter.Allows(t)))
                .ToList();

            // If we found no valid ingredients, let the original method run
            if (!relevantThings.Any())
            {
                __result = false;
                return false;
            }

            // Process each ingredient requirement
            foreach (var ingredient in bill.recipe.ingredients)
            {
                var availableThings = relevantThings
                    .Where(t => ingredient.filter.Allows(t))
                    .OrderBy(t => t.Position.DistanceToSquared(billGiver.Position))
                    .ToList();
                
                if (!availableThings.Any())
                {
                    missingIngredients?.Add(ingredient);
                    continue;
                }
                
                var needed = ingredient.GetBaseCount();
                foreach (var thing in availableThings)
                {
                    var toTake = (int)Math.Min(needed, thing.stackCount);
                    chosen.Add(new ThingCount(thing, toTake));
                    needed -= toTake;
                    
                    if (needed <= 0) break;
                }
                
                if (needed > 0)
                {
                    missingIngredients?.Add(ingredient);
                }
            }
            
            __result = chosen.Any();
            return false;
        }
    }

    public class StockpileSelectorSettings : ModSettings
    {
        public bool enableDebugLogging = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableDebugLogging, "enableDebugLogging", false);
        }
    }

    public class StockpileSelectorMod : Mod
    {
        public static StockpileSelectorSettings settings;

        public StockpileSelectorMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<StockpileSelectorSettings>();
        }

        public override string SettingsCategory()
        {
            return "Stockpile Selector";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            
            listing.CheckboxLabeled("Enable debug logging", ref settings.enableDebugLogging);
            
            listing.End();
            
            base.DoSettingsWindowContents(inRect);
        }
    }

    public class StockpileFilter
    {
        private static readonly Dictionary<Bill, StockpileFilter> filters = new Dictionary<Bill, StockpileFilter>();
        
        public bool enabled = false;
        public List<Zone_Stockpile> allowedStockpiles = new List<Zone_Stockpile>();

        public static StockpileFilter GetFilterForBill(Bill bill)
        {
            if (!filters.TryGetValue(bill, out var filter))
            {
                filter = new StockpileFilter();
                filters[bill] = filter;
            }
            return filter;
        }
    }
}