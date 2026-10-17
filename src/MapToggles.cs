using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HideFriendMapPings
{
    /// <summary>
    /// Casillas del mapa grande para mostrar / ocultar el mapa compartido, los marcadores compartidos y los propios.
    /// Son copias de la casilla "Visible para otros jugadores" del juego, apiladas encima de ella. Sustituyen a la
    /// casilla "Mesa de cartografía" del juego (que ocultaba mapa y marcadores compartidos a la vez), que se oculta.
    /// </summary>
    internal static class MapToggles
    {
        private const float Gap = 4f;

        private static readonly int s_sharedFadeId = Shader.PropertyToID("_SharedFade");

        private static Toggle s_source;
        private static Toggle s_mapToggle;
        private static Toggle s_sharedPinsToggle;
        private static Toggle s_ownPinsToggle;

        internal static void Create(Minimap minimap)
        {
            // Sin la casilla del juego nadie puede apagar lo compartido por su cuenta: que lo controlen solo las nuestras.
            minimap.m_showSharedMapData = true;
            HideGameSharedToggle(minimap);

            s_source = minimap.m_publicPosition;
            if (s_source == null)
            {
                HideFriendMapPingsPlugin.Log.LogWarning("Public position toggle not found, map checkboxes not created.");
                return;
            }
            try
            {
                s_mapToggle = CloneToggle(s_source, "HFMP_SharedMap", HideFriendMapPingsPlugin.ShowSharedMap.Value,
                    isOn => HideFriendMapPingsPlugin.ShowSharedMap.Value = isOn);
                s_sharedPinsToggle = CloneToggle(s_source, "HFMP_SharedPins", HideFriendMapPingsPlugin.ShowSharedPins.Value,
                    isOn => HideFriendMapPingsPlugin.ShowSharedPins.Value = isOn);
                s_ownPinsToggle = CloneToggle(s_source, "HFMP_OwnPins", HideFriendMapPingsPlugin.ShowOwnPins.Value,
                    isOn => HideFriendMapPingsPlugin.ShowOwnPins.Value = isOn);
                UpdatePositions();
                RefreshTexts();
            }
            catch (Exception e)
            {
                HideFriendMapPingsPlugin.Log.LogError("Could not create the map checkboxes: " + e);
            }
        }

        /// <summary>El juego la vuelve a activar cada vez que encuentra datos compartidos.</summary>
        internal static void HideGameSharedToggle(Minimap minimap)
        {
            if (minimap.m_sharedMapHint != null && minimap.m_sharedMapHint.activeSelf)
            {
                minimap.m_sharedMapHint.SetActive(false);
            }
        }

        /// <summary>
        /// Encima de la casilla "Visible para otros jugadores", de abajo arriba: mapa compartido, marcadores
        /// compartidos y mis marcadores. Se usa el tamaño visual (fondo incluido), no el del RectTransform de la
        /// casilla, que es más pequeño.
        /// </summary>
        internal static void UpdatePositions()
        {
            if (s_source == null)
            {
                return;
            }
            RectTransform source = (RectTransform)s_source.transform;
            Rect bounds = VisualBounds(source, (RectTransform)source.parent);
            float step = bounds.height + Gap;
            Vector2 basePosition = source.anchoredPosition + HideFriendMapPingsPlugin.TogglesOffset.Value;
            SetPosition(s_mapToggle, basePosition + new Vector2(0f, step));
            SetPosition(s_sharedPinsToggle, basePosition + new Vector2(0f, step * 2f));
            SetPosition(s_ownPinsToggle, basePosition + new Vector2(0f, step * 3f));
        }

        internal static void RefreshTexts()
        {
            SetText(s_mapToggle, "$hfmp_shared_map");
            SetText(s_sharedPinsToggle, "$hfmp_shared_pins");
            SetText(s_ownPinsToggle, "$hfmp_own_pins");
        }

        /// <summary>Cambio desde la casilla o desde el archivo de configuración (p. ej. con ConfigurationManager).</summary>
        internal static void OnSettingsChanged()
        {
            SetIsOn(s_mapToggle, HideFriendMapPingsPlugin.ShowSharedMap.Value);
            SetIsOn(s_sharedPinsToggle, HideFriendMapPingsPlugin.ShowSharedPins.Value);
            SetIsOn(s_ownPinsToggle, HideFriendMapPingsPlugin.ShowOwnPins.Value);
            if (Minimap.instance != null)
            {
                Minimap.instance.m_pinUpdateRequired = true;
                ApplySharedMapFade(Minimap.instance);
            }
        }

        /// <summary>
        /// El shader del mapa mezcla la niebla quitada por otros jugadores según "_SharedFade"; se fuerza a 0 si
        /// solo se quiere ver lo explorado por uno mismo.
        /// </summary>
        internal static void ApplySharedMapFade(Minimap minimap)
        {
            float fade = HideFriendMapPingsPlugin.ShowSharedMap.Value ? minimap.m_sharedMapDataFade : 0f;
            if (minimap.m_mapLargeShader != null)
            {
                minimap.m_mapLargeShader.SetFloat(s_sharedFadeId, fade);
            }
            if (minimap.m_mapSmallShader != null)
            {
                minimap.m_mapSmallShader.SetFloat(s_sharedFadeId, fade);
            }
        }

        /// <summary>
        /// Rectángulo (en el espacio de <paramref name="space"/>) que ocupan el elemento y lo que dibujan sus hijos,
        /// sin contar la pista del botón de mando, que normalmente está oculta.
        /// </summary>
        private static Rect VisualBounds(RectTransform target, RectTransform space)
        {
            HashSet<Transform> hints = new HashSet<Transform>();
            foreach (UIGamePad gamePad in target.GetComponentsInChildren<UIGamePad>(true))
            {
                if (gamePad.m_hint != null)
                {
                    hints.Add(gamePad.m_hint.transform);
                }
            }

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            Vector3[] corners = new Vector3[4];
            void Add(RectTransform rect)
            {
                rect.GetWorldCorners(corners);
                foreach (Vector3 corner in corners)
                {
                    Vector2 local = space.InverseTransformPoint(corner);
                    min = Vector2.Min(min, local);
                    max = Vector2.Max(max, local);
                }
            }

            Add(target);
            foreach (Graphic graphic in target.GetComponentsInChildren<Graphic>(true))
            {
                if (!IsUnderAny(graphic.transform, hints, target))
                {
                    Add(graphic.rectTransform);
                }
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool IsUnderAny(Transform transform, HashSet<Transform> ancestors, Transform root)
        {
            for (Transform current = transform; current != null && current != root.parent; current = current.parent)
            {
                if (ancestors.Contains(current))
                {
                    return true;
                }
            }
            return false;
        }

        private static Toggle CloneToggle(Toggle source, string name, bool isOn, Action<bool> onChanged)
        {
            GameObject clone = Object.Instantiate(source.gameObject, source.transform.parent, false);
            clone.name = name;
            // Quitar la traducción automática del texto y el atajo de mando del original, incluida la pista
            // del botón de mando: sin UIGamePad nadie la oculta y se vería como texto suelto.
            foreach (Localize localize in clone.GetComponentsInChildren<Localize>(true))
            {
                Object.DestroyImmediate(localize);
            }
            foreach (UIGamePad gamePad in clone.GetComponentsInChildren<UIGamePad>(true))
            {
                if (gamePad.m_hint != null)
                {
                    Object.DestroyImmediate(gamePad.m_hint);
                }
                Object.DestroyImmediate(gamePad);
            }
            Toggle toggle = clone.GetComponent<Toggle>();
            // Un evento nuevo descarta también el listener del original (OnTogglePublicPosition).
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.SetIsOnWithoutNotify(isOn);
            toggle.onValueChanged.AddListener(value => onChanged(value));
            return toggle;
        }

        private static void SetIsOn(Toggle toggle, bool isOn)
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(isOn);
            }
        }

        private static void SetPosition(Toggle toggle, Vector2 position)
        {
            if (toggle != null)
            {
                ((RectTransform)toggle.transform).anchoredPosition = position;
            }
        }

        private static void SetText(Toggle toggle, string token)
        {
            if (toggle == null)
            {
                return;
            }
            TMP_Text text = toggle.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = Localization.instance.Localize(token);
                text.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }
    }
}
