using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Core;
using ActionRPG.Characters;
using ActionRPG.Items;
using ActionRPG.UI;
using ActionRPG.UI.Character;

namespace ActionRPG.UI.Gameplay
{
    /// <summary>
    /// Central controller for the gameplay HUD. Handles avatar, name, level, HP/MP/EXP bars,
    /// skill bar, quick slots [E] & [R], minimap, quest summary and inventory shortcut buttons.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Top‑Left – Player Info")]
        public Image avatarImage;            // Player portrait
        public TMP_Text playerNameText;      // Name
        public TMP_Text levelText;           // Lv.
        public Image hpBarFill;              // HP fill (Image type – Fill Method = Horizontal)
        public Image shieldBarFill;          // Virtual HP / Shield overlay bar on HP bar
        public TMP_Text hpText;              // HP numbers
        public Image mpBarFill;
        public TMP_Text mpText;
        public Image expBarFill;
        public TMP_Text expText;             // Optional numeric XP

        [Header("Top‑Left – Pet Companion Widget")]
        public PetHUDWidget petWidget;       // Pet companion widget displaying avatar, element, HP & bond

        [Header("Quick Slots (Sprint 3: [E] HP Potion, [R] MP Potion)")]
        public GameObject quickSlotsContainer;
        public Image hpPotionIcon;
        public Image hpPotionCooldownOverlay;
        public TMP_Text hpPotionCountText;
        public Button hpPotionButton;

        public Image mpPotionIcon;
        public Image mpPotionCooldownOverlay;
        public TMP_Text mpPotionCountText;
        public Button mpPotionButton;

        public static float HpPotionCooldownTimer { get; private set; } = 0f;
        public static float MpPotionCooldownTimer { get; private set; } = 0f;
        public const float DefaultPotionCooldown = 8.0f;

        [Header("Top‑Right – Quest Summary")]
        public TMP_Text questTitleText;
        public Slider questProgressSlider;   // Simple progress bar
        public Button questPanelButton;      // Opens full quest panel

        [Header("Bottom‑Right – Skill Bar")]
        public SkillSlotUI[] skillSlots;      // Array of skill slot UI components

        [Header("Bottom‑Left – Main Menu Buttons")]
        public Button menuButton;            // Opens main menu panel
        public Button characterButton;        // Opens Character UI
        public Button inventoryButton;        // Opens Inventory UI
        public Button equipmentButton;        // Opens Equipment UI (may reuse Inventory)
        public Button skillsButton;           // Opens Skills UI
        public Button questButton;            // Opens Quest UI
        public Button mapButton;              // Opens Minimap (full screen)
        public Button settingsButton;         // Opens Settings panel
        public Button demoClassButton;        // Opens Class Demo Switcher panel

        [Header("Center – Minimap (small) & Crosshair")]
        public RawImage minimapImage;        // Placeholder – could be a RenderTexture
        public RectTransform playerMarker;   // Icon indicating player position on minimap
        public Image crosshairImage;         // Simple crosshair

