using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Items;
using ActionRPG.Characters;
using ActionRPG.Pets;

namespace ActionRPG.Town
{
    public enum ShopType
    {
        Blacksmith,         // Tiệm Rèn: Rèn và cường hóa trang bị
        FashionShop,        // Tiệm Thời Trang: Ngoại trang và diện mạo
        EquipmentDealer,    // Tiệm Bán Trang Bị & Vũ Khí
        JewelryShop,        // Tiệm Trang Sức: Nhẫn, dây chuyền
        AlchemyLab,         // Tiệm Thuốc & Giả Kim: Bình hồi phục, thuốc thuộc tính
        PetSanctuary,       // Tiệm Thú Cưng: Giấy Thu Phục, đá tiến hóa
        Restaurant,         // Nhà Hàng: Thực phẩm no bụng & Buff chỉ số
        AdventurerGuild     // Hiệp Hội Mạo Hiểm Giả: Bảng nhiệm vụ săn boss
    }

    public class ShopNPC : MonoBehaviour
    {
        [Header("Thông Tin Cửa Hàng")]
        public string npcName = "Chủ Tiệm Rèn";
        public ShopType shopType = ShopType.Blacksmith;
        [TextArea] public string greetingDialogue = "Chào mừng nhà thám hiểm đến với cửa tiệm của ta!";

        [Header("Mặt Hàng Bày Bán")]
        public List<ItemData> shopInventory = new List<ItemData>();

        public void Interact()
        {
            // Kiểm tra điều kiện riêng của Tiệm Thú Cưng
            if (shopType == ShopType.PetSanctuary && !PetCaptureSystem.IsPetFeatureUnlocked)
            {
                int reqLvl = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.petUnlockLevel : 10;
                Debug.LogWarning($"<color=orange>[NPC {npcName} (Tiệm Thú Cưng)]: \"Chào mạo hiểm giả! Hiện tại ta chưa thể mở bán Giấy Thu Phục cho ngươi. Hãy đạt Cấp {reqLvl} để chính thức mở khóa tính năng Thú Cưng nhé!\"</color>");
                return;
            }

            Debug.Log($"<color=yellow>[NPC {npcName} ({shopType})]: \"{greetingDialogue}\"</color>");

            // Mở giao diện cửa hàng tương ứng
            if (shopType == ShopType.Restaurant)
            {
                OpenRestaurantMenu();
            }
            else
            {
                OpenStandardShop();
            }
        }

        public List<ItemData> GetAvailableShopInventory()
        {
            // Nếu chưa mở khóa Pet, ẩn các loại Giấy Thu Phục khỏi danh mục bán
            if (!PetCaptureSystem.IsPetFeatureUnlocked)
            {
                return shopInventory.FindAll(item => item != null && item.itemType != ItemType.CaptureScroll && !item.itemId.Contains("capture_scroll"));
            }
            return shopInventory;
        }

        private void OpenStandardShop()
        {
            var items = GetAvailableShopInventory();
            Debug.Log($"[Shop] Đang mở danh mục cửa hàng {npcName} ({items.Count} mặt hàng sẵn sàng giao dịch).");
        }

        private void OpenRestaurantMenu()
        {
            Debug.Log("[Nhà Hàng] Đang phục vụ các món ăn giải trừ cơn đói và tăng cường thể lực!");
        }

        public bool BuyItem(ItemData item)
        {
            if (item == null) return false;

            // Chặn mua Giấy Thu Phục nếu chưa đạt Cấp 15 và chuyển Class cấp cao
            if (item.itemType == ItemType.CaptureScroll || item.itemId.Contains("capture_scroll"))
            {
                if (!PetCaptureSystem.IsPetFeatureUnlocked)
                {
                    int reqLvl = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.petUnlockLevel : 10;
                    Debug.LogWarning($"<color=orange>[{npcName}]: \"Ngươi chưa đủ tư cách sử dụng Giấy Thu Phục! Hãy đạt Cấp {reqLvl} để mở khóa tính năng Thú Cưng rồi quay lại!\"</color>");
                    return false;
                }
            }

            if (Inventory.Instance != null && Inventory.Instance.SpendGold(item.buyPrice))
            {
                Inventory.Instance.AddItem(item, 1);
                Debug.Log($"[Cửa Hàng] Đã mua thành công: {item.itemName} (-{item.buyPrice} vàng)!");
                return true;
            }
            Debug.LogWarning("[Cửa Hàng] Bạn không đủ tiền vàng để mua món đồ này!");
            return false;
        }
    }
}
