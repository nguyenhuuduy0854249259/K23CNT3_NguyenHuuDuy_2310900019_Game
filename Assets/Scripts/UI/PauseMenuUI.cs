using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ActionRPG.UI
{
    /// <summary>
    /// Menu Tạm Dừng & Cài Đặt Hệ Thống (Pause Menu).
    /// Kích hoạt bằng phím [Escape] hoặc nút Menu trên HUD.
    /// Cung cấp liên kết nhanh tới Class Demo Switcher [F1], Bảng Nhân Vật [U], Túi Đồ [I] và Bảng Phím Tắt.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        [Header("Tham Chiếu Panel")]
        public GameObject panelRoot;
        public Button resumeButton;
        public Button classDemoButton;
        public Button characterButton;
        public Button inventoryButton;
        public Button keybindsButton;
        public Button quitButton;

        [Header("Bảng Hướng Dẫn Phím Tắt")]
        public GameObject keybindsModal;
        public Button closeKeybindsButton;

        private bool isPaused = false;

        private void Awake()
        {
            Time.timeScale = 1f;
            if (Instance == null) Instance = this;

            EnsureUIHierarchy();
        }

        private void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (keybindsModal != null) keybindsModal.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (classDemoButton != null) classDemoButton.onClick.AddListener(OpenClassDemoSwitcher);
            if (characterButton != null) characterButton.onClick.AddListener(OpenCharacterPanel);
            if (inventoryButton != null) inventoryButton.onClick.AddListener(OpenInventoryPanel);
            if (keybindsButton != null) keybindsButton.onClick.AddListener(OpenKeybindsModal);
            if (closeKeybindsButton != null) closeKeybindsButton.onClick.AddListener(CloseKeybindsModal);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
            {
                ActionRPG.UI.FontManager.ApplyFont(txt);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Nếu modal phím tắt đang mở, đóng modal phím tắt trước
                if (keybindsModal != null && keybindsModal.activeSelf)
                {
                    CloseKeybindsModal();
                    return;
                }

                // Nếu Class Demo Switcher đang mở, đóng nó trước
                if (ClassDemoSwitcherUI.Instance != null && ClassDemoSwitcherUI.Instance.panelRoot != null && ClassDemoSwitcherUI.Instance.panelRoot.activeSelf)
                {
                    ClassDemoSwitcherUI.Instance.Close();
                    return;
                }

                // Nếu Character Status UI đang mở, đóng nó trước
                if (CharacterStatusUI.Instance != null && CharacterStatusUI.Instance.panelRoot != null && CharacterStatusUI.Instance.panelRoot.activeSelf)
                {
                    CharacterStatusUI.Instance.Close();
                    return;
                }

                // Nếu Inventory Bag UI đang mở, đóng nó trước
                if (InventoryBagUI.Instance != null && InventoryBagUI.Instance.panelRoot != null && InventoryBagUI.Instance.panelRoot.activeSelf)
                {
                    InventoryBagUI.Instance.Close();
                    return;
                }

                // Nếu Character UI đang mở, đóng nó trước
                if (Character.CharacterUI.Instance != null && Character.CharacterUI.Instance.panelRoot != null && Character.CharacterUI.Instance.panelRoot.activeSelf)
                {
                    Character.CharacterUI.Instance.Close();
                    return;
                }

                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (panelRoot == null) EnsureUIHierarchy();
            if (panelRoot == null) return;
            isPaused = !panelRoot.activeSelf;
            panelRoot.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
        }

        public void ResumeGame()
        {
            isPaused = false;
            if (panelRoot != null) panelRoot.SetActive(false);
            if (keybindsModal != null) keybindsModal.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OpenClassDemoSwitcher()
        {
            ResumeGame();
            if (ClassDemoSwitcherUI.Instance != null)
            {
                ClassDemoSwitcherUI.Instance.Open();
            }
        }

        public void OpenCharacterPanel()
        {
            ResumeGame();
            if (CharacterStatusUI.Instance != null)
            {
                CharacterStatusUI.Instance.Open();
            }
            else if (Character.CharacterUI.Instance != null)
            {
                Character.CharacterUI.Instance.Open();
            }
        }

        public void OpenInventoryPanel()
        {
            ResumeGame();
            if (InventoryBagUI.Instance != null)
            {
                InventoryBagUI.Instance.Open();
            }
            else
            {
                var invUI = FindObjectOfType<InventoryUI>(true);
                if (invUI != null)
                {
                    invUI.ToggleInventory();
                }
            }
        }

        public void OpenKeybindsModal()
        {
            if (keybindsModal != null) keybindsModal.SetActive(true);
        }

        public void CloseKeybindsModal()
        {
            if (keybindsModal != null) keybindsModal.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("[PauseMenu] Thoát game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #region Runtime UI Auto-Builder
        public void EnsureUIHierarchy()
        {
            if (panelRoot != null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
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

            Transform canvasTrans = canvas != null ? canvas.transform : (transform.parent != null ? transform.parent : transform);

            // Root panel
            GameObject root = new GameObject("PauseMenu_Panel", typeof(RectTransform));
            root.transform.SetParent(canvasTrans, false);
            panelRoot = root;

            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 520);

            var img = root.AddComponent<Image>();
            img.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
            var outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.7f, 1f, 0.75f);
            outline.effectDistance = new Vector2(2, -2);

            // Title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(root.transform, false);
            var trt = titleObj.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -20);
            trt.sizeDelta = new Vector2(0, 40);

            var tmpTitle = titleObj.AddComponent<TextMeshProUGUI>();
            tmpTitle.text = "MENU HỆ THỐNG";
            tmpTitle.fontSize = 20;
            tmpTitle.fontStyle = FontStyles.Bold;
            tmpTitle.alignment = TextAlignmentOptions.Center;
            tmpTitle.color = new Color(1f, 0.85f, 0.3f);

            // Button List Container
            GameObject btnList = new GameObject("ButtonsContainer", typeof(RectTransform));
            btnList.transform.SetParent(root.transform, false);
            var blrt = btnList.GetComponent<RectTransform>();
            blrt.anchorMin = new Vector2(0, 0);
            blrt.anchorMax = new Vector2(1, 1);
            blrt.offsetMin = new Vector2(30, 25);
            blrt.offsetMax = new Vector2(-30, -70);

            var vLayout = btnList.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 10;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;

            resumeButton = CreateMenuBtn(btnList.transform, "Tiếp Tục Chơi", new Color(0.15f, 0.5f, 0.25f, 0.95f));
            classDemoButton = CreateMenuBtn(btnList.transform, "⚔ Đổi Class Demo [F1]", new Color(0.2f, 0.45f, 0.75f, 0.95f));
            characterButton = CreateMenuBtn(btnList.transform, "👤 Bảng Nhân Vật & 3 Ấn [U]", new Color(0.18f, 0.22f, 0.30f, 0.95f));
            inventoryButton = CreateMenuBtn(btnList.transform, "🎒 Túi Đồ & Trang Bị [I]", new Color(0.18f, 0.22f, 0.30f, 0.95f));
            keybindsButton = CreateMenuBtn(btnList.transform, "📖 Hướng Dẫn Phím Tắt", new Color(0.18f, 0.22f, 0.30f, 0.95f));
            quitButton = CreateMenuBtn(btnList.transform, "✕ Thoát Game", new Color(0.6f, 0.18f, 0.18f, 0.95f));

            // Keybindings Modal
            CreateKeybindsModal(canvasTrans);

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private Button CreateMenuBtn(Transform parent, string label, Color bgColor)
        {
            GameObject btnObj = new GameObject($"Btn_{label}", typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = bgColor;
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.6f, 0.8f, 0.4f);

            var btn = btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(btnObj.transform, false);
            var trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;

            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private void CreateKeybindsModal(Transform parent)
        {
            GameObject kObj = new GameObject("KeybindsModal", typeof(RectTransform));
            kObj.transform.SetParent(parent, false);
            keybindsModal = kObj;

            var rt = kObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620, 520);

            var img = kObj.AddComponent<Image>();
            img.color = new Color(0.05f, 0.07f, 0.10f, 0.99f);
            var outline = kObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.3f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            // Title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(kObj.transform, false);
            var trt = titleObj.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -15);
            trt.sizeDelta = new Vector2(0, 35);

            var tmpTitle = titleObj.AddComponent<TextMeshProUGUI>();
            tmpTitle.text = "📖 HƯỚNG DẪN PHÍM TẮT ĐIỀU KHIỂN LUMINA";
            tmpTitle.fontSize = 16;
            tmpTitle.fontStyle = FontStyles.Bold;
            tmpTitle.alignment = TextAlignmentOptions.Center;
            tmpTitle.color = new Color(1f, 0.85f, 0.3f);

            // Content text
            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(kObj.transform, false);
            var crt = contentObj.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(30, 60);
            crt.offsetMax = new Vector2(-30, -55);

            var tmpContent = contentObj.AddComponent<TextMeshProUGUI>();
            tmpContent.fontSize = 13;
            tmpContent.lineSpacing = 4;
            tmpContent.text =
                "<color=#80D8FF><b>[W, A, S, D]</b></color> : Di chuyển nhân vật 8 hướng\n" +
                "<color=#80D8FF><b>[Chuột Trái]</b></color> : Đánh thường / Gồng lực bắn cung\n" +
                "<color=#80D8FF><b>[Chuột Phải]</b></color> : Giơ khiên phòng thủ (Active Block)\n" +
                "<color=#80D8FF><b>[Space]</b></color> : Lướt né đòn (Dash i-frame bất tử 0.22s)\n" +
                "<color=#FFD54F><b>[1, 2, 3, 4, 5, Q]</b></color> : Tung 6 Kỹ Năng Class tương ứng\n" +
                "<color=#FF8A80><b>[E]</b></color> : Tương tác NPC / Dùng nhanh Bình Máu\n" +
                "<color=#82B1FF><b>[R]</b></color> : Dùng nhanh Bình Mana (CD 8s)\n" +
                "<color=#EA80FC><b>[X]</b></color> : Hoán đổi Stance Thiên Thần ↔ Ác Quỷ (Mục Sư)\n" +
                "<color=#B9F6CA><b>[T]</b></color> : Triệu hồi ↔ Thu hồi Pet đồng hành\n" +
                "<color=#FFE57F><b>[C]</b></color> : Thu phục Quái Vật (Máu < 30%)\n" +
                "<color=#CCFF90><b>[F]</b></color> : Thu hoạch Thảo dược / Khoáng thạch (Channel 1.0s)\n" +
                "<color=#FFD180><b>[U]</b></color> : Bảng Nhân Vật & 3 Ấn Nội Tại Cốt Lõi\n" +
                "<color=#FFD180><b>[I] / [B]</b></color> : Mở Túi Đồ & Trang Bị\n" +
                "<color=#00E5FF><b>[F1] / [K]</b></color> : Bảng Chuyển Đổi 14 Class Demo & Test Cơ Chế\n" +
                "<color=#00E676><b>[F5] / [F9]</b></color> : Quick Save [AES-256] ↔ Quick Load Tiến Trình\n" +
                "<color=#CFD8DC><b>[Escape]</b></color> : Mở / Đóng Menu Hệ Thống";

            // Close button
            GameObject closeObj = new GameObject("CloseButton", typeof(RectTransform));
            closeObj.transform.SetParent(kObj.transform, false);
            var clrt = closeObj.GetComponent<RectTransform>();
            clrt.anchorMin = new Vector2(0.5f, 0);
            clrt.anchorMax = new Vector2(0.5f, 0);
            clrt.pivot = new Vector2(0.5f, 0);
            clrt.anchoredPosition = new Vector2(0, 15);
            clrt.sizeDelta = new Vector2(160, 36);

            var clImg = closeObj.AddComponent<Image>();
            clImg.color = new Color(0.2f, 0.45f, 0.75f, 0.95f);
            closeKeybindsButton = closeObj.AddComponent<Button>();

            GameObject clTxtObj = new GameObject("Text", typeof(RectTransform));
            clTxtObj.transform.SetParent(closeObj.transform, false);
            var clTxtRt = clTxtObj.GetComponent<RectTransform>();
            clTxtRt.anchorMin = Vector2.zero;
            clTxtRt.anchorMax = Vector2.one;
            clTxtRt.offsetMin = Vector2.zero;
            clTxtRt.offsetMax = Vector2.zero;

            var clTxt = clTxtObj.AddComponent<TextMeshProUGUI>();
            clTxt.text = "ĐÃ HIỂU";
            clTxt.fontSize = 13;
            clTxt.fontStyle = FontStyles.Bold;
            clTxt.alignment = TextAlignmentOptions.Center;
            clTxt.color = Color.white;
        }
        #endregion
    }
}
