using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class PremiumMember : BaseFeature
    {
        public override string Name => "Premium Member";

        private bool wasApplied = false;

        public override void OnUpdate()
        {
            if (!IsEnabled)
            {
                if (wasApplied)
                {
                    RestoreAccountType();
                    wasApplied = false;
                }
                return;
            }

            ApplyPremiumMembership();
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });
        }

        public override float GetDynamicHeight()
        {
            return 25f;
        }

        private void ApplyPremiumMembership()
        {
            try
            {
                var actorsObject = GameObject.Find("Actors");
                if (actorsObject == null) return;

                var localPlayerTransform = actorsObject.transform.Find("LocalPlayer");
                if (localPlayerTransform == null) return;

                GameObject actorLocal = null;
                for (int i = 0; i < localPlayerTransform.childCount; i++)
                {
                    var child = localPlayerTransform.GetChild(i);
                    if (child.name.StartsWith("(Actor_Local)"))
                    {
                        actorLocal = child.gameObject;
                        break;
                    }
                }

                if (actorLocal == null) return;

                var avatarLocal = actorLocal.GetComponent<Il2Cpp.Avatar_Local>();
                if (avatarLocal == null) return;

                var userInfo = avatarLocal.userInfo;
                if (userInfo == null) return;

                if (userInfo.accountType != Il2Cpp.AccountType.PremiumMember)
                {
                    userInfo.accountType = Il2Cpp.AccountType.PremiumMember;
                    userInfo.accountTypeChanged = true;

                    if (!wasApplied)
                    {
                        MelonLogger.Msg($"[WildBerry] Premium membership activated on {actorLocal.name}!");
                        wasApplied = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying premium membership: {ex.Message}");
            }
        }

        private void RestoreAccountType()
        {
            try
            {
                var actorsObject = GameObject.Find("Actors");
                if (actorsObject == null) return;

                var localPlayerTransform = actorsObject.transform.Find("LocalPlayer");
                if (localPlayerTransform == null) return;

                GameObject actorLocal = null;
                for (int i = 0; i < localPlayerTransform.childCount; i++)
                {
                    var child = localPlayerTransform.GetChild(i);
                    if (child.name.StartsWith("(Actor_Local)"))
                    {
                        actorLocal = child.gameObject;
                        break;
                    }
                }

                if (actorLocal == null) return;

                var avatarLocal = actorLocal.GetComponent<Il2Cpp.Avatar_Local>();
                if (avatarLocal == null) return;

                var userInfo = avatarLocal.userInfo;
                if (userInfo == null) return;

                if (userInfo.accountType == Il2Cpp.AccountType.PremiumMember)
                {
                    userInfo.accountType = Il2Cpp.AccountType.NonMember;
                    userInfo.accountTypeChanged = true;
                    MelonLogger.Msg($"[WildBerry] Premium membership deactivated on {actorLocal.name}!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring account type: {ex.Message}");
            }
        }

        public override void OnDisable()
        {
            if (wasApplied)
            {
                RestoreAccountType();
                wasApplied = false;
            }
        }
    }
}
