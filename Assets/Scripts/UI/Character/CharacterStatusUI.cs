using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Core;
using ActionRPG.Items;

namespace ActionRPG.UI
{
    /// <summary>
    /// Bảng Chỉ Số Nhân Vật (Mở bằng phím [U] hoặc nút Nhân vật ở góc dưới bên trái).
    /// Tự động sinh giao diện Procedural UI hoàn chỉnh, phong cách Pixel Art nền tối viền vàng đồng.
    /// Hiển thị thời gian thực: Cấp độ, EXP, HP, MP, ATK, DEF, Tốc độ, Chí mạng.
    /// Hỗ trợ đầy đủ: Mặc / Tháo / Đổi 4 ô Trang Bị theo đúng quy tắc nhánh (Gậy cho Pháp Sư, Cung cho Xạ Thủ, Kiếm cho Cận Chiến).
    /// </summary>
    public class CharacterStatusUI : MonoBehaviour
    {
        public static CharacterStatusUI Instance { get; private set; }

        [Header("Root Panel")]
        public GameObject panelRoot;
        public Button closeButton;

        [Header("Thông Tin Cơ Bản")]
        public Image avatarImage;
        public TMP_Text characterNameText;
        public TMP_Text classNameText;
        public TMP_Text levelText;
        public TMP_Text expText;
        public Image expBarFill;

        [Header("Chỉ Số Chiến Đấu Thời Gian Thực")]
        public TMP_Text hpValueText;
        public TMP_Text mpValueText;
        public TMP_Text atkValueText;
        public TMP_Text defValueText;
        public TMP_Text speedValueText;
        public TMP_Text critRateValueText;

        [Header("4 Ô Trang Bị Hiện Tại")]
        public Image weaponSlotIcon;
        public TMP_Text weaponSlotName;
        public TMP_Text weaponSlotSubText;
        public Button weaponSlotButton;

        public Image armorSlotIcon;
        public TMP_Text armorSlotName;
        public TMP_Text armorSlotSubText;
        public Button armorSlotButton;

        public Image helmetSlotIcon;
        public TMP_Text helmetSlotName;
        public TMP_Text helmetSlotSubText;
        public Button helmetSlotButton;

        public Image bootsSlotIcon;
        public TMP_Text bootsSlotName;
        public TMP_Text bootsSlotSubText;
        public Button bootsSlotButton;

        // Dữ liệu trang bị đang mặc trên người
        public BagItemSlotData equippedWeapon;
        public BagItemSlotData equippedArmor;
        public BagItemSlotData equippedHelmet;
        public BagItemSlotData equippedBoots;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureUI(transform.parent != null ? transform.parent : transform);
            if (panelRoot != null)
            {
                panelRoot.SetActive(false); // Mặc định ẩn khi vào game
            }
        }

        private void OnEnable()
        {
            EventBus.OnClassApplied += HandleClassApplied;
            EventBus.OnClassChanged += HandleClassChanged;
        }

        private void OnDisable()
        {
            EventBus.OnClassApplied -= HandleClassApplied;
            EventBus.OnClassChanged -= HandleClassChanged;
        }

        private void Start()
        {
            InitDefaultEquipment();
            RefreshUI();
        }

        private void Update()
        {
            // Bấm phím [U] hoặc [C] (khi không đứng cạnh quái cần bắt) để Bật / Tắt Bảng chỉ số nhân vật
            if (Input.GetKeyDown(KeyCode.U))
            {
                Toggle();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (!HasCapturableCreatureNearby())
                {
                    Toggle();
                }
            }

            // Khi bảng đang mở, tự động cập nhật liên tục các chỉ số thời gian thực
            if (panelRoot != null && panelRoot.activeSelf)
            {
                RefreshUI();
            }
        }

