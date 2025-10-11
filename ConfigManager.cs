using BepInEx.Configuration;
using System.Linq;
using UnityEngine;

namespace MegabonkBetterMinimap {
    internal class ConfigManager {
        internal static ConfigManager Instance { get; set; }
        private static ConfigFile Cfg { get; set; }
        internal static ConfigEntry<float> CurrentScale { get; set; }
        internal static ConfigEntry<int> CurrentZoom { get; set; }
        internal static ConfigEntry<int> CurrentFullZoom { get; set; }
        internal static ConfigEntry<int> ZoomStepping { get; private set; }
        internal static ConfigEntry<KeyCode> ScaleMinimapHotkey { get; private set; }
        internal static ConfigEntry<KeyCode> ZoomMapHotkey { get; private set; }
        internal static ConfigEntry<KeyCode> ToggleFullMapHotkey { get; private set; }
        internal static ConfigEntry<KeyCode> ToggleStatsHotkey { get; private set; }
        internal static ConfigEntry<KeyCode> HideProjectilesHotkey { get; private set; }
        internal static ConfigEntry<KeyCode> InstantResetHotkey { get; private set; }
        internal ConfigManager(ConfigFile config) {
            Instance = this;
            Cfg = config;
            Cfg.SaveOnConfigSet = true;

            CurrentScale = Cfg.Bind("Minimap", "CurrentScale", 1.0f, "Current minimap scale.\nThis value is updated automatically when changed in game.");
            CurrentZoom = Cfg.Bind("Minimap", "CurrentZoom", 100, "Current normal zoom\nThis value is updated automatically when changed in game.");
            CurrentFullZoom = Cfg.Bind("Minimap", "CurrentFullZoom", 300, "Current full/minimap zoom\nThis value is updated automatically when changed in game.");
            ZoomStepping = Cfg.Bind("Minimap", "Zoom Stepping", 5, new ConfigDescription("Adjust the increment when zooming the map." +
                "\nHigher values change zoom faster.", new AcceptableValueRange<int>(5, 25)));

            ToggleFullMapHotkey = Cfg.Bind("Hotkeys", "Toggle full map", KeyCode.M, "Press this key to open the full map view. While the full map is opened, the game is paused.");
            ScaleMinimapHotkey = Cfg.Bind("Hotkeys", "Minimap scale", KeyCode.F1, "Press this key to scale up the minimap in the top right corner of the screen.\nHold shift to scale down.");
            ZoomMapHotkey = Cfg.Bind("Hotkeys", "Map zoom", KeyCode.F2, "Press this key to zoom out the map itself both in minimap and full map view.\nHold shift to zoom in.");
            ToggleStatsHotkey = Cfg.Bind("Hotkeys", "Toggle stats", KeyCode.T, "Press this key toggle the statistics panel.");
            HideProjectilesHotkey = Cfg.Bind("Hotkeys", "Hide projectiles", KeyCode.F3, "Press this key to hide projectiles and effects. Useful in god runs.");
            InstantResetHotkey = Cfg.Bind("Hotkeys", "Instant reset", KeyCode.P, "Press this key to instantly reset the run.");
        }

        internal static void Refresh() {
            if (Instance == null) return;
            Cfg.Reload();
        }
    }
}
