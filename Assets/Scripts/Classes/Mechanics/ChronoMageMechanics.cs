using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    public struct PositionSnapshot
    {
        public float timestamp;
        public Vector2 position;
    }

    /// <summary>
    /// Xử lý cơ chế Thao Túng Thời Không của Pháp Sư Thời Không:
    /// 1. Slow: Giảm tốc độ di chuyển và tốc độ hành động.
    /// 2. Stasis: Đóng băng mục tiêu, nén sát thương gánh chịu và kích nổ khi kết thúc.
    /// 3. Reverse: Lưu vết vị trí trong 3 giây và khôi phục tức thì về quá khứ.
    /// Kèm 3 Ấn nội tại: Sụp Đổ Tuyến Tính, Nghịch Đảo Nhân Quả, Linh Thú Hiệp Đồng.
    /// </summary>
    public class ChronoMageMechanics : MonoBehaviour
    {
        [Header("Bộ Nhớ Vết Vị Trí (Reverse History Buffer - 3 Giây)")]
        private List<PositionSnapshot> playerPositionHistory = new List<PositionSnapshot>();
        private const float HistoryDuration = 3.0f;

        [Header("Trạng Thái Nén Sát Thương (Stasis Damage Accumulation)")]
        public Dictionary<int, float> stasisAccumulatedDamage = new Dictionary<int, float>();

        [Header("Ấn 2: Nghịch Đảo Nhân Quả (Causality Reversal)")]
        public bool hasPhaseShift = false;
        public float phaseShiftTimer = 0f;
        private const float PhaseShiftDuration = 3.0f;

        private void Update()
        {
            RecordPositionSnapshot();

            if (hasPhaseShift)
            {
                phaseShiftTimer -= Time.deltaTime;
                if (phaseShiftTimer <= 0f)
                    hasPhaseShift = false;
            }
        }

        private void RecordPositionSnapshot()
        {
            float now = Time.time;
            playerPositionHistory.Add(new PositionSnapshot { timestamp = now, position = transform.position });

            // Xóa các mốc cũ hơn 3 giây
            while (playerPositionHistory.Count > 0 && now - playerPositionHistory[0].timestamp > HistoryDuration)
            {
                playerPositionHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Kích hoạt cơ chế Reverse (Nghịch chuyển): Đưa vị trí về tọa độ 3s trước.
        /// Kích hoạt Ấn 2: Lệch Pha Không Gian (+20% MoveSpeed, hoàn 15% mana).
        /// </summary>
        public bool TriggerSpatialRewind()
        {
            if (playerPositionHistory.Count == 0) return false;

            Vector2 pastPosition = playerPositionHistory[0].position;
            transform.position = pastPosition;

            // Áp dụng Ấn 2: Nghịch Đảo Nhân Quả
            hasPhaseShift = true;
            phaseShiftTimer = PhaseShiftDuration;

            Debug.Log($"<color=cyan>[ChronoMage] NGHỊCH CHUYỂN KHÔNG GIAN! Trở về tọa độ {pastPosition} của 3 giây trước! Nhận buff Lệch Pha (+20% MoveSpeed, hoàn 15% mana).</color>");
            return true;
        }

        /// <summary>
        /// Nén sát thương khi mục tiêu đang trong trạng thái Stasis (Ngưng Đọng).
        /// </summary>
        public void AccumulateStasisDamage(int targetInstanceId, float incomingDamage)
        {
            if (!stasisAccumulatedDamage.ContainsKey(targetInstanceId))
                stasisAccumulatedDamage[targetInstanceId] = 0f;

            stasisAccumulatedDamage[targetInstanceId] += incomingDamage;
            Debug.Log($"[ChronoMage] Nén sát thương Stasis lên mục tiêu {targetInstanceId}: +{incomingDamage} (Tổng nén: {stasisAccumulatedDamage[targetInstanceId]})");
        }

        /// <summary>
        /// Kết thúc Stasis: Bùng nổ toàn bộ sát thương nén + Thêm 15% sát thương từ Ấn 1 (Sụp Đổ Tuyến Tính).
        /// </summary>
        public float ReleaseStasisDamage(int targetInstanceId)
        {
            if (!stasisAccumulatedDamage.ContainsKey(targetInstanceId)) return 0f;

            float totalDamage = stasisAccumulatedDamage[targetInstanceId];
            stasisAccumulatedDamage.Remove(targetInstanceId);

            // Ấn 1: Sụp Đổ Tuyến Tính cộng thêm 15% tổng sát thương đã chịu
            float bonusCollapseDamage = totalDamage * 0.15f;
            float finalBurst = totalDamage + bonusCollapseDamage;

            Debug.Log($"<color=orange>[ChronoMage] 💥 BÙNG NỔ THỜI KHÔNG! Xả {totalDamage} + {bonusCollapseDamage} (Ấn 1: 15%) = {finalBurst} sát thương!</color>");
            return finalBurst;
        }
    }
}
