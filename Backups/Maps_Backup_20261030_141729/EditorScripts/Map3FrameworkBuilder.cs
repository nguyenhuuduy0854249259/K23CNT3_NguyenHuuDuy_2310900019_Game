#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Visuals;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ Quản Lý & Chèn Khung Kỹ Thuật Map 3:
    /// - 100% AN TOÀN: Tuyệt đối không bao giờ dùng NewSceneMode.Single làm mất các Map cũ.
    /// - Hỗ trợ mở lại MainGame (Map 1 & Map 2) ngay lập tức.
    /// - Hỗ trợ chèn Prefab Map_Zone_3 trực tiếp vào Scene hiện tại ở vị trí độc lập (120, 80, 0),
    ///   giúp hiển thị đồng thời cả Map 1, Map 2 và Map 3 trong cùng một Scene mà không đè lên nhau.
    /// - Hỗ trợ mở Scene Map 3 riêng biệt.
    /// </summary>
    [InitializeOnLoad]
    public static class Map3FrameworkBuilder
    {
        public const string SceneMainGamePath = "Assets/Scenes/MainGame.unity";
        public const string SceneMap3Path = "Assets/Scenes/Map3.unity";
        public const string PrefabMap3Path = "Assets/Prefabs/Map3/Map_Zone_3.prefab";

        public const int Map3Width = 120;
        public const int Map3Height = 60;

        [MenuItem("Tools/Map 3/↩️ 1. MỞ LẠI MAINGAME (Khôi Phục Màn Hình Map 1 & Map 2)", priority = -250)]
        public static void OpenMainGameScene()
        {
            if (!File.Exists(SceneMainGamePath))
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy file scene MainGame.unity!", "OK");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            EditorSceneManager.OpenScene(SceneMainGamePath, OpenSceneMode.Single);
            Debug.Log("<color=green><b>[Map3FrameworkBuilder] ✅ ĐÃ MỞ LẠI MAINGAME.UNITY THÀNH CÔNG! (Toàn bộ Map 1 & Map 2 vẫn nguyên vẹn 100%)</b></color>");
        }

        [MenuItem("Tools/Map 3/📥 2. CHÈN KHUNG MAP 3 VÀO SCENE ĐANG MỞ (Giữ Nguyên Map 1 & Map 2)", priority = -249)]
        public static void InstantiateIntoCurrentScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();

            // Kiểm tra xem Map_Zone_3 đã có trong scene chưa
            GameObject existing = GameObject.Find("Map_Zone_3");
            if (existing != null)
            {
                Selection.activeGameObject = existing;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.pivot = existing.transform.position + new Vector3(Map3Width / 2f, Map3Height / 2f, 0f);
                    SceneView.lastActiveSceneView.size = 75f;
                    SceneView.lastActiveSceneView.Repaint();
                }
                EditorUtility.DisplayDialog("Thông Báo", "Khung 'Map_Zone_3' đã có sẵn trong Scene hiện tại!", "OK");
                return;
            }

            // Load Prefab đã tạo sẵn
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabMap3Path);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy prefab tại {PrefabMap3Path}!", "OK");
                return;
            }

            // Chèn Prefab vào Scene đang mở TUYỆT ĐỐI KHÔNG RELOAD SCENE
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, activeScene);
            instance.name = "Map_Zone_3";
            // Đặt tại tọa độ (120, 80, 0): cách biệt hoàn toàn khỏi Map 1 ([-65..95], [-32..32]) và Map 2 ([-60..0], [34..94])
            instance.transform.position = new Vector3(120f, 80f, 0f);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Map_Zone_3");

            EditorSceneManager.MarkSceneDirty(activeScene);

            // Focus SceneView vào Map 3
            Selection.activeGameObject = instance;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.pivot = instance.transform.position + new Vector3(Map3Width / 2f, Map3Height / 2f, 0f);
                SceneView.lastActiveSceneView.size = 75f;
                SceneView.lastActiveSceneView.Repaint();
            }

            Debug.Log("<color=green><b>[Map3FrameworkBuilder] ✅ ĐÃ CHÈN KHUNG MAP 3 (120 x 60) VÀO SCENE HIỆN TẠI TẠI TỌA ĐỘ (120, 80, 0)! Map 1 & Map 2 được bảo toàn tuyệt đối.</b></color>");

            EditorUtility.DisplayDialog(
                "Chèn Khung Map 3 Thành Công",
                "Đã chèn khung kỹ thuật 'Map_Zone_3' vào Scene hiện tại!\n\n" +
                "• Vị trí đặt: (X: 120, Y: 80, Z: 0) - Không đè lên Map 1 và Map 2.\n" +
                "• Kích thước: 120 x 60 (Chiều dài trục X gấp đôi Map 2).\n" +
                "• Toàn bộ Map 1 (Làng Oakhaven, Thảo Nguyên Xanh) và Map 2 được giữ nguyên vẹn 100%.\n" +
                "• Bạn có thể bắt đầu vẽ tile vào các Tilemap con bên trong Map_Zone_3!",
                "Bắt Đầu Vẽ");
        }

        [MenuItem("Tools/Map 3/🚀 3. MỞ SCENE MAP 3 RIÊNG BIỆT (Open Map 3 Scene)", priority = -248)]
        public static void OpenMap3Scene()
        {
            if (!File.Exists(SceneMap3Path))
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy file scene Map3.unity!", "OK");
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            EditorSceneManager.OpenScene(SceneMap3Path, OpenSceneMode.Single);

            GameObject root = GameObject.Find("Map_Zone_3");
            if (root != null)
            {
                Selection.activeGameObject = root;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.pivot = new Vector3(Map3Width / 2f, Map3Height / 2f, 0f);
                    SceneView.lastActiveSceneView.size = 75f;
                    SceneView.lastActiveSceneView.Repaint();
                }
            }

            Debug.Log("<color=green><b>[Map3FrameworkBuilder] ✅ Đã mở Scene riêng biệt Map3.unity!</b></color>");
        }
    }
}
#endif
