using UnityEngine;
using PlayWild.Features.Base;

namespace PlayWild.Interface
{
    public class WildBerryGUI
    {
        private int currentTab = 0; // 0=Player, 1=World, 2=Minigames
        private string[] tabNames = { "Player", "World", "Minigames" };
        private FeatureManager featureManager;

        public WildBerryGUI(FeatureManager featureManager)
        {
            this.featureManager = featureManager;
        }

        public void OnGUI()
        {
            // Calculate dynamic height based on current tab content
            float contentHeight = GetCurrentTabHeight();
            float totalHeight = 85 + contentHeight + 20; // Header + content + padding
            
            // Ensure minimum height
            totalHeight = Mathf.Max(totalHeight, 200f);
            
            // Draw solid black background using DrawTexture with dynamic size
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(10, 10, 380, totalHeight), Texture2D.whiteTexture);
            
            // Title
            GUI.color = Color.white;
            GUI.Label(new Rect(15, 15, 240, 20), "WildBerry v1.4");
            
            // Draw tabs
            DrawTabs();
            
            // Draw content based on current tab with dynamic area
            Rect contentArea = new Rect(15, 85, 350, contentHeight);
            switch (currentTab)
            {
                case 0: // Player
                    featureManager.DrawPlayerTab(contentArea);
                    break;
                case 1: // World
                    featureManager.DrawWorldTab(contentArea);
                    break;
                case 2: // Minigames
                    featureManager.DrawMinigamesTab(contentArea);
                    break;
            }
        }

        private void DrawTabs()
        {
            int tabWidth = 80;
            int tabHeight = 35;
            int startY = 40;
            
            for (int i = 0; i < tabNames.Length; i++)
            {
                Rect tabRect = new Rect(15 + (i * tabWidth), startY, tabWidth, tabHeight);
                
                // Draw tab background
                if (currentTab == i)
                {
                    GUI.color = Color.white; // Active tab
                }
                else
                {
                    GUI.color = Color.gray; // Inactive tab
                }
                GUI.DrawTexture(tabRect, Texture2D.whiteTexture);
                
                // Draw tab border
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(tabRect.x + 1, tabRect.y + 1, tabRect.width - 2, tabRect.height - 2), Texture2D.whiteTexture);
                
                // Tab text
                GUI.color = Color.white;
                GUI.Label(new Rect(tabRect.x + 5, tabRect.y + 5, tabRect.width - 10, tabRect.height - 10), tabNames[i]);
                
                // Tab click button
                if (GUI.Button(tabRect, "", GUIStyle.none))
                {
                    currentTab = i;
                }
            }
        }

        private float GetCurrentTabHeight()
        {
            // Get the required height for the current tab's content
            switch (currentTab)
            {
                case 0: // Player
                    return featureManager.GetTabContentHeight("Player");
                case 1: // World  
                    return featureManager.GetTabContentHeight("World");
                case 2: // Minigames
                    return featureManager.GetTabContentHeight("Minigames");
                default:
                    return 300f; // Default height
            }
        }
    }
}