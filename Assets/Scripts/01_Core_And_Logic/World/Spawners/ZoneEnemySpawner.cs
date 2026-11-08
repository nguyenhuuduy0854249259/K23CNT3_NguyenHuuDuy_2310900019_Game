using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Core.World;
using ActionRPG.Enemies;
using ActionRPG.Items.Loot;

namespace ActionRPG.Core.World.Spawners
{
    /// <summary>
    /// Bộ điều phối Spawner quái dã ngoại theo phân vùng (Zone Enemy Spawner) chuẩn Sprint 11:
    /// - Quản lý hồ bơi quái vật (EnemyData pool) tương ứng từng vùng đất.
    /// - Duy trì sĩ số quái vật tối đa (maxEnemiesAlive).
    /// - Tự động đếm ngược hồi sinh (respawnDelay 20s - 30s) sau khi quái bị tiêu diệt.
    /// - Hỗ trợ vẽ Gizmos trực quan hóa bán kính spawn trong Unity Scene View.
    /// </summary>
    public class ZoneEnemySpawner : MonoBehaviour
    {
        [Header("Cấu Hình Phân Vùng")]
        public GameZone zone = GameZone.Grassland_Lv1_10;
        public string spawnerName = "Bãi Quái Thảo Nguyên";

        [Header("Hồ Bơi Quái Vật (Enemy Pool)")]
        [Tooltip("Danh sách các loài quái vật có thể sinh ra tại cụm này")]
        public List<EnemyData> enemyPool = new List<EnemyData>();

        [Header("Thông Số Sinh Quái & Hồi Sinh")]
        [Tooltip("Số lượng quái tối đa tồn tại cùng lúc")]
        [Range(1, 15)] public int maxEnemiesAlive = 4;

        [Tooltip("Bán kính khu vực sinh quái (mét)")]
        public float spawnRadius = 6.0f;

        [Tooltip("Thời gian đếm ngược hồi sinh sau khi quái chết (20s - 30s)")]
        [Range(5f, 60f)] public float respawnDelay = 25.0f;

        [Header("Prefab Quái Vật")]
        [Tooltip("Prefab quái chuẩn (nếu trống, hệ thống sẽ tự động khởi tạo theo quy chuẩn 2D)")]
        public GameObject enemyPrefab;

        // Trạng thái vận hành
        private readonly List<GameObject> activeEnemies = new List<GameObject>();
        private readonly List<float> pendingRespawnTimers = new List<float>();

        private void Start()
        {
            // Khởi tạo ban đầu đủ sĩ số quái vật
            SpawnInitialWave();
        }

        private void Update()
        {
            // 1. Kiểm tra quái vật đã bị tiêu diệt hoặc thu phục
            int countBefore = activeEnemies.Count;
            activeEnemies.RemoveAll(e => e == null);
            int destroyedCount = countBefore - activeEnemies.Count;

            // Mỗi quái bị tiêu diệt sẽ mở một bộ đếm hồi sinh
            for (int i = 0; i < destroyedCount; i++)
            {
                pendingRespawnTimers.Add(respawnDelay);
            }

            // 2. Cập nhật các bộ đếm hồi sinh
            for (int i = pendingRespawnTimers.Count - 1; i >= 0; i--)
            {
                pendingRespawnTimers[i] -= Time.deltaTime;
                if (pendingRespawnTimers[i] <= 0f)
                {
                    pendingRespawnTimers.RemoveAt(i);
                    if (activeEnemies.Count < maxEnemiesAlive)
                    {
                        SpawnSingleEnemy();
                    }
                }
            }

            // 3. Đảm bảo nếu chưa đủ quái và không có bộ đếm nào đang chạy thì bổ sung
            if (activeEnemies.Count + pendingRespawnTimers.Count < maxEnemiesAlive)
            {
                pendingRespawnTimers.Add(Random.Range(2f, 5f));
            }
        }

        private void SpawnInitialWave()
        {
            if (enemyPool == null || enemyPool.Count == 0) return;

            for (int i = 0; i < maxEnemiesAlive; i++)
            {
                SpawnSingleEnemy();
            }
        }

        public GameObject SpawnSingleEnemy()
        {
            if (enemyPool == null || enemyPool.Count == 0) return null;

            // 1. Chọn ngẫu nhiên loại quái từ hồ bơi
            EnemyData selectedData = enemyPool[Random.Range(0, enemyPool.Count)];
            if (selectedData == null) return null;

            // 2. Tính toán vị trí spawn ngẫu nhiên trong bán kính
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

            GameObject enemyObj;

            if (enemyPrefab != null)
            {
                enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Tự động dựng GameObject quái vật chuẩn
                enemyObj = CreateDefaultEnemyGameObject(selectedData, spawnPos);
            }

            // Gán thông tin EnemyData
            var stats = enemyObj.GetComponent<EnemyStats>();
            if (stats != null)
            {
                stats.Initialize(selectedData);
            }

            activeEnemies.Add(enemyObj);
            return enemyObj;
        }

        private GameObject CreateDefaultEnemyGameObject(EnemyData data, Vector3 position)
        {
            GameObject obj = new GameObject($"Enemy_{data.enemyName}");
            obj.transform.position = position;
            obj.tag = "Enemy";

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) obj.layer = enemyLayer;

            // SpriteRenderer
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = data.sprite;
            sr.sortingOrder = 4;

            // Collider & Rigidbody2D
            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Combat & Logic Components
            obj.AddComponent<Hurtbox2D>();
            var stats = obj.AddComponent<EnemyStats>();
            stats.Initialize(data);

            var ctrl = obj.AddComponent<EnemyController>();
            ctrl.enemyData = data;

            obj.AddComponent<ActionRPG.UI.EnemyOverheadUI>();
            obj.AddComponent<LootDropper>();

            return obj;
        }

        private void OnDrawGizmosSelected()
        {
            // Hiển thị vòng tròn trực quan trong Scene View theo màu phân vùng
            switch (zone)
            {
                case GameZone.Grassland_Lv1_10:
                    Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.4f);
                    break;
                case GameZone.DeepForest_Lv10_20:
                    Gizmos.color = new Color(0.2f, 0.4f, 0.8f, 0.4f);
                    break;
                case GameZone.BossArena:
                    Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.4f);
                    break;
                default:
                    Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
                    break;
            }

            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
