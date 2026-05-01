using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;
using System;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using HarmonyLib;

namespace PlayWild.Features.Player
{
    public class DeviceIDSpoofing : BaseFeature
    {
        public override string Name => "Device ID Spoofing";

        internal string spoofedDeviceId;
        internal string spoofedDeviceName;
        internal string spoofedDeviceModel;
        internal string spoofedUUID;

        private string originalDeviceId;
        private string originalDeviceName;
        private string originalDeviceModel;

        internal bool isSpoofed = false;
        private bool harmonyPatchFailed = false;
        private static HarmonyLib.Harmony harmonyInstance;
        internal static DeviceIDSpoofing currentInstance;

        public override void OnEnable()
        {
            try
            {
                MelonLogger.Msg("[WildBerry] Device ID Spoofing: Initializing...");

                StoreOriginalValues();
                GenerateNewSpoofedValues();

                if (!ApplyHarmonyPatches())
                {
                    harmonyPatchFailed = true;
                    MelonLogger.Warning("[WildBerry] Device ID Spoofing: Harmony patching failed, using CodeStage-only spoofing");
                }

                SpoofCodeStageDeviceId();

                isSpoofed = true;
                MelonLogger.Msg("[WildBerry] Device ID Spoofing: ENABLED with randomized values");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error enabling Device ID Spoofing: {ex.Message}");
                isSpoofed = false;
            }
        }

