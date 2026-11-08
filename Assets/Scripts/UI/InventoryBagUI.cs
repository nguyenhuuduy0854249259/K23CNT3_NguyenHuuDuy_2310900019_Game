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
    public enum WeaponType
    {
        None = 0,
        Sword = 1,  // Kiếm -> Dành cho nhánh Cận Chiến (Tân Thủ & Chiến Binh)
        Bow = 2,    // Cung -> Chỉ dành cho nhánh Xạ Thủ
        Staff = 3   // Gậy Pháp -> Chỉ dành cho nhánh Pháp Sư
    }

    [Serializable]
    public class BagItemSlotData
    {
        public string itemId;
        public string itemName;
        public int quantity;
        public Sprite iconSprite;
        public string spritePath;

        // Vật phẩm tiêu hao (Bình thuốc hồi phục)
        public bool isConsumable;
        public int healHp;
        public int restoreMp;

        // Trang bị (Vũ khí, Giáp, Mũ, Giày)
        public bool isEquipment;
        public EquipSlot equipSlot;
        public WeaponType weaponType = WeaponType.None; // Phân loại vũ khí theo nhánh
        public int bonusAtk;
        public int bonusDef;
        public float bonusSpeed;
        public float bonusCritRate;
        public int bonusHp;
        public int bonusMp;
    }

    /// <summary>
    /// Bảng Balo / Túi Đồ 20 Ô (Mở bằng phím [I] hoặc click nút Túi Đồ [I] ở góc dưới bên trái).
    /// Tự động sinh giao diện Procedural UI hoàn chỉnh, phong cách Pixel Art nền tối viền vàng đồng.
    /// Hỗ trợ đầy đủ: Phân chia vũ khí theo nhánh (Gậy Pháp cho Pháp Sư, Cung cho Xạ Thủ, Kiếm cho nhánh còn lại).
    /// </summary>
    public class InventoryBagUI : MonoBehaviour
    {
        public static InventoryBagUI Instance { get; private set; }

        [Header("Root Panel")]
        public GameObject panelRoot;
        public Button closeButton;

        [Header("Lưới 20 Ô Hành Trang")]
        public Transform gridContainer;
        public List<BagItemSlotData> itemSlots = new List<BagItemSlotData>();

        [Header("Thanh Dưới Cùng (Bottom Bar)")]
        public TMP_Text goldText;
        public Button sortButton;

        // Quản lý UI Elements của từng slot trong 20 ô (SerializeField để không bị mất tham chiếu khi lưu Scene/Prefab)
        [SerializeField] public List<Image> slotIconImages = new List<Image>();
        [SerializeField] public List<TMP_Text> slotQuantityTexts = new List<TMP_Text>();
        [SerializeField] public List<Button> slotButtons = new List<Button>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureUI(transform.parent != null ? transform.parent : transform);
            if (panelRoot != null)
            {
                panelRoot.SetActive(false); // Mặc định ẩn khi vào game
            }
        }

        private void Start()
        {
            InitStarterItems();
            RefreshBagUI();
        }

        private void Update()
        {
            // Bấm phím [I] hoặc [B] để Bật / Tắt Balo Túi Đồ
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.B))
            {
                Toggle();
            }
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
                Debug.Log($"<color=green>[InventoryBagUI] Toggled panel to {newState}</color>");
                if (newState)
                {
                    RefreshBagUI();
                    panelRoot.transform.SetAsLastSibling(); // Đưa lên lớp hiển thị trên cùng
                }
            }
            else
            {
                Debug.LogError("[InventoryBagUI] panelRoot is null in Toggle!");
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
                Debug.Log("<color=green>[InventoryBagUI] Opened panel</color>");
                RefreshBagUI();
                panelRoot.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogError("[InventoryBagUI] panelRoot is null in Open!");
            }
        }

        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ khi trang bị vũ khí theo đúng nhánh hệ phái:
        /// - Gậy Pháp Sư: Chỉ sử dụng cho nhánh Pháp Sư (Mage branch).
        /// - Cung: Chỉ sử dụng cho nhánh Xạ Thủ (Archer branch).
        /// - Kiếm: Sử dụng cho các nhánh còn lại (Novice & Warrior branch).
        /// </summary>
        public static bool CanEquipWeaponForBranch(WeaponType weaponType, ClassBranch branch, out string rejectReason)
        {
            switch (weaponType)
            {
                case WeaponType.Staff:
                    if (branch != ClassBranch.Mage)
                    {
                        rejectReason = "Gậy Pháp Sư chỉ có thể sử dụng cho nhánh Pháp Sư!";
                        return false;
                    }
                    break;

                case WeaponType.Bow:
                    if (branch != ClassBranch.Archer)
                    {
                        rejectReason = "Cung chỉ có thể sử dụng cho nhánh Xạ Thủ!";
                        return false;
                    }
                    break;

                case WeaponType.Sword:
                    if (branch == ClassBranch.Mage || branch == ClassBranch.Archer)
                    {
                        rejectReason = "Kiếm chỉ có thể sử dụng cho nhánh Cận Chiến (Tân Thủ & Chiến Binh)!";
                        return false;
                    }
                    break;
            }

            rejectReason = string.Empty;
            return true;
        }

        /// <summary>
        /// Tạo sẵn dữ liệu vật phẩm và trang bị khởi đầu trong túi đồ:
        /// - Ô 1: Bình Máu Đỏ (x5)
        /// - Ô 2: Bình Mana Xanh (x5)
        /// - Ô 3: Thảo Mộc Xanh (x12)
        /// - Ô 4: Quặng Đá Tím (x8)
        /// - Ô 5: Quả Cầu Tinh Linh (x3)
        /// - Ô 6: Kiếm Sắt Tân Thủ [Kiếm - Nhánh Cận Chiến] (ATK +12)
        /// - Ô 7: Cung Săn Rừng [Cung - Nhánh Xạ Thủ] (ATK +10, Tốc độ +0.5)
        /// - Ô 8: Gậy Pháp Sư [Gậy - Nhánh Pháp Sư] (ATK +10, MP +30)
        /// - Ô 9: Giáp Da Thợ Săn [Áo Giáp] (DEF +8, HP +40)
        /// - Ô 10: Mũ Da Thợ Săn [Mũ] (DEF +5)
        /// - Ô 11: Giày Da Tân Thủ [Giày] (Tốc độ +1.0, DEF +3)
        /// - Ô 12 -> 20: Ô Trống
        /// </summary>
        public void InitStarterItems()
        {
            if (itemSlots.Count > 0) return; // Đã có dữ liệu

            itemSlots.Clear();

            // Ô 1: Bình Máu Đỏ (x5)
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "potion_hp_starter",
                itemName = "Bình Máu Đỏ",
                quantity = 5,
                spritePath = "Assets/Sprites/UI/HUD/Icons/icon_potion_hp.png",
                isConsumable = true,
                healHp = 50
            });

            // Ô 2: Bình Mana Xanh (x5)
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "potion_mp_starter",
                itemName = "Bình Mana Xanh",
                quantity = 5,
                spritePath = "Assets/Sprites/UI/HUD/Icons/icon_potion_mp.png",
                isConsumable = true,
                restoreMp = 40
            });

            // Ô 3: Thảo Mộc Xanh (x12)
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "mat_herb_leaf",
                itemName = "Thảo Mộc Xanh",
                quantity = 12,
                spritePath = "Assets/Sprites/Items/icon_material_herb_leaf.png",
                isConsumable = false
            });

            // Ô 4: Quặng Đá Tím (x8)
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "mat_purple_ore",
                itemName = "Quặng Đá Tím",
                quantity = 8,
                spritePath = "Assets/Sprites/Items/icon_material_ore_chunk.png",
                isConsumable = false
            });

            // Ô 5: Quả Cầu Tinh Linh (x3)
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "mat_spirit_orb",
                itemName = "Quả Cầu Tinh Linh",
                quantity = 3,
                spritePath = "Assets/Sprites/UI/HUD/Icons/icon_spirit_orb.png",
                isConsumable = false
            });

            // Ô 6: Kiếm Sắt Tân Thủ [Kiếm - Nhánh Cận Chiến]
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "weapon_starter_sword",
                itemName = "Kiếm Sắt Tân Thủ",
                quantity = 1,
                spritePath = "Assets/Sprites/Items/icon_weapon_starter_sword.png",
                isEquipment = true,
                equipSlot = EquipSlot.Weapon,
                weaponType = WeaponType.Sword,
                bonusAtk = 12
            });

            // Ô 7: Cung Săn Rừng [Cung - Nhánh Xạ Thủ]
            itemSlots.Add(new BagItemSlotData
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
            });

            // Ô 8: Gậy Pháp Sư [Gậy - Nhánh Pháp Sư]
            itemSlots.Add(new BagItemSlotData
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
            });

            // Ô 9: Giáp Da Thợ Săn [Áo Giáp]
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "armor_leather_hunter",
                itemName = "Giáp Da Thợ Săn",
                quantity = 1,
                spritePath = "Assets/Sprites/Items/icon_armor_leather_novice.png",
                isEquipment = true,
                equipSlot = EquipSlot.Armor,
                bonusDef = 8,
                bonusHp = 40
            });

            // Ô 10: Mũ Da Thợ Săn [Mũ]
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "helmet_leather_hunter",
                itemName = "Mũ Da Thợ Săn",
                quantity = 1,
                spritePath = "Assets/Sprites/Items/icon_helmet_leather_novice.png",
                isEquipment = true,
                equipSlot = EquipSlot.Helmet,
                bonusDef = 5
            });

            // Ô 11: Giày Da Tân Thủ [Giày]
            itemSlots.Add(new BagItemSlotData
            {
                itemId = "boots_leather_novice",
                itemName = "Giày Da Tân Thủ",
                quantity = 1,
                spritePath = "Assets/Sprites/Items/icon_boots_leather_novice.png",
                isEquipment = true,
                equipSlot = EquipSlot.Boots,
                bonusSpeed = 1.0f,
                bonusDef = 3
            });

            // Ô 12 -> 20: Ô trống
            while (itemSlots.Count < 20)
            {
                itemSlots.Add(new BagItemSlotData
                {
                    itemId = string.Empty,
                    itemName = string.Empty,
                    quantity = 0,
                    spritePath = string.Empty
                });
            }

            // Nạp Sprite tài nguyên
            foreach (var slot in itemSlots)
            {
                if (!string.IsNullOrEmpty(slot.spritePath))
                {
                    slot.iconSprite = LoadSpriteAsset(slot.spritePath);
                }
            }
        }

        public void RefreshBagUI()
        {
            if (itemSlots.Count == 0)
            {
                InitStarterItems();
            }

            EnsureSlotReferencesBound();

            // 1. Cập nhật 20 ô lưới (Trị dứt điểm lỗi ô trắng xóa)
            for (int i = 0; i < 20; i++)
            {
                if (i >= slotIconImages.Count || i >= slotQuantityTexts.Count) continue;

                var iconImg = slotIconImages[i];
                var qtyTxt = slotQuantityTexts[i];
                if (iconImg == null || qtyTxt == null) continue;

                // Đảm bảo khung nền của ô luôn có Sprite 9-Slice tối màu sắc nét
                if (iconImg.transform.parent != null)
                {
                    var slotBg = iconImg.transform.parent.GetComponent<Image>();
                    if (slotBg != null)
                    {
                        if (slotBg.sprite == null)
                        {
                            slotBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
                            slotBg.type = Image.Type.Sliced;
                        }
                        slotBg.color = FontManager.SlotEmptyBgColor;
                    }
                }

                var data = (i < itemSlots.Count) ? itemSlots[i] : null;

                if (data != null && !string.IsNullOrEmpty(data.itemId) && data.quantity > 0)
                {
                    if (data.iconSprite == null && !string.IsNullOrEmpty(data.spritePath))
                    {
                        data.iconSprite = LoadSpriteAsset(data.spritePath);
                    }

                    if (data.iconSprite != null)
                    {
                        iconImg.sprite = data.iconSprite;
                        iconImg.color = Color.white;
                        iconImg.preserveAspect = true;
                        iconImg.enabled = true;
                    }
                    else
                    {
                        iconImg.sprite = null;
                        iconImg.color = Color.clear;
                        iconImg.enabled = false;
                    }

                    // Phân biệt hiển thị nhãn số lượng / trang bị ở góc dưới phải của ô
                    if (data.isEquipment)
                    {
                        if (data.weaponType == WeaponType.Sword)
                        {
                            qtyTxt.text = "x1";
                            qtyTxt.color = new Color(1f, 0.88f, 0.35f);
                        }
                        else if (data.weaponType == WeaponType.Bow)
                        {
                            qtyTxt.text = "x1";
                            qtyTxt.color = new Color(0.45f, 1f, 0.55f);
                        }
                        else if (data.weaponType == WeaponType.Staff)
                        {
                            qtyTxt.text = "x1";
                            qtyTxt.color = new Color(0.45f, 0.85f, 1f);
                        }
                        else
                        {
                            qtyTxt.text = "x1";
                            qtyTxt.color = new Color(1f, 0.92f, 0.5f);
                        }
                    }
                    else
                    {
                        qtyTxt.text = $"x{data.quantity}";
                        qtyTxt.color = FontManager.IvoryTextColor;
                    }
                    qtyTxt.raycastTarget = false;
                    qtyTxt.enabled = true;
                }
                else
                {
                    // Ô trống: tắt hoàn toàn Icon và đặt Color.clear để không bao giờ hiện ô trắng xóa
                    iconImg.sprite = null;
                    iconImg.color = Color.clear;
                    iconImg.enabled = false;
                    qtyTxt.text = string.Empty;
                    qtyTxt.enabled = false;
                }
            }

            // 2. Cập nhật Vàng: 1,500 G
            int currentGold = 1500;
            if (Inventory.Instance != null && Inventory.Instance.gold > 0)
            {
                currentGold = Mathf.Max(1500, Inventory.Instance.gold);
            }
            if (goldText != null)
            {
                goldText.text = $"Vàng: {currentGold:N0} G";
            }
        }

        private void EnsureSlotReferencesBound()
        {
            if (gridContainer == null && panelRoot != null)
            {
                gridContainer = panelRoot.transform.Find("Grid_Container");
            }
            if (gridContainer == null) return;

            bool needsRebind = (slotIconImages == null || slotIconImages.Count < 20 || slotQuantityTexts == null || slotQuantityTexts.Count < 20);
            if (!needsRebind)
            {
                for (int i = 0; i < 20; i++)
                {
                    if (slotIconImages[i] == null || slotQuantityTexts[i] == null)
                    {
                        needsRebind = true;
                        break;
                    }
                }
            }

            if (needsRebind && gridContainer.childCount >= 20)
            {
                slotIconImages.Clear();
                slotQuantityTexts.Clear();
                slotButtons.Clear();

                for (int i = 0; i < 20; i++)
                {
                    int idx = i;
                    Transform slotTr = gridContainer.GetChild(i);
                    Transform iconTr = slotTr.Find("Icon");
                    Transform qtyTr = slotTr.Find("Qty_Text");

                    var iconImg = iconTr != null ? iconTr.GetComponent<Image>() : null;
                    var qtyTxt = qtyTr != null ? qtyTr.GetComponent<TMP_Text>() : null;
                    var btn = slotTr.GetComponent<Button>();

                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnSlotClicked(idx));
                    }

                    slotIconImages.Add(iconImg);
                    slotQuantityTexts.Add(qtyTxt);
                    slotButtons.Add(btn);
                }
            }
        }

        /// <summary>
        /// Xử lý khi click vào một ô trong Balo 20 Ô:
        /// - Nếu là Bình Máu / Bình Mana: Uống hồi phục tức thì.
        /// - Nếu là Trang Bị (Vũ Khí, Giáp, Mũ, Giày): Kiểm tra đúng nhánh hệ phái trước khi trang bị!
        /// </summary>
        private void OnSlotClicked(int index)
        {
            if (index < 0 || index >= itemSlots.Count) return;
            var data = itemSlots[index];
            if (data == null || string.IsNullOrEmpty(data.itemId) || data.quantity <= 0) return;

            // 1. Nếu là vật phẩm tiêu hao (Bình Máu / Mana)
            if (data.isConsumable)
            {
                var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
                if (data.healHp > 0 && stats != null)
                {
                    stats.HealHp(data.healHp);
                    data.quantity--;
                    EventBus.OnShowUIMessage?.Invoke($"Đã uống {data.itemName}! Hồi +{data.healHp} HP.");
                    EventBus.OnPlaySound?.Invoke("potion");
                }
                else if (data.restoreMp > 0 && stats != null)
                {
                    stats.RestoreMp(data.restoreMp);
                    data.quantity--;
                    EventBus.OnShowUIMessage?.Invoke($"Đã uống {data.itemName}! Hồi +{data.restoreMp} MP.");
                    EventBus.OnPlaySound?.Invoke("potion");
                }

                if (data.quantity <= 0)
                {
                    ClearSlot(index);
                }

                RefreshBagUI();
                return;
            }

            // 2. Nếu là Trang Bị (Vũ Khí, Áo Giáp, Mũ, Giày)
            if (data.isEquipment)
            {
                // Kiểm tra ràng buộc nhánh nếu là Vũ Khí
                if (data.equipSlot == EquipSlot.Weapon)
                {
                    ClassBranch currentBranch = ClassBranch.Novice;
                    var cs = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
                    if (cs != null && cs.currentClass != null) currentBranch = cs.currentClass.branch;

                    if (!CanEquipWeaponForBranch(data.weaponType, currentBranch, out string rejectReason))
                    {
                        Debug.LogWarning($"<color=orange>[Equip Reject] {rejectReason}</color>");
                        EventBus.OnShowUIMessage?.Invoke(rejectReason);
                        EventBus.OnPlaySound?.Invoke("error");
                        return;
                    }
                }

                EquipItemToCharacter(data, index);
                return;
            }

            // 3. Vật phẩm nguyên liệu thông thường
            string info = $"[{data.itemName}] (Số lượng: {data.quantity})";
            Debug.Log($"<color=yellow>[Balo] {info}</color>");
            EventBus.OnShowUIMessage?.Invoke(info);
        }

        /// <summary>
        /// Trang bị vật phẩm từ Balo lên người nhân vật và hoán đổi món cũ (nếu có)
        /// </summary>
        public void EquipItemToCharacter(BagItemSlotData itemToEquip, int bagIndex)
        {
            if (itemToEquip == null || !itemToEquip.isEquipment) return;

            var charUI = CharacterStatusUI.Instance;
            if (charUI == null) charUI = FindObjectOfType<CharacterStatusUI>();

            if (charUI != null)
            {
                // Thao tác trang bị và hoán đổi món cũ
                BagItemSlotData oldEquip = charUI.EquipItem(itemToEquip);

                if (oldEquip != null && !string.IsNullOrEmpty(oldEquip.itemId))
                {
                    // Đặt món cũ vào lại chính ô này trong Balo (Hoán đổi - Swap)
                    itemSlots[bagIndex] = oldEquip;
                }
                else
                {
                    // Ô này trong Balo trở thành ô trống
                    ClearSlot(bagIndex);
                }

                string statBonusInfo = itemToEquip.bonusAtk > 0 ? $" (ATK +{itemToEquip.bonusAtk})" : 
                                      (itemToEquip.bonusDef > 0 ? $" (DEF +{itemToEquip.bonusDef})" : 
                                      (itemToEquip.bonusSpeed > 0 ? $" (Tốc độ +{itemToEquip.bonusSpeed:F1})" : ""));
                string msg = $"Đã trang bị: {itemToEquip.itemName}{statBonusInfo}!";
                Debug.Log($"<color=green>[Equip] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                EventBus.OnPlaySound?.Invoke("equip");

                RefreshBagUI();
                charUI.RefreshUI();
            }
        }

        /// <summary>
        /// Tìm ô trống đầu tiên trong 20 ô của Balo để cất vật phẩm khi tháo trang bị
        /// </summary>
        public int FindFirstFreeSlotIndex()
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i] == null || string.IsNullOrEmpty(itemSlots[i].itemId) || itemSlots[i].quantity <= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Thêm một món trang bị được tháo từ người vào ô trống đầu tiên trong Balo
        /// </summary>
        public bool AddItemToBag(BagItemSlotData itemData)
        {
            int freeIdx = FindFirstFreeSlotIndex();
            if (freeIdx == -1)
            {
                EventBus.OnShowUIMessage?.Invoke("Túi đồ đã đầy, không thể tháo thêm!");
                return false;
            }

            itemSlots[freeIdx] = itemData;
            RefreshBagUI();
            return true;
        }

        private void ClearSlot(int index)
        {
            if (index < 0 || index >= itemSlots.Count) return;
            itemSlots[index] = new BagItemSlotData
            {
                itemId = string.Empty,
                itemName = string.Empty,
                quantity = 0,
                spritePath = string.Empty
            };
        }

        /// <summary>
        /// Nút [Sắp Xếp]: Dồn tất cả các ô có vật phẩm lên đầu và hiển thị thông báo.
        /// </summary>
        public void SortBagItems()
        {
            var validItems = new List<BagItemSlotData>();
            foreach (var slot in itemSlots)
            {
                if (slot != null && !string.IsNullOrEmpty(slot.itemId) && slot.quantity > 0)
                {
                    validItems.Add(slot);
                }
            }

            // Sắp xếp ưu tiên: Trang bị -> Tiêu hao -> Nguyên liệu
            validItems.Sort((a, b) =>
            {
                int typeA = a.isEquipment ? 0 : (a.isConsumable ? 1 : 2);
                int typeB = b.isEquipment ? 0 : (b.isConsumable ? 1 : 2);
                if (typeA != typeB) return typeA.CompareTo(typeB);
                return string.Compare(a.itemName, b.itemName, StringComparison.Ordinal);
            });

            itemSlots.Clear();
            itemSlots.AddRange(validItems);

            while (itemSlots.Count < 20)
            {
                itemSlots.Add(new BagItemSlotData
                {
                    itemId = string.Empty,
                    itemName = string.Empty,
                    quantity = 0,
                    spritePath = string.Empty
                });
            }

            RefreshBagUI();
            string msg = "Đã sắp xếp lại túi đồ gọn gàng!";
            Debug.Log($"<color=green>[InventoryBagUI] {msg}</color>");
            EventBus.OnShowUIMessage?.Invoke(msg);
            EventBus.OnPlaySound?.Invoke("click");
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
                catch (Exception ex)
                {
                    Debug.LogWarning($"[LoadSpriteAsset] Không thể nạp ảnh từ {path}: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Tự động sinh hoặc nâng cấp giao diện Procedural UI cho Balo 20 Ô (4 cột x 5 hàng)
        /// neo ở bên phải màn hình (Middle-Right), nằm gọn giữa Minimap và Thanh 6 ô kỹ năng.
        /// </summary>
        public static InventoryBagUI EnsureUI(Transform parent)
        {
            if (parent == null) return Instance;

            Transform canvasTr = parent;
            if (canvasTr.name == "InventoryBagUI_Panel" && canvasTr.parent != null)
            {
                canvasTr = canvasTr.parent;
            }

            InventoryBagUI compInstance = canvasTr.GetComponent<InventoryBagUI>();
            if (compInstance == null)
            {
                compInstance = Instance != null ? Instance : canvasTr.gameObject.AddComponent<InventoryBagUI>();
            }
            Instance = compInstance;

            GameObject rootGo = null;
            Transform existing = canvasTr.Find("InventoryBagUI_Panel");
            if (existing != null)
            {
                rootGo = existing.gameObject;
            }
            else
            {
                rootGo = new GameObject("InventoryBagUI_Panel", typeof(RectTransform));
                rootGo.transform.SetParent(canvasTr, false);
            }

            compInstance.panelRoot = rootGo;

            // Xóa component InventoryBagUI trùng lặp trên chính rootGo nếu có (để MonoBehaviour luôn chạy trên canvasTr)
            var duplicateOnPanel = rootGo.GetComponent<InventoryBagUI>();
            if (duplicateOnPanel != null && duplicateOnPanel != compInstance)
            {
                if (Application.isPlaying) Destroy(duplicateOnPanel);
                else DestroyImmediate(duplicateOnPanel);
            }

            if (rootGo != null)
            {
                // Kiểm tra xem Grid đã đúng chuẩn 4 cột x 5 hàng và đủ 20 slot chưa
                Transform existingGrid = rootGo.transform.Find("Grid_Container");
                GridLayoutGroup existingLayout = existingGrid != null ? existingGrid.GetComponent<GridLayoutGroup>() : null;
                if (existingLayout != null && existingLayout.constraintCount == 4 && existingGrid.childCount == 20 && compInstance.goldText != null)
                {
                    compInstance.gridContainer = existingGrid;
                    compInstance.EnsureSlotReferencesBound();
                    FontManager.SanitizeUIHierarchy(rootGo.transform);
                    compInstance.RefreshBagUI();
                    return compInstance;
                }

                // Xóa các phần tử con cũ (5x4) để dựng lại chuẩn 4 cột x 5 hàng (20 ô) không lỗi ô trắng
                for (int i = rootGo.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = rootGo.transform.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            compInstance.panelRoot = rootGo;

            // 1. Cấu hình Root Panel neo bên Phải màn hình (Middle-Right):
            // Đặt nằm gọn khoảng trống bên dưới Minimap (Y=-16..-212) và phía trên thanh 6 ô kỹ năng (Y=16..72)
            var rootRt = rootGo.GetComponent<RectTransform>() ?? rootGo.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(1f, 0.5f);
            rootRt.anchorMax = new Vector2(1f, 0.5f);
            rootRt.pivot = new Vector2(1f, 0.5f);
            rootRt.anchoredPosition = new Vector2(-16f, -24f);
            rootRt.sizeDelta = new Vector2(268f, 436f);

            var rootBg = rootGo.GetComponent<Image>() ?? rootGo.AddComponent<Image>();
            rootBg.sprite = FontManager.GetOrCreatePanelNineSliceSprite();
            rootBg.type = Image.Type.Sliced;
            rootBg.color = FontManager.PanelBgColor; // Xám đen 94%

            var rootOutline = rootGo.GetComponent<Outline>() ?? rootGo.AddComponent<Outline>();
            rootOutline.effectColor = FontManager.BronzeGoldBorder; // Viền vàng đồng
            rootOutline.effectDistance = new Vector2(2f, -2f);

            // 2. Header: Tiêu đề + Nút đóng [X]
            RectTransform headerRt = CreateRect("Header_Bar", rootRt, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(-12f, 40f));
            var headerBg = headerRt.gameObject.AddComponent<Image>();
            headerBg.sprite = FontManager.GetOrCreateSlotNineSliceSprite();
            headerBg.type = Image.Type.Sliced;
            headerBg.color = FontManager.HeaderBgColor;

            var headerTitle = CreateText("Title", headerRt, "BALO HÀNH TRANG (20 Ô)", 16f, FontStyles.Bold, TextAlignmentOptions.Center, FontManager.GoldTitleColor);
            headerTitle.rectTransform.anchorMin = Vector2.zero;
            headerTitle.rectTransform.anchorMax = Vector2.one;
            headerTitle.rectTransform.offsetMin = new Vector2(10f, 0f);
            headerTitle.rectTransform.offsetMax = new Vector2(-38f, 0f);

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

            // 3. Grid Container (Lưới vật phẩm: 4 cột x 5 hàng = 20 ô, kích thước 52x52 px, Spacing = 6px)
            RectTransform gridRt = CreateRect("Grid_Container", rootRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(246f, 306f));
            compInstance.gridContainer = gridRt;

            var layout = gridRt.gameObject.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.cellSize = new Vector2(52f, 52f);
            layout.spacing = new Vector2(6f, 6f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4; // 4 cột x 5 hàng = 20 ô chuẩn
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Sinh 20 ô vuông với Sprite 9-Slice nền tối đồng nhất, trị dứt điểm lỗi ô trắng
            compInstance.slotIconImages.Clear();
            compInstance.slotQuantityTexts.Clear();
            compInstance.slotButtons.Clear();

            Sprite slotFrameSprite = FontManager.GetOrCreateSlotNineSliceSprite();

            for (int i = 0; i < 20; i++)
            {
                int slotIndex = i;
                RectTransform slotBox = CreateRect($"Slot_{i + 1}", gridRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52f, 52f));
                
                var slotBg = slotBox.gameObject.AddComponent<Image>();
                slotBg.sprite = slotFrameSprite;
                slotBg.type = Image.Type.Sliced;
                slotBg.color = FontManager.SlotEmptyBgColor;

                var outline = slotBox.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.55f, 0.45f, 0.22f, 0.78f);
                outline.effectDistance = new Vector2(1f, -1f);

                // Icon Image ở giữa (giữ tỷ lệ gốc, tắt mặc định khi ô trống)
                RectTransform iconRt = CreateRect("Icon", slotBox, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                iconRt.offsetMin = new Vector2(6f, 6f);
                iconRt.offsetMax = new Vector2(-6f, -6f);
                var iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.color = Color.clear;
                iconImg.preserveAspect = true;
                iconImg.enabled = false;
                iconImg.raycastTarget = false;
                compInstance.slotIconImages.Add(iconImg);

                // Quantity Text ở góc dưới phải của ô, có viền đen mảnh dễ đọc
                RectTransform qtyRt = CreateRect("Qty_Text", slotBox, Vector2.zero, Vector2.one, new Vector2(1f, 0f), Vector2.zero, Vector2.zero);
                qtyRt.offsetMin = new Vector2(2f, 2f);
                qtyRt.offsetMax = new Vector2(-4f, -2f);
                var qtyTxt = qtyRt.gameObject.AddComponent<TextMeshProUGUI>();
                qtyTxt.text = "";
                qtyTxt.fontSize = 11f;
                qtyTxt.fontStyle = FontStyles.Bold;
                qtyTxt.alignment = TextAlignmentOptions.BottomRight;
                qtyTxt.color = FontManager.IvoryTextColor;
                qtyTxt.raycastTarget = false;
                FontManager.ApplyFont(qtyTxt);
                compInstance.slotQuantityTexts.Add(qtyTxt);

                // Button click
                var btn = slotBox.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => compInstance.OnSlotClicked(slotIndex));
                compInstance.slotButtons.Add(btn);
            }

            // 4. Footer bảng: Số Vàng ("Vàng: 1,500 G") căn trái, nút [Sắp Xếp] căn phải
            RectTransform btmRt = CreateRect("Bottom_Bar", rootRt, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(-20f, 42f));
            var btmBg = btmRt.gameObject.AddComponent<Image>();
            btmBg.sprite = slotFrameSprite;
            btmBg.type = Image.Type.Sliced;
            btmBg.color = FontManager.SectionBgColor;

            var btmOutline = btmRt.gameObject.AddComponent<Outline>();
            btmOutline.effectColor = new Color(0.60f, 0.48f, 0.22f, 0.7f);
            btmOutline.effectDistance = new Vector2(1f, -1f);

            // Text Vàng: 1,500 G căn trái
            compInstance.goldText = CreateText("Txt_Gold", btmRt, "Vàng: 1,500 G", 13f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, FontManager.GoldTitleColor);
            compInstance.goldText.rectTransform.anchorMin = new Vector2(0f, 0f);
            compInstance.goldText.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            compInstance.goldText.rectTransform.offsetMin = new Vector2(12f, 0f);
            compInstance.goldText.rectTransform.offsetMax = Vector2.zero;

            // Nút [Sắp Xếp] căn phải
            RectTransform sortBtnRt = CreateRect("Btn_Sort", btmRt, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(92f, 28f));
            var sortBg = sortBtnRt.gameObject.AddComponent<Image>();
            sortBg.sprite = FontManager.GetOrCreatePanelNineSliceSprite();
            sortBg.type = Image.Type.Sliced;
            sortBg.color = FontManager.AetherBlueColor;

            var sortOutline = sortBtnRt.gameObject.AddComponent<Outline>();
            sortOutline.effectColor = FontManager.BronzeGoldBorder;
            sortOutline.effectDistance = new Vector2(1f, -1f);

            var sortBtn = sortBtnRt.gameObject.AddComponent<Button>();
            var sortTxt = CreateText("Sort_Text", sortBtnRt, "Sắp Xếp", 12f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            sortTxt.rectTransform.anchorMin = Vector2.zero;
            sortTxt.rectTransform.anchorMax = Vector2.one;
            sortTxt.rectTransform.offsetMin = sortTxt.rectTransform.offsetMax = Vector2.zero;
            sortBtn.onClick.RemoveAllListeners();
            sortBtn.onClick.AddListener(() => compInstance.SortBagItems());
            compInstance.sortButton = sortBtn;

            FontManager.SanitizeUIHierarchy(rootGo.transform);
            compInstance.InitStarterItems();
            compInstance.RefreshBagUI();

            return compInstance;
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
