using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;

namespace ActionRPG.Classes.Mechanics
{
    public enum PriestForm
    {
        Angel,  // Thiên Thần: Hồi phục, Bảo vệ, Khiên, Thanh tẩy
        Demon   // Ác Quỷ: Tấn công, Nguyền rủa, Hút sinh lực, Sát thương bộc phát
    }

    /// <summary>
    /// Xử lý cơ chế Song Ấn Thánh / Ma của Mục Sư Thánh - Ma:
    /// Chuyển đổi giữa 2 Form (Angel Form & Demon Form).
    /// Hoán đổi tức thì toàn bộ 5 ô kỹ năng trên HUD giữa 2 bộ loadout riêng biệt.
    /// </summary>
    public class HolyDarkPriestMechanics : MonoBehaviour
    {
        [Header("Trạng Thái Form Hiện Tại")]
        public PriestForm currentForm = PriestForm.Angel;
        public float stanceSwitchCooldown = 1.0f;
        private float switchCooldownTimer = 0f;

        [Header("5 Kỹ Năng Đang Kích Hoạt")]
        public string[] activeSkills = new string[5];

        public event Action<PriestForm> OnFormSwitched;

        private void Start()
        {
            UpdateActiveSkills();
        }

        private void Update()
        {
            if (switchCooldownTimer > 0f)
                switchCooldownTimer -= Time.deltaTime;
        }

        /// <summary>
        /// Chuyển đổi trạng thái giữa Thiên Thần và Ác Quỷ. Hoán đổi 5 ô kỹ năng.
        /// </summary>
        public bool TogglePriestForm()
        {
            if (switchCooldownTimer > 0f) return false;

            currentForm = (currentForm == PriestForm.Angel) ? PriestForm.Demon : PriestForm.Angel;
            switchCooldownTimer = stanceSwitchCooldown;

            UpdateActiveSkills();

            string formName = (currentForm == PriestForm.Angel) ? "THIÊN THẦN (Angel Form)" : "ÁC QUỶ (Demon Form)";
            Debug.Log($"<color={(currentForm == PriestForm.Angel ? "yellow" : "magenta")}>[HolyDarkPriest] CHUYỂN TRẠNG THÁI SANG: {formName}! 5 ô kỹ năng đã hoán đổi!</color>");

            OnFormSwitched?.Invoke(currentForm);
            return true;
        }

        private void UpdateActiveSkills()
        {
            if (currentForm == PriestForm.Angel)
            {
                activeSkills[0] = "Thánh Kích (Light Bolt - Sát thương nhẹ / Hồi máu)";
                activeSkills[1] = "Cầu Phúc (Bless - Hồi phục theo nhịp HoT)";
                activeSkills[2] = "Quang Thuẫn (Light Shield - Lá chắn ánh sáng)";
                activeSkills[3] = "Thanh Tẩy (Cleanse - Xóa bỏ hiệu ứng bất lợi)";
                activeSkills[4] = "Thánh Vực (Holy Zone - Vùng hồi máu & bảo hộ)";
            }
            else
            {
                activeSkills[0] = "Ma Kích (Dark Bolt - Sát thương + Yếu hóa)";
                activeSkills[1] = "Nguyền Ấn (Curse - Giảm công & thủ của quái)";
                activeSkills[2] = "Hắc Thuẫn (Dark Mark - Tăng sát thương nhận vào)";
                activeSkills[3] = "Huyết Khế (Blood Pact - Hút sinh lực mục tiêu)";
                activeSkills[4] = "Ma Vực (Demon Zone - Vùng DoT & nguyền rủa)";
            }
        }
    }
}
