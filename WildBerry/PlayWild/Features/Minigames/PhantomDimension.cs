using UnityEngine;
using PlayWild.Features.Base;
using MelonLoader;
using System.Collections.Generic;

namespace PlayWild.Features.Minigames
{
    public class PhantomDimension : BaseFeature
    {
        public override string Name => "Phantom Dimension";
        
        private bool isPhDActive = false;
        private GameObject phdGameObject = null;
        private Il2CppMiniGames.PhD.PhDGame phdGame = null;
        private List<Il2CppMiniGames.PhD.TimingBasedTapUI> activeTapUIs = new List<Il2CppMiniGames.PhD.TimingBasedTapUI>();
        private float lastTapTime = 0f;
        private bool wasInBattle = false;

        // Sub-feature toggles (disabled by default)
        private bool infiniteLives = false;
        private bool maxKillCount = false;
        private bool phantomAvoidance = false;
        private bool perfectTapAutomation = false;

        public override void Initialize()
        {
            base.Initialize();
            
            // Load sub-feature states (default to false)
            infiniteLives = GetPersistedValue("infiniteLives", false);
            maxKillCount = GetPersistedValue("maxKillCount", false);
            phantomAvoidance = GetPersistedValue("phantomAvoidance", false);
            perfectTapAutomation = GetPersistedValue("perfectTapAutomation", false);
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) 
            {
                if (isPhDActive)
                {
                    RestorePhDValues();
                    isPhDActive = false;
                }
                return;
            }
            
            DetectAndModifyPhD();
        }

        public override void OnGUI(Rect area)
        {
            float yOffset = 0;
            
            // Main toggle
            DrawCheckbox(new Rect(area.x, area.y + yOffset, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            yOffset += 25;

            if (IsEnabled)
            {
                GUI.color = Color.white;
                
                // Sub-feature toggles
                DrawSubFeatureToggle(new Rect(area.x + 20, area.y + yOffset, 16, 16), infiniteLives, "Infinite Lives", 
                    (value) => { infiniteLives = value; SetPersistedValue("infiniteLives", value); });
                yOffset += 20;
                
                DrawSubFeatureToggle(new Rect(area.x + 20, area.y + yOffset, 16, 16), maxKillCount, "Max Kill Count", 
                    (value) => { maxKillCount = value; SetPersistedValue("maxKillCount", value); });
                yOffset += 20;
                
                DrawSubFeatureToggle(new Rect(area.x + 20, area.y + yOffset, 16, 16), phantomAvoidance, "Phantom Avoidance", 
                    (value) => { phantomAvoidance = value; SetPersistedValue("phantomAvoidance", value); });
                yOffset += 20;
                
                DrawSubFeatureToggle(new Rect(area.x + 20, area.y + yOffset, 16, 16), perfectTapAutomation, "Perfect Tap Auto", 
                    (value) => { perfectTapAutomation = value; SetPersistedValue("perfectTapAutomation", value); });
                yOffset += 20;
                
                // Status display
                if (isPhDActive)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(area.x + 20, area.y + yOffset, 200, 20), "PhD Active - Godmode ON");
                }
                else
                {
                    GUI.color = Color.gray;
                    GUI.Label(new Rect(area.x + 20, area.y + yOffset, 200, 20), "PhD Inactive");
                }
            }
        }

        private void DrawSubFeatureToggle(Rect position, bool currentValue, string label, System.Action<bool> onValueChanged)
        {
            // Smaller checkbox for sub-features
            Rect checkboxRect = new Rect(position.x, position.y, 12, 12);
            
            // Checkbox border
            GUI.color = Color.white;
            GUI.DrawTexture(checkboxRect, Texture2D.whiteTexture);
            
            // Checkbox inner area
            Rect innerRect = new Rect(checkboxRect.x + 1, checkboxRect.y + 1, 10, 10);
            
            // Set fill color based on state
            if (currentValue)
            {
                GUI.color = Color.white; // White when enabled
            }
            else
            {
                GUI.color = Color.black; // Black when disabled
            }
            
            // Draw the inner fill
            GUI.DrawTexture(innerRect, Texture2D.whiteTexture);
            
            // Reset color for button
            GUI.color = Color.white;
            
            // Invisible clickable button
            if (GUI.Button(checkboxRect, "", GUIStyle.none))
            {
                onValueChanged(!currentValue);
            }
            
            // Label
            GUI.color = Color.white;
            GUI.Label(new Rect(position.x + 18, position.y - 2, 200, 20), label);
        }

