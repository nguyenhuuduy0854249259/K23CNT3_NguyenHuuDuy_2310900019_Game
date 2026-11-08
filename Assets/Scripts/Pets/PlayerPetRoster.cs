using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Items;

namespace ActionRPG.Pets
{
    /// <summary>
    /// Quản lý toàn bộ danh sách Thú Cưng mà người chơi sở hữu (Pet Roster / Kho Pet).
    /// Điều phối việc triệu hồi, thu hồi (phím T), nuôi dưỡng, và tương tác với các Class Mechanics.
    /// </summary>
    public class PlayerPetRoster : MonoBehaviour
    {
        public static PlayerPetRoster Instance { get; private set; }

        [Header("Kho Thú Cưng Của Người Chơi")]
        public List<PetInstance> ownedPets = new List<PetInstance>();

        [Header("Pet Đang Xuất Chiến")]
        public PetInstance activePetInstance;
        public PetController activePetController;

        [Header("Pet Thứ Hai (Dành riêng cho Pháp Sư Triệu Hồi)")]
        public PetInstance secondaryPetInstance;
        public PetController secondaryPetController;

        [Header("Cài Đặt Tự Động")]
        [Tooltip("Tự động triệu hồi Pet đồng hành khi vào Game nếu đã có Pet trong kho")]
        public bool autoSummonAtStart = true;
        [Tooltip("Prefab Pet cơ sở dùng để sinh ra trong Scene nếu chưa có sẵn")]
        public GameObject petPrefab;

        public event Action<PetInstance> OnActivePetChanged;
        public event Action<PetInstance> OnPetAdded;

        public static bool IsPetUnlocked
        {
            get
            {
                var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
                return stats != null ? (stats.isPetUnlocked || stats.level >= stats.petUnlockLevel) : false;
            }
        }

