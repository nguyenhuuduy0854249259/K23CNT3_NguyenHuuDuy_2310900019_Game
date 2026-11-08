using UnityEngine;

namespace ActionRPG.Visuals
{
    /// <summary>
    /// Bộ hàm lõi tiện ích dùng chung (PixelArtShadingHelper) chuẩn hóa đồ họa Pixel Art Chibi 32x32 (PPU = 16):
    /// 1. Cấm tô mảng phẳng đơn sắc (FillRect): Mọi vùng màu bắt buộc vẽ bằng bảng màu 4 sắc độ:
    ///    Outline (viền tối bao quanh) -> Shadow (vùng tối) -> Base (màu gốc) -> Highlight (điểm sáng / đan caro dithering).
    /// 2. Giải phẫu Chibi 32x32 đồng bộ 100% với Player Class (player_novice_4direction):
    ///    Đầu to (Y=8..20, rộng 12px), mắt có điểm sáng trắng (#F7EFEB), 2 tay tách nhẹ 2 bên hông có khe tối 1px,
    ///    2 chân tách rõ (X=12..14 và X=17..19) và có bóng đen mờ (DropShadow) dưới bàn chân.
    /// 3. Đồng bộ lưới Pixel (No Mixels): Đạo cụ vẽ trên cùng lưới 32x32 (PPU = 16), độ dày tối thiểu 1-2px,
    ///    gắn sát vào đúng tọa độ bàn tay nhân vật.
    /// </summary>
    public static class PixelArtShadingHelper
    {
        public const int CanvasSize = 32;
        public const float StandardPixelsPerUnit = 16f;

        /// <summary>
        /// Bảng màu 4 sắc độ chuẩn Pixel Art: Outline -> Shadow -> Base -> Highlight.
        /// </summary>
        [System.Serializable]
        public struct ColorRamp4
        {
            public Color outline;
            public Color shadow;
            public Color baseColor;
            public Color highlight;

            public ColorRamp4(Color outlineColor, Color shadowColor, Color baseTone, Color highlightTone)
            {
                outline = outlineColor;
                shadow = shadowColor;
                baseColor = baseTone;
                highlight = highlightTone;
            }

            public static ColorRamp4 FromHex(string outlineHex, string shadowHex, string baseHex, string highlightHex)
            {
                ColorUtility.TryParseHtmlString(outlineHex, out Color o);
                ColorUtility.TryParseHtmlString(shadowHex, out Color s);
                ColorUtility.TryParseHtmlString(baseHex, out Color b);
                ColorUtility.TryParseHtmlString(highlightHex, out Color h);
                return new ColorRamp4(o, s, b, h);
            }
        }

        /// <summary>
        /// Khởi tạo Texture2D 32x32 chuẩn Pixel Art (FilterMode.Point, RGBA32, nền trong suốt).
        /// </summary>
        public static Texture2D CreateStandardCanvas(int width = CanvasSize, int height = CanvasSize)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] clearPixels = new Color[width * height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }

