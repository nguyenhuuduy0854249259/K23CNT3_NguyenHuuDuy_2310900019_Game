using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Items;
using ActionRPG.Items.Loot;
using ActionRPG.Core;
using ActionRPG.Characters;

namespace ActionRPG.Environment
{
    public enum GatheringNodeType
    {
        HerbBush,   // Bụi Cây Thảo Mộc -> Bột / Sợi Thảo Mộc
        OreVein,    // Bãi Khoáng Thạch -> Bột Đá Khoáng
        PureSpring  // Suối Nước Tinh Khiết -> Giọt Nước Tinh Khiết
    }

    /// <summary>
    /// Điểm thu thập dã ngoại (Bụi Cây Thảo Mộc / Bãi Khoáng Thạch / Suối Nước) chuẩn Sprint 3:
    /// - Hiển thị UI gợi ý "Bấm [F] để thu hoạch" khi Player vào vùng tương tác (2.0m).
    /// - Khi người chơi bấm [F]: Chạy thanh tương tác 1.0s (Channeling bar).
    /// - Hoàn tất: Văng ra các nguyên liệu pha chế cho Luyện Dược Sư (Giọt nước tinh khiết, Bột thảo mộc, Bột đá khoáng).
    /// - Chuyển node sang trạng thái xám (Depleted), tự phục hồi (Respawn) sau 30s.
    /// </summary>
    public class GatheringNode : MonoBehaviour
    {
        [Header("Loại Điểm Thu Hoạch (Sprint 3)")]
        public GatheringNodeType nodeType = GatheringNodeType.HerbBush;
        public string nodeName = "Bụi Cây Thảo Mộc";

        [Header("Vật Phẩm Thu Được")]
        public ItemData harvestedItem;
        public int minQuantity = 1;
        public int maxQuantity = 3;

        [Header("Cài Đặt Tương Tác & Kênh Dẫn (Channeling)")]
        public float interactDistance = 2.0f;
        public float channelDuration = 1.0f; // Thời gian chạy thanh tương tác 1.0s
        public float respawnCooldown = 30.0f; // Tự phục hồi sau 30s theo yêu cầu

        [Header("Trạng Thái Hiện Tại")]
        public bool isHarvested = false;
        public float respawnTimer = 0f;
        public bool isChanneling = false;
        public float currentChannelProgress = 0f;

        [Header("UI Tương Tác World-Space")]
        public GameObject interactionPromptObj;
        public TMP_Text promptText;
        public GameObject channelBarObj;
        public Image channelBarFill;

        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;
        private Color originalColor = Color.white;
        private bool isPlayerNearby = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            SetupDefaultItemIfNeeded();
            EnsureInWorldUI();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            // 1. Đếm ngược thời gian hồi phục 30s sau khi đã khai thác
            if (isHarvested)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
                return;
            }

