#!/bin/bash

# Steam Workshop upload script
WORKSHOP_ID="YOUR_WORKSHOP_ID"
CONTENT_PATH="$GITHUB_WORKSPACE"

# Install steamcmd
wget https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz
tar -xvzf steamcmd_linux.tar.gz

# Create Steam config directory
mkdir -p ~/.steam/steam/config

# Write Steam Guard file
echo "$STEAM_GUARD_FILE" > ~/.steam/steam/config/guard.txt

# First login to cache credentials
./steamcmd.sh +login $STEAM_USERNAME $STEAM_PASSWORD +quit

# Upload to workshop
./steamcmd.sh +workshop_build_item $WORKSHOP_ID $CONTENT_PATH +quit 