        private void DetectAndModifyPhD()
        {
            try
            {
                // Look for PhD game objects
                var allObjects = GameObject.FindObjectsOfType<GameObject>();
                bool foundPhDGame = false;
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;

                    // Check for PhDGame component
                    var phdGameComponent = obj.GetComponent<Il2CppMiniGames.PhD.PhDGame>();
                    if (phdGameComponent != null)
                    {
                        phdGame = phdGameComponent;
                        phdGameObject = obj;
                        foundPhDGame = true;
                        
                        if (!isPhDActive)
                        {
                            MelonLogger.Msg("[WildBerry] Phantom Dimension detected! Activating godmode features...");
                            isPhDActive = true;
                        }
                        
                        ApplyPhDModifications();
                        break;
                    }
                }


                if (!foundPhDGame && isPhDActive)
                {
                    MelonLogger.Msg("[WildBerry] PhD ended, deactivating godmode features.");
                    isPhDActive = false;
                    phdGame = null;
                    phdGameObject = null;
                    activeTapUIs.Clear();
                }
                
                // Handle battle state and perfect tap automation
                if (isPhDActive && perfectTapAutomation)
                {
                    HandlePerfectTapAutomation();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in PhD detection: {ex.Message}");
            }
        }

        private void ApplyPhDModifications()
        {
            if (phdGame == null) return;
            
            try
            {
                // Infinite Lives
                if (infiniteLives && phdGame.PlayerStrikes > -99999)
                {
                    phdGame.PlayerStrikes = -99999;
                    if (Time.frameCount % 120 == 0) // Every 2 seconds
                    {
                        MelonLogger.Msg($"[WildBerry] PhD: Applied infinite lives (Strikes: {phdGame.PlayerStrikes})");
                    }
                }

                // Max Kill Count for area unlocks
                if (maxKillCount && phdGame.KillCount < 99999)
                {
                    phdGame.KillCount = 99999;
                    MelonLogger.Msg($"[WildBerry] PhD: Set kill count to 99999 for area unlocks");
                }


                // Phantom Avoidance
                if (phantomAvoidance)
                {
                    ApplyPhantomAvoidance();
                }

            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying PhD modifications: {ex.Message}");
            }
        }

