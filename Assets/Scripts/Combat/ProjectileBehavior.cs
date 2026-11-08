using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Enemies;

namespace ActionRPG.Combat
{
    /// <summary>
    /// Xử lý chuyển động, va chạm Trigger và gây sát thương cho đạn/mũi tên/cầu phép trong không gian 2D 360 độ.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileBehavior : MonoBehaviour
    {
        [Header("Thông Số Đạn")]
        [SerializeField] private float speed = 14f;
        [SerializeField] private float damage = 20f;
        [SerializeField] private ElementType element = ElementType.None;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool isCrit = false;
        [SerializeField] private float knockbackForce = 3.5f;
        [SerializeField] private float lifetime = 3.5f;
        [SerializeField] private int pierceCount = 1;

        [Header("Mục Tiêu & Hiệu Ứng")]
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private GameObject hitVfxPrefab;

        private Rigidbody2D rb;
        private GameObject owner;
        private bool isDestroyed = false;
        private float spawnTime = 0f;
        private readonly System.Collections.Generic.HashSet<GameObject> hitEntities = new System.Collections.Generic.HashSet<GameObject>();

        private void Awake()
        {
            spawnTime = Time.time;
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            // Đảm bảo sortingOrder của đạn luôn nằm trên Tilemap nền và YSortOrder2D (5000+)
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].sortingOrder < 5020)
                {
                    renderers[i].sortingOrder = 5020 + i;
                }
            }

            Destroy(gameObject, lifetime);
        }

        /// <summary>
        /// Khởi tạo đường đạn theo góc 360 độ và các chỉ số sát thương
        /// </summary>
        public void Initialize(
            Vector2 direction, 
            float dmg, 
            ElementType elem, 
            bool crit, 
            GameObject projectileOwner, 
            LayerMask targets, 
            float projSpeed = 14f, 
            GameObject hitVfx = null,
            int pierce = 1,
            DamageType dmgType = DamageType.Physical)
        {
            spawnTime = Time.time;
            damage = dmg;
            element = elem;
            isCrit = crit;
            owner = projectileOwner;
            targetLayers = targets;
            speed = projSpeed;
            if (hitVfx != null) hitVfxPrefab = hitVfx;
            pierceCount = Mathf.Max(1, pierce);
            damageType = dmgType;

            if (owner != null)
            {
                SpriteRenderer ownerSr = owner.GetComponent<SpriteRenderer>();
                if (ownerSr != null)
                {
                    int baseOrder = Mathf.Max(ownerSr.sortingOrder, 5000) + 15;
                    SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        if (renderers[i] != null)
                        {
                            renderers[i].sortingLayerID = ownerSr.sortingLayerID;
                            renderers[i].sortingOrder = baseOrder + i;
                        }
                    }
                }
            }

            // Quay đầu đạn theo vector hướng
            Vector2 normDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            float angle = Mathf.Atan2(normDir.y, normDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Gán vận tốc di chuyển
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = normDir * speed;
            }
        }

        private void Update()
        {
            // Fallback di chuyển bằng Transform nếu prefab không có Rigidbody2D hoặc bị kẹt vận tốc
            if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            {
                transform.position += transform.right * (speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDestroyed || other == null) return;

            // 1. Bỏ qua chính người bắn, Nhân Vật Người Chơi hoặc Thú Cưng đồng hành
            if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
            {
                return;
            }
            if (other.CompareTag("Player")) return;
            if (other.GetComponentInParent<ActionRPG.Characters.PlayerController>() != null) return;
            if (other.GetComponentInParent<ActionRPG.Characters.CharacterStats>() != null) return;
            if (other.GetComponentInParent<ActionRPG.Pets.PetController>() != null) return;

            // 2. Kiểm tra thực thể Quái Vật (Hurtbox2D hoặc EnemyStats) - Không dùng toán tử ?? trên Unity Component
            Hurtbox2D hurtbox = other.GetComponent<Hurtbox2D>();
            if (hurtbox == null) hurtbox = other.GetComponentInParent<Hurtbox2D>();

            EnemyStats enemyStats = other.GetComponent<EnemyStats>();
            if (enemyStats == null) enemyStats = other.GetComponentInParent<EnemyStats>();

            if (hurtbox != null || enemyStats != null)
            {
                // Xác định GameObject gốc của kẻ địch để tránh gây sát thương lặp (multi-hit) khi quái có nhiều collider
                GameObject targetRoot = hurtbox != null
                    ? hurtbox.transform.root.gameObject
                    : (enemyStats != null ? enemyStats.transform.root.gameObject : other.gameObject);

                if (!hitEntities.Add(targetRoot))
                {
                    return; // Đã trúng kẻ địch này rồi, bỏ qua
                }

                Vector2 hitDir = (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
                    ? rb.linearVelocity.normalized
                    : (Vector2)transform.right;

                if (hurtbox != null)
                {
                    DamageInfo info = new DamageInfo(damage, element, isCrit, owner, damageType)
                    {
                        knockbackDirection = hitDir,
                        knockbackForce = knockbackForce
                    };
                    hurtbox.ReceiveDamage(info);
                }
                else if (enemyStats != null)
                {
                    enemyStats.TakeDamage(damage, element, isCrit, hitDir, damageType);
                }

                // Sinh hiệu ứng va chạm (Hit VFX)
                SpawnHitVfx(other.transform.position);

                pierceCount--;
                if (pierceCount <= 0)
                {
                    isDestroyed = true;
                    Destroy(gameObject);
                }
                return;
            }

            // 3. Kiểm tra Vật Cản / Tường (chỉ kiểm tra sau 0.06s để không tự hủy ngay tại điểm xuất phát)
            if (Time.time - spawnTime < 0.06f) return;

            int worldLayer = LayerMask.NameToLayer("World");
            bool isSolidWall = (worldLayer != -1 && other.gameObject.layer == worldLayer) 
                               || other.CompareTag("Obstacle") 
                               || other.CompareTag("Wall");

            if (isSolidWall)
            {
                SpawnHitVfx(transform.position);
                isDestroyed = true;
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision != null && collision.collider != null)
            {
                OnTriggerEnter2D(collision.collider);
            }
        }

        private void SpawnHitVfx(Vector3 pos)
        {
            if (hitVfxPrefab != null)
            {
                GameObject vfx = Instantiate(hitVfxPrefab, pos, Quaternion.identity);
                SpriteRenderer[] srs = vfx.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < srs.Length; i++)
                {
                    if (srs[i] != null && srs[i].sortingOrder < 5025)
                        srs[i].sortingOrder = 5025 + i;
                }
                Destroy(vfx, 0.4f);
            }
        }
    }
}
