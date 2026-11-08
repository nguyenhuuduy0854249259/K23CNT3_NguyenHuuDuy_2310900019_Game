using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Core;
using ActionRPG.Pets;
using ActionRPG.Combat;
using ActionRPG.Characters;

namespace ActionRPG.UI.Gameplay
{
    /// <summary>
    /// Widget hiển thị thông tin Thú Cưng đồng hành trên HUD (Góc trên bên trái, cạnh thông tin Người Chơi):
    /// - Avatar Thú Cưng
    /// - Icon & Phù hiệu Hệ Nguyên Tố (8 Hệ Lumina)
    /// - Thanh Máu Pet (HP Bar Fill + Text)
    /// - Điểm Thân Mật (Bond 0 - 100)
    /// - Tên Pet & Cấp độ
    /// - Gợi ý phím tắt [T] (Triệu hồi / Thu hồi)
    /// Tự động ẩn/hiện mượt mà khi Pet xuất chiến hoặc thu hồi.
    /// </summary>
    public class PetHUDWidget : MonoBehaviour
    {
        public static PetHUDWidget Instance { get; private set; }

        [Header("Trạng Thái Khóa Cấp Độ (Sprint 1)")]
        [Tooltip("Nếu bật: Ẩn hoàn toàn widget khi chưa đủ cấp. Nếu tắt: Hiển thị biểu tượng Ổ Khóa kèm text 'Mở khóa ở Cấp 10'")]
        public bool hideWhenLocked = false;
        public GameObject lockedPanel;
        public Image lockIcon;
        public TMP_Text lockedText;
        public GameObject petContentPanel; // Panel chứa avatar, hp, bond...

        [Header("Banner / Toast Thông Báo Nổi")]
        public GameObject toastPanel;
        public TMP_Text toastText;
        private Coroutine toastCoroutine;

        [Header("Khung Giao Diện Chính")]
        public GameObject widgetRoot;
        public CanvasGroup canvasGroup;

        [Header("Avatar & Nhận Diện")]
        public Image avatarImage;
        public Image elementIcon;
        public Image elementBadgeBg;
        public TMP_Text elementNameText;
        public TMP_Text petNameText;
        public TMP_Text petLevelText;

        [Header("Thanh Sinh Lực (HP)")]
        public Image hpBarFill;
        public TMP_Text hpText;

        [Header("Điểm Gắn Kết Thân Mật (Bond: 0 - 100)")]
        public Image bondBarFill;
        public TMP_Text bondText;

        [Header("Gợi Ý Phím Tắt")]
        public TMP_Text hotkeyHintText;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (widgetRoot == null) widgetRoot = gameObject;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && widgetRoot != null) canvasGroup = widgetRoot.AddComponent<CanvasGroup>();

