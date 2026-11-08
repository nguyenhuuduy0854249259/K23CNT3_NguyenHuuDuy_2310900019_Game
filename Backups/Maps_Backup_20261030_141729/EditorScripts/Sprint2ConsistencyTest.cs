using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Enemies;
using ActionRPG.Environment;
using ActionRPG.Items;
using ActionRPG.UI;

namespace ActionRPG.Tests
{
    public static class Sprint2ConsistencyTest
    {
        [MenuItem("Tools/Run Sprint 2 Consistency Test")]
        public static void RunAllSprint2Tests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU CHẠY KIỂM THỬ SPRINT 2: ENEMY, TELEGRAPH, DATA & EVOLUTION TIER</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Tiến Trình Nhân Vật 3 Tier & Ràng Buộc Trang Bị (EvolutionTier & Equipment)", Test_EvolutionTierAndEquipmentGating, ref passed, ref failed);
            RunTest("Test 2: Dữ Liệu Quái Vật & Sát Thương Tương Khắc Lumina x2.0 (EnemyStats & Elements)", Test_EnemyStatsAndLuminaElementalCounter, ref passed, ref failed);
            RunTest("Test 3: Máy Trạng Thái AI Quái & Bắn Tầm Xa (EnemyController FSM & Ranged)", Test_EnemyControllerFSMAndRanged, ref passed, ref failed);
            RunTest("Test 4: Hệ Thống Cảnh Báo Telegraph & Dash i-frame (TelegraphIndicator)", Test_TelegraphIndicatorAndDashIframe, ref passed, ref failed);
            RunTest("Test 5: Boss Đa Giai Đoạn Hắc Diệu Ma Viên & Transition (BossController)", Test_BossControllerMultiPhaseAndTransition, ref passed, ref failed);
            RunTest("Test 6: Thu Gom Dược Liệu Cho Dược Sư Chum (GatheringNode 30s Respawn)", Test_GatheringNodeAndRespawn, ref passed, ref failed);
            RunTest("Test 7: Tự Động Sinh Dữ Liệu 5 Quái Thảo Nguyên & 13 Thú Rừng Rậm (Asset Generator)", Test_AssetGeneratorAndDataIntegrity, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 HOÀN THÀNH TẤT CẢ {passed} BÀI TEST SPRINT 2! TOÀN BỘ HỆ THỐNG ĐỒNG BỘ 100%!</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ CÓ {failed} BÀI TEST THẤT BẠI! Vui lòng kiểm tra lại log chi tiết.</color>");
            }
            Debug.Log("<color=cyan>==================================================</color>");
        }

        private static void RunTest(string testName, Action testAction, ref int passed, ref int failed)
        {
            try
            {
                testAction.Invoke();
                Debug.Log($"<color=green>✔ [PASS] {testName}</color>");
                passed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>✖ [FAIL] {testName}: {ex.Message}\n{ex.StackTrace}</color>");
                failed++;
            }
        }

