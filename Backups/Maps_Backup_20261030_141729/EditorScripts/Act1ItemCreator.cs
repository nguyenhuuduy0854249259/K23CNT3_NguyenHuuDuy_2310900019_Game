using UnityEngine;
using UnityEditor;
using ActionRPG.Items;
using ActionRPG.Combat;
using ActionRPG.Characters;

namespace ActionRPG.Editor
{
    public static class Act1ItemCreator
    {
        [MenuItem("ActionRPG/Tao Asset Vat Pham va Trang Bi Hoi 1")]
        public static void CreateAllAct1Items()
        {
            string itemDir = "Assets/Resources/Items";
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(itemDir)) AssetDatabase.CreateFolder("Assets/Resources", "Items");

            // 1. Starter Sword (Kiếm Sắt Rỉ)
            CreateWeapon("Starter_Sword", "Kiếm Sắt Rỉ", ItemRarity.Common, 
                "Thanh kiếm sắt cơ bản phát cho lính mới. Tuy hoen rỉ nhưng lưỡi kiếm vẫn đủ bén để hạ gục lũ quái vật cấp thấp.",
                "Assets/Sprites/Items/icon_weapon_starter_sword.png", 1, 15, 8, 8f, 0f, 0.05f);

            // 2. Leather Armor (Giáp Da Thô)
            CreateArmor("armor_leather_novice", "Áo Giáp Da Thô", ItemRarity.Common, EquipSlot.Armor,
                "Áo giáp làm từ da thú thường khâu lại. Cung cấp khả năng bảo vệ cơ bản trước móng vuốt dã thú.",
                "Assets/Sprites/Items/icon_armor_leather_novice.png", 1, 20, 10, 0f, 6f, 30f, 0f, 0f);

            // 3. Leather Helmet (Mũ Da Thợ Săn)
            CreateArmor("helmet_leather_novice", "Mũ Da Thợ Săn", ItemRarity.Common, EquipSlot.Helmet,
                "Mũ trùm đầu làm bằng da mềm, nhẹ và thoáng khí, giúp người đội dễ dàng quan sát trong rừng rậm.",
                "Assets/Sprites/Items/icon_helmet_leather_novice.png", 1, 15, 7, 0f, 3f, 15f, 0f, 0f);

            // 4. Leather Gloves (Găng Da Bền)
            CreateArmor("gloves_leather_novice", "Găng Tay Da Bền", ItemRarity.Common, EquipSlot.Gloves,
                "Đôi găng tay da ôm sát lòng bàn tay, hỗ trợ cầm chắc chuôi kiếm và kéo cung mà không lo trầy xước.",
                "Assets/Sprites/Items/icon_gloves_leather_novice.png", 1, 12, 6, 2f, 2f, 10f, 0f, 0f);

            // 5. Leather Boots (Ủng Da Đi Rừng)
            CreateArmor("boots_leather_novice", "Ủng Da Đi Rừng", ItemRarity.Common, EquipSlot.Boots,
                "Đôi ủng đế cao su dày chống trơn trượt trên bùn lầy, tăng tốc độ di chuyển khi thám hiểm thảo nguyên.",
                "Assets/Sprites/Items/icon_boots_leather_novice.png", 1, 15, 7, 0f, 2f, 10f, 0f, 0.5f);

            // 6. Small HP Potion (Bình Máu Nhỏ)
            CreateConsumable("potion_small_hp", "Bình Máu Nhỏ", ItemType.Potion, ItemRarity.Common,
                "Chất lỏng màu đỏ chiết xuất từ thảo mộc tươi. Lập tức hồi phục 60 HP khi uống.",
                "Assets/Sprites/Items/icon_potion_small_hp.png", 10, 5, true, 99);

            // 7. Small MP Potion (Bình Năng Lượng Nhỏ)
            CreateConsumable("potion_small_mp", "Bình Năng Lượng Nhỏ", ItemType.Potion, ItemRarity.Common,
                "Dược thủy màu lam phát sáng lấp lánh. Lập tức hồi phục 40 MP giúp thi triển kỹ năng liên tục.",
                "Assets/Sprites/Items/icon_potion_small_mp.png", 12, 6, true, 99);

            // 8. Herb Leaf (Thảo Mộc Xanh)
            CreateConsumable("material_herb_leaf", "Thảo Mộc Xanh Oakhaven", ItemType.Material, ItemRarity.Common,
                "Lá thảo mộc mọc nhiều ven hồ và bờ suối thảo nguyên, nguyên liệu chính để chế tạo bình hồi máu.",
                "Assets/Sprites/Items/icon_material_herb_leaf.png", 5, 2, true, 99);

