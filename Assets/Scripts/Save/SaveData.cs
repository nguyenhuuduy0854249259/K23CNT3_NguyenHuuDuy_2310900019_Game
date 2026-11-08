using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionRPG.Save
{
    /// <summary>
    /// Vector3 hỗ trợ tuần tự hóa JSON chuẩn Unity JsonUtility.
    /// </summary>
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializableVector3(Vector3 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public static implicit operator Vector3(SerializableVector3 sv) => sv.ToVector3();
        public static implicit operator SerializableVector3(Vector3 v) => new SerializableVector3(v);

        public override string ToString() => $"({x:F2}, {y:F2}, {z:F2})";
    }

    /// <summary>
    /// Dữ liệu Nhân Vật Người Chơi (Cấp độ, máu, mana, exp, vàng, tier, class, tọa độ).
    /// </summary>
    [Serializable]
    public class PlayerSaveSection
    {
        public int level = 1;
        public int currentExp = 0;
        public float currentHp = 100f;
        public float currentMp = 50f;
        public int gold = 150;
        public string currentTier = "Tier1_Novice";
        public string currentClassId = "Class_Novice";
        public SerializableVector3 lastPosition = new SerializableVector3(0f, 0f, 0f);
    }

    /// <summary>
    /// Thuộc tính RPG gốc của nhân vật (STR, DEX, INT, VIT).
    /// </summary>
    [Serializable]
    public class AttributesSaveSection
    {
        public int baseStr = 10;
        public int baseDex = 10;
        public int baseInt = 10;
        public int baseVit = 10;
    }

    /// <summary>
    /// Thực thể vật phẩm trong túi đồ khi lưu.
    /// </summary>
    [Serializable]
    public class SavedInventoryItem
    {
        public string itemId = "";
        public string itemName = "";
        public int quantity = 1;
        public float durability = 100f;

        public SavedInventoryItem() { }

        public SavedInventoryItem(string id, string name, int qty, float dur = 100f)
        {
            itemId = id;
            itemName = name;
            quantity = qty;
            durability = dur;
        }
    }

    /// <summary>
    /// Dữ liệu túi đồ.
    /// </summary>
    [Serializable]
    public class InventorySaveSection
    {
        public List<SavedInventoryItem> items = new List<SavedInventoryItem>();
    }

    /// <summary>
    /// Trang bị trong 7 slot (Weapon, Helmet, Armor, Gloves, Boots, Ring, Necklace).
    /// </summary>
    [Serializable]
    public class SavedEquippedSlot
    {
        public string slotName = "";
        public string itemId = "";
        public string itemName = "";

        public SavedEquippedSlot() { }

        public SavedEquippedSlot(string slot, string id, string name)
        {
            slotName = slot;
            itemId = id;
            itemName = name;
        }
    }

    /// <summary>
    /// Dữ liệu 7 slot trang bị.
    /// </summary>
    [Serializable]
    public class EquipmentSaveSection
    {
        public List<SavedEquippedSlot> equippedSlots = new List<SavedEquippedSlot>();
    }

    /// <summary>
    /// Dữ liệu một Thú Cưng đã thu phục.
    /// </summary>
    [Serializable]
    public class SavedPetData
    {
        public string petId = "";
        public string customName = "";
        public int level = 1;
        public float currentHp = 80f;
        public float bondLevel = 50f;
        public bool isSummoned = false;

        public SavedPetData() { }

        public SavedPetData(string id, string name, int lvl, float hp, float bond, bool summoned)
        {
            petId = id;
            customName = name;
            level = lvl;
            currentHp = hp;
            bondLevel = bond;
            isSummoned = summoned;
        }
    }

    /// <summary>
    /// Kho Thú Cưng đồng hành.
    /// </summary>
    [Serializable]
    public class PetStorageSaveSection
    {
        public List<SavedPetData> pets = new List<SavedPetData>();
        public string activePetId = "";
    }

    /// <summary>
    /// Cờ trạng thái thế giới (Waystone, Quest, Boss).
    /// </summary>
    [Serializable]
    public class WorldFlagsSaveSection
    {
        public List<string> unlockedWaystones = new List<string>();
        public List<string> completedQuests = new List<string>();
        public List<string> defeatedBosses = new List<string>();
    }

    /// <summary>
    /// Siêu dữ liệu phiên lưu game.
    /// </summary>
    [Serializable]
    public class MetadataSaveSection
    {
        public string saveTimestamp = "";
        public float playTime = 0f;
        public string saveVersion = "1.0.0";
    }

    /// <summary>
    /// Cấu trúc dữ liệu Lưu/Tải toàn diện dự án Lumina RPG.
    /// Chuẩn hóa theo tiêu chí đồ án tốt nghiệp, hỗ trợ tuần tự hóa JsonUtility và mã hóa bảo mật.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public PlayerSaveSection playerData = new PlayerSaveSection();
        public AttributesSaveSection attributes = new AttributesSaveSection();
        public InventorySaveSection inventoryData = new InventorySaveSection();
        public EquipmentSaveSection equipmentData = new EquipmentSaveSection();
        public PetStorageSaveSection petStorageData = new PetStorageSaveSection();
        public WorldFlagsSaveSection worldFlags = new WorldFlagsSaveSection();
        public MetadataSaveSection metadata = new MetadataSaveSection();

        public SaveData Clone()
        {
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}
