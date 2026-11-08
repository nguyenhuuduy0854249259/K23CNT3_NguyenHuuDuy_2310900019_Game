using System;
using ActionRPG.Combat;
using ActionRPG.Core.Domain.ValueObjects;

namespace ActionRPG.Core.Services
{
    /// <summary>
    /// Service thuần C# tính toán quy tắc sát thương, phòng thủ (Giáp vật lý & Giáp phép) và tương tác nguyên tố.
    /// Có thể chạy Unit Test mọi kịch bản sát thương mà không cần mở Unity Play Mode.
    /// </summary>
    public static class DamageCalculationService
    {
        public const float MinDamage = 1.0f;
        public const float PhysicalDefenseMitigationFactor = 0.6f;
        public const float MagicDefenseMitigationFactor = 0.6f;

        /// <summary>
        /// Tính toán sát thương thực tế sau khi trừ giáp vật lý hoặc giáp phép:
        /// - Sát thương Vật Lý (Physical) -> Trừ vào Giáp Vật Lý (PhysicalDef * 0.6f).
        /// - Sát thương Phép Thuật (Magical) -> Trừ vào Giáp Phép (MagicDef * 0.6f).
        /// - Sát thương Chuẩn (TrueDamage) -> Bỏ qua giáp (không trừ).
        /// - Hệ số nguyên tố (elementalMultiplier) nhân theo bảng tương khắc Lumina.
        /// - Sát thương bạo kích (Critical) nhân hệ số bạo kích (mặc định x1.5).
        /// - Hiện tại chưa kích hoạt khả năng xuyên giáp (để dành phát triển sau này).
        /// </summary>
        public static float CalculateFinalDamage(
            float rawDamage, 
            DamageType damageType, 
            float targetPhysicalDef, 
            float targetMagicDef, 
            float elementalMultiplier = 1.0f, 
            bool isCritical = false,
            float critMultiplier = 1.5f)
        {
            float defReduction = 0f;

            switch (damageType)
            {
                case DamageType.Physical:
                    defReduction = Math.Max(0f, targetPhysicalDef * PhysicalDefenseMitigationFactor);
                    break;

                case DamageType.Magical:
                    defReduction = Math.Max(0f, targetMagicDef * MagicDefenseMitigationFactor);
                    break;

                case DamageType.TrueDamage:
                    defReduction = 0f; // Sát thương chuẩn không bị cản bởi giáp
                    break;
            }

            float netDamage = Math.Max(MinDamage, rawDamage - defReduction) * elementalMultiplier;

            if (isCritical)
            {
                netDamage *= critMultiplier;
            }

            return Math.Max(MinDamage, netDamage);
        }

        /// <summary>
        /// Quá tải tương thích ngược với DamageData cũ.
        /// </summary>
        public static float CalculateFinalDamage(DamageData damage, float targetDefense, float elementalMultiplier = 1.0f)
        {
            return CalculateFinalDamage(damage.Amount, damage.Type, targetDefense, targetDefense, elementalMultiplier, damage.IsCritical);
        }
    }
}
