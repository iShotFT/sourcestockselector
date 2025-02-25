#!/bin/bash

# File paths
VERSION_FILE="Source/StockpileSelector/Properties/AssemblyInfo.cs"
ABOUT_FILE="About/About.xml"

# Function to check if any source files have changed
check_changes() {
    if git rev-parse --verify HEAD >/dev/null 2>&1 && git diff --quiet HEAD Source/; then
        return 1  # No changes
    else
        return 0  # Changes detected
    fi
}

# Function to bump version
bump_version() {
    # Get current version from AssemblyInfo.cs
    CURRENT_VERSION=$(grep "AssemblyVersion" "$VERSION_FILE" | grep -o '".*"' | tr -d '"')
    
    # Split version into parts
    IFS='.' read -r MAJOR MINOR PATCH BUILD <<< "$CURRENT_VERSION"
    
    # Handle wildcards or missing parts
    [[ $PATCH == "*" ]] && PATCH=0
    [[ -z $BUILD ]] && BUILD=0
    
    # Increment build number
    BUILD=$((BUILD + 1))
    
    # New version
    NEW_VERSION="$MAJOR.$MINOR.$PATCH.$BUILD"
    
    echo "Bumping version from $CURRENT_VERSION to $NEW_VERSION"
    
    # Update AssemblyInfo.cs
    sed -i "s/AssemblyVersion(\".*\")/AssemblyVersion(\"$NEW_VERSION\")/" "$VERSION_FILE"
    sed -i "s/AssemblyFileVersion(\".*\")/AssemblyFileVersion(\"$NEW_VERSION\")/" "$VERSION_FILE"
    
    # Update About.xml
    sed -i "s/<targetVersion>.*<\/targetVersion>/<targetVersion>$MAJOR.$MINOR<\/targetVersion>/" "$ABOUT_FILE"
    
    # Stage the modified files
    git add "$VERSION_FILE" "$ABOUT_FILE"
    
    echo "Version updated to $NEW_VERSION"
}

# Main script
if check_changes; then
    bump_version
fi 