using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using ActionRPG.Enemies;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Core.World.Interactables;
using ActionRPG.Environment;
using ActionRPG.Items.Loot;
using ActionRPG.Presentation.Views;
using ActionRPG.Core.Bridges;

public class SetupForestPrototypeScene : EditorWindow
{
    [MenuItem("Tools/Lumina/Build Forest Prototype Scene")]
    public static void BuildScene()
    {
        Debug.Log("Starting Forest Prototype Setup...");
        
        string sourcePath = "Assets/tiny-RPG-forest-files/tiny-RPG-forest-files/Assets";
        string spriteDestPath = "Assets/Sprites/ForestPack";
        string audioDestPath = "Assets/Audio/ForestPack";
        string prefabDestPath = "Assets/Prefabs/ForestPrototype";
        
        if (!Directory.Exists(spriteDestPath)) Directory.CreateDirectory(spriteDestPath);
        if (!Directory.Exists(audioDestPath)) Directory.CreateDirectory(audioDestPath);
        if (!Directory.Exists(prefabDestPath)) Directory.CreateDirectory(prefabDestPath);
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

        AssetDatabase.Refresh();

        // 1. Copy & Configure Assets
        CopyAndConfigureSprites(sourcePath, spriteDestPath);
        CopyAudio(sourcePath, audioDestPath);

        // 2. Create Prefabs
        CreatePrefabs(spriteDestPath, prefabDestPath);

        // 3. Build Scene
        BuildPrototypeScene(prefabDestPath, spriteDestPath, audioDestPath);

        Debug.Log("Forest Prototype Setup Completed Successfully!");
    }

