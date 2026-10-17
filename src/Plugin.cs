using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HideFriendMapPings
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class HideFriendMapPingsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "adrirf7.HideFriendMapPings";
        public const string ModName = "HideFriendMapPings";
        public const string ModVersion = "1.0.0";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> ShowSharedPins;
        internal static ConfigEntry<bool> ShowSharedMap;
        internal static ConfigEntry<bool> ShowOwnPins;
        internal static ConfigEntry<Vector2> TogglesOffset;

        private void Awake()
        {
            Log = Logger;

            ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "HideFriendMapPings.adrirf7.cfg"), true, Info.Metadata);

            ShowSharedPins = config.Bind("Map", "ShowSharedPins", true,
                "true = pins that other players shared with you through the cartography table are shown on the map. " +
                "false = they are hidden. Also changed with the checkbox on the large map.");
            ShowSharedMap = config.Bind("Map", "ShowSharedMap", true,
                "true = map areas explored by other players (read from the cartography table) are shown. " +
                "false = only the areas you explored yourself are shown. Also changed with the checkbox on the large map.");
            ShowOwnPins = config.Bind("Map", "ShowOwnPins", true,
                "true = your own pins (the ones you placed, your tombstones and discovered bosses) are shown on the map. " +
                "false = they are hidden. Also changed with the checkbox on the large map.");
            TogglesOffset = config.Bind("Map", "TogglesOffset", Vector2.zero,
                "Moves the checkboxes of the large map by this many pixels (x = right, y = up).");

            ShowSharedPins.SettingChanged += (sender, args) => MapToggles.OnSettingsChanged();
            ShowSharedMap.SettingChanged += (sender, args) => MapToggles.OnSettingsChanged();
            ShowOwnPins.SettingChanged += (sender, args) => MapToggles.OnSettingsChanged();
            TogglesOffset.SettingChanged += (sender, args) => MapToggles.UpdatePositions();

            new Harmony(ModGuid).PatchAll();
            Translations.Register(Localization.instance, Localization.instance.GetSelectedLanguage());
            Log.LogInfo(ModName + " " + ModVersion + " loaded.");
        }
    }
}
