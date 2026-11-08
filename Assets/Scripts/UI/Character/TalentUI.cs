using UnityEngine;
using TMPro;
using ActionRPG.Characters;
using ActionRPG.Classes;

namespace ActionRPG.UI.Character
{
    /// <summary>
    /// Giao diện hiển thị 3 Ấn Nội Tại Cốt Lõi (Passive Seals) của Class:
    /// - Hiển thị tên và hiệu ứng chi tiết của 3 Ấn theo chuẩn tài liệu thiết kế Lumina.
    /// </summary>
    public class TalentUI : MonoBehaviour
    {
        [Header("Tiêu Đề & Class")]
        public TMP_Text classHeaderTitle;
        public TMP_Text mechanicDescText;

        [Header("Ấn 1")]
        public TMP_Text seal1Title;
        public TMP_Text seal1Desc;

        [Header("Ấn 2")]
        public TMP_Text seal2Title;
        public TMP_Text seal2Desc;

        [Header("Ấn 3")]
        public TMP_Text seal3Title;
        public TMP_Text seal3Desc;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            ClassSystem cs = ClassSystem.Instance;
            if (cs == null) cs = FindObjectOfType<ClassSystem>();
            if (cs == null || cs.currentClass == null) return;

            CharacterClassData cd = cs.currentClass;

            if (classHeaderTitle != null)
                classHeaderTitle.text = $"3 Ấn Nội Tại: {cd.className}";

            if (mechanicDescText != null)
                mechanicDescText.text = $"<color=#70d6ff>Cơ chế: {cd.specialMechanic}</color>\n{cd.mechanicDescription}";

            // Ấn 1
            if (seal1Title != null) seal1Title.text = string.IsNullOrEmpty(cd.passiveSeal1Name) ? "Ấn 1: Chưa Khai Mở" : cd.passiveSeal1Name;
            if (seal1Desc != null) seal1Desc.text = cd.passiveSeal1Desc;

            // Ấn 2
            if (seal2Title != null) seal2Title.text = string.IsNullOrEmpty(cd.passiveSeal2Name) ? "Ấn 2: Chưa Khai Mở" : cd.passiveSeal2Name;
            if (seal2Desc != null) seal2Desc.text = cd.passiveSeal2Desc;

            // Ấn 3
            if (seal3Title != null) seal3Title.text = string.IsNullOrEmpty(cd.passiveSeal3Name) ? "Ấn 3: Chưa Khai Mở" : cd.passiveSeal3Name;
            if (seal3Desc != null) seal3Desc.text = cd.passiveSeal3Desc;
        }
    }
}
