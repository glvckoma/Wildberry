using UnityEngine;
using MelonLoader;

namespace PlayWild.Utils
{
    public static class PlayerFinder
    {
        public static GameObject FindPlayerObject()
        {
            try
            {
                // Try common player object names
                GameObject player = GameObject.Find("Player");
                if (player != null) return player;

                player = GameObject.Find("LocalPlayer");
                if (player != null) return player;

                player = GameObject.Find("PlayerAvatar");
                if (player != null) return player;

                // Try finding by tag
                player = GameObject.FindWithTag("Player");
                if (player != null) return player;

                // Search for objects with player-related components
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("player") && 
                        (obj.GetComponent<CharacterController>() != null || obj.GetComponent<Rigidbody>() != null))
                    {
                        return obj;
                    }
                }

                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error finding player object: {ex.Message}");
                return null;
            }
        }
    }
}