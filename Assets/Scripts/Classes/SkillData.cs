using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes
{
    public enum SkillTargetType
    {
        DirectMelee,
        Projectile,
        AreaOfEffect,
        SelfBuff,
        PetCommand
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "ActionRPG/Skill")]
    public class SkillData : ScriptableObject
    {
        public string skillId;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;
        public Sprite skillIcon { get => icon; set => icon = value; }
        public SkillTargetType targetType;
        public ElementType element = ElementType.None;
        public DamageType damageType = DamageType.Physical;

        [Header("Thông Số Kỹ Năng")]
        public float manaCost = 10f;
        public float cooldownSeconds = 3f;
        public float damageMultiplier = 1.5f;
        public float range = 5f;
        public float radius = 2f;
        public float knockback = 4f;

        [Header("Hiệu Ứng Bất Lợi (Debuff)")]
        public StatusEffectType statusEffect = StatusEffectType.None;
        public float statusDuration = 2f;

        [Header("Prefabs & VFX")]
        public GameObject projectilePrefab;
        public GameObject hitVfxPrefab;
    }
}
