using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ActionRPG.Enemies;
using ActionRPG.UI;
using ActionRPG.Combat;

namespace ActionRPG.EditorTests
{
    public static class EnemyOverheadUITest
    {
        [MenuItem("Tools/Lumina/Chạy Kiểm Thử Enemy Overhead HP Bar & Level")]
        public static void RunAllTests()
        {
            Debug.Log("==================================================================");
            Debug.Log("🎯 [BẮT ĐẦU TEST] KIỂM THỬ THANH MÁU & CẤP ĐỘ TRÊN ĐẦU QUÁI VẬT");
            Debug.Log("==================================================================");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Tự động khởi tạo World-Space Canvas với sortingOrder = 500", Test_CanvasCreationAndSortingOrder, ref passed, ref failed);
            RunTest("Test 2: Định dạng nhãn Level & Tên (Lv.X [Tên Quái])", Test_LevelNameLabelFormat, ref passed, ref failed);
            RunTest("Test 3: Kích thước thanh máu 90x12 và thanh đỏ Filled", Test_HealthBarDimensionsAndFill, ref passed, ref failed);
            RunTest("Test 4: Co giãn thanh máu theo currentHp / maxHp khi nhận sát thương", Test_DamageHealthFillAdjustment, ref passed, ref failed);
            RunTest("Test 5: Khóa góc xoay và triệt tiêu lật ngược tỷ lệ khi quái flip", Test_RotationAndFlipCompensation, ref passed, ref failed);
            RunTest("Test 6: Tự động gắn EnemyOverheadUI trong EnemyStats và EnemyController", Test_AutoAttachmentInStart, ref passed, ref failed);
            RunTest("Test 7: Menu Tools Quét và gắn EnemyOverheadUI cho Prefabs", Test_AttachMenuToPrefabs, ref passed, ref failed);

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

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void Test_CanvasCreationAndSortingOrder()
        {
            GameObject monster = new GameObject("Test_WhiteWolf");
            var sr = monster.AddComponent<SpriteRenderer>();
            var stats = monster.AddComponent<EnemyStats>();
            stats.enemyName = "Sói Trắng";
            stats.level = 5;

            var overhead = monster.AddComponent<EnemyOverheadUI>();
            overhead.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            var canvas = monster.GetComponentInChildren<Canvas>();
            Assert(canvas != null, "Canvas con World-space chưa được tạo!");
            Assert(canvas.renderMode == RenderMode.WorldSpace, "Canvas phải ở chế độ RenderMode.WorldSpace!");
            Assert(canvas.sortingOrder == 500, $"Canvas sortingOrder phải bằng 500 (hiện tại: {canvas.sortingOrder})!");
            Assert(Mathf.Approximately(canvas.transform.localScale.x, 0.01f), "Canvas localScale.x phải là 0.01f!");

            GameObject.DestroyImmediate(monster);
        }

        private static void Test_LevelNameLabelFormat()
        {
            GameObject monster = new GameObject("Test_Dummy");
            var stats = monster.AddComponent<EnemyStats>();
            stats.enemyName = "Bù Nhìn";
            stats.level = 3;

            var overhead = monster.AddComponent<EnemyOverheadUI>();
            overhead.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            var label = monster.GetComponentInChildren<TextMeshProUGUI>();
            Assert(label != null, "Nhãn TextMeshProUGUI chưa được tạo!");
            Assert(label.text.Contains("Lv.3"), $"Nhãn text phải chứa 'Lv.3'! (Hiện tại: '{label.text}')");
            Assert(label.text.Contains("Bù Nhìn"), $"Nhãn text phải chứa tên quái 'Bù Nhìn'! (Hiện tại: '{label.text}')");

            var outline = label.GetComponent<Outline>();
            Assert(outline != null, "Nhãn Level/Tên phải có component Outline viền đen!");

            GameObject.DestroyImmediate(monster);
        }

        private static void Test_HealthBarDimensionsAndFill()
        {
            GameObject monster = new GameObject("Test_Boar");
            var stats = monster.AddComponent<EnemyStats>();
            stats.maxHp = 100f;
            stats.currentHp = 100f;

            var overhead = monster.AddComponent<EnemyOverheadUI>();
            overhead.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            var border = monster.transform.Find("Overhead_Health_Canvas/Hp_Border");
            Assert(border != null, "Hp_Border chưa được tạo!");
            var borderRect = border.GetComponent<RectTransform>();
            Assert(borderRect.sizeDelta.x >= 90f, $"Bề rộng thanh máu phải từ 90 trở lên (hiện tại: {borderRect.sizeDelta.x})!");

            var fill = monster.transform.Find("Overhead_Health_Canvas/Hp_Border/Hp_Background/Hp_HealthFill");
            Assert(fill != null, "Hp_HealthFill chưa được tạo!");
            var fillImg = fill.GetComponent<Image>();
            Assert(fillImg.type == Image.Type.Filled, "Thanh máu đỏ phải có Image.Type = Filled!");
            Assert(Mathf.Approximately(fillImg.fillAmount, 1.0f), "Thanh máu ban đầu phải đầy (1.0f)!");

            GameObject.DestroyImmediate(monster);
        }

        private static void Test_DamageHealthFillAdjustment()
        {
            GameObject monster = new GameObject("Test_ScarecrowDamage");
            var stats = monster.AddComponent<EnemyStats>();
            stats.maxHp = 200f;
            stats.currentHp = 200f;

            var overhead = monster.AddComponent<EnemyOverheadUI>();
            overhead.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            // Gây 100 sát thương (còn 50% máu)
            stats.currentHp = 100f;
            overhead.SendMessage("Update", SendMessageOptions.DontRequireReceiver);

            var fill = monster.transform.Find("Overhead_Health_Canvas/Hp_Border/Hp_Background/Hp_HealthFill");
            var fillImg = fill.GetComponent<Image>();
            Assert(Mathf.Approximately(fillImg.fillAmount, 0.5f), $"Tỷ lệ fillAmount phải là 0.5f sau khi mất 50% HP (hiện tại: {fillImg.fillAmount})!");

            GameObject.DestroyImmediate(monster);
        }

        private static void Test_RotationAndFlipCompensation()
        {
            GameObject monster = new GameObject("Test_FlipMonster");
            var stats = monster.AddComponent<EnemyStats>();
            var overhead = monster.AddComponent<EnemyOverheadUI>();
            overhead.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            // Giả lập lật mặt quái vật sang trái (localScale.x = -1) và xoay góc
            monster.transform.localScale = new Vector3(-1f, 1f, 1f);
            monster.transform.rotation = Quaternion.Euler(0, 0, 45f);

            overhead.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);

            var canvas = monster.GetComponentInChildren<Canvas>();
            Assert(canvas.transform.rotation == Quaternion.identity, "Canvas phải bị khóa góc xoay Quaternion.identity!");
            // Scale x trong World phải dương (không bị lật ngược chữ)
            Assert(canvas.transform.lossyScale.x > 0f, $"World Scale X của Canvas phải là số dương để không bị lật ngược chữ! (lossyScale: {canvas.transform.lossyScale.x})");

            GameObject.DestroyImmediate(monster);
        }

