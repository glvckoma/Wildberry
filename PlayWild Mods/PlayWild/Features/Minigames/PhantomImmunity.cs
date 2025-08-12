using UnityEngine;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.Minigames
{
    public class PhantomImmunity : BaseFeature
    {
        public override string Name => "Phantom Dodger Godmode + Huge Gems";
        
        private bool isFingerDodgeActive = false;
        private GameObject fingerDodgeGameObject = null;
        private float gemCollectionMultiplier = 3.0f;
        private string gemMultiplierInput = "3.0";
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Load persisted values
            gemCollectionMultiplier = GetPersistedValue("gemCollectionMultiplier", 3.0f);
            gemMultiplierInput = gemCollectionMultiplier.ToString();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            
            DetectAndModifyFingerDodge();
        }

        public override void OnGUI(Rect area)
        {
            // Checkbox
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });

            // If enabled, show gem magnet multiplier input
            if (IsEnabled)
            {
                // Multiplier label and text input
                GUI.color = Color.white;
                GUI.Label(new Rect(area.x + 25, area.y + 25, 100, 20), "Gem Magnet:");
                
                string newInput = GUI.TextField(
                    new Rect(area.x + 130, area.y + 25, 50, 20), 
                    gemMultiplierInput
                );
                
                if (newInput != gemMultiplierInput)
                {
                    gemMultiplierInput = newInput;
                    
                    // Try to parse the input
                    if (float.TryParse(gemMultiplierInput, out float newMultiplier) && newMultiplier > 0)
                    {
                        gemCollectionMultiplier = newMultiplier;
                        SetPersistedValue("gemCollectionMultiplier", gemCollectionMultiplier);
                        if (isFingerDodgeActive)
                        {
                            ApplyGemMagnet(); // Reapply with new size
                        }
                    }
                }
                
                // Show current value
                GUI.Label(new Rect(area.x + 185, area.y + 25, 30, 20), $"{gemCollectionMultiplier:F1}x");
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return 25f; // Just checkbox height
            }
            
            // Checkbox (25) + Gem Magnet label and input (25)
            return 50f;
        }

        private void DetectAndModifyFingerDodge()
        {
            // Check if Finger Dodge minigame is currently active
            var fingerDodgeObject = GameObject.Find("FingerDodge_GameManager");
            bool gameActive = fingerDodgeObject != null;
            
            // If we don't find the manager directly, try finding other finger dodge objects
            if (!gameActive)
            {
                var fingerDodgePlayer = GameObject.Find("FingerDodge_Player");
                gameActive = fingerDodgePlayer != null;
                if (gameActive)
                {
                    fingerDodgeObject = fingerDodgePlayer;
                }
            }

            // Game became active
            if (gameActive && !isFingerDodgeActive)
            {
                MelonLogger.Msg("[WildBerry] Finger Dodge detected! Enabling phantom immunity and gem magnet...");
                fingerDodgeGameObject = fingerDodgeObject;
                ApplyPhantomImmunity();
                ApplyGemMagnet();
                isFingerDodgeActive = true;
            }
            // Game became inactive
            else if (!gameActive && isFingerDodgeActive)
            {
                MelonLogger.Msg("[WildBerry] Finger Dodge ended, phantom immunity and gem magnet disabled.");
                isFingerDodgeActive = false;
                fingerDodgeGameObject = null;
            }
            // Continue applying immunity and gem magnet while game is active
            else if (gameActive && Time.frameCount % 30 == 0) // Every ~0.5 seconds
            {
                ApplyPhantomImmunity();
                ApplyGemMagnet();
            }
        }

        private void ApplyPhantomImmunity()
        {
            try
            {
                if (fingerDodgeGameObject == null) return;

                // Find the FingerDodgePlayer component
                var fingerDodgePlayer = FindFingerDodgePlayer();
                if (fingerDodgePlayer == null)
                {
                    MelonLogger.Warning("[WildBerry] Could not find FingerDodgePlayer!");
                    return;
                }

                // Disable colliders on phantom obstacles while preserving gem collection
                DisablePhantomCollisions();
                
                MelonLogger.Msg("[WildBerry] Phantom immunity applied!");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying phantom immunity: {ex.Message}");
            }
        }

        private GameObject FindFingerDodgePlayer()
        {
            try
            {
                // Look for the actual Finger Dodge player object
                var player = GameObject.Find("FingerDodge_Player");
                if (player != null)
                {
                    return player;
                }

                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error finding FingerDodgePlayer: {ex.Message}");
                return null;
            }
        }

        private void DisablePhantomCollisions()
        {
            try
            {
                // Find obstacle objects by naming patterns only
                var allObjects = GameObject.FindObjectsOfType<GameObject>();
                int disabledCount = 0;
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;

                    // Check if this object is likely a phantom obstacle by name
                    if (IsPhantomObstacleByName(obj.name) && !IsGemOrCollectible(obj))
                    {
                        // Disable colliders on this obstacle
                        var colliders = obj.GetComponents<Collider2D>();
                        foreach (var collider in colliders)
                        {
                            if (collider != null && collider.enabled)
                            {
                                collider.enabled = false;
                                disabledCount++;
                            }
                        }
                    }
                }
                
                if (disabledCount > 0)
                {
                    MelonLogger.Msg($"[WildBerry] Disabled {disabledCount} phantom colliders!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error disabling phantom collisions: {ex.Message}");
            }
        }

        private bool IsGemOrCollectible(GameObject obj)
        {
            // Check if object is likely a collectible gem vs a phantom obstacle
            string name = obj.name.ToLower();
            
            // Include gem pickups and other collectibles
            return name.Contains("gem") || name.Contains("pickup") || name.Contains("coin") || 
                   name.Contains("collectible") || name.Contains("prize") || name.Contains("reward") ||
                   (name.Contains("fingerdodge_") && (name.Contains("purple") || name.Contains("blue") || 
                   name.Contains("red") || name.Contains("green") || name.Contains("yellow")));
        }

        private void ApplyGemMagnet()
        {
            try
            {
                var allObjects = GameObject.FindObjectsOfType<GameObject>();
                int expandedCount = 0;
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;

                    // Check if this object is a gem
                    if (IsGemOrCollectible(obj))
                    {
                        // Expand colliders on this gem
                        var colliders = obj.GetComponents<Collider2D>();
                        foreach (var collider in colliders)
                        {
                            if (collider != null)
                            {
                                // Method 1: Scale the collider directly if possible
                                if (collider is CircleCollider2D circleCollider)
                                {
                                    // Store original radius if not already stored
                                    if (circleCollider.radius <= 1.0f) // Assume original radius is <= 1
                                    {
                                        circleCollider.radius = gemCollectionMultiplier;
                                        expandedCount++;
                                    }
                                }
                                else if (collider is BoxCollider2D boxCollider)
                                {
                                    // Store original size if not already stored
                                    if (boxCollider.size.magnitude <= 2.0f) // Assume original size is small
                                    {
                                        boxCollider.size = Vector2.one * gemCollectionMultiplier;
                                        expandedCount++;
                                    }
                                }
                                else
                                {
                                    // Method 2: Scale the entire object (affects visual and collider)
                                    if (obj.transform.localScale.magnitude <= 3.0f) // Don't re-scale already scaled objects
                                    {
                                        obj.transform.localScale = Vector3.one * gemCollectionMultiplier;
                                        expandedCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (expandedCount > 0)
                {
                    MelonLogger.Msg($"[WildBerry] Expanded {expandedCount} gem colliders to {gemCollectionMultiplier:F1}x size!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error expanding gem collection: {ex.Message}");
            }
        }

        private bool IsPhantomObstacleByName(string name)
        {
            string lowerName = name.ToLower();
            
            // Specific phantom object names found in Finger Dodge
            return lowerName.Contains("phantom_sphere") || 
                   lowerName.Contains("phantom spike") || 
                   lowerName.Contains("phantom_spike") ||
                   lowerName.Contains("round_phantom") ||
                   lowerName.Contains("spike_phantom") ||
                   lowerName.Contains("combinedphantoms") ||
                   (lowerName.Contains("phantom") && !lowerName.Contains("gem"));
        }

        public override void OnDisable()
        {
            // Re-enable collisions and restore gem sizes when feature is disabled
            try
            {
                if (isFingerDodgeActive)
                {
                    RestorePhantomCollisions();
                    RestoreGemSizes();
                    MelonLogger.Msg("[WildBerry] Phantom immunity and gem magnet disabled, everything restored.");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring collisions: {ex.Message}");
            }
        }

        private void RestorePhantomCollisions()
        {
            try
            {
                // Re-enable all colliders that we may have disabled
                var allObjects = GameObject.FindObjectsOfType<GameObject>();
                int restoredCount = 0;
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;

                    // Check if this is likely an obstacle we disabled by name pattern
                    if (IsPhantomObstacleByName(obj.name) && !IsGemOrCollectible(obj))
                    {
                        var colliders = obj.GetComponents<Collider2D>();
                        foreach (var collider in colliders)
                        {
                            if (collider != null && !collider.enabled)
                            {
                                collider.enabled = true;
                                restoredCount++;
                            }
                        }
                    }
                }
                
                if (restoredCount > 0)
                {
                    MelonLogger.Msg($"[WildBerry] Restored {restoredCount} phantom colliders!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring phantom collisions: {ex.Message}");
            }
        }

        private void RestoreGemSizes()
        {
            try
            {
                var allObjects = GameObject.FindObjectsOfType<GameObject>();
                int restoredCount = 0;
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;

                    // Check if this is likely a gem we expanded
                    if (IsGemOrCollectible(obj))
                    {
                        var colliders = obj.GetComponents<Collider2D>();
                        foreach (var collider in colliders)
                        {
                            if (collider != null)
                            {
                                // Restore collider sizes to reasonable defaults
                                if (collider is CircleCollider2D circleCollider && circleCollider.radius > 1.0f)
                                {
                                    circleCollider.radius = 0.5f; // Default gem radius
                                    restoredCount++;
                                }
                                else if (collider is BoxCollider2D boxCollider && boxCollider.size.magnitude > 2.0f)
                                {
                                    boxCollider.size = Vector2.one; // Default gem size
                                    restoredCount++;
                                }
                            }
                        }
                        
                        // Restore object scale if it was modified
                        if (obj.transform.localScale.magnitude > 2.0f)
                        {
                            obj.transform.localScale = Vector3.one;
                            restoredCount++;
                        }
                    }
                }
                
                if (restoredCount > 0)
                {
                    MelonLogger.Msg($"[WildBerry] Restored {restoredCount} gem sizes!");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring gem sizes: {ex.Message}");
            }
        }
    }
}