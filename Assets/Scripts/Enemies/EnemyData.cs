using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Items;

namespace ActionRPG.Enemies
{
    public enum MorphologicalGroup
    {
        None,
        Thu4Chan,          // Thú 4 chân (Sói, Heo rừng, Báo, Hươu...)
        CoCanh,            // Động vật có cánh (Cú đêm, Ưng sấm...)
        GiapXacBoSat,      // Giáp xác / Bò sát (Rắn gai, Bọ cánh cứng, Bọ ngựa, Cua đá...)
        TinhLinhTheLong    // Tinh linh thể lỏng / Thực vật (Slime, Mầm cây, Bào tử nấm, Người cây...)
    }

    [System.Serializable]
    public class DropEntry
    {
        public ItemData item;
        [Range(0f, 100f)] public float dropChancePercent = 20f;
        public int minQuantity = 1;
        public int maxQuantity = 1;
    }

    [CreateAssetMenu(fileName = "NewEnemy", menuName = "ActionRPG/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Nhận Diện & Phân Loại")]
        public string enemyId;
        public string enemyName;
        public int level = 1;
        public int enemyLevel { get => level; set => level = value; }
        public ElementType element = ElementType.None;
        public MorphologicalGroup morphologicalGroup = MorphologicalGroup.None;
        public Sprite sprite;

        [Header("Chỉ Số Quái")]
        public float maxHp = 50f;

        [Tooltip("Sát thương vật lý (Physical Attack)")]
        public float physicalAtk = 8f;

        [Tooltip("Sát thương phép thuật (Magic Attack)")]
        public float magicAtk = 0f;

        [Tooltip("Giáp vật lý (Physical Defense)")]
        public float physicalDef = 2f;

        [Tooltip("Giáp phép / Kháng phép (Magic Defense)")]
        public float magicDef = 2f;

        [Tooltip("Tốc độ di chuyển")]
        public float moveSpeed = 2.8f;

        [Tooltip("Tốc độ tấn công")]
        public float attackSpeed = 1.0f;

        // Thuộc tính kế thừa tương thích ngược
        public float attackPower { get => physicalAtk; set => physicalAtk = value; }
        public float baseAtk { get => physicalAtk; set => physicalAtk = value; }
        public float defense { get => physicalDef; set => physicalDef = value; }
        public float baseDef { get => physicalDef; set => physicalDef = value; }

        public float detectionRadius = 6.0f;
        public float attackRange = 1.3f;
        public float attackCooldown = 1.8f;
        public bool isRanged = false;
        public float projectileSpeed = 6.0f;

        [Header("Thu Phục / Bắt Pet (Capture System)")]
        public bool isCapturable = true;
        [Range(0.05f, 1.0f)] public float baseCaptureRate = 0.25f; // Mặc định 0.1f - 0.5f

        [Header("Phần Thưởng Khi Bị Tiêu Diệt")]
        public int expReward = 25;
        public int goldReward = 15;

        [Header("Danh Sách Rớt Đồ (Drop Table)")]
        public List<DropEntry> dropTable = new List<DropEntry>();

        [Header("Phân Cấp Boss")]
        public bool isAreaBoss = false;
        public bool isWorldBoss = false;
    }
}
