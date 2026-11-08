using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Characters;

namespace ActionRPG.Items
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;

        public InventorySlot(ItemData item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; set; }

        [Header("Túi Đồ & Tiền Tệ")]
        [SerializeField] private int maxSlots = 30;
        public int gold = 150;
        public List<InventorySlot> slots = new List<InventorySlot>();

        [Header("Trang Bị Đang Mặc")]
        [System.NonSerialized] public Dictionary<EquipSlot, EquipmentData> equippedItems = new Dictionary<EquipSlot, EquipmentData>();

        public Action OnInventoryChanged;
        public Action OnEquipmentChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (equippedItems == null) equippedItems = new Dictionary<EquipSlot, EquipmentData>();
            GiveStarterItems();
        }

        public ItemData GetItemById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            foreach (var slot in slots)
            {
                if (slot.item != null && slot.item.itemId == itemId)
                {
                    return slot.item;
                }
            }

            return null;
        }

        public bool EnsureItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            if (HasItem(item.itemId, 1)) return true;
            return AddItem(item, quantity);
        }

        private void GiveStarterItems()
        {
            // Đầu game nhân vật KHÔNG THỂ có pet và không có sẵn Giấy Thu Phục.
            // Chỉ khi đạt Cấp 15 và hoàn tất chuyển đổi Class cấp cao hơn thì Cửa Hàng mới bắt đầu bán Giấy Thu Phục.
        }

        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // Kiểm tra nếu item có thể xếp chồng
            if (item.isStackable)
            {
                InventorySlot existingSlot = slots.Find(s => s.item.itemId == item.itemId && s.quantity < item.maxStack);
                if (existingSlot != null)
                {
                    int addable = Mathf.Min(quantity, item.maxStack - existingSlot.quantity);
                    existingSlot.quantity += addable;
                    quantity -= addable;

                    if (quantity <= 0)
                    {
                        NotifyChange();
                        return true;
                    }
                }
            }

            // Thêm vào slot mới nếu còn chỗ
            while (quantity > 0)
            {
                if (slots.Count >= maxSlots)
                {
                    Debug.LogWarning("[Inventory] Túi đồ đã đầy!");
                    NotifyChange();
                    return false;
                }

                int toAdd = item.isStackable ? Mathf.Min(quantity, item.maxStack) : 1;
                slots.Add(new InventorySlot(item, toAdd));
                quantity -= toAdd;
            }

            NotifyChange();
            return true;
        }

        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            InventorySlot slot = slots.Find(s => s.item.itemId == item.itemId);
            if (slot == null || slot.quantity < quantity) return false;

            slot.quantity -= quantity;
            if (slot.quantity <= 0)
            {
                slots.Remove(slot);
            }

            NotifyChange();
            return true;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                if (slot.item != null && slot.item.itemId == itemId)
                {
                    total += slot.quantity;
                    if (total >= quantity) return true;
                }
            }
            return false;
        }

        public void Equip(EquipmentData equip)
        {
            if (equip == null) return;

            // Kiểm tra điều kiện cấp độ & Tier từ CharacterStats (Sprint 3)
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : UnityEngine.Object.FindObjectOfType<CharacterStats>();
            if (stats != null)
            {
                if (stats.currentLevel < equip.requiredLevel)
                {
                    string errorMsg = $"Cần Cấp {equip.requiredLevel} để trang bị vật phẩm này!";
                    Debug.LogWarning($"<color=orange>[Inventory] {errorMsg}</color>");
                    EventBus.OnShowUIMessage?.Invoke(errorMsg);
                    EventBus.OnPlaySound?.Invoke("error");
                    return;
                }

                if (stats.CurrentTier < equip.requiredTier)
                {
                    string errorMsg = $"Cần đạt giai đoạn {equip.requiredTier} để trang bị vật phẩm này!";
                    Debug.LogWarning($"<color=orange>[Inventory] {errorMsg}</color>");
                    EventBus.OnShowUIMessage?.Invoke(errorMsg);
                    EventBus.OnPlaySound?.Invoke("error");
                    return;
                }

                if (!stats.CanEquip(equip, out string reason))
                {
                    Debug.LogWarning($"<color=orange>[Inventory] Không thể trang bị {equip.itemName}: {reason}</color>");
                    EventBus.OnShowUIMessage?.Invoke(reason);
                    EventBus.OnPlaySound?.Invoke("error");
                    return;
                }
            }

            // Tháo trang bị cũ cùng slot nếu có
            if (equippedItems.ContainsKey(equip.equipSlot))
            {
                Unequip(equip.equipSlot);
            }

            RemoveItem(equip, 1);
            equippedItems[equip.equipSlot] = equip;
            Debug.Log($"[Inventory] Đã trang bị: {equip.itemName} ({equip.equipSlot})");

            OnEquipmentChanged?.Invoke();
            NotifyChange();
        }

        public void Unequip(EquipSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out EquipmentData current))
            {
                if (slots.Count < maxSlots)
                {
                    equippedItems.Remove(slot);
                    AddItem(current, 1);
                    Debug.Log($"[Inventory] Đã gỡ trang bị: {current.itemName}");
                    OnEquipmentChanged?.Invoke();
                    NotifyChange();
                }
                else
                {
                    Debug.LogWarning("[Inventory] Không đủ chỗ trong túi để gỡ trang bị!");
                }
            }
        }

        public void AddGold(int amount)
        {
            gold += amount;
            NotifyChange();
        }

        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                NotifyChange();
                return true;
            }
            return false;
        }

        private void NotifyChange()
        {
            OnInventoryChanged?.Invoke();
            EventBus.OnInventoryUpdated?.Invoke();
        }
    }
}
