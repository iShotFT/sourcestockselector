#!/bin/bash

echo "Setting up SteamCMD..."

# Get the project root directory (parent of the directory containing this script)
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$SCRIPT_DIR"

echo "Project root directory: $PROJECT_ROOT"

# Create steamcmd directory if it doesn't exist
STEAMCMD_DIR="$HOME/steamcmd"
mkdir -p "$STEAMCMD_DIR"
cd "$STEAMCMD_DIR"

# Download SteamCMD if not present
if [ ! -f "steamcmd.exe" ]; then
    echo "Downloading SteamCMD..."
    curl -L -o steamcmd.zip https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip
    unzip -o steamcmd.zip
    rm steamcmd.zip
fi

# Create Steam directories
mkdir -p ~/.steam/steam/config

echo "Please enter your Steam credentials:"
read -p "Username: " STEAM_USER
read -s -p "Password: " STEAM_PASS
echo

echo "Logging in to Steam..."
./steamcmd.exe +login "$STEAM_USER" "$STEAM_PASS" +quit

# Wait a bit for the files to be created
sleep 2

# Try both possible locations for the config file
CONFIG_PATHS=(
    "/c/Users/$USERNAME/Steam/config/config.vdf"
    "/c/Program Files (x86)/Steam/config/config.vdf"
)

for CONFIG_PATH in "${CONFIG_PATHS[@]}"; do
    if [ -f "$CONFIG_PATH" ]; then
        echo "Found Steam config at: $CONFIG_PATH"
        cp "$CONFIG_PATH" ~/.steam/steam/config/guard.txt
        # Save to project root with absolute path
        cp "$CONFIG_PATH" "$PROJECT_ROOT/steamguard.txt"
        echo "Attempting to save to: $PROJECT_ROOT/steamguard.txt"
        if [ -f "$PROJECT_ROOT/steamguard.txt" ]; then
            echo "Successfully saved Steam Guard file"
        else
            echo "Failed to save Steam Guard file"
        fi
        echo "Steam Guard file contents:"
        cat ~/.steam/steam/config/guard.txt
        echo
        echo "Steam Guard file has been saved to steamguard.txt"
        echo "Remember to add this as a GitHub secret and then delete the file!"
        exit 0
    fi
done

echo "Error: Could not find Steam config file"
exit 1

# Steam Workshop upload script
WORKSHOP_ID="2009463077"  # Replace with your actual Workshop ID 