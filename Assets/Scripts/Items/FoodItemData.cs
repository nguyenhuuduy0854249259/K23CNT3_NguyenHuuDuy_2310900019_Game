using UnityEngine;

namespace ActionRPG.Items
{
    public enum FoodCategory
    {
        NormalFood,         // Loại 1: Chống đói đơn thuần (Hồi thanh no bụng, không buff chỉ số)
        BuffAndHungerFood,  // Loại 2: Vừa chống đói vừa tăng chỉ số thuộc tính
        PureBuffFood        // Loại 3: Thực phẩm đặc biệt/Thuốc bổ, chỉ tăng thuộc tính (không chống đói)
    }

    public enum BuffStatType
    {
        None,
        AttackPower,
        DefensePower,
        MoveSpeed,
        CritRate,
        MaxHpBuff,
        MaxMpBuff
    }

    [CreateAssetMenu(fileName = "NewFoodItem", menuName = "ActionRPG/Food Item")]
    public class FoodItemData : ItemData
    {
        [Header("Cơ Chế Đói & Dinh Dưỡng")]
        public FoodCategory category = FoodCategory.NormalFood;
        [Range(0f, 100f)] public float hungerRestoreAmount = 30f; // Điểm hồi phục thanh đói

        [Header("Hiệu Ứng Tăng Thuộc Tính")]
        public BuffStatType buffStat = BuffStatType.None;
        public float buffValue = 0f; // Giá trị buff (ví dụ +15 công hoặc +20% tốc)
        public float buffDurationSeconds = 300f; // Thời gian duy trì (ví dụ 5 phút = 300s)

        private void OnEnable()
        {
            itemType = ItemType.Food;
            isStackable = true;
            maxStack = 50;

            // Nếu là loại 3: thuần thuộc tính không chống đói
            if (category == FoodCategory.PureBuffFood)
            {
                hungerRestoreAmount = 0f;
            }
        }
    }
}
