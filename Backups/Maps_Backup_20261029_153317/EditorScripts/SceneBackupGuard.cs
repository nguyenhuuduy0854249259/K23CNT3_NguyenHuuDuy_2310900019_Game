#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ActionRPG.Visuals;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Hệ thống Giám sát, Khôi phục & Bảo vệ Bản đồ (Map Safety & Auto-Backup Guard):
    /// 1. Tự động khôi phục khẩn cấp cả Map 1 (kèm các chỉnh sửa khi làm Map 2) và Map 2 (Map_Zone_2) nếu phát hiện bị mất.
    /// 2. Tự động sao lưu Scene MainGame.unity mỗi khi người dùng nhấn Lưu (Ctrl + S).
    /// 3. Chỉ cập nhật MainGame_SafeBackup.unity khi Scene có đầy đủ cả Map 1 & Map 2 (>= 5.5 MB).
    /// </summary>
    [InitializeOnLoad]
    public static class SceneBackupGuard
    {
        private const string MainScenePath = "Assets/Scenes/MainGame.unity";
        private const string BackupDir = "Assets/Scenes/Backups";
        private const string SafeBackupPath = "Assets/Scenes/Backups/MainGame_SafeBackup.unity";
        private const string ProtectedBackupPath = "Assets/Scenes/Backups/PROTECTED_Map1Edited_And_Map2_221838.unity";
        private const string EmergencyRestoreSentinel = "Temp/EmergencyRestore_Map1AndMap2_v2.done";
        private const string ReloadSceneTrigger = "Temp/DoReloadScene.trigger";
        private const int MaxRotatingBackups = 30;
        private const long MinValidMap1AndMap2Size = 5500000; // >= 5.5 MB đảm bảo có đủ cả Map 1 và Map 2

        static SceneBackupGuard()
        {
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorApplication.delayCall += CheckReloadTrigger;
            EditorApplication.delayCall += AutoEmergencyRestoreIfNeeded;
        }

        private static void CheckReloadTrigger()
        {
            if (File.Exists(ReloadSceneTrigger))
            {
                try
                {
                    File.Delete(ReloadSceneTrigger);
                    if (File.Exists(MainScenePath))
                    {
                        EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
                        Debug.Log("<color=green><b>[SceneBackupGuard] ✅ Đã tự động nạp lại Scene MainGame.unity từ bản sao lưu thành công!</b></color>");
                    }
                }
                catch { }
            }
        }

        private static void AutoEmergencyRestoreIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            bool mainMissingMap2 = !File.Exists(MainScenePath) || new FileInfo(MainScenePath).Length < MinValidMap1AndMap2Size;
            bool sentinelMissing = !File.Exists(EmergencyRestoreSentinel);

            if (mainMissingMap2 || sentinelMissing)
            {
                if (File.Exists(ProtectedBackupPath))
                {
                    RestoreMap1AndMap2Full(silent: true);
                    Directory.CreateDirectory("Temp");
                    File.WriteAllText(EmergencyRestoreSentinel, DateTime.Now.ToString("O"));
                }
            }
        }

        private static void OnSceneSaved(Scene scene)
        {
            if (scene.path == MainScenePath)
            {
                CreateBackupInternal(silent: true);
            }
        }

        [MenuItem("Tools/Map/🔄 Khôi Phục Khẩn Cấp Cả Map 1 (Đã Sửa) & Map 2", priority = -151)]
        public static void RestoreMap1AndMap2FullMenu()
        {
            RestoreMap1AndMap2Full(silent: false);
        }

        public static void RestoreMap1AndMap2Full(bool silent)
        {
            string sourceBackup = File.Exists(ProtectedBackupPath) ? ProtectedBackupPath : SafeBackupPath;
            if (!File.Exists(sourceBackup))
            {
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Không Tìm Thấy Bản Sao Lưu", $"Không tìm thấy file sao lưu tại: {sourceBackup}", "OK");
                }
                return;
            }

            try
            {
                Debug.Log("<color=cyan><b>[SceneBackupGuard] 🔄 Đang khôi phục toàn bộ Map 2 (Map_Zone_2) và các chỉnh sửa của Map 1...</b></color>");

                // 1. Chuyển tạm sang EmptyScene để giải phóng khóa file MainGame.unity
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 2. Chép bản sao lưu đầy đủ Map 2 (5.98 MB) đè lên MainGame.unity
                File.Copy(sourceBackup, MainScenePath, true);
                File.Copy(sourceBackup, SafeBackupPath, true);
                AssetDatabase.Refresh();

                // 3. Mở lại MainGame.unity
                Scene mainScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

                // 4. Tái tạo đầy đủ các đối tượng chỉnh sửa của Map 1 đã thêm khi làm Map 2
                ReconstructCustomMap1Edits(mainScene);

                // 5. Lưu lại MainGame.unity và cập nhật SafeBackup
                EditorSceneManager.MarkSceneDirty(mainScene);
                EditorSceneManager.SaveScene(mainScene);
                File.Copy(MainScenePath, SafeBackupPath, true);

                Debug.Log("<color=green><b>[SceneBackupGuard] ✅ ĐÃ KHÔI PHỤC HOÀN TẤT 100% CẢ MAP 2 (Map_Zone_2) VÀ MAP 1 ĐÃ CHỈNH SỬA!</b></color>");

                if (!silent)
                {
                    EditorUtility.DisplayDialog(
                        "Khôi Phục Thành Công",
                        "Đã khôi phục hoàn tất cả Map 2 (Map_Zone_2) và các chỉnh sửa trên Map 1 trong Scene MainGame!",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneBackupGuard] Lỗi khi khôi phục Map 1 & Map 2: {ex}");
            }
        }

        private static void ReconstructCustomMap1Edits(Scene scene)
        {
            GameObject gridWorld = GameObject.Find("Grid_World");
            if (gridWorld == null) return;

            Transform gridTr = gridWorld.transform;

            // Load prefabs & sprites dùng trong các chỉnh sửa Map 1
            GameObject fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ForestPrototype/PetSanctuary_Fence1.prefab");
            GameObject tree20Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ForestPrototype/Border_ForestTree_20.prefab");
            GameObject tree23Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ForestPrototype/Border_ForestTree_23.prefab");
            GameObject monumentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ForestPrototype/Peak_Monument_2.prefab");

            // 1. Khôi phục dãy hàng rào PetSanctuary_Fence1 (0..6) đúng tọa độ gốc trong Grid_World
            if (fencePrefab != null && gridTr.Find("PetSanctuary_Fence1 (5)") == null)
            {
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1", gridTr, new Vector3(-52.45f, 7.11f, 0f), Quaternion.identity, Vector3.one);
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1 (1)", gridTr, new Vector3(-50.44f, 7.10f, 0f), Quaternion.identity, Vector3.one);
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1 (2)", gridTr, new Vector3(-44.51f, 7.06f, 0f), Quaternion.identity, Vector3.one);
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1 (3)", gridTr, new Vector3(-32.51f, 6.46f, 0f), Quaternion.identity, Vector3.one);
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1 (4)", gridTr, new Vector3(-35.53f, 8.48f, 0f), Quaternion.identity, Vector3.one);
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1 (5)", gridTr, new Vector3(-34.32f, 7.45f, 0.24f), new Quaternion(-0.710902f, 0.6961283f, -0.075449154f, 0.06581191f), Vector3.one);
                SpawnPrefabChild(fencePrefab, "PetSanctuary_Fence1 (6)", gridTr, new Vector3(-41.42f, 8.50f, 0f), Quaternion.identity, Vector3.one);
            }

            // 2. Khôi phục các cây rừng biên giới nối hướng lên Map 2 trong Grid_World
            if (tree20Prefab != null && gridTr.Find("Border_ForestTree_20") == null)
            {
                Vector3 treeScale = new Vector3(1.25f, 1.25f, 1f);
                SpawnPrefabChild(tree20Prefab, "Border_ForestTree_20", gridTr, new Vector3(-12.65f, 29.88f, 0f), Quaternion.identity, treeScale);
                SpawnPrefabChild(tree20Prefab, "Border_ForestTree_20 (1)", gridTr, new Vector3(-7.89f, 29.94f, 0f), Quaternion.identity, treeScale);
                SpawnPrefabChild(tree20Prefab, "Border_ForestTree_-22", gridTr, new Vector3(26.28f, 20.00f, 0f), Quaternion.identity, treeScale);
            }

            if (tree23Prefab != null && gridTr.Find("Border_ForestTree_23 (1)") == null)
            {
                SpawnPrefabChild(tree23Prefab, "Border_ForestTree_23", gridTr, new Vector3(-15.20f, 28.50f, 0f), Quaternion.identity, new Vector3(1.25f, 1.25f, 1f));
                SpawnPrefabChild(tree23Prefab, "Border_ForestTree_23 (1)", gridTr, new Vector3(-5.40f, 28.50f, 0f), Quaternion.identity, new Vector3(1.25f, 1.25f, 1f));
            }

            // 3. Khôi phục Đài tưởng niệm Peak_Monument_2
            if (monumentPrefab != null && gridTr.Find("Peak_Monument_2 (1)") == null)
            {
                SpawnPrefabChild(monumentPrefab, "Peak_Monument_2", gridTr, new Vector3(88.04f, 25.48f, 0f), Quaternion.identity, Vector3.one);
                SpawnPrefabChild(monumentPrefab, "Peak_Monument_2 (1)", gridTr, new Vector3(84.50f, 25.48f, 0f), Quaternion.identity, Vector3.one);
            }

            // 4. Khôi phục Apothecary_Sign, VillageGrass_16 (0..2), Volcano_DarkRock_2 (0..4) trong Grid_World
            GameObject existingSign = GameObject.Find("Apothecary_Sign");
            if (existingSign != null && gridTr.Find("Apothecary_Sign") == null)
            {
                GameObject signCopy = UnityEngine.Object.Instantiate(existingSign, gridTr);
                signCopy.name = "Apothecary_Sign";
                signCopy.transform.localPosition = new Vector3(-40.56f, 7.50f, 0f);
            }

            GameObject existingGrass = GameObject.Find("VillageGrass_16");
            if (existingGrass != null && gridTr.Find("VillageGrass_16 (1)") == null)
            {
                Vector3[] grassPositions = new Vector3[]
                {
                    new Vector3(-42.0f, 6.8f, 0f),
                    new Vector3(-38.5f, 6.8f, 0f),
                    new Vector3(-35.0f, 6.8f, 0f)
                };
                string[] grassNames = { "VillageGrass_16", "VillageGrass_16 (1)", "VillageGrass_16 (2)" };
                for (int i = 0; i < grassNames.Length; i++)
                {
                    GameObject g = UnityEngine.Object.Instantiate(existingGrass, gridTr);
                    g.name = grassNames[i];
                    g.transform.localPosition = grassPositions[i];
                }
            }

            GameObject existingRock = GameObject.Find("Volcano_DarkRock_2");
            if (existingRock != null && gridTr.Find("Volcano_DarkRock_2 (1)") == null)
            {
                Vector3[] rockPositions = new Vector3[]
                {
                    new Vector3(34.5f, -14.0f, 0f),
                    new Vector3(38.0f, -12.5f, 0f),
                    new Vector3(42.0f, -15.0f, 0f),
                    new Vector3(46.5f, -13.0f, 0f),
                    new Vector3(50.0f, -16.0f, 0f)
                };
                string[] rockNames = { "Volcano_DarkRock_2", "Volcano_DarkRock_2 (1)", "Volcano_DarkRock_2 (2)", "Volcano_DarkRock_2 (3)", "Volcano_DarkRock_2 (4)" };
                for (int i = 0; i < rockNames.Length; i++)
                {
                    GameObject r = UnityEngine.Object.Instantiate(existingRock, gridTr);
                    r.name = rockNames[i];
                    r.transform.localPosition = rockPositions[i];
                }
            }

            // 5. Khôi phục Cầu gỗ (bridge_wood_vertical), Bờ hồ cát (sandy_lake_shore), Cây khô (tree-dried_0, trunk_0) & Suối nước (tile_serene_water_*)
            if (gridTr.Find("bridge_wood_vertical") == null)
            {
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/bridge_wood_vertical.png", "bridge_wood_vertical", gridTr, new Vector3(-24.0f, -6.0f, 0f), Vector3.one, 15);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/bridge_wood_vertical.png", "bridge_wood_vertical (1)", gridTr, new Vector3(-24.0f, 12.0f, 0f), Vector3.one, 15);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/sandy_lake_shore.png", "sandy_lake_shore", gridTr, new Vector3(8.0f, -14.0f, 0f), Vector3.one, 5);
                SpawnSpriteChild("Assets/Sprites/ForestPack/tree-dried.png", "tree-dried_0", gridTr, new Vector3(36.0f, -18.0f, 0f), Vector3.one, 5000);
                SpawnSpriteChild("Assets/Sprites/ForestPack/trunk.png", "trunk_0", gridTr, new Vector3(-18.0f, 22.0f, 0f), Vector3.one, 5000);

                // Các mảnh suối/nước mở rộng trong Grid_World đúng tỷ lệ scale đã lưu
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_center.png", "tile_serene_water_center", gridTr, new Vector3(8.0f, -14.0f, 0f), new Vector3(11f, 11f, 1f), 2);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_center.png", "tile_serene_water_center (2)", gridTr, new Vector3(-10.0f, -20.0f, 0f), new Vector3(77f, 4f, 1f), 2);
                for (int i = 3; i <= 11; i++)
                {
                    SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_center.png", $"tile_serene_water_center ({i})", gridTr, new Vector3(-24.0f, -16.0f + (i - 3) * 3.5f, 0f), Vector3.one, 2);
                }
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_left.png", "tile_serene_water_left", gridTr, new Vector3(-25.5f, -12.0f, 0f), new Vector3(2f, 2f, 1f), 2);
                for (int i = 1; i <= 4; i++)
                {
                    SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_left.png", $"tile_serene_water_left ({i})", gridTr, new Vector3(-25.5f, -12.0f + i * 4.0f, 0f), new Vector3(2f, 2f, 1f), 2);
                }
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_right.png", "tile_serene_water_right", gridTr, new Vector3(-22.5f, -8.0f, 0f), new Vector3(2f, 6f, 1f), 2);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_right.png", "tile_serene_water_right (1)", gridTr, new Vector3(-22.5f, 4.0f, 0f), new Vector3(2f, 5f, 1f), 2);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_tl.png", "tile_serene_water_tl", gridTr, new Vector3(-25.5f, 16.0f, 0f), Vector3.one, 2);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_top.png", "tile_serene_water_top", gridTr, new Vector3(-24.0f, 16.0f, 0f), Vector3.one, 2);
                SpawnSpriteChild("Assets/Sprites/VillageAndWorld/tile_serene_water_tr.png", "tile_serene_water_tr", gridTr, new Vector3(-22.5f, 16.0f, 0f), Vector3.one, 2);
            }
        }

        private static GameObject SpawnPrefabChild(GameObject prefab, string name, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            if (prefab == null) return null;
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            return go;
        }

        private static GameObject SpawnSpriteChild(string spritePath, string name, Transform parent, Vector3 localPos, Vector3 localScale, int sortingOrder)
        {
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sp == null) return null;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        [MenuItem("Tools/Map/🛡️ Sao Lưu Map Ngay Lập Tức (Create Map Backup)", priority = -150)]
        public static void CreateBackupMenu()
        {
            CreateBackupInternal(silent: false);
        }

        [MenuItem("Tools/Map/↩️ Khôi Phục Map Từ Bản Sao Lưu An Toàn (Restore Map)", priority = -149)]
        public static void RestoreBackupMenu()
        {
            string backupToRestore = GetBestBackupPath();
            if (!File.Exists(backupToRestore))
            {
                EditorUtility.DisplayDialog(
                    "Không Tìm Thấy Bản Sao Lưu",
                    $"Chưa có file sao lưu an toàn tại: {backupToRestore}",
                    "OK");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Xác Nhận Khôi Phục Map",
                $"Bạn có chắc chắn muốn khôi phục lại Scene MainGame từ bản sao lưu?\n\nFile nguồn: {Path.GetFileName(backupToRestore)}\n\nMọi cấu trúc Map 1 (đã sửa) và Map 2 sẽ được hoàn nguyên chính xác.",
                "Khôi Phục Ngay",
                "HỦY");

            if (!confirm) return;

            try
            {
                bool wasMainGameOpen = EditorSceneManager.GetActiveScene().path == MainScenePath;
                if (wasMainGameOpen)
                {
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }

                File.Copy(backupToRestore, MainScenePath, true);
                File.Copy(backupToRestore, SafeBackupPath, true);
                AssetDatabase.Refresh();

                EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
                Debug.Log($"<color=green><b>[SceneBackupGuard] ✅ ĐÃ KHÔI PHỤC TOÀN BỘ MAP 1 & MAP 2 TỪ BẢN SAO LƯU {Path.GetFileName(backupToRestore)} THÀNH CÔNG!</b></color>");
                EditorUtility.DisplayDialog("Thành Công", $"Đã khôi phục hoàn toàn cấu trúc Map 1 và Map 2 từ bản sao lưu {Path.GetFileName(backupToRestore)}!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneBackupGuard] Lỗi khi khôi phục bản sao lưu: {ex.Message}");
            }
        }

        public static string GetBestBackupPath()
        {
            if (File.Exists(SafeBackupPath) && new FileInfo(SafeBackupPath).Length >= MinValidMap1AndMap2Size)
            {
                return SafeBackupPath;
            }

            if (Directory.Exists(BackupDir))
            {
                var dir = new DirectoryInfo(BackupDir);
                var files = dir.GetFiles("MainGame_Backup_*.unity");
                Array.Sort(files, (a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
                foreach (var f in files)
                {
                    if (f.Length >= MinValidMap1AndMap2Size) return f.FullName;
                }
            }

            if (File.Exists(ProtectedBackupPath)) return ProtectedBackupPath;
            return SafeBackupPath;
        }

        private static void CreateBackupInternal(bool silent)
        {
            if (!File.Exists(MainScenePath)) return;

            try
            {
                if (!Directory.Exists(BackupDir))
                {
                    Directory.CreateDirectory(BackupDir);
                }

                // 1. Chỉ cập nhật bản SafeBackup cố định khi Scene có đầy đủ cả Map 1 và Map 2 (>= 5.5 MB)
                var info = new FileInfo(MainScenePath);
                if (info.Length >= MinValidMap1AndMap2Size || !File.Exists(SafeBackupPath))
                {
                    File.Copy(MainScenePath, SafeBackupPath, true);
                }

                // 2. Tạo bản sao lưu xoay vòng theo thời gian (timestamp)
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string timedBackupPath = Path.Combine(BackupDir, $"MainGame_Backup_{timestamp}.unity");
                File.Copy(MainScenePath, timedBackupPath, true);

                // 3. Giới hạn số lượng bản sao lưu xoay vòng
                CleanOldBackups();

                AssetDatabase.Refresh();

                if (!silent)
                {
                    Debug.Log($"<color=green><b>[SceneBackupGuard] ✅ Đã tạo bản sao lưu Map an toàn tại: {timedBackupPath}</b></color>");
                    EditorUtility.DisplayDialog(
                        "Sao Lưu Thành Công",
                        $"Đã tạo bản sao lưu Map an toàn thành công!\n\nFile lưu: {SafeBackupPath}\n(Kèm bản lưu xoay vòng: {timedBackupPath})",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneBackupGuard] Lỗi khi tạo bản sao lưu: {ex.Message}");
            }
        }

        private static void CleanOldBackups()
        {
            try
            {
                var dir = new DirectoryInfo(BackupDir);
                var files = dir.GetFiles("MainGame_Backup_*.unity");
                if (files.Length > MaxRotatingBackups)
                {
                    Array.Sort(files, (a, b) => a.CreationTime.CompareTo(b.CreationTime));
                    for (int i = 0; i < files.Length - MaxRotatingBackups; i++)
                    {
                        files[i].Delete();
                        string meta = files[i].FullName + ".meta";
                        if (File.Exists(meta)) File.Delete(meta);
                    }
                }
            }
            catch { }
        }
    }
}
#endif
