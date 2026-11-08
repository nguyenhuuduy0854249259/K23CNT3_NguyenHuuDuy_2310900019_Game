using System;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Classes;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Items;

namespace ActionRPG.Pets
{
    public enum PetRole
    {
        Tanker,     // Đứng trước chịu đòn, HP cao
        DPS,        // Sát thương lớn, tấn công nhanh
        Support,    // Hồi máu, tăng chỉ số cho chủ
        Debuffer    // Làm chậm, choáng, giảm giáp kẻ địch
    }

    public enum PetType
    {
        Beast,          // Dã Thú (Sói, Hồ Ly, Gấu...)
        Elemental,      // Thể Nguyên Tố (Hỏa Linh, Thủy Tinh...)
        Golem,          // Người Đá / Cơ Giới
        Spirit,         // Tiên Linh / Linh Hồn
        Crustacean,     // Giáp Xác (Cua Gai, Bọ Cạp...)
        Liquid,         // Thể Lỏng / Slime
        Reptile,        // Bò Sát (Rắn, Thằn Lằn...)
        Flying          // Thú Bay (Bướm Nguyệt, Đại Bàng...)
    }

    [CreateAssetMenu(fileName = "NewPet", menuName = "ActionRPG/Pet Data")]
    public class PetData : ScriptableObject
    {
        [Header("Thông Tin Nhận Dạng")]
        public string petId = "pet_default";
        public string petName = "Thú Cưng";
        public string speciesName = "Dã Thú";
        public PetType petType = PetType.Beast;
        public ElementType element = ElementType.Fire;
        public PetRole role = PetRole.DPS;
        public PetBodyShape bodyShape = PetBodyShape.SmallFast; // Dùng cho Cận Chiến Tự Do biến hình vũ khí
        public int evolutionStage = 1; // 1, 2, 3

        [Header("Hình Ảnh & Mô Hình")]
        public Sprite petSprite;
        public Sprite icon;
        public GameObject petPrefab;

        [Header("Chỉ Số Gốc (Base Stats)")]
        public float maxHp = 80f;
        public float attackPower = 12f;
        public float defense = 4f;
        public float moveSpeed = 5f;
        public float attackRange = 1.8f;
        public float attackCooldown = 1.5f;

        // Tiện ích alias thuộc tính theo yêu cầu
        public float baseHp { get => maxHp; set => maxHp = value; }
        public float baseAtk { get => attackPower; set => attackPower = value; }
        public float baseDef { get => defense; set => defense = value; }

        [Header("Thức Ăn & Nuôi Dưỡng")]
        public string favoriteFood = "Thịt Nướng Thơm";
        public ItemData favoriteFoodItem;

        [Header("Kỹ Năng Pet")]
        public SkillData activeSkill;
        [TextArea] public string passiveAuraDescription = "Hào quang tăng 10% sát thương cho chủ nhân";

        [Header("Tiến Hóa")]
        public int evolutionRequiredLevel = 15;
        public ItemData evolutionRequiredItem;
        public PetData nextEvolution;
        public PetData evolutionTarget => nextEvolution;

        public string DisplaySpecies => !string.IsNullOrEmpty(speciesName) ? speciesName : petType.ToString();
    }
}
