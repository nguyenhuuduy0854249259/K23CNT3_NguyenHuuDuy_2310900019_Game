using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ActionRPG.Classes;
using ActionRPG.Core;
using ActionRPG.Combat;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.UI
{
    /// <summary>
    /// Quản lý thanh phím tắt kỹ năng (ActionBar UI 5 ô):
    /// Lắng nghe EventBus.OnSkillChanged để cập nhật hình ảnh Icon kỹ năng.
    /// Khớp 100% đặc tả yêu cầu:
    /// Duyệt 5 ô kỹ năng:
    /// - Nếu skillData != null: gán iconImage.sprite = skillData.skillIcon và BẮT BUỘC gán iconImage.enabled = true;
    /// - Nếu skillData == null: gán iconImage.enabled = false;
    /// </summary>
    public class ActionBarUI : MonoBehaviour
    {
        public static ActionBarUI Instance { get; private set; }

        [Header("5 Ô Kỹ Năng (Action Bar Slots)")]
        public Image[] skillIcons = new Image[5];

        private void Awake()
        {
            if (Instance == null) Instance = this;
            AutoFindSkillIconsIfMissing();
        }

        private void OnEnable()
        {
            EventBus.OnSkillChanged += UpdateActionBar;
        }

        private void OnDisable()
        {
            EventBus.OnSkillChanged -= UpdateActionBar;
        }

        private void Start()
        {
            // Tự động làm mới UI ActionBar lúc bắt đầu
            var combat = FindObjectOfType<PlayerSkillCombat2D>();
            if (combat != null)
            {
                UpdateActionBar(combat.GetActiveSkills());
            }
        }

        /// <summary>
        /// Hàm cập nhật UI lắng nghe EventBus.OnSkillChanged.
        /// Vòng lặp duyệt 5 ô kỹ năng:
        /// Nếu skillData != null: gán iconImage.sprite = skillData.skillIcon và BẮT BUỘC gán iconImage.enabled = true;
        /// Nếu skillData == null: gán iconImage.enabled = false;
        /// </summary>
        public void UpdateActionBar(List<SkillData> skills)
        {
            AutoFindSkillIconsIfMissing();
            if (skillIcons == null || skillIcons.Length == 0) return;

            // Nếu không truyền danh sách, tự động lấy từ PlayerSkillCombat2D
            if (skills == null || skills.Count == 0)
            {
                var combat = FindObjectOfType<PlayerSkillCombat2D>();
                if (combat != null) skills = combat.GetActiveSkills();
            }

            for (int i = 0; i < 5; i++)
            {
                if (i >= skillIcons.Length) break;
                Image iconImage = skillIcons[i];
                if (iconImage == null) continue;

                SkillData skillData = (skills != null && i < skills.Count) ? skills[i] : null;

                if (skillData != null)
                {
                    Sprite spr = skillData.skillIcon != null ? skillData.skillIcon : skillData.icon;
                    iconImage.sprite = spr;
                    iconImage.color = Color.white;
                    iconImage.enabled = (spr != null);
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }
            }
        }

        public void AutoFindSkillIconsIfMissing()
        {
            bool hasMissing = false;
            if (skillIcons == null || skillIcons.Length < 5)
            {
                skillIcons = new Image[5];
                hasMissing = true;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (skillIcons[i] == null) { hasMissing = true; break; }
                }
            }

            if (!hasMissing) return;

            // 1. Tìm từ GameplayHUD hiện tại
            var hud = GetComponent<GameplayHUD>() ?? FindObjectOfType<GameplayHUD>();
            if (hud != null)
            {
                hud.AutoFindSkillSlots();
                if (hud.skillSlots != null && hud.skillSlots.Length > 0)
                {
                    int count = Mathf.Min(5, hud.skillSlots.Length);
                    for (int i = 0; i < count; i++)
                    {
                        if (hud.skillSlots[i] != null)
                        {
                            hud.skillSlots[i].EnsureReferences();
                            if (hud.skillSlots[i].iconImage != null)
                            {
                                skillIcons[i] = hud.skillSlots[i].iconImage;
                            }
                        }
                    }
                }
            }

            // 2. Tự động tìm tất cả các ô con (child slots) bên trong GameObject chứa thanh Skill Bar này
            var childSlots = GetComponentsInChildren<SkillSlotUI>(true);
            if (childSlots != null && childSlots.Length > 0)
            {
                var list = new System.Collections.Generic.List<SkillSlotUI>(childSlots);
                list.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                for (int i = 0; i < Mathf.Min(5, list.Count); i++)
                {
                    if (skillIcons[i] == null && list[i] != null)
                    {
                        list[i].EnsureReferences();
                        if (list[i].iconImage != null) skillIcons[i] = list[i].iconImage;
                    }
                }
            }

            // 3. Tìm theo Image con có tên Icon hoặc Button con
            for (int i = 0; i < 5; i++)
            {
                if (skillIcons[i] == null)
                {
                    Transform slotTr = transform.Find($"SkillSlot_{i + 1}") ?? transform.Find($"Slot_{i + 1}") ?? transform.Find($"Slot{i + 1}");
                    if (slotTr != null)
                    {
                        Transform iconTr = slotTr.Find("Icon");
                        if (iconTr != null) skillIcons[i] = iconTr.GetComponent<Image>();
                        if (skillIcons[i] == null) skillIcons[i] = slotTr.GetComponentInChildren<Image>();
                    }
                }
            }
        }
    }
}
