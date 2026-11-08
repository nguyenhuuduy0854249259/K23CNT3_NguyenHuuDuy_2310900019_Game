using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Core.Bridges;

namespace ActionRPG.Core.World
{
    /// <summary>
    /// Vùng kích hoạt an toàn (Safe Zone Trigger) gắn tại Làng Tân Thủ.
    /// Khóa toàn bộ hành động tấn công khi Player ở trong làng và mở lại khi ra cổng làng.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SafeZoneTrigger : MonoBehaviour
    {
        [Header("Cài Đặt Vùng An Toàn")]
        [SerializeField] private string safeZoneName = "Làng Tân Thủ Oakhaven";
        [SerializeField] private GameZone insideZone = GameZone.Village_Oakhaven;
        [SerializeField] private GameZone outsideZone = GameZone.Grassland_Lv1_10;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;

            // Bật cờ an toàn trên PlayerPresenter & PlayerController
            SetPlayerSafeState(collision.gameObject, true);

            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.ChangeZone(insideZone);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;

            // Tắt cờ an toàn khi bước ra ngoài
            SetPlayerSafeState(collision.gameObject, false);

            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.ChangeZone(outsideZone);
            }
        }

        private void SetPlayerSafeState(GameObject playerObj, bool isSafe)
        {
            var presenter = playerObj.GetComponent<PlayerPresenter>();
            if (presenter != null)
            {
                presenter.IsSafeZone = isSafe;
            }

            var controller = playerObj.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.isSafeZone = isSafe;
            }
        }
    }
}
