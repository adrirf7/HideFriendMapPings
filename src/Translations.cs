using System.Collections.Generic;
using HarmonyLib;

namespace HideFriendMapPings
{
    /// <summary>
    /// Textos del mod. Se registran en el sistema de localización de Valheim, así que siguen el idioma
    /// elegido en el juego (también al cambiarlo en Ajustes). Inglés para cualquier idioma sin traducción.
    /// </summary>
    internal static class Translations
    {
        private const string DefaultLanguage = "English";

        // Orden de las claves; cada idioma da sus textos en este mismo orden.
        private static readonly string[] s_keys =
        {
            "hfmp_shared_pins", "hfmp_shared_map"
        };

        // idioma -> textos (mismo orden que s_keys)
        private static readonly Dictionary<string, string[]> s_languages = new Dictionary<string, string[]>
        {
            ["English"] = new[] { "Shared pins", "Shared map" },
            ["Spanish"] = new[] { "Marcadores compartidos", "Mapa compartido" },
            ["German"] = new[] { "Geteilte Markierungen", "Geteilte Karte" },
            ["French"] = new[] { "Marqueurs partagés", "Carte partagée" },
            ["Italian"] = new[] { "Segnalini condivisi", "Mappa condivisa" },
            ["Portuguese_Brazilian"] = new[] { "Marcadores compartilhados", "Mapa compartilhado" },
            ["Portuguese_European"] = new[] { "Marcadores partilhados", "Mapa partilhado" },
            ["Russian"] = new[] { "Общие метки", "Общая карта" },
            ["Ukrainian"] = new[] { "Спільні мітки", "Спільна мапа" },
            ["Polish"] = new[] { "Udostępnione znaczniki", "Udostępniona mapa" },
            ["Chinese"] = new[] { "共享标记", "共享地图" },
            ["Chinese_Trad"] = new[] { "共享標記", "共享地圖" },
            ["Japanese"] = new[] { "共有ピン", "共有マップ" },
            ["Korean"] = new[] { "공유된 핀", "공유된 지도" },
        };

        internal static void Register(Localization localization, string language)
        {
            if (!s_languages.TryGetValue(language, out string[] texts))
            {
                texts = s_languages[DefaultLanguage];
            }
            for (int i = 0; i < s_keys.Length; i++)
            {
                localization.AddWord(s_keys[i], texts[i]);
            }
        }
    }

    [HarmonyPatch(typeof(Localization), nameof(Localization.SetupLanguage))]
    internal static class Localization_SetupLanguage_Patch
    {
        private static void Postfix(Localization __instance, string language)
        {
            Translations.Register(__instance, language);
            MapToggles.RefreshTexts();
        }
    }
}
