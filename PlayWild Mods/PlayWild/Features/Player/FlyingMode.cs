using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Utils;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class FlyingMode : BaseFeature
    {
        public override string Name => "Flying Mode";
        
        private float speedMultiplier = 3.0f;
        private bool isFlying = false;

        public override void OnDisable()
        {
            // Restore gravity when disabled
            GameObject player = PlayerFinder.FindPlayerObject();
            if (player != null)
            {
                var rigidbody = player.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.useGravity = true;
                }
            }
            isFlying = false;
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;
            ApplyFlyingMode();
        }

        public override void OnGUI(Rect area)
        {
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            
            if (IsEnabled)
            {
                float yOffset = 25;
                GUI.color = Color.white;
                
                // Only show flying status when active
                if (isFlying)
                {
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Flying: ON");
                    yOffset += 20;
                }
                
                // Control instructions
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Controls:");
                yOffset += 20;
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Space: Toggle flying");
                yOffset += 20;
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "WASD: Move");
                yOffset += 20;
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Shift: Up, Ctrl: Down");
            }
        }

        private void ApplyFlyingMode()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player == null) return;

                // Toggle flying with Space key
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isFlying = !isFlying;
                    MelonLogger.Msg($"[WildBerry] Flying {(isFlying ? "enabled" : "disabled")}! Use WASD + Shift/Ctrl for movement");
                }

                // Flying controls
                if (isFlying)
                {
                    Vector3 flyMovement = Vector3.zero;
                    if (Input.GetKey(KeyCode.W)) flyMovement += Vector3.forward;
                    if (Input.GetKey(KeyCode.S)) flyMovement += Vector3.back;
                    if (Input.GetKey(KeyCode.A)) flyMovement += Vector3.left;
                    if (Input.GetKey(KeyCode.D)) flyMovement += Vector3.right;
                    if (Input.GetKey(KeyCode.LeftShift)) flyMovement += Vector3.up;
                    if (Input.GetKey(KeyCode.LeftControl)) flyMovement += Vector3.down;

                    if (flyMovement != Vector3.zero)
                    {
                        // Try multiple movement methods for flying
                        var rigidbody = player.GetComponent<Rigidbody>();
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

                        // Direct transform movement as fallback
                        player.transform.position += flyMovement * speedMultiplier * Time.deltaTime * 5f;
                    }
                    else
                    {
                        // Stop movement when no input
                        var rigidbody = player.GetComponent<Rigidbody>();
                        if (rigidbody != null && isFlying)
                        {
                            rigidbody.velocity = Vector3.zero;
                            rigidbody.useGravity = false;
                        }
                    }
                }
                else
                {
                    // Restore gravity when not flying
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

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f; // Just checkbox
            
            // Base checkbox (25) + "Controls:" label (20) + 4 control lines (20*4) + optional flying status (20 when active)
            float baseHeight = 25f + 20f + (20f * 4); // = 105px
            
            // Add flying status height only when actually flying
            if (isFlying) baseHeight += 20f;
            
            return baseHeight;
        }
    }
}