        private static bool HasCapturableCreatureNearby()
        {
            var player = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.transform : null;
            if (player == null) return false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, 3.0f);
            foreach (var hit in hits)
            {
                if (hit == null || hit.transform == player) continue;
                var eStats = hit.GetComponent<ActionRPG.Enemies.EnemyStats>() ?? hit.GetComponentInParent<ActionRPG.Enemies.EnemyStats>();
                if (eStats != null && eStats.currentHp > 0 && (eStats.currentHp / Mathf.Max(1f, eStats.maxHp)) <= 0.35f)
                {
                    return true;
                }
            }
            return false;
        }

        public void Toggle()
        {
            if (panelRoot == null)
            {
                EnsureUI(transform.parent != null ? transform.parent : transform);
            }
            if (panelRoot != null)
            {
                bool newState = !panelRoot.activeSelf;
                panelRoot.SetActive(newState);
                Debug.Log($"<color=green>[CharacterStatusUI] Toggled panel to {newState}</color>");
                if (newState)
                {
                    RefreshUI();
                    panelRoot.transform.SetAsLastSibling(); // Đưa lên lớp hiển thị trên cùng
                }
            }
            else
            {
                Debug.LogError("[CharacterStatusUI] panelRoot is null in Toggle!");
            }
        }

        public void Open()
        {
            if (panelRoot == null)
            {
                EnsureUI(transform.parent != null ? transform.parent : transform);
            }
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                Debug.Log("<color=green>[CharacterStatusUI] Opened panel</color>");
                RefreshUI();
                panelRoot.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogError("[CharacterStatusUI] panelRoot is null in Open!");
            }
        }

        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void HandleClassApplied(CharacterClassData classData)
        {
            if (classData != null)
            {
                AutoAdaptWeaponForBranch(classData.branch);
                RefreshUI();
            }
        }

        private void HandleClassChanged(string newClassName)
        {
            var cs = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
            if (cs != null && cs.currentClass != null)
            {
                AutoAdaptWeaponForBranch(cs.currentClass.branch);
                RefreshUI();
            }
        }

        /// <summary>
        /// Tự động đổi hoặc tháo vũ khí khi chuyển sang nhánh hệ phái khác nếu vũ khí cũ không còn hợp lệ.
        /// </summary>
        public void AutoAdaptWeaponForBranch(ClassBranch newBranch)
        {
            if (equippedWeapon != null && !string.IsNullOrEmpty(equippedWeapon.itemId))
            {
                // Kiểm tra xem vũ khí hiện tại có dùng được cho nhánh mới không
                if (!InventoryBagUI.CanEquipWeaponForBranch(equippedWeapon.weaponType, newBranch, out _))
                {
                    // Cất vũ khí cũ vào Balo
                    var bagUI = InventoryBagUI.Instance != null ? InventoryBagUI.Instance : FindObjectOfType<InventoryBagUI>();
                    if (bagUI != null)
                    {
                        bagUI.AddItemToBag(equippedWeapon);
                    }
                    equippedWeapon = null;
                }
            }

            // Nếu ô vũ khí trống, trang bị vũ khí mặc định cho nhánh mới
            if (equippedWeapon == null || string.IsNullOrEmpty(equippedWeapon.itemId))
            {
                equippedWeapon = CreateDefaultWeaponForBranch(newBranch);
                SyncEquipmentToInventory(equippedWeapon);
            }

            EventBus.OnEquipmentChanged?.Invoke();
            EventBus.OnStatsChanged?.Invoke();
        }

        private BagItemSlotData CreateDefaultWeaponForBranch(ClassBranch branch)
        {
            switch (branch)
            {
                case ClassBranch.Mage:
                    return new BagItemSlotData
                    {
                        itemId = "weapon_mage_staff",
                        itemName = "Gậy Pháp Sư",
                        quantity = 1,
                        spritePath = "Assets/Sprites/Items/weapon_mage_staff.png",
                        isEquipment = true,
                        equipSlot = EquipSlot.Weapon,
                        weaponType = WeaponType.Staff,
                        bonusAtk = 10,
                        bonusMp = 30
                    };

                case ClassBranch.Archer:
                    return new BagItemSlotData
                    {
                        itemId = "weapon_archer_bow",
                        itemName = "Cung Săn Rừng",
                        quantity = 1,
                        spritePath = "Assets/Sprites/Items/weapon_archer_bow.png",
                        isEquipment = true,
                        equipSlot = EquipSlot.Weapon,
                        weaponType = WeaponType.Bow,
                        bonusAtk = 10,
                        bonusSpeed = 0.5f
                    };

                default: // Novice & Warrior (Cận chiến / Tân thủ)
                    return new BagItemSlotData
                    {
                        itemId = "weapon_starter_sword",
                        itemName = "Kiếm Sắt Tân Thủ",
                        quantity = 1,
                        spritePath = "Assets/Sprites/Items/icon_weapon_starter_sword.png",
                        isEquipment = true,
                        equipSlot = EquipSlot.Weapon,
                        weaponType = WeaponType.Sword,
                        bonusAtk = 12
                    };
            }
        }

        /// <summary>
        /// Khởi tạo trang bị mặc định ban đầu phù hợp với nhánh hệ phái hiện tại
        /// </summary>
        public void InitDefaultEquipment()
        {
            ClassBranch currentBranch = ClassBranch.Novice;
            var cs = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
            if (cs != null && cs.currentClass != null) currentBranch = cs.currentClass.branch;

            if (equippedWeapon == null || string.IsNullOrEmpty(equippedWeapon.itemId))
            {
                equippedWeapon = CreateDefaultWeaponForBranch(currentBranch);
                SyncEquipmentToInventory(equippedWeapon);
            }

            if (equippedArmor == null || string.IsNullOrEmpty(equippedArmor.itemId))
            {
                equippedArmor = new BagItemSlotData
                {
                    itemId = "armor_leather_novice",
                    itemName = "Giáp Da Tân Thủ",
                    quantity = 1,
                    spritePath = "Assets/Sprites/Items/icon_armor_leather_novice.png",
                    isEquipment = true,
                    equipSlot = EquipSlot.Armor,
                    bonusDef = 8,
                    bonusHp = 30
                };
                SyncEquipmentToInventory(equippedArmor);
            }

            if (equippedHelmet == null || string.IsNullOrEmpty(equippedHelmet.itemId))
            {
                equippedHelmet = new BagItemSlotData
                {
                    itemId = "helmet_leather_novice",
                    itemName = "Mũ Da Tân Thủ",
                    quantity = 1,
                    spritePath = "Assets/Sprites/Items/icon_helmet_leather_novice.png",
                    isEquipment = true,
                    equipSlot = EquipSlot.Helmet,
                    bonusDef = 5
                };
                SyncEquipmentToInventory(equippedHelmet);
            }

            if (equippedBoots == null || string.IsNullOrEmpty(equippedBoots.itemId))
            {
                equippedBoots = new BagItemSlotData
                {
                    itemId = "boots_leather_novice",
                    itemName = "Giày Da Tân Thủ",
                    quantity = 1,
                    spritePath = "Assets/Sprites/Items/icon_boots_leather_novice.png",
                    isEquipment = true,
                    equipSlot = EquipSlot.Boots,
                    bonusSpeed = 1.0f,
                    bonusDef = 3
                };
                SyncEquipmentToInventory(equippedBoots);
            }

            EventBus.OnEquipmentChanged?.Invoke();
            EventBus.OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// Trang bị một món đồ từ Balo lên người nhân vật. Kiểm tra điều kiện nhánh hệ phái.
        /// </summary>
        public BagItemSlotData EquipItem(BagItemSlotData newItem)
        {
            if (newItem == null || !newItem.isEquipment) return null;

            // Kiểm tra ràng buộc nhánh vũ khí
            if (newItem.equipSlot == EquipSlot.Weapon)
            {
                ClassBranch currentBranch = ClassBranch.Novice;
                var cs = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
                if (cs != null && cs.currentClass != null) currentBranch = cs.currentClass.branch;

                if (!InventoryBagUI.CanEquipWeaponForBranch(newItem.weaponType, currentBranch, out string rejectReason))
                {
                    EventBus.OnShowUIMessage?.Invoke(rejectReason);
                    EventBus.OnPlaySound?.Invoke("error");
                    return null;
                }
            }

            BagItemSlotData oldItem = null;

            switch (newItem.equipSlot)
            {
                case EquipSlot.Weapon:
                    oldItem = equippedWeapon;
                    equippedWeapon = newItem;
                    break;
                case EquipSlot.Armor:
                    oldItem = equippedArmor;
                    equippedArmor = newItem;
                    break;
                case EquipSlot.Helmet:
                    oldItem = equippedHelmet;
                    equippedHelmet = newItem;
                    break;
                case EquipSlot.Boots:
                    oldItem = equippedBoots;
                    equippedBoots = newItem;
                    break;
            }

            SyncEquipmentToInventory(newItem);

            EventBus.OnEquipmentChanged?.Invoke();
            EventBus.OnStatsChanged?.Invoke();
            RefreshUI();

            return oldItem;
        }

        /// <summary>
        /// Tháo trang bị tại một slot và chuyển vào Balo
        /// </summary>
        public void UnequipSlot(EquipSlot slot)
        {
            BagItemSlotData itemToUnequip = null;

            switch (slot)
            {
                case EquipSlot.Weapon:
                    itemToUnequip = equippedWeapon;
                    break;
                case EquipSlot.Armor:
                    itemToUnequip = equippedArmor;
                    break;
                case EquipSlot.Helmet:
                    itemToUnequip = equippedHelmet;
                    break;
                case EquipSlot.Boots:
                    itemToUnequip = equippedBoots;
                    break;
            }

            if (itemToUnequip == null || string.IsNullOrEmpty(itemToUnequip.itemId))
            {
                EventBus.OnShowUIMessage?.Invoke("Ô này đang trống! Hãy click trang bị trong Balo [I] để mặc.");
                return;
            }

            var bagUI = InventoryBagUI.Instance;
            if (bagUI == null) bagUI = FindObjectOfType<InventoryBagUI>();

            if (bagUI != null)
            {
                bool added = bagUI.AddItemToBag(itemToUnequip);
                if (!added) return; // Balo đầy

                switch (slot)
                {
                    case EquipSlot.Weapon:
                        equippedWeapon = null;
                        break;
                    case EquipSlot.Armor:
                        equippedArmor = null;
                        break;
                    case EquipSlot.Helmet:
                        equippedHelmet = null;
                        break;
                    case EquipSlot.Boots:
                        equippedBoots = null;
                        break;
                }

                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    Inventory.Instance.equippedItems.Remove(slot);
                }

                string msg = $"Đã tháo {itemToUnequip.itemName} cất vào Balo!";
                Debug.Log($"<color=yellow>[Unequip] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                EventBus.OnPlaySound?.Invoke("click");

                EventBus.OnEquipmentChanged?.Invoke();
                EventBus.OnStatsChanged?.Invoke();
                RefreshUI();
            }
        }

        private void SyncEquipmentToInventory(BagItemSlotData equipData)
        {
            if (equipData == null || !equipData.isEquipment) return;
            if (Inventory.Instance == null) return;
            if (Inventory.Instance.equippedItems == null)
            {
                Inventory.Instance.equippedItems = new Dictionary<EquipSlot, EquipmentData>();
            }

            // Tạo EquipmentData đại diện đồng bộ cho logic combat
            EquipmentData eq = ScriptableObject.CreateInstance<EquipmentData>();
            eq.itemId = equipData.itemId;
            eq.itemName = equipData.itemName;
            eq.equipSlot = equipData.equipSlot;
            eq.bonusAtk = equipData.bonusAtk;
            eq.bonusDef = equipData.bonusDef;
            eq.bonusSpeed = equipData.bonusSpeed;
            eq.bonusHp = equipData.bonusHp;
            eq.bonusMp = equipData.bonusMp;
            eq.bonusCritRate = equipData.bonusCritRate;

            if (equipData.iconSprite != null)
            {
                eq.visualSprite = equipData.iconSprite;
            }

            Inventory.Instance.equippedItems[equipData.equipSlot] = eq;
        }

        public void RefreshUI()
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            var classSys = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();

            // 1. Tên, Class, Avatar
            string cName = "Tân Thủ (Novice)";
            Sprite cIcon = null;

            if (classSys != null && classSys.currentClass != null)
            {
                cName = classSys.currentClass.className;
                cIcon = classSys.currentClass.classIcon;
            }

            if (characterNameText != null) characterNameText.text = "Hiệp Sĩ Lumina";
            if (classNameText != null) classNameText.text = cName;
            if (avatarImage != null && cIcon != null) avatarImage.sprite = cIcon;

            // 2. Level & EXP
            if (stats != null)
            {
                if (levelText != null) levelText.text = $"Lv. {stats.level}";
                if (expText != null) expText.text = $"EXP: {stats.currentExp} / {stats.maxExp}";
                if (expBarFill != null && stats.maxExp > 0)
                {
                    expBarFill.fillAmount = Mathf.Clamp01((float)stats.currentExp / stats.maxExp);
                }

                // Tính toán chỉ số cộng từ trang bị
                float totalAtk = stats.TotalAtk;
                float totalDef = stats.TotalDef;
                float totalSpeed = stats.TotalMoveSpeed;
                float totalCrit = Mathf.Max(0.15f, stats.TotalCritRate);

                // Cộng thêm từ 4 slot trang bị nếu chưa được tính trong stats
                int extraAtk = (equippedWeapon != null ? equippedWeapon.bonusAtk : 0);
                int extraDef = (equippedArmor != null ? equippedArmor.bonusDef : 0) + 
                               (equippedHelmet != null ? equippedHelmet.bonusDef : 0) + 
                               (equippedBoots != null ? equippedBoots.bonusDef : 0);
                float extraSpeed = (equippedBoots != null ? equippedBoots.bonusSpeed : 0f) + 
                                   (equippedWeapon != null ? equippedWeapon.bonusSpeed : 0f);

                // 3. Chỉ Số Chiến Đấu
                if (hpValueText != null) hpValueText.text = $"{Mathf.CeilToInt(stats.currentHp)} / {Mathf.CeilToInt(stats.maxHp)}";
                if (mpValueText != null) mpValueText.text = $"{Mathf.CeilToInt(stats.currentMp)} / {Mathf.CeilToInt(stats.maxMp)}";
                if (atkValueText != null) atkValueText.text = $"{Mathf.CeilToInt(totalAtk)} <color=#55FF77>(+{extraAtk})</color>";
                if (defValueText != null) defValueText.text = $"{Mathf.CeilToInt(totalDef)} <color=#55FF77>(+{extraDef})</color>";
                if (speedValueText != null) speedValueText.text = $"{totalSpeed:F1} <color=#55FF77>(+{extraSpeed:F1})</color>";
                if (critRateValueText != null) critRateValueText.text = $"{Mathf.RoundToInt(totalCrit * 100f)}%";
            }

            // 4. Cập nhật 4 ô trang bị
            RefreshSlotView(weaponSlotIcon, weaponSlotName, weaponSlotSubText, equippedWeapon, "Chưa có Vũ Khí");
            RefreshSlotView(armorSlotIcon, armorSlotName, armorSlotSubText, equippedArmor, "Chưa có Áo Giáp");
            RefreshSlotView(helmetSlotIcon, helmetSlotName, helmetSlotSubText, equippedHelmet, "Chưa có Mũ");
            RefreshSlotView(bootsSlotIcon, bootsSlotName, bootsSlotSubText, equippedBoots, "Chưa có Giày");
        }

        private void RefreshSlotView(Image icon, TMP_Text title, TMP_Text sub, BagItemSlotData data, string emptyLabel)
        {
            if (data != null && !string.IsNullOrEmpty(data.itemId))
            {
                if (data.iconSprite == null && !string.IsNullOrEmpty(data.spritePath))
                {
                    data.iconSprite = LoadSpriteAsset(data.spritePath);
                }

                if (icon != null)
                {
                    icon.sprite = data.iconSprite;
                    icon.color = Color.white;
                    icon.enabled = (data.iconSprite != null);
                }

                if (title != null)
                {
                    title.text = data.itemName;
                    title.color = new Color(1f, 0.9f, 0.4f); // Vàng kim
                }

                if (sub != null)
                {
                    string branchTag = "";
                    if (data.equipSlot == EquipSlot.Weapon)
                    {
                        if (data.weaponType == WeaponType.Sword) branchTag = " [Cận Chiến]";
                        else if (data.weaponType == WeaponType.Bow) branchTag = " [Xạ Thủ]";
                        else if (data.weaponType == WeaponType.Staff) branchTag = " [Pháp Sư]";
                    }

                    string bonus = data.bonusAtk > 0 ? $"+{data.bonusAtk} Sát Thương{branchTag}" :
                                  (data.bonusDef > 0 ? $"+{data.bonusDef} Phòng Thủ" :
                                  (data.bonusSpeed > 0 ? $"+{data.bonusSpeed:F1} Tốc Độ" : "Trang Bị"));
                    sub.text = bonus;
                    sub.color = new Color(0.4f, 1f, 0.6f); // Xanh lục ngọc
                }
            }
            else
            {
                if (icon != null)
                {
                    icon.sprite = null;
                    icon.enabled = false;
                }

                if (title != null)
                {
                    title.text = "[Trống]";
                    title.color = new Color(0.6f, 0.6f, 0.65f);
                }

                if (sub != null)
                {
                    sub.text = "Click Balo [I] để mặc";
                    sub.color = new Color(0.5f, 0.5f, 0.55f);
                }
            }
        }

        private static Sprite LoadSpriteAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_EDITOR
            Sprite assetSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (assetSpr != null) return assetSpr;
