using UnityEngine;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.Minigames
{
    public class PestControl : BaseFeature
    {
        public override string Name => "Pest Control";
        
        private bool maxResourcesEnabled = false;
        private bool autoWinEnabled = false;
        private bool enemyWeakeningEnabled = false;
        
        private Il2CppMiniGames.TowerDefense.TowerDefenseGameManager gameManager;
        private float lastEnemyWeakenTime = 0f;

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            
            // Find game manager if not found yet
            if (gameManager == null)
            {
                FindGameManager();
            }
            
            if (gameManager != null)
            {
                ApplyMaxResources();
                HandleAutoWin();
                WeakenEnemies();
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            
            if (IsEnabled)
            {
                float yOffset = 25;
                GUI.color = Color.white;
                
                // Max Resources toggle
                DrawCheckbox(new Rect(area.x + 5, area.y + yOffset, 16, 16), maxResourcesEnabled, "Max Cash + Lives",
                    (value) => { maxResourcesEnabled = value; });
                yOffset += 25;
                
                // Auto Win toggle
                DrawCheckbox(new Rect(area.x + 5, area.y + yOffset, 16, 16), autoWinEnabled, "Auto Win (Ctrl+W)",
                    (value) => { autoWinEnabled = value; });
                yOffset += 25;
                
                // Enemy Weakening toggle
                DrawCheckbox(new Rect(area.x + 5, area.y + yOffset, 16, 16), enemyWeakeningEnabled, "Weaken Enemies (1 HP)",
                    (value) => { enemyWeakeningEnabled = value; });
                yOffset += 25;
                
                // Status display
                if (gameManager != null)
                {
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Tower Defense: ACTIVE");
                }
                else
                {
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Tower Defense: Not Found");
                }
            }
        }

        private void FindGameManager()
        {
            try
            {
                // Search for TowerDefenseGameManager in active objects
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    var manager = obj.GetComponent<Il2CppMiniGames.TowerDefense.TowerDefenseGameManager>();
                    if (manager != null)
                    {
                        gameManager = manager;
                        MelonLogger.Msg("[WildBerry] Found Tower Defense Game Manager!");
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error finding Tower Defense Game Manager: {ex.Message}");
            }
        }

        private void ApplyMaxResources()
        {
            if (!maxResourcesEnabled || gameManager == null) return;
            
            try
            {
                // Set max starting tokens and lives
                if (gameManager.extraStartingTokens < 99999)
                {
                    gameManager.extraStartingTokens = 99999;
                    MelonLogger.Msg("[WildBerry] Set max starting tokens: 99999");
                }
                
                if (gameManager.startingLives < 99999)
                {
                    gameManager.startingLives = 99999;
                    MelonLogger.Msg("[WildBerry] Set max starting lives: 99999");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying max resources: {ex.Message}");
            }
        }

        private void HandleAutoWin()
        {
            if (!autoWinEnabled) return;
            
            try
            {
                // Check for Ctrl+W shortcut
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W))
                {
                    TriggerAutoWin();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error handling auto win: {ex.Message}");
            }
        }

        private void TriggerAutoWin()
        {
            if (gameManager == null) return;
            
            try
            {
                MelonLogger.Msg("[WildBerry] Triggering Auto Win for Tower Defense!");
                
                // Force game over with SUCCESS condition
                // Try multiple win condition approaches
                gameManager.GameOver(Il2CppMiniGames.EWinCondition.SUCCESS);
                
                // Also try the base class methods
                var miniGameBase = gameManager.TryCast<Il2CppMiniGames.MiniGameBase>();
                if (miniGameBase != null)
                {
                    miniGameBase.GameOver(Il2CppMiniGames.EWinCondition.SUCCESS);
                    miniGameBase.UpdatePlayCount(Il2CppMiniGames.EWinCondition.SUCCESS);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error triggering auto win: {ex.Message}");
            }
        }

        private void WeakenEnemies()
        {
            if (!enemyWeakeningEnabled) return;
            
            // Only check every 0.5 seconds to avoid performance issues
            if (Time.time - lastEnemyWeakenTime < 0.5f) return;
            lastEnemyWeakenTime = Time.time;
            
            try
            {
                // Find TowerDefense Enemy Manager
                GameObject enemyManager = GameObject.Find("TowerDefense Enemy Manager");
                if (enemyManager == null) return;
                
                // Search for all enemy objects with Enemy#_type pattern
                Transform[] allChildren = enemyManager.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    if (child.name.StartsWith("Enemy") && child.name.Contains("_"))
                    {
                        // Get TowerDefenseEnemy component
                        var enemy = child.GetComponent<Il2CppMiniGames.TowerDefense.TowerDefenseEnemy>();
                        if (enemy != null && enemy._currentHitPoints > 1)
                        {
                            enemy._currentHitPoints = 1;
                            // MelonLogger.Msg($"[WildBerry] Weakened enemy: {child.name} to 1 HP");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error weakening enemies: {ex.Message}");
            }
        }

        public override void OnDisable()
        {
            // Reset toggles when feature is disabled
            maxResourcesEnabled = false;
            autoWinEnabled = false;
            enemyWeakeningEnabled = false;
            gameManager = null;
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f; // Just checkbox
            
            // Base checkbox (25) + 3 sub-toggles (20*3) + status (20) + 3px extra padding
            return 25f + (20f * 3) + 20f + 10f; // = 108px when enabled
        }
    }
}