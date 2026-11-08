using UnityEngine;

namespace ActionRPG.Core
{
    public class PortalTrigger : MonoBehaviour
    {
        [Header("Điểm Đến Cục Bộ (Tọa Độ)")]
        public Vector3 destinationPosition;
        public string portalName = "Portal";

        [Header("Chuyển Đổi Scene (Để trống nếu chỉ dịch chuyển trong cùng 1 Map)")]
        [Tooltip("Tên Scene đích (ví dụ: 'Map3' hoặc 'MainGame'). Nếu để trống, sẽ chỉ dịch chuyển tọa độ trong cùng Scene.")]
        public string targetSceneName = "";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.Log($"[Portal] {portalName}: Kích hoạt chuyển sang Scene '{targetSceneName}' tại tọa độ {destinationPosition}");
                    LuminaSceneManager.Instance.LoadScene(targetSceneName, destinationPosition);
                }
                else
                {
                    other.transform.position = destinationPosition;
                    Debug.Log($"[Portal] {portalName}: Đã dịch chuyển người chơi đến {destinationPosition}");
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 1.5f);
            
            string label = string.IsNullOrEmpty(targetSceneName) ? portalName : $"{portalName} ➔ [{targetSceneName}]";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        }
#endif
    }
}
