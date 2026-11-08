using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes
{
    public enum ClassBranch
    {
        Novice,     // Tân thủ – lớp khởi đầu
        Archer,     // Xạ thủ
        Warrior,    // Cận chiến
        Mage        // Pháp sư
    }

    /// <summary>
    /// Vai trò chiến đấu của class trong party
    /// </summary>
    public enum ClassRole
    {
        DPS,        // Sát thương thuần túy
        SustainDPS, // DPS duy trì theo thời gian
        BurstDPS,   // Sát thương tập trung cao
        Tank,       // Chịu đòn, phòng thủ
        Support,    // Hồi máu, buff đội
        Controller, // Khống chế kẻ địch
        Tactician   // Điều phối chiến thuật
    }

    /// <summary>
    /// Cơ chế đặc trưng riêng của class – dùng để ClassSystem kích hoạt logic đúng
    /// </summary>
    public enum SpecialMechanic
    {
        None,
        // Cận chiến
        PetWeapon,          // Cận Chiến Tự Do – Pet biến thành 11 loại vũ khí (4 nhóm loài thú, CD 5s)

        // Pháp sư
        ElementalQueue,     // Pháp Sư Nguyên Tố – Hàng đợi 2 ô, 64 chiêu thức hợp thành, Thiên Tai sau 3 combo
        DualPetSummon,      // Pháp Sư Triệu Hồi – Song hành 2 Pet active, HUD 6 ô kỹ năng, quản lý HP Pet nguy kịch
        TimeControl,        // Pháp Sư Thời Không – Thao túng thời gian (Slow, Stasis nén sát thương, Reverse lùi tọa độ 3s)
        HolyDarkPriest,     // Mục Sư Thánh - Ma – Stance switch giữa Angel và Demon Form, 2 bộ loadout 5 ô riêng biệt
        CauldronAlchemy,    // Luyện Dược Sư Chum Ma Pháp – Đặt chum, 3 cấp chất lượng nguyên liệu (60/30/10), hoàn nguyên 5/10/15%

        // Xạ thủ
        GunOverheat,        // Xạ Thủ Súng – Quản lý nhiệt lượng súng (Heat / Overheat lockout 3.5s)
        EngineerTurrets,    // Cơ Khí Sư – Triển khai tối đa 3 trụ phòng thủ (Gatling, Laser, EMP), Pet bảo trì di động
        StealthCritical,    // Sát Thủ Bóng Đêm – Tàng hình 5-10s, Đòn mở đầu bạo kích 200-300%, Backstab 180° +35% dmg, Pet Decoy
        ElementalQuiver,    // Xạ Thủ Nguyên Tố – Khóa tốc bắn 1.0/s, chuyển đổi AtkSpd -> % Sát thương nguyên tố, 4 loại mũi tên
        SpiritCharge        // Xạ Thủ Tinh Linh – 3 Charge Linh Thể, tự đuổi 8-10s, Hợp thể Fusion x3, kế thừa hệ Pet
    }

    [CreateAssetMenu(fileName = "NewCharacterClass", menuName = "ActionRPG/Character Class")]
    public class CharacterClassData : ScriptableObject
    {
        [Header("Thông Tin Class")]
        public string className = "Tân Thủ (Novice)";
        public ClassBranch branch = ClassBranch.Novice;
        public ClassRole role = ClassRole.DPS;
        public int tier = 0;   // 0 = Novice, 1 = Tier-1 (Lv 10+), 2 = Tier-2 (Lv 30+)
        public int requiredLevelForPromotion = 10;
        [TextArea] public string description;
        [TextArea] public string mechanicDescription; // Mô tả cơ chế đặc trưng
        public Sprite classIcon;
        public Sprite characterSprite;

        [Header("Chỉ Số Cơ Bản Của Class")]
        public float baseAtk = 15f;
        public float baseDef = 5f;
        public float baseHp = 100f;
        public float baseMp = 50f;
        public float baseSpeed = 4.5f;
        public float baseCritRate = 0.05f; // 5%
        public ElementType defaultElement = ElementType.None;

        [Header("Cơ Chế Đặc Trưng")]
        public SpecialMechanic specialMechanic = SpecialMechanic.None;

        [Header("3 Ấn Nội Tại Cốt Lõi (Passive Seals)")]
        public string passiveSeal1Name = "Ấn 1";
        [TextArea] public string passiveSeal1Desc;
        public string passiveSeal2Name = "Ấn 2";
        [TextArea] public string passiveSeal2Desc;
        public string passiveSeal3Name = "Ấn 3";
        [TextArea] public string passiveSeal3Desc;

        [Header("Hệ Thống Kỹ Năng")]
        public List<SkillData> unlockedSkills = new List<SkillData>();

        [Header("Nhánh Chuyển Nghề Tiếp Theo (Tier cao hơn)")]
        public List<CharacterClassData> promotionOptions = new List<CharacterClassData>();

        [Header("Hiển Thị UI")]
        [Range(1, 5)]
        public int difficulty = 1; // 1=Dễ, 2=TB, 3=Khó, 4=Rất khó, 5=Master
    }
}
