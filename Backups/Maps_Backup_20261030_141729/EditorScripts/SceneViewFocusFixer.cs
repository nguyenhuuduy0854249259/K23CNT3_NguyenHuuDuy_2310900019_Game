#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Tự động khắc phục triệt để lỗi "Không hiện gì khi không chạy (trong Edit Mode)":
    /// 1. Tự động kéo góc nhìn Scene View (Camera Editor) từ khoảng không xa xôi về ngay trung tâm Map 1 (-37, 0, 0).
    /// 2. Bắt buộc toàn bộ Tilemap gọi RefreshAllTiles() để tái tạo mesh hiển thị địa hình trong Edit Mode.
    /// 3. Phím tắt: Ctrl + Alt + F hoặc menu Tools -> Map -> 🎯 Focus Vào Map.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneViewFocusFixer
    {
        private static bool hasFocused = false;

        static SceneViewFocusFixer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!hasFocused && !Application.isPlaying)
                {
                    hasFocused = true;
                    FocusSceneViewOnMap();
                }
            };
        }

        [MenuItem("Tools/Map/🎯 Focus Vào Map (Sửa Lỗi Không Hiện Map Khi Không Chạy) %&f", priority = -120)]
        public static void FocusSceneViewOnMap()
        {
            // 1. Làm mới và dựng lại toàn bộ Tilemap trong Scene
            var tilemaps = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (var tm in tilemaps)
            {
                if (tm != null)
                {
                    tm.RefreshAllTiles();
                    EditorUtility.SetDirty(tm);
                }
            }

            // 2. Chuyển Camera Scene View về ngay vị trí trung tâm Làng Tân Thủ Map 1
            Vector3 targetPivot = new Vector3(-37f, 0f, 0f);
            float targetSize = 14f;

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.in2DMode = true;
                SceneView.lastActiveSceneView.pivot = targetPivot;
                SceneView.lastActiveSceneView.size = targetSize;
                SceneView.lastActiveSceneView.Repaint();
                Debug.Log("<color=green><b>[SceneViewFocusFixer] ✅ Đã định vị Scene View về trung tâm Map 1 (-37, 0, 0) và làm mới toàn bộ Tilemap!</b></color>");
            }
            else if (SceneView.sceneViews.Count > 0)
            {
                var sv = (SceneView)SceneView.sceneViews[0];
                sv.in2DMode = true;
                sv.pivot = targetPivot;
                sv.size = targetSize;
                sv.Repaint();
                Debug.Log("<color=green><b>[SceneViewFocusFixer] ✅ Đã định vị Scene View về trung tâm Map 1 (-37, 0, 0) và làm mới toàn bộ Tilemap!</b></color>");
            }
        }
    }
}
#endif
