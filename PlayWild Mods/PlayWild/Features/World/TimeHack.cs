using UnityEngine;
using PlayWild.Features.Base;
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
            
            // Load persisted values
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
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            
            if (IsEnabled)
            {
                float yOffset = 25;
                GUI.color = Color.white;
                
                // Time scale input field
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 100, 20), "Time multiplier:");
                yOffset += 20;
                
                string newTimeText = GUI.TextField(new Rect(area.x + 5, area.y + yOffset, 80, 20), timeInputText);
                if (newTimeText != timeInputText)
                {
                    timeInputText = newTimeText;
                    if (float.TryParse(timeInputText, out float newTime) && newTime > 0)
                    {
                        timeScale = newTime;
                        SetPersistedValue("timeScale", timeScale);
                    }
                }
                yOffset += 25;
                
                // Current time scale display
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), $"Current: {Time.timeScale:F1}x");
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return 25f; // Just checkbox height
            }
            
            // Checkbox (25) + Label (20) + TextField (25) + Display Label (20)
            return 90f;
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