# HideFriendMapPings

A BepInEx mod for Valheim that adds two checkboxes to the large map: one shows / hides the pins other players shared with you through the cartography table, and the other shows / hides the map areas they explored, leaving only what you explored yourself. Nothing is deleted, it only changes what is drawn. Client-side only.

[Versión en español](README.es.md)

![The two new checkboxes on the large map](docs/map_toggles.png)

## Features

- **Shared pins** checkbox: hides the pins that have an owner (the ones read from the cartography table). Your own pins are always shown.
- **Shared map** checkbox: hides the fog cleared by other players, on the large map and on the minimap.
- Both are independent from each other and from the game's own "Cartography Table" checkbox (which hides both at once).
- State is saved in the config file and kept between sessions.
- In-game text follows the game language (14 languages, English by default).

## How it works

The code only uses Harmony patches and Unity components. It does not access the network or any files outside the game process, except for writing its own config file.

- `src/MapToggles.cs`: clones the game's "Visible to other players" checkbox twice and places the copies above it. Hides the shared map by forcing the map shader's `_SharedFade` to 0.
- `src/Patches.cs`: `Minimap.Start` creates the checkboxes, `Minimap.Update` applies the shared map fade, and `Minimap.UpdatePins` sets the shared fade to 0 while drawing pins, which makes the game skip pins with an owner.
- `src/Translations.cs`: registers the mod's in-game text with Valheim's localization.
- `src/Plugin.cs`: BepInEx entry point and configuration.

## Building

Requirements:

- [.NET SDK 8](https://dotnet.microsoft.com/download) or newer
- Valheim with [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) installed (the project references the game's and BepInEx's assemblies from the install folder)

Steps:

```
git clone <this repository>
cd HideFriendMapPings
dotnet build -c Release -p:ValheimDir="C:\Program Files (x86)\Steam\steamapps\common\Valheim" -p:DeployToGame=false
```

The compiled mod is `bin/Release/HideFriendMapPings.dll`. `ValheimDir` defaults to the Steam path above. Without `-p:DeployToGame=false`, the build also copies the DLL to `BepInEx/plugins/HideFriendMapPings/` in that Valheim install.

A Release build also creates the upload packages in `release/`:

- `HideFriendMapPings-<version>-nexus.zip`: `BepInEx/plugins/HideFriendMapPings/HideFriendMapPings.dll`, to extract into the Valheim folder.
- `HideFriendMapPings-<version>-thunderstore.zip`: `manifest.json`, `README.md`, `icon.png` and the DLL, from the `thunderstore/` folder.

The Nexus Mods page texts are in `nexus/`: `summary.txt` (short summary) and `description.bbcode` (long description).

The NuGet package `BepInEx.AssemblyPublicizer.MSBuild` is restored automatically. It lets the code access non-public members of `assembly_valheim.dll` at compile time, which is standard practice for Valheim mods.

## Installation

1. Install BepInExPack Valheim.
2. Put `HideFriendMapPings.dll` in `BepInEx/plugins/HideFriendMapPings/`.
3. Only the player who wants it needs the mod.
