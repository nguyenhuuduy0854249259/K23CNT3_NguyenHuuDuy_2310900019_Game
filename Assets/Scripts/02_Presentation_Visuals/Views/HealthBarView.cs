using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ActionRPG.Presentation.Views
{
    /// <summary>
    /// View chuyên trách hiển thị thanh máu UI (UGUI hoặc World Space Canvas).
    /// Nhận thông số từ Presenter để cập nhật Slider / Text.
    /// </summary>
    public class HealthBarView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Hiệu Ứng Lerp Mượt")]
        [SerializeField] private bool useSmoothLerp = true;
        [SerializeField] private float lerpSpeed = 10f;

        private float targetFillRatio = 1f;

        private void Update()
        {
            if (useSmoothLerp && healthSlider != null)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetFillRatio, Time.deltaTime * lerpSpeed);
            }
        }

        public void UpdateHealth(float currentHp, float maxHp)
        {
            if (maxHp <= 0f) maxHp = 1f;
            targetFillRatio = Mathf.Clamp01(currentHp / maxHp);

            if (!useSmoothLerp && healthSlider != null)
            {
                healthSlider.value = targetFillRatio;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
            }
        }
    }
}
