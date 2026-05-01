using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class TimeHack : BaseFeature
    {
        public override string Name => "Time Hack";

        private float timeScale = 2.0f;
        private string timeInputText = "2.0";
        private float originalTimeScale = 1.0f;

        public override void Initialize()
        {
            base.Initialize();

            timeScale = GetPersistedValue("timeScale", 2.0f);
            timeInputText = timeScale.ToString();
        }

        public override void OnEnable()
        {
            originalTimeScale = Time.timeScale;
        }

        public override void OnDisable()
        {
            RestoreTimeScale();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            ApplyTimeHack();
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 100, 20), "Time multiplier:");
                yOffset += 20;

                string newTimeText = DrawStyledTextField(new Rect(area.x + 5, area.y + yOffset, 80, WildBerryTheme.TextFieldHeight), timeInputText);
                if (newTimeText != timeInputText)
                {
                    timeInputText = newTimeText;
                    if (float.TryParse(timeInputText, out float newTime) && newTime > 0)
                    {
                        timeScale = newTime;
                        SetPersistedValue("timeScale", timeScale);
                    }
                }
                yOffset += 29;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), $"Current: {Time.timeScale:F1}x");
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return WildBerryTheme.ToggleHeight;
            }

            return 94f;
        }

        private void ApplyTimeHack()
        {
            try
            {
                if (Time.timeScale != timeScale)
                {
                    Time.timeScale = timeScale;
                    MelonLogger.Msg($"[WildBerry] Time scale set to {timeScale:F1}x");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying time hack: {ex.Message}");
            }
        }

        private void RestoreTimeScale()
        {
            try
            {
                Time.timeScale = originalTimeScale;
                MelonLogger.Msg("[WildBerry] Time scale restored to normal");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error restoring time scale: {ex.Message}");
            }
        }

    }
}
