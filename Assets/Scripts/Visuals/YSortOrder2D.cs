using UnityEngine;

namespace ActionRPG.Visuals
{
    /// <summary>
    /// Tự động cập nhật Sorting Order theo trục Y (Top-down 2D Depth Sorting).
    /// Giúp nhân vật hiển thị tự nhiên khi đi trước hoặc sau cây cối, chướng ngại vật, quái vật.
    /// Sử dụng baseSortingOffset = 5000 để đảm bảo Sorting Order luôn dương (> 4000),
    /// tuyệt đối KHÔNG bao giờ bị chìm/mất hình dưới các lớp Tilemap nền (Ground = 0).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSortOrder2D : MonoBehaviour
    {
        [Tooltip("Khoảng bù trục Y để tính điểm chân tiếp đất")]
        public float footOffsetY = 0f;

        [Tooltip("Độ ưu tiên cơ bản (Base Order) để luôn cao hơn Tilemap Ground")]
        public int baseSortingOffset = 5000;

        [Tooltip("Hệ số nhân theo trục Y (32 = tương ứng 32 pixel/unit)")]
        public float sortFactor = 32f;

        [Tooltip("Nếu là vật thể tĩnh (Cây, Đá, Nhà), chỉ tính 1 lần lúc Start để tối ưu hiệu năng")]
        public bool isStatic = false;

        [Tooltip("Tự động đồng bộ Sorting Order cho các SpriteRenderer con (Vũ khí, Giáp, Bóng râm)")]
        public bool syncChildRenderers = true;

        [Tooltip("Độ lệch Sorting Order cho các con so với thân chính")]
        public int childSortingOffset = 1;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSorting();
        }

        private void Start()
        {
            UpdateSorting();
        }

        private void LateUpdate()
        {
            if (!isStatic)
            {
                UpdateSorting();
            }
        }

        public void UpdateSorting()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                float footY = transform.position.y + footOffsetY;
                // footY càng cao (ở phía bắc bản đồ) -> sortingOrder càng thấp
                // footY càng thấp (ở phía nam bản đồ) -> sortingOrder càng cao (vẽ đè lên trên)
                int order = Mathf.RoundToInt(-footY * sortFactor) + baseSortingOffset;
                spriteRenderer.sortingOrder = order;

                if (syncChildRenderers)
                {
                    SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();
                    for (int i = 0; i < children.Length; i++)
                    {
                        SpriteRenderer childSr = children[i];
                        if (childSr != null && childSr != spriteRenderer)
                        {
                            // Bỏ qua các renderer vũ khí & hiệu ứng đầu gậy/mũi tên do WeaponSocketView tự quản lý, hoặc modular props của NPC
                            string childName = childSr.gameObject.name;
                            if (childName == "Weapon_Renderer" || childName == "StaffTip_Glow" || childName == "StaffTip_Core" || childName == "Bow_NockedArrow"
                                || childName.StartsWith("Prop_") || childName.StartsWith("Socket_") || childName.StartsWith("Modular_"))
                            {
                                continue;
                            }

                            // Chỉ cập nhật nếu GameObject con không có YSortOrder2D riêng
                            if (childSr.GetComponent<YSortOrder2D>() == null)
                            {
                                int minChildOrder = order + childSortingOffset;
                                if (childSr.sortingOrder < minChildOrder)
                                {
                                    childSr.sortingOrder = minChildOrder;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
