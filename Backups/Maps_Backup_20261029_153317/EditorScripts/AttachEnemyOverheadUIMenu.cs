using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ActionRPG.Enemies;
using ActionRPG.UI;
using ActionRPG.Combat;
using ActionRPG.Items;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ Editor quét toàn bộ các đối tượng Quái Vật, Bù Nhìn, Boss
    /// trong Scene hiện tại và trong các Prefabs để gắn sẵn EnemyOverheadUI.
    /// Menu: Tools -> Lumina -> Attach Overhead HP Bar To All Enemies
    /// </summary>
    public static class AttachEnemyOverheadUIMenu
    {
        [MenuItem("Tools/Lumina/Attach Overhead HP Bar To All Enemies", priority = 120)]
        public static void AttachOverheadHPBarToAllEnemies()
        {
            int sceneAttachedCount = AttachToCurrentScene();
            int prefabAttachedCount = AttachToEnemyPrefabs();

            string message = $"Hoàn tất tích hợp Thanh Máu & Level trên đầu Quái Vật!\n\n" +
                             $"• Quái vật/Bù nhìn trong Scene: {sceneAttachedCount} đối tượng được gắn.\n" +
                             $"• Prefabs Quái vật trong Assets: {prefabAttachedCount} prefab được cập nhật.\n\n" +
                             $"Sorting Order: 500 (Không bị che khuất).\n" +
                             $"Hiển thị định dạng: 'Lv.X [Tên Quái]' kèm thanh máu đỏ.";

            Debug.Log($"<color=green>[Lumina Overhead UI] {message.Replace("\n", " ")}</color>");
            EditorUtility.DisplayDialog("Lumina Overhead Health Bar", message, "Tuyệt vời!");
        }

        public static int AttachToCurrentScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            int count = 0;

            var allGameObjects = new List<GameObject>();
            foreach (var root in rootObjects)
            {
                CollectAllChildren(root, allGameObjects);
            }

            foreach (var go in allGameObjects)
            {
                if (IsEnemyOrDummy(go))
                {
                    // Đảm bảo có EnemyStats nếu chưa có
                    if (go.GetComponent<EnemyStats>() == null && go.GetComponent<EnemyController>() == null)
                    {
                        Undo.AddComponent<EnemyStats>(go);
                    }

                    // Gắn EnemyOverheadUI nếu chưa có
                    if (go.GetComponent<EnemyOverheadUI>() == null)
                    {
                        Undo.AddComponent<EnemyOverheadUI>(go);
                        count++;
                        EditorUtility.SetDirty(go);
                    }
                }
            }

            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            return count;
        }

        public static int AttachToEnemyPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            int count = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                if (IsEnemyOrDummy(prefab))
                {
                    if (prefab.GetComponent<EnemyOverheadUI>() == null)
                    {
                        GameObject root = PrefabUtility.LoadPrefabContents(path);
                        if (root.GetComponent<EnemyOverheadUI>() == null)
                        {
                            root.AddComponent<EnemyOverheadUI>();
                            PrefabUtility.SaveAsPrefabAsset(root, path);
                            count++;
                        }
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }

            return count;
        }

        [MenuItem("Tools/Lumina/Cấu Hình Động Vật Có Thể Tiêu Diệt & Thu Phục Pet", priority = 125)]
        public static void ConfigureAllAnimalsKillableAndCapturable()
        {
            int countScene = ConfigureAnimalsInCurrentScene();
            int countPrefabs = ConfigureAnimalPrefabs();

            string msg = $"Hoàn tất cấu hình toàn bộ Động Vật:\n" +
                         $"• {countScene} động vật trong Scene đã có thể bị tấn công, tiêu diệt và thu phục!\n" +
                         $"• {countPrefabs} Prefabs động vật đã được cập nhật chuẩn (Tag Enemy, Dynamic Rigidbody, Hurtbox2D, EnemyStats, EnemyOverheadUI, Loot Da Thú).\n" +
                         $"• Khi máu < 30%, người chơi có thể bấm [C] để dùng Quả Cầu Tinh Linh bắt làm Pet!";

            Debug.Log($"<color=green>[Lumina Animals] {msg.Replace("\n", " ")}</color>");
            EditorUtility.DisplayDialog("Cấu Hình Động Vật", msg, "Tuyệt vời!");
        }

        public static int ConfigureAnimalsInCurrentScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            int count = 0;

            var allGameObjects = new List<GameObject>();
            foreach (var root in rootObjects)
            {
                CollectAllChildren(root, allGameObjects);
            }

            var hideItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Resources/Items/material_animal_hide.asset");
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            foreach (var go in allGameObjects)
            {
                if (IsAnimalOrCreature(go))
                {
                    Undo.RecordObject(go, "Configure Animal");

                    go.tag = "Enemy";
                    if (enemyLayer != -1) go.layer = enemyLayer;

                    var rb = go.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.gravityScale = 0f;
                        rb.freezeRotation = true;
                    }

                    if (go.GetComponent<Hurtbox2D>() == null)
                    {
                        Undo.AddComponent<Hurtbox2D>(go);
                    }

                    var stats = go.GetComponent<EnemyStats>();
                    if (stats == null)
                    {
                        stats = Undo.AddComponent<EnemyStats>(go);
                        stats.maxHp = 60f;
                        stats.currentHp = 60f;
                    }

                    if (stats.dropTable == null || stats.dropTable.Count == 0)
                    {
                        if (hideItem != null)
                        {
                            stats.dropTable = new List<DropEntry>
                            {
                                new DropEntry { item = hideItem, dropChancePercent = 85f, minQuantity = 1, maxQuantity = 2 }
                            };
                        }
                    }

                    if (go.GetComponent<EnemyOverheadUI>() == null)
                    {
                        Undo.AddComponent<EnemyOverheadUI>(go);
                    }

                    count++;
                    EditorUtility.SetDirty(go);
                }
            }

            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
            return count;
        }

        public static int ConfigureAnimalPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            int count = 0;
            var hideItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Resources/Items/material_animal_hide.asset");
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                if (IsAnimalOrCreature(prefab))
                {
                    GameObject root = PrefabUtility.LoadPrefabContents(path);

                    root.tag = "Enemy";
                    if (enemyLayer != -1) root.layer = enemyLayer;

                    var rb = root.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.gravityScale = 0f;
                        rb.freezeRotation = true;
                    }

                    if (root.GetComponent<Hurtbox2D>() == null)
                    {
                        root.AddComponent<Hurtbox2D>();
                    }

                    var stats = root.GetComponent<EnemyStats>();
                    if (stats == null)
                    {
                        stats = root.AddComponent<EnemyStats>();
                        stats.maxHp = 60f;
                        stats.currentHp = 60f;
                    }

                    if (stats.dropTable == null || stats.dropTable.Count == 0)
                    {
                        if (hideItem != null)
                        {
                            stats.dropTable = new List<DropEntry>
                            {
                                new DropEntry { item = hideItem, dropChancePercent = 85f, minQuantity = 1, maxQuantity = 2 }
                            };
                        }
                    }

                    if (root.GetComponent<EnemyOverheadUI>() == null)
                    {
                        root.AddComponent<EnemyOverheadUI>();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    PrefabUtility.UnloadPrefabContents(root);
                    count++;
                }
            }

            return count;
        }

        public static bool IsAnimalOrCreature(GameObject go)
        {
            if (go == null) return false;
            string lower = go.name.ToLower();
            bool match = lower.Contains("deer") || lower.Contains("hươu") ||
                         lower.Contains("rabbit") || lower.Contains("thỏ") ||
                         lower.Contains("wolf") || lower.Contains("sói") ||
                         lower.Contains("boar") || lower.Contains("lợn") || lower.Contains("heo") ||
                         lower.Contains("animal");

            if (match)
            {
                if (go.GetComponent<SpriteRenderer>() != null || go.GetComponent<Collider2D>() != null || go.GetComponent<Hurtbox2D>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsEnemyOrDummy(GameObject go)
        {
            if (go == null) return false;

            // 1. Kiểm tra Component
            if (go.GetComponent<EnemyStats>() != null || go.GetComponent<EnemyController>() != null)
            {
                return true;
            }

            // 2. Kiểm tra Tag
            if (go.CompareTag("Enemy"))
            {
                return true;
            }

            // 3. Kiểm tra Tên Quái / Bù nhìn / Động vật
            string lower = go.name.ToLower();
            bool nameMatch = lower.Contains("enemy") || lower.Contains("monster") ||
                             lower.Contains("wolf") || lower.Contains("sói") ||
                             lower.Contains("dummy") || lower.Contains("bù nhìn") ||
                             lower.Contains("mole") || lower.Contains("treant") ||
                             lower.Contains("slime") || lower.Contains("boar") ||
                             lower.Contains("crab") || lower.Contains("deer") ||
                             lower.Contains("hươu") || lower.Contains("rabbit") ||
                             lower.Contains("thỏ") || lower.Contains("animal");

            if (nameMatch)
            {
                // Chỉ nhận diện là quái vật nếu có SpriteRenderer hoặc Collider (tránh nhầm lẫn Spawner / Waypoint)
                if (go.GetComponent<SpriteRenderer>() != null || go.GetComponent<Collider2D>() != null || go.GetComponent<Hurtbox2D>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CollectAllChildren(GameObject current, List<GameObject> list)
        {
            if (current == null) return;
            list.Add(current);
            for (int i = 0; i < current.transform.childCount; i++)
            {
                CollectAllChildren(current.transform.GetChild(i).gameObject, list);
            }
        }
    }
}
