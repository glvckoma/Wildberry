using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;
using Il2Cpp;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PlayWild.Features.World
{
    public class UsernameLogger : BaseFeature
    {
        public override string Name => "Username Logger";

        private HashSet<string> loggedUsernames = new HashSet<string>();
        private string logFile;
        private float lastCheckTime = 0f;
        private const float CHECK_INTERVAL = 3f;
        private Dictionary<string, float> pendingUsernames = new Dictionary<string, float>();
        private const float USERNAME_DELAY = 1f;

        private static readonly Regex GuidRegex = new Regex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase);

        public override void Initialize()
        {
            base.Initialize();
            LoadLoggedUsernames();
        }

        public override void OnEnable()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDirectory = Path.Combine(appDataPath, "strawberry-jam", "UsernameLogger");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                logFile = Path.Combine(logDirectory, "collected_usernames.txt");
                MelonLogger.Msg($"[WildBerry] Username Logger: Logging to {logFile}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Username Logger: Error setting up log file: {ex.Message}");
            }
        }

        public override void OnUpdate()
        {
            if (!IsEnabled) return;

            ProcessPendingUsernames();

            if (Time.time - lastCheckTime < CHECK_INTERVAL) return;
            lastCheckTime = Time.time;

            try
            {
                LogNetworkActorUsernames();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Username Logger: Error during update: {ex.Message}");
            }
        }

        public override void OnGUI(Rect area)
        {
            DrawToggle(new Rect(area.x, area.y, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });

            if (IsEnabled)
            {
                float yOffset = 25;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 300, 20),
                    $"Logged usernames: {loggedUsernames.Count}");
                yOffset += 20;

                DrawLabel(new Rect(area.x + 5, area.y + yOffset, 400, 20),
                    $"Log file: collected_usernames.txt");
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
                return WildBerryTheme.ToggleHeight;
            return 70f;
        }

        private void LogNetworkActorUsernames()
        {
            try
            {
                LogFromAvatarNetwork();
                LogFromAvatarManager();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Username Logger: Error logging usernames: {ex.Message}");
            }
        }

        private void LogFromAvatarNetwork()
        {
            try
            {
                var avatarNetworks = UnityEngine.Object.FindObjectsOfType<Il2Cpp.Avatar_Network>();

                foreach (var avatar in avatarNetworks)
                {
                    if (avatar == null) continue;

                    try
                    {
                        var avatarInfo = GetAvatarInfoFromObject(avatar);
                        if (avatarInfo != null)
                        {
                            ExtractAndLogUsername(avatarInfo);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error finding Avatar_Network objects: {ex.Message}");
            }
        }

        private void LogFromAvatarManager()
        {
            try
            {
                var avatarManager = UnityEngine.Object.FindObjectOfType<Il2Cpp.AvatarManager>();
                if (avatarManager == null) return;

                var type = avatarManager.GetType();
                var buildingNetworkedAvatarsField = type.GetField("_buildingNetworkedAvatars", BindingFlags.NonPublic | BindingFlags.Instance);

                if (buildingNetworkedAvatarsField != null)
                {
                    var buildingNetworkedAvatars = buildingNetworkedAvatarsField.GetValue(avatarManager);
                    if (buildingNetworkedAvatars != null)
                    {
                        ProcessNetworkedAvatarsList(buildingNetworkedAvatars);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error accessing AvatarManager: {ex.Message}");
            }
        }

        private Il2Cpp.AvatarInfo GetAvatarInfoFromObject(object avatarObject)
        {
            if (avatarObject == null) return null;

            try
            {
                var type = avatarObject.GetType();

                string[] possibleProperties = { "Info", "AvatarInfo", "avatarInfo", "info" };

                foreach (var propName in possibleProperties)
                {
                    var property = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null)
                    {
                        var value = property.GetValue(avatarObject);
                        if (value is Il2Cpp.AvatarInfo avatarInfo)
                        {
                            return avatarInfo;
                        }
                    }
                }

                string[] possibleFields = { "Info", "AvatarInfo", "avatarInfo", "info", "_avatarInfo", "_info" };

                foreach (var fieldName in possibleFields)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        var value = field.GetValue(avatarObject);
                        if (value is Il2Cpp.AvatarInfo avatarInfo)
                        {
                            return avatarInfo;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private void ProcessPendingUsernames()
        {
            try
            {
                var usernamesToProcess = new List<string>();
                var currentTime = Time.time;

                foreach (var kvp in pendingUsernames)
                {
                    if (currentTime - kvp.Value >= USERNAME_DELAY)
                    {
                        usernamesToProcess.Add(kvp.Key);
                    }
                }

                foreach (var username in usernamesToProcess)
                {
                    pendingUsernames.Remove(username);
                    if (!loggedUsernames.Contains(username))
                    {
                        LogUsername(username);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error processing pending usernames: {ex.Message}");
            }
        }

        private void ExtractAndLogUsername(Il2Cpp.AvatarInfo avatarInfo)
        {
            if (avatarInfo == null) return;

            try
            {
                string username = GetUsernameFromAvatarInfo(avatarInfo);

                if (!string.IsNullOrEmpty(username) && IsValidUsername(username) && !loggedUsernames.Contains(username) && !pendingUsernames.ContainsKey(username))
                {
                    pendingUsernames[username] = Time.time;
                }
            }
            catch (Exception)
            {
            }
        }

        private bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            if (GuidRegex.IsMatch(username))
                return false;

            if (username.Length < 2 || username.Length > 50)
                return false;

            if (Regex.IsMatch(username, @"^[\d\-_\.]+$"))
                return false;

            return true;
        }

        private string GetUsernameFromAvatarInfo(Il2Cpp.AvatarInfo avatarInfo)
        {
            if (avatarInfo == null) return null;

            try
            {
                var type = avatarInfo.GetType();

                string[] possibleInventoryProperties = { "InventoryItemInfo", "ItemInfo", "inventoryItemInfo", "itemInfo" };

                foreach (var propName in possibleInventoryProperties)
                {
                    var property = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null)
                    {
                        var inventoryInfo = property.GetValue(avatarInfo);
                        if (inventoryInfo != null)
                        {
                            string username = GetUsernameFromObject(inventoryInfo);
                            if (!string.IsNullOrEmpty(username))
                                return username;
                        }
                    }
                }

                string[] possibleInventoryFields = { "InventoryItemInfo", "ItemInfo", "inventoryItemInfo", "itemInfo", "_inventoryItemInfo", "_itemInfo" };

                foreach (var fieldName in possibleInventoryFields)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        var inventoryInfo = field.GetValue(avatarInfo);
                        if (inventoryInfo != null)
                        {
                            string username = GetUsernameFromObject(inventoryInfo);
                            if (!string.IsNullOrEmpty(username))
                                return username;
                        }
                    }
                }

                string directUsername = GetUsernameFromObject(avatarInfo);
                if (!string.IsNullOrEmpty(directUsername))
                    return directUsername;
            }
            catch (Exception)
            {
            }

            return null;
        }

        private string GetUsernameFromObject(object obj)
        {
            if (obj == null) return null;

            try
            {
                var type = obj.GetType();

                string[] possibleUsernameProperties = { "userName", "UserName", "username", "Username", "name", "Name" };

                foreach (var propName in possibleUsernameProperties)
                {
                    var property = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null && property.PropertyType == typeof(string))
                    {
                        var value = property.GetValue(obj) as string;
                        if (!string.IsNullOrEmpty(value))
                            return value;
                    }
                }

                string[] possibleUsernameFields = { "userName", "UserName", "username", "Username", "name", "Name", "_userName", "_username" };

                foreach (var fieldName in possibleUsernameFields)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        var value = field.GetValue(obj) as string;
                        if (!string.IsNullOrEmpty(value))
                            return value;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private void ProcessNetworkedAvatarsList(object networkedAvatarsList)
        {
            if (networkedAvatarsList == null) return;

            try
            {
                if (networkedAvatarsList is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item == null) continue;

                        var avatarInfo = GetAvatarInfoFromObject(item);
                        if (avatarInfo != null)
                        {
                            ExtractAndLogUsername(avatarInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error processing networked avatars list: {ex.Message}");
            }
        }

        private void LogUsername(string username)
        {
            try
            {
                if (loggedUsernames.Add(username))
                {
                    string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    string logEntry = $"{timestamp} - {username}";
                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                    MelonLogger.Msg($"[WildBerry] Username Logger: Logged new username: {username}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Username Logger: Error writing to log file: {ex.Message}");
            }
        }

        private void LoadLoggedUsernames()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDirectory = Path.Combine(appDataPath, "strawberry-jam", "UsernameLogger");
                logFile = Path.Combine(logDirectory, "collected_usernames.txt");

                if (File.Exists(logFile))
                {
                    var lines = File.ReadAllLines(logFile);
                    foreach (var line in lines)
                    {
                        if (line.Contains(" - "))
                        {
                            int dashIndex = line.IndexOf(" - ");
                            if (dashIndex > 0 && dashIndex + 3 < line.Length)
                            {
                                string username = line.Substring(dashIndex + 3).Trim();
                                if (!string.IsNullOrEmpty(username) && IsValidUsername(username))
                                {
                                    loggedUsernames.Add(username);
                                }
                            }
                        }
                        else if (line.Contains("] ") && line.StartsWith("["))
                        {
                            int endBracket = line.IndexOf("] ");
                            if (endBracket > 0 && endBracket + 2 < line.Length)
                            {
                                string username = line.Substring(endBracket + 2).Trim();
                                if (!string.IsNullOrEmpty(username) && IsValidUsername(username))
                                {
                                    loggedUsernames.Add(username);
                                }
                            }
                        }
                    }

                    MelonLogger.Msg($"[WildBerry] Username Logger: Loaded {loggedUsernames.Count} previously logged usernames");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error loading previously logged usernames: {ex.Message}");
            }
        }
    }
}
