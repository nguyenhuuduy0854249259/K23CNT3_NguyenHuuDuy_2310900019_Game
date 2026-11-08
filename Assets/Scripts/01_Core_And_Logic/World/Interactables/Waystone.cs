using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Core;

namespace ActionRPG.Core.World.Interactables
{
    /// <summary>
    /// Trụ Đá Dịch Chuyển & Điểm Hồi Sinh (Waystone / Teleport & Respawn Pillar) chuẩn Sprint 11:
    /// - Đặt tại các phân vùng (Làng Oakhaven, Bìa Rừng Rậm).
    /// - Bấm [F] khi lại gần: Kích hoạt mở khóa, lưu tọa độ làm điểm hồi sinh mặc định khi Player tử trận.
    /// - Đổi màu vệt sáng phát quang (Glow) khi chuyển từ trạng thái chưa mở -> đã kích hoạt.
    /// </summary>
    public class Waystone : MonoBehaviour
    {
        public static readonly List<Waystone> AllWaystones = new List<Waystone>();
        public static Waystone ActiveRespawnWaystone { get; private set; }

        [Header("Nhận Diện Trụ Đá")]
        public string waystoneId = "waystone_village";
        public string waystoneName = "Trụ Đá Làng Oakhaven";
        public bool isUnlocked = false;

        [Header("Cài Đặt Tương Tác")]
        public float interactRadius = 2.2f;
        public Transform respawnPoint;

        [Header("Hiển Thị Visual & Glow")]
        public SpriteRenderer stoneRenderer;
        public SpriteRenderer glowRenderer;
        public Color inactiveColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        public Color activeColor = new Color(0.2f, 0.85f, 1f, 1f);

        private Transform playerTransform;
        private bool isPlayerNearby = false;

        private void Awake()
        {
            if (respawnPoint == null) respawnPoint = transform;
            AllWaystones.Add(this);
            UpdateVisualGlow();
        }

        private void Start()
        {
            // Nếu là trụ đá Làng Tân Thủ, tự động kích hoạt làm điểm hồi sinh mặc định ban đầu
            if (waystoneId == "waystone_village" && ActiveRespawnWaystone == null)
            {
                ActivateWaystone(false);
            }
        }

        private void Update()
        {
            FindPlayerIfNeeded();
            if (playerTransform == null) return;

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool nearby = dist <= interactRadius;

            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                if (isPlayerNearby && !isUnlocked)
                {
                    EventBus.OnShowUIMessage?.Invoke($"Bấm [F] để kích hoạt điểm hồi sinh: {waystoneName}");
                }
            }

            // Người chơi bấm [F] để kích hoạt
            if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
            {
                ActivateWaystone(true);
            }
        }

        public void ActivateWaystone(bool showBanner)
        {
            isUnlocked = true;
            ActiveRespawnWaystone = this;
            UpdateVisualGlow();

            if (showBanner)
            {
                EventBus.OnShowUIMessage?.Invoke($"✨ ĐÃ GHI NHỚ ĐIỂM HỒI SINH: {waystoneName.ToUpper()}!");
                EventBus.OnPlaySound?.Invoke("waystone");
            }

            Debug.Log($"<color=cyan>[Waystone] Điểm hồi sinh tích cực hiện tại: {waystoneName} ({transform.position})</color>");
        }

        private void UpdateVisualGlow()
        {
            if (glowRenderer != null)
            {
                glowRenderer.color = isUnlocked ? activeColor : inactiveColor;
            }
            else if (stoneRenderer != null)
            {
                stoneRenderer.color = isUnlocked ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            }
        }

        private void FindPlayerIfNeeded()
        {
            if (playerTransform != null) return;
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        /// <summary>
        /// Lấy tọa độ hồi sinh mặc định cho Player khi máu về 0.
        /// </summary>
        public static Vector3 GetDefaultRespawnPosition()
        {
            if (ActiveRespawnWaystone != null && ActiveRespawnWaystone.respawnPoint != null)
            {
                return ActiveRespawnWaystone.respawnPoint.position;
            }

            // Fallback nếu chưa mở khóa trụ đá nào
            return Vector3.zero;
        }

        private void OnDestroy()
        {
            AllWaystones.Remove(this);
            if (ActiveRespawnWaystone == this)
            {
                ActiveRespawnWaystone = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isUnlocked ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
