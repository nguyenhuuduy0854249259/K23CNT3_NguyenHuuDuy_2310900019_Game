using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    [Serializable]
    public class ActiveSpiritEntity
    {
        public int entityId;
        public ElementType element;
        public bool isFusion;
        public float remainingLifetime;
        public float attackInterval = 1.0f;
        public float attackTimer;
        public Transform currentTarget;
    }

    /// <summary>
    /// Xử lý cơ chế Xạ Thủ Tinh Linh (Spirit Archer):
    /// 1. Tài nguyên Charge Linh Thể: Tối đa 3 Charge, hồi 1 Charge mỗi 4 giây.
    /// 2. Nhấn nút: Tiêu hao 1 Charge gọi Linh Thể thường tự truy đuổi mục tiêu (8-10s).
    /// 3. Giữ nút: Tiêu hao 3 Charge kích hoạt Hợp Thể (Spirit Fusion) tạo Linh Thể khổng lồ x3 sát thương.
    /// 4. Linh Thể kế thừa hiệu ứng theo Hệ Nguyên Tố của Pet Active.
    /// Kèm 3 Ấn nội tại: Tuần Hoàn Tinh Phách, Đồng Điệu Linh Tố, Kế Thừa Linh Thú.
    /// </summary>
    public class SpiritArcherMechanics : MonoBehaviour
    {
        [Header("Tài Nguyên Charge Linh Thể")]
        public int currentCharges = 3;
        public const int MaxCharges = 3;
        public float rechargeTimer = 0f;
        public float baseRechargeDuration = 4.0f;

        [Header("Danh Sách Linh Thể Đang Hoạt Động")]
        public List<ActiveSpiritEntity> activeSpirits = new List<ActiveSpiritEntity>();
        private int entityCounter = 0;

        [Header("Ấn 1: Tuần Hoàn Tinh Phách (Buff Hợp Thể)")]
        public bool hasFusionSpeedBuff = false;
        public float fusionSpeedBuffTimer = 0f;

        public event Action<ActiveSpiritEntity> OnSpiritSpawned;
        public event Action<int> OnChargeChanged;

        private void Update()
        {
            // Tự động hồi phục Charge theo chu kỳ
            if (currentCharges < MaxCharges)
            {
                rechargeTimer += Time.deltaTime;
                if (rechargeTimer >= baseRechargeDuration)
                {
                    currentCharges = Mathf.Min(currentCharges + 1, MaxCharges);
                    rechargeTimer = 0f;
                    Debug.Log($"[SpiritArcher] Hồi Charge: {currentCharges}/{MaxCharges}");
                    OnChargeChanged?.Invoke(currentCharges);
                }
            }

            // Cập nhật vòng đời các Linh Thể đang bay trên sân
            for (int i = activeSpirits.Count - 1; i >= 0; i--)
            {
                var spirit = activeSpirits[i];
                spirit.remainingLifetime -= Time.deltaTime;

                if (spirit.remainingLifetime <= 0f)
                {
                    Debug.Log($"[SpiritArcher] Linh Thể #{spirit.entityId} ({spirit.element}) đã hết thời gian tồn tại!");
                    activeSpirits.RemoveAt(i);
                }
            }

            if (hasFusionSpeedBuff)
            {
                fusionSpeedBuffTimer -= Time.deltaTime;
                if (fusionSpeedBuffTimer <= 0f)
                    hasFusionSpeedBuff = false;
            }
        }

        /// <summary>
        /// Nhấn nút: Tiêu hao 1 Charge để triệu hồi 1 Linh Thể thường tự tìm quái (8-10s).
        /// </summary>
        public bool TrySpawnRegularSpirit(ElementType petElement)
        {
            if (currentCharges <= 0)
            {
                Debug.LogWarning("[SpiritArcher] Không đủ điểm Charge để gọi Linh Thể!");
                return false;
            }

            currentCharges--;
            OnChargeChanged?.Invoke(currentCharges);

            ActiveSpiritEntity spirit = new ActiveSpiritEntity
            {
                entityId = ++entityCounter,
                element = petElement,
                isFusion = false,
                remainingLifetime = 9.0f,
                attackInterval = 0.8f
            };
            activeSpirits.Add(spirit);

            Debug.Log($"<color=cyan>[SpiritArcher] GỌI LINH THỂ #{spirit.entityId} HỆ {petElement}! Tự truy đuổi mục tiêu trong 9s. (Charge: {currentCharges}/3)</color>");
            OnSpiritSpawned?.Invoke(spirit);
            return true;
        }

        /// <summary>
        /// Giữ nút khi đủ 3 Charge: Tiêu hao toàn bộ 3 Charge để tạo Linh Thể Hợp Thể (x3 sát thương).
        /// Kích hoạt Ấn 1: Tăng 20% MoveSpeed và miễn nhiễm knockback/slow trong 5s.
        /// </summary>
        public bool TryTriggerSpiritFusion(ElementType petElement)
        {
            if (currentCharges < MaxCharges)
            {
                Debug.LogWarning($"[SpiritArcher] Chưa đủ 3 Charge để Hợp Thể! Hiện có: {currentCharges}/3");
                return false;
            }

            currentCharges = 0;
            rechargeTimer = 0f;
            OnChargeChanged?.Invoke(currentCharges);

            // Kích hoạt Ấn 1: Thân Linh Hợp Nhất
            hasFusionSpeedBuff = true;
            fusionSpeedBuffTimer = 5.0f;

            ActiveSpiritEntity fusionSpirit = new ActiveSpiritEntity
            {
                entityId = ++entityCounter,
                element = petElement,
                isFusion = true,
                remainingLifetime = 12.0f,
                attackInterval = 1.2f
            };
            activeSpirits.Add(fusionSpirit);

            Debug.Log($"<color=yellow>[SpiritArcher] ⚡ HỢP THỂ LINH THỂ KHỔNG LỒ (Spirit Fusion)! Sát thương x3, uy lực hệ {petElement}! Nhận buff Thân Linh Hợp Nhất (+20% MoveSpeed 5s).</color>");
            OnSpiritSpawned?.Invoke(fusionSpirit);
            return true;
        }

        /// <summary>
        /// Kỹ năng Thu Hồi Linh Phách (Soul Recall): Thu hồi toàn bộ Linh Thể để hoàn trả Charge và tạo khiên.
        /// </summary>
        public int SoulRecall()
        {
            int recalledCount = activeSpirits.Count;
            if (recalledCount == 0) return 0;

            activeSpirits.Clear();
            currentCharges = Mathf.Min(currentCharges + recalledCount, MaxCharges);
            OnChargeChanged?.Invoke(currentCharges);

            Debug.Log($"[SpiritArcher] Thu hồi thành công {recalledCount} Linh Thể! Hồi phục Charge: {currentCharges}/3.");
            return recalledCount;
        }
    }
}
