using System.Collections;
using UnityEngine;

namespace ActionRPG.Presentation.Views
{
    /// <summary>
    /// View chuyên biệt quản lý hiệu ứng nhấp nháy đỏ/trắng khi nhận sát thương cho Pixel Art.
    /// Tách rời hoàn toàn khỏi Hurtbox2D và CharacterStats logic.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlashEffectView : MonoBehaviour
    {
        [Header("Tùy Chọn Hiệu Ứng Pixel Flash")]
        [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float flashDuration = 0.12f;

        [Header("Tùy Chọn Flash Material (Nếu dùng Shader chớp trắng)")]
        [SerializeField] private Material flashMaterial;

        private SpriteRenderer spriteRenderer;
        private Color defaultColor;
        private Material defaultMaterial;
        private Coroutine flashRoutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
                defaultMaterial = spriteRenderer.material;
            }
        }

        /// <summary>
        /// Kích hoạt hiệu ứng nháy khi nhận sát thương
        /// </summary>
        public void PlayHitFlash()
        {
            if (spriteRenderer == null || !gameObject.activeInHierarchy) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                ResetToDefault();
            }

            flashRoutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            if (flashMaterial != null)
            {
                spriteRenderer.material = flashMaterial;
            }
            else
            {
                spriteRenderer.color = flashColor;
            }

            yield return new WaitForSeconds(flashDuration);

            ResetToDefault();
            flashRoutine = null;
        }

        private void ResetToDefault()
        {
            if (spriteRenderer != null)
            {
                if (flashMaterial != null && defaultMaterial != null)
                {
                    spriteRenderer.material = defaultMaterial;
                }
                spriteRenderer.color = defaultColor;
            }
        }

        private void OnDisable()
        {
            ResetToDefault();
            flashRoutine = null;
        }
    }
}
