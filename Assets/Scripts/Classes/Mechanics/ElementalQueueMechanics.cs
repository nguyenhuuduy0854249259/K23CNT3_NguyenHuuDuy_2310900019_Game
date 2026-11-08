using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    public struct ElementalSkillRecipe
    {
        public ElementType baseElement;      // Ô 1: Dạng thức
        public ElementType catalystElement;  // Ô 2: Biến tính phản ứng
        public string skillName;
        public string effectDescription;
        public bool isMonoElement;
    }

    /// <summary>
    /// Xử lý cơ chế Hàng đợi 2 ô (Elemental Queue) của Pháp Sư Nguyên Tố:
    /// Nạp 2 ô -> xác định kỹ năng trong 64 công thức -> tích lũy 3 combo kích hoạt Thiên Tai.
    /// Kèm 3 Ấn nội tại: Lưỡng Nghi Hợp Thành, Mạch Chuỗi Thiên Tai, Linh Thú Xúc Tác.
    /// </summary>
    public class ElementalQueueMechanics : MonoBehaviour
    {
        [Header("Hàng Đợi 2 Ô")]
        public List<ElementType> currentQueue = new List<ElementType>();
        public const int MaxQueueCapacity = 2;
        public bool IsQueueFull => currentQueue != null && currentQueue.Count >= MaxQueueCapacity;

        [Header("Bộ Đếm Thiên Tai (Cataclysm)")]
        public int comboStreak = 0;
        public const int MaxComboForCataclysm = 3;
        public bool isCataclysmReady = false;

        public event Action<ElementalSkillRecipe> OnSkillSynthesized;
        public event Action<ElementType> OnCataclysmTriggered;

        /// <summary>
        /// Nạp 1 hạt nguyên tố vào hàng đợi. Nếu đủ 2 ô sẽ tự động hợp thành chiêu thức.
        /// </summary>
        public bool PushElement(ElementType element)
        {
            if (element == ElementType.None) return false;
            if (currentQueue.Count >= MaxQueueCapacity) return false;

            currentQueue.Add(element);
            Debug.Log($"[ElementalQueue] Đã nạp nguyên tố: {element} ({currentQueue.Count}/2)");

            if (currentQueue.Count >= MaxQueueCapacity)
            {
                SynthesizeAndCast();
            }

            return true;
        }

        /// <summary>
        /// Ấn 3: Pet đánh trúng có 25% tỷ lệ tự động mồi hạt nguyên tố vào hàng đợi.
        /// </summary>
        public void TryPetAutoCatalyst(ElementType petElement)
        {
            if (currentQueue.Count < MaxQueueCapacity && UnityEngine.Random.value <= 0.25f)
            {
                Debug.Log($"<color=yellow>[ElementalQueue] Linh Thú Xúc Tác tự động nạp hạt: {petElement}</color>");
                PushElement(petElement);
            }
        }

        /// <summary>
        /// Kỹ năng Chuyển Đổi Tinh Đẩu (Elemental Shift): đảo vị trí hoặc nhân bản nếu có 1 ô.
        /// </summary>
        public void ElementalShift()
        {
            if (currentQueue.Count == 1)
            {
                // Nhân bản để ép ra chiêu Đơn hệ
                PushElement(currentQueue[0]);
            }
            else if (currentQueue.Count == 2)
            {
                // Đảo ngược thứ tự Ô 1 và Ô 2
                ElementType temp = currentQueue[0];
                currentQueue[0] = currentQueue[1];
                currentQueue[1] = temp;
                Debug.Log($"[ElementalQueue] Đảo vị trí: {currentQueue[0]} - {currentQueue[1]}");
            }
        }

        private void SynthesizeAndCast()
        {
            ElementType e1 = currentQueue[0];
            ElementType e2 = currentQueue[1];
            currentQueue.Clear();

            ElementalSkillRecipe recipe = ResolveRecipe(e1, e2);
            comboStreak++;

            Debug.Log($"<color=cyan>[ElementalQueue] KÍCH HOẠT HỢP THÀNH: {recipe.skillName} ({recipe.effectDescription}) - Combo streak: {comboStreak}/3</color>");

            // Kiểm tra trạng thái Thiên Tai (Ấn 2)
            if (comboStreak >= MaxComboForCataclysm)
            {
                isCataclysmReady = true;
                comboStreak = 0;
                Debug.Log($"<color=orange>[ElementalQueue] ⚡ THIÊN TAI KÍCH PHÁT! Toàn màn hình hệ {e2}!</color>");
                OnCataclysmTriggered?.Invoke(e2);
            }

            OnSkillSynthesized?.Invoke(recipe);
        }

        private ElementalSkillRecipe ResolveRecipe(ElementType e1, ElementType e2)
        {
            ElementalSkillRecipe r = new ElementalSkillRecipe
            {
                baseElement = e1,
                catalystElement = e2,
                isMonoElement = (e1 == e2)
            };

            // Nếu đơn hệ (Ấn 1: +25% sát thương và +25% AoE)
            if (e1 == e2)
            {
                r.skillName = e1 switch
                {
                    ElementType.Fire => "Đại Hỏa Cầu (Explosion nổ cực đại)",
                    ElementType.Water => "Đại Hồng Thủy (Sóng thần quét Slow diện rộng)",
                    ElementType.Electric => "Lôi Phạt Oanh Kích (Cột sét Stun diện rộng)",
                    ElementType.Earth => "Nham Thạch Trận (Vách đá nứt vỡ giáp)",
                    ElementType.Wind => "Cơn Lốc Cuồng Nộ (Lốc xoáy hất tung Knockback)",
                    ElementType.Poison => "Đầm Lầy Tử Thần (Vũng bùn độc DoT cực mạnh)",
                    ElementType.Angel => "Thánh Quang Thiên Giới (Hồi phục lớn + Khiên ánh sáng)",
                    ElementType.Demon => "Vực Thẳm Hư Vô (Hố đen hút quái & nguyền rủa Curse)",
                    _ => $"{e1} Thuần Khiết"
                };
                r.effectDescription = "Đơn hệ cường hóa (+25% Damage & +25% AoE)";
                return r;
            }

            // Đa hệ (Ấn 1: kéo dài debuff +1.5s, tương khắc x2 dmg & +15% crit rate)
            r.skillName = GetDualElementName(e1, e2);
            r.effectDescription = "Đa hệ biến tính (Debuff +1.5s, x2 sát thương khắc chế, +15% Crit)";
            return r;
        }

        private string GetDualElementName(ElementType e1, ElementType e2)
        {
            if (e1 == ElementType.Fire && e2 == ElementType.Water) return "Hơi Nước Bỏng (Burn DoT + Mù quái)";
            if (e1 == ElementType.Fire && e2 == ElementType.Electric) return "Tia Sét Plasma (Xuyên giáp + Sốc điện)";
            if (e1 == ElementType.Fire && e2 == ElementType.Earth) return "Nham Thạch Nung Chảy (Vũng magma Armor Break)";
            if (e1 == ElementType.Fire && e2 == ElementType.Wind) return "Bão Lửa Cuồng Phong (Lốc xoáy thiêu đốt liên tục)";
            if (e1 == ElementType.Fire && e2 == ElementType.Poison) return "Bộc Hỏa Khí Độc (Chuỗi nổ độc tố lan tỏa)";
            if (e1 == ElementType.Fire && e2 == ElementType.Angel) return "Thánh Viêm Rực Sáng (Hồi máu đồng minh + Thiêu quái)";
            if (e1 == ElementType.Fire && e2 == ElementType.Demon) return "Ma Diễm Địa Ngục (Đốt % máu tối đa + Chặn hồi phục)";

            if (e1 == ElementType.Water && e2 == ElementType.Fire) return "Sương Mù Ẩm Nhiệt (Giảm 30% tốc đánh)";
            if (e1 == ElementType.Water && e2 == ElementType.Electric) return "Điện Thủy Lan Tỏa (Chain Stun chuyền 5 quái)";
            if (e1 == ElementType.Water && e2 == ElementType.Earth) return "Đầm Lầy Bùn Lún (Giam chân quái & giảm tốc ra chiêu)";
            if (e1 == ElementType.Water && e2 == ElementType.Wind) return "Xoáy Nước Lớn (Vòng xoáy hút quái vào tâm)";
            if (e1 == ElementType.Water && e2 == ElementType.Poison) return "Thủy Triều Độc Dịch (Sóng nọc độc loang DoT)";
            if (e1 == ElementType.Water && e2 == ElementType.Angel) return "Thánh Tuyền Hồi Sinh (Hồi máu nhịp + Cleanse)";
            if (e1 == ElementType.Water && e2 == ElementType.Demon) return "Vực Nước Ăn Mòn (Hút mana chuyển cho bản thân)";

            if (e1 == ElementType.Electric && e2 == ElementType.Fire) return "Quả Cầu Xung Lôi (Cầu điện nén lửa nổ văng)";
            if (e1 == ElementType.Electric && e2 == ElementType.Water) return "Thủy Lôi Kích (Tê liệt toàn bộ quái trong nước)";
            if (e1 == ElementType.Electric && e2 == ElementType.Earth) return "Từ Trường Khóa Chân (Ghìm chặt quái mang giáp)";
            if (e1 == ElementType.Electric && e2 == ElementType.Wind) return "Bão Điện Từ (Phóng sét ngẫu nhiên)";
            if (e1 == ElementType.Electric && e2 == ElementType.Poison) return "Lôi Độc Tê Liệt (Ngắt chiêu thức niệm chú)";
            if (e1 == ElementType.Electric && e2 == ElementType.Angel) return "Phán Xét Kim Lôi (+20% tốc đánh đồng minh)";
            if (e1 == ElementType.Electric && e2 == ElementType.Demon) return "Hắc Lôi Trừng Phạt (Khắc ấn nguyền rủa Curse)";

            if (e1 == ElementType.Earth && e2 == ElementType.Fire) return "Mưa Đá Lửa (Rải mưa mảnh đá bốc cháy)";
            if (e1 == ElementType.Earth && e2 == ElementType.Water) return "Giáp Bùn Tái Tạo (Lớp bùn hấp thụ sát thương)";
            if (e1 == ElementType.Earth && e2 == ElementType.Electric) return "Gai Đá Nhiễm Điện (Cọc đá phóng điện liên tục)";
            if (e1 == ElementType.Earth && e2 == ElementType.Wind) return "Bão Cát Sa Mạc (Bào mòn + Quái đánh trượt 30%)";
            if (e1 == ElementType.Earth && e2 == ElementType.Poison) return "Địa Gai Tẩm Độc (Chông gai đất trồi đâm tiêm độc)";
            if (e1 == ElementType.Earth && e2 == ElementType.Angel) return "Pháo Đài Ánh Sáng (Vòng chắn tăng tối đa Defense)";
            if (e1 == ElementType.Earth && e2 == ElementType.Demon) return "Hắc Thạch Mộ Phần (Tảng đá đen đè bẹp quái)";

            if (e1 == ElementType.Wind && e2 == ElementType.Fire) return "Phong Đao Bốc Lửa (3 lưỡi đao gió rực lửa)";
            if (e1 == ElementType.Wind && e2 == ElementType.Water) return "Bão Tuyết Lạnh Giá (Đông cứng quái 1.5s)";
            if (e1 == ElementType.Wind && e2 == ElementType.Electric) return "Lốc Xoáy Tích Điện (Hút quái lên không rồi giật điện)";
            if (e1 == ElementType.Wind && e2 == ElementType.Earth) return "Đạn Gió Bụi Đá (Bắn áp lực cao phá giáp diện rộng)";
            if (e1 == ElementType.Wind && e2 == ElementType.Poison) return "Chướng Khí Lan Tràn (Đẩy màn sương độc ra xa)";
            if (e1 == ElementType.Wind && e2 == ElementType.Angel) return "Gió Ban Phước (Thổi bay bùa hại, +25% MoveSpeed)";
            if (e1 == ElementType.Wind && e2 == ElementType.Demon) return "Hắc Phong Áp Chế (Câm lặng quái trong 2s)";

            return $"Dung Hợp {e1} + {e2}";
        }
    }
}
