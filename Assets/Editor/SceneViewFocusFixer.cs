#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Bộ điều hướng Scene View và làm mới Tilemap:
    /// - Tự động nạp lại Scene sạch sẽ khi khởi động
    /// - Hỗ trợ các phím tắt nhanh chuyển đổi giữa Map 1, Map 2 và Map 3
    /// </summary>
    [InitializeOnLoad]
    public static class SceneViewFocusFixer
    {
        private const string MainScenePath = "Assets/Scenes/MainGame.unity";

        static SceneViewFocusFixer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying)
                {
                    RestoreAndFocus();
                }
            };
        }

        private static void RestoreAndFocus()
        {
            if (File.Exists(MainScenePath))
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                if (activeScene.path != MainScenePath)
                {
                    EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
                }
            }
            FocusMap1();
        }

        [MenuItem("Tools/Map/🏡 Xem Map 1 (Toàn Cảnh 6 Vùng Sinh Thái) %&1", priority = -130)]
        public static void FocusMap1()
        {
            EnsureSceneReady();

            GameObject map1 = GameObject.Find("Grid_World");
            if (map1 != null)
            {
                Selection.activeGameObject = map1;
            }

            Vector3 targetPivot = new Vector3(15f, 0f, 0f);
            float targetSize = 48f;

            ApplySceneViewCamera(targetPivot, targetSize);
            RefreshAllTilemaps();
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            Debug.Log("<color=green><b>[SceneViewFocusFixer] ✅ Đã định vị Scene View vào toàn cảnh Map 1 (160x64)!</b></color>");
        }

        [MenuItem("Tools/Map/🗺️ Xem Map 2 (Procedural Blueprint) %&2", priority = -120)]
        public static void FocusMap2()
        {
            EnsureSceneReady();

            GameObject map2 = GameObject.Find("Map_Zone_2");
            if (map2 != null)
            {
                Selection.activeGameObject = map2;
            }

            Vector3 targetPivot = new Vector3(-30.9f, 63.9f, 0f);
            float targetSize = 35f;

            ApplySceneViewCamera(targetPivot, targetSize);
            RefreshAllTilemaps();
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            Debug.Log("<color=green><b>[SceneViewFocusFixer] ✅ Đã định vị Scene View vào Map 2 cũ!</b></color>");
        }

        [MenuItem("Tools/Map/🏰 Xem Map 3 (Thung Lũng Bậc Thang) %&3", priority = -110)]
        public static void FocusMap3()
        {
            EnsureSceneReady();

            GameObject map3 = GameObject.Find("Map3_TopRight_Quarter");
            if (map3 != null)
            {
                Selection.activeGameObject = map3;
            }

            Vector3 targetPivot = new Vector3(-265.1f, 42.3f, 0f);
            float targetSize = 40f;

            ApplySceneViewCamera(targetPivot, targetSize);
            RefreshAllTilemaps();
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            Debug.Log("<color=green><b>[SceneViewFocusFixer] ✅ Đã định vị Scene View vào Map 3 (-265.1, 42.3)!</b></color>");
        }

        private static void EnsureSceneReady()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != MainScenePath && File.Exists(MainScenePath))
            {
                EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            }
        }

        private static void ApplySceneViewCamera(Vector3 pivot, float size)
        {
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv == null && SceneView.sceneViews.Count > 0)
            {
                sv = (SceneView)SceneView.sceneViews[0];
            }

            if (sv != null)
            {
                sv.in2DMode = true;
                sv.pivot = pivot;
                sv.size = size;
                sv.Repaint();
            }
        }

        private static void RefreshAllTilemaps()
        {
            var tilemaps = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (var tm in tilemaps)
            {
                if (tm != null)
                {
                    tm.RefreshAllTiles();
                }
            }
        }
    }
}
#endif
