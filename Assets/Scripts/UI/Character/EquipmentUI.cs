using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Items;
using ActionRPG.Core;

namespace ActionRPG.UI.Character
{
    [Serializable]
    public class EquipSlotView
    {
        public EquipSlot slotType;
        public Image slotIcon;
        public Image itemIcon;
        public TMP_Text itemNameText;
        public Button unequipButton;
    }

    /// <summary>
    /// Giao diện trang bị nhân vật:
    /// - Quản lý và hiển thị các slot: Vũ Khí, Mũ, Áo Giáp, Găng Tay, Giày, Nhẫn, Dây Chuyền.
    /// - Hỗ trợ tháo trang bị, cập nhật chỉ số tức thì.
    /// </summary>
    public class EquipmentUI : MonoBehaviour
    {
        [Header("Danh Sách Các Slot Trang Bị")]
        public List<EquipSlotView> slotViews = new List<EquipSlotView>();

        private void Awake()
        {
            foreach (var view in slotViews)
            {
                if (view.unequipButton != null)
                {
                    EquipSlot slot = view.slotType;
                    view.unequipButton.onClick.AddListener(() => UnequipSlot(slot));
                }
            }
        }

        private void OnEnable()
        {
            if (Inventory.Instance != null)
                Inventory.Instance.OnEquipmentChanged += Refresh;

            EventBus.OnEquipmentChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (Inventory.Instance != null)
                Inventory.Instance.OnEquipmentChanged -= Refresh;

            EventBus.OnEquipmentChanged -= Refresh;
        }

        public void UnequipSlot(EquipSlot slot)
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.Unequip(slot);
                Refresh();
            }
        }

        public void Refresh()
        {
            if (Inventory.Instance == null) return;

            foreach (var view in slotViews)
            {
                if (Inventory.Instance.equippedItems.TryGetValue(view.slotType, out EquipmentData equip) && equip != null)
                {
                    if (view.itemIcon != null)
                    {
                        view.itemIcon.enabled = true;
                        view.itemIcon.sprite = equip.icon;
                    }
                    if (view.itemNameText != null)
                        view.itemNameText.text = equip.itemName;

                    if (view.unequipButton != null)
                        view.unequipButton.gameObject.SetActive(true);
                }
                else
                {
                    if (view.itemIcon != null)
                        view.itemIcon.enabled = false;

                    if (view.itemNameText != null)
                        view.itemNameText.text = $"Trống ({view.slotType})";

                    if (view.unequipButton != null)
                        view.unequipButton.gameObject.SetActive(false);
                }
            }
        }
    }
}
