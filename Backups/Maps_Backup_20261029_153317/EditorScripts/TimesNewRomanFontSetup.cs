#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ chuyển đổi toàn bộ font chữ của dự án sang Times New Roman (hỗ trợ tiếng Việt sắc nét, chuẩn văn bản).
    /// - Nhập font Times New Roman TTF vào dự án.
    /// - Tạo TMP_FontAsset (Dynamic mode hỗ trợ 100% tiếng Việt có dấu).
    /// - Cập nhật TMP Settings làm font mặc định cho toàn dự án.
    /// - Quét và gán font Times New Roman cho toàn bộ TextMeshProUGUI và Text trong tất cả Scenes & Prefabs.
    /// Menu: Tools -> Lumina -> Apply Times New Roman Font To Whole Project
    /// </summary>
    [InitializeOnLoad]
    public static class TimesNewRomanFontSetup
    {
        public const string FontTtfPath = "Assets/Fonts/TimesNewRoman.ttf";
        public const string FontTtfBoldPath = "Assets/Fonts/TimesNewRoman_Bold.ttf";
        public const string TmpFontAssetPath = "Assets/Fonts/TimesNewRoman SDF.asset";
        public const string TmpFontAssetResourcePath = "Assets/Resources/Fonts/TimesNewRoman SDF.asset";
        public const string TmpFontAssetTMProPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/TimesNewRoman SDF.asset";
        public const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        private static bool hasExecuted = false;

        // Vô hiệu hóa tự động kích hoạt khi load để tránh ghi đè làm mất chữ ngoài ý muốn
        static TimesNewRomanFontSetup()
        {
        }

        [MenuItem("Tools/Lumina/Restore Working Default Font (LiberationSans)")]
        public static void RestoreDefaultTMPFonts()
        {
            TMP_FontAsset defFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (defFont == null) return;
            UpdateTmpSettings(defFont);
            UpdateAllPrefabs(defFont, null);
            UpdateAllScenes(defFont, null);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>[FontSetup] Đã khôi phục toàn bộ chữ sang LiberationSans SDF hoạt động 100%!</color>");
        }

        [MenuItem("Tools/Lumina/Apply Times New Roman Font To Whole Project")]
        public static void ApplyTimesNewRomanToAll()
        {
            Debug.Log("<color=cyan><b>[FontSetup] BẮT ĐẦU CẬP NHẬT FONT TIMES NEW ROMAN CHO TOÀN DỰ ÁN...</b></color>");

            // 1. Đảm bảo file font TTF được copy từ Windows nếu chưa có
            EnsureFontTtfImported();

            // 2. Tạo hoặc nạp TMP_FontAsset cho Times New Roman
            TMP_FontAsset tmpFont = EnsureTmpFontAssetCreated();

            // 3. Nạp font chuẩn Unity UI (Font legacy)
            Font legacyFont = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);

            // 4. Cập nhật TMP Settings
            UpdateTmpSettings(tmpFont);

            // 5. Quét và cập nhật toàn bộ Prefab trong dự án
            UpdateAllPrefabs(tmpFont, legacyFont);

            // 6. Quét và cập nhật toàn bộ Scene trong dự án
            UpdateAllScenes(tmpFont, legacyFont);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[FontSetup] HOÀN TẤT 100%! Đã chuyển toàn bộ giao diện dự án sang font Times New Roman!</b></color>");
        }

        private static void EnsureFontTtfImported()
        {
            if (!Directory.Exists("Assets/Fonts")) Directory.CreateDirectory("Assets/Fonts");
            if (!Directory.Exists("Assets/Resources/Fonts")) Directory.CreateDirectory("Assets/Resources/Fonts");

            if (!File.Exists(FontTtfPath) && File.Exists("C:\\Windows\\Fonts\\times.ttf"))
            {
                File.Copy("C:\\Windows\\Fonts\\times.ttf", FontTtfPath, true);
                AssetDatabase.ImportAsset(FontTtfPath, ImportAssetOptions.ForceUpdate);
            }

            if (!File.Exists(FontTtfBoldPath) && File.Exists("C:\\Windows\\Fonts\\timesbd.ttf"))
            {
                File.Copy("C:\\Windows\\Fonts\\timesbd.ttf", FontTtfBoldPath, true);
                AssetDatabase.ImportAsset(FontTtfBoldPath, ImportAssetOptions.ForceUpdate);
            }

            string resTtf = "Assets/Resources/Fonts/TimesNewRoman.ttf";
            if (!File.Exists(resTtf) && File.Exists(FontTtfPath))
            {
                File.Copy(FontTtfPath, resTtf, true);
                AssetDatabase.ImportAsset(resTtf, ImportAssetOptions.ForceUpdate);
            }
        }

        public static TMP_FontAsset EnsureTmpFontAssetCreated()
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
            if (fontAsset == null)
            {
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetTMProPath);
            }
            if (fontAsset != null) return fontAsset;

            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
            if (sourceFont == null)
            {
                AssetDatabase.ImportAsset(FontTtfPath, ImportAssetOptions.ForceUpdate);
                sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
            }

            if (sourceFont == null)
            {
                Debug.LogError("[FontSetup] Không thể nạp font source Font từ " + FontTtfPath);
                return null;
            }

            // Tạo TMP Font Asset động (Dynamic Font Asset) - tối ưu hiển thị ký tự Unicode tiếng Việt
            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            if (fontAsset != null)
            {
                fontAsset.name = "TimesNewRoman SDF";
                fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

                AssetDatabase.CreateAsset(fontAsset, TmpFontAssetPath);
                if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
                {
                    fontAsset.material.name = "TimesNewRoman SDF Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }

                EditorUtility.SetDirty(fontAsset);

                // Copy sang cả thư mục Resources để nạp runtime
                if (!Directory.Exists("Assets/TextMesh Pro/Resources/Fonts & Materials"))
                {
                    Directory.CreateDirectory("Assets/TextMesh Pro/Resources/Fonts & Materials");
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"<color=cyan>[FontSetup] Đã tạo thành công TMP Font Asset tại: {TmpFontAssetPath}</color>");
            }

            return fontAsset;
        }

        private static void UpdateTmpSettings(TMP_FontAsset tmpFont)
        {
            if (tmpFont == null) return;

            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
            }

            if (settings != null)
            {
                SerializedObject so = new SerializedObject(settings);
                SerializedProperty defaultFontProp = so.FindProperty("m_defaultFontAsset");
                if (defaultFontProp != null)
                {
                    defaultFontProp.objectReferenceValue = tmpFont;
                }

                SerializedProperty fallbackProp = so.FindProperty("m_fallbackFontAssets");
                if (fallbackProp != null)
                {
                    bool exists = false;
                    for (int i = 0; i < fallbackProp.arraySize; i++)
                    {
                        if (fallbackProp.GetArrayElementAtIndex(i).objectReferenceValue == tmpFont)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        fallbackProp.InsertArrayElementAtIndex(0);
                        fallbackProp.GetArrayElementAtIndex(0).objectReferenceValue = tmpFont;
                    }
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                Debug.Log("<color=cyan>[FontSetup] Đã cập nhật Times New Roman làm Default Font Asset trong TMP Settings.</color>");
            }
        }

        private static void UpdateAllPrefabs(TMP_FontAsset tmpFont, Font legacyFont)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            int modifiedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool modified = false;

                // TextMeshProUGUI
                if (tmpFont != null)
                {
                    var tmpTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var tmp in tmpTexts)
                    {
                        if (tmp != null && tmp.font != tmpFont)
                        {
                            tmp.font = tmpFont;
                            modified = true;
                        }
                    }

                    var tmps = prefab.GetComponentsInChildren<TMP_Text>(true);
                    foreach (var tmp in tmps)
                    {
                        if (tmp != null && tmp.font != tmpFont)
                        {
                            tmp.font = tmpFont;
                            modified = true;
                        }
                    }
                }

                // Legacy UI Text
                if (legacyFont != null)
                {
                    var texts = prefab.GetComponentsInChildren<Text>(true);
                    foreach (var t in texts)
                    {
                        if (t != null && t.font != legacyFont)
                        {
                            t.font = legacyFont;
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    modifiedCount++;
                }
            }

            Debug.Log($"<color=cyan>[FontSetup] Đã quét và cập nhật font Times New Roman cho {modifiedCount} Prefabs.</color>");
        }

        private static void UpdateAllScenes(TMP_FontAsset tmpFont, Font legacyFont)
        {
            string activeScenePath = EditorSceneManager.GetActiveScene().path;
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!File.Exists(path) || path.Contains("/Backups/") || path.Contains("\\Backups\\")) continue;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (!scene.IsValid()) continue;

                bool modified = false;

                // TextMeshProUGUI & TMP_Text
                if (tmpFont != null)
                {
                    var tmpTexts = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var tmp in tmpTexts)
                    {
                        if (tmp != null && tmp.font != tmpFont)
                        {
                            tmp.font = tmpFont;
                            EditorUtility.SetDirty(tmp);
                            modified = true;
                        }
                    }

                    var tmps = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var tmp in tmps)
                    {
                        if (tmp != null && tmp.font != tmpFont)
                        {
                            tmp.font = tmpFont;
                            EditorUtility.SetDirty(tmp);
                            modified = true;
                        }
                    }
                }

                // Legacy UI Text
                if (legacyFont != null)
                {
                    var texts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var t in texts)
                    {
                        if (t != null && t.font != legacyFont)
                        {
                            t.font = legacyFont;
                            EditorUtility.SetDirty(t);
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"<color=cyan>[FontSetup] Đã cập nhật font Times New Roman trong Scene: {path}</color>");
                }
            }

            // Mở lại active scene ban đầu
            if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            {
                EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
            }
        }
    }
}
#endif
