using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Items;

namespace ActionRPG.Crafting
{
    [Serializable]
    public class MaterialRequirement
    {
        public ItemData material;
        public int requiredCount = 2;
    }

    [Serializable]
    public class BlacksmithRecipe
    {
        public string recipeName;
        public EquipmentData resultEquipment;
        public List<MaterialRequirement> materials = new List<MaterialRequirement>();
        public int goldCost = 50;
    }

    public class BlacksmithSystem : MonoBehaviour
    {
        public static BlacksmithSystem Instance { get; private set; }

        public List<BlacksmithRecipe> knownRecipes = new List<BlacksmithRecipe>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public bool CraftEquipment(BlacksmithRecipe recipe)
        {
            if (recipe == null || recipe.resultEquipment == null) return false;
            if (Inventory.Instance == null) return false;

            // Kiểm tra vàng
            if (Inventory.Instance.gold < recipe.goldCost)
            {
                Debug.LogWarning("[Tiệm Rèn] Bạn không đủ vàng để rèn trang bị này!");
                return false;
            }

            // Kiểm tra nguyên liệu
            foreach (var req in recipe.materials)
            {
                if (!Inventory.Instance.HasItem(req.material.itemId, req.requiredCount))
                {
                    Debug.LogWarning($"[Tiệm Rèn] Thiếu nguyên liệu: {req.material.itemName} ({req.requiredCount})!");
                    return false;
                }
            }

            // Tiêu hao nguyên liệu & vàng
            Inventory.Instance.SpendGold(recipe.goldCost);
            foreach (var req in recipe.materials)
            {
                Inventory.Instance.RemoveItem(req.material, req.requiredCount);
            }

            // Tạo trang bị thành công
            Inventory.Instance.AddItem(recipe.resultEquipment, 1);
            Debug.Log($"<color=green>[Tiệm Rèn] RÈN THÀNH CÔNG: {recipe.resultEquipment.itemName}!</color>");
            return true;
        }

        public bool EnhanceEquipment(EquipmentData equipment)
        {
            if (equipment == null) return false;
            if (Inventory.Instance == null) return false;

            int enhanceCost = 100;
            if (Inventory.Instance.SpendGold(enhanceCost))
            {
                equipment.bonusAtk += 4f;
                equipment.bonusDef += 2f;
                Debug.Log($"<color=yellow>[Tiệm Rèn] Cường hóa thành công! {equipment.itemName} (+4 Công, +2 Thủ)!</color>");
                return true;
            }
            return false;
        }
    }
}