        public static GameplayHUD Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            AutoFindSkillSlots();
            try
            {
                EnsureUIHierarchy();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameplayHUD] Error EnsureUIHierarchy: {ex}");
            }

            // Subscribe to core events
            EventBus.OnPlayerHpChanged += UpdateHp;
            EventBus.OnPlayerMpChanged += UpdateMp;
            EventBus.OnPlayerExpChanged += UpdateExp;
            EventBus.OnPlayerLevelUp   += UpdateLevel;
            EventBus.OnClassChanged    += UpdateClassName;
            EventBus.OnSkillCooldown   += UpdateSkillCooldown;
            EventBus.OnClassApplied    += HandleClassApplied;
            EventBus.OnSkillChanged    += HandleSkillsChanged;

            if (petWidget == null)
            {
                petWidget = GetComponentInChildren<PetHUDWidget>(true);
            }

            HookButtons();
        }

        public void ToggleCharacterUI()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;
            if (CharacterStatusUI.Instance == null)
            {
                CharacterStatusUI.EnsureUI(parentCanvas);
            }

            Debug.Log($"<color=cyan>[GameplayHUD] ToggleCharacterUI. Instance = {CharacterStatusUI.Instance != null}</color>");

            if (CharacterStatusUI.Instance != null)
            {
                CharacterStatusUI.Instance.Toggle();
            }
            else if (CharacterUI.Instance != null)
            {
                CharacterUI.Instance.Toggle();
            }
        }

        public void ToggleInventoryBagUI()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;
            if (InventoryBagUI.Instance == null)
            {
                InventoryBagUI.EnsureUI(parentCanvas);
            }

            Debug.Log($"<color=cyan>[GameplayHUD] ToggleInventoryBagUI. Instance = {InventoryBagUI.Instance != null}</color>");

            if (InventoryBagUI.Instance != null)
            {
                InventoryBagUI.Instance.Toggle();
            }
            else
            {
                var invUI = FindObjectOfType<InventoryUI>(true);
                if (invUI != null) invUI.gameObject.SetActive(!invUI.gameObject.activeSelf);
            }
        }

        public void ToggleClassDemoUI()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;
            if (ClassDemoSwitcherUI.Instance == null)
            {
                var switcher = parentCanvas.GetComponent<ClassDemoSwitcherUI>() ?? parentCanvas.GetComponentInChildren<ClassDemoSwitcherUI>(true);
                if (switcher == null) switcher = parentCanvas.gameObject.AddComponent<ClassDemoSwitcherUI>();
                switcher.EnsureUIHierarchy();
            }

            Debug.Log($"<color=cyan>[GameplayHUD] ToggleClassDemoUI. Instance = {ClassDemoSwitcherUI.Instance != null}</color>");

            if (ClassDemoSwitcherUI.Instance != null)
            {
                ClassDemoSwitcherUI.Instance.Toggle();
            }
        }

        public void TogglePauseMenuUI()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;
            if (PauseMenuUI.Instance == null)
            {
                var pause = parentCanvas.GetComponent<PauseMenuUI>() ?? parentCanvas.GetComponentInChildren<PauseMenuUI>(true);
                if (pause == null) pause = parentCanvas.gameObject.AddComponent<PauseMenuUI>();
            }

            Debug.Log($"<color=cyan>[GameplayHUD] TogglePauseMenuUI. Instance = {PauseMenuUI.Instance != null}</color>");

            if (PauseMenuUI.Instance != null)
            {
                PauseMenuUI.Instance.TogglePause();
            }
        }

        private void HookButtons()
        {
            Transform bl = transform.Find("BottomLeft_MenuCluster");
            if (bl != null)
            {
                if (demoClassButton == null) demoClassButton = bl.Find("Btn_DemoClass")?.GetComponent<Button>();
                if (characterButton == null) characterButton = bl.Find("Btn_Character")?.GetComponent<Button>();
                if (inventoryButton == null) inventoryButton = bl.Find("Btn_Inventory")?.GetComponent<Button>();
                if (menuButton == null) menuButton = bl.Find("Btn_Menu")?.GetComponent<Button>();
            }

            // Hook bottom-left navigation buttons
            if (characterButton != null)
            {
                characterButton.onClick.RemoveAllListeners();
                characterButton.onClick.AddListener(() => {
                    Debug.Log("<color=yellow>[GameplayHUD] Clicked Btn_Character</color>");
                    ToggleCharacterUI();
                });
            }

            if (inventoryButton != null)
            {
                inventoryButton.onClick.RemoveAllListeners();
                inventoryButton.onClick.AddListener(() => {
                    Debug.Log("<color=yellow>[GameplayHUD] Clicked Btn_Inventory</color>");
                    ToggleInventoryBagUI();
                });
            }

            if (equipmentButton != null)
            {
                equipmentButton.onClick.RemoveAllListeners();
                equipmentButton.onClick.AddListener(ToggleCharacterUI);
            }

            if (skillsButton != null)
            {
                skillsButton.onClick.RemoveAllListeners();
                skillsButton.onClick.AddListener(ToggleCharacterUI);
            }

            if (menuButton != null)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(() => {
                    Debug.Log("<color=yellow>[GameplayHUD] Clicked Btn_Menu</color>");
                    TogglePauseMenuUI();
                });
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(TogglePauseMenuUI);
            }

            if (demoClassButton != null)
            {
                demoClassButton.onClick.RemoveAllListeners();
                demoClassButton.onClick.AddListener(() => {
                    Debug.Log("<color=yellow>[GameplayHUD] Clicked Btn_DemoClass</color>");
                    ToggleClassDemoUI();
                });
            }

            // Quick Slots Hook
            if (hpPotionButton != null)
            {
                hpPotionButton.onClick.RemoveAllListeners();
                hpPotionButton.onClick.AddListener(() => UseHpPotion());
            }
            if (mpPotionButton != null)
            {
                mpPotionButton.onClick.RemoveAllListeners();
                mpPotionButton.onClick.AddListener(() => UseMpPotion());
            }
        }

        private void Start()
        {
            AutoFindSkillSlots();
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null)
            {
                UpdateHp(stats.currentHp, stats.maxHp);
                UpdateMp(stats.currentMp, stats.maxMp);
                UpdateExp(stats.currentExp, stats.maxExp);
                UpdateLevel(stats.level);
            }

            var classSys = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
            if (classSys != null && classSys.currentClass != null)
            {
                UpdateClassName(classSys.currentClass.className);
                if (avatarImage != null && classSys.currentClass.classIcon != null)
                    avatarImage.sprite = classSys.currentClass.classIcon;
                UpdateSkillBar(classSys.currentClass);
            }
            else
            {
                var novice = Resources.Load<ActionRPG.Classes.CharacterClassData>("Classes/Class_Novice");
#if UNITY_EDITOR
                if (novice == null)
                {
                    novice = UnityEditor.AssetDatabase.LoadAssetAtPath<ActionRPG.Classes.CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
                }
#endif
                if (novice != null)
                {
                    UpdateClassName(novice.className);
                    if (avatarImage != null && novice.classIcon != null)
                        avatarImage.sprite = novice.classIcon;
                    UpdateSkillBar(novice);
                }
            }

            EnsureQuickSlotsUI();
            HookButtons();

            foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
            {
                ActionRPG.UI.FontManager.ApplyFont(txt);
            }
        }

        private void Update()
        {
            // Phím tắt điều khiển các bảng UI chính: Đổi Class [K]/[F1], Nhân Vật [C]/[U], Túi Đồ [I]/[B], Cài Đặt [ESC]
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.U))
            {
                ToggleCharacterUI();
            }
            else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.B))
            {
                ToggleInventoryBagUI();
            }
            else if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.F1))
            {
                ToggleClassDemoUI();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseMenuUI();
            }

            // Phím tắt nhanh Quick Slot [R] (Bình Mana). Phím [E] được PlayerController điều phối ưu tiên NPC
            if (Input.GetKeyDown(KeyCode.R))
            {
                UseMpPotion();
            }

            // Đếm ngược thời gian hồi chiêu độc lập 8s cho bình thuốc
            if (HpPotionCooldownTimer > 0f)
            {
                HpPotionCooldownTimer = Mathf.Max(0f, HpPotionCooldownTimer - Time.deltaTime);
            }
            if (MpPotionCooldownTimer > 0f)
            {
                MpPotionCooldownTimer = Mathf.Max(0f, MpPotionCooldownTimer - Time.deltaTime);
            }

            UpdateQuickSlotsUI();
        }

        private void OnDestroy()
        {
            EventBus.OnPlayerHpChanged -= UpdateHp;
            EventBus.OnPlayerMpChanged -= UpdateMp;
            EventBus.OnPlayerExpChanged -= UpdateExp;
            EventBus.OnPlayerLevelUp   -= UpdateLevel;
            EventBus.OnClassChanged    -= UpdateClassName;
            EventBus.OnSkillCooldown   -= UpdateSkillCooldown;
            EventBus.OnClassApplied    -= HandleClassApplied;
            EventBus.OnSkillChanged    -= HandleSkillsChanged;
        }

        #region UI Update Callbacks
        private void UpdateHp(float current, float max)
        {
            if (hpBarFill != null) hpBarFill.fillAmount = max > 0 ? current / max : 0;
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            float shield = stats != null ? stats.temporaryShieldHp : 0f;

            UpdateVirtualShieldBar(shield, max);

            if (hpText != null)
            {
                if (shield > 0.01f)
                {
                    hpText.text = $"{Mathf.CeilToInt(current)} <color=#FFD93D>(+{Mathf.CeilToInt(shield)})</color> / {Mathf.CeilToInt(max)}";
                }
                else
                {
                    hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
                }
            }
        }

        /// <summary>
        /// Tạo và hiển thị lớp giáp ảo màu vàng kim (ShieldBarFill) đè trực tiếp lên thanh máu.
        /// </summary>
        public void UpdateVirtualShieldBar(float shieldHp, float maxHp)
        {
            EnsureShieldBarFill();
            if (shieldBarFill != null)
            {
                if (shieldHp > 0.01f && maxHp > 0f)
                {
                    shieldBarFill.enabled = true;
                    shieldBarFill.fillAmount = Mathf.Clamp01(shieldHp / maxHp);
                }
                else
                {
                    shieldBarFill.fillAmount = 0f;
                    shieldBarFill.enabled = false;
                }
            }
        }

        private void EnsureShieldBarFill()
        {
            if (shieldBarFill != null || hpBarFill == null || hpBarFill.transform.parent == null) return;
            Transform barParent = hpBarFill.transform.parent;
            RectTransform shieldRt = EnsureRectTransform(barParent, "Shield_Fill");
            shieldRt.anchorMin = Vector2.zero;
            shieldRt.anchorMax = Vector2.one;
            shieldRt.offsetMin = new Vector2(4, 2);
            shieldRt.offsetMax = new Vector2(-4, -2);

            shieldBarFill = shieldRt.GetComponent<Image>() ?? shieldRt.gameObject.AddComponent<Image>();
            shieldBarFill.sprite = hpBarFill.sprite;
            shieldBarFill.color = new Color(1f, 0.88f, 0.22f, 0.88f); // Màu vàng Kim Cang / Máu ảo
            shieldBarFill.type = Image.Type.Filled;
            shieldBarFill.fillMethod = Image.FillMethod.Horizontal;
            shieldBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            shieldBarFill.fillAmount = 0f;
            shieldBarFill.enabled = false;
            shieldBarFill.raycastTarget = false;

            // Đặt ngay trước HP_Text để không che chữ số máu
            if (hpText != null && hpText.transform.parent == barParent)
            {
                shieldRt.SetSiblingIndex(Mathf.Max(1, hpText.transform.GetSiblingIndex()));
            }
        }

        private void UpdateMp(float current, float max)
        {
            if (mpBarFill != null) mpBarFill.fillAmount = max > 0 ? current / max : 0;
            if (mpText != null) mpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void UpdateExp(int current, int max)
        {
            if (expBarFill != null) expBarFill.fillAmount = max > 0 ? (float)current / max : 0;
            if (expText != null) expText.text = $"{current}/{max}";
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null) levelText.text = $"Lv. {level}";
        }

        private void UpdateClassName(string className)
        {
            if (playerNameText != null) playerNameText.text = className;
            var cs = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
            if (cs != null && cs.currentClass != null)
            {
                if (avatarImage != null && cs.currentClass.classIcon != null)
                    avatarImage.sprite = cs.currentClass.classIcon;
                UpdateSkillBar(cs.currentClass);
            }
        }

        /// <summary>Làm mới toàn bộ tên class, avatar, level, 5 icon kỹ năng và hồi chiêu.</summary>
        public void RefreshClassUI(ActionRPG.Classes.CharacterClassData classData)
        {
            if (classData == null) return;
            if (playerNameText != null) playerNameText.text = classData.className;
            if (avatarImage != null && classData.classIcon != null) avatarImage.sprite = classData.classIcon;

            var st = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (st != null)
            {
                UpdateLevel(st.level);
                UpdateHp(st.currentHp, st.maxHp);
                UpdateMp(st.currentMp, st.maxMp);
            }
            UpdateSkillBar(classData);
        }

        private void HandleClassApplied(ActionRPG.Classes.CharacterClassData classData)
        {
            RefreshClassUI(classData);
        }

        private void HandleSkillsChanged(System.Collections.Generic.List<ActionRPG.Classes.SkillData> skills)
        {
            UpdateSkillBarWithSkills(skills);
        }

        public void UpdateSkillBar(ActionRPG.Classes.CharacterClassData classData)
        {
            AutoFindSkillSlots();
            if (skillSlots == null || skillSlots.Length == 0) return;
            if (classData == null) return;

            // Ưu tiên danh sách kỹ năng thực sự đang nạp trong PlayerSkillCombat2D để ActionBar == kỹ năng có thể tung
            System.Collections.Generic.List<ActionRPG.Classes.SkillData> skills = null;
            var combat = FindObjectOfType<ActionRPG.Combat.PlayerSkillCombat2D>();
            if (combat != null) skills = combat.GetActiveSkills();
            if (skills == null) skills = classData.unlockedSkills;
            if (skills == null) return;

            UpdateSkillBarWithSkills(skills);
        }

        public void UpdateSkillBarWithSkills(System.Collections.Generic.List<ActionRPG.Classes.SkillData> skills)
        {
            AutoFindSkillSlots();
            if (skillSlots == null || skillSlots.Length == 0) return;
            skillMaxCooldownMap.Clear();
            string[] defaultKeys = { "1", "2", "3", "4", "5", "6" };
            for (int i = 0; i < skillSlots.Length; i++)
            {
                var slot = skillSlots[i];
                if (slot == null) continue;
                slot.EnsureReferences();

                string key = i < defaultKeys.Length ? defaultKeys[i] : (i + 1).ToString();
                ActionRPG.Classes.SkillData skill = (skills != null && i < skills.Count) ? skills[i] : null;

                if (skill != null)
                {
                    slot.gameObject.SetActive(true);
                    Sprite spr = skill.skillIcon != null ? skill.skillIcon : skill.icon;
                    slot.Initialise(skill.skillId, spr, (int)skill.manaCost, key);
                    if (slot.iconImage != null)
                    {
                        slot.iconImage.sprite = spr;
                        slot.iconImage.color = Color.white;
                        slot.iconImage.enabled = (spr != null);
                    }
                    slot.SetCooldown(0f, 0f);
                }
                else
                {
                    // Ô trống / chưa mở khóa kỹ năng: ẩn icon & mana, hiển thị slot trống với số phím làm mờ
                    slot.SkillId = string.Empty;
                    if (slot.iconImage != null)
                    {
                        slot.iconImage.sprite = null;
                        slot.iconImage.enabled = false;
                    }
                    if (slot.shortcutKeyText != null)
                    {
                        slot.shortcutKeyText.text = key;
                        slot.shortcutKeyText.color = new Color(0.6f, 0.6f, 0.6f, 0.45f);
                    }
                    if (slot.manaCostText != null) slot.manaCostText.text = string.Empty;
                    slot.SetCooldown(0f, 0f);
                }
            }
        }

        private static readonly System.Collections.Generic.Dictionary<string, float> skillMaxCooldownMap = new System.Collections.Generic.Dictionary<string, float>();

        private void UpdateSkillCooldown(string skillId, float remaining)
        {
            if (!skillMaxCooldownMap.ContainsKey(skillId) || remaining > skillMaxCooldownMap[skillId])
            {
                skillMaxCooldownMap[skillId] = remaining > 0 ? remaining : 1f;
            }

            float maxCd = skillMaxCooldownMap[skillId];
            float norm = maxCd > 0 ? remaining / maxCd : 0f;

            foreach (var slot in skillSlots)
            {
                if (slot != null && slot.SkillId == skillId)
                {
                    slot.SetCooldown(norm, remaining);
                    break;
                }
            }
        }
        #endregion

        #region Quick Slots Management (Sprint 3)
        public static void ResetPotionCooldowns()
        {
            HpPotionCooldownTimer = 0f;
            MpPotionCooldownTimer = 0f;
        }

        public static bool UseHpPotion()
        {
            if (HpPotionCooldownTimer > 0f)
            {
                string msg = $"Bình Máu đang trong thời gian hồi ({HpPotionCooldownTimer:F1}s)!";
                EventBus.OnShowUIMessage?.Invoke(msg);
                EventBus.OnPlaySound?.Invoke("error");
                return false;
            }

            var inv = Inventory.Instance != null ? Inventory.Instance : UnityEngine.Object.FindAnyObjectByType<Inventory>();
            if (inv == null) return false;

            InventorySlot foundSlot = inv.slots.Find(s => s != null && s.item is ConsumableItemData c && (c.isHpPotion || c.healHpAmount > 0));
            if (foundSlot == null || foundSlot.quantity <= 0)
            {
                EventBus.OnShowUIMessage?.Invoke("Hết Bình Máu trong túi đồ!");
                EventBus.OnPlaySound?.Invoke("error");
                return false;
            }

            var potion = foundSlot.item as ConsumableItemData;
            var playerStats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : UnityEngine.Object.FindAnyObjectByType<CharacterStats>();
            if (playerStats == null) return false;

            playerStats.HealHp(potion.healHpAmount);
            inv.RemoveItem(potion, 1);
            HpPotionCooldownTimer = potion.cooldownTime > 0 ? potion.cooldownTime : DefaultPotionCooldown;

            string successMsg = $"Đã dùng {potion.itemName}! Hồi +{potion.healHpAmount} HP.";
            Debug.Log($"<color=green>[QuickSlot E] {successMsg}</color>");
            EventBus.OnShowUIMessage?.Invoke(successMsg);
            EventBus.OnPlaySound?.Invoke("potion");
            return true;
        }

        public static bool UseMpPotion()
        {
            if (MpPotionCooldownTimer > 0f)
            {
                string msg = $"Bình Mana đang trong thời gian hồi ({MpPotionCooldownTimer:F1}s)!";
                EventBus.OnShowUIMessage?.Invoke(msg);
                EventBus.OnPlaySound?.Invoke("error");
                return false;
            }

            var inv = Inventory.Instance != null ? Inventory.Instance : UnityEngine.Object.FindAnyObjectByType<Inventory>();
            if (inv == null) return false;

            InventorySlot foundSlot = inv.slots.Find(s => s != null && s.item is ConsumableItemData c && (c.isMpPotion || c.restoreMpAmount > 0));
            if (foundSlot == null || foundSlot.quantity <= 0)
            {
                EventBus.OnShowUIMessage?.Invoke("Hết Bình Mana trong túi đồ!");
                EventBus.OnPlaySound?.Invoke("error");
                return false;
            }

            var potion = foundSlot.item as ConsumableItemData;
            var playerStats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : UnityEngine.Object.FindAnyObjectByType<CharacterStats>();
            if (playerStats == null) return false;

            playerStats.RestoreMp(potion.restoreMpAmount);
            Inventory.Instance.RemoveItem(potion, 1);
            MpPotionCooldownTimer = potion.cooldownTime > 0 ? potion.cooldownTime : DefaultPotionCooldown;

            string successMsg = $"Đã dùng {potion.itemName}! Hồi +{potion.restoreMpAmount} MP.";
            Debug.Log($"<color=cyan>[QuickSlot R] {successMsg}</color>");
            EventBus.OnShowUIMessage?.Invoke(successMsg);
            EventBus.OnPlaySound?.Invoke("potion");
            return true;
        }

        public void UpdateQuickSlotsUI()
        {
            int hpCount = 0;
            int mpCount = 0;
            if (Inventory.Instance != null && Inventory.Instance.slots != null)
            {
                foreach (var s in Inventory.Instance.slots)
                {
                    if (s != null && s.item is ConsumableItemData c)
                    {
                        if (c.isHpPotion || c.healHpAmount > 0) hpCount += s.quantity;
                        if (c.isMpPotion || c.restoreMpAmount > 0) mpCount += s.quantity;
                    }
                }
            }

            if (hpPotionCountText != null)
                hpPotionCountText.text = hpCount > 0 ? $"{hpCount}" : "0";

            if (mpPotionCountText != null)
                mpPotionCountText.text = mpCount > 0 ? $"{mpCount}" : "0";

            if (hpPotionCooldownOverlay != null)
            {
                hpPotionCooldownOverlay.fillAmount = Mathf.Clamp01(HpPotionCooldownTimer / DefaultPotionCooldown);
                hpPotionCooldownOverlay.enabled = HpPotionCooldownTimer > 0;
            }

            if (mpPotionCooldownOverlay != null)
            {
                mpPotionCooldownOverlay.fillAmount = Mathf.Clamp01(MpPotionCooldownTimer / DefaultPotionCooldown);
                mpPotionCooldownOverlay.enabled = MpPotionCooldownTimer > 0;
            }
        }

        private static void CleanNonRectTransformChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child != null && !(child is RectTransform))
                {
                    child.name += "_stale_destroying";
                    child.SetParent(null);
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
        }

        private static RectTransform EnsureRectTransform(Transform parent, string name)
        {
            if (parent != null)
            {
                Transform existing = parent.Find(name);
                if (existing != null)
                {
                    if (existing is RectTransform rt && rt != null)
                        return rt;

                    existing.name = name + "_stale_destroying";
                    existing.SetParent(null);
                    if (Application.isPlaying)
                        Destroy(existing.gameObject);
                    else
                        DestroyImmediate(existing.gameObject);
                }
            }

            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private void EnsureQuickSlotsUI()
        {
            if (quickSlotsContainer != null)
            {
                var qRt = quickSlotsContainer.GetComponent<RectTransform>();
                if (qRt != null)
                {
                    qRt.anchorMin = new Vector2(0.5f, 0f);
                    qRt.anchorMax = new Vector2(0.5f, 0f);
                    qRt.pivot = new Vector2(0.5f, 0f);
                    qRt.anchoredPosition = new Vector2(0f, 16f);
                    qRt.sizeDelta = new Vector2(120f, 56f);
                }
                var qLayout = quickSlotsContainer.GetComponent<HorizontalLayoutGroup>();
                if (qLayout != null) qLayout.spacing = 8f;
                return;
            }

            RectTransform rect = EnsureRectTransform(transform, "QuickSlots_Container");
            quickSlotsContainer = rect.gameObject;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 16f);
            rect.sizeDelta = new Vector2(120f, 56f);

            var layout = rect.GetComponent<HorizontalLayoutGroup>() ?? rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Slot HP [E]
            hpPotionButton = CreateQuickSlotButton(rect, Vector2.zero, "E", "icon_potion_hp.png", new Color(0.95f, 0.25f, 0.25f), out hpPotionIcon, out hpPotionCooldownOverlay, out hpPotionCountText);
            hpPotionButton.onClick.AddListener(() => UseHpPotion());

            // Slot MP [R]
            mpPotionButton = CreateQuickSlotButton(rect, Vector2.zero, "R", "icon_potion_mp.png", new Color(0.25f, 0.6f, 1.0f), out mpPotionIcon, out mpPotionCooldownOverlay, out mpPotionCountText);
            mpPotionButton.onClick.AddListener(() => UseMpPotion());
        }

        private Button CreateQuickSlotButton(Transform parent, Vector2 pos, string keyLabel, string iconName, Color tint, out Image icon, out Image overlay, out TMP_Text countText)
        {
            RectTransform rt = EnsureRectTransform(parent, $"Slot_{keyLabel}");
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(56f, 56f);

            Image bg = rt.GetComponent<Image>() ?? rt.gameObject.AddComponent<Image>();
            bg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;
#if UNITY_EDITOR
            Sprite slotSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_skill_slot.png");
            if (slotSprite != null) { bg.sprite = slotSprite; bg.type = Image.Type.Sliced; }
#endif

            Button btn = rt.GetComponent<Button>() ?? rt.gameObject.AddComponent<Button>();

            // Icon
            RectTransform iconRt = EnsureRectTransform(rt, "Icon");
            icon = iconRt.GetComponent<Image>() ?? iconRt.gameObject.AddComponent<Image>();
            icon.color = Color.white;
            icon.preserveAspect = true;
#if UNITY_EDITOR
            Sprite potionSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/HUD/Icons/{iconName}");
            if (potionSprite != null) icon.sprite = potionSprite;
#endif
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(7, 7);
            iconRt.offsetMax = new Vector2(-7, -7);

            // Overlay cooldown
            RectTransform overRt = EnsureRectTransform(rt, "CooldownOverlay");
            overlay = overRt.GetComponent<Image>() ?? overRt.gameObject.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.75f);
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillAmount = 0f;
            overlay.enabled = false;
            overRt.anchorMin = Vector2.zero;
            overRt.anchorMax = Vector2.one;
            overRt.offsetMin = new Vector2(5, 5);
            overRt.offsetMax = new Vector2(-5, -5);

            // Key Hint
            RectTransform keyRt = EnsureRectTransform(rt, "KeyText");
            var keyText = keyRt.GetComponent<TextMeshProUGUI>() ?? keyRt.gameObject.AddComponent<TextMeshProUGUI>();
            keyText.text = $"[{keyLabel}]";
            keyText.fontSize = 11;
            keyText.fontStyle = FontStyles.Bold;
            keyText.alignment = TextAlignmentOptions.TopLeft;
            keyText.color = new Color(1f, 0.88f, 0.35f);
            keyRt.anchorMin = Vector2.zero;
            keyRt.anchorMax = Vector2.one;
            keyRt.offsetMin = new Vector2(5, 5);
            keyRt.offsetMax = new Vector2(-5, -5);

            // Count Text
            RectTransform countRt = EnsureRectTransform(rt, "CountText");
            countText = countRt.GetComponent<TextMeshProUGUI>() ?? countRt.gameObject.AddComponent<TextMeshProUGUI>();
            countText.text = "0";
            countText.fontSize = 12;
            countText.fontStyle = FontStyles.Bold;
            countText.alignment = TextAlignmentOptions.BottomRight;
            countText.color = Color.white;
            countRt.anchorMin = Vector2.zero;
            countRt.anchorMax = Vector2.one;
            countRt.offsetMin = new Vector2(4, 4);
            countRt.offsetMax = new Vector2(-6, -4);

            return btn;
        }
        #endregion

        #region UI Hierarchy Auto-Construction
        public void EnsureUIHierarchy()
        {
            RectTransform hudRt = transform as RectTransform;
            if (hudRt != null)
            {
                hudRt.anchorMin = Vector2.zero;
                hudRt.anchorMax = Vector2.one;
                hudRt.offsetMin = Vector2.zero;
                hudRt.offsetMax = Vector2.zero;
                hudRt.localScale = Vector3.one;
            }

            // 1. Góc Trên - Trái (Top-Left): Cụm Avatar + Thanh HP, MP, EXP và Cấp độ. Cách lề trên và lề trái 16px.
            RectTransform existingTl = transform.Find("TopLeft_PlayerInfo") as RectTransform;
            if (existingTl != null)
            {
                existingTl.anchorMin = new Vector2(0f, 1f);
                existingTl.anchorMax = new Vector2(0f, 1f);
                existingTl.pivot = new Vector2(0f, 1f);
                existingTl.anchoredPosition = new Vector2(16f, -16f);
                existingTl.sizeDelta = new Vector2(350f, 108f);
            }

            if (hpBarFill == null)
            {
                RectTransform tlRt = EnsureRectTransform(transform, "TopLeft_PlayerInfo");
                tlRt.anchorMin = new Vector2(0f, 1f);
                tlRt.anchorMax = new Vector2(0f, 1f);
                tlRt.pivot = new Vector2(0f, 1f);
                tlRt.anchoredPosition = new Vector2(16f, -16f);
                tlRt.sizeDelta = new Vector2(350f, 108f);

                var bg = tlRt.GetComponent<Image>() ?? tlRt.gameObject.AddComponent<Image>();
                bg.sprite = FontManager.GetOrCreatePanelNineSliceSprite();
                bg.type = Image.Type.Sliced;
                bg.color = FontManager.PanelBgColor;
                var outline = tlRt.GetComponent<Outline>() ?? tlRt.gameObject.AddComponent<Outline>();
                outline.effectColor = FontManager.BronzeGoldBorder;
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                // Portrait Frame & Avatar
                RectTransform portraitRt = EnsureRectTransform(tlRt, "Portrait_Frame");
                portraitRt.anchorMin = new Vector2(0f, 1f);
                portraitRt.anchorMax = new Vector2(0f, 1f);
                portraitRt.pivot = new Vector2(0f, 1f);
                portraitRt.anchoredPosition = new Vector2(10, -10);
                portraitRt.sizeDelta = new Vector2(72, 72);
                var pImg = portraitRt.GetComponent<Image>() ?? portraitRt.gameObject.AddComponent<Image>();
                pImg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
                pImg.type = Image.Type.Sliced;
                pImg.color = Color.white;
#if UNITY_EDITOR
                Sprite portraitFrameSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_portrait.png");
                if (portraitFrameSprite != null) { pImg.sprite = portraitFrameSprite; pImg.type = Image.Type.Sliced; }
#endif

                // Avatar Image inside Portrait
                RectTransform avRt = EnsureRectTransform(portraitRt, "Avatar_Image");
                avRt.anchorMin = Vector2.zero;
                avRt.anchorMax = Vector2.one;
                avRt.offsetMin = new Vector2(8, 8);
                avRt.offsetMax = new Vector2(-8, -8);
                avatarImage = avRt.GetComponent<Image>() ?? avRt.gameObject.AddComponent<Image>();
                avatarImage.color = Color.white;
                avatarImage.preserveAspect = true;
#if UNITY_EDITOR
                Sprite knightIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Icons/icon_character.png");
                if (knightIcon != null) avatarImage.sprite = knightIcon;
#endif

                if (playerNameText == null)
                {
                    playerNameText = CreateTMPText("PlayerName", tlRt, "Hiệp Sĩ Lumina", 16, TextAlignmentOptions.Left);
                    playerNameText.rectTransform.anchoredPosition = new Vector2(90, -12);
                    playerNameText.color = FontManager.IvoryTextColor;
                    playerNameText.fontStyle = FontStyles.Bold;
                }

                if (levelText == null)
                {
                    levelText = CreateTMPText("LevelText", tlRt, "Lv. 1", 15, TextAlignmentOptions.Right);
                    levelText.rectTransform.anchoredPosition = new Vector2(280, -12);
                    levelText.color = FontManager.GoldTitleColor;
                    levelText.fontStyle = FontStyles.Bold;
                }

                if (hpBarFill == null)
                {
                    hpBarFill = CreateBar("HP_Bar", tlRt, new Vector2(90, -36), "fill_hp_bar.png", new Color(0.95f, 0.25f, 0.28f), 246, 20);
                    hpText = CreateTMPText("HP_Text", hpBarFill.transform.parent, "100 / 100", 12, TextAlignmentOptions.Center);
                    hpText.rectTransform.anchoredPosition = new Vector2(123, 0);
                    hpText.fontStyle = FontStyles.Bold;
                }
                EnsureShieldBarFill();

                if (mpBarFill == null)
                {
                    mpBarFill = CreateBar("MP_Bar", tlRt, new Vector2(90, -60), "fill_mp_bar.png", new Color(0.2f, 0.6f, 1f), 246, 18);
                    mpText = CreateTMPText("MP_Text", mpBarFill.transform.parent, "50 / 50", 12, TextAlignmentOptions.Center);
                    mpText.rectTransform.anchoredPosition = new Vector2(123, 0);
                    mpText.fontStyle = FontStyles.Bold;
                }

                if (expBarFill == null)
                {
                    expBarFill = CreateBar("EXP_Bar", tlRt, new Vector2(90, -82), "fill_exp_bar.png", new Color(0.3f, 0.9f, 0.45f), 246, 14);
                    expText = CreateTMPText("EXP_Text", expBarFill.transform.parent, "0 / 100", 11, TextAlignmentOptions.Center);
                    expText.rectTransform.anchoredPosition = new Vector2(123, 0);
                    expText.fontStyle = FontStyles.Bold;
                }
            }

            // 2. Góc Dưới - Phải (Bottom-Right): Cụm 6 ô kỹ năng chiến đấu (kích thước vuông bằng nhau 56x56 px, cách lề 16px)
            AutoFindSkillSlots();
            RectTransform existingBr = transform.Find("BottomRight_SkillBar") as RectTransform;
            if (existingBr != null)
            {
                existingBr.anchorMin = new Vector2(1f, 0f);
                existingBr.anchorMax = new Vector2(1f, 0f);
                existingBr.pivot = new Vector2(1f, 0f);
                existingBr.anchoredPosition = new Vector2(-16f, 16f);
                existingBr.sizeDelta = new Vector2(376f, 56f);
                var brLayout = existingBr.GetComponent<HorizontalLayoutGroup>();
                if (brLayout != null) brLayout.spacing = 8f;
            }

            if (skillSlots == null || skillSlots.Length < 6)
            {
                RectTransform brRt = EnsureRectTransform(transform, "BottomRight_SkillBar");
                CleanNonRectTransformChildren(brRt);
                brRt.anchorMin = new Vector2(1f, 0f);
                brRt.anchorMax = new Vector2(1f, 0f);
                brRt.pivot = new Vector2(1f, 0f);
                brRt.anchoredPosition = new Vector2(-16f, 16f);
                brRt.sizeDelta = new Vector2(376f, 56f);

                // Dọn dẹp triệt để các ô kỹ năng cũ nếu trước đó chỉ có 5 slot
                for (int c = brRt.childCount - 1; c >= 0; c--)
                {
                    Transform child = brRt.GetChild(c);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                var layout = brRt.GetComponent<HorizontalLayoutGroup>() ?? brRt.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 8;
                layout.childAlignment = TextAnchor.MiddleRight;
                layout.childControlWidth = false;
                layout.childControlHeight = false;

                var slots = new System.Collections.Generic.List<SkillSlotUI>();
                string[] hotkeys = { "1", "2", "3", "4", "5", "6" };
                for (int i = 0; i < hotkeys.Length; i++)
                {
                    string slotName = $"SkillSlot_{hotkeys[i]}";
                    RectTransform sRt = EnsureRectTransform(brRt, slotName);
                    CleanNonRectTransformChildren(sRt);
                    sRt.sizeDelta = new Vector2(56, 56);

                    var sImg = sRt.GetComponent<Image>() ?? sRt.gameObject.AddComponent<Image>();
                    sImg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
                    sImg.type = Image.Type.Sliced;
                    sImg.color = Color.white;
#if UNITY_EDITOR
                    Sprite slotSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_skill_slot.png");
                    if (slotSprite != null) { sImg.sprite = slotSprite; sImg.type = Image.Type.Sliced; }
#endif

                    var slotUI = sRt.GetComponent<SkillSlotUI>() ?? sRt.gameObject.AddComponent<SkillSlotUI>();

                    // Icon
                    RectTransform iconRt = EnsureRectTransform(sRt, "Icon");
                    iconRt.anchorMin = Vector2.zero;
                    iconRt.anchorMax = Vector2.one;
                    iconRt.offsetMin = new Vector2(6, 6);
                    iconRt.offsetMax = new Vector2(-6, -6);
                    var iconImg = iconRt.GetComponent<Image>() ?? iconRt.gameObject.AddComponent<Image>();
                    iconImg.color = Color.white;
                    iconImg.preserveAspect = true;
                    slotUI.iconImage = iconImg;

                    // Cooldown overlay
                    RectTransform cdRt = EnsureRectTransform(sRt, "CooldownOverlay");
                    cdRt.anchorMin = Vector2.zero;
                    cdRt.anchorMax = Vector2.one;
                    cdRt.offsetMin = new Vector2(4, 4);
                    cdRt.offsetMax = new Vector2(-4, -4);
                    var cdImg = cdRt.GetComponent<Image>() ?? cdRt.gameObject.AddComponent<Image>();
                    cdImg.color = new Color(0f, 0f, 0f, 0.75f);
                    cdImg.type = Image.Type.Filled;
                    cdImg.fillMethod = Image.FillMethod.Radial360;
                    cdImg.fillClockwise = false;
                    cdImg.fillAmount = 0f;
                    slotUI.cooldownOverlay = cdImg;

                    // Cooldown countdown text
                    RectTransform cdTextRt = EnsureRectTransform(sRt, "CDText");
                    var cdTxt = cdTextRt.GetComponent<TMP_Text>() ?? cdTextRt.gameObject.AddComponent<TextMeshProUGUI>();
                    cdTxt.fontSize = 16;
                    cdTxt.alignment = TextAlignmentOptions.Center;
                    cdTxt.rectTransform.anchorMin = Vector2.zero;
                    cdTxt.rectTransform.anchorMax = Vector2.one;
                    cdTxt.rectTransform.offsetMin = cdTxt.rectTransform.offsetMax = Vector2.zero;
                    cdTxt.color = new Color(1f, 0.95f, 0.6f);
                    cdTxt.fontStyle = FontStyles.Bold;
                    cdTxt.enabled = false;
                    slotUI.cdText = cdTxt;

                    // Hotkey label (Số 1..6)
                    RectTransform keyRt = EnsureRectTransform(sRt, "Key");
                    var keyTxt = keyRt.GetComponent<TMP_Text>() ?? keyRt.gameObject.AddComponent<TextMeshProUGUI>();
                    keyTxt.text = hotkeys[i];
                    keyTxt.fontSize = 12;
                    keyTxt.alignment = TextAlignmentOptions.TopLeft;
                    keyTxt.rectTransform.anchorMin = Vector2.zero;
                    keyTxt.rectTransform.anchorMax = Vector2.one;
                    keyTxt.rectTransform.offsetMin = new Vector2(5, 5);
                    keyTxt.rectTransform.offsetMax = new Vector2(-5, -5);
                    keyTxt.color = new Color(1f, 0.9f, 0.4f);
                    keyTxt.fontStyle = FontStyles.Bold;
                    slotUI.shortcutKeyText = keyTxt;

                    // Mana cost text badge
                    RectTransform manaRt = EnsureRectTransform(sRt, "ManaCost");
                    var manaTxt = manaRt.GetComponent<TMP_Text>() ?? manaRt.gameObject.AddComponent<TextMeshProUGUI>();
                    manaTxt.text = "";
                    manaTxt.fontSize = 11;
                    manaTxt.alignment = TextAlignmentOptions.BottomRight;
                    manaTxt.rectTransform.anchorMin = Vector2.zero;
                    manaTxt.rectTransform.anchorMax = Vector2.one;
                    manaTxt.rectTransform.offsetMin = new Vector2(4, 4);
                    manaTxt.rectTransform.offsetMax = new Vector2(-5, -4);
                    manaTxt.color = new Color(0.45f, 0.9f, 1f);
                    manaTxt.fontStyle = FontStyles.Bold;
                    slotUI.manaCostText = manaTxt;

                    slots.Add(slotUI);
                }
                skillSlots = slots.ToArray();
            }
            else
            {
                // Đồng bộ kích thước 6 ô kỹ năng hiện có về đúng 56x56 px (bằng với 2 ô bình thuốc)
                for (int i = 0; i < skillSlots.Length; i++)
                {
                    if (skillSlots[i] != null)
                    {
                        var sRt = skillSlots[i].GetComponent<RectTransform>();
                        if (sRt != null) sRt.sizeDelta = new Vector2(56f, 56f);
                        if (skillSlots[i].iconImage != null) skillSlots[i].iconImage.preserveAspect = true;
                    }
                }
            }
            AutoFindSkillSlots();

            // 3. Góc Dưới - Trái (Bottom-Left): Cụm 4 nút menu (Đổi Class [K], Nhân Vật [C], Túi Đồ [I], Cài Đặt [ESC])
            // Dùng HorizontalLayoutGroup, Spacing = 8px, kích thước nút đồng nhất (126 x 38 px), cách lề 16px.
            RectTransform blRt = EnsureRectTransform(transform, "BottomLeft_MenuCluster");
            blRt.anchorMin = new Vector2(0f, 0f);
            blRt.anchorMax = new Vector2(0f, 0f);
            blRt.pivot = new Vector2(0f, 0f);
            blRt.anchoredPosition = new Vector2(16f, 16f);
            blRt.sizeDelta = new Vector2(528f, 38f);

            var blLayout = blRt.GetComponent<HorizontalLayoutGroup>() ?? blRt.gameObject.AddComponent<HorizontalLayoutGroup>();
            blLayout.spacing = 8;
            blLayout.childAlignment = TextAnchor.MiddleLeft;
            blLayout.childControlWidth = false;
            blLayout.childControlHeight = false;

            if (demoClassButton == null)
                demoClassButton = CreateMenuButton("Btn_DemoClass", blRt, "Đổi Class [K]", "icon_demo_class.png", new Color(0.85f, 0.55f, 0.1f, 0.95f), new Color(1f, 0.85f, 0.3f, 0.9f), 126f);
            else
                SyncMenuButton(demoClassButton, "Đổi Class [K]", 126f);

            if (characterButton == null)
                characterButton = CreateMenuButton("Btn_Character", blRt, "Nhân Vật [C]", "icon_character.png", new Color(0.15f, 0.2f, 0.35f, 0.95f), new Color(0.3f, 0.7f, 1f, 0.6f), 126f);
            else
                SyncMenuButton(characterButton, "Nhân Vật [C]", 126f);

            if (inventoryButton == null)
                inventoryButton = CreateMenuButton("Btn_Inventory", blRt, "Túi Đồ [I]", "icon_backpack.png", new Color(0.12f, 0.25f, 0.18f, 0.95f), new Color(0.3f, 0.9f, 0.5f, 0.6f), 126f);
            else
                SyncMenuButton(inventoryButton, "Túi Đồ [I]", 126f);

            if (menuButton == null)
                menuButton = CreateMenuButton("Btn_Menu", blRt, "Cài Đặt [ESC]", "icon_settings.png", new Color(0.2f, 0.2f, 0.25f, 0.95f), new Color(0.6f, 0.6f, 0.7f, 0.6f), 126f);
            else
                SyncMenuButton(menuButton, "Cài Đặt [ESC]", 126f);

            // 4. Quick Slots (Góc Dưới - Giữa)
            EnsureQuickSlotsUI();

            // 5. Pet Companion HUD Widget (Đặt cạnh phải TopLeft_PlayerInfo để không đè lên Bảng Chỉ Số Nhân Vật)
            EnsurePetWidgetUI();

            // 6. Bảng Chỉ Số [C/U], Balo Túi Đồ [I] và Minimap 2D
            EnsureStatsInventoryMinimapUI();

            // Làm tròn tọa độ Pixel-Perfect cho toàn bộ HUD
            FontManager.SanitizeUIHierarchy(transform);

            // Hook lại các button click listeners
            HookButtons();
        }

        private static void SyncMenuButton(Button btn, string label, float width)
        {
            if (btn == null) return;
            var rt = btn.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(width, 38f);
            var txt = btn.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
            {
                txt.text = label;
                txt.fontSize = 12;
                FontManager.ApplyFont(txt);
            }
        }

        private void EnsureStatsInventoryMinimapUI()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;
            try { CharacterStatusUI.EnsureUI(parentCanvas); } catch (System.Exception ex) { Debug.LogError($"[GameplayHUD] Error EnsureUI CharacterStatusUI: {ex}"); }
            try { InventoryBagUI.EnsureUI(parentCanvas); } catch (System.Exception ex) { Debug.LogError($"[GameplayHUD] Error EnsureUI InventoryBagUI: {ex}"); }
            try { MinimapSystem2D.EnsureUI(parentCanvas); } catch (System.Exception ex) { Debug.LogError($"[GameplayHUD] Error EnsureUI MinimapSystem2D: {ex}"); }
        }

        private void EnsurePetWidgetUI()
        {
            if (petWidget == null)
            {
                petWidget = GetComponentInChildren<PetHUDWidget>(true);
            }

            if (petWidget != null)
            {
                var existingPetRt = petWidget.GetComponent<RectTransform>();
                if (existingPetRt != null)
                {
                    existingPetRt.anchorMin = new Vector2(0f, 1f);
                    existingPetRt.anchorMax = new Vector2(0f, 1f);
                    existingPetRt.pivot = new Vector2(0f, 1f);
                    existingPetRt.anchoredPosition = new Vector2(374f, -16f);
                }
                return;
            }

            // Đặt Panel Pet Companion Widget bên phải TopLeft_PlayerInfo (X = 374, Y = -16) để nhường trọn vùng Middle-Left cho Bảng Chỉ Số
            RectTransform petRt = EnsureRectTransform(transform, "TopLeft_PetCompanionWidget");
            petRt.anchorMin = new Vector2(0f, 1f);
            petRt.anchorMax = new Vector2(0f, 1f);
            petRt.pivot = new Vector2(0f, 1f);
            petRt.anchoredPosition = new Vector2(374f, -16f);
            petRt.sizeDelta = new Vector2(340, 78);

            var bg = petRt.GetComponent<Image>() ?? petRt.gameObject.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);
            var outline = petRt.GetComponent<Outline>() ?? petRt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.7f, 0.9f, 0.6f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            petWidget = petRt.GetComponent<PetHUDWidget>() ?? petRt.gameObject.AddComponent<PetHUDWidget>();
            petWidget.widgetRoot = petRt.gameObject;

            // Frame Avatar Pet
            RectTransform pPortRt = EnsureRectTransform(petRt, "Pet_Avatar_Frame");
            pPortRt.anchorMin = new Vector2(0f, 0.5f);
            pPortRt.anchorMax = new Vector2(0f, 0.5f);
            pPortRt.pivot = new Vector2(0f, 0.5f);
            pPortRt.anchoredPosition = new Vector2(8, 0);
            pPortRt.sizeDelta = new Vector2(60, 60);

            var pPortImg = pPortRt.GetComponent<Image>() ?? pPortRt.gameObject.AddComponent<Image>();
            pPortImg.color = Color.white;
