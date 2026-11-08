using UnityEngine;
using ActionRPG.Items;
using ActionRPG.Core;
using System.Collections.Generic;

namespace ActionRPG.Presentation.Views
{
    /// <summary>
    /// View hiển thị trang phục Modular 2D (Paperdoll System).
    /// </summary>
    public class PlayerEquipmentVisualView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer helmetRenderer;
        public SpriteRenderer armorRenderer;
        public SpriteRenderer glovesRenderer;
        public SpriteRenderer bootsRenderer;
        
        [Header("Dependencies")]
        [SerializeField] private CharacterVisualView characterVisualView;

        private void Awake()
        {
            if (characterVisualView == null)
                characterVisualView = GetComponent<CharacterVisualView>();
        }

        private void OnEnable()
        {
            EventBus.OnEquipmentChanged += UpdateVisuals;
            EventBus.OnClassApplied += HandleClassApplied;
            if (characterVisualView != null)
                characterVisualView.OnFlipX += UpdateFlip;
        }

        private void OnDisable()
        {
            EventBus.OnEquipmentChanged -= UpdateVisuals;
            EventBus.OnClassApplied -= HandleClassApplied;
            if (characterVisualView != null)
                characterVisualView.OnFlipX -= UpdateFlip;
        }

        private void HandleClassApplied(ActionRPG.Classes.CharacterClassData data)
        {
            UpdateVisuals();
        }

        private void Start()
        {
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (Inventory.Instance == null) return;

            var equipped = Inventory.Instance.equippedItems;

            UpdateSlot(EquipSlot.Helmet, helmetRenderer, equipped);
            UpdateSlot(EquipSlot.Armor, armorRenderer, equipped);
            UpdateSlot(EquipSlot.Gloves, glovesRenderer, equipped);
            UpdateSlot(EquipSlot.Boots, bootsRenderer, equipped);
        }

        private void UpdateSlot(EquipSlot slot, SpriteRenderer renderer, Dictionary<EquipSlot, EquipmentData> equipped)
        {
            if (renderer == null) return;

            if (equipped.TryGetValue(slot, out EquipmentData equipData) && equipData != null)
            {
                renderer.sprite = equipData.visualSprite;
            }
            else
            {
                renderer.sprite = null;
            }
        }

        private void UpdateFlip(bool isFlipped)
        {
            if (helmetRenderer != null) helmetRenderer.flipX = isFlipped;
            if (armorRenderer != null) armorRenderer.flipX = isFlipped;
            if (glovesRenderer != null) glovesRenderer.flipX = isFlipped;
            if (bootsRenderer != null) bootsRenderer.flipX = isFlipped;
        }
    }
}
