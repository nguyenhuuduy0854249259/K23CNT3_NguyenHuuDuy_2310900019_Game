using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ActionRPG.Classes;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.UI
{
    /// <summary>
    /// Màn hình chọn Class:
    ///   - Lần đầu vào game (isFirstSelection = true): Chọn 1 trong 3 nhánh cơ bản (Archer / Warrior / Mage).
    ///   - Chuyển nghề Advanced (isFirstSelection = false): Hiện các Advanced Class trong nhánh đã chọn.
    /// Gắn script này vào UI Panel trong Scene, điền các tham chiếu trong Inspector.
    /// </summary>
    public class ClassSelectionUI : MonoBehaviour
    {
        public static ClassSelectionUI Instance { get; private set; }

        // ──────────────────────────────────────────────────────
        // REFERENCES – kéo thả từ Inspector
        // ──────────────────────────────────────────────────────
        [Header("Panel Gốc")]
        public GameObject panelRoot;

        [Header("Tiêu Đề & Hướng Dẫn")]
        public Text titleText;
        public Text subtitleText;
        public Text levelRequirementText;

        [Header("Chi Tiết Class Đang Chọn")]
        public Text classNameText;
        public Text classDescriptionText;
        public Text classMechanicText;
        public Text classStatsText;
        public Image classIconImage;

        [Header("Danh Sách Lựa Chọn")]
        public Transform optionsContainer;        // Nơi chứa các nút chọn class
        public GameObject optionButtonPrefab;     // Prefab nút (Button + Image + Text)

        [Header("Nút Xác Nhận / Hủy")]
        public Button confirmButton;
        public Text confirmButtonText;
        public Button cancelButton;               // Chỉ hiện khi isFirstSelection = false

        [Header("Độ Khó")]
        public Text difficultyText;

        // ──────────────────────────────────────────────────────
        // STATE
        // ──────────────────────────────────────────────────────
        private CharacterClassData selectedClass;
        private bool isFirstSelection = false; // True = chọn nhánh lần đầu

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureUIHierarchy();
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // ══════════════════════════════════════════════════════
        // MỞ MÀN HÌNH
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// Mở màn hình CHỌN CLASS LẦN ĐẦU (khi mới tạo nhân vật hoặc đạt Lv10 sau Novice).
        /// availableClasses = [Class_Archer, Class_Warrior, Class_Mage]
        /// </summary>
        public void OpenFirstClassSelection(List<CharacterClassData> availableClasses)
        {
            isFirstSelection = true;
            Open(
                availableClasses,
                title: "CHỌN TRƯỜNG PHÁI (TIẾN CẤP CẤP 15)",
                subtitle: "Chọn hướng đi cho nhân vật và mở khóa tính năng Thu Phục Thú Cưng!",
                levelReq: "Yêu cầu: Cấp 15"
            );
        }

        /// <summary>
        /// Mở màn hình NÂNG CẤP CLASS (Advanced Class ở Lv30).
        /// availableClasses = danh sách class nâng cao của nhánh hiện tại.
        /// </summary>
        public void OpenAdvancedClassSelection(List<CharacterClassData> availableClasses, int requiredLevel)
        {
            isFirstSelection = false;
            Open(
                availableClasses,
                title: "NÂNG CẤP NGHỀ NGHIỆP",
                subtitle: "Chọn hướng chuyên sâu phù hợp với phong cách chơi của bạn.",
                levelReq: $"Yêu cầu: Cấp {requiredLevel}"
            );
        }

        private void Open(List<CharacterClassData> classes, string title, string subtitle, string levelReq)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            if (titleText != null) titleText.text = title;
            if (subtitleText != null) subtitleText.text = subtitle;
            if (levelRequirementText != null) levelRequirementText.text = levelReq;

            // Ẩn/hiện nút Hủy dựa trên loại màn hình: lần đầu không cho hủy, chuyển nghề nâng cao thì có thể hủy
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(!isFirstSelection);
                cancelButton.onClick.RemoveAllListeners();
                if (!isFirstSelection) cancelButton.onClick.AddListener(Close);
            }

            // Cập nhật nhãn nút xác nhận
            if (confirmButtonText != null)
                confirmButtonText.text = isFirstSelection ? "CHỌN TRƯỜNG PHÁI NÀY" : "CHUYỂN NGHỀ";

            BuildOptionButtons(classes);

            if (classes.Count > 0)
                SelectClass(classes[0]);

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmSelection);
            }
        }

        // ══════════════════════════════════════════════════════
        // XÂY DỰNG DANH SÁCH CHỌN
        // ══════════════════════════════════════════════════════

        private static GameObject CreateUIGameObject(string name, Transform parent = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private void BuildOptionButtons(List<CharacterClassData> classes)
        {
            if (optionsContainer == null) return;

            // Xóa nút cũ
            foreach (Transform child in optionsContainer)
                Destroy(child.gameObject);

            foreach (var classData in classes)
            {
                GameObject btnObj;

                // Dùng prefab nếu có, tạo tạm nếu chưa có
                if (optionButtonPrefab != null)
                {
                    btnObj = Instantiate(optionButtonPrefab, optionsContainer);
                }
                else
                {
                    btnObj = CreateUIGameObject($"Btn_{classData.className}", optionsContainer);
                    btnObj.AddComponent<Button>();
                    Text txt = btnObj.AddComponent<Text>();
                    txt.font = ActionRPG.UI.FontManager.GetTimesNewRomanLegacy() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    txt.fontSize = 16;
                    txt.color = Color.white;
                    txt.alignment = TextAnchor.MiddleCenter;
                }

                // Điền tên class vào button
                Text btnText = btnObj.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = classData.className;

                // Gán icon nếu có Image
                Image btnIcon = btnObj.GetComponentInChildren<Image>();
                if (btnIcon != null && classData.classIcon != null)
                    btnIcon.sprite = classData.classIcon;

                Button btn = btnObj.GetComponent<Button>();
                CharacterClassData captured = classData;
                if (btn != null)
                    btn.onClick.AddListener(() => SelectClass(captured));
            }
        }

        // ══════════════════════════════════════════════════════
        // CHỌN CLASS – CẬP NHẬT PREVIEW
        // ══════════════════════════════════════════════════════

        private void ClearDetailTexts()
        {
            if (classNameText != null) classNameText.text = string.Empty;
            if (classDescriptionText != null) classDescriptionText.text = string.Empty;
            if (classMechanicText != null) classMechanicText.text = string.Empty;
            if (classStatsText != null) classStatsText.text = string.Empty;
            if (difficultyText != null) difficultyText.text = string.Empty;
            if (classIconImage != null) classIconImage.sprite = null;
        }

        private void SelectClass(CharacterClassData classData)
        {
            ClearDetailTexts();
            if (classData == null) return;
            selectedClass = classData;

            if (classNameText != null) classNameText.text = classData.className;

            if (classDescriptionText != null) classDescriptionText.text = classData.description;

            if (classMechanicText != null) classMechanicText.text =
                string.IsNullOrEmpty(classData.mechanicDescription)
                    ? "(Không có cơ chế đặc biệt)"
                    : "⚙ " + classData.mechanicDescription;

            if (classStatsText != null)
            {
                classStatsText.text =
                    $"Công: {classData.baseAtk}  |  Thủ: {classData.baseDef}\n" +
                    $"Máu: {classData.baseHp}  |  Mana: {classData.baseMp}\n" +
                    $"Tốc: {classData.baseSpeed}  |  Chí mạng: {classData.baseCritRate * 100f:F0}%";
            }

            if (classIconImage != null && classData.classIcon != null)
                classIconImage.sprite = classData.classIcon;

            if (difficultyText != null)
            {
                string stars = GetDifficultyStars(classData.difficulty);
                string diffLabel = classData.difficulty switch
                {
                    1 => "Dễ",
                    2 => "Trung Bình",
                    3 => "Khó",
                    4 => "Rất Khó",
                    5 => "Master",
                    _ => ""
                };
                difficultyText.text = $"Độ Khó: {diffLabel} {stars}";
            }
        }

        private static string GetDifficultyStars(int diff)
        {
            string stars = "";
            for (int i = 0; i < 5; i++)
                stars += i < diff ? "★" : "☆";
            return stars;
        }

        // ══════════════════════════════════════════════════════
        // XÁC NHẬN & HỦY
        // ══════════════════════════════════════════════════════

        private void OnConfirmSelection()
        {
            if (selectedClass == null || ClassSystem.Instance == null) return;

            // Nếu class yêu cầu Tier 2 (Lv 10+), tự động nâng cấp tối thiểu Lv 10 khi người chơi đang ở Lv 1
            int minLevel = selectedClass.tier >= 2 ? 20 : (selectedClass.tier == 1 || selectedClass.branch != ClassBranch.Novice ? 10 : 1);
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null && stats.level < minLevel)
            {
                stats.level = minLevel;
                stats.maxExp = CharacterStats.GetRequiredExpForLevel(minLevel);
                stats.currentExp = 0;
                stats.maxHp = 100f + (minLevel - 1) * 18f;
                stats.maxMp = 50f + (minLevel - 1) * 10f;
                stats.currentHp = stats.maxHp;
                stats.currentMp = stats.maxMp;
                stats.CheckPetUnlock(true);
                stats.UpdateStatsUI();
                EventBus.OnPlayerLevelUp?.Invoke(minLevel);
            }

            ClassSystem.Instance.PromoteTo(selectedClass);

            // Nạp trực tiếp 5 kỹ năng của Class vào PlayerSkillCombat2D
            var combat = FindObjectOfType<Combat.PlayerSkillCombat2D>();
            if (combat != null) combat.LoadClassSkills(selectedClass);

            // Làm mới GameplayHUD
            var hud = FindObjectOfType<Gameplay.GameplayHUD>();
            if (hud != null) hud.RefreshClassUI(selectedClass);

            // Cập nhật vũ khí hệ phái
            var weaponSocket = FindObjectOfType<Presentation.Views.WeaponSocketView>();
            if (weaponSocket != null) weaponSocket.ApplyClassWeapon(selectedClass.branch);

            var equipVisual = FindObjectOfType<Presentation.Views.PlayerEquipmentVisualView>();
            if (equipVisual != null) equipVisual.UpdateVisuals();

            var fourDir = FindObjectOfType<Visuals.LuminaFourDirectionCharacterAnimator>();
            if (fourDir != null) fourDir.ApplyClassBranch(selectedClass.branch);

            Debug.Log($"[ClassSelectionUI] Nhân vật đã chọn: {selectedClass.className}");
            Close();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        #region Runtime UI Auto-Hierarchy Builder
        public void EnsureUIHierarchy()
        {
            if (panelRoot != null) return;

            Transform canvasTrans = transform.parent != null ? transform.parent : transform;
            if (canvasTrans.GetComponentInParent<Canvas>() == null && canvasTrans.GetComponent<Canvas>() == null)
            {
                var c = canvasTrans.gameObject.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var cs = canvasTrans.gameObject.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                canvasTrans.gameObject.AddComponent<GraphicRaycaster>();
            }

            Font defaultFont = ActionRPG.UI.FontManager.GetTimesNewRomanLegacy() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0) defaultFont = fonts[0];
            }

            // Panel Root
            GameObject root = CreateUIGameObject("ClassSelection_Panel", canvasTrans);
            panelRoot = root;

            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900, 600);

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.09f, 0.14f, 0.97f);
            var outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.6f, 0.95f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            // 1. Header
            GameObject header = CreateUIGameObject("Header", root.transform);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.anchoredPosition = new Vector2(0, -15);
            headerRt.sizeDelta = new Vector2(-40, 70);

            GameObject titleObj = CreateUIGameObject("TitleText", header.transform);
            var titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0.5f);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;
            titleText = titleObj.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(1f, 0.85f, 0.3f);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "CHỌN NGHỀ NGHIỆP";

            GameObject subtitleObj = CreateUIGameObject("SubtitleText", header.transform);
            var subRt = subtitleObj.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0, 0);
            subRt.anchorMax = new Vector2(0.7f, 0.5f);
            subRt.offsetMin = Vector2.zero;
            subRt.offsetMax = Vector2.zero;
            subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.font = defaultFont;
            subtitleText.fontSize = 14;
            subtitleText.color = new Color(0.8f, 0.85f, 0.9f);
            subtitleText.alignment = TextAnchor.MiddleLeft;
            subtitleText.text = "Chọn nhánh phù hợp với lối chơi của bạn";

            GameObject reqObj = CreateUIGameObject("LevelReqText", header.transform);
            var reqRt = reqObj.GetComponent<RectTransform>();
            reqRt.anchorMin = new Vector2(0.7f, 0);
            reqRt.anchorMax = new Vector2(1, 0.5f);
            reqRt.offsetMin = Vector2.zero;
            reqRt.offsetMax = Vector2.zero;
            levelRequirementText = reqObj.AddComponent<Text>();
            levelRequirementText.font = defaultFont;
            levelRequirementText.fontSize = 14;
            levelRequirementText.fontStyle = FontStyle.Bold;
            levelRequirementText.color = new Color(0.4f, 0.9f, 0.5f);
            levelRequirementText.alignment = TextAnchor.MiddleRight;
            levelRequirementText.text = "Yêu cầu: Cấp 15";

            // 2. Body Split: Left (Class Options List) & Right (Details)
            GameObject body = CreateUIGameObject("Body", root.transform);
            var bodyRt = body.GetComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0, 0.12f);
            bodyRt.anchorMax = new Vector2(1, 0.85f);
            bodyRt.offsetMin = new Vector2(20, 0);
            bodyRt.offsetMax = new Vector2(-20, 0);

            // Left Panel
            GameObject leftPanel = CreateUIGameObject("LeftListPanel", body.transform);
            var leftRt = leftPanel.GetComponent<RectTransform>();
            leftRt.anchorMin = new Vector2(0, 0);
            leftRt.anchorMax = new Vector2(0.35f, 1);
            leftRt.offsetMin = Vector2.zero;
            leftRt.offsetMax = Vector2.zero;

            var leftBg = leftPanel.AddComponent<Image>();
            leftBg.color = new Color(0.04f, 0.05f, 0.08f, 0.8f);

            GameObject scrollObj = CreateUIGameObject("ScrollView", leftPanel.transform);
            var scrollRt = scrollObj.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(8, 8);
            scrollRt.offsetMax = new Vector2(-8, -8);
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            GameObject contentObj = CreateUIGameObject("OptionsContainer", scrollObj.transform);
            var contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRt;
            optionsContainer = contentObj.transform;

            // Right Panel (Details)
            GameObject rightPanel = CreateUIGameObject("RightDetailsPanel", body.transform);
            var rightRt = rightPanel.GetComponent<RectTransform>();
            rightRt.anchorMin = new Vector2(0.37f, 0);
            rightRt.anchorMax = new Vector2(1, 1);
            rightRt.offsetMin = Vector2.zero;
            rightRt.offsetMax = Vector2.zero;

            var rightBg = rightPanel.AddComponent<Image>();
            rightBg.color = new Color(0.05f, 0.07f, 0.10f, 0.85f);

            // Icon Image
            GameObject iconObj = CreateUIGameObject("ClassIcon", rightPanel.transform);
            var iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 1);
            iconRt.anchorMax = new Vector2(0, 1);
            iconRt.pivot = new Vector2(0, 1);
            iconRt.anchoredPosition = new Vector2(16, -16);
            iconRt.sizeDelta = new Vector2(64, 64);
            classIconImage = iconObj.AddComponent<Image>();
            classIconImage.color = Color.white;

            // Class Name
            GameObject nameObj = CreateUIGameObject("ClassName", rightPanel.transform);
            var nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 1);
            nameRt.anchorMax = new Vector2(1, 1);
            nameRt.pivot = new Vector2(0, 1);
            nameRt.anchoredPosition = new Vector2(90, -16);
            nameRt.sizeDelta = new Vector2(-106, 32);
            classNameText = nameObj.AddComponent<Text>();
            classNameText.font = defaultFont;
            classNameText.fontSize = 20;
            classNameText.fontStyle = FontStyle.Bold;
            classNameText.color = new Color(0.3f, 0.85f, 1f);
            classNameText.alignment = TextAnchor.MiddleLeft;

            // Difficulty
            GameObject diffObj = CreateUIGameObject("Difficulty", rightPanel.transform);
            var diffRt = diffObj.GetComponent<RectTransform>();
            diffRt.anchorMin = new Vector2(0, 1);
            diffRt.anchorMax = new Vector2(1, 1);
            diffRt.pivot = new Vector2(0, 1);
            diffRt.anchoredPosition = new Vector2(90, -48);
            diffRt.sizeDelta = new Vector2(-106, 26);
            difficultyText = diffObj.AddComponent<Text>();
            difficultyText.font = defaultFont;
            difficultyText.fontSize = 14;
            difficultyText.color = new Color(1f, 0.75f, 0.2f);
            difficultyText.alignment = TextAnchor.MiddleLeft;

            // Description
            GameObject descObj = CreateUIGameObject("ClassDescription", rightPanel.transform);
            var descRt = descObj.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 1);
            descRt.anchorMax = new Vector2(1, 1);
            descRt.pivot = new Vector2(0, 1);
            descRt.anchoredPosition = new Vector2(16, -90);
            descRt.sizeDelta = new Vector2(-32, 60);
            classDescriptionText = descObj.AddComponent<Text>();
            classDescriptionText.font = defaultFont;
            classDescriptionText.fontSize = 14;
            classDescriptionText.color = new Color(0.85f, 0.88f, 0.92f);
            classDescriptionText.alignment = TextAnchor.UpperLeft;

            // Mechanic Box
            GameObject mechObj = CreateUIGameObject("ClassMechanic", rightPanel.transform);
            var mechRt = mechObj.GetComponent<RectTransform>();
            mechRt.anchorMin = new Vector2(0, 1);
            mechRt.anchorMax = new Vector2(1, 1);
            mechRt.pivot = new Vector2(0, 1);
            mechRt.anchoredPosition = new Vector2(16, -155);
            mechRt.sizeDelta = new Vector2(-32, 80);
            classMechanicText = mechObj.AddComponent<Text>();
            classMechanicText.font = defaultFont;
            classMechanicText.fontSize = 13;
            classMechanicText.color = new Color(0.5f, 0.95f, 0.85f);
            classMechanicText.alignment = TextAnchor.UpperLeft;

            // Stats Box
            GameObject statsObj = CreateUIGameObject("ClassStats", rightPanel.transform);
            var statsRt = statsObj.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0, 0);
            statsRt.anchorMax = new Vector2(1, 0);
            statsRt.pivot = new Vector2(0.5f, 0);
            statsRt.anchoredPosition = new Vector2(0, 10);
            statsRt.sizeDelta = new Vector2(-32, 70);
            classStatsText = statsObj.AddComponent<Text>();
            classStatsText.font = defaultFont;
            classStatsText.fontSize = 13;
            classStatsText.color = new Color(0.9f, 0.85f, 0.7f);
            classStatsText.alignment = TextAnchor.MiddleLeft;

            // 3. Bottom Action Buttons
            GameObject footer = CreateUIGameObject("Footer", root.transform);
            var footerRt = footer.GetComponent<RectTransform>();
            footerRt.anchorMin = new Vector2(0, 0);
            footerRt.anchorMax = new Vector2(1, 0.12f);
            footerRt.offsetMin = new Vector2(20, 8);
            footerRt.offsetMax = new Vector2(-20, -8);

            // Confirm Button
            GameObject confBtnObj = CreateUIGameObject("Btn_Confirm", footer.transform);
            var confRt = confBtnObj.GetComponent<RectTransform>();
            confRt.anchorMin = new Vector2(0.6f, 0);
            confRt.anchorMax = new Vector2(1f, 1f);
            confRt.offsetMin = Vector2.zero;
            confRt.offsetMax = Vector2.zero;
            var confImg = confBtnObj.AddComponent<Image>();
            confImg.color = new Color(0.15f, 0.5f, 0.85f);
            confirmButton = confBtnObj.AddComponent<Button>();

            GameObject confTxtObj = CreateUIGameObject("Text", confBtnObj.transform);
            var confTxtRt = confTxtObj.GetComponent<RectTransform>();
            confTxtRt.anchorMin = Vector2.zero;
            confTxtRt.anchorMax = Vector2.one;
            confirmButtonText = confTxtObj.AddComponent<Text>();
            confirmButtonText.font = defaultFont;
            confirmButtonText.fontSize = 16;
            confirmButtonText.fontStyle = FontStyle.Bold;
            confirmButtonText.color = Color.white;
            confirmButtonText.alignment = TextAnchor.MiddleCenter;
            confirmButtonText.text = "XÁC NHẬN CHỌN";

            // Cancel Button
            GameObject cancelBtnObj = CreateUIGameObject("Btn_Cancel", footer.transform);
            var cancelRt = cancelBtnObj.GetComponent<RectTransform>();
            cancelRt.anchorMin = new Vector2(0.35f, 0);
            cancelRt.anchorMax = new Vector2(0.55f, 1f);
            cancelRt.offsetMin = Vector2.zero;
            cancelRt.offsetMax = Vector2.zero;
            var cancelImg = cancelBtnObj.AddComponent<Image>();
            cancelImg.color = new Color(0.35f, 0.25f, 0.25f);
            cancelButton = cancelBtnObj.AddComponent<Button>();

            GameObject cancelTxtObj = CreateUIGameObject("Text", cancelBtnObj.transform);
            var cancelTxtRt = cancelTxtObj.GetComponent<RectTransform>();
            cancelTxtRt.anchorMin = Vector2.zero;
            cancelTxtRt.anchorMax = Vector2.one;
            var cancelTxt = cancelTxtObj.AddComponent<Text>();
            cancelTxt.font = defaultFont;
            cancelTxt.fontSize = 14;
            cancelTxt.color = Color.white;
            cancelTxt.alignment = TextAnchor.MiddleCenter;
            cancelTxt.text = "ĐÓNG";
        }
        #endregion
    }
}