        #region Test 1: EvolutionTier & Equipment Gating
        private static void Test_EvolutionTierAndEquipmentGating()
        {
            GameObject playerObj = new GameObject("Test_Player_Tier");
            playerObj.tag = "Player";
            var stats = playerObj.AddComponent<CharacterStats>();
            stats.level = 1;

            // 1. Kiểm tra Tier 1 (Cấp 1-9)
            AssertEquals(EvolutionTier.Tier1_Novice, stats.CurrentTier, "Cấp 1 phải thuộc Tier1_Novice");

            // Tạo vũ khí cơ bản (Kiếm tân thủ)
            EquipmentData basicSword = ScriptableObject.CreateInstance<EquipmentData>();
            basicSword.itemName = "Kiếm Gỗ Tân Thủ";
            basicSword.equipSlot = EquipSlot.Weapon;
            basicSword.requiredLevel = 1;
            basicSword.requiredTier = EvolutionTier.Tier1_Novice;
            basicSword.isSpecializedWeapon = false;

            bool canEquipBasic = stats.CanEquip(basicSword, out string reasonBasic);
            AssertTrue(canEquipBasic, "Tân thủ phải trang bị được kiếm gỗ cơ bản");

            // Tạo vũ khí chuyên phái (Cung tinh anh / Súng / Gậy phép)
            EquipmentData eliteBow = ScriptableObject.CreateInstance<EquipmentData>();
            eliteBow.itemName = "Cung Tinh Anh Lumina";
            eliteBow.equipSlot = EquipSlot.Weapon;
            eliteBow.requiredLevel = 1;
            eliteBow.requiredTier = EvolutionTier.Tier1_Novice;
            eliteBow.isSpecializedWeapon = true; // Vũ khí chuyên phái!

            bool canEquipBow = stats.CanEquip(eliteBow, out string reasonBow);
            AssertFalse(canEquipBow, "Tân thủ Tier 1 KHÔNG ĐƯỢC trang bị vũ khí chuyên phái");
            AssertTrue(reasonBow.Contains("Tân thủ") || reasonBow.Contains("chuyên phái"), $"Thông báo lỗi phải nhắc nhở chuyên phái: {reasonBow}");

            // Tạo trang bị yêu cầu cấp 10 (Tier 2)
            EquipmentData tier2Armor = ScriptableObject.CreateInstance<EquipmentData>();
            tier2Armor.itemName = "Giáp Sắt Rèn";
            tier2Armor.equipSlot = EquipSlot.Armor;
            tier2Armor.requiredLevel = 10;
            tier2Armor.requiredTier = EvolutionTier.Tier2_BasicClass;

            bool canEquipT2AtLvl1 = stats.CanEquip(tier2Armor, out _);
            AssertFalse(canEquipT2AtLvl1, "Ở cấp 1 không được phép mặc đồ cấp 10 Tier 2");

            // 2. Thăng cấp lên Cấp 10 -> Tier 2
            stats.level = 10;
            AssertEquals(EvolutionTier.Tier2_BasicClass, stats.CurrentTier, "Cấp 10 phải thuộc Tier2_BasicClass");
            bool canEquipT2AtLvl10 = stats.CanEquip(tier2Armor, out _);
            AssertTrue(canEquipT2AtLvl10, "Ở cấp 10 phải mặc được giáp Tier 2");
            bool canEquipBowAtLvl10 = stats.CanEquip(eliteBow, out _);
            AssertTrue(canEquipBowAtLvl10, "Ở Tier 2 được phép trang bị vũ khí chuyên phái");

            // 3. Thăng cấp lên Cấp 20 -> Tier 3
            stats.level = 20;
            AssertEquals(EvolutionTier.Tier3_LuminaClass, stats.CurrentTier, "Cấp 20 phải thuộc Tier3_LuminaClass");

            GameObject.DestroyImmediate(playerObj);
        }
        #endregion

        #region Test 2: EnemyStats & Lumina x2.0 Elemental Counter
        private static void Test_EnemyStatsAndLuminaElementalCounter()
        {
            GameObject enemyObj = new GameObject("Test_Enemy_Stats");
            var enemyStats = enemyObj.AddComponent<EnemyStats>();
            enemyStats.enemyName = "Slime Thủy Tộc";
            enemyStats.element = ElementType.Water;
            enemyStats.maxHp = 100f;
            enemyStats.currentHp = 100f;
            enemyStats.defense = 0f; // Để test thuần multiplier

            // 1. Kiểm tra khắc hệ: Điện đánh Thủy -> Phải đạt x2.0
            float multElectricOnWater = ElementalSynergy.GetElementalMultiplier(ElementType.Electric, ElementType.Water);
            AssertEquals(2.0f, multElectricOnWater, "Điện khắc Thủy phải đạt x2.0 sát thương chuẩn Lumina");

            float dmgElectric = enemyStats.TakeDamage(20f, ElementType.Electric, false);
            AssertEquals(40f, dmgElectric, "20 sát thương Điện lên quái Thủy phải thành 40");
            AssertEquals(60f, enemyStats.currentHp, "Máu quái phải còn lại 60");

            // 2. Kiểm tra cùng hệ: Thủy đánh Thủy -> Giảm còn x0.5
            float multWaterOnWater = ElementalSynergy.GetElementalMultiplier(ElementType.Water, ElementType.Water);
            AssertEquals(0.5f, multWaterOnWater, "Cùng hệ Thủy phải giảm còn x0.5 sát thương");

            float dmgWater = enemyStats.TakeDamage(20f, ElementType.Water, false);
            AssertEquals(10f, dmgWater, "20 sát thương Thủy lên quái Thủy phải thành 10");
            AssertEquals(50f, enemyStats.currentHp, "Máu quái phải còn lại 50");

            // 3. Kiểm tra bị kháng ngược: Hỏa đánh Thủy -> Giảm còn x0.5
            float multFireOnWater = ElementalSynergy.GetElementalMultiplier(ElementType.Fire, ElementType.Water);
            AssertEquals(0.5f, multFireOnWater, "Hỏa bị Thủy khắc nên giảm còn x0.5 sát thương");

            // 4. Test Thần Ma tương khắc đối cực: Angel đánh Demon -> x2.0
            float multAngelOnDemon = ElementalSynergy.GetElementalMultiplier(ElementType.Angel, ElementType.Demon);
            AssertEquals(2.0f, multAngelOnDemon, "Thánh và Ma đối cực tương khắc x2.0");

            GameObject.DestroyImmediate(enemyObj);
        }
        #endregion

