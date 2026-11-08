using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Core;
using ActionRPG.Enemies;
using ActionRPG.Items;
using ActionRPG.Combat;

namespace ActionRPG.Pets
{
    /// <summary>
    /// Hệ thống Thuần Hóa / Thu Phục Thú Cưng (Pet Capture System):
    /// - Quản lý việc dùng "Quả Cầu Tinh Linh" (Spirit Orb) hoặc "Giấy Thu Phục" để bắt quái dã ngoại.
    /// - Công thức: Tỷ lệ = orbCaptureRate * (1.0f - (currentHp / maxHp)) * 1.5f.
    /// - Mở khóa khi Nhân vật đạt Cấp >= 15 và đã chuyển sang Class cao hơn (Tier >= 1).
    /// </summary>
    public class PetCaptureSystem : MonoBehaviour
    {
        public static PetCaptureSystem Instance { get; private set; }

        [Header("Tỉ Lệ Thu Phục & Giới Hạn Máu")]
        [Tooltip("Ngưỡng máu tối đa của quái vật để có thể bắt (30%)")]
        public float maxHpPercentageToCapture = 0.30f;
        [Tooltip("Tỷ lệ bắt cơ bản của Quả Cầu Tinh Linh (50%)")]
        public float defaultOrbCaptureRate = 0.50f;

