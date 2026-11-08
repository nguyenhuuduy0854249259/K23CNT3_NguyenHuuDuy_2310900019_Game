using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Classes;
using ActionRPG.Characters;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Items;
using ActionRPG.Pets;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Visuals;
using ActionRPG.Presentation.Views;
using ActionRPG.Player;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.UI
{
    /// <summary>
    /// Bảng Điều Khiển & Chuyển Đổi Class Demo (Lumina Class Demo Switcher).
    /// Cho phép chuyển đổi qua lại tức thì giữa 14 Class, kiểm tra 3 Ấn nội tại,
    /// kích hoạt thử nghiệm các cơ chế chiến đấu độc quyền và công cụ hỗ trợ test thủ công.
    /// Phím tắt nhanh: [F1] hoặc [K] hoặc Nút "⚔ Đổi Class [F1]" trên HUD.
    /// </summary>
    public class ClassDemoSwitcherUI : MonoBehaviour
    {
        public static ClassDemoSwitcherUI Instance { get; private set; }

        [Header("Tham Chiếu Panel")]
        public GameObject panelRoot;
        public Button closeButton;

        [Header("Bộ Lọc Nhánh")]
        public Button btnFilterAll;
        public Button btnFilterWarrior;
        public Button btnFilterMage;
        public Button btnFilterArcher;
        public Button btnFilterNovice;

        [Header("Danh Sách Class")]
        public Transform classListContainer;
        public List<CharacterClassData> registeredClasses = new List<CharacterClassData>();

        [Header("Chi Tiết Class Đang Chọn")]
        public TMP_Text classNameText;
        public TMP_Text classTagText;
        public TMP_Text difficultyStarsText;
        public TMP_Text statsOverviewText;
        public TMP_Text mechanicTitleText;
        public TMP_Text mechanicDescText;

        [Header("3 Ấn Nội Tại (Passive Seals)")]
        public TMP_Text seal1Title;
        public TMP_Text seal1Desc;
        public TMP_Text seal2Title;
        public TMP_Text seal2Desc;
        public TMP_Text seal3Title;
        public TMP_Text seal3Desc;

        [Header("Kỹ Năng Class")]
        public Transform skillsListContainer;

        [Header("Các Nút Hành Động")]
        public Button applyClassButton;
        public Button testMechanicButton;
        public TMP_Text testMechanicButtonText;

        [Header("Thanh Công Cụ Debug / Test")]
        public Button btnSetLevel1;
        public Button btnSetLevel10;
        public Button btnSetLevel20;
        public Button btnSetLevel30;
        public Button btnAddLevel5;
        public Button btnFullHeal;
        public Button btnToggleInfiniteMana;
        public Button btnAddItems;
        public Button btnSpawnEnemy;

        private CharacterClassData selectedClass;
        private ClassBranch currentFilter = (ClassBranch)(-1); // -1 = All
        private readonly List<GameObject> classListItems = new List<GameObject>();
        private readonly List<GameObject> skillListItems = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null) Instance = this;

            EnsureUIHierarchy();
            LoadClassesDatabase();
        }

        private void Start()
        {
            SetupButtonListeners();

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (registeredClasses.Count > 0)
            {
                SelectClass(registeredClasses[0]);
            }

            foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
            {
                ActionRPG.UI.FontManager.ApplyFont(txt);
            }
        }

        private void Update()
        {
            // Phím tắt F1 hoặc K để đóng / mở bảng Hệ Thống Chuyển Đổi 14 Class
            if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.K))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (panelRoot == null) EnsureUIHierarchy();
            if (panelRoot == null) return;
            bool active = !panelRoot.activeSelf;
            panelRoot.SetActive(active);
            Debug.Log($"<color=green>[ClassDemoSwitcherUI] Toggled panel to {active}</color>");

            if (active)
            {
                panelRoot.transform.SetAsLastSibling();
                // Đồng bộ class đang chọn với class hiện tại của Player
                if (ClassSystem.Instance != null && ClassSystem.Instance.currentClass != null)
                {
                    var current = registeredClasses.Find(c => c.className == ClassSystem.Instance.currentClass.className);
                    if (current != null) SelectClass(current);
                }
                PopulateClassList();
            }
        }

        public void Open()
        {
            if (panelRoot == null) EnsureUIHierarchy();
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                Debug.Log("<color=green>[ClassDemoSwitcherUI] Opened panel</color>");
                panelRoot.transform.SetAsLastSibling();
            }
            PopulateClassList();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void LoadClassesDatabase()
        {
            if (registeredClasses.Count == 0)
            {
                // Nạp từ Resources nếu có
                var loaded = Resources.LoadAll<CharacterClassData>("");
                if (loaded != null && loaded.Length > 0)
                {
                    registeredClasses.AddRange(loaded);
                }

#if UNITY_EDITOR
                if (registeredClasses.Count == 0)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterClassData");
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var c = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterClassData>(path);
                        if (c != null && !registeredClasses.Contains(c))
                        {
                            registeredClasses.Add(c);
                        }
                    }
                }
