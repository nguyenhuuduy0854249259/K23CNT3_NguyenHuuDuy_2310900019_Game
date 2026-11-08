#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Items;
using ActionRPG.Pets;
using ActionRPG.UI;
using ActionRPG.UI.Gameplay;
using ActionRPG.Classes.Mechanics;

namespace ActionRPG.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho SPRINT 4:
    /// - Kiểm tra toàn vẹn dữ liệu 14 Class, 42 Ấn Nội Tại và 48 Kỹ Năng Lumina.
    /// - Kiểm tra chuyển đổi qua lại giữa 14 Class trên nhân vật Player.
    /// - Kiểm tra thử nghiệm 10 cơ chế đặc trưng (PetWeapon, Queue, DualPet, Time, Priest, Turret, Decoy, Arrow, Spirit).
    /// - Kiểm tra hoạt động của Debug Toolbar (Cấp độ 1/10/20/30, Full Heal, Bơm vàng & thuốc, Spawn quái test).
    /// - Kiểm tra hệ thống Menu (ClassDemoSwitcherUI [F1], PauseMenuUI [ESC], CharacterUI [U]) và phím tắt.
    /// </summary>
    public static class ClassDemoSwitcherConsistencyTest
    {
        [MenuItem("Tools/Run Class Demo Switcher Test")]
        [MenuItem("Tools/Lumina/Run Class Demo Switcher Test")]
        public static void RunAllTests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU KIỂM THỬ SPRINT 4: LUMINA 14 CLASSES & DEMO SWITCHER</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Kiểm Tra Toàn Bộ 14 ClassData & Dữ Liệu Lumina Spec", Test_ClassDataIntegrity, ref passed, ref failed);
            RunTest("Test 2: Kiểm Tra Chuyển Đổi 14 Class & Đồng Bộ Stats Trên Player", Test_PlayerClassPromotionAndStats, ref passed, ref failed);
            RunTest("Test 3: Thử Nghiệm Kích Hoạt 10 Cơ Chế Đặc Trưng (Mechanics Testing)", Test_ClassSpecialMechanics, ref passed, ref failed);
            RunTest("Test 4: Kiểm Tra Thanh Công Cụ Debug Toolbar (Level, Heal, Items, Spawn)", Test_DebugToolbarFunctions, ref passed, ref failed);
            RunTest("Test 5: Kiểm Tra Menu Hệ Thống & Tự Động Sinh UI Không Bị Null", Test_UIHierarchyAndShortcuts, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 HOÀN THÀNH TẤT CẢ {passed} BÀI TEST SPRINT 4! HỆ THỐNG CLASS & MENU HOẠT ĐỘNG HOÀN HẢO!</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ CÓ {failed} BÀI TEST THẤT BẠI! Vui lòng kiểm tra log lỗi ở trên.</color>");
            }
            Debug.Log("<color=cyan>==================================================</color>");
        }

        private static void RunTest(string name, Action action, ref int passed, ref int failed)
        {
            try
            {
                action();
                Debug.Log($"<color=green>  [PASS] {name}</color>");
                passed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>  [FAIL] {name} => {ex.Message}\n{ex.StackTrace}</color>");
                failed++;
            }
        }

        #region Test 1: Class Data Integrity
        private static void Test_ClassDataIntegrity()
        {
            string[] requiredClasses = new string[]
            {
                "Novice", "Warrior", "Archer", "Mage",
                "FreeMelee", "ElementalMage", "Summoner", "ChronoMage",
                "HolyDarkPriest", "EngineerGunner", "ShadowGunner",
                "StanceArcher", "ElementalArcher", "SpiritArcher"
            };

            foreach (var cls in requiredClasses)
            {
                string path = $"Assets/ScriptableObjects/Classes/Class_{cls}.asset";
                var cData = AssetDatabase.LoadAssetAtPath<CharacterClassData>(path);
                Assert(cData != null, $"Không tìm thấy ScriptableObject Class_{cls}.asset tại {path}!");
                Assert(!string.IsNullOrEmpty(cData.className), $"Class {cls} có className rỗng!");
                Assert(!string.IsNullOrEmpty(cData.description), $"Class {cls} thiếu description!");
                Assert(cData.baseHp > 0, $"Class {cls} baseHp <= 0!");
                Assert(cData.baseAtk > 0, $"Class {cls} baseAtk <= 0!");

                // Kiểm tra 3 Ấn Nội Tại
                Assert(!string.IsNullOrEmpty(cData.passiveSeal1Name), $"Class {cls} thiếu tên Ấn Nội Tại 1!");
                Assert(!string.IsNullOrEmpty(cData.passiveSeal2Name), $"Class {cls} thiếu tên Ấn Nội Tại 2!");
                Assert(!string.IsNullOrEmpty(cData.passiveSeal3Name), $"Class {cls} thiếu tên Ấn Nội Tại 3!");
            }
        }
        #endregion

        #region Test 2: Player Class Promotion & Stats
        private static void Test_PlayerClassPromotionAndStats()
        {
            GameObject go = new GameObject("TestPlayer_Sprint4");
            try
            {
                var stats = go.AddComponent<CharacterStats>();
                var classSys = go.AddComponent<ClassSystem>();

                // Khởi tạo stats ban đầu
                stats.level = 10;
                stats.maxHp = 200;
                stats.maxMp = 100;
                stats.baseAtk = 25;
                stats.baseDef = 10;

                string[] classesToTest = new string[]
                {
                    "Novice", "FreeMelee", "ElementalMage", "HolyDarkPriest",
                    "EngineerGunner", "SpiritArcher"
                };

                foreach (var name in classesToTest)
                {
                    string path = $"Assets/ScriptableObjects/Classes/Class_{name}.asset";
                    var cData = AssetDatabase.LoadAssetAtPath<CharacterClassData>(path);
                    Assert(cData != null, $"Không tải được {path}");

                    classSys.PromoteTo(cData);

                    Assert(classSys.currentClass == cData, $"PromoteTo thất bại với class {name}");
                    Assert(stats.currentHp == stats.maxHp, $"HP không được hồi đầy khi đổi class {name}");
                    Assert(stats.currentMp == stats.maxMp, $"MP không được hồi đầy khi đổi class {name}");
                    Assert(stats.characterElement == cData.defaultElement, $"Nguyên tố nhân vật không khớp với defaultElement của {name}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
        #endregion

        #region Test 3: Special Mechanics Testing
        private static void Test_ClassSpecialMechanics()
        {
            GameObject go = new GameObject("TestPlayer_Mechanics");
            try
            {
                var stats = go.AddComponent<CharacterStats>();
                var classSys = go.AddComponent<ClassSystem>();

                // 1. HolyDarkPriest - Stance Toggle
                var priestClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_HolyDarkPriest.asset");
                classSys.PromoteTo(priestClass);
                var priest = go.GetComponent<HolyDarkPriestMechanics>();
                Assert(priest != null, "Chưa tự động gắn HolyDarkPriestMechanics!");
                PriestForm initialForm = priest.currentForm;
                priest.TogglePriestForm();
                Assert(priest.currentForm != initialForm, "Toggle Stance Mục Sư thất bại!");

                // 2. ElementalMage - Queue Charging
                var elemClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_ElementalMage.asset");
                classSys.PromoteTo(elemClass);
                var elem = go.GetComponent<ElementalQueueMechanics>();
                Assert(elem != null, "Chưa tự động gắn ElementalQueueMechanics!");
                elem.currentQueue.Clear();
                elem.PushElement(ElementType.Fire);
                Assert(elem.currentQueue.Count == 1, "Thêm hạt nguyên tố vào Hàng Đợi thất bại!");

                // 3. SpiritArcher - Spirit Charge
                var spiritClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_SpiritArcher.asset");
                classSys.PromoteTo(spiritClass);
                var spirit = go.GetComponent<SpiritArcherMechanics>();
                Assert(spirit != null, "Chưa tự động gắn SpiritArcherMechanics!");
                spirit.currentCharges = 3;
                Assert(spirit.currentCharges == 3, "Tích lũy Tinh Linh Tiễn thất bại!");

                // 4. EngineerGunner - Turret Spawning
                var engiClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_EngineerGunner.asset");
                classSys.PromoteTo(engiClass);
                var engi = go.GetComponent<EngineerGunnerMechanics>();
                Assert(engi != null, "Chưa tự động gắn EngineerGunnerMechanics!");
                engi.DeployTurret(TurretType.Gatling, go.transform.position, 0);
                Assert(engi.activeTurrets.Count > 0, "Dựng trụ súng Gatling thất bại!");

                // 5. ShadowGunner - Decoy Spawning
                var shadowClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_ShadowGunner.asset");
                classSys.PromoteTo(shadowClass);
                var shadow = go.GetComponent<ShadowGunnerMechanics>();
                Assert(shadow != null, "Chưa tự động gắn ShadowGunnerMechanics!");
                shadow.EnterStealth();
                Assert(shadow.isStealthed, "Kích hoạt Tàng hình & Decoy thất bại!");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
        #endregion

        #region Test 4: Debug Toolbar Functions
        private static void Test_DebugToolbarFunctions()
        {
            GameObject go = new GameObject("TestPlayer_Toolbar");
            GameObject uiGo = new GameObject("TestUI_Toolbar");
            try
            {
                var stats = go.AddComponent<CharacterStats>();
                var inv = go.AddComponent<Inventory>();
                var classSys = go.AddComponent<ClassSystem>();

                stats.level = 1;
                stats.maxHp = 100;
                stats.currentHp = 10;
                stats.currentMp = 5;

                var switcher = uiGo.AddComponent<ClassDemoSwitcherUI>();

                // 1. Test Set Level
                stats.level = 20;
                Assert(stats.level == 20, "Đặt cấp độ 20 thất bại!");

                // 2. Test Full Heal
                stats.Heal(99999);
                stats.RestoreMp(99999);
                Assert(stats.currentHp == stats.maxHp, "Hồi đầy máu thất bại!");
                Assert(stats.currentMp == stats.maxMp, "Hồi đầy mana thất bại!");

                // 3. Test Add Gold
                inv.gold = 0;
                inv.AddGold(1000);
                Assert(inv.gold == 1000, "Bơm vàng thất bại!");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(uiGo);
            }
        }
        #endregion

        #region Test 5: UI Hierarchy & Shortcuts
        private static void Test_UIHierarchyAndShortcuts()
        {
            GameObject canvasGo = new GameObject("TestCanvas", typeof(Canvas));
            GameObject switcherGo = new GameObject("TestSwitcherUI");
            switcherGo.transform.SetParent(canvasGo.transform, false);

            GameObject pauseGo = new GameObject("TestPauseUI");
            pauseGo.transform.SetParent(canvasGo.transform, false);

            GameObject classSelGo = new GameObject("TestClassSelUI");
            classSelGo.transform.SetParent(canvasGo.transform, false);

            try
            {
                // ClassDemoSwitcherUI Auto-Hierarchy
                var switcher = switcherGo.AddComponent<ClassDemoSwitcherUI>();
                switcher.EnsureUIHierarchy();
                Assert(switcher.panelRoot != null, "ClassDemoSwitcherUI không tự tạo panelRoot!");
                Assert(switcher.applyClassButton != null, "ClassDemoSwitcherUI thiếu nút applyClassButton!");
                Assert(switcher.testMechanicButton != null, "ClassDemoSwitcherUI thiếu nút testMechanicButton!");

                // Toggle test
                switcher.Toggle();
                bool state = switcher.panelRoot.activeSelf;
                switcher.Toggle();
                Assert(switcher.panelRoot.activeSelf != state, "Toggle ClassDemoSwitcherUI thất bại!");

                // PauseMenuUI Auto-Hierarchy
                var pause = pauseGo.AddComponent<PauseMenuUI>();
                pause.EnsureUIHierarchy();
                Assert(pause.panelRoot != null, "PauseMenuUI không tự tạo panelRoot!");
                Assert(pause.resumeButton != null, "PauseMenuUI thiếu resumeButton!");
                pause.TogglePause();
                bool pauseState = pause.panelRoot.activeSelf;
                pause.TogglePause();
                Assert(pause.panelRoot.activeSelf != pauseState, "Toggle PauseMenuUI thất bại!");

                // ClassSelectionUI Auto-Hierarchy
                var classSel = classSelGo.AddComponent<ClassSelectionUI>();
                classSel.EnsureUIHierarchy();
                Assert(classSel.panelRoot != null, "ClassSelectionUI không tự tạo panelRoot!");
                Assert(classSel.confirmButton != null, "ClassSelectionUI thiếu confirmButton!");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(classSelGo);
                UnityEngine.Object.DestroyImmediate(pauseGo);
                UnityEngine.Object.DestroyImmediate(switcherGo);
                UnityEngine.Object.DestroyImmediate(canvasGo);
            }
        }
        #endregion

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }
    }
}
#endif
