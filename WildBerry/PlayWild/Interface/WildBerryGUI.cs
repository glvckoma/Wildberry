using UnityEngine;
using PlayWild.Features.Base;

namespace PlayWild.Interface
{
    public class WildBerryGUI
    {
        private FeatureManager featureManager;
        private int currentTab = 0;
        private string[] tabNames = { "Player", "World", "Minigames" };
        private Rect windowRect;
        private Vector2 scrollPosition = Vector2.zero;
        private bool isMinimized = false;
        private bool isDragging = false;
        private Vector2 dragOffset = Vector2.zero;

        public bool IsVisible { get; set; } = true;

        public WildBerryGUI(FeatureManager featureManager)
        {
            this.featureManager = featureManager;
            windowRect = new Rect(20, 20, WildBerryTheme.WindowWidth, 500);
        }

        public void OnGUI()
        {
            if (!IsVisible) return;

            if (!WildBerryTheme.IsInitialized)
                WildBerryTheme.Initialize();

            GUI.depth = 0;

            UpdateWindowSize();
            HandleDragging();

            windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
            windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);

            GUI.BeginGroup(windowRect);
            DrawWindowContents();
            GUI.EndGroup();
        }

        private void UpdateWindowSize()
        {
            float maxHeight = Mathf.Min(Screen.height * 0.75f, 650f);
            float contentHeight = featureManager.GetTabContentHeight(tabNames[currentTab]);
            float headerHeight = WildBerryTheme.TitleBarHeight + WildBerryTheme.TabBarHeight + WildBerryTheme.TabUnderlineHeight;

            windowRect.width = WildBerryTheme.WindowWidth;

            if (isMinimized)
            {
                windowRect.height = WildBerryTheme.TitleBarHeight;
            }
            else
            {
                float neededHeight = headerHeight + WildBerryTheme.ContentPadTop + contentHeight + WildBerryTheme.WindowPadding;
                windowRect.height = Mathf.Min(neededHeight, maxHeight);
            }
        }

