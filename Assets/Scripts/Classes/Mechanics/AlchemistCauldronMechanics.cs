using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    public enum MaterialQualityTier
    {
        Low = 1,    // Thấp (1 điểm, 60% drop)
        Medium = 2, // Vừa (2 điểm, 30% drop)
        High = 3    // Cao (3 điểm, 10% drop, +30% hiệu số)
    }

    [Serializable]
    public struct AlchemyIngredient
    {
        public string ingredientName;
        public MaterialQualityTier quality;
        public ElementType element;
    }

    /// <summary>
    /// Xử lý cơ chế Luyện Dược Sư Chum Ma Pháp:
    /// Đặt Chum -> ném nguyên liệu -> delay ủ -> kích hoạt hiệu ứng -> hoàn nguyên.
    /// Tính toán chất lượng nguyên liệu 1/2/3 điểm theo tỷ lệ 60/30/10.
    /// Tỷ lệ hoàn nguyên theo cấp: Lv 1-15: 5%, Lv 16-29: 10%, Lv 30+: 15%.
    /// Gián đoạn: hoàn trả 100% nguyên liệu.
    /// </summary>
    public class AlchemistCauldronMechanics : MonoBehaviour
    {
        [Header("Trạng Thái Chum")]
        public bool isCauldronPlaced = false;
        public Vector2 cauldronPosition;
        public float brewingTimer = 0f;
        public float currentDelayDuration = 2.0f;
        public bool isBrewing = false;

        [Header("Danh Sách Nguyên Liệu Nạp Vào Chum")]
        public List<AlchemyIngredient> loadedIngredients = new List<AlchemyIngredient>();

        public event Action<MaterialQualityTier, string> OnCauldronActivated;
        public event Action OnBrewingInterrupted;

        /// <summary>
        /// Đặt Chum xuống sàn đấu và ném nguyên liệu vào.
        /// </summary>
        public bool PlaceCauldron(Vector2 position, List<AlchemyIngredient> ingredients, float delayTime = 2.0f)
        {
            if (isCauldronPlaced) return false;
            if (ingredients == null || ingredients.Count == 0) return false;

            isCauldronPlaced = true;
            cauldronPosition = position;
            loadedIngredients = new List<AlchemyIngredient>(ingredients);
            currentDelayDuration = delayTime;
            brewingTimer = currentDelayDuration;
            isBrewing = true;

            Debug.Log($"[Alchemist] Đã đặt Chum tại {position}! Bắt đầu ủ {ingredients.Count} nguyên liệu trong {delayTime}s.");
            return true;
        }

        private void Update()
        {
            if (isBrewing)
            {
                brewingTimer -= Time.deltaTime;
                if (brewingTimer <= 0f)
                {
                    CompleteBrewing();
                }
            }
        }

        /// <summary>
        /// Hoàn thành quá trình ủ: tính toán chất lượng tổng và kích hoạt hiệu ứng.
        /// </summary>
        private void CompleteBrewing()
        {
            isBrewing = false;
            isCauldronPlaced = false;

            MaterialQualityTier finalQuality = CalculateCompositeQuality(loadedIngredients);
            float qualityMultiplier = (finalQuality == MaterialQualityTier.High) ? 1.3f : 1.0f;

            Debug.Log($"<color=green>[Alchemist] CHUM KÍCH HOẠT THÀNH CÔNG! Phẩm chất: {finalQuality} (Hệ số: {qualityMultiplier}x)</color>");
            OnCauldronActivated?.Invoke(finalQuality, $"Hiệu ứng dược phẩm cấp {finalQuality}");

            loadedIngredients.Clear();
        }

        /// <summary>
        /// Ấn 1: Nếu bị quái đánh ngắt quãng / dính khống chế trong lúc đặt Chum, hoàn 100% nguyên liệu.
        /// </summary>
        public void InterruptBrewing()
        {
            if (!isCauldronPlaced) return;

            Debug.LogWarning("[Alchemist] QUÁ TRÌNH Ủ BỊ NGẮT QUÃNG! Ấn 1 (Quy Hoàn Dược Lý) hoàn trả 100% nguyên liệu vào túi!");
            isBrewing = false;
            isCauldronPlaced = false;
            loadedIngredients.Clear();

            OnBrewingInterrupted?.Invoke();
        }

        /// <summary>
        /// Tính tỷ lệ hoàn nguyên theo Level nhân vật (5% -> 10% -> 15%).
        /// </summary>
        public float GetRefundRate(int characterLevel)
        {
            if (characterLevel >= 30) return 0.15f; // Bậc cao nhất: 15%
            if (characterLevel >= 16) return 0.10f; // Bậc 2: 10%
            return 0.05f;                          // Bậc 1: 5%
        }

        /// <summary>
        /// Tính toán chất lượng tổng của công thức theo luật:
        /// 2 nguyên liệu: Thấp+Thấp=Thấp, Thấp+Vừa=Vừa, Vừa+Vừa=Vừa, Vừa+Cao=Vừa, Cao+Cao=Cao.
        /// 3-5 nguyên liệu: Trung bình điểm (1.00-1.49 Thấp, 1.50-2.49 Vừa, 2.50-3.00 Cao).
        /// </summary>
        public static MaterialQualityTier CalculateCompositeQuality(List<AlchemyIngredient> ingredients)
        {
            if (ingredients == null || ingredients.Count == 0)
                return MaterialQualityTier.Low;

            if (ingredients.Count == 2)
            {
                var q1 = ingredients[0].quality;
                var q2 = ingredients[1].quality;
                if (q1 == MaterialQualityTier.High && q2 == MaterialQualityTier.High) return MaterialQualityTier.High;
                if (q1 == MaterialQualityTier.Low && q2 == MaterialQualityTier.Low) return MaterialQualityTier.Low;
                return MaterialQualityTier.Medium;
            }

            // 3 - 5 nguyên liệu: tính điểm trung bình
            float sum = 0f;
            foreach (var ing in ingredients)
                sum += (int)ing.quality;

            float avg = sum / ingredients.Count;
            if (avg >= 2.50f) return MaterialQualityTier.High;
            if (avg >= 1.50f) return MaterialQualityTier.Medium;
            return MaterialQualityTier.Low;
        }
    }
}
