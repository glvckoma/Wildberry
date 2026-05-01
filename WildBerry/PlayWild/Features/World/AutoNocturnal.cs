using UnityEngine;
using System.Collections.Generic;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class AutoNocturnal : BaseFeature
    {
        public override string Name => "Auto Nocturnal";

        private HashSet<int> processedNocturnals = new HashSet<int>();
        private float lastScanTime = 0f;
        private const float SCAN_INTERVAL = 2f;

        public override void OnEnable()
        {
            processedNocturnals.Clear();
            lastScanTime = 0f;
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            if (Time.time - lastScanTime < SCAN_INTERVAL) return;
            lastScanTime = Time.time;
            AutoClickNocturnals();
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

        private void AutoClickNocturnals()
        {
            try
            {
                var nocturnalSpawns = UnityEngine.Object.FindObjectsOfType<Il2Cpp.RoomItemSpawn_NocturnalAnimals>(true);
                if (nocturnalSpawns == null) return;

                foreach (var nocturnalComponent in nocturnalSpawns)
                {
                    if (nocturnalComponent == null) continue;

                    int instanceId = nocturnalComponent.gameObject.GetInstanceID();
                    if (processedNocturnals.Contains(instanceId)) continue;

                    if (!nocturnalComponent.gameObject.activeInHierarchy)
                        nocturnalComponent.gameObject.SetActive(true);

                    var touchable = nocturnalComponent.GetComponent<Il2Cpp.Touchable>();
                    if (touchable == null)
                        touchable = nocturnalComponent.GetComponentInChildren<Il2Cpp.Touchable>(true);

                    if (touchable == null)
                    {
                        MelonLogger.Warning($"[WildBerry] No Touchable found on nocturnal: {nocturnalComponent.gameObject.name}");
                        continue;
                    }

                    nocturnalComponent.OnTouched(touchable);
                    processedNocturnals.Add(instanceId);
                    MelonLogger.Msg($"[WildBerry] Auto-clicked nocturnal: {nocturnalComponent.gameObject.name}");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in AutoClickNocturnals: {ex.Message}");
            }
        }
    }
}
