using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PlayWild.Features.Base;
using MelonLoader;

namespace PlayWild.Features.Minigames
{
    public class MassiveBallBlockBreak : BaseFeature
    {
        public override string Name => "Massive ball in Block Break";
        
        private HashSet<int> processedBalls = new HashSet<int>();

        public override void OnEnable()
        {
            processedBalls.Clear();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            ModifyBallScale();
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

        private void ModifyBallScale()
        {
            try
            {
                // Check if we're in the Breakout scene
                Scene currentScene = SceneManager.GetActiveScene();
                if (currentScene.name != "Breakout")
                {
                    return;
                }

                // Find all Ball(Clone) objects in the scene
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "Ball(Clone)")
                    {
                        int instanceId = obj.GetInstanceID();
                        
                        // Check if we've already processed this ball
                        if (!processedBalls.Contains(instanceId))
                        {
                            // Modify the scale to 20, 20, 1
                            obj.transform.localScale = new Vector3(20f, 20f, 1f);
                            processedBalls.Add(instanceId);
                            MelonLogger.Msg($"[WildBerry] Modified Ball(Clone) scale to (20, 20, 1) - ID: {instanceId}");
                        }
                        else
                        {
                            // Force set scale if it's been changed by something else
                            if (obj.transform.localScale != new Vector3(20f, 20f, 1f))
                            {
                                obj.transform.localScale = new Vector3(20f, 20f, 1f);
                                MelonLogger.Msg($"[WildBerry] Reset Ball(Clone) scale back to (20, 20, 1) - ID: {instanceId}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error modifying ball scale: {ex.Message}");
            }
        }
    }
}