#endif
            }

            // Sắp xếp theo Branch và Tier
            registeredClasses.Sort((a, b) =>
            {
                int bCompare = a.branch.CompareTo(b.branch);
                return bCompare != 0 ? bCompare : a.tier.CompareTo(b.tier);
            });
        }

        private void SetupButtonListeners()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            if (btnFilterAll != null) btnFilterAll.onClick.AddListener(() => SetFilter((ClassBranch)(-1)));
            if (btnFilterWarrior != null) btnFilterWarrior.onClick.AddListener(() => SetFilter(ClassBranch.Warrior));
            if (btnFilterMage != null) btnFilterMage.onClick.AddListener(() => SetFilter(ClassBranch.Mage));
            if (btnFilterArcher != null) btnFilterArcher.onClick.AddListener(() => SetFilter(ClassBranch.Archer));
            if (btnFilterNovice != null) btnFilterNovice.onClick.AddListener(() => SetFilter(ClassBranch.Novice));

            if (applyClassButton != null) applyClassButton.onClick.AddListener(ApplySelectedClass);
            if (testMechanicButton != null) testMechanicButton.onClick.AddListener(TriggerSelectedClassMechanic);

            // Debug Toolbar
            if (btnSetLevel1 != null) btnSetLevel1.onClick.AddListener(() => SetPlayerLevel(1));
            if (btnSetLevel10 != null) btnSetLevel10.onClick.AddListener(() => SetPlayerLevel(10));
            if (btnSetLevel20 != null) btnSetLevel20.onClick.AddListener(() => SetPlayerLevel(20));
            if (btnSetLevel30 != null) btnSetLevel30.onClick.AddListener(() => SetPlayerLevel(30));
            if (btnAddLevel5 != null) btnAddLevel5.onClick.AddListener(AddPlayer5Levels);
            if (btnFullHeal != null) btnFullHeal.onClick.AddListener(FullHealPlayer);
            if (btnToggleInfiniteMana != null) btnToggleInfiniteMana.onClick.AddListener(ToggleInfiniteMana);
            if (btnAddItems != null) btnAddItems.onClick.AddListener(AddTestingItems);
            if (btnSpawnEnemy != null) btnSpawnEnemy.onClick.AddListener(SpawnTestingEnemy);
        }

        private void SetFilter(ClassBranch branch)
        {
            currentFilter = branch;
            PopulateClassList();
        }

        private void PopulateClassList()
        {
            if (classListContainer == null) return;

            foreach (var go in classListItems)
            {
                if (go != null) Destroy(go);
            }
            classListItems.Clear();

            foreach (var cData in registeredClasses)
            {
                if (cData == null) continue;
                if ((int)currentFilter != -1 && cData.branch != currentFilter) continue;

                var btnObj = CreateClassListItem(cData);
                classListItems.Add(btnObj);
            }
        }

        private static GameObject CreateUIGameObject(string name, Transform parent = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private GameObject CreateClassListItem(CharacterClassData cData)
        {
            GameObject itemObj = CreateUIGameObject($"Item_{cData.className}", classListContainer);
            var rt = itemObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 50);

            var le = itemObj.AddComponent<LayoutElement>();
            le.minHeight = 50;
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            var img = itemObj.AddComponent<Image>();
            img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            img.type = Image.Type.Sliced;

            bool isCurrent = ClassSystem.Instance != null && ClassSystem.Instance.currentClass != null && ClassSystem.Instance.currentClass.className == cData.className;
            bool isSelected = selectedClass == cData;

            if (isSelected) img.color = FontManager.AetherBlueColor;
            else if (isCurrent) img.color = new Color(0.14f, 0.34f, 0.24f, 0.92f);
            else img.color = FontManager.SlotEmptyBgColor;

            var outline = itemObj.AddComponent<Outline>();
            outline.effectColor = isSelected ? FontManager.AetherCyanColor : (isCurrent ? new Color(0.35f, 0.95f, 0.50f, 0.95f) : new Color(0.45f, 0.48f, 0.58f, 0.45f));
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = itemObj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                SelectClass(cData);
                PopulateClassList();
            });

            // Text thông tin class (căn gọn bên trong)
            GameObject textObj = CreateUIGameObject("Text", itemObj.transform);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 4);
            textRt.offsetMax = new Vector2(-10, -4);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            string tierStr = cData.tier == 0 ? "Tân Thủ" : (cData.tier == 1 ? "Tier 1" : "Tier 2 Lumina");
            string currentMarker = isCurrent ? " <color=#55FF77>✔ Đang Dùng</color>" : "";
            tmp.text = $"<b>{cData.className}</b>{currentMarker}\n<size=11><color=#FFD54F>{tierStr}</color> | <color=#80D8FF>{cData.role}</color></size>";
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = FontManager.IvoryTextColor;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            ActionRPG.UI.FontManager.ApplyFont(tmp);

            return itemObj;
        }

        public void ClearDetailTexts()
        {
            if (classNameText != null) { classNameText.text = string.Empty; classNameText.ForceMeshUpdate(); }
            if (classTagText != null) { classTagText.text = string.Empty; classTagText.ForceMeshUpdate(); }
            if (difficultyStarsText != null) { difficultyStarsText.text = string.Empty; difficultyStarsText.ForceMeshUpdate(); }
            if (statsOverviewText != null) { statsOverviewText.text = string.Empty; statsOverviewText.ForceMeshUpdate(); }
            if (mechanicTitleText != null) { mechanicTitleText.text = string.Empty; mechanicTitleText.ForceMeshUpdate(); }
            if (mechanicDescText != null) { mechanicDescText.text = string.Empty; mechanicDescText.ForceMeshUpdate(); }
            if (seal1Title != null) { seal1Title.text = string.Empty; seal1Title.ForceMeshUpdate(); }
            if (seal1Desc != null) { seal1Desc.text = string.Empty; seal1Desc.ForceMeshUpdate(); }
            if (seal2Title != null) { seal2Title.text = string.Empty; seal2Title.ForceMeshUpdate(); }
            if (seal2Desc != null) { seal2Desc.text = string.Empty; seal2Desc.ForceMeshUpdate(); }
            if (seal3Title != null) { seal3Title.text = string.Empty; seal3Title.ForceMeshUpdate(); }
            if (seal3Desc != null) { seal3Desc.text = string.Empty; seal3Desc.ForceMeshUpdate(); }
            if (testMechanicButtonText != null) { testMechanicButtonText.text = string.Empty; testMechanicButtonText.ForceMeshUpdate(); }

            if (skillsListContainer != null)
            {
                foreach (var go in skillListItems)
                {
                    if (go != null) Destroy(go);
                }
                skillListItems.Clear();
            }
        }

        public void SelectClass(CharacterClassData cData)
        {
            ClearDetailTexts();
            if (cData == null) return;
            selectedClass = cData;

            if (classNameText != null)
                classNameText.text = $"<color=#FFE082>{cData.className}</color>";

            if (classTagText != null)
            {
                string tierName = cData.tier == 0 ? "Giai Đoạn Tân Thủ (Tier 0)" : (cData.tier == 1 ? "Giai Đoạn Cơ Bản (Tier 1)" : "Chuyên Phái Lumina Tối Thượng (Tier 2)");
                string elemStr = cData.defaultElement != ElementType.None ? $" | Hệ: {cData.defaultElement}" : "";
                classTagText.text = $"{tierName} | Nhánh: {cData.branch} | Vai trò: {cData.role}{elemStr}";
            }

            if (difficultyStarsText != null)
            {
                string stars = "";
                for (int i = 0; i < 5; i++) stars += i < cData.difficulty ? "★" : "☆";
                string diffName = cData.difficulty switch { 1 => "Dễ", 2 => "Bình Thường", 3 => "Khó", 4 => "Rất Khó", _ => "Bậc Thầy (Master)" };
                difficultyStarsText.text = $"Độ khó: <color=#FFB300>{diffName} {stars}</color>";
            }

            if (statsOverviewText != null)
            {
                statsOverviewText.text =
                    $"<b>Máu cơ bản (HP):</b> <color=#EF5350>{cData.baseHp:F0}</color>   " +
                    $"<b>Năng lượng (MP):</b> <color=#42A5F5>{cData.baseMp:F0}</color>   " +
                    $"<b>Tấn công (ATK):</b> <color=#FFA726>{cData.baseAtk:F0}</color>   " +
                    $"<b>Phòng ngự (DEF):</b> <color=#66BB6A>{cData.baseDef:F0}</color>   " +
                    $"<b>Tốc chạy:</b> {cData.baseSpeed:F1}m/s   " +
                    $"<b>Chí mạng:</b> {cData.baseCritRate * 100f:F0}%";
            }

            // Cơ chế đặc trưng
            if (mechanicTitleText != null)
                mechanicTitleText.text = $"⚙ CƠ CHẾ ĐẶC TRƯNG: <color=#00E5FF>{cData.specialMechanic}</color>";

            if (mechanicDescText != null)
                mechanicDescText.text = string.IsNullOrEmpty(cData.mechanicDescription)
                    ? cData.description
                    : $"{cData.description}\n\n<color=#B2EBF2><b>Vận hành cơ chế:</b> {cData.mechanicDescription}</color>";

            // 3 Ấn Nội Tại
            if (seal1Title != null) seal1Title.text = string.IsNullOrEmpty(cData.passiveSeal1Name) ? "Ấn 1" : $"Ấn 1: {cData.passiveSeal1Name}";
            if (seal1Desc != null) seal1Desc.text = cData.passiveSeal1Desc;

            if (seal2Title != null) seal2Title.text = string.IsNullOrEmpty(cData.passiveSeal2Name) ? "Ấn 2" : $"Ấn 2: {cData.passiveSeal2Name}";
            if (seal2Desc != null) seal2Desc.text = cData.passiveSeal2Desc;

            if (seal3Title != null) seal3Title.text = string.IsNullOrEmpty(cData.passiveSeal3Name) ? "Ấn 3" : $"Ấn 3: {cData.passiveSeal3Name}";
            if (seal3Desc != null) seal3Desc.text = cData.passiveSeal3Desc;

            // Nút test cơ chế
            if (testMechanicButtonText != null)
            {
                testMechanicButtonText.text = GetMechanicActionLabel(cData.specialMechanic);
            }

            // Hiển thị danh sách kỹ năng
            PopulateSkillsList(cData);
        }

        private string GetMechanicActionLabel(SpecialMechanic mech)
        {
            return mech switch
            {
                SpecialMechanic.PetWeapon => "⚡ Đổi Vũ Khí Pet (+25% Dmg)",
                SpecialMechanic.ElementalQueue => "⚡ Nạp Hạt Nguyên Tố (Queue)",
                SpecialMechanic.DualPetSummon => "⚡ Triệu Hồi 2 Pet / Cộng Hưởng",
                SpecialMechanic.TimeControl => "⚡ Nghịch Chuyển Không Gian 3s",
                SpecialMechanic.HolyDarkPriest => "⚡ Hoán Đổi Stance Thánh / Ma",
                SpecialMechanic.CauldronAlchemy => "⚡ Đặt Chum Ma Pháp & Nấu Thuốc",
                SpecialMechanic.GunOverheat or SpecialMechanic.EngineerTurrets => "⚡ Dựng Trụ Súng / Tăng Nhiệt",
                SpecialMechanic.StealthCritical => "⚡ Kích Hoạt Tàng Hình & Decoy",
                SpecialMechanic.ElementalQuiver => "⚡ Đổi Loại Mũi Tên Nguyên Tố",
                SpecialMechanic.SpiritCharge => "⚡ Gọi Linh Thể / Hợp Thể x3",
                _ => "⚡ Thử Nghiệm Kỹ Năng Class"
            };
        }

        private void PopulateSkillsList(CharacterClassData cData)
        {
            if (skillsListContainer == null) return;

            foreach (var go in skillListItems)
            {
                if (go != null) Destroy(go);
            }
            skillListItems.Clear();

            if (cData.unlockedSkills == null || cData.unlockedSkills.Count == 0) return;

            string[] hotkeys = { "1", "2", "3", "4", "5", "6" };
            for (int i = 0; i < cData.unlockedSkills.Count; i++)
            {
                var skill = cData.unlockedSkills[i];
                if (skill == null) continue;

                GameObject skObj = CreateUIGameObject($"Skill_{skill.skillId}", skillsListContainer);
                var rt = skObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 30);

                var le = skObj.AddComponent<LayoutElement>();
                le.minHeight = 30;
                le.preferredHeight = 30;
                le.flexibleWidth = 1;

                var img = skObj.AddComponent<Image>();
                img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
                img.type = Image.Type.Sliced;
                img.color = FontManager.SlotEmptyBgColor;

                var outline = skObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.25f, 0.65f, 0.90f, 0.40f);
                outline.effectDistance = new Vector2(1, -1);

                string key = i < hotkeys.Length ? $"[{hotkeys[i]}]" : $"[{i + 1}]";
                string elemStr = skill.element != ElementType.None ? $" <color=#80D8FF>({skill.element})</color>" : "";

                GameObject txtObj = CreateUIGameObject("Text", skObj.transform);
                var trt = txtObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(10, 2);
                trt.offsetMax = new Vector2(-10, -2);

                var tmp = txtObj.AddComponent<TextMeshProUGUI>();
                // Tách màu rõ giữa Phím tắt (Xanh ngọc Lumina), Tên chiêu (Vàng đồng) và Mô tả (Trắng ngà)
                tmp.text = $"<color=#00E5FF><b>{key}</b></color> <color=#FFE066><b>{skill.skillName}</b></color>{elemStr}  <size=11><color=#64B5F6>MP:{skill.manaCost}</color> | <color=#FFAB91>CD:{skill.cooldownSeconds:F1}s</color> — <color=#F7F2E6>{skill.description}</color></size>";
                tmp.fontSize = 12;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
                ActionRPG.UI.FontManager.ApplyFont(tmp);

                skillListItems.Add(skObj);
            }
        }

        /// <summary>
        /// Áp dụng ngay Class đang chọn vào nhân vật người chơi!
        /// Đồng bộ đầy đủ: Tự động nâng Level nếu thiếu, nạp 5 kỹ năng [Q,W,E,R,T], cập nhật ActionBar & HUD, đổi vũ khí & animation.
        /// </summary>
        public void ApplySelectedClass()
        {
            if (selectedClass == null) return;

            // 1. Tự động nâng cấp Level nếu class yêu cầu Tier 1 / Tier 2 (Lv 10+) hoặc Tier 3 (Lv 20+) khi đang ở cấp thấp
            int minLevel = 1;
            if (selectedClass.tier >= 2) minLevel = 20;
            else if (selectedClass.tier == 1 || selectedClass.branch != ClassBranch.Novice) minLevel = 10;

            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null && stats.level < minLevel)
            {
                SetPlayerLevel(minLevel);
            }

            if (ClassSystem.Instance != null)
            {
                ClassSystem.Instance.PromoteTo(selectedClass);

                // 2. Nạp trực tiếp 6 SkillData tương ứng của Class vào các phím số [1, 2, 3, 4, 5, 6]
                var combat = FindObjectOfType<PlayerSkillCombat2D>();
                if (combat != null)
                {
                    combat.LoadClassSkills(selectedClass);
                }

                // 3. Bắn Event / Cập nhật sang ActionBar & GameplayHUD để làm mới toàn bộ 6 Icon kỹ năng & hồi chiêu
                var hud = GameplayHUD.Instance != null ? GameplayHUD.Instance : FindObjectOfType<GameplayHUD>();
                if (hud != null)
                {
                    hud.RefreshClassUI(selectedClass);
                }

                // 4. Đồng bộ vũ khí hệ phái (Kiếm / Gậy / Cung) sang WeaponSocketView & PlayerEquipmentVisualView
                var weaponSocket = FindObjectOfType<WeaponSocketView>();
                if (weaponSocket != null)
                {
                    weaponSocket.ApplyClassWeapon(selectedClass.branch);
                }

                var equipVisual = FindObjectOfType<PlayerEquipmentVisualView>();
                if (equipVisual != null)
                {
                    equipVisual.UpdateVisuals();
                }

                // 5. Cập nhật hoạt họa 4 hướng & chuyển class visual
                var fourDirAnim = FindObjectOfType<LuminaFourDirectionCharacterAnimator>();
                if (fourDirAnim != null)
                {
                    fourDirAnim.ApplyClassBranch(selectedClass.branch);
                }

                var switcher = FindObjectOfType<PlayerVisualSwitcher>();
                if (switcher != null)
                {
                    switcher.SwitchClassVisual(selectedClass.branch);
                }

                var visualAnim = FindObjectOfType<Visuals.ClassVisualAnimator>();
                if (visualAnim != null)
                {
                    visualAnim.ApplyClassAnimation(selectedClass.specialMechanic);
                }

                // Cập nhật UI Nhân Vật nếu đang mở
                if (Character.CharacterUI.Instance != null)
                {
                    Character.CharacterUI.Instance.RefreshAll();
                }

                string msg = $"Đã chuyển thành công sang: {selectedClass.className} (Cấp { (stats != null ? stats.level : minLevel) })!";
                Debug.Log($"<color=green>[ClassDemoSwitcher] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                EventBus.OnPlaySound?.Invoke("equip");

                PopulateClassList();
            }
            else
            {
                Debug.LogWarning("[ClassDemoSwitcher] Không tìm thấy ClassSystem trên người chơi!");
            }
        }

        /// <summary>
        /// Kích hoạt cơ chế đặc trưng của class để thử nghiệm ngay lập tức.
        /// </summary>
        public void TriggerSelectedClassMechanic()
        {
            if (ClassSystem.Instance == null) return;
            var cs = ClassSystem.Instance;

            // Nếu player chưa thuộc class này, tự động chuyển class trước
            if (cs.currentClass == null || cs.currentClass.className != selectedClass.className)
            {
                ApplySelectedClass();
            }

            switch (selectedClass.specialMechanic)
            {
                case SpecialMechanic.PetWeapon:
                    // Thử chuyển đổi vũ khí tiếp theo từ Pet
                    PetBodyShape shape = PetBodyShape.HeavyLarge;
                    if (PlayerPetRoster.Instance != null && PlayerPetRoster.Instance.activePetInstance != null)
                        shape = PlayerPetRoster.Instance.activePetInstance.petData.bodyShape;
                    int nextType = ((int)cs.PetWeapon.currentWeapon + 1) % 4;
                    cs.PetWeapon.swapCooldownTimer = 0f;
                    cs.PetWeapon.TryTransformWeaponFromPet(shape, nextType, ElementType.Fire);
                    EventBus.OnShowUIMessage?.Invoke($"[Cận Chiến Tự Do] Đã biến Pet thành: {cs.PetWeapon.currentWeapon} (+25% Dmg 3s)!");
                    break;

                case SpecialMechanic.ElementalQueue:
                    // Nạp 1 hạt nguyên tố vào hàng đợi
                    ElementType[] elems = { ElementType.Fire, ElementType.Water, ElementType.Wind, ElementType.Electric };
                    ElementType randomElem = elems[UnityEngine.Random.Range(0, elems.Length)];
                    cs.PushElementalQueue(randomElem);
                    EventBus.OnShowUIMessage?.Invoke($"[Pháp Sư Nguyên Tố] Đã nạp hạt: {randomElem} (Hàng đợi: {cs.ElementalQueue.currentQueue.Count}/2)!");
                    break;

                case SpecialMechanic.DualPetSummon:
                    // Triệu hồi 2 Pet
                    PetController[] pets = FindObjectsOfType<PetController>(true);
                    if (pets.Length > 0 && pets[0].petData != null)
                    {
                        cs.SummonerDualPet.SummonBothPets(pets[0].petData, pets[0].petData);
                    }
                    cs.SummonerDualPet.TriggerDualPetResonance(ElementType.Fire, ElementType.Electric);
                    EventBus.OnShowUIMessage?.Invoke("[Pháp Sư Triệu Hồi] Đã kích nổ Cộng Hưởng Lưỡng Thú (+20% Dmg phản ứng)!");
                    break;

                case SpecialMechanic.TimeControl:
                    // Nghịch chuyển không gian
                    cs.TriggerChronoRewind();
                    EventBus.OnShowUIMessage?.Invoke("[Pháp Sư Thời Không] Nghịch chuyển vị trí về 3s trước (+20% MoveSpeed)!");
                    break;

                case SpecialMechanic.HolyDarkPriest:
                    // Đổi Form Thánh / Ma
                    cs.ToggleStance();
                    string fName = cs.HolyDarkPriest.currentForm == PriestForm.Angel ? "Thiên Thần (Hồi Máu/Bảo Vệ)" : "Ác Quỷ (Nguyền Rủa/Hút Máu)";
                    EventBus.OnShowUIMessage?.Invoke($"[Mục Sư Thánh - Ma] Đã đổi sang: {fName}!");
                    break;

                case SpecialMechanic.CauldronAlchemy:
                    // Đặt Chum Ma Pháp
                    var ings = new List<AlchemyIngredient>
                    {
                        new AlchemyIngredient { ingredientName = "Thảo Mộc", quality = MaterialQualityTier.High, element = ElementType.Fire },
                        new AlchemyIngredient { ingredientName = "Quặng Năng Lượng", quality = MaterialQualityTier.High, element = ElementType.Water }
                    };
                    cs.AlchemistCauldron.isCauldronPlaced = false;
                    cs.AlchemistCauldron.PlaceCauldron(cs.transform.position, ings, 1.5f);
                    EventBus.OnShowUIMessage?.Invoke("[Luyện Dược Sư] Đã đặt Chum Ma Pháp ủ nguyên liệu phẩm chất Cao!");
                    break;

                case SpecialMechanic.GunOverheat:
                case SpecialMechanic.EngineerTurrets:
                    // Khai hỏa súng tăng nhiệt & dựng trụ
                    cs.TryShootGun(25f, out float dmgMult);
                    cs.EngineerGunner.DeployTurret(TurretType.Gatling, cs.transform.position + Vector3.right * 1.5f, 0);
                    EventBus.OnShowUIMessage?.Invoke($"[Cơ Khí Sư] Đã dựng Trụ Gatling! Nhiệt lượng súng: {cs.gunHeat:F0}%");
                    break;

                case SpecialMechanic.StealthCritical:
                    // Kích hoạt tàng hình
                    cs.TryEnterStealth();
                    EventBus.OnShowUIMessage?.Invoke("[Sát Thủ Bóng Đêm] Tàng hình 6s (Ambush x300% crit)! Pet tạo Decoy khiêu khích!");
                    break;

                case SpecialMechanic.ElementalQuiver:
                    // Đổi mũi tên tiếp theo
                    var nextArrow = (ElementalArrowType)(((int)cs.ElementalArcher.currentArrow + 1) % 4);
                    cs.ElementalArcher.SelectArrow(nextArrow);
                    EventBus.OnShowUIMessage?.Invoke($"[Xạ Thủ Nguyên Tố] Đã đổi sang: {nextArrow} Tên!");
                    break;

                case SpecialMechanic.SpiritCharge:
                    // Kích hoạt Hợp Thể Fusion
                    if (cs.spiritCharges < 3) cs.SpiritArcher.currentCharges = 3;
                    cs.TriggerSpiritFusion(ElementType.Wind);
                    EventBus.OnShowUIMessage?.Invoke("[Xạ Thủ Tinh Linh] Hợp Thể Fusion x3 Sát Thương! Nhận buff Thần Linh (+20% MoveSpeed)!");
                    break;

                default:
                    EventBus.OnShowUIMessage?.Invoke($"[Class System] Class {selectedClass.className} vận hành ổn định!");
                    break;
            }
        }

        #region Debug Toolbar Actions
        public void SetPlayerLevel(int targetLevel)
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats == null) return;

            stats.level = targetLevel;
            stats.maxExp = CharacterStats.GetRequiredExpForLevel(targetLevel);
            stats.currentExp = 0;
            stats.maxHp = 100f + (targetLevel - 1) * 18f;
            stats.maxMp = 50f + (targetLevel - 1) * 10f;
            stats.currentHp = stats.maxHp;
            stats.currentMp = stats.maxMp;

            stats.CheckPetUnlock(true);
            stats.UpdateStatsUI();

            EventBus.OnPlayerLevelUp?.Invoke(targetLevel);
            EventBus.OnShowUIMessage?.Invoke($"Đã thiết lập Cấp độ: {targetLevel} ({stats.CurrentTier})!");
        }

        public void AddPlayer5Levels()
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null) SetPlayerLevel(stats.level + 5);
        }

        public void FullHealPlayer()
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null)
            {
                stats.currentHp = stats.maxHp;
                stats.currentMp = stats.maxMp;
                stats.UpdateStatsUI();
                EventBus.OnShowUIMessage?.Invoke("Đã hồi đầy 100% HP & MP!");
            }
        }

        public void ToggleInfiniteMana()
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null)
            {
                stats.infiniteManaForTesting = !stats.infiniteManaForTesting;
                if (stats.infiniteManaForTesting)
                {
                    stats.currentMp = stats.maxMp;
                    stats.UpdateStatsUI();
                    EventBus.OnShowUIMessage?.Invoke("⚡ [DEV TEST] Vô Hạn Mana: ĐÃ BẬT (Thoải mái thử nghiệm kỹ năng!)");
                }
                else
                {
                    EventBus.OnShowUIMessage?.Invoke("⚡ [DEV TEST] Vô Hạn Mana: ĐÃ TẮT");
                }
            }
        }

        public void AddTestingItems()
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddGold(1000);

                var hpPotion = Resources.Load<ConsumableItemData>("Items/potion_small_hp") ??
                               AssetDatabaseFindItem<ConsumableItemData>("potion_small_hp");
                var mpPotion = Resources.Load<ConsumableItemData>("Items/potion_small_mp") ??
                               AssetDatabaseFindItem<ConsumableItemData>("potion_small_mp");

                if (hpPotion != null) Inventory.Instance.AddItem(hpPotion, 10);
                if (mpPotion != null) Inventory.Instance.AddItem(mpPotion, 10);

                EventBus.OnShowUIMessage?.Invoke("Đã nhận: +1000 Vàng, 10 Bình Máu [E], 10 Bình Mana [R]!");
            }
        }

        public void SpawnTestingEnemy()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPos = player != null ? player.transform.position + Vector3.right * 2.5f : Vector3.zero;

            GameObject enemyObj = new GameObject("TestEnemy_TrainingDummy");
            enemyObj.transform.position = spawnPos;

            var sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.85f, 0.35f, 0.25f);

            var col = enemyObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.0f);

            var rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var stats = enemyObj.AddComponent<Enemies.EnemyStats>();
            stats.enemyName = "Bù Nhìn Luyện Võ";
            stats.level = 1;
            stats.maxHp = 250f;
            stats.currentHp = 250f;
            stats.defense = 5f;
            stats.element = ElementType.Fire;

            var hurt = enemyObj.AddComponent<Hurtbox2D>();
            hurt.OnTakeDamageCustom = (dmg) => stats.TakeDamage(dmg);

            enemyObj.AddComponent<Enemies.EnemyController>();

            EventBus.OnShowUIMessage?.Invoke("Đã triệu hồi Quái Vật Mục Tiêu thử nghiệm gần người chơi!");
        }

        private T AssetDatabaseFindItem<T>(string name) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }
