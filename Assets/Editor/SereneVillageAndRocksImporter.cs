#pragma warning disable 0618
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lumina.EditorTools
{
    [InitializeOnLoad]
    public static class SereneVillageAndRocksImporter
    {
        private const string SESSION_KEY = "Lumina_Imported_Rocks_SereneVillage_Done";

        static SereneVillageAndRocksImporter()
        {
            // Auto run once per session or on startup when files are present
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool(SESSION_KEY, false))
                {
                    SessionState.SetBool(SESSION_KEY, true);
                    ConfigureAll();
                }
            };
        }

        [MenuItem("Tools/Lumina/Configure Imported Sprites (Rocks & Serene Village)")]
        public static void ConfigureAll()
        {
            Debug.Log("[LuminaImporter] Starting configuration of Rocks, Serene Village, VillageAndWorld, Gathering, Animals & NPCs sprites...");
            int rockCount = ConfigureRocks();
            int sereneCount = ConfigureSereneVillage();
            int pixelCount = ConfigurePixelFolders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[LuminaImporter] Complete! Configured {rockCount} Rock sprites, {sereneCount} Serene Village sheets, and {pixelCount} world/animal/NPC sprites.");
        }

        private static int ConfigurePixelFolders()
        {
            string[] folders = new[]
            {
                "Assets/Sprites/VillageAndWorld",
                "Assets/Sprites/Gathering",
                "Assets/Sprites/Animals",
                "Assets/Sprites/NPCs"
            };

            int configured = 0;
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    bool modified = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        modified = true;
                    }
                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        modified = true;
                    }
                    if (importer.filterMode != FilterMode.Point)
                    {
                        importer.filterMode = FilterMode.Point;
                        modified = true;
                    }
                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        modified = true;
                    }
                    if (importer.spritePixelsPerUnit != 16)
                    {
                        importer.spritePixelsPerUnit = 16;
                        modified = true;
                    }
                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        modified = true;
                    }

                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    if (settings.filterMode != FilterMode.Point || settings.spritePixelsPerUnit != 16 || settings.spriteMeshType != SpriteMeshType.FullRect)
                    {
                        settings.filterMode = FilterMode.Point;
                        settings.spritePixelsPerUnit = 16;
                        settings.spriteMeshType = SpriteMeshType.FullRect;
                        importer.SetTextureSettings(settings);
                        modified = true;
                    }

                    if (modified)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                        configured++;
                    }
                }
            }
            return configured;
        }

        private static int ConfigureRocks()
        {
            string folder = "Assets/Sprites/Rocks";
            if (!Directory.Exists(folder)) return 0;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            int configured = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool modified = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    modified = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    modified = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    modified = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    modified = true;
                }
                if (importer.spritePixelsPerUnit != 640)
                {
                    importer.spritePixelsPerUnit = 640;
                    modified = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    modified = true;
                }
                if (importer.maxTextureSize < 2048)
                {
                    importer.maxTextureSize = 2048;
                    modified = true;
                }

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                if (settings.filterMode != FilterMode.Point || settings.spritePixelsPerUnit != 640)
                {
                    settings.filterMode = FilterMode.Point;
                    settings.spritePixelsPerUnit = 640;
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    modified = true;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    configured++;
                }
            }
            return configured;
        }

        private static int ConfigureSereneVillage()
        {
            string folder = "Assets/Sprites/SereneVillage";
            if (!Directory.Exists(folder)) return 0;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            int configured = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase)) continue;

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                string fileName = Path.GetFileName(path);
                int cellSize = 16;
                int ppu = 16;
                int maxTexSize = 2048;
                bool isSheet = false;
                int customCols = -1;
                int customRows = -1;

                if (fileName.Contains("16x16"))
                {
                    cellSize = 16;
                    ppu = 16;
                    isSheet = true;
                }
                else if (fileName.Contains("32x32"))
                {
                    cellSize = 32;
                    ppu = 32;
                    isSheet = true;
                }
                else if (fileName.Contains("48x48"))
                {
                    cellSize = 48;
                    ppu = 48;
                    maxTexSize = 4096;
                    isSheet = true;
                }
                else if (fileName.Contains("RPG_MAKER_MV") || path.Contains("RPG_MAKER_MV"))
                {
                    cellSize = 48;
                    ppu = 48;
                    maxTexSize = 2048;
                    isSheet = true;
                }
                else if (path.Contains("RPG_MAKER_VX_ACE") || path.Contains("RPG_MAKER_XP"))
                {
                    cellSize = 32;
                    ppu = 32;
                    maxTexSize = 4096;
                    isSheet = true;
                }
                else if (path.Contains("Construct 3") || fileName.Contains("Autotiles"))
                {
                    cellSize = 16;
                    ppu = 16;
                    isSheet = true;
                }

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                    tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }

                int texW = tex != null ? tex.width : 0;
                int texH = tex != null ? tex.height : 0;

                bool modified = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    modified = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    modified = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    modified = true;
                }
                if (importer.spritePixelsPerUnit != ppu)
                {
                    importer.spritePixelsPerUnit = ppu;
                    modified = true;
                }
                if (importer.maxTextureSize < maxTexSize)
                {
                    importer.maxTextureSize = maxTexSize;
                    modified = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    modified = true;
                }

                if (isSheet && texW > 0 && texH > 0)
                {
                    int cols = (customCols > 0) ? customCols : (texW / cellSize);
                    int rows = (customRows > 0) ? customRows : (texH / cellSize);

                    if (cols > 0 && rows > 0)
                    {
                        if (importer.spriteImportMode != SpriteImportMode.Multiple)
                        {
                            importer.spriteImportMode = SpriteImportMode.Multiple;
                            modified = true;
                        }

                        int expectedSprites = cols * rows;
                        if (importer.spritesheet == null || importer.spritesheet.Length != expectedSprites)
                        {
                            List<SpriteMetaData> metas = new List<SpriteMetaData>();
                            string baseName = Path.GetFileNameWithoutExtension(path);

                            for (int r = 0; r < rows; r++)
                            {
                                float y = (rows - 1 - r) * cellSize;
                                for (int c = 0; c < cols; c++)
                                {
                                    float x = c * cellSize;
                                    SpriteMetaData smd = new SpriteMetaData
                                    {
                                        name = $"{baseName}_{r}_{c}",
                                        rect = new Rect(x, y, cellSize, cellSize),
                                        alignment = (int)SpriteAlignment.Center,
                                        pivot = new Vector2(0.5f, 0.5f)
                                    };
                                    metas.Add(smd);
                                }
                            }
                            importer.spritesheet = metas.ToArray();
                            modified = true;
                        }
                    }
                }

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                if (settings.filterMode != FilterMode.Point || settings.spritePixelsPerUnit != ppu)
                {
                    settings.filterMode = FilterMode.Point;
                    settings.spritePixelsPerUnit = ppu;
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    modified = true;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    configured++;
                }
            }
            return configured;
        }
    }
}
