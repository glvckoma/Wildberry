using UnityEngine;
using PlayWild.Features.Base;
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
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
        }

        public override float GetDynamicHeight()
        {
            // Just checkbox height since this feature has no additional GUI when enabled
            return 25f;
        }

        private void ApplyPremiumMembership()
        {
            try
            {
                // Find the LocalPlayer through the correct path: Actors -> LocalPlayer -> (Actor_Local) *any animal*
                var actorsObject = GameObject.Find("Actors");
                if (actorsObject == null) return;

                var localPlayerTransform = actorsObject.transform.Find("LocalPlayer");
                if (localPlayerTransform == null) return;

                // Search for child that starts with "(Actor_Local)" - could be any animal
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

                // Check if already premium
                if (userInfo.accountType != Il2Cpp.AccountType.PremiumMember)
                {
                    // Set to PremiumMember
                    userInfo.accountType = Il2Cpp.AccountType.PremiumMember;
                    userInfo.accountTypeChanged = true;
                    
                    if (!wasApplied)
                    {
                        MelonLogger.Msg($"[WildBerry] Premium membership activated on {actorLocal.name}! AccountType: {userInfo.accountType}, IsMember: {userInfo.isMember}");
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
                // Find the LocalPlayer through the correct path: Actors -> LocalPlayer -> (Actor_Local) *any animal*
                var actorsObject = GameObject.Find("Actors");
                if (actorsObject == null) return;

                var localPlayerTransform = actorsObject.transform.Find("LocalPlayer");
                if (localPlayerTransform == null) return;

                // Search for child that starts with "(Actor_Local)" - could be any animal
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

                // Check if currently premium
                if (userInfo.accountType == Il2Cpp.AccountType.PremiumMember)
                {
                    // Restore to NonMember (default)
                    userInfo.accountType = Il2Cpp.AccountType.NonMember;
                    userInfo.accountTypeChanged = true;
                    
                    MelonLogger.Msg($"[WildBerry] Premium membership deactivated on {actorLocal.name}! AccountType: {userInfo.accountType}, IsMember: {userInfo.isMember}");
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