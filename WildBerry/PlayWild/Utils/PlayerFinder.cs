using UnityEngine;
using MelonLoader;

namespace PlayWild.Utils
{
    public static class PlayerFinder
    {
        private static GameObject cachedPlayer;
        private static float lastCacheTime;
        private const float CACHE_DURATION = 2f;

        public static GameObject FindPlayerObject()
        {
            try
            {
                if (cachedPlayer != null && Time.time - lastCacheTime < CACHE_DURATION)
                    return cachedPlayer;

                cachedPlayer = null;

                var actorsObject = GameObject.Find("Actors");
                if (actorsObject != null)
                {
                    var localPlayerTransform = actorsObject.transform.Find("LocalPlayer");
                    if (localPlayerTransform != null)
                    {
                        for (int i = 0; i < localPlayerTransform.childCount; i++)
                        {
                            var child = localPlayerTransform.GetChild(i);
                            if (child.name.StartsWith("(Actor_Local)"))
                            {
                                cachedPlayer = child.gameObject;
                                lastCacheTime = Time.time;
                                return cachedPlayer;
                            }
                        }
                    }
                }

                GameObject player = GameObject.Find("LocalPlayer");
                if (player != null)
                {
                    cachedPlayer = player;
                    lastCacheTime = Time.time;
                    return cachedPlayer;
                }

                player = GameObject.Find("Player");
                if (player != null)
                {
                    cachedPlayer = player;
                    lastCacheTime = Time.time;
                    return cachedPlayer;
                }

                return null;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error finding player object: {ex.Message}");
                return null;
            }
        }

        public static void ClearCache()
        {
            cachedPlayer = null;
        }
    }
}