        private void ApplyPhantomAvoidance()
        {
            try
            {
                // Find all phantom enemies and disable their colliders (working method from debug)
                var phantoms = GameObject.FindObjectsOfType<Il2CppMiniGames.PhD.PhDEnemyPhantom>();
                int modifiedPhantoms = 0;
                
                foreach (var phantom in phantoms)
                {
                    if (phantom != null)
                    {
                        // Disable collider to prevent phantom attacks (confirmed working)
                        var collider = phantom.GetComponent<Collider>();
                        if (collider != null && collider.enabled)
                        {
                            collider.enabled = false;
                            modifiedPhantoms++;
                        }
                        
                        // Also try to disable the phantom GameObject to make them inactive
                        if (phantom.gameObject.activeInHierarchy)
                        {
                            phantom.gameObject.SetActive(false);
                        }
                    }
                }
                
                if (modifiedPhantoms > 0 && Time.frameCount % 180 == 0) // Every 3 seconds
                {
                    MelonLogger.Msg($"[WildBerry] PhD: Applied phantom avoidance to {modifiedPhantoms} phantoms");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in phantom avoidance: {ex.Message}");
            }
        }


        private void HandlePerfectTapAutomation()
        {
            try
            {
                bool isCurrentlyInBattle = phdGame != null && phdGame.IsInBattle;
                
                // Check if we just entered battle
                if (isCurrentlyInBattle && !wasInBattle)
                {
                    MelonLogger.Msg("[WildBerry] PhD: Entered battle - starting perfect tap automation via BattleArena.OnSequenceComplete");
                    wasInBattle = true;
                }
                else if (!isCurrentlyInBattle && wasInBattle)
                {
                    MelonLogger.Msg("[WildBerry] PhD: Exited battle - stopping perfect tap automation");
                    wasInBattle = false;
                    activeTapUIs.Clear();
                }

                if (!isCurrentlyInBattle) return;

                // Find BattleArena component - this is the key to perfect taps
                var battleArena = GameObject.FindObjectOfType<Il2CppMiniGames.PhD.BattleArena>();
                if (battleArena == null) return;

                // Monitor phantom HP to know when to stop attacking
                var currentPhantom = GetCurrentPhantomEnemy();
                if (currentPhantom == null) return;

                // Check phantom HP and only attack if it's alive
                float phantomHp = GetPhantomHealth(currentPhantom);
                if (phantomHp <= 0)
                {
                    MelonLogger.Msg("[WildBerry] PhD: Phantom is dead, stopping auto-tap");
                    return;
                }

                // Execute taps with human-like timing and occasional imperfection
                float tapInterval = UnityEngine.Random.Range(0.7f, 1.2f); // Vary between 0.7-1.2s 
                if (Time.time - lastTapTime > tapInterval)
                {
                    try
                    {
                        // Add realistic imperfection - 85% perfect, 15% good
                        Il2CppMiniGames.PhD.TapResult tapResult = Il2CppMiniGames.PhD.TapResult.Perfect;
                        if (UnityEngine.Random.Range(0f, 1f) < 0.15f) // 15% chance
                        {
                            tapResult = Il2CppMiniGames.PhD.TapResult.Good; // Sometimes use "Good" instead
                        }
                        
                        MelonLogger.Msg($"[WildBerry] PhD: Executing BattleArena.OnSequenceComplete({tapResult}) - Phantom HP: {phantomHp}");
                        
                        // Execute tap with result variation
                        battleArena.OnSequenceComplete(tapResult);
                        
                        lastTapTime = Time.time;
                        MelonLogger.Msg($"[WildBerry] PhD: {tapResult} TAP EXECUTED via BattleArena!");
                    }
                    catch (System.Exception tapEx)
                    {
                        MelonLogger.Error($"[WildBerry] PhD: BattleArena.OnSequenceComplete error: {tapEx.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in perfect tap automation: {ex.Message}");
            }
        }

        private Il2CppMiniGames.PhD.PhDEnemyPhantom GetCurrentPhantomEnemy()
        {
            try
            {
                // Find all active phantom enemies
                var phantoms = GameObject.FindObjectsOfType<Il2CppMiniGames.PhD.PhDEnemyPhantom>();
                foreach (var phantom in phantoms)
                {
                    if (phantom != null && phantom.gameObject.activeInHierarchy)
                    {
                        // Return the first active phantom (current target)
                        return phantom;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error finding current phantom: {ex.Message}");
            }
            return null;
        }

        private float GetPhantomHealth(Il2CppMiniGames.PhD.PhDEnemyPhantom phantom)
        {
            try
            {
                if (phantom == null) return 0f;

                // Try to access phantom health/HP property
                // Check common health property names
                var phantomType = phantom.GetType();
                
                // Look for HP/Health/CurrentHP properties
                var healthProp = phantomType.GetProperty("Health") ?? 
                               phantomType.GetProperty("HP") ?? 
                               phantomType.GetProperty("CurrentHP") ?? 
                               phantomType.GetProperty("CurrentHealth");
                
                if (healthProp != null)
                {
                    var healthValue = healthProp.GetValue(phantom);
                    if (healthValue != null)
                    {
                        return System.Convert.ToSingle(healthValue);
                    }
                }

                // Fallback: check if phantom is still alive by checking if it's active and enabled
                return phantom.gameObject.activeInHierarchy ? 100f : 0f;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error getting phantom health: {ex.Message}");
                // Default to alive if we can't check health
                return phantom != null && phantom.gameObject.activeInHierarchy ? 100f : 0f;
            }
        }



        private void RestorePhDValues()
        {
            try
            {
                if (phdGame != null)
                {
                    // Restore normal values if needed
                    MelonLogger.Msg("[WildBerry] PhD: Restored normal values");
                }
                
                activeTapUIs.Clear();
                wasInBattle = false;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring PhD values: {ex.Message}");
            }
        }

        public override void OnDisable()
        {
            if (isPhDActive)
            {
                RestorePhDValues();
                isPhDActive = false;
            }
        }


        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f; // Just checkbox height
            
            // When enabled, show all sub-toggles (22px each for better text spacing) + status (20px)
            // Base: main checkbox (25) + 4 sub-toggles (22 each) + status (20) + extra padding (5)
            return 25f + (4 * 22f) + 20f + 5f; // = 138px total (reduced by one toggle)
        }
    }
}