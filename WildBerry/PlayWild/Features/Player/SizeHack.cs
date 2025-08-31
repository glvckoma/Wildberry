using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Utils;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class SizeHack : BaseFeature
    {
        public override string Name => "Size Hack";
        
        private float sizeMultiplier = 2.0f;
        private string sizeInputText = "2.0";
        private Vector3 originalPlayerScale = Vector3.one;
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Load persisted values
            sizeMultiplier = GetPersistedValue("sizeMultiplier", 2.0f);
            sizeInputText = sizeMultiplier.ToString();
        }

        public override void OnDisable()
        {
            RestorePlayerSize();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            ApplySizeHack();
        }

        public override void OnGUI(Rect area)
        {
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            
            if (IsEnabled)
            {
                float yOffset = 25;
                GUI.color = Color.white;
                
                // Size input field
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 100, 20), "Size multiplier:");
                yOffset += 20;
                
                string newSizeText = GUI.TextField(new Rect(area.x + 5, area.y + yOffset, 80, 20), sizeInputText);
                if (newSizeText != sizeInputText)
                {
                    sizeInputText = newSizeText;
                    if (float.TryParse(sizeInputText, out float newSize) && newSize > 0)
                    {
                        sizeMultiplier = newSize;
                        SetPersistedValue("sizeMultiplier", sizeMultiplier);
                    }
                }
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return 25f; // Just checkbox height
            }
            
            // Checkbox (25) + Label (20) + TextField (25)
            return 70f;
        }

        private void ApplySizeHack()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player == null) return;

                if (originalPlayerScale == Vector3.one)
                {
                    originalPlayerScale = player.transform.localScale;
                }

                Vector3 targetScale = originalPlayerScale * sizeMultiplier;
                if (player.transform.localScale != targetScale)
                {
                    player.transform.localScale = targetScale;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying size hack: {ex.Message}");
            }
        }

        private void RestorePlayerSize()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player != null && originalPlayerScale != Vector3.one)
                {
                    player.transform.localScale = originalPlayerScale;
                    MelonLogger.Msg("[WildBerry] Player size restored to original");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error restoring player size: {ex.Message}");
            }
        }
    }
}