        private void HandleDragging()
        {
            Event e = Event.current;
            if (e == null) return;

            Rect titleDragArea = new Rect(
                windowRect.x,
                windowRect.y,
                windowRect.width - 70,
                WildBerryTheme.TitleBarHeight);

            if (e.type == EventType.MouseDown && e.button == 0 && titleDragArea.Contains(e.mousePosition))
            {
                isDragging = true;
                dragOffset = e.mousePosition - new Vector2(windowRect.x, windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                windowRect.x = e.mousePosition.x - dragOffset.x;
                windowRect.y = e.mousePosition.y - dragOffset.y;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
            }
        }

        private void DrawWindowContents()
        {
            float contentHeight = featureManager.GetTabContentHeight(tabNames[currentTab]);
            float headerHeight = WildBerryTheme.TitleBarHeight + WildBerryTheme.TabBarHeight + WildBerryTheme.TabUnderlineHeight;
            float maxHeight = Mathf.Min(Screen.height * 0.75f, 650f);
            float availableContentHeight = maxHeight - headerHeight - WildBerryTheme.ContentPadTop - WildBerryTheme.WindowPadding;

            WildBerryTheme.DrawRoundedRect(new Rect(0, 0, windowRect.width, windowRect.height), WildBerryTheme.WindowBg);

            DrawTitleBar();

            if (!isMinimized)
            {
                DrawTabBar();
                DrawContent(headerHeight, availableContentHeight, contentHeight);
            }
        }

        private void DrawTitleBar()
        {
            Rect titleBarRect = new Rect(0, 0, windowRect.width, WildBerryTheme.TitleBarHeight);
            WildBerryTheme.DrawRoundedRect(titleBarRect, WildBerryTheme.TitleBarBg);

            Rect titleTextRect = new Rect(WildBerryTheme.WindowPadding, 0, 200, WildBerryTheme.TitleBarHeight);
            GUI.Label(titleTextRect, "WildBerry v2.0", WildBerryTheme.TitleStyle);

            float btnY = (WildBerryTheme.TitleBarHeight - WildBerryTheme.ControlButtonSize) / 2f;
            float closeX = windowRect.width - WildBerryTheme.WindowPadding - WildBerryTheme.ControlButtonSize;
            float minX = closeX - WildBerryTheme.ControlButtonSize - 6;

            Rect minRect = new Rect(minX, btnY, WildBerryTheme.ControlButtonSize, WildBerryTheme.ControlButtonSize);
            WildBerryTheme.DrawDot(minRect, WildBerryTheme.MinimizeButton);
            if (GUI.Button(minRect, "", GUIStyle.none))
                isMinimized = !isMinimized;

            Rect closeRect = new Rect(closeX, btnY, WildBerryTheme.ControlButtonSize, WildBerryTheme.ControlButtonSize);
            WildBerryTheme.DrawDot(closeRect, WildBerryTheme.CloseButton);
            if (GUI.Button(closeRect, "", GUIStyle.none))
                IsVisible = false;
        }

        private void DrawTabBar()
        {
            float tabBarY = WildBerryTheme.TitleBarHeight;
            Rect tabBarRect = new Rect(0, tabBarY, windowRect.width, WildBerryTheme.TabBarHeight);

            GUI.color = WildBerryTheme.TabBarBg;
            GUI.DrawTexture(tabBarRect, WildBerryTheme.Pixel);
            GUI.color = Color.white;

            float tabWidth = (windowRect.width - WildBerryTheme.WindowPadding * 2) / tabNames.Length;

            for (int i = 0; i < tabNames.Length; i++)
            {
                float tabX = WildBerryTheme.WindowPadding + i * tabWidth;
                Rect tabRect = new Rect(tabX, tabBarY, tabWidth, WildBerryTheme.TabBarHeight);

                GUIStyle style = (i == currentTab) ? WildBerryTheme.TabActiveStyle : WildBerryTheme.TabInactiveStyle;
                GUI.Label(tabRect, tabNames[i], style);

                if (i == currentTab)
                {
                    Rect underlineRect = new Rect(
                        tabX + tabWidth * 0.15f,
                        tabBarY + WildBerryTheme.TabBarHeight - WildBerryTheme.TabUnderlineHeight,
                        tabWidth * 0.7f,
                        WildBerryTheme.TabUnderlineHeight);
                    WildBerryTheme.DrawRoundedRect(underlineRect, WildBerryTheme.TabUnderline);
                }

                if (GUI.Button(tabRect, "", GUIStyle.none) && i != currentTab)
                {
                    currentTab = i;
                    scrollPosition = Vector2.zero;
                }
            }
        }

        private void DrawContent(float headerHeight, float availableHeight, float contentHeight)
        {
            float contentY = headerHeight + WildBerryTheme.ContentPadTop;
            float scrollAreaWidth = windowRect.width - WildBerryTheme.WindowPadding * 2;

            Rect scrollAreaRect = new Rect(
                WildBerryTheme.WindowPadding,
                contentY,
                scrollAreaWidth,
                Mathf.Min(availableHeight, contentHeight + WildBerryTheme.WindowPadding));

            Rect viewRect = new Rect(0, 0, scrollAreaWidth - WildBerryTheme.ScrollbarWidth - 4, contentHeight);

            scrollPosition = GUI.BeginScrollView(
                scrollAreaRect,
                scrollPosition,
                viewRect,
                false,
                contentHeight > availableHeight,
                GUIStyle.none,
                WildBerryTheme.VertThumbStyle);

            Rect contentRect = new Rect(0, 0, viewRect.width, contentHeight);

            switch (currentTab)
            {
                case 0:
                    featureManager.DrawPlayerTab(contentRect);
                    break;
                case 1:
                    featureManager.DrawWorldTab(contentRect);
                    break;
                case 2:
                    featureManager.DrawMinigamesTab(contentRect);
                    break;
            }

            GUI.EndScrollView();
        }
    }
}
