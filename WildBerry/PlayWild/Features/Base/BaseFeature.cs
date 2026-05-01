using UnityEngine;
using MelonLoader;
using PlayWild.Utils;
using PlayWild.Interface;

namespace PlayWild.Features.Base
{
    public abstract class BaseFeature : IFeature
    {
        public abstract string Name { get; }

        private bool _isEnabled = false;
        private bool _isInitialized = false;

        public virtual bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (value)
                        OnEnable();
                    else
                        OnDisable();

                    if (_isInitialized)
                    {
                        PersistenceManager.SetFeatureEnabled(GetPersistenceKey(), value);
                    }

                    MelonLogger.Msg($"[WildBerry] {Name}: {(value ? "ENABLED" : "DISABLED")}");
                }
            }
        }

        public virtual void Initialize()
        {
            _isEnabled = PersistenceManager.GetFeatureEnabled(GetPersistenceKey(), false);
            _isInitialized = true;

            if (_isEnabled)
            {
                OnEnable();
            }
        }

        public virtual void OnUpdate() { }
        public abstract void OnGUI(Rect area);
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        protected void DrawToggle(Rect position, bool currentValue, string label, System.Action<bool> onValueChanged)
        {
            Rect trackRect = new Rect(position.x, position.y, WildBerryTheme.ToggleWidth, WildBerryTheme.ToggleHeight);

            GUI.color = currentValue ? WildBerryTheme.ToggleOn : WildBerryTheme.ToggleOff;
            GUI.DrawTexture(trackRect, WildBerryTheme.ToggleTrackTex, ScaleMode.StretchToFill);

            float knobX = currentValue
                ? trackRect.x + trackRect.width - WildBerryTheme.ToggleKnobSize - WildBerryTheme.ToggleKnobPad
                : trackRect.x + WildBerryTheme.ToggleKnobPad;
            float knobY = trackRect.y + (trackRect.height - WildBerryTheme.ToggleKnobSize) / 2f;
            Rect knobRect = new Rect(knobX, knobY, WildBerryTheme.ToggleKnobSize, WildBerryTheme.ToggleKnobSize);

            GUI.color = WildBerryTheme.ToggleKnobColor;
            GUI.DrawTexture(knobRect, WildBerryTheme.CircleDot, ScaleMode.ScaleToFit);

            GUI.color = Color.white;
            Rect labelRect = new Rect(
                trackRect.x + WildBerryTheme.ToggleWidth + 10,
                trackRect.y,
                position.width - WildBerryTheme.ToggleWidth - 10,
                WildBerryTheme.ToggleHeight);
            GUI.Label(labelRect, label, WildBerryTheme.FeatureNameStyle);

            Rect clickArea = new Rect(position.x, position.y, position.width, WildBerryTheme.ToggleHeight);
            if (GUI.Button(clickArea, "", GUIStyle.none))
            {
                onValueChanged(!currentValue);
            }
        }

        protected void DrawSubToggle(Rect position, bool currentValue, string label, System.Action<bool> onValueChanged)
        {
            Rect trackRect = new Rect(position.x, position.y, WildBerryTheme.SubToggleWidth, WildBerryTheme.SubToggleHeight);

            GUI.color = currentValue ? WildBerryTheme.ToggleOn : WildBerryTheme.ToggleOff;
            GUI.DrawTexture(trackRect, WildBerryTheme.ToggleTrackTex, ScaleMode.StretchToFill);

            float knobX = currentValue
                ? trackRect.x + trackRect.width - WildBerryTheme.SubToggleKnobSize - WildBerryTheme.ToggleKnobPad
                : trackRect.x + WildBerryTheme.ToggleKnobPad;
            float knobY = trackRect.y + (trackRect.height - WildBerryTheme.SubToggleKnobSize) / 2f;
            Rect knobRect = new Rect(knobX, knobY, WildBerryTheme.SubToggleKnobSize, WildBerryTheme.SubToggleKnobSize);

            GUI.color = WildBerryTheme.ToggleKnobColor;
            GUI.DrawTexture(knobRect, WildBerryTheme.CircleDot, ScaleMode.ScaleToFit);

            GUI.color = Color.white;
            Rect labelRect = new Rect(
                trackRect.x + WildBerryTheme.SubToggleWidth + 8,
                trackRect.y,
                position.width - WildBerryTheme.SubToggleWidth - 8,
                WildBerryTheme.SubToggleHeight);
            GUI.Label(labelRect, label, WildBerryTheme.LabelStyle);

            Rect clickArea = new Rect(position.x, position.y, position.width, WildBerryTheme.SubToggleHeight);
            if (GUI.Button(clickArea, "", GUIStyle.none))
            {
                onValueChanged(!currentValue);
            }
        }

        protected string DrawStyledTextField(Rect position, string currentText)
        {
            return GUI.TextField(position, currentText, WildBerryTheme.TextFieldStyle);
        }

        protected string DrawStyledTextArea(Rect position, string currentText)
        {
            return GUI.TextArea(position, currentText, WildBerryTheme.TextAreaStyle);
        }

        protected bool DrawStyledButton(Rect position, string label)
        {
            return GUI.Button(position, label, WildBerryTheme.ButtonStyle);
        }

        protected void DrawStatusLabel(Rect position, string text, Color color)
        {
            var style = new GUIStyle(WildBerryTheme.StatusStyle) { normal = { textColor = color } };
            GUI.Label(position, text, style);
        }

        protected void DrawLabel(Rect position, string text)
        {
            GUI.Label(position, text, WildBerryTheme.LabelStyle);
        }

        protected string GetGameObjectPath(GameObject obj)
        {
            try
            {
                string path = obj.name;
                Transform current = obj.transform.parent;
                while (current != null)
                {
                    path = current.name + "/" + path;
                    current = current.parent;
                }
                return path;
            }
            catch
            {
                return obj.name;
            }
        }

        protected virtual string GetPersistenceKey()
        {
            return Name.Replace(" ", "_").Replace("+", "_");
        }

        protected T GetPersistedValue<T>(string valueName, T defaultValue = default(T))
        {
            return PersistenceManager.GetFeatureValue(GetPersistenceKey(), valueName, defaultValue);
        }

        protected void SetPersistedValue<T>(string valueName, T value)
        {
            if (_isInitialized)
            {
                PersistenceManager.SetFeatureValue(GetPersistenceKey(), valueName, value);
            }
        }

        public virtual float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return 25f;
            }
            return 50f;
        }
    }
}
