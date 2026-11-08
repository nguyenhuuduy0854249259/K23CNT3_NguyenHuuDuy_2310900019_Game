using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Combat;
using ActionRPG.Core;

namespace ActionRPG.UI
{
    /// <summary>
    /// Thanh máu Boss hoành tráng ở đỉnh màn hình HUD:
    /// - Hiển thị Tên Boss ("Hắc Diệu Ma Viên - Chúa Tể Rừng Rậm")
    /// - Phù hiệu & Tên Hệ Nguyên Tố (Hỏa / Ám)
    /// - Thanh trượt HP % dạng mượt mà (Smooth Fill)
    /// - Đổi màu rực lửa khi Boss chuyển sang Phase 2 (Cuồng Nộ)
    /// - Tự động bật/tắt qua Trigger Hang Boss hoặc EventBus.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance { get; private set; }

        [Header("Gốc Thanh Máu Boss")]
        public GameObject barRoot;
        public CanvasGroup canvasGroup;

        [Header("Thành Phần Hiển Thị")]
        public TMP_Text bossNameText;
        public TMP_Text phaseText;
        public Image elementIcon;
        public TMP_Text elementText;
        public Image hpFill;
        public TMP_Text hpValueText;

        [Header("Màu Sắc Thanh Máu")]
        public Color normalHpColor = new Color(0.9f, 0.25f, 0.25f);
        public Color enrageHpColor = new Color(1f, 0.1f, 0.1f);
        public Color flashWhiteColor = Color.white;

        private float currentHp = 100f;
        private float maxHp = 100f;
        private float targetFill = 1.0f;
        private bool isEnraged = false;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureBossUIStructure();

            // Đăng ký sự kiện
            EventBus.OnBossSpawned += HandleBossSpawned;
            EventBus.OnBossHpChanged += HandleBossHpChanged;
            EventBus.OnBossPhaseChanged += HandleBossPhaseChanged;
            EventBus.OnBossDefeated += HandleBossDefeated;

