using UnityEngine;

namespace ActionRPG.Items
{
    public enum ItemRarity
    {
        Common,     // Trắng (Thường)
        Uncommon,   // Xanh Lục (Tinh Anh)
        Rare,       // Xanh Lam (Hiếm)
        Epic,       // Tím (Sử Thi)
        Legendary,  // Cam (Huyền Thoại)
        Unique      // Đỏ (Độc Bản / Signature Unique)
    }

    public enum ItemType
    {
        Equipment,
        Food,
        Potion,
        Material,
        CaptureScroll,
        QuestItem
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "ActionRPG/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public ItemRarity rarity;
        [TextArea] public string description;
        public Sprite icon;
        public int buyPrice = 10;
        public int sellPrice = 5;
        public bool isStackable = true;
        public int maxStack = 99;

        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common: return Color.white;
                case ItemRarity.Uncommon: return new Color(0.2f, 0.85f, 0.3f);
                case ItemRarity.Rare: return new Color(0.2f, 0.6f, 1f);
                case ItemRarity.Epic: return new Color(0.7f, 0.25f, 0.95f);
                case ItemRarity.Legendary: return new Color(1f, 0.6f, 0.1f);
                case ItemRarity.Unique: return new Color(1f, 0.15f, 0.2f);
                default: return Color.white;
            }
        }
    }
}
