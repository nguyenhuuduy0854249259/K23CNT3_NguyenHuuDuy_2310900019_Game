#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ chuyên dụng khôi phục Map 1 cũ trước đây:
    /// - Thay thế Map 1 trong MainGame bằng cấu trúc Map 1 gốc sạch đẹp (từ Forest_Demo_Map).
    /// - Dọn sạch các đối tượng rác / camera trùng lặp x7 / tile rời rạc ở root.
    /// - Bảo toàn nguyên vẹn 100% khu vực Map 2 (Map_Zone_2).
    /// </summary>
    public static class Map1Restorer
    {
        private const string MainGamePath = "Assets/Scenes/MainGame.unity";
        private const string ForestDemoPath = "Assets/Scenes/Forest_Demo_Map.unity";

        // Đã tắt tự động chạy khi mở Unity để bảo vệ tuyệt đối các chỉnh sửa trên Map 1 & Map 2 của người dùng.

        [MenuItem("Tools/Map/↩️ Khôi Phục Map 1 Gốc (Giữ Nguyên Map 2)", priority = -130)]
        public static void RestoreOriginalMap1IntoMainGame()
        {
            if (!File.Exists(MainGamePath) || !File.Exists(ForestDemoPath))
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy file scene MainGame hoặc Forest_Demo_Map!", "OK");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Xác Nhận Khôi Phục Map 1",
                "Thao tác này sẽ:\n" +
                "1. Khôi phục Map 1 trong MainGame về cấu trúc gốc sạch đẹp (từ Forest_Demo_Map).\n" +
                "2. Dọn sạch các camera trùng lặp x7 và các mảnh tile/cầu rời rạc ở root.\n" +
                "3. GIỮ NGUYÊN 100% khu vực Map 2 (Map_Zone_2).\n\n" +
                "Bạn có chắc chắn muốn thực hiện không?",
                "Khôi Phục Ngay",
                "HỦY");

            if (!confirm) return;

            PerformRestore(silent: false);
        }

        public static void PerformRestore(bool silent)
        {
            if (!File.Exists(MainGamePath) || !File.Exists(ForestDemoPath)) return;

            try
            {
                // 1. Sao lưu MainGame hiện tại trước khi khôi phục
                SceneBackupGuard.CreateBackupMenu();

                // 2. Mở MainGame làm active scene
                Scene mainScene = EditorSceneManager.OpenScene(MainGamePath, OpenSceneMode.Single);

                // 3. Mở Forest_Demo_Map dưới dạng Additive để đọc dữ liệu Map 1 gốc
                Scene demoScene = EditorSceneManager.OpenScene(ForestDemoPath, OpenSceneMode.Additive);

                try
                {
                    // Tìm Map 1 gốc trong demoScene
                    GameObject demoWorldEnv = null;
                    GameObject demoGridWorld = null;
                    GameObject demoMainCam = null;
                    GameObject demoMinimapCam = null;
                    GameObject demoBgm = null;

                    foreach (var rootGo in demoScene.GetRootGameObjects())
                    {
                        if (rootGo.name == "=== WORLD ENVIRONMENT ===") demoWorldEnv = rootGo;
                        else if (rootGo.name == "Grid_World") demoGridWorld = rootGo;
                        else if (rootGo.name == "Main Camera") demoMainCam = rootGo;
                        else if (rootGo.name == "Minimap_Camera") demoMinimapCam = rootGo;
                        else if (rootGo.name == "BGM") demoBgm = rootGo;
                    }

                    // Dọn dẹp Map 1 cũ và các vật thể rác ở root của mainScene (nhưng giữ Map_Zone_2)
                    var mainRoots = new List<GameObject>(mainScene.GetRootGameObjects());
                    foreach (var go in mainRoots)
                    {
                        if (go == null) continue;

                        // Giữ nguyên tuyệt đối Map_Zone_2, GameUIRoot, Player
                        if (go.name == "Map_Zone_2" || go.name == "GameUIRoot" || go.name.StartsWith("Player"))
                        {
                            continue;
                        }

                        // Xóa các thành phần Map 1 cũ và rác rời rạc ngoài root
                        if (go.name == "Grid_World" ||
                            go.name == "=== WORLD ENVIRONMENT ===" ||
                            go.name.StartsWith("Minimap_Camera") ||
                            go.name.StartsWith("tileset_") ||
                            go.name.StartsWith("tile_serene_water") ||
                            go.name.StartsWith("bridge_wood") ||
                            go.name.StartsWith("concepts_") ||
                            go.name.StartsWith("tree-dried") ||
                            go.name.StartsWith("trunk_0") ||
                            go.name.StartsWith("sandy_lake_shore") ||
                            go.name.StartsWith("PetSanctuary_Fence1") ||
                            go.name.StartsWith("Volcano_DarkRock_2") ||
                            go.name.StartsWith("Peak_Monument_2") ||
                            go.name.StartsWith("Border_ForestTree_23") ||
                            go.name.Contains("Bảng sprite quái vật"))
                        {
                            UnityEngine.Object.DestroyImmediate(go);
                        }
                    }

                    // Sao chép Map 1 chuẩn từ demoScene sang mainScene
                    if (demoGridWorld != null)
                    {
                        GameObject newGrid = UnityEngine.Object.Instantiate(demoGridWorld);
                        newGrid.name = "Grid_World";
                        SceneManager.MoveGameObjectToScene(newGrid, mainScene);
                    }

                    if (demoWorldEnv != null)
                    {
                        GameObject newEnv = UnityEngine.Object.Instantiate(demoWorldEnv);
                        newEnv.name = "=== WORLD ENVIRONMENT ===";
                        SceneManager.MoveGameObjectToScene(newEnv, mainScene);
                    }

                    // Đảm bảo chỉ có 1 Minimap_Camera duy nhất
                    if (GameObject.Find("Minimap_Camera") == null && demoMinimapCam != null)
                    {
                        GameObject newMiniCam = UnityEngine.Object.Instantiate(demoMinimapCam);
                        newMiniCam.name = "Minimap_Camera";
                        SceneManager.MoveGameObjectToScene(newMiniCam, mainScene);
                    }

                    EditorSceneManager.MarkSceneDirty(mainScene);
                    EditorSceneManager.SaveScene(mainScene);

                    Debug.Log("<color=green><b>[Map1Restorer] ✅ ĐÃ KHÔI PHỤC MAP 1 GỐC NGUYÊN BẢN THÀNH CÔNG! Map 2 vẫn được giữ nguyên vẹn 100%.</b></color>");
                    if (!silent)
                    {
                        EditorUtility.DisplayDialog("Hoàn Tất", "Đã khôi phục Map 1 cũ trước đây nguyên bản sạch sẽ thành công!\n\nKhu vực Map 2 vẫn được bảo toàn nguyên vẹn.", "OK");
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(demoScene, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Map1Restorer] Lỗi khi khôi phục Map 1: {ex.Message}");
            }
        }

        [MenuItem("Tools/Map/↩️ Chuyển Hẳn Về Scene Demo Gốc (Forest_Demo_Map)", priority = -129)]
        public static void OpenPureDemoScene()
        {
            if (!File.Exists(ForestDemoPath))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy Scene: {ForestDemoPath}", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ForestDemoPath, OpenSceneMode.Single);
                Debug.Log("<color=green><b>[Map1Restorer] ✅ Đã mở Scene Forest_Demo_Map gốc (Map 1 nguyên bản).</b></color>");
            }
        }
    }
}
#endif
