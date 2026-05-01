using System.Collections.Generic;
using UnityEngine;
using PlayWild.Features.Player;
using PlayWild.Features.World;
using PlayWild.Features.Minigames;
using PlayWild.Utils;
using PlayWild.Interface;

namespace PlayWild.Features.Base
{
    public class FeatureManager
    {
        private List<IFeature> _playerFeatures = new List<IFeature>();
        private List<IFeature> _worldFeatures = new List<IFeature>();
        private List<IFeature> _minigameFeatures = new List<IFeature>();

        public void Initialize()
        {
            PersistenceManager.LoadConfig();

            _playerFeatures.Add(new SpeedHack());
            _playerFeatures.Add(new SizeHack());
            _playerFeatures.Add(new FlyingMode());
            _playerFeatures.Add(new PlayerTeleport());
            _playerFeatures.Add(new PremiumMember());
            _playerFeatures.Add(new BypassMembershipLimit());
            _playerFeatures.Add(new DeviceIDSpoofing());
            _playerFeatures.Add(new AntiAFK());

            _worldFeatures.Add(new AutoNocturnal());
            _worldFeatures.Add(new AutoFact());
            _worldFeatures.Add(new TimeHack());
            _worldFeatures.Add(new UsernameLogger());
            _worldFeatures.Add(new AutoAdvertising());
            _worldFeatures.Add(new ExpeditionBypass());
            _worldFeatures.Add(new RoomInfo());

            _minigameFeatures.Add(new AlwaysShowDigRewards());
            _minigameFeatures.Add(new AutoCompleteHedgehogRoll());
            _minigameFeatures.Add(new MassiveBallBlockBreak());
            _minigameFeatures.Add(new PestControl());
            _minigameFeatures.Add(new PhantomImmunity());
            _minigameFeatures.Add(new PhantomDimension());
            _minigameFeatures.Add(new TreasureHunterSolver());

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
            DrawFeaturesTab(_playerFeatures, area, WildBerryTheme.PlayerDot);
        }

        public void DrawWorldTab(Rect area)
        {
            DrawFeaturesTab(_worldFeatures, area, WildBerryTheme.WorldDot);
        }

        public void DrawMinigamesTab(Rect area)
        {
            DrawFeaturesTab(_minigameFeatures, area, WildBerryTheme.MinigamesDot);
        }

        private void DrawFeaturesTab(List<IFeature> features, Rect area, Color dotColor)
        {
            float yOffset = 0;
            foreach (var feature in features)
            {
                float featureContentHeight = feature.GetDynamicHeight();
                float cardHeight = featureContentHeight + WildBerryTheme.CardPadInner * 2;

                Rect cardRect = new Rect(area.x, area.y + yOffset, area.width, cardHeight);
                Rect innerRect = WildBerryTheme.DrawCard(cardRect);

                float dotY = innerRect.y + (WildBerryTheme.ToggleHeight - WildBerryTheme.DotSize) / 2f;
                WildBerryTheme.DrawDot(
                    new Rect(innerRect.x, dotY, WildBerryTheme.DotSize, WildBerryTheme.DotSize),
                    dotColor);

                Rect featureRect = new Rect(
                    innerRect.x + WildBerryTheme.DotSize + WildBerryTheme.DotMargin,
                    innerRect.y,
                    innerRect.width - WildBerryTheme.DotSize - WildBerryTheme.DotMargin,
                    featureContentHeight);

                feature.OnGUI(featureRect);

                yOffset += cardHeight + WildBerryTheme.CardPadOuter;
            }
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
                float featureContentHeight = feature.GetDynamicHeight();
                float cardHeight = featureContentHeight + WildBerryTheme.CardPadInner * 2;
                totalHeight += cardHeight + WildBerryTheme.CardPadOuter;
            }

            return totalHeight;
        }
    }
}
