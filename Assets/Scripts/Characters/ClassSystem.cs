using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Classes;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Core;
using ActionRPG.Combat;
using ActionRPG.Pets;
using ActionRPG.Items;

namespace ActionRPG.Characters
{
    /// <summary>
    /// Quản lý class nhân vật toàn diện của Lumina:
    /// - Tiến trình: Novice (Lv1) -> Chọn 1 trong 3 nhánh cơ bản (Lv10) -> Chuyển chức chuyên sâu (Lv30+).
    /// - Tích hợp đầy đủ 10 hệ thống cơ chế chuyên biệt (PetWeapon, ElementalQueue, SummonerDualPet,
    ///   ChronoMage, HolyDarkPriest, AlchemistCauldron, EngineerGunner, ShadowGunner, ElementalArcher, SpiritArcher).
    /// </summary>
    public class ClassSystem : MonoBehaviour
    {
        public static ClassSystem Instance { get; private set; }

        [Header("Class Hiện Tại")]
        public CharacterClassData currentClass;

        // ──────────────────────────────────────────────────────
        // CÁC MODULE CƠ CHẾ CHUYÊN BIỆT (MODULAR MECHANICS)
        // ──────────────────────────────────────────────────────
        [Header("Cơ Chế Cận Chiến")]
        public PetWeaponMechanics PetWeapon { get; private set; }

        [Header("Cơ Chế Pháp Sư")]
        public ElementalQueueMechanics ElementalQueue { get; private set; }
        public SummonerDualPetMechanics SummonerDualPet { get; private set; }
        public ChronoMageMechanics ChronoMage { get; private set; }
        public HolyDarkPriestMechanics HolyDarkPriest { get; private set; }
        public AlchemistCauldronMechanics AlchemistCauldron { get; private set; }

        [Header("Cơ Chế Xạ Thủ")]
        public EngineerGunnerMechanics EngineerGunner { get; private set; }
        public ShadowGunnerMechanics ShadowGunner { get; private set; }
        public ElementalArcherMechanics ElementalArcher { get; private set; }
        public SpiritArcherMechanics SpiritArcher { get; private set; }

        // ──────────────────────────────────────────────────────
        // TRUY XUẤT NHANH (BACKWARD COMPATIBILITY)
        // ──────────────────────────────────────────────────────
        public int spiritCharges => SpiritArcher != null ? SpiritArcher.currentCharges : 3;
        public float gunHeat => EngineerGunner != null ? EngineerGunner.currentHeat : 0f;
        public bool isGunOverheated => EngineerGunner != null && EngineerGunner.isOverheated;
        public bool isAngelStance => HolyDarkPriest == null || HolyDarkPriest.currentForm == PriestForm.Angel;
        public bool isStealthed => ShadowGunner != null && ShadowGunner.isStealthed;

        [Header("Cận Chiến – Parry")]
        public bool isBlocking = false;
        public float parryWindowTimer = 0f;

        // Tham chiếu nội bộ
        private CharacterStats stats;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (Instance == null || Instance.currentClass == null)
            {
                Instance = this;
            }
            stats = GetComponent<CharacterStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Đồng bộ dữ liệu nếu trên Player có 2 component ClassSystem do override trong Scene
            ClassSystem[] allOnSelf = GetComponents<ClassSystem>();
            if (allOnSelf != null && allOnSelf.Length > 1)
            {
                CharacterClassData resolvedClass = currentClass;
                for (int i = 0; i < allOnSelf.Length; i++)
                {
                    if (allOnSelf[i] != null && allOnSelf[i].currentClass != null)
                    {
                        resolvedClass = allOnSelf[i].currentClass;
                        break;
                    }
                }
                for (int i = 0; i < allOnSelf.Length; i++)
                {
                    if (allOnSelf[i] != null && resolvedClass != null)
                    {
                        allOnSelf[i].currentClass = resolvedClass;
                    }
                }
                if (allOnSelf[0] != null) Instance = allOnSelf[0];
            }

            InitializeMechanicsComponents();
        }

        private void InitializeMechanicsComponents()
        {
            PetWeapon = GetOrAddComponent<PetWeaponMechanics>();
            ElementalQueue = GetOrAddComponent<ElementalQueueMechanics>();
            SummonerDualPet = GetOrAddComponent<SummonerDualPetMechanics>();
            ChronoMage = GetOrAddComponent<ChronoMageMechanics>();
            HolyDarkPriest = GetOrAddComponent<HolyDarkPriestMechanics>();
            AlchemistCauldron = GetOrAddComponent<AlchemistCauldronMechanics>();
            EngineerGunner = GetOrAddComponent<EngineerGunnerMechanics>();
            ShadowGunner = GetOrAddComponent<ShadowGunnerMechanics>();
            ElementalArcher = GetOrAddComponent<ElementalArcherMechanics>();
            SpiritArcher = GetOrAddComponent<SpiritArcherMechanics>();
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T comp = GetComponent<T>();
            if (comp == null)
                comp = gameObject.AddComponent<T>();
            return comp;
        }

