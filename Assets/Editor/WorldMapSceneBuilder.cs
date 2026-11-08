#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Core.Bridges;
using ActionRPG.Core.World;
using ActionRPG.Core.World.Interactables;
using ActionRPG.Core.World.Spawners;
using ActionRPG.Enemies;
using ActionRPG.Environment;
using ActionRPG.Items.Loot;
using ActionRPG.Presentation.Views;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ Editor tự động thiết lập bản đồ thế giới Lumina (World Map Layout) chuẩn Sprint 11:
    /// - Grid_World & Hệ thống Phân Vùng.
    /// - Zone 01: Làng Tân Thủ (SafeZoneTrigger, Waystone_Village, Điểm spawn Player).
    /// - Zone 02: Thảo Nguyên Xanh (3 Spawner Lv 1-10, 4 GatheringNode thảo dược & khoáng thạch).
    /// - Zone 03: Rừng Rậm Hắc Ám (4 Spawner Lv 10-20, Waystone_Forest, Cổng vào Hang Boss).
    /// - Boss Arena: Sào huyệt Boss Hắc Diệu Ma Viên (Telegraph, đa phase).
    /// </summary>
    public static class WorldMapSceneBuilder
    {
        private const string SoEnemyPath = "Assets/ScriptableObjects/Enemies";
        private const string PrefabPath = "Assets/Prefabs";

        [MenuItem("Tools/Setup Lumina World Layout")]
        [MenuItem("Tools/Lumina/Setup Lumina World Layout")]
        public static void SetupLuminaWorldLayout()
        {
            bool confirm = EditorUtility.DisplayDialog(
                "⚠️ CẢNH BÁO BẢO VỆ MAP",
                "Thao tác này sẽ tái thiết lập các phân vùng và có thể xóa object '=== WORLD ENVIRONMENT ===' trong Scene.\n\n" +
                "Bạn có chắc chắn muốn thực hiện không?",
                "Tiếp tục",
                "HỦY BỎ (Bảo vệ Map)");

            if (!confirm)
            {
                Debug.Log("<color=yellow>[WorldMapSceneBuilder] Đã hủy thao tác. Map hiện tại được giữ nguyên an toàn.</color>");
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "MainGame")
            {
                if (System.IO.File.Exists("Assets/Scenes/MainGame.unity"))
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
                }
            }

            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🗺️ BẮT ĐẦU THIẾT LẬP BẢN ĐỒ THẾ GIỚI LUMINA (SPRINT 11)</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            // 1. Đảm bảo GameManager & ZoneManager
            EnsureGameSystems();

            // 2. Tìm hoặc tạo Root World Environment
            GameObject worldRoot = GameObject.Find("=== WORLD ENVIRONMENT ===");
            if (worldRoot != null)
            {
                Object.DestroyImmediate(worldRoot);
            }
            worldRoot = new GameObject("=== WORLD ENVIRONMENT ===");

            // 3. Xây dựng Grid & Phân vùng 1: Làng Tân Thủ (Khu An Toàn)
            BuildVillageZone(worldRoot);

            // 4. Xây dựng Phân vùng 2: Thảo Nguyên Xanh (Lv 1 - 10)
            BuildGrasslandZone(worldRoot);

            // 5. Xây dựng Phân vùng 3: Rừng Rậm Hắc Ám (Lv 10 - 20)
            BuildForestZone(worldRoot);

            // 6. Xây dựng Phân vùng 4: Sào Huyệt Boss Hắc Diệu Ma Viên
            BuildBossArenaZone(worldRoot);

            // 7. Đồng bộ Player với Clean Architecture Presenter & Waystone
            SetupPlayerArchitecture();

            // 8. Lưu Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=green>🎉 THIẾT LẬP HOÀN TẤT BẢN ĐỒ THẾ GIỚI LUMINA SPRINT 11 TRONG SCENE MAINGAME.UNITY!</color>");
            Debug.Log("<color=cyan>==================================================</color>");
        }

        private static void EnsureGameSystems()
        {
            GameObject systems = GameObject.Find("GameSystems");
            if (systems == null)
            {
                systems = new GameObject("GameSystems");
            }

            if (systems.GetComponent<GameManager>() == null) systems.AddComponent<GameManager>();
            if (systems.GetComponent<ZoneManager>() == null) systems.AddComponent<ZoneManager>();
        }

        private static void BuildVillageZone(GameObject root)
        {
            GameObject villageObj = new GameObject("Zone_01_Village");
            villageObj.transform.SetParent(root.transform);
            villageObj.transform.position = Vector3.zero;

            // 1. Safe Zone Trigger
            GameObject safeTriggerObj = new GameObject("SafeZoneTrigger_Village");
            safeTriggerObj.transform.SetParent(villageObj.transform);
            safeTriggerObj.transform.position = Vector3.zero;

            var boxCol = safeTriggerObj.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(28f, 24f);
            safeTriggerObj.AddComponent<SafeZoneTrigger>();

            // 2. Waystone Làng Tân Thủ
            GameObject waystoneObj = new GameObject("Waystone_Village");
            waystoneObj.transform.SetParent(villageObj.transform);
            waystoneObj.transform.position = new Vector3(0f, 2.0f, 0f);

            var stoneCol = waystoneObj.AddComponent<CircleCollider2D>();
            stoneCol.isTrigger = true;
            stoneCol.radius = 2.2f;

            var waystone = waystoneObj.AddComponent<Waystone>();
            waystone.waystoneId = "waystone_village";
            waystone.waystoneName = "Trụ Đá Làng Oakhaven";
            waystone.isUnlocked = true;

            // Visual biểu trưng cho Trụ Đá
            var sr = waystoneObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.85f, 1f, 1f);
            sr.sortingOrder = 3;
            waystone.stoneRenderer = sr;

            // 3. Điểm Spawn Player
            GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
            spawnPoint.transform.SetParent(villageObj.transform);
            spawnPoint.transform.position = Vector3.zero;
        }

        private static void BuildGrasslandZone(GameObject root)
        {
            GameObject grasslandObj = new GameObject("Zone_02_Grassland");
            grasslandObj.transform.SetParent(root.transform);
            grasslandObj.transform.position = Vector3.zero;

            // 1. Nhóm Spawner Quái Thảo Nguyên (Lv 1 - 10)
            GameObject spawnerGroup = new GameObject("Spawners_Grassland");
            spawnerGroup.transform.SetParent(grasslandObj.transform);

            // Spawner 1: Đồng Cỏ Ven Làng (Lv 1 - 3: Slime, Mầm Cây)
            CreateZoneSpawner(spawnerGroup, "Spawner_Steppe_01_Slimes", new Vector3(18f, 5f, 0f),
                GameZone.Grassland_Lv1_10, 4, 6.0f, 20.0f,
                new[] { "enemy_steppe_slime.asset", "enemy_steppe_sprout.asset" });

            // Spawner 2: Đồng Cỏ Bụi Gai (Lv 4 - 7: Nhím Gai, Heo Rừng)
            CreateZoneSpawner(spawnerGroup, "Spawner_Steppe_02_Boars", new Vector3(28f, -6f, 0f),
                GameZone.Grassland_Lv1_10, 4, 7.0f, 22.0f,
                new[] { "enemy_steppe_hedgehog.asset", "enemy_steppe_boar.asset" });

            // Spawner 3: Bãi Đá Thảo Nguyên (Lv 8 - 10: Cua Đá, Slime)
            CreateZoneSpawner(spawnerGroup, "Spawner_Steppe_03_RockCrabs", new Vector3(38f, 8f, 0f),
                GameZone.Grassland_Lv1_10, 4, 7.0f, 25.0f,
                new[] { "enemy_steppe_rock_crab.asset", "enemy_steppe_slime.asset" });

            // 2. Nhóm Node Thu Thập Nguyên Liệu (Gathering Nodes)
            GameObject nodesGroup = new GameObject("GatheringNodes_Grassland");
            nodesGroup.transform.SetParent(grasslandObj.transform);

            CreateGatheringNode(nodesGroup, "Gathering_Herb_01", new Vector3(15f, -4f, 0f), GatheringNodeType.HerbBush, "Bụi Thảo Dược Ven Đường");
            CreateGatheringNode(nodesGroup, "Gathering_Herb_02", new Vector3(32f, 2f, 0f), GatheringNodeType.HerbBush, "Bụi Cây Thảo Mộc");
            CreateGatheringNode(nodesGroup, "Gathering_Ore_01", new Vector3(22f, 10f, 0f), GatheringNodeType.OreVein, "Bãi Khoáng Thạch Đồng Thau");
            CreateGatheringNode(nodesGroup, "Gathering_Ore_02", new Vector3(36f, -8f, 0f), GatheringNodeType.OreVein, "Bãi Đá Khoáng");
        }

        private static void BuildForestZone(GameObject root)
        {
            GameObject forestObj = new GameObject("Zone_03_Forest");
            forestObj.transform.SetParent(root.transform);
            forestObj.transform.position = Vector3.zero;

            // 1. Waystone Bìa Rừng Rậm
            GameObject waystoneObj = new GameObject("Waystone_Forest");
            waystoneObj.transform.SetParent(forestObj.transform);
            waystoneObj.transform.position = new Vector3(50f, 0f, 0f);

            var stoneCol = waystoneObj.AddComponent<CircleCollider2D>();
            stoneCol.isTrigger = true;
            stoneCol.radius = 2.2f;

            var waystone = waystoneObj.AddComponent<Waystone>();
            waystone.waystoneId = "waystone_forest";
            waystone.waystoneName = "Trụ Đá Bìa Rừng Rậm";
            waystone.isUnlocked = false;

            var sr = waystoneObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            sr.sortingOrder = 3;
            waystone.stoneRenderer = sr;

            // 2. Nhóm Spawner Thú Rừng (Lv 10 - 20)
            GameObject spawnerGroup = new GameObject("Spawners_Forest");
            spawnerGroup.transform.SetParent(forestObj.transform);

            // Spawner 1: Sói Lửa Rừng Rậm (Lv 11 - 14)
            CreateZoneSpawner(spawnerGroup, "Spawner_Forest_01_FireWolves", new Vector3(58f, 8f, 0f),
                GameZone.DeepForest_Lv10_20, 4, 6.5f, 22.0f,
                new[] { "enemy_forest_fire_wolf.asset" });

            // Spawner 2: Cú Đêm Rừng Sâu (Lv 14 - 16)
            CreateZoneSpawner(spawnerGroup, "Spawner_Forest_02_NightOwls", new Vector3(68f, -8f, 0f),
                GameZone.DeepForest_Lv10_20, 3, 6.5f, 24.0f,
                new[] { "enemy_forest_night_owl.asset" });

            // Spawner 3: Báo Tuyết & Lợn Rừng Đen (Lv 16 - 18)
            CreateZoneSpawner(spawnerGroup, "Spawner_Forest_03_SnowLeopards", new Vector3(76f, 12f, 0f),
                GameZone.DeepForest_Lv10_20, 4, 7.0f, 25.0f,
                new[] { "enemy_forest_snow_leopard.asset", "enemy_forest_dark_boar.asset" });

            // Spawner 4: Cổ Thụ & Nấm Độc (Lv 18 - 20)
            CreateZoneSpawner(spawnerGroup, "Spawner_Forest_04_Treants", new Vector3(84f, -4f, 0f),
                GameZone.DeepForest_Lv10_20, 4, 7.0f, 25.0f,
                new[] { "enemy_forest_treant.asset", "enemy_forest_spore_mushroom.asset" });

            // 3. Cổng Vào Hang Boss (Boss Gate)
            GameObject bossGate = new GameObject("BossGate_Entrance");
            bossGate.transform.SetParent(forestObj.transform);
            bossGate.transform.position = new Vector3(92f, 0f, 0f);

            var gateCol = bossGate.AddComponent<BoxCollider2D>();
            gateCol.isTrigger = true;
            gateCol.size = new Vector2(3f, 8f);
        }

        private static void BuildBossArenaZone(GameObject root)
        {
            GameObject arenaObj = new GameObject("Boss_Arena");
            arenaObj.transform.SetParent(root.transform);
            arenaObj.transform.position = new Vector3(105f, 0f, 0f);

            // 1. Boss Arena Zone Trigger
            GameObject arenaTrigger = new GameObject("BossArena_Trigger");
            arenaTrigger.transform.SetParent(arenaObj.transform);
            arenaTrigger.transform.localPosition = Vector3.zero;

            var triggerCol = arenaTrigger.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(26f, 22f);

            var safeTrigger = arenaTrigger.AddComponent<SafeZoneTrigger>();
            // Gán trigger chuyển sang BossArena
            var fieldInside = typeof(SafeZoneTrigger).GetField("insideZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInside != null) fieldInside.SetValue(safeTrigger, GameZone.BossArena);

            // 2. Khởi tạo Boss Hắc Diệu Ma Viên
            GameObject bossObj = new GameObject("Boss_BlackDemonApe");
            bossObj.transform.SetParent(arenaObj.transform);
            bossObj.transform.localPosition = new Vector3(4f, 0f, 0f);
            bossObj.tag = "Enemy";

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) bossObj.layer = enemyLayer;

            var sr = bossObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            sr.sortingOrder = 5;

            var col = bossObj.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;

            var rb = bossObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            bossObj.AddComponent<Hurtbox2D>();
            var stats = bossObj.AddComponent<EnemyStats>();
            var bossCtrl = bossObj.AddComponent<BossController>();
            bossCtrl.isWorldBoss = true;

            // Nạp dữ liệu Boss từ ScriptableObject
            var bossData = AssetDatabase.LoadAssetAtPath<EnemyData>($"{SoEnemyPath}/boss_black_demon_ape.asset");
            if (bossData != null)
            {
                stats.Initialize(bossData);
                bossCtrl.enemyData = bossData;
            }

            bossObj.AddComponent<EnemyHealthBarWorldSpace>();
            bossObj.AddComponent<LootDropper>();
        }

        private static void CreateZoneSpawner(GameObject parent, string spawnerName, Vector3 pos, GameZone zone, int maxMobs, float radius, float respawnDelay, string[] enemyAssetFiles)
        {
            GameObject spawnerObj = new GameObject(spawnerName);
            spawnerObj.transform.SetParent(parent.transform);
            spawnerObj.transform.position = pos;

            var spawner = spawnerObj.AddComponent<ZoneEnemySpawner>();
            spawner.zone = zone;
            spawner.spawnerName = spawnerName;
            spawner.maxEnemiesAlive = maxMobs;
            spawner.spawnRadius = radius;
            spawner.respawnDelay = respawnDelay;

            foreach (var assetFile in enemyAssetFiles)
            {
                var data = AssetDatabase.LoadAssetAtPath<EnemyData>($"{SoEnemyPath}/{assetFile}");
                if (data != null && !spawner.enemyPool.Contains(data))
                {
                    spawner.enemyPool.Add(data);
                }
            }
        }

        private static void CreateGatheringNode(GameObject parent, string nodeName, Vector3 pos, GatheringNodeType type, string displayName)
        {
            GameObject nodeObj = null;

            // Ưu tiên nạp từ Prefab mẫu nếu có
            string prefabName = (type == GatheringNodeType.HerbBush) ? "Prefab_Gathering_HerbBush.prefab" : "Prefab_Gathering_OreVein.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/{prefabName}");

            if (prefab != null)
            {
                nodeObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                nodeObj.name = nodeName;
                nodeObj.transform.SetParent(parent.transform);
                nodeObj.transform.position = pos;
            }
            else
            {
                nodeObj = new GameObject(nodeName);
                nodeObj.transform.SetParent(parent.transform);
                nodeObj.transform.position = pos;

                var sr = nodeObj.AddComponent<SpriteRenderer>();
                sr.color = (type == GatheringNodeType.HerbBush) ? new Color(0.3f, 0.85f, 0.3f) : new Color(0.7f, 0.7f, 0.85f);
                sr.sortingOrder = 3;

                var gNode = nodeObj.AddComponent<GatheringNode>();
                gNode.nodeType = type;
                gNode.nodeName = displayName;
            }
        }

        private static void SetupPlayerArchitecture()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ForestPrototype/Player_Novice_Tier1.prefab");
                if (playerPrefab != null)
                {
                    player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, EditorSceneManager.GetActiveScene());
                    player.name = "Player_Novice_Tier1";
                    player.tag = "Player";
                    GameObject sp = GameObject.Find("PlayerSpawnPoint");
                    player.transform.position = sp != null ? sp.transform.position : new Vector3(-37f, 0f, 0f);
                }
            }
            if (player == null) return;

            player.name = "Player_Novice_Tier1";
            player.tag = "Player";

            // Đảm bảo gắn đủ kiến trúc Clean Architecture MVP
            if (player.GetComponent<PlayerPresenter>() == null)
            {
                player.AddComponent<PlayerPresenter>();
            }

            if (player.GetComponent<CharacterVisualView>() == null)
            {
                player.AddComponent<CharacterVisualView>();
            }

            if (player.GetComponent<HitFlashEffectView>() == null)
            {
                player.AddComponent<HitFlashEffectView>();
            }

            var cam = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
            if (cam != null)
            {
                cam.target = player.transform;
                cam.offset = new Vector3(0, 0, -10f);
            }

            Debug.Log("[WorldMapSceneBuilder] Đã kết nối thành công Player với PlayerPresenter, CharacterVisualView & HitFlashEffectView.");
        }
    }
}
#endif
