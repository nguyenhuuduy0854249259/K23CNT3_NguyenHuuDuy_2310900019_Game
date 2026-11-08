using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using ActionRPG.UI;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.EditorScripts
{
    /// <summary>
    /// Editor Script hỗ trợ 1-Click Setup hệ thống giao diện:
    /// - Bảng Chỉ Số Nhân Vật [U] (CharacterStatusUI)
    /// - Balo Túi Đồ 20 Ô [I] (InventoryBagUI)
    /// - Bản Đồ Nhỏ Minimap 2D (MinimapSystem2D)
    /// Menu: Tools -> Lumina -> Setup Stats, Inventory & Minimap UI
    /// </summary>
    public static class SetupLuminaStatsInventoryMinimapUI
    {
        [MenuItem("Tools/Lumina/Setup Stats, Inventory & Minimap UI", false, 10)]
        public static void SetupUI()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            GameObject gameUIRoot = GameObject.Find("GameUIRoot");
            if (gameUIRoot != null)
            {
                var c = gameUIRoot.GetComponent<Canvas>();
                if (c != null) canvas = c;
            }

            if (canvas == null)
            {
                gameUIRoot = new GameObject("GameUIRoot", typeof(RectTransform));
                canvas = gameUIRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = gameUIRoot.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                gameUIRoot.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(gameUIRoot, "Create GameUIRoot Canvas");
            }
            else
            {
                gameUIRoot = canvas.gameObject;
            }

            // Ensure EventSystem
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            // Ensure GameplayHUD
            GameplayHUD hud = Object.FindAnyObjectByType<GameplayHUD>();
            if (hud == null)
            {
                GameObject hudGO = new GameObject("GameplayHUD", typeof(RectTransform));
                hudGO.transform.SetParent(gameUIRoot.transform, false);
                var rt = hudGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                hud = hudGO.AddComponent<GameplayHUD>();
                Undo.RegisterCreatedObjectUndo(hudGO, "Create GameplayHUD");
            }

            hud.EnsureUIHierarchy();

            // 1. Tạo Bảng Chỉ Số Nhân Vật [U]
            var charUI = CharacterStatusUI.EnsureUI(gameUIRoot.transform);
            if (charUI != null)
            {
                charUI.RefreshUI();
                if (charUI.panelRoot != null) charUI.panelRoot.SetActive(false); // Mặc định ẩn
                EditorUtility.SetDirty(charUI);
            }

            // 2. Tạo Balo Túi Đồ 20 Ô [I]
            var bagUI = InventoryBagUI.EnsureUI(gameUIRoot.transform);
            if (bagUI != null)
            {
                bagUI.InitStarterItems();
                bagUI.RefreshBagUI();
                if (bagUI.panelRoot != null) bagUI.panelRoot.SetActive(false); // Mặc định ẩn
                EditorUtility.SetDirty(bagUI);
            }

            // 3. Tạo Bản Đồ Nhỏ Minimap 2D Góc Trên Bên Phải (170x170 px)
            var miniMap = MinimapSystem2D.EnsureUI(gameUIRoot.transform);
            if (miniMap != null)
            {
                EditorUtility.SetDirty(miniMap);
            }

            EditorSceneManager.MarkSceneDirty(gameUIRoot.scene);
            Selection.activeGameObject = gameUIRoot;

            EditorUtility.DisplayDialog(
                "Lumina UI Setup",
                "✅ Tích hợp hoàn tất 100% không cần kéo thả thủ công!\n\n" +
                "1. BẢNG CHỈ SỐ NHÂN VẬT [U] (CharacterStatusUI):\n" +
                "   - Đã sinh panel bên trái màn hình (nền tối viền vàng đồng).\n" +
                "   - Đầy đủ HP, MP, ATK, DEF, Speed, Crit Rate (15%), Cấp, EXP, Class và 4 ô Trang bị.\n\n" +
                "2. BALO TÚI ĐỒ 20 Ô [I] (InventoryBagUI):\n" +
                "   - Đã sinh panel bên phải màn hình (lưới 5x4 = 20 ô).\n" +
                "   - Có sẵn 5 vật phẩm: Bình Máu (x5), Bình Mana (x5), Thảo Mộc (x12), Quặng Đá Tím (x8), Quả Cầu Tinh Linh (x3).\n" +
                "   - Đầy đủ hiển thị 'Vàng: 1,500 G' và nút [Sắp Xếp].\n\n" +
                "3. MINIMAP 2D GÓC TRÊN PHẢI (MinimapSystem2D):\n" +
                "   - Kích thước 170x170 px neo Top-Right, viền vàng, tiêu đề 'Làng Oakhaven'.\n" +
                "   - RenderTexture + Minimap_Camera tự động bám theo Player, chấm tròn xanh neon chính giữa, nút [+] và [-] zoom.",
                "Tuyệt vời!"
            );

            Debug.Log("<color=green>[LuminaUISetup] Đã tự động tạo và tích hợp thành công Bảng Chỉ Số [U], Balo Túi Đồ [I] và Minimap 2D!</color>");
        }
    }
}
