using UnityEngine;
using TMPro;

namespace ActionRPG.UI
{
    /// <summary>
    /// Trình quản lý Font hệ thống cho toàn bộ UI và HUD của Lumina.
    /// Đảm bảo font chữ luôn là Times New Roman sắc nét, dễ đọc và hiển thị hoàn hảo tiếng Việt có dấu.
    /// </summary>
    public static class FontManager
    {
        private static TMP_FontAsset cachedTmpFont;
        private static TMP_FontAsset cachedFallbackFont;
        private static Font cachedLegacyFont;
        private static Sprite cachedNineSlicePanelSprite;
        private static Sprite cachedNineSliceSlotSprite;

        // Bảng màu chuẩn Lumina 16-bit Pixel Art UI (Section 4C)
        public static readonly Color PanelBgColor = new Color(0.07f, 0.08f, 0.11f, 0.94f);       // Xám đen 94%
        public static readonly Color HeaderBgColor = new Color(0.11f, 0.13f, 0.18f, 0.98f);      // Nền thanh tiêu đề
        public static readonly Color SectionBgColor = new Color(0.10f, 0.12f, 0.16f, 0.95f);     // Nền khung con
        public static readonly Color SlotEmptyBgColor = new Color(0.11f, 0.13f, 0.18f, 0.98f);   // Nền ô trống (Trị lỗi ô trắng)
        public static readonly Color BronzeGoldBorder = new Color(0.85f, 0.70f, 0.25f, 0.95f);   // Viền Vàng đồng
        public static readonly Color GoldTitleColor = new Color(1.00f, 0.86f, 0.35f, 1.00f);     // Chữ tiêu đề Vàng đồng
        public static readonly Color IvoryTextColor = new Color(0.97f, 0.95f, 0.90f, 1.00f);     // Chữ chính Trắng ngà
        public static readonly Color AetherCyanColor = new Color(0.00f, 0.90f, 1.00f, 1.00f);    // Năng lượng Lumina Aether Xanh ngọc
        public static readonly Color AetherBlueColor = new Color(0.14f, 0.36f, 0.58f, 0.96f);    // Nền chọn Xanh lam Lumina