        private void Start()
        {
            // Nếu có nhiều ClassSystem trên cùng 1 GameObject, chỉ chạy Start trên Instance chính
            if (Instance != null && Instance != this && Instance.gameObject == gameObject)
            {
                if (Instance.currentClass != null) currentClass = Instance.currentClass;
                return;
            }

            if (currentClass == null)
            {
#if UNITY_EDITOR
                currentClass = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterClassData>("Assets/ScriptableObjects/Classes/Class_Novice.asset");
#endif
                if (currentClass == null)
                    currentClass = Resources.Load<CharacterClassData>("Classes/Class_Novice");
            }

            if (currentClass != null)
            {
                ClassSystem[] allOnSelf = GetComponents<ClassSystem>();
                for (int i = 0; i < allOnSelf.Length; i++)
                {
                    if (allOnSelf[i] != null) allOnSelf[i].currentClass = currentClass;
                }

                ApplyClassAttributes(currentClass);
                InitializeClassMechanic(currentClass.specialMechanic);
                EventBus.OnClassApplied?.Invoke(currentClass);
                var skills = ClassEvolutionManager.LoadSkillsForClass(currentClass);
                if (skills != null && skills.Count > 0)
                {
                    EventBus.RaiseSkillChanged(skills);
                }
            }
        }

        private void Update()
        {
            // Parry window (áp dụng cho cận chiến phòng thủ)
            if (parryWindowTimer > 0f)
                parryWindowTimer -= Time.deltaTime;
        }

        // ══════════════════════════════════════════════════════
        // CHUYỂN NGHỀ & TIẾN HÓA CLASS
        // ══════════════════════════════════════════════════════

        public bool CanPromote(out List<CharacterClassData> availableOptions)
        {
            availableOptions = new List<CharacterClassData>();
            if (currentClass == null || currentClass.promotionOptions == null || currentClass.promotionOptions.Count == 0)
                return false;

            if (stats != null && stats.level >= currentClass.requiredLevelForPromotion)
            {
                availableOptions = currentClass.promotionOptions;
                return true;
            }
            return false;
        }

        public void PromoteTo(CharacterClassData newClass)
        {
            if (newClass == null) return;
            if (stats == null) stats = GetComponent<CharacterStats>();
            if (HolyDarkPriest == null) InitializeMechanicsComponents();

            currentClass = newClass;
            ClassSystem[] allOnSelf = GetComponents<ClassSystem>();
            for (int i = 0; i < allOnSelf.Length; i++)
            {
                if (allOnSelf[i] != null) allOnSelf[i].currentClass = newClass;
            }
            ApplyClassAttributes(newClass);
            InitializeClassMechanic(newClass.specialMechanic);

            // Nạp dữ liệu kỹ năng từ ClassEvolutionManager
            ClassEvolutionManager.EvolveClass(newClass);

            // Mở khóa tính năng Thu Phục Thú Cưng khi chuyển chức thành 3 chức nghiệp cơ bản (hoặc bất kỳ class nào khác Tân Thủ)
            if (stats != null)
            {
                stats.isPetUnlocked = true;
            }

            // Tặng 5 Quả Cầu Tinh Linh vào túi đồ nếu chưa có để sẵn sàng thu phục quái
            GrantStarterSpiritOrbs();

            // Phát sự kiện mở khóa tính năng Pet trên toàn hệ thống
            EventBus.OnPetSystemUnlocked?.Invoke();

            Debug.Log($"<color=yellow>[ClassSystem] CHUYỂN NGHỀ THÀNH CÔNG: {newClass.className}! Đã mở khóa tính năng Thu Phục Pet!</color>");
            EventBus.OnClassChanged?.Invoke(newClass.className);
            EventBus.OnShowUIMessage?.Invoke($"🎉 Chuyển chức thành công: {newClass.className}! Đã mở khóa tính năng Thu Phục Pet [C] & Gọi Pet [P]!");
        }

        private void GrantStarterSpiritOrbs()
        {
            var inv = Inventory.Instance != null ? Inventory.Instance : FindObjectOfType<Inventory>();
            if (inv != null)
            {
                var existingOrb = inv.GetItemById("item_spirit_orb");
                if (existingOrb == null)
                {
                    var orbItem = PetCaptureSystem.Instance != null ? PetCaptureSystem.Instance.GetCaptureItemReference() : null;
                    if (orbItem != null)
                    {
                        inv.AddItem(orbItem, 5);
                    }
                }
            }
        }

