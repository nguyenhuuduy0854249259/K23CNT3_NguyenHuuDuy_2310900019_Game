using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Combat;

namespace ActionRPG.Enemies
{
    /// <summary>
    /// Thanh máu World-space chuẩn RPG hiển thị trên đầu quái vật:
    /// - Nhãn cấp độ (Lv.X) thu nhỏ, định vị sắc nét ngay trên thanh máu.
    /// - Hiệu ứng mất máu từ từ (Gradual Damage Lag): Thanh đỏ/cam giữ delay và rút từ từ sau thanh máu xanh.
    /// - Icon / Tên và màu sắc Hệ Nguyên Tố chuẩn Lumina.
    /// - Tự động cân bằng hướng nhìn (không bị lật ngược khi quái đổi hướng).
    /// </summary>
    public class EnemyHealthBarWorldSpace : MonoBehaviour
    {
        [Header("Tham Chiếu Quái")]
        public EnemyController targetController;
        public EnemyStats targetStats;

        [Header("Thành Phần UI")]
        public Canvas canvas;
        public Image bgBar;
        public Image easeBar; // Thanh đỏ cam rút từ từ (Damage Lag)
        public Image fillBar; // Thanh xanh chính hạ ngay
        public TMP_Text levelText;
        public TMP_Text nameText;
        public Image elementIcon;
        public TMP_Text elementText;

        [Header("Cài Đặt Hiển Thị")]
        public Vector3 offset = new Vector3(0f, 0.85f, 0f);
        public bool autoHideWhenFull = false;

        [Header("Hiệu Ứng Mất Máu Từ Từ (Damage Lag Ease)")]
        [Tooltip("Thời gian chờ trước khi thanh đỏ bắt đầu rút từ từ (giây)")]
        public float easeDelay = 0.35f;

        [Tooltip("Tốc độ rút từ từ của thanh máu đỏ (giây)")]
        public float easeDrainSpeed = 2.0f;

        [Tooltip("Tốc độ hạ của thanh máu xanh chính (rớt nhanh, tức thì)")]
        public float instantDropSpeed = 16.0f;

        private float currentFill = 1.0f;
        private float targetFill = 1.0f;
        private float easeFill = 1.0f;
        private float easeDelayTimer = 0f;
        private float lastHpRecorded = -1f;

        private void Awake()
        {
            if (targetController == null) targetController = GetComponent<EnemyController>() ?? GetComponentInParent<EnemyController>();
            if (targetStats == null) targetStats = GetComponent<EnemyStats>() ?? GetComponentInParent<EnemyStats>();

            EnsureWorldSpaceUI();
        }

        private void Start()
        {
            UpdateVisualsImmediate();
        }

        private void LateUpdate()
        {
            Transform container = GetUIContainer();
            if (container != null)
            {
                // Giữ cho thanh máu luôn hướng thẳng đứng và không bị lật ngược khi quái quay đầu
                float parentSign = Mathf.Sign(transform.lossyScale.x);
                container.localScale = new Vector3(0.01f * parentSign, 0.01f, 1f);
                container.rotation = Quaternion.identity;
                container.localPosition = offset;
            }

            UpdateHp();
        }

        public void UpdateVisualsImmediate()
        {
            string eName = "Quái";
            int lvl = 1;
            ElementType elem = ElementType.None;
            float maxHp = 50f;
            float curHp = 50f;

            if (targetStats != null)
            {
                eName = targetStats.enemyName;
                lvl = targetStats.level;
                elem = targetStats.element;
                maxHp = targetStats.maxHp;
                curHp = targetStats.currentHp;
            }
            else if (targetController != null)
            {
                eName = targetController.enemyName;
                lvl = targetController.level;
                elem = targetController.element;
                maxHp = targetController.maxHp;
                curHp = targetController.currentHp;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{lvl}";
                levelText.color = new Color(1.0f, 0.88f, 0.35f);
            }
            if (nameText != null) nameText.text = eName;

            Color elemColor = ElementalSynergy.GetElementColor(elem);
            if (elementIcon != null) elementIcon.color = elemColor;
            if (elementText != null)
            {
                string sName = GetElementShortName(elem);
                elementText.text = string.IsNullOrEmpty(sName) ? "" : $"[{sName}]";
                elementText.color = elemColor;
            }

            targetFill = maxHp > 0 ? Mathf.Clamp01(curHp / maxHp) : 0f;
            currentFill = targetFill;
            easeFill = targetFill;
            lastHpRecorded = curHp;

            if (fillBar != null) fillBar.fillAmount = targetFill;
            if (easeBar != null) easeBar.fillAmount = targetFill;
        }