#if UNITY_EDITOR
            Sprite portraitFrame = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_portrait.png");
            if (portraitFrame != null) { pPortImg.sprite = portraitFrame; pPortImg.type = Image.Type.Sliced; }
#endif

            // Pet Avatar Image
            RectTransform avRt = EnsureRectTransform(pPortRt, "Avatar_Image");
            avRt.anchorMin = Vector2.zero;
            avRt.anchorMax = Vector2.one;
            avRt.offsetMin = new Vector2(6, 6);
            avRt.offsetMax = new Vector2(-6, -6);
            petWidget.avatarImage = avRt.GetComponent<Image>() ?? avRt.gameObject.AddComponent<Image>();
            petWidget.avatarImage.color = Color.white;
#if UNITY_EDITOR
            Sprite wolfIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Icons/icon_pet_wolf.png");
            if (wolfIcon != null) petWidget.avatarImage.sprite = wolfIcon;
#endif

            // Pet Name & Level
            petWidget.petNameText = CreateTMPText("PetName", petRt, "Sói Tinh Linh", 13, TextAlignmentOptions.Left);
            petWidget.petNameText.rectTransform.anchoredPosition = new Vector2(76, 22);
            petWidget.petNameText.color = new Color(0.6f, 0.9f, 1f);
            petWidget.petNameText.fontStyle = FontStyles.Bold;

            petWidget.petLevelText = CreateTMPText("PetLevel", petRt, "Lv. 1", 12, TextAlignmentOptions.Left);
            petWidget.petLevelText.rectTransform.anchoredPosition = new Vector2(210, 22);
            petWidget.petLevelText.color = new Color(1f, 0.85f, 0.3f);
            petWidget.petLevelText.fontStyle = FontStyles.Bold;

            petWidget.elementNameText = CreateTMPText("PetElement", petRt, "[Hỏa]", 11, TextAlignmentOptions.Right);
            petWidget.elementNameText.rectTransform.anchoredPosition = new Vector2(275, 22);
            petWidget.elementNameText.color = new Color(1f, 0.45f, 0.2f);
            petWidget.elementNameText.fontStyle = FontStyles.Bold;

            // HP Bar
            petWidget.hpBarFill = CreateBar("Pet_HP_Bar", petRt, new Vector2(76, 2), "bar_fill_hp.png", new Color(0.9f, 0.2f, 0.2f), 245, 14);
            petWidget.hpText = CreateTMPText("HP_Text", petWidget.hpBarFill.transform.parent, "120 / 120", 9, TextAlignmentOptions.Center);
            petWidget.hpText.rectTransform.anchoredPosition = new Vector2(122, 0);

            // Bond Bar
            petWidget.bondBarFill = CreateBar("Pet_Bond_Bar", petRt, new Vector2(76, -18), "bar_fill_mp.png", new Color(1f, 0.5f, 0.8f), 245, 12);
            petWidget.bondText = CreateTMPText("Bond_Text", petWidget.bondBarFill.transform.parent, "Thân Mật: 50/100", 9, TextAlignmentOptions.Center);
            petWidget.bondText.rectTransform.anchoredPosition = new Vector2(122, 0);

            // Hint Text [P]
            petWidget.hotkeyHintText = CreateTMPText("HotkeyHint", petRt, "[P] Gọi / Thu Hồi", 10, TextAlignmentOptions.Right);
            petWidget.hotkeyHintText.rectTransform.anchoredPosition = new Vector2(325, -28);
            petWidget.hotkeyHintText.color = new Color(0.7f, 0.85f, 1f, 0.85f);
        }

        private static Image CreateBar(string name, Transform parent, Vector2 pos, string fillSpriteName, Color fallbackFillColor, float width = 265, float height = 18)
        {
            RectTransform rt = EnsureRectTransform(parent, name);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, height);

            var bg = rt.GetComponent<Image>() ?? rt.gameObject.AddComponent<Image>();
            bg.color = Color.white;
