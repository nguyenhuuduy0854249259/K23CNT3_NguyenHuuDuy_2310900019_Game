using System;
using UnityEngine;

namespace ActionRPG.Combat
{
    public enum ElementType
    {
        None,
        Fire,       // Hỏa: Thiêu đốt, DoT
        Water,      // Thủy: Làm chậm
        Electric,   // Điện: Tê liệt, Choáng
        Earth,      // Thổ: Xuyên giáp
        Wind,       // Phong: Đẩy lùi
        Poison,     // Độc: Xuất huyết
        Angel,      // Thiên Thần: Hồi máu, giảm sát thương
        Demon       // Ác Quỷ: Tăng sát thương, thiêu đốt linh hồn
    }

    public static class ElementalSynergy
    {
        /// <summary>
        /// Tính toán hệ số sát thương dựa trên tương khắc nguyên tố chuẩn Lumina:
        /// Thắng hệ (Khắc chế): x2.0 sát thương.
        /// Cùng hệ hoặc bị kháng: x0.5 sát thương.
        /// Tương tác bình thường: x1.0.
        /// </summary>
        public static float GetElementalMultiplier(ElementType attacker, ElementType defender)
        {
            if (attacker == ElementType.None || defender == ElementType.None)
                return 1.0f;

            // Hỏa -> Phong -> Thổ -> Điện -> Thủy -> Hỏa (Vòng tròn ngũ hành x2.0)
            if (attacker == ElementType.Fire && defender == ElementType.Wind) return 2.0f;
            if (attacker == ElementType.Wind && defender == ElementType.Earth) return 2.0f;
            if (attacker == ElementType.Earth && defender == ElementType.Electric) return 2.0f;
            if (attacker == ElementType.Electric && defender == ElementType.Water) return 2.0f;
            if (attacker == ElementType.Water && defender == ElementType.Fire) return 2.0f;

            // Độc khắc Thổ, bị Phong khắc
            if (attacker == ElementType.Poison && defender == ElementType.Earth) return 2.0f;
            if (attacker == ElementType.Wind && defender == ElementType.Poison) return 2.0f;

            // Thiên Thần và Ác Quỷ đối cực khắc chế lẫn nhau (cả 2 đều gây x2.0 sát thương)
            if (attacker == ElementType.Angel && defender == ElementType.Demon) return 2.0f;
            if (attacker == ElementType.Demon && defender == ElementType.Angel) return 2.0f;

            // Cùng hệ giảm 50%
            if (attacker == defender) return 0.5f;

            // Bị khắc hệ / kháng ngược
            if (attacker == ElementType.Wind && defender == ElementType.Fire) return 0.5f;
            if (attacker == ElementType.Earth && defender == ElementType.Wind) return 0.5f;
            if (attacker == ElementType.Electric && defender == ElementType.Earth) return 0.5f;
            if (attacker == ElementType.Water && defender == ElementType.Electric) return 0.5f;
            if (attacker == ElementType.Fire && defender == ElementType.Water) return 0.5f;
            if (attacker == ElementType.Earth && defender == ElementType.Poison) return 0.5f;
            if (attacker == ElementType.Poison && defender == ElementType.Wind) return 0.5f;

            return 1.0f;
        }

        public static Color GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Fire: return new Color(1f, 0.35f, 0.15f);
                case ElementType.Water: return new Color(0.2f, 0.6f, 1f);
                case ElementType.Electric: return new Color(1f, 0.9f, 0.2f);
                case ElementType.Earth: return new Color(0.7f, 0.5f, 0.3f);
                case ElementType.Wind: return new Color(0.4f, 0.9f, 0.6f);
                case ElementType.Poison: return new Color(0.65f, 0.2f, 0.85f);
                case ElementType.Angel: return new Color(1f, 1f, 0.7f);
                case ElementType.Demon: return new Color(0.5f, 0.1f, 0.2f);
                default: return Color.white;
            }
        }
    }
}
