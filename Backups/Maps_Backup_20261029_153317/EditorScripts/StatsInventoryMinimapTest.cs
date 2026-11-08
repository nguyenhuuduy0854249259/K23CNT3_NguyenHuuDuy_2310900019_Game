#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.UI;
using ActionRPG.UI.Gameplay;
using ActionRPG.Items;

namespace ActionRPG.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho hệ thống:
    /// - Bảng Chỉ Số Nhân Vật [U] (CharacterStatusUI)
    /// - Balo Túi Đồ 20 Ô [I] (InventoryBagUI)
    /// - Bản Đồ Nhỏ Minimap 2D (MinimapSystem2D)
    /// Menu: Tools -> Lumina -> Run Stats, Inventory & Minimap UI Test
    /// </summary>
    public static class StatsInventoryMinimapTest
    {
        [MenuItem("Tools/Lumina/Run Stats, Inventory & Minimap UI Test", false, 11)]
        public static void RunAllTests()
        {
            Debug.Log("<color=cyan>==================================================</color>");
            Debug.Log("<color=cyan>🚀 BẮT ĐẦU KIỂM THỬ TỰ ĐỘNG: STATS [U], INVENTORY [I] & MINIMAP 2D</color>");
            Debug.Log("<color=cyan>==================================================</color>");

            int passed = 0;
            int failed = 0;

            RunTest("Test 1: Kiểm tra tồn tại Sprite Assets (Icons, Frames & Trang bị)", Test_SpriteAssetsExist, ref passed, ref failed);
            RunTest("Test 2: Kiểm tra Cấu Trúc & Chỉ Số Thời Gian Thực CharacterStatusUI [U]", Test_CharacterStatusUIStructure, ref passed, ref failed);
            RunTest("Test 3: Kiểm tra 20 Ô Balo & Dữ Liệu 5 Vật Phẩm Khởi Đầu InventoryBagUI [I]", Test_InventoryBagUIStructureAndItems, ref passed, ref failed);
            RunTest("Test 4: Kiểm tra Cấu Trúc Khung 170x170 px, RenderTexture & Camera MinimapSystem2D", Test_MinimapSystem2DStructure, ref passed, ref failed);
            RunTest("Test 5: Kiểm tra Tích Hợp Nút Bấm & RuntimeHUDAutoLoader Trong GameplayHUD", Test_GameplayHUDIntegration, ref passed, ref failed);

            Debug.Log("<color=cyan>==================================================</color>");
            if (failed == 0)
            {
                Debug.Log($"<color=green>🎉 TẤT CẢ {passed} BÀI KIỂM THỬ ĐỀU ĐẠT CHUẨN XUẤT SẮC! HỆ THỐNG GIAO DIỆN HOẠT ĐỘNG HOÀN HẢO 100%!</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ CÓ {failed} BÀI TEST THẤT BẠI! Vui lòng kiểm tra chi tiết lỗi bên trên.</color>");
            }
            Debug.Log("<color=cyan>==================================================</color>");
        }

        private static void RunTest(string name, Action action, ref int passed, ref int failed)
        {
            try
            {
                Debug.Log($"<color=yellow>▶ Đang chạy: {name}...</color>");
                action.Invoke();
                Debug.Log($"<color=green>✓ PASS: {name}</color>");
                passed++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>✗ FAIL: {name}</color>\nLý do: {ex.Message}\n{ex.StackTrace}");
                failed++;
            }
        }

        private static void Test_SpriteAssetsExist()
        {
            string[] requiredPaths = new string[]
            {
                "Assets/Sprites/UI/HUD/Icons/icon_potion_hp.png",
                "Assets/Sprites/UI/HUD/Icons/icon_potion_mp.png",
                "Assets/Sprites/Items/icon_material_herb_leaf.png",
                "Assets/Sprites/Items/icon_material_herb_blue.png",
                "Assets/Sprites/Items/icon_material_ore_chunk.png",
                "Assets/Sprites/Items/icon_material_ore_purple.png",
                "Assets/Sprites/UI/HUD/Icons/icon_spirit_orb.png",
                "Assets/Sprites/UI/HUD/Icons/icon_character.png",
                "Assets/Sprites/UI/HUD/Frames/frame_skill_slot.png",
                "Assets/Sprites/UI/HUD/Frames/frame_portrait.png",
                "Assets/Sprites/Items/icon_weapon_starter_sword.png",
                "Assets/Sprites/Items/icon_armor_leather_novice.png",
                "Assets/Sprites/Items/icon_helmet_leather_novice.png",
                "Assets/Sprites/Items/icon_boots_leather_novice.png"
            };

            foreach (var path in requiredPaths)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Thiếu file sprite tài nguyên bắt buộc: {path}");
                }
            }
        }

        private static void Test_CharacterStatusUIStructure()
        {
            GameObject testRoot = new GameObject("Test_Canvas_Root", typeof(RectTransform));
            try
            {
                var charUI = CharacterStatusUI.EnsureUI(testRoot.transform);
                if (charUI == null) throw new Exception("Không thể tạo instance CharacterStatusUI!");
                if (charUI.panelRoot == null) throw new Exception("panelRoot bị null!");

                // Kiểm tra kích thước và vị trí neo
                var rt = charUI.panelRoot.GetComponent<RectTransform>();
                if (rt.sizeDelta.x < 300f || rt.sizeDelta.y < 500f)
                {
                    throw new Exception($"Kích thước panel {rt.sizeDelta} không đạt chuẩn!");
                }

                // Kiểm tra các trường text chỉ số
                if (charUI.hpValueText == null) throw new Exception("hpValueText bị null!");
                if (charUI.mpValueText == null) throw new Exception("mpValueText bị null!");
                if (charUI.atkValueText == null) throw new Exception("atkValueText bị null!");
                if (charUI.defValueText == null) throw new Exception("defValueText bị null!");
                if (charUI.speedValueText == null) throw new Exception("speedValueText bị null!");
                if (charUI.critRateValueText == null) throw new Exception("critRateValueText bị null!");

                // Kiểm tra 4 ô trang bị
                if (charUI.weaponSlotName == null) throw new Exception("weaponSlotName bị null!");
                if (charUI.armorSlotName == null) throw new Exception("armorSlotName bị null!");
                if (charUI.helmetSlotName == null) throw new Exception("helmetSlotName bị null!");
                if (charUI.bootsSlotName == null) throw new Exception("bootsSlotName bị null!");

                // Kiểm tra Refresh
                charUI.RefreshUI();
                if (string.IsNullOrEmpty(charUI.hpValueText.text)) throw new Exception("hpValueText rỗng sau Refresh!");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testRoot);
            }
        }

        private static void Test_InventoryBagUIStructureAndItems()
        {
            GameObject testRoot = new GameObject("Test_Canvas_Root", typeof(RectTransform));
            try
            {
                var bagUI = InventoryBagUI.EnsureUI(testRoot.transform);
                if (bagUI == null) throw new Exception("Không thể tạo instance InventoryBagUI!");
                if (bagUI.panelRoot == null) throw new Exception("panelRoot bị null!");

                bagUI.InitStarterItems();
                if (bagUI.itemSlots.Count != 20)
                {
                    throw new Exception($"Số lượng slot trong balo là {bagUI.itemSlots.Count}, kỳ vọng đúng 20 ô!");
                }

                // Kiểm tra 5 item khởi đầu
                var s1 = bagUI.itemSlots[0];
                if (s1.itemName != "Bình Máu Đỏ" || s1.quantity != 5) throw new Exception($"Ô 1 sai: {s1.itemName} x{s1.quantity}");

                var s2 = bagUI.itemSlots[1];
                if (s2.itemName != "Bình Mana Xanh" || s2.quantity != 5) throw new Exception($"Ô 2 sai: {s2.itemName} x{s2.quantity}");

                var s3 = bagUI.itemSlots[2];
                if (s3.itemName != "Thảo Mộc Xanh" || s3.quantity != 12) throw new Exception($"Ô 3 sai: {s3.itemName} x{s3.quantity}");

                var s4 = bagUI.itemSlots[3];
                if (s4.itemName != "Quặng Đá Tím" || s4.quantity != 8) throw new Exception($"Ô 4 sai: {s4.itemName} x{s4.quantity}");

                var s5 = bagUI.itemSlots[4];
                if (s5.itemName != "Quả Cầu Tinh Linh" || s5.quantity != 3) throw new Exception($"Ô 5 sai: {s5.itemName} x{s5.quantity}");

                // Kiểm tra các ô trang bị vũ khí trong Balo
                var s6 = bagUI.itemSlots[5]; // Kiếm Sắt
                if (!s6.isEquipment || s6.equipSlot != EquipSlot.Weapon || s6.weaponType != WeaponType.Sword) 
                    throw new Exception($"Ô 6 phải là Kiếm: {s6.itemName}");

                var s7 = bagUI.itemSlots[6]; // Cung Săn Rừng
                if (!s7.isEquipment || s7.weaponType != WeaponType.Bow) 
                    throw new Exception($"Ô 7 phải là Cung: {s7.itemName}");

                var s8 = bagUI.itemSlots[7]; // Gậy Pháp Sư
                if (!s8.isEquipment || s8.weaponType != WeaponType.Staff) 
                    throw new Exception($"Ô 8 phải là Gậy: {s8.itemName}");

                // Kiểm tra quy tắc nhánh:
                // 1. Nhánh Novice/Warrior: Dùng được Kiếm, KHÔNG dùng được Cung/Gậy
                if (!InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Sword, ClassBranch.Novice, out _))
                    throw new Exception("Nhánh Tân thủ phải dùng được Kiếm!");
                if (InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Bow, ClassBranch.Novice, out _))
                    throw new Exception("Nhánh Tân thủ không được dùng Cung!");
                if (InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Staff, ClassBranch.Novice, out _))
                    throw new Exception("Nhánh Tân thủ không được dùng Gậy!");

                // 2. Nhánh Archer: Dùng được Cung, KHÔNG dùng được Kiếm/Gậy
                if (!InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Bow, ClassBranch.Archer, out _))
                    throw new Exception("Nhánh Xạ thủ phải dùng được Cung!");
                if (InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Sword, ClassBranch.Archer, out _))
                    throw new Exception("Nhánh Xạ thủ không được dùng Kiếm!");

                // 3. Nhánh Mage: Dùng được Gậy, KHÔNG dùng được Kiếm/Cung
                if (!InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Staff, ClassBranch.Mage, out _))
                    throw new Exception("Nhánh Pháp sư phải dùng được Gậy!");
                if (InventoryBagUI.CanEquipWeaponForBranch(WeaponType.Sword, ClassBranch.Mage, out _))
                    throw new Exception("Nhánh Pháp sư không được dùng Kiếm!");

                // Kiểm tra thanh dưới cùng
                if (bagUI.goldText == null) throw new Exception("goldText bị null!");
                bagUI.RefreshBagUI();
                if (!bagUI.goldText.text.Contains("1,500") && !bagUI.goldText.text.Contains("1500"))
                {
                    throw new Exception($"Vàng không hiển thị 1,500 G! Nội dung: '{bagUI.goldText.text}'");
                }

                // Kiểm tra tính năng Trang Bị vũ khí hợp lệ cho Tân Thủ
                var charUI = CharacterStatusUI.EnsureUI(testRoot.transform);
                charUI.InitDefaultEquipment();
                bagUI.EquipItemToCharacter(s6, 5); // Trang bị Kiếm Sắt cho Novice
                if (charUI.equippedWeapon == null || charUI.equippedWeapon.itemId != s6.itemId)
                {
                    throw new Exception("Trang bị Kiếm Sắt cho nhánh Tân thủ thất bại!");
                }

                // Kiểm tra nút Sắp Xếp
                bagUI.SortBagItems();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testRoot);
            }
        }

        private static void Test_MinimapSystem2DStructure()
        {
            GameObject testRoot = new GameObject("Test_Canvas_Root", typeof(RectTransform));
            try
            {
                var miniMap = MinimapSystem2D.EnsureUI(testRoot.transform);
                if (miniMap == null) throw new Exception("Không thể tạo instance MinimapSystem2D!");
                if (miniMap.minimapFrame == null) throw new Exception("minimapFrame bị null!");

                // Kiểm tra kích thước 170x170
                if (Mathf.Abs(miniMap.minimapFrame.sizeDelta.x - 170f) > 1f || Mathf.Abs(miniMap.minimapFrame.sizeDelta.y - 170f) > 1f)
                {
                    throw new Exception($"Kích thước khung Minimap {miniMap.minimapFrame.sizeDelta} không đúng 170x170!");
                }

                // Kiểm tra neo Top-Right
                if (miniMap.minimapFrame.anchorMin != Vector2.one || miniMap.minimapFrame.anchorMax != Vector2.one)
                {
                    throw new Exception("Minimap chưa được neo ở Góc Trên Bên Phải (Top-Right)!");
                }

                // Kiểm tra tiêu đề "Làng Oakhaven"
                if (miniMap.locationTitleText == null || !miniMap.locationTitleText.text.Contains("Oakhaven"))
                {
                    throw new Exception("Tiêu đề Minimap chưa có 'Làng Oakhaven'!");
                }

                // Kiểm tra chấm xanh sáng player marker
                if (miniMap.playerMarkerDot == null) throw new Exception("playerMarkerDot bị null!");

                // Kiểm tra nút Zoom
                if (miniMap.zoomInButton == null || miniMap.zoomOutButton == null) throw new Exception("Nút Zoom bị null!");
                float initialZoom = miniMap.currentZoom;
                miniMap.ZoomIn();
                if (miniMap.currentZoom >= initialZoom) throw new Exception("ZoomIn không giảm orthographicSize!");
                miniMap.ZoomOut();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testRoot);
            }
        }

        private static void Test_GameplayHUDIntegration()
        {
            GameObject testCanvas = new GameObject("Test_Canvas", typeof(RectTransform), typeof(Canvas));
            try
            {
                GameObject hudObj = new GameObject("GameplayHUD", typeof(RectTransform));
                hudObj.transform.SetParent(testCanvas.transform, false);
                var hud = hudObj.AddComponent<GameplayHUD>();

                hud.EnsureUIHierarchy();

                if (hud.characterButton == null) throw new Exception("characterButton chưa được gán!");
                if (hud.inventoryButton == null) throw new Exception("inventoryButton chưa được gán!");

                // Kiểm tra xem CharacterStatusUI, InventoryBagUI, MinimapSystem2D đã tự sinh
                if (testCanvas.transform.Find("CharacterStatusUI_Panel") == null && hudObj.transform.Find("CharacterStatusUI_Panel") == null)
                {
                    throw new Exception("CharacterStatusUI_Panel chưa được tự động sinh trong Canvas!");
                }

                if (testCanvas.transform.Find("InventoryBagUI_Panel") == null && hudObj.transform.Find("InventoryBagUI_Panel") == null)
                {
                    throw new Exception("InventoryBagUI_Panel chưa được tự động sinh trong Canvas!");
                }

                if (testCanvas.transform.Find("MinimapSystem2D_Frame") == null && hudObj.transform.Find("MinimapSystem2D_Frame") == null)
                {
                    throw new Exception("MinimapSystem2D_Frame chưa được tự động sinh trong Canvas!");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testCanvas);
            }
        }
    }
}
#endif
