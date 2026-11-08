using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Items;
using ActionRPG.Pets;
using ActionRPG.Core;
using ActionRPG.UI.Gameplay;

namespace ActionRPG.Save
{
    /// <summary>
    /// Singleton Quản lý vòng đời Lưu/Tải dữ liệu an toàn toàn game.
    /// Thu thập thông tin từ PlayerController, CharacterStats, Inventory, ClassSystem, PetController / PlayerPetRoster,
    /// đóng gói SaveData, mã hóa AES-256 và ký SHA-256 bảo vệ dữ liệu.
    /// Hỗ trợ phím tắt [F5] Quick Save, [F9] Quick Load.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Trạng Thái Lưu Trữ")]
        [SerializeField] private bool autoSaveOnQuit = false;

        public SaveData CurrentSaveData { get; private set; }

        public event Action<SaveData> OnGameSaved;
        public event Action<SaveData> OnGameLoaded;
        public event Action<string> OnSaveError;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("<color=cyan>[SaveManager] [F5] Kích hoạt Quick Save...</color>");
                SaveGame();
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("<color=cyan>[SaveManager] [F9] Kích hoạt Quick Load...</color>");
                LoadGame();
            }
#endif
        }

        private void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
            {
                SaveGame();
            }
        }

        public bool HasSaveFile()
        {
            return File.Exists(SaveSecurity.GetSaveDataPath()) && File.Exists(SaveSecurity.GetSaveSignaturePath());
        }

        #region Thu Thập Dữ Liệu & Lưu Game (SaveGame)
        /// <summary>
        /// Thu thập toàn bộ dữ liệu runtime từ các hệ thống, mã hóa AES-256, ký SHA-256 và ghi ra đĩa.
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                SaveData data = new SaveData();

                // 1. PlayerController & Vị trí
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    data.playerData.lastPosition = player.transform.position;
                }
                else
                {
                    GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                    if (pObj != null) data.playerData.lastPosition = pObj.transform.position;
                }

                // 2. CharacterStats & RPG Attributes
                CharacterStats stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
                if (stats != null)
                {
                    data.playerData.level = stats.level;
                    data.playerData.currentExp = stats.currentExp;
                    data.playerData.currentHp = stats.currentHp;
                    data.playerData.currentMp = stats.currentMp;
                    data.playerData.currentTier = stats.CurrentTier.ToString();

                    data.attributes.baseStr = stats.baseStr;
                    data.attributes.baseDex = stats.baseDex;
                    data.attributes.baseInt = stats.baseInt;
                    data.attributes.baseVit = stats.baseVit;
                }

                // 3. ClassSystem
                ClassSystem classSys = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
                if (classSys != null && classSys.currentClass != null)
                {
                    data.playerData.currentClassId = !string.IsNullOrEmpty(classSys.currentClass.name) ? classSys.currentClass.name : classSys.currentClass.className;
                }

                // 4. Inventory & Equipment
                if (Inventory.Instance != null)
                {
                    data.playerData.gold = Inventory.Instance.gold;

                    // Túi đồ
                    data.inventoryData.items.Clear();
                    foreach (var slot in Inventory.Instance.slots)
                    {
                        if (slot != null && slot.item != null && slot.quantity > 0)
                        {
                            data.inventoryData.items.Add(new SavedInventoryItem(slot.item.itemId, slot.item.itemName, slot.quantity));
                        }
                    }

                    // 7 Slot Trang bị
                    data.equipmentData.equippedSlots.Clear();
                    if (Inventory.Instance.equippedItems != null)
                    {
                        foreach (var kvp in Inventory.Instance.equippedItems)
                        {
                            if (kvp.Value != null)
                            {
                                data.equipmentData.equippedSlots.Add(new SavedEquippedSlot(kvp.Key.ToString(), kvp.Value.itemId, kvp.Value.itemName));
                            }
                        }
                    }
                }

                // 5. Thú Cưng (PlayerPetRoster)
                if (PlayerPetRoster.Instance != null)
                {
                    data.petStorageData.pets.Clear();
                    foreach (var pet in PlayerPetRoster.Instance.ownedPets)
                    {
                        if (pet != null)
                        {
                            string pId = pet.petData != null ? pet.petData.name : (!string.IsNullOrEmpty(pet.customName) ? pet.customName : "Pet_Starter");
                            data.petStorageData.pets.Add(new SavedPetData(
                                pId,
                                pet.customName,
                                pet.level,
                                pet.currentHp,
                                pet.bondLevel,
                                pet.isSummoned
                            ));
                        }
                    }

                    if (PlayerPetRoster.Instance.activePetInstance != null)
                    {
                        var ap = PlayerPetRoster.Instance.activePetInstance;
                        data.petStorageData.activePetId = ap.petData != null ? ap.petData.name : ap.customName;
                    }
                }

                // 6. WorldFlags (Waystone, Quests, Bosses)
                var guild = FindObjectOfType<Town.AdventurerGuild>();
                if (guild != null && guild.activeQuests != null)
                {
                    foreach (var q in guild.activeQuests)
                    {
                        if (q.isCompleted)
                        {
                            data.worldFlags.completedQuests.Add(q.questId);
                        }
                    }
                }

                // 7. Metadata
                data.metadata.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.metadata.playTime = Time.realtimeSinceStartup;
                data.metadata.saveVersion = "1.0.0";

                // 8. Đóng gói JSON & Mã Hóa Lưu
                string json = JsonUtility.ToJson(data, true);
                bool success = SaveSecurity.SaveSecureFiles(json);

                if (success)
                {
                    CurrentSaveData = data;
                    OnGameSaved?.Invoke(data);
                    string msg = $"Đã lưu tiến trình thành công! (Lv.{data.playerData.level}, {data.playerData.gold} Vàng) [AES-256 + SHA-256]";
                    Debug.Log($"<color=green><b>[SaveManager] {msg}</b></color>");
                    EventBus.OnShowUIMessage?.Invoke("💾 Đã lưu tiến trình game an toàn [AES-256]!");
                    EventBus.OnPlaySound?.Invoke("save");
                    return true;
                }
                else
                {
                    OnSaveError?.Invoke("Ghi tệp lưu thất bại!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                string err = $"Lỗi ngoại lệ khi lưu game: {ex.Message}";
                Debug.LogError($"<color=red>[SaveManager] {err}\n{ex.StackTrace}</color>");
                OnSaveError?.Invoke(err);
                return false;
            }
        }
        #endregion

        #region Xác Thực & Tải Game (LoadGame)
        /// <summary>
        /// Đọc tệp, kiểm tra chữ ký SHA-256 chống gian lận, giải mã AES-256 và phục hồi toàn bộ thế giới trò chơi.
        /// </summary>
        public bool LoadGame()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("<color=yellow>[SaveManager] Không tìm thấy dữ liệu lưu để tải!</color>");
                EventBus.OnShowUIMessage?.Invoke("Chưa có file lưu nào trên hệ thống!");
                return false;
            }

            try
            {
                // 1. Đọc và xác thực tính toàn vẹn chữ ký SHA-256
                if (!SaveSecurity.LoadSecureFiles(out string plainJson, out string errorMessage))
                {
                    string warning = errorMessage ?? "Dữ liệu lưu đã bị can thiệp!";
                    Debug.LogError($"<color=red><b>[SaveManager] TỪ CHỐI TẢI: {warning}</b></color>");
                    EventBus.OnShowUIMessage?.Invoke($"❌ {warning}");
                    EventBus.OnPlaySound?.Invoke("error");
                    OnSaveError?.Invoke(warning);
                    return false;
                }

                // 2. Chuyển đổi JSON sang SaveData
                SaveData data = JsonUtility.FromJson<SaveData>(plainJson);
                if (data == null)
                {
                    Debug.LogError("<color=red>[SaveManager] Dữ liệu JSON rỗng hoặc lỗi cấu trúc!</color>");
                    return false;
                }

                CurrentSaveData = data;

                // 3. Phục hồi Tọa độ Người Chơi
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.transform.position = data.playerData.lastPosition.ToVector3();
                }

                // 4. Phục hồi CharacterStats & Attributes
                CharacterStats stats = CharacterStats.PlayerInstance != null ? CharacterStats.PlayerInstance : FindObjectOfType<CharacterStats>();
                if (stats != null)
                {
                    stats.level = Mathf.Max(1, data.playerData.level);
                    stats.currentExp = Mathf.Max(0, data.playerData.currentExp);
                    stats.baseStr = data.attributes.baseStr;
                    stats.baseDex = data.attributes.baseDex;
                    stats.baseInt = data.attributes.baseInt;
                    stats.baseVit = data.attributes.baseVit;

                    stats.maxHp = 100f + (stats.level - 1) * 20f + (stats.baseVit - 10) * 15f;
                    stats.maxMp = 50f + (stats.level - 1) * 10f + (stats.baseInt - 10) * 10f;
                    stats.currentHp = Mathf.Clamp(data.playerData.currentHp, 1f, stats.maxHp);
                    stats.currentMp = Mathf.Clamp(data.playerData.currentMp, 0f, stats.maxMp);

                    stats.isPetUnlocked = stats.level >= stats.petUnlockLevel;

                    EventBus.OnPlayerHpChanged?.Invoke(stats.currentHp, stats.maxHp);
                    EventBus.OnPlayerMpChanged?.Invoke(stats.currentMp, stats.maxMp);
                    EventBus.OnPlayerExpChanged?.Invoke(stats.currentExp, CharacterStats.GetRequiredExpForLevel(stats.level));
                }

                // 5. Phục hồi Class
                ClassSystem classSys = ClassSystem.Instance != null ? ClassSystem.Instance : FindObjectOfType<ClassSystem>();
                if (classSys != null && !string.IsNullOrEmpty(data.playerData.currentClassId))
                {
                    CharacterClassData classAsset = FindAssetByNameOrId<CharacterClassData>(data.playerData.currentClassId);
                    if (classAsset != null)
                    {
                        classSys.PromoteTo(classAsset);
                        if (GameplayHUD.Instance != null)
                        {
                            GameplayHUD.Instance.UpdateSkillBar(classAsset);
                        }
                    }
                }

                // 6. Phục hồi Túi Đồ & Trang Bị
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.gold = data.playerData.gold;
                    Inventory.Instance.slots.Clear();

                    if (data.inventoryData != null && data.inventoryData.items != null)
                    {
                        foreach (var savedItem in data.inventoryData.items)
                        {
                            ItemData itemAsset = FindAssetByNameOrId<ItemData>(savedItem.itemId);
                            if (itemAsset != null)
                            {
                                Inventory.Instance.AddItem(itemAsset, savedItem.quantity);
                            }
                        }
                    }

                    // Phục hồi 7 Slot Trang bị
                    if (Inventory.Instance.equippedItems == null)
                    {
                        Inventory.Instance.equippedItems = new Dictionary<EquipSlot, EquipmentData>();
                    }
                    Inventory.Instance.equippedItems.Clear();

                    if (data.equipmentData != null && data.equipmentData.equippedSlots != null)
                    {
                        foreach (var savedEquip in data.equipmentData.equippedSlots)
                        {
                            EquipmentData equipAsset = FindAssetByNameOrId<EquipmentData>(savedEquip.itemId);
                            if (equipAsset != null && Enum.TryParse(savedEquip.slotName, out EquipSlot slot))
                            {
                                Inventory.Instance.equippedItems[slot] = equipAsset;
                            }
                        }
                    }

                    Inventory.Instance.OnInventoryChanged?.Invoke();
                    Inventory.Instance.OnEquipmentChanged?.Invoke();
                }

                // 7. Phục hồi Thú Cưng (PlayerPetRoster)
                if (PlayerPetRoster.Instance != null && data.petStorageData != null)
                {
                    PlayerPetRoster.Instance.ownedPets.Clear();

                    PetInstance activePetToSummon = null;

                    foreach (var savedPet in data.petStorageData.pets)
                    {
                        PetData petDataAsset = FindAssetByNameOrId<PetData>(savedPet.petId);
                        if (petDataAsset != null)
                        {
                            PetInstance instance = new PetInstance(petDataAsset, savedPet.level, savedPet.bondLevel);
                            instance.customName = savedPet.customName;
                            instance.currentHp = savedPet.currentHp;
                            instance.isSummoned = savedPet.isSummoned;

                            PlayerPetRoster.Instance.ownedPets.Add(instance);

                            if (savedPet.petId == data.petStorageData.activePetId || savedPet.isSummoned)
                            {
                                activePetToSummon = instance;
                            }
                        }
                    }

                    // Triệu hồi Pet nếu trước đó đang xuất chiến
                    if (activePetToSummon != null && stats != null && stats.isPetUnlocked)
                    {
                        PlayerPetRoster.Instance.SummonPet(activePetToSummon);
                    }
                }

                // 8. Thông báo thành công
                OnGameLoaded?.Invoke(data);
                string loadMsg = $"Tải game thành công! (Lv.{data.playerData.level}, {data.playerData.gold} Vàng) - Xác thực SHA-256 Khớp 100%!";
                Debug.Log($"<color=green><b>[SaveManager] {loadMsg}</b></color>");
                EventBus.OnShowUIMessage?.Invoke("📂 Đã tải tiến trình thành công!");
                EventBus.OnPlaySound?.Invoke("equip");
                return true;
            }
            catch (Exception ex)
            {
                string err = $"Lỗi ngoại lệ khi tải game: {ex.Message}";
                Debug.LogError($"<color=red>[SaveManager] {err}\n{ex.StackTrace}</color>");
                OnSaveError?.Invoke(err);
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Xóa tệp lưu game.
        /// </summary>
        public void DeleteSave()
        {
            SaveSecurity.DeleteSecureFiles();
            CurrentSaveData = null;
            EventBus.OnShowUIMessage?.Invoke("Đã xóa dữ liệu lưu game!");
        }

        #region Tiện ích Tra Cứu Asset Linh Hoạt
        /// <summary>
        /// Tìm kiếm ScriptableObject theo Id hoặc Tên trong bộ nhớ hoặc AssetDatabase (nếu trong Editor).
        /// </summary>
        public static T FindAssetByNameOrId<T>(string idOrName) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(idOrName)) return null;

            // 1. Thử Resources.Load
            T res = Resources.Load<T>(idOrName);
            if (res != null) return res;

            // 2. Thử Resources.FindObjectsOfTypeAll
            T[] inMemory = Resources.FindObjectsOfTypeAll<T>();
            foreach (var item in inMemory)
            {
                if (item != null)
                {
                    if (item.name.Equals(idOrName, StringComparison.OrdinalIgnoreCase)) return item;
                    if (item is ItemData it && it.itemId.Equals(idOrName, StringComparison.OrdinalIgnoreCase)) return item;
                }
            }

#if UNITY_EDITOR
            // 3. Tra cứu AssetDatabase trong Editor
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{idOrName} t:{typeof(T).Name}");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }
#endif
            return null;
        }
        #endregion
    }
}
