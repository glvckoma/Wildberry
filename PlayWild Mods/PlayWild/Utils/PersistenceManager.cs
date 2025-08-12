using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;

namespace PlayWild.Utils
{
    public static class PersistenceManager
    {
        private static readonly string ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\WildWorks\Animal Jam\Data\build\UserData", "WildBerry_Config.cfg");
        private static Dictionary<string, object> _config = new Dictionary<string, object>();

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string[] lines = File.ReadAllLines(ConfigFilePath);
                    _config = new Dictionary<string, object>();
                    
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                        
                        string[] parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            
                            // Try to parse different types
                            if (bool.TryParse(value, out bool boolVal))
                                _config[key] = boolVal;
                            else if (float.TryParse(value, out float floatVal))
                                _config[key] = floatVal;
                            else if (int.TryParse(value, out int intVal))
                                _config[key] = intVal;
                            else
                                _config[key] = value;
                        }
                    }
                    
                    MelonLogger.Msg($"[WildBerry] Loaded config from: {ConfigFilePath}");
                }
                else
                {
                    _config = new Dictionary<string, object>();
                    MelonLogger.Msg("[WildBerry] No config file found, using defaults.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error loading config: {ex.Message}");
                _config = new Dictionary<string, object>();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                var lines = new List<string>();
                lines.Add("# WildBerry Configuration File");
                lines.Add("# Generated automatically - do not edit manually");
                lines.Add("");
                
                foreach (var kvp in _config)
                {
                    lines.Add($"{kvp.Key}={kvp.Value}");
                }
                
                File.WriteAllLines(ConfigFilePath, lines);
                MelonLogger.Msg($"[WildBerry] Saved config to: {ConfigFilePath}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error saving config: {ex.Message}");
            }
        }

        public static T GetValue<T>(string key, T defaultValue = default(T))
        {
            try
            {
                if (_config.ContainsKey(key))
                {
                    var value = _config[key];
                    if (value is T directValue)
                    {
                        return directValue;
                    }
                    
                    // Handle type conversion (JSON deserializes numbers as long/double)
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error getting config value for key '{key}': {ex.Message}");
            }
            
            return defaultValue;
        }

        public static void SetValue<T>(string key, T value)
        {
            try
            {
                _config[key] = value;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error setting config value for key '{key}': {ex.Message}");
            }
        }

        // Feature-specific helpers
        public static bool GetFeatureEnabled(string featureName, bool defaultValue = false)
        {
            return GetValue($"feature_{featureName}_enabled", defaultValue);
        }

        public static void SetFeatureEnabled(string featureName, bool enabled)
        {
            SetValue($"feature_{featureName}_enabled", enabled);
            SaveConfig(); // Auto-save when feature states change
        }

        public static T GetFeatureValue<T>(string featureName, string valueName, T defaultValue = default(T))
        {
            return GetValue($"feature_{featureName}_{valueName}", defaultValue);
        }

        public static void SetFeatureValue<T>(string featureName, string valueName, T value)
        {
            SetValue($"feature_{featureName}_{valueName}", value);
            SaveConfig(); // Auto-save when feature values change
        }
    }
}