using System;
using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Core.Domain.Models;
using ActionRPG.Core.Domain.ValueObjects;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Presentation.Views;

namespace ActionRPG.Core.Bridges
{
    /// <summary>
    /// Presenter / Bridge kết nối Unity Engine (Physics, Input, MonoBehaviours) với Pure C# Domain Logic.
    /// Giữ cho Logic hoàn toàn độc lập, đồng thời điều phối các Views hiển thị đồ họa.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerPresenter : MonoBehaviour
    {
        [Header("Views (Đồ Họa & UI)")]
        [SerializeField] private CharacterVisualView visualView;
        [SerializeField] private HealthBarView healthBarView;
        [SerializeField] private PlayerEquipmentVisualView equipmentVisualView;
        [SerializeField] private WeaponSocketView weaponSocketView;

        [Header("Thông Số Khởi Tạo Logic")]
        [SerializeField] private float initialMaxHp = 100f;
        [SerializeField] private float initialDefense = 5f;
        [SerializeField] private float baseMoveSpeed = 4.5f;
        [SerializeField] private float dashSpeed = 10.5f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashCooldown = 0.8f;

        // Pure C# Domain Models (Trọng tâm kiến trúc)
        public PlayerHealthModel HealthModel { get; private set; }
        public PlayerMovementModel MovementModel { get; private set; }

        [Header("Khu Vực An Toàn (Safe Zone)")]
        [SerializeField] private bool isSafeZone = false;
        public bool IsSafeZone
        {
            get => isSafeZone;
            set
            {
                isSafeZone = value;
                var pCtrl = GetComponent<PlayerController>();
                if (pCtrl != null) pCtrl.isSafeZone = value;
            }
        }

        private Rigidbody2D rb;
        private Hurtbox2D hurtbox;
        private CharacterStats legacyStats;
        private Vector2 currentInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            hurtbox = GetComponent<Hurtbox2D>();
            legacyStats = GetComponent<CharacterStats>();

            if (visualView == null) visualView = GetComponent<CharacterVisualView>();
            if (visualView == null) visualView = gameObject.AddComponent<CharacterVisualView>();

            // 1. Khởi tạo Pure C# Models
            float startHp = legacyStats != null ? legacyStats.maxHp : initialMaxHp;
            float startPDef = legacyStats != null ? legacyStats.TotalPhysicalDef : initialDefense;
            float startMDef = legacyStats != null ? legacyStats.TotalMagicDef : initialDefense;
            float startSpeed = legacyStats != null ? legacyStats.TotalMoveSpeed : baseMoveSpeed;

            HealthModel = new PlayerHealthModel(startHp, startPDef, startMDef);
            MovementModel = new PlayerMovementModel(startSpeed, dashSpeed, dashDuration, dashCooldown);

            // 2. Subscribe sự kiện từ Pure C# Models
            BindHealthEvents();
            BindMovementEvents();
        }

        private void Start()
        {
            // Cập nhật UI lần đầu
            if (healthBarView != null)
            {
                healthBarView.UpdateHealth(HealthModel.CurrentHp, HealthModel.MaxHp);
            }
        }

        private void BindHealthEvents()
        {
            HealthModel.OnHealthChanged += (current, max) =>
            {
                if (healthBarView != null) healthBarView.UpdateHealth(current, max);
                // Bắn EventBus để tương thích toàn bộ hệ thống UI cũ trong game
                EventBus.OnPlayerHpChanged?.Invoke(current, max);

                if (legacyStats != null)
                {
                    legacyStats.currentHp = current;
                }
            };

            HealthModel.OnDamageTaken += (data) =>
            {
                if (visualView != null) visualView.PlayDamageVisual();
                // Bắn text sát thương bay lên
                EventBus.OnShowDamageText?.Invoke(transform.position, data.Amount, data.IsCritical, false);
            };

            HealthModel.OnDied += () =>
            {
                if (visualView != null) visualView.PlayDeathVisual();
                Debug.Log("[PlayerPresenter] Người chơi đã tử trận! Chuẩn bị hồi sinh tại Trụ Đá gần nhất...");
                StartCoroutine(RespawnRoutine());
            };
        }

        private System.Collections.IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(2.5f);
            Vector3 respawnPos = ActionRPG.Core.World.Interactables.Waystone.GetDefaultRespawnPosition();
            transform.position = respawnPos;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            HealthModel.Heal(HealthModel.MaxHp);
            if (legacyStats != null) legacyStats.currentHp = legacyStats.maxHp;
            EventBus.OnShowUIMessage?.Invoke("✨ Bạn đã hồi sinh an toàn tại Trụ Đá!");
        }

        private void BindMovementEvents()
        {
            MovementModel.OnFacingChanged += (facingX) =>
            {
                if (visualView != null) visualView.SetFacingDirection(facingX);
            };

            MovementModel.OnMovementStateChanged += (isMoving) =>
            {
                if (visualView != null) visualView.SetMovingState(isMoving);
            };

            MovementModel.OnDashStarted += () =>
            {
                if (visualView != null) visualView.PlayDashVisual();
                if (hurtbox != null) hurtbox.isInvulnerable = true;
            };

            MovementModel.OnDashEnded += () =>
            {
                if (hurtbox != null) hurtbox.isInvulnerable = false;
            };
        }

        private void Update()
        {
            if (HealthModel.IsDead) return;

            // 1. Đọc Input người chơi
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");

            // Phím cứng WASD / Arrow
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputX = 1f;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputY = -1f;
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputY = 1f;

            currentInput = new Vector2(inputX, inputY);

            // 2. Kích hoạt Dash nếu bấm Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MovementModel.TryStartDash(currentInput.x, currentInput.y);
            }

            // Kích hoạt animation chém khi đánh (nếu chưa có PlayerSkillCombat2D điều phối riêng cho 3 phái)
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) && GetComponent<PlayerSkillCombat2D>() == null)
            {
                TriggerAttackVisual();
            }

            // 3. Cập nhật Timers của Pure C# Model
            MovementModel.UpdateTimers(Time.deltaTime);
        }

        public void TriggerAttackVisual(float duration = 0.2f)
        {
            if (weaponSocketView != null)
            {
                weaponSocketView.PlayAttackSlashAnimation(duration);
            }
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            if (HealthModel.IsDead)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // 4. Pure C# tính toán vận tốc -> Bridge áp dụng vào Rigidbody2D
            var (vx, vy) = MovementModel.ComputeVelocity(currentInput.x, currentInput.y);
            rb.linearVelocity = new Vector2(vx, vy);
        }

        /// <summary>
        /// Điểm nhận sát thương chuẩn kiến trúc mới (từ Hurtbox hoặc Combat)
        /// </summary>
        public void ReceiveDamage(DamageData damageData)
        {
            HealthModel.TakeDamage(damageData);
        }

        /// <summary>
        /// Điểm nhận sát thương tương thích kiểu DamageInfo cũ của dự án
        /// </summary>
        public void ReceiveDamage(DamageInfo info)
        {
            var data = new DamageData(
                info.rawDamage,
                info.isCrit,
                (int)info.element,
                info.knockbackDirection.x,
                info.knockbackDirection.y,
                info.knockbackForce,
                info.damageType
            );

            HealthModel.TakeDamage(data);

            // Áp dụng lực đẩy lùi nếu có
            if (rb != null && info.knockbackForce > 0f)
            {
                rb.AddForce(info.knockbackDirection * info.knockbackForce, ForceMode2D.Impulse);
            }
        }

        private void OnDestroy()
        {
            // Tránh rò rỉ nếu cần
        }
    }
}
