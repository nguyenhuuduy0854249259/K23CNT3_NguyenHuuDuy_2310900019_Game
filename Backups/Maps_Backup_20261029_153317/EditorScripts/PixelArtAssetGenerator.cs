#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class PixelArtAssetGenerator
{
    private static readonly string SpritePath = "Assets/Sprites";

    [MenuItem("Tools/ActionRPG/Generate All 2D Pixel Art Sprites")]
    public static void GenerateAllAssets()
    {
        if (!Directory.Exists(SpritePath))
        {
            Directory.CreateDirectory(SpritePath);
        }

        GenerateCharacterSpriteSheet("player_novice.png", new Color(0.2f, 0.45f, 0.85f), new Color(0.5f, 0.3f, 0.15f), "dagger");
        GenerateCharacterSpriteSheet("player_archer.png", new Color(0.2f, 0.65f, 0.3f), new Color(0.7f, 0.5f, 0.2f), "bow");
        GenerateCharacterSpriteSheet("player_warrior.png", new Color(0.7f, 0.75f, 0.8f), new Color(0.9f, 0.2f, 0.2f), "sword");
        GenerateCharacterSpriteSheet("player_mage.png", new Color(0.45f, 0.2f, 0.75f), new Color(0.2f, 0.7f, 0.9f), "staff");

        GeneratePetSpriteSheet("pet_firefox.png", new Color(1f, 0.4f, 0.1f), new Color(1f, 0.85f, 0.2f));
        GeneratePetSpriteSheet("pet_waterslime.png", new Color(0.2f, 0.65f, 1f), new Color(0.8f, 0.95f, 1f));
        GeneratePetSpriteSheet("pet_windfairy.png", new Color(0.4f, 0.95f, 0.65f), new Color(1f, 1f, 1f));

        GenerateEnemySprite("enemy_slime.png", 32, 32, new Color(0.2f, 0.85f, 0.3f));
        GenerateEnemySprite("enemy_wolf.png", 32, 32, new Color(0.45f, 0.45f, 0.5f));
        GenerateEnemySprite("enemy_boss_wolfking.png", 64, 64, new Color(0.2f, 0.15f, 0.3f), true);

        GenerateVfxSprite("vfx_slash.png", new Color(0.9f, 0.95f, 1f));
        GenerateVfxSprite("vfx_fireball.png", new Color(1f, 0.45f, 0.1f));
        GenerateVfxSprite("vfx_arrow.png", new Color(0.6f, 0.4f, 0.2f));

        GenerateItemIcon("item_capture_scroll.png", new Color(0.95f, 0.85f, 0.6f), new Color(0.2f, 0.5f, 1f));
        GenerateItemIcon("item_potion_hp.png", new Color(0.95f, 0.2f, 0.2f), Color.white);
        GenerateItemIcon("item_food_meat.png", new Color(0.8f, 0.35f, 0.25f), new Color(0.95f, 0.9f, 0.8f));

        AssetDatabase.Refresh();
        ConfigureAllSprites();
        Debug.Log("<color=green>[PixelArtAssetGenerator] Đã tạo thành công toàn bộ Sprite Sheet và hoạt họa 2D!</color>");
    }

    private static void GenerateCharacterSpriteSheet(string filename, Color tunicColor, Color accentColor, string weaponType)
    {
        // 4 frames idle + 4 frames walk + 4 frames attack = 12 frames (12 * 32 = 384 x 32)
        int frameW = 32;
        int frameH = 32;
        int totalFrames = 12;
        Texture2D tex = new Texture2D(frameW * totalFrames, frameH, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color skinColor = new Color(0.98f, 0.82f, 0.68f);
        Color hairColor = new Color(0.35f, 0.22f, 0.12f);
        Color bootColor = new Color(0.25f, 0.18f, 0.12f);

        // Xóa nền trong suốt
        for (int y = 0; y < frameH; y++)
            for (int x = 0; x < frameW * totalFrames; x++)
                tex.SetPixel(x, y, transparent);

        for (int f = 0; f < totalFrames; f++)
        {
            int offsetX = f * frameW;
            int bounce = (f % 4 == 1 || f % 4 == 3) ? 1 : 0;
            if (f >= 8) bounce = 0; // Attack frame

            // 1. Tóc (Hair)
            FillRect(tex, offsetX + 11, 22 + bounce, 10, 6, hairColor);
            FillRect(tex, offsetX + 10, 20 + bounce, 12, 3, hairColor);

            // 2. Khuôn mặt & Mắt (Face & Eyes)
            FillRect(tex, offsetX + 12, 17 + bounce, 8, 5, skinColor);
            tex.SetPixel(offsetX + 14, 19 + bounce, Color.black); // Mắt trái
            tex.SetPixel(offsetX + 17, 19 + bounce, Color.black); // Mắt phải

            // 3. Áo giáp / Trang phục (Tunic / Armor)
            FillRect(tex, offsetX + 11, 9 + bounce, 10, 8, tunicColor);
            FillRect(tex, offsetX + 14, 9 + bounce, 4, 8, accentColor); // Vạt áo giữa

            // 4. Chân & Ủng (Legs & Boots)
            int legOffsetL = (f >= 4 && f < 8 && f % 2 == 0) ? 2 : 0;
            int legOffsetR = (f >= 4 && f < 8 && f % 2 != 0) ? 2 : 0;

            FillRect(tex, offsetX + 11, 2 + legOffsetL, 4, 7, bootColor);
            FillRect(tex, offsetX + 17, 2 + legOffsetR, 4, 7, bootColor);

            // 5. Vũ khí & Đòn tấn công (Weapon & Attack Swing)
            if (weaponType == "dagger")
            {
                if (f >= 8)
                {
                    // Vung dao chém tới
                    FillRect(tex, offsetX + 20, 13, 8, 3, Color.gray);
                    FillRect(tex, offsetX + 25, 14, 2, 2, Color.white);
                    DrawSlashArc(tex, offsetX + 23, 10, 8, Color.cyan);
                }
                else
                {
                    FillRect(tex, offsetX + 21, 10 + bounce, 3, 5, Color.gray);
                }
            }
            else if (weaponType == "sword")
            {
                if (f >= 8)
                {
                    // Kiếm bổ xuống kèm khiên chắn
                    FillRect(tex, offsetX + 22, 6, 4, 18, Color.white);
                    FillRect(tex, offsetX + 8, 11, 5, 8, accentColor); // Khiên bên trái
                    DrawSlashArc(tex, offsetX + 22, 12, 9, Color.white);
                }
                else
                {
                    FillRect(tex, offsetX + 21, 11 + bounce, 3, 14, Color.white);
                    FillRect(tex, offsetX + 8, 11 + bounce, 4, 7, accentColor);
                }
            }
            else if (weaponType == "bow")
            {
                if (f >= 8)
                {
                    // Giương cung bắn tên
                    FillRect(tex, offsetX + 22, 8, 2, 16, accentColor);
                    FillRect(tex, offsetX + 17, 15, 10, 2, Color.yellow); // Mũi tên
                }
                else
                {
                    FillRect(tex, offsetX + 21, 9 + bounce, 2, 14, accentColor);
                }
            }
            else if (weaponType == "staff")
            {
                // Trượng phép thuật
                FillRect(tex, offsetX + 21, 8 + bounce, 2, 16, Color.yellow);
                FillRect(tex, offsetX + 20, 23 + bounce, 4, 4, Color.cyan); // Ngọc phép phát sáng
            }
        }

        tex.Apply();
        SaveTexture(tex, filename);
    }

    private static void GeneratePetSpriteSheet(string filename, Color bodyColor, Color glowColor)
    {
        int frameW = 24;
        int frameH = 24;
        int totalFrames = 4;
        Texture2D tex = new Texture2D(frameW * totalFrames, frameH, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        for (int f = 0; f < totalFrames; f++)
        {
            int offsetX = f * frameW;
            int floatY = (f % 2 == 0) ? 1 : 0;

            // Thân Pet
            FillCircle(tex, offsetX + 12, 11 + floatY, 6, bodyColor);

            // Mắt to tròn dễ thương
            tex.SetPixel(offsetX + 10, 12 + floatY, Color.black);
            tex.SetPixel(offsetX + 14, 12 + floatY, Color.black);
            tex.SetPixel(offsetX + 11, 13 + floatY, Color.white); // Ánh sáng lấp lánh
            tex.SetPixel(offsetX + 15, 13 + floatY, Color.white);

            // Tai hoặc Cánh
            FillRect(tex, offsetX + 7, 16 + floatY + (f % 2), 3, 5, glowColor);
            FillRect(tex, offsetX + 14, 16 + floatY + (f % 2), 3, 5, glowColor);

            // Đuôi phát sáng
            FillRect(tex, offsetX + 3, 9 + floatY, 4, 4, glowColor);
        }

        tex.Apply();
        SaveTexture(tex, filename);
    }

    private static void GenerateEnemySprite(string filename, int width, int height, Color bodyColor, bool isBoss = false)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        FillCircle(tex, width / 2, height / 2, width / 3, bodyColor);

        // Mắt quái vật đỏ ngầu
        int eyeY = height / 2 + 2;
        tex.SetPixel(width / 2 - 3, eyeY, Color.red);
        tex.SetPixel(width / 2 + 3, eyeY, Color.red);

        if (isBoss)
        {
            // Vương miện / Sừng Boss
            FillRect(tex, width / 2 - 6, height - 12, 3, 8, Color.yellow);
            FillRect(tex, width / 2 + 3, height - 12, 3, 8, Color.yellow);
            FillRect(tex, width / 2 - 2, height - 10, 4, 6, Color.yellow);
        }

        tex.Apply();
        SaveTexture(tex, filename);
    }

    private static void GenerateVfxSprite(string filename, Color color)
    {
        Texture2D tex = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        DrawSlashArc(tex, 12, 12, 9, color);
        tex.Apply();
        SaveTexture(tex, filename);
    }

    private static void GenerateItemIcon(string filename, Color primary, Color secondary)
    {
        Texture2D tex = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex);

        FillRect(tex, 4, 4, 16, 16, primary);
        FillRect(tex, 8, 8, 8, 8, secondary);

        tex.Apply();
        SaveTexture(tex, filename);
    }

    private static void FillRect(Texture2D tex, int startX, int startY, int w, int h, Color col)
    {
        for (int y = startY; y < startY + h && y < tex.height; y++)
        {
            for (int x = startX; x < startX + w && x < tex.width; x++)
            {
                if (x >= 0 && y >= 0) tex.SetPixel(x, y, col);
            }
        }
    }

    private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        for (int y = cy - r; y <= cy + r && y < tex.height; y++)
        {
            for (int x = cx - r; x <= cx + r && x < tex.width; x++)
            {
                if (x >= 0 && y >= 0 && (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                {
                    tex.SetPixel(x, y, col);
                }
            }
        }
    }

    private static void DrawSlashArc(Texture2D tex, int cx, int cy, int radius, Color col)
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = (i / 12f) * Mathf.PI * 0.8f;
            int px = cx + (int)(Mathf.Cos(angle) * radius);
            int py = cy + (int)(Mathf.Sin(angle) * radius);
            if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
            {
                tex.SetPixel(px, py, col);
                tex.SetPixel(px + 1, py, col);
            }
        }
    }

    private static void ClearTexture(Texture2D tex)
    {
        Color transparent = new Color(0, 0, 0, 0);
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, transparent);
    }

    private static void SaveTexture(Texture2D tex, string filename)
    {
        byte[] bytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(SpritePath, filename);
        File.WriteAllBytes(fullPath, bytes);
    }

    private static void ConfigureAllSprites()
    {
        string[] files = Directory.GetFiles(SpritePath, "*.png");
        foreach (var file in files)
        {
            string unityPath = file.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(unityPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
