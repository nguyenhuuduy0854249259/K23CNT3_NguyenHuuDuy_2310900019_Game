using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ActionRPG.Visuals
{
    /// <summary>
    /// Nạp các Sprite & chuỗi khung hình hiệu ứng Pixel Art sắc nét được tạo bởi bộ công cụ codeart2d
    /// (Assets/Resources/CodeArt/*.png) với FilterMode.Point, alpha nhị phân sắc nét và tỷ lệ chuẩn 32 PPU.
    /// </summary>
    public static class CodeArtSpriteBank
    {
        private static readonly Dictionary<string, Sprite> s_spriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite[]> s_sequenceCache = new Dictionary<string, Sprite[]>();

        public static Sprite LoadSprite(string name, Vector2 pivot, float ppu = 32f)
        {
            string cacheKey = $"{name}_{pivot.x:F2}_{pivot.y:F2}_{ppu:F0}";
            if (s_spriteCache.TryGetValue(cacheKey, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Texture2D tex = LoadTexturePoint(name);
            if (tex == null) return null;

            Sprite spr = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, ppu);
            spr.name = name;
            s_spriteCache[cacheKey] = spr;
            return spr;
        }

        public static Sprite[] LoadFrameSequence(string prefix, int frameCount, Vector2 pivot, float ppu = 32f)
        {
            string cacheKey = $"{prefix}_{frameCount}_{pivot.x:F2}_{pivot.y:F2}_{ppu:F0}";
            if (s_sequenceCache.TryGetValue(cacheKey, out Sprite[] cachedSeq) && cachedSeq != null && cachedSeq.Length > 0 && cachedSeq[0] != null)
            {
                return cachedSeq;
            }

            var list = new List<Sprite>(frameCount);
            for (int i = 0; i < frameCount; i++)
            {
                string frameName = $"{prefix}-{i:D2}";
                Sprite spr = LoadSprite(frameName, pivot, ppu);
                if (spr != null) list.Add(spr);
            }

            if (list.Count == 0) return null;
            Sprite[] arr = list.ToArray();
            s_sequenceCache[cacheKey] = arr;
            return arr;
        }

        private static string ResolveFileName(string name)
        {
            switch (name)
            {
                case "vfx_frost_spike": return "mage_frost_spike";
                case "vfx_rock_spike": return "warrior_rock_spike";
                case "vfx_shield_icon": return "warrior_shield_icon";
                case "vfx_crosshair": return "archer_crosshair";
                case "vfx_snare_trap": return "archer_snare_trap";
                case "vfx_fire_orb": return "mage_fire_orb";
                case "vfx_white_orb": return "mage_white_orb";
                case "vfx_gold_orb": return "mage_gold_orb";
                default: return name;
            }
        }

        private static Texture2D LoadTexturePoint(string name)
        {
            string resolved = ResolveFileName(name);

            // 1. Ưu tiên đọc trực tiếp file PNG chuẩn từ Assets/Resources/CodeArt/ để không bị ảnh hưởng bởi Bilinear/100 PPU mặc định của Unity
            string diskPath = Path.Combine(Application.dataPath, "Resources", "CodeArt", resolved + ".png");
            if (!File.Exists(diskPath) && resolved != name)
            {
                diskPath = Path.Combine(Application.dataPath, "Resources", "CodeArt", name + ".png");
            }
            if (File.Exists(diskPath))
            {
                byte[] bytes = File.ReadAllBytes(diskPath);
                Texture2D rawTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (ImageConversion.LoadImage(rawTex, bytes))
                {
                    rawTex.name = resolved;
                    rawTex.filterMode = FilterMode.Point;
                    rawTex.wrapMode = TextureWrapMode.Clamp;
                    return rawTex;
                }
            }

            // 2. Fallback qua Resources.Load khi chạy bản Build
            Texture2D resTex = Resources.Load<Texture2D>("CodeArt/" + resolved);
            if (resTex == null && resolved != name) resTex = Resources.Load<Texture2D>("CodeArt/" + name);
            if (resTex != null)
            {
                resTex.filterMode = FilterMode.Point;
                resTex.wrapMode = TextureWrapMode.Clamp;
                return resTex;
            }

            Sprite resSpr = Resources.Load<Sprite>("CodeArt/" + resolved);
            if (resSpr == null && resolved != name) resSpr = Resources.Load<Sprite>("CodeArt/" + name);
            if (resSpr != null && resSpr.texture != null)
            {
                resSpr.texture.filterMode = FilterMode.Point;
                resSpr.texture.wrapMode = TextureWrapMode.Clamp;
                return resSpr.texture;
            }

            return null;
        }
    }
}
