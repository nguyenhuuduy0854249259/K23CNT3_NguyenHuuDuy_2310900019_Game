using UnityEngine;

namespace ActionRPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [Header("Thông Số Đạn / Mũi Tên")]
        public float speed = 10f;
        public float lifetime = 3f;
        public int pierceCount = 1; // Số quái có thể xuyên qua
        public bool isHoming = false; // Tự đuổi mục tiêu (Linh thể của Xạ Thủ Tinh Linh)
        public Transform target;
        public float homingTurnSpeed = 200f;

        [Header("Sát Thương")]
        public DamageInfo damageInfo;
        public LayerMask hitLayers;

        private int currentPierces = 0;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        public void Setup(Vector2 direction, DamageInfo info, int pierces = 1)
        {
            damageInfo = info;
            pierceCount = pierces;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * speed;
            }
        }

        private void Update()
        {
            if (isHoming && target != null)
            {
                Vector2 dir = (Vector2)target.position - rb.position;
                dir.Normalize();
                float rotateAmount = Vector3.Cross(dir, transform.right).z;
                rb.angularVelocity = -rotateAmount * homingTurnSpeed;
                rb.linearVelocity = transform.right * speed;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & hitLayers) == 0) return;

            Hurtbox2D hurtbox = collision.GetComponent<Hurtbox2D>();
            if (hurtbox != null)
            {
                damageInfo.knockbackDirection = transform.right;
                hurtbox.ReceiveDamage(damageInfo);

                currentPierces++;
                if (currentPierces >= pierceCount)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
