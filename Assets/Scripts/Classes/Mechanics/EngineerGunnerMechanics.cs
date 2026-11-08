using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    public enum TurretType
    {
        Gatling,    // Trụ Liên Thanh: Sát thương vật lý liên tục
        Laser,      // Trụ Laser: Xuyên giáp mục tiêu trâu bò
        EMP         // Trụ Điện Từ: Sóng điện làm chậm và tê liệt diện rộng
    }

    [Serializable]
    public class ActiveTurret
    {
        public TurretType type;
        public Vector2 position;
        public float currentHp;
        public float maxHp;
        public float fireRateModifier = 1.0f;
    }

    /// <summary>
    /// Xử lý cơ chế Xạ Thủ Súng & Cơ Khí Sư:
    /// Quản lý thanh Nhiệt Lượng (Heat / Overheat).
    /// Triển khai tối đa 3 trụ phòng thủ (Gatling, Laser, EMP).
    /// Pet là Trạm bảo trì di động tự động sửa trụ và buff tốc bắn.
    /// Kèm 3 Ấn nội tại: Xung Áp Quá Nhiệt, Trạm Cơ Động Hiệp Đồng, Thợ Rèn Chế Tác.
    /// </summary>
    public class EngineerGunnerMechanics : MonoBehaviour
    {
        [Header("Thanh Nhiệt Lượng (Gun Heat)")]
        public float currentHeat = 0f;
        public float maxHeat = 100f;
        public bool isOverheated = false;
        public float overheatLockoutTimer = 0f;
        private const float OverheatLockoutDuration = 3.5f;

        [Header("Danh Sách Trụ Phòng Thủ (Tối Đa 3 Trụ)")]
        public List<ActiveTurret> activeTurrets = new List<ActiveTurret>();
        public const int MaxTurretLimit = 3;

        [Header("Linh Kiện Cơ Khí")]
        public int mechanicalScraps = 10;

        public event Action OnOverheated;
        public event Action<ActiveTurret> OnTurretDeployed;

        private void Update()
        {
            if (isOverheated)
            {
                overheatLockoutTimer -= Time.deltaTime;
                currentHeat = Mathf.MoveTowards(currentHeat, 0f, 30f * Time.deltaTime);

                if (overheatLockoutTimer <= 0f && currentHeat <= 10f)
                {
                    isOverheated = false;
                    Debug.Log("[Gunner] Súng đã hạ nhiệt hoàn toàn!");
                }
            }
            else
            {
                // Tự động tản nhiệt khi ngừng bắn
                currentHeat = Mathf.MoveTowards(currentHeat, 0f, 15f * Time.deltaTime);
            }
        }

        /// <summary>
        /// Khai hỏa súng. Trả true nếu bắn thành công.
        /// Vùng Nhiệt Cao (70-99%): Tăng 20% sát thương.
        /// </summary>
        public bool Shoot(float heatPerShot, out float damageMultiplier)
        {
            damageMultiplier = 1.0f;
            if (isOverheated) return false;

            currentHeat += heatPerShot;

            // Kiểm tra vùng Nhiệt Cao (Ấn 1: Xung Áp Quá Nhiệt)
            if (currentHeat >= 70f && currentHeat < maxHeat)
            {
                damageMultiplier = 1.20f; // +20% sát thương
            }

            // Kích hoạt Overheat khi chạm vạch 100%
            if (currentHeat >= maxHeat)
            {
                currentHeat = maxHeat;
                isOverheated = true;
                overheatLockoutTimer = OverheatLockoutDuration;

                // Ấn 1: Bung van xả khí nóng đẩy lùi 3m & nhận khiên 15% HP
                Debug.LogWarning("<color=red>[Gunner] ⚠️ OVERHEAT! Súng kẹt 3.5 giây! Bung van xả khí nóng đẩy lùi quái 3m + Nhận khiên 15% HP!</color>");
                OnOverheated?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Triển khai Trụ Phòng Thủ (tối đa 3 trụ). Tiêu hao linh kiện cơ khí.
        /// </summary>
        public bool DeployTurret(TurretType type, Vector2 deployPosition, int scrapCost = 3)
        {
            if (activeTurrets.Count >= MaxTurretLimit)
            {
                Debug.LogWarning("[Engineer] Đã đạt giới hạn tối đa 3 trụ phòng thủ!");
                return false;
            }

            if (mechanicalScraps < scrapCost)
            {
                Debug.LogWarning("[Engineer] Không đủ Linh kiện cơ khí để dựng trụ!");
                return false;
            }

            mechanicalScraps -= scrapCost;
            ActiveTurret turret = new ActiveTurret
            {
                type = type,
                position = deployPosition,
                maxHp = 150f,
                currentHp = 150f
            };
            activeTurrets.Add(turret);

            Debug.Log($"<color=cyan>[Engineer] ĐÃ DỰNG {type} TẠI {deployPosition}! Trụ đang hoạt động: {activeTurrets.Count}/{MaxTurretLimit}</color>");
            OnTurretDeployed?.Invoke(turret);
            return true;
        }

        /// <summary>
        /// Pet - Trạm Bảo Trì Di Động: Tìm trụ thấp máu nhất để hàn sửa và tăng 25% tốc bắn.
        /// </summary>
        public void PetPerformMaintenance()
        {
            if (activeTurrets.Count == 0) return;

            ActiveTurret lowestHpTurret = activeTurrets[0];
            foreach (var t in activeTurrets)
            {
                if (t.currentHp < lowestHpTurret.currentHp)
                    lowestHpTurret = t;
            }

            lowestHpTurret.currentHp = Mathf.Min(lowestHpTurret.currentHp + 40f, lowestHpTurret.maxHp);
            lowestHpTurret.fireRateModifier = 1.25f; // +25% tốc độ bắn
            Debug.Log($"[Engineer] Pet đã sửa trụ {lowestHpTurret.type} (HP: {lowestHpTurret.currentHp}/{lowestHpTurret.maxHp}) và buff +25% tốc bắn!");
        }
    }
}