        public static int UnlockLevel
        {
            get
            {
                var stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
                return stats != null ? stats.petUnlockLevel : 10;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            EventBus.OnPetSystemUnlocked += HandlePetSystemUnlocked;

            if (IsPetUnlocked)
            {
                InitializeStarterPetIfNeeded();

                if (autoSummonAtStart && ownedPets.Count > 0 && activePetController == null)
                {
                    SummonPet(ownedPets[0]);
                }
            }
            else
            {
                // Nếu chưa mở khóa mà trong scene có sẵn Pet, tạm ẩn đi
                PetController scenePet = FindObjectOfType<PetController>(true);
                if (scenePet != null && scenePet.gameObject.activeSelf)
                {
                    scenePet.gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            EventBus.OnPetSystemUnlocked -= HandlePetSystemUnlocked;
        }

        private void HandlePetSystemUnlocked()
        {
            Debug.Log("<color=green>[PlayerPetRoster] Sự kiện mở khóa Thú Cưng kích hoạt! Đang chuẩn bị đồng hành cùng người chơi.</color>");
            InitializeStarterPetIfNeeded();

            if (autoSummonAtStart && ownedPets.Count > 0 && (activePetController == null || !activePetController.gameObject.activeSelf))
            {
                SummonPet(ownedPets[0]);
            }
        }

        private void Update()
        {
            // Phím tắt [P]: Triệu hồi ↔ Thu hồi Pet
            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePetSummon();
            }
        }

        /// <summary>
        /// Khởi tạo Thú Cưng khởi đầu cho nhân vật nếu kho đang trống.
        /// Ưu tiên tìm Pet_Starter trong Scene hoặc nạp Pet_FireFox từ Resources/ScriptableObjects.
        /// </summary>
        private void InitializeStarterPetIfNeeded()
        {
            // 1. Kiểm tra xem trong Scene đã có GameObject Pet nào thuộc sở hữu chưa (bỏ qua các Pet hoang dã dã ngoại)
            PetController[] allScenePets = FindObjectsOfType<PetController>(true);
            PetController scenePet = null;
            foreach (var p in allScenePets)
            {
                if (!p.isWild)
                {
                    scenePet = p;
                    break;
                }
            }
            if (scenePet != null && scenePet.petData != null)
            {
                if (ownedPets.Count == 0)
                {
                    PetInstance starterInstance = new PetInstance(scenePet.petData, scenePet.level, 60f);
                    starterInstance.currentHp = scenePet.currentHp;
                    ownedPets.Add(starterInstance);
                    activePetInstance = starterInstance;
                }

                // Gán liên kết controller
                activePetController = scenePet;
                scenePet.BindInstance(activePetInstance ?? ownedPets[0]);
                return;
            }

            // 2. Nếu chưa có, nạp ScriptableObject Pet_FireFox
            if (ownedPets.Count == 0)
            {
                PetData starterData = null;
#if UNITY_EDITOR
                starterData = UnityEditor.AssetDatabase.LoadAssetAtPath<PetData>("Assets/ScriptableObjects/Pets/Pet_FireFox.asset");
#endif
                if (starterData == null)
                {
                    starterData = Resources.Load<PetData>("ScriptableObjects/Pets/Pet_FireFox");
                }

                if (starterData == null)
                {
                    // Fallback tự sinh instance đầy đủ thông số chuẩn Lumina
                    starterData = ScriptableObject.CreateInstance<PetData>();
                    starterData.petId = "pet_firefox";
                    starterData.petName = "Hồ Ly Lửa";
                    starterData.speciesName = "Linh Hồ";
                    starterData.petType = PetType.Beast;
                    starterData.element = ElementType.Fire;
                    starterData.role = PetRole.DPS;
                    starterData.maxHp = 90f;
                    starterData.attackPower = 15f;
                    starterData.defense = 5f;
                    starterData.moveSpeed = 5.2f;
                }

                PetInstance instance = new PetInstance(starterData, 1, 65f);
                ownedPets.Add(instance);
                activePetInstance = instance;
                Debug.Log($"<color=cyan>[PlayerPetRoster] Đã cấp Thú Cưng khởi đầu: {instance.DisplayName} (Hệ {instance.Element})!</color>");
            }
        }

        /// <summary>
        /// Thêm Pet mới thu phục được vào kho.
        /// </summary>
        public void AddPet(PetInstance newPet)
        {
            if (newPet == null) return;
            ownedPets.Add(newPet);
            OnPetAdded?.Invoke(newPet);
            Debug.Log($"<color=green>[PlayerPetRoster] Thêm {newPet.DisplayName} vào Kho Thú Cưng! Hiện có {ownedPets.Count} Pet.</color>");

            // Nếu chưa có Pet nào đang xuất chiến thì tự động gọi ra luôn
            if (activePetInstance == null || activePetController == null)
            {
                SummonPet(newPet);
            }
        }

        /// <summary>
        /// Bật/Tắt trạng thái triệu hồi của Pet đang chọn (Phím [T]).
        /// </summary>
        public void TogglePetSummon()
        {
            if (!IsPetUnlocked)
            {
                string warningMsg = $"Cần đạt Cấp {UnlockLevel} để mở khóa Đồng Hành!";
                Debug.LogWarning($"<color=orange>[PlayerPetRoster] {warningMsg}</color>");
                EventBus.OnShowUIMessage?.Invoke(warningMsg);
                return;
            }

            if (activePetController != null && activePetController.gameObject.activeSelf)
            {
                DismissPet(activePetInstance);
            }
            else
            {
                if (activePetInstance == null && ownedPets.Count > 0)
                {
                    activePetInstance = ownedPets[0];
                }

                if (activePetInstance != null)
                {
                    SummonPet(activePetInstance);
                }
            }
        }

        /// <summary>
        /// Triệu hồi Thú Cưng ra trận.
        /// </summary>
        public void SummonPet(PetInstance pet)
        {
            if (pet == null) return;

            if (!IsPetUnlocked)
            {
                string warningMsg = $"Cần đạt Cấp {UnlockLevel} để mở khóa Đồng Hành!";
                Debug.LogWarning($"<color=orange>[PlayerPetRoster] {warningMsg}</color>");
                EventBus.OnShowUIMessage?.Invoke(warningMsg);
                return;
            }

            Transform player = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.transform :
                               GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("[PlayerPetRoster] Không tìm thấy Player để triệu hồi Pet!");
                return;
            }

            activePetInstance = pet;
            pet.isSummoned = true;

            // Kiểm tra xem đã có activePetController chưa
            if (activePetController == null)
            {
                // Tìm trong Scene trước
                activePetController = FindObjectOfType<PetController>(true);
            }

            if (activePetController == null)
            {
                GameObject prefabSource = (pet.petData != null && pet.petData.petPrefab != null) ? pet.petData.petPrefab : petPrefab;
                GameObject pObj;

                if (prefabSource != null)
                {
                    pObj = Instantiate(prefabSource, player.position + Vector3.left * 1.5f, Quaternion.identity);
                    pObj.name = $"Pet_{pet.DisplayName}";
                }
                else
                {
                    pObj = new GameObject($"Pet_{pet.DisplayName}");
                    pObj.transform.position = player.position + Vector3.left * 1.5f;
                }

                SpriteRenderer sr = pObj.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = pObj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 10;
                }
                if (pet.petData != null && pet.petData.petSprite != null && sr.sprite == null)
                {
                    sr.sprite = pet.petData.petSprite;
                }

                CircleCollider2D col = pObj.GetComponent<CircleCollider2D>();
                if (col == null)
                {
                    col = pObj.AddComponent<CircleCollider2D>();
                    col.radius = 0.35f;
                }
                col.isTrigger = true;

                Rigidbody2D rb = pObj.GetComponent<Rigidbody2D>();
                if (rb == null) rb = pObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;

                activePetController = pObj.GetComponent<PetController>();
                if (activePetController == null) activePetController = pObj.AddComponent<PetController>();

                PetAI ai = pObj.GetComponent<PetAI>();
                if (ai == null) ai = pObj.AddComponent<PetAI>();
                ai.SetOwner(player);
            }

            activePetController.gameObject.SetActive(true);
            activePetController.transform.position = player.position + Vector3.left * 1.5f;
            activePetController.BindInstance(pet);
            activePetController.PlaySummonEffect();

            OnActivePetChanged?.Invoke(pet);
            EventBus.OnPetSummoned?.Invoke(pet);
            EventBus.OnPetHpChanged?.Invoke(pet.currentHp, pet.MaxHp);
            EventBus.OnPetBondChanged?.Invoke(pet.bondLevel, 100f);

            Debug.Log($"<color=cyan>[PlayerPetRoster] Đã triệu hồi Pet: {pet.DisplayName} (Cấp {pet.level}, HP: {pet.currentHp}/{pet.MaxHp})!</color>");
        }