#endif
            return null;
        }
        #endregion

        #region Runtime UI Auto-Hierarchy Builder
        public void EnsureUIHierarchy()
        {
            if (this == null || gameObject == null) return;

            if (panelRoot != null)
            {
                // Kiểm tra nếu panelRoot đã được nâng cấp lên chuẩn VerticalLayoutGroup 30%/70% chưa
                if (panelRoot.transform.Find("RightPanel/HeaderInfoBox") != null)
                {
                    var existingRt = panelRoot.GetComponent<RectTransform>();
                    if (existingRt != null)
                    {
                        existingRt.anchorMin = new Vector2(0.5f, 0.5f);
                        existingRt.anchorMax = new Vector2(0.5f, 0.5f);
                        existingRt.pivot = new Vector2(0.5f, 0.5f);
                        existingRt.anchoredPosition = new Vector2(40f, 14f);
                        existingRt.sizeDelta = new Vector2(1210f, 720f);
                    }
                    FontManager.SanitizeUIHierarchy(panelRoot.transform);
                    return;
                }

                // Dọn các đối tượng con cũ để dựng lại bố cục chuẩn Pixel-Perfect
                for (int i = panelRoot.transform.childCount - 1; i >= 0; i--)
                {
                    var child = panelRoot.transform.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            Canvas canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject rootGO = GameObject.Find("GameUIRoot");
                if (rootGO != null) canvas = rootGO.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var c in allCanvases)
                {
                    if (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        canvas = c;
                        break;
                    }
                }
            }

            Transform canvasTrans = canvas != null ? canvas.transform : transform;

            if (panelRoot == null)
            {
                Transform existing = canvasTrans.Find("ClassDemoSwitcher_Panel");
                if (existing != null) panelRoot = existing.gameObject;
            }

            GameObject root = panelRoot != null ? panelRoot : CreateUIGameObject("ClassDemoSwitcher_Panel", canvasTrans);
            panelRoot = root;

            var rt = root.GetComponent<RectTransform>() ?? root.AddComponent<RectTransform>();
            // Neo Chính Giữa (Center) với độ lệch nhẹ sang phải (+40px) để cách đều Bảng Chỉ Số (Trái: X=16..366) và Balo (Phải: X=1636..1904) ~30px
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(40f, 14f);
            rt.sizeDelta = new Vector2(1210f, 720f);
            rt.localScale = Vector3.one;

            // Nền Xám Đen 95% (9-Slice) + Viền Vàng Đồng Lumina
            var bg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            bg.sprite = FontManager.GetOrCreatePanelNineSliceSprite();
            bg.type = Image.Type.Sliced;
            bg.color = FontManager.PanelBgColor;
            var outline = root.GetComponent<Outline>() ?? root.AddComponent<Outline>();
            outline.effectColor = FontManager.BronzeGoldBorder;
            outline.effectDistance = new Vector2(2, -2);

            // Header Title (Nằm gọn trong thanh tiêu đề trên cùng)
            CreateHeader(root.transform);

            // Left Panel (30% chiều rộng bảng: 4 Tab lọc & ScrollRect Danh sách Class)
            CreateLeftPanel(root.transform);

            // Right Panel (70% chiều rộng bảng: VerticalLayoutGroup Chi tiết Class, Cơ chế, 3 Ấn, 5 Kỹ năng, 2 Nút hành động)
            CreateRightPanel(root.transform);

            // Bottom Toolbar (Thanh công cụ Test dưới đáy bảng)
            CreateBottomToolbar(root.transform);

            FontManager.SanitizeUIHierarchy(root.transform);
        }

        private void CreateHeader(Transform parent)
        {
            GameObject headerObj = CreateUIGameObject("Header", parent);
            var rt = headerObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -6);
            rt.sizeDelta = new Vector2(-12, 42);

            var hBg = headerObj.AddComponent<Image>();
            hBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            hBg.type = Image.Type.Sliced;
            hBg.color = FontManager.HeaderBgColor;

            var title = CreateTMP(headerObj.transform, "Title", "⚔ HỆ THỐNG CHUYỂN ĐỔI 14 CLASS & TEST CƠ CHẾ LUMINA", 18, TextAlignmentOptions.Center, FontManager.GoldTitleColor);
            title.fontStyle = FontStyles.Bold;
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(16, 0);
            title.rectTransform.offsetMax = new Vector2(-48, 0);

            // Nút đóng [X] đặt góc trên bên phải
            GameObject closeObj = CreateUIGameObject("CloseBtn", headerObj.transform);
            var crt = closeObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(1, 0.5f);
            crt.anchorMax = new Vector2(1, 0.5f);
            crt.pivot = new Vector2(1, 0.5f);
            crt.anchoredPosition = new Vector2(-8, 0);
            crt.sizeDelta = new Vector2(30, 30);

            var cImg = closeObj.AddComponent<Image>();
            cImg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            cImg.type = Image.Type.Sliced;
            cImg.color = new Color(0.68f, 0.16f, 0.18f, 0.96f);
            closeButton = closeObj.AddComponent<Button>();
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);

            var cTxt = CreateTMP(closeObj.transform, "X", "X", 14, TextAlignmentOptions.Center, Color.white);
            cTxt.fontStyle = FontStyles.Bold;
            cTxt.rectTransform.anchorMin = Vector2.zero;
            cTxt.rectTransform.anchorMax = Vector2.one;
            cTxt.rectTransform.offsetMin = Vector2.zero;
            cTxt.rectTransform.offsetMax = Vector2.zero;
        }

        private void CreateLeftPanel(Transform parent)
        {
            // Cột Trái (Danh sách Class - chiếm ~30% chiều rộng bảng = 350px)
            GameObject leftObj = CreateUIGameObject("LeftPanel", parent);
            var rt = leftObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(12, -54);
            rt.sizeDelta = new Vector2(350, -108);

            // 4 Tab lọc chia đều nhau ở phía trên
            GameObject tabsObj = CreateUIGameObject("FilterTabs", leftObj.transform);
            var trt = tabsObj.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = Vector2.zero;
            trt.sizeDelta = new Vector2(0, 34);

            var hLayout = tabsObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 6;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            btnFilterAll = CreateFilterTab(tabsObj.transform, "Tất Cả");
            btnFilterWarrior = CreateFilterTab(tabsObj.transform, "Cận Chiến");
            btnFilterMage = CreateFilterTab(tabsObj.transform, "Pháp Sư");
            btnFilterArcher = CreateFilterTab(tabsObj.transform, "Xạ Thủ");

            // ScrollRect chứa danh sách các ô Class
            GameObject scrollObj = CreateUIGameObject("ClassListScrollView", leftObj.transform);
            var srt = scrollObj.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = new Vector2(0, -40);

            var scrollImg = scrollObj.AddComponent<Image>();
            scrollImg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            scrollImg.type = Image.Type.Sliced;
            scrollImg.color = FontManager.SectionBgColor;
            scrollObj.AddComponent<RectMask2D>();

            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            // Content
            GameObject contentObj = CreateUIGameObject("Content", scrollObj.transform);
            var crt = contentObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0, 1);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(0, 600);

            var vLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(6, 6, 6, 6);
            vLayout.spacing = 6;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            var fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = crt;
            scroll.viewport = srt;
            classListContainer = crt;
        }

        private Button CreateFilterTab(Transform parent, string label)
        {
            GameObject tabObj = CreateUIGameObject($"Tab_{label}", parent);

            var img = tabObj.AddComponent<Image>();
            img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.14f, 0.20f, 0.28f, 0.95f);

            var outline = tabObj.AddComponent<Outline>();
            outline.effectColor = FontManager.BronzeGoldBorder;
            outline.effectDistance = new Vector2(1, -1);

            var btn = tabObj.AddComponent<Button>();
            var txt = CreateTMP(tabObj.transform, "Label", label, 12, TextAlignmentOptions.Center, FontManager.IvoryTextColor);
            txt.fontStyle = FontStyles.Bold;
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;

            return btn;
        }

        private static void SetTopStretch(RectTransform rt, float left, float top, float right, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(left, top);
            rt.sizeDelta = new Vector2(-(left + right), height);
        }

        private void CreateRightPanel(Transform parent)
        {
            // Cột Phải (Chi tiết Class - chiếm ~70% chiều rộng bảng = 824px)
            GameObject rightObj = CreateUIGameObject("RightPanel", parent);
            var rt = rightObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(374, -54);
            rt.sizeDelta = new Vector2(-386, -108);

            var rBg = rightObj.AddComponent<Image>();
            rBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            rBg.type = Image.Type.Sliced;
            rBg.color = FontManager.SectionBgColor;

            // VerticalLayoutGroup xếp rõ từ trên xuống (Padding = 10px, Spacing = 6px)
            var rightVLayout = rightObj.AddComponent<VerticalLayoutGroup>();
            rightVLayout.padding = new RectOffset(10, 10, 8, 8);
            rightVLayout.spacing = 6;
            rightVLayout.childAlignment = TextAnchor.UpperLeft;
            rightVLayout.childControlWidth = true;
            rightVLayout.childControlHeight = true;
            rightVLayout.childForceExpandWidth = true;
            rightVLayout.childForceExpandHeight = false;

            // 1. Khung Tên Class lớn + 1 dòng tóm tắt chỉ số cơ bản (HP, MP, ATK, DEF, Tốc chạy, Chí mạng)
            GameObject headerBox = CreateUIGameObject("HeaderInfoBox", rightObj.transform);
            var headerLE = headerBox.AddComponent<LayoutElement>();
            headerLE.minHeight = 72;
            headerLE.preferredHeight = 72;
            headerLE.flexibleWidth = 1;

            classNameText = CreateTMP(headerBox.transform, "ClassName", "Pháp Sư Nguyên Tố (Elemental Archmage)", 19, TextAlignmentOptions.TopLeft, FontManager.GoldTitleColor);
            SetTopStretch(classNameText.rectTransform, 0, 0, 210, 26);
            classNameText.fontStyle = FontStyles.Bold;

            difficultyStarsText = CreateTMP(headerBox.transform, "Difficulty", "Độ khó: ★★★★★", 12, TextAlignmentOptions.TopRight, new Color(1f, 0.75f, 0.25f));
            difficultyStarsText.rectTransform.anchorMin = new Vector2(1, 1);
            difficultyStarsText.rectTransform.anchorMax = new Vector2(1, 1);
            difficultyStarsText.rectTransform.pivot = new Vector2(1, 1);
            difficultyStarsText.rectTransform.anchoredPosition = new Vector2(0, -2);
            difficultyStarsText.rectTransform.sizeDelta = new Vector2(205, 22);

            classTagText = CreateTMP(headerBox.transform, "ClassTag", "Chuyên Phái Lumina Tối Thượng (Tier 2) | Nhánh: Mage | Vai Trò: BurstDPS", 12, TextAlignmentOptions.TopLeft, new Color(0.55f, 0.85f, 1f));
            SetTopStretch(classTagText.rectTransform, 0, -26, 0, 20);

            statsOverviewText = CreateTMP(headerBox.transform, "StatsOverview", "HP: 100 | MP: 50 | ATK: 25 | DEF: 10 | Tốc chạy: 4.5m/s | Chí mạng: 10%", 13, TextAlignmentOptions.MidlineLeft, FontManager.IvoryTextColor);
            SetTopStretch(statsOverviewText.rectTransform, 0, -48, 0, 22);

            // 2. Khung "Cơ chế đặc trưng": Có lề trong Padding = 8px để chữ không dính sát viền
            GameObject mechBox = CreateUIGameObject("MechanicBox", rightObj.transform);
            var mechLE = mechBox.AddComponent<LayoutElement>();
            mechLE.minHeight = 96;
            mechLE.preferredHeight = 96;
            mechLE.flexibleWidth = 1;

            var mImg = mechBox.AddComponent<Image>();
            mImg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            mImg.type = Image.Type.Sliced;
            mImg.color = new Color(0.06f, 0.11f, 0.17f, 0.94f);
            var mOutline = mechBox.AddComponent<Outline>();
            mOutline.effectColor = FontManager.AetherCyanColor;
            mOutline.effectDistance = new Vector2(1, -1);

            mechanicTitleText = CreateTMP(mechBox.transform, "MechTitle", "⚙ CƠ CHẾ ĐẶC TRƯNG: ElementalQueue", 14, TextAlignmentOptions.TopLeft, FontManager.AetherCyanColor);
            SetTopStretch(mechanicTitleText.rectTransform, 8, -8, 8, 20);
            mechanicTitleText.fontStyle = FontStyles.Bold;

            mechanicDescText = CreateTMP(mechBox.transform, "MechDesc", "Mô tả chi tiết cách vận hành cơ chế...", 12, TextAlignmentOptions.TopLeft, FontManager.IvoryTextColor);
            mechanicDescText.enableWordWrapping = true;
            mechanicDescText.overflowMode = TextOverflowModes.Ellipsis;
            mechanicDescText.rectTransform.anchorMin = Vector2.zero;
            mechanicDescText.rectTransform.anchorMax = Vector2.one;
            mechanicDescText.rectTransform.offsetMin = new Vector2(8, 8);
            mechanicDescText.rectTransform.offsetMax = new Vector2(-8, -28);

            // 3. Cụm 3 ô Nội tại/Đặc tính: Dùng HorizontalLayoutGroup chia 3 cột bằng nhau, bật Word Wrapping
            GameObject sealsContainer = CreateUIGameObject("SealsContainer", rightObj.transform);
            var sealsLE = sealsContainer.AddComponent<LayoutElement>();
            sealsLE.minHeight = 104;
            sealsLE.preferredHeight = 104;
            sealsLE.flexibleWidth = 1;

            var sLayout = sealsContainer.AddComponent<HorizontalLayoutGroup>();
            sLayout.spacing = 8;
            sLayout.childControlWidth = true;
            sLayout.childControlHeight = true;
            sLayout.childForceExpandWidth = true;
            sLayout.childForceExpandHeight = true;

            CreateSealCard(sealsContainer.transform, 1, out seal1Title, out seal1Desc);
            CreateSealCard(sealsContainer.transform, 2, out seal2Title, out seal2Desc);
            CreateSealCard(sealsContainer.transform, 3, out seal3Title, out seal3Desc);

            // 4. Danh sách Kỹ năng ([1] đến [6]): Mỗi kỹ năng 1 dòng khung riêng, căn lề trái
            GameObject skillsSection = CreateUIGameObject("SkillsSection", rightObj.transform);
            var skillsLE = skillsSection.AddComponent<LayoutElement>();
            skillsLE.minHeight = 224;
            skillsLE.preferredHeight = 224;
            skillsLE.flexibleWidth = 1;

            var skillsHeader = CreateTMP(skillsSection.transform, "SkillsHeader", "BỘ KỸ NĂNG CHIẾN ĐẤU ([1] - [6]):", 13, TextAlignmentOptions.TopLeft, FontManager.GoldTitleColor);
            SetTopStretch(skillsHeader.rectTransform, 0, 0, 0, 20);
            skillsHeader.fontStyle = FontStyles.Bold;

            GameObject skillsArea = CreateUIGameObject("SkillsArea", skillsSection.transform);
            var sart = skillsArea.GetComponent<RectTransform>();
            sart.anchorMin = Vector2.zero;
            sart.anchorMax = Vector2.one;
            sart.offsetMin = Vector2.zero;
            sart.offsetMax = new Vector2(0, -22);

            var vLayout = skillsArea.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 4;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;
            skillsListContainer = skillsArea.transform;

            // 5. Cụm 2 nút hành động chính: [Áp Dụng Class Này Ngay] và [Thử Nghiệm Kỹ Năng Class] song song, bằng nhau
            GameObject actionArea = CreateUIGameObject("ActionButtons", rightObj.transform);
            var actionLE = actionArea.AddComponent<LayoutElement>();
            actionLE.minHeight = 42;
            actionLE.preferredHeight = 42;
            actionLE.flexibleWidth = 1;

            var aLayout = actionArea.AddComponent<HorizontalLayoutGroup>();
            aLayout.spacing = 12;
            aLayout.childControlWidth = true;
            aLayout.childControlHeight = true;
            aLayout.childForceExpandWidth = true;
            aLayout.childForceExpandHeight = true;

            applyClassButton = CreateActionButton(actionArea.transform, "Btn_Apply", "▶ ÁP DỤNG CLASS NÀY NGAY", new Color(0.14f, 0.52f, 0.26f, 0.96f), Color.white);
            testMechanicButton = CreateActionButton(actionArea.transform, "Btn_TestMechanic", "⚡ THỬ NGHIỆM KỸ NĂNG CLASS", new Color(0.12f, 0.42f, 0.68f, 0.96f), FontManager.GoldTitleColor);
            testMechanicButtonText = testMechanicButton.GetComponentInChildren<TMP_Text>();
        }

        private void CreateSealCard(Transform parent, int sealIndex, out TMP_Text title, out TMP_Text desc)
        {
            GameObject cardObj = CreateUIGameObject($"SealCard_{sealIndex}", parent);
            var le = cardObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.flexibleHeight = 1;

            var img = cardObj.AddComponent<Image>();
            img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            img.type = Image.Type.Sliced;
            img.color = FontManager.SlotEmptyBgColor;
            var outline = cardObj.AddComponent<Outline>();
            outline.effectColor = FontManager.BronzeGoldBorder;
            outline.effectDistance = new Vector2(1, -1);

            title = CreateTMP(cardObj.transform, "Title", $"Ấn {sealIndex}", 13, TextAlignmentOptions.TopLeft, FontManager.GoldTitleColor);
            SetTopStretch(title.rectTransform, 8, -6, 8, 20);
            title.fontStyle = FontStyles.Bold;
            title.enableWordWrapping = true;
            title.overflowMode = TextOverflowModes.Ellipsis;

            desc = CreateTMP(cardObj.transform, "Desc", "Mô tả ấn nội tại...", 11, TextAlignmentOptions.TopLeft, FontManager.IvoryTextColor);
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Ellipsis;
            desc.rectTransform.anchorMin = Vector2.zero;
            desc.rectTransform.anchorMax = Vector2.one;
            desc.rectTransform.offsetMin = new Vector2(8, 6);
            desc.rectTransform.offsetMax = new Vector2(-8, -26);
        }

        private Button CreateActionButton(Transform parent, string name, string label, Color bgColor, Color textColor)
        {
            GameObject btnObj = CreateUIGameObject(name, parent);
            var le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minHeight = 42;
            le.preferredHeight = 42;

            var img = btnObj.AddComponent<Image>();
            img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            img.type = Image.Type.Sliced;
            img.color = bgColor;
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = FontManager.BronzeGoldBorder;
            outline.effectDistance = new Vector2(1, -1);

            var btn = btnObj.AddComponent<Button>();
            var txt = CreateTMP(btnObj.transform, "Text", label, 14, TextAlignmentOptions.Center, textColor);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = new Vector2(6, 2);
            txt.rectTransform.offsetMax = new Vector2(-6, -2);
            txt.fontStyle = FontStyles.Bold;

            return btn;
        }

        private void CreateBottomToolbar(Transform parent)
        {
            GameObject barObj = CreateUIGameObject("DebugToolbar", parent);
            var rt = barObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 8);
            rt.sizeDelta = new Vector2(-24, 40);

            var img = barObj.AddComponent<Image>();
            img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            img.type = Image.Type.Sliced;
            img.color = FontManager.HeaderBgColor;
            var outline = barObj.AddComponent<Outline>();
            outline.effectColor = FontManager.BronzeGoldBorder;
            outline.effectDistance = new Vector2(1, -1);

            var layout = barObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(8, 8, 5, 5);

            btnSetLevel1 = CreateToolButton(barObj.transform, "Lv 1");
            btnSetLevel10 = CreateToolButton(barObj.transform, "Lv 10");
            btnSetLevel20 = CreateToolButton(barObj.transform, "Lv 20");
            btnSetLevel30 = CreateToolButton(barObj.transform, "Lv 30");
            btnAddLevel5 = CreateToolButton(barObj.transform, "+5 Cấp");
            btnFullHeal = CreateToolButton(barObj.transform, "Hồi Máu/MP");
            btnToggleInfiniteMana = CreateToolButton(barObj.transform, "Vô Hạn MP");
            btnAddItems = CreateToolButton(barObj.transform, "Thêm Đá/Vàng");
            btnSpawnEnemy = CreateToolButton(barObj.transform, "Spawn Quái");
        }

        private Button CreateToolButton(Transform parent, string label)
        {
            GameObject btnObj = CreateUIGameObject($"Btn_{label}", parent);
            var le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            var img = btnObj.AddComponent<Image>();
            img.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.14f, 0.20f, 0.28f, 0.95f);

            var btn = btnObj.AddComponent<Button>();
            var txt = CreateTMP(btnObj.transform, "Label", label, 12, TextAlignmentOptions.Center, FontManager.IvoryTextColor);
            txt.fontStyle = FontStyles.Bold;
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;

            return btn;
        }

        private TMP_Text CreateTMP(Transform parent, string name, string content, float size, TextAlignmentOptions align, Color color)
        {
            GameObject obj = CreateUIGameObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = color;
            ActionRPG.UI.FontManager.ApplyFont(tmp);
            return tmp;
        }
        #endregion
    }
}
