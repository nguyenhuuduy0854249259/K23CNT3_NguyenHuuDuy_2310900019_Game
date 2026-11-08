using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ActionRPG.Save
{
    /// <summary>
    /// Module mã hóa AES-256 và bảo vệ toàn vẹn dữ liệu chống gian lận SHA-256 (Anti-Tamper).
    /// Chuẩn tiêu chí bảo mật cấp cao cho đồ án tốt nghiệp Action RPG.
    /// </summary>
    public static class SaveSecurity
    {
        public const string SaveDataFileName = "savegame.dat";
        public const string SaveSignatureFileName = "savegame.sig";

        // Khóa mã hóa 256-bit (32 bytes) và IV 128-bit (16 bytes) chuẩn mực
        public static readonly string DefaultKey = "LuminaRPG_Security_Key_2026_AES256!";
        public static readonly string DefaultIV  = "Lumina_IV_16Byte!";

        public static string GetSaveDataPath() => Path.Combine(Application.persistentDataPath, SaveDataFileName);
        public static string GetSaveSignaturePath() => Path.Combine(Application.persistentDataPath, SaveSignatureFileName);

        /// <summary>
        /// Chuẩn hóa chuỗi khóa/IV về độ dài byte chính xác (32 bytes cho Key, 16 bytes cho IV).
        /// </summary>
        public static byte[] NormalizeKey(string key, int targetLength)
        {
            if (string.IsNullOrEmpty(key)) key = targetLength == 32 ? DefaultKey : DefaultIV;
            byte[] raw = Encoding.UTF8.GetBytes(key);
            if (raw.Length == targetLength) return raw;

            byte[] result = new byte[targetLength];
            for (int i = 0; i < targetLength; i++)
            {
                result[i] = (i < raw.Length) ? raw[i] : (byte)0x5A;
            }
            return result;
        }

        #region 1. Mã hóa & Giải mã AES-256 (Rijndael)
        /// <summary>
        /// Mã hóa chuỗi văn bản (JSON) sang chuỗi mã hóa Base64 bằng thuật toán AES-256 (CBC, PKCS7).
        /// </summary>
        public static string EncryptString(string plainText, string key = null, string iv = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            byte[] keyBytes = NormalizeKey(key ?? DefaultKey, 32);
            byte[] ivBytes = NormalizeKey(iv ?? DefaultIV, 16);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Giải mã chuỗi Base64 trở về chuỗi văn bản gốc (JSON) bằng thuật toán AES-256.
        /// </summary>
        public static string DecryptString(string cipherText, string key = null, string iv = null)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] keyBytes = NormalizeKey(key ?? DefaultKey, 32);
            byte[] ivBytes = NormalizeKey(iv ?? DefaultIV, 16);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        #endregion

        #region 2. Băm Chữ Ký SHA-256 (Anti-Tamper & Data Integrity)
        /// <summary>
        /// Tạo chuỗi băm chữ ký SHA-256 (Hex) từ chuỗi JSON thô phục vụ kiểm định toàn vẹn.
        /// </summary>
        public static string ComputeHash(string data)
        {
            if (data == null) data = string.Empty;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                StringBuilder sb = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Kiểm tra tính khớp của chuỗi dữ liệu với mã băm SHA-256 kỳ vọng.
        /// </summary>
        public static bool VerifyHash(string data, string expectedHash)
        {
            if (string.IsNullOrEmpty(expectedHash)) return false;
            string actualHash = ComputeHash(data);
            return string.Equals(actualHash, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region 3. Lưu Trữ Tệp Tin Bảo Mật Trên Đĩa
        /// <summary>
        /// Lưu file savegame.dat (dữ liệu mã hóa AES-256) và file savegame.sig (chữ ký SHA-256)
        /// </summary>
        public static bool SaveSecureFiles(string plainJson, string key = null, string iv = null)
        {
            try
            {
                string hash = ComputeHash(plainJson);
                string encrypted = EncryptString(plainJson, key, iv);

                string datPath = GetSaveDataPath();
                string sigPath = GetSaveSignaturePath();

                string dir = Path.GetDirectoryName(datPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(datPath, encrypted, Encoding.UTF8);
                File.WriteAllText(sigPath, hash, Encoding.UTF8);

                Debug.Log($"<color=cyan>[SaveSecurity] Đã lưu an toàn savegame.dat (AES-256) & savegame.sig (SHA-256) tại: {Application.persistentDataPath}</color>");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>[SaveSecurity] Lỗi lưu tệp: {ex.Message}</color>");
                return false;
            }
        }

        /// <summary>
        /// Đọc và xác thực tệp tin lưu:
        /// 1. Đọc cipherText và chữ ký SHA-256 từ đĩa.
        /// 2. Giải mã AES-256.
        /// 3. Tính toán lại mã băm SHA-256 từ chuỗi JSON đã giải mã.
        /// 4. So sánh với file .sig. Nếu không khớp -> Báo lỗi "Dữ liệu lưu đã bị can thiệp!".
        /// </summary>
        public static bool LoadSecureFiles(out string plainJson, out string errorMessage, string key = null, string iv = null)
        {
            plainJson = null;
            errorMessage = null;

            string datPath = GetSaveDataPath();
            string sigPath = GetSaveSignaturePath();

            if (!File.Exists(datPath) || !File.Exists(sigPath))
            {
                errorMessage = "Không tìm thấy tệp lưu savegame.dat hoặc savegame.sig.";
                return false;
            }

            try
            {
                string cipherText = File.ReadAllText(datPath, Encoding.UTF8);
                string savedSignature = File.ReadAllText(sigPath, Encoding.UTF8).Trim();

                string decryptedJson = DecryptString(cipherText, key, iv);
                string computedHash = ComputeHash(decryptedJson);

                if (!string.Equals(computedHash, savedSignature, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "Dữ liệu lưu đã bị can thiệp!";
                    Debug.LogWarning($"<color=red><b>[SaveSecurity] CẢNH BÁO TOÀN VẸN: {errorMessage}</b> (SHA-256 tính lại: '{computedHash}' != File .sig: '{savedSignature}')</color>");
                    return false;
                }

                plainJson = decryptedJson;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Dữ liệu lưu đã bị can thiệp!";
                Debug.LogWarning($"<color=red>[SaveSecurity] {errorMessage} (Chi tiết: {ex.Message})</color>");
                return false;
            }
        }

        /// <summary>
        /// Xóa bỏ file lưu cũ (.dat và .sig).
        /// </summary>
        public static void DeleteSecureFiles()
        {
            string datPath = GetSaveDataPath();
            string sigPath = GetSaveSignaturePath();

            if (File.Exists(datPath)) File.Delete(datPath);
            if (File.Exists(sigPath)) File.Delete(sigPath);
            Debug.Log("<color=yellow>[SaveSecurity] Đã xóa dữ liệu savegame.dat và savegame.sig.</color>");
        }
        #endregion
    }
}
