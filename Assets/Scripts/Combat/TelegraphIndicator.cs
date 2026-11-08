using System;
using System.Collections;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Combat
{
    public enum TelegraphShape
    {
        Circle, // AoE Tròn bung dần từ tâm hoặc nạp alpha 0% -> 100%
        Line    // Vệt chữ nhật cảnh báo đường húc / lướt
    }

    /// <summary>
    /// Hệ thống cảnh báo đòn đánh nguy hiểm (Telegraph Indicator):
    /// - Circle Telegraph: Vùng đỏ cảnh báo nổ lan diện rộng (AoE).
    /// - Line/Box Telegraph: Vệt đỏ cảnh báo đòn húc/lướt của Boss hoặc quái thú.
    /// - Kiểm tra i-frame: Bỏ qua sát thương nếu Player đang Dash (Phím Space).
    /// </summary>
    public class TelegraphIndicator : MonoBehaviour
    {
        [Header("Cấu Hình Cảnh Báo")]
        public TelegraphShape shape = TelegraphShape.Circle;
        public float delay = 1.0f;          // Thời gian nạp (0.8s - 1.2s)
        public float damage = 25f;          // Sát thương khi kích nổ
        public ElementType element = ElementType.Fire;
        public GameObject attacker;
        public Action onExploded;

        [Header("Thông Số Hình Học")]
        public float radius = 3.0f;         // Dành cho Circle
        public float lineLength = 5.0f;     // Dành cho Line
        public float lineWidth = 1.6f;      // Dành cho Line
        public Vector2 lineDirection = Vector2.right;

        [Header("Màu Sắc & Hiệu Ứng")]
        public Color warningColor = new Color(1f, 0.15f, 0.15f, 0.4f);
        public Color flashColor = new Color(1f, 0.85f, 0.2f, 0.9f);

        private float elapsed = 0f;
        private bool isExploded = false;
        private SpriteRenderer innerRenderer;
        private SpriteRenderer outerRenderer;

        public static TelegraphIndicator CreateCircleTelegraph(Vector3 pos, float radius, float delay, float damage, ElementType element, GameObject attacker, Action onExplode = null)
        {
            GameObject obj = new GameObject("Telegraph_Circle");
            obj.transform.position = pos;
            var indicator = obj.AddComponent<TelegraphIndicator>();
            indicator.shape = TelegraphShape.Circle;
            indicator.radius = radius;
            indicator.delay = Mathf.Max(0.2f, delay);
            indicator.damage = damage;
            indicator.element = element;
            indicator.attacker = attacker;
            indicator.onExploded = onExplode;
            return indicator;
        }

        public static TelegraphIndicator CreateLineTelegraph(Vector3 startPos, Vector2 direction, float length, float width, float delay, float damage, ElementType element, GameObject attacker, Action onExplode = null)
        {
            GameObject obj = new GameObject("Telegraph_Line");
            obj.transform.position = startPos;
            var indicator = obj.AddComponent<TelegraphIndicator>();
            indicator.shape = TelegraphShape.Line;
            indicator.lineDirection = direction.normalized;
            indicator.lineLength = length;
            indicator.lineWidth = width;
            indicator.delay = Mathf.Max(0.2f, delay);
            indicator.damage = damage;
            indicator.element = element;
            indicator.attacker = attacker;
            indicator.onExploded = onExplode;
            return indicator;
        }

        private void Start()
        {
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            // Outer marker (vùng giới hạn)
            GameObject outerObj = new GameObject("OuterVisual");
            outerObj.transform.SetParent(transform, false);
            outerRenderer = outerObj.AddComponent<SpriteRenderer>();
            outerRenderer.sortingOrder = 1;
            outerRenderer.sprite = CreateRuntimeShapeSprite(shape == TelegraphShape.Circle);
            outerRenderer.color = new Color(warningColor.r, warningColor.g, warningColor.b, 0.25f);

            // Inner marker (vòng nạp đầy dần)
            GameObject innerObj = new GameObject("InnerFill");
            innerObj.transform.SetParent(transform, false);
            innerRenderer = innerObj.AddComponent<SpriteRenderer>();
            innerRenderer.sortingOrder = 2;
            innerRenderer.sprite = CreateRuntimeShapeSprite(shape == TelegraphShape.Circle);
            innerRenderer.color = warningColor;

            if (shape == TelegraphShape.Circle)
            {
                outerObj.transform.localScale = Vector3.one * (radius * 2f);
                innerObj.transform.localScale = Vector3.zero;
            }
            else // Line
            {
                float angle = Mathf.Atan2(lineDirection.y, lineDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);

                // Căn tâm vệt chữ nhật dịch về phía trước
                Vector3 centerOffset = new Vector3(lineLength * 0.5f, 0, 0);
                outerObj.transform.localPosition = centerOffset;
                outerObj.transform.localScale = new Vector3(lineLength, lineWidth, 1f);

                innerObj.transform.localPosition = Vector3.zero;
                innerObj.transform.localScale = new Vector3(0, lineWidth, 1f);
            }
        }

        private void Update()
        {
            if (isExploded) return;

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / delay);

            UpdateVisualProgress(progress);

            if (elapsed >= delay)
            {
                Explode();
            }
        }

        private void UpdateVisualProgress(float progress)
        {
            if (innerRenderer == null) return;

            if (shape == TelegraphShape.Circle)
            {
                // Vòng nạp nở dần từ tâm ra viền ngoài
                innerRenderer.transform.localScale = Vector3.one * (radius * 2f * progress);
                float alpha = Mathf.Lerp(0.35f, 0.85f, progress);
                innerRenderer.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            }
            else // Line
            {
                // Vệt đỏ trượt dài dần về phía trước
                innerRenderer.transform.localPosition = new Vector3(lineLength * progress * 0.5f, 0, 0);
                innerRenderer.transform.localScale = new Vector3(lineLength * progress, lineWidth, 1f);
                float alpha = Mathf.Lerp(0.35f, 0.85f, progress);
                innerRenderer.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            }
        }

        public void Explode()
        {
            if (isExploded) return;
            isExploded = true;

            StartCoroutine(ExplosionRoutine());
        }

        private IEnumerator ExplosionRoutine()
        {
            // Hiệu ứng chớp sáng bung vỡ
            if (innerRenderer != null) innerRenderer.color = flashColor;
            if (outerRenderer != null) outerRenderer.color = flashColor;

            CheckDamageArea();

            onExploded?.Invoke();

            yield return new WaitForSeconds(0.12f);
            Destroy(gameObject);
        }

        public void CheckDamageArea()
        {
            Physics2D.SyncTransforms();
            Collider2D[] hits;

            if (shape == TelegraphShape.Circle)
            {
                hits = Physics2D.OverlapCircleAll(transform.position, radius);
            }
            else // Line
            {
                Vector2 center = (Vector2)transform.position + lineDirection * (lineLength * 0.5f);
                float angle = Mathf.Atan2(lineDirection.y, lineDirection.x) * Mathf.Rad2Deg;
                hits = Physics2D.OverlapBoxAll(center, new Vector2(lineLength, lineWidth), angle);
            }

            foreach (var hit in hits)
            {
                if (hit == null || hit.gameObject == attacker) continue;

                // Chỉ gây sát thương cho Player hoặc Pet đồng minh
                if (hit.CompareTag("Player"))
                {
                    // 1. Kiểm tra Dash i-frame của Player
                    var playerCtrl = hit.GetComponent<PlayerController>();
                    if (playerCtrl != null && playerCtrl.IsDashing)
                    {
                        Debug.Log("<color=cyan>[Telegraph Né Đòn] Player đã kích hoạt Dash i-frame né đòn diện rộng thành công!</color>");
                        continue;
                    }

                    // 2. Kiểm tra bất tử từ Hurtbox2D
                    var hurt = hit.GetComponent<Hurtbox2D>();
                    if (hurt != null)
                    {
                        if (hurt.isInvulnerable)
                        {
                            Debug.Log("<color=cyan>[Telegraph Bất Tử] Mục tiêu đang có trạng thái bất tử (i-frame)!</color>");
                            continue;
                        }

                        DamageInfo dmg = new DamageInfo(damage, element, false, attacker);
                        dmg.knockbackForce = 6.0f;
                        hurt.ReceiveDamage(dmg);
                        Debug.Log($"<color=red>[Telegraph Hit] Đòn nổ diện rộng trúng Player gây {damage} sát thương ({element})!</color>");
                    }
                }
            }
        }

        /// <summary>
        /// Tạo texture hình tròn hoặc chữ nhật runtime không phụ thuộc vào asset ngoài.
        /// </summary>
        private static Sprite CreateRuntimeShapeSprite(bool isCircle)
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[res * res];
            float center = res * 0.5f;
            float radiusPx = center - 1f;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    if (isCircle)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                        pixels[y * res + x] = d <= radiusPx ? Color.white : Color.clear;
                    }
                    else
                    {
                        pixels[y * res + x] = Color.white;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
        }
    }
}
