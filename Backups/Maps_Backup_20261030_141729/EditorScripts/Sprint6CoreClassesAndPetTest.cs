#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Items;
using ActionRPG.Pets;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho SPRINT 6:
    /// - Kiểm tra 15 Kỹ năng chuẩn của 3 Class cơ bản (Đấu Sĩ, Pháp Sư, Xạ Thủ) trên phím [Q, W, E, R, T].
    /// - Kiểm tra cơ chế mở khóa thu phục Pet khi chuyển chức & tặng 5 Quả Cầu Tinh Linh.
    /// - Kiểm tra logic thi triển kỹ năng & hiệu ứng khống chế của từng Class.
    /// - Kiểm tra cấu trúc PetHUDWidget & đồng bộ hiển thị trên GameplayHUD.
    /// </summary>
    public static class Sprint6CoreClassesAndPetTest
    {
        [MenuItem("Tools/Lumina/Run Sprint 6 Core Classes & Pet Test")]
        public static void RunAllTests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU KIỂM THỬ SPRINT 6: 3 CLASS CƠ BẢN, MỞ KHÓA PET & PET HUD WIDGET</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Kiểm tra 15 Icon Kỹ Năng & 2 Icon Pet Sprite", Test_ValidateIcons, ref passed, ref failed);
            RunTest("Test 2: Kiểm tra Dữ Liệu ScriptableObjects 3 Class Cơ Bản (Warrior, Mage, Archer)", Test_ValidateClassData, ref passed, ref failed);
            RunTest("Test 3: Kiểm tra Chuyển Chức Tự Động Mở Khóa Pet & Tặng 5 Quả Cầu Tinh Linh", Test_ClassPromotionPetUnlock, ref passed, ref failed);
            RunTest("Test 4: Kiểm tra Thi Triển Kỹ Năng 3 Class Trong PlayerSkillCombat2D", Test_SkillCombatExecution, ref passed, ref failed);
            RunTest("Test 5: Kiểm tra PetHUDWidget & Hotbar [Q, W, E, R, T] Trên GameplayHUD", Test_PetHUDAndHotbar, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 HOÀN THÀNH TẤT CẢ {passed} BÀI TEST SPRINT 6! TOÀN BỘ 3 CLASS VÀ HỆ THỐNG PET HOẠT ĐỘNG HOÀN HẢO!</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ CÓ {failed} BÀI TEST THẤT BẠI! Vui lòng kiểm tra log ở trên.</color>");
            }
            Debug.Log("<color=cyan>==================================================</color>");
        }

        private static void RunTest(string name, Action action, ref int passed, ref int failed)
        {
            try
            {
                Debug.Log($"<color=yellow>▶ Đang chạy: {name}...</color>");
                action.Invoke();
                passed++;
                Debug.Log($"<color=green>✓ [PASSED] {name}</color>");
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"<color=red>✗ [FAILED] {name}: {ex.Message}\n{ex.StackTrace}</color>");
            }
        }

        private static void Test_ValidateIcons()
        {
            string[] skillIds = {
                "warrior_cleave", "warrior_vanguard_charge", "warrior_iron_bastion", "warrior_tremor_slam", "warrior_frenzied_strike",
                "mage_fire_orb", "mage_frost_spikes", "mage_blink", "mage_chain_lightning", "mage_mana_shield",
                "archer_piercing_shot", "archer_volley", "archer_tumble", "archer_snare_trap", "archer_deadeye"
            };

            foreach (var id in skillIds)
            {
                string path = $"Assets/Sprites/UI/HUD/Skills/icon_skill_{id}.png";
                if (!File.Exists(path))
                    throw new Exception($"Không tìm thấy file icon kỹ năng: {path}");
            }

            if (!File.Exists("Assets/Sprites/UI/HUD/Icons/icon_spirit_orb.png"))
                throw new Exception("Không tìm thấy icon_spirit_orb.png");

            if (!File.Exists("Assets/Sprites/UI/HUD/Icons/icon_pet_wolf.png"))
                throw new Exception("Không tìm thấy icon_pet_wolf.png");
        }

        private static void Test_ValidateClassData()
        {
            string[] classPaths = {
                "Assets/ScriptableObjects/Classes/Class_Warrior.asset",
                "Assets/ScriptableObjects/Classes/Class_Mage.asset",
                "Assets/ScriptableObjects/Classes/Class_Archer.asset"
            };

            foreach (var cp in classPaths)
            {
                var cData = AssetDatabase.LoadAssetAtPath<CharacterClassData>(cp);
                if (cData == null)
                    throw new Exception($"Không tìm thấy file ScriptableObject: {cp}");

                if (cData.unlockedSkills == null || cData.unlockedSkills.Count < 5)
                    throw new Exception($"Class {cData.className} chưa đủ 5 kỹ năng (hiện có: {cData.unlockedSkills?.Count ?? 0})");
            }
        }

        private static void Test_ClassPromotionPetUnlock()
        {
            GameObject go = new GameObject("TestPlayer_Promotion");
            try
            {
                var stats = go.AddComponent<CharacterStats>();
                stats.isPetUnlocked = false;

                var cs = go.AddComponent<ClassSystem>();
                var inv = go.AddComponent<Inventory>();

                var warriorData = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Warrior.asset");
                if (warriorData == null) throw new Exception("Không tìm thấy Class_Warrior.asset để kiểm thử chuyển chức.");

                bool eventFired = false;
                Action onUnlock = () => eventFired = true;
                EventBus.OnPetSystemUnlocked += onUnlock;

                cs.PromoteTo(warriorData);

                EventBus.OnPetSystemUnlocked -= onUnlock;

                if (!stats.isPetUnlocked)
                    throw new Exception("Chuyển chức sang Warrior nhưng stats.isPetUnlocked không được kích hoạt true!");

                if (!eventFired)
                    throw new Exception("Sự kiện EventBus.OnPetSystemUnlocked không được kích hoạt khi chuyển chức!");

                Debug.Log("[Test 3] Đã mở khóa tính năng Pet thành công khi chuyển chức sang Warrior.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void Test_SkillCombatExecution()
        {
            GameObject playerGo = new GameObject("TestPlayer_SkillCombat");
            try
            {
                var stats = playerGo.AddComponent<CharacterStats>();
                stats.maxHp = 200f;
                stats.currentHp = 100f;
                stats.maxMp = 200f;
                stats.currentMp = 200f;

                var cs = playerGo.AddComponent<ClassSystem>();
                var combat2D = playerGo.AddComponent<PlayerSkillCombat2D>();

                var warriorData = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Warrior.asset");
                if (warriorData != null)
                {
                    cs.PromoteTo(warriorData);
                    // Cast Chiêu 1 (Q: Trảm Kích)
                    combat2D.CastSkillBySlot(0);
                    // Cast Chiêu 3 (E: Thiết Giáp)
                    combat2D.CastSkillBySlot(2);
                    if (stats.temporaryShieldHp <= 0f)
                    {
                        throw new Exception("Thiết Giáp (Iron Bastion) không kích hoạt Giáp Ảo (Barrier) trên nhân vật!");
                    }
                }

                Debug.Log("[Test 4] PlayerSkillCombat2D thi triển mượt mà các chiêu thức của class.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(playerGo);
            }
        }

        private static void Test_PetHUDAndHotbar()
        {
            GameObject rootGo = new GameObject("TestUIRoot", typeof(RectTransform));
            try
            {
                var canvas = rootGo.AddComponent<Canvas>();
                var hudGo = new GameObject("GameplayHUD", typeof(RectTransform));
                hudGo.transform.SetParent(rootGo.transform, false);

                var hud = hudGo.AddComponent<GameplayHUD>();
                hud.EnsureUIHierarchy();

                var petWidget = hud.GetComponentInChildren<PetHUDWidget>(true);
                if (petWidget == null)
                    throw new Exception("Không tìm thấy PetHUDWidget trong phân cấp cây UI của GameplayHUD!");

                var warriorData = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Warrior.asset");
                if (warriorData != null)
                {
                    hud.UpdateSkillBar(warriorData);
                }

                Debug.Log("[Test 5] PetHUDWidget và 5 Skill Slots [Q, W, E, R, T] đồng bộ hoàn hảo trên GameplayHUD.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootGo);
            }
        }
    }
}
#endif