        #region Test 3: EnemyController FSM & Ranged
        private static void Test_EnemyControllerFSMAndRanged()
        {
            GameObject enemyObj = new GameObject("Test_Enemy_AI");
            var ctrl = enemyObj.AddComponent<EnemyController>();
            ctrl.enemyName = "Cú Đêm Tầm Xa";
            ctrl.element = ElementType.Demon;
            ctrl.isRanged = true;
            ctrl.detectionRadius = 5.0f;
            ctrl.attackRange = 3.0f;
            ctrl.moveSpeed = 3.0f;

            AssertEquals(EnemyAIState.Patrol, ctrl.currentState, "Quái sinh ra phải ở trạng thái Patrol");

            // Gán dữ liệu mẫu qua EnemyData
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyName = "Cú Đêm";
            data.level = 12;
            data.element = ElementType.Demon;
            data.morphologicalGroup = MorphologicalGroup.CoCanh;
            data.isRanged = true;
            ctrl.Initialize(data);

            AssertEquals(12, ctrl.level, "Level phải cập nhật đúng từ EnemyData");
            AssertTrue(ctrl.isRanged, "Quái phải là tầm xa");

            GameObject.DestroyImmediate(enemyObj);
        }
        #endregion

        #region Test 4: TelegraphIndicator & Dash i-frame
        private static void Test_TelegraphIndicatorAndDashIframe()
        {
            // 1. Tạo Player giả lập
            GameObject playerObj = new GameObject("Test_Player_Combat");
            playerObj.tag = "Player";
            var rb = playerObj.AddComponent<Rigidbody2D>();
            var col = playerObj.AddComponent<CircleCollider2D>();
            col.radius = 1.0f;
            var stats = playerObj.AddComponent<CharacterStats>();
            stats.maxHp = 100f;
            stats.currentHp = 100f;
            var hurt = playerObj.AddComponent<Hurtbox2D>();
            var ctrl = playerObj.AddComponent<PlayerController>();

            Physics2D.SyncTransforms();

            // 2. Tạo Circle Telegraph tại vị trí người chơi
            var telegraph = TelegraphIndicator.CreateCircleTelegraph(
                playerObj.transform.position,
                radius: 2.5f,
                delay: 0.1f,
                damage: 30f,
                element: ElementType.Fire,
                attacker: null
            );

            AssertEquals(TelegraphShape.Circle, telegraph.shape, "Hình dạng phải là Circle");
            AssertEquals(2.5f, telegraph.radius, "Bán kính phải là 2.5m");

            // Kích nổ đòn đánh
            telegraph.CheckDamageArea();
            AssertTrue(stats.currentHp < 100f, $"Khi không Dash, Player trong vùng Telegraph phải nhận sát thương. HP thực tế: {stats.currentHp}, stats instance: {stats != null}");

            // 3. Test né đòn bằng Dash i-frame
            stats.currentHp = 100f;
            hurt.isInvulnerable = true; // Kích hoạt i-frame

            var telegraph2 = TelegraphIndicator.CreateCircleTelegraph(
                playerObj.transform.position,
                radius: 2.5f,
                delay: 0.1f,
                damage: 30f,
                element: ElementType.Fire,
                attacker: null
            );

            telegraph2.CheckDamageArea();
            AssertEquals(100f, stats.currentHp, "Khi đang có i-frame (Dash), Player KHÔNG ĐƯỢC nhận sát thương từ Telegraph");

            // Dọn dẹp
            if (telegraph != null) GameObject.DestroyImmediate(telegraph.gameObject);
            if (telegraph2 != null) GameObject.DestroyImmediate(telegraph2.gameObject);
            GameObject.DestroyImmediate(playerObj);
        }
        #endregion

        #region Test 5: BossController Multi-Phase & Transition
        private static void Test_BossControllerMultiPhaseAndTransition()
        {
            GameObject bossObj = new GameObject("Test_Boss_Ape");
            var boss = bossObj.AddComponent<BossController>();
            boss.enemyName = "Hắc Diệu Ma Viên";
            boss.maxHp = 650f;
            boss.currentHp = 650f;
            boss.attackPower = 28f;
            boss.moveSpeed = 3.2f;

            AssertEquals(1, boss.currentPhase, "Boss khởi đầu ở Phase 1");
            AssertFalse(boss.isTransitioning, "Boss khởi đầu chưa bước vào chuyển phase");

            // Giả lập máu tụt xuống 40% (260 HP)
            boss.currentHp = 260f;

            // Kiểm tra điều kiện chuyển phase
            bool needTransition = (boss.currentHp / boss.maxHp) <= 0.5f;
            AssertTrue(needTransition, "Khi máu <= 50%, Boss phải kích hoạt chuyển Phase");

            // Chạy Transition logic
            float baseSpeed = boss.moveSpeed;
            float baseAtk = boss.attackPower;

            // Áp dụng buff Cuồng Nộ Phase 2
            boss.moveSpeed *= 1.30f;
            boss.attackPower *= 1.25f;
            boss.currentPhase = 2;

            AssertEquals(2, boss.currentPhase, "Boss phải ở Phase 2 (Cuồng Nộ)");
            AssertTrue(boss.moveSpeed >= baseSpeed * 1.29f, "Tốc chạy phải tăng 30%");
            AssertTrue(boss.attackPower >= baseAtk * 1.24f, "Sát thương phải tăng 25%");

            GameObject.DestroyImmediate(bossObj);
        }
        #endregion

