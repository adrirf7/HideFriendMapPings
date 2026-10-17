using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HideFriendMapPings
{
    /// <summary>
    /// Casillas del mapa grande para mostrar / ocultar los marcadores y las zonas exploradas que vienen de la
    /// mesa de cartografía. Son copias de la casilla "Visible para otros jugadores" del juego, apiladas encima
    /// de ella y de la casilla "Mesa de cartografía" del juego.
    /// </summary>
    internal static class MapToggles
    {
        private const float Gap = 4f;

        private static readonly int s_sharedFadeId = Shader.PropertyToID("_SharedFade");

        private static Minimap s_minimap;
        private static Toggle s_source;
        private static Toggle s_pinsToggle;
        private static Toggle s_mapToggle;

        internal static void Create(Minimap minimap)
        {
            s_minimap = minimap;
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
                s_pinsToggle = CloneToggle(s_source, "HFMP_SharedPins", HideFriendMapPingsPlugin.ShowSharedPins.Value,
                    isOn => HideFriendMapPingsPlugin.ShowSharedPins.Value = isOn);
                UpdatePositions();
                RefreshTexts();
            }
            catch (Exception e)
            {
                HideFriendMapPingsPlugin.Log.LogError("Could not create the map checkboxes: " + e);
            }
        }

        /// <summary>
        /// Encima de la casilla "Visible para otros jugadores" y de la de "Mesa de cartografía" (aunque esta solo
        /// aparezca cuando hay datos compartidos): primero la del mapa y, sobre ella, la de los marcadores.
        /// Se usa el tamaño visual (fondo incluido), no el del RectTransform de la casilla, que es más pequeño.
        /// </summary>
        internal static void UpdatePositions()
        {
            if (s_source == null)
            {
                return;
            }
            RectTransform source = (RectTransform)s_source.transform;
            RectTransform space = (RectTransform)source.parent;
            Rect sourceBounds = VisualBounds(source, space);

            float top = sourceBounds.yMax;
            if (s_minimap != null && s_minimap.m_sharedMapHint != null)
            {
                Rect hintBounds = VisualBounds((RectTransform)s_minimap.m_sharedMapHint.transform, space);
                top = Mathf.Max(top, hintBounds.yMax);
            }

            Vector2 offset = HideFriendMapPingsPlugin.TogglesOffset.Value;
            float height = sourceBounds.height;
            float firstBottom = top + Gap;
            SetPosition(s_mapToggle, source.anchoredPosition + offset + new Vector2(0f, firstBottom - sourceBounds.yMin));
            SetPosition(s_pinsToggle, source.anchoredPosition + offset + new Vector2(0f, firstBottom + height + Gap - sourceBounds.yMin));
        }

        internal static void RefreshTexts()
        {
            SetText(s_pinsToggle, "$hfmp_shared_pins");
            SetText(s_mapToggle, "$hfmp_shared_map");
        }

        /// <summary>Cambio desde la casilla o desde el archivo de configuración (p. ej. con ConfigurationManager).</summary>
        internal static void OnSettingsChanged()
        {
            if (s_pinsToggle != null)
            {
                s_pinsToggle.SetIsOnWithoutNotify(HideFriendMapPingsPlugin.ShowSharedPins.Value);
            }
            if (s_mapToggle != null)
            {
                s_mapToggle.SetIsOnWithoutNotify(HideFriendMapPingsPlugin.ShowSharedMap.Value);
            }
            if (Minimap.instance != null)
            {
                Minimap.instance.m_pinUpdateRequired = true;
                ApplySharedMapFade(Minimap.instance);
            }
        }

        /// <summary>
        /// El shader del mapa mezcla la niebla quitada por otros jugadores según "_SharedFade". El juego lo ajusta
        /// con su propio botón (que oculta mapa y marcadores a la vez); aquí se fuerza a 0 si solo se quiere lo propio.
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
