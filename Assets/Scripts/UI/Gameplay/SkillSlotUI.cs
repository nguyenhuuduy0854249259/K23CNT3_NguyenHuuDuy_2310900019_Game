using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ActionRPG.Core;

namespace ActionRPG.UI.Gameplay
{
    /// <summary>
    /// UI component for a single skill slot. Contains icon, cooldown overlay, mana cost label and shortcut key label.
    /// Tự động tìm kiếm các thành phần con (Icon, CooldownOverlay, Key, ManaCost, CDText) để tránh lỗi thiếu tham chiếu ().
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SkillSlotUI : MonoBehaviour
    {
        public string SkillId;               // Assigned at runtime from player skill data
        public Image iconImage;              // Skill icon
        public Image cooldownOverlay;        // Image with fillAmount representing remaining cd (radial)
        public TMP_Text manaCostText;        // Mana cost label
        public TMP_Text shortcutKeyText;     // Shortcut label (e.g., "1", "Q")
        public TMP_Text cdText;              // Numeric cooldown text

        private Button button;

        private void Awake()
        {
            EnsureReferences();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClicked);
            }
        }

        public void EnsureReferences()
        {
            if (button == null) button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();

            if (iconImage == null)
            {
                Transform iconTr = transform.Find("Icon");
                if (iconTr != null) iconImage = iconTr.GetComponent<Image>();
                if (iconImage == null)
                {
                    var imgs = GetComponentsInChildren<Image>(true);
                    foreach (var img in imgs)
                    {
                        if (img.gameObject != gameObject && img != cooldownOverlay)
                        {
                            iconImage = img;
                            break;
                        }
                    }
                }
            }

            if (cooldownOverlay == null)
            {
                Transform cdTr = transform.Find("CooldownOverlay");
                if (cdTr != null) cooldownOverlay = cdTr.GetComponent<Image>();
            }

            if (shortcutKeyText == null)
            {
                Transform keyTr = transform.Find("Key") ?? transform.Find("ShortcutKey") ?? transform.Find("Hotkey");
                if (keyTr != null) shortcutKeyText = keyTr.GetComponent<TMP_Text>();
            }

            if (manaCostText == null)
            {
                Transform manaTr = transform.Find("ManaCost") ?? transform.Find("Mana");
                if (manaTr != null) manaCostText = manaTr.GetComponent<TMP_Text>();
            }

            if (cdText == null)
            {
                Transform cdTextTr = transform.Find("CDText") ?? transform.Find("CooldownText");
                if (cdTextTr != null) cdText = cdTextTr.GetComponent<TMP_Text>();
            }
        }

        public void Initialise(string skillId, Sprite icon, int manaCost, string shortcutKey)
        {
            EnsureReferences();
            SkillId = skillId;
            if (iconImage != null)
            {
                iconImage.preserveAspect = true;
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.color = Color.white;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                    iconImage.enabled = false;
                }
            }
            if (manaCostText != null) 
            {
                manaCostText.text = manaCost > 0 ? manaCost.ToString() : string.Empty;
                manaCostText.enabled = manaCost > 0;
            }
            if (shortcutKeyText != null) 
            {
                shortcutKeyText.text = shortcutKey;
                shortcutKeyText.enabled = !string.IsNullOrEmpty(shortcutKey);
                shortcutKeyText.color = !string.IsNullOrEmpty(skillId) ? new Color(1f, 0.9f, 0.4f) : new Color(0.6f, 0.6f, 0.6f, 0.5f);
            }
            SetCooldown(0, 0);
        }

        public void SetCooldown(float remainingNorm, float remainingSeconds = 0f)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = Mathf.Clamp01(remainingNorm);
                cooldownOverlay.enabled = remainingNorm > 0.001f;
            }
            if (cdText != null)
            {
                if (remainingSeconds > 0.1f)
                {
                    cdText.enabled = true;
                    cdText.text = remainingSeconds >= 1f ? Mathf.CeilToInt(remainingSeconds).ToString() : remainingSeconds.ToString("F1");
                }
                else
                {
                    cdText.enabled = false;
                }
            }
        }

        private void OnClicked()
        {
            if (string.IsNullOrEmpty(SkillId)) return;
            // Broadcast an event so the player controller / combat system can use the skill
            EventBus.RaiseSkillRequested(SkillId);
        }
    }
}