            tex.SetPixels(clearPixels);
            return tex;
        }

        /// <summary>
        /// Đặt điểm ảnh an toàn trên hệ tọa độ Top-Down (y=0 ở đỉnh đầu, y=31 ở dưới chân)
        /// chuyển đổi tự động sang hệ tọa độ Texture2D của Unity.
        /// </summary>
        public static void SetPixelTopDown(Color[] pixels, int width, int height, int x, int yTopDown, Color color)
        {
            if (x < 0 || x >= width || yTopDown < 0 || yTopDown >= height)
            {
                return;
            }

            int unityY = (height - 1) - yTopDown;
            int idx = unityY * width + x;

            if (color.a >= 0.999f || pixels[idx].a <= 0.001f)
            {
                pixels[idx] = color;
            }
            else
            {
                pixels[idx] = BlendAlpha(pixels[idx], color);
            }
        }

        /// <summary>
        /// Đọc điểm ảnh theo hệ tọa độ Top-Down (y=0 ở đỉnh đầu, y=31 ở dưới chân).
        /// </summary>
        public static Color GetPixelTopDown(Color[] pixels, int width, int height, int x, int yTopDown)
        {
            if (x < 0 || x >= width || yTopDown < 0 || yTopDown >= height)
            {
                return Color.clear;
            }

            int unityY = (height - 1) - yTopDown;
            return pixels[unityY * width + x];
        }

        /// <summary>
        /// Lấy mẫu màu từ bảng 4 sắc độ (Outline -> Shadow -> Base -> Highlight)
        /// kết hợp đan caro Dithering (x + y) % 2 == 0 giống hệt phong cách mũ/tóc của Player Class.
        /// </summary>
        public static Color Sample4ToneDithered(ColorRamp4 ramp, float lightFactor, int x, int y, bool isBorderOutline = false)
        {
            if (isBorderOutline)
            {
                return ramp.outline;
            }

            bool checker = ((x + y) & 1) == 0;

            if (lightFactor >= 0.80f)
            {
                return ramp.highlight;
            }
            if (lightFactor >= 0.58f)
            {
                return checker ? ramp.highlight : ramp.baseColor;
            }
            if (lightFactor >= 0.38f)
            {
                return ramp.baseColor;
            }
            if (lightFactor >= 0.22f)
            {
                return checker ? ramp.baseColor : ramp.shadow;
            }
            return ramp.shadow;
        }

        /// <summary>
        /// Vẽ vùng đổ bóng 4 sắc độ kèm đan caro Dithering (thay thế hoàn toàn FillRect đơn sắc).
        /// </summary>
        public static void PaintShadedRegionTopDown(
            Color[] pixels,
            int width,
            int height,
            int xMin,
            int yMin,
            int xMax,
            int yMax,
            ColorRamp4 ramp,
            bool drawOuterOutline = true)
        {
            float spanX = Mathf.Max(1f, xMax - xMin);
            float spanY = Mathf.Max(1f, yMax - yMin);

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    bool isBorder = drawOuterOutline && (x == xMin || x == xMax || y == yMin || y == yMax);
                    float nx = (x - xMin) / spanX;
                    float ny = (y - yMin) / spanY;

                    // Ánh sáng chiếu từ đỉnh-trái-trước xuống đáy-phải
                    float lightFactor = (1f - ny * 0.65f) * (1f - Mathf.Abs(nx - 0.42f) * 0.55f);
                    Color c = Sample4ToneDithered(ramp, lightFactor, x, y, isBorder);
                    SetPixelTopDown(pixels, width, height, x, y, c);
                }
            }
        }

        /// <summary>
        /// Vẽ bóng đen mờ hình elip dưới bàn chân (DropShadow) bắt buộc cho mọi nhân vật Chibi 32x32.
        /// </summary>
        public static void DrawGroundedDropShadow(
            Color[] pixels,
            int width,
            int height,
            int centerX = 15,
            int centerYTopDown = 29,
            int radiusX = 5,
            int radiusY = 1)
        {
            Color coreShadow = new Color(0.06f, 0.04f, 0.08f, 0.55f);
            Color softEdgeShadow = new Color(0.06f, 0.04f, 0.08f, 0.28f);

            for (int y = centerYTopDown - radiusY - 1; y <= centerYTopDown + radiusY + 1; y++)
            {
                for (int x = centerX - radiusX - 1; x <= centerX + radiusX + 2; x++)
                {
                    float dx = (x - (centerX + 0.5f)) / Mathf.Max(1f, radiusX);
                    float dy = (y - centerYTopDown) / Mathf.Max(1f, radiusY + 0.5f);
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= 0.85f)
                    {
                        SetPixelTopDown(pixels, width, height, x, y, coreShadow);
                    }
                    else if (distSq <= 1.25f)
                    {
                        SetPixelTopDown(pixels, width, height, x, y, softEdgeShadow);
                    }
                }
            }
        }

        /// <summary>
        /// Vẽ vòm đầu Chibi 32x32 (Y=8..17, X=10..21) với hiệu ứng đan caro Dithering (x+y)%2==0
        /// và đổ bóng từng lọn tóc theo đúng chuẩn giải phẫu của Player Class.
        /// </summary>
        public static void DrawChibiHairCrownFront(
            Color[] pixels,
            int width,
            int height,
            ColorRamp4 hairRamp,
            int bobOffset = 0)
        {
            int[] rowMinX = { 13, 12, 11, 10, 10, 10, 10, 10, 11 };
            int[] rowMaxX = { 18, 19, 20, 21, 21, 21, 21, 21, 20 };

            for (int i = 0; i < rowMinX.Length; i++)
            {
                int y = 9 + i + bobOffset;
                int xStart = rowMinX[i];
                int xEnd = rowMaxX[i];

                for (int x = xStart; x <= xEnd; x++)
                {
                    bool isOutline = (i == 0) || (x == xStart) || (x == xEnd);
                    if (isOutline)
                    {
                        SetPixelTopDown(pixels, width, height, x, y, hairRamp.outline);
                    }
                    else if (x == xStart + 1 || x == xEnd - 1 || i >= 7)
                    {
                        SetPixelTopDown(pixels, width, height, x, y, hairRamp.shadow);
                    }
                    else
                    {
                        bool checker = ((x + y) & 1) == 0;
                        Color lockTone = checker ? hairRamp.highlight : hairRamp.baseColor;
                        // Tạo rãnh chia lọn tóc tự nhiên ở x=14 và x=17
                        if ((x == 14 || x == 17) && i >= 3)
                        {
                            lockTone = hairRamp.shadow;
                        }
                        SetPixelTopDown(pixels, width, height, x, y, lockTone);
                    }
                }
            }
        }

        /// <summary>
        /// Vẽ khuôn mặt Chibi 32x32 (Y=18..20, X=12..19) có mắt đen và điểm sáng trắng (#F7EFEB) giống hệt Player Class.
        /// </summary>
        public static void DrawChibiFaceAndEyesFront(
            Color[] pixels,
            int width,
            int height,
            ColorRamp4 skinRamp,
            Color eyePupilColor,
            Color eyeWhiteGlint,
            int bobOffset = 0)
        {
            int y18 = 18 + bobOffset;
            int y19 = 19 + bobOffset;
            int y20 = 20 + bobOffset;

            // Hàng Y=18: Viền mặt + Mí mắt trên / Con ngươi tối tại X=13 và X=18
            SetPixelTopDown(pixels, width, height, 12, y18, skinRamp.outline);
            SetPixelTopDown(pixels, width, height, 13, y18, eyePupilColor);
            SetPixelTopDown(pixels, width, height, 14, y18, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 15, y18, skinRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 16, y18, skinRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 17, y18, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 18, y18, eyePupilColor);
            SetPixelTopDown(pixels, width, height, 19, y18, skinRamp.outline);

            // Hàng Y=19: Điểm sáng mắt trắng (#F7EFEB) tại X=13 và X=18 + Sống mũi đổ bóng
            SetPixelTopDown(pixels, width, height, 12, y19, skinRamp.outline);
            SetPixelTopDown(pixels, width, height, 13, y19, eyeWhiteGlint);
            SetPixelTopDown(pixels, width, height, 14, y19, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 15, y19, skinRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 16, y19, skinRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 17, y19, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 18, y19, eyeWhiteGlint);
            SetPixelTopDown(pixels, width, height, 19, y19, skinRamp.outline);

            // Hàng Y=20: Hai má Chibi sáng ấm (#FFC69E) ở giữa và đổ bóng (#B8815B) ở rìa
            SetPixelTopDown(pixels, width, height, 12, y20, skinRamp.outline);
            SetPixelTopDown(pixels, width, height, 13, y20, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 14, y20, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 15, y20, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 16, y20, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 17, y20, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 18, y20, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 19, y20, skinRamp.outline);
        }

        /// <summary>
        /// Vẽ 2 cánh tay Chibi tách nhẹ 2 bên hông (X=10..11 và X=20..21, Y=21..24)
        /// kèm đường viền khe nách tối 1px tại X=12 và X=19 (Y=23) đúng chuẩn Player Class.
        /// </summary>
        public static void DrawChibiSeparatedArmsFront(
            Color[] pixels,
            int width,
            int height,
            ColorRamp4 sleeveRamp,
            ColorRamp4 skinRamp,
            int bobOffset = 0,
            int leftArmSwingY = 0,
            int rightArmSwingY = 0)
        {
            int yL = 21 + bobOffset + leftArmSwingY;
            int yR = 21 + bobOffset + rightArmSwingY;

            // Tay bên trái khung hình (Tay phải của nhân vật: X=10..11)
            SetPixelTopDown(pixels, width, height, 11, yL, sleeveRamp.shadow);
            SetPixelTopDown(pixels, width, height, 10, yL + 1, sleeveRamp.outline);
            SetPixelTopDown(pixels, width, height, 11, yL + 1, sleeveRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 10, yL + 2, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 11, yL + 2, skinRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 12, yL + 2, sleeveRamp.outline); // Khe tối 1px tách tay khỏi thân
            SetPixelTopDown(pixels, width, height, 10, yL + 3, skinRamp.shadow);
            SetPixelTopDown(pixels, width, height, 11, yL + 3, skinRamp.highlight);

            // Tay bên phải khung hình (Tay trái của nhân vật: X=20..21)
            SetPixelTopDown(pixels, width, height, 20, yR, sleeveRamp.shadow);
            SetPixelTopDown(pixels, width, height, 20, yR + 1, sleeveRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 21, yR + 1, sleeveRamp.outline);
            SetPixelTopDown(pixels, width, height, 19, yR + 2, sleeveRamp.outline); // Khe tối 1px tách tay khỏi thân
            SetPixelTopDown(pixels, width, height, 20, yR + 2, skinRamp.baseColor);
            SetPixelTopDown(pixels, width, height, 21, yR + 2, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 20, yR + 3, skinRamp.highlight);
            SetPixelTopDown(pixels, width, height, 21, yR + 3, skinRamp.shadow);
        }

        /// <summary>
        /// Vẽ 2 bàn chân / ủng Chibi tách rõ (X=12..14 và X=17..19, Y=26..29)
        /// có khe hở 2px ở giữa (X=15..16) và đế giày viền tối giống hệt Player Class.
        /// </summary>
        public static void DrawChibiSeparatedBootsFront(
            Color[] pixels,
            int width,
            int height,
            ColorRamp4 bootRamp,
            int leftLegLiftY = 0,
            int rightLegLiftY = 0)
        {
            // Chân trái khung hình (X=12..14)
            int lTop = 26 - leftLegLiftY;
            int lBot = 29 - leftLegLiftY;
            for (int y = lTop; y <= lBot; y++)
            {
                SetPixelTopDown(pixels, width, height, 11, y, bootRamp.outline);
                for (int x = 12; x <= 14; x++)
                {
                    Color c = (y == lBot) ? bootRamp.outline : ((x == 13 && y == lTop + 1) ? bootRamp.highlight : bootRamp.baseColor);
                    SetPixelTopDown(pixels, width, height, x, y, c);
                }
                SetPixelTopDown(pixels, width, height, 15, y, bootRamp.outline);
            }

            // Chân phải khung hình (X=17..19)
            int rTop = 26 - rightLegLiftY;
            int rBot = 29 - rightLegLiftY;
            for (int y = rTop; y <= rBot; y++)
            {
                SetPixelTopDown(pixels, width, height, 16, y, bootRamp.outline);
                for (int x = 17; x <= 19; x++)
                {
                    Color c = (y == rBot) ? bootRamp.outline : ((x == 18 && y == rTop + 1) ? bootRamp.highlight : bootRamp.baseColor);
                    SetPixelTopDown(pixels, width, height, x, y, c);
                }
                SetPixelTopDown(pixels, width, height, 20, y, bootRamp.outline);
            }
        }

        /// <summary>
        /// Tạo viền tối (Dark Outline) 1 pixel tự động bao quanh mọi điểm ảnh đặc trong Texture2D.
        /// </summary>
        public static Texture2D ApplyDarkOutline(Texture2D source, Color outlineColor)
        {
            if (source == null) return null;

            int width = source.width;
            int height = source.height;
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Point;
            result.wrapMode = TextureWrapMode.Clamp;

            Color[] srcPixels = source.GetPixels();
            Color[] dstPixels = new Color[srcPixels.Length];
            System.Array.Copy(srcPixels, dstPixels, srcPixels.Length);

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    if (srcPixels[idx].a <= 0.05f)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (srcPixels[ny * width + nx].a > 0.45f)
                                {
                                    dstPixels[idx] = outlineColor;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            result.SetPixels(dstPixels);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Chuẩn hóa SpriteRenderer về chuẩn Pixel Art: Color.white, không bị ám màu.
        /// </summary>
        public static void EnforceWhiteSpriteRenderer(SpriteRenderer renderer)
        {
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
        }

        private static Color BlendAlpha(Color dst, Color src)
        {
            if (src.a <= 0.001f) return dst;
            if (dst.a <= 0.001f) return src;

            float outAlpha = src.a + dst.a * (1f - src.a);
            if (outAlpha <= 0f) return Color.clear;

            float outR = (src.r * src.a + dst.r * dst.a * (1f - src.a)) / outAlpha;
            float outG = (src.g * src.a + dst.g * dst.a * (1f - src.a)) / outAlpha;
            float outB = (src.b * src.a + dst.b * dst.a * (1f - src.a)) / outAlpha;

            return new Color(outR, outG, outB, outAlpha);
        }
    }
}