            // 9. Copper Ore (Mẩu Quặng Đồng Thô)
            CreateConsumable("material_copper_ore", "Mẩu Quặng Đồng Thô", ItemType.Material, ItemRarity.Common,
                "Khoáng thạch đồng đỏ khai thác từ các vách đá thảo nguyên. Mang cho Thợ Rèn Barnaby để chế tạo hợp kim.",
                "Assets/Sprites/Items/icon_material_copper_ore.png", 6, 3, true, 99);

            // 10. Iron Ore (Mẩu Quặng Sắt Thô)
            CreateConsumable("material_iron_ore", "Mẩu Quặng Sắt Thô", ItemType.Material, ItemRarity.Common,
                "Khoáng thạch khai thác từ các vách đá thảo nguyên. Mang cho Thợ Rèn Barnaby để tinh luyện vũ khí.",
                "Assets/Sprites/Items/icon_material_iron_ore.png", 8, 4, true, 99);

            // 11. Animal Hide (Da Thú Rừng)
            CreateConsumable("material_animal_hide", "Tấm Da Thú Rừng", ItemType.Material, ItemRarity.Common,
                "Lớp da bền thu hoạch từ Dã Lang hoặc Chuột Chũi. Dùng để may vá mũ, áo giáp và ủng đi rừng.",
                "Assets/Sprites/Items/icon_material_animal_hide.png", 6, 3, true, 99);

            // 12. Broken Rune (Mảnh Phù Văn Vỡ Vụn)
            CreateConsumable("quest_broken_rune", "Mảnh Phù Văn Vỡ Vụn", ItemType.QuestItem, ItemRarity.Rare,
                "Mảnh đá cổ khắc cổ tự phát sáng ánh tím bí ẩn rơi ra từ Chúa Tể Sói Bóng Tối. Chứa manh mối Hồi 2.",
                "Assets/Sprites/Items/icon_quest_broken_rune.png", 0, 0, true, 10);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>Đã tạo thành công toàn bộ ScriptableObject Item và Equipment Hồi 1 tại Assets/Resources/Items!</color>");
        }

        private static void CreateWeapon(string id, string name, ItemRarity rarity, string desc, string spritePath, int reqLv, int buy, int sell, float atk, float def, float crit)
        {
            string assetPath = $"Assets/Resources/Items/{id}.asset";
            var item = AssetDatabase.LoadAssetAtPath<EquipmentData>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<EquipmentData>();
                AssetDatabase.CreateAsset(item, assetPath);
            }
            item.itemId = id;
            item.itemName = name;
            item.itemType = ItemType.Equipment;
            item.rarity = rarity;
            item.description = desc;
            item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            item.visualSprite = item.icon;
            item.equipSlot = EquipSlot.Weapon;
            item.requiredLevel = reqLv;
            item.buyPrice = buy;
            item.sellPrice = sell;
            item.bonusAtk = atk;
            item.bonusDef = def;
            item.bonusCritRate = crit;
            EditorUtility.SetDirty(item);
        }

        private static void CreateArmor(string id, string name, ItemRarity rarity, EquipSlot slot, string desc, string spritePath, int reqLv, int buy, int sell, float atk, float def, float hp, float mp, float speed)
        {
            string assetPath = $"Assets/Resources/Items/{id}.asset";
            var item = AssetDatabase.LoadAssetAtPath<EquipmentData>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<EquipmentData>();
                AssetDatabase.CreateAsset(item, assetPath);
            }
            item.itemId = id;
            item.itemName = name;
            item.itemType = ItemType.Equipment;
            item.rarity = rarity;
            item.description = desc;
            item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            item.visualSprite = item.icon;
            item.equipSlot = slot;
            item.requiredLevel = reqLv;
            item.buyPrice = buy;
            item.sellPrice = sell;
            item.bonusAtk = atk;
            item.bonusDef = def;
            item.bonusHp = hp;
            item.bonusMp = mp;
            item.bonusSpeed = speed;
            EditorUtility.SetDirty(item);
        }

        private static void CreateConsumable(string id, string name, ItemType type, ItemRarity rarity, string desc, string spritePath, int buy, int sell, bool stackable, int maxStack)
        {
            string assetPath = $"Assets/Resources/Items/{id}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(item, assetPath);
            }
            item.itemId = id;
            item.itemName = name;
            item.itemType = type;
            item.rarity = rarity;
            item.description = desc;
            item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            item.buyPrice = buy;
            item.sellPrice = sell;
            item.isStackable = stackable;
            item.maxStack = maxStack;
            EditorUtility.SetDirty(item);
        }
    }
}