        public static TMP_FontAsset GetTimesNewRomanTMP()
        {
            if (cachedTmpFont != null && IsFontAssetValid(cachedTmpFont))
            {
                EnsureFallbackFontAttached(cachedTmpFont);
                return cachedTmpFont;
            }

            // 1. Thử nạp Times New Roman SDF từ Resources
            cachedTmpFont = Resources.Load<TMP_FontAsset>("Fonts/TimesNewRoman SDF");
            if (cachedTmpFont == null || !IsFontAssetValid(cachedTmpFont))
            {
                cachedTmpFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/TimesNewRoman SDF");
            }

#if UNITY_EDITOR
            // 2. Thử nạp từ Assets/Fonts khi chạy Editor
            if (cachedTmpFont == null || !IsFontAssetValid(cachedTmpFont))
            {
                cachedTmpFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/TimesNewRoman SDF.asset");
            }
#endif

            // 3. Fallback an toàn tuyệt đối: Sử dụng font chuẩn mặc định của TextMesh Pro (LiberationSans SDF)
            if (cachedTmpFont == null || !IsFontAssetValid(cachedTmpFont))
            {
                cachedTmpFont = TMPro.TMP_Settings.defaultFontAsset;
            }

            if (cachedTmpFont == null || !IsFontAssetValid(cachedTmpFont))
            {
                cachedTmpFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

#if UNITY_EDITOR
            if (cachedTmpFont == null || !IsFontAssetValid(cachedTmpFont))
            {
                cachedTmpFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            }
#endif
            EnsureFallbackFontAttached(cachedTmpFont);
            return cachedTmpFont;
        }

        private static void EnsureFallbackFontAttached(TMP_FontAsset primaryFont)
        {
            if (primaryFont == null) return;
            if (cachedFallbackFont == null)
            {
                cachedFallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
#if UNITY_EDITOR
                if (cachedFallbackFont == null)
                {
                    cachedFallbackFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                }
#endif
            }

            if (cachedFallbackFont != null && cachedFallbackFont != primaryFont && IsFontAssetValid(cachedFallbackFont))
            {
                if (primaryFont.fallbackFontAssetTable == null)
                {
                    primaryFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                }
                if (!primaryFont.fallbackFontAssetTable.Contains(cachedFallbackFont))
                {
                    primaryFont.fallbackFontAssetTable.Add(cachedFallbackFont);
                }
            }
        }

        private static bool IsFontAssetValid(TMP_FontAsset font)
        {
            if (font == null) return false;
            if (font.material == null) return false;
            // Font hỏng khi material.mainTexture là null và không có texture atlas hợp lệ
            if (font.material.mainTexture == null && (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null))
            {
                return false;
            }
            return true;
        }

        public static Font GetTimesNewRomanLegacy()
        {
            if (cachedLegacyFont != null) return cachedLegacyFont;

            cachedLegacyFont = Resources.Load<Font>("Fonts/TimesNewRoman");
#if UNITY_EDITOR
            if (cachedLegacyFont == null)
            {
                cachedLegacyFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/TimesNewRoman.ttf");
            }
#endif
            if (cachedLegacyFont == null)
            {
                cachedLegacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return cachedLegacyFont;
        }

        /// <summary>
        /// Áp dụng font chữ đảm bảo hiển thị 100% cho đối tượng TextMeshPro bất kỳ,
        /// bật Extra Padding tránh cắt dấu tiếng Việt và khóa Scale = (1, 1, 1).
        /// </summary>
        public static void ApplyFont(TMP_Text text)
        {
            if (text == null) return;
            var font = GetTimesNewRomanTMP();
            if (font != null && IsFontAssetValid(font))
            {
                text.font = font;
            }
            text.extraPadding = true;
            if (text.rectTransform != null)
            {
                text.rectTransform.localScale = Vector3.one;
                SnapRectToIntegers(text.rectTransform);
            }
        }

        /// <summary>
        /// Áp dụng font Times New Roman cho đối tượng UI Text cổ điển.
        /// </summary>
        public static void ApplyFont(UnityEngine.UI.Text text)
        {
            if (text == null) return;
            var font = GetTimesNewRomanLegacy();
            if (font != null)
            {
                text.font = font;
            }
            if (text.rectTransform != null)
            {
                text.rectTransform.localScale = Vector3.one;
                SnapRectToIntegers(text.rectTransform);
            }
        }

        /// <summary>
        /// Làm tròn tọa độ Position (X, Y) và kích thước Width/Height của RectTransform thành số nguyên (Integer)
        /// để tránh hiện tượng mờ pixel (Sub-pixel blurring).
        /// </summary>
        public static void SnapRectToIntegers(RectTransform rt)
        {
            if (rt == null) return;
            rt.localScale = Vector3.one;
            Vector2 pos = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            Vector2 size = rt.sizeDelta;
            rt.sizeDelta = new Vector2(Mathf.Round(size.x), Mathf.Round(size.y));
        }

        /// <summary>
        /// Làm tròn toàn bộ RectTransform và chuẩn hóa mọi TMP_Text trong cây phân cấp UI.
        /// </summary>
        public static void SanitizeUIHierarchy(Transform root)
        {
            if (root == null) return;
            foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                SnapRectToIntegers(rt);
            }
            foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
            {
                ApplyFont(tmp);
            }
        }

        /// <summary>
        /// Tạo hoặc lấy Sprite 9-Slice sắc nét (Point filter, không nén) cho các khung Panel và Ô Slot,
        /// đảm bảo không bao giờ bị lỗi ô trắng xóa hoặc méo viền khi co giãn.
        /// </summary>
        public static Sprite GetOrCreateSlotNineSliceSprite()
        {
#if UNITY_EDITOR
            Sprite assetSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_skill_slot.png");
            if (assetSpr != null) return assetSpr;
#endif
            if (cachedNineSliceSlotSprite != null) return cachedNineSliceSlotSprite;

            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color borderOuter = new Color(0.72f, 0.58f, 0.24f, 1f);
            Color borderInner = new Color(0.22f, 0.26f, 0.34f, 1f);
            Color fillCenter  = new Color(0.11f, 0.13f, 0.18f, 0.98f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isOuter = (x == 0 || y == 0 || x == size - 1 || y == size - 1);
                    bool isInner = (x == 1 || y == 1 || x == size - 2 || y == size - 2);
                    tex.SetPixel(x, y, isOuter ? borderOuter : (isInner ? borderInner : fillCenter));
                }
            }
            tex.Apply(false, false);
            cachedNineSliceSlotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(4, 4, 4, 4));
            return cachedNineSliceSlotSprite;
        }

        public static Sprite GetOrCreatePanelNineSliceSprite()
        {
#if UNITY_EDITOR
            Sprite assetSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_hud_bar_bg.png");
            if (assetSpr != null) return assetSpr;
#endif
            if (cachedNineSlicePanelSprite != null) return cachedNineSlicePanelSprite;

            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color borderOuter = new Color(0.85f, 0.70f, 0.25f, 1f);
            Color fillCenter  = new Color(0.92f, 0.94f, 0.98f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isOuter = (x == 0 || y == 0 || x == size - 1 || y == size - 1);
                    tex.SetPixel(x, y, isOuter ? borderOuter : fillCenter);
                }
            }
            tex.Apply(false, false);
            cachedNineSlicePanelSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(4, 4, 4, 4));
            return cachedNineSlicePanelSprite;
        }
    }
}
