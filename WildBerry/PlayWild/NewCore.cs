using MelonLoader;
using UnityEngine;
using PlayWild.Features.Base;
using PlayWild.Interface;

[assembly: MelonInfo(typeof(PlayWild.Core), "PlayWild", "1.0.0", "glockoma", null)]
[assembly: MelonGame("WildWorks", "Animal Jam")]

namespace PlayWild
{
    public class Core : MelonMod
    {
        private FeatureManager featureManager;
        private WildBerryGUI gui;
        private bool showGUI = false;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("WildBerry v1.0 mod initialized.");
            LoggerInstance.Msg("Press F10 to toggle GUI.");
            
            // Initialize the modular system
            featureManager = new FeatureManager();
            featureManager.Initialize();
            
            gui = new WildBerryGUI(featureManager);
        }

        public override void OnUpdate()
        {
            // Toggle GUI visibility with F10
            if (Input.GetKeyDown(KeyCode.F10))
            {
                showGUI = !showGUI;
                LoggerInstance.Msg($"[WildBerry] GUI: {(showGUI ? "SHOWN" : "HIDDEN")}");
            }

            // Update all enabled features
            featureManager.OnUpdate();
        }

        public override void OnGUI()
        {
            if (!showGUI) return;
            gui.OnGUI();
        }
    }
}