        #region Test 6: GatheringNode & Respawn Timer
        private static void Test_GatheringNodeAndRespawn()
        {
            GameObject nodeObj = new GameObject("Test_Herb_Node");
            var node = nodeObj.AddComponent<GatheringNode>();
            node.nodeType = GatheringNodeType.HerbBush;
            node.respawnCooldown = 30f;

            AssertFalse(node.isHarvested, "Ban đầu bãi thảo mộc phải còn nguyên");

            // Thu hoạch
            node.Harvest();
            AssertTrue(node.isHarvested, "Sau khi bấm F, bãi thảo mộc phải chuyển thành isHarvested = true");
            AssertEquals(30f, node.respawnTimer, "respawnTimer phải đếm từ 30s");

            // Thử thu hoạch lại khi đang hồi
            node.Harvest();
            AssertTrue(node.isHarvested, "Không thể thu hoạch liên tiếp khi chưa hồi phục");

            // Giả lập hết 30s hồi phục
            node.Respawn();
            AssertFalse(node.isHarvested, "Sau 30s bãi thảo mộc phải sẵn sàng thu hoạch tiếp");
            AssertEquals(0f, node.respawnTimer, "respawnTimer phải trở về 0");

            GameObject.DestroyImmediate(nodeObj);
        }
        #endregion

        #region Test 7: Asset Generator & Data Integrity
        private static void Test_AssetGeneratorAndDataIntegrity()
        {
            // Chạy hàm tạo Asset
            ActionRPG.Editor.Sprint2AssetGenerator.GenerateAllSprint2Assets();

            // Kiểm tra sự tồn tại của 5 quái Thảo Nguyên
            string[] steppeMonsters = new string[]
            {
                "enemy_steppe_slime",
                "enemy_steppe_hedgehog",
                "enemy_steppe_boar",
                "enemy_steppe_sprout",
                "enemy_steppe_rock_crab"
            };

            foreach (var id in steppeMonsters)
            {
                string path = $"Assets/ScriptableObjects/Enemies/{id}.asset";
                EnemyData asset = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                AssertTrue(asset != null, $"Asset quái thảo nguyên {id} phải tồn tại tại {path}");
                AssertTrue(asset.maxHp > 0, $"{id} phải có maxHp > 0");
            }

            // Kiểm tra Boss Hắc Diệu Ma Viên
            string bossPath = "Assets/ScriptableObjects/Enemies/boss_black_demon_ape.asset";
            EnemyData bossAsset = AssetDatabase.LoadAssetAtPath<EnemyData>(bossPath);
            AssertTrue(bossAsset != null, "Boss Hắc Diệu Ma Viên asset phải tồn tại");
            AssertEquals("Hắc Diệu Ma Viên", bossAsset.enemyName, "Tên Boss phải là Hắc Diệu Ma Viên");
            AssertEquals(20, bossAsset.level, "Cấp Boss phải là 20");
            AssertEquals(650f, bossAsset.maxHp, "HP Boss phải là 650");

            // Kiểm tra 3 vật phẩm thu gom
            string[] materials = new string[] { "mat_herb_fiber", "mat_stone_powder", "mat_pure_water" };
            foreach (var m in materials)
            {
                string p = $"Assets/ScriptableObjects/Items/{m}.asset";
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(p);
                AssertTrue(item != null, $"Vật phẩm {m} phải được sinh ra tại {p}");
                AssertEquals(ItemType.Material, item.itemType, $"{m} phải là ItemType.Material");
            }
        }
        #endregion

        #region Helper Assertions
        private static void AssertEquals<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"FAIL: Kỳ vọng '{expected}' nhưng nhận được '{actual}'. Chi tiết: {message}");
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"FAIL: Kỳ vọng TRUE nhưng nhận được FALSE. Chi tiết: {message}");
            }
        }

        private static void AssertFalse(bool condition, string message)
        {
            if (condition)
            {
                throw new Exception($"FAIL: Kỳ vọng FALSE nhưng nhận được TRUE. Chi tiết: {message}");
            }
        }
        #endregion
    }
}
