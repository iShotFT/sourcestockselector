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
            var stockpileRef = __instance.GetStockpileRef();

            // Create a button to select a stockpile
            Rect stockpileRect = new Rect(baseRect.x, baseRect.yMax + 4f, baseRect.width, 24f);
            string stockpileLabel = stockpileRef.stockpile != null 
                ? "Pull from: " + stockpileRef.stockpile.label 
                : "Select stockpile to pull from";
            
            if (Widgets.ButtonText(stockpileRect, stockpileLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                
                options.Add(new FloatMenuOption("Any stockpile (default)", delegate
                {
                    stockpileRef.stockpile = null;
                }));
                
                foreach (Zone_Stockpile stockpile in Find.CurrentMap.zoneManager.AllZones.OfType<Zone_Stockpile>())
                {
                    options.Add(new FloatMenuOption(stockpile.label, delegate
                    {
                        stockpileRef.stockpile = stockpile;
                    }));
                }
                
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }

    // Patch the WorkGiver_DoBill.TryFindBestBillIngredients method
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    public static class WorkGiver_DoBill_TryFindBestBillIngredients_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, ref bool __result, ref List<Thing> relevantThings)
        {
            var stockpileRef = bill.GetStockpileRef();
            if (stockpileRef.stockpile == null)
            {
                return true; // Use vanilla logic
            }

            // Filter things to only those in our selected stockpile
            relevantThings = relevantThings.Where(t => t.Position.GetZone(pawn.Map) == stockpileRef.stockpile).ToList();
            
            if (!relevantThings.Any())
            {
                __result = false;
                return false;
            }

            return true; // Continue with vanilla logic using filtered list
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
}