        public override void OnDisable()
        {
            try
            {
                if (isSpoofed)
                {
                    RemoveHarmonyPatches();
                    RestoreOriginalDeviceInfo();
                    currentInstance = null;
                    isSpoofed = false;
                    harmonyPatchFailed = false;
                    MelonLogger.Msg("[WildBerry] Device ID Spoofing: DISABLED, restored original values");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error disabling Device ID Spoofing: {ex.Message}");
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnGUI(Rect area)
        {
            float yOffset = 0;

            DrawToggle(new Rect(area.x, area.y + yOffset, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });
            yOffset += 25;

            if (IsEnabled)
            {
                if (isSpoofed)
                {
                    Color statusColor = harmonyPatchFailed ? Color.yellow : Color.green;
                    string statusText = harmonyPatchFailed ? "Partial spoof (CodeStage only)" : "Device ID/UUID spoofed";
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), statusText, statusColor);
                    yOffset += 20;

                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 18), $"Device ID: {spoofedDeviceId?.Substring(0, Math.Min(12, spoofedDeviceId.Length))}...", Color.cyan);
                    yOffset += 18;

                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 18), $"Device Name: {spoofedDeviceName}", Color.cyan);
                    yOffset += 18;

                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 18), $"UUID: {spoofedUUID?.Substring(0, Math.Min(8, spoofedUUID.Length))}...", Color.cyan);
                    yOffset += 18;
                }
                else
                {
                    DrawStatusLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), "Initializing spoofing...", Color.yellow);
                    yOffset += 20;
                }

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 250, 20), "New random device ID each game session");
            }
        }

        private void StoreOriginalValues()
        {
            try
            {
                originalDeviceId = SystemInfo.deviceUniqueIdentifier;
                originalDeviceName = SystemInfo.deviceName;
                originalDeviceModel = SystemInfo.deviceModel;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error storing original values: {ex.Message}");
            }
        }

        private void GenerateNewSpoofedValues()
        {
            spoofedDeviceId = GenerateRandomHexString(32);

            string[] deviceNames = { "SpoofDevice", "TestDevice", "RandomDevice", "FakeDevice", "MockDevice" };
            spoofedDeviceName = deviceNames[UnityEngine.Random.Range(0, deviceNames.Length)] + UnityEngine.Random.Range(1000, 9999);

            string[] deviceModels = { "SpoofModel", "TestModel", "RandomModel", "FakeModel", "MockModel" };
            spoofedDeviceModel = deviceModels[UnityEngine.Random.Range(0, deviceModels.Length)] + " v" + UnityEngine.Random.Range(1, 10);

            spoofedUUID = Guid.NewGuid().ToString();
        }

        private bool ApplyHarmonyPatches()
        {
            try
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new HarmonyLib.Harmony("WildBerry.DeviceIDSpoofing");
                }

                currentInstance = this;

                var systemInfoType = typeof(SystemInfo);

                var deviceUniqueIdentifierMethod = systemInfoType.GetProperty("deviceUniqueIdentifier")?.GetGetMethod();
                if (deviceUniqueIdentifierMethod != null)
                {
                    try
                    {
                        harmonyInstance.Patch(deviceUniqueIdentifierMethod,
                            prefix: new HarmonyMethod(typeof(SystemInfoPatches), nameof(SystemInfoPatches.DeviceUniqueIdentifier_Prefix)));
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WildBerry] Failed to patch deviceUniqueIdentifier: {ex.Message}");
                        return false;
                    }
                }

                var deviceNameMethod = systemInfoType.GetProperty("deviceName")?.GetGetMethod();
                if (deviceNameMethod != null)
                {
                    try
                    {
                        harmonyInstance.Patch(deviceNameMethod,
                            prefix: new HarmonyMethod(typeof(SystemInfoPatches), nameof(SystemInfoPatches.DeviceName_Prefix)));
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WildBerry] Failed to patch deviceName: {ex.Message}");
                    }
                }

                var deviceModelMethod = systemInfoType.GetProperty("deviceModel")?.GetGetMethod();
                if (deviceModelMethod != null)
                {
                    try
                    {
                        harmonyInstance.Patch(deviceModelMethod,
                            prefix: new HarmonyMethod(typeof(SystemInfoPatches), nameof(SystemInfoPatches.DeviceModel_Prefix)));
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WildBerry] Failed to patch deviceModel: {ex.Message}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying Harmony patches: {ex.Message}");
                return false;
            }
        }

        private void RemoveHarmonyPatches()
        {
            try
            {
                if (harmonyInstance != null)
                {
                    harmonyInstance.UnpatchSelf();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error removing Harmony patches: {ex.Message}");
            }
        }

        private void SpoofCodeStageDeviceId()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    Type obscuredPrefsType = null;
                    try
                    {
                        obscuredPrefsType = assembly.GetType("CodeStage.AntiCheat.Storage.ObscuredPrefs");
                    }
                    catch
                    {
                        continue;
                    }

                    if (obscuredPrefsType == null) continue;

                    try
                    {
                        var deviceIdProperty = obscuredPrefsType.GetProperty("DeviceId");
                        if (deviceIdProperty != null && deviceIdProperty.CanWrite)
                        {
                            deviceIdProperty.SetValue(null, spoofedDeviceId);
                        }
                    }
                    catch { }

                    try
                    {
                        var deviceIdField = obscuredPrefsType.GetField("deviceId", BindingFlags.NonPublic | BindingFlags.Static);
                        if (deviceIdField != null)
                        {
                            deviceIdField.SetValue(null, spoofedDeviceId);
                        }
                    }
                    catch { }

                    try
                    {
                        var deviceIdHashField = obscuredPrefsType.GetField("deviceIdHash", BindingFlags.NonPublic | BindingFlags.Static);
                        if (deviceIdHashField != null)
                        {
                            uint hash = (uint)spoofedDeviceId.GetHashCode();
                            deviceIdHashField.SetValue(null, hash);
                        }
                    }
                    catch { }

                    break;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Error spoofing CodeStage DeviceId: {ex.Message}");
            }
        }

        private void RestoreOriginalDeviceInfo()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    Type obscuredPrefsType = null;
                    try
                    {
                        obscuredPrefsType = assembly.GetType("CodeStage.AntiCheat.Storage.ObscuredPrefs");
                    }
                    catch
                    {
                        continue;
                    }

                    if (obscuredPrefsType == null) continue;

                    try
                    {
                        var deviceIdProperty = obscuredPrefsType.GetProperty("DeviceId");
                        if (deviceIdProperty != null && deviceIdProperty.CanWrite)
                        {
                            deviceIdProperty.SetValue(null, originalDeviceId);
                        }
                    }
                    catch { }

                    break;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring original device info: {ex.Message}");
            }
        }

        private string GenerateRandomHexString(int length)
        {
            var bytes = new byte[length / 2];
            RandomNumberGenerator.Fill(bytes);
            StringBuilder sb = new StringBuilder(length);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f;

            float baseHeight = 25f;

            if (isSpoofed)
            {
                baseHeight += 20f;
                baseHeight += 18f * 3;
                baseHeight += 20f;
            }
            else
            {
                baseHeight += 20f;
                baseHeight += 20f;
            }

            return baseHeight;
        }
    }

    public static class SystemInfoPatches
    {
        public static bool DeviceUniqueIdentifier_Prefix(ref string __result)
        {
            if (DeviceIDSpoofing.currentInstance != null && DeviceIDSpoofing.currentInstance.isSpoofed)
            {
                __result = DeviceIDSpoofing.currentInstance.spoofedDeviceId;
                return false;
            }
            return true;
        }

        public static bool DeviceName_Prefix(ref string __result)
        {
            if (DeviceIDSpoofing.currentInstance != null && DeviceIDSpoofing.currentInstance.isSpoofed)
            {
                __result = DeviceIDSpoofing.currentInstance.spoofedDeviceName;
                return false;
            }
            return true;
        }

        public static bool DeviceModel_Prefix(ref string __result)
        {
            if (DeviceIDSpoofing.currentInstance != null && DeviceIDSpoofing.currentInstance.isSpoofed)
            {
                __result = DeviceIDSpoofing.currentInstance.spoofedDeviceModel;
                return false;
            }
            return true;
        }
    }
}
