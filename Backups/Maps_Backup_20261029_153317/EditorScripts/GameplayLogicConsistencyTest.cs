#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Items;
using ActionRPG.UI.Gameplay;
using ActionRPG.UI.Character;

namespace ActionRPG.Editor
{
    public static class GameplayLogicConsistencyTest
    {
        [MenuItem("Tools/Run Gameplay Logic Consistency Test")]
        public static bool RunAllTests()
        {
            Debug.Log("==========================================================");
            Debug.Log("<color=cyan><b>[LUMINA TEST SUITE] BẮT ĐẦU KIỂM TRA ĐỒNG NHẤT LOGIC GAMEPLAY & HỆ THỐNG GAME</b></color>");
            Debug.Log("==========================================================");

            int passed = 0;
            int failed = 0;

            void Assert(bool condition, string testName, string details = "")
            {
                if (condition)
                {
                    passed++;
                    Debug.Log($"<color=green>[PASS]</color> {testName} {details}");
                }
                else
                {
                    failed++;
                    Debug.LogError($"<color=red>[FAIL]</color> {testName} {details}");
                }
            }

            GameObject testObj = new GameObject("GameplayLogicTestObject");

            try
            {
                // ========================================================
                // TEST 1: CHARACTER STATS & RPG ATTRIBUTES
                // ========================================================
                var stats = testObj.AddComponent<CharacterStats>();
                stats.level = 1;
                stats.maxHp = 100f;
                stats.currentHp = 100f;
                stats.maxMp = 50f;
                stats.currentMp = 50f;
                stats.baseAtk = 20f;
                stats.baseDef = 10f;
                stats.baseMoveSpeed = 5.0f;
                stats.baseCritRate = 0.1f;
                stats.characterElement = ElementType.Fire;

                Assert(stats.TotalAtk == 20f, "Stats - TotalAtk tính đúng baseAtk ban đầu");
                Assert(stats.TotalDef == 10f, "Stats - TotalDef tính đúng baseDef ban đầu");
                Assert(stats.TotalMoveSpeed == 5.0f, "Stats - TotalMoveSpeed tính đúng");
                Assert(Mathf.Approximately(stats.TotalCritRate, 0.1f), "Stats - TotalCritRate tính đúng");
                Assert(stats.TotalStr == 10 && stats.TotalDex == 10 && stats.TotalInt == 10 && stats.TotalVit == 10,
                    "Stats - Thuộc tính RPG STR/DEX/INT/VIT khởi tạo chuẩn cấp 1");

                // Test Mana consumption and HP damage
                bool mpConsumed = stats.ConsumeMp(15f);
                Assert(mpConsumed && Mathf.Approximately(stats.currentMp, 35f), "Stats - Tiêu hao MP đúng khi đủ năng lượng", $"MP còn: {stats.currentMp}");
                bool mpFailed = stats.ConsumeMp(100f);
                Assert(!mpFailed && Mathf.Approximately(stats.currentMp, 35f), "Stats - Chặn tiêu hao MP khi không đủ mana");

                stats.Heal(10f);
                stats.RestoreMp(10f);
                Assert(stats.currentHp == 100f && stats.currentMp == 45f, "Stats - Hồi phục HP và MP chuẩn xác");

                // ========================================================
                // TEST 2: INVENTORY & EQUIPMENT INTEGRATION
                // ========================================================
                var invObj = new GameObject("TestInventory");
                var inv = invObj.AddComponent<Inventory>();

                var testWeapon = ScriptableObject.CreateInstance<EquipmentData>();
                testWeapon.itemName = "Thần Kiếm Lumina";
                testWeapon.equipSlot = EquipSlot.Weapon;
                testWeapon.bonusAtk = 25f;
                testWeapon.bonusCritRate = 0.05f;

                var testArmor = ScriptableObject.CreateInstance<EquipmentData>();
                testArmor.itemName = "Giáp Thánh Hiệp Sĩ";
                testArmor.equipSlot = EquipSlot.Armor;
                testArmor.bonusDef = 15f;
                testArmor.bonusHp = 50f;

                inv.AddItem(testWeapon, 1);
                inv.AddItem(testArmor, 1);
                Assert(inv.HasItem(testWeapon.itemId, 1) && inv.HasItem(testArmor.itemId, 1), "Inventory - Thêm trang bị vào túi thành công");

                inv.Equip(testWeapon);
                inv.Equip(testArmor);
                Assert(inv.equippedItems.ContainsKey(EquipSlot.Weapon) && inv.equippedItems.ContainsKey(EquipSlot.Armor),
                    "Inventory - Trang bị vũ khí và áo giáp thành công");
                Assert(stats.TotalAtk == 45f, "Inventory <-> Stats - TotalAtk tăng đúng +25 từ vũ khí", $"Hiện tại: {stats.TotalAtk}");
                Assert(stats.TotalDef == 25f, "Inventory <-> Stats - TotalDef tăng đúng +15 từ áo giáp", $"Hiện tại: {stats.TotalDef}");
                Assert(Mathf.Approximately(stats.TotalCritRate, 0.15f), "Inventory <-> Stats - TotalCritRate tăng đúng +5% từ vũ khí");

                inv.Unequip(EquipSlot.Weapon);
                Assert(!inv.equippedItems.ContainsKey(EquipSlot.Weapon) && stats.TotalAtk == 20f,
                    "Inventory <-> Stats - Tháo vũ khí khôi phục TotalAtk về gốc");

                // ========================================================
                // TEST 3: CLASSSYSTEM & 10 CHUYÊN PHÁI CƠ CHẾ
                // ========================================================
                var classSystem = testObj.AddComponent<ClassSystem>();
                Assert(classSystem.PetWeapon != null, "ClassSystem - Khởi tạo PetWeaponMechanics thành công");
                Assert(classSystem.ElementalQueue != null, "ClassSystem - Khởi tạo ElementalQueueMechanics thành công");
                Assert(classSystem.SummonerDualPet != null, "ClassSystem - Khởi tạo SummonerDualPetMechanics thành công");
                Assert(classSystem.ChronoMage != null, "ClassSystem - Khởi tạo ChronoMageMechanics thành công");
                Assert(classSystem.HolyDarkPriest != null, "ClassSystem - Khởi tạo HolyDarkPriestMechanics thành công");
                Assert(classSystem.AlchemistCauldron != null, "ClassSystem - Khởi tạo AlchemistCauldronMechanics thành công");
                Assert(classSystem.EngineerGunner != null, "ClassSystem - Khởi tạo EngineerGunnerMechanics thành công");
                Assert(classSystem.ShadowGunner != null, "ClassSystem - Khởi tạo ShadowGunnerMechanics thành công");
                Assert(classSystem.ElementalArcher != null, "ClassSystem - Khởi tạo ElementalArcherMechanics thành công");
                Assert(classSystem.SpiritArcher != null, "ClassSystem - Khởi tạo SpiritArcherMechanics thành công");

                // Test Cơ chế Cận Chiến: Pet hóa vũ khí (11 loại)
                bool transformed = classSystem.PetWeapon.TryTransformWeaponFromPet(PetBodyShape.HeavyLarge, 0, ElementType.Fire);
                Assert(transformed && classSystem.PetWeapon.currentWeapon == MeleeWeaponType.GreatHammer && classSystem.PetWeapon.hasMetamorphosisBuff,
                    "PetWeapon - Pet hóa thành GreatHammer thành công, kích hoạt Ấn Binh Khí Đồng Thể");

                // Test Cơ chế Pháp Sư: Hàng đợi 2 ô & Thiên Tai
                classSystem.ElementalQueue.PushElement(ElementType.Fire);
                classSystem.ElementalQueue.PushElement(ElementType.Water);
                Assert(classSystem.ElementalQueue.IsQueueFull, "ElementalQueue - Hàng đợi 2 ô đầy đủ khi nạp 2 nguyên tố");

                // Test Cơ chế Mục Sư: Toggle Stance Angel <-> Demon
                var initialForm = classSystem.HolyDarkPriest.currentForm;
                classSystem.ToggleStance();
                Assert(classSystem.HolyDarkPriest.currentForm != initialForm, "HolyDarkPriest - Đổi Stance giữa Thiên Thần và Ác Quỷ hoạt động chuẩn");

                // Test Cơ chế Xạ Thủ Súng: Tích lũy nhiệt lượng
                bool shotOk = classSystem.TryShootGun(20f, out float dmgMult);
                Assert(shotOk && classSystem.gunHeat > 0f, "EngineerGunner - Bắn súng tích nhiệt lượng chính xác", $"Nhiệt hiện tại: {classSystem.gunHeat}");

                // Test Cơ chế Xạ Thủ Tinh Linh: 3 Charge
                Assert(classSystem.spiritCharges == 3, "SpiritArcher - Khởi tạo đầy đủ 3 Charge Linh Thể");
                bool spiritSpawned = classSystem.ConsumeSpiritCharge(ElementType.Wind);
                Assert(spiritSpawned && classSystem.spiritCharges == 2, "SpiritArcher - Tiêu hao 1 Charge gọi Linh Thể chuẩn");

                // ========================================================
                // TEST 4: SKILL SYSTEM & COMBAT LOGIC
                // ========================================================
                var testClass = ScriptableObject.CreateInstance<CharacterClassData>();
                testClass.className = "Kiếm Sĩ Nguyên Tố";
                testClass.specialMechanic = SpecialMechanic.PetWeapon;
                testClass.passiveSeal1Name = "Binh Khí Đồng Thể";
                testClass.passiveSeal1Desc = "Đổi Pet biến vũ khí tăng +25% sát thương trong 3s.";

                var testSkill = ScriptableObject.CreateInstance<SkillData>();
                testSkill.skillId = "slash_01";
                testSkill.skillName = "Đoạn Phong Trảm";
                testSkill.manaCost = 15f;
                testSkill.cooldownSeconds = 2.0f;
                testSkill.damageMultiplier = 2.0f;
                testSkill.targetType = SkillTargetType.DirectMelee;
                testClass.unlockedSkills.Add(testSkill);

                classSystem.PromoteTo(testClass);
                Assert(classSystem.currentClass == testClass, "ClassSystem - Chuyển sang Kiếm Sĩ Nguyên Tố thành công");

                // ========================================================
                // TEST 5: UI DATA BINDING & EVENT PROPAGATION
                // ========================================================
                var hudObj = new GameObject("TestHUD");
                var hud = hudObj.AddComponent<GameplayHUD>();

                var charPanelObj = new GameObject("TestCharPanel");
                var charUI = charPanelObj.AddComponent<CharacterUI>();
                charUI.panelRoot = charPanelObj;
                charUI.statsUI = charPanelObj.AddComponent<CharacterStatsUI>();
                charUI.equipmentUI = charPanelObj.AddComponent<EquipmentUI>();
                charUI.skillsUI = charPanelObj.AddComponent<SkillsUI>();
                charUI.talentUI = charPanelObj.AddComponent<TalentUI>();

                // Test opening/closing
                charUI.Open();
                Assert(charPanelObj.activeSelf, "CharacterUI - Mở giao diện nhân vật thành công");
                charUI.Close();
                Assert(!charPanelObj.activeSelf, "CharacterUI - Đóng giao diện nhân vật thành công");

                charUI.RefreshAll();
                Assert(true, "CharacterUI - RefreshAll() chạy mượt mà, không gặp NullReferenceException");

                // ========================================================
                // TEST 6: DAMAGE CALCULATION & DEFENSE MITIGATION
                // ========================================================
                // Physical Attack vs Physical Defense (mitigation = pDef * 0.6f)
                // 100 raw dmg vs 50 pDef -> defReduction = 30 -> 70 dmg
                float physDmg = ActionRPG.Core.Services.DamageCalculationService.CalculateFinalDamage(100f, DamageType.Physical, 50f, 20f);
                Assert(Mathf.Approximately(physDmg, 70f), "Combat - Sát thương vật lý trừ vào giáp vật lý (100 dmg vs 50 pDef -> 70 dmg)");

                // Magic Attack vs Magic Defense (mitigation = mDef * 0.6f)
                // 100 raw dmg vs 40 mDef -> defReduction = 24 -> 76 dmg
                float magDmg = ActionRPG.Core.Services.DamageCalculationService.CalculateFinalDamage(100f, DamageType.Magical, 50f, 40f);
                Assert(Mathf.Approximately(magDmg, 76f), "Combat - Sát thương phép trừ vào giáp phép (100 dmg vs 40 mDef -> 76 dmg)");

                // True Damage ignores both defenses
                float trueDmg = ActionRPG.Core.Services.DamageCalculationService.CalculateFinalDamage(100f, DamageType.TrueDamage, 80f, 80f);
                Assert(Mathf.Approximately(trueDmg, 100f), "Combat - Sát thương chuẩn bỏ qua toàn bộ giáp vật lý và giáp phép");

                // High defense clamps to min damage (1.0f)
                float clampedDmg = ActionRPG.Core.Services.DamageCalculationService.CalculateFinalDamage(10f, DamageType.Physical, 100f, 0f);
                Assert(Mathf.Approximately(clampedDmg, 1.0f), "Combat - Sát thương sau khi trừ giáp luôn được giới hạn tối thiểu 1.0f");

                // Cleanup test objects
                GameObject.DestroyImmediate(testObj);
                GameObject.DestroyImmediate(invObj);
                GameObject.DestroyImmediate(hudObj);
                GameObject.DestroyImmediate(charPanelObj);
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"<color=red>[EXCEPTION IN TEST]</color> {ex.Message}\n{ex.StackTrace}");
            }

            Debug.Log("==========================================================");
            if (failed == 0)
            {
                Debug.Log($"<color=green><b>[KẾT QUẢ KIỂM TRA] TOÀN BỘ {passed} BÀI TEST ĐÃ VƯỢT QUA! LOGIC GAMEPLAY ĐỒNG NHẤT 100% VỚI HỆ THỐNG GAME.</b></color>");
            }
            else
            {
                Debug.LogError($"<color=red><b>[KẾT QUẢ KIỂM TRA] CÓ {failed} BÀI TEST THẤT BẠI TRÊN TỔNG SỐ {passed + failed} BÀI KIỂM TRA!</b></color>");
            }
            Debug.Log("==========================================================");

            return failed == 0;
        }
    }
}
#endif
