using UnityEngine;
using ActionRPG.Core;

namespace ActionRPG.UI
{
    public class FloatingDamageText : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.OnShowDamageText += ShowDamage;
        }

        private void OnDisable()
        {
            EventBus.OnShowDamageText -= ShowDamage;
        }

        private void ShowDamage(Vector3 position, float amount, bool isCrit, bool isHeal)
        {
            GameObject popupObj = new GameObject("DamagePopup");
            popupObj.transform.position = position + (Vector3)Random.insideUnitCircle * 0.25f;

            TextMesh textMesh = popupObj.AddComponent<TextMesh>();
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontSize = isCrit ? 28 : 20;

            if (isHeal)
            {
                textMesh.text = $"+{Mathf.CeilToInt(amount)}";
                textMesh.color = new Color(0.2f, 1f, 0.3f);
            }
            else
            {
                textMesh.text = isCrit ? $"CRIT! {Mathf.CeilToInt(amount)}" : $"{Mathf.CeilToInt(amount)}";
                textMesh.color = isCrit ? new Color(1f, 0.85f, 0.1f) : Color.white;
            }

            popupObj.AddComponent<FloatingTextAnimator>();
        }
    }

    public class FloatingTextAnimator : MonoBehaviour
    {
        private float timer = 0f;
        private float lifetime = 0.75f;
        private Vector3 moveDirection = Vector3.up * 1.5f;

        private void Update()
        {
            transform.position += moveDirection * Time.deltaTime;
            timer += Time.deltaTime;

            TextMesh tm = GetComponent<TextMesh>();
            if (tm != null)
            {
                Color c = tm.color;
                c.a = Mathf.Clamp01(1f - (timer / lifetime));
                tm.color = c;
            }

            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
