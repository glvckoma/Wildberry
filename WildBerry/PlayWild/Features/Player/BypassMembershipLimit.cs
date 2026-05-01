using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class BypassMembershipLimit : BaseFeature
    {
        public override string Name => "Bypass Membership Limit";

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            try
            {
                Il2Cpp.UserInfo.Me.SetPendingFlagsFromServer(Il2Cpp.AccountPendingFlag.SapphireMembershipCountLowBit, false);
                Il2Cpp.UserInfo.Me.SetPendingFlagsFromServer(Il2Cpp.AccountPendingFlag.SapphireMembershipCountHighBit, false);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Bypass Membership Limit error: {ex.Message}");
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;
                DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), "Membership limit removed", Color.green);
                yOffset += 18;

                try
                {
                    int count = Il2Cpp.UserInfo.Me.NumSapphireMembershipPurchases;
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), $"Purchase count: {count}", Color.cyan);
                }
                catch
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), "Purchase count: N/A", Color.gray);
                }
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
                MelonLogger.Msg("[WildBerry] Bypass Membership Limit: Restored normal membership limits");
            }
            catch { }
        }
    }
}
