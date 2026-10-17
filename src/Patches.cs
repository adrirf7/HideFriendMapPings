using HarmonyLib;

namespace HideFriendMapPings
{
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
    internal static class Minimap_Start_Patch
    {
        private static void Postfix(Minimap __instance)
        {
            MapToggles.Create(__instance);
        }
    }

    /// <summary>Después de que el juego ajuste el fundido del mapa compartido, se aplica la casilla del mapa.</summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Update))]
    internal static class Minimap_Update_Patch
    {
        private static void Postfix(Minimap __instance)
        {
            MapToggles.ApplySharedMapFade(__instance);
        }
    }

    /// <summary>
    /// UpdatePins ya oculta los marcadores con dueño (los que vienen de la mesa) cuando el fundido compartido es 0.
    /// Mientras dibuja se pone a 0 si la casilla de marcadores está desactivada, y luego se restaura, así el mapa
    /// compartido no se ve afectado. Los marcadores ocultos no tienen icono, por lo que tampoco se pueden borrar sin querer.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]
    internal static class Minimap_UpdatePins_Patch
    {
        private static void Prefix(Minimap __instance, out float __state)
        {
            __state = __instance.m_sharedMapDataFade;
            if (!HideFriendMapPingsPlugin.ShowSharedPins.Value)
            {
                __instance.m_sharedMapDataFade = 0f;
            }
        }

        private static void Postfix(Minimap __instance, float __state)
        {
            __instance.m_sharedMapDataFade = __state;
        }
    }
}
