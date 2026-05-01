using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PlayWild.Features.Base;
using PlayWild.Interface;
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
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });
        }

        public override float GetDynamicHeight()
        {
            return WildBerryTheme.ToggleHeight;
        }

        private void ModifyBallScale()
        {
            try
            {
                Scene currentScene = SceneManager.GetActiveScene();
                if (currentScene.name != "Breakout")
                    return;

                GameObject ball = GameObject.Find("Ball(Clone)");
                if (ball == null) return;

                int instanceId = ball.GetInstanceID();

                if (!processedBalls.Contains(instanceId))
                {
                    ball.transform.localScale = new Vector3(20f, 20f, 1f);
                    processedBalls.Add(instanceId);
                    MelonLogger.Msg($"[WildBerry] Modified Ball(Clone) scale to (20, 20, 1)");
                }
                else if (ball.transform.localScale != new Vector3(20f, 20f, 1f))
                {
                    ball.transform.localScale = new Vector3(20f, 20f, 1f);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error modifying ball scale: {ex.Message}");
            }
        }
    }
}