            // Đăng ký sự kiện từ EventBus
            EventBus.OnPetSummoned += HandlePetSummoned;
            EventBus.OnPetDismissed += HandlePetDismissed;
            EventBus.OnPetHpChanged += HandlePetHpChanged;
            EventBus.OnPetBondChanged += HandlePetBondChanged;
            EventBus.OnPetLevelUp += HandlePetLevelUp;
            EventBus.OnPetSystemUnlocked += HandlePetSystemUnlocked;
            EventBus.OnPlayerLevelUp += HandlePlayerLevelUp;
            EventBus.OnShowUIMessage += ShowToast;
        }

        private void Start()
        {
            var btn = GetComponent<Button>();
            if (btn == null && widgetRoot != null) btn = widgetRoot.GetComponent<Button>() ?? widgetRoot.AddComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (PlayerPetRoster.Instance != null)
                        PlayerPetRoster.Instance.TogglePetSummon();
                });
            }

            RefreshDisplay();

            foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
            {
                ActionRPG.UI.FontManager.ApplyFont(txt);
            }
        }

        private void OnDestroy()
        {
            EventBus.OnPetSummoned -= HandlePetSummoned;
            EventBus.OnPetDismissed -= HandlePetDismissed;
            EventBus.OnPetHpChanged -= HandlePetHpChanged;
            EventBus.OnPetBondChanged -= HandlePetBondChanged;
            EventBus.OnPetLevelUp -= HandlePetLevelUp;
            EventBus.OnPetSystemUnlocked -= HandlePetSystemUnlocked;
            EventBus.OnPlayerLevelUp -= HandlePlayerLevelUp;
            EventBus.OnShowUIMessage -= ShowToast;
        }

        public bool IsPetUnlocked()
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null)
            {
                return stats.isPetUnlocked || stats.level >= stats.petUnlockLevel;
            }
            return false;
        }

        public int GetUnlockLevel()
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            return stats != null ? stats.petUnlockLevel : 10;
        }

        public void RefreshDisplay()
        {
            bool unlocked = IsPetUnlocked();

            if (!unlocked)
            {
                if (hideWhenLocked)
                {
                    SetWidgetVisible(false, true);
                }
                else
                {
                    SetWidgetVisible(true, true);
                    SetLockState(true);
                }
                return;
            }

            // Đã mở khóa: Ẩn ổ khóa
            SetLockState(false);

            // Kiểm tra xem hiện tại Player đã có Pet xuất chiến chưa
            if (PlayerPetRoster.Instance != null && PlayerPetRoster.Instance.activePetInstance != null)
            {
                DisplayPet(PlayerPetRoster.Instance.activePetInstance);
            }
            else
            {
                // Tìm PetController trong Scene
                PetController scenePet = FindObjectOfType<PetController>();
                if (scenePet != null && scenePet.gameObject.activeInHierarchy && scenePet.petData != null)
                {
                    DisplayPetData(scenePet.petData, scenePet.level, scenePet.currentHp, scenePet.maxHp, scenePet.bondLevel);
                }
                else
                {
                    SetWidgetVisible(false, true);
                }
            }
        }

        private void HandlePetSystemUnlocked()
        {
            RefreshDisplay();
            ShowToast("🎉 Hệ Thống Thú Cưng Đã Mở Khóa! Bấm [P] để Triệu Hồi.");
        }

        private void HandlePlayerLevelUp(int newLevel)
        {
            RefreshDisplay();
        }

        #region Xử Lý Sự Kiện EventBus
        private void HandlePetSummoned(object petObj)
        {
            if (petObj is PetInstance pet)
            {
                DisplayPet(pet);
            }
            else if (petObj is PetData data)
            {
                DisplayPetData(data, 1, data.maxHp, data.maxHp, 50f);
            }
        }

        private void HandlePetDismissed(object petObj)
        {
            SetWidgetVisible(false);
        }

        private void HandlePetHpChanged(float current, float max)
        {
            if (hpBarFill != null) hpBarFill.fillAmount = max > 0 ? Mathf.Clamp01(current / max) : 0f;
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void HandlePetBondChanged(float currentBond, float maxBond)
        {
            if (bondBarFill != null) bondBarFill.fillAmount = maxBond > 0 ? Mathf.Clamp01(currentBond / maxBond) : 0f;
            if (bondText != null) bondText.text = $"Thân Mật: {Mathf.RoundToInt(currentBond)}/100";
        }

        private void HandlePetLevelUp(int newLevel)
        {
            if (petLevelText != null) petLevelText.text = $"Lv. {newLevel}";
            StartCoroutine(PulseAnimation());
        }
        #endregion

        #region Cập Nhật Hiển Thị Dữ Liệu
        public void DisplayPet(PetInstance pet)
        {
            if (pet == null) return;
            DisplayPetData(pet.petData, pet.level, pet.currentHp, pet.MaxHp, pet.bondLevel, pet.DisplayName);
        }

        public void DisplayPetData(PetData data, int level, float currentHp, float maxHp, float bond, string customName = "")
        {
            SetLockState(false);
            SetWidgetVisible(true);

            // Tên và cấp độ
            if (petNameText != null)
            {
                string pName = !string.IsNullOrEmpty(customName) ? customName : (data != null ? data.petName : "Thú Cưng");
                petNameText.text = pName;
            }

            if (petLevelText != null) petLevelText.text = $"Lv. {level}";

            // Avatar
            if (avatarImage != null && data != null && (data.icon != null || data.petSprite != null))
            {
                avatarImage.sprite = data.icon != null ? data.icon : data.petSprite;
                avatarImage.color = Color.white;
            }

            // Nguyên tố & Màu sắc nhận diện
            ElementType elem = data != null ? data.element : ElementType.Fire;
            UpdateElementVisuals(elem);

            // Máu và Thân Mật
            HandlePetHpChanged(currentHp, maxHp);
            HandlePetBondChanged(bond, 100f);

            // Gợi ý phím
            if (hotkeyHintText != null) hotkeyHintText.text = "[P] Thu Hồi";
        }

        private void UpdateElementVisuals(ElementType elem)
        {
            Color elemColor = GetElementColor(elem);

            if (elementBadgeBg != null) elementBadgeBg.color = elemColor;
            if (elementNameText != null)
            {
                elementNameText.text = GetElementVietnameseName(elem);
                elementNameText.color = elemColor;
            }
        }

        public static Color GetElementColor(ElementType elem)
        {
            switch (elem)
            {
                case ElementType.Fire: return new Color(1.0f, 0.35f, 0.15f);      // Cam đỏ rực lửa
                case ElementType.Water: return new Color(0.2f, 0.65f, 1.0f);      // Xanh lam biển cả
                case ElementType.Electric: return new Color(1.0f, 0.85f, 0.1f);   // Vàng sấm sét
                case ElementType.Earth: return new Color(0.7f, 0.5f, 0.3f);       // Nâu đất kiên cố
                case ElementType.Wind: return new Color(0.3f, 0.9f, 0.65f);       // Xanh ngọc phong lôi
                case ElementType.Poison: return new Color(0.75f, 0.2f, 0.9f);     // Tím độc dược
                case ElementType.Angel: return new Color(1.0f, 0.95f, 0.6f);      // Vàng ánh kim thiên thần
                case ElementType.Demon: return new Color(0.9f, 0.15f, 0.35f);     // Đỏ ma quỷ thẫm
                default: return Color.gray;
            }
        }

        public static string GetElementVietnameseName(ElementType elem)
        {
            switch (elem)
            {
                case ElementType.Fire: return "Hỏa";
                case ElementType.Water: return "Thủy";
                case ElementType.Electric: return "Điện";
                case ElementType.Earth: return "Thổ";
                case ElementType.Wind: return "Phong";
                case ElementType.Poison: return "Độc";
                case ElementType.Angel: return "Thánh";
                case ElementType.Demon: return "Ma";
                default: return "Vô Hệ";
            }
        }
        #endregion

        #region Quản Lý Khóa Cấp Độ & Thông Báo UI
        /// <summary>
        /// Bật/Tắt trạng thái Khóa hiển thị (Khi người chơi chưa đạt cấp petUnlockLevel).
        /// </summary>
        public void SetLockState(bool locked)
        {
            if (locked)
            {
                EnsureLockedPanelUI();
            }

            if (lockedPanel != null)
            {
                lockedPanel.SetActive(locked);
                if (locked && lockedText != null)
                {
                    lockedText.text = $"🔒 Mở khóa ở Cấp {GetUnlockLevel()}";
                }
            }

            if (petContentPanel != null)
            {
                petContentPanel.SetActive(!locked);
            }
            else
            {
                // Nếu chưa phân tách riêng petContentPanel, ẩn/hiện trực tiếp các thành phần con nhận diện
                if (avatarImage != null) avatarImage.gameObject.SetActive(!locked);
                if (elementBadgeBg != null) elementBadgeBg.gameObject.SetActive(!locked);
                if (elementNameText != null) elementNameText.gameObject.SetActive(!locked);
                if (petNameText != null) petNameText.gameObject.SetActive(!locked);
                if (petLevelText != null) petLevelText.gameObject.SetActive(!locked);
                if (hpBarFill != null && hpBarFill.transform.parent != null) hpBarFill.transform.parent.gameObject.SetActive(!locked);
                if (bondBarFill != null && bondBarFill.transform.parent != null) bondBarFill.transform.parent.gameObject.SetActive(!locked);
                if (hotkeyHintText != null) hotkeyHintText.gameObject.SetActive(!locked);
            }
        }

        private void EnsureLockedPanelUI()
        {
            if (lockedPanel != null) return;
            if (widgetRoot == null) return;

            Transform existing = widgetRoot.transform.Find("LockedPanel");
            if (existing != null)
            {
                lockedPanel = existing.gameObject;
                lockedText = lockedPanel.GetComponentInChildren<TMP_Text>();
                return;
            }

            // Tự động khởi tạo Panel Khóa runtime nếu chưa gắn trong Inspector
            GameObject lockObj = new GameObject("LockedPanel", typeof(RectTransform));
            lockObj.transform.SetParent(widgetRoot.transform, false);
            lockedPanel = lockObj;

            RectTransform rt = lockObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = lockObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.15f, 0.94f);

            GameObject textObj = new GameObject("LockText", typeof(RectTransform));
            textObj.transform.SetParent(lockObj.transform, false);
            RectTransform trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(8, 4);
            trt.offsetMax = new Vector2(-8, -4);

            lockedText = textObj.AddComponent<TextMeshProUGUI>();
            lockedText.alignment = TextAlignmentOptions.Center;
            lockedText.fontSize = 13;
            lockedText.color = new Color(1f, 0.85f, 0.25f);
            lockedText.text = $"🔒 Mở khóa ở Cấp {GetUnlockLevel()}";
        }

        /// <summary>
        /// Hiển thị thông báo nổi Toast trên màn hình khi người chơi tương tác chưa hợp lệ.
        /// </summary>
        public void ShowToast(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            EnsureToastPanelUI();
            if (toastPanel == null || toastText == null) return;

            if (toastCoroutine != null) StopCoroutine(toastCoroutine);
            
            // Thêm kiểm tra activeInHierarchy để tránh lỗi coroutine trên object đang tắt
            if (gameObject.activeInHierarchy)
            {
                toastCoroutine = StartCoroutine(ToastRoutine(message));
            }
        }

        private void EnsureToastPanelUI()
        {
            if (toastPanel != null) return;
            Transform canvasTransform = transform.root;

            Transform existing = canvasTransform.Find("PetHUD_ToastPanel");
            if (existing != null)
            {
                toastPanel = existing.gameObject;
                toastText = toastPanel.GetComponentInChildren<TMP_Text>();
                return;
            }

            GameObject tObj = new GameObject("PetHUD_ToastPanel", typeof(RectTransform));
            tObj.transform.SetParent(canvasTransform, false);
            toastPanel = tObj;

            RectTransform rt = tObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.82f);
            rt.anchorMax = new Vector2(0.5f, 0.82f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(420f, 48f);

            Image bg = tObj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.18f, 0.96f);

            Outline outline = tObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.65f, 0.15f, 0.9f);
            outline.effectDistance = new Vector2(2, -2);

            CanvasGroup cg = tObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            GameObject txtObj = new GameObject("ToastText", typeof(RectTransform));
            txtObj.transform.SetParent(tObj.transform, false);
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 4);
            trt.offsetMax = new Vector2(-10, -4);

            toastText = txtObj.AddComponent<TextMeshProUGUI>();
            toastText.alignment = TextAlignmentOptions.Center;
            toastText.fontSize = 14;
            toastText.color = new Color(1f, 0.95f, 0.6f);
            toastText.text = "";

            tObj.SetActive(false);
        }

        private IEnumerator ToastRoutine(string msg)
        {
            toastPanel.SetActive(true);
            toastText.text = msg;
            CanvasGroup cg = toastPanel.GetComponent<CanvasGroup>();

            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                if (cg != null) cg.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.2f);
                yield return null;
            }
            if (cg != null) cg.alpha = 1f;

            yield return new WaitForSeconds(2.5f);

            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                if (cg != null) cg.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                yield return null;
            }
            if (cg != null) cg.alpha = 0f;
            toastPanel.SetActive(false);
        }
        #endregion

        #region Hiệu Ứng Fade & Nhấp Nháy UI
        public void SetWidgetVisible(bool visible, bool immediate = false)
        {
            if (widgetRoot == null) return;

            if (immediate)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = visible ? 1f : 0f;
                    canvasGroup.interactable = visible;
                    canvasGroup.blocksRaycasts = visible;
                }
                widgetRoot.SetActive(visible);
                return;
            }

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeWidgetRoutine(visible));
        }

        private IEnumerator FadeWidgetRoutine(bool visible)
        {
            if (visible) widgetRoot.SetActive(true);

            if (canvasGroup != null)
            {
                float startAlpha = canvasGroup.alpha;
                float targetAlpha = visible ? 1f : 0f;
                float duration = 0.25f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                    yield return null;
                }

                canvasGroup.alpha = targetAlpha;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (!visible) widgetRoot.SetActive(false);
        }

        private IEnumerator PulseAnimation()
        {
            if (widgetRoot == null) yield break;
            Vector3 originalScale = widgetRoot.transform.localScale;
            widgetRoot.transform.localScale = originalScale * 1.15f;
            yield return new WaitForSeconds(0.18f);
            widgetRoot.transform.localScale = originalScale;
        }
        #endregion
    }
}
