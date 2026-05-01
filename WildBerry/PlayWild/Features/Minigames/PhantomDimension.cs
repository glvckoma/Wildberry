using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;
using MelonLoader;
using System.Collections.Generic;

namespace PlayWild.Features.Minigames
{
    public class PhantomDimension : BaseFeature
    {
        public override string Name => "Phantom Dimension";

        private bool isPhDActive = false;
        private Il2CppMiniGames.PhD.PhDGame phdGame = null;
        private float lastTapTime = 0f;
        private bool wasInBattle = false;

        private bool infiniteLives = false;
        private bool maxKillCount = false;
        private bool phantomAvoidance = false;
        private bool perfectTapAutomation = false;

        public override void Initialize()
        {
            base.Initialize();
            infiniteLives = GetPersistedValue("infiniteLives", false);
            maxKillCount = GetPersistedValue("maxKillCount", false);
            phantomAvoidance = GetPersistedValue("phantomAvoidance", false);
            perfectTapAutomation = GetPersistedValue("perfectTapAutomation", false);
        }

        public override void OnUpdate()
        {
            if (!IsEnabled)
            {
                if (isPhDActive)
                {
                    RestorePhDValues();
                    isPhDActive = false;
                }
                return;
            }

            DetectAndModifyPhD();
        }

        public override void OnGUI(Rect area)
        {
            float yOffset = 0;

            DrawToggle(new Rect(area.x, area.y + yOffset, area.width, WildBerryTheme.ToggleHeight), IsEnabled, Name,
                (value) => { IsEnabled = value; });
            yOffset += 25;

            if (IsEnabled)
            {
                DrawSubToggle(new Rect(area.x + 20, area.y + yOffset, area.width - 20, WildBerryTheme.SubToggleHeight), infiniteLives, "Infinite Lives",
                    (value) => { infiniteLives = value; SetPersistedValue("infiniteLives", value); });
                yOffset += 22;

                DrawSubToggle(new Rect(area.x + 20, area.y + yOffset, area.width - 20, WildBerryTheme.SubToggleHeight), maxKillCount, "Max Kill Count",
                    (value) => { maxKillCount = value; SetPersistedValue("maxKillCount", value); });
                yOffset += 22;

                DrawSubToggle(new Rect(area.x + 20, area.y + yOffset, area.width - 20, WildBerryTheme.SubToggleHeight), phantomAvoidance, "Phantom Avoidance",
                    (value) => { phantomAvoidance = value; SetPersistedValue("phantomAvoidance", value); });
                yOffset += 22;

                DrawSubToggle(new Rect(area.x + 20, area.y + yOffset, area.width - 20, WildBerryTheme.SubToggleHeight), perfectTapAutomation, "Perfect Tap Auto",
                    (value) => { perfectTapAutomation = value; SetPersistedValue("perfectTapAutomation", value); });
                yOffset += 22;

                if (isPhDActive)
                {
                    DrawStatusLabel(new Rect(area.x + 20, area.y + yOffset, 200, 20), "PhD Active - Godmode ON", WildBerryTheme.StatusActive);
                }
                else
                {
                    DrawStatusLabel(new Rect(area.x + 20, area.y + yOffset, 200, 20), "PhD Inactive", WildBerryTheme.StatusText);
                }
            }
        }

        private void DetectAndModifyPhD()
        {
            try
            {
                var phdGameComponent = UnityEngine.Object.FindObjectOfType<Il2CppMiniGames.PhD.PhDGame>();

                if (phdGameComponent != null)
                {
                    phdGame = phdGameComponent;

                    if (!isPhDActive)
                    {
                        MelonLogger.Msg("[WildBerry] Phantom Dimension detected! Activating godmode features...");
                        isPhDActive = true;
                    }

                    ApplyPhDModifications();
                }
                else if (isPhDActive)
                {
                    MelonLogger.Msg("[WildBerry] PhD ended, deactivating godmode features.");
                    isPhDActive = false;
                    phdGame = null;
                    wasInBattle = false;
                }

                if (isPhDActive && perfectTapAutomation)
                {
                    HandlePerfectTapAutomation();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in PhD detection: {ex.Message}");
            }
        }

        private void ApplyPhDModifications()
        {
            if (phdGame == null) return;

            try
            {
                if (infiniteLives)
                {
                    try { phdGame.PlayerStrikes = -99999; } catch { }
                }

                if (maxKillCount)
                {
                    try
                    {
                        if (phdGame.KillCount < 99999)
                        {
                            phdGame.KillCount = 99999;
                            MelonLogger.Msg("[WildBerry] PhD: Set kill count to 99999 for area unlocks");
                        }
                    }
                    catch { }
                }

                if (phantomAvoidance)
                {
                    ApplyPhantomAvoidance();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error applying PhD modifications: {ex.Message}");
            }
        }

        private void ApplyPhantomAvoidance()
        {
            try
            {
                var phantoms = UnityEngine.Object.FindObjectsOfType<Il2CppMiniGames.PhD.PhDEnemyPhantom>();

                foreach (var phantom in phantoms)
                {
                    if (phantom == null) continue;

                    var collider = phantom.GetComponent<Collider>();
                    if (collider != null && collider.enabled)
                    {
                        collider.enabled = false;
                    }

                    if (phantom.gameObject.activeInHierarchy)
                    {
                        phantom.gameObject.SetActive(false);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in phantom avoidance: {ex.Message}");
            }
        }

        private void HandlePerfectTapAutomation()
        {
            try
            {
                bool isCurrentlyInBattle = phdGame != null && phdGame.IsInBattle;

                if (isCurrentlyInBattle && !wasInBattle)
                {
                    MelonLogger.Msg("[WildBerry] PhD: Entered battle, starting perfect tap automation");
                    wasInBattle = true;
                }
                else if (!isCurrentlyInBattle && wasInBattle)
                {
                    MelonLogger.Msg("[WildBerry] PhD: Exited battle");
                    wasInBattle = false;
                }

                if (!isCurrentlyInBattle) return;

                var battleArena = UnityEngine.Object.FindObjectOfType<Il2CppMiniGames.PhD.BattleArena>();
                if (battleArena == null) return;

                float tapInterval = UnityEngine.Random.Range(0.7f, 1.2f);
                if (Time.time - lastTapTime > tapInterval)
                {
                    try
                    {
                        Il2CppMiniGames.PhD.TapResult tapResult = Il2CppMiniGames.PhD.TapResult.Perfect;
                        if (UnityEngine.Random.Range(0f, 1f) < 0.15f)
                        {
                            tapResult = Il2CppMiniGames.PhD.TapResult.Good;
                        }

                        battleArena.OnSequenceComplete(tapResult);
                        lastTapTime = Time.time;
                    }
                    catch (System.Exception tapEx)
                    {
                        MelonLogger.Error($"[WildBerry] PhD: BattleArena.OnSequenceComplete error: {tapEx.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error in perfect tap automation: {ex.Message}");
            }
        }

        private void RestorePhDValues()
        {
            try
            {
                if (phdGame != null)
                {
                    MelonLogger.Msg("[WildBerry] PhD: Restored normal values");
                }
                wasInBattle = false;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WildBerry] Error restoring PhD values: {ex.Message}");
            }
        }

        public override void OnDisable()
        {
            if (isPhDActive)
            {
                RestorePhDValues();
                isPhDActive = false;
            }
        }

        public override float GetDynamicHeight()
        {
            if (!IsEnabled) return WildBerryTheme.ToggleHeight;
            return 25f + (4 * 22f) + 20f + 5f;
        }
    }
}