#endif

            // Fallback nạp Texture2D trực tiếp từ file PNG trên ổ cứng để luôn luôn hiện icon 100%
            if (System.IO.File.Exists(path))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Point; // Giữ nguyên độ sắc nét Pixel Art
                    if (tex.LoadImage(bytes))
                    {
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[LoadSpriteAsset] Không thể nạp ảnh từ {path}: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Tự động sinh hoặc nâng cấp cây phân cấp giao diện Procedural UI hoàn chỉnh.
        /// Neo ở cạnh trái màn hình (Middle-Left) nằm gọn giữa cụm HUD trên-trái và 4 nút menu dưới-trái.
        /// </summary>
        public static CharacterStatusUI EnsureUI(Transform parent)
        {
            if (parent == null) return Instance;

            Transform canvasTr = parent;
            if (canvasTr.name == "CharacterStatusUI_Panel" && canvasTr.parent != null)
            {
                canvasTr = canvasTr.parent;
            }

            CharacterStatusUI compInstance = canvasTr.GetComponent<CharacterStatusUI>();
            if (compInstance == null)
            {
                compInstance = Instance != null ? Instance : canvasTr.gameObject.AddComponent<CharacterStatusUI>();
            }
            Instance = compInstance;

            GameObject rootGo = null;
            Transform existing = canvasTr.Find("CharacterStatusUI_Panel");
            if (existing != null)
            {
                rootGo = existing.gameObject;
            }
            else
            {
                rootGo = new GameObject("CharacterStatusUI_Panel", typeof(RectTransform));
                rootGo.transform.SetParent(canvasTr, false);
            }

            compInstance.panelRoot = rootGo;

            // Xóa component CharacterStatusUI trùng lặp trên chính rootGo nếu có (để MonoBehaviour luôn chạy trên canvasTr)
            var duplicateOnPanel = rootGo.GetComponent<CharacterStatusUI>();
            if (duplicateOnPanel != null && duplicateOnPanel != compInstance)
            {
                if (Application.isPlaying) Destroy(duplicateOnPanel);
                else DestroyImmediate(duplicateOnPanel);
            }

            if (rootGo != null)
            {
                // Nếu đã nâng cấp lên chuẩn Layout v2 và đủ tham chiếu thì giữ nguyên
                if (rootGo.transform.Find("CombatStats_Container") != null && compInstance.hpValueText != null && compInstance.weaponSlotName != null)
                {
                    FontManager.SanitizeUIHierarchy(rootGo.transform);
                    return compInstance;
                }
                // Xóa các phần tử con cũ để dựng lại chuẩn Pixel-Perfect v2
                for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = rootGo.transform.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            compInstance.panelRoot = rootGo;

            // 1. Cấu hình Root Panel neo bên Trái (Middle-Left): X=[16..366], Y nằm gọn giữa Top-Left HUD và Bottom-Left Menu
            var rootRt = rootGo.GetComponent<RectTransform>() ?? rootGo.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0f, 0.5f);
            rootRt.anchorMax = new Vector2(0f, 0.5f);
            rootRt.pivot = new Vector2(0f, 0.5f);
            rootRt.anchoredPosition = new Vector2(16f, -18f);
            rootRt.sizeDelta = new Vector2(350f, 596f);

            var rootBg = rootGo.GetComponent<Image>() ?? rootGo.AddComponent<Image>();
            rootBg.sprite = FontManager.GetOrCreatePanelNineSliceSprite();
            rootBg.type = Image.Type.Sliced;
            rootBg.color = FontManager.PanelBgColor; // Xám đen 94%

            var rootOutline = rootGo.GetComponent<Outline>() ?? rootGo.AddComponent<Outline>();
            rootOutline.effectColor = FontManager.BronzeGoldBorder; // Viền vàng đồng
            rootOutline.effectDistance = new Vector2(2f, -2f);

            // 2. Header: Tiêu đề "BẢNG CHỈ SỐ NHÂN VẬT" căn giữa + Nút đóng [X] góc phải trên
            RectTransform headerRt = CreateRect("Header_Bar", rootRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(-12f, 40f));
            var headerBg = headerRt.gameObject.AddComponent<Image>();
            headerBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            headerBg.type = Image.Type.Sliced;
            headerBg.color = FontManager.HeaderBgColor;

            var headerTitle = CreateText("Title", headerRt, "BẢNG CHỈ SỐ NHÂN VẬT", 18f, FontStyles.Bold, TextAlignmentOptions.Center, FontManager.GoldTitleColor);
            headerTitle.rectTransform.anchorMin = Vector2.zero;
            headerTitle.rectTransform.anchorMax = Vector2.one;
            headerTitle.rectTransform.offsetMin = new Vector2(12f, 0f);
            headerTitle.rectTransform.offsetMax = new Vector2(-40f, 0f);

            // Nút đóng [X]
            RectTransform closeBtnRt = CreateRect("Btn_Close", headerRt, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(28f, 28f));
            var closeBg = closeBtnRt.gameObject.AddComponent<Image>();
            closeBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            closeBg.type = Image.Type.Sliced;
            closeBg.color = new Color(0.78f, 0.18f, 0.18f, 0.96f);
            var closeBtn = closeBtnRt.gameObject.AddComponent<Button>();
            var closeTxt = CreateText("X_Text", closeBtnRt, "X", 14f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            closeTxt.rectTransform.anchorMin = Vector2.zero;
            closeTxt.rectTransform.anchorMax = Vector2.one;
            closeTxt.rectTransform.offsetMin = closeTxt.rectTransform.offsetMax = Vector2.zero;
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() => compInstance.Close());
            compInstance.closeButton = closeBtn;

            // 3. Khung Nhân Vật (Avatar bên trái, Tên "Hiệp Sĩ Lumina", danh hiệu và thanh EXP bên phải)
            RectTransform infoBoxRt = CreateRect("Info_Box", rootRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(-20f, 86f));
            var infoBg = infoBoxRt.gameObject.AddComponent<Image>();
            infoBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            infoBg.type = Image.Type.Sliced;
            infoBg.color = FontManager.SectionBgColor;

            // Avatar Frame bên trái
            RectTransform portRt = CreateRect("Avatar_Frame", infoBoxRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(68f, 68f));
            var portBg = portRt.gameObject.AddComponent<Image>();
            portBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            portBg.type = Image.Type.Sliced;
            portBg.color = Color.white;
#if UNITY_EDITOR
            Sprite portFrameSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Frames/frame_portrait.png");
            if (portFrameSpr != null) { portBg.sprite = portFrameSpr; portBg.type = Image.Type.Sliced; }
#endif
            RectTransform avInnerRt = CreateRect("Avatar_Inner", portRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            avInnerRt.offsetMin = new Vector2(6f, 6f);
            avInnerRt.offsetMax = new Vector2(-6f, -6f);
            var avImg = avInnerRt.gameObject.AddComponent<Image>();
            avImg.color = Color.white;
            avImg.preserveAspect = true;
#if UNITY_EDITOR
            Sprite knightSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD/Icons/icon_character.png");
            if (knightSpr != null) avImg.sprite = knightSpr;
#endif
            compInstance.avatarImage = avImg;

            // Tên nhân vật, Cấp độ & Danh hiệu hệ phái bên phải Avatar
            compInstance.characterNameText = CreateText("Txt_CharName", infoBoxRt, "Hiệp Sĩ Lumina", 16f, FontStyles.Bold, TextAlignmentOptions.Left, FontManager.IvoryTextColor);
            compInstance.characterNameText.rectTransform.anchorMin = new Vector2(0f, 1f);
            compInstance.characterNameText.rectTransform.anchorMax = new Vector2(1f, 1f);
            compInstance.characterNameText.rectTransform.pivot = new Vector2(0f, 1f);
            compInstance.characterNameText.rectTransform.anchoredPosition = new Vector2(84f, -8f);
            compInstance.characterNameText.rectTransform.sizeDelta = new Vector2(-150f, 22f);

            compInstance.levelText = CreateText("Txt_Level", infoBoxRt, "Lv. 1", 14f, FontStyles.Bold, TextAlignmentOptions.Right, FontManager.GoldTitleColor);
            compInstance.levelText.rectTransform.anchorMin = new Vector2(1f, 1f);
            compInstance.levelText.rectTransform.anchorMax = new Vector2(1f, 1f);
            compInstance.levelText.rectTransform.pivot = new Vector2(1f, 1f);
            compInstance.levelText.rectTransform.anchoredPosition = new Vector2(-10f, -8f);
            compInstance.levelText.rectTransform.sizeDelta = new Vector2(64f, 22f);

            compInstance.classNameText = CreateText("Txt_ClassName", infoBoxRt, "Tân Thủ (Novice)", 13f, FontStyles.Bold, TextAlignmentOptions.Left, FontManager.AetherCyanColor);
            compInstance.classNameText.rectTransform.anchorMin = new Vector2(0f, 1f);
            compInstance.classNameText.rectTransform.anchorMax = new Vector2(1f, 1f);
            compInstance.classNameText.rectTransform.pivot = new Vector2(0f, 1f);
            compInstance.classNameText.rectTransform.anchoredPosition = new Vector2(84f, -32f);
            compInstance.classNameText.rectTransform.sizeDelta = new Vector2(-94f, 20f);

            // Thanh EXP bên phải dưới
            RectTransform expBarRt = CreateRect("EXP_Bar_Mini", infoBoxRt, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(84f, 10f), new Vector2(-94f, 18f));
            var expBg = expBarRt.gameObject.AddComponent<Image>();
            expBg.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);

            RectTransform expFillRt = CreateRect("EXP_Fill", expBarRt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero);
            expFillRt.offsetMin = new Vector2(2f, 2f);
            expFillRt.offsetMax = new Vector2(-2f, -2f);
            var expFill = expFillRt.gameObject.AddComponent<Image>();
            expFill.color = new Color(0.20f, 0.82f, 0.45f, 1f);
            expFill.type = Image.Type.Filled;
            expFill.fillMethod = Image.FillMethod.Horizontal;
            expFill.fillAmount = 0.35f;
            compInstance.expBarFill = expFill;

            compInstance.expText = CreateText("Txt_EXP", expBarRt, "EXP: 0 / 100", 11f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            compInstance.expText.rectTransform.anchorMin = Vector2.zero;
            compInstance.expText.rectTransform.anchorMax = Vector2.one;
            compInstance.expText.rectTransform.offsetMin = compInstance.expText.rectTransform.offsetMax = Vector2.zero;

            // 4. Phần CHỈ SỐ CHIẾN ĐẤU (Dùng VerticalLayoutGroup, Padding = 10px, Spacing = 6px)
            RectTransform combatHeader = CreateRect("Combat_Header", rootRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(12f, -146f), new Vector2(-24f, 22f));
            var combatTitle = CreateText("Combat_Title", combatHeader, "CHỈ SỐ CHIẾN ĐẤU", 14f, FontStyles.Bold, TextAlignmentOptions.Left, FontManager.GoldTitleColor);
            combatTitle.rectTransform.anchorMin = Vector2.zero;
            combatTitle.rectTransform.anchorMax = Vector2.one;
            combatTitle.rectTransform.offsetMin = combatTitle.rectTransform.offsetMax = Vector2.zero;

            RectTransform statsBoxRt = CreateRect("CombatStats_Container", rootRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(-20f, 212f));
            var statsBoxBg = statsBoxRt.gameObject.AddComponent<Image>();
            statsBoxBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            statsBoxBg.type = Image.Type.Sliced;
            statsBoxBg.color = FontManager.SectionBgColor;

            var vLayout = statsBoxRt.gameObject.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(10, 10, 8, 8);
            vLayout.spacing = 6f;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = true;

            compInstance.hpValueText = CreateStatRowLayout(statsBoxRt, "Row_HP", "Sinh Lực (HP)", "100 / 100", new Color(1f, 0.45f, 0.45f));
            compInstance.mpValueText = CreateStatRowLayout(statsBoxRt, "Row_MP", "Năng Lượng (MP)", "50 / 50", new Color(0.40f, 0.80f, 1f));
            compInstance.atkValueText = CreateStatRowLayout(statsBoxRt, "Row_ATK", "Sát Thương (ATK)", "15", FontManager.IvoryTextColor);
            compInstance.defValueText = CreateStatRowLayout(statsBoxRt, "Row_DEF", "Phòng Thủ (DEF)", "5", FontManager.IvoryTextColor);
            compInstance.speedValueText = CreateStatRowLayout(statsBoxRt, "Row_Speed", "Tốc Độ Di Chuyển", "4.5", FontManager.IvoryTextColor);
            compInstance.critRateValueText = CreateStatRowLayout(statsBoxRt, "Row_Crit", "Tỷ Lệ Chí Mạng", "15%", FontManager.GoldTitleColor);

            // 5. Phần TRANG BỊ HIỆN TẠI (Chia lưới 2x2 cân đối cho 4 ô: Vũ khí, Áo giáp, Mũ, Giày)
            RectTransform equipHeader = CreateRect("Equip_Header", rootRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(12f, -390f), new Vector2(-24f, 22f));
            var equipTitle = CreateText("Equip_Title", equipHeader, "TRANG BỊ (CLICK ĐỂ THÁO)", 14f, FontStyles.Bold, TextAlignmentOptions.Left, FontManager.GoldTitleColor);
            equipTitle.rectTransform.anchorMin = Vector2.zero;
            equipTitle.rectTransform.anchorMax = Vector2.one;
            equipTitle.rectTransform.offsetMin = equipTitle.rectTransform.offsetMax = Vector2.zero;

            RectTransform equipGridRt = CreateRect("Equip_Grid", rootRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -416f), new Vector2(330f, 168f));
            var layout = equipGridRt.gameObject.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.cellSize = new Vector2(156f, 76f);
            layout.spacing = new Vector2(10f, 8f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 2;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Slot 1: Vũ Khí
            compInstance.weaponSlotButton = CreateEquipSlot(equipGridRt, "Slot_Weapon", "Vũ Khí", "Kiếm Sắt Tân Thủ", "Assets/Sprites/Items/icon_weapon_starter_sword.png", 
                out compInstance.weaponSlotIcon, out compInstance.weaponSlotName, out compInstance.weaponSlotSubText, () => compInstance.UnequipSlot(EquipSlot.Weapon));

            // Slot 2: Áo Giáp
            compInstance.armorSlotButton = CreateEquipSlot(equipGridRt, "Slot_Armor", "Áo Giáp", "Giáp Da Tân Thủ", "Assets/Sprites/Items/icon_armor_leather_novice.png", 
                out compInstance.armorSlotIcon, out compInstance.armorSlotName, out compInstance.armorSlotSubText, () => compInstance.UnequipSlot(EquipSlot.Armor));

            // Slot 3: Mũ
            compInstance.helmetSlotButton = CreateEquipSlot(equipGridRt, "Slot_Helmet", "Mũ", "Mũ Da Tân Thủ", "Assets/Sprites/Items/icon_helmet_leather_novice.png", 
                out compInstance.helmetSlotIcon, out compInstance.helmetSlotName, out compInstance.helmetSlotSubText, () => compInstance.UnequipSlot(EquipSlot.Helmet));

            // Slot 4: Giày
            compInstance.bootsSlotButton = CreateEquipSlot(equipGridRt, "Slot_Boots", "Giày", "Giày Da Tân Thủ", "Assets/Sprites/Items/icon_boots_leather_novice.png", 
                out compInstance.bootsSlotIcon, out compInstance.bootsSlotName, out compInstance.bootsSlotSubText, () => compInstance.UnequipSlot(EquipSlot.Boots));

            FontManager.SanitizeUIHierarchy(rootGo.transform);
            compInstance.InitDefaultEquipment();
            compInstance.RefreshUI();
            return compInstance;
        }

        private static Button CreateEquipSlot(Transform parent, string name, string slotLabel, string defaultItemName, string iconPath, 
            out Image outIcon, out TMP_Text outName, out TMP_Text outSub, UnityEngine.Events.UnityAction onClickAction)
        {
            RectTransform slotRt = CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(156f, 76f));
            var slotBg = slotRt.gameObject.AddComponent<Image>();
            slotBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            slotBg.type = Image.Type.Sliced;
            slotBg.color = FontManager.SlotEmptyBgColor;

            var outline = slotRt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.65f, 0.52f, 0.24f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            var btn = slotRt.gameObject.AddComponent<Button>();
            if (onClickAction != null) btn.onClick.AddListener(onClickAction);

            // Frame icon vuông bên trái ô (44x44 px)
            RectTransform iconFrameRt = CreateRect("Icon_Frame", slotRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(44f, 44f));
            var iconBg = iconFrameRt.gameObject.AddComponent<Image>();
            iconBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            iconBg.type = Image.Type.Sliced;
            iconBg.color = new Color(0.07f, 0.08f, 0.11f, 1f);

            // Icon Image giữ nguyên tỷ lệ gốc
            RectTransform iconImgRt = CreateRect("Icon", iconFrameRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            iconImgRt.offsetMin = new Vector2(4f, 4f);
            iconImgRt.offsetMax = new Vector2(-4f, -4f);
            outIcon = iconImgRt.gameObject.AddComponent<Image>();
            outIcon.color = Color.white;
            outIcon.preserveAspect = true;
            Sprite itemSpr = LoadSpriteAsset(iconPath);
            if (itemSpr != null)
            {
                outIcon.sprite = itemSpr;
                outIcon.enabled = true;
            }
            else
            {
                outIcon.enabled = false;
            }

            // Cột chữ bên phải ô (X từ 54 đến -6): Nhãn Slot, Tên Trang Bị và Chỉ Số Phụ tự xuống dòng gọn trong khung
            var lbl = CreateText("SlotLabel", slotRt, slotLabel, 11f, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.72f, 0.78f, 0.88f));
            lbl.rectTransform.anchorMin = new Vector2(0f, 1f);
            lbl.rectTransform.anchorMax = new Vector2(1f, 1f);
            lbl.rectTransform.pivot = new Vector2(0f, 1f);
            lbl.rectTransform.anchoredPosition = new Vector2(54f, -4f);
            lbl.rectTransform.sizeDelta = new Vector2(-60f, 16f);

            outName = CreateText("ItemName", slotRt, defaultItemName, 11f, FontStyles.Bold, TextAlignmentOptions.TopLeft, FontManager.GoldTitleColor);
            outName.rectTransform.anchorMin = new Vector2(0f, 1f);
            outName.rectTransform.anchorMax = new Vector2(1f, 1f);
            outName.rectTransform.pivot = new Vector2(0f, 1f);
            outName.rectTransform.anchoredPosition = new Vector2(54f, -20f);
            outName.rectTransform.sizeDelta = new Vector2(-60f, 28f);
            outName.enableWordWrapping = true;
            outName.overflowMode = TextOverflowModes.Ellipsis;

            outSub = CreateText("SubText", slotRt, "+12 Sát Thương", 10f, FontStyles.Bold, TextAlignmentOptions.BottomLeft, new Color(0.35f, 1f, 0.55f));
            outSub.rectTransform.anchorMin = new Vector2(0f, 0f);
            outSub.rectTransform.anchorMax = new Vector2(1f, 0f);
            outSub.rectTransform.pivot = new Vector2(0f, 0f);
            outSub.rectTransform.anchoredPosition = new Vector2(54f, 4f);
            outSub.rectTransform.sizeDelta = new Vector2(-60f, 24f);
            outSub.enableWordWrapping = true;
            outSub.overflowMode = TextOverflowModes.Ellipsis;

            return btn;
        }

        private static TMP_Text CreateStatRowLayout(Transform parent, string name, string label, string value, Color valColor)
        {
            RectTransform rowRt = CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 26f));
            var rowBg = rowRt.gameObject.AddComponent<Image>();
            rowBg.color = new Color(0.06f, 0.07f, 0.10f, 0.65f);

            var le = rowRt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 26f;
            le.preferredHeight = 28f;

            var lblText = CreateText("Label", rowRt, label, 13f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, FontManager.IvoryTextColor);
            lblText.rectTransform.anchorMin = new Vector2(0f, 0f);
            lblText.rectTransform.anchorMax = new Vector2(0.58f, 1f);
            lblText.rectTransform.offsetMin = new Vector2(8f, 0f);
            lblText.rectTransform.offsetMax = Vector2.zero;

            var valText = CreateText("Value", rowRt, value, 13f, FontStyles.Bold, TextAlignmentOptions.MidlineRight, valColor);
            valText.rectTransform.anchorMin = new Vector2(0.58f, 0f);
            valText.rectTransform.anchorMax = new Vector2(1f, 1f);
            valText.rectTransform.offsetMin = Vector2.zero;
            valText.rectTransform.offsetMax = new Vector2(-8f, 0f);

            return valText;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            rt.sizeDelta = new Vector2(Mathf.Round(size.x), Mathf.Round(size.y));
            return rt;
        }

        private static TMP_Text CreateText(string name, Transform parent, string content, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = Mathf.Round(size);
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.extraPadding = true;
            tmp.raycastTarget = false;
            FontManager.ApplyFont(tmp);
            return tmp;
        }
    }
}
