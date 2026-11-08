using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Items;

namespace ActionRPG.Crafting
{
    [Serializable]
    public class AlchemyRecipe
    {
        public string potionName;
        public ItemData resultPotion;
        public List<MaterialRequirement> herbsAndCrystals = new List<MaterialRequirement>();
        public int goldCost = 30;
    }

    public class AlchemySystem : MonoBehaviour
    {
        public static AlchemySystem Instance { get; private set; }

        public List<AlchemyRecipe> knownRecipes = new List<AlchemyRecipe>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public bool BrewPotion(AlchemyRecipe recipe)
        {
            if (recipe == null || recipe.resultPotion == null) return false;
            if (Inventory.Instance == null) return false;

            if (Inventory.Instance.gold < recipe.goldCost)
            {
                Debug.LogWarning("[Giả Kim Thuật] Không đủ vàng để điều chế dược phẩm!");
                return false;
            }

            foreach (var req in recipe.herbsAndCrystals)
            {
                if (!Inventory.Instance.HasItem(req.material.itemId, req.requiredCount))
                {
                    Debug.LogWarning($"[Giả Kim Thuật] Thiếu dược liệu: {req.material.itemName} ({req.requiredCount})!");
                    return false;
                }
            }

            Inventory.Instance.SpendGold(recipe.goldCost);
            foreach (var req in recipe.herbsAndCrystals)
            {
                Inventory.Instance.RemoveItem(req.material, req.requiredCount);
            }

            Inventory.Instance.AddItem(recipe.resultPotion, 1);
            Debug.Log($"<color=green>[Giả Kim Thuật] ĐIỀU CHẾ THÀNH CÔNG: {recipe.resultPotion.itemName}!</color>");
            return true;
        }
    }
}
