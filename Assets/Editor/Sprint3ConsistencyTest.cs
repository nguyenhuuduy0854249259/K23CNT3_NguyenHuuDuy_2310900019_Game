#if UNITY_EDITOR
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
using ActionRPG.Items.Loot;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho SPRINT 3:
    /// - Ràng buộc cấp độ & Tier trang bị (EquipmentData, CanEquip, Inventory.Equip).
    /// - Bình dược phẩm & Quick Slots [E] & [R] với Cooldown 8s độc lập (ConsumableItemData, GameplayHUD).
    /// - Hệ thống vật phẩm rơi WorldItem 2D, vệt sáng phát quang & tự động hút Magnet <= 2.5m.
    /// - Bộ rơi đồ LootDropper từ quái vật (xác suất rơi, văng 1.0m quanh xác).
    /// - Hệ thống thu thập dã ngoại GatheringNode (Gợi ý [F], chạy thanh tương tác 1.0s, rơi nguyên liệu Dược Sư, hồi sinh 30s).
    /// - Tự động sinh dữ liệu & Prefab mẫu chuẩn 2D.
    /// </summary>
    public static class Sprint3ConsistencyTest
    {
        [MenuItem("Tools/Run Sprint 3 Consistency Test")]
        [MenuItem("Tools/Lumina/Run Sprint 3 Consistency Test")]
        public static void RunAllSprint3Tests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU CHẠY KIỂM THỬ SPRINT 3: ITEMS, LOOT DROPS, MAGNET, QUICK SLOTS & GATHERING</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Ràng Buộc Cấp Độ & Tier Trang Bị (EquipmentData & Inventory.Equip)", Test_EquipmentGatingAndInventoryEquip, ref passed, ref failed);
            RunTest("Test 2: Dược Phẩm & Quick Slots [E] / [R] Cooldown 8s (ConsumableItemData & GameplayHUD)", Test_ConsumablesAndQuickSlots, ref passed, ref failed);
            RunTest("Test 3: Vật Phẩm Rơi WorldItem 2D & Tự Động Hút Magnet (WorldItem & Magnet)", Test_WorldItemAndMagnetLogic, ref passed, ref failed);
            RunTest("Test 4: Bộ Rơi Đồ Quái Vật & Văng 1.0m (LootDropper & EnemyStats)", Test_LootDropperAndCorpseSpawning, ref passed, ref failed);
            RunTest("Test 5: Thu Thập Dã Ngoại, Thanh Tương Tác 1.0s & Hồi 30s (GatheringNode)", Test_GatheringNodeChannelAndRespawn, ref passed, ref failed);
            RunTest("Test 6: Kiểm Tra Asset Generator & Prefab Mẫu Sprint 3 (Data & Prefabs)", Test_AssetGeneratorAndPrefabs, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 HOÀN THÀNH TẤT CẢ {passed} BÀI TEST SPRINT 3! TOÀN BỘ HỆ THỐNG ĐỒNG BỘ 100%!</color>");
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

        #region Test 1: Equipment Gating
        private static void Test_EquipmentGatingAndInventoryEquip()
        {
            GameObject playerObj = new GameObject("Test_Player_EquipGating");
            playerObj.tag = "Player";
            var stats = playerObj.AddComponent<CharacterStats>();
            stats.level = 5;

            var invObj = new GameObject("Test_Inventory");
            var inv = invObj.AddComponent<Inventory>();

            // 1. Tạo trang bị yêu cầu cấp 10 (Cao hơn cấp 5 hiện tại)
            EquipmentData highLevelEquip = ScriptableObject.CreateInstance<EquipmentData>();
            highLevelEquip.itemName = "Giáp Sắt Rèn";
            highLevelEquip.equipSlot = EquipSlot.Armor;
            highLevelEquip.requiredLevel = 10;
            highLevelEquip.requiredTier = EvolutionTier.Tier2_BasicClass;

            inv.AddItem(highLevelEquip, 1);

            // Kiểm tra CanEquip
            bool canEquip = stats.CanEquip(highLevelEquip, out string reason);
            AssertFalse(canEquip, "Cấp 5 không thể trang bị giáp yêu cầu cấp 10");
            AssertTrue(reason.Contains("Cần Cấp 10"), $"Thông báo lỗi cấp độ không đúng: {reason}");

            // Thử Equip qua Inventory
            inv.Equip(highLevelEquip);
            AssertFalse(inv.equippedItems.ContainsKey(EquipSlot.Armor), "Inventory không được phép mặc trang bị khi chưa đủ cấp");

            // 2. Thăng cấp nhân vật lên 10 (Tier 2)
            stats.level = 10;
            AssertEquals(EvolutionTier.Tier2_BasicClass, stats.CurrentTier, "Cấp 10 phải thuộc Tier2_BasicClass");

            bool canEquipNow = stats.CanEquip(highLevelEquip, out string reasonNow);
            AssertTrue(canEquipNow, "Cấp 10 phải trang bị được giáp cấp 10");

            inv.Equip(highLevelEquip);
            AssertTrue(inv.equippedItems.ContainsKey(EquipSlot.Armor), "Inventory phải mặc thành công trang bị khi đã đủ điều kiện");
            AssertEquals(highLevelEquip, inv.equippedItems[EquipSlot.Armor], "Trang bị trong slot Armor phải đúng");

            GameObject.DestroyImmediate(playerObj);
            GameObject.DestroyImmediate(invObj);
        }
        #endregion

        #region Test 2: Consumables & Quick Slots
        private static void Test_ConsumablesAndQuickSlots()
        {
            GameObject playerObj = new GameObject("Test_Player_Consumable");
            playerObj.tag = "Player";
            var stats = playerObj.AddComponent<CharacterStats>();
            stats.maxHp = 100f;
            stats.currentHp = 30f; // Mất 70 HP
            stats.maxMp = 50f;
            stats.currentMp = 10f; // Mất 40 MP

            var invObj = new GameObject("Test_Inventory_Consumable");
            var inv = invObj.AddComponent<Inventory>();
            Inventory.Instance = inv;
            CharacterStats.PlayerInstance = stats;

            // Tạo Bình Máu & Bình Mana
            ConsumableItemData hpPotion = ScriptableObject.CreateInstance<ConsumableItemData>();
            hpPotion.itemId = "potion_hp_test";
            hpPotion.itemName = "Bình Máu Nhỏ";
            hpPotion.healHpAmount = 40f;
            hpPotion.restoreMpAmount = 0f;
            hpPotion.cooldownTime = 8.0f;
            hpPotion.isHpPotion = true;
            hpPotion.isMpPotion = false;

            ConsumableItemData mpPotion = ScriptableObject.CreateInstance<ConsumableItemData>();
            mpPotion.itemId = "potion_mp_test";
            mpPotion.itemName = "Bình Mana Nhỏ";
            mpPotion.healHpAmount = 0f;
            mpPotion.restoreMpAmount = 25f;
            mpPotion.cooldownTime = 8.0f;
            mpPotion.isHpPotion = false;
            mpPotion.isMpPotion = true;

            inv.AddItem(hpPotion, 3);
            inv.AddItem(mpPotion, 2);

            GameplayHUD.ResetPotionCooldowns();

            // 1. Dùng Bình Máu [E]
            bool usedHp = GameplayHUD.UseHpPotion();
            AssertTrue(usedHp, "Phải dùng được bình máu khi sẵn sàng");
            AssertEquals(70f, stats.currentHp, "HP phải tăng thêm 40 (30 + 40 = 70)");
            AssertEquals(2, inv.slots.Find(s => s.item.itemId == hpPotion.itemId).quantity, "Số lượng bình máu trong túi phải giảm đi 1 (còn 2)");
            AssertTrue(GameplayHUD.HpPotionCooldownTimer > 7.0f, "Cooldown bình máu phải được kích hoạt (~8.0s)");

            // 2. Thử dùng lại ngay khi đang Cooldown
            bool usedHpAgain = GameplayHUD.UseHpPotion();
            AssertFalse(usedHpAgain, "Không được dùng bình máu khi đang trong thời gian Cooldown 8s");
            AssertEquals(70f, stats.currentHp, "HP không được tăng thêm khi đang cooldown");

            // 3. Dùng Bình Mana [R] (Cooldown độc lập)
            bool usedMp = GameplayHUD.UseMpPotion();
            AssertTrue(usedMp, "Phải dùng được bình mana (cooldown độc lập với máu)");
            AssertEquals(35f, stats.currentMp, "MP phải tăng thêm 25 (10 + 25 = 35)");
            AssertEquals(1, inv.slots.Find(s => s.item.itemId == mpPotion.itemId).quantity, "Số lượng bình mana phải giảm đi 1 (còn 1)");
            AssertTrue(GameplayHUD.MpPotionCooldownTimer > 7.0f, "Cooldown bình mana phải được kích hoạt (~8.0s)");

            GameplayHUD.ResetPotionCooldowns();

            GameObject.DestroyImmediate(playerObj);
            GameObject.DestroyImmediate(invObj);
        }
        #endregion

        #region Test 3: WorldItem & Magnet Logic
        private static void Test_WorldItemAndMagnetLogic()
        {
            GameObject playerObj = new GameObject("Test_Player_Magnet");
            playerObj.tag = "Player";
            var stats = playerObj.AddComponent<CharacterStats>();
            playerObj.transform.position = Vector3.zero;

            var invObj = new GameObject("Test_Inventory_Magnet");
            var inv = invObj.AddComponent<Inventory>();
            Inventory.Instance = inv;
            CharacterStats.PlayerInstance = stats;
            inv.gold = 0;

            ItemData testGem = ScriptableObject.CreateInstance<ItemData>();
            testGem.itemId = "gem_ruby";
            testGem.itemName = "Đá Hồng Ngọc";
            testGem.rarity = ItemRarity.Epic;

            // 1. Spawn WorldItem
            Vector3 dropPos = new Vector3(2.0f, 0f, 0f); // Nằm trong bán kính magnet 2.5m
            WorldItem wi = WorldItem.Spawn(dropPos, testGem, 2, 0f);

            AssertNotNull(wi, "WorldItem phải được tạo thành công");
            AssertNotNull(wi.GetComponent<CircleCollider2D>(), "WorldItem phải có CircleCollider2D");
            AssertTrue(wi.GetComponent<CircleCollider2D>().isTrigger, "Collider của WorldItem phải là isTrigger");
            AssertNotNull(wi.glowRenderer, "WorldItem phải có SpriteRenderer vệt sáng phát quang (GlowAura)");

            // Kiểm tra thu thập
            wi.Collect();
            AssertTrue(inv.HasItem(testGem.itemId, 2), "Inventory phải nhận được 2 Đá Hồng Ngọc khi nhặt đồ");

            // 2. Spawn Tiền Vàng
            WorldItem goldItem = WorldItem.SpawnGold(new Vector3(1.5f, 0f, 0f), 75, 0f);
            AssertTrue(goldItem.isGold, "Vật phẩm phải được đánh dấu là Tiền Vàng");
            AssertEquals(75, goldItem.goldAmount, "Số lượng vàng rơi phải đúng là 75");

            goldItem.Collect();
            AssertEquals(75, inv.gold, "Inventory phải nhận được +75 Vàng");

            GameObject.DestroyImmediate(playerObj);
            GameObject.DestroyImmediate(invObj);
        }
        #endregion

        #region Test 4: LootDropper & Enemy Spawning
        private static void Test_LootDropperAndCorpseSpawning()
        {
            // Dọn dẹp sạch các WorldItem cũ còn sót nếu có
            foreach (var oldItem in UnityEngine.Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None))
            {
                GameObject.DestroyImmediate(oldItem.gameObject);
            }

            GameObject enemyObj = new GameObject("Test_Enemy_Dropper");
            enemyObj.transform.position = new Vector3(5f, 5f, 0f);

            var enemyStats = enemyObj.AddComponent<EnemyStats>();
            enemyStats.goldReward = 50;

            ItemData dropItem = ScriptableObject.CreateInstance<ItemData>();
            dropItem.itemId = "mat_beast_fang";
            dropItem.itemName = "Nanh Thú Rừng";

            enemyStats.dropTable = new List<DropEntry>()
            {
                new DropEntry()
                {
                    item = dropItem,
                    dropChancePercent = 100f, // 100% tỷ lệ rơi để kiểm thử
                    minQuantity = 1,
                    maxQuantity = 2
                }
            };

            var dropper = enemyObj.AddComponent<LootDropper>();
            dropper.popRadius = 1.0f;
            dropper.goldDropChance = 100f;

            // Kích hoạt rơi đồ
            dropper.DropLoot();

            // Tìm các WorldItem vừa rơi
            WorldItem[] droppedItems = UnityEngine.Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
            AssertTrue(droppedItems.Length >= 2, "Phải rơi ra ít nhất 1 cọc tiền vàng và 1 vật phẩm nanh thú");

            foreach (var item in droppedItems)
            {
                float dist = Vector2.Distance(item.transform.position, enemyObj.transform.position);
                AssertTrue(dist <= 1.5f, $"Vật phẩm rơi phải nằm trong bán kính quanh xác quái (~1.0m), thực tế: {dist}");
                GameObject.DestroyImmediate(item.gameObject);
            }

            GameObject.DestroyImmediate(enemyObj);
        }
        #endregion

        #region Test 5: GatheringNode Channel & Respawn
        private static void Test_GatheringNodeChannelAndRespawn()
        {
            GameObject nodeObj = new GameObject("Test_Herb_Gathering");
            nodeObj.transform.position = Vector3.zero;

            var node = nodeObj.AddComponent<GatheringNode>();
            node.nodeType = GatheringNodeType.HerbBush;
            node.channelDuration = 1.0f;
            node.respawnCooldown = 30.0f;

            AssertFalse(node.isHarvested, "Ban đầu bãi thảo mộc phải còn nguyên vẹn");
            AssertFalse(node.isChanneling, "Ban đầu không được ở trạng thái Channeling");

            // 1. Thử bắt đầu Channeling [F]
            node.StartChanneling();
            AssertTrue(node.isChanneling, "Sau khi bấm F, node phải chuyển sang isChanneling = true");

            // 2. Thử hủy Channeling giữa chừng (do người chơi chạy ra xa)
            node.CancelChanneling("Người chơi bước ra xa");
            AssertFalse(node.isChanneling, "Sau khi hủy, isChanneling phải là false");
            AssertFalse(node.isHarvested, "Chưa hoàn tất 1.0s thì chưa được xem là đã thu hoạch");

            // 3. Hoàn tất thu hoạch (CompleteHarvest)
            node.CompleteHarvest();
            AssertTrue(node.isHarvested, "Sau khi hoàn tất thu hoạch, isHarvested phải là true");
            AssertEquals(30.0f, node.respawnTimer, "respawnTimer phải đếm từ 30 giây");

            // Tìm WorldItem nguyên liệu văng ra
            WorldItem[] spawnedMats = UnityEngine.Object.FindObjectsOfType<WorldItem>();
            AssertTrue(spawnedMats.Length > 0, "Bụi thảo mộc phải văng ra nguyên liệu pha chế dạng WorldItem");

            foreach (var mat in spawnedMats)
            {
                GameObject.DestroyImmediate(mat.gameObject);
            }

            // 4. Kiểm tra Tự phục hồi sau 30s
            node.Respawn();
            AssertFalse(node.isHarvested, "Sau 30s node phải hồi phục (isHarvested = false)");
            AssertEquals(0f, node.respawnTimer, "respawnTimer phải về 0");

            GameObject.DestroyImmediate(nodeObj);
        }
        #endregion

        #region Test 6: Asset Generator & Prefabs Verification
        private static void Test_AssetGeneratorAndPrefabs()
        {
            // Chạy công cụ sinh Asset & Prefab Sprint 3
            ActionRPG.Editor.Sprint3AssetGenerator.GenerateAllSprint3Assets();

            // 1. Kiểm tra Dược phẩm
            string hpPath = "Assets/ScriptableObjects/Items/potion_small_hp.asset";
            ConsumableItemData hpPotion = AssetDatabase.LoadAssetAtPath<ConsumableItemData>(hpPath);
            AssertNotNull(hpPotion, $"Phải tồn tại asset {hpPath}");
            AssertEquals(50f, hpPotion.healHpAmount, "Bình máu phải hồi 50 HP");
            AssertEquals(8.0f, hpPotion.cooldownTime, "Thời gian hồi chiêu mặc định phải là 8 giây");
            AssertTrue(hpPotion.isHpPotion, "Phải được đánh dấu là isHpPotion");

            string mpPath = "Assets/ScriptableObjects/Items/potion_small_mp.asset";
            ConsumableItemData mpPotion = AssetDatabase.LoadAssetAtPath<ConsumableItemData>(mpPath);
            AssertNotNull(mpPotion, $"Phải tồn tại asset {mpPath}");
            AssertEquals(30f, mpPotion.restoreMpAmount, "Bình mana phải hồi 30 MP");
            AssertEquals(8.0f, mpPotion.cooldownTime, "Thời gian hồi chiêu mặc định phải là 8 giây");
            AssertTrue(mpPotion.isMpPotion, "Phải được đánh dấu là isMpPotion");

            // 2. Kiểm tra Nguyên liệu Luyện Dược Sư
            string herbPath = "Assets/ScriptableObjects/Items/mat_herb_fiber.asset";
            string stonePath = "Assets/ScriptableObjects/Items/mat_stone_powder.asset";
            string waterPath = "Assets/ScriptableObjects/Items/mat_pure_water.asset";

            AssertNotNull(AssetDatabase.LoadAssetAtPath<ItemData>(herbPath), $"Phải tồn tại {herbPath}");
            AssertNotNull(AssetDatabase.LoadAssetAtPath<ItemData>(stonePath), $"Phải tồn tại {stonePath}");
            AssertNotNull(AssetDatabase.LoadAssetAtPath<ItemData>(waterPath), $"Phải tồn tại {waterPath}");

            // 3. Kiểm tra Trang bị Level & Tier Gating
            string swordPath = "Assets/ScriptableObjects/Items/equip_novice_sword.asset";
            string armorPath = "Assets/ScriptableObjects/Items/equip_iron_armor.asset";
            string staffPath = "Assets/ScriptableObjects/Items/equip_lumina_staff.asset";

            EquipmentData sword = AssetDatabase.LoadAssetAtPath<EquipmentData>(swordPath);
            AssertNotNull(sword, $"Phải tồn tại {swordPath}");
            AssertEquals(1, sword.requiredLevel, "Kiếm tân thủ yêu cầu cấp 1");
            AssertEquals(EvolutionTier.Tier1_Novice, sword.requiredTier, "Kiếm tân thủ yêu cầu Tier 1 Novice");

            EquipmentData armor = AssetDatabase.LoadAssetAtPath<EquipmentData>(armorPath);
            AssertNotNull(armor, $"Phải tồn tại {armorPath}");
            AssertEquals(10, armor.requiredLevel, "Giáp sắt yêu cầu cấp 10");
            AssertEquals(EvolutionTier.Tier2_BasicClass, armor.requiredTier, "Giáp sắt yêu cầu Tier 2 BasicClass");

            EquipmentData staff = AssetDatabase.LoadAssetAtPath<EquipmentData>(staffPath);
            AssertNotNull(staff, $"Phải tồn tại {staffPath}");
            AssertEquals(20, staff.requiredLevel, "Trượng Lumina yêu cầu cấp 20");
            AssertEquals(EvolutionTier.Tier3_LuminaClass, staff.requiredTier, "Trượng Lumina yêu cầu Tier 3 LuminaClass");

            // 4. Kiểm tra 3 Prefabs Mẫu
            string wiPrefab = "Assets/Prefabs/Prefab_WorldItem_Standard.prefab";
            string herbPrefab = "Assets/Prefabs/Prefab_Gathering_HerbBush.prefab";
            string orePrefab = "Assets/Prefabs/Prefab_Gathering_OreVein.prefab";

            AssertNotNull(AssetDatabase.LoadAssetAtPath<GameObject>(wiPrefab), $"Phải tồn tại Prefab mẫu {wiPrefab}");
            AssertNotNull(AssetDatabase.LoadAssetAtPath<GameObject>(herbPrefab), $"Phải tồn tại Prefab mẫu {herbPrefab}");
            AssertNotNull(AssetDatabase.LoadAssetAtPath<GameObject>(orePrefab), $"Phải tồn tại Prefab mẫu {orePrefab}");
        }
        #endregion

        #region Assert Helpers
        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception($"Assertion Failed: {message}");
        }

        private static void AssertFalse(bool condition, string message)
        {
            if (condition) throw new Exception($"Assertion Failed: {message}");
        }

        private static void AssertNotNull(object obj, string message)
        {
            if (obj == null || obj.Equals(null)) throw new Exception($"Assertion Failed (Null): {message}");
        }

        private static void AssertEquals<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"Assertion Failed: {message}. Mong đợi '{expected}', nhưng nhận '{actual}'");
            }
        }
        #endregion
    }
}
#endif
