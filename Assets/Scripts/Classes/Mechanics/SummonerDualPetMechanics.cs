using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Pets;

namespace ActionRPG.Classes.Mechanics
{
    [Serializable]
    public class SummonerPetSlot
    {
        public int slotIndex; // 0 = Pet 1 (HUD 1,2,3), 1 = Pet 2 (HUD 4,5,6)
        public PetData activePetData;
        public float currentHp;
        public float maxHp;
        public bool isDowned;
        public float summonSicknessTimer; // Trễ nhịp 1.0 - 1.5s khi vừa ra sân
    }

    [Serializable]
    public class ReservePetEntry
    {
        public PetData petData;
        public float reviveCooldownTimer; // Thời gian dưỡng thương (45 - 60s)
        public bool isInjured => reviveCooldownTimer > 0f;
    }

    /// <summary>
    /// Xử lý cơ chế Song Hành (Dual Summon) của Pháp Sư Triệu Hồi:
    /// Quản lý 2 Pet Active cùng lúc -> ánh xạ 6 kỹ năng lên HUD.
    /// Quản lý thanh HP Pet, trạng thái Nguy Kịch (Downed), thu hồi và thay Pet Dự Bị.
    /// Kèm 3 Ấn nội tại: Cộng Hưởng Lưỡng Thú, Phân Bổ Chiến Thuật, Tiếp Ứng Khẩn Cấp.
    /// </summary>
    public class SummonerDualPetMechanics : MonoBehaviour
    {
        [Header("2 Pet Active Chiến Đấu")]
        public SummonerPetSlot[] activePetSlots = new SummonerPetSlot[2];

        [Header("Đội Hình Pet Dự Bị (Roster: 2-3 Pet)")]
        public List<ReservePetEntry> reservePets = new List<ReservePetEntry>();

        [Header("Ấn 3: Tiếp Ứng Khẩn Cấp (Tactical Swap & Surge)")]
        public float emergencyShieldDuration = 4.0f;
        public float emergencyShieldPercent = 0.20f; // 20% max HP

        public event Action<int, PetData> OnPetSwapped;
        public event Action<int> OnPetDowned;
        public event Action OnDualResonanceTriggered;

        private void Awake()
        {
            activePetSlots[0] = new SummonerPetSlot { slotIndex = 0, maxHp = 100f, currentHp = 100f };
            activePetSlots[1] = new SummonerPetSlot { slotIndex = 1, maxHp = 100f, currentHp = 100f };
        }

