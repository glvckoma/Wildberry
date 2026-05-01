using UnityEngine;
using System.Collections.Generic;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class AutoFact : BaseFeature
    {
        public override string Name => "Auto Fact";

        private HashSet<int> processedFacts = new HashSet<int>();
        private float lastFactClickTime = 0f;
        private const float FACT_CLICK_DELAY = 0.3f;

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
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });
        }

        public override float GetDynamicHeight()
        {
            return WildBerryTheme.ToggleHeight;
        }

        private void AutoClickFacts()
        {
            try
            {
                if (Time.time - lastFactClickTime < FACT_CLICK_DELAY)
                    return;

                var factButtons = UnityEngine.Object.FindObjectsOfType<Il2Cpp.UI_FactsButton>();

                foreach (var factsButtonComponent in factButtons)
                {
                    if (factsButtonComponent == null) continue;

                    int instanceId = factsButtonComponent.GetInstanceID();

                    if (processedFacts.Contains(instanceId)) continue;

                    try
                    {
                        factsButtonComponent.BtnClicked_OpenFactWindow();
                        processedFacts.Add(instanceId);
                        lastFactClickTime = Time.time;
                        MelonLogger.Msg($"[WildBerry] Auto-clicked fact button: {factsButtonComponent.gameObject.name}");
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Warning($"[WildBerry] Failed to click fact button: {ex.Message}");
                        processedFacts.Add(instanceId);
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
