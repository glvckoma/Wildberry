using UnityEngine;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.Minigames
{
    public class AlwaysShowDigRewards : BaseFeature
    {
        public override string Name => "Always Show Dig Rewards";
        
        private bool wasLuckyShovelUIVisible = false;
        private bool hasUncovered = false;

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            AutoRevealLogic();
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

        private void AutoRevealLogic()
        {
            var luckyShovelUI = GameObject.Find("UI_Window_LuckyShovel");
            bool isCurrentlyVisible = luckyShovelUI != null;

            // Check if Lucky Shovel UI just became visible
            if (isCurrentlyVisible && !wasLuckyShovelUIVisible)
            {
                // UI just appeared, reveal rewards
                RevealLuckyShovelRewards();
                hasUncovered = true;
                MelonLogger.Msg("[WildBerry] Auto-revealed dig rewards!");
            }
            // Re-reveal every few frames to maintain visibility after digging
            else if (isCurrentlyVisible && hasUncovered && Time.frameCount % 30 == 0) // Every ~0.5 seconds
            {
                var component = luckyShovelUI.GetComponent<Il2Cpp.UI_Window_LuckyShovel>();
                if (component != null)
                {
                    RevealLuckyShovelRewards();
                }
            }
            // Reset when UI disappears
            else if (!isCurrentlyVisible && wasLuckyShovelUIVisible)
            {
                hasUncovered = false;
                MelonLogger.Msg("[WildBerry] Lucky Shovel closed.");
            }

            wasLuckyShovelUIVisible = isCurrentlyVisible;
        }

        private void RevealLuckyShovelRewards()
        {
            try
            {
                MelonLogger.Msg("Attempting to uncover Lucky Shovel rewards...");

                var luckyShovelUI = GameObject.Find("UI_Window_LuckyShovel");
                if (luckyShovelUI != null)
                {
                    var component = luckyShovelUI.GetComponent<Il2Cpp.UI_Window_LuckyShovel>();
                    if (component != null)
                    {
                        component.UncoverBoard();
                        MelonLogger.Msg("Successfully executed UncoverBoard() - all rewards revealed!");
                        return;
                    }
                }

                MelonLogger.Warning("Lucky Shovel window not found! Make sure the dig game is open.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error uncovering Lucky Shovel rewards: {ex.Message}");
            }
        }
    }
}