using System.Collections.Generic;
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

    /// <summary>
    /// Después de que el juego ajuste el fundido del mapa compartido, se aplica la casilla del mapa y se vuelve a
    /// ocultar la casilla "Mesa de cartografía" del juego, que se activa sola al haber datos compartidos.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Update))]
    internal static class Minimap_Update_Patch
    {
        private static void Postfix(Minimap __instance)
        {
            MapToggles.ApplySharedMapFade(__instance);
            MapToggles.HideGameSharedToggle(__instance);
        }
    }

    /// <summary>
    /// Mientras el juego dibuja los marcadores, la lista se cambia por una sin los marcadores ocultos (y se les
    /// quita el icono); al terminar se devuelve la lista original, así no se pierde ni se guarda nada distinto.
    /// Los marcadores ocultos no tienen icono, por lo que tampoco se pueden borrar sin querer.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]
    internal static class Minimap_UpdatePins_Patch
    {
        private static readonly List<Minimap.PinData> s_visiblePins = new List<Minimap.PinData>();

        private static void Prefix(Minimap __instance, out List<Minimap.PinData> __state)
        {
            __state = null;
            if (HideFriendMapPingsPlugin.ShowSharedPins.Value && HideFriendMapPingsPlugin.ShowOwnPins.Value)
            {
                return;
            }
            s_visiblePins.Clear();
            foreach (Minimap.PinData pin in __instance.m_pins)
            {
                if (IsHidden(__instance, pin))
                {
                    __instance.DestroyPinMarker(pin);
                }
                else
                {
                    s_visiblePins.Add(pin);
                }
            }
            __state = __instance.m_pins;
            __instance.m_pins = s_visiblePins;
        }

        private static void Postfix(Minimap __instance, List<Minimap.PinData> __state)
        {
            if (__state != null)
            {
                __instance.m_pins = __state;
                s_visiblePins.Clear();
            }
        }

        /// <summary>
        /// Compartidos: los que tienen dueño (vienen de la mesa). Propios: los guardados sin dueño (los que pones,
        /// tumbas, jefes descubiertos...). Los temporales del juego (jugadores, pings, eventos, cama) no se tocan,
        /// ni el marcador al que se le está escribiendo el nombre.
        /// </summary>
        private static bool IsHidden(Minimap minimap, Minimap.PinData pin)
        {
            if (pin.m_ownerID != 0L)
            {
                return !HideFriendMapPingsPlugin.ShowSharedPins.Value;
            }
            return pin.m_save && pin != minimap.m_namePin && !HideFriendMapPingsPlugin.ShowOwnPins.Value;
        }
    }
}