        /// <summary>
        /// Thu hồi Thú Cưng về kho.
        /// </summary>
        public void DismissPet(PetInstance pet = null)
        {
            if (pet == null) pet = activePetInstance;

            if (activePetController != null && activePetController.gameObject.activeSelf)
            {
                activePetController.PlayDismissEffect(() =>
                {
                    activePetController.gameObject.SetActive(false);
                });
            }

            if (pet != null)
            {
                pet.isSummoned = false;
            }

            EventBus.OnPetDismissed?.Invoke(pet);
            Debug.Log($"<color=yellow>[PlayerPetRoster] Đã thu hồi Pet {pet?.DisplayName} về kho!</color>");
        }

        /// <summary>
        /// Hỗ trợ Pháp Sư Triệu Hồi: Triệu hồi Pet thứ hai ra trận cùng lúc.
        /// </summary>
        public void SummonSecondaryPet(PetInstance pet)
        {
            if (pet == null) return;

            if (!IsPetUnlocked)
            {
                string warningMsg = $"Cần đạt Cấp {UnlockLevel} để mở khóa Đồng Hành!";
                Debug.LogWarning($"<color=orange>[PlayerPetRoster] {warningMsg}</color>");
                EventBus.OnShowUIMessage?.Invoke(warningMsg);
                return;
            }

            Transform player = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance.transform :
                               GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;

            secondaryPetInstance = pet;
            pet.isSummoned = true;

            if (secondaryPetController == null)
            {
                GameObject prefabSource = (pet.petData != null && pet.petData.petPrefab != null) ? pet.petData.petPrefab : petPrefab;
                GameObject p2Obj;

                if (prefabSource != null)
                {
                    p2Obj = Instantiate(prefabSource, player.position + Vector3.right * 1.5f, Quaternion.identity);
                    p2Obj.name = $"Pet2_{pet.DisplayName}";
                }
                else
                {
                    p2Obj = new GameObject($"Pet2_{pet.DisplayName}");
                    p2Obj.transform.position = player.position + Vector3.right * 1.5f;
                }

                SpriteRenderer sr = p2Obj.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = p2Obj.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 10;
                }
                if (pet.petData != null && pet.petData.petSprite != null && sr.sprite == null)
                    sr.sprite = pet.petData.petSprite;

                CircleCollider2D col = p2Obj.GetComponent<CircleCollider2D>();
                if (col == null)
                {
                    col = p2Obj.AddComponent<CircleCollider2D>();
                    col.radius = 0.35f;
                }
                col.isTrigger = true;

                Rigidbody2D rb = p2Obj.GetComponent<Rigidbody2D>();
                if (rb == null) rb = p2Obj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;

                secondaryPetController = p2Obj.GetComponent<PetController>();
                if (secondaryPetController == null) secondaryPetController = p2Obj.AddComponent<PetController>();
                secondaryPetController.isSecondaryPet = true;

                PetAI ai = p2Obj.GetComponent<PetAI>();
                if (ai == null) ai = p2Obj.AddComponent<PetAI>();
                ai.SetOwner(player);
            }

            secondaryPetController.gameObject.SetActive(true);
            secondaryPetController.transform.position = player.position + Vector3.right * 1.5f;
            secondaryPetController.BindInstance(pet);
            secondaryPetController.PlaySummonEffect();

            Debug.Log($"<color=magenta>[SummonerDualPet] Đã triệu hồi Pet thứ 2: {pet.DisplayName} (Hệ {pet.Element}) ra trận!</color>");
        }

        /// <summary>
        /// Cho Pet ăn từ túi đồ.
        /// </summary>
        public bool FeedActivePet(ItemData foodItem)
        {
            if (activePetInstance == null)
            {
                Debug.LogWarning("[PlayerPetRoster] Chưa có Pet nào xuất chiến để cho ăn!");
                return false;
            }

            if (Inventory.Instance != null && !Inventory.Instance.RemoveItem(foodItem, 1))
            {
                Debug.LogWarning("[PlayerPetRoster] Không đủ thức ăn trong túi đồ!");
                return false;
            }

            return activePetInstance.Feed(foodItem);
        }
    }
}
