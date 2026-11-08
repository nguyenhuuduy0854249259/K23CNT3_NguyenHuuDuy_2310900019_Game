using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Pets;

namespace ActionRPG.Classes.Mechanics
{
    public enum PetBodyShape
    {
        HeavyLarge,    // Gấu, Rùa, Hổ -> Búa lớn, Đại kiếm, Kiếm + Khiên
        LongSlender,   // Rắn, Cá, Thằn lằn -> Trường giáo, Lưỡi hái, Roi
        SmallFast,     // Chim, Mèo, Chuột -> Dao găm, Móng vuốt, Phi tiêu
        Flying         // Bướm, Tiên linh -> Quạt chiến, Dây xích
    }

    public enum MeleeWeaponType
    {
        // Nhóm Thú to/nặng
        GreatHammer,    // Búa Lớn: Nện choáng, phá giáp
        Greatsword,     // Đại Kiếm: Quét rộng, gồng lực Charge 1.5s
        SwordAndShield, // Kiếm & Khiên: Chặn đòn và phản kích

        // Nhóm Thú dài/mảnh
        Spear,          // Trường Giáo: Đâm xuyên hàng dọc tầm xa
        GreatScythe,    // Lưỡi Hái Lớn: Quét 180°, kéo quái vào tâm
        Whip,           // Roi Ma Pháp: Quất diện rộng, quật ngã quái

        // Nhóm Thú nhỏ/nhanh
        DualDaggers,    // Dao Găm: Đâm tốc độ cao, chí mạng sau lưng
        Claws,          // Móng Vuốt: Cào xé dã tính, gây DoT
        ThrowingDarts,  // Phi Tiêu/Kiếm Ném: Boomerang đi và về

        // Nhóm Thú bay
        WarFan,         // Quạt Chiến: Đẩy lùi, lướt lùi né đòn, lốc xoáy
        MagicChains     // Dây Xích: Móc kéo quái hoặc kéo người, giam cầm
    }

    /// <summary>
    /// Xử lý cơ chế độc quyền của Cận Chiến Tự Do: Pet biến thành 11 loại vũ khí,
    /// thời gian đổi Pet 5s, nhận buff Cường Lực Hoán Vũ (+25% dmg 3s), phủ ngấn nguyên tố Pet.
    /// </summary>
    public class PetWeaponMechanics : MonoBehaviour
    {
        [Header("Trạng Thái Vũ Khí Hiện Tại")]
        public MeleeWeaponType currentWeapon = MeleeWeaponType.Greatsword;
        public ElementType currentElement = ElementType.Fire;
        public float weaponSwapCooldown = 5.0f;
        public float swapCooldownTimer = 0f;

        [Header("Ấn 1: Binh Khí Đồng Thể (Weapon Metamorphosis)")]
        public bool hasMetamorphosisBuff = false;
        public float metamorphosisBuffTimer = 0f;
        private const float MetamorphosisBuffDuration = 3.0f;
        private const float MetamorphosisDamageBonus = 0.25f; // +25%

        [Header("Kỹ Năng Theo Vũ Khí (Slot 3, Slot 4, Slot 5)")]
        public string slot3SkillName = "Slot 3 Skill";
        public string slot4SkillName = "Slot 4 Skill";
        public string slot5SkillName = "Slot 5 Skill (Tuyệt Kỹ)";

        public event Action<MeleeWeaponType, ElementType> OnWeaponTransformed;

        private void Update()
        {
            if (swapCooldownTimer > 0f)
                swapCooldownTimer -= Time.deltaTime;

            if (hasMetamorphosisBuff)
            {
                metamorphosisBuffTimer -= Time.deltaTime;
                if (metamorphosisBuffTimer <= 0f)
                    hasMetamorphosisBuff = false;
            }
        }