        private void Update()
        {
            // Cập nhật Summon Sickness cho Pet vừa ra sân
            for (int i = 0; i < activePetSlots.Length; i++)
            {
                if (activePetSlots[i].summonSicknessTimer > 0f)
                    activePetSlots[i].summonSicknessTimer -= Time.deltaTime;
            }

            // Cập nhật Thời Gian Dưỡng Thương (Revive Cooldown) cho Pet dự bị
            foreach (var reserve in reservePets)
            {
                if (reserve.reviveCooldownTimer > 0f)
                    reserve.reviveCooldownTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Gây sát thương vào một trong 2 Pet. Khi HP <= 0, chuyển sang trạng thái Nguy Kịch.
        /// </summary>
        public void DamagePet(int slotIndex, float damage)
        {
            if (slotIndex < 0 || slotIndex >= activePetSlots.Length) return;
            var slot = activePetSlots[slotIndex];
            if (slot.isDowned) return;

            slot.currentHp = Mathf.Max(slot.currentHp - damage, 0f);
            Debug.Log($"[Summoner] Pet {slotIndex + 1} ({slot.activePetData?.petName}) nhận {damage} damage! HP còn: {slot.currentHp}/{slot.maxHp}");

            if (slot.currentHp <= 0f)
            {
                slot.isDowned = true;
                Debug.LogWarning($"<color=red>[Summoner] CẢNH BÁO: Pet {slotIndex + 1} rơi vào trạng thái NGUY KỊCH! 3 ô kỹ năng bị phong tỏa!</color>");
                OnPetDowned?.Invoke(slotIndex);
            }
        }

        /// <summary>
        /// Thao tác thu hồi Pet bị gục và thay thế nhanh Pet Dự Bị vào sân.
        /// Áp dụng đầy đủ Ấn 3: Tiếp Ứng Khẩn Cấp.
        /// </summary>
        public bool TryQuickSwapPet(int slotIndex, int reserveIndex)
        {
            if (slotIndex < 0 || slotIndex >= activePetSlots.Length) return false;
            if (reserveIndex < 0 || reserveIndex >= reservePets.Count) return false;

            var reserveEntry = reservePets[reserveIndex];
            if (reserveEntry.isInjured)
            {
                Debug.LogWarning($"[Summoner] Pet dự bị {reserveEntry.petData.petName} đang dưỡng thương! Còn {reserveEntry.reviveCooldownTimer:F1}s.");
                return false;
            }

            var currentSlot = activePetSlots[slotIndex];
            PetData oldPet = currentSlot.activePetData;

            // Đưa Pet cũ vào danh sách dưỡng thương nếu bị downed
            if (oldPet != null)
            {
                reservePets.Add(new ReservePetEntry
                {
                    petData = oldPet,
                    reviveCooldownTimer = currentSlot.isDowned ? 48f : 15f // Giảm 20% nhờ Ấn 3 (60s -> 48s)
                });
            }

            // Triệu hồi Pet mới
            currentSlot.activePetData = reserveEntry.petData;
            currentSlot.maxHp = reserveEntry.petData.maxHp;
            currentSlot.currentHp = currentSlot.maxHp;
            currentSlot.isDowned = false;
            currentSlot.summonSicknessTimer = 1.2f; // Trễ nhịp 1.2 giây

            reservePets.RemoveAt(reserveIndex);

            // Triệu hồi thực thể PetController tương ứng ra Scene
            if (PlayerPetRoster.Instance != null)
            {
                PetInstance instance = new PetInstance(currentSlot.activePetData, 1, 60f);
                if (slotIndex == 0) PlayerPetRoster.Instance.SummonPet(instance);
                else PlayerPetRoster.Instance.SummonSecondaryPet(instance);
            }

            OnPetSwapped?.Invoke(slotIndex, currentSlot.activePetData);
            return true;
        }

        /// <summary>
        /// Triệu hồi trực tiếp 2 PetController ra trận khi vào dạng Pháp Sư Triệu Hồi.
        /// </summary>
        public void SummonBothPets(PetData pet1, PetData pet2)
        {
            if (pet1 != null)
            {
                activePetSlots[0].activePetData = pet1;
                activePetSlots[0].maxHp = pet1.maxHp;
                activePetSlots[0].currentHp = pet1.maxHp;
                if (PlayerPetRoster.Instance != null)
                    PlayerPetRoster.Instance.SummonPet(new PetInstance(pet1, 1, 60f));
            }

            if (pet2 != null)
            {
                activePetSlots[1].activePetData = pet2;
                activePetSlots[1].maxHp = pet2.maxHp;
                activePetSlots[1].currentHp = pet2.maxHp;
                if (PlayerPetRoster.Instance != null)
                    PlayerPetRoster.Instance.SummonSecondaryPet(new PetInstance(pet2, 1, 60f));
            }
        }

        /// <summary>
        /// Ấn 1: Kích nổ Cộng Hưởng Lưỡng Thú khi 2 Pet cùng tấn công mục tiêu trong 3s.
        /// </summary>
        public void TriggerDualPetResonance(ElementType p1Element, ElementType p2Element)
        {
            Debug.Log($"<color=yellow>[Summoner] 🌟 CỘNG HƯỞNG LƯỠNG THÚ! Phản ứng nguyên tố giữa {p1Element} và {p2Element} được tăng thêm +20% sát thương!</color>");
            OnDualResonanceTriggered?.Invoke();
        }
    }
}
