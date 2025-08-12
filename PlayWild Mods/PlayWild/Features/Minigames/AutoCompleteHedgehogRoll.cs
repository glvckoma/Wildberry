using UnityEngine;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.Minigames
{
    public class AutoCompleteHedgehogRoll : BaseFeature
    {
        public override string Name => "Auto Complete Hedgehog Roll";
        
        private bool wasHedgehogRollActive = false;

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            HedgehogRollAutoComplete();
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

        private void HedgehogRollAutoComplete()
        {
            var phantomBlastGame = GameObject.Find("Phantom Blast Game");
            bool isGameActive = phantomBlastGame != null;
            bool isLevelActive = false;

            // Only trigger if a level is actually loaded (wurk_ object exists)
            if (isGameActive)
            {
                var levelManager = phantomBlastGame.transform.Find("Level Manager");
                if (levelManager != null)
                {
                    // Check if any wurk_ level is loaded
                    for (int i = 0; i < levelManager.childCount; i++)
                    {
                        var child = levelManager.GetChild(i);
                        if (child.name.StartsWith("wurk_"))
                        {
                            isLevelActive = true;
                            break;
                        }
                    }
                }
            }

            // Check if a level just became active
            if (isLevelActive && !wasHedgehogRollActive)
            {
                MelonLogger.Msg("[WildBerry] Hedgehog Roll level detected! Starting auto-complete...");
                CompleteHedgehogRoll();
            }
            // Continue auto-complete while level is active
            else if (isLevelActive && Time.frameCount % 10 == 0) // Every ~0.17 seconds
            {
                CompleteHedgehogRoll();
            }

            wasHedgehogRollActive = isLevelActive;
        }

        private void CompleteHedgehogRoll()
        {
            try
            {
                var phantomBlastGame = GameObject.Find("Phantom Blast Game");
                if (phantomBlastGame == null) return;

                // Get the goal position
                Vector3 goalPosition = FindLevelGoal(phantomBlastGame);
                if (goalPosition == Vector3.zero)
                {
                    MelonLogger.Warning("[WildBerry] Could not find Hedgehog Roll level goal!");
                    return;
                }

                // 1. Move all gems to player position
                MoveAllGemsToPlayer(phantomBlastGame, goalPosition);

                // 2. Expand gem collector
                ExpandGemCollector(phantomBlastGame);

                // 3. Teleport player ball to goal
                TeleportPlayerBallToGoal(phantomBlastGame, goalPosition);

                // 4. Teleport hedgehog to goal
                TeleportHedgehogToGoal(phantomBlastGame, goalPosition);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error auto-completing Hedgehog Roll: {ex.Message}");
            }
        }

        private void MoveAllGemsToPlayer(GameObject phantomBlastGame, Vector3 playerPosition)
        {
            try
            {
                var levelManager = phantomBlastGame.transform.Find("Level Manager");
                if (levelManager != null)
                {
                    for (int i = 0; i < levelManager.childCount; i++)
                    {
                        var child = levelManager.GetChild(i);
                        if (child.name.StartsWith("wurk_"))
                        {
                            var world = child.Find("World");
                            if (world != null)
                            {
                                var gemsParent = world.Find("gems");
                                if (gemsParent != null)
                                {
                                    int gemCount = 0;
                                    for (int j = 0; j < gemsParent.childCount; j++)
                                    {
                                        var gem = gemsParent.GetChild(j);
                                        if (gem.name.Contains("Item_RandomGem_noShadow"))
                                        {
                                            gem.position = playerPosition;
                                            gemCount++;
                                        }
                                    }
                                    MelonLogger.Msg($"[WildBerry] Moved {gemCount} gems to player position!");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error moving gems: {ex.Message}");
            }
        }

        private void ExpandGemCollector(GameObject phantomBlastGame)
        {
            try
            {
                var player = phantomBlastGame.transform.Find("Player");
                if (player != null)
                {
                    var gemCollector = player.Find("Gem Collector");
                    if (gemCollector != null)
                    {
                        gemCollector.localScale = new Vector3(50f, 50f, 50f);
                        MelonLogger.Msg("[WildBerry] Expanded Gem Collector scale!");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error expanding gem collector: {ex.Message}");
            }
        }

        private void TeleportPlayerBallToGoal(GameObject phantomBlastGame, Vector3 goalPosition)
        {
            try
            {
                var player = phantomBlastGame.transform.Find("Player");
                if (player != null)
                {
                    var playerBall = player.Find("Player Ball");
                    if (playerBall != null)
                    {
                        playerBall.position = goalPosition;
                        MelonLogger.Msg("[WildBerry] Teleported Player Ball to goal!");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error teleporting player ball: {ex.Message}");
            }
        }

        private void TeleportHedgehogToGoal(GameObject phantomBlastGame, Vector3 goalPosition)
        {
            try
            {
                var player = phantomBlastGame.transform.Find("Player");
                if (player != null)
                {
                    var hedgehog = player.Find("HedgeHog");
                    if (hedgehog != null)
                    {
                        hedgehog.position = goalPosition;
                        MelonLogger.Msg("[WildBerry] Teleported HedgeHog to goal!");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error teleporting hedgehog: {ex.Message}");
            }
        }

        private Vector3 FindLevelGoal(GameObject phantomBlastGame)
        {
            try
            {
                var levelManager = phantomBlastGame.transform.Find("Level Manager");
                if (levelManager == null) return Vector3.zero;

                // Find any wurk_ level dynamically
                for (int i = 0; i < levelManager.childCount; i++)
                {
                    var child = levelManager.GetChild(i);
                    if (child.name.StartsWith("wurk_"))
                    {
                        var world = child.Find("World");
                        if (world != null)
                        {
                            var envGoal = world.Find("Environment_LevelGoal");
                            if (envGoal != null)
                            {
                                var levelGoalClone = envGoal.Find("Level_Goal(Clone)");
                                if (levelGoalClone != null)
                                {
                                    MelonLogger.Msg($"[WildBerry] Found goal in {child.name}!");
                                    return levelGoalClone.position;
                                }
                                else
                                {
                                    MelonLogger.Warning($"[WildBerry] Found Environment_LevelGoal but no Level_Goal(Clone) inside!");
                                }
                            }
                        }
                        MelonLogger.Warning($"[WildBerry] Found {child.name} but no goal inside!");
                    }
                }

                MelonLogger.Warning("[WildBerry] No wurk_ level found!");
                return Vector3.zero;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error finding level goal: {ex.Message}");
                return Vector3.zero;
            }
        }
    }
}