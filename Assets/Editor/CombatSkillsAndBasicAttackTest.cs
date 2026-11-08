using System;
using UnityEngine;
using UnityEditor;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Presentation.Views;

namespace ActionRPG.EditorTests
{
    public static class CombatSkillsAndBasicAttackTest
    {
        [MenuItem("Tools/Lumina/Chạy Kiểm Thử Đòn Đánh Thường & Kỹ Năng 1-6")]
        public static void RunAllTests()
        {
            Debug.Log("==================================================================");
            Debug.Log("🔥 [BẮT ĐẦU TEST] KIỂM THỬ GÁN PHÍM 1..6 & ĐÒN ĐÁNH THƯỜNG 3 PHÁI");
            Debug.Log("==================================================================");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Kiểm tra Hoạt ảnh Đòn Đánh Thường 3 Phái trong WeaponSocketView", Test_WeaponSocketAnimations, ref passed, ref failed);
            RunTest("Test 2: Kiểm tra Tung Chiêu 1..6 qua TryCastSkill trong PlayerSkillCombat2D", Test_TryCastSkillSlots, ref passed, ref failed);
            RunTest("Test 3: Kiểm tra Đòn Đánh Thường (Chuột Trái) 3 Phái trong PlayerSkillCombat2D", Test_PerformBasicAttack3Branches, ref passed, ref failed);

            Debug.Log($"==================================================================");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 TẤT CẢ TEST ĐỀU THÀNH CÔNG! ({passed}/{passed + failed} PASSED)</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ CÓ {failed} TEST THẤT BẠI! ({passed}/{passed + failed} PASSED)</color>");
            }
            Debug.Log($"==================================================================");
        }

        private static void RunTest(string testName, Action testAction, ref int passed, ref int failed)
        {
            try
            {
                testAction();
                passed++;
                Debug.Log($"<color=green>✅ [PASS] {testName}</color>");
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"<color=red>❌ [FAIL] {testName}: {ex.Message}\n{ex.StackTrace}</color>");
            }
        }

        private static void Test_WeaponSocketAnimations()
        {
            GameObject testGo = new GameObject("Test_WeaponSocket");
            try
            {
                var wsv = testGo.AddComponent<WeaponSocketView>();
                wsv.ApplyClassWeapon(ClassBranch.Warrior);
                if (wsv.weaponRenderer == null) throw new Exception("weaponRenderer không được tự động khởi tạo!");

                // 1. Melee Slash
                wsv.PlayMeleeSlashAnimation(0.2f);
                if (!wsv.weaponRenderer.enabled) throw new Exception("weaponRenderer phải được bật khi bắt đầu vung kiếm!");

                // 2. Bow Draw & Release
                bool arrowFired = false;
                wsv.ApplyClassWeapon(ClassBranch.Archer);
                wsv.PlayBowShotAnimation(Vector2.right, 0.12f, () => arrowFired = true);
                if (!wsv.weaponRenderer.enabled) throw new Exception("weaponRenderer phải được bật khi giương cung!");

                // 3. Staff Thrust
                bool magicFired = false;
                wsv.ApplyClassWeapon(ClassBranch.Mage);
                wsv.PlayStaffThrustAnimation(Vector2.right, 0.15f, () => magicFired = true);
                if (!wsv.weaponRenderer.enabled) throw new Exception("weaponRenderer phải được bật khi đẩy gậy phép!");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testGo);
            }
        }

        private static void Test_TryCastSkillSlots()
        {
            GameObject playerGo = new GameObject("Test_Player_Combat2D");
            try
            {
                var stats = playerGo.AddComponent<CharacterStats>();
                stats.maxHp = 200f;
                stats.currentHp = 200f;
                stats.maxMp = 500f;
                stats.currentMp = 500f;

                var cs = playerGo.AddComponent<ClassSystem>();
                var combat2D = playerGo.AddComponent<PlayerSkillCombat2D>();

                // Kiểm tra Novice 5 kỹ năng mặc định
                bool cast1 = combat2D.TryCastSkill(0);
                if (!cast1) throw new Exception("Tân thủ tung chiêu 1 (TryCastSkill(0)) thất bại!");

                // Kiểm tra Warrior
                var warriorClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Warrior.asset");
                if (warriorClass != null)
                {
                    combat2D.LoadClassSkills(warriorClass);
                    // Chiêu 3: Iron Bastion (Slot 2)
                    bool castBastion = combat2D.TryCastSkill(2);
                    if (!castBastion) throw new Exception("Chiến Binh tung Thiết Giáp (Slot 2) thất bại!");
                    if (stats.temporaryShieldHp <= 0f) throw new Exception("Thiết Giáp không tạo giáp ảo trên nhân vật!");
                }

                // Kiểm tra Mage
                var mageClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Mage.asset");
                if (mageClass != null)
                {
                    combat2D.LoadClassSkills(mageClass);
                    // Chiêu 5: Mana Shield (Slot 4)
                    bool castShield = combat2D.TryCastSkill(4);
                    if (!castShield) throw new Exception("Pháp Sư tung Nguyên Tố Hộ Thể (Slot 4) thất bại!");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(playerGo);
            }
        }

        private static void Test_PerformBasicAttack3Branches()
        {
            GameObject playerGo = new GameObject("Test_Player_BasicAttack");
            try
            {
                var stats = playerGo.AddComponent<CharacterStats>();
                stats.maxHp = 200f;
                stats.currentHp = 200f;
                var cs = playerGo.AddComponent<ClassSystem>();
                var combat2D = playerGo.AddComponent<PlayerSkillCombat2D>();
                var wsv = playerGo.AddComponent<WeaponSocketView>();

                // Đánh thường Tân Thủ / Chiến Binh (Chuột Trái)
                combat2D.ExecuteAttack();

                // Đổi sang Archer
                var archerClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Archer.asset");
                if (archerClass != null)
                {
                    cs.PromoteTo(archerClass);
                    wsv.ApplyClassWeapon(ClassBranch.Archer);
                    combat2D.ExecuteAttack();
                }

                // Đổi sang Mage
                var mageClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Mage.asset");
                if (mageClass != null)
                {
                    cs.PromoteTo(mageClass);
                    wsv.ApplyClassWeapon(ClassBranch.Mage);
                    combat2D.ExecuteAttack();
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(playerGo);
            }
        }
    }
}
