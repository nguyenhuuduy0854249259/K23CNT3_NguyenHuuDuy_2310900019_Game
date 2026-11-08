using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.UI;

namespace ActionRPG.Classes
{
    /// <summary>
    /// ClassEvolutionManager: Quản lý nạp dữ liệu kỹ năng khi chuyển đổi Class.
    /// - Nạp ĐÚNG các ScriptableObject kỹ năng từ "Assets/ScriptableObjects/Skills/" (hoặc Resources.Load).
    /// - Nạp 5 chiêu tương ứng vào mảng kỹ năng hiện tại của PlayerSkillCombat2D.
    /// - Bắn sự kiện EventBus.OnSkillChanged để ActionBarUI và HUD cập nhật icon tức thì.
    /// </summary>
    public class ClassEvolutionManager : MonoBehaviour
    {
        public static ClassEvolutionManager Instance { get; private set; }

        private const string SkillsPathPrefix = "Assets/ScriptableObjects/Skills/Skill_";

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        /// <summary>
        /// Thực hiện chuyển class và nạp 5 kỹ năng chuẩn từ ScriptableObject.
        /// </summary>
        public static void EvolveClass(CharacterClassData newClass)
        {
            if (newClass == null) return;

            // 1. Nạp danh sách 5 kỹ năng tương ứng từ ScriptableObject
            List<SkillData> skills = LoadSkillsForClass(newClass);

            // 2. Nạp vào PlayerSkillCombat2D
            var combat = Object.FindAnyObjectByType<PlayerSkillCombat2D>();
            if (combat != null)
            {
                combat.ApplySkills(skills, newClass.branch);
            }

            // 3. Bắn Event để ActionBarUI và GameplayHUD cập nhật ngay lập tức
            EventBus.RaiseSkillChanged(skills);
            EventBus.OnClassApplied?.Invoke(newClass);

            Debug.Log($"<color=cyan>[ClassEvolutionManager] Đã nạp thành công {skills.Count} kỹ năng cho class {newClass.className} và bắn EventBus.OnSkillChanged!</color>");
        }

        /// <summary>
        /// Nạp ĐÚNG các ScriptableObject từ "Assets/ScriptableObjects/Skills/" hoặc Resources
        /// </summary>
        public static List<SkillData> LoadSkillsForClass(CharacterClassData classData)
        {
            var result = new List<SkillData>(6);
            if (classData == null) return result;

            string[] skillIds = GetDefaultSkillIdsForBranch(classData.branch);

            // Nếu classData đã có danh sách unlockedSkills hợp lệ thì sử dụng (tối đa 6 chiêu)
            if (classData.unlockedSkills != null && classData.unlockedSkills.Count >= 5)
            {
                int maxCount = Mathf.Min(6, classData.unlockedSkills.Count);
                for (int i = 0; i < maxCount; i++)
                {
                    var s = classData.unlockedSkills[i];
                    if (s != null)
                    {
                        EnsureSkillVisuals(s);
                        result.Add(s);
                    }
                }
                // Bổ sung Tuyệt Kỹ (Chiêu 6) nếu class cơ bản Warrior/Mage/Archer trong asset cũ chỉ có 5 chiêu
                if (result.Count == 5 && skillIds.Length >= 6)
                {
                    SkillData ultSkill = LoadSkillAssetById(skillIds[5]);
                    if (ultSkill != null)
                    {
                        EnsureSkillVisuals(ultSkill);
                        result.Add(ultSkill);
                    }
                }
                if (result.Count >= 5) return result;
            }

            foreach (string id in skillIds)
            {
                SkillData skill = LoadSkillAssetById(id);
                if (skill != null)
                {
                    EnsureSkillVisuals(skill);
                    result.Add(skill);
                }
            }

            return result;
        }

        public static string[] GetDefaultSkillIdsForBranch(ClassBranch branch)
        {
            switch (branch)
            {
                case ClassBranch.Warrior:
                    return new[] {
                        "warrior_cleave",
                        "warrior_vanguard_charge",
                        "warrior_iron_bastion",
                        "warrior_tremor_slam",
                        "warrior_frenzied_strike",
                        "melee_ultimate_crush"
                    };

                case ClassBranch.Mage:
                    return new[] {
                        "mage_fire_orb",
                        "mage_frost_spikes",
                        "mage_blink",
                        "mage_chain_lightning",
                        "mage_mana_shield",
                        "mage_arcane_blast"
                    };

                case ClassBranch.Archer:
                    return new[] {
                        "archer_piercing_shot",
                        "archer_volley",
                        "archer_tumble",
                        "archer_snare_trap",
                        "archer_deadeye",
                        "arch_elemental_storm"
                    };

                case ClassBranch.Novice:
                default:
                    return new[] {
                        "novice_fan_slash",
                        "novice_flurry_slash",
                        "novice_aegis_barrier",
                        "novice_quad_dash",
                        "novice_holy_heal",
                        "melee_ultimate_crush"
                    };
            }
        }

        public static SkillData LoadSkillAssetById(string skillId)
        {
            SkillData skill = null;

            // 1. Thử Resources.Load
            skill = Resources.Load<SkillData>($"Skills/Skill_{skillId}") ?? Resources.Load<SkillData>(skillId);

#if UNITY_EDITOR
            // 2. Thử nạp trực tiếp từ thư mục "Assets/ScriptableObjects/Skills/"
            if (skill == null)
            {
                string assetPath = $"{SkillsPathPrefix}{skillId}.asset";
                skill = UnityEditor.AssetDatabase.LoadAssetAtPath<SkillData>(assetPath);
            }
#endif

            return skill;
        }

        private static void EnsureSkillVisuals(SkillData skill)
        {
            if (skill == null) return;

#if UNITY_EDITOR
            // Đảm bảo icon không bị null
            if (skill.icon == null)
            {
                string iconPath = $"Assets/Sprites/UI/HUD/Skills/icon_skill_{skill.skillId}.png";
                var spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (spr != null) skill.icon = spr;
            }

            // Đảm bảo VFX Prefabs được liên kết nếu bị thiếu
            if (skill.hitVfxPrefab == null && skill.projectilePrefab == null)
            {
                string vfxPath = $"Assets/Prefabs/VFX/VFX_{GetCapitalizedSkillName(skill.skillId)}.prefab";
                var vfx = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(vfxPath);
                if (vfx != null)
                {
                    if (skill.targetType == SkillTargetType.Projectile)
                        skill.projectilePrefab = vfx;
                    else
                        skill.hitVfxPrefab = vfx;
                }
            }
#endif
        }

        public static string GetCapitalizedSkillName(string id)
        {
            // warrior_cleave -> Warrior_Cleave
            string[] parts = id.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }
            return string.Join("_", parts);
        }
    }
}
