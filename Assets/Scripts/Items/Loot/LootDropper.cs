using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Enemies;
using ActionRPG.Items;

namespace ActionRPG.Items.Loot
{
    /// <summary>
    /// Bộ rơi vật phẩm (Loot Dropper) gắn vào Quái vật (EnemyStats) chuẩn Sprint 3:
    /// - Khi quái chết (Die()): Đọc danh sách dropTable, tính toán tỷ lệ % xác suất rơi tiền vàng, nguyên liệu, phôi trang bị.
    /// - Tự động Spawn WorldItem văng nhẹ ngẫu nhiên trong bán kính 1.0m quanh xác quái.
    /// </summary>
    public class LootDropper : MonoBehaviour
    {
        [Header("Bảng Rơi Đồ (Drop Table)")]
        public List<DropEntry> dropTable = new List<DropEntry>();

        [Header("Cài Đặt Rơi Tiền Vàng")]
        public int minGold = 5;
        public int maxGold = 25;
        [Range(0f, 100f)] public float goldDropChance = 100f;

        [Header("Bán Kính Văng Đồ")]
        [Tooltip("Bán kính văng ngẫu nhiên quanh xác quái (Mặc định: 1.0m)")]
        public float popRadius = 1.0f;

        private EnemyStats enemyStats;

        private void Awake()
        {
            enemyStats = GetComponent<EnemyStats>();
            SyncFromEnemyStats();
        }

        public void SyncFromEnemyStats()
        {
            if (enemyStats == null) enemyStats = GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                if (enemyStats.dropTable != null && enemyStats.dropTable.Count > 0)
                {
                    dropTable = new List<DropEntry>(enemyStats.dropTable);
                }
                minGold = Mathf.Max(1, enemyStats.goldReward / 2);
                maxGold = Mathf.Max(minGold, enemyStats.goldReward);
            }
        }

        /// <summary>
        /// Kích hoạt tính toán xác suất và spawn toàn bộ vật phẩm rơi ra đất.
        /// </summary>
        public void DropLoot()
        {
            SyncFromEnemyStats();

            Vector3 spawnOrigin = transform.position;

            // 1. Tính toán rơi Tiền Vàng
            float goldRoll = Random.Range(0f, 100f);
            if (goldRoll <= goldDropChance)
            {
                int goldAmount = Random.Range(minGold, maxGold + 1);
                if (goldAmount > 0)
                {
                    WorldItem.SpawnGold(spawnOrigin, goldAmount, popRadius);
                }
            }

            // 2. Tính toán rơi Nguyên liệu, Dược liệu, Phôi Trang bị theo Drop Table
            if (dropTable != null && dropTable.Count > 0)
            {
                foreach (var entry in dropTable)
                {
                    if (entry == null || entry.item == null) continue;

                    float roll = Random.Range(0f, 100f);
                    if (roll <= entry.dropChancePercent)
                    {
                        int qty = Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                        if (qty > 0)
                        {
                            WorldItem.Spawn(spawnOrigin, entry.item, qty, popRadius);
                            Debug.Log($"<color=yellow>[LootDropper] Rơi {entry.item.itemName} x{qty} tại {spawnOrigin}!</color>");
                        }
                    }
                }
            }
        }
    }
}
