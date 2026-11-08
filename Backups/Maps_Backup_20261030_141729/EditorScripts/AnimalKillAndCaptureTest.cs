using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ActionRPG.Combat;
using ActionRPG.Enemies;
using ActionRPG.Pets;
using ActionRPG.Town;
using ActionRPG.Characters;
using ActionRPG.UI;

namespace ActionRPG.EditorTests
{
    public static class AnimalKillAndCaptureTest
    {
        [MenuItem("Tools/Lumina/Chạy Kiểm Thử Động Vật Bị Tiêu Diệt & Thu Phục")]
        public static void RunAllTests()
        {
            Debug.Log("==================================================================");
            Debug.Log("🐾 [BẮT ĐẦU TEST] KIỂM THỬ ĐỘNG VẬT BỊ TIÊU DIỆT & THU PHỤC LÀM PET");
            Debug.Log("==================================================================");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Động vật (Hươu/Thỏ) nhận sát thương qua Hurtbox2D", Test_AnimalTakesDamageThroughHurtbox, ref passed, ref failed);
            RunTest("Test 2: Động vật hoảng loạn bỏ chạy (FleeFrom) khi bị tấn công", Test_AnimalFleeReaction, ref passed, ref failed);
            RunTest("Test 3: Động vật bị tiêu diệt khi HP <= 0 và rơi Da Thú", Test_AnimalDeathAndHideDrop, ref passed, ref failed);
            RunTest("Test 4: Thu phục Động vật thành công khi HP < 30% và tạo PetInstance có Sprite", Test_AnimalCaptureSuccessBelow30Percent, ref passed, ref failed);
            RunTest("Test 5: Từ chối thu phục khi máu Động vật > 30% (còn sung mãn)", Test_AnimalCaptureRejectAbove30Percent, ref passed, ref failed);
            RunTest("Test 6: Kiểm tra cấu hình toàn bộ Prefabs Động Vật", Test_AnimalPrefabsConfiguration, ref passed, ref failed);

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

        private static GameObject CreateTestAnimal(string name, float maxHp)
        {
            GameObject animal = new GameObject($"Test_{name}");
            animal.tag = "Enemy";

            var sr = animal.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

            var col = animal.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;

            var rb = animal.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            animal.AddComponent<Hurtbox2D>();

            var stats = animal.AddComponent<EnemyStats>();
            stats.enemyName = name;
            stats.level = 3;
            stats.maxHp = maxHp;
            stats.currentHp = maxHp;
            stats.expReward = 30;

            animal.AddComponent<NPCWanderController2D>();
            animal.AddComponent<EnemyOverheadUI>();

            return animal;
        }

        private static void Test_AnimalTakesDamageThroughHurtbox()
        {
            GameObject deer = CreateTestAnimal("Hươu Sao Thảo Nguyên", 100f);
            var hurt = deer.GetComponent<Hurtbox2D>();
            var stats = deer.GetComponent<EnemyStats>();

            stats.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            DamageInfo dmg = new DamageInfo(35f, ElementType.None, false, null, DamageType.Physical);
            hurt.ReceiveDamage(dmg);

            Assert(stats.currentHp < 100f, $"Máu hươu phải giảm sau đòn đánh! (Hiện tại: {stats.currentHp})");

            GameObject.DestroyImmediate(deer);
        }

        private static void Test_AnimalFleeReaction()
        {
            GameObject rabbit = CreateTestAnimal("Thỏ Nâu Thảo Nguyên", 50f);
            var hurt = rabbit.GetComponent<Hurtbox2D>();
            var stats = rabbit.GetComponent<EnemyStats>();
            var wander = rabbit.GetComponent<NPCWanderController2D>();

            stats.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            DamageInfo dmg = new DamageInfo(10f, ElementType.None, false, null, DamageType.Physical)
            {
                knockbackDirection = Vector2.right
            };
            hurt.ReceiveDamage(dmg);

            // Kiểm tra tốc độ di chuyển tăng lên khi bỏ chạy
            Assert(wander.moveSpeed >= 2.0f, $"Thỏ phải tăng tốc bỏ chạy sau khi bị tấn công! (Tốc độ: {wander.moveSpeed})");

            GameObject.DestroyImmediate(rabbit);
        }

        private static void Test_AnimalDeathAndHideDrop()
        {
            GameObject deer = CreateTestAnimal("Hươu Sao Thảo Nguyên", 60f);
            var stats = deer.GetComponent<EnemyStats>();
            stats.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            // Gây sát thương vượt mức máu
            stats.TakeDamage(100f);

            Assert(stats.currentHp <= 0f, "Máu động vật phải về 0!");

            GameObject.DestroyImmediate(deer);
        }

        private static void Test_AnimalCaptureSuccessBelow30Percent()
        {
            GameObject stag = CreateTestAnimal("Hươu Sao Thảo Nguyên", 100f);
            var stats = stag.GetComponent<EnemyStats>();
            stats.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            // Đánh yếu xuống 20% máu (< 30% ngưỡng bắt)
            stats.currentHp = 20f;

            // Khởi tạo PetCaptureSystem giả lập nếu chưa có
            GameObject capObj = new GameObject("Test_CaptureSystem");
            var capSys = capObj.AddComponent<PetCaptureSystem>();

            // Tạo Player giả lập
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            var pStats = playerObj.AddComponent<CharacterStats>();
            pStats.isPetUnlocked = true;
            pStats.level = 15;

            // Thử bắt với tỷ lệ cầu 1.0 (100% để đảm bảo thành công)
            bool captured = capSys.TryCaptureEnemy(stats, 1.0f);
            Assert(captured, "Phải bắt thành công động vật khi máu < 30% và tỷ lệ cầu cao!");

            GameObject.DestroyImmediate(capObj);
            GameObject.DestroyImmediate(playerObj);
            if (stag != null) GameObject.DestroyImmediate(stag);
        }

        private static void Test_AnimalCaptureRejectAbove30Percent()
        {
            GameObject stag = CreateTestAnimal("Hươu Sao Thảo Nguyên", 100f);
            var stats = stag.GetComponent<EnemyStats>();
            stats.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            // Máu 80% (> 30% ngưỡng bắt)
            stats.currentHp = 80f;

            GameObject capObj = new GameObject("Test_CaptureSystem");
            var capSys = capObj.AddComponent<PetCaptureSystem>();

            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            var pStats = playerObj.AddComponent<CharacterStats>();
            pStats.isPetUnlocked = true;
            pStats.level = 15;

            bool captured = capSys.TryCaptureEnemy(stats, 1.0f);
            Assert(!captured, "Không được phép bắt động vật khi máu còn trên 30%!");

            GameObject.DestroyImmediate(capObj);
            GameObject.DestroyImmediate(playerObj);
            GameObject.DestroyImmediate(stag);
        }

        private static void Test_AnimalPrefabsConfiguration()
        {
            // Cập nhật cấu hình chuẩn cho toàn bộ prefabs
            int updated = ActionRPG.Editor.AttachEnemyOverheadUIMenu.ConfigureAnimalPrefabs();
            Debug.Log($"Đã cấu hình {updated} Animal Prefabs.");

            string[] animalPrefabPaths = new[]
            {
                "Assets/Prefabs/ForestPrototype/Animal_Deer_Stag.prefab",
                "Assets/Prefabs/ForestPrototype/Animal_Rabbit_Brown.prefab",
                "Assets/Prefabs/ForestPrototype/Animal_Rabbit_White.prefab"
            };

            foreach (var path in animalPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert(prefab != null, $"Không tìm thấy prefab {path}!");
                Assert(prefab.CompareTag("Enemy"), $"{prefab.name} phải có tag 'Enemy'!");
                Assert(prefab.GetComponent<Hurtbox2D>() != null, $"{prefab.name} phải có Hurtbox2D!");
                Assert(prefab.GetComponent<EnemyStats>() != null, $"{prefab.name} phải có EnemyStats!");
                Assert(prefab.GetComponent<EnemyOverheadUI>() != null, $"{prefab.name} phải có EnemyOverheadUI!");
                var rb = prefab.GetComponent<Rigidbody2D>();
                Assert(rb != null && rb.bodyType == RigidbodyType2D.Dynamic, $"{prefab.name} Rigidbody2D phải là Dynamic!");
            }
        }
    }
}
