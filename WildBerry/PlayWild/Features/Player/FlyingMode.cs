using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using PlayWild.Utils;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class FlyingMode : BaseFeature
    {
        public override string Name => "Flying Mode";

        private float speedMultiplier = 3.0f;
        private string speedInputText = "3.0";
        private bool isFlying = false;
        private bool noclip = false;
        private Collider[] disabledColliders;

        public override void Initialize()
        {
            base.Initialize();
            speedMultiplier = GetPersistedValue("flySpeed", 3.0f);
            speedInputText = speedMultiplier.ToString();
            noclip = GetPersistedValue("noclip", false);
        }

        public override void OnDisable()
        {
            GameObject player = PlayerFinder.FindPlayerObject();
            if (player != null)
            {
                var rigidbody = player.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.useGravity = true;
                }
            }
            RestoreColliders();
            isFlying = false;
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            ApplyFlyingMode();
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 60, 20), "Speed:");
                string newSpeedText = DrawStyledTextField(new Rect(area.x + 65, area.y + yOffset, 50, 20), speedInputText);
                if (newSpeedText != speedInputText)
                {
                    speedInputText = newSpeedText;
                    if (float.TryParse(speedInputText, out float newSpeed) && newSpeed > 0)
                    {
                        speedMultiplier = newSpeed;
                        SetPersistedValue("flySpeed", speedMultiplier);
                    }
                }
                yOffset += 22;

                DrawSubToggle(new Rect(area.x + 5, area.y + yOffset, area.width - 5, WildBerryTheme.SubToggleHeight), noclip, "Noclip",
                    (value) =>
                    {
                        noclip = value;
                        SetPersistedValue("noclip", value);
                        if (!value) RestoreColliders();
                    });
                yOffset += 22;

                if (isFlying)
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Flying: ON", Color.green);
                }
                else
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Flying: OFF", Color.gray);
                }
                yOffset += 20;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Space: Toggle | WASD: Move");
                yOffset += 18;
                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Shift: Up | Ctrl: Down");
            }
        }

        private void ApplyFlyingMode()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player == null) return;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isFlying = !isFlying;
                    if (isFlying && noclip) DisableColliders(player);
                    if (!isFlying) RestoreColliders();
                    MelonLogger.Msg($"[WildBerry] Flying {(isFlying ? "enabled" : "disabled")}");
                }

                if (isFlying)
                {
                    Vector3 flyMovement = Vector3.zero;
                    if (Input.GetKey(KeyCode.W)) flyMovement += Vector3.forward;
                    if (Input.GetKey(KeyCode.S)) flyMovement += Vector3.back;
                    if (Input.GetKey(KeyCode.A)) flyMovement += Vector3.left;
                    if (Input.GetKey(KeyCode.D)) flyMovement += Vector3.right;
                    if (Input.GetKey(KeyCode.LeftShift)) flyMovement += Vector3.up;
                    if (Input.GetKey(KeyCode.LeftControl)) flyMovement += Vector3.down;

                    var rigidbody = player.GetComponent<Rigidbody>();

                    if (flyMovement != Vector3.zero)
                    {
                        if (rigidbody != null)
                        {
                            rigidbody.useGravity = false;
                            rigidbody.velocity = flyMovement * speedMultiplier * 8f;
                            return;
                        }

                        var characterController = player.GetComponent<CharacterController>();
                        if (characterController != null)
                        {
                            characterController.Move(flyMovement * speedMultiplier * Time.deltaTime * 10f);
                            return;
                        }

                        player.transform.position += flyMovement * speedMultiplier * Time.deltaTime * 5f;
                    }
                    else
                    {
                        if (rigidbody != null)
                        {
                            rigidbody.velocity = Vector3.zero;
                            rigidbody.useGravity = false;
                        }
                    }
                }
                else
                {
                    var rigidbody = player.GetComponent<Rigidbody>();
                    if (rigidbody != null)
                    {
                        rigidbody.useGravity = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying flying mode: {ex.Message}");
            }
        }

        private void DisableColliders(GameObject player)
        {
            try
            {
                disabledColliders = player.GetComponentsInChildren<Collider>();
                foreach (var col in disabledColliders)
                {
                    if (col != null) col.enabled = false;
                }
            }
            catch { }
        }

        private void RestoreColliders()
        {
            try
            {
                if (disabledColliders != null)
                {
                    foreach (var col in disabledColliders)
                    {
                        if (col != null) col.enabled = true;
                    }
                    disabledColliders = null;
                }
            }
            catch { }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f;
            return 135f;
        }
    }
}
