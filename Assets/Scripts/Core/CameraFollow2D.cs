using UnityEngine;

namespace ActionRPG.Core
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Mục Tiêu Theo Dõi")]
        public Transform target;
        public float smoothSpeed = 0.125f;
        public Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Giới Hạn Biên Bản Đồ (Map Bounds)")]
        public bool useBounds = false;
        public Vector2 minBounds = new Vector2(-50, -50);
        public Vector2 maxBounds = new Vector2(50, 50);

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
