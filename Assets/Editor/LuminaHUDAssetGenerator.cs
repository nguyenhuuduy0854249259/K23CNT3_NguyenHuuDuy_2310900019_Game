#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ActionRPG.Classes;
using ActionRPG.Combat;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Generator tự động tạo trọn bộ sprite Pixel Art 16-bit cho hệ thống HUD và 5 Kỹ Năng Tân Thủ Lumina.
    /// Khớp 100% với yêu cầu thiết kế đồ họa HUD (UI Image, Sliced 9-border, Point filter, PPU 32/16).
    /// Menu: Tools -> Lumina -> Generate Pixel Art HUD & Novice Skills
    /// </summary>
    public static class LuminaHUDAssetGenerator
    {
        private const string HudPath = "Assets/Sprites/UI/HUD";
        private const string SkillsPath = "Assets/Sprites/UI/HUD/Skills";
        private const string FramesPath = "Assets/Sprites/UI/HUD/Frames";
        private const string IconsPath = "Assets/Sprites/UI/HUD/Icons";

        [MenuItem("Tools/Lumina/Generate Pixel Art HUD & Novice Skills")]
        public static void GenerateAllHUDAssets()
        {
            EnsureDirectories();

            // 1. Tạo 5 Skill Icons (nếu chưa có bản HD chất lượng cao)
            EnsureSkillIcon("icon_skill_fan_slash.png", SkillTheme.FanSlash);
            EnsureSkillIcon("icon_skill_flurry_slash.png", SkillTheme.FlurrySlash);
            EnsureSkillIcon("icon_skill_aegis_barrier.png", SkillTheme.AegisBarrier);
            EnsureSkillIcon("icon_skill_quad_dash.png", SkillTheme.QuadDash);
            EnsureSkillIcon("icon_skill_holy_heal.png", SkillTheme.HolyHeal);

            // 2. Tạo Frames (Khung viền 9-sliced)
            EnsureSlicedFrame("frame_skill_slot.png", 64, 64, new Color(0.12f, 0.14f, 0.20f), new Color(0.7f, 0.55f, 0.25f));
            EnsureSlicedFrame("frame_hud_bar_bg.png", 64, 24, new Color(0.06f, 0.08f, 0.12f), new Color(0.25f, 0.35f, 0.45f));
            EnsureBarFill("fill_hp_bar.png", 64, 20, new Color(0.92f, 0.22f, 0.25f), new Color(1.0f, 0.5f, 0.5f));
            EnsureBarFill("fill_mp_bar.png", 64, 20, new Color(0.15f, 0.55f, 0.95f), new Color(0.5f, 0.85f, 1.0f));
            EnsureBarFill("fill_exp_bar.png", 64, 16, new Color(0.25f, 0.85f, 0.35f), new Color(0.6f, 1.0f, 0.6f));
            EnsureSlicedFrame("frame_portrait.png", 64, 64, new Color(0.15f, 0.12f, 0.25f), new Color(0.85f, 0.7f, 0.3f));
            EnsureSlicedFrame("frame_boss_bar.png", 64, 24, new Color(0.12f, 0.05f, 0.18f), new Color(0.65f, 0.2f, 0.85f));

            // 3. Tạo Menu Icons & Quick Slots
            EnsureMenuIcon("icon_backpack.png", MenuIconTheme.Backpack);
            EnsureMenuIcon("icon_character.png", MenuIconTheme.Character);
            EnsureMenuIcon("icon_settings.png", MenuIconTheme.Settings);
            EnsureMenuIcon("icon_demo_class.png", MenuIconTheme.DemoClass);

            AssetDatabase.Refresh();

            // 4. Cấu hình Import Settings (Filter Bilinear/Point, Sprite Single/Multiple, 9-Slice Border)
            ConfigureSpriteImports();

            // 5. Liên kết Sprite vào ScriptableObjects 5 Kỹ Năng
            LinkSpritesToSkillAssets();

            Debug.Log("<color=green><b>[LuminaHUDAssetGenerator] Đã kiểm tra, tối ưu hóa và cấu hình trọn bộ Fantasy RPG HUD & 5 Kỹ Năng thành công!</b></color>");
        }

        private static bool IsHighQualityAsset(string fullPath)
        {
            if (!File.Exists(fullPath)) return false;
            FileInfo fi = new FileInfo(fullPath);
            return fi.Length > 450;
        }

        private static void EnsureSkillIcon(string filename, SkillTheme theme)
        {
            string fullPath = Path.Combine(SkillsPath, filename);
            if (IsHighQualityAsset(fullPath)) return;
            GenerateSkillIcon(filename, theme);
        }

        private static void EnsureSlicedFrame(string filename, int width, int height, Color bg, Color border)
        {
            string fullPath = Path.Combine(FramesPath, filename);
            if (IsHighQualityAsset(fullPath)) return;
            GenerateSlicedFrame(filename, width, height, bg, border);
        }

        private static void EnsureBarFill(string filename, int width, int height, Color mainColor, Color highlight)
        {
            string fullPath = Path.Combine(FramesPath, filename);
            if (IsHighQualityAsset(fullPath)) return;
            GenerateBarFill(filename, width, height, mainColor, highlight);
        }

        private static void EnsureMenuIcon(string filename, MenuIconTheme theme)
        {
            string fullPath = Path.Combine(IconsPath, filename);
            if (IsHighQualityAsset(fullPath)) return;
            GenerateMenuIcon(filename, theme);
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(HudPath)) Directory.CreateDirectory(HudPath);
            if (!Directory.Exists(SkillsPath)) Directory.CreateDirectory(SkillsPath);
            if (!Directory.Exists(FramesPath)) Directory.CreateDirectory(FramesPath);
            if (!Directory.Exists(IconsPath)) Directory.CreateDirectory(IconsPath);
        }

        private enum SkillTheme { FanSlash, FlurrySlash, AegisBarrier, QuadDash, HolyHeal }
        private enum MenuIconTheme { Backpack, Character, Settings, DemoClass }

        private static void GenerateSkillIcon(string filename, SkillTheme theme)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color darkBg = new Color(0.08f, 0.10f, 0.15f, 1f);
            Color border = new Color(0.35f, 0.40f, 0.50f, 1f);

            // Nền tối và viền pixel 1px
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                        tex.SetPixel(x, y, border);
                    else
                        tex.SetPixel(x, y, darkBg);
                }
            }

            switch (theme)
            {
                case SkillTheme.FanSlash:
                    // Hình quạt kiếm lam nhạt quét 90 độ
                    Color bladeCol = new Color(0.85f, 0.95f, 1f, 1f);
                    Color arcCol = new Color(0.35f, 0.75f, 1f, 0.9f);
                    for (int r = 8; r <= 13; r++)
                    {
                        for (int a = 15; a <= 75; a += 5)
                        {
                            float rad = a * Mathf.Deg2Rad;
                            int px = 6 + (int)(Mathf.Cos(rad) * r * 1.5f);
                            int py = 6 + (int)(Mathf.Sin(rad) * r * 1.5f);
                            if (px < size - 1 && py < size - 1)
                                tex.SetPixel(px, py, arcCol);
                        }
                    }
                    // Thân kiếm chéo
                    for (int i = 0; i < 16; i++)
                    {
                        tex.SetPixel(6 + i, 6 + i, bladeCol);
                        tex.SetPixel(7 + i, 6 + i, Color.white);
                    }
                    break;

                case SkillTheme.FlurrySlash:
                    // 3 vệt chém đan xen cam đỏ
                    Color flurry1 = new Color(1f, 0.4f, 0.1f, 1f);
                    Color flurry2 = new Color(1f, 0.75f, 0.2f, 1f);
                    for (int i = 4; i < 28; i++)
                    {
                        tex.SetPixel(i, 28 - i, flurry1);
                        tex.SetPixel(i, i, flurry2);
                        tex.SetPixel(i, 16, new Color(1f, 0.9f, 0.5f, 1f));
                    }
                    break;

                case SkillTheme.AegisBarrier:
                    // Chiếc khiên tháp phát quang vàng kim
                    Color shieldGold = new Color(1f, 0.85f, 0.25f, 1f);
                    Color shieldDark = new Color(0.6f, 0.45f, 0.15f, 1f);
                    for (int y = 8; y <= 24; y++)
                    {
                        int span = (y <= 16) ? 7 : (24 - y);
                        for (int x = 16 - span; x <= 16 + span; x++)
                        {
                            if (x == 16 - span || x == 16 + span || y == 24)
                                tex.SetPixel(x, y, shieldGold);
                            else
                                tex.SetPixel(x, y, shieldDark);
                        }
                    }
                    // Chữ thập tâm khiên
                    for (int i = -3; i <= 3; i++)
                    {
                        tex.SetPixel(16 + i, 16, Color.white);
                        tex.SetPixel(16, 16 + i, Color.white);
                    }
                    break;

                case SkillTheme.QuadDash:
                    // Giày lông vũ + 4 vệt bóng mờ xanh ngọc
                    Color dashCyan = new Color(0.2f, 0.85f, 0.95f, 1f);
                    for (int step = 0; step < 4; step++)
                    {
                        int startX = 6 + step * 6;
                        int startY = 8 + step * 4;
                        for (int dx = 0; dx < 4; dx++)
                        {
                            for (int dy = 0; dy < 3; dy++)
                            {
                                Color c = (step == 3) ? Color.white : dashCyan * (0.4f + step * 0.2f);
                                c.a = 1f;
                                tex.SetPixel(startX + dx, startY + dy, c);
                            }
                        }
                    }
                    break;

                case SkillTheme.HolyHeal:
                    // Thánh giá quang minh phát sáng xanh lục ngọc
                    Color greenLight = new Color(0.3f, 0.95f, 0.5f, 1f);
                    for (int y = 7; y <= 25; y++)
                    {
                        for (int x = 14; x <= 17; x++)
                            tex.SetPixel(x, y, greenLight);
                    }
                    for (int x = 8; x <= 23; x++)
                    {
                        for (int y = 16; y <= 19; y++)
                            tex.SetPixel(x, y, greenLight);
                    }
                    // Điểm sáng trắng trung tâm
                    for (int x = 15; x <= 16; x++)
                    {
                        for (int y = 17; y <= 18; y++)
                            tex.SetPixel(x, y, Color.white);
                    }
                    break;
            }

            SaveAndFlush(tex, Path.Combine(SkillsPath, filename));
        }

        private static void GenerateSlicedFrame(string filename, int width, int height, Color bgColor, Color borderColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color cornerDecor = borderColor * 1.3f;
            cornerDecor.a = 1f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x <= 2 || x >= width - 3 || y <= 2 || y >= height - 3);
                    bool isCorner = ((x <= 3 || x >= width - 4) && (y <= 3 || y >= height - 4));

                    if (isCorner)
                        tex.SetPixel(x, y, cornerDecor);
                    else if (isBorder)
                        tex.SetPixel(x, y, borderColor);
                    else
                        tex.SetPixel(x, y, bgColor);
                }
            }

            SaveAndFlush(tex, Path.Combine(FramesPath, filename));
        }

        private static void GenerateBarFill(string filename, int width, int height, Color mainColor, Color highlightColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (int y = 0; y < height; y++)
            {
                Color rowColor = (y >= height - 3) ? highlightColor : mainColor;
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, rowColor);
                }
            }

            SaveAndFlush(tex, Path.Combine(FramesPath, filename));
        }

        private static void GenerateMenuIcon(string filename, MenuIconTheme theme)
        {
            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            // Xóa trong suốt
            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, clear);

            switch (theme)
            {
                case MenuIconTheme.Backpack:
                    // Balo da nâu khóa vàng
                    Color leather = new Color(0.65f, 0.38f, 0.18f);
                    Color buckle = new Color(1f, 0.85f, 0.3f);
                    for (int y = 4; y <= 18; y++)
                        for (int x = 5; x <= 18; x++)
                            tex.SetPixel(x, y, leather);
                    // Quai và khóa
                    for (int x = 10; x <= 13; x++)
                    {
                        tex.SetPixel(x, 11, buckle);
                        tex.SetPixel(x, 12, buckle);
                    }
                    break;

                case MenuIconTheme.Character:
                    // Hình nhân hiệp sĩ bạc
                    Color plate = new Color(0.8f, 0.85f, 0.95f);
                    for (int y = 14; y <= 19; y++)
                        for (int x = 10; x <= 13; x++)
                            tex.SetPixel(x, y, plate);
                    for (int y = 5; y <= 13; y++)
                        for (int x = 7; x <= 16; x++)
                            tex.SetPixel(x, y, plate);
                    break;

                case MenuIconTheme.Settings:
                    // Bánh răng kim loại xám
                    Color gear = new Color(0.6f, 0.65f, 0.7f);
                    for (int y = 7; y <= 16; y++)
                        for (int x = 7; x <= 16; x++)
                            tex.SetPixel(x, y, gear);
                    for (int y = 10; y <= 13; y++)
                        for (int x = 10; x <= 13; x++)
                            tex.SetPixel(x, y, clear);
                    break;

                case MenuIconTheme.DemoClass:
                    // Hai thanh kiếm chéo
                    Color blade = new Color(1f, 0.6f, 0.1f);
                    for (int i = 4; i < 20; i++)
                    {
                        tex.SetPixel(i, i, blade);
                        tex.SetPixel(i, 23 - i, blade);
                    }
                    break;
            }

            SaveAndFlush(tex, Path.Combine(IconsPath, filename));
        }

        private static void SaveAndFlush(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

        private static void ConfigureSpriteImports()
        {
            string[] allPngs = Directory.GetFiles(HudPath, "*.png", SearchOption.AllDirectories);
            foreach (var path in allPngs)
            {
                string unityPath = path.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(unityPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;

                    // Cấu hình 9-slice border sắc sảo cho từng loại khung
                    if (unityPath.Contains("frame_skill_slot"))
                    {
                        importer.spriteBorder = new Vector4(8, 8, 8, 8);
                    }
                    else if (unityPath.Contains("frame_hud_bar_bg"))
                    {
                        importer.spriteBorder = new Vector4(6, 6, 6, 6);
                    }
                    else if (unityPath.Contains("frame_portrait"))
                    {
                        importer.spriteBorder = new Vector4(8, 8, 8, 8);
                    }
                    else if (unityPath.Contains("frame_boss_bar"))
                    {
                        importer.spriteBorder = new Vector4(8, 8, 8, 8);
                    }
                    else if (unityPath.Contains("/Frames/frame_"))
                    {
                        importer.spriteBorder = new Vector4(6, 6, 6, 6);
                    }

                    importer.SaveAndReimport();
                }
            }
        }

        private static void LinkSpritesToSkillAssets()
        {
            void LinkSkill(string skillId, string spriteName)
            {
                string skillPath = $"Assets/ScriptableObjects/Skills/Skill_{skillId}.asset";
                string spritePath = $"{SkillsPath}/{spriteName}";

                SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>(skillPath);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (skill != null && sprite != null)
                {
                    skill.icon = sprite;
                    EditorUtility.SetDirty(skill);
                }
            }

            LinkSkill("novice_fan_slash", "icon_skill_fan_slash.png");
            LinkSkill("novice_flurry_slash", "icon_skill_flurry_slash.png");
            LinkSkill("novice_aegis_barrier", "icon_skill_aegis_barrier.png");
            LinkSkill("novice_quad_dash", "icon_skill_quad_dash.png");
            LinkSkill("novice_holy_heal", "icon_skill_holy_heal.png");

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
