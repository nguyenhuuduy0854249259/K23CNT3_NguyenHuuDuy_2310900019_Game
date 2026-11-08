using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Characters;

namespace ActionRPG.Items
{
    public enum EquipSlot
    {
        Weapon = 0,
        Armor = 1,
        Helmet = 2,
        Ring = 3,
        Necklace = 4,
        Gloves = 5,
        Boots = 6
    }

    public enum UniqueEffectType
    {
        None,
        DarkExplosion,      // Đòn kết liễu nổ bóng tối 200% sát thương AoE
        PhoenixRevive,      // Hồi sinh 50% HP khi chết (cooldown 10 phút)
        ChainLightning,     // 20% đòn đánh kích hoạt sét giật qua 4 mục tiêu
        VampiricStrike,     // Hút 10% sát thương gây ra thành máu
        SuperArmorGrit,     // Miễn nhiễm khựng đòn khi HP dưới 50%
        ElementalOvercharge // Tăng 30% hiệu ứng tương khắc nguyên tố
    }

    [CreateAssetMenu(fileName = "NewEquipment", menuName = "ActionRPG/Equipment")]
    public class EquipmentData : ItemData
    {
        [Header("Equipment Slots & Stats")]
        public EquipSlot equipSlot;
        public int requiredLevel = 1;
        public EvolutionTier requiredTier = EvolutionTier.Tier1_Novice;
        public bool isSpecializedWeapon = false; // Vũ khí chuyên phái (Cung tinh nhuệ, Gậy phép, Súng lục/đại bác)
        public float bonusAtk = 0;
        public float bonusMagicAtk = 0;
        public float bonusDef = 0;
        public float bonusMagicDef = 0;
        public float bonusHp = 0;
        public float bonusMp = 0;
        public float bonusSpeed = 0;
        public float bonusCritRate = 0;
        public ElementType weaponElement = ElementType.None;

        [Header("Visual Settings (Modular Paperdoll)")]
        public Sprite visualSprite; // Sprite hiển thị trên cơ thể nhân vật
        public Vector2 weaponGripOffset = Vector2.zero; // Độ lệch tâm cầm nắm (chuôi kiếm)
        public float weaponRotationAngle = 0f; // Góc nghiêng vũ khí ở trạng thái nghỉ

        [Header("4-Direction Sprites (Vũ khí 4 hướng)")]
        public Sprite spriteDown;
        public Sprite spriteUp;
        public Sprite spriteRight;
        public Sprite spriteLeft;

        [Header("Độc Bản (Signature Unique Affix)")]
        public bool isUnique = false;
        public UniqueEffectType uniqueEffect = UniqueEffectType.None;
        public string uniqueEffectName = "";
        [TextArea] public string uniqueEffectDescription = "";

        private void OnEnable()
        {
            itemType = ItemType.Equipment;
            isStackable = false;
            maxStack = 1;
        }
    }
}