            // Ẩn ban đầu nếu chưa gặp Boss
            SetBarVisible(false, true);
        }

        private void OnDestroy()
        {
            EventBus.OnBossSpawned -= HandleBossSpawned;
            EventBus.OnBossHpChanged -= HandleBossHpChanged;
            EventBus.OnBossPhaseChanged -= HandleBossPhaseChanged;
            EventBus.OnBossDefeated -= HandleBossDefeated;
        }

        private void Update()
        {
            if (hpFill != null && hpFill.gameObject.activeInHierarchy)
            {
                hpFill.fillAmount = Mathf.Lerp(hpFill.fillAmount, targetFill, Time.deltaTime * 6.5f);
            }
        }

        #region Kích Hoạt & Cập Nhật Giao Diện
        public void ShowBossBar(string bName, float curHp, float mHp, ElementType elem)
        {
            currentHp = curHp;
            maxHp = mHp;
            targetFill = maxHp > 0 ? Mathf.Clamp01(currentHp / maxHp) : 0f;

            if (bossNameText != null) bossNameText.text = bName;
            if (phaseText != null) phaseText.text = isEnraged ? "★ CUỒNG NỘ - GIAI ĐOẠN 2 ★" : "GIAI ĐOẠN 1";

            Color elemCol = ElementalSynergy.GetElementColor(elem);
            if (elementIcon != null) elementIcon.color = elemCol;
            if (elementText != null)
            {
                elementText.text = $"Hệ: {GetElementVietnamese(elem)}";
                elementText.color = elemCol;
            }

            if (hpValueText != null)
            {
                hpValueText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)} ({Mathf.RoundToInt(targetFill * 100f)}%)";
            }

            if (hpFill != null)
            {
                hpFill.color = isEnraged ? enrageHpColor : normalHpColor;
            }

            SetBarVisible(true);
        }

        public void HideBossBar()
        {
            SetBarVisible(false);
        }

        public void SetBarVisible(bool visible, bool immediate = false)
        {
            if (canvasGroup == null)
            {
                if (barRoot != null) barRoot.SetActive(visible);
                return;
            }

            if (immediate)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                if (barRoot != null) barRoot.SetActive(visible);
            }
            else
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeRoutine(visible ? 1f : 0f, 0.4f, visible));
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, bool activate)
        {
            if (activate && barRoot != null) barRoot.SetActive(true);

            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = targetAlpha;
                canvasGroup.blocksRaycasts = activate;
            }

            if (!activate && barRoot != null)
            {
                barRoot.SetActive(false);
            }
        }
        #endregion

        #region Xử Lý Sự Kiện EventBus
        private void HandleBossSpawned(string bName)
        {
            ShowBossBar(bName, maxHp, maxHp, ElementType.Fire);
        }

        private void HandleBossHpChanged(string bName, float curHp, float mHp, ElementType elem)
        {
            ShowBossBar(bName, curHp, mHp, elem);
        }

        private void HandleBossPhaseChanged(string bName, int newPhase)
        {
            if (newPhase >= 2)
            {
                isEnraged = true;
                if (phaseText != null)
                {
                    phaseText.text = "★★★ CUỒNG NỘ - GIAI ĐOẠN 2 ★★★";
                    phaseText.color = new Color(1f, 0.3f, 0.3f);
                }

                if (hpFill != null)
                {
                    StartCoroutine(FlashBossBarRoutine());
                }
            }
        }

        private IEnumerator FlashBossBarRoutine()
        {
            for (int i = 0; i < 3; i++)
            {
                if (hpFill != null) hpFill.color = flashWhiteColor;
                yield return new WaitForSeconds(0.12f);
                if (hpFill != null) hpFill.color = enrageHpColor;
                yield return new WaitForSeconds(0.12f);
            }
        }

        private void HandleBossDefeated(string bName)
        {
            if (phaseText != null) phaseText.text = "★★★ ĐÃ TIÊU DIỆT ★★★";
            targetFill = 0f;
            if (hpValueText != null) hpValueText.text = "0 / " + Mathf.CeilToInt(maxHp);

            StartCoroutine(DelayHideRoutine(2.5f));
        }

        private IEnumerator DelayHideRoutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            HideBossBar();
        }
        #endregion

        #region Trigger Vùng Hang Boss
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // Người chơi bước vào vùng Hang Boss -> Bật thanh máu
                SetBarVisible(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && currentHp > 0f)
            {
                // Người chơi chạy trốn khỏi hang Boss -> Tạm ẩn
                SetBarVisible(false);
            }
        }
        #endregion

        #region Sinh Cấu Trúc UI Tự Động
        private void EnsureBossUIStructure()
        {
            if (barRoot == null) barRoot = gameObject;
            if (canvasGroup == null) canvasGroup = barRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = barRoot.AddComponent<CanvasGroup>();

            // Tạo khung UI nếu chưa kéo Inspector
            if (hpFill == null)
            {
                RectTransform rootRect = barRoot.GetComponent<RectTransform>();
                if (rootRect != null)
                {
                    rootRect.anchorMin = new Vector2(0.5f, 1f);
                    rootRect.anchorMax = new Vector2(0.5f, 1f);
                    rootRect.pivot = new Vector2(0.5f, 1f);
                    rootRect.anchoredPosition = new Vector2(0, -25f);
                    rootRect.sizeDelta = new Vector2(520f, 60f);
                }

                // Background
                GameObject bg = new GameObject("Boss_HP_BG");
                bg.transform.SetParent(barRoot.transform, false);
                var bgImg = bg.AddComponent<Image>();
                bgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
                RectTransform bgRect = bg.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(500f, 22f);
                bgRect.anchoredPosition = new Vector2(0, -15f);

                // Fill
                GameObject fill = new GameObject("Boss_HP_Fill");
                fill.transform.SetParent(bg.transform, false);
                hpFill = fill.AddComponent<Image>();
                hpFill.type = Image.Type.Filled;
                hpFill.fillMethod = Image.FillMethod.Horizontal;
                hpFill.color = normalHpColor;
                RectTransform fillRect = fill.GetComponent<RectTransform>();
                fillRect.sizeDelta = new Vector2(494f, 18f);
                fillRect.anchoredPosition = Vector2.zero;

                // Boss Name Text
                GameObject nameObj = new GameObject("Boss_Name");
                nameObj.transform.SetParent(barRoot.transform, false);
                bossNameText = nameObj.AddComponent<TextMeshProUGUI>();
                bossNameText.text = "Hắc Diệu Ma Viên";
                bossNameText.fontSize = 20f;
                bossNameText.fontStyle = FontStyles.Bold;
                bossNameText.alignment = TextAlignmentOptions.Left;
                bossNameText.color = new Color(1f, 0.85f, 0.2f);
                RectTransform nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.sizeDelta = new Vector2(300f, 25f);
                nameRect.anchoredPosition = new Vector2(-90f, 12f);

                // Phase Text
                GameObject phaseObj = new GameObject("Boss_Phase");
                phaseObj.transform.SetParent(barRoot.transform, false);
                phaseText = phaseObj.AddComponent<TextMeshProUGUI>();
                phaseText.text = "GIAI ĐOẠN 1";
                phaseText.fontSize = 14f;
                phaseText.alignment = TextAlignmentOptions.Right;
                phaseText.color = Color.white;
                RectTransform phaseRect = phaseObj.GetComponent<RectTransform>();
                phaseRect.sizeDelta = new Vector2(200f, 25f);
                phaseRect.anchoredPosition = new Vector2(140f, 12f);

                // HP Value Text
                GameObject valObj = new GameObject("Boss_HP_Value");
                valObj.transform.SetParent(bg.transform, false);
                hpValueText = valObj.AddComponent<TextMeshProUGUI>();
                hpValueText.fontSize = 13f;
                hpValueText.alignment = TextAlignmentOptions.Center;
                hpValueText.color = Color.white;
                RectTransform valRect = valObj.GetComponent<RectTransform>();
                valRect.sizeDelta = new Vector2(250f, 20f);
                valRect.anchoredPosition = Vector2.zero;
            }
        }

        private static string GetElementVietnamese(ElementType elem)
        {
            switch (elem)
            {
                case ElementType.Fire: return "Hỏa";
                case ElementType.Water: return "Thủy";
                case ElementType.Wind: return "Phong";
                case ElementType.Earth: return "Thổ";
                case ElementType.Electric: return "Điện";
                case ElementType.Poison: return "Độc";
                case ElementType.Angel: return "Thánh";
                case ElementType.Demon: return "Ma / Ám";
                default: return "Vô Tính";
            }
        }
        #endregion
    }
}
