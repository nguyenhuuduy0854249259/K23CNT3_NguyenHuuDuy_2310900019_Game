#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ActionRPG.Combat;
using ActionRPG.Classes;
using ActionRPG.Items;
using ActionRPG.Visuals;

namespace ActionRPG.Editor
{
    public static class SetupLuminaFireOrbAndVisuals
    {
        private const string VFX_DIR = "Assets/Prefabs/VFX";
        private const string SKILL_ASSET_PATH = "Assets/ScriptableObjects/Skills/Skill_mage_fire_orb.asset";

        [MenuItem("Tools/Lumina/Setup Fire Orb & 4Dir Visuals")]
        public static void SetupAll()
        {
            if (!Directory.Exists(VFX_DIR)) Directory.CreateDirectory(VFX_DIR);

            // 1. Tạo FireOrb_Explosion_VFX Prefab
            GameObject explosionPrefab = SetupFireOrbExplosionPrefab();

            // 2. Tạo FireOrb_Projectile Prefab
            GameObject projectilePrefab = SetupFireOrbProjectilePrefab(explosionPrefab);

            // 3. Liên kết vào Skill_mage_fire_orb.asset
            LinkFireOrbToSkillData(projectilePrefab, explosionPrefab);

            // 4. Cấu hình vũ khí 4 hướng trong EquipmentData
            SetupEquipmentDirectionalSprites();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green><b>[LUMINA] Đã khởi tạo hoàn tất Animation Hỏa Cầu (Fire Orb) & Visuals 4 Hướng!</b></color>");
        }

        private static GameObject SetupFireOrbExplosionPrefab()
        {
            string prefabPath = $"{VFX_DIR}/FireOrb_Explosion_VFX.prefab";
            string sheetPath = "Assets/Sprites/VFX/vfx_mage_fire_orb.png";

            Sprite[] expSprites = LoadSprites(sheetPath, new[] {
                "vfx_mage_fire_orb_4",
                "vfx_mage_fire_orb_5",
                "vfx_mage_fire_orb_6",
                "vfx_mage_fire_orb_7"
            });

            GameObject go = new GameObject("FireOrb_Explosion_VFX");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            if (expSprites.Length > 0) sr.sprite = expSprites[0];

            var anim = go.AddComponent<SimpleFrameAnimator2D>();
            anim.Initialize(expSprites, 14f, false, true);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"✅ Đã tạo Prefab Vụ Nổ Hỏa Cầu: {prefabPath}");
            return prefab;
        }

        private static GameObject SetupFireOrbProjectilePrefab(GameObject explosionPrefab)
        {
            string prefabPath = $"{VFX_DIR}/FireOrb_Projectile.prefab";
            string sheetPath = "Assets/Sprites/VFX/vfx_mage_fire_orb.png";

            Sprite[] flySprites = LoadSprites(sheetPath, new[] {
                "vfx_mage_fire_orb_0",
                "vfx_mage_fire_orb_1",
                "vfx_mage_fire_orb_2",
                "vfx_mage_fire_orb_3"
            });

            GameObject go = new GameObject("FireOrb_Projectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 14;
            if (flySprites.Length > 0) sr.sprite = flySprites[0];

            var anim = go.AddComponent<SimpleFrameAnimator2D>();
            anim.Initialize(flySprites, 12f, true, false);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var pb = go.AddComponent<ProjectileBehavior>();
            // ProjectileBehavior sẽ tự sinh explosionPrefab khi va chạm
            SerializedObject so = new SerializedObject(pb);
            so.Update();
            so.FindProperty("speed").floatValue = 14f;
            so.FindProperty("damage").floatValue = 25f;
            so.FindProperty("element").enumValueIndex = (int)ElementType.Fire;
            so.FindProperty("hitVfxPrefab").objectReferenceValue = explosionPrefab;
            so.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"✅ Đã tạo Prefab Đạn Bay Hỏa Cầu: {prefabPath}");
            return prefab;
        }

        private static void LinkFireOrbToSkillData(GameObject projPrefab, GameObject expPrefab)
        {
            SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>(SKILL_ASSET_PATH);
            if (skill != null)
            {
                skill.projectilePrefab = projPrefab;
                skill.hitVfxPrefab = expPrefab;
                EditorUtility.SetDirty(skill);
                Debug.Log($"✅ Đã liên kết Prefab Hỏa Cầu vào {SKILL_ASSET_PATH}");
            }
        }

        private static void SetupEquipmentDirectionalSprites()
        {
            // Trượng phép
            var staff = AssetDatabase.LoadAssetAtPath<EquipmentData>("Assets/ScriptableObjects/Items/equip_lumina_staff.asset");
            if (staff != null)
            {
                staff.spriteDown = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_mage_staff_down.png");
                staff.spriteUp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_mage_staff_up.png");
                staff.spriteRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_mage_staff_right.png");
                staff.spriteLeft = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_mage_staff_left.png");
                staff.visualSprite = staff.spriteDown;
                EditorUtility.SetDirty(staff);
            }

            // Kiếm tân thủ
            var sword = AssetDatabase.LoadAssetAtPath<EquipmentData>("Assets/ScriptableObjects/Items/equip_novice_sword.asset");
            if (sword != null)
            {
                sword.spriteDown = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_warrior_sword_down.png");
                sword.spriteUp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_warrior_sword_up.png");
                sword.spriteRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_warrior_sword_right.png");
                sword.spriteLeft = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/weapon_warrior_sword_left.png");
                sword.visualSprite = sword.spriteDown;
                EditorUtility.SetDirty(sword);
            }
        }

        private static Sprite[] LoadSprites(string assetPath, string[] spriteNames)
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var dict = new Dictionary<string, Sprite>();
            foreach (var obj in objs)
            {
                if (obj is Sprite s) dict[s.name] = s;
            }

            var list = new List<Sprite>();
            foreach (var name in spriteNames)
            {
                if (dict.TryGetValue(name, out var s))
                {
                    list.Add(s);
                }
            }
            return list.ToArray();
        }
    }
}
#endif