#if UNITY_EDITOR
            Sprite bgSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_hud_bar_bg.png");
            if (bgSprite != null) { bg.sprite = bgSprite; bg.type = Image.Type.Sliced; }
#endif

            RectTransform fillRt = EnsureRectTransform(rt, "Fill");
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(4, 2);
            fillRt.offsetMax = new Vector2(-4, -2);

            var fillImg = fillRt.GetComponent<Image>() ?? fillRt.gameObject.AddComponent<Image>();
            fillImg.color = Color.white;
#if UNITY_EDITOR
            Sprite fillSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/HUD/Frames/{fillSpriteName}");
            if (fillSprite != null) fillImg.sprite = fillSprite;
            else fillImg.color = fallbackFillColor;
#else
            fillImg.color = fallbackFillColor;
#endif
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;

            return fillImg;
        }

        private static Button CreateMenuButton(string name, Transform parent, string label, string iconName, Color bgColor, Color outlineColor, float width = 120f)
        {
            RectTransform rt = EnsureRectTransform(parent, name);
            rt.sizeDelta = new Vector2(width, 38);

            var img = rt.GetComponent<Image>() ?? rt.gameObject.AddComponent<Image>();
            img.color = Color.white;
#if UNITY_EDITOR
            Sprite btnSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_hud_bar_bg.png");
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; }
#endif
            var outline = rt.GetComponent<Outline>() ?? rt.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = rt.GetComponent<Button>() ?? rt.gameObject.AddComponent<Button>();

            // Small Icon on Left
            if (!string.IsNullOrEmpty(iconName))
            {
                RectTransform iconRt = EnsureRectTransform(rt, "Icon");
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.anchoredPosition = new Vector2(8, 0);
                iconRt.sizeDelta = new Vector2(20, 20);
                var iconImg = iconRt.GetComponent<Image>() ?? iconRt.gameObject.AddComponent<Image>();
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
#if UNITY_EDITOR
                Sprite spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/HUD/Icons/{iconName}");
                if (spr != null) iconImg.sprite = spr;
#endif
            }

            var txt = CreateTMPText("Label", rt, label, 11, TextAlignmentOptions.Center);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = new Vector2(24, 0);
            txt.rectTransform.offsetMax = new Vector2(-4, 0);
            txt.fontStyle = FontStyles.Bold;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return btn;
        }

        private static TMP_Text CreateTMPText(string name, Transform parent, string content, float size, TextAlignmentOptions align)
        {
            RectTransform rt = EnsureRectTransform(parent, name);
            var tmp = rt.GetComponent<TextMeshProUGUI>() ?? rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            ActionRPG.UI.FontManager.ApplyFont(tmp);
            return tmp;
        }

        /// <summary>
        /// Tự động tìm kiếm các ô SkillSlotUI con bên trong thanh Skill Bar mà không cần kéo thả thủ công trên Inspector.
        /// Tự động sửa lỗi mảng skillSlots bị gán nhầm () hoặc null.
        /// </summary>
        public void AutoFindSkillSlots()
        {
            bool hasInvalid = (skillSlots == null || skillSlots.Length == 0);
            if (!hasInvalid)
            {
                for (int i = 0; i < skillSlots.Length; i++)
                {
                    if (skillSlots[i] == null) { hasInvalid = true; break; }
                }
            }

            if (hasInvalid)
            {
                // 1. Tìm container thanh kỹ năng "BottomRight_SkillBar"
                Transform skillBarTr = transform.Find("BottomRight_SkillBar");
                SkillSlotUI[] foundSlots = null;

                if (skillBarTr != null)
                {
                    foundSlots = skillBarTr.GetComponentsInChildren<SkillSlotUI>(true);
                }

                if (foundSlots == null || foundSlots.Length == 0)
                {
                    foundSlots = GetComponentsInChildren<SkillSlotUI>(true);
                }

                if (foundSlots == null || foundSlots.Length == 0)
                {
                    // Thử tìm trực tiếp theo tên GameObject "SkillSlot_1".."SkillSlot_6"
                    Transform bar = skillBarTr != null ? skillBarTr : transform;
                    var dynamicList = new System.Collections.Generic.List<SkillSlotUI>();
                    for (int i = 1; i <= 6; i++)
                    {
                        Transform slotTr = bar.Find($"SkillSlot_{i}") ?? transform.Find($"SkillSlot_{i}");
                        if (slotTr == null)
                        {
                            foreach (Transform child in GetComponentsInChildren<Transform>(true))
                            {
                                if (child.name == $"SkillSlot_{i}") { slotTr = child; break; }
                            }
                        }
                        if (slotTr != null)
                        {
                            var sUI = slotTr.GetComponent<SkillSlotUI>() ?? slotTr.gameObject.AddComponent<SkillSlotUI>();
                            sUI.EnsureReferences();
                            dynamicList.Add(sUI);
                        }
                    }
                    if (dynamicList.Count > 0)
                    {
                        foundSlots = dynamicList.ToArray();
                    }
                }

                if (foundSlots != null && foundSlots.Length > 0)
                {
                    var slotList = new System.Collections.Generic.List<SkillSlotUI>(foundSlots);
                    slotList.Sort((a, b) =>
                    {
                        int aNum = ExtractSlotNumber(a.name);
                        int bNum = ExtractSlotNumber(b.name);
                        if (aNum != bNum) return aNum.CompareTo(bNum);
                        return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
                    });
                    skillSlots = slotList.ToArray();
                }
            }

            // Đảm bảo từng slot con có đầy đủ tham chiếu Image / Text bên trong
            if (skillSlots != null)
            {
                for (int i = 0; i < skillSlots.Length; i++)
                {
                    if (skillSlots[i] != null)
                    {
                        skillSlots[i].EnsureReferences();
                    }
                }
            }
        }

        private static int ExtractSlotNumber(string name)
        {
            if (string.IsNullOrEmpty(name)) return 999;
            var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int n)) return n;
            return 999;
        }
        #endregion
    }

    /// <summary>
    /// Tự động nạp Canvas UI, GameplayHUD, ClassDemoSwitcherUI [F1], PauseMenuUI [ESC]
    /// vào bất kỳ scene nào khi chạy Play mode nếu chưa có sẵn.
    /// </summary>
    public static class RuntimeHUDAutoLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void EnsureHUDInScene()
        {
            if (Object.FindAnyObjectByType<GameplayHUD>() == null)
            {
                Canvas overlayCanvas = null;
                GameObject gameUIRoot = GameObject.Find("GameUIRoot");
                if (gameUIRoot != null)
                {
                    overlayCanvas = gameUIRoot.GetComponent<Canvas>();
                }

                if (overlayCanvas == null)
                {
                    Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                    foreach (var c in allCanvases)
                    {
                        if (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            overlayCanvas = c;
                            break;
                        }
                    }
                }

                GameObject canvasObj;
                if (overlayCanvas == null)
                {
                    canvasObj = new GameObject("GameUIRoot", typeof(RectTransform));
                    overlayCanvas = canvasObj.AddComponent<Canvas>();
                    overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var scaler = canvasObj.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                    canvasObj.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    canvasObj = overlayCanvas.gameObject;
                }

                // Ensure EventSystem
                if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var es = new GameObject("EventSystem");
                    es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                // Add GameplayHUD
                GameObject hudGO = new GameObject("GameplayHUD", typeof(RectTransform));
                hudGO.transform.SetParent(canvasObj.transform, false);
                var rt = hudGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                hudGO.AddComponent<GameplayHUD>();

                // Ensure ClassDemoSwitcherUI, PauseMenuUI, CharacterUI, ClassSelectionUI, InventoryUI
                if (Object.FindAnyObjectByType<ClassDemoSwitcherUI>() == null)
                {
                    canvasObj.AddComponent<ClassDemoSwitcherUI>();
                }
                if (Object.FindAnyObjectByType<PauseMenuUI>() == null)
                {
                    canvasObj.AddComponent<PauseMenuUI>();
                }
                if (Object.FindAnyObjectByType<ActionRPG.UI.Character.CharacterUI>() == null)
                {
                    canvasObj.AddComponent<ActionRPG.UI.Character.CharacterUI>();
                }
                if (Object.FindAnyObjectByType<ClassSelectionUI>() == null)
                {
                    canvasObj.AddComponent<ClassSelectionUI>();
                }
                if (Object.FindAnyObjectByType<InventoryUI>() == null)
                {
                    canvasObj.AddComponent<InventoryUI>();
                }

                // Khởi tạo Bảng Chỉ Số [U], Balo Túi Đồ [I] và Minimap 2D
                CharacterStatusUI.EnsureUI(canvasObj.transform);
                InventoryBagUI.EnsureUI(canvasObj.transform);
                MinimapSystem2D.EnsureUI(canvasObj.transform);

                Debug.Log("<color=cyan>[RuntimeHUDAutoLoader] Tự động khởi tạo đầy đủ hệ thống UI Canvas, GameplayHUD, CharacterStatusUI [U], InventoryBagUI [I], MinimapSystem2D thành công!</color>");
            }
        }
    }
}

