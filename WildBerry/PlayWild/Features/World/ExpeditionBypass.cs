using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class ExpeditionBypass : BaseFeature
    {
        public override string Name => "Expedition Bypass";

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            try
            {
                Il2Cpp.ExpeditionManager.IgnoreEnergyCostDEBUG = true;
                Il2Cpp.ExpeditionManager.IgnoreTimeRequirementDEBUG = true;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Expedition Bypass error: {ex.Message}");
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;
                DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), "No energy cost", Color.green);
                yOffset += 18;
                DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), "No time requirement", Color.green);
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return WildBerryTheme.ToggleHeight;
            return 65f;
        }

        public override void OnDisable()
        {
            try
            {
                Il2Cpp.ExpeditionManager.IgnoreEnergyCostDEBUG = false;
                Il2Cpp.ExpeditionManager.IgnoreTimeRequirementDEBUG = false;
                MelonLogger.Msg("[WildBerry] Expedition Bypass: Restored normal expedition costs");
            }
            catch { }
        }
    }
}
