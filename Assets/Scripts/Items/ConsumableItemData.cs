using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Items
{
    /// <summary>
    /// Dữ liệu Dược phẩm / Tiêu hao (Bình Máu, Bình Mana, Thuốc bổ trợ) chuẩn Sprint 3.
    /// Cho phép hồi phục HP/MP và tích hợp thời gian hồi chiêu chung (Cooldown 8s).
    /// </summary>
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "ActionRPG/Consumable Item")]
    public class ConsumableItemData : ItemData
    {
        [Header("Chỉ Số Hồi Phục & Cooldown (Sprint 3)")]
        [Tooltip("Lượng máu hồi phục ngay lập tức")]
        public float healHpAmount = 50f;

        [Tooltip("Lượng năng lượng hồi phục ngay lập tức")]
        public float restoreMpAmount = 0f;

        [Tooltip("Thời gian hồi chiêu chung của bình thuốc (Mặc định: 8 giây)")]
        public float cooldownTime = 8.0f;

        [Header("Phân Loại Dược Phẩm")]
        public bool isHpPotion = true;
        public bool isMpPotion = false;

        private void OnEnable()
        {
            itemType = ItemType.Potion;
            isStackable = true;
            maxStack = 99;
        }

        /// <summary>
        /// Kích hoạt sử dụng bình dược phẩm lên mục tiêu CharacterStats.
        /// </summary>
        public bool Use(CharacterStats target)
        {
            if (target == null) return false;

            bool effectApplied = false;

            if (healHpAmount > 0)
            {
                target.HealHp(healHpAmount);
                effectApplied = true;
            }

            if (restoreMpAmount > 0)
            {
                target.RestoreMp(restoreMpAmount);
                effectApplied = true;
            }

            if (effectApplied)
            {
                EventBus.OnPlaySound?.Invoke("potion");
            }

            return effectApplied;
        }
    }
}
