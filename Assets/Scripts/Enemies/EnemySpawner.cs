using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Core;

namespace ActionRPG.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Cấu Hình Khu Vực (Zone)")]
        public string zoneName = "Khu Vực Tân Thủ (Lv 1-5)";
        public List<EnemyData> spawnableEnemies = new List<EnemyData>();
        public int maxActiveEnemies = 6;
        public float spawnInterval = 4.0f;
        public float spawnRadius = 8.0f;

        [Header("Vùng An Toàn (Làng Tân Thủ - Safe Zone)")]
        [Tooltip("Tâm của Làng Tân Thủ nơi tuyệt đối không được sinh quái")]
        public Vector3 villageSafeCenter = Vector3.zero;
        [Tooltip("Bán kính an toàn của Làng Tân Thủ (mặc định 18m)")]
        public float villageSafeRadius = 18f;

        [Header("Chu Kỳ Hồi Sinh Quái & Boss (Continuous Respawn - MMORPG Style)")]
        [Tooltip("Có phải Spawner dành cho Boss thế giới/khu vực không")]
        public bool isBossSpawner = false;
        [Tooltip("Thời gian đếm ngược hồi sinh Boss sau khi bị tiêu diệt (giây)")]
        public float bossRespawnCooldown = 35.0f;
        [Tooltip("Thời gian tự động hồi sinh quái thường khi bị tiêu diệt (giây)")]
        public float mobRespawnDelay = 10.0f;

        private List<GameObject> activeEnemies = new List<GameObject>();
        private float spawnTimer = 0f;
        private float bossRespawnTimer = 0f;
        private bool isBossAwaitingRespawn = false;
        private EnemyData cachedBossData = null;

        private void Update()
        {
            // Dọn dẹp các quái vật đã bị tiêu diệt
            int beforeCount = activeEnemies.Count;
            activeEnemies.RemoveAll(e => e == null);

            // Xử lý đếm ngược hồi sinh Boss liên tục
            if (isBossAwaitingRespawn)
            {
                bossRespawnTimer -= Time.deltaTime;
                if (bossRespawnTimer <= 0f)
                {
                    isBossAwaitingRespawn = false;
                    SpawnBossDirectly(cachedBossData);
                }
            }

            if (activeEnemies.Count < maxActiveEnemies && !isBossAwaitingRespawn)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    spawnTimer = 0f;
                    SpawnRandomEnemy();
                }
            }
        }

        private void SpawnRandomEnemy()
        {
            if (spawnableEnemies.Count == 0) return;

            EnemyData data = spawnableEnemies[Random.Range(0, spawnableEnemies.Count)];
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Bảo vệ Làng Tân Thủ: Nếu vị trí sinh nằm trong bán kính an toàn của làng, đẩy ra ngoài
            float distToVillage = Vector3.Distance(spawnPos, villageSafeCenter);
            if (distToVillage < villageSafeRadius)
            {
                Vector3 dirFromVillage = (spawnPos - villageSafeCenter).normalized;
                if (dirFromVillage == Vector3.zero) dirFromVillage = Vector3.right;
                spawnPos = villageSafeCenter + dirFromVillage * (villageSafeRadius + Random.Range(2f, 6f));
            }

            GameObject enemyObj = new GameObject($"Enemy_{data.enemyName}");
            enemyObj.transform.position = spawnPos;

            // Xử lý kiểm tra Layer an toàn để tránh lỗi Range [0...31]
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer != -1)
            {
                enemyObj.layer = enemyLayer;
            }
            else
            {
                Debug.LogWarning("Chưa tạo Layer 'Enemy' trong Project Settings! Tạm thời gán về Default (Layer 0).");
                enemyObj.layer = 0;
            }

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = data.sprite;
            sr.sortingOrder = 5;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.color = Color.white;

            CircleCollider2D col = enemyObj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            enemyObj.AddComponent<Hurtbox2D>();

            if (data.isAreaBoss || data.isWorldBoss)
            {
                cachedBossData = data;
                BossController boss = enemyObj.AddComponent<BossController>();
                boss.isWorldBoss = data.isWorldBoss;
                boss.Initialize(data);
            }
            else
            {
                EnemyController enemy = enemyObj.AddComponent<EnemyController>();
                enemy.Initialize(data);
            }

            activeEnemies.Add(enemyObj);
        }

        private void SpawnBossDirectly(EnemyData data)
        {
            if (data == null) return;
            string respawnMsg = $"⚠️ CẢNH BÁO THẾ GIỚI: {data.enemyName} ĐÃ TÁI SINH TẠI SÀO HUYỆT!";
            Debug.Log($"<color=red>[EnemySpawner] {respawnMsg}</color>");
            EventBus.OnShowUIMessage?.Invoke(respawnMsg);
            EventBus.OnPlaySound?.Invoke("boss_roar");

            Vector3 spawnPos = transform.position;
            GameObject enemyObj = new GameObject($"Boss_{data.enemyName}");
            enemyObj.transform.position = spawnPos;

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            enemyObj.layer = enemyLayer != -1 ? enemyLayer : 0;

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = data.sprite;
            sr.sortingOrder = 6;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.color = Color.white;

            CircleCollider2D col = enemyObj.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;

            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            enemyObj.AddComponent<Hurtbox2D>();

            BossController boss = enemyObj.AddComponent<BossController>();
            boss.isWorldBoss = data.isWorldBoss;
            boss.Initialize(data);

            activeEnemies.Add(enemyObj);
        }

        private void OnEnable()
        {
            EventBus.OnBossDefeated += HandleBossDefeated;
        }

        private void OnDisable()
        {
            EventBus.OnBossDefeated -= HandleBossDefeated;
        }

        private void HandleBossDefeated(string defeatedBossName)
        {
            if (cachedBossData != null && (cachedBossData.enemyName == defeatedBossName || isBossSpawner))
            {
                isBossAwaitingRespawn = true;
                bossRespawnTimer = bossRespawnCooldown;
                string msg = $"👑 {defeatedBossName} đã bị đánh bại! Chu kỳ tái sinh: {bossRespawnCooldown:F0} giây...";
                Debug.Log($"<color=orange>[EnemySpawner] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
            }
        }
    }
}