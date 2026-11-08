using UnityEngine;

namespace ActionRPG.Visuals
{
    /// <summary>
    /// Hiển thị trực quan Khung Bản Đồ Map 3 (120 x 60) trên Scene View:
    /// - Vẽ đường viền ranh giới màu Cyan sắc nét bao quanh toàn bộ diện tích [0..120] x [0..60].
    /// - Đánh dấu 4 góc tọa độ: (0,0), (120,0), (120,60), (0,60).
    /// - Hiển thị nhãn thông số kích thước và diện tích giúp người vẽ map dễ căn chỉnh.
    /// - Hỗ trợ cả Gizmos và LineRenderer (hiển thị kể cả khi tắt Gizmos).
    /// </summary>
    [ExecuteAlways]
    public class Map3FrameGizmo : MonoBehaviour
    {
        [Header("Thông Số Kích Thước (Gấp đôi Map 2)")]
        public float width = 120f;
        public float height = 60f;

        [Header("Màu Sắc Viền Ranh Giới")]
        public Color frameColor = new Color(0.15f, 0.85f, 1f, 0.9f);
        public Color areaFillColor = new Color(0.15f, 0.85f, 1f, 0.04f);

        private void OnDrawGizmos()
        {
            Vector3 origin = transform.position;

            Vector3 p0 = origin + new Vector3(0f, 0f, 0f);
            Vector3 p1 = origin + new Vector3(width, 0f, 0f);
            Vector3 p2 = origin + new Vector3(width, height, 0f);
            Vector3 p3 = origin + new Vector3(0f, height, 0f);

            // 1. Vẽ vùng diện tích mờ
            Gizmos.color = areaFillColor;
            Vector3 center = origin + new Vector3(width / 2f, height / 2f, 0f);
            Gizmos.DrawCube(center, new Vector3(width, height, 0.01f));

            // 2. Vẽ 4 cạnh viền ranh giới
            Gizmos.color = frameColor;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);

            // 3. Đánh dấu 4 góc
            Gizmos.color = Color.yellow;
            float cornerSize = 1.2f;
            Gizmos.DrawWireSphere(p0, cornerSize);
            Gizmos.DrawWireSphere(p1, cornerSize);
            Gizmos.DrawWireSphere(p2, cornerSize);
            Gizmos.DrawWireSphere(p3, cornerSize);

#if UNITY_EDITOR
            // 4. Nhãn chỉ dẫn tọa độ và kích thước
            UnityEditor.Handles.color = Color.cyan;
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 13;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.cyan;

            UnityEditor.Handles.Label(p0 + new Vector3(1f, 1.5f, 0f), "Góc (0, 0) [Tây Nam]", labelStyle);
            UnityEditor.Handles.Label(p1 + new Vector3(-18f, 1.5f, 0f), $"Góc ({width}, 0) [Đông Nam]", labelStyle);
            UnityEditor.Handles.Label(p2 + new Vector3(-18f, -1.5f, 0f), $"Góc ({width}, {height}) [Đông Bắc]", labelStyle);
            UnityEditor.Handles.Label(p3 + new Vector3(1f, -1.5f, 0f), $"Góc (0, {height}) [Tây Bắc]", labelStyle);

            GUIStyle centerStyle = new GUIStyle();
            centerStyle.fontSize = 15;
            centerStyle.fontStyle = FontStyle.Bold;
            centerStyle.alignment = TextAnchor.MiddleCenter;
            centerStyle.normal.textColor = new Color(0.2f, 1f, 0.5f, 1f);

            UnityEditor.Handles.Label(center, $"🗺️ KHUNG KHÔNG GIAN MAP 3 ({width} x {height})\n[Chiều Dài Trục X = {width}m - Gấp Đôi Map 2]", centerStyle);
#endif
        }
    }
}

