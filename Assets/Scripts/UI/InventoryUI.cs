using UnityEngine;
using UnityEngine.UI;
using ActionRPG.Items;
using ActionRPG.Characters;

namespace ActionRPG.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Giao Diện Túi Đồ")]
        public GameObject inventoryPanel;
        public Transform slotsContainer;
        public Text goldText;

        private void Start()
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnInventoryChanged += RefreshUI;
            }
        }

        private void OnDestroy()
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnInventoryChanged -= RefreshUI;
            }
        }

        private void Update()
        {
            if (InventoryBagUI.Instance != null) return;

            // Phím B hoặc I mở túi đồ
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }

        public void ToggleInventory()
        {
            if (inventoryPanel != null)
            {
                bool active = !inventoryPanel.activeSelf;
                inventoryPanel.SetActive(active);
                if (active) RefreshUI();
            }
            else if (InventoryBagUI.Instance != null)
            {
                InventoryBagUI.Instance.Toggle();
            }
        }

        public void RefreshUI()
        {
            if (Inventory.Instance == null || slotsContainer == null) return;

            if (goldText != null) goldText.text = $"{Inventory.Instance.gold} Vàng";

            // Xóa slot cũ
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }

            // Tạo các ô item
            foreach (var slot in Inventory.Instance.slots)
            {
                if (slot.item == null) continue;

                GameObject slotObj = new GameObject($"Slot_{slot.item.itemName}");
                slotObj.transform.SetParent(slotsContainer, false);

                Image bg = slotObj.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);

                // Viền màu theo độ hiếm
                Outline outline = slotObj.AddComponent<Outline>();
                outline.effectColor = slot.item.GetRarityColor();
                outline.effectDistance = new Vector2(2, -2);

                Button btn = slotObj.AddComponent<Button>();
                ItemData item = slot.item;
                btn.onClick.AddListener(() => OnItemClicked(item));

                // Text tên & số lượng
                GameObject textObj = new GameObject("ItemText");
                textObj.transform.SetParent(slotObj.transform, false);
                Text txt = textObj.AddComponent<Text>();
                txt.text = slot.quantity > 1 ? $"{item.itemName} x{slot.quantity}" : item.itemName;
                txt.font = ActionRPG.UI.FontManager.GetTimesNewRomanLegacy() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 14;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = item.GetRarityColor();
            }
        }

        private void OnItemClicked(ItemData item)
        {
            if (item == null) return;

            if (item is EquipmentData equip)
            {
                Inventory.Instance.Equip(equip);
            }
            else if (item is FoodItemData food)
            {
                if (HungerSystem.Instance != null)
                {
                    HungerSystem.Instance.EatFood(food);
                    Inventory.Instance.RemoveItem(food, 1);
                }
            }
        }
    }
}
