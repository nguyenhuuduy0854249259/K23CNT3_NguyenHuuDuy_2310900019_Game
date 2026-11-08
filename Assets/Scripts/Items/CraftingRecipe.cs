using UnityEngine;
using System.Collections.Generic;

namespace ActionRPG.Items
{
    [System.Serializable]
    public class CraftingIngredient
    {
        public ItemData item;
        public int amount = 1;
    }

    /// <summary>
    /// Cấu trúc công thức chế tạo hỗ trợ kết hợp nhiều ItemData riêng rẽ.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "ActionRPG/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Thông Tin Công Thức")]
        public string recipeId;
        public string recipeName;
        
        [Header("Nguyên Liệu Đầu Vào")]
        public List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
        public int goldCost = 0;
        
        [Header("Sản Phẩm Đầu Ra")]
        public ItemData resultItem;
        public int resultAmount = 1;
        
        [Header("Yêu Cầu Nâng Cao (Tùy chọn)")]
        [Tooltip("Cấp độ nghề nghiệp (thợ rèn/thuốc) cần thiết để chế tạo")]
        public int requiredCrafterLevel = 1;
    }
}
