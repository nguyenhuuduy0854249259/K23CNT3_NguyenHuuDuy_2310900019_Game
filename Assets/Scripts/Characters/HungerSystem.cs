using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Items;

namespace ActionRPG.Characters
{
    public class HungerSystem : MonoBehaviour
    {
        public static HungerSystem Instance { get; private set; }

        [Header("Thanh Độ No / Đói")]
        public float maxHunger = 100f;
        public float currentHunger = 100f;

        [Header("Tốc Độ Tiêu Hao")]
        [Tooltip("Số điểm đói tiêu hao mỗi giây khi đứng yên")]
        [SerializeField] private float baseDrainRate = 0.12f;
        [Tooltip("Hệ số tiêu hao thêm khi di chuyển")]
        [SerializeField] private float movingDrainMultiplier = 1.6f;

        private CharacterStats stats;
        private Rigidbody2D rb;

        public bool IsStarving => currentHunger < 20f;
        public bool IsWellFed => currentHunger >= 80f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            stats = GetComponent<CharacterStats>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            NotifyHungerChange();
        }

        private void Update()
        {
            float drain = baseDrainRate * Time.deltaTime;
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.05f)
            {
                drain *= movingDrainMultiplier;
            }

            currentHunger = Mathf.Max(0f, currentHunger - drain);

            // Xử lý tự hồi phục nếu đang ở trạng thái no
            if (IsWellFed && stats != null && stats.currentHp < stats.maxHp)
            {
                stats.Heal(0.5f * Time.deltaTime);
            }

            NotifyHungerChange();
        }

        public bool EatFood(FoodItemData food)
        {
            if (food == null) return false;

            // Xử lý hồi thanh đói (Loại 1 và Loại 2)
            if (food.hungerRestoreAmount > 0)
            {
                currentHunger = Mathf.Min(maxHunger, currentHunger + food.hungerRestoreAmount);
                Debug.Log($"[HungerSystem] Đã ăn {food.itemName}, hồi {food.hungerRestoreAmount} điểm đói! (Hiện tại: {currentHunger:F1}/{maxHunger})");
            }

            // Xử lý áp dụng buff chỉ số (Loại 2 và Loại 3)
            if (food.buffStat != BuffStatType.None && food.buffValue > 0f)
            {
                if (stats != null)
                {
                    stats.ApplyBuff(food.buffStat, food.buffValue, food.buffDurationSeconds);
                    Debug.Log($"[HungerSystem] Nhận Buff {food.buffStat} +{food.buffValue} trong {food.buffDurationSeconds} giây!");
                }
            }

            NotifyHungerChange();
            return true;
        }

        private void NotifyHungerChange()
        {
            string status = "Bình Thường";
            if (IsWellFed) status = "No Nê (Hồi Máu Nhanh)";
            else if (IsStarving) status = "Đói Lã (Giảm Tốc Độ & Công)";

            EventBus.OnHungerChanged?.Invoke(currentHunger, maxHunger, status);
        }
    }
}
