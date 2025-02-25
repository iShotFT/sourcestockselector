# Configuration
RIMWORLD_MODS_DIR=/c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld/Mods
MOD_NAME=StockpileSelector
HARMONY_DLL=Assemblies/0Harmony.dll

# Get the full path of the current directory
MOD_DIR=$(shell pwd)

# Variables
RIMWORLD_DIR := /c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld
MANAGED_DIR := $(RIMWORLD_DIR)/RimWorldWin64_Data/Managed

.PHONY: build install clean link setup-references

# Default target
all: version build clean-harmony install

# Version management
version:
	@echo "Checking for version update..."
	@chmod +x $(MOD_DIR)/version.sh
	@bash $(MOD_DIR)/version.sh

# Build the mod
build: setup-references
	@echo "Building mod..."
	@cmd //c "build.bat & exit"

# Remove Harmony DLL if it exists
clean-harmony:
	@echo "Cleaning up Harmony DLL..."
	@rm -f $(HARMONY_DLL) 2>/dev/null || true

# Create symbolic link if it doesn't exist
install:
	@echo "Installing mod..."
	@if [ ! -L $(RIMWORLD_MODS_DIR)/$(MOD_NAME) ]; then \
		cmd //c mklink //D $(RIMWORLD_MODS_DIR)/$(MOD_NAME) $(MOD_DIR); \
	else \
		echo "Mod already installed"; \
	fi

# Remove the symbolic link
uninstall:
	@echo "Uninstalling mod..."
	@rm -f $(RIMWORLD_MODS_DIR)/$(MOD_NAME) 2>/dev/null || true

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@rm -rf Source/$(MOD_NAME)/obj 2>/dev/null || true
	@rm -rf Source/$(MOD_NAME)/bin 2>/dev/null || true
	@rm -f $(HARMONY_DLL) 2>/dev/null || true
	@rm -rf References

# Rebuild everything from scratch
rebuild: clean all

setup-references:
	mkdir -p References
	cp $(MANAGED_DIR)/Assembly-CSharp.dll References/
	cp $(MANAGED_DIR)/UnityEngine.dll References/
	cp $(MANAGED_DIR)/UnityEngine.CoreModule.dll References/
	cp $(MANAGED_DIR)/UnityEngine.IMGUIModule.dll References/
	cp $(MANAGED_DIR)/UnityEngine.TextRenderingModule.dll References/ 