using UnityEngine;
using PlayWild.Features.Base;
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
        private string originalUUID;
        
        internal bool isSpoofed = false;
        private static HarmonyLib.Harmony harmonyInstance;
        internal static DeviceIDSpoofing currentInstance;

        public override void OnEnable()
        {
            try
            {
                MelonLogger.Msg("[WildBerry] Device ID Spoofing: Initializing...");
                
                // Store original values first
                StoreOriginalValues();
                
                // Generate new spoofed values
                GenerateNewSpoofedValues();
                
                // Set up Harmony patches
                ApplyHarmonyPatches();
                
                // Apply CodeStage spoofing (if present)
                SpoofCodeStageDeviceId();
                
                isSpoofed = true;
                
                MelonLogger.Msg("[WildBerry] Device ID Spoofing: ENABLED with randomized values");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error enabling Device ID Spoofing: {ex.Message}");
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
                    MelonLogger.Msg("[WildBerry] Device ID Spoofing: DISABLED - restored original values");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error disabling Device ID Spoofing: {ex.Message}");
            }
        }

        public override void OnUpdate()
        {
            // No periodic updates needed - spoofing is applied once on enable
        }

        public override void OnGUI(Rect area)
        {
            float yOffset = 0;
            
            // Main toggle
            DrawCheckbox(new Rect(area.x, area.y + yOffset, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            yOffset += 25;

            if (IsEnabled)
            {
                GUI.color = Color.white;
                
                // Status display
                if (isSpoofed)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 250, 20), "✓ Device ID/UUID spoofed");
                    yOffset += 20;
                    
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 250, 18), $"Device ID: {spoofedDeviceId?.Substring(0, Math.Min(12, spoofedDeviceId.Length))}...");
                    yOffset += 18;
                    
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 250, 18), $"Device Name: {spoofedDeviceName}");
                    yOffset += 18;
                    
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 250, 18), $"UUID: {spoofedUUID?.Substring(0, Math.Min(8, spoofedUUID.Length))}...");
                    yOffset += 18;
                }
                else
                {
                    GUI.color = Color.yellow;
                    GUI.Label(new Rect(area.x + 5, area.y + yOffset, 250, 20), "⚠ Initializing spoofing...");
                    yOffset += 20;
                }
                
                GUI.color = Color.white;
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 250, 20), "New random device ID each game session");
            }
        }

        private void StoreOriginalValues()
        {
            try
            {
                // Store original Unity SystemInfo values
                originalDeviceId = SystemInfo.deviceUniqueIdentifier;
                originalDeviceName = SystemInfo.deviceName;
                originalDeviceModel = SystemInfo.deviceModel;
                
                MelonLogger.Msg($"[WildBerry] Stored original Device ID: {originalDeviceId}");
                MelonLogger.Msg($"[WildBerry] Stored original Device Name: {originalDeviceName}");
                MelonLogger.Msg($"[WildBerry] Stored original Device Model: {originalDeviceModel}");
                
                // Generate a placeholder UUID for logging purposes
                originalUUID = Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error storing original values: {ex.Message}");
            }
        }

        private void GenerateNewSpoofedValues()
        {
            // Generate random device ID (hex string like Unity typically uses)
            spoofedDeviceId = GenerateRandomHexString(32);
            
            // Generate random device name
            string[] deviceNames = { "SpoofDevice", "TestDevice", "RandomDevice", "FakeDevice", "MockDevice" };
            spoofedDeviceName = deviceNames[UnityEngine.Random.Range(0, deviceNames.Length)] + UnityEngine.Random.Range(1000, 9999);
            
            // Generate random device model
            string[] deviceModels = { "SpoofModel", "TestModel", "RandomModel", "FakeModel", "MockModel" };
            spoofedDeviceModel = deviceModels[UnityEngine.Random.Range(0, deviceModels.Length)] + " v" + UnityEngine.Random.Range(1, 10);
            
            // Generate random UUID
            spoofedUUID = GenerateRandomUUID();
            
            MelonLogger.Msg($"[WildBerry] Generated new spoofed Device ID: {spoofedDeviceId}");
            MelonLogger.Msg($"[WildBerry] Generated new spoofed Device Name: {spoofedDeviceName}");
            MelonLogger.Msg($"[WildBerry] Generated new spoofed Device Model: {spoofedDeviceModel}");
            MelonLogger.Msg($"[WildBerry] Generated new spoofed UUID: {spoofedUUID}");
        }

        private void ApplyHarmonyPatches()
        {
            try
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new HarmonyLib.Harmony("WildBerry.DeviceIDSpoofing");
                }

                currentInstance = this;

                // Patch Unity SystemInfo properties
                var systemInfoType = typeof(SystemInfo);
                
                // Patch deviceUniqueIdentifier getter
                var deviceUniqueIdentifierMethod = systemInfoType.GetProperty("deviceUniqueIdentifier")?.GetGetMethod();
                if (deviceUniqueIdentifierMethod != null)
                {
                    harmonyInstance.Patch(deviceUniqueIdentifierMethod, 
                        prefix: new HarmonyMethod(typeof(SystemInfoPatches), nameof(SystemInfoPatches.DeviceUniqueIdentifier_Prefix)));
                    MelonLogger.Msg("[WildBerry] Patched SystemInfo.deviceUniqueIdentifier");
                }

                // Patch deviceName getter
                var deviceNameMethod = systemInfoType.GetProperty("deviceName")?.GetGetMethod();
                if (deviceNameMethod != null)
                {
                    harmonyInstance.Patch(deviceNameMethod, 
                        prefix: new HarmonyMethod(typeof(SystemInfoPatches), nameof(SystemInfoPatches.DeviceName_Prefix)));
                    MelonLogger.Msg("[WildBerry] Patched SystemInfo.deviceName");
                }

                // Patch deviceModel getter
                var deviceModelMethod = systemInfoType.GetProperty("deviceModel")?.GetGetMethod();
                if (deviceModelMethod != null)
                {
                    harmonyInstance.Patch(deviceModelMethod, 
                        prefix: new HarmonyMethod(typeof(SystemInfoPatches), nameof(SystemInfoPatches.DeviceModel_Prefix)));
                    MelonLogger.Msg("[WildBerry] Patched SystemInfo.deviceModel");
                }

                MelonLogger.Msg("[WildBerry] Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying Harmony patches: {ex.Message}");
            }
        }

        private void RemoveHarmonyPatches()
        {
            try
            {
                if (harmonyInstance != null)
                {
                    harmonyInstance.UnpatchSelf();
                    MelonLogger.Msg("[WildBerry] Harmony patches removed");
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
                // Look for CodeStage AntiCheat ObscuredPrefs DeviceId
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var obscuredPrefsType = assembly.GetType("CodeStage.AntiCheat.Storage.ObscuredPrefs");
                    if (obscuredPrefsType != null)
                    {
                        // Try to set DeviceId property
                        var deviceIdProperty = obscuredPrefsType.GetProperty("DeviceId");
                        if (deviceIdProperty != null && deviceIdProperty.CanWrite)
                        {
                            deviceIdProperty.SetValue(null, spoofedDeviceId);
                            MelonLogger.Msg($"[WildBerry] Spoofed CodeStage DeviceId: {spoofedDeviceId}");
                        }
                        
                        // Try to find and modify internal device id field
                        var deviceIdField = obscuredPrefsType.GetField("deviceId", BindingFlags.NonPublic | BindingFlags.Static);
                        if (deviceIdField != null)
                        {
                            deviceIdField.SetValue(null, spoofedDeviceId);
                            MelonLogger.Msg($"[WildBerry] Spoofed CodeStage internal deviceId field: {spoofedDeviceId}");
                        }
                        
                        // Force recalculation of device id hash
                        var deviceIdHashField = obscuredPrefsType.GetField("deviceIdHash", BindingFlags.NonPublic | BindingFlags.Static);
                        if (deviceIdHashField != null)
                        {
                            uint hash = (uint)spoofedDeviceId.GetHashCode();
                            deviceIdHashField.SetValue(null, hash);
                            MelonLogger.Msg($"[WildBerry] Updated CodeStage deviceIdHash: {hash}");
                        }
                        
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error spoofing CodeStage DeviceId: {ex.Message}");
            }
        }

        private void RestoreOriginalDeviceInfo()
        {
            try
            {
                // Restore CodeStage DeviceId if possible
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var obscuredPrefsType = assembly.GetType("CodeStage.AntiCheat.Storage.ObscuredPrefs");
                    if (obscuredPrefsType != null)
                    {
                        var deviceIdProperty = obscuredPrefsType.GetProperty("DeviceId");
                        if (deviceIdProperty != null && deviceIdProperty.CanWrite)
                        {
                            deviceIdProperty.SetValue(null, originalDeviceId);
                        }
                    }
                }

                isSpoofed = false;
                MelonLogger.Msg("[WildBerry] Restored original device info");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring original device info: {ex.Message}");
            }
        }

        private string GenerateRandomHexString(int length)
        {
            var bytes = new byte[length / 2];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            StringBuilder sb = new StringBuilder(length);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private string GenerateRandomUUID()
        {
            return Guid.NewGuid().ToString();
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return 25f; // Just checkbox
            
            float baseHeight = 25f; // Main toggle
            
            if (isSpoofed)
            {
                baseHeight += 20f; // Status message
                baseHeight += 18f * 3; // Device ID, Name, UUID (18px each)
                baseHeight += 20f; // Description
            }
            else
            {
                baseHeight += 20f; // Initializing message
                baseHeight += 20f; // Description
            }
            
            return baseHeight;
        }
    }

    // Harmony patch classes for SystemInfo spoofing
    public static class SystemInfoPatches
    {
        public static bool DeviceUniqueIdentifier_Prefix(ref string __result)
        {
            if (DeviceIDSpoofing.currentInstance != null && DeviceIDSpoofing.currentInstance.isSpoofed)
            {
                __result = DeviceIDSpoofing.currentInstance.spoofedDeviceId;
                return false; // Skip original method
            }
            return true; // Use original method
        }

        public static bool DeviceName_Prefix(ref string __result)
        {
            if (DeviceIDSpoofing.currentInstance != null && DeviceIDSpoofing.currentInstance.isSpoofed)
            {
                __result = DeviceIDSpoofing.currentInstance.spoofedDeviceName;
                return false; // Skip original method
            }
            return true; // Use original method
        }

        public static bool DeviceModel_Prefix(ref string __result)
        {
            if (DeviceIDSpoofing.currentInstance != null && DeviceIDSpoofing.currentInstance.isSpoofed)
            {
                __result = DeviceIDSpoofing.currentInstance.spoofedDeviceModel;
                return false; // Skip original method
            }
            return true; // Use original method
        }
    }
}