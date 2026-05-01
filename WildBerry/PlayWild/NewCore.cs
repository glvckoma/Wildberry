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

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("WildBerry v1.4 mod initialized.");
            LoggerInstance.Msg("Press F10 to toggle GUI.");

            featureManager = new FeatureManager();
            featureManager.Initialize();

            gui = new WildBerryGUI(featureManager);
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                gui.IsVisible = !gui.IsVisible;
                LoggerInstance.Msg($"[WildBerry] GUI: {(gui.IsVisible ? "SHOWN" : "HIDDEN")}");
            }

            featureManager.OnUpdate();
        }

        public override void OnGUI()
        {
            gui.OnGUI();
        }
    }
}
