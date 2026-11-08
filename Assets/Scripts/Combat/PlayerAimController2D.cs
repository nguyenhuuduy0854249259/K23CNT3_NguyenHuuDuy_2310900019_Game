using UnityEngine;

namespace ActionRPG.Combat
{
    /// <summary>
    /// Điều khiển tâm ngắm 360 độ (Aim Controller) theo con trỏ chuột cho Player 2D:
    /// - Xoay AimPivot và FirePoint 360° hướng về vị trí chuột trong World Space.
    /// - Tự động lật mặt nhân vật (SpriteRenderer.flipX) trái/phải theo hướng chuột.
    /// - Chống lộn ngược vũ khí (Inverted Weapon) khi góc quay vượt quá 90 độ (lật trục Y của AimPivot).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerAimController2D : MonoBehaviour
    {
        [Header("Tham Chiếu Tâm Ngắm")]
        [Tooltip("Transform gốc quay 360 độ")]
        [SerializeField] private Transform aimPivot;

        [Tooltip("Vị trí nòng/đầu vũ khí để bắn đạn hoặc vung kiếm")]
        [SerializeField] private Transform firePoint;

        [Header("Visuals & Lật Mặt Nhân Vật")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flipThreshold = 0.05f;

        [Header("Tùy Chọn Con Trỏ Chuột")]
        [SerializeField] private Texture2D customCrosshair;
        [SerializeField] private Vector2 crosshairHotspot = new Vector2(16, 16);

        // Thuộc tính công khai phục vụ hệ thống Skill & Combat
        public Transform AimPivot => aimPivot;
        public Transform FirePoint => firePoint != null ? firePoint : (aimPivot != null ? aimPivot : transform);
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public float AimAngle { get; private set; } = 0f;
        public Vector3 MouseWorldPosition { get; private set; } = Vector3.zero;
        public bool IsFacingLeft => spriteRenderer != null && spriteRenderer.flipX;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureAimHierarchy();

            if (customCrosshair != null)
            {
                Cursor.SetCursor(customCrosshair, crosshairHotspot, CursorMode.Auto);
            }
        }

        private void Update()
        {
            UpdateMousePositionAndAim();
        }

        private void EnsureAimHierarchy()
        {
            if (aimPivot == null)
            {
                Transform found = transform.Find("AimPivot");
                if (found != null)
                {
                    aimPivot = found;
                }
                else
                {
                    GameObject pivotGo = new GameObject("AimPivot");
                    pivotGo.transform.SetParent(transform);
                    pivotGo.transform.localPosition = new Vector3(0, 0.1f, 0);
                    pivotGo.transform.localRotation = Quaternion.identity;
                    aimPivot = pivotGo.transform;
                }
            }

            if (firePoint == null)
            {
                Transform foundFp = aimPivot.Find("FirePoint");
                if (foundFp != null)
                {
                    firePoint = foundFp;
                }
                else
                {
                    GameObject fpGo = new GameObject("FirePoint");
                    fpGo.transform.SetParent(aimPivot);
                    fpGo.transform.localPosition = new Vector3(0.75f, 0, 0);
                    fpGo.transform.localRotation = Quaternion.identity;
                    firePoint = fpGo.transform;
                }
            }
        }

        private void UpdateMousePositionAndAim()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // 1. Chuyển đổi tọa độ chuột từ màn hình sang không gian thế giới (World Space)
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            mouseWorld.z = 0f;
            MouseWorldPosition = mouseWorld;

            if (aimPivot == null) return;

            // 2. Tính toán Vector và Góc quay 360 độ
            Vector2 origin = aimPivot.position;
            Vector2 diff = (Vector2)mouseWorld - origin;

            if (diff.sqrMagnitude > 0.0001f)
            {
                AimDirection = diff.normalized;
                AimAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            }

            // 3. Xoay AimPivot theo góc quay 360 độ
            aimPivot.rotation = Quaternion.Euler(0, 0, AimAngle);

            // 4. Cơ chế chống lộn ngược vũ khí khi quay sang trái (Angle > 90 hoặc < -90)
            bool pointingLeft = Mathf.Abs(AimAngle) > 90f;
            aimPivot.localScale = pointingLeft ? new Vector3(1f, -1f, 1f) : new Vector3(1f, 1f, 1f);

            // 5. Tự động lật mặt nhân vật:
            // Nếu đã có LuminaFourDirectionCharacterAnimator (4 hướng riêng biệt Up/Down/Left/Right),
            // giữ nguyên flipX = false để tránh lỗi lật ngược sprite Left thành Right.
            var fourDir = GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
            if (fourDir != null && fourDir.enabled)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = false;
                return;
            }

            // Fallback cho sprite 2 hướng (chỉ lật khi đang bấm phím di chuyển ngang hoặc đang bấm chuột tấn công)
            float moveX = 0f;
            try
            {
                moveX = Input.GetAxisRaw("Horizontal");
            }
            catch { }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveX = 1f;

            bool isMovingHorizontally = Mathf.Abs(moveX) > 0.05f;
            bool isAttackingOrAiming = Input.GetMouseButton(0) || Input.GetMouseButton(1);

            if (isMovingHorizontally && !isAttackingOrAiming)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (moveX < 0);
                }
                var cvView = GetComponent<ActionRPG.Presentation.Views.CharacterVisualView>();
                if (cvView != null)
                {
                    cvView.SetFacingDirection(moveX < 0 ? -1 : 1);
                }
            }
            else if (isAttackingOrAiming)
            {
                if (spriteRenderer != null)
                {
                    if (AimDirection.x < -flipThreshold)
                    {
                        spriteRenderer.flipX = true;
                        var cvView = GetComponent<ActionRPG.Presentation.Views.CharacterVisualView>();
                        if (cvView != null) cvView.SetFacingDirection(-1);
                    }
                    else if (AimDirection.x > flipThreshold)
                    {
                        spriteRenderer.flipX = false;
                        var cvView = GetComponent<ActionRPG.Presentation.Views.CharacterVisualView>();
                        if (cvView != null) cvView.SetFacingDirection(1);
                    }
                }
            }
        }

        public void SetAimTarget(Vector2 worldTarget)
        {
            if (aimPivot == null) return;
            Vector2 diff = worldTarget - (Vector2)aimPivot.position;
            if (diff.sqrMagnitude > 0.0001f)
            {
                AimDirection = diff.normalized;
                AimAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0, 0, AimAngle);
                bool pointingLeft = Mathf.Abs(AimAngle) > 90f;
                aimPivot.localScale = pointingLeft ? new Vector3(1f, -1f, 1f) : new Vector3(1f, 1f, 1f);
            }
        }

        /// <summary>
        /// Thông báo hành động chiến đấu (đánh thường hoặc tung chiêu) để xoay hướng mặt 4 hướng bám theo con trỏ chuột.
        /// </summary>
        public void NotifyCombatAction(Vector2 aimDir)
        {
            var fourDir = GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
            if (fourDir != null && fourDir.enabled && aimDir.sqrMagnitude > 0.001f)
            {
                fourDir.SetFacingDirection(aimDir);
            }
        }
    }
}
