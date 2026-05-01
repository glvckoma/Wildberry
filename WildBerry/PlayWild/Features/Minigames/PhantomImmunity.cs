using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
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
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                DrawLabel(new Rect(area.x + 25, area.y + 25, 100, 20), "Gem Magnet:");

                string newInput = DrawStyledTextField(
                    new Rect(area.x + 130, area.y + 25, 50, WildBerryTheme.TextFieldHeight),
                    gemMultiplierInput
                );

                if (newInput != gemMultiplierInput)
                {
                    gemMultiplierInput = newInput;

                    if (float.TryParse(gemMultiplierInput, out float newMultiplier) && newMultiplier > 0)
                    {
                        gemCollectionMultiplier = newMultiplier;
                        SetPersistedValue("gemCollectionMultiplier", gemCollectionMultiplier);
                        if (isFingerDodgeActive)
                        {
                            ApplyGemMagnet();
                        }
                    }
                }

                DrawLabel(new Rect(area.x + 185, area.y + 25, 30, 20), $"{gemCollectionMultiplier:F1}x");
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
                return WildBerryTheme.ToggleHeight;
            return 54f;
        }

        private void DetectAndModifyFingerDodge()
        {
            var fingerDodgeObject = GameObject.Find("FingerDodge_GameManager");
            bool gameActive = fingerDodgeObject != null;

            if (!gameActive)
            {
                var fingerDodgePlayer = GameObject.Find("FingerDodge_Player");
                gameActive = fingerDodgePlayer != null;
                if (gameActive)
                {
                    fingerDodgeObject = fingerDodgePlayer;
                }
            }

            if (gameActive && !isFingerDodgeActive)
            {
                MelonLogger.Msg("[WildBerry] Finger Dodge detected! Enabling phantom immunity and gem magnet...");
                fingerDodgeGameObject = fingerDodgeObject;
                ApplyPhantomImmunity();
                ApplyGemMagnet();
                isFingerDodgeActive = true;
            }
            else if (!gameActive && isFingerDodgeActive)
            {
                MelonLogger.Msg("[WildBerry] Finger Dodge ended, phantom immunity and gem magnet disabled.");
                isFingerDodgeActive = false;
                fingerDodgeGameObject = null;
            }
            else if (gameActive && Time.frameCount % 30 == 0)
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

                var fingerDodgePlayer = GameObject.Find("FingerDodge_Player");
                if (fingerDodgePlayer == null) return;

                DisablePhantomCollisions();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying phantom immunity: {ex.Message}");
            }
        }

        private void DisablePhantomCollisions()
        {
            try
            {
                var allColliders = UnityEngine.Object.FindObjectsOfType<Collider2D>();
                int disabledCount = 0;

                foreach (var collider in allColliders)
                {
                    if (collider == null || !collider.enabled) continue;

                    var obj = collider.gameObject;
                    if (IsPhantomObstacleByName(obj.name) && !IsGemOrCollectible(obj))
                    {
                        collider.enabled = false;
                        disabledCount++;
                    }
                }

                if (disabledCount > 0 && Time.frameCount % 60 == 0)
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
            string name = obj.name.ToLower();
            return name.Contains("gem") || name.Contains("pickup") || name.Contains("coin") ||
                   name.Contains("collectible") || name.Contains("prize") || name.Contains("reward") ||
                   (name.Contains("fingerdodge_") && (name.Contains("purple") || name.Contains("blue") ||
                   name.Contains("red") || name.Contains("green") || name.Contains("yellow")));
        }

        private void ApplyGemMagnet()
        {
            try
            {
                var allColliders = UnityEngine.Object.FindObjectsOfType<Collider2D>();
                int expandedCount = 0;

                foreach (var collider in allColliders)
                {
                    if (collider == null) continue;

                    var obj = collider.gameObject;
                    if (!IsGemOrCollectible(obj)) continue;

                    if (collider is CircleCollider2D circleCollider)
                    {
                        if (circleCollider.radius <= 1.0f)
                        {
                            circleCollider.radius = gemCollectionMultiplier;
                            expandedCount++;
                        }
                    }
                    else if (collider is BoxCollider2D boxCollider)
                    {
                        if (boxCollider.size.magnitude <= 2.0f)
                        {
                            boxCollider.size = Vector2.one * gemCollectionMultiplier;
                            expandedCount++;
                        }
                    }
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
                var allColliders = UnityEngine.Object.FindObjectsOfType<Collider2D>();

                foreach (var collider in allColliders)
                {
                    if (collider == null || collider.enabled) continue;

                    var obj = collider.gameObject;
                    if (IsPhantomObstacleByName(obj.name) && !IsGemOrCollectible(obj))
                    {
                        collider.enabled = true;
                    }
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
                var allColliders = UnityEngine.Object.FindObjectsOfType<Collider2D>();

                foreach (var collider in allColliders)
                {
                    if (collider == null) continue;

                    var obj = collider.gameObject;
                    if (!IsGemOrCollectible(obj)) continue;

                    if (collider is CircleCollider2D circleCollider && circleCollider.radius > 1.0f)
                    {
                        circleCollider.radius = 0.5f;
                    }
                    else if (collider is BoxCollider2D boxCollider && boxCollider.size.magnitude > 2.0f)
                    {
                        boxCollider.size = Vector2.one;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring gem sizes: {ex.Message}");
            }
        }
    }
}