        private void UpdateHp()
        {
            float maxHp = 1f;
            float curHp = 1f;
            int lvl = 1;

            if (targetStats != null)
            {
                maxHp = targetStats.maxHp;
                curHp = targetStats.currentHp;
                lvl = targetStats.level;
            }
            else if (targetController != null)
            {
                maxHp = targetController.maxHp;
                curHp = targetController.currentHp;
                lvl = targetController.level;
            }

            // Đồng bộ cấp độ hiển thị nếu có cập nhật
            if (levelText != null && (levelText.text == "" || !levelText.text.EndsWith(lvl.ToString())))
            {
                levelText.text = $"Lv.{lvl}";
            }

            if (lastHpRecorded < 0f)
            {
                lastHpRecorded = curHp;
            }

            targetFill = maxHp > 0 ? Mathf.Clamp01(curHp / maxHp) : 0f;

            // 1. Kiểm tra biến động máu
            if (curHp < lastHpRecorded - 0.001f)
            {
                // Khi bị mất máu: kích hoạt thời gian chờ cho thanh máu đỏ rút từ từ
                easeDelayTimer = easeDelay;
                lastHpRecorded = curHp;
            }
            else if (curHp > lastHpRecorded + 0.001f)
            {
                // Khi được hồi máu: thanh đỏ bắt kịp ngay để không bị lỗi hiển thị
                easeFill = targetFill;
                if (easeBar != null) easeBar.fillAmount = easeFill;
                lastHpRecorded = curHp;
            }

            // 2. Thanh máu xanh chính hạ nhanh về targetFill
            currentFill = Mathf.MoveTowards(currentFill, targetFill, Time.deltaTime * instantDropSpeed);
            if (fillBar != null)
            {
                fillBar.fillAmount = currentFill;

                // Đổi màu thanh máu: Xanh lá (>50%) -> Vàng (>25%) -> Đỏ cam (<=25%)
                if (currentFill > 0.5f)
                    fillBar.color = Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.20f, 0.88f, 0.38f), (currentFill - 0.5f) * 2f);
                else
                    fillBar.color = Color.Lerp(new Color(0.95f, 0.25f, 0.2f), new Color(1f, 0.85f, 0.2f), currentFill * 2f);
            }

            // 3. Xử lý thanh đỏ rút từ từ (Gradual Health Loss Effect)
            if (easeDelayTimer > 0f)
            {
                easeDelayTimer -= Time.deltaTime;
            }
            else
            {
                easeFill = Mathf.MoveTowards(easeFill, targetFill, Time.deltaTime * easeDrainSpeed);
                if (easeBar != null)
                {
                    easeBar.fillAmount = easeFill;
                }
            }

            // 4. Quản lý ẩn hiện
            Transform container = GetUIContainer();
            Canvas targetCanvas = canvas != null ? canvas : (container != null ? container.GetComponent<Canvas>() : null);
            if (targetCanvas != null)
            {
                if (autoHideWhenFull)
                {
                    targetCanvas.enabled = targetFill < 0.999f && curHp > 0f;
                }
                else
                {
                    targetCanvas.enabled = curHp > 0f;
                }
            }
        }

        private Transform GetUIContainer()
        {
            // Nếu component nằm trực tiếp trên Enemy (có SpriteRenderer / EnemyController),
            // dùng GameObject con để không làm thay đổi scale của Sprite quái vật
            if (GetComponent<SpriteRenderer>() != null || GetComponent<EnemyController>() != null || GetComponent<EnemyStats>() != null)
            {
                Transform container = transform.Find("HealthBar_Container");
                if (container == null)
                {
                    GameObject cObj = new GameObject("HealthBar_Container");
                    cObj.transform.SetParent(transform, false);
                    cObj.transform.localPosition = offset;
                    container = cObj.transform;
                }
                return container;
            }
            return transform;
        }

        private void EnsureWorldSpaceUI()
        {
            Transform container = GetUIContainer();

            if (canvas == null)
            {
                canvas = container.GetComponent<Canvas>();
                if (canvas == null) canvas = container.gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 15; // Hiển thị đè rõ trên đầu quái

            RectTransform rootRect = container.GetComponent<RectTransform>();
            if (rootRect == null) rootRect = container.gameObject.AddComponent<RectTransform>();

            rootRect.sizeDelta = new Vector2(100f, 32f);
            rootRect.localPosition = offset;
            rootRect.localScale = new Vector3(0.01f, 0.01f, 1f);

            // Tự động dựng lại cây UI nếu chưa có thanh máu đỏ hoặc text
            if (fillBar == null || easeBar == null || levelText == null)
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    var child = container.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
                BuildUIHierarchy(container);
            }
        }

        private void BuildUIHierarchy(Transform container)
        {
            // 1. Khung viền đen RPG (Frame Border)
            GameObject borderObj = new GameObject("Hp_Border");
            borderObj.transform.SetParent(container, false);
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.color = new Color(0.05f, 0.06f, 0.08f, 0.95f);
            var borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.sizeDelta = new Vector2(84f, 11f);
            borderRect.anchoredPosition = Vector2.zero;

            // 2. Nền thanh máu (Dark Background)
            GameObject bgObj = new GameObject("Hp_BG");
            bgObj.transform.SetParent(borderObj.transform, false);
            bgBar = bgObj.AddComponent<Image>();
            bgBar.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(80f, 7f);
            bgRect.anchoredPosition = Vector2.zero;

            // 3. Thanh Máu Rút Từ Từ (Ease / Damage Lag Bar - Màu đỏ cam)
            GameObject easeObj = new GameObject("Hp_DamageEase");
            easeObj.transform.SetParent(bgObj.transform, false);
            easeBar = easeObj.AddComponent<Image>();
            easeBar.type = Image.Type.Filled;
            easeBar.fillMethod = Image.FillMethod.Horizontal;
            easeBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            easeBar.color = new Color(0.95f, 0.28f, 0.20f, 0.95f);
            var easeRect = easeObj.GetComponent<RectTransform>();
            easeRect.anchorMin = Vector2.zero;
            easeRect.anchorMax = Vector2.one;
            easeRect.sizeDelta = Vector2.zero;
            easeRect.anchoredPosition = Vector2.zero;

            // 4. Thanh Máu Chính (Instant Green Fill Bar)
            GameObject fillObj = new GameObject("Hp_Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            fillBar = fillObj.AddComponent<Image>();
            fillBar.type = Image.Type.Filled;
            fillBar.fillMethod = Image.FillMethod.Horizontal;
            fillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillBar.color = new Color(0.20f, 0.88f, 0.38f, 1.0f);
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            // 5. Ánh sáng viền bóng (Gloss Line trên đỉnh thanh máu)
            GameObject glossObj = new GameObject("Hp_Gloss");
            glossObj.transform.SetParent(bgObj.transform, false);
            var glossImg = glossObj.AddComponent<Image>();
            glossImg.color = new Color(1f, 1f, 1f, 0.20f);
            var glossRect = glossObj.GetComponent<RectTransform>();
            glossRect.anchorMin = new Vector2(0f, 0.5f);
            glossRect.anchorMax = Vector2.one;
            glossRect.sizeDelta = Vector2.zero;
            glossRect.anchoredPosition = Vector2.zero;

            // 6. Nhãn Cấp Độ (Lv.X) - Thu nhỏ gọn gàng, nằm ngay trên mép trái thanh máu
            GameObject lvObj = new GameObject("Level_Text");
            lvObj.transform.SetParent(container, false);
            levelText = lvObj.AddComponent<TextMeshProUGUI>();
            levelText.fontSize = 10f;
            levelText.fontStyle = FontStyles.Bold;
            levelText.textWrappingMode = TextWrappingModes.NoWrap;
            levelText.overflowMode = TextOverflowModes.Overflow;
            levelText.alignment = TextAlignmentOptions.BottomLeft;
            levelText.color = new Color(1.0f, 0.88f, 0.35f);
            ActionRPG.UI.FontManager.ApplyFont(levelText);

            var lvRect = lvObj.GetComponent<RectTransform>();
            lvRect.sizeDelta = new Vector2(40f, 14f);
            lvRect.pivot = new Vector2(0f, 0f);
            lvRect.anchoredPosition = new Vector2(-40f, 6.5f);

            // 7. Nhãn Nguyên Tố (Hệ) - Nằm ngay trên mép phải thanh máu
            GameObject elemObj = new GameObject("Element_Text");
            elemObj.transform.SetParent(container, false);
            elementText = elemObj.AddComponent<TextMeshProUGUI>();
            elementText.fontSize = 9.0f;
            elementText.fontStyle = FontStyles.Bold;
            elementText.textWrappingMode = TextWrappingModes.NoWrap;
            elementText.overflowMode = TextOverflowModes.Overflow;
            elementText.alignment = TextAlignmentOptions.BottomRight;
            ActionRPG.UI.FontManager.ApplyFont(elementText);

            var elemRect = elemObj.GetComponent<RectTransform>();
            elemRect.sizeDelta = new Vector2(40f, 14f);
            elemRect.pivot = new Vector2(1f, 0f);
            elemRect.anchoredPosition = new Vector2(40f, 6.5f);
        }

        public static string GetElementShortName(ElementType elem)
        {
            switch (elem)
            {
                case ElementType.Fire: return "Hỏa";
                case ElementType.Water: return "Thủy";
                case ElementType.Wind: return "Phong";
                case ElementType.Earth: return "Thổ";
                case ElementType.Electric: return "Điện";
                case ElementType.Poison: return "Độc";
                case ElementType.Angel: return "Thánh";
                case ElementType.Demon: return "Ma";
                default: return "";
            }
        }
    }
}
