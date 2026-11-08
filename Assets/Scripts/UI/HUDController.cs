using UnityEngine;
using UnityEngine.UI;
using ActionRPG.Core;
using ActionRPG.Characters;
using ActionRPG.Items;
using ActionRPG.Pets;

namespace ActionRPG.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Thanh Máu, Năng Lượng & Kinh Nghiệm")]
        public Image hpBarFill;
        public Text hpText;
        public Image mpBarFill;
        public Text mpText;
        public Image expBarFill;
        public Text levelText;
        public Text classNameText;
        public Text goldText;

        [Header("Thanh Độ No / Đói")]
        public Image hungerBarFill;
        public Text hungerText;
        public Text hungerStatusText;

        [Header("Thanh Máu Thú Cưng")]
        public GameObject petHudPanel;
        public Image petHpBarFill;
        public Text petSkillText;

        [Header("Ô Thu Phục Thú Cưng (Mở Khóa Cấp 15 + Chuyển Class)")]
        public GameObject petCaptureSlotPanel;
        public Text petCaptureKeyText;
        public Text petScrollCountText;
        public Image petCaptureSlotIcon;

        private void Start()
        {
            // Ban đầu người chơi chưa có pet, ẩn thanh máu pet
            if (petHudPanel != null) petHudPanel.SetActive(false);

            EnsurePetCaptureSlotUI();
            UpdatePetCaptureSlot();
        }

        private void OnEnable()
        {
            EventBus.OnPlayerHpChanged += UpdateHp;
            EventBus.OnPlayerMpChanged += UpdateMp;
            EventBus.OnPlayerExpChanged += UpdateExp;
            EventBus.OnPlayerLevelUp += OnLevelChangedHandler;
            EventBus.OnHungerChanged += UpdateHunger;
            EventBus.OnClassChanged += OnClassChangedHandler;
            EventBus.OnPetHpChanged += UpdatePetHp;
            EventBus.OnPetSkillTriggered += UpdatePetSkill;
            EventBus.OnInventoryUpdated += OnInventoryUpdatedHandler;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerHpChanged -= UpdateHp;
            EventBus.OnPlayerMpChanged -= UpdateMp;
            EventBus.OnPlayerExpChanged -= UpdateExp;
            EventBus.OnPlayerLevelUp -= OnLevelChangedHandler;
            EventBus.OnHungerChanged -= UpdateHunger;
            EventBus.OnClassChanged -= OnClassChangedHandler;
            EventBus.OnPetHpChanged -= UpdatePetHp;
            EventBus.OnPetSkillTriggered -= UpdatePetSkill;
            EventBus.OnInventoryUpdated -= OnInventoryUpdatedHandler;
        }

        private void OnLevelChangedHandler(int level)
        {
            UpdateLevel(level);
            UpdatePetCaptureSlot();
        }

        private void OnClassChangedHandler(string className)
        {
            UpdateClassName(className);
            UpdatePetCaptureSlot();
        }

        private void OnInventoryUpdatedHandler()
        {
            UpdateGold();
            UpdatePetCaptureSlot();
        }

        private void UpdateHp(float current, float max)
        {
            if (hpBarFill != null) hpBarFill.fillAmount = max > 0 ? current / max : 0;
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void UpdateMp(float current, float max)
        {
            if (mpBarFill != null) mpBarFill.fillAmount = max > 0 ? current / max : 0;
            if (mpText != null) mpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void UpdateExp(int current, int max)
        {
            if (expBarFill != null) expBarFill.fillAmount = max > 0 ? (float)current / max : 0;
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null) levelText.text = $"Lv. {level}";
        }

        private void UpdateClassName(string className)
        {
            if (classNameText != null) classNameText.text = className;
        }

        private void UpdateGold()
        {
            if (goldText != null && Inventory.Instance != null)
            {
                goldText.text = $"{Inventory.Instance.gold} Vàng";
            }
        }

        private void UpdateHunger(float current, float max, string status)
        {
            if (hungerBarFill != null)
            {
                float pct = max > 0 ? current / max : 0;
                hungerBarFill.fillAmount = pct;

                // Đổi màu theo trạng thái
                if (pct >= 0.8f)
                    hungerBarFill.color = new Color(0.2f, 0.85f, 0.3f); // Xanh lá: No nê
                else if (pct < 0.2f)
                    hungerBarFill.color = new Color(0.9f, 0.2f, 0.2f);  // Đỏ: Đói lã
                else
                    hungerBarFill.color = new Color(1f, 0.7f, 0.2f);   // Cam: Bình thường
            }

            if (hungerText != null) hungerText.text = $"{Mathf.CeilToInt(current)}%";
            if (hungerStatusText != null) hungerStatusText.text = status;
        }

        private void UpdatePetHp(float current, float max)
        {
            if (petHudPanel != null) petHudPanel.SetActive(true);
            if (petHpBarFill != null) petHpBarFill.fillAmount = max > 0 ? current / max : 0;
        }

        private void UpdatePetSkill(string skillName, float cd)
        {
            if (petSkillText != null) petSkillText.text = $"Skill: {skillName}";
        }

        public void UpdatePetCaptureSlot()
        {
            bool isUnlocked = PetCaptureSystem.IsPetFeatureUnlocked;

            if (petCaptureSlotPanel != null)
            {
                petCaptureSlotPanel.SetActive(isUnlocked);
            }

            if (isUnlocked && petScrollCountText != null)
            {
                int count = 0;
                if (Inventory.Instance != null && Inventory.Instance.slots != null)
                {
                    var slot = Inventory.Instance.slots.Find(s => s.item != null && (s.item.itemType == ItemType.CaptureScroll || s.item.itemId.Contains("capture_scroll")));
                    if (slot != null) count = slot.quantity;
                }
                petScrollCountText.text = $"x{count}";
            }
        }

        private void EnsurePetCaptureSlotUI()
        {
            if (petCaptureSlotPanel != null) return;

            Transform canvasTransform = transform.parent != null ? transform.parent : transform;

            // Kiểm tra xem đã có GameObject PetCaptureSlot trong Canvas chưa
            Transform existing = canvasTransform.Find("PetCaptureSlot_HUD");
            if (existing != null)
            {
                petCaptureSlotPanel = existing.gameObject;
                Text[] texts = petCaptureSlotPanel.GetComponentsInChildren<Text>();
                if (texts.Length > 1)
                {
                    petCaptureKeyText = texts[0];
                    petScrollCountText = texts[1];
                }
                return;
            }

            // Tạo Panel Ô Thu Phục Pet
            GameObject slotObj = new GameObject("PetCaptureSlot_HUD", typeof(RectTransform));
            slotObj.transform.SetParent(canvasTransform, false);
            petCaptureSlotPanel = slotObj;

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(160f, 25f);
            rt.sizeDelta = new Vector2(110f, 65f);

            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);

            Outline outline = slotObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 0.95f, 0.85f);
            outline.effectDistance = new Vector2(2, -2);

            // Text Phím Tắt & Tên chức năng
            GameObject keyObj = new GameObject("KeyText", typeof(RectTransform));
            keyObj.transform.SetParent(slotObj.transform, false);
            RectTransform keyRt = keyObj.GetComponent<RectTransform>();
            keyRt.anchorMin = new Vector2(0f, 0.5f);
            keyRt.anchorMax = new Vector2(1f, 1f);
            keyRt.offsetMin = new Vector2(4, 0);
            keyRt.offsetMax = new Vector2(-4, -4);

            Text keyTxt = keyObj.AddComponent<Text>();
            keyTxt.font = ActionRPG.UI.FontManager.GetTimesNewRomanLegacy() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            keyTxt.fontSize = 13;
            keyTxt.alignment = TextAnchor.MiddleCenter;
            keyTxt.color = new Color(1f, 0.9f, 0.3f);
            keyTxt.text = "[C] Bắt Pet";
            petCaptureKeyText = keyTxt;

            // Text Số Lượng Giấy Thu Phục
            GameObject countObj = new GameObject("CountText", typeof(RectTransform));
            countObj.transform.SetParent(slotObj.transform, false);
            RectTransform countRt = countObj.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0f, 0f);
            countRt.anchorMax = new Vector2(1f, 0.5f);
            countRt.offsetMin = new Vector2(4, 2);
            countRt.offsetMax = new Vector2(-4, 0);

            Text countTxt = countObj.AddComponent<Text>();
            countTxt.font = ActionRPG.UI.FontManager.GetTimesNewRomanLegacy() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countTxt.fontSize = 14;
            countTxt.fontStyle = FontStyle.Bold;
            countTxt.alignment = TextAnchor.MiddleCenter;
            countTxt.color = Color.white;
            countTxt.text = "x0";
            petScrollCountText = countTxt;

            // Ban đầu ẩn đi nếu chưa đủ điều kiện mở khóa
            slotObj.SetActive(PetCaptureSystem.IsPetFeatureUnlocked);
        }
    }
}
