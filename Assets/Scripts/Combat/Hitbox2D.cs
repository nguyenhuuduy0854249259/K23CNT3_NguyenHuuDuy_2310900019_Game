using System.Collections.Generic;
using UnityEngine;

namespace ActionRPG.Combat
{
    public class Hitbox2D : MonoBehaviour
    {
        [Header("Cấu Hình Va Chạm")]
        public LayerMask targetLayer;
        public DamageInfo damageInfo;
        public bool deactivateOnHit = false;

        private HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();

        private void OnEnable()
        {
            hitColliders.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Kiểm tra Layer
            if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;
            if (hitColliders.Contains(collision)) return;

            hitColliders.Add(collision);

            Hurtbox2D hurtbox = collision.GetComponent<Hurtbox2D>();
            if (hurtbox != null)
            {
                // Tính toán hướng đẩy lùi
                Vector2 dir = (collision.transform.position - transform.position).normalized;
                damageInfo.knockbackDirection = dir;
                hurtbox.ReceiveDamage(damageInfo);
            }

            if (deactivateOnHit)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
