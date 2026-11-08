#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Bộ công cụ hỗ trợ thiết kế Map 3 chỉ bằng bộ asset map_doi_thuong (Thảm cỏ xanh, Cây cối, Nhà cửa, Đá, Hoa cỏ).
    /// </summary>
    public static class Map3DoiThuongHelper
    {
        private const string PaletteFolderPath = "Assets/Prefabs/Map3/DoiThuong_Palette";
        private const string GrassTilePath = "Assets/Sprites/map_doi_thuong/exterior_157.asset";

        [MenuItem("Tools/Map 3 (Đời Thường)/🎨 1. Mở Bộ 30 Model Đời Thường Để Kéo Thả (DoiThuong_Palette)", priority = 10)]
        public static void SelectDoiThuongPaletteFolder()
        {
            Object folderObj = AssetDatabase.LoadAssetAtPath<Object>(PaletteFolderPath);
            if (folderObj != null)
            {
                Selection.activeObject = folderObj;
                EditorGUIUtility.PingObject(folderObj);
                Debug.Log("<color=green><b>[Map3 Đời Thường] ✅ Đã mở thư mục Assets/Prefabs/Map3/DoiThuong_Palette! Bạn có thể kéo thả trực tiếp Cây, Nhà, Đá, Đường đá, Bụi cỏ vào Scene.</b></color>");
            }
            else
            {
                Debug.LogWarning($"[Map3 Đời Thường] Không tìm thấy thư mục tại: {PaletteFolderPath}");
            }
        }

        [MenuItem("Tools/Map 3 (Đời Thường)/🗺️ 2. Xem Toàn Cảnh Map 3 Hoàn Chỉnh (120 x 60)", priority = 20)]
        public static void FocusFullMap3()
        {
            FocusMap3Region("Map_Zone_3", new Vector3(60f, 30f, 0f), 42f, "Toàn Cảnh Map 3 (4 Góc)");
        }

        [MenuItem("Tools/Map 3 (Đời Thường)/🔍 3. Góc Trên - Bên Phải (Map3_TopRight_Quarter)", priority = 21)]
        public static void FocusTopRightQuarter()
        {
            FocusMap3Region("Map3_TopRight_Quarter", new Vector3(90f, 45f, 0f), 22f, "1/4 Góc Trên - Bên Phải");
        }

        [MenuItem("Tools/Map 3 (Đời Thường)/🔍 4. Góc Trên - Bên Trái: Khu Làng Cổ (Map3_TopLeft_Quarter)", priority = 22)]
        public static void FocusTopLeftQuarter()
        {
            FocusMap3Region("Map3_TopLeft_Quarter", new Vector3(30f, 45f, 0f), 22f, "1/4 Góc Trên - Bên Trái (Khu Làng Cổ)");
        }

        [MenuItem("Tools/Map 3 (Đời Thường)/🔍 5. Góc Dưới - Bên Trái: Cổng Dịch Chuyển & Đường Đá (Map3_BottomLeft_Quarter)", priority = 23)]
        public static void FocusBottomLeftQuarter()
        {
            FocusMap3Region("Map3_BottomLeft_Quarter", new Vector3(30f, 15f, 0f), 22f, "1/4 Góc Dưới - Bên Trái (Cổng Dịch Chuyển)");
        }

        [MenuItem("Tools/Map 3 (Đời Thường)/🔍 6. Góc Dưới - Bên Phải: Rừng Sâu & Đường Mòn Đất (Map3_BottomRight_Quarter)", priority = 24)]
        public static void FocusBottomRightQuarter()
        {
            FocusMap3Region("Map3_BottomRight_Quarter", new Vector3(90f, 15f, 0f), 22f, "1/4 Góc Dưới - Bên Phải (Rừng Sâu)");
        }

        private static void FocusMap3Region(string goName, Vector3 localOffset, float orthoSize, string label)
        {
            GameObject target = GameObject.Find(goName);
            if (target != null)
            {
                Selection.activeGameObject = target;
                if (SceneView.lastActiveSceneView != null)
                {
                    Vector3 centerWorld = target.transform.position + localOffset;
                    SceneView.lastActiveSceneView.LookAt(centerWorld, Quaternion.identity, orthoSize);
                    SceneView.lastActiveSceneView.Repaint();
                }
                Debug.Log($"<color=cyan><b>[Map3 Đời Thường] 🔍 Đã đưa Camera Scene View tới {label}!</b></color>");
            }
            else
            {
                Debug.LogWarning($"[Map3 Đời Thường] Không tìm thấy đối tượng {goName} trong Scene hiện tại.");
            }
        }

        [MenuItem("Tools/Map 3 (Đời Thường)/🌿 7. Phủ Thảm Cỏ Xanh Toàn Bộ Map 3 (120 x 60)", priority = 30)]
        public static void FillGrassCarpetMap3()
        {
            GameObject map3 = GameObject.Find("Map_Zone_3");
            if (map3 == null)
            {
                EditorUtility.DisplayDialog("Map 3", "Không tìm thấy Map_Zone_3 trong Scene hiện tại!", "OK");
                return;
            }

            Transform groundTr = map3.transform.Find("Tilemap_Ground");
            if (groundTr == null)
            {
                EditorUtility.DisplayDialog("Map 3", "Không tìm thấy Tilemap_Ground bên trong Map_Zone_3!", "OK");
                return;
            }

            Tilemap groundTilemap = groundTr.GetComponent<Tilemap>();
            TileBase grassTile = AssetDatabase.LoadAssetAtPath<TileBase>(GrassTilePath);
            if (groundTilemap == null || grassTile == null)
            {
                EditorUtility.DisplayDialog("Map 3", $"Không thể tải Tile cỏ xanh tại: {GrassTilePath}", "OK");
                return;
            }

            Undo.RecordObject(groundTilemap, "Fill Map 3 Green Grass Carpet");
            for (int y = 0; y < 60; y++)
            {
                for (int x = 0; x < 120; x++)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), grassTile);
                }
            }
            groundTilemap.RefreshAllTiles();
            EditorUtility.SetDirty(groundTilemap);
            Debug.Log("<color=green><b>[Map3 Đời Thường] 🌿 Đã phủ thảm cỏ xanh (exterior_157) kín toàn bộ diện tích 120 x 60 của Map 3!</b></color>");
        }
    }
}
#endif
