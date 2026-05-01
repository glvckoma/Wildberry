using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class AntiAFK : BaseFeature
    {
        public override string Name => "Anti-AFK";

        private const float ACTIVITY_INTERVAL = 25f;
        private const int IDLE_TIMER_OFFSET = 0x118;
        private float lastActivityTime = 0f;

        public override void OnEnable()
        {
            base.OnEnable();
            lastActivityTime = Time.time;
            MelonLogger.Msg("[WildBerry] Anti-AFK: Enabled");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            MelonLogger.Msg("[WildBerry] Anti-AFK: Disabled");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            if (Time.time - lastActivityTime < ACTIVITY_INTERVAL) return;
            lastActivityTime = Time.time;
            ResetIdleTimer();
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                DrawLabel(new Rect(area.x + 5, area.y + 25, 200, 20), "Resets idle timer every 25s");
            }
        }

        public override float GetDynamicHeight()
        {
            return IsEnabled ? 45f : WildBerryTheme.ToggleHeight;
        }

        private unsafe void ResetIdleTimer()
        {
            try
            {
                var localAvatar = UnityEngine.Object.FindObjectOfType<Il2Cpp.Avatar_Local>();
                if (localAvatar == null) return;

                *(float*)((byte*)(void*)localAvatar.Pointer + IDLE_TIMER_OFFSET) = 0f;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Anti-AFK error: {ex.Message}");
            }
        }
    }
}
