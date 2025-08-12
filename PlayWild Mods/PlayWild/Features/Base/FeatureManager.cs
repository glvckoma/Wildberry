using System.Collections.Generic;
using UnityEngine;
using PlayWild.Features.Player;
using PlayWild.Features.World;
using PlayWild.Features.Minigames;
using PlayWild.Utils;

namespace PlayWild.Features.Base
{
    public class FeatureManager
    {
        private List<IFeature> _playerFeatures = new List<IFeature>();
        private List<IFeature> _worldFeatures = new List<IFeature>();
        private List<IFeature> _minigameFeatures = new List<IFeature>();

        public void Initialize()
        {
            // Initialize persistence system first
            PersistenceManager.LoadConfig();
            // Initialize Player Features
            _playerFeatures.Add(new SpeedHack());
            _playerFeatures.Add(new SizeHack());
            _playerFeatures.Add(new FlyingMode());
            _playerFeatures.Add(new PremiumMember());
            _playerFeatures.Add(new DeviceIDSpoofing());
            _playerFeatures.Add(new AntiAFK());

            // Initialize World Features
            _worldFeatures.Add(new AutoNocturnal());
            _worldFeatures.Add(new AutoFact());
            _worldFeatures.Add(new TimeHack());
            _worldFeatures.Add(new UsernameLogger());
            _worldFeatures.Add(new AutoAdvertising());

            // Initialize Minigame Features
            _minigameFeatures.Add(new AlwaysShowDigRewards());
            _minigameFeatures.Add(new AutoCompleteHedgehogRoll());
            _minigameFeatures.Add(new MassiveBallBlockBreak());
            _minigameFeatures.Add(new PestControl());
            _minigameFeatures.Add(new PhantomImmunity());
            _minigameFeatures.Add(new PhantomDimension());

            // Initialize all features
            InitializeFeatures(_playerFeatures);
            InitializeFeatures(_worldFeatures);
            InitializeFeatures(_minigameFeatures);
        }

        private void InitializeFeatures(List<IFeature> features)
        {
            foreach (var feature in features)
            {
                feature.Initialize();
            }
        }

        public void OnUpdate()
        {
            UpdateFeatures(_playerFeatures);
            UpdateFeatures(_worldFeatures);
            UpdateFeatures(_minigameFeatures);
        }

        private void UpdateFeatures(List<IFeature> features)
        {
            foreach (var feature in features)
            {
                if (feature.IsEnabled)
                {
                    feature.OnUpdate();
                }
            }
        }

        public void DrawPlayerTab(Rect area)
        {
            DrawFeaturesTab(_playerFeatures, area);
        }

        public void DrawWorldTab(Rect area)
        {
            DrawFeaturesTab(_worldFeatures, area);
        }

        public void DrawMinigamesTab(Rect area)
        {
            DrawFeaturesTab(_minigameFeatures, area);
        }

        private void DrawFeaturesTab(List<IFeature> features, Rect area)
        {
            float yOffset = 0;
            foreach (var feature in features)
            {
                // Calculate dynamic height based on feature type
                float featureHeight = GetFeatureHeight(feature);
                
                Rect featureRect = new Rect(area.x, area.y + yOffset, area.width, featureHeight);
                feature.OnGUI(featureRect);
                
                // Add spacing between features
                yOffset += featureHeight + 10;
            }
        }

        private float GetFeatureHeight(IFeature feature)
        {
            // Use GetDynamicHeight() method for all features to ensure consistent behavior
            return feature.GetDynamicHeight();
        }

        public float GetTabContentHeight(string tabName)
        {
            List<IFeature> features = null;
            switch (tabName)
            {
                case "Player":
                    features = _playerFeatures;
                    break;
                case "World":
                    features = _worldFeatures;
                    break;
                case "Minigames":
                    features = _minigameFeatures;
                    break;
                default:
                    return 300f;
            }

            float totalHeight = 0;
            foreach (var feature in features)
            {
                float featureHeight = GetFeatureHeight(feature);
                totalHeight += featureHeight + 10; // Add spacing
            }

            return totalHeight;
        }
    }
}