        /// <summary>
        /// Chuyển đổi vũ khí dựa trên Pet đang trang bị. Cooldown 5s.
        /// </summary>
        public bool TryTransformWeaponFromPet(PetBodyShape shape, int subTypeIndex, ElementType petElement)
        {
            if (swapCooldownTimer > 0f)
            {
                Debug.LogWarning($"[PetWeapon] Đang trong thời gian chờ đổi Pet: {swapCooldownTimer:F1}s còn lại.");
                return false;
            }

            currentWeapon = DetermineWeaponType(shape, subTypeIndex);
            currentElement = petElement;
            swapCooldownTimer = weaponSwapCooldown;

            // Kích hoạt Ấn 1: Binh Khí Đồng Thể (+25% sát thương trong 3s)
            hasMetamorphosisBuff = true;
            metamorphosisBuffTimer = MetamorphosisBuffDuration;

            UpdateWeaponSkills(currentWeapon);

            // Ẩn mô hình Pet và gắn vũ khí lên người chơi
            if (PlayerPetRoster.Instance != null && PlayerPetRoster.Instance.activePetController != null)
            {
                PlayerPetRoster.Instance.activePetController.HideForWeaponTransform();
            }
            else
            {
                PetController pet = FindObjectOfType<PetController>();
                if (pet != null) pet.HideForWeaponTransform();
            }

            Debug.Log($"<color=cyan>[PetWeapon] ĐÃ BIẾN THÀNH VŨ KHÍ: {currentWeapon} mang hệ {currentElement}! Buff +25% dmg trong 3s.</color>");
            OnWeaponTransformed?.Invoke(currentWeapon, currentElement);
            return true;
        }

        /// <summary>
        /// Thu hồi vũ khí và hoàn trả mô hình Pet ra chiến trường.
        /// </summary>
        public void RevertWeaponTransform()
        {
            if (PlayerPetRoster.Instance != null && PlayerPetRoster.Instance.activePetController != null)
            {
                PlayerPetRoster.Instance.activePetController.RestoreFromWeaponTransform();
            }
            else
            {
                PetController pet = FindObjectOfType<PetController>();
                if (pet != null) pet.RestoreFromWeaponTransform();
            }
        }

        /// <summary>
        /// Tính toán sát thương đòn đánh kèm buff chuyển vũ khí.
        /// </summary>
        public float ApplyMetamorphosisMultiplier(float baseDamage)
        {
            if (hasMetamorphosisBuff)
            {
                hasMetamorphosisBuff = false; // Tiêu thụ buff ở đòn đánh/chiêu đầu tiên
                return baseDamage * (1f + MetamorphosisDamageBonus);
            }
            return baseDamage;
        }

        private MeleeWeaponType DetermineWeaponType(PetBodyShape shape, int subIndex)
        {
            int index = Mathf.Clamp(subIndex, 0, 2);
            switch (shape)
            {
                case PetBodyShape.HeavyLarge:
                    return index switch
                    {
                        0 => MeleeWeaponType.GreatHammer,
                        1 => MeleeWeaponType.Greatsword,
                        _ => MeleeWeaponType.SwordAndShield
                    };
                case PetBodyShape.LongSlender:
                    return index switch
                    {
                        0 => MeleeWeaponType.Spear,
                        1 => MeleeWeaponType.GreatScythe,
                        _ => MeleeWeaponType.Whip
                    };
                case PetBodyShape.SmallFast:
                    return index switch
                    {
                        0 => MeleeWeaponType.DualDaggers,
                        1 => MeleeWeaponType.Claws,
                        _ => MeleeWeaponType.ThrowingDarts
                    };
                case PetBodyShape.Flying:
                    return (index == 0) ? MeleeWeaponType.WarFan : MeleeWeaponType.MagicChains;
                default:
                    return MeleeWeaponType.Greatsword;
            }
        }

