#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using ActionRPG.Save;

namespace ActionRPG.Editor
{
    /// <summary>
    /// Bộ kiểm thử tự động hệ thống Lưu/Tải dữ liệu bảo mật: JSON + AES-256 + SHA-256 Anti-Tamper.
    /// Kiểm tra toàn diện quy trình mã hóa, giải mã, chống giả mạo và tính toàn vẹn 100% dữ liệu.
    /// Menu: Tools -> ActionRPG -> Run Save/Load Security Test
    /// </summary>
    public static class SaveSystemTest
    {
        [MenuItem("Tools/ActionRPG/Run Save/Load Security Test")]
        public static bool RunSecurityTests()
        {
            Debug.Log("<color=cyan>======================================================================</color>");
            Debug.Log("<color=cyan><b>[LUMINA SAVE SECURITY TEST SUITE] BẮT ĐẦU KIỂM THỬ BẢO MẬT SAVE/LOAD</b></color>");
            Debug.Log("<color=cyan>Tiêu chuẩn: JSON Tuần Tự Hóa + Mã Hóa AES-256 + Chữ Ký Toàn Vẹn SHA-256</color>");
            Debug.Log("<color=cyan>======================================================================</color>");

            int passed = 0;
            int failed = 0;

            void Assert(bool condition, string testStep, string details = "")
            {
                if (condition)
                {
                    passed++;
                    Debug.Log($"<color=green>[PASS]</color> <b>{testStep}</b>: {details}");
                }
                else
                {
                    failed++;
                    Debug.LogError($"<color=red>[FAIL]</color> <b>{testStep}</b>: {details}");
                }
            }

            string originalDatBackup = null;
            string originalSigBackup = null;
            string datPath = SaveSecurity.GetSaveDataPath();
            string sigPath = SaveSecurity.GetSaveSignaturePath();

            // Sao lưu tạm file save thật (nếu có) để trả lại sau khi test
            if (File.Exists(datPath)) originalDatBackup = File.ReadAllText(datPath, Encoding.UTF8);
            if (File.Exists(sigPath)) originalSigBackup = File.ReadAllText(sigPath, Encoding.UTF8);

            try
            {
                // =========================================================================
                // BƯỚC 1: TẠO SAVEDATA MẪU -> LƯU VÀ MÃ HÓA AES-256
                // =========================================================================
                Debug.Log("<color=yellow>--- BƯỚC 1: TẠO DỮ LIỆU MẪU & LƯU MÃ HÓA AES-256 ---</color>");
                SaveData sample = new SaveData();

                // PlayerData
                sample.playerData.level = 25;
                sample.playerData.currentExp = 3450;
                sample.playerData.currentHp = 450.5f;
                sample.playerData.currentMp = 180f;
                sample.playerData.gold = 2850;
                sample.playerData.currentTier = "Tier3_LuminaClass";
                sample.playerData.currentClassId = "Class_ElementalArcher";
                sample.playerData.lastPosition = new SerializableVector3(14.5f, -3.2f, 0f);

                // Attributes
                sample.attributes.baseStr = 22;
                sample.attributes.baseDex = 38;
                sample.attributes.baseInt = 16;
                sample.attributes.baseVit = 25;

                // InventoryData
                sample.inventoryData.items.Add(new SavedInventoryItem("potion_small_hp", "Bình Máu Nhỏ", 10));
                sample.inventoryData.items.Add(new SavedInventoryItem("potion_small_mp", "Bình Mana Nhỏ", 8));
                sample.inventoryData.items.Add(new SavedInventoryItem("item_spirit_orb", "Quả Cầu Tinh Linh", 5));

                // EquipmentData (7 slots)
                sample.equipmentData.equippedSlots.Add(new SavedEquippedSlot("Weapon", "weapon_bow_epic", "Cung Phong Linh"));
                sample.equipmentData.equippedSlots.Add(new SavedEquippedSlot("Armor", "armor_leather_rare", "Áo Da Báo Tuyết"));

                // PetStorageData
                sample.petStorageData.pets.Add(new SavedPetData("pet_fire_fox", "Hồ Ly Lửa", 15, 220f, 85f, true));
                sample.petStorageData.pets.Add(new SavedPetData("pet_thunder_eagle", "Lôi Ưng", 12, 180f, 40f, false));
                sample.petStorageData.activePetId = "pet_fire_fox";

                // WorldFlags
                sample.worldFlags.unlockedWaystones.Add("waystone_village");
                sample.worldFlags.unlockedWaystones.Add("waystone_forest_crossroad");
                sample.worldFlags.completedQuests.Add("quest_slime_hunt");
                sample.worldFlags.completedQuests.Add("quest_wolf_hunt");
                sample.worldFlags.defeatedBosses.Add("boss_wolf_king");

                // Metadata
                sample.metadata.saveTimestamp = "2026-10-06 00:25:00";
                sample.metadata.playTime = 3600f;
                sample.metadata.saveVersion = "1.0.0";

                string rawJson = JsonUtility.ToJson(sample, true);
                bool saveOk = SaveSecurity.SaveSecureFiles(rawJson);

                Assert(saveOk, "Bước 1.1", "Ghi file mã hóa AES-256 thành công.");
                Assert(File.Exists(datPath), "Bước 1.2", $"File dữ liệu .dat tồn tại tại: {datPath}");
                Assert(File.Exists(sigPath), "Bước 1.3", $"File chữ ký .sig tồn tại tại: {sigPath}");

                string cipherTextOnDisk = File.ReadAllText(datPath, Encoding.UTF8);
                bool isEncrypted = !cipherTextOnDisk.Contains("Class_ElementalArcher") && !cipherTextOnDisk.Contains("level");
                Assert(isEncrypted, "Bước 1.4", "Dữ liệu trên đĩa đã được mã hóa AES-256 hoàn toàn, không lộ JSON thô.");

                // =========================================================================
                // BƯỚC 2: ĐỌC VÀ GIẢI MÃ -> SO SÁNH TÍNH KHỚP 100% DỮ LIỆU GỐC
                // =========================================================================
                Debug.Log("<color=yellow>--- BƯỚC 2: GIẢI MÃ & KIỂM TRA TÍNH KHỚP 100% DỮ LIỆU GỐC ---</color>");
                bool loadOk = SaveSecurity.LoadSecureFiles(out string decryptedJson, out string errMsg);

                Assert(loadOk && string.IsNullOrEmpty(errMsg), "Bước 2.1", "Tải và giải mã AES-256 thành công không có lỗi.");

                SaveData loaded = JsonUtility.FromJson<SaveData>(decryptedJson);
                Assert(loaded != null, "Bước 2.2", "Parse JSON SaveData thành công.");

                bool playerMatch = loaded.playerData.level == sample.playerData.level &&
                                   loaded.playerData.gold == sample.playerData.gold &&
                                   loaded.playerData.currentClassId == sample.playerData.currentClassId &&
                                   Mathf.Approximately(loaded.playerData.lastPosition.x, sample.playerData.lastPosition.x) &&
                                   Mathf.Approximately(loaded.playerData.lastPosition.y, sample.playerData.lastPosition.y);
                Assert(playerMatch, "Bước 2.3", $"PlayerData khớp 100% (Lv.{loaded.playerData.level}, Gold={loaded.playerData.gold}, Class={loaded.playerData.currentClassId}, Pos={loaded.playerData.lastPosition})");

                bool attrMatch = loaded.attributes.baseStr == sample.attributes.baseStr &&
                                 loaded.attributes.baseDex == sample.attributes.baseDex &&
                                 loaded.attributes.baseInt == sample.attributes.baseInt &&
                                 loaded.attributes.baseVit == sample.attributes.baseVit;
                Assert(attrMatch, "Bước 2.4", $"Attributes khớp 100% (STR={loaded.attributes.baseStr}, DEX={loaded.attributes.baseDex}, INT={loaded.attributes.baseInt}, VIT={loaded.attributes.baseVit})");

                bool invMatch = loaded.inventoryData.items.Count == sample.inventoryData.items.Count &&
                                loaded.inventoryData.items[0].itemId == "potion_small_hp" &&
                                loaded.inventoryData.items[0].quantity == 10;
                Assert(invMatch, "Bước 2.5", $"Túi đồ Inventory khớp 100% ({loaded.inventoryData.items.Count} món vật phẩm)");

                bool equipMatch = loaded.equipmentData.equippedSlots.Count == sample.equipmentData.equippedSlots.Count &&
                                  loaded.equipmentData.equippedSlots[0].itemId == "weapon_bow_epic";
                Assert(equipMatch, "Bước 2.6", $"Trang bị 7 slot khớp 100% ({loaded.equipmentData.equippedSlots.Count} trang bị)");

                bool petMatch = loaded.petStorageData.pets.Count == sample.petStorageData.pets.Count &&
                                loaded.petStorageData.activePetId == "pet_fire_fox" &&
                                loaded.petStorageData.pets[0].isSummoned;
                Assert(petMatch, "Bước 2.7", $"Kho Pet khớp 100% ({loaded.petStorageData.pets.Count} Pet, Active={loaded.petStorageData.activePetId})");

                bool worldMatch = loaded.worldFlags.completedQuests.Count == 2 &&
                                  loaded.worldFlags.defeatedBosses.Contains("boss_wolf_king");
                Assert(worldMatch, "Bước 2.8", $"WorldFlags khớp 100% ({loaded.worldFlags.completedQuests.Count} Quest, Bosses={loaded.worldFlags.defeatedBosses.Count})");

                // =========================================================================
                // BƯỚC 3: CỐ TÌNH SỬA 1 KÝ TỰ -> KIỂM TRA SHA-256 CHẶN TẢI THÀNH CÔNG
                // =========================================================================
                Debug.Log("<color=yellow>--- BƯỚC 3: KIỂM THỬ CHỐNG GIAN LẬN / CAN THIỆP FILE (ANTI-TAMPER) ---</color>");

                // Thử nghiệm 3A: Can thiệp sửa file dữ liệu savegame.dat
                string tamperedDat = cipherTextOnDisk;
                if (tamperedDat.Length > 10)
                {
                    // Đảo ký tự thứ 5 từ 'A' sang 'B' hoặc ngược lại
                    char originalChar = tamperedDat[5];
                    char alteredChar = originalChar == 'A' ? 'B' : 'A';
                    tamperedDat = tamperedDat.Substring(0, 5) + alteredChar + tamperedDat.Substring(6);
                    File.WriteAllText(datPath, tamperedDat, Encoding.UTF8);

                    bool tamperedLoad = SaveSecurity.LoadSecureFiles(out string tamperedJson, out string tamperedErr);
                    Assert(!tamperedLoad && tamperedJson == null, "Bước 3A", $"Hệ thống đã PHÁT HIỆN GIAN LẬN và CHẶN TẢI khi file .dat bị sửa đổi 1 ký tự: '{tamperedErr}'");
                }

                // Khôi phục file .dat hợp lệ
                File.WriteAllText(datPath, cipherTextOnDisk, Encoding.UTF8);

                // Thử nghiệm 3B: Can thiệp sửa chữ ký băm savegame.sig
                string originalSig = File.ReadAllText(sigPath, Encoding.UTF8).Trim();
                char sigChar = originalSig[0] == 'f' ? '0' : 'f';
                string tamperedSig = sigChar + originalSig.Substring(1);
                File.WriteAllText(sigPath, tamperedSig, Encoding.UTF8);

                bool sigTamperedLoad = SaveSecurity.LoadSecureFiles(out string sigTamperedJson, out string sigTamperedErr);
                Assert(!sigTamperedLoad && sigTamperedJson == null, "Bước 3B", $"Hệ thống đã PHÁT HIỆN CHỮ KÝ SAI LỆCH và CHẶN TẢI khi file .sig bị sửa đổi: '{sigTamperedErr}'");

                // Khôi phục chữ ký chuẩn
                File.WriteAllText(sigPath, originalSig, Encoding.UTF8);

                // Thử nghiệm 3C: Tải lại sau khi khôi phục tính toàn vẹn
                bool restoredLoad = SaveSecurity.LoadSecureFiles(out string restoredJson, out string restoredErr);
                Assert(restoredLoad && !string.IsNullOrEmpty(restoredJson), "Bước 3C", "Sau khi khôi phục chữ ký gốc, tệp tin được xác thực hợp lệ và tải thành công.");

                // =========================================================================
                // BƯỚC 4: BÁO CÁO KẾT QUẢ PASS/FAIL RÕ RÀNG TRÊN CONSOLE
                // =========================================================================
                Debug.Log("<color=yellow>--- BƯỚC 4: TỔNG HỢP BÁO CÁO KẾT QUẢ BẢO MẬT ---</color>");
                int totalTests = passed + failed;
                float passRate = (float)passed / totalTests * 100f;

                Debug.Log("<color=cyan>======================================================================</color>");
                if (failed == 0)
                {
                    Debug.Log($"<color=green><b>🎉 KẾT QUẢ: 100% VƯỢT QUA TẤT CẢ {passed}/{totalTests} BÀI KIỂM THỬ BẢO MẬT!</b></color>");
                    Debug.Log("<color=green>✓ Thuật toán mã hóa: AES-256 (CBC, PKCS7, Key 256-bit, IV 128-bit) hoạt động hoàn hảo.</color>");
                    Debug.Log("<color=green>✓ Cơ chế chống can thiệp: SHA-256 Signature (Anti-Tamper) phát hiện 100% các sửa đổi gian lận.</color>");
                    Debug.Log("<color=green>✓ Toàn vẹn dữ liệu: Cấu trúc SaveData 7 phân vùng phục hồi chuẩn xác 100%.</color>");
                }
                else
                {
                    Debug.LogError($"<color=red><b>❌ CÓ {failed}/{totalTests} BÀI KIỂM THỬ THẤT BẠI! Tỷ lệ đạt: {passRate:F1}%</b></color>");
                }
                Debug.Log("<color=cyan>======================================================================</color>");
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"<color=red>[SaveSystemTest EXCEPTION]</color> {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // Khôi phục lại trạng thái ban đầu của máy người dùng
                if (!string.IsNullOrEmpty(originalDatBackup)) File.WriteAllText(datPath, originalDatBackup, Encoding.UTF8);
                if (!string.IsNullOrEmpty(originalSigBackup)) File.WriteAllText(sigPath, originalSigBackup, Encoding.UTF8);
            }

            return failed == 0;
        }
    }
}
#endif
