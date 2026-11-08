using System;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Pets
{
    /// <summary>
    /// Cho phép Thú Cưng sinh sống hoang dã dã ngoại trong Map (Map 2):
    /// - Tự do đi dạo nhẹ nhàng trong bán kính quanh điểm xuất hiện (Home Position).
    /// - Phát hiện Người Chơi đến gần: dừng lại, quay mặt chào đón và hiện gợi ý tương tác.
    /// - Nhấn phím [E] hoặc [F] khi lại gần để kết giao và thu nhận Pet vào Roster!
    /// - Hỗ trợ tương thích đồng bộ với hệ thống Thuần Hóa PetCaptureSystem.
    /// </summary>
    [RequireComponent(typeof(PetController))]
    [RequireComponent(typeof(PetAI))]
    public class WildPetInteractable : MonoBehaviour
    {
        [Header("Thông Tin Thú Cưng Hoang Dã")]
        public string wildTitle = "Linh Thú Hoang Dã";
        public float interactionRadius = 2.2f;

        [Header("Đi Dạo Quanh Khu Vực (Wander)")]
        public float wanderRadius = 2.5f;
        public float wanderSpeed = 1.2f;
        public float minIdleTime = 2.0f;
        public float maxIdleTime = 4.5f;

        private PetController petController;
        private PetAI petAI;
        private Rigidbody2D rb;
        private Vector2 homePosition;
        private Vector2 targetPosition;
        private bool isWalking = false;
        private float stateTimer = 0f;
        private Transform playerTransform;
        private bool isRecruited = false;
        private bool isPlayerNearby = false;

        private void Awake()
        {
            petController = GetComponent<PetController>();
            petAI = GetComponent<PetAI>();
            rb = GetComponent<Rigidbody2D>();

            // Đảm bảo đánh dấu là thú hoang dã
            if (petController != null) petController.isWild = true;
            if (petAI != null) petAI.isWild = true;

            homePosition = transform.position;
            stateTimer = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
        }

        private void Start()
        {
            if (petController != null && petController.petData != null)
            {
                wildTitle = petController.petData.petName;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            if (isRecruited) return;

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            float distToPlayer = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : 999f;
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = distToPlayer <= interactionRadius;

            // Hiển thị thông báo khi người chơi lần đầu bước vào phạm vi
            if (!wasNearby && isPlayerNearby)
            {
                string petName = petController?.petData?.petName ?? wildTitle;
                EventBus.OnShowUIMessage?.Invoke($"✨ Phát hiện {petName}! Nhấn [E] để thu nhận làm Bạn Đồng Hành!");
            }

            // Nhấn [E] hoặc [F] khi lại gần để thu nhận Pet
            if (isPlayerNearby && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)))
            {
                RecruitPet();
                return;
            }

            // Nếu người chơi ở gần, Pet dừng lại và quay mặt nhìn Người Chơi
            if (isPlayerNearby)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                Vector2 lookDir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
                SendMessage("UpdateAnimatorState", new object[] { lookDir, false }, SendMessageOptions.DontRequireReceiver);
                return;
            }

            // Di chuyển đi dạo tự nhiên trong bán kính homePosition
            UpdateWandering();
        }

        private void UpdateWandering()
        {
            stateTimer -= Time.deltaTime;

            if (isWalking)
            {
                Vector2 currentPos = transform.position;
                Vector2 dir = (targetPosition - currentPos).normalized;
                float dist = Vector2.Distance(currentPos, targetPosition);

                if (dist <= 0.15f || stateTimer <= 0f)
                {
                    // Dừng bước, chuyển sang nghỉ ngơi
                    isWalking = false;
                    stateTimer = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                    if (petAI != null) petAI.UpdateAnimatorState(Vector2.zero, false);
                }
                else
                {
                    if (rb != null) rb.linearVelocity = dir * wanderSpeed;
                    if (petAI != null) petAI.UpdateAnimatorState(dir, true);
                }
            }
            else
            {
                if (stateTimer <= 0f)
                {
                    // Chọn một điểm ngẫu nhiên mới trong vòng wanderRadius quanh homePosition
                    Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
                    targetPosition = homePosition + randomOffset;
                    isWalking = true;
                    stateTimer = UnityEngine.Random.Range(1.5f, 3.5f);
                }
            }
        }

        /// <summary>
        /// Thu nhận Pet vào Roster của người chơi
        /// </summary>
        public void RecruitPet()
        {
            if (isRecruited) return;
            isRecruited = true;

            PetData data = petController != null ? petController.petData : null;
            string petName = data != null ? data.petName : wildTitle;

            Debug.Log($"<color=green>[Wild Pet] Người chơi đã kết giao và thu nhận {petName} tại Map 2!</color>");
            EventBus.OnShowUIMessage?.Invoke($"🎉 Chúc mừng! Bạn đã thu nhận Linh Thú {petName} làm Bạn Đồng Hành!");

            if (PlayerPetRoster.Instance != null && data != null)
            {
                PetInstance newInstance = new PetInstance(data, 1, 65f);
                PlayerPetRoster.Instance.AddPet(newInstance);
            }

            if (petController != null) petController.PlaySummonEffect();

            // Nếu người chơi chưa có Pet xuất chiến thì biến thành Pet theo sau, ngược lại ẩn về Roster
            if (playerTransform != null && PlayerPetRoster.Instance != null && PlayerPetRoster.Instance.activePetController == null)
            {
                if (petController != null) petController.isWild = false;
                if (petAI != null)
                {
                    petAI.isWild = false;
                    petAI.SetOwner(playerTransform);
                }
                Destroy(this);
            }
            else
            {
                Destroy(gameObject, 0.4f);
            }
        }
    }
}
