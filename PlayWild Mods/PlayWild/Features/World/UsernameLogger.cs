using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using PlayWild.Features.Base;
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
        private const float CHECK_INTERVAL = 3f; // Check every 3 seconds to avoid logging too quickly
        private Dictionary<string, float> pendingUsernames = new Dictionary<string, float>();
        private const float USERNAME_DELAY = 1f; // Wait 1 second before logging a username
        
        // Regex to detect GUIDs (device IDs)
        private static readonly Regex GuidRegex = new Regex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase);
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Load previously logged usernames to avoid duplicates across sessions
            LoadLoggedUsernames();
        }

        public override void OnEnable()
        {
            try
            {
                // Use current user's AppData\Roaming directory (works for any user)
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logDirectory = Path.Combine(appDataPath, "strawberry-jam", "UsernameLogger");
                
                // Create directory if it doesn't exist
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
            
            // Process pending usernames that have waited long enough
            ProcessPendingUsernames();
            
            // Throttle checks to avoid performance issues
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
            DrawCheckbox(new Rect(area.x, area.y, 16, 16), IsEnabled, Name, 
                (value) => { IsEnabled = value; });
            
            if (IsEnabled)
            {
                float yOffset = 25;
                GUI.color = Color.white;
                
                // Show status
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 300, 20), 
                    $"Logged usernames: {loggedUsernames.Count}");
                yOffset += 20;
                
                // Show log file path
                GUI.Label(new Rect(area.x + 5, area.y + yOffset, 400, 20), 
                    $"Log file: collected_usernames.txt");
                yOffset += 25;
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled)
            {
                return 25f; // Just checkbox height
            }
            
            // Checkbox (25) + Status (20) + File path (25)
            return 70f;
        }

        private void LogNetworkActorUsernames()
        {
            try
            {
                // Method 1: Try to find Avatar_Network objects directly
                LogFromAvatarNetwork();
                
                // Method 2: Try to find networked avatars through AvatarManager
                LogFromAvatarManager();
                
                // Method 3: Try to find all GameObjects with Avatar components
                LogFromAllAvatars();
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
                // Find all Avatar_Network objects
                var avatarNetworks = UnityEngine.Object.FindObjectsOfType<Il2Cpp.Avatar_Network>();
                
                foreach (var avatar in avatarNetworks)
                {
                    if (avatar == null) continue;
                    
                    try
                    {
                        // Try to get AvatarInfo from the avatar
                        var avatarInfo = GetAvatarInfoFromObject(avatar);
                        if (avatarInfo != null)
                        {
                            ExtractAndLogUsername(avatarInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WildBerry] Username Logger: Error processing Avatar_Network: {ex.Message}");
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
                // Try to find AvatarManager and get networked avatars from it
                var avatarManager = UnityEngine.Object.FindObjectOfType<Il2Cpp.AvatarManager>();
                if (avatarManager == null) return;
                
                // Use reflection to access private fields
                var type = avatarManager.GetType();
                var buildingNetworkedAvatarsField = type.GetField("_buildingNetworkedAvatars", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (buildingNetworkedAvatarsField != null)
                {
                    var buildingNetworkedAvatars = buildingNetworkedAvatarsField.GetValue(avatarManager);
                    if (buildingNetworkedAvatars != null)
                    {
                        // Process the list of networked avatars
                        ProcessNetworkedAvatarsList(buildingNetworkedAvatars);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error accessing AvatarManager: {ex.Message}");
            }
        }

        private void LogFromAllAvatars()
        {
            try
            {
                // Find all objects that might contain avatar information
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;
                    
                    try
                    {
                        // Check if object has Avatar_Network component
                        var avatarNetwork = obj.GetComponent<Il2Cpp.Avatar_Network>();
                        if (avatarNetwork != null)
                        {
                            var avatarInfo = GetAvatarInfoFromObject(avatarNetwork);
                            if (avatarInfo != null)
                            {
                                ExtractAndLogUsername(avatarInfo);
                            }
                        }
                        
                        // Also check for other avatar-related components
                        var avatarBase = obj.GetComponent<Il2Cpp.AvatarBase>();
                        if (avatarBase != null)
                        {
                            var avatarInfo = GetAvatarInfoFromObject(avatarBase);
                            if (avatarInfo != null)
                            {
                                ExtractAndLogUsername(avatarInfo);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Silently continue for individual object errors
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error scanning all objects: {ex.Message}");
            }
        }

        private Il2Cpp.AvatarInfo GetAvatarInfoFromObject(object avatarObject)
        {
            if (avatarObject == null) return null;
            
            try
            {
                var type = avatarObject.GetType();
                
                // Try common property names for AvatarInfo
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
                
                // Try common field names
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
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error getting AvatarInfo from object: {ex.Message}");
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
                    // Add to pending list with current time
                    pendingUsernames[username] = Time.time;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error extracting username: {ex.Message}");
            }
        }

        private bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;
            
            // Filter out GUIDs (device IDs)
            if (GuidRegex.IsMatch(username))
                return false;
            
            // Filter out usernames that are too short or too long
            if (username.Length < 2 || username.Length > 50)
                return false;
            
            // Filter out usernames that are all numbers or contain only special characters
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
                
                // Try to find InventoryItemInfo or similar
                string[] possibleInventoryProperties = { "InventoryItemInfo", "ItemInfo", "inventoryItemInfo", "itemInfo" };
                
                foreach (var propName in possibleInventoryProperties)
                {
                    var property = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null)
                    {
                        var inventoryInfo = property.GetValue(avatarInfo);
                        if (inventoryInfo != null)
                        {
                            string username = GetUsernameFromInventoryInfo(inventoryInfo);
                            if (!string.IsNullOrEmpty(username))
                            {
                                return username;
                            }
                        }
                    }
                }
                
                // Try fields as well
                string[] possibleInventoryFields = { "InventoryItemInfo", "ItemInfo", "inventoryItemInfo", "itemInfo", "_inventoryItemInfo", "_itemInfo" };
                
                foreach (var fieldName in possibleInventoryFields)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        var inventoryInfo = field.GetValue(avatarInfo);
                        if (inventoryInfo != null)
                        {
                            string username = GetUsernameFromInventoryInfo(inventoryInfo);
                            if (!string.IsNullOrEmpty(username))
                            {
                                return username;
                            }
                        }
                    }
                }
                
                // Try to get username directly from AvatarInfo
                string directUsername = GetUsernameFromObject(avatarInfo);
                if (!string.IsNullOrEmpty(directUsername))
                {
                    return directUsername;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error getting username from AvatarInfo: {ex.Message}");
            }
            
            return null;
        }

        private string GetUsernameFromInventoryInfo(object inventoryInfo)
        {
            if (inventoryInfo == null) return null;
            
            return GetUsernameFromObject(inventoryInfo);
        }

        private string GetUsernameFromObject(object obj)
        {
            if (obj == null) return null;
            
            try
            {
                var type = obj.GetType();
                
                // Try common username property names
                string[] possibleUsernameProperties = { "userName", "UserName", "username", "Username", "name", "Name" };
                
                foreach (var propName in possibleUsernameProperties)
                {
                    var property = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null && property.PropertyType == typeof(string))
                    {
                        var value = property.GetValue(obj) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                }
                
                // Try common username field names
                string[] possibleUsernameFields = { "userName", "UserName", "username", "Username", "name", "Name", "_userName", "_username" };
                
                foreach (var fieldName in possibleUsernameFields)
                {
                    var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        var value = field.GetValue(obj) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WildBerry] Username Logger: Error getting username from object: {ex.Message}");
            }
            
            return null;
        }

        private void ProcessNetworkedAvatarsList(object networkedAvatarsList)
        {
            if (networkedAvatarsList == null) return;
            
            try
            {
                // Try to process as IEnumerable
                if (networkedAvatarsList is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item == null) continue;
                        
                        // Try to get avatar info from the item
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
                    // Use ISO 8601 format with milliseconds
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
                        // Extract username from log entry format: timestamp - username
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
                        // Also handle old format for backwards compatibility
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