        private void ApplyClassAttributes(CharacterClassData classData)
        {
            if (stats == null) stats = GetComponent<CharacterStats>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (classData.characterSprite != null && spriteRenderer != null)
                spriteRenderer.sprite = classData.characterSprite;

            if (stats != null)
            {
                stats.baseAtk = classData.baseAtk;
                stats.baseDef = classData.baseDef;
                stats.baseCritRate = classData.baseCritRate;
                stats.maxHp = classData.baseHp + (stats.level - 1) * 18f;
                stats.maxMp = classData.baseMp + (stats.level - 1) * 10f;
                stats.baseMoveSpeed = classData.baseSpeed;
                if (classData.defaultElement != ElementType.None)
                    stats.characterElement = classData.defaultElement;
                stats.currentHp = stats.maxHp;
                stats.currentMp = stats.maxMp;
                stats.UpdateStatsUI();
            }
        }

        public void InitializeClassMechanic(SpecialMechanic mechanic)
        {
            switch (mechanic)
            {
                case SpecialMechanic.SpiritCharge:
                    if (SpiritArcher != null) SpiritArcher.currentCharges = 3;
                    break;
                case SpecialMechanic.GunOverheat:
                case SpecialMechanic.EngineerTurrets:
                    if (EngineerGunner != null)
                    {
                        EngineerGunner.currentHeat = 0f;
                        EngineerGunner.isOverheated = false;
                    }
                    break;
                case SpecialMechanic.ElementalQueue:
                    if (ElementalQueue != null)
                    {
                        ElementalQueue.currentQueue.Clear();
                        ElementalQueue.comboStreak = 0;
                        ElementalQueue.isCataclysmReady = false;
                    }
                    break;
                case SpecialMechanic.HolyDarkPriest:
                    if (HolyDarkPriest != null)
                    {
                        HolyDarkPriest.currentForm = PriestForm.Angel;
                    }
                    break;
                case SpecialMechanic.StealthCritical:
                    if (ShadowGunner != null)
                    {
                        ShadowGunner.isStealthed = false;
                        ShadowGunner.isAmbushReady = false;
                    }
                    break;
                case SpecialMechanic.ElementalQuiver:
                    if (ElementalArcher != null)
                    {
                        ElementalArcher.SelectArrow(ElementalArrowType.Flame);
                    }
                    break;
            }
        }

        // ══════════════════════════════════════════════════════
        // CÁC HÀM TIỆN ÍCH CHO PLAYER CONTROLLER & HUD
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// Xạ Thủ Tinh Linh: Tiêu 1 charge triệu hồi Linh Thể.
        /// </summary>
        public bool ConsumeSpiritCharge(ElementType petElement)
        {
            return SpiritArcher != null && SpiritArcher.TrySpawnRegularSpirit(petElement);
        }

        /// <summary>
        /// Xạ Thủ Tinh Linh: Hợp thể 3 charge thành Linh Thể khổng lồ.
        /// </summary>
        public bool TriggerSpiritFusion(ElementType petElement)
        {
            return SpiritArcher != null && SpiritArcher.TryTriggerSpiritFusion(petElement);
        }

        /// <summary>
        /// Xạ Thủ Súng: Bắn súng tăng nhiệt.
        /// </summary>
        public bool TryShootGun(float heatPerShot, out float damageMultiplier)
        {
            damageMultiplier = 1.0f;
            return EngineerGunner != null && EngineerGunner.Shoot(heatPerShot, out damageMultiplier);
        }

        /// <summary>
        /// Sát Thủ Bóng Đêm: Kích hoạt Tàng Hình.
        /// </summary>
        public bool TryEnterStealth()
        {
            return ShadowGunner != null && ShadowGunner.EnterStealth();
        }

        /// <summary>
        /// Mục Sư Thánh - Ma: Đổi Form giữa Thiên Thần và Ác Quỷ.
        /// </summary>
        public void ToggleStance()
        {
            if (HolyDarkPriest != null)
                HolyDarkPriest.TogglePriestForm();
        }

        /// <summary>
        /// Pháp Sư Nguyên Tố: Nạp nguyên tố vào hàng đợi 2 ô.
        /// </summary>
        public void PushElementalQueue(ElementType element)
        {
            if (ElementalQueue != null)
                ElementalQueue.PushElement(element);
        }

        /// <summary>
        /// Pháp Sư Thời Không: Nghịch chuyển vị trí về 3 giây trước.
        /// </summary>
        public bool TriggerChronoRewind()
        {
            return ChronoMage != null && ChronoMage.TriggerSpatialRewind();
        }
    }
}
