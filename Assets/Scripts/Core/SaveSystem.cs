using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ActionRPG.Core
{
    [Serializable]
    public class PlayerSaveData
    {
        public string playerId = "player_001";
        public int level = 1;
        public int currentExp = 0;
        public int gold = 100;
        public string currentClassName = "Tân Thủ (Novice)";
        public float hunger = 100f;
        public float currentHp = 100f;
        public float currentMp = 50f;
        public string activePetId = "";
        public string[] inventoryItemIds = new string[0];
        public int[] inventoryItemCounts = new int[0];
    }


    [Serializable]
    public class EncryptedSaveContainer
    {
        public string encryptedData;
        public string sha256Checksum;
    }

    public static class SaveSystem
    {
        private static readonly string SaveFileName = "savegame.dat";
        private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("GamePetRPGKeyForAES256_32Byte!!!"); // 32 bytes = 256 bit
        private static readonly byte[] AesIV  = Encoding.UTF8.GetBytes("InitVector128Bit"); // 16 bytes

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        public static bool Save(PlayerSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                string checksum = ComputeSHA256(json);
                string encrypted = EncryptAES(json);

                EncryptedSaveContainer container = new EncryptedSaveContainer
                {
                    encryptedData = encrypted,
                    sha256Checksum = checksum
                };

                string containerJson = JsonUtility.ToJson(container);
                File.WriteAllText(GetSavePath(), containerJson);
                Debug.Log($"[SaveSystem] Game saved successfully with AES-256 & SHA256! Path: {GetSavePath()}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save game: {ex.Message}");
                return false;
            }
        }

        public static PlayerSaveData Load()
        {
            string path = GetSavePath();
            if (!File.Exists(path))
            {
                Debug.Log("[SaveSystem] No save file found, creating new profile.");
                return new PlayerSaveData();
            }

            try
            {
                string containerJson = File.ReadAllText(path);
                EncryptedSaveContainer container = JsonUtility.FromJson<EncryptedSaveContainer>(containerJson);

                string decryptedJson = DecryptAES(container.encryptedData);
                string computedChecksum = ComputeSHA256(decryptedJson);

                if (computedChecksum != container.sha256Checksum)
                {
                    Debug.LogWarning("[SaveSystem] CHECKSUM MISMATCH! Save file tampering detected. Rollbacking to default!");
                    return new PlayerSaveData();
                }

                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(decryptedJson);
                Debug.Log("[SaveSystem] Save file verified and loaded successfully!");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Corrupted save file, reset to default: {ex.Message}");
                return new PlayerSaveData();
            }
        }

        private static string EncryptAES(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AesKey;
                aes.IV = AesIV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static string DecryptAES(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = AesKey;
                aes.IV = AesIV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static string ComputeSHA256(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
