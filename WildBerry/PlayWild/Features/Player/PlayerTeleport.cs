using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using PlayWild.Utils;
using MelonLoader;

namespace PlayWild.Features.Player
{
    public class PlayerTeleport : BaseFeature
    {
        public override string Name => "Player Teleport";

        private string xInput = "0";
        private string yInput = "0";
        private string zInput = "0";

        public override void Initialize()
        {
            base.Initialize();
            xInput = GetPersistedValue("lastX", "0");
            yInput = GetPersistedValue("lastY", "0");
            zInput = GetPersistedValue("lastZ", "0");
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            if (Input.GetKeyDown(KeyCode.T) && !Input.GetKey(KeyCode.LeftControl))
            {
                ClickTeleport();
            }
        }

        private void ClickTeleport()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player == null) return;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    Vector3 targetPos = hit.point + Vector3.up * 0.5f;
                    player.transform.position = targetPos;

                    xInput = targetPos.x.ToString("F1");
                    yInput = targetPos.y.ToString("F1");
                    zInput = targetPos.z.ToString("F1");

                    SetPersistedValue("lastX", xInput);
                    SetPersistedValue("lastY", yInput);
                    SetPersistedValue("lastZ", zInput);

                    MelonLogger.Msg($"[WildBerry] Teleported to ({targetPos.x:F1}, {targetPos.y:F1}, {targetPos.z:F1})");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Click teleport error: {ex.Message}");
            }
        }

        private void CoordinateTeleport()
        {
            try
            {
                GameObject player = PlayerFinder.FindPlayerObject();
                if (player == null)
                {
                    MelonLogger.Warning("[WildBerry] Player not found for teleport");
                    return;
                }

                if (float.TryParse(xInput, out float x) &&
                    float.TryParse(yInput, out float y) &&
                    float.TryParse(zInput, out float z))
                {
                    player.transform.position = new Vector3(x, y, z);

                    SetPersistedValue("lastX", xInput);
                    SetPersistedValue("lastY", yInput);
                    SetPersistedValue("lastZ", zInput);

                    MelonLogger.Msg($"[WildBerry] Teleported to ({x:F1}, {y:F1}, {z:F1})");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Coordinate teleport error: {ex.Message}");
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 200, 20), "Press T to click-teleport");
                yOffset += 22;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 20, 20), "X:");
                xInput = DrawStyledTextField(new Rect(area.x + 25, area.y + yOffset, 55, 20), xInput);
                DrawLabel(new Rect(area.x + 85, area.y + yOffset, 20, 20), "Y:");
                yInput = DrawStyledTextField(new Rect(area.x + 105, area.y + yOffset, 55, 20), yInput);
                DrawLabel(new Rect(area.x + 165, area.y + yOffset, 20, 20), "Z:");
                zInput = DrawStyledTextField(new Rect(area.x + 185, area.y + yOffset, 55, 20), zInput);
                yOffset += 25;

                if (DrawStyledButton(new Rect(area.x + 5, area.y + yOffset, 80, 22), "Teleport"))
                {
                    CoordinateTeleport();
                }

                GameObject player = PlayerFinder.FindPlayerObject();
                if (player != null)
                {
                    Vector3 pos = player.transform.position;
                    DrawStatusLabel(new Rect(area.x + 95, area.y + yOffset, 200, 20),
                        $"Pos: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}", Color.cyan);
                }
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f;
            return 100f;
        }
    }
}
