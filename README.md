# Stockpile Selector

A RimWorld mod that enhances bill management by allowing you to select specific stockpiles to pull ingredients from, rather than just using the vanilla radius setting.

## Features

- Select specific stockpiles for bills to pull resources from
- Compatible with vanilla stockpile system
- Adds a new button to the bill interface
- Maintains compatibility with other mods that modify the bill system
- Settings to enable/disable debug logging

## Installation

### Steam Workshop
[Subscribe on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3434050137)

### Manual Installation
1. Download the latest release
2. Extract the contents into your RimWorld mods folder:
   - Windows: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods`
3. Enable the mod in RimWorld's mod menu
4. Make sure Harmony is loaded before this mod

## Development

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (for building)
- [Git Bash](https://git-scm.com/downloads) (for development environment)
- [Make](https://www.gnu.org/software/make/) (comes with Git Bash)
- Visual Studio 2022 or Visual Studio Code (recommended)
- RimWorld 1.5
- [Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077) mod

### Dependencies

- .NET Framework 4.7.2
- Harmony 2.2.2
- RimWorld Assembly
- UnityEngine Assemblies

### Building

The project uses a Makefile for building and installation. Run these commands from Git Bash:

    # Build and install
    make

    # Just build
    make build

    # Remove the mod
    make uninstall

    # Clean and rebuild everything
    make rebuild

### Deployment

#### Automatic Deployment via GitHub Actions

The project uses two GitHub workflows:
1. **Build** - Runs on every push and PR
   - Builds the mod
   - Creates build artifacts

2. **Deploy to Steam Workshop** - Manual trigger only
   - Uses latest successful build artifacts
   - Creates GitHub release with specified version
   - Publishes to Steam Workshop

To deploy a new version:

1. Trigger deployment:
   - Go to GitHub repository Actions tab
   - Select "Deploy to Steam Workshop"
   - Click "Run workflow"
   - Enter version number (e.g. "1.0.0")
   - Click "Run workflow"
   - Approve the deployment when prompted
   - Enter Steam Guard code when requested

#### Required Secrets

The following secrets need to be set in GitHub repository settings:
- `STEAM_USERNAME`: Your Steam account username
- `STEAM_PASSWORD`: Your Steam account password
- `STEAM_WORKSHOP_ID`: The mod's Steam Workshop ID (3434050137)

### Project Structure

    StockpileSelector/
    ├── About/
    │   ├── About.xml          # Mod metadata
    │   └── PublishedFileId.txt # Steam Workshop ID
    ├── Assemblies/
    │   └── StockpileSelector.dll
    ├── Source/
    │   └── StockpileSelector/
    │       ├── Properties/
    │       ├── StockpileSelector.cs
    │       └── StockpileSelector.csproj
    ├── .github/
    │   └── workflows/
    │       ├── build.yml      # Build workflow
    │       └── deploy.yml     # Deploy workflow
    ├── Makefile              # Build automation
    └── README.md            # This file

### Development Notes

- Uses modern .NET SDK-style project structure
- Targets .NET Framework 4.7.2 for RimWorld compatibility
- Uses latest C# language features
- Harmony patches for:
  - Bill.ExposeData
  - Bill_Production.DoConfigInterface
  - WorkGiver_DoBill.TryFindBestBillIngredients

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

MIT License - see LICENSE file for details

## Credits

- Created by iShot
- Built for RimWorld by Ludeon Studios
- Uses Harmony by Andreas Pardeike

## Troubleshooting

### Common Issues

1. **Mod not appearing in RimWorld:**
   - Verify Harmony is installed and loaded first
   - Check the RimWorld log for errors (Ctrl+F12 in game)

2. **Build errors:**
   - Ensure all prerequisites are installed
   - Check RimWorld installation path in StockpileSelector.csproj
   - Run `make clean` and try rebuilding

3. **Runtime errors:**
   - Enable debug logging in mod settings
   - Check `Player.log` in RimWorld's log directory

### Getting Help

- Create an issue on GitHub
- Check RimWorld's modding forums
- Review the [RimWorld modding wiki](https://rimworldwiki.com/wiki/Modding_Tutorials)