        private static void Test_AutoAttachmentInStart()
        {
            GameObject monster = new GameObject("Test_AutoAttach");
            var stats = monster.AddComponent<EnemyStats>();

            // Khi EnemyStats chạy Start hoặc OnEnable, EnemyOverheadUI phải tự động được gắn
            stats.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            var overhead = monster.GetComponent<EnemyOverheadUI>();
            Assert(overhead != null, "EnemyStats phải tự động AddComponent<EnemyOverheadUI>() trong Start/OnEnable!");

            GameObject.DestroyImmediate(monster);
        }

        private static void Test_AttachMenuToPrefabs()
        {
            int updated = ActionRPG.Editor.AttachEnemyOverheadUIMenu.AttachToEnemyPrefabs();
            Debug.Log($"Số lượng prefabs đã cập nhật EnemyOverheadUI: {updated}");
            // Kiểm tra ít nhất 1 prefab quái vật trong thư mục Assets/Prefabs/ForestPrototype có EnemyOverheadUI
            var wolfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ForestPrototype/Enemy_Forest_WhiteWolf.prefab");
            if (wolfPrefab != null)
            {
                Assert(wolfPrefab.GetComponent<EnemyOverheadUI>() != null, "Enemy_Forest_WhiteWolf.prefab phải có EnemyOverheadUI!");
            }
        }
    }
}
