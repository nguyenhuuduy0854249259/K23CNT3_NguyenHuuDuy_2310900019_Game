using System.Collections;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Items.Loot
{
    /// <summary>
    /// Prefab 2D đại diện cho vật phẩm rơi dưới đất chuẩn Sprint 3:
    /// - SpriteRenderer + CircleCollider2D Trigger.
    /// - Vệt sáng phát quang theo phẩm chất: Trắng (Common), Lục (Uncommon), Lam (Rare), Tím (Epic), Cam (Legendary), Đỏ (Unique).
    /// - Logic Magnet: Tự động hút về Player mượt mà khi cự ly <= 2.5m.
    /// - Thu thập: Chạm Player -> Cộng vật phẩm vào Inventory (hoặc cộng vàng), popup UI, hiệu ứng hạt và Destroy.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class WorldItem : MonoBehaviour
    {
        [Header("Dữ Liệu Vật Phẩm / Tiền Rơi")]
        public ItemData itemData;
        public int quantity = 1;
        public bool isGold = false;
        public int goldAmount = 0;

        [Header("Logic Tự Động Hút (Magnet)")]
        [Tooltip("Bán kính bắt đầu kích hoạt lực hút về phía Player (Mặc định: 2.5m)")]
        public float magnetRadius = 2.5f;
        [Tooltip("Tốc độ bay ban đầu khi bị hút")]
        public float initialMagnetSpeed = 3.5f;
        [Tooltip("Tốc độ tối đa khi bay đến Player")]
        public float maxMagnetSpeed = 14.0f;
        [Tooltip("Gia tốc bay")]
        public float magnetAcceleration = 22.0f;
        [Tooltip("Khoảng cách tối thiểu để kích hoạt nhặt")]
        public float pickupDistance = 0.45f;

        [Header("Thành Phần Hiển Thị")]
        public SpriteRenderer mainRenderer;
        public SpriteRenderer glowRenderer;

        // Trạng thái nội bộ
        private Transform playerTransform;
        private bool isMagnetized = false;
        private bool isPickedUp = false;
        private float currentSpeed = 0f;
        private float timeOffset = 0f;
        private Vector3 basePosition;
        private bool isPopping = false;

        private void Awake()
        {
            timeOffset = Random.Range(0f, 10f);
            EnsureCollider();
            EnsureRenderers();
        }

        public void EnsureCollider()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
        }

        private void Start()
        {
            basePosition = transform.position;
            FindPlayer();
            ApplyVisuals();
        }

        private void Update()
        {
            if (isPickedUp) return;

            // 1. Nếu đang thực hiện hiệu ứng văng (Pop Arc) thì tạm dừng magnet
            if (isPopping) return;

            // 2. Tìm người chơi nếu chưa có
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);

            // 3. Logic Hút Tự Động (Magnet) khi <= 2.5m
            if (!isMagnetized && dist <= magnetRadius)
            {
                isMagnetized = true;
                currentSpeed = initialMagnetSpeed;
            }

            if (isMagnetized)
            {
                // Bay mượt mà gia tốc dần về Player
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxMagnetSpeed, magnetAcceleration * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);

                // Khi chạm Player thì nhặt
                if (dist <= pickupDistance)
                {
                    Collect();
                }
            }
            else
            {
                // Hiệu ứng bồng bềnh nhấp nhô nhẹ khi còn nằm trên mặt đất
                float bob = Mathf.Sin((Time.time + timeOffset) * 3.5f) * 0.08f;
                transform.position = new Vector3(basePosition.x, basePosition.y + bob, basePosition.z);

                // Hiệu ứng nhịp đập phát quang (Glow Pulse)
                if (glowRenderer != null)
                {
                    Color c = glowRenderer.color;
                    c.a = 0.35f + 0.25f * Mathf.Sin((Time.time + timeOffset) * 4f);
                    glowRenderer.color = c;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;
            if (other.CompareTag("Player") || other.GetComponent<CharacterStats>() != null)
            {
                Collect();
            }
        }

        /// <summary>
        /// Xử lý khi chạm vào Player: Cộng vào Inventory, hiển thị popup, phát hiệu ứng và Destroy.
        /// </summary>
        public void Collect()
        {
            if (isPickedUp) return;
            isPickedUp = true;

            if (isGold)
            {
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.AddGold(goldAmount);
                }
                string goldMsg = $"+{goldAmount} Vàng";
                EventBus.OnShowUIMessage?.Invoke($"Nhặt được {goldMsg}!");
                EventBus.OnShowDamageText?.Invoke(transform.position + Vector3.up * 0.5f, goldAmount, false, true);
                Debug.Log($"<color=yellow>[Loot Pickup] Nhặt được {goldMsg}!</color>");
            }
            else if (itemData != null)
            {
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.AddItem(itemData, quantity);
                }
                string itemMsg = quantity > 1 ? $"{itemData.itemName} x{quantity}" : itemData.itemName;
                EventBus.OnShowUIMessage?.Invoke($"Nhặt được {itemMsg}!");
                EventBus.OnShowDamageText?.Invoke(transform.position + Vector3.up * 0.5f, quantity, false, true);
                Debug.Log($"<color=green>[Loot Pickup] Nhặt được {itemMsg}!</color>");
            }

            // Phát âm thanh nhặt đồ
            EventBus.OnPlaySound?.Invoke("pickup");

            // Hiệu ứng hạt nhặt đồ
            SpawnPickupVFX();

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Kích hoạt văng nhẹ trong bán kính quanh tâm khi vừa spawn (Pop Arc).
        /// </summary>
        public void TriggerPopArc(Vector3 origin, float popRadius = 1.0f)
        {
            StartCoroutine(PopArcRoutine(origin, popRadius));
        }

        private IEnumerator PopArcRoutine(Vector3 origin, float popRadius)
        {
            isPopping = true;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(0.4f, popRadius);
            Vector3 targetGround = origin + new Vector3(randomDir.x, randomDir.y * 0.6f, 0f) * randomDist;

            float duration = 0.35f;
            float elapsed = 0f;
            Vector3 startPos = origin;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Quỹ đạo parabol nảy lên cao
                Vector3 current = Vector3.Lerp(startPos, targetGround, t);
                float arcHeight = Mathf.Sin(t * Mathf.PI) * 0.6f;
                current.y += arcHeight;

                transform.position = current;
                yield return null;
            }

            transform.position = targetGround;
            basePosition = targetGround;
            isPopping = false;
        }

        private void FindPlayer()
        {
            if (CharacterStats.PlayerInstance != null)
            {
                playerTransform = CharacterStats.PlayerInstance.transform;
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }
        }

        private void EnsureRenderers()
        {
            if (mainRenderer == null)
            {
                mainRenderer = GetComponent<SpriteRenderer>();
                if (mainRenderer == null) mainRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            mainRenderer.sortingOrder = 5;

            if (glowRenderer == null)
            {
                Transform glowChild = transform.Find("GlowAura");
                if (glowChild != null)
                {
                    glowRenderer = glowChild.GetComponent<SpriteRenderer>();
                }
                else
                {
                    GameObject glowObj = new GameObject("GlowAura");
                    glowObj.transform.SetParent(transform, false);
                    glowObj.transform.localScale = Vector3.one * 1.5f;
                    glowRenderer = glowObj.AddComponent<SpriteRenderer>();
                    glowRenderer.sortingOrder = 4;
                }
            }
        }

        public void ApplyVisuals()
        {
            EnsureRenderers();

            Color qualityColor = Color.white;
            Sprite itemSprite = null;

            if (isGold)
            {
                qualityColor = new Color(1f, 0.85f, 0.15f); // Vàng óng
                if (mainRenderer != null)
                {
                    mainRenderer.color = qualityColor;
                    if (mainRenderer.sprite == null)
                    {
                        mainRenderer.sprite = CreateCircleSprite();
                    }
                }
            }
            else if (itemData != null)
            {
                qualityColor = itemData.GetRarityColor();
                itemSprite = itemData.icon;

                if (mainRenderer != null)
                {
                    if (itemSprite != null)
                    {
                        mainRenderer.sprite = itemSprite;
                        mainRenderer.color = Color.white;
                    }
                    else
                    {
                        mainRenderer.sprite = CreateCircleSprite();
                        mainRenderer.color = qualityColor;
                    }
                }
            }

            // Vệt sáng phát quang theo phẩm chất
            if (glowRenderer != null)
            {
                if (glowRenderer.sprite == null)
                {
                    glowRenderer.sprite = CreateGlowSprite();
                }
                glowRenderer.color = new Color(qualityColor.r, qualityColor.g, qualityColor.b, 0.45f);
            }
        }

        private void SpawnPickupVFX()
        {
            GameObject vfx = new GameObject("VFX_PickupBurst");
            vfx.transform.position = transform.position;
            var ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.4f;
            main.startSpeed = 2.5f;
            main.startSize = 0.15f;
            main.startColor = isGold ? new Color(1f, 0.85f, 0.2f) : (itemData != null ? itemData.GetRarityColor() : Color.white);
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            ps.Play();
        }

        #region Procedural Sprite Fallbacks
        private static Sprite cachedCircleSprite;
        private static Sprite cachedGlowSprite;

        public static Sprite CreateCircleSprite()
        {
            if (cachedCircleSprite != null) return cachedCircleSprite;
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float edge = Mathf.Clamp01((radius - dist) / 1.5f);
                        tex.SetPixel(x, y, new Color(1, 1, 1, edge));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return cachedCircleSprite;
        }

        public static Sprite CreateGlowSprite()
        {
            if (cachedGlowSprite != null) return cachedGlowSprite;
            int size = 48;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = Mathf.Pow(alpha, 1.8f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }
            tex.Apply();
            cachedGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return cachedGlowSprite;
        }
        #endregion

        #region Static Spawner Helpers
        /// <summary>
        /// Tạo nhanh WorldItem rơi trong thế giới với hiệu ứng văng nhẹ 1.0m.
        /// </summary>
        public static WorldItem Spawn(Vector3 origin, ItemData item, int quantity = 1, float popRadius = 1.0f)
        {
            GameObject obj = new GameObject($"WorldItem_{item?.itemName ?? "Unknown"}");
            obj.transform.position = origin;

            var wi = obj.AddComponent<WorldItem>();
            wi.EnsureCollider();
            wi.itemData = item;
            wi.quantity = Mathf.Max(1, quantity);
            wi.isGold = false;
            wi.ApplyVisuals();
            wi.TriggerPopArc(origin, popRadius);
            return wi;
        }

        /// <summary>
        /// Tạo nhanh tiền vàng rơi trong thế giới.
        /// </summary>
        public static WorldItem SpawnGold(Vector3 origin, int amount, float popRadius = 1.0f)
        {
            GameObject obj = new GameObject($"WorldItem_Gold_{amount}");
            obj.transform.position = origin;

            var wi = obj.AddComponent<WorldItem>();
            wi.EnsureCollider();
            wi.isGold = true;
            wi.goldAmount = amount;
            wi.ApplyVisuals();
            wi.TriggerPopArc(origin, popRadius);
            return wi;
        }
        #endregion
    }
}
