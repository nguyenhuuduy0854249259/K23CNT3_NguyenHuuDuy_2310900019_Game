using System;
using System.Collections;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Pets
{
    /// <summary>
    /// Bộ điều khiển thực thể Pet trong Scene:
    /// Quản lý hiển thị, va chạm, nhận sát thương, hiệu ứng triệu hồi/thu hồi,
    /// và tương thích với các cơ chế Class (Biến hình vũ khí Cận chiến, Phân thân Sát thủ).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PetController : MonoBehaviour
    {
        [Header("Dữ Liệu Thú Cưng")]
        public PetData petData;
        public PetInstance petInstance;
        public Transform owner;

        [Header("Chỉ Số Hiện Tại (Đồng bộ Runtime)")]
        public int level = 1;
        public int currentExp = 0;
        public float currentHp = 90f;
        public float maxHp = 90f;
        public float bondLevel = 50f;

        [Header("Khoảng Cách Định Tuyến")]
        [Tooltip("Cự ly tối thiểu an toàn bám theo Player (1.5m)")]
        public float minFollowDistance = 1.5f;
        [Tooltip("Cự ly tối đa bắt đầu bám theo Player (2.5m)")]
        public float maxFollowDistance = 2.5f;
        [Tooltip("Khoảng cách tự động Teleport về cạnh Player nếu quá xa (> 12m)")]
        public float teleportDistance = 12.0f;

        [Header("Trạng Thái Chiến Đấu & Class")]
        public bool isManualMode = false;
        public bool isTransformedIntoWeapon = false; // Khi Cận Chiến Tự Do biến pet thành vũ khí
        public bool isSecondaryPet = false;          // Hỗ trợ Pháp Sư Triệu Hồi gọi 2 Pet
        [Tooltip("Pet sinh sống dã ngoại trong Map (chưa có chủ sở hữu), không tự bắt Player làm chủ")]
        public bool isWild = false;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private CircleCollider2D col;
        private PetAI petAI;
        private Animator animator;
        private Coroutine fadeCoroutine;

        public void EnsureComponents()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (col == null) col = GetComponent<CircleCollider2D>();
            if (petAI == null) petAI = GetComponent<PetAI>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Awake()
        {
            EnsureComponents();

            if (petAI != null)
            {
                petAI.minFollowDistance = minFollowDistance;
                petAI.maxFollowDistance = maxFollowDistance;
                petAI.teleportDistance = teleportDistance;
            }
        }

        private void Start()
        {
            if (petInstance != null)
            {
                SyncFromInstance();
            }
            else if (petData != null)
            {
                Initialize(petData);
            }

            if (isWild)
            {
                // Pet hoang dã dã ngoại trong Map: Không tự bắt Player làm chủ và không bị ẩn
                return;
            }

            if (owner == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) owner = player.transform;
            }

            // Nếu người chơi chưa đủ cấp mở khóa (Tier 1: Lv 1-9), Pet tuyệt đối không được phép xuất hiện hay theo sau
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            if (stats != null && (!stats.isPetUnlocked && stats.level < stats.petUnlockLevel))
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Phím tắt [P]: Triệu hồi ↔ Thu hồi Pet đồng hành
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Nếu có PlayerPetRoster xử lý tập trung thì để Roster điều phối;
                // Nếu chạy độc lập, PetController tự xử lý kiểm tra cấp độ và toggle.
                if (PlayerPetRoster.Instance == null)
                {
                    HandleSummonToggle();
                }
            }
        }

        public void BindInstance(PetInstance instance)
        {
            petInstance = instance;
            if (instance != null)
            {
                petData = instance.petData;
                SyncFromInstance();
            }
        }

        public void SyncFromInstance()
        {
            EnsureComponents();
            if (petInstance == null) return;
            level = petInstance.level;
            currentExp = petInstance.currentExp;
            currentHp = petInstance.currentHp;
            maxHp = petInstance.MaxHp;
            bondLevel = petInstance.bondLevel;

            SyncVisualAndAnimator(petData);
        }

        public void Initialize(PetData data)
        {
            EnsureComponents();
            petData = data;
            if (petInstance == null)
            {
                petInstance = new PetInstance(data, level, bondLevel);
            }
            maxHp = petInstance.MaxHp;
            currentHp = maxHp;

            SyncVisualAndAnimator(data);
        }

        private void SyncVisualAndAnimator(PetData data)
        {
            if (data == null) return;

            if (spriteRenderer != null && data.petSprite != null)
            {
                spriteRenderer.sprite = data.petSprite;
            }

            if (data.petPrefab != null)
            {
                Animator prefabAnim = data.petPrefab.GetComponent<Animator>();
                if (prefabAnim != null && prefabAnim.runtimeAnimatorController != null)
                {
                    if (animator == null) animator = gameObject.AddComponent<Animator>();
                    animator.runtimeAnimatorController = prefabAnim.runtimeAnimatorController;
                }
            }
        }

        public void TakeDamage(DamageInfo info)
        {
            if (isTransformedIntoWeapon) return;

            float def = petInstance != null ? petInstance.Defense : (petData != null ? petData.defense : 3f);
            float finalDamage = Mathf.Max(1f, info.rawDamage - def);

            currentHp = Mathf.Max(0f, currentHp - finalDamage);
            if (petInstance != null)
            {
                petInstance.currentHp = currentHp;
            }

            EventBus.OnPetHpChanged?.Invoke(currentHp, maxHp);

            // Bật nhấp nháy đỏ khi trúng đòn
            StartCoroutine(FlashHurtEffect());

            if (currentHp <= 0f)
            {
                Debug.Log($"<color=orange>[Pet] {petData?.petName ?? "Pet"} đã kiệt sức và tạm thời thu hồi!</color>");
                PlayDismissEffect(() => gameObject.SetActive(false));
            }
        }

        private IEnumerator FlashHurtEffect()
        {
            if (spriteRenderer == null) yield break;
            Color orig = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.4f, 0.4f, orig.a);
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = orig;
        }

        public void GainExp(int exp)
        {
            if (petInstance != null)
            {
                petInstance.GainExp(exp);
                SyncFromInstance();
            }
            else
            {
                currentExp += exp;
                if (currentExp >= level * 60)
                {
                    currentExp -= level * 60;
                    level++;
                    maxHp += 14f;
                    currentHp = maxHp;
                    Debug.Log($"[Pet] {petData?.petName} đã thăng lên cấp {level}!");
                }
            }
        }

        public bool Evolve()
        {
            if (petInstance != null && petInstance.CanEvolve())
            {
                bool success = petInstance.Evolve();
                if (success)
                {
                    petData = petInstance.petData;
                    SyncFromInstance();
                }
                return success;
            }
            return false;
        }

        #region Định Tuyến & Di Chuyển (Follow & Teleport)
        /// <summary>
        /// Tự động dịch chuyển Pet về gần chủ nhân khi cự ly vượt quá teleportDistance (> 12m).
        /// </summary>
        public void TeleportToOwner()
        {
            EnsureComponents();
            if (petAI != null)
            {
                petAI.TeleportToOwner();
            }
            else if (owner != null)
            {
                transform.position = owner.position + Vector3.left * 1.5f;
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
        #endregion

        #region Hiệu Ứng Triệu Hồi & Thu Hồi (Phím [T] / Summon & Dismiss)
        /// <summary>
        /// Kiểm tra điều kiện mở khóa cấp độ để triệu hồi Pet (Mặc định Cấp 10).
        /// </summary>
        public bool CanSummon(out string reason)
        {
            var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
            int unlockLevel = stats != null ? stats.petUnlockLevel : 10;
            bool isUnlocked = stats != null ? stats.isPetUnlocked : false;

            if (!isUnlocked)
            {
                reason = $"Cần đạt Cấp {unlockLevel} để mở khóa Đồng Hành!";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Xử lý thao tác bấm phím [T] (Triệu hồi ↔ Thu hồi).
        /// Chặn và báo lỗi nếu chưa đủ cấp mở khóa.
        /// </summary>
        public bool HandleSummonToggle()
        {
            if (!CanSummon(out string reason))
            {
                Debug.LogWarning($"<color=orange>[PetController] {reason}</color>");
                EventBus.OnShowUIMessage?.Invoke(reason);
                return false;
            }

            if (gameObject.activeSelf)
            {
                Dismiss();
            }
            else
            {
                Summon();
            }
            return true;
        }

        /// <summary>
        /// Triệu hồi Pet ra chiến trường và kích hoạt AI bám theo chủ nhân.
        /// </summary>
        public void Summon()
        {
            if (!CanSummon(out string reason))
            {
                Debug.LogWarning($"<color=orange>[PetController] {reason}</color>");
                EventBus.OnShowUIMessage?.Invoke(reason);
                return;
            }

            gameObject.SetActive(true);
            if (owner != null)
            {
                transform.position = owner.position + (isSecondaryPet ? Vector3.right : Vector3.left) * 1.5f;
            }
            if (petAI != null)
            {
                petAI.enabled = true;
            }
            PlaySummonEffect();

            if (petInstance != null)
            {
                petInstance.isSummoned = true;
            }

            EventBus.OnPetSummoned?.Invoke(petInstance ?? (object)petData);
            EventBus.OnPetHpChanged?.Invoke(currentHp, maxHp);
            EventBus.OnPetBondChanged?.Invoke(bondLevel, 100f);
        }

        /// <summary>
        /// Thu hồi Pet về trạng thái nghỉ.
        /// </summary>
        public void Dismiss()
        {
            PlayDismissEffect(() =>
            {
                gameObject.SetActive(false);
                if (petInstance != null)
                {
                    petInstance.isSummoned = false;
                }
                EventBus.OnPetDismissed?.Invoke(petInstance ?? (object)petData);
            });
        }

        public void PlaySummonEffect()
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, 0.35f, null));
        }

        public void PlayDismissEffect(Action onComplete)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0f, 0.3f, onComplete));
        }

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            if (spriteRenderer == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            Color c = spriteRenderer.color;

            // Hiệu ứng ánh sáng ma thuật / bung sáng
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float a = Mathf.Lerp(fromAlpha, toAlpha, t);
                spriteRenderer.color = new Color(c.r, c.g, c.b, a);

                // Nở nhẹ kích thước khi xuất hiện
                if (fromAlpha < toAlpha)
                {
                    float scaleMultiplier = Mathf.Lerp(0.5f, 1.0f, t);
                    transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1f);
                }

                yield return null;
            }

            c.a = toAlpha;
            spriteRenderer.color = c;
            transform.localScale = Vector3.one;
            onComplete?.Invoke();
        }
        #endregion

        #region Tương Thích Các Cơ Chế Class (Cận Chiến Hóa Vũ Khí, Sát Thủ Decoy)
        /// <summary>
        /// Dành cho Class CẬN CHIẾN TỰ DO: Pet biến hình thành vũ khí -> Ẩn mô hình Pet và ngưng tương tác.
        /// </summary>
        public void HideForWeaponTransform()
        {
            EnsureComponents();
            isTransformedIntoWeapon = true;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (col != null) col.enabled = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (petAI != null) petAI.enabled = false;

            Debug.Log($"<color=cyan>[PetController] Ẩn mô hình Pet {petData?.petName}: Đã nhập thể biến thành vũ khí cho Player!</color>");
        }

        /// <summary>
        /// Khôi phục mô hình Pet sau khi thôi biến hình vũ khí.
        /// </summary>
        public void RestoreFromWeaponTransform()
        {
            EnsureComponents();
            isTransformedIntoWeapon = false;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            if (col != null) col.enabled = true;
            if (petAI != null) petAI.enabled = true;

            PlaySummonEffect();
            Debug.Log($"<color=cyan>[PetController] Khôi phục mô hình Pet {petData?.petName}: Tách khỏi vũ khí, trở lại chiến trường!</color>");
        }

        /// <summary>
        /// Dành cho Class SÁT THỦ BÓNG ĐÊM: Pet tạo phân thân Decoy thu hút quái vật xung quanh.
        /// </summary>
        public GameObject SpawnDecoy(float duration = 4.5f)
        {
            GameObject decoy = new GameObject($"Decoy_{name}");
            decoy.transform.position = transform.position;

            SpriteRenderer dSr = decoy.AddComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                dSr.sprite = spriteRenderer.sprite;
                dSr.color = new Color(0.7f, 0.3f, 1f, 0.65f); // Tím mờ ảo
                dSr.sortingOrder = spriteRenderer.sortingOrder - 1;
            }

            CircleCollider2D dCol = decoy.AddComponent<CircleCollider2D>();
            dCol.radius = 1.0f;
            dCol.isTrigger = true;

            // Tự hủy sau duration
            Destroy(decoy, duration);

            Debug.Log($"<color=magenta>[PetController] Phân thân Decoy của Pet đã được triển khai tại {transform.position} (Tồn tại {duration}s)!</color>");
            return decoy;
        }

        /// <summary>
        /// Dành cho Class PHÁP SƯ TRIỆU HỒI (Summoner): Hỗ trợ gọi 2 Pet cùng lúc ra chiến trường.
        /// </summary>
        public static void SummonDualPets(PetInstance primaryPet, PetInstance secondaryPet)
        {
            if (PlayerPetRoster.Instance != null)
            {
                if (primaryPet != null) PlayerPetRoster.Instance.SummonPet(primaryPet);
                if (secondaryPet != null) PlayerPetRoster.Instance.SummonSecondaryPet(secondaryPet);
            }
        }

        /// <summary>
        /// Hỗ trợ Pháp Sư Triệu Hồi: Khởi tạo và gọi 2 Pet từ PetData ScriptableObjects.
        /// </summary>
        public static void SummonDualPets(PetData petData1, PetData petData2)
        {
            PetInstance p1 = petData1 != null ? new PetInstance(petData1, 1, 50f) : null;
            PetInstance p2 = petData2 != null ? new PetInstance(petData2, 1, 50f) : null;
            SummonDualPets(p1, p2);
        }
        #endregion
    }
}
