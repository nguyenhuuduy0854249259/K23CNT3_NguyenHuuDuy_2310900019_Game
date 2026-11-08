#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Enemies;
using ActionRPG.Items;

namespace ActionRPG.Editor
{
    public static class Sprint2AssetGenerator
    {
        private const string EnemyAssetFolder = "Assets/ScriptableObjects/Enemies";
        private const string ItemAssetFolder = "Assets/ScriptableObjects/Items";

        [MenuItem("Tools/ActionRPG/Generate Sprint 2 Enemy & Material Assets")]
        public static void GenerateAllSprint2Assets()
        {
            EnsureDirectories();

            // 1. Tạo 3 nguyên liệu thu gom Dược Sư Chum
            ItemData herbItem = CreateMaterialAsset("mat_herb_fiber", "Sợi Thảo Mộc", "Sợi cỏ dược liệu tươi tốt thu hái từ thảo nguyên xanh, làm thuốc cơ bản.", ItemRarity.Common, 6, 3);
            ItemData stoneItem = CreateMaterialAsset("mat_stone_powder", "Bột Đá", "Khoáng thạch nghiền mịn làm chất xúc tác gia cố cấu trúc bài thuốc.", ItemRarity.Common, 8, 4);
            ItemData waterItem = CreateMaterialAsset("mat_pure_water", "Giọt Nước Tinh Khiết", "Giọt sương nước suối thanh khiết dùng làm dung môi điều hòa.", ItemRarity.Common, 10, 5);

            // 2. Tạo 5 Quái Thảo Nguyên
            CreateEnemyAsset("enemy_steppe_slime", "Slime Xanh Thảo Nguyên", 2, ElementType.Water, MorphologicalGroup.TinhLinhTheLong, 45f, 7f, 2f, 2.3f, 15, 8, true, 0.45f, waterItem);
            CreateEnemyAsset("enemy_steppe_hedgehog", "Nhím Gai Cỏ", 4, ElementType.Wind, MorphologicalGroup.Thu4Chan, 75f, 12f, 5f, 2.8f, 25, 14, true, 0.35f, herbItem);
            CreateEnemyAsset("enemy_steppe_boar", "Heo Rừng Đất", 6, ElementType.Earth, MorphologicalGroup.Thu4Chan, 120f, 16f, 7f, 3.2f, 40, 20, true, 0.30f, stoneItem);
            CreateEnemyAsset("enemy_steppe_sprout", "Mầm Cây Tinh Linh", 5, ElementType.Earth, MorphologicalGroup.TinhLinhTheLong, 90f, 14f, 4f, 2.5f, 32, 16, true, 0.40f, herbItem);
            CreateEnemyAsset("enemy_steppe_rock_crab", "Cua Bọc Đá Cổ Đại", 9, ElementType.Earth, MorphologicalGroup.GiapXacBoSat, 260f, 22f, 14f, 2.0f, 90, 55, true, 0.15f, stoneItem, false, true);

            // 3. Tạo 13 Quái/Thú Rừng Rậm
            CreateEnemyAsset("enemy_forest_fire_wolf", "Sói Lửa", 11, ElementType.Fire, MorphologicalGroup.Thu4Chan, 160f, 22f, 6f, 3.8f, 55, 30, true, 0.35f, herbItem);
            CreateEnemyAsset("enemy_forest_night_owl", "Cú Đêm", 12, ElementType.Demon, MorphologicalGroup.CoCanh, 140f, 25f, 5f, 3.5f, 60, 32, true, 0.30f, waterItem, true);
            CreateEnemyAsset("enemy_forest_thorn_snake", "Rắn Gai", 13, ElementType.Poison, MorphologicalGroup.GiapXacBoSat, 150f, 24f, 6f, 3.0f, 65, 35, true, 0.30f, herbItem);
            CreateEnemyAsset("enemy_forest_dark_boar", "Lợn Lòi Hắc Ám", 14, ElementType.Demon, MorphologicalGroup.Thu4Chan, 220f, 28f, 9f, 3.4f, 75, 40, true, 0.25f, stoneItem);
            CreateEnemyAsset("enemy_forest_thunder_beetle", "Bọ Cánh Cứng Sấm", 13, ElementType.Electric, MorphologicalGroup.GiapXacBoSat, 170f, 23f, 11f, 2.6f, 70, 38, true, 0.28f, stoneItem);
            CreateEnemyAsset("enemy_forest_fire_cat", "Hỏa Miêu", 15, ElementType.Fire, MorphologicalGroup.Thu4Chan, 180f, 30f, 7f, 4.2f, 85, 45, true, 0.25f, herbItem);
            CreateEnemyAsset("enemy_forest_thunder_eagle", "Ưng Sấm", 16, ElementType.Electric, MorphologicalGroup.CoCanh, 210f, 32f, 8f, 4.0f, 95, 50, true, 0.22f, waterItem, true);
            CreateEnemyAsset("enemy_forest_snow_leopard", "Báo Tuyết", 17, ElementType.Water, MorphologicalGroup.Thu4Chan, 240f, 35f, 9f, 4.4f, 110, 60, true, 0.20f, waterItem);
            CreateEnemyAsset("enemy_forest_fire_mantis", "Bọ Ngựa Lửa", 16, ElementType.Fire, MorphologicalGroup.GiapXacBoSat, 200f, 36f, 8f, 3.6f, 100, 52, true, 0.22f, herbItem);
            CreateEnemyAsset("enemy_forest_spore_mushroom", "Bào Tử Nấm", 15, ElementType.Poison, MorphologicalGroup.TinhLinhTheLong, 190f, 26f, 7f, 2.4f, 80, 42, true, 0.30f, herbItem, true);
            CreateEnemyAsset("enemy_forest_treant", "Người Cây", 18, ElementType.Earth, MorphologicalGroup.TinhLinhTheLong, 340f, 30f, 15f, 2.0f, 125, 70, true, 0.18f, stoneItem);
            CreateEnemyAsset("enemy_forest_pure_deer", "Hươu Tinh Khôi", 18, ElementType.Angel, MorphologicalGroup.Thu4Chan, 260f, 28f, 10f, 3.9f, 130, 75, true, 0.20f, waterItem);

            // Boss Chúa Tể Rừng Rậm: Hắc Diệu Ma Viên (Lv 20 - Hệ Hỏa/Ám)
            CreateEnemyAsset("boss_black_demon_ape", "Hắc Diệu Ma Viên", 20, ElementType.Fire, MorphologicalGroup.Thu4Chan, 650f, 28f, 8f, 3.2f, 350, 200, false, 0f, stoneItem, false, true, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>🎉 [Sprint2AssetGenerator] Đã tạo thành công toàn bộ 5 Quái Thảo Nguyên, 13 Quái Rừng Rậm và 3 Vật Phẩm Thu Gom!</color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(EnemyAssetFolder))
            {
                Directory.CreateDirectory(EnemyAssetFolder);
            }
            if (!Directory.Exists(ItemAssetFolder))
            {
                Directory.CreateDirectory(ItemAssetFolder);
            }
        }

