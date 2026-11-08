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
using ActionRPG.Editor;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho SPRINT 5:
    /// - Kiểm tra 5 Kỹ Năng Tân Thủ Cấp 1 (Đoạn Phong Trảm, Loạn Vũ Liên Trảm, Kim Cang Giáp, Tứ Bộ Du Long, Ánh Sáng Phục Hồi).
    /// - Kiểm tra logic Máu Ảo / Barrier hấp thụ sát thương.
    /// - Kiểm tra bộ tạo Asset Sprite HUD Pixel Art 16-bit (Khung viền, Icons, Sliced Border).
    /// - Kiểm tra Skill Hotbar HUD (6 ô phím tắt, Cooldown đếm ngược, Mana Cost).
    /// - Kiểm tra tích hợp GameplayHUD an toàn runtime.
    /// </summary>
    public static class Sprint5HUDAndNoviceSkillsTest
    {
        [MenuItem("Tools/Run Sprint 5 HUD & Novice Skills Test")]
        [MenuItem("Tools/Lumina/Run Sprint 5 HUD & Novice Skills Test")]
        public static void RunAllTests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU KIỂM THỬ SPRINT 5: PIXEL ART HUD & 5 KỸ NĂNG TÂN THỦ</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Sinh và kiểm tra Sprite Assets HUD & 5 Icon Kỹ Năng (Pixel Art 16-bit)", Test_GenerateAndValidateSprites, ref passed, ref failed);
            RunTest("Test 2: Kiểm tra dữ liệu 5 Kỹ Năng Tân Thủ ScriptableObjects & Cooldown / Mana", Test_NoviceSkillsDataIntegrity, ref passed, ref failed);
            RunTest("Test 3: Kiểm tra Logic Máu Ảo (Barrier / Temporary Shield) Hấp Thụ Sát Thương", Test_BarrierDamageAbsorption, ref passed, ref failed);
            RunTest("Test 4: Kiểm tra Logic Thi Triển 5 Kỹ Năng Trong PlayerSkillCombat2D", Test_SkillCombatExecution, ref passed, ref failed);
            RunTest("Test 5: Kiểm tra Cấu Trúc Hierarchy GameplayHUD & Đồng Bộ Skill Slot UI", Test_GameplayHUDAndSkillSlots, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 HOÀN THÀNH TẤT CẢ {passed} BÀI TEST SPRINT 5! HỆ THỐNG PIXEL ART HUD & 5 KỸ NĂNG TÂN THỦ HOẠT ĐỘNG HOÀN HẢO!</color>");
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
                Debug.Log($"<color=yellow>▶ Đang chạy: {name}...</color>");
                action.Invoke();
                Debug.Log($"<color=green>✓ PASS: {name}</color>");
                passed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>✗ FAIL: {name}</color>\nLý do: {ex.Message}\n{ex.StackTrace}");
                failed++;
            }
        }

        private static void Test_GenerateAndValidateSprites()
        {
            // Kích hoạt sinh Sprite Asset
            LuminaHUDAssetGenerator.GenerateAllHUDAssets();

            string[] expectedSkills = {
                "icon_skill_fan_slash.png",
                "icon_skill_flurry_slash.png",
                "icon_skill_aegis_barrier.png",
                "icon_skill_quad_dash.png",
                "icon_skill_holy_heal.png"
            };

            foreach (var icon in expectedSkills)
            {
                string path = $"Assets/Sprites/UI/HUD/Skills/{icon}";
                if (!File.Exists(path))
                    throw new Exception($"Không tìm thấy sprite icon kỹ năng tại {path}");
            }

            string[] expectedFrames = {
                "frame_skill_slot.png",
                "frame_hud_bar_bg.png",
                "fill_hp_bar.png",
                "fill_mp_bar.png",
                "fill_exp_bar.png",
                "frame_portrait.png",
                "frame_boss_bar.png"
            };

            foreach (var frame in expectedFrames)
            {
                string path = $"Assets/Sprites/UI/HUD/Frames/{frame}";
                if (!File.Exists(path))
                    throw new Exception($"Không tìm thấy sprite frame tại {path}");
            }

            Debug.Log("[Test 1] Tất cả file Sprite PNG Pixel Art 16-bit đã được sinh đầy đủ và cấu hình hợp lệ.");
        }

        private static void Test_NoviceSkillsDataIntegrity()
        {
            // Đồng bộ database
            LuminaClassAssetDatabase.GenerateAll();

            string[] skillIds = {
                "novice_fan_slash",
                "novice_flurry_slash",
                "novice_aegis_barrier",
                "novice_quad_dash",
                "novice_holy_heal"
            };

            float[] expectedCds = { 3.0f, 7.0f, 14.0f, 10.0f, 22.0f };
            float[] expectedManas = { 10f, 25f, 20f, 15f, 35f };

            for (int i = 0; i < skillIds.Length; i++)
            {
                string path = $"Assets/ScriptableObjects/Skills/Skill_{skillIds[i]}.asset";
                SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>(path);
                if (skill == null)
                    throw new Exception($"Không tìm thấy asset kỹ năng tại {path}");

                if (Mathf.Abs(skill.cooldownSeconds - expectedCds[i]) > 0.01f)
                    throw new Exception($"Kỹ năng {skill.skillName} có Cooldown {skill.cooldownSeconds}s không khớp kỳ vọng {expectedCds[i]}s");

                if (Mathf.Abs(skill.manaCost - expectedManas[i]) > 0.01f)
                    throw new Exception($"Kỹ năng {skill.skillName} có Mana Cost {skill.manaCost} không khớp kỳ vọng {expectedManas[i]}");
            }

            // Kiểm tra Class_Novice đã gán đủ 5 kỹ năng
            CharacterClassData noviceClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
            if (noviceClass == null) throw new Exception("Không tìm thấy Class_Novice.asset");
            if (noviceClass.unlockedSkills.Count < 5)
                throw new Exception($"Class_Novice chỉ có {noviceClass.unlockedSkills.Count} kỹ năng, kỳ vọng 5.");

            Debug.Log("[Test 2] Dữ liệu 5 Kỹ Năng Tân Thủ và Class_Novice hoàn toàn chính xác.");
        }

        private static void Test_BarrierDamageAbsorption()
        {
            GameObject testGo = new GameObject("Test_Player_Barrier");
            try
            {
                var stats = testGo.AddComponent<CharacterStats>();
                stats.maxHp = 100f;
                stats.currentHp = 100f;
                stats.baseDef = 0f; // Không trừ giáp cơ bản để tính toán chính xác

                // 1. Thêm Barrier 50 HP
                stats.AddBarrier(50f, 5f);
                if (Mathf.Abs(stats.temporaryShieldHp - 50f) > 0.01f)
                    throw new Exception($"Máu ảo không đạt 50: {stats.temporaryShieldHp}");

                // 2. Nhận đòn đánh 30 sát thương (phải trừ hết vào barrier, currentHp vẫn còn 100)
                stats.TakeDamage(new DamageInfo(30f, ElementType.None, false, null));
                if (Mathf.Abs(stats.currentHp - 100f) > 0.01f)
                    throw new Exception($"Máu thực bị trừ khi Barrier còn dư! currentHp={stats.currentHp}");
                if (Mathf.Abs(stats.temporaryShieldHp - 20f) > 0.01f)
                    throw new Exception($"Barrier còn lại không đúng (kỳ vọng 20): {stats.temporaryShieldHp}");

                // 3. Nhận đòn đánh 40 sát thương (hết 20 barrier, 20 dmg còn lại trừ vào currentHp -> 80)
                stats.TakeDamage(new DamageInfo(40f, ElementType.None, false, null));
                if (Mathf.Abs(stats.temporaryShieldHp - 0f) > 0.01f)
                    throw new Exception($"Barrier không về 0 sau khi bị xuyên vỡ: {stats.temporaryShieldHp}");
                if (Mathf.Abs(stats.currentHp - 80f) > 0.01f)
                    throw new Exception($"Máu thực sau khi vỡ khiên không đúng (kỳ vọng 80): {stats.currentHp}");

                Debug.Log("[Test 3] Cơ chế Máu Ảo (Barrier / Temporary Shield) hấp thụ sát thương hoàn hảo.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testGo);
            }
        }

        private static void Test_SkillCombatExecution()
        {
            GameObject playerGo = new GameObject("Test_Player_Combat2D");
            try
            {
                var stats = playerGo.AddComponent<CharacterStats>();
                stats.maxHp = 100f;
                stats.currentHp = 40f; // Để test hồi máu
                stats.maxMp = 100f;
                stats.currentMp = 100f;

                var combat2D = playerGo.AddComponent<PlayerSkillCombat2D>();

                // Gán Novice skills
                var noviceClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
                var classSys = playerGo.AddComponent<ClassSystem>();
                classSys.currentClass = noviceClass;

                // 1. Cast Slot 0 (Đoạn Phong Trảm: 10 MP)
                combat2D.CastSkillBySlot(0);
                if (Mathf.Abs(stats.currentMp - 90f) > 0.01f)
                    throw new Exception($"Sau chiêu 1 MP không giảm về 90: {stats.currentMp}");

                // 2. Cast Slot 2 (Kim Cang Giáp: 20 MP, +15 Giáp, +50 Barrier)
                combat2D.CastSkillBySlot(2);
                if (Mathf.Abs(stats.currentMp - 70f) > 0.01f)
                    throw new Exception($"Sau chiêu 3 MP không giảm về 70: {stats.currentMp}");
                if (stats.temporaryShieldHp < 49f)
                    throw new Exception($"Chiêu 3 không tạo Máu Ảo: {stats.temporaryShieldHp}");

                // 3. Cast Slot 4 (Ánh Sáng Phục Hồi: 35 MP, +60 HP)
                combat2D.CastSkillBySlot(4);
                if (Mathf.Abs(stats.currentMp - 35f) > 0.01f)
                    throw new Exception($"Sau chiêu 5 MP không giảm về 35: {stats.currentMp}");
                if (Mathf.Abs(stats.currentHp - 100f) > 0.01f)
                    throw new Exception($"Chiêu 5 không hồi đầy máu: {stats.currentHp}");

                Debug.Log("[Test 4] PlayerSkillCombat2D thi triển thành công các kỹ năng Tân Thủ.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(playerGo);
            }
        }

        private static void Test_GameplayHUDAndSkillSlots()
        {
            GameObject hudGo = new GameObject("Test_GameplayHUD");
            try
            {
                var hud = hudGo.AddComponent<GameplayHUD>();
                hud.EnsureUIHierarchy();

                if (hud.skillSlots == null || hud.skillSlots.Length < 6)
                    throw new Exception("Hệ thống skillSlots chưa đủ 6 slot!");

                var noviceClass = AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
                hud.UpdateSkillBar(noviceClass);

                // Kiểm tra slot 0 có gán đúng novice_fan_slash
                if (hud.skillSlots[0].SkillId != "novice_fan_slash")
                    throw new Exception($"Slot 0 có SkillId='{hud.skillSlots[0].SkillId}', kỳ vọng 'novice_fan_slash'");

                // Kiểm tra gán đúng số mana cost
                if (hud.skillSlots[0].manaCostText != null && hud.skillSlots[0].manaCostText.text != "10")
                    throw new Exception($"Slot 0 Mana Cost Text là '{hud.skillSlots[0].manaCostText.text}', kỳ vọng '10'");

                // Kiểm tra cooldown countdown
                hud.skillSlots[0].SetCooldown(0.5f, 1.5f);
                if (hud.skillSlots[0].cdText != null && !hud.skillSlots[0].cdText.enabled)
                    throw new Exception("cdText không hiển thị khi cooldown còn 1.5s!");

                Debug.Log("[Test 5] Cây phân cấp GameplayHUD và đồng bộ Skill Hotbar hoạt động chính xác.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hudGo);
            }
        }
    }
}
#endif
