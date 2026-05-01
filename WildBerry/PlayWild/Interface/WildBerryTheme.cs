using UnityEngine;

namespace PlayWild.Interface
{
    public static class WildBerryTheme
    {
        public static bool IsInitialized { get; private set; }

        public static readonly Color WindowBg = new Color(0.102f, 0.102f, 0.180f, 0.92f);
        public static readonly Color CardBg = new Color(0.086f, 0.129f, 0.243f, 0.85f);
        public static readonly Color CardBorder = new Color(0.059f, 0.204f, 0.376f, 0.60f);
        public static readonly Color TitleBarBg = new Color(0.067f, 0.067f, 0.133f, 0.95f);
        public static readonly Color TabBarBg = new Color(0.078f, 0.078f, 0.153f, 0.90f);

        public static readonly Color ActiveTabText = Color.white;
        public static readonly Color InactiveTabText = new Color(0.55f, 0.55f, 0.65f, 1.0f);
        public static readonly Color TabUnderline = new Color(0.337f, 0.502f, 0.965f, 1.0f);

        public static readonly Color ToggleOn = new Color(0.337f, 0.502f, 0.965f, 1.0f);
        public static readonly Color ToggleOff = new Color(0.25f, 0.25f, 0.30f, 1.0f);
        public static readonly Color ToggleKnobColor = Color.white;

        public static readonly Color FeatureNameColor = Color.white;
        public static readonly Color StatusText = new Color(0.55f, 0.60f, 0.70f, 1.0f);
        public static readonly Color StatusActive = new Color(0.40f, 0.85f, 0.55f, 1.0f);
        public static readonly Color StatusWarning = new Color(0.95f, 0.85f, 0.30f, 1.0f);
        public static readonly Color StatusInfo = new Color(0.40f, 0.78f, 0.95f, 1.0f);

        public static readonly Color TextFieldBg = new Color(0.12f, 0.12f, 0.20f, 1.0f);
        public static readonly Color TextFieldBorderColor = new Color(0.20f, 0.20f, 0.30f, 1.0f);
        public static readonly Color TextFieldFocusBorder = new Color(0.337f, 0.502f, 0.965f, 0.70f);

        public static readonly Color ButtonNormal = new Color(0.20f, 0.22f, 0.32f, 1.0f);
        public static readonly Color ButtonHover = new Color(0.25f, 0.28f, 0.40f, 1.0f);
        public static readonly Color ButtonActive = new Color(0.337f, 0.502f, 0.965f, 1.0f);

        public static readonly Color ScrollTrack = new Color(0.10f, 0.10f, 0.16f, 0.50f);
        public static readonly Color ScrollThumb = new Color(0.25f, 0.25f, 0.35f, 0.70f);

        public static readonly Color CloseButton = new Color(0.85f, 0.30f, 0.35f, 1.0f);
        public static readonly Color MinimizeButton = new Color(0.95f, 0.85f, 0.30f, 1.0f);

        public static readonly Color PlayerDot = new Color(0.337f, 0.502f, 0.965f, 1.0f);
        public static readonly Color WorldDot = new Color(0.40f, 0.85f, 0.55f, 1.0f);
        public static readonly Color MinigamesDot = new Color(0.95f, 0.60f, 0.30f, 1.0f);

        public const float WindowWidth = 420f;
        public const float WindowPadding = 12f;
        public const float TitleBarHeight = 36f;
        public const float TabBarHeight = 36f;
        public const float TabUnderlineHeight = 3f;
        public const float ContentPadTop = 8f;

        public const float CardPadOuter = 6f;
        public const float CardPadInner = 10f;
        public const float CardBorderWidth = 1f;
        public const float CardBorderRadius = 6f;

        public const float ToggleWidth = 36f;
        public const float ToggleHeight = 20f;
        public const float ToggleKnobSize = 16f;
        public const float ToggleKnobPad = 2f;

        public const float SubToggleWidth = 28f;
        public const float SubToggleHeight = 14f;
        public const float SubToggleKnobSize = 10f;

        public const float TextFieldHeight = 24f;
        public const float ButtonHeight = 26f;
        public const float DotSize = 8f;
        public const float DotMargin = 8f;
        public const float ScrollbarWidth = 6f;
        public const float ControlButtonSize = 14f;

        public static Texture2D RoundedRect;
        public static Texture2D CircleDot;
        public static Texture2D ToggleTrackTex;
        public static Texture2D Pixel;

        public static Texture2D TextFieldNormalBg;
        public static Texture2D TextFieldFocusBg;
        public static Texture2D ButtonNormalBg;
        public static Texture2D ButtonHoverBg;
        public static Texture2D ButtonActiveBg;
        public static Texture2D ScrollThumbTex;
        public static Texture2D ScrollTrackTex;

        public static GUIStyle TitleStyle;
        public static GUIStyle TabActiveStyle;
        public static GUIStyle TabInactiveStyle;
        public static GUIStyle FeatureNameStyle;
        public static GUIStyle StatusStyle;
        public static GUIStyle LabelStyle;
        public static GUIStyle TextFieldStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle VertScrollStyle;
        public static GUIStyle VertThumbStyle;
        public static GUIStyle TextAreaStyle;

        public static void Initialize()
        {
            if (IsInitialized) return;

            GenerateTextures();
            CreateStyles();

            IsInitialized = true;
        }