    static void CopyAndConfigureSprites(string source, string dest)
    {
        if (!Directory.Exists(source)) return;

        string[] pngFiles = Directory.GetFiles(source, "*.png", SearchOption.AllDirectories);
        foreach (string file in pngFiles)
        {
            string fileName = Path.GetFileName(file);
            // Reconstruct a simplified subpath
            string relative = file.Replace("\\", "/").Replace(source.Replace("\\", "/"), "");
            
            // To flatten it a bit but keep some structure:
            string destFile = dest + "/" + fileName;
            
            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile);
            }
        }
        AssetDatabase.Refresh();

        string[] copiedFiles = Directory.GetFiles(dest, "*.png", SearchOption.AllDirectories);
        foreach (string file in copiedFiles)
        {
            string assetPath = file.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
                if (importer.spritePixelsPerUnit != 16) { importer.spritePixelsPerUnit = 16; changed = true; }
                if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
                
                // Slice condition
                bool shouldSlice = assetPath.Contains("idle") || assetPath.Contains("walk") || assetPath.Contains("attack") || 
                                   assetPath.Contains("mole") || assetPath.Contains("treant") || assetPath.Contains("coin") || 
                                   assetPath.Contains("gem") || assetPath.Contains("enemy-death") || assetPath.Contains("waterfall");
                
                if (shouldSlice && importer.spriteImportMode != SpriteImportMode.Multiple)
                {
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    changed = true;
                }
                
                if (changed)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }

                if (shouldSlice)
                {
                    AutoSliceSprite(assetPath, importer);
                }
            }
        }
    }

    static void AutoSliceSprite(string path, TextureImporter importer)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;
        
        Rect[] rects = UnityEditorInternal.InternalSpriteUtility.GenerateAutomaticSpriteRectangles(tex, 0, 0);
        if (rects != null && rects.Length > 0)
        {
            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            for (int i = 0; i < rects.Length; i++)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.rect = rects[i];
                smd.alignment = 0;
                smd.name = tex.name + "_" + i;
                metas.Add(smd);
            }
            importer.spritesheet = metas.ToArray();
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }

    static void CopyAudio(string source, string dest)
    {
        if (!Directory.Exists(source)) return;
        string[] oggFiles = Directory.GetFiles(source, "*.ogg", SearchOption.AllDirectories);
        foreach (string file in oggFiles)
        {
            string fileName = Path.GetFileName(file);
            string destFile = dest + "/" + fileName;
            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile);
            }
        }
        AssetDatabase.Refresh();
    }

    static void CreatePrefabs(string spriteDest, string prefabDest)
    {
        CreatePrefab(prefabDest, "Player_Novice_Tier1", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 10;
            go.AddComponent<Rigidbody2D>().gravityScale = 0;
            var col = go.AddComponent<CapsuleCollider2D>();
            go.AddComponent<CharacterVisualView>();
            go.AddComponent<WeaponSocketView>();
            go.AddComponent<PlayerPresenter>();
            go.AddComponent<PlayerController>();
            SetSprite(go, spriteDest + "/hero-idle-front.png");
        });

        CreatePrefab(prefabDest, "Enemy_Mole", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 10;
            go.AddComponent<Rigidbody2D>().gravityScale = 0;
            var col = go.AddComponent<CircleCollider2D>();
            var stats = go.AddComponent<EnemyStats>();
            stats.level = 3;
            stats.enemyName = "Mole";
            go.AddComponent<EnemyController>();
            SetSprite(go, spriteDest + "/mole-idle-front.png");
        });

        CreatePrefab(prefabDest, "Enemy_Treant", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 11;
            go.AddComponent<Rigidbody2D>().gravityScale = 0;
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Hurtbox2D>();
            var stats = go.AddComponent<EnemyStats>();
            stats.level = 18;
            stats.enemyName = "Treant Boss";
            go.AddComponent<EnemyController>();
            go.AddComponent<TelegraphIndicator>();
            SetSprite(go, spriteDest + "/treant-idle-front.png");
        });

        CreatePrefab(prefabDest, "GatheringNode_Bush", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 8;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            go.AddComponent<GatheringNode>();
            SetSprite(go, spriteDest + "/bush.png");
        });

        CreatePrefab(prefabDest, "Waystone_Monument", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 20;
            var col = go.AddComponent<BoxCollider2D>();
            go.AddComponent<Waystone>();
            SetSprite(go, spriteDest + "/rock-monument.png");
        });

        CreatePrefab(prefabDest, "Item_Coin", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 5;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            go.AddComponent<WorldItem>();
            SetSprite(go, spriteDest + "/coin.png");
        });

        CreatePrefab(prefabDest, "VFX_EnemyDeath", go => {
            go.AddComponent<SpriteRenderer>().sortingOrder = 50;
            SetSprite(go, spriteDest + "/enemy-death.png");
            go.AddComponent<ActionRPG.Core.DestroyAfterTime>().timeToDestroy = 1f;
        });
    }

    static void CreatePrefab(string folder, string name, System.Action<GameObject> setup)
    {
        string path = folder + "/" + name + ".prefab";
        GameObject go = new GameObject(name);
        setup(go);
        PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
    }

    static void SetSprite(GameObject go, string spritePath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        Sprite firstSprite = null;
        if (assets != null)
        {
            foreach (var a in assets)
            {
                if (a is Sprite sp)
                {
                    firstSprite = sp;
                    break;
                }
            }
        }

        if (firstSprite != null)
        {
            go.GetComponent<SpriteRenderer>().sprite = firstSprite;
        }
        else
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (s != null) go.GetComponent<SpriteRenderer>().sprite = s;
        }
    }

    static void BuildPrototypeScene(string prefabDest, string spriteDest, string audioDest)
    {
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        GameObject environment = new GameObject("Environment");

        // Khu 1: Village
        Spawn(prefabDest + "/Waystone_Monument.prefab", new Vector3(-15, 0, 0), environment.transform);
        Spawn(prefabDest + "/Player_Novice_Tier1.prefab", new Vector3(-10, 0, 0), null);
        
        // Khu 2: Field
        Spawn(prefabDest + "/GatheringNode_Bush.prefab", new Vector3(5, 2, 0), environment.transform);
        Spawn(prefabDest + "/GatheringNode_Bush.prefab", new Vector3(10, -2, 0), environment.transform);
        Spawn(prefabDest + "/Enemy_Mole.prefab", new Vector3(8, 3, 0), null);
        Spawn(prefabDest + "/Enemy_Mole.prefab", new Vector3(12, -3, 0), null);

        // Khu 3: Forest
        Spawn(prefabDest + "/Enemy_Treant.prefab", new Vector3(30, 0, 0), null);
        
        // Decor
        GameObject decor = new GameObject("Decor");
        decor.transform.SetParent(environment.transform);
        SpawnDecor(spriteDest + "/tree-orange.png", new Vector3(25, 5, 0), decor.transform);
        SpawnDecor(spriteDest + "/tree-pink.png", new Vector3(28, -5, 0), decor.transform);
        SpawnDecor(spriteDest + "/sign.png", new Vector3(-5, 2, 0), decor.transform);

        // Audio
        GameObject bgm = new GameObject("BGM");
        AudioSource src = bgm.AddComponent<AudioSource>();
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioDest + "/ancient_path.ogg");
        if (clip != null)
        {
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = true;
        }

        // Camera
        Camera cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.transform.position = new Vector3(-10, 0, -10);

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/Forest_Prototype.unity");
    }

    static GameObject Spawn(string prefabPath, Vector3 pos, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = pos;
            if (parent != null) go.transform.SetParent(parent);
            return go;
        }
        return null;
    }
    
    static void SpawnDecor(string spritePath, Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("Decor");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20;
        SetSprite(go, spritePath);
        go.transform.position = pos;
        go.transform.SetParent(parent);
    }
}