        private static ItemData CreateMaterialAsset(string id, string name, string desc, ItemRarity rarity, int buy, int sell)
        {
            string path = $"{ItemAssetFolder}/{id}.asset";
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

        private static EnemyData CreateEnemyAsset(
            string id, string name, int level, ElementType element, MorphologicalGroup morph,
            float hp, float atk, float def, float speed, int exp, int gold,
            bool capturable, float captureRate, ItemData dropItem,
            bool isRanged = false, bool isAreaBoss = false, bool isWorldBoss = false)
        {
            string path = $"{EnemyAssetFolder}/{id}.asset";
            EnemyData enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(enemy, path);
            }

            enemy.enemyId = id;
            enemy.enemyName = name;
            enemy.level = level;
            enemy.element = element;
            enemy.morphologicalGroup = morph;
            enemy.maxHp = hp;
            enemy.attackPower = atk;
            enemy.defense = def;
            enemy.moveSpeed = speed;
            enemy.expReward = exp;
            enemy.goldReward = gold;
            enemy.isCapturable = capturable;
            enemy.baseCaptureRate = captureRate;
            enemy.isRanged = isRanged;
            enemy.isAreaBoss = isAreaBoss;
            enemy.isWorldBoss = isWorldBoss;

            enemy.dropTable.Clear();
            if (dropItem != null)
            {
                enemy.dropTable.Add(new DropEntry
                {
                    item = dropItem,
                    dropChancePercent = 50f,
                    minQuantity = 1,
                    maxQuantity = isAreaBoss ? 3 : 2
                });
            }

            EditorUtility.SetDirty(enemy);
            return enemy;
        }
    }
}
#endif
