# HideFriendMapPings

Your friends shared their map at the cartography table and now yours is full of their pins and areas you've never been to? **HideFriendMapPings** adds two checkboxes to the large map so you can hide what came from the table and see only your own pins and your own exploration, then bring it all back with one click.

![The two new checkboxes on the large map](https://raw.githubusercontent.com/adrirf7/HideFriendMapPings/main/docs/map_toggles.png)

## Features

- **Shared pins:** show or hide the pins other players shared with you through the cartography table. Your own pins are always shown.
- **Shared map:** show or hide the areas other players explored. Only the areas you explored yourself stay uncovered, on the large map and on the minimap.
- **Independent:** each checkbox works on its own. The game's own "Cartography Table" checkbox, which hides both at once, still works as before.
- **Nothing is deleted:** it only changes what you see. Tick the box again and everything is back.
- **Remembered:** both checkboxes keep their state between sessions.
- **Safe:** hidden pins can't be removed by accident with a click.
- **Translated:** follows the game language (14 languages, English for the rest).
- **Client-side only:** other players and the server don't need it.

## How to use

1. Open the large map (**M**).
2. In the bottom-right corner, above **Visible to other players**, use the **Shared pins** and **Shared map** checkboxes.

## Configuration

`BepInEx/config/HideFriendMapPings.adrirf7.cfg`:

| Setting | Default | Description |
|---|---|---|
| `ShowSharedPins` | `true` | Show the pins shared through the cartography table. |
| `ShowSharedMap` | `true` | Show the map areas explored by other players. |
| `TogglesOffset` | `0, 0` | Moves the two checkboxes by this many pixels (x = right, y = up). |

## Installation

Install with r2modman / Thunderstore Mod Manager, or manually: put `HideFriendMapPings.dll` in `BepInEx/plugins/HideFriendMapPings/`.

## Source code

[github.com/adrirf7/HideFriendMapPings](https://github.com/adrirf7/HideFriendMapPings)
