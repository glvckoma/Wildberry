using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using PlayWild.Utils;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class SpeedHack : BaseFeature
    {
        public override string Name => "Speed Hack";

        private float speedMultiplier = 3.0f;
        private string speedInputText = "3.0";

        public override void Initialize()
        {
            base.Initialize();

            speedMultiplier = GetPersistedValue("speedMultiplier", 3.0f);
            speedInputText = speedMultiplier.ToString();
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            ApplySpeedHack();
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 100, 20), "Speed multiplier:");
                yOffset += 20;

                string newSpeedText = DrawStyledTextField(new Rect(area.x + 5, area.y + yOffset, 80, 20), speedInputText);
                if (newSpeedText != speedInputText)
                {
                    speedInputText = newSpeedText;
                    if (float.TryParse(speedInputText, out float newSpeed) && newSpeed > 0)
                    {
                        speedMultiplier = newSpeed;
                        SetPersistedValue("speedMultiplier", speedMultiplier);
                    }
                }
                yOffset += 25;
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return 25f;
            }

            return 70f;
        }

        private void ApplySpeedHack()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player == null) return;

                Vector3 movement = Vector3.zero;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement += Vector3.forward;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement += Vector3.back;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement += Vector3.left;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement += Vector3.right;

                if (movement != Vector3.zero)
                {
                    var rigidbody = player.GetComponent<Rigidbody>();
                    if (rigidbody != null)
                    {
                        movement = player.transform.TransformDirection(movement);
                        rigidbody.AddForce(movement * speedMultiplier * 15f, ForceMode.Force);
                        return;
                    }

                    var characterController = player.GetComponent<CharacterController>();
                    if (characterController != null)
                    {
                        movement = player.transform.TransformDirection(movement);
                        characterController.Move(movement * speedMultiplier * Time.deltaTime * 5f);
                        return;
                    }

                    movement = player.transform.TransformDirection(movement);
                    player.transform.position += movement * speedMultiplier * Time.deltaTime * 3f;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying speed hack: {ex.Message}");
            }
        }
    }
}