        private void UpdateWeaponSkills(MeleeWeaponType weapon)
        {
            switch (weapon)
            {
                case MeleeWeaponType.GreatHammer:
                    slot3SkillName = "Địa Chấn Nện (Stun 1s, Cone 4m)";
                    slot4SkillName = "Búa Tạ Phá Giáp (-35% Def trong 5s)";
                    slot5SkillName = "Khai Sơn Bạt Địa (Tuyệt Kỹ - Hất tung 5m, Kháng CC)";
                    break;
                case MeleeWeaponType.Greatsword:
                    slot3SkillName = "Đoạn Phong Trảm (Gồng 1.5s x2 sát thương)";
                    slot4SkillName = "Nghịch Kiếm Hất Tung (Vô hiệu hóa đòn quái 1.5s)";
                    slot5SkillName = "Khí Kiếm Trảm Nguyệt (Tuyệt Kỹ - Quét 360°, phóng kiếm khí 6m)";
                    break;
                case MeleeWeaponType.SwordAndShield:
                    slot3SkillName = "Húc Khiên Choáng Váng (Lao 3m, Stun 1.2s)";
                    slot4SkillName = "Khiên Chắn Phản Đòn (Chặn hoàn toàn 1 đòn & phản kích)";
                    slot5SkillName = "Trận Địa Pháo Đài (Tuyệt Kỹ - Giảm 40% dmg nhận, chém 3 nhát)";
                    break;
                case MeleeWeaponType.Spear:
                    slot3SkillName = "Liên Hoàn Thương Kích (Đâm 4 mũi xuyên hàng dọc)";
                    slot4SkillName = "Chống Giáo Đạp Lưng (Bật nhảy ra sau, Knockback mục tiêu)";
                    slot5SkillName = "Long Thương Xuất Động (Tuyệt Kỹ - Lao 8m xiên nứt đất)";
                    break;
                case MeleeWeaponType.GreatScythe:
                    slot3SkillName = "Lưỡi Quét Tử Thần (Quét 180°, rìa kiếm +40% dmg)";
                    slot4SkillName = "Trảm Câu Kéo Quái (Móc quái cự ly 5m về trước mặt)";
                    slot5SkillName = "Hư Không Xoáy Lốc (Tuyệt Kỹ - Hút quái chém nát trong 2.5s)";
                    break;
                case MeleeWeaponType.Whip:
                    slot3SkillName = "Roi Quất Xé Gió (Slow 30% nhóm quái)";
                    slot4SkillName = "Quấn Chân Quật Ngã (Quật ngã quái khóa 1.5s)";
                    slot5SkillName = "Vũ Điệu Hỗn Loạn (Tuyệt Kỹ - Quất 8 hướng ngăn áp sát)";
                    break;
                case MeleeWeaponType.DualDaggers:
                    slot3SkillName = "Đoản Đao Xé Thịt (Đâm 6 nhát trong 0.8s)";
                    slot4SkillName = "Bộ Pháp Bóng Ma (Lướt sau lưng, chắc chắn chí mạng)";
                    slot5SkillName = "Vũ Điệu Tuyệt Sát (Tuyệt Kỹ - Phân thân chém 10 nhát cắt cổ)";
                    break;
                case MeleeWeaponType.Claws:
                    slot3SkillName = "Cào Xé Dã Tính (Cào chéo chữ X, gây Bleed DoT)";
                    slot4SkillName = "Vồ Mồi Áp Đảo (Lao chồm đè quái khóa cứng 1s)";
                    slot5SkillName = "Cuồng Nộ Thú Tính (Tuyệt Kỹ - +50% AtkSpeed, phóng vuốt khí AoE)";
                    break;
                case MeleeWeaponType.ThrowingDarts:
                    slot3SkillName = "Phi Đao Tản Xạ (Bắn 5 phi tiêu cánh quạt đẩy lùi)";
                    slot4SkillName = "Kiếm Khí Hồi Mã (Ném 6m boomerang gây damage 2 lượt)";
                    slot5SkillName = "Vạn Tiêu Trận Địa (Tuyệt Kỹ - Trận địa phi đao tự găm quái)";
                    break;
                case MeleeWeaponType.WarFan:
                    slot3SkillName = "Phong Ba Phiến (Đẩy lùi 4m, thổi bay đạn đạo)";
                    slot4SkillName = "Vũ Điệu Khinh Linh (Lùi 3m để lại lốc xoáy giữ chân)";
                    slot5SkillName = "Phong Quyển Tàn Hoa (Tuyệt Kỹ - 2 cơn lốc song song cuốn quái)";
                    break;
                case MeleeWeaponType.MagicChains:
                    slot3SkillName = "Móc Sắt Tầm Xa (Móc quái nhỏ về hoặc đu tới quái to)";
                    slot4SkillName = "Xích Quét Trùng Phùng (Quay xích 4m quất ngã quái)";
                    slot5SkillName = "Tỏa Ngục Trầm Uyên (Tuyệt Kỹ - Lồng xích giam giữ 3s)";
                    break;
            }
        }
    }
}