        private static void GenerateTextures()
        {
            Pixel = CreateSolidTexture(1, 1, Color.white);

            RoundedRect = CreateRoundedRectTexture(64, 64, 10);
            CircleDot = CreateCircleTexture(16);
            ToggleTrackTex = CreatePillTexture(36, 20);

            TextFieldNormalBg = CreateTintedRoundedRect(48, 24, 6, TextFieldBg, TextFieldBorderColor);
            TextFieldFocusBg = CreateTintedRoundedRect(48, 24, 6, TextFieldBg, TextFieldFocusBorder);
            ButtonNormalBg = CreateTintedRoundedRect(48, 26, 6, ButtonNormal, new Color(0.25f, 0.25f, 0.35f, 0.5f));
            ButtonHoverBg = CreateTintedRoundedRect(48, 26, 6, ButtonHover, new Color(0.30f, 0.30f, 0.45f, 0.6f));
            ButtonActiveBg = CreateTintedRoundedRect(48, 26, 6, ButtonActive, TabUnderline);
            ScrollThumbTex = CreateRoundedRectTexture(6, 32, 3);
            ScrollTrackTex = CreateSolidTexture(6, 32, ScrollTrack);
        }

        private static void CreateStyles()
        {
            TitleStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            TabActiveStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            TabInactiveStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = InactiveTabText }
            };

            FeatureNameStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            StatusStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = StatusText }
            };

            LabelStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            TextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 6, 3, 3),
                border = new RectOffset(8, 8, 8, 8),
                normal = { background = TextFieldNormalBg, textColor = Color.white },
                focused = { background = TextFieldFocusBg, textColor = Color.white },
                hover = { background = TextFieldNormalBg, textColor = Color.white },
                active = { background = TextFieldFocusBg, textColor = Color.white }
            };

            ButtonStyle = new GUIStyle(GUIStyle.none)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(12, 12, 4, 4),
                border = new RectOffset(8, 8, 8, 8),
                normal = { background = ButtonNormalBg, textColor = Color.white },
                hover = { background = ButtonHoverBg, textColor = Color.white },
                active = { background = ButtonActiveBg, textColor = Color.white }
            };

            TextAreaStyle = new GUIStyle(TextFieldStyle)
            {
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };

            VertScrollStyle = new GUIStyle(GUIStyle.none)
            {
                fixedWidth = ScrollbarWidth
            };

            VertThumbStyle = new GUIStyle(GUIStyle.none)
            {
                fixedWidth = ScrollbarWidth,
                normal = { background = ScrollThumbTex },
                border = new RectOffset(3, 3, 3, 3)
            };
        }

        private static Texture2D CreateSolidTexture(int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static Texture2D CreateRoundedRectTexture(int w, int h, int radius)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = RoundedRectAlpha(x, y, w, h, radius);
                    pixels[y * w + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static Texture2D CreateCircleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float center = (size - 1) / 2f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01((radius - dist + 0.5f) * 2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static Texture2D CreatePillTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[w * h];
            int radius = h / 2;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = RoundedRectAlpha(x, y, w, h, radius);
                    pixels[y * w + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static Texture2D CreateTintedRoundedRect(int w, int h, int radius, Color fill, Color border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float outerAlpha = RoundedRectAlpha(x, y, w, h, radius);
                    float innerAlpha = RoundedRectAlpha(x, y, w, h, Mathf.Max(radius - 1, 0), 1f);

                    if (outerAlpha <= 0f)
                    {
                        pixels[y * w + x] = Color.clear;
                    }
                    else if (innerAlpha >= 1f)
                    {
                        pixels[y * w + x] = fill;
                    }
                    else
                    {
                        Color blended = Color.Lerp(border, fill, innerAlpha);
                        blended.a = outerAlpha;
                        pixels[y * w + x] = blended;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static float RoundedRectAlpha(int px, int py, int w, int h, int radius, float borderSoftness = 1.5f)
        {
            float x = px;
            float y = py;
            float r = radius;

            float dx = 0f;
            float dy = 0f;

            if (x < r) dx = r - x;
            else if (x > w - 1 - r) dx = x - (w - 1 - r);

            if (y < r) dy = r - y;
            else if (y > h - 1 - r) dy = y - (h - 1 - r);

            if (dx > 0 && dy > 0)
            {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                return Mathf.Clamp01((r - dist + borderSoftness) / borderSoftness);
            }

            return 1f;
        }

        public static void DrawRoundedRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, RoundedRect, ScaleMode.StretchToFill);
            GUI.color = Color.white;
        }

        public static Rect DrawCard(Rect cardRect)
        {
            DrawRoundedRect(cardRect, CardBorder);
            Rect innerBg = new Rect(
                cardRect.x + CardBorderWidth,
                cardRect.y + CardBorderWidth,
                cardRect.width - CardBorderWidth * 2,
                cardRect.height - CardBorderWidth * 2);
            DrawRoundedRect(innerBg, CardBg);

            return new Rect(
                cardRect.x + CardPadInner,
                cardRect.y + CardPadInner,
                cardRect.width - CardPadInner * 2,
                cardRect.height - CardPadInner * 2);
        }

        public static void DrawDot(Rect position, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(position, CircleDot, ScaleMode.ScaleToFit);
            GUI.color = Color.white;
        }
    }
}
