#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Combat;
using ActionRPG.Environment;
using ActionRPG.Items;
using ActionRPG.Items.Loot;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Công cụ sinh tự động toàn bộ Asset, ScriptableObject và Prefab mẫu của Sprint 3:
    /// - 2 Bình dược phẩm (Bình Máu Nhỏ, Bình Mana Nhỏ với Cooldown 8s).
    /// - 3 Nguyên liệu pha chế Luyện Dược Sư (Giọt nước tinh khiết, Bột thảo mộc, Bột đá khoáng).
    /// - 3 Trang bị mẫu với ràng buộc Level & Tier (Tier 1 Novice, Tier 2 Basic, Tier 3 Lumina).
    /// - 3 Prefab mẫu chuẩn 2D: WorldItem chuẩn, Bụi Thảo Dược mẫu, Node Quặng mẫu.
    /// </summary>
    public static class Sprint3AssetGenerator
    {
        private const string ItemFolder = "Assets/ScriptableObjects/Items";
        private const string PrefabFolder = "Assets/Prefabs";

        [MenuItem("Tools/ActionRPG/Generate Sprint 3 Assets & Prefabs")]
        [MenuItem("Tools/Lumina/Generate Sprint 3 Assets")]
        public static void GenerateAllSprint3Assets()
        {
            EnsureDirectories();

            // 1. Tạo 2 Bình Dược Phẩm (Sprint 3: Cooldown 8s)
            ConsumableItemData hpPotion = CreateConsumableAsset("potion_small_hp", "Bình Máu Nhỏ", "Bình dược phẩm tinh chế đỏ thắm, hồi phục tức thì 50 HP. Hồi chiêu 8 giây.", 50f, 0f, 8.0f, true, false, ItemRarity.Common, 15, 8);
            ConsumableItemData mpPotion = CreateConsumableAsset("potion_small_mp", "Bình Mana Nhỏ", "Bình dược phẩm thanh lam, hồi phục tức thì 30 MP. Hồi chiêu 8 giây.", 0f, 30f, 8.0f, false, true, ItemRarity.Common, 15, 8);

            // 2. Tạo 3 Nguyên Liệu Pha Chế Luyện Dược Sư
            ItemData herbMat = CreateMaterialAsset("mat_herb_fiber", "Bột Thảo Mộc", "Dược liệu thảo mộc nghiền khô từ đồng cỏ, dược tính thanh lọc cơ bản.", ItemRarity.Common, 8, 4);
            ItemData stoneMat = CreateMaterialAsset("mat_stone_powder", "Bột Đá Khoáng", "Khoáng thạch nguyên sinh nghiền mịn, hỗ trợ kết tinh và ngưng tụ dược chất.", ItemRarity.Common, 10, 5);
            ItemData waterMat = CreateMaterialAsset("mat_pure_water", "Giọt Nước Tinh Khiết", "Giọt sương nước suối ngọt lành, dung môi tuyệt hảo cho mọi bài thuốc.", ItemRarity.Common, 12, 6);

            // 3. Tạo Trang Bị Ràng Buộc Level & Tier
            CreateEquipmentAsset("equip_novice_sword", "Kiếm Gỗ Tân Thủ", EquipSlot.Weapon, 1, EvolutionTier.Tier1_Novice, false, 8f, 0f, 0f, 0f, ItemRarity.Common);
            CreateEquipmentAsset("equip_iron_armor", "Giáp Sắt Rèn", EquipSlot.Armor, 10, EvolutionTier.Tier2_BasicClass, false, 0f, 15f, 60f, 0f, ItemRarity.Uncommon);
            CreateEquipmentAsset("equip_lumina_staff", "Trượng Pháp Lumina", EquipSlot.Weapon, 20, EvolutionTier.Tier3_LuminaClass, true, 45f, 0f, 0f, 100f, ItemRarity.Rare);

            // 4. Tạo 3 Prefab Mẫu Chuẩn 2D
            GenerateWorldItemPrefab(hpPotion);
            GenerateGatheringNodePrefab("Prefab_Gathering_HerbBush", GatheringNodeType.HerbBush, "Bụi Cây Thảo Mộc Mẫu", herbMat, new Color(0.2f, 0.85f, 0.35f));
            GenerateGatheringNodePrefab("Prefab_Gathering_OreVein", GatheringNodeType.OreVein, "Bãi Khoáng Thạch Mẫu", stoneMat, new Color(0.45f, 0.65f, 0.95f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>🎉 [Sprint3AssetGenerator] Đã tạo thành công toàn bộ Dược Phẩm, Nguyên Liệu, Trang Bị Ràng Buộc & 3 Prefab Mẫu Sprint 3!</color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(ItemFolder)) Directory.CreateDirectory(ItemFolder);
            if (!Directory.Exists(PrefabFolder)) Directory.CreateDirectory(PrefabFolder);
        }

        private static ConsumableItemData CreateConsumableAsset(string id, string name, string desc, float hp, float mp, float cd, bool isHp, bool isMp, ItemRarity rarity, int buy, int sell)
        {
            string path = $"{ItemFolder}/{id}.asset";
            ConsumableItemData item = AssetDatabase.LoadAssetAtPath<ConsumableItemData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ConsumableItemData>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemId = id;
            item.itemName = name;
            item.description = desc;
            item.itemType = ItemType.Potion;
            item.rarity = rarity;
            item.buyPrice = buy;
            item.sellPrice = sell;
            item.isStackable = true;
            item.maxStack = 99;
            item.healHpAmount = hp;
            item.restoreMpAmount = mp;
            item.cooldownTime = cd;
            item.isHpPotion = isHp;
            item.isMpPotion = isMp;

            EditorUtility.SetDirty(item);
            return item;
        }

        private static ItemData CreateMaterialAsset(string id, string name, string desc, ItemRarity rarity, int buy, int sell)
        {
            string path = $"{ItemFolder}/{id}.asset";
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemId = id;
            item.itemName = name;
            item.description = desc;
            item.itemType = ItemType.Material;
            item.rarity = rarity;
            item.buyPrice = buy;
            item.sellPrice = sell;
            item.isStackable = true;
            item.maxStack = 99;

            EditorUtility.SetDirty(item);
            return item;
        }

        private static EquipmentData CreateEquipmentAsset(string id, string name, EquipSlot slot, int reqLevel, EvolutionTier reqTier, bool isSpec, float atk, float def, float hp, float mp, ItemRarity rarity)
        {
            string path = $"{ItemFolder}/{id}.asset";
            EquipmentData item = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<EquipmentData>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemId = id;
            item.itemName = name;
            item.itemType = ItemType.Equipment;
            item.equipSlot = slot;
            item.requiredLevel = reqLevel;
            item.requiredTier = reqTier;
            item.isSpecializedWeapon = isSpec;
            item.bonusAtk = atk;
            item.bonusDef = def;
            item.bonusHp = hp;
            item.bonusMp = mp;
            item.rarity = rarity;
            item.isStackable = false;
            item.maxStack = 1;

            EditorUtility.SetDirty(item);
            return item;
        }

        private static void GenerateWorldItemPrefab(ItemData sampleItem)
        {
            string prefabPath = $"{PrefabFolder}/Prefab_WorldItem_Standard.prefab";
            GameObject root = new GameObject("WorldItem_Standard");

            var collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.45f;

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;
            renderer.sprite = WorldItem.CreateCircleSprite();
            renderer.color = Color.white;

            // Glow Aura child
            GameObject glowObj = new GameObject("GlowAura");
            glowObj.transform.SetParent(root.transform, false);
            glowObj.transform.localScale = Vector3.one * 1.5f;
            var glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            glowRenderer.sortingOrder = 4;
            glowRenderer.sprite = WorldItem.CreateGlowSprite();
            glowRenderer.color = new Color(1f, 1f, 1f, 0.45f);

            var wi = root.AddComponent<WorldItem>();
            wi.mainRenderer = renderer;
            wi.glowRenderer = glowRenderer;
            wi.itemData = sampleItem;
            wi.quantity = 1;
            wi.magnetRadius = 2.5f;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void GenerateGatheringNodePrefab(string prefabName, GatheringNodeType nodeType, string displayName, ItemData dropItem, Color nodeColor)
        {
            string prefabPath = $"{PrefabFolder}/{prefabName}.prefab";
            GameObject root = new GameObject(prefabName);

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.2f;

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 2;
            renderer.sprite = WorldItem.CreateCircleSprite();
            renderer.color = nodeColor;

            var node = root.AddComponent<GatheringNode>();
            node.nodeType = nodeType;
            node.nodeName = displayName;
            node.harvestedItem = dropItem;
            node.minQuantity = 1;
            node.maxQuantity = 3;
            node.interactDistance = 2.0f;
            node.channelDuration = 1.0f;
            node.respawnCooldown = 30.0f;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
#endif
