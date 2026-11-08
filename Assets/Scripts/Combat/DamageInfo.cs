using UnityEngine;

namespace ActionRPG.Combat
{
    public enum DamageType
    {
        Physical = 0,   // Sát thương vật lý (trừ vào Giáp vật lý)
        Magical = 1,    // Sát thương phép (trừ vào Giáp phép / Kháng phép)
        TrueDamage = 2  // Sát thương chuẩn (bỏ qua giáp - dành cho phát triển sau)
    }

    public enum StatusEffectType
    {
        None,
        Burn,       // DoT damage
        Slow,       // Reduce move speed
        Stun,       // Cannot move or attack
        Bleed       // % Max HP damage over time
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public float rawDamage;
        public DamageType damageType;
        public bool isCrit;
        public ElementType element;
        public Vector2 knockbackDirection;
        public float knockbackForce;
        public GameObject attacker;
        public StatusEffectType statusEffect;
        public float statusDuration;

        public DamageInfo(float damage, ElementType elem = ElementType.None, bool crit = false, GameObject source = null, DamageType type = DamageType.Physical)
        {
            rawDamage = damage;
            element = elem;
            isCrit = crit;
            attacker = source;
            damageType = type;
            knockbackDirection = Vector2.zero;
            knockbackForce = 0f;
            statusEffect = StatusEffectType.None;
            statusDuration = 0f;
        }
    }
}
