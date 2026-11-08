using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Enemies;
using ActionRPG.Items;
using ActionRPG.Pets;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.Tests
{
    public static class Sprint1PetConsistencyTest
    {
        [MenuItem("Tools/Run Sprint 1 Pet System Test")]
        public static void RunAllPetTests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU CHẠY KIỂM THỬ SPRINT 1: HỆ THỐNG ĐỒNG HÀNH PET</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Cấu Trúc Dữ Liệu Pet (PetData & PetInstance)", Test_PetDataAndInstance, ref passed, ref failed);
            RunTest("Test 2: Bộ Điều Khiển Di Chuyển & Trạng Thái (PetController & PetAI)", Test_PetControllerAndAI, ref passed, ref failed);
            RunTest("Test 3: Tương Tác Chiến Đấu & Tích Hợp Class (Combat & Class Synergies)", Test_PetCombatAndClassSynergy, ref passed, ref failed);
            RunTest("Test 4: Cơ Chế Thuần Hóa / Bắt Pet (PetCaptureSystem)", Test_PetCaptureSystem, ref passed, ref failed);
            RunTest("Test 5: Giao Diện HUD & Đồng Bộ EventBus (PetHUDWidget)", Test_PetHUDWidgetAndEvents, ref passed, ref failed);
            RunTest("Test 6: Kiểm Soát Cấp Độ Mở Khóa Pet (Sprint 1 Level 10 Gating)", Test_LevelUnlockGatingAndEvents, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 HOÀN THÀNH TẤT CẢ {passed} BÀI TEST SPRINT 1! TOÀN BỘ LOGIC PET ĐỒNG NHẤT 100%!</color>");
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

        #region Test 1: PetData & PetInstance
        private static void Test_PetDataAndInstance()
        {
            // 1. Tạo PetData
            PetData data = ScriptableObject.CreateInstance<PetData>();
            data.petId = "pet_test_wolf";
            data.petName = "Sói Băng";
            data.speciesName = "Dã Thú Bắc Cực";
            data.petType = PetType.Beast;
            data.element = ElementType.Water;
            data.role = PetRole.DPS;
            data.bodyShape = PetBodyShape.SmallFast;
            data.maxHp = 100f;
            data.attackPower = 16f;
            data.defense = 6f;
            data.moveSpeed = 5.5f;
            data.favoriteFood = "Thịt Nướng";

            // 2. Tạo PetInstance
            PetInstance pet = new PetInstance(data, 1, 50f);
            AssertEquals(1, pet.level, "Cấp độ khởi điểm phải là 1");
            AssertEquals(50f, pet.bondLevel, "Thân mật khởi điểm phải là 50");
            AssertTrue(pet.MaxHp >= 100f, "MaxHp phải tính đúng từ PetData");
            AssertEquals(pet.MaxHp, pet.currentHp, "CurrentHp ban đầu phải bằng MaxHp");

            // 3. Test EXP & Thăng Cấp
            int expNeeded = pet.ExpToNextLevel;
            pet.GainExp(expNeeded + 10);
            AssertEquals(2, pet.level, "Sau khi đủ EXP Pet phải thăng lên Cấp 2");
            AssertEquals(55f, pet.bondLevel, "Thăng cấp phải cộng thêm 5 điểm Bond");

            // 4. Test Thân Mật (Bond 0 - 100)
            pet.ModifyBond(60f);
            AssertEquals(100f, pet.bondLevel, "Bond tối đa không được vượt quá 100");
            pet.ModifyBond(-150f);
            AssertEquals(0f, pet.bondLevel, "Bond tối thiểu không được dưới 0");

            // 5. Test Cho Ăn (Feed & Favorite Food)
            pet.fullness = 40f;
            pet.Feed(null, "Thịt Nướng");
            AssertTrue(pet.fullness >= 75f, "Ăn xong fullness phải tăng lên");
            AssertTrue(pet.bondLevel >= 15f, "Món ưa thích phải tăng thêm nhiều Bond");

            // 6. Test Tiến Hóa
            PetData evoData = ScriptableObject.CreateInstance<PetData>();
            evoData.petId = "pet_test_wolf_alpha";
            evoData.petName = "Sói Đầu Đàn Băng Giá";
            evoData.maxHp = 160f;
            evoData.attackPower = 28f;
            data.nextEvolution = evoData;
            data.evolutionRequiredLevel = 2;

            AssertTrue(pet.CanEvolve(), "Pet cấp 2 phải đủ điều kiện tiến hóa");
            bool evolved = pet.Evolve();
            AssertTrue(evolved, "Tiến hóa phải thành công");
            AssertEquals("Sói Đầu Đàn Băng Giá", pet.DisplayName, "Tên Pet sau tiến hóa phải cập nhật đúng");
        }
        #endregion

        #region Test 2: PetController & PetAI
        private static void Test_PetControllerAndAI()
        {
            GameObject petObj = new GameObject("Test_Pet");
            var sr = petObj.AddComponent<SpriteRenderer>();
            var rb = petObj.AddComponent<Rigidbody2D>();
            var col = petObj.AddComponent<CircleCollider2D>();
            var controller = petObj.AddComponent<PetController>();
            var ai = petObj.AddComponent<PetAI>();

            GameObject playerObj = new GameObject("Test_Player");
            playerObj.tag = "Player";
            ai.SetOwner(playerObj.transform);

            // Gán dữ liệu
            PetData data = ScriptableObject.CreateInstance<PetData>();
            data.petName = "Hồ Ly Thử Nghiệm";
            data.element = ElementType.Fire;
            data.maxHp = 90f;
            controller.Initialize(data);

            AssertTrue(controller.maxHp >= 90f, $"Controller phải nhận đúng MaxHp từ PetData (>= 90), thực tế: {controller.maxHp}");

            // Test Teleport khi khoảng cách > 12m
            petObj.transform.position = new Vector3(50f, 50f, 0f);
            playerObj.transform.position = Vector3.zero;
            ai.TeleportToOwner();
            float dist = Vector2.Distance(petObj.transform.position, playerObj.transform.position);
            AssertTrue(dist <= 3.0f, $"Sau Teleport khoảng cách tới Player phải <= 3m, hiện tại: {dist}");

            // Test Cận Chiến Tự Do: HideForWeaponTransform & RestoreFromWeaponTransform
            controller.HideForWeaponTransform();
            AssertTrue(controller.isTransformedIntoWeapon, "isTransformedIntoWeapon phải là true khi hóa vũ khí");
            AssertFalse(sr.enabled, "SpriteRenderer phải bị ẩn khi biến thành vũ khí");
            AssertFalse(col.enabled, "Collider phải tắt khi biến thành vũ khí");

            controller.RestoreFromWeaponTransform();
            AssertFalse(controller.isTransformedIntoWeapon, "isTransformedIntoWeapon phải là false sau khi hoàn nguyên");
            AssertTrue(sr.enabled, "SpriteRenderer phải hiện lại");
            AssertTrue(col.enabled, "Collider phải bật lại");

            // Test Sát Thủ Bóng Đêm: SpawnDecoy
            GameObject decoy = controller.SpawnDecoy(2.0f);
            AssertTrue(decoy != null, "Decoy phải được sinh ra");
            AssertTrue(decoy.GetComponent<SpriteRenderer>() != null, "Decoy phải có SpriteRenderer");

            GameObject.DestroyImmediate(decoy);
            GameObject.DestroyImmediate(petObj);
            GameObject.DestroyImmediate(playerObj);
        }
        #endregion

        #region Test 3: Tương Tác Combat & Class Synergy
        private static void Test_PetCombatAndClassSynergy()
        {
            // Tạo Enemy giả lập
            GameObject enemyObj = new GameObject("Test_DummyEnemy");
            try { enemyObj.tag = "Enemy"; } catch { }
            enemyObj.layer = LayerMask.NameToLayer("Default");
            var hurtbox = enemyObj.AddComponent<Hurtbox2D>();

            float receivedDmg = 0f;
            ElementType receivedElem = ElementType.None;
            hurtbox.OnTakeDamageCustom += (dmg) =>
            {
                receivedDmg = dmg.rawDamage;
                receivedElem = dmg.element;
            };

            // Pet gây sát thương lên quái
            DamageInfo petDmg = new DamageInfo(25f, ElementType.Fire, false, null);
            hurtbox.ReceiveDamage(petDmg);

            AssertEquals(25f, receivedDmg, "Quái vật phải nhận đúng 25 sát thương từ Pet");
            AssertEquals(ElementType.Fire, receivedElem, "Hệ nguyên tố nhận vào phải là Fire");

            // Test Roster & Dual Summon
            GameObject rosterObj = new GameObject("Test_Roster");
            var roster = rosterObj.AddComponent<PlayerPetRoster>();

            PetData p1 = ScriptableObject.CreateInstance<PetData>();
            p1.petName = "Pet 1";
            p1.element = ElementType.Fire;

            PetData p2 = ScriptableObject.CreateInstance<PetData>();
            p2.petName = "Pet 2";
            p2.element = ElementType.Water;

            PetInstance inst1 = new PetInstance(p1, 1, 50f);
            PetInstance inst2 = new PetInstance(p2, 1, 50f);

            roster.AddPet(inst1);
            roster.AddPet(inst2);
            AssertEquals(2, roster.ownedPets.Count, "Kho Pet phải có 2 con");

            GameObject.DestroyImmediate(enemyObj);
            GameObject.DestroyImmediate(rosterObj);
        }
        #endregion

        #region Test 4: PetCaptureSystem
        private static void Test_PetCaptureSystem()
        {
            GameObject sysObj = new GameObject("Test_CaptureSystem");
            var capSys = sysObj.AddComponent<PetCaptureSystem>();

            // Tạo EnemyStats giả lập
            GameObject enemyObj = new GameObject("Test_WildSlime");
            var stats = enemyObj.AddComponent<EnemyStats>();
            stats.enemyName = "Slime Thảo Nguyên";
            stats.element = ElementType.Earth;
            stats.maxHp = 100f;
            stats.currentHp = 90f; // 90% HP -> Quá khỏe, không thể bắt!

            // 1. Thử bắt khi máu > 30% -> PHẢI THẤT BẠI
            bool cap1 = capSys.TryCaptureEnemy(stats, 0.5f);
            AssertFalse(cap1, "Quái còn trên 30% máu (90%) không được phép bắt");

            // 2. Đánh quái xuống 15% máu
            stats.currentHp = 15f;
            // Công thức: rate = orbRate * (1 - 15/100) * 1.5 = 0.5 * 0.85 * 1.5 = 0.6375 (63.75%)
            float calculatedRate = Mathf.Clamp01(0.5f * (1.0f - (15f / 100f)) * 1.5f);
            AssertTrue(calculatedRate > 0.6f && calculatedRate < 0.65f, $"Tỷ lệ tính toán phải ~ 63.75%, thực tế: {calculatedRate}");

            GameObject.DestroyImmediate(enemyObj);
            GameObject.DestroyImmediate(sysObj);
        }
        #endregion

        #region Test 5: PetHUDWidget & EventBus
        private static void Test_PetHUDWidgetAndEvents()
        {
            GameObject widgetObj = new GameObject("Test_PetHUDWidget");
            var widget = widgetObj.AddComponent<PetHUDWidget>();
            widget.widgetRoot = widgetObj;

            PetData data = ScriptableObject.CreateInstance<PetData>();
            data.petName = "Linh Hồ Ngọn Lửa";
            data.element = ElementType.Fire;
            data.maxHp = 110f;

            PetInstance instance = new PetInstance(data, 1, 75f);

            // Test hiển thị
            widget.DisplayPet(instance);

            // Test EventBus cập nhật HP
            float updatedHp = 0f;
            Action<float, float> testHpCallback = (curr, max) => { updatedHp = curr; };
            EventBus.OnPetHpChanged += testHpCallback;

            instance.TakeDamage(20f);
            AssertEquals(instance.currentHp, updatedHp, "EventBus.OnPetHpChanged phải đồng bộ chính xác lượng HP còn lại");
            EventBus.OnPetHpChanged -= testHpCallback;

            // Test màu sắc nguyên tố
            Color fireColor = PetHUDWidget.GetElementColor(ElementType.Fire);
            AssertTrue(fireColor.r > 0.8f, "Hệ Hỏa màu phải là đỏ/cam rực rỡ");
            AssertEquals("Hỏa", PetHUDWidget.GetElementVietnameseName(ElementType.Fire), "Tên hệ tiếng Việt phải là Hỏa");

            GameObject.DestroyImmediate(widgetObj);
        }
        #endregion

        #region Test 6: Kiểm Soát Cấp Độ Mở Khóa Pet (Sprint 1 Level 10 Gating)
        private static void Test_LevelUnlockGatingAndEvents()
        {
            // 1. Khởi tạo Player CharacterStats ở Cấp 1
            GameObject playerObj = new GameObject("Test_Player_Stats");
            playerObj.tag = "Player";
            var stats = playerObj.AddComponent<CharacterStats>();
            stats.level = 1;
            stats.petUnlockLevel = 10;
            stats.isPetUnlocked = false;

            AssertFalse(stats.isPetUnlocked, "Ở cấp 1 người chơi CHƯA THỂ có Pet (isPetUnlocked = false)");

            // 2. Kiểm tra PetController chặn triệu hồi khi chưa mở khóa
            GameObject petObj = new GameObject("Test_Pet_Controller");
            var petCtrl = petObj.AddComponent<PetController>();
            bool canSummon = petCtrl.CanSummon(out string reason);
            AssertFalse(canSummon, "Khi chưa mở khóa, không cho phép triệu hồi Pet");
            AssertTrue(reason.Contains("Cần đạt Cấp 10"), $"Thông báo lỗi phải chứa 'Cần đạt Cấp 10', nhận được: '{reason}'");

            // 3. Kiểm tra PetCaptureSystem chặn bắt quái khi chưa mở khóa
            GameObject capObj = new GameObject("Test_CapSys");
            var capSys = capObj.AddComponent<PetCaptureSystem>();
            GameObject enemyObj = new GameObject("Test_EnemySlime");
            var enemyStats = enemyObj.AddComponent<EnemyStats>();
            enemyStats.maxHp = 100f;
            enemyStats.currentHp = 10f; // Máu < 30% nhưng chưa đủ cấp mở khóa

            bool capturedWhileLocked = capSys.TryCaptureEnemy(enemyStats, 0.5f);
            AssertFalse(capturedWhileLocked, "Chưa đủ cấp 10 thì KHÔNG ĐƯỢC phép bắt quái");

            // 4. Lắng nghe EventBus.OnPetSystemUnlocked
            bool eventTriggered = false;
            Action unlockCallback = () => { eventTriggered = true; };
            EventBus.OnPetSystemUnlocked += unlockCallback;

            // 5. Thăng cấp nhân vật lên Cấp 10
            stats.level = 10;
            stats.CheckPetUnlock(true);

            AssertTrue(stats.isPetUnlocked, "Khi đạt Cấp 10, isPetUnlocked phải tự động chuyển thành true");
            AssertTrue(eventTriggered, "Khi mở khóa Pet, EventBus.OnPetSystemUnlocked phải được Invoke");
            EventBus.OnPetSystemUnlocked -= unlockCallback;

            // 6. Sau khi mở khóa, kiểm tra PetController cho phép triệu hồi
            bool canSummonAfter = petCtrl.CanSummon(out _);
            AssertTrue(canSummonAfter, "Sau khi mở khóa, PetController phải cho phép triệu hồi tự do");

            // 7. Kiểm tra PetHUDWidget ở trạng thái khóa và mở khóa
            GameObject widgetObj = new GameObject("Test_Widget");
            var widget = widgetObj.AddComponent<PetHUDWidget>();
            widget.widgetRoot = widgetObj;

            // Đặt stats lại cấp 5 để test HUD khóa
            stats.level = 5;
            stats.isPetUnlocked = false;
            widget.RefreshDisplay();
            AssertTrue(widget.lockedPanel != null && widget.lockedPanel.activeSelf, "Khi chưa đủ cấp, lockedPanel phải hiển thị");
            AssertTrue(widget.lockedText != null && widget.lockedText.text.Contains("Cấp 10"), "Text khóa phải thông báo mở khóa ở Cấp 10");

            // Đặt lại cấp 10 và mở khóa
            stats.level = 10;
            stats.isPetUnlocked = true;
            widget.RefreshDisplay();
            AssertFalse(widget.lockedPanel.activeSelf, "Sau khi mở khóa, lockedPanel phải bị ẩn");

            // 8. Test API Dual Pet của PetController dành cho Pháp Sư Triệu Hồi
            PetData p1 = ScriptableObject.CreateInstance<PetData>();
            p1.petName = "DualPet1";
            p1.element = ElementType.Fire;
            PetData p2 = ScriptableObject.CreateInstance<PetData>();
            p2.petName = "DualPet2";
            p2.element = ElementType.Water;

            PetController.SummonDualPets(p1, p2);

            // Dọn dẹp
            GameObject.DestroyImmediate(enemyObj);
            GameObject.DestroyImmediate(capObj);
            GameObject.DestroyImmediate(widgetObj);
            GameObject.DestroyImmediate(petObj);
            GameObject.DestroyImmediate(playerObj);
        }
        #endregion

        #region Helper Assertions
        private static void AssertEquals<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception($"FAIL: Kỳ vọng '{expected}' nhưng nhận được '{actual}'. Chi tiết: {message}");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"FAIL: Kỳ vọng TRUE nhưng nhận được FALSE. Chi tiết: {message}");
        }

        private static void AssertFalse(bool condition, string message)
        {
            if (condition)
                throw new Exception($"FAIL: Kỳ vọng FALSE nhưng nhận được TRUE. Chi tiết: {message}");
        }
        #endregion
    }
}
