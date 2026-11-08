using System;
using System.Collections;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Core.Bridges;
using ActionRPG.Presentation.Views;

namespace ActionRPG.Combat
{
    public class Hurtbox2D : MonoBehaviour
    {
        [Header("Tùy Chọn Nhận Đòn")]
        public bool isInvulnerable = false;
        public Action<DamageInfo> OnTakeDamageCustom;
        public event Action<DamageInfo> OnDamageTaken;

        private CharacterStats stats;
        private PlayerPresenter playerPresenter;
        private HitFlashEffectView flashEffectView;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Color originalColor = Color.white;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
            playerPresenter = GetComponent<PlayerPresenter>();
            flashEffectView = GetComponent<HitFlashEffectView>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void ReceiveDamage(DamageInfo info)
        {
            if (isInvulnerable) return;

            if (playerPresenter == null) playerPresenter = GetComponent<PlayerPresenter>();
            if (stats == null || !stats) stats = GetComponent<CharacterStats>();
            if (flashEffectView == null) flashEffectView = GetComponent<HitFlashEffectView>();
            if (rb == null || !rb) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null || !spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            // Xử lý cơ chế chặn đòn của Kiếm & Khiên
            ClassSystem classSys = GetComponent<ClassSystem>();
            if (classSys != null && classSys.isBlocking)
            {
                // Giảm 85% sát thương khi đang giơ khiên
                info.rawDamage *= 0.15f;
                info.knockbackForce *= 0.2f;
                Debug.Log("[Combat] Chặn đòn thành công bằng Khiên (-85% ST)!");
            }

            // Ưu tiên chuyển giao xử lý sát thương cho PlayerPresenter (Kiến trúc Clean mới)
            if (playerPresenter != null)
            {
                playerPresenter.ReceiveDamage(info);
            }
            else if (stats != null)
            {
                stats.TakeDamage(info);
            }
            else
            {
                // Xử lý trực tiếp cho Quái vật / Động vật (EnemyStats) nếu chưa có EnemyController
                var enemyStats = GetComponent<ActionRPG.Enemies.EnemyStats>() ?? GetComponentInParent<ActionRPG.Enemies.EnemyStats>();
                if (enemyStats != null && GetComponent<ActionRPG.Enemies.EnemyController>() == null)
                {
                    enemyStats.TakeDamage(info);
                }
            }

            // Bắn các sự kiện Observer
            OnDamageTaken?.Invoke(info);
            OnTakeDamageCustom?.Invoke(info);

            // Đẩy lùi (Knockback) nếu không do Presenter xử lý
            if (playerPresenter == null && rb != null && info.knockbackForce > 0f)
            {
                rb.AddForce(info.knockbackDirection * info.knockbackForce, ForceMode2D.Impulse);
            }

            // Hiệu ứng Visual chớp nháy đỏ: ưu tiên HitFlashEffectView riêng biệt
            if (flashEffectView != null)
            {
                flashEffectView.PlayHitFlash();
            }
            else if (Application.isPlaying && isActiveAndEnabled && spriteRenderer != null)
            {
                StartCoroutine(FlashHurtEffect());
            }
        }

        private IEnumerator FlashHurtEffect()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
                yield return new WaitForSeconds(0.12f);
                spriteRenderer.color = originalColor;
            }
        }
    }
}
