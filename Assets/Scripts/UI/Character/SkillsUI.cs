using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Classes;
using ActionRPG.Characters;

namespace ActionRPG.UI.Character
{
    /// <summary>
    /// Giao diện danh sách kỹ năng của Class hiện tại:
    /// - Hiển thị danh sách kỹ năng đã mở khóa, icon, tiêu hao Mana, Cooldown, Mô tả và Hệ nguyên tố.
    /// </summary>
    public class SkillsUI : MonoBehaviour
    {
        [Header("UI References")]
        public Transform skillListContainer;
        public GameObject skillItemPrefab;
        public TMP_Text classSkillHeader;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            ClassSystem cs = ClassSystem.Instance;
            if (cs == null) cs = FindObjectOfType<ClassSystem>();
            if (cs == null || cs.currentClass == null) return;

            if (classSkillHeader != null)
                classSkillHeader.text = $"Kỹ Năng: {cs.currentClass.className}";

            if (skillListContainer == null) return;

            // Xóa các item cũ
            for (int i = skillListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(skillListContainer.GetChild(i).gameObject);
            }

            List<SkillData> skills = cs.currentClass.unlockedSkills;
            if (skills == null || skills.Count == 0) return;

            foreach (var skill in skills)
            {
                if (skill == null) continue;
                CreateSkillItem(skill);
            }
        }

        private void CreateSkillItem(SkillData skill)
        {
            GameObject itemObj;
            if (skillItemPrefab != null)
            {
                itemObj = Instantiate(skillItemPrefab, skillListContainer);
            }
            else
            {
                itemObj = new GameObject($"SkillItem_{skill.skillId}", typeof(RectTransform));
                itemObj.transform.SetParent(skillListContainer, false);

                var rt = itemObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 50);

                var img = itemObj.AddComponent<Image>();
                img.color = new Color(0.12f, 0.14f, 0.18f, 0.9f);

                var layout = itemObj.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(8, 8, 4, 4);
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childControlWidth = false;

                // Icon
                var iconObj = new GameObject("Icon", typeof(RectTransform));
                iconObj.transform.SetParent(itemObj.transform, false);
                var iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.sizeDelta = new Vector2(40, 40);
                var iconImg = iconObj.AddComponent<Image>();
                if (skill.icon != null) iconImg.sprite = skill.icon;
                else iconImg.color = new Color(0.2f, 0.7f, 0.9f);

                // Text Description
                var textObj = new GameObject("Info", typeof(RectTransform));
                textObj.transform.SetParent(itemObj.transform, false);
                var textRt = textObj.GetComponent<RectTransform>();
                textRt.sizeDelta = new Vector2(300, 40);
                var txt = textObj.AddComponent<TextMeshProUGUI>();
                txt.fontSize = 13;
                txt.text = $"<b>{skill.skillName}</b> [MP: {skill.manaCost} | CD: {skill.cooldownSeconds}s]\n<size=11><color=#aaaaaa>{skill.description}</color></size>";
            }
        }
    }
}
