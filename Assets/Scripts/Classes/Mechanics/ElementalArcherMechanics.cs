using System;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    public enum ElementalArrowType
    {
        Flame,  // Hỏa Tên: Sát thương bùng nổ diện rộng 2.5m (Burn DoT)
        Water,  // Thủy Tên: Làm chậm 35% MoveSpeed và 20% AtkSpeed
        Wind,   // Phong Tên: Đẩy lùi 2.5m và ngắt chiêu niệm chú
        Terra   // Thổ Tên: Bỏ qua 40% giáp (Armor Break)
    }

    /// <summary>
    /// Xử lý cơ chế Xạ Thủ Nguyên Tố:
    /// 1. Tốc độ bắn cố định ở mức 1.0 hit/s (không tăng theo trang bị).
    /// 2. Chuyển hóa Attack Speed dư thừa thành Sát thương Nguyên tố (1% AtkSpd -> 1.5% ElemDmg).
    /// 3. Lựa chọn và bắn 4 loại Mũi Tên Nguyên Tố (Hỏa, Thủy, Phong, Thổ).
    /// Kèm 3 Ấn nội tại: Chuẩn Xác Tuyệt Đối, Khắc Chế Điểm Yếu, Dẫn Tiễn Hiệp Đồng.
    /// </summary>
    public class ElementalArcherMechanics : MonoBehaviour
    {
        [Header("Tốc Độ Bắn Cố Định")]
        public float fixedAttackInterval = 1.0f; // Khóa cứng 1 hit/s
        private float attackTimer = 0f;

        [Header("Loại Mũi Tên Đang Chọn")]
        public ElementalArrowType currentArrow = ElementalArrowType.Flame;

        [Header("Ấn 1: Chuẩn Xác Tuyệt Đối (Chuỗi Bắn Cùng Hệ)")]
        private ElementalArrowType lastArrowType;
        private int consecutiveArrowHits = 0;

        public event Action<ElementalArrowType> OnArrowSelected;

        private void Update()
        {
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }

        /// <summary>
        /// Chọn loại mũi tên bắn (Slot 1-4).
        /// </summary>
        public void SelectArrow(ElementalArrowType arrowType)
        {
            currentArrow = arrowType;
            Debug.Log($"[ElementalArcher] Đã chọn mũi tên: {currentArrow}!");
            OnArrowSelected?.Invoke(currentArrow);
        }

        /// <summary>
        /// Chuyển đổi Attack Speed từ trang bị sang % Sát Thương Nguyên Tố theo tỷ lệ 1% -> 1.5%.
        /// </summary>
        public float ConvertAttackSpeedToElementalDamage(float bonusAttackSpeedPercent)
        {
            // 1% Attack Speed -> 1.5% Elemental Damage
            return bonusAttackSpeedPercent * 1.5f;
        }

        /// <summary>
        /// Bắn tên theo nhịp cố định 1.0s. Trả true nếu đủ điều kiện khai hỏa.
        /// Kiểm tra Ấn 1: Bắn 3 phát cùng hệ thì phát thứ 4 chắc chắn bạo kích.
        /// </summary>
        public bool TryShootArrow(out bool isGuaranteedCrit)
        {
            isGuaranteedCrit = false;
            if (attackTimer > 0f) return false;

            attackTimer = fixedAttackInterval;

            // Xử lý Ấn 1: Chuẩn Xác Tuyệt Đối
            if (currentArrow == lastArrowType)
            {
                consecutiveArrowHits++;
                if (consecutiveArrowHits >= 3)
                {
                    isGuaranteedCrit = true;
                    consecutiveArrowHits = 0;
                    Debug.Log($"<color=yellow>[ElementalArcher] 🎯 PHÁT BẮN HOÀN HẢO! Ấn 1 chắc chắn kích hoạt BẠO KÍCH NGUYÊN TỐ!</color>");
                }
            }
            else
            {
                lastArrowType = currentArrow;
                consecutiveArrowHits = 1;
            }

            Debug.Log($"[ElementalArcher] Bắn {currentArrow} Tên! (Chuỗi cùng hệ: {consecutiveArrowHits}/3)");
            return true;
        }

        /// <summary>
        /// Lấy nguyên tố tương ứng với loại mũi tên đang dùng.
        /// </summary>
        public ElementType GetCurrentArrowElement()
        {
            return currentArrow switch
            {
                ElementalArrowType.Flame => ElementType.Fire,
                ElementalArrowType.Water => ElementType.Water,
                ElementalArrowType.Wind => ElementType.Wind,
                ElementalArrowType.Terra => ElementType.Earth,
                _ => ElementType.None
            };
        }
    }
}
