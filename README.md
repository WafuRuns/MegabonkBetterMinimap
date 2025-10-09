# MegabonkBetterMinimap

(Much more than a minimap mod now)

A BepInEx 6 mod for Megabonk that allows you to resize and zoom out the minimap, open the full map, adds new icons, instant reset, hiding enemies and projectiles, map rating, pickup screen etc.

## Research project

-   Since the project now uses map rating to tell the player how good the map potentially is, it would be great to have it backed up by data.
-   Currently, everytime your run ends, the game outputs a line of numbers into the consile, you can copy it by highlighting it and pretting right mouse button.
-   This data can be then pasted into a Google Sheet by pasting it into the first column and pressing Data -> Split text to columns.
-   [Google Sheets template to clone](https://docs.google.com/spreadsheets/d/1g9hjzonZ7EyAw2dnt1n__JMNKmneXhfFAec5CB_US2U/edit?usp=sharing)
-   Share you spreadsheet on Discord: wafuruns

# Installation

-   Install [BepInEx 6 Bleeding Edge IL2CPP](https://builds.bepinex.dev/projects/bepinex_be) for your game (tested on BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.738+af0cba7)
-   Download the [latest release of MegabonkBetterMinimap](https://github.com/WafuRuns/MegabonkBetterMinimap/releases/download/1.4.1/MegabonkBetterMinimap.dll)
-   Paste MegabonkBetterMinimap.dll into Megabonk\BepInEx\plugins folder
-   **I strongly recommend going into the game settings and setting "Upload score to Leaderboards" to Off.** This mod is an accessibility and comfort feature, not something to give you an unintended advantage in competition. Feel free to consult the game dev.

# Controls

-   `F1`: Increases the minimap size
-   `F2`: Zooms out the minimap
-   `F3`: Hide particles and enemies (lag reduction for god runs)
-   `M`: Opens/closes the full map
-   `T`: Show remaining pickups screen
-   `P`: Instantly reset map

# Features

-   Changing the minimap size
-   Changing the minimap zoom
-   Provides full map
-   Changes to map icons
    -   Everything important has a specific icon
    -   Icons for Microwave and Shady Guy have rarity colors
-   Remaining pickups screen (including rarities and overall map rating)
-   Improved map resolution
-   Toggle for turning off enemy models and particles, helpful for infinite runs to reduce lag
-   Instant reset key
-   Map rating

# Configuration

Configuration is automatically saved based on your last settings, but you can also edit it manually.

```toml
[Minimap]

## Current minimap scale
# Setting type: Single
# Default value: 1
CurrentScale = 3.4999988

## Current normal zoom
# Setting type: Int32
# Default value: 100
CurrentZoom = 280

## Current full/minimap zoom
# Setting type: Int32
# Default value: 300
CurrentFullZoom = 300
```

# TODO

-   Fullscreen controllable map
-   Fog of war

![Opened map](https://github.com/user-attachments/assets/552c3649-a2a6-43f6-a1d7-cff6f0a10b86)
![Minimap](https://github.com/user-attachments/assets/db8de967-48f2-44cc-92e6-f5fd08b318d4)
![Stats screen](https://github.com/user-attachments/assets/cdcb184d-07fc-4e62-9319-4203b1251202)
