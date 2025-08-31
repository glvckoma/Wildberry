using UnityEngine;
using System.Collections.Generic;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class AutoFact : BaseFeature
    {
        public override string Name => "Auto Fact";
        
        private HashSet<int> processedFacts = new HashSet<int>();
        private float lastFactClickTime = 0f;
        private const float FACT_CLICK_DELAY = 0.3f; // 300ms delay between fact clicks

        public override void OnEnable()
        {
            processedFacts.Clear();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            AutoClickFacts();
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

        private void AutoClickFacts()
        {
            try
            {
                // Check if enough time has passed since last click
                if (Time.time - lastFactClickTime < FACT_CLICK_DELAY)
                {
                    return; // Wait for delay
                }

                // Find all UI_Facts_Button objects in the entire scene
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                int totalFound = 0;
                bool clickedThisFrame = false;

                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "UI_Facts_Button")
                    {
                        totalFound++;
                        int instanceId = obj.GetInstanceID();
                        
                        // Skip if already processed
                        if (processedFacts.Contains(instanceId)) continue;

                        // Try to click the fact button using the working method
                        var factsButtonComponent = obj.GetComponent<Il2Cpp.UI_FactsButton>();
                        if (factsButtonComponent != null)
                        {
                            try
                            {
                                factsButtonComponent.BtnClicked_OpenFactWindow();
                                processedFacts.Add(instanceId);
                                lastFactClickTime = Time.time; // Record click time
                                clickedThisFrame = true;
                                MelonLogger.Msg($"[WildBerry] Auto-clicked fact button: {obj.name} (path: {GetGameObjectPath(obj)})");
                                return; // Exit after clicking one button to enforce delay
                            }
                            catch (System.Exception ex)
                            {
                                MelonLogger.Warning($"[WildBerry] Failed to click fact button {obj.name}: {ex.Message}");
                                processedFacts.Add(instanceId); // Mark as processed to avoid spam
                            }
                        }
                        else
                        {
                            MelonLogger.Warning($"[WildBerry] No UI_FactsButton component on {obj.name}");
                            processedFacts.Add(instanceId); // Mark as processed to avoid spam
                        }
                    }
                }

                // Log summary when all buttons have been processed
                if (!clickedThisFrame && totalFound > 0)
                {
                    int processedCount = 0;
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name == "UI_Facts_Button" && processedFacts.Contains(obj.GetInstanceID()))
                        {
                            processedCount++;
                        }
                    }
                    
                    if (processedCount == totalFound && Time.time - lastFactClickTime > 1f) // Only log once per second when done
                    {
                        MelonLogger.Msg($"[WildBerry] Auto Fact complete: Processed {processedCount}/{totalFound} fact buttons");
                        lastFactClickTime = Time.time; // Prevent spam logging
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in AutoClickFacts: {ex.Message}");
            }
        }
    }
}