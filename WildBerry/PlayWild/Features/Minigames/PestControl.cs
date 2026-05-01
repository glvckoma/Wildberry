using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
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
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawSubToggle(new Rect(area.x + 5, area.y + yOffset, area.width - 5, WildBerryTheme.SubToggleHeight), maxResourcesEnabled, "Max Cash + Lives",
                    (value) => { maxResourcesEnabled = value; });
                yOffset += 22;

                DrawSubToggle(new Rect(area.x + 5, area.y + yOffset, area.width - 5, WildBerryTheme.SubToggleHeight), autoWinEnabled, "Auto Win (Ctrl+W)",
                    (value) => { autoWinEnabled = value; });
                yOffset += 22;

                DrawSubToggle(new Rect(area.x + 5, area.y + yOffset, area.width - 5, WildBerryTheme.SubToggleHeight), enemyWeakeningEnabled, "Weaken Enemies (1 HP)",
                    (value) => { enemyWeakeningEnabled = value; });
                yOffset += 25;

                if (gameManager != null)
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Tower Defense: ACTIVE", WildBerryTheme.StatusActive);
                }
                else
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Tower Defense: Not Found", WildBerryTheme.StatusText);
                }
            }
        }

        private void FindGameManager()
        {
            try
            {
                gameManager = UnityEngine.Object.FindObjectOfType<Il2CppMiniGames.TowerDefense.TowerDefenseGameManager>();
                if (gameManager != null)
                {
                    MelonLogger.Msg("[WildBerry] Found Tower Defense Game Manager!");
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
                gameManager.GameOver(Il2CppMiniGames.EWinCondition.SUCCESS);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error triggering auto win: {ex.Message}");
            }
        }

        private void WeakenEnemies()
        {
            if (!enemyWeakeningEnabled) return;

            if (Time.time - lastEnemyWeakenTime < 0.5f) return;
            lastEnemyWeakenTime = Time.time;

            try
            {
                var enemies = UnityEngine.Object.FindObjectsOfType<Il2CppMiniGames.TowerDefense.TowerDefenseEnemy>();
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy._currentHitPoints > 1)
                    {
                        enemy._currentHitPoints = 1;
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
            maxResourcesEnabled = false;
            autoWinEnabled = false;
            enemyWeakeningEnabled = false;
            gameManager = null;
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return WildBerryTheme.ToggleHeight;
            return 25f + (22f * 3) + 25f;
        }
    }
}
