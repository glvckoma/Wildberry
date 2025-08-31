using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.World
{
    public class AutoNocturnal : BaseFeature
    {
        public override string Name => "Auto Nocturnal";
        
        private HashSet<int> processedNocturnals = new HashSet<int>();

        public override void OnEnable()
        {
            processedNocturnals.Clear();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            AutoClickNocturnals();
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

        private void AutoClickNocturnals()
        {
            try
            {
                // Look for SharedJamaaAssets/LevelEventManager/Night_Nocturnals/ path
                GameObject sharedJamaaAssets = GameObject.Find("SharedJamaaAssets");
                if (sharedJamaaAssets == null) return;

                Transform levelEventManager = sharedJamaaAssets.transform.Find("LevelEventManager");
                if (levelEventManager == null) return;

                Transform nightNocturnals = levelEventManager.Find("Night_Nocturnals");
                if (nightNocturnals == null) return;

                // Search for any Nocturnal_* objects in Night_Nocturnals
                for (int i = 0; i < nightNocturnals.childCount; i++)
                {
                    Transform child = nightNocturnals.GetChild(i);
                    if (child.name.StartsWith("Nocturnal_"))
                    {
                        // Process this nocturnal category and all its children recursively
                        ProcessNocturnalCategoryRecursive(child);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in AutoClickNocturnals: {ex.Message}");
            }
        }

        private void ProcessNocturnalCategoryRecursive(Transform nocturnalCategory)
        {
            try
            {
                // First, try to process this object directly
                ProcessNocturnalObject(nocturnalCategory.gameObject);

                // Then recursively process all children
                for (int i = 0; i < nocturnalCategory.childCount; i++)
                {
                    Transform child = nocturnalCategory.GetChild(i);
                    ProcessNocturnalCategoryRecursive(child);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error processing nocturnal category {nocturnalCategory.name}: {ex.Message}");
            }
        }

        private void ProcessNocturnalObject(GameObject nocturnalObject)
        {
            try
            {
                int instanceId = nocturnalObject.GetInstanceID();
                
                // Skip if already processed
                if (processedNocturnals.Contains(instanceId)) return;

                // Try to get the nocturnal component using Il2Cpp wrapper
                var nocturnalComponent = nocturnalObject.GetComponent<Il2Cpp.RoomItemSpawn_NocturnalAnimals>();
                if (nocturnalComponent != null)
                {
                    // Get the Touchable component for OnTouched parameter
                    var touchableComponent = nocturnalObject.GetComponent<Il2Cpp.Touchable>();
                    if (touchableComponent != null)
                    {
                        // Auto-click the nocturnal
                        nocturnalComponent.OnTouched(touchableComponent);
                        processedNocturnals.Add(instanceId);
                        MelonLogger.Msg($"[WildBerry] Auto-clicked nocturnal: {nocturnalObject.name} (path: {GetGameObjectPath(nocturnalObject)})");
                        return;
                    }
                }

                // Also check all direct children for nocturnal components
                for (int i = 0; i < nocturnalObject.transform.childCount; i++)
                {
                    GameObject child = nocturnalObject.transform.GetChild(i).gameObject;
                    int childInstanceId = child.GetInstanceID();
                    
                    if (processedNocturnals.Contains(childInstanceId)) continue;

                    var childNocturnalComponent = child.GetComponent<Il2Cpp.RoomItemSpawn_NocturnalAnimals>();
                    if (childNocturnalComponent != null)
                    {
                        var childTouchableComponent = child.GetComponent<Il2Cpp.Touchable>();
                        if (childTouchableComponent != null)
                        {
                            childNocturnalComponent.OnTouched(childTouchableComponent);
                            processedNocturnals.Add(childInstanceId);
                            MelonLogger.Msg($"[WildBerry] Auto-clicked nocturnal: {child.name} (path: {GetGameObjectPath(child)})");
                            return;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error processing nocturnal object {nocturnalObject.name}: {ex.Message}");
            }
        }
    }
}