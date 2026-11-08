using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Enemies;

namespace ActionRPG.UI
{
    /// <summary>
    /// Hệ thống hiển thị thanh máu (HP Bar) và cấp độ (Lv. + Tên Quái) tự động trên đầu quái vật:
    /// - Tự động tính toán chiều cao dựa trên SpriteRenderer.bounds.extents.y + 0.35f.
    /// - Khởi tạo toàn bộ World-Space Canvas và UI Elements bằng code trong Start() mà không cần prefab thủ công.
    /// - Sorting Order = 500 để không bao giờ bị che khuất bởi mặt đất hay tán cây.
    /// - Khóa góc xoay và triệt tiêu lật ngược tỷ lệ (flipX / scale.x = -1) trong LateUpdate().
    /// - Thanh máu đỏ co giãn theo currentHp / maxHp kèm hiệu ứng Damage Ease lag mượt mà.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyOverheadUI : MonoBehaviour
    {
        [Header("Cài Đặt Vị Trí & Kích Thước")]
        [Tooltip("Khoảng cách trên đỉnh đầu quái vật")]
        public float offsetY = 0.85f;
        public bool autoCalculateHeight = true;

        [Header("Cấu Hình Thanh Máu")]
        public Vector2 barSize = new Vector2(90f, 12f);
        public Color barBgColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        public Color barBorderColor = new Color(0.02f, 0.02f, 0.02f, 0.95f);
        public Color healthFillColor = new Color(0.96f, 0.20f, 0.20f, 1.0f);
        public Color damageEaseColor = new Color(1.0f, 0.72f, 0.22f, 0.90f);
        public Color levelTextColor = new Color(1.0f, 0.92f, 0.35f, 1.0f);

        [Header("Tùy Chọn Hiển Thị")]
        public bool autoHideWhenDead = true;
        public float easeDrainSpeed = 2.0f;
        public float easeDelay = 0.35f;

        // Thành phần UI được tạo Procedural
        private Canvas overheadCanvas;
        private RectTransform canvasRect;
        private Image borderImage;
        private Image bgImage;
        private Image easeFillBar;
        private Image mainFillBar;
        private TextMeshProUGUI levelNameLabel;

        // Dữ liệu tham chiếu quái vật
        private EnemyStats enemyStats;
        private EnemyController enemyController;
        private SpriteRenderer spriteRenderer;

        // Bộ nhớ đệm theo dõi thay đổi (tránh garbage GC)
        private float lastHp = -1f;
        private float lastMaxHp = -1f;
        private int lastLevel = -1;
        private string lastName = null;
        private float currentFill = 1.0f;
        private float targetFill = 1.0f;
        private float easeFill = 1.0f;
        private float easeTimer = 0f;

        private static Sprite s_solidSprite;

        private void Awake()
        {
            CacheReferences();
            CleanupLegacyHealthBars();
        }

        private void Start()
        {
            if (autoCalculateHeight)
            {
                CalculateHeightOffset();
            }

            BuildProceduralUI();
            RefreshAllVisuals(true);
        }

        private void OnEnable()
        {
            if (overheadCanvas != null)
            {
                overheadCanvas.enabled = true;
            }
            RefreshAllVisuals(true);
        }

        private void Update()
        {
            UpdateHealthAndText();
        }

        private void LateUpdate()
        {
            KeepUIUprightAndAligned();
        }

        #region Thu Thập Tham Chiếu & Dọn Dẹp
        private void CacheReferences()
        {
            if (enemyStats == null) enemyStats = GetComponent<EnemyStats>() ?? GetComponentInParent<EnemyStats>();
            if (enemyController == null) enemyController = GetComponent<EnemyController>() ?? GetComponentInParent<EnemyController>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        }

        private void CleanupLegacyHealthBars()
        {
            // Dọn dẹp đối tượng con HealthBar_WorldSpace cũ (nếu có) để tránh hiển thị trùng lặp
            Transform oldBar = transform.Find("HealthBar_WorldSpace");
            if (oldBar != null)
            {
                if (Application.isPlaying) Destroy(oldBar.gameObject);
                else DestroyImmediate(oldBar.gameObject);
            }

            Transform oldContainer = transform.Find("HealthBar_Container");
            if (oldContainer != null)
            {
                if (Application.isPlaying) Destroy(oldContainer.gameObject);
                else DestroyImmediate(oldContainer.gameObject);
            }

            var oldScript = GetComponent<EnemyHealthBarWorldSpace>();
            if (oldScript != null)
            {
                oldScript.enabled = false;
            }
        }

        private void CalculateHeightOffset()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Ưu tiên tính chiều cao từ đỉnh sprite so với transform pivot
                float topAbovePivot = spriteRenderer.bounds.max.y - transform.position.y;
                if (topAbovePivot > 0.15f)
                {
                    offsetY = topAbovePivot + 0.35f;
                }
                else
                {
                    // Fallback theo extents.y
                    offsetY = spriteRenderer.bounds.extents.y + 0.35f;
                }
            }
            else
            {
                offsetY = 0.85f;
            }
        }
        #endregion

        #region Xây Dựng UI Thủ Tục (Procedural World-Space Canvas)
        private void BuildProceduralUI()
        {
            // Nếu đã tồn tại Canvas con, tái sử dụng hoặc làm mới
            Transform existing = transform.Find("Overhead_Health_Canvas");
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
            }

            // 1. Tạo Canvas con
            GameObject canvasObj = new GameObject("Overhead_Health_Canvas");
            canvasObj.transform.SetParent(transform, false);
            canvasObj.transform.localPosition = new Vector3(0f, offsetY, 0f);
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 1.0f);

            overheadCanvas = canvasObj.AddComponent<Canvas>();
            overheadCanvas.renderMode = RenderMode.WorldSpace;
            overheadCanvas.overrideSorting = true;

            // Sorting Layer & Order: sortingOrder = 500 để không bị che khuất
            if (spriteRenderer != null && !string.IsNullOrEmpty(spriteRenderer.sortingLayerName))
            {
                overheadCanvas.sortingLayerName = spriteRenderer.sortingLayerName;
            }
            else
            {
                overheadCanvas.sortingLayerName = "Default";
            }
            overheadCanvas.sortingOrder = 500;

            canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(140f, 40f);

            // 2. Nhãn Level & Tên Quái ("Lv.X [Tên Quái]")
            GameObject labelObj = new GameObject("Label_LevelName");
            labelObj.transform.SetParent(canvasObj.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(140f, 16f);
            labelRect.anchoredPosition = new Vector2(0f, 11f);

            levelNameLabel = labelObj.AddComponent<TextMeshProUGUI>();
            levelNameLabel.fontSize = 11.5f;
            levelNameLabel.fontStyle = FontStyles.Bold;
            levelNameLabel.alignment = TextAlignmentOptions.Center;
            levelNameLabel.textWrappingMode = TextWrappingModes.NoWrap;
            levelNameLabel.overflowMode = TextOverflowModes.Overflow;
            levelNameLabel.color = levelTextColor;
            FontManager.ApplyFont(levelNameLabel);

            // Outline viền đen rõ nét
            var outline = labelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            // 3. Khung Viền Ngoài (Border)
            GameObject borderObj = new GameObject("Hp_Border");
            borderObj.transform.SetParent(canvasObj.transform, false);

            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.sizeDelta = new Vector2(barSize.x + 2f, barSize.y + 2f);
            borderRect.anchoredPosition = new Vector2(0f, -4f);

            borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = GetSolidSprite();
            borderImage.color = barBorderColor;

            // 4. Khung Nền Thanh Máu (Dark Background)
            GameObject bgObj = new GameObject("Hp_Background");
            bgObj.transform.SetParent(borderObj.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(1f, 1f);
            bgRect.offsetMax = new Vector2(-1f, -1f);

            bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = GetSolidSprite();
            bgImage.color = barBgColor;

            // 5. Thanh Máu Rút Từ Từ (Damage Ease Lag Fill)
            GameObject easeObj = new GameObject("Hp_DamageEase");
            easeObj.transform.SetParent(bgObj.transform, false);

            var easeRect = easeObj.AddComponent<RectTransform>();
            easeRect.anchorMin = Vector2.zero;
            easeRect.anchorMax = Vector2.one;
            easeRect.offsetMin = Vector2.zero;
            easeRect.offsetMax = Vector2.zero;

            easeFillBar = easeObj.AddComponent<Image>();
            easeFillBar.sprite = GetSolidSprite();
            easeFillBar.type = Image.Type.Filled;
            easeFillBar.fillMethod = Image.FillMethod.Horizontal;
            easeFillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            easeFillBar.color = damageEaseColor;

            // 6. Thanh Máu Đỏ Chính (Health Fill Bar)
            GameObject fillObj = new GameObject("Hp_HealthFill");
            fillObj.transform.SetParent(bgObj.transform, false);

            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            mainFillBar = fillObj.AddComponent<Image>();
            mainFillBar.sprite = GetSolidSprite();
            mainFillBar.type = Image.Type.Filled;
            mainFillBar.fillMethod = Image.FillMethod.Horizontal;
            mainFillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            mainFillBar.color = healthFillColor;
        }
        #endregion

        #region Đồng Bộ & Cập Nhật Máu & Level
        private void UpdateHealthAndText()
        {
            FetchTargetData(out float curHp, out float maxHp, out int lvl, out string eName);

            // 1. Quản lý hiển thị khi quái chết
            if (autoHideWhenDead && curHp <= 0f)
            {
                if (overheadCanvas != null && overheadCanvas.enabled)
                {
                    overheadCanvas.enabled = false;
                }
                return;
            }
            else
            {
                if (overheadCanvas != null && !overheadCanvas.enabled)
                {
                    overheadCanvas.enabled = true;
                }
            }

            // 2. Cập nhật nhãn Level & Tên nếu có thay đổi
            if (lvl != lastLevel || eName != lastName)
            {
                lastLevel = lvl;
                lastName = eName;
                if (levelNameLabel != null)
                {
                    levelNameLabel.text = $"Lv.{lvl} {eName}";
                }
            }

            // 3. Cập nhật tỷ lệ máu
            targetFill = maxHp > 0f ? Mathf.Clamp01(curHp / maxHp) : 0f;

            if (lastHp < 0f)
            {
                lastHp = curHp;
                lastMaxHp = maxHp;
                currentFill = targetFill;
                easeFill = targetFill;
            }

            // Phát hiện quái bị nhận sát thương
            if (curHp < lastHp - 0.01f)
            {
                easeTimer = easeDelay;
                lastHp = curHp;
            }
            else if (curHp > lastHp + 0.01f)
            {
                // Khi được hồi máu
                easeFill = targetFill;
                lastHp = curHp;
            }
            lastMaxHp = maxHp;

            // Thanh máu chính hạ ngay
            currentFill = targetFill;
            if (mainFillBar != null)
            {
                mainFillBar.fillAmount = currentFill;
            }

            // Thanh đỏ cam rút từ từ sau delay (Damage Lag)
            if (easeTimer > 0f)
            {
                easeTimer -= Time.deltaTime;
            }
            else
            {
                easeFill = Mathf.MoveTowards(easeFill, targetFill, Time.deltaTime * easeDrainSpeed);
                if (easeFillBar != null)
                {
                    easeFillBar.fillAmount = easeFill;
                }
            }
        }

        public void RefreshAllVisuals(bool force = false)
        {
            if (force)
            {
                lastHp = -1f;
                lastLevel = -1;
                lastName = null;
            }
            UpdateHealthAndText();
        }

        private void FetchTargetData(out float curHp, out float maxHp, out int lvl, out string eName)
        {
            if (enemyStats == null) enemyStats = GetComponent<EnemyStats>() ?? GetComponentInParent<EnemyStats>();
            if (enemyController == null) enemyController = GetComponent<EnemyController>() ?? GetComponentInParent<EnemyController>();

            curHp = 50f;
            maxHp = 50f;
            lvl = 1;
            eName = "Quái";

            if (enemyStats != null)
            {
                curHp = enemyStats.currentHp;
                maxHp = enemyStats.maxHp;
                lvl = enemyStats.level;
                eName = enemyStats.enemyName;
            }
            else if (enemyController != null)
            {
                curHp = enemyController.currentHp;
                maxHp = enemyController.maxHp;
                lvl = enemyController.level;
                eName = enemyController.enemyName;
            }

            // Chuẩn hóa tên quái vật nếu trống hoặc là tên mặc định
            if (string.IsNullOrEmpty(eName) || eName == "Quái" || eName == "Quái Rừng" || eName == "Quái Dã Ngoại")
            {
                string objName = gameObject.name;
                if (objName.Contains("Dummy") || objName.Contains("dummy") || objName.Contains("Bù Nhìn") || objName.Contains("bù nhìn"))
                {
                    eName = "Bù Nhìn Luyện Võ";
                }
                else if (objName.Contains("WhiteWolf") || objName.Contains("Wolf") || objName.Contains("Sói"))
                {
                    eName = "Sói Trắng Rừng";
                }
                else if (objName.Contains("Boar"))
                {
                    eName = "Lợn Lòi Hoang";
                }
                else if (objName.Contains("Mole"))
                {
                    eName = "Chuột Chũi";
                }
                else if (objName.Contains("Treant"))
                {
                    eName = "Người Cây Treant";
                }
                else if (objName.Contains("Slime"))
                {
                    eName = "Slime Nước";
                }
                else if (objName.Contains("Crab"))
                {
                    eName = "Cua Thảo Nguyên";
                }
            }
        }
        #endregion

        #region Khóa Góc Xoay & Triệt Tiêu Lật Ngược Tỷ Lệ (Billboard / Rotation Lock)
        private void KeepUIUprightAndAligned()
        {
            if (overheadCanvas == null) return;

            Transform cTransform = overheadCanvas.transform;

            // 1. Khóa góc xoay tuyệt đối theo World Space
            cTransform.rotation = Quaternion.identity;

            // 2. Triệt tiêu hiện tượng chữ và thanh máu bị lộn ngược khi quái lật mặt (localScale.x = -1)
            float parentSignX = Mathf.Sign(transform.lossyScale.x);
            if (Mathf.Approximately(parentSignX, 0f)) parentSignX = 1f;

            float parentSignY = Mathf.Sign(transform.lossyScale.y);
            if (Mathf.Approximately(parentSignY, 0f)) parentSignY = 1f;

            cTransform.localScale = new Vector3(0.01f * parentSignX, 0.01f * parentSignY, 1.0f);

            // 3. Giữ vị trí luôn ở đỉnh đầu quái vật
            cTransform.localPosition = new Vector3(0f, offsetY, 0f);
        }
        #endregion

        #region Tạo Sprite Trắng Thủ Tục (100% Thuần Code)
        public static Sprite GetSolidSprite()
        {
            if (s_solidSprite == null)
            {
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                Color[] colors = new Color[4] { Color.white, Color.white, Color.white, Color.white };
                tex.SetPixels(colors);
                tex.Apply();
                s_solidSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
                s_solidSprite.name = "Solid_White_Square_Overhead";
            }
            return s_solidSprite;
        }
        #endregion
    }
}
