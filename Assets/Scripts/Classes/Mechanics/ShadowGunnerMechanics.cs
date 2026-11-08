using System;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Pets;

namespace ActionRPG.Classes.Mechanics
{
    /// <summary>
    /// Xử lý cơ chế Sát Thủ Bóng Đêm (Shadow Gunner):
    /// 1. Tàng hình chủ động (Stealth 5-10s).
    /// 2. Đòn mở đầu bạo kích cao (Ambush Shot x200-300%).
    /// 3. Bắn sau lưng (Backstab 180°: +35% dmg, +25% crit).
    /// 4. Pet tạo phân thân Decoy khiêu khích 6m.
    /// Kèm 3 Ấn nội tại: Ám Ảnh Trảm Kích, Ảnh Phách Thế Thân, Thợ Săn Bóng Đêm.
    /// </summary>
    public class ShadowGunnerMechanics : MonoBehaviour
    {
        [Header("Trạng Thái Tàng Hình")]
        public bool isStealthed = false;
        public float stealthTimer = 0f;
        public float stealthDuration = 6.0f;
        public float stealthCooldown = 14.0f;
        public float stealthCooldownTimer = 0f;

        [Header("Cờ Đòn Mở Đầu (Ambush Ready)")]
        public bool isAmbushReady = false;

        [Header("Phân Thân Pet (Decoy)")]
        public bool isDecoyActive = false;
        public Vector2 decoyPosition;
        public float decoyTimer = 0f;

        public event Action OnStealthEntered;
        public event Action OnStealthExited;
        public event Action OnDecoySpawned;

        private void Update()
        {
            if (stealthCooldownTimer > 0f)
                stealthCooldownTimer -= Time.deltaTime;

            if (isStealthed)
            {
                stealthTimer -= Time.deltaTime;
                if (stealthTimer <= 0f)
                {
                    ExitStealth();
                }
            }

            if (isDecoyActive)
            {
                decoyTimer -= Time.deltaTime;
                if (decoyTimer <= 0f)
                {
                    isDecoyActive = false;
                    Debug.Log("[ShadowGunner] Phân thân Decoy đã nổ tung kết thúc!");
                }
            }
        }

        /// <summary>
        /// Kích hoạt Tàng Hình. Pet tự động tạo phân thân Decoy tại chỗ (Ấn 2).
        /// </summary>
        public bool EnterStealth()
        {
            if (stealthCooldownTimer > 0f || isStealthed) return false;

            isStealthed = true;
            isAmbushReady = true;
            stealthTimer = stealthDuration;

            // Kích hoạt Ấn 2: Pet tạo phân thân Decoy
            isDecoyActive = true;
            decoyPosition = transform.position;
            decoyTimer = 4.0f;

            if (PlayerPetRoster.Instance != null && PlayerPetRoster.Instance.activePetController != null)
            {
                PlayerPetRoster.Instance.activePetController.SpawnDecoy(decoyTimer);
            }
            else
            {
                PetController pet = FindObjectOfType<PetController>();
                if (pet != null) pet.SpawnDecoy(decoyTimer);
            }

            Debug.Log($"<color=magenta>[ShadowGunner] BƯỚC VÀO BÓNG TỐI! Tàng hình {stealthDuration}s. Pet tạo phân thân Decoy khiêu khích 6m!</color>");
            OnStealthEntered?.Invoke();
            OnDecoySpawned?.Invoke();
            return true;
        }

        public void ExitStealth()
        {
            if (!isStealthed) return;
            isStealthed = false;
            stealthCooldownTimer = stealthCooldown;
            Debug.Log($"[ShadowGunner] Thoát tàng hình. Cooldown: {stealthCooldown}s.");
            OnStealthExited?.Invoke();
        }

        /// <summary>
        /// Kiểm tra góc tấn công sau lưng (Backstab): nửa bán cầu 180° sau lưng mục tiêu.
        /// </summary>
        public static bool IsBackstab(Vector2 attackerPos, Vector2 targetPos, Vector2 targetFacingDirection)
        {
            Vector2 toAttacker = (attackerPos - targetPos).normalized;
            // Góc giữa hướng nhìn mục tiêu và hướng tới người tấn công
            // Nếu góc > 90° (tức dot < 0), người tấn công nằm ở phía sau lưng mục tiêu
            float dot = Vector2.Dot(targetFacingDirection.normalized, toAttacker);
            return dot < 0f;
        }

        /// <summary>
        /// Tính toán sát thương tổng hợp của phát bắn từ bóng tối:
        /// Ambush Shot (x2.5) + Backstab (+35% dmg, +25% crit).
        /// </summary>
        public float CalculateAttackDamage(float baseDamage, bool isBackstab, out bool wasAmbush)
        {
            wasAmbush = false;
            float finalDamage = baseDamage;

            // 1. Kiểm tra Backstab (Ấn 1: Ám Ảnh Trảm Kích +35% dmg)
            if (isBackstab)
            {
                finalDamage *= 1.35f;
                Debug.Log("[ShadowGunner] Đòn đánh thỏa mãn Backstab (+35% sát thương)!");
            }

            // 2. Kiểm tra Ambush Shot (Ấn 3: Thợ Săn Bóng Đêm x2.5 chí mạng)
            if (isAmbushReady)
            {
                wasAmbush = true;
                isAmbushReady = false;
                finalDamage *= 2.5f; // Sát thương mở đầu x250%
                ExitStealth(); // Phá vỡ tàng hình
                Debug.Log($"<color=red>[ShadowGunner] ⚡ ĐÒN MỞ ĐẦU AMBUSH SHOT! Sát thương chí mạng bùng nổ x250%! (Tổng: {finalDamage})</color>");
            }

            return finalDamage;
        }
    }
}
