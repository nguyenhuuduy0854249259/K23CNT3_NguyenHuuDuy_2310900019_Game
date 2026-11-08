using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Quản lý cơ chế tháo/đội Mũ Rơm (Prop_StrawHat) và đặt/cầm Giỏ Hoa Quả (Prop_FruitBasket) của Cư Dân Oakhaven.
    /// Chuyển đổi linh hoạt giữa Child Object trên người NPC và World Object trên mặt đất (có YSortOrder2D và Collider riêng).
    /// Hỗ trợ cả kích hoạt tự động theo nhịp nghỉ ngơi (Autonomous Routine) và gọi hàm trực tiếp từ tương tác hội thoại.
    /// </summary>
    public class VillagerPropHandler : MonoBehaviour
    {
        [Header("Đạo Cụ Gắn Trên Người (Child Props)")]
        public NPCModularProp hatProp;
        public NPCModularProp basketProp;

        [Header("Tài Nguyên Vật Thể Thế Giới (World Props)")]
        public GameObject worldHatPrefab;
        public GameObject worldBasketPrefab;
        public Sprite worldHatSprite;
        public Sprite worldBasketSprite;

        [Header("Trạng Thái Hiện Tại")]
        public bool isWearingHat = true;
        public bool isCarryingBasket = true;

        [Header("Tự Động Đặt/Cầm Theo Nhịp Nghỉ (Autonomous Toggle)")]
        public bool enableAutonomousRoutine = true;
        public float toggleInterval = 12.0f;

        private GameObject currentWorldHat;
        private GameObject currentWorldBasket;
        private float routineTimer = 0f;
        private NPCWanderController2D wanderer;

        private void Awake()
        {
            wanderer = GetComponent<NPCWanderController2D>();
            routineTimer = Random.Range(6.0f, toggleInterval);
        }

        private void Update()
        {
            if (!enableAutonomousRoutine) return;

            routineTimer -= Time.deltaTime;
            if (routineTimer <= 0f)
            {
                routineTimer = toggleInterval;
                // Nếu đang mang cả hai thì đặt xuống nghỉ ngơi; nếu đang tay không/đầu trần thì nhặt lên tiếp tục đi
                if (isCarryingBasket || isWearingHat)
                {
                    PlaceBasketOnGround();
                    RemoveHat();
                }
                else
                {
                    WearHat();
                    PickUpBasket();
                }
            }
        }

        /// <summary>
        /// Đặt Giỏ hoa quả xuống đất tại vị trí hiện tại (Tương thích chuẩn DropBasket)
        /// </summary>
        public void DropBasket() => PlaceBasketOnGround();

        /// <summary>
        /// Tháo Mũ rơm đặt xuống đất bên cạnh (Tương thích chuẩn TakeOffHat)
        /// </summary>
        public void TakeOffHat() => RemoveHat();

        /// <summary>
        /// Đội Mũ rơm lại lên đầu (Tương thích chuẩn PutOnHat)
        /// </summary>
        public void PutOnHat() => WearHat();

        /// <summary>
        /// Đặt Giỏ hoa quả xuống đất tại vị trí hiện tại
        /// </summary>
        public void PlaceBasketOnGround()
        {
            if (!isCarryingBasket) return;

            if (basketProp != null)
            {
                basketProp.gameObject.SetActive(false);
            }

            Vector3 dropPos = transform.position + new Vector3(0.24f, -0.15f, 0f);

            if (worldBasketPrefab != null)
            {
                currentWorldBasket = Instantiate(worldBasketPrefab, dropPos, Quaternion.identity);
            }
            else
            {
                currentWorldBasket = CreateWorldPropObject("WorldProp_FruitBasket", worldBasketSprite, dropPos, new Vector2(0.24f, 0.18f), -0.1f);
            }

            isCarryingBasket = false;
        }

        /// <summary>
        /// Cầm lại Giỏ hoa quả lên tay
        /// </summary>
        public void PickUpBasket()
        {
            if (isCarryingBasket) return;

            if (currentWorldBasket != null)
            {
                Destroy(currentWorldBasket);
                currentWorldBasket = null;
            }

            if (basketProp != null)
            {
                basketProp.gameObject.SetActive(true);
            }

            isCarryingBasket = true;
        }

        /// <summary>
        /// Tháo Mũ rơm đặt xuống đất bên cạnh
        /// </summary>
        public void RemoveHat()
        {
            if (!isWearingHat) return;

            if (hatProp != null)
            {
                hatProp.gameObject.SetActive(false);
            }

            Vector3 dropPos = transform.position + new Vector3(-0.24f, -0.16f, 0f);

            if (worldHatPrefab != null)
            {
                currentWorldHat = Instantiate(worldHatPrefab, dropPos, Quaternion.identity);
            }
            else
            {
                currentWorldHat = CreateWorldPropObject("WorldProp_StrawHat", worldHatSprite, dropPos, new Vector2(0.25f, 0.16f), -0.1f);
            }

            isWearingHat = false;
        }

        /// <summary>
        /// Đội Mũ rơm lại lên đầu
        /// </summary>
        public void WearHat()
        {
            if (isWearingHat) return;

            if (currentWorldHat != null)
            {
                Destroy(currentWorldHat);
                currentWorldHat = null;
            }

            if (hatProp != null)
            {
                hatProp.gameObject.SetActive(true);
            }

            isWearingHat = true;
        }

        /// <summary>
        /// Chuyển đổi trạng thái đội mũ / tháo mũ đặt xuống đất
        /// </summary>
        [ContextMenu("Toggle Hat")]
        public void ToggleHat()
        {
            if (isWearingHat) RemoveHat();
            else WearHat();
        }

        /// <summary>
        /// Chuyển đổi trạng thái cầm giỏ / đặt giỏ xuống đất
        /// </summary>
        [ContextMenu("Toggle Basket")]
        public void ToggleBasket()
        {
            if (isCarryingBasket) PlaceBasketOnGround();
            else PickUpBasket();
        }

        private GameObject CreateWorldPropObject(string objName, Sprite sprite, Vector3 position, Vector2 colSize, float footY)
        {
            GameObject obj = new GameObject(objName);
            obj.transform.position = position;
            obj.transform.localScale = transform.localScale; // Giữ nguyên cùng tỷ lệ 1.5x

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = colSize;
            col.isTrigger = false;

            var ySort = obj.AddComponent<YSortOrder2D>();
            ySort.footOffsetY = footY;
            ySort.isStatic = true;
            ySort.baseSortingOffset = 5000;

            return obj;
        }

        private void OnDestroy()
        {
            if (currentWorldHat != null) Destroy(currentWorldHat);
            if (currentWorldBasket != null) Destroy(currentWorldBasket);
        }
    }
}
