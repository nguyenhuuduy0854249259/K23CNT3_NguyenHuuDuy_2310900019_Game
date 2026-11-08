// Unity Editor script to generate simple placeholder sprite sheets for character classes
// Place this file in an "Editor" folder so Unity compiles it as an editor script.
// Usage: Open the menu "Tools → Generate Class Sprite Sheet" and fill the popup.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ClassSpriteGenerator : EditorWindow
{
    // Basic settings for the sheet
    private string className = "NewClass";
    private int frameWidth = 64;
    private int frameHeight = 64;
    private int idleFrames = 4;
    private int walkFrames = 6;
    private int attackFrames = 8;
    private int castFrames = 5;
    private Color idleColor = Color.green;
    private Color walkColor = Color.yellow;
    private Color attackColor = Color.red;
    private Color castColor = Color.cyan;

    [MenuItem("Tools/Generate Class Sprite Sheet")]
    private static void ShowWindow()
    {
        var window = GetWindow<ClassSpriteGenerator>(false, "Class Sprite Sheet Generator", true);
        window.minSize = new Vector2(300, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Sheet Settings", EditorStyles.boldLabel);
        className = EditorGUILayout.TextField("Class Name", className);
        frameWidth = EditorGUILayout.IntField("Frame Width", frameWidth);
        frameHeight = EditorGUILayout.IntField("Frame Height", frameHeight);
        idleFrames = EditorGUILayout.IntField("Idle Frames", idleFrames);
        walkFrames = EditorGUILayout.IntField("Walk Frames", walkFrames);
        attackFrames = EditorGUILayout.IntField("Attack Frames", attackFrames);
        castFrames = EditorGUILayout.IntField("Cast Frames", castFrames);

        idleColor = EditorGUILayout.ColorField("Idle Color", idleColor);
        walkColor = EditorGUILayout.ColorField("Walk Color", walkColor);
        attackColor = EditorGUILayout.ColorField("Attack Color", attackColor);
        castColor = EditorGUILayout.ColorField("Cast Color", castColor);

        if (GUILayout.Button("Generate Sprite Sheet & Animation"))
        {
            GenerateSpriteSheetAndAnimations();
        }
    }

    private void GenerateSpriteSheetAndAnimations()
    {
        // Calculate total width (all frames concatenated horizontally)
        int totalFrames = idleFrames + walkFrames + attackFrames + castFrames;
        int sheetWidth = totalFrames * frameWidth;
        int sheetHeight = frameHeight;

        Texture2D tex = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false);
        // Fill background transparent
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < sheetHeight; y++)
        {
            for (int x = 0; x < sheetWidth; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Helper to draw a colored rectangle per frame
        void DrawFrames(int startIdx, int count, Color col)
        {
            for (int i = 0; i < count; i++)
            {
                int xOffset = (startIdx + i) * frameWidth;
                for (int y = 0; y < frameHeight; y++)
                {
                    for (int x = 0; x < frameWidth; x++)
                    {
                        tex.SetPixel(xOffset + x, y, col);
                    }
                }
                // Add a simple diagonal line to differentiate frames
                for (int d = 0; d < Mathf.Min(frameWidth, frameHeight); d++)
                {
                    tex.SetPixel(xOffset + d, d, Color.black);
                }
            }
        }

        int idx = 0;
        DrawFrames(idx, idleFrames, idleColor);
        idx += idleFrames;
        DrawFrames(idx, walkFrames, walkColor);
        idx += walkFrames;
        DrawFrames(idx, attackFrames, attackColor);
        idx += attackFrames;
        DrawFrames(idx, castFrames, castColor);

        tex.Apply();

        // Save texture asset
        string folderPath = "Assets/Sprites/Classes/" + className;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Sprites/Classes", className);
        }
        string texPath = folderPath + "/" + className + "_Sheet.png";
        System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        // Configure sprite importer for multiple sprites
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.spritePixelsPerUnit = frameWidth; // keep 1 unit per pixel width
        importer.SaveAndReimport();

        // Define slicing metadata
        List<SpriteMetaData> metas = new List<SpriteMetaData>();
        idx = 0;
        void AddMeta(int count, string prefix)
        {
            for (int i = 0; i < count; i++)
            {
                SpriteMetaData m = new SpriteMetaData();
                m.rect = new Rect((idx + i) * frameWidth, 0, frameWidth, frameHeight);
                m.name = $"{className}_{prefix}_{i}";
                m.pivot = new Vector2(0.5f, 0.5f);
                metas.Add(m);
            }
            idx += count;
        }
        AddMeta(idleFrames, "Idle");
        AddMeta(walkFrames, "Walk");
        AddMeta(attackFrames, "Attack");
        AddMeta(castFrames, "Cast");
        importer.spritesheet = metas.ToArray();
        importer.SaveAndReimport();

        // Generate AnimationClips
        string animFolder = folderPath + "/Animations";
        if (!AssetDatabase.IsValidFolder(animFolder))
        {
            AssetDatabase.CreateFolder(folderPath, "Animations");
        }

        // Helper to create a clip from an array of sprite names
        void CreateClip(string clipName, string prefix, int count)
        {
            AnimationClip clip = new AnimationClip();
            clip.frameRate = 12f;
            EditorCurveBinding spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(SpriteRenderer);
            spriteBinding.path = ""; // same GameObject
            spriteBinding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[count];
            for (int i = 0; i < count; i++)
            {
                string spritePath = texPath + "#" + (i + (prefix == "Idle" ? 0 :
                                                                  prefix == "Walk" ? idleFrames :
                                                                  prefix == "Attack" ? idleFrames + walkFrames :
                                                                  idleFrames + walkFrames + attackFrames));
                // The above concatenated approach works because Unity will resolve the sub‑sprite name.
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
                // Load by name
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/" + $"{className}_{prefix}_{i}");
                keyFrames[i] = new ObjectReferenceKeyframe();
                keyFrames[i].time = i / clip.frameRate;
                keyFrames[i].value = sprite;
            }
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);
            // Loop for idle & walk
            if (prefix == "Idle" || prefix == "Walk")
                clip.wrapMode = WrapMode.Loop;
            else
                clip.wrapMode = WrapMode.Once;

            string clipPath = animFolder + $"/{className}_{prefix}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        // Create each animation clip
        CreateClip(className + "_Idle", "Idle", idleFrames);
        CreateClip(className + "_Walk", "Walk", walkFrames);
        CreateClip(className + "_Attack", "Attack", attackFrames);
        CreateClip(className + "_Cast", "Cast", castFrames);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Sprite Sheet Generator", "Successfully generated sprite sheet and animation clips for " + className, "OK");
    }
}
#endif // UNITY_EDITOR
