using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.UI.Character
{
    /// <summary>
    /// Hiển thị toàn diện chỉ số và thông tin nhân vật:
    /// - Thông tin: Avatar, Tên, Cấp độ, Class, HP, Mana, EXP
    /// - Thuộc tính: STR, DEX, INT, VIT
    /// - Chiến đấu: ATK, DEF, Crit Rate, Crit Damage, Atk Speed, Move Speed, Nguyên tố
    /// </summary>
    public class CharacterStatsUI : MonoBehaviour
    {
        [Header("Thông Tin Cơ Bản")]
        public Image avatarImage;
        public TMP_Text nameText;
        public TMP_Text classText;
        public TMP_Text levelText;
        public TMP_Text hpText;
        public TMP_Text manaText;
        public TMP_Text expText;

        [Header("Thuộc Tính Gốc (Attributes)")]
        public TMP_Text strText;
        public TMP_Text dexText;
        public TMP_Text intText;
        public TMP_Text vitText;

        [Header("Chỉ Số Chiến Đấu (Combat Stats)")]
        public TMP_Text atkText;
        public TMP_Text defText;
        public TMP_Text physicalAtkText;
        public TMP_Text magicAtkText;
        public TMP_Text physicalDefText;
        public TMP_Text magicDefText;
        public TMP_Text critRateText;
        public TMP_Text critDamageText;
        public TMP_Text atkSpeedText;
        public TMP_Text moveSpeedText;
        public TMP_Text elementText;

        private void OnEnable()
        {
            EventBus.OnPlayerHpChanged += OnHpChanged;
            EventBus.OnPlayerMpChanged += OnMpChanged;
            EventBus.OnPlayerExpChanged += OnExpChanged;
            EventBus.OnPlayerLevelUp += OnLevelUp;
            EventBus.OnClassChanged += OnClassChanged;
            EventBus.OnStatsChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            EventBus.OnPlayerHpChanged -= OnHpChanged;
            EventBus.OnPlayerMpChanged -= OnMpChanged;
            EventBus.OnPlayerExpChanged -= OnExpChanged;
            EventBus.OnPlayerLevelUp -= OnLevelUp;
            EventBus.OnClassChanged -= OnClassChanged;
            EventBus.OnStatsChanged -= Refresh;
        }

        private void OnHpChanged(float cur, float max) => SetText(hpText, $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}");
        private void OnMpChanged(float cur, float max) => SetText(manaText, $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}");
        private void OnExpChanged(int cur, int max) => SetText(expText, $"{cur} / {max}");
        private void OnLevelUp(int lvl) => SetText(levelText, $"Cấp {lvl}");
        private void OnClassChanged(string cName) => SetText(classText, cName);

        public void Refresh()
        {
            CharacterStats stats = CharacterStats.PlayerInstance;
            if (stats == null) stats = FindObjectOfType<CharacterStats>();
            if (stats == null) return;

            ClassSystem cs = ClassSystem.Instance;
            if (cs == null) cs = FindObjectOfType<ClassSystem>();

            // Basic Info
            SetText(nameText, "Hiệp Sĩ Lumina");
            if (cs != null && cs.currentClass != null)
            {
                SetText(classText, cs.currentClass.className);
                if (avatarImage != null && cs.currentClass.classIcon != null)
                    avatarImage.sprite = cs.currentClass.classIcon;
            }
            else
            {
                SetText(classText, "Tân Thủ (Novice)");
            }

            SetText(levelText, $"Cấp {stats.level}");
            SetText(hpText, $"{Mathf.CeilToInt(stats.currentHp)} / {Mathf.CeilToInt(stats.maxHp)}");
            SetText(manaText, $"{Mathf.CeilToInt(stats.currentMp)} / {Mathf.CeilToInt(stats.maxMp)}");
            SetText(expText, $"{stats.currentExp} / {stats.maxExp}");

            // Attributes
            SetText(strText, stats.TotalStr.ToString());
            SetText(dexText, stats.TotalDex.ToString());
            SetText(intText, stats.TotalInt.ToString());
            SetText(vitText, stats.TotalVit.ToString());

            // Combat Stats
            SetText(atkText, $"Vật: {stats.TotalPhysicalAtk:F0} | Phép: {stats.TotalMagicAtk:F0}");
            SetText(defText, $"Vật: {stats.TotalPhysicalDef:F0} | Phép: {stats.TotalMagicDef:F0}");
            SetText(physicalAtkText, $"{stats.TotalPhysicalAtk:F0}");
            SetText(magicAtkText, $"{stats.TotalMagicAtk:F0}");
            SetText(physicalDefText, $"{stats.TotalPhysicalDef:F0}");
            SetText(magicDefText, $"{stats.TotalMagicDef:F0}");
            SetText(critRateText, $"{(stats.TotalCritRate * 100f):F1}%");
            SetText(critDamageText, $"{(stats.TotalCritDamage * 100f):F0}%");
            SetText(atkSpeedText, $"{stats.TotalAttackSpeed:F1}/s");
            SetText(moveSpeedText, $"{stats.TotalMoveSpeed:F1}");
            SetText(elementText, stats.characterElement.ToString());
        }

        private void SetText(TMP_Text label, string value)
        {
            if (label != null) label.text = value;
        }
    }
}
