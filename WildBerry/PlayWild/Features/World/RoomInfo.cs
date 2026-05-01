using UnityEngine;
using UnityEngine.SceneManagement;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class RoomInfo : BaseFeature
    {
        public override string Name => "Room Info";

        private string currentScene = "";
        private int playerCount = 0;
        private float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 2f;

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            if (Time.time - lastUpdateTime < UPDATE_INTERVAL) return;
            lastUpdateTime = Time.time;

            try
            {
                currentScene = SceneManager.GetActiveScene().name;

                var networkAvatars = UnityEngine.Object.FindObjectsOfType<Il2Cpp.Avatar_Network>();
                playerCount = networkAvatars != null ? networkAvatars.Length : 0;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Room Info error: {ex.Message}");
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 300, 20), $"Scene: {currentScene}");
                yOffset += 18;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 300, 20), $"Other players: {playerCount}");
                yOffset += 18;

                var localPlayer = Il2Cpp.Avatar_Local.instance;
                if (localPlayer != null)
                {
                    try
                    {
                        var userInfo = localPlayer.userInfo;
                        if (userInfo != null)
                        {
                            DrawLabel(new Rect(area.x + 5, area.y + yOffset, 300, 20),
                                $"Account: {userInfo.accountType} | Member: {userInfo.isMember}");
                        }
                    }
                    catch { }
                }
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return WildBerryTheme.ToggleHeight;
            return 85f;
        }
    }
}