            // 2. Tìm người chơi
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool nearby = dist <= interactDistance;

            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                UpdatePromptUI();
            }

            // 3. Xử lý thanh tương tác 1.0s (Channeling)
            if (isChanneling)
            {
                // Nếu người chơi di chuyển ra quá xa (> 2.5m) thì hủy kênh tương tác
                if (dist > interactDistance + 0.5f)
                {
                    CancelChanneling("Đã rời khỏi vùng tương tác!");
                    return;
                }

                currentChannelProgress += Time.deltaTime;
                if (channelBarFill != null)
                {
                    channelBarFill.fillAmount = Mathf.Clamp01(currentChannelProgress / channelDuration);
                }

                // Khi chạy hết 1.0s
                if (currentChannelProgress >= channelDuration)
                {
                    CompleteHarvest();
                }
                return;
            }

            // 4. Bấm phím [F] để bắt đầu thu hoạch
            if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
            {
                StartChanneling();
            }
        }

        /// <summary>
        /// Bắt đầu thanh chạy tương tác 1.0s.
        /// </summary>
        public void StartChanneling()
        {
            if (isHarvested || isChanneling) return;

            isChanneling = true;
            currentChannelProgress = 0f;

            if (channelBarObj != null) channelBarObj.SetActive(true);
            if (channelBarFill != null) channelBarFill.fillAmount = 0f;

            Debug.Log($"<color=yellow>[Gathering] Đang thu thập {nodeName}... (1.0s)</color>");
        }

        public void CancelChanneling(string reason = "")
        {
            if (!isChanneling) return;
            isChanneling = false;
            currentChannelProgress = 0f;

            if (channelBarObj != null) channelBarObj.SetActive(false);
            if (!string.IsNullOrEmpty(reason))
            {
                Debug.LogWarning($"[Gathering] Hủy thu hoạch: {reason}");
            }
        }

        /// <summary>
        /// Hoàn tất thu hoạch: Văng ra nguyên liệu Luyện Dược Sư dạng WorldItem, đổi màu xám và đếm ngược 30s.
        /// </summary>
        public void CompleteHarvest()
        {
            isChanneling = false;
            currentChannelProgress = 0f;
            if (channelBarObj != null) channelBarObj.SetActive(false);

            isHarvested = true;
            respawnTimer = respawnCooldown;

            int qty = Random.Range(minQuantity, maxQuantity + 1);

            // Văng vật phẩm dạng 2D WorldItem quanh bán kính 1.0m
            if (harvestedItem != null)
            {
                WorldItem.Spawn(transform.position, harvestedItem, qty, 1.0f);
            }
            else
            {
                // Fallback nếu thiếu asset
                SetupDefaultItemIfNeeded();
                WorldItem.Spawn(transform.position, harvestedItem, qty, 1.0f);
            }

            string itemName = harvestedItem != null ? harvestedItem.itemName : GetDefaultItemName(nodeType);
            string msg = $"🌿 Đã thu hoạch: {qty}x {itemName}!";
            Debug.Log($"<color=green>[Gathering] {msg}</color>");
            EventBus.OnShowUIMessage?.Invoke(msg);

            // Chuyển node sang trạng thái xám mờ (Depleted)
            SetDepletedVisual(true);

            UpdatePromptUI();
        }

        /// <summary>
        /// Phương thức thu hoạch trực tiếp hỗ trợ Test Suite và tự động hóa.
        /// </summary>
        public void Harvest()
        {
            if (isHarvested) return;
            CompleteHarvest();
        }

        /// <summary>
        /// Tự phục hồi sau 30s (Respawn).
        /// </summary>
        public void Respawn()
        {
            isHarvested = false;
            respawnTimer = 0f;
            isChanneling = false;
            currentChannelProgress = 0f;

            SetDepletedVisual(false);

            Debug.Log($"<color=cyan>[Gathering] {nodeName} đã tự phục hồi tài nguyên sau {respawnCooldown}s!</color>");
            UpdatePromptUI();
        }

        private void SetDepletedVisual(bool depleted)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (depleted)
                {
                    // Chuyển sang trạng thái xám mờ
                    spriteRenderer.color = new Color(0.45f, 0.45f, 0.45f, 0.4f);
                }
                else
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        private void UpdatePromptUI()
        {
            if (interactionPromptObj != null)
            {
                bool show = isPlayerNearby && !isHarvested && !isChanneling;
                interactionPromptObj.SetActive(show);
            }
        }

        private void FindPlayer()
        {
            if (CharacterStats.PlayerInstance != null)
            {
                playerTransform = CharacterStats.PlayerInstance.transform;
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }
        }

        private void SetupDefaultItemIfNeeded()
        {
            if (harvestedItem != null) return;

            // Nạp nguyên liệu pha chế Dược Sư từ Resources/Data hoặc tự tạo trong bộ nhớ
            harvestedItem = ScriptableObject.CreateInstance<ItemData>();
            harvestedItem.itemType = ItemType.Material;
            harvestedItem.isStackable = true;
            harvestedItem.maxStack = 99;

            switch (nodeType)
            {
                case GatheringNodeType.HerbBush:
                    nodeName = "Bụi Cây Thảo Mộc";
                    harvestedItem.itemId = "mat_herb_fiber";
                    harvestedItem.itemName = "Bột Thảo Mộc";
                    harvestedItem.description = "Bột thảo dược phơi khô, nguyên liệu cốt lõi cho bình máu Dược Sư.";
                    break;

                case GatheringNodeType.OreVein:
                    nodeName = "Bãi Khoáng Thạch";
                    harvestedItem.itemId = "mat_stone_powder";
                    harvestedItem.itemName = "Bột Đá Khoáng";
                    harvestedItem.description = "Bột khoáng thạch dã ngoại, nguyên liệu ổn định năng lượng dược tính.";
                    break;

                case GatheringNodeType.PureSpring:
                    nodeName = "Suối Nước Tinh Khiết";
                    harvestedItem.itemId = "mat_pure_water";
                    harvestedItem.itemName = "Giọt Nước Tinh Khiết";
                    harvestedItem.description = "Nước suối ngọt lành tự nhiên, dung môi hoà tan cho mọi bài thuốc.";
                    break;
            }
        }

        public static string GetDefaultItemName(GatheringNodeType type)
        {
            switch (type)
            {
                case GatheringNodeType.HerbBush: return "Bột Thảo Mộc";
                case GatheringNodeType.OreVein: return "Bột Đá Khoáng";
                case GatheringNodeType.PureSpring: return "Giọt Nước Tinh Khiết";
                default: return "Nguyên Liệu";
            }
        }

        #region In-World UI Creation
        private void EnsureInWorldUI()
        {
            Transform existing = transform.Find("NodeInWorldUI");
            if (existing != null)
            {
                interactionPromptObj = existing.Find("Prompt")?.gameObject;
                channelBarObj = existing.Find("ChannelBar")?.gameObject;
                if (channelBarObj != null)
                {
                    channelBarFill = channelBarObj.transform.Find("Fill")?.GetComponent<Image>();
                }
                return;
            }

            // Tạo Canvas World-Space nhỏ hiển thị Prompt & Channel Bar trên đầu
            GameObject canvasObj = new GameObject("NodeInWorldUI");
            canvasObj.transform.SetParent(transform, false);
            canvasObj.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2f, 1f);
            canvasObj.transform.localScale = Vector3.one * 0.015f;

            // Prompt Object
            interactionPromptObj = new GameObject("Prompt", typeof(RectTransform));
            interactionPromptObj.transform.SetParent(canvasObj.transform, false);
            var promptRt = interactionPromptObj.GetComponent<RectTransform>();
            promptRt.sizeDelta = new Vector2(160f, 30f);
            promptRt.anchoredPosition = Vector2.zero;

            var bg = interactionPromptObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            GameObject txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(interactionPromptObj.transform, false);
            promptText = txtObj.AddComponent<TextMeshProUGUI>();
            promptText.text = "Bấm [F] để thu hoạch";
            promptText.fontSize = 14;
            promptText.fontStyle = FontStyles.Bold;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            interactionPromptObj.SetActive(false);

            // Channel Bar Object
            channelBarObj = new GameObject("ChannelBar", typeof(RectTransform));
            channelBarObj.transform.SetParent(canvasObj.transform, false);
            var barRt = channelBarObj.GetComponent<RectTransform>();
            barRt.sizeDelta = new Vector2(140f, 16f);
            barRt.anchoredPosition = new Vector2(0f, -5f);

            var barBg = channelBarObj.AddComponent<Image>();
            barBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform));
            fillObj.transform.SetParent(channelBarObj.transform, false);
            channelBarFill = fillObj.AddComponent<Image>();
            channelBarFill.color = new Color(0.2f, 0.9f, 0.3f);
            channelBarFill.type = Image.Type.Filled;
            channelBarFill.fillMethod = Image.FillMethod.Horizontal;
            channelBarFill.fillAmount = 0f;
            var fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2, 2);
            fillRt.offsetMax = new Vector2(-2, -2);

            channelBarObj.SetActive(false);
        }
        #endregion
    }
}