        /// <summary>
        /// Điều kiện mở khóa tính năng Thú Cưng:
        /// Người chơi phải đạt cấp độ quy định (Mặc định Cấp 10) trong CharacterStats.
        /// </summary>
        public static bool IsPetFeatureUnlocked
        {
            get
            {
                var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
                if (stats != null)
                {
                    return stats.isPetUnlocked;
                }
                return true; // Cho phép test nếu chưa khởi tạo CharacterStats trong scene
            }
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        #region API Bắt Quái Vật (TryCaptureEnemy)
        /// <summary>
        /// Thử thu phục kẻ địch bằng EnemyStats theo yêu cầu chuẩn Sprint 1.
        /// Chặn lại và báo thông báo nếu chưa đạt cấp mở khóa Pet.
        /// </summary>
        public bool TryCaptureEnemy(EnemyStats targetEnemy, float orbCaptureRate)
        {
            if (targetEnemy == null) return false;

            if (!IsPetFeatureUnlocked)
            {
                int reqLevel = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.petUnlockLevel : 10;
                string msg = $"Cần đạt Cấp {reqLevel} để mở khóa tính năng Thu Phục Thú Cưng!";
                Debug.LogWarning($"<color=orange>[Thu Phục Thất Bại] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                return false;
            }

            EnemyController controller = targetEnemy.GetComponent<EnemyController>();
            return ExecuteCapture(controller, targetEnemy.enemyName, targetEnemy.element, targetEnemy.currentHp, targetEnemy.maxHp, targetEnemy.attackPower, targetEnemy.defense, targetEnemy.moveSpeed, targetEnemy.level, orbCaptureRate, null, targetEnemy.gameObject);
        }

        /// <summary>
        /// Thử thu phục kẻ địch bằng EnemyController theo yêu cầu chuẩn.
        /// </summary>
        public bool TryCaptureEnemy(EnemyController targetEnemy, float orbCaptureRate)
        {
            if (targetEnemy == null) return false;

            if (!IsPetFeatureUnlocked)
            {
                int reqLevel = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.petUnlockLevel : 10;
                string msg = $"Cần đạt Cấp {reqLevel} để mở khóa tính năng Thu Phục Thú Cưng!";
                Debug.LogWarning($"<color=orange>[Thu Phục Thất Bại] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                return false;
            }

            return ExecuteCapture(targetEnemy, targetEnemy.enemyName, targetEnemy.element, targetEnemy.currentHp, targetEnemy.maxHp, targetEnemy.attackPower, targetEnemy.defense, targetEnemy.moveSpeed, targetEnemy.level, orbCaptureRate, null, targetEnemy.gameObject);
        }

        /// <summary>
        /// Thử thu phục quái vật hoặc động vật bằng EnemyStats và ItemData.
        /// </summary>
        public bool TryCaptureEnemy(EnemyStats targetEnemy, ItemData scrollOrOrbItem)
        {
            if (targetEnemy == null) return false;

            if (!IsPetFeatureUnlocked)
            {
                int reqLevel = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.petUnlockLevel : 10;
                string msg = $"Cần đạt Cấp {reqLevel} để mở khóa tính năng Thu Phục Thú Cưng!";
                Debug.LogWarning($"<color=orange>[Thu Phục Thất Bại] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                return false;
            }

            float rate = defaultOrbCaptureRate;
            if (scrollOrOrbItem != null && (scrollOrOrbItem.itemId.Contains("high") || scrollOrOrbItem.itemId.Contains("super")))
                rate = 0.75f;

            EnemyController controller = targetEnemy.GetComponent<EnemyController>();
            return ExecuteCapture(controller, targetEnemy.enemyName, targetEnemy.element, targetEnemy.currentHp, targetEnemy.maxHp, targetEnemy.attackPower, targetEnemy.defense, targetEnemy.moveSpeed, targetEnemy.level, rate, scrollOrOrbItem, targetEnemy.gameObject);
        }

        /// <summary>
        /// Overload tương thích ngược cho các lệnh gọi truyền ItemData.
        /// </summary>
        public bool TryCaptureEnemy(EnemyController enemy, ItemData scrollOrOrbItem)
        {
            if (enemy == null) return false;
            float rate = defaultOrbCaptureRate;
            if (scrollOrOrbItem != null && (scrollOrOrbItem.itemId.Contains("high") || scrollOrOrbItem.itemId.Contains("super")))
                rate = 0.75f;

            return ExecuteCapture(enemy, enemy.enemyName, enemy.element, enemy.currentHp, enemy.maxHp, enemy.attackPower, enemy.defense, enemy.moveSpeed, enemy.level, rate, scrollOrOrbItem, enemy.gameObject);
        }

        /// <summary>
        /// Alias tương thích ngược cho các script tham chiếu GetCaptureScrollReference.
        /// </summary>
        public ItemData GetCaptureScrollReference() => GetCaptureItemReference();
        #endregion

        /// <summary>
        /// Tính toán tỷ lệ thu phục quái vật (Capture Chance):
        /// - Công thức: orbCaptureRate * (1.0f - (currentHp / maxHp)) * 1.5f
        /// - Áp dụng Mathf.Clamp01 để đảm bảo tỷ lệ luôn nằm trong khoảng [0.0f, 1.0f] (0% - 100%).
        /// - Cơ chế may mắn: Nếu tỷ lệ tính ra < 0.05f thì gán tỷ lệ tối thiểu là 0.05f (5% cơ hội may mắn).
        /// </summary>
        public static float CalculateCaptureChance(float orbCaptureRate, float currentHp, float maxHp)
        {
            if (maxHp <= 0f) maxHp = 1f;
            float hpRatio = Mathf.Clamp01(currentHp / maxHp);
            float rawChance = orbCaptureRate * (1.0f - hpRatio) * 1.5f;
            float clampedChance = Mathf.Clamp01(rawChance);
            return Mathf.Max(0.05f, clampedChance);
        }

        private bool ExecuteCapture(EnemyController enemyController, string enemyName, ElementType elem, float currentHp, float maxHp, float atk, float def, float speed, int level, float orbCaptureRate, ItemData usedItem = null, GameObject targetGameObject = null)
        {
            // 1. Kiểm tra điều kiện mở khóa cấp độ
            if (!IsPetFeatureUnlocked)
            {
                int reqLevel = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.petUnlockLevel : 10;
                string msg = $"Cần đạt Cấp {reqLevel} để mở khóa tính năng Thu Phục Thú Cưng!";
                Debug.LogWarning($"<color=orange>[Thu Phục Thất Bại] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                return false;
            }

            if (maxHp <= 0f) maxHp = 1f;
            float hpRatio = currentHp / maxHp;

            // 2. Kiểm tra điều kiện máu quái: < 30% HP tỷ lệ bắt cao hơn, nếu > 30% thì quái quá sung mãn
            if (hpRatio > maxHpPercentageToCapture)
            {
                string msg = $"Quái vật còn quá sung mãn ({hpRatio * 100:F0}% HP)! Phải đánh yếu xuống dưới 30% máu mới có thể bắt!";
                Debug.LogWarning($"<color=orange>[Thu Phục Thất Bại] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                return false;
            }

            // 3. Tiêu hao Quả Cầu Tinh Linh / Giấy Thu Phục từ túi đồ nếu có
            ItemData captureItem = usedItem ?? GetCaptureItemReference();
            if (Inventory.Instance != null && !Inventory.Instance.RemoveItem(captureItem, 1))
            {
                Debug.LogWarning("[Thu Phục] Bạn không có Quả Cầu Tinh Linh hay Giấy Thu Phục trong túi đồ!");
                return false;
            }

            // 4. Áp dụng chuẩn công thức bắt quái:
            // Tỷ lệ thành công = orbCaptureRate * (1.0f - (targetEnemy.currentHp / targetEnemy.maxHp)) * 1.5f
            // Đảm bảo trong khoảng [0.05f, 1.0f] với 5% cơ hội may mắn tối thiểu
            float totalChance = CalculateCaptureChance(orbCaptureRate, currentHp, maxHp);

            float roll = Random.value;
            bool isSuccess = roll <= totalChance;

            if (isSuccess)
            {
                // Bắt THÀNH CÔNG!
                Debug.Log($"<color=green>[🎉 THU PHỤC THÀNH CÔNG!] Chúc mừng! Bạn đã bắt được {enemyName}! (Tỷ lệ: {totalChance * 100:F0}%, Roll: {roll:F2})</color>");

                // Tạo PetInstance mới và thêm vào Kho Thú Cưng của người chơi
                PetInstance newPet = CreatePetInstanceFromEnemy(enemyController, enemyName, elem, maxHp, atk, def, speed, level, targetGameObject);
                if (PlayerPetRoster.Instance != null)
                {
                    PlayerPetRoster.Instance.AddPet(newPet);
                }

                // Phát Event lên EventBus
                EventBus.OnPetCaptured?.Invoke(enemyName);
                EventBus.OnPetCaptureResult?.Invoke(enemyName, true);

                // Hủy quái vật hoặc động vật hoang dã
                if (enemyController != null)
                {
                    Destroy(enemyController.gameObject);
                }
                else if (targetGameObject != null)
                {
                    Destroy(targetGameObject);
                }
                return true;
            }
            else
            {
                // Bắt hụt! Quái vật nổi giận phản kháng, động vật hoảng loạn bỏ chạy
                Debug.Log($"<color=red>[Thu Phục Hụt!] Quái vật đã thoát khỏi phong ấn (Roll: {roll:F2} > Tỷ lệ: {totalChance:F2})!</color>");
                if (enemyController != null)
                {
                    enemyController.Enrage();
                }
                else if (targetGameObject != null)
                {
                    var wander = targetGameObject.GetComponent<ActionRPG.Town.NPCWanderController2D>();
                    if (wander != null && CharacterStats.PlayerInstance != null)
                    {
                        wander.FleeFrom(CharacterStats.PlayerInstance.transform.position);
                    }
                }

                EventBus.OnPetCaptureResult?.Invoke(enemyName, false);
                return false;
            }
        }

        private PetInstance CreatePetInstanceFromEnemy(EnemyController enemyController, string enemyName, ElementType elem, float maxHp, float atk, float def, float speed, int level, GameObject targetObj = null)
        {
            PetData data = ScriptableObject.CreateInstance<PetData>();
            data.petId = $"pet_{enemyName.ToLower().Replace(" ", "_")}_{Random.Range(100, 999)}";
            data.petName = enemyName;
            data.speciesName = enemyName;

            // Xác định hệ nguyên tố phù hợp cho động vật nếu chưa có
            if (elem == ElementType.None)
            {
                string lower = enemyName.ToLower();
                if (lower.Contains("hươu") || lower.Contains("deer")) elem = ElementType.Earth;
                else if (lower.Contains("thỏ") || lower.Contains("rabbit")) elem = ElementType.Wind;
                else if (lower.Contains("sói") || lower.Contains("wolf")) elem = ElementType.Fire;
                else if (lower.Contains("heo") || lower.Contains("boar")) elem = ElementType.Earth;
                else if (lower.Contains("cua") || lower.Contains("crab")) elem = ElementType.Water;
                else elem = ElementType.Earth;
            }

            data.element = elem;
            data.maxHp = Mathf.Max(50f, maxHp * 0.85f);
            data.attackPower = Mathf.Max(8f, atk * 0.80f);
            data.defense = Mathf.Max(2f, def);
            data.moveSpeed = Mathf.Max(4.0f, speed);
            data.role = PetRole.DPS;

            GameObject src = enemyController != null ? enemyController.gameObject : targetObj;
            if (src != null)
            {
                SpriteRenderer sr = src.GetComponent<SpriteRenderer>() ?? src.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    data.petSprite = sr.sprite;
                    data.icon = sr.sprite;
                }
            }

            // Kỹ năng chủ động cơ bản của Pet mới bắt
            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillId = $"skill_{data.petId}_bite";
            skill.skillName = $"{enemyName} Vồ";
            skill.element = data.element;
            skill.damageMultiplier = 1.35f;
            skill.cooldownSeconds = 3.5f;
            skill.range = 2.0f;
            data.activeSkill = skill;

            // Khởi tạo PetInstance với điểm thân mật khởi điểm 30/100
            PetInstance petInstance = new PetInstance(data, Mathf.Max(1, level), 30f);
            return petInstance;
        }

        public ItemData GetCaptureItemReference(ItemData overrideItem = null)
        {
            if (overrideItem != null) return overrideItem;

            if (Inventory.Instance != null)
            {
                ItemData orb = Inventory.Instance.GetItemById("item_spirit_orb");
                if (orb != null) return orb;

                ItemData scroll = Inventory.Instance.GetItemById("item_capture_scroll");
                if (scroll != null) return scroll;
            }

            ItemData loaded = Resources.Load<ItemData>("Items/Item_Capture_Scroll");
            if (loaded != null) return loaded;

            // Tạo item Quả Cầu Tinh Linh mặc định
            ItemData spiritOrb = ScriptableObject.CreateInstance<ItemData>();
            spiritOrb.itemId = "item_spirit_orb";
            spiritOrb.itemName = "Quả Cầu Tinh Linh";
            spiritOrb.itemType = ItemType.CaptureScroll;
            spiritOrb.rarity = ItemRarity.Uncommon;
            spiritOrb.isStackable = true;
            spiritOrb.maxStack = 99;
            spiritOrb.description = "Quả cầu huyền bí dùng để thu phục quái vật dã ngoại khi máu của chúng giảm xuống dưới 30%.";
            return spiritOrb;
        }
    }
}
