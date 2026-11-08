# NHẬT KÝ LÀM VIỆC DỰ ÁN LUMINA (WORK LOG)
> **Dự án:** Lumina – 2D Action RPG kết hợp Hệ thống Đồng hành Pet  
> **Thời gian cập nhật:** 2026-10-09 03:15 (Local Time)  
> **Tài liệu nguồn tham chiếu:** `D:\gam\class gam\` (13 file tài liệu `.docx`)  

---

## 📌 1. MỤC TIÊU VÀ PHẠM VI CÔNG VIỆC
1. **Khảo sát & Đọc hiểu toàn diện:** Truy cập và trích xuất toàn bộ 13 file tài liệu thiết kế Class game từ thư mục `D:\gam\class gam\`.
2. **Tổng hợp kiến trúc Class:** Phân tích sâu toàn bộ cơ chế cốt lõi, 3 Ấn nội tại ẩn, Skill Pool, tương tác Pet và hệ thống Nguyên Tố của từng Class thuộc 3 phái (Cận Chiến, Pháp Sư, Xạ Thủ).
3. **Thiết lập Knowledge Item (KI):** Tạo hệ thống cơ sở tri thức kiên cố để AI ghi nhớ toàn bộ logic, công thức, thông số cân bằng, tránh phải hỏi lại người dùng trong các phiên sau.
4. **Triển khai Mã nguồn Unity C#:** Nâng cấp và mở rộng các script dữ liệu (`CharacterClassData.cs`, `SpecialMechanic`), các bộ điều khiển cơ chế (`Mechanics/`), tích hợp vào `ClassSystem.cs`.
5. **Duy trì Nhật ký làm việc (Work Log):** Cập nhật tiến độ rõ ràng, minh bạch để người dùng nắm được tiến trình phát triển từng giai đoạn.

---

## 🕒 2. LỊCH TRÌNH THỰC HIỆN & TIẾN ĐỘ CHI TIẾT

### Giai đoạn 1: Quét, Trích Xuất & Phân Tích Tài Liệu (Hoàn thành 100% ✅)
- **19:33 - 19:36:** Quét toàn bộ thư mục `D:\gam\class gam\` phát hiện 13 tài liệu `.docx`.
- **19:35 - 19:36:** Viết script PowerShell trích xuất tự động `word/document.xml` từ các file `.docx` sang text định dạng sạch trong thư mục scratch.
- **19:36 - 19:37:** Đọc hiểu và phân tích trọn vẹn 13 tài liệu:
  1. `cơ chế game.docx`: Khung kiến trúc Character $\rightarrow$ Class $\rightarrow$ Evolution $\rightarrow$ Profession.
  2. `Lumina_He_Thong_Nguyen_To_Khac_Che_Hieu_Ung.docx`: Hệ 8 nguyên tố, vòng khắc chế, hiệu ứng lõi, phản ứng nguyên tố toàn game.
  3. `Lumina_Can_Chien_Tu_Do_Co_Che_Vietnamese.docx`: Pet biến thành 11 loại vũ khí, CD đổi Pet 5s, 3 Ấn nội tại, 11 bộ skill vũ khí.
  4. `Lumina_Phap_Su_Nguyen_To_Co_Che_Vietnamese.docx`: Hàng đợi 2 ô, 64 chiêu thức hợp thành, Thiên Tai sau 3 combo, 3 Ấn nội tại.
  5. `Phap_Su_Trieu_Hoi_Co_Che_Lumina_v2.docx`: Song Hành 2 Pet, HUD 6 ô chiêu, quản lý HP Pet nguy kịch & đổi dự bị, 3 Ấn nội tại.
  6. `Phap_Su_Thoi_Khong_Co_Che_Lumina_v3_CLEAN.docx`: Thao túng Slow, Stasis, Reverse, 10 kỹ năng, 3 Ấn nội tại.
  7. `Muc_Su_Thanh_Ma_Co_Che_Lumina_v3_2_Loadout.docx`: Stance switch Angel / Demon, 2 bộ loadout 5 ô riêng biệt, 10 cặp kỹ năng.
  8. `Luyen_Duoc_Su_Chum_Ma_Phap_Draft_03_final.docx`: Đặt Chum, 3 cấp chất lượng 60/30/10, delay, hoàn nguyên 5/10/15%, 3 Ấn nội tại.
  9. `Lumina_Xa_Thu_Sung_Co_Che_Vietnamese.docx`: Súng nhiệt lượng, Overheat, Cơ Khí Sư (3 trụ) và Sát Thủ Bóng Đêm.
  10. `LUMINA_Sat_Thu_Bong_Dem_Shadow_Gunner.docx`: Tàng hình 5-10s, Ambush 200-300%, Backstab +35% dmg, Pet Decoy.
  11. `Lumina_Xa_Thu_Nguyen_To_Che_Vietnamese.docx`: Tốc bắn cố định 1.0/s, đổi AtkSpd $\rightarrow$ Sát thương Nguyên tố, 4 mũi tên, 3 Ấn nội tại.
  12. `LUMINA_Xa_Thu_Tinh_Linh.docx`: 3 Charge Linh Thể, tự đuổi 8-10s, Hợp thể Fusion x3, kế thừa hệ Pet, 3 Ấn nội tại.
  13. `TỔNG KẾT VÀ GỢI Ý.docx`: Bảng đối chiếu cân bằng và gợi ý mở rộng.

---

### Giai đoạn 2: Thiết Lập Cơ Sở Tri Thức (KI) (Hoàn thành 100% ✅)
- **19:41:** Tạo KI metadata tại `C:\Users\ASUS\.gemini\antigravity-ide\knowledge\lumina_classes_system\metadata.json`.
- **19:41:** Tạo tài liệu đặc tả KI đầy đủ tại `C:\Users\ASUS\.gemini\antigravity-ide\knowledge\lumina_classes_system\artifacts\lumina_full_classes_and_mechanics_spec.md`.
- **19:42:** Tạo tài liệu tra cứu nội bộ dự án tại `D:\Unity\prj1\gam\Assets\Docs\LUMINA_CLASSES_KNOWLEDGE_BASE.md`.
- **Nội dung lưu trữ:**
  - Bảng ma trận 8 nguyên tố, tương khắc $\times 2$, hiệu ứng lõi.
  - Chi tiết 10 class / hướng phát triển.
  - Bảng danh sách toàn bộ 3 Ấn Nội Tại (Passive Seals) của tất cả các class.
  - Thông số cân bằng, công thức tính toán và ràng buộc kiến trúc.

---

### Giai đoạn 3: Triển Khai Kiến Trúc Code Unity C# (Hoàn thành 100% ✅)
- **19:42 - 19:45:** Đã hoàn thành toàn bộ hệ thống code C# chuyên biệt tại `Assets/Scripts/Classes/`:
  1. `CharacterClassData.cs`: Đã mở rộng toàn bộ enum `SpecialMechanic` cho 10 class và thêm các trường lưu trữ 3 Ấn Nội Tại (Passive Seals).
  2. `Assets/Scripts/Classes/Mechanics/PetWeaponMechanics.cs`: Quản lý 11 loại vũ khí theo 4 nhóm loài Pet, đổi vũ khí CD 5s, buff Cường Lực Hoán Vũ (+25% dmg 3s), phủ ngấn nguyên tố, 3 chiêu vũ khí theo slot 3, 4, 5.
  3. `Assets/Scripts/Classes/Mechanics/ElementalQueueMechanics.cs`: Quản lý hàng đợi 2 ô, 64 công thức chiêu thức (8 đơn hệ + 56 đa hệ), bộ đếm 3 combo kích hoạt Thiên Tai AoE, mồi hạt nguyên tố từ Pet.
  4. `Assets/Scripts/Classes/Mechanics/SummonerDualPetMechanics.cs`: Quản lý Song Hành 2 Pet active, ánh xạ 6 kỹ năng lên HUD, theo dõi HP Pet, trạng thái Nguy Kịch (Downed) và chuyển đổi Pet dự bị, tính thời gian dưỡng thương 48s.
  5. `Assets/Scripts/Classes/Mechanics/ChronoMageMechanics.cs`: Quản lý trạng thái Slow, Stasis (nén sát thương và xả nổ +15% từ Ấn 1), Reverse (lịch sử vị trí 3s) và buff Lệch Pha Không Gian (+20% MoveSpeed, hoàn 15% mana).
  6. `Assets/Scripts/Classes/Mechanics/HolyDarkPriestMechanics.cs`: Quản lý Stance Toggle giữa Angel Form (Hồi máu, Bảo vệ) và Demon Form (Nguyền rủa, Hút máu), hoán đổi tức thì 2 bộ loadout 5 ô kỹ năng.
  7. `Assets/Scripts/Classes/Mechanics/AlchemistCauldronMechanics.cs`: Quản lý đặt Chum, xếp hàng nguyên liệu, tính toán chất lượng 1/2/3 điểm theo tỷ lệ 60/30/10, delay ủ, hoàn nguyên 5%/10%/15%, chống ngắt quãng hoàn 100% nguyên liệu.
  8. `Assets/Scripts/Classes/Mechanics/EngineerGunnerMechanics.cs`: Quản lý Nhiệt Lượng (Heat), vùng nhiệt cao +20% dmg, Overheat lockout 3.5s bung van đẩy lùi 3m, triển khai tối đa 3 trụ (Gatling, Laser, EMP), Pet bảo trì di động.
  9. `Assets/Scripts/Classes/Mechanics/ShadowGunnerMechanics.cs`: Quản lý Tàng hình (Stealth 5-10s), Đòn mở đầu Ambush (x250% crit), Backstab 180° (+35% dmg), Pet tạo phân thân Decoy khiêu khích 6m.
  10. `Assets/Scripts/Classes/Mechanics/ElementalArcherMechanics.cs`: Khóa tốc bắn 1.0 hit/s, chuyển đổi 1% AtkSpd -> 1.5% Sát thương Nguyên tố, 4 loại mũi tên (Hỏa, Thủy, Phong, Thổ), bắn 3 phát cùng hệ phát 4 chắc chắn bạo kích.
  11. `Assets/Scripts/Classes/Mechanics/SpiritArcherMechanics.cs`: Quản lý 3 Charge Linh Thể (hồi 4s/charge), bắn Linh Thể thường tự tìm quái (8-10s), giữ nút Hợp Thể Fusion (x3 sát thương, buff Thân Linh Hợp Nhất +20% MoveSpeed 5s), thu hồi Linh Thể hoàn Charge.
  12. `Assets/Scripts/Characters/ClassSystem.cs`: Đã kết nối và điều phối trọn vẹn cả 10 module cơ chế trên vào nhân vật người chơi (`Player`), cung cấp API đồng nhất cho HUD và Controller.

---

### Giai đoạn 4: Triển Khai Toàn Diện Hệ Thống UI (Hoàn thành 100% ✅)
- **21:08 - 21:38:**
  1. `GameplayHUD.cs`: Quản lý toàn bộ HUD góc trên trái (Avatar, Level, HP, MP, EXP), góc trên phải (Nhiệm vụ), góc dưới phải (8-10 ô Skill Slot có Cooldown, Phím tắt, Mana cost), góc dưới trái (Cụm phím Menu, Nhân vật, Túi đồ, Trang bị, Kỹ năng), và tâm màn hình (Minimap, Crosshair).
  2. `CharacterUI.cs`: Quản lý toàn bộ Character Panel trung tâm phong cách Fantasy RPG cao cấp với 5 tab: Character, Stats, Equipment, Skills, Talent. Phím tắt mở nhanh `[U]`, nút Close `[X]`, switch tab mượt mà.
  3. `CharacterStatsUI.cs`: Hiển thị chi tiết HP, MP, EXP, cấp độ, thuộc tính RPG gốc (STR, DEX, INT, VIT), chỉ số chiến đấu (ATK, DEF, Crit Rate, Crit Damage, Atk Speed, Move Speed, Hệ nguyên tố).
  4. `EquipmentUI.cs`: Quản lý hiển thị và tương tác tháo/mặc 7 slot trang bị: Weapon, Helmet, Armor, Gloves, Boots, Ring, Necklace. Cập nhật chỉ số nhân vật lập tức khi thay đổi.
  5. `SkillsUI.cs`: Hiển thị danh sách kỹ năng mở khóa của Class hiện tại, tiêu hao Mana, Cooldown, tầm đánh, loại chiêu thức (Projectile, AoE, Melee, Buff).
  6. `TalentUI.cs`: Hiển thị 3 Ấn Nội Tại Cốt Lõi (Passive Seals) theo đúng đặc tả của Class từ cơ sở tri thức Lumina.
  7. `GenerateUIAssets.cs`: Tool Editor `Tools -> Generate UI Layout` tự động dựng Canvas chuẩn tỉ lệ 1920x1080 (Scaler, EventSystem, Anchors, Layout Groups, TextMeshPro, Glow/Outline).

---

### Giai đoạn 5: Kiểm Tra Tính Đồng Nhất Logic Gameplay, EventBus & Hệ Thống Phím Tắt (Hoàn thành 100% ✅)
- **Mục tiêu & Phạm vi**: Đảm bảo toàn bộ luồng logic giữa CharacterStats, Inventory, ClassSystem (10 sub-mechanics), Skill System, EventBus và UI Data Binding hoạt động đồng bộ, không phát sinh lỗi ngoại lệ runtime hoặc xung đột dữ liệu.
- **Xây dựng Bộ Kiểm Thử Tự Động (`GameplayLogicConsistencyTest.cs`)**:
  + Tích hợp menu `Tools -> Run Gameplay Logic Consistency Test` với 5 bài kiểm tra toàn diện:
    * `Test 1: Character Stats & RPG Attributes`: Kiểm tra khởi tạo STR, DEX, INT, VIT (10 điểm), các chỉ số ATK, DEF, MoveSpeed, CritRate, cơ chế tiêu hao MP (`ConsumeMp`) và hồi phục HP/MP chuẩn xác.
    * `Test 2: Inventory & Equipment Integration`: Kiểm tra thêm trang bị vào túi, trang bị slot Weapon/Armor, cập nhật chỉ số thuộc tính thời gian thực (`TotalAtk`, `TotalDef`, `TotalCritRate`), tháo trang bị hoàn trả chỉ số gốc.
    * `Test 3: ClassSystem & 10 Chuyên Phái Cơ Chế`: Khởi tạo đầy đủ 10 sub-mechanics độc quyền, kiểm tra Pet biến vũ khí GreatHammer (kích hoạt Ấn Binh Khí Đồng Thể), Hàng đợi nguyên tố 2 ô đầy đủ, chuyển đổi Stance Thánh/Ma, tích nhiệt súng, tiêu hao Charge Linh Thể.
    * `Test 4: Skill System & Combat Logic`: Gán class Kiếm Sĩ Nguyên Tố, unlock kỹ năng Đoạn Phong Trảm, tính toán tiêu hao mana, cooldown, damage multiplier.
    * `Test 5: UI Data Binding, EventBus & Event Propagation`: Khởi tạo và kiểm tra chu trình Open/Close/RefreshAll của `CharacterUI`, `StatsUI`, `EquipmentUI`, `SkillsUI`, `TalentUI` mà không gặp `NullReferenceException`.
  + Kết quả kiểm thử: **Vượt qua 100% (Pass toàn bộ các bài test)**.
- **Chuẩn Hóa EventBus & Quy Chuẩn Phím Tắt Toàn Game**:
  + Kiến trúc sự kiện trung tâm thông qua `EventBus.cs` (`OnPlayerHpChanged`, `OnPlayerMpChanged`, `OnPlayerExpChanged`, `OnSkillTriggered`, `OnShowUIMessage`, `OnPlaySound`, `OnPetSummonChanged`...).
  + Thống nhất sơ đồ phím điều khiển: `[WASD]` di chuyển, `[Space]` Dash lướt né đòn, `[Left Click]` Đánh thường / Tụ lực gồng đòn, `[Right Click]` Chặn đòn (Active Block), `[1] - [5], [Q]` Kỹ năng & Tuyệt kỹ, `[E]` Dùng bình máu / Tương tác NPC, `[R]` Dùng bình mana, `[T]` Triệu hồi / Thu hồi Pet, `[U]` Bảng nhân vật & Trang bị, `[I]` Túi đồ, `[K] / [F1]` Bảng chọn Class, `[Escape]` Menu tạm dừng.

---

### Giai đoạn 6: SPRINT 1 - TRIỂN KHAI HỆ THỐNG ĐỒNG HÀNH PET (Hoàn thành 100% ✅)
- **Cấu trúc Dữ liệu Pet (Task 1.1):**
  1. `PetData.cs` (ScriptableObject): Bổ sung `speciesName`, enum `PetType` (Beast, Elemental, Golem, Spirit, Crustacean, Liquid, Reptile, Flying), `PetBodyShape` (tương thích Cận Chiến Tự Do), thức ăn ưa thích `favoriteFood`, kỹ năng `activeSkill`, cây tiến hóa `evolutionTarget`, alias `baseHp/baseAtk/baseDef`.
  2. `PetInstance.cs`: Lưu trạng thái thực tế khi chơi (cấp độ `level`, `currentExp`, `currentHp`, điểm thân mật `bondLevel: 0 - 100`, trạng thái no đói `fullness`, `isSummoned`). Hỗ trợ tính toán chỉ số theo Bond/Level, cơ chế cho ăn `Feed()` với món ưa thích tăng gấp đôi Bond, và tiến hóa `Evolve()`.
- **Bộ điều khiển Di chuyển Pet (Task 1.2):**
  1. `PetController.cs`: Quản lý thực thể Pet trong Scene, hiệu ứng mờ dần / ánh sáng ma thuật khi triệu hồi/thu hồi, nhận sát thương kèm nhấp nháy đỏ, API `HideForWeaponTransform()` và `RestoreFromWeaponTransform()` cho Cận Chiến Tự Do, API `SpawnDecoy()` cho Sát Thủ Bóng Đêm.
  2. `PetAI.cs`: Cải tiến FSM bám theo Player trong cự ly an toàn **1.5m – 2.5m**, tích hợp thuật toán **Raycast 2D Steering Avoidance** (bắn chùm 5 tia góc rộng) tránh kẹt tường và vật cản, tự động **Teleport** khi cự ly > 12m, trạng thái **Idle Sniffing/Breathing** nhún nhảy nhẹ nhàng khi Player đứng yên.
  3. `PlayerPetRoster.cs`: Quản lý kho Pet của người chơi, tự động cấp và triệu hồi Pet Starter (Hồ Ly Lửa) ngay khi vào Scene `MainGame.unity`, phím tắt `[T]` đóng/mở triệu hồi Pet.
- **Tương tác Combat & Đồng bộ Class (Task 1.3):**
  1. Pet tự động quét và hỗ trợ tấn công quái Player đang đánh (`lastTarget`).
  2. Tích hợp chặt chẽ với:
     - *Cận chiến tự do:* Pet ẩn mô hình và nhập thể thành vũ khí trên tay Player.
     - *Pháp sư triệu hồi:* Hỗ trợ triệu hồi đồng thời 2 PetController ra trận (`SummonerDualPetMechanics.SummonBothPets()`).
     - *Pháp sư nguyên tố:* 25% tỷ lệ đòn đánh Pet tự nạp hạt nguyên tố vào Hàng Đợi (`PushElementalQueue`).
     - *Sát thủ bóng đêm:* Pet tạo phân thân Decoy khiêu khích quái 4s khi Player tàng hình.
- **Cơ chế Thuần Hóa / Bắt Pet (Task 1.4):**
  1. `PetCaptureSystem.cs`: Hỗ trợ item Quả Cầu Tinh Linh (`item_spirit_orb`) và Giấy Thu Phục. Cung cấp API `TryCaptureEnemy(EnemyStats, float)` và `TryCaptureEnemy(EnemyController, float)`.
  2. Công thức chuẩn: `Tỷ lệ thành công = orbCaptureRate * (1.0f - (currentHp / maxHp)) * 1.5f` khi quái vật tụt xuống <= 30% HP.
  3. Bắt thành công tự động sinh `PetInstance` mới và thêm ngay vào kho `PlayerPetRoster.Instance`. Bắt hụt quái nổi giận (`Enrage()`).
  4. `EnemyStats.cs`: Component chuẩn hóa chỉ số quái vật dùng cho cơ chế bắt thú.
- **Giao diện Pet trên HUD (Task 1.5):**
  1. `PetHUDWidget.cs`: Widget đặt ở góc trên bên trái (cạnh Player) hiển thị Avatar Pet, Phù hiệu và Icon Nguyên Tố (8 màu hệ chuẩn), Thanh máu Pet (Fill + Text), Điểm Thân Mật (Bond 0 - 100), Tên Pet, Cấp độ và Gợi ý phím tắt `[T] Thu Hồi`. Tự động ẩn khi không có Pet, hiện mượt mà khi triệu hồi.
  2. Tích hợp tự động vào `GameplayHUD.cs` và công cụ `GenerateUIAssets.cs` (`Tools -> Generate UI Layout`).
- **Kích hoạt Pet trong Scene MainGame.unity:**
  Đã bật `m_IsActive: 1` cho GameObject `Pet_Starter` (Hồ Ly Lửa) trong `Assets/Scenes/MainGame.unity`, giúp chú Pet tung tăng chạy theo sau nhân vật ngay khi mở Scene.
- **Bộ Kiểm Thử Toàn Diện Sprint 1 (`Sprint1PetConsistencyTest.cs`):**
  Đã tạo menu `Tools -> Run Sprint 1 Pet System Test` kiểm tra toàn bộ 5 bài test: Dữ liệu Pet, Bộ điều khiển & Dẫn đường Raycast, Combat & Class Synergies, Thuần hóa bắt quái, HUD Widget & EventBus.

---

### Giai đoạn 7: SPRINT 2 - HỆ THỐNG KẺ ĐỊCH, TELEGRAPH INDICATOR, DATA QUÁI VẬT & TIẾN HÓA 3 CẤP ĐỘ (Hoàn thành 100% ✅)
- **Tiến trình Nhân vật & Ràng buộc Cấp độ (Task 2.1):**
  1. `CharacterStats.cs`:
     - Bổ sung enum `EvolutionTier` (3 giai đoạn):
       + `Tier1_Novice` (Lv 1 - 9): Giai đoạn Tân Thủ. `isPetUnlocked = false`. Bấm `[T]` hiển thị thông báo "Cần Cấp 10 để mở khóa Đồng Hành!". Chặn trang bị vũ khí chuyên phái (Cung, Gậy phép, Súng).
       + `Tier2_BasicClass` (Lv 10 - 19): Giai đoạn Class Cơ Bản. Tự động mở khóa `isPetUnlocked = true`, phát sự kiện `EventBus.OnPetSystemUnlocked`, mở khóa tính năng bắt Pet và trang bị vũ khí chuyên phái.
       + `Tier3_LuminaClass` (Lv 20+): Giai đoạn Class Thức Tỉnh Lumina Tối Thượng. Mở khóa trang bị bậc cao, săn Boss Thế Giới.
     - Thuộc tính `CurrentTier` tính toán tự động: `currentLevel < 10 ? Tier1 : (currentLevel < 20 ? Tier2 : Tier3)`.
     - Phương thức `CanEquip(EquipmentData, out string reason)` kiểm tra đồng bộ cả `requiredLevel`, `requiredTier`, và `isSpecializedWeapon`.
  2. `EquipmentData.cs`: Bổ sung trường `requiredTier` và cờ `isSpecializedWeapon` (đánh dấu Cung tên, Gậy phép, Súng đại bác).
  3. `Inventory.cs`: Tích hợp logic `CanEquip` trước khi cho phép nhân vật gắn trang bị vào slot.
  4. `PlayerController.cs`: Cung cấp thuộc tính `public bool IsDashing => isDashing;` phục vụ kiểm tra i-frame né đòn.

- **Dữ liệu Quái vật & Hệ thống Khắc chế Sát Thương Lumina x2.0 (Task 2.2):**
  1. `ElementalSynergy.cs`: Chuẩn hóa hệ số tương tác nguyên tố Lumina:
     + Khắc chế nguyên tố (Ưu thế hệ): **x2.0 sát thương**.
     + Bị kháng hoặc cùng hệ: **x0.5 sát thương**.
     + Trung tính: **x1.0 sát thương**.
  2. `EnemyData.cs` (ScriptableObject):
     + Enum `MorphologicalGroup` (9 nhóm hình thái): Beast, Elemental, Golem, Spirit, Crustacean, Reptile, Liquid, Undead, Demon.
     + Chỉ số mở rộng: `enemyLevel`, `baseAtk`, `baseDef`, `isCapturable`, `baseCaptureRate`, `isRanged`, `projectileSpeed`, `isAreaBoss`, `isWorldBoss`.
  3. `EnemyStats.cs`:
     + Tính toán sát thương chuẩn Lumina x2.0 khi bị khắc hệ, trừ giáp `TotalDef * 0.6f`.
     + Hiệu ứng Hit Flash (chớp đỏ 0.1s khi trúng đòn).
     + Lực đẩy lùi Knockback lên Rigidbody2D.
     + Tự động cộng EXP cho người chơi khi tiêu diệt quái (`GainExp()`).
     + Tự động kích hoạt bảng rơi đồ `LootDropSystem` rơi vật phẩm dã ngoại.
     + Tích hợp cơ chế thu phục `TryCaptureEnemy` khi quái có cờ `isCapturable`.

- **Bộ Điều Khiển AI Quái Vật (FSM) & Thanh Máu Trên Đầu (Task 2.3):**
  1. `EnemyController.cs`:
     + FSM 4 trạng thái linh hoạt:
       * `Patrol`: Tuần tra ngẫu nhiên quanh điểm xuất phát bán kính 3m, tự đổi hướng sau mỗi 2 - 4s.
       * `Chase`: Phát hiện và truy đuổi mục tiêu (Player/Pet) trong tầm phát hiện 5 - 7m.
       * `Attack`: Dừng lại tấn công khi cự ly <= 1.5m (Cận chiến) hoặc bắn đạn nguyên tố (Tầm xa), thời gian hồi chiêu 1.5s - 2.0s.
       * `Dead`: Ngừng di chuyển, phát hiệu ứng rơi đồ, tiêu biến sau 1.5s.
     + Hỗ trợ quái tầm xa: Sinh đạn ma pháp nguyên tố (`EnemyProjectile`) bay thẳng về phía mục tiêu kèm hiệu ứng va chạm.
     + Tự động khởi tạo và gắn `EnemyHealthBarWorldSpace` lên đầu quái vật.
  2. `EnemyHealthBarWorldSpace.cs`:
     + Canvas World-Space hiển thị thanh máu trượt mượt mà (smooth lerp).
     + Hiển thị cấp độ `Lv.X`, tên quái vật và Phù hiệu Nguyên Tố chuẩn 8 màu sắc.
     + Cơ chế Anti-flip: Tự động giữ nguyên hướng hiển thị không bị lật ngược chữ khi quái quay trái/phải.

- **Vùng Cảnh Báo Sát Thương (Telegraph Indicator) & Dash i-frame (Task 2.4):**
  1. `TelegraphIndicator.cs`:
     + Hỗ trợ 2 kiểu dáng: Vòng tròn (Circle AoE) và Hình chữ nhật/Tia (Line/Box).
     + Thời gian cảnh báo nạp đầy 0.8s - 1.2s trước khi kích nổ.
     + Hiệu ứng thị giác chân thực: Vùng cảnh báo nạp đầy trực quan tăng dần alpha từ 0.35 lên 0.85, chớp sáng bung nổ khi kích hoạt.
     + Sinh texture Circle/Line runtime tự động, không phụ thuộc sprite ngoài.
     + Tự động đồng bộ hóa `Physics2D.SyncTransforms()` và quét va chạm diện rộng.
     + Cơ chế né đòn bằng Dash i-frame: Bỏ qua sát thương nếu Player đang lướt Dash (`playerCtrl.IsDashing`) hoặc có `isInvulnerable`.

- **Boss Nhiều Giai Đoạn & Thanh Máu Toàn Màn Hình HUD (Task 2.5):**
  1. `BossController.cs`:
     + Thiết kế hoàn chỉnh Boss "Hắc Diệu Ma Viên" (Obsidian Demon Ape) - Cấp 20, Hệ Hỏa (Fire), Nhóm Quỷ (Demon), HP 650, ATK 28, DEF 12, `isAreaBoss = true`, `isWorldBoss = true`.
     + Giai đoạn 1 (Phase 1): Tấn công cận chiến mạnh, đập đất tạo chấn động và ném đá dung nham diện rộng.
     + Chuyển Phase (Khi HP <= 50%): Kích hoạt tiếng gầm chấn động, tạo lực đẩy lùi 4.5m hất văng người chơi và Pet.
     + Giai đoạn 2 (Phase 2 - Cuồng Nộ / Enrage): Cơ thể bốc lửa đỏ rực, tăng 35% tốc độ di chuyển, tăng 25% công kích, thi triển combo đập đất 3 nhịp liên hoàn diện rộng.
  2. `BossHealthBarUI.cs`:
     + Thanh máu toàn màn hình hiển thị trang trọng trên đỉnh HUD khi Boss xuất hiện.
     + Hiển thị đầy đủ Tên Boss, Huy hiệu Boss Khu Vực / Thế Giới, Thanh máu phân đoạn (Segmented Health Bar) và Cấp độ.
- **Bộ Kiểm Thử Toàn Diện Sprint 2 (`Sprint2ConsistencyTest.cs`):**
  - Đã tích hợp menu `Tools -> Run Sprint 2 Enemy & Progression Test` với 7 bài kiểm thử:
    * `Test 1: Tiến Trình Nhân Vật 3 Tier & Ràng Buộc Trang Bị (EvolutionTier & Equipment)`: Kiểm tra tiến trình cấp độ (Tier 1 Novice, Tier 2 Basic, Tier 3 Lumina) và rào cản vũ khí chuyên phái.
    * `Test 2: Dữ Liệu Quái Vật & Sát Thương Tương Khắc Lumina x2.0 (EnemyStats & Elements)`: Kiểm tra dữ liệu quái vật, 9 nhóm hình thái MorphologicalGroup và hệ số tương khắc Lumina x2.0 khi khắc hệ, x0.5 khi cùng hệ/kháng, trừ giáp phòng ngự.
    * `Test 3: Máy Trạng Thái AI Quái & Bắn Tầm Xa (EnemyController FSM & Ranged)`: Kiểm tra FSM 4 trạng thái của quái vật (Patrol, Chase, Attack, Dead), bắn đạn ma pháp và thanh máu World-Space trên đầu.
    * `Test 4: Hệ Thống Cảnh Báo Telegraph & Dash i-frame (TelegraphIndicator)`: Kiểm tra vùng cảnh báo Telegraph Indicator (Circle & Line) và cơ chế né đòn bằng Dash i-frame.
    * `Test 5: Boss Đa Giai Đoạn Hắc Diệu Ma Viên & Transition (BossController)`: Kiểm tra 2 giai đoạn Boss (Phase 1 & Phase 2 Enrage), gầm rú đẩy lùi 4.5m, tăng tốc độ và combo đập đất.
    * `Test 6: Thu Gom Dược Liệu Cho Dược Sư Chum (GatheringNode 30s Respawn)`: Kiểm tra tương tác thu hoạch phím [F] tại GatheringNode và bộ đếm hồi sinh 30 giây.
    * `Test 7: Tự Động Sinh Dữ Liệu 5 Quái Thảo Nguyên & 13 Thú Rừng Rậm (Asset Generator)`: Kiểm tra tính toàn vẹn và hợp lệ của toàn bộ 18 quái vật và Boss Sprint 2.

---

### 📦 SPRINT 3: HỆ THỐNG VẬT PHẨM, LOOT DROPS, RÀNG BUỘC CẤP ĐỘ TRANG BỊ & THU THẬP NGUYÊN LIỆU

- **Ràng Buộc Cấp Độ & Tiến Hóa Trang Bị (Task 3.1):**
  1. `EquipmentData.cs`:
     + Khai báo ràng buộc chuẩn: `public int requiredLevel;` (Cấp độ tối thiểu cần đạt).
     + Khai báo ràng buộc giai đoạn: `public EvolutionTier requiredTier;` (`Tier1_Novice`, `Tier2_BasicClass`, `Tier3_LuminaClass`).
     + Ràng buộc vũ khí chuyên phái (`isSpecializedWeapon = true`) không cho phép Tier 1 (Lv 1-9) sử dụng Cung tinh anh, Gậy phép, Súng đại bác.
  2. `CharacterStats.cs`:
     + Bổ sung property alias `public int currentLevel => level;` và `public void HealHp(float amount)`.
     + `CanEquip(EquipmentData equip, out string reason)`: Kiểm tra điều kiện cấp độ và Tier, trả về thông báo lỗi chuẩn UI:
       * Cấp độ chưa đủ: `"Cần Cấp {requiredLevel} để trang bị vật phẩm này!"`.
       * Giai đoạn chưa đủ: `"Cần đạt {requiredTier} để trang bị vật phẩm này!"`.
  3. `Inventory.cs` & `EquipmentUI.cs`:
     + Trong `Inventory.Equip(EquipmentData equip)`: Kiểm tra nghiêm ngặt `currentLevel < equip.requiredLevel` hoặc `CurrentTier < equip.requiredTier`.
     + Nếu không đạt: Huỷ thao tác trang bị, phát âm thanh báo lỗi (`EventBus.OnPlaySound?.Invoke("error")`) và hiển thị thông báo nổi trên UI qua `EventBus.OnShowUIMessage`.

- **Hệ Thống Vật Phẩm Rơi 2D & Tự Động Hút Magnet (Task 3.2):**
  1. `WorldItem.cs` (`Assets/Scripts/Items/Loot/WorldItem.cs`):
     + Đại diện 2D hoàn chỉnh: `SpriteRenderer` (vật phẩm/tiền vàng) + `CircleCollider2D` (Trigger).
     + Vệt sáng phát quang (Glow Aura) đồng bộ 6 cấp phẩm chất chuẩn ARPG:
       * Common: Trắng (`Color.white`)
       * Uncommon: Xanh Lục (`#32D74B`)
       * Rare: Xanh Lam (`#0A84FF`)
       * Epic: Tím (`#BF5AF2`)
       * Legendary: Cam (`#FF9F0A`)
       * Unique: Đỏ (`#FF453A`)
       * Tiền Vàng: Vàng Óng (`#FFD700`)
     + Hiệu ứng văng nhẹ ban đầu (Pop Arc): Quỹ đạo parabol nảy lên cao trong bán kính ngẫu nhiên 0.4m - 1.0m quanh tâm.
     + Logic Tự Động Hút (Magnet): Khi khoảng cách giữa Player và WorldItem <= 2.5m, vật phẩm kích hoạt lực từ trường, bay lượn gia tốc mượt mà về phía Player (3.5 m/s -> 14.0 m/s).
     + Khi chạm Player (cự ly <= 0.45m hoặc TriggerEnter2D):
       * Vật phẩm: Cộng vào `Inventory.Instance.AddItem(itemData, quantity)`, hiển thị popup UI `Nhặt được {item.itemName} x{quantity}!`.
       * Tiền vàng: Cộng thẳng vào `Inventory.Instance.AddGold(goldAmount)`, hiển thị popup `Nhặt được +{goldAmount} Vàng!`.
       * Phát âm thanh `pickup`, bung hạt VFX `VFX_PickupBurst` và hủy GameObject.
     + Hỗ trợ tạo tức thì: `WorldItem.Spawn(...)` và `WorldItem.SpawnGold(...)`.
  2. `LootDropper.cs` (`Assets/Scripts/Items/Loot/LootDropper.cs`):
     + Gắn vào Quái vật (`EnemyStats`): Đọc danh sách `dropTable` và `goldReward`.
     + Khi quái chết (`Die()`): Tính toán tỷ lệ % xác suất rơi Tiền vàng, Nguyên liệu, Phôi trang bị.
     + Tự động kích hoạt văng `WorldItem` ngẫu nhiên trong bán kính 1.0m quanh xác quái.
     + Tích hợp trực tiếp vào `EnemyStats.Die()` và `EnemyStats.DropLoot()`.

- **Hệ Thống Thu Thập Dã Ngoại & Kênh Tương Tác 1.0s (Task 3.3):**
  1. `GatheringNode.cs` (`Assets/Scripts/Environment/Gathering/GatheringNode.cs`):
     + Đại diện cho Bụi Cây Thảo Mộc (`HerbBush`), Bãi Khoáng Thạch (`OreVein`), và Suối Nước Tinh Khiết (`PureSpring`).
     + Giao diện tương tác World-Space: Tự động hiển thị gợi ý `"Bấm [F] để thu hoạch"` khi Player bước vào cự ly 2.0m.
     + Cơ chế Channeling 1.0s: Khi bấm [F], kích hoạt thanh chạy tiến trình tương tác (0% -> 100% trong đúng 1.0 giây). Nếu Player di chuyển ra xa >2.5m thì tự động hủy kênh.
     + Hoàn tất thu hoạch: Văng ra các nguyên liệu pha chế cho Class Luyện Dược Sư:
       * `mat_herb_fiber`: Bột Thảo Mộc
       * `mat_stone_powder`: Bột Đá Khoáng
       * `mat_pure_water`: Giọt Nước Tinh Khiết
     + Chuyển node sang trạng thái xám mờ (Depleted, độ trong suốt 0.4f), khóa tương tác và kích hoạt bộ đếm tự phục hồi (Respawn) sau 30 giây.
     + Tương thích ngược 100% với phương thức `Harvest()` của Sprint 2 Test Suite.

- **Bình Dược Phẩm & Quick Slots Phím Tắt [E] / [R] (Task 3.4):**
  1. `ConsumableItemData.cs` (`Assets/Scripts/Items/ConsumableItemData.cs`):
     + Kế thừa từ `ItemData` với các trường chỉ số:
       * `healHpAmount`: Lượng máu hồi phục tức thì.
       * `restoreMpAmount`: Lượng năng lượng hồi phục tức thì.
       * `cooldownTime = 8.0f`: Thời gian hồi chiêu độc lập (Mặc định: 8.0 giây).
       * Phân loại: `isHpPotion` (Bình Máu), `isMpPotion` (Bình Mana).
  2. Tích hợp Quick Slots trên `GameplayHUD.cs`:
     + **Phím tắt [E]**: Dùng nhanh Bình Máu từ túi đồ:
       * Tìm bình máu đầu tiên trong `Inventory.Instance.slots`.
       * Hồi máu ngay cho Player (`playerStats.HealHp`).
       * Tiêu hao 1 bình thuốc trong túi đồ.
       * Kích hoạt thời gian hồi chiêu chung cho Bình Máu (`HpPotionCooldownTimer = 8.0s`).
       * Nếu đang trong thời gian hồi chiêu hoặc hết bình máu: Phát âm thanh báo lỗi và hiển thị cảnh báo UI.
     + **Phím tắt [R]**: Dùng nhanh Bình Mana từ túi đồ:
       * Tìm bình mana, hồi phục MP ngay lập tức (`playerStats.RestoreMp`).
       * Tiêu hao 1 bình thuốc trong túi đồ.
       * Kích hoạt thời gian hồi chiêu độc lập cho Bình Mana (`MpPotionCooldownTimer = 8.0s`).
     + Widget Quick Slots trên HUD: Hiển thị icon bình thuốc, số lượng còn lại trong túi (`hpPotionCountText` / `mpPotionCountText`), phím tắt `[E]` / `[R]` và hiệu ứng lớp phủ hồi chiêu tỏa tròn (Radial Cooldown Overlay 8s).

- **Tích Hợp & Bộ Test Tự Động Sprint 3 (Task 3.5):**
  1. `Sprint3AssetGenerator.cs` (Menu `Tools -> Lumina -> Generate Sprint 3 Assets`):
     + Tự động sinh ScriptableObject `potion_small_hp.asset` (Hồi 50 HP, CD 8s) & `potion_small_mp.asset` (Hồi 30 MP, CD 8s).
     + Tự động sinh ScriptableObject 3 nguyên liệu Dược Sư: `mat_herb_fiber.asset`, `mat_stone_powder.asset`, `mat_pure_water.asset`.
     + Tự động sinh ScriptableObject 3 trang bị mẫu: Kiếm Tân Thủ Lv1, Giáp Sắt Rèn Lv10 (Tier 2), Trượng Pháp Lumina Lv20 (Tier 3).
     + Tự động sinh 3 Prefab mẫu chuẩn 2D tại `Assets/Prefabs/`:
       * `Prefab_WorldItem_Standard.prefab`: WorldItem chuẩn 2D với CircleCollider2D trigger, vệt sáng GlowAura phát quang.
       * `Prefab_Gathering_HerbBush.prefab`: Bụi Cây Thảo Mộc mẫu với GatheringNode, UI Prompt [F], thanh channel 1.0s.
       * `Prefab_Gathering_OreVein.prefab`: Bãi Khoáng Thạch mẫu với GatheringNode, UI Prompt [F], thanh channel 1.0s.
  2. `Sprint3ConsistencyTest.cs` (Menu `Tools -> Lumina -> Run Sprint 3 Consistency Test`):
     + Bộ kiểm thử tự động toàn diện gồm 6 bài test đạt tỷ lệ vượt qua **100% (6/6 Pass)**:
       * `Test 1`: Kiểm tra ràng buộc cấp độ & Tier trang bị, huỷ thao tác và thông báo lỗi UI.
       * `Test 2`: Kiểm tra sử dụng bình dược phẩm, hồi máu HP/MP, trừ túi đồ, và khóa hồi chiêu 8s độc lập của phím [E] và [R].
       * `Test 3`: Kiểm tra vật phẩm rơi WorldItem 2D, vệt sáng phẩm chất, lực hút từ trường Magnet <= 2.5m và cộng vào Inventory / Gold.
       * `Test 4`: Kiểm tra bộ rơi đồ LootDropper từ quái vật, xác suất rơi và bán kính văng quanh xác 1.0m.
       * `Test 5`: Kiểm tra điểm thu gom GatheringNode, gợi ý UI [F], tiến trình tương tác 1.0s, văng nguyên liệu Dược Sư và hồi sinh 30s.
       * `Test 6`: Kiểm tra tính toàn vẹn của Asset Generator và 3 Prefab mẫu chuẩn 2D.
  3. Kiểm tra kiểm thử hồi quy: Sprint 2 Consistency Test tiếp tục đạt **100% (7/7 Pass)**, tổng cộng 13/13 bài test toàn hệ thống hoạt động hoàn hảo, biên dịch 0 lỗi (0 compilation errors).

---

## 📊 3. BẢNG TRẠNG THÁI HIỆN TẠI (ĐÃ LÀM ĐẾN ĐÂU)
| Hạng mục | Trạng thái | Ghi chú |
|---|---|---|
| Đọc & Trích xuất 13 tài liệu Word | **Hoàn thành (100%)** | Đã đọc hiểu toàn bộ thông số và cơ chế |
| Tổng hợp cơ chế 10 class nhân vật | **Hoàn thành (100%)** | Xác định rõ cơ chế, 3 ấn nội tại và skill pool |
| Khởi tạo Knowledge Item (KI) | **Hoàn thành (100%)** | Lưu trữ tại Knowledge Base hệ thống & Workspace |
| Tạo Nhật ký làm việc (Work Log) | **Hoàn thành (100%)** | File `WORKLOG.md` theo dõi liên tục |
| Mở rộng `CharacterClassData.cs` | **Hoàn thành (100%)** | Cập nhật đầy đủ enum SpecialMechanic & 3 Passive Seals |
| Xây dựng 10 C# Mechanics Scripts | **Hoàn thành (100%)** | 10 script modular tại `Assets/Scripts/Classes/Mechanics/` |
| Cập nhật `ClassSystem.cs` | **Hoàn thành (100%)** | Tích hợp toàn diện các cơ chế vào Player |
| Xây dựng Gameplay HUD Controller | **Hoàn thành (100%)** | `GameplayHUD.cs` & `SkillSlotUI.cs` |
| Xây dựng Character UI Panel & Tabs | **Hoàn thành (100%)** | `CharacterUI.cs`, `CharacterStatsUI.cs`, `EquipmentUI.cs`, `SkillsUI.cs`, `TalentUI.cs` |
| **SPRINT 1: Dữ liệu PetData & PetInstance** | **Hoàn thành (100%)** | `PetData.cs` (8 hệ, loài, thức ăn) & `PetInstance.cs` (Bond 0-100, đói no) |
| **SPRINT 1: PetController & PetAI Steering** | **Hoàn thành (100%)** | Bám theo 1.5-2.5m, Raycast né tường, Teleport >12m, phím [T], Idle sniffing |
| **SPRINT 1: Tương tác Combat & Đồng bộ Class** | **Hoàn thành (100%)** | Hỗ trợ đánh, Free Melee hóa vũ khí, Summoner 2 Pet, Shadow Decoy, Hàng đợi |
| **SPRINT 1: Cơ chế Thuần Hóa PetCaptureSystem** | **Hoàn thành (100%)** | Quả cầu tinh linh, tỷ lệ theo máu <30%, popup chúc mừng, nạp kho Pet |
| **SPRINT 1: Widget Pet trên Gameplay HUD** | **Hoàn thành (100%)** | `PetHUDWidget.cs` avatar, phù hiệu hệ, thanh máu, Bond 0-100 |
| **SPRINT 1: Pet trong MainGame.unity** | **Hoàn thành (100%)** | Kích hoạt `Pet_Starter` Hồ Ly Lửa chạy ngay sau Player trong Scene |
| **SPRINT 1: Test Suite Tự Động** | **Hoàn thành (100%)** | `Tools -> Run Sprint 1 Pet System Test` (5 nhóm kiểm tra) |
| **SPRINT 2: Phân cấp EvolutionTier & CanEquip** | **Hoàn thành (100%)** | Tier 1 Novice (Lv1-9), Tier 2 Basic (Lv10-19), Tier 3 Lumina (Lv20+), chặn vũ khí chuyên phái |
| **SPRINT 2: Dữ liệu 18 Quái vật & Khắc hệ x2.0** | **Hoàn thành (100%)** | 9 nhóm hình thái, khắc chế Lumina x2.0, kháng x0.5, Hit Flash, Knockback, Loot Drops |
| **SPRINT 2: AI Quái vật FSM & Thanh Máu Trên Đầu** | **Hoàn thành (100%)** | Patrol 3m, Chase 5-7m, Attack (Melee/Ranged), Overhead Health Bar Anti-flip |
| **SPRINT 2: Vùng Cảnh Báo Telegraph & Dash i-frame** | **Hoàn thành (100%)** | Circle & Line Telegraph (0.8-1.2s delay), bỏ qua sát thương khi Player Dash |
| **SPRINT 2: Boss Hắc Diệu Ma Viên & HUD Bar** | **Hoàn thành (100%)** | Phase 1 Melee/Magma Boulder -> Gầm 4.5m -> Phase 2 Cuồng nộ (+30% spd, +25% atk, 3-slam) |
| **SPRINT 2: Điểm Thu Thập Dã Ngoại GatheringNode** | **Hoàn thành (100%)** | Thảo mộc, quặng đồng, nước suối, phím [F] thu hoạch, hồi sinh 30s |
| **SPRINT 2: Generator & Test Suite Tự Động** | **Hoàn thành (100%)** | `Tools -> Lumina -> Generate Sprint 2 Assets` & `Run Sprint 2 Consistency Test` (7/7 Pass) |
| **SPRINT 3: Ràng Buộc Cấp Độ & Tier Trang Bị** | **Hoàn thành (100%)** | `EquipmentData.cs`, `CharacterStats.CanEquip`, `Inventory.Equip`, âm thanh báo lỗi, toast UI |
| **SPRINT 3: WorldItem 2D & Hút Tự Động Magnet** | **Hoàn thành (100%)** | `WorldItem.cs`, vệt sáng phát quang 6 phẩm chất, bay mượt mà <= 2.5m, nhặt đồ và vàng |
| **SPRINT 3: Bộ Rơi Đồ Quái Vật LootDropper** | **Hoàn thành (100%)** | `LootDropper.cs`, tính toán % dropTable, văng đồ hình vòng cung 1.0m quanh xác quái |
| **SPRINT 3: Thu Thập Dã Ngoại & Channeling 1.0s** | **Hoàn thành (100%)** | `GatheringNode.cs`, gợi ý UI [F], thanh channel 1.0s, văng nguyên liệu Dược Sư, hồi sinh 30s |
| **SPRINT 3: Bình Dược Phẩm & Quick Slots [E] / [R]** | **Hoàn thành (100%)** | `ConsumableItemData.cs`, phím [E] (Máu) & [R] (Mana), hồi chiêu 8s độc lập, overlay HUD |
| **SPRINT 3: Generator & Test Suite Tự Động** | **Hoàn thành (100%)** | `Tools -> Lumina -> Generate Sprint 3 Assets` & `Run Sprint 3 Consistency Test` (6/6 Pass, 0 lỗi biên dịch) |
| **SPRINT 4: Menu UI Cao Cấp & Trực Quan** | **Hoàn thành (100%)** | Fantasy RPG Dark Glassmorphism, viền neon nguyên tố, tự động dựng layout runtime không lo missing prefab |
| **SPRINT 4: Giải Quyết Xung Đột Phím Tắt** | **Hoàn thành (100%)** | [X] Stance Thánh/Ma, [E] Ưu tiên NPC hoặc Bơm Máu, [T] Gom quyền điều phối Pet tránh double-toggle |
| **SPRINT 4: Đồng Bộ Skill Bar HUD Theo Class** | **Hoàn thành (100%)** | `UpdateSkillBar` tự đổi icon, mana cost, hotkey 1-5/Q và cooldown khi chuyển class qua EventBus |
| **SPRINT 4: Hệ Thống Class Demo Switcher [F1]** | **Hoàn thành (100%)** | Chuyển đổi 14 Class, bộ lọc nhánh, preview 3 Ấn nội tại, kích hoạt 10 cơ chế chiến đấu độc quyền |
| **SPRINT 4: Thanh Công Cụ Debug & Test Thủ Công** | **Hoàn thành (100%)** | Đổi Lv 1/10/20/30, Full Heal HP/MP, Bơm 1000 Vàng & Thuốc, Spawn Quái Vật Mục Tiêu (Dummy Target) |
| **SPRINT 4: Pause Menu & Bảng Phím Tắt [Escape]** | **Hoàn thành (100%)** | Menu tạm dừng TimeScale=0, Cheat Sheet tổng hợp 15 phím tắt điều khiển, điều hướng toàn bộ UI |
| **SPRINT 4: Lumina Database & Test Suite Tự Động** | **Hoàn thành (100%)** | 14 Class, 42 Ấn, 48 Skill, `ClassDemoSwitcherConsistencyTest` (5/5 Pass), hồi quy Sprint 3 (6/6 Pass) |
| **SPRINT 5: Bộ 5 Kỹ Năng Tân Thủ Cấp 1 & Cooldown** | **Hoàn thành (100%)** | Fan Slash 3s, Flurry Slash 2s (7s CD), Kim Cang Barrier 14s, Quad Dash 10s, Heal 22s |
| **SPRINT 5: Cơ Chế Máu Ảo (Barrier / Temp Shield)** | **Hoàn thành (100%)** | `temporaryShieldHp` trong `CharacterStats.cs`, ưu tiên trừ sát thương trước khi trừ HP gốc |
| **SPRINT 5: Generator Pixel Art HUD & 16-bit Assets** | **Hoàn thành (100%)** | `LuminaHUDAssetGenerator.cs` sinh 5 icon skill, 7 frames 9-slice, 4 menu icons (Point filter, 32 PPU) |
| **SPRINT 5: Tích Hợp Skill Hotbar & Đếm Ngược CD HUD** | **Hoàn thành (100%)** | `GameplayHUD.cs` & `SkillSlotUI.cs` hỗ trợ `cdText` đếm ngược từng giây, overlay tỏa tròn, gán phím 1-5 |
| **SPRINT 5: Test Suite Tự Động Sprint 5** | **Hoàn thành (100%)** | `Sprint5HUDAndNoviceSkillsTest.cs` (5/5 Pass, biên dịch 0 lỗi hoàn hảo) |

---

## 🚀 4. CHI TIẾT SPRINT 4: GIAO DIỆN UI CAO CẤP, ĐỒNG BỘ LOGIC & HỆ THỐNG CLASS DEMO SWITCHER

### 1. Nâng cấp Giao diện Menu & Layout Đồ Họa Cao Cấp (Fantasy RPG Dark Glassmorphism):
- **Ngôn ngữ thiết kế nhất quán**:
  + Tông màu nền tối huyền ảo (`#0E131C` ~ `#121824`, độ trong suốt 90-95%).
  + Đường viền Neon phát quang theo phong cách giả kim thuật và nguyên tố Lumina (`Cyan #4DD0E1`, `Gold #FFD54F`, `Emerald #81C784`).
  + Font chữ chuẩn RPG cổ điển với phân cấp tiêu đề, phụ đề, badge vai trò, và hệ thống đánh giá độ khó `★☆☆☆☆` đến `★★★★★`.
  + Tự động thích ứng đa độ phân giải thông qua CanvasScaler (chuẩn 1920x1080).
- **Cơ chế Runtime Auto-Hierarchy Builder (`EnsureUIHierarchy`)**:
  + Cả 3 hệ thống giao diện lớn: [`ClassDemoSwitcherUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassDemoSwitcherUI.cs), [`PauseMenuUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/PauseMenuUI.cs), [`ClassSelectionUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassSelectionUI.cs) đều được trang bị cơ chế tự động xây dựng cây phân cấp UI hoàn chỉnh khi chạy runtime.
  + Không phụ thuộc vào prefab bị missing hoặc thao tác kéo thả Inspector thủ công; chỉ cần gắn script hoặc bấm phím tắt là toàn bộ panel, scrollview, container, nút bấm, và text hiển thị tức thì.

### 2. Loại bỏ Triệt để Xung đột Phím tắt & Đồng bộ Logic Gameplay:
- **Xung đột Stance Toggle vs Quick Slot [R]**:
  + Chuyển phím đổi Stance Thiên Thần ↔ Ác Quỷ của Mục Sư Thánh - Ma sang **`[X]`**, giải phóng phím **`[R]`** dành riêng cho Dùng Nhanh Bình Mana.
- **Điều phối phím [E] Tương Tác NPC & Bình Máu**:
  + Trong [`PlayerController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/PlayerController.cs): Khi bấm **`[E]`**, hệ thống ưu tiên kiểm tra NPC trong cự ly $\le 2.0$m. Nếu có NPC, mở hội thoại `npc.Interact()`. Nếu không có NPC trong tầm, tự động gọi `GameplayHUD.UseHpPotion()` để uống máu.
- **Loại bỏ Bug Kích hoạt Kép khi Triệu Hồi Pet [T]**:
  + Tập trung duy nhất quyền điều phối phím **`[T]`** vào [`PlayerPetRoster.TogglePetSummon()`](file:///d:/Unity/prj1/gam/Assets/Scripts/Pets/PlayerPetRoster.cs). Loại bỏ hoàn toàn các đoạn code bắt sự kiện `[T]` độc lập trong `PlayerController` và `PetController`, xóa bỏ dứt điểm tình trạng nhấn 1 lần bị toggle 2 lần liên tiếp.
- **Tự Động Làm Mới Thanh Kỹ Năng HUD Khi Chuyển Class**:
  + Bổ sung phương thức `GameplayHUD.UpdateSkillBar(CharacterClassData classData)`: Mỗi khi người chơi đổi sang bất kỳ class nào, HUD sẽ tự động tra cứu danh sách `unlockedSkills`, gán icon chiêu thức, cập nhật Mana Cost, gán nhãn phím nóng `[1]`, `[2]`, `[3]`, `[4]`, `[5]`, `[Q]` và reset lại lớp phủ Cooldown tương ứng.

### 3. Hệ Thống Class Demo Switcher Toàn Diện (Phím `[F1]` hoặc `[K]`):
- **Danh mục 14 Class chuẩn Lumina**:
  + 4 Class cơ bản: Tân Thủ (Novice), Chiến Binh (Warrior), Cung Thủ (Archer), Pháp Sư (Mage).
  + 10 Chuyên phái Lumina: Cận Chiến Tự Do (Free Melee), Pháp Sư Nguyên Tố (Elemental Archmage), Pháp Sư Triệu Hồi (Summoner), Pháp Sư Thời Không (Chrono Mage), Mục Sư Thánh - Ma (Holy/Dark Priest), Luyện Dược Sư (Alchemist), Xạ Thủ Súng (Gunner), Cơ Khí Sư (Engineer Gunner), Sát Thủ Bóng Đêm (Shadow Gunner), Xạ Thủ Nguyên Tố (Elemental Archer), Xạ Thủ Tinh Linh (Spirit Archer).
- **Bộ lọc thông minh theo trường phái**: Nút lọc nhanh `Tất Cả`, `Cận Chiến`, `Pháp Sư`, `Xạ Thủ`.
- **Thẻ hiển thị chi tiết 3 Ấn Nội Tại (Passive Seals)**: Hiển thị tên gọi, mô tả chi tiết hiệu ứng của cả 3 ấn ẩn theo tài liệu thiết kế.
- **Nút "⚡ KÍCH HOẠT THỬ CƠ CHẾ CLASS"**:
  + Chạy trực tiếp logic đặc trưng của class đang chọn: Biến Pet thành vũ khí, Nạp hạt nguyên tố vào Hàng đợi, Triệu hồi 2 Pet cộng hưởng, Lùi thời gian 3s, Đổi Form Thánh/Ma, Đặt Chum ủ dược liệu, Dựng Trụ Gatling, Tàng hình & tạo Decoy, Đổi hệ mũi tên, Kích hoạt Hợp Thể Fusion x3.
- **Thanh Công Cụ Debug / Test Thủ Công**:
  + `Lv 1`, `Lv 10`, `Lv 20`, `Lv 30`, `+5 Lv`: Đổi nhanh cấp độ và giai đoạn tiến hóa Tier 1 / 2 / 3.
  + `Hồi Đầy Máu/Mana`: Phục hồi 100% HP và MP ngay lập tức.
  + `+1000 Vàng & Thuốc`: Bơm tiền vàng và cấp thêm các bình máu, mana vào túi đồ.
  + `Spawn Quái Test`: Triệu hồi Quái Vật Mục Tiêu (Training Dummy) đứng yên cạnh người chơi có thanh máu 250 HP, phòng thủ 5, hệ Lửa để thử nghiệm combo kỹ năng và khắc chế nguyên tố.

### 4. Hệ Thống Menu Tạm Dừng Pause Menu (Phím `[Escape]`):
- Tạm dừng thế giới trò chơi (`Time.timeScale = 0`).
- Menu điều hướng nhanh:
  + `Tiếp Tục Chơi`: Quay lại trận chiến (`Time.timeScale = 1`).
  + `Đổi Class Demo [F1]`: Mở bảng chọn và chuyển đổi 14 Class.
  + `Bảng Nhân Vật [U]`: Mở giao diện thông số nhân vật và trang bị.
  + `Túi Đồ [I]`: Mở túi đồ và kho vật phẩm.
  + `Hướng Dẫn Phím Tắt`: Mở Cheat Sheet tổng hợp 15 phím nóng của trò chơi.
  + `Thoát Game`: Thoát game an toàn.

### 5. Khởi tạo Cơ sở Dữ liệu & Bộ Kiểm Thử Tự Động:
- **`LuminaClassAssetDatabase.GenerateAll()`**: Tự động sinh và cấu hình 14 file `Class_*.asset` và 48 file `Skill_*.asset` với 42 Ấn nội tại khớp 100% đặc tả KI.
- **`ClassDemoSwitcherConsistencyTest.cs`**: Bộ test tự động 5 bài đạt tỷ lệ **100% (5/5 Pass)**:
  + `Test 1`: Kiểm tra tính toàn vẹn của toàn bộ 14 ClassData và 42 Ấn nội tại.
  + `Test 2`: Kiểm tra chuyển đổi 14 Class, đồng bộ thuộc tính nguyên tố và phục hồi đầy đủ HP/MP.
  + `Test 3`: Thử nghiệm kích hoạt 10 cơ chế đặc trưng riêng biệt.
  + `Test 4`: Kiểm tra thanh công cụ Debug (Level, Heal, Items, Spawn Dummy).
  + `Test 5`: Kiểm tra hệ thống Menu, tính năng phím tắt và khả năng tự động sinh giao diện không bị lỗi null.
- **Kiểm thử hồi quy Sprint 3 (`Sprint3ConsistencyTest`)**: Tiếp tục đạt **100% (6/6 Pass)**.
- **Biên dịch**: Toàn bộ dự án biên dịch sạch sẽ, 0 lỗi (0 compilation errors).

---

## 🎯 5. HƯỚNG DẪN TRẢI NGHIỆM TEST THỦ CÔNG TRONG GAME

Trong Scene `Assets/Scenes/MainGame.unity` khi nhấn **Play**:

1. **Chuyển đổi Class & Test Cơ chế Nhanh (`[F1]` hoặc `[K]` hoặc click nút `⚔ Đổi Class [F1]` góc trái dưới HUD)**:
   - Bảng **Lumina Class Demo Switcher** sẽ mở ra ở giữa màn hình.
   - Chọn bất kỳ Class nào trong danh sách 14 Class ở cột bên trái (hoặc dùng các tab lọc `Tất Cả`, `Cận Chiến`, `Pháp Sư`, `Xạ Thủ`).
   - Quan sát khung chi tiết: Thông số RPG, Badge vai trò, sao độ khó, mô tả cơ chế nguyên tố, 3 Ấn nội tại phát quang, và danh sách kỹ năng mở khóa.
   - Nhấn **`▶ ÁP DỤNG CLASS NÀY NGAY`**: Nhân vật chuyển ngay sang class đó, thanh kỹ năng HUD cập nhật ngay chiêu thức của class, nguyên tố nhân vật đồng bộ, HP và MP hồi đầy.
   - Nhấn **`⚡ KÍCH HOẠT THỬ CƠ CHẾ CLASS`**: Trực tiếp kích hoạt hiệu ứng đặc trưng (như dựng trụ súng, nạp hạt nguyên tố, tàng hình, đổi dạng Mục Sư...).

2. **Sử dụng Thanh Công Cụ Debug (ở đáy bảng F1)**:
   - Bấm `Lv 1`, `Lv 10`, `Lv 20`, `Lv 30` hoặc `+5 Lv` để test nhanh các mốc cấp độ mở khóa tính năng và trang bị Tier 1/2/3.
   - Bấm `Hồi Đầy Máu/Mana` để cứu nguy tức thì khi chiến đấu.
   - Bấm `+1000 Vàng & Thuốc` để nhận ngay 1000 vàng và 5 bình máu + 5 bình mana.
   - Bấm `Spawn Quái Test` để tạo một Quái Vật Mục Tiêu ngay bên cạnh và thử nghiệm xả chiêu thức.

3. **Menu Tạm Dừng & Tra Cứu Phím Tắt (`[Escape]`)**:
   - Nhấn **`[Escape]`** bất kỳ lúc nào để mở Menu Tạm Dừng (Pause Menu).
   - Chọn mục **`Hướng Dẫn Phím Tắt`** để xem Cheat Sheet tổng hợp toàn bộ 15 phím điều khiển trong Lumina.
   - Nhấn `Tiếp Tục Chơi` hoặc phím `[Escape]` một lần nữa để quay lại trận đấu.

4. **Trải nghiệm các phím tắt đã tối ưu**:
   - **`[X]`**: Đổi Stance Thiên Thần ↔ Ác Quỷ khi chơi Mục Sư.
   - **`[E]`**: Uống Bình Máu nhanh (hoặc Tương tác NPC nếu đứng gần).
   - **`[R]`**: Uống Bình Mana nhanh (hồi chiêu 8s độc lập với bình máu).
   - **`[T]`**: Triệu hồi / Thu hồi Thú cưng mượt mà, không bị kích hoạt kép.
   - **`[U]`**: Mở Bảng Thông Tin Nhân Vật & Trang Bị.
   - **`[I]`**: Mở Túi Đồ.
   - **`[Space]`**: Dash lướt né đòn với khung bất tử (i-frame).

5. **Chi tiết cách Đánh Thường & Tụ Lực Gồng Đòn (Normal Attack & Charge)**:
   - **Chuột Trái (`[Left Click]`)**:
     + **Bấm nhanh**: Tung đòn đánh cơ bản (thời gian hồi đòn: 0.45s).
       * Nhóm Cận Chiến (*Novice, Warrior, Free Melee...*): Quét chém theo hướng nhân vật nhìn (Attack Point 0.7m), gây 100% Sát thương Công Vật Lý / Nguyên Tố và đẩy lùi quái (Knockback 4.5f).
       * Nhóm Xạ Thủ (*Archer, Gunner, Spirit Archer, Elemental Archer...*): Bắn 1 mũi tên hoặc viên đạn bay thẳng theo hướng nhìn.
     + **Nhấn giữ để Tụ Lực (Charge Attack)**:
       * Giữ Chuột Trái để tích năng lượng: Thời gian giữ chuột càng lâu sát thương đòn đánh càng tăng vọt (tối đa **+150% sát thương**, tức 2.5 lần đòn thường).
       * Nhóm Xạ Thủ: Khi tụ lực trên 1.0 giây, mũi tên phóng ra sẽ có khả năng **xuyên thấu qua 3 mục tiêu liên tiếp** (`pierces = 3`).
   - **Chuột Phải (`[Right Click]`) - Chặn Đòn (Active Block)**:
     + Nhấn giữ Chuột Phải để giơ Khiên hoặc vũ khí thủ thế: **Giảm 85% sát thương nhận vào** và **giảm 80% lực đẩy lùi** (tốc độ di chuyển giảm 65% khi đang trong thế thủ).

6. **Chi tiết cách Tung Chiêu & Quản Lý Kỹ Năng (Skill Hotkeys `[1]` - `[5]`, `[Q]`)**:
   - **Thao tác bàn phím tương ứng với 6 Slot Kỹ Năng trên thanh HUD**:
     + **`[1]`**: Kỹ năng Slot 1 - Đòn tấn công nhanh, áp sát hoặc bắn tia cơ bản (tiêu hao Mana thấp, CD 3 - 5s).
     + **`[2]`**: Kỹ năng Slot 2 - Đòn khống chế / diện rộng AoE / bắn tản xạ (tiêu hao 15 - 25 MP, CD 6 - 8s).
     + **`[3]`**: Kỹ năng Slot 3 - Đòn cường hóa, buff bản thân hoặc kỹ năng nguyên tố chuyên sâu (CD 8 - 12s).
     + **`[4]`**: Kỹ năng Slot 4 - Kỹ năng phòng hộ, đặt bẫy, dựng trụ Gatling, đặt Chum ma pháp hoặc rút lui an toàn.
     + **`[5]`**: Kỹ năng Slot 5 - **Tuyệt Kỹ / Đại Chiêu (Ultimate)**: Sát thương diện rộng cực lớn, hiệu ứng nổ hoành tráng (tiêu hao nhiều MP, CD 15 - 25s).
     + **`[Q]`**: Kỹ năng Slot 6 / Tuyệt Kỹ Phản Ứng: Kích hoạt Thiên Tai 3 Combo của Pháp Sư Nguyên Tố, Hợp Thể Fusion x3 của Xạ Thủ Tinh Linh, hoặc chiêu hỗ trợ thứ 2 của Thú Cưng.
   - **Cơ chế thi triển tự động**:
     + **Kiểm tra Mana & Cooldown**: Hệ thống tự động kiểm tra lượng Mana hiện có và thời gian hồi chiêu. Nếu không đủ Mana hoặc đang hồi chiêu, hệ thống sẽ chặn và phát cảnh báo UI.
     + **Tính toán Sát thương & Bạo kích**: Tự động tính toán theo chỉ số Công (`TotalAtk`), tỷ lệ Chí Mạng (`TotalCritRate`), Sát thương Chí Mạng (`TotalCritDamage`), và áp dụng hệ số Khắc Chế Nguyên Tố x2.0.
     + **Lớp phủ HUD Cooldown**: Mỗi khi bấm phím, ô kỹ năng trên thanh HUD sẽ hiển thị vòng tròn hồi chiêu và số giây đếm ngược mượt mà.
   - **Hoán đổi bộ chiêu thức tức thì theo Stance**:
     + Khi chơi class **Mục Sư Thánh - Ma (Holy/Dark Priest)**: Nhấn phím **`[X]`** để hoán đổi tức thì toàn bộ 5 kỹ năng trên thanh HUD giữa **Bộ Chiêu Thiên Thần (Angel Form)** *(Hồi máu, khiên hộ thể, vòng hào quang)* và **Bộ Chiêu Ác Quỷ (Demon Form)** *(Nguyền rủa, tia hủy diệt, hút máu kẻ địch)*.

### Sprint 4.1: Kiểm Tra & Khắc Phục Triệt Để Lỗi Không Thể Di Chuyển & Không Hiện Thanh Menu
- Đã sửa file cấu hình `TimeManager.asset`: Thiết lập `m_TimeScale: 1.0` tránh tình trạng Scene bị đóng băng thời gian ngầm khi khởi động.
- Bổ sung cơ chế nhận diện Input đa tầng (Axis `Horizontal`/`Vertical` + Phím thô WASD + Mũi tên điều hướng + New Input System) cho [`PlayerController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/PlayerController.cs).
- Tích hợp cơ chế tự động xây dựng cây phân cấp UI `EnsureUIHierarchy` và `RuntimeHUDAutoLoader` vào [`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/GameplayHUD.cs) để tự động nạp đầy đủ thanh menu, thanh kỹ năng, quick slots và các panel `[F1]`, `[Escape]`, `[U]`, `[I]` khi Play game.

---

### Sprint 4.2: KIỂM TOÁN TÀI NGUYÊN THỰC TẾ TRÊN ĐĨA (ASSET AUDIT), SỬA LỖI TOÁN HỌC BẮT PET & KHÔI PHỤC TOÀN VẸN WORKLOG
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn thành 100% ✅ (0 Compilation Errors)

#### 1. Sửa Lỗi Toán Học Công Thức Bắt Pet ([`PetCaptureSystem.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Pets/PetCaptureSystem.cs)):
- **Nhận diện lỗi:** Công thức gốc `orbCaptureRate * (1.0f - (currentHp / maxHp)) * 1.5f` khi quái vật máu cực thấp hoặc sử dụng quả cầu cấp cao có thể vượt quá 1.0 (> 100%), đồng thời thiếu mức sàn may mắn tối thiểu khi tỷ lệ quá bé.
- **Giải pháp chuẩn hóa:**
  * Xây dựng phương thức tính toán riêng biệt: `public static float CalculateCaptureChance(float orbCaptureRate, float currentHp, float maxHp)`.
  * Áp dụng `Mathf.Clamp01(...)` để đảm bảo kết quả luôn nằm trong đoạn toán học chính xác `[0.0f, 1.0f]` (0% - 100%).
  * Bổ sung cơ chế may mắn: `Mathf.Max(0.05f, clampedChance)` đảm bảo luôn có ít nhất **5% cơ hội may mắn** thành công.
  * Tích hợp gọi trực tiếp trong quy trình `ExecuteCapture(...)`.

#### 2. Kết Quả Kiểm Đếm Thực Tế Dữ Liệu Trên Đĩa (Asset Audit Log):
- **Dọn dẹp Asset dư thừa:** Đã phát hiện và xóa file nháp cũ không hợp lệ: `Class_StanceArcher.asset` và `.meta`.
- **Kiểm đếm `CharacterClassData` (.asset) — Tổng cộng chính xác 14 Class:**
  1. **Tier 0 (Giai đoạn Tân Thủ - 1 Class):**
     * `Class_Novice.asset`: Tân Thủ (Novice)
  2. **Tier 1 (Giai đoạn Class Cơ Bản - 3 Class):**
     * `Class_Warrior.asset`: Chiến Binh (Warrior)
     * `Class_Archer.asset`: Cung Thủ (Archer)
     * `Class_Mage.asset`: Pháp Sư (Mage)
  3. **Tier 2 (Giai đoạn Chuyên Phái Lumina - 10 Chuyên Phái):**
     * `Class_FreeMelee.asset`: Cận Chiến Tự Do (Free Melee)
     * `Class_ElementalArcher.asset`: Xạ Thủ Nguyên Tố (Elemental Archer)
     * `Class_SpiritArcher.asset`: Xạ Thủ Tinh Linh (Spirit Archer)
     * `Class_EngineerGunner.asset`: Cơ Khí Sư (Engineer Gunner)
     * `Class_ShadowGunner.asset`: Sát Thủ Bóng Đêm (Shadow Gunner)
     * `Class_ElementalMage.asset`: Pháp Sư Nguyên Tố (Elemental Archmage)
     * `Class_Summoner.asset`: Pháp Sư Triệu Hồi (Summoner)
     * `Class_ChronoMage.asset`: Pháp Sư Thời Không (Chrono Mage)
     * `Class_HolyDarkPriest.asset`: Mục Sư Thánh - Ma (Holy/Dark Priest)
     * `Class_Alchemist.asset`: Luyện Dược Sư (Alchemist)
- **Kiểm đếm Ấn Nội Tại Thực Tế:**
  * Mỗi Class sở hữu đầy đủ 3 trường Ấn Nội Tại: `passiveSeal1Name`, `passiveSeal2Name`, `passiveSeal3Name`.
  * **Tổng cộng toàn bộ hệ thống: 14 Class × 3 = 42 Ấn Nội Tại**.
  * Riêng 10 Chuyên Phái Lumina Thức Tỉnh sở hữu đúng **30 Ấn Nội Tại** độc quyền theo tài liệu thiết kế.
- **Kiểm đếm `EnemyData` (.asset) — Tổng cộng 21 File Quái Vật:**
  * **Đúng 18 quái vật & boss đặc tả chuẩn của Sprint 2:**
    - *5 Quái Thảo Nguyên Trù Phú (Lv 2 - 9):* `enemy_steppe_slime`, `enemy_steppe_hedgehog`, `enemy_steppe_sprout`, `enemy_steppe_boar`, `enemy_steppe_rock_crab`.
    - *12 Quái Rừng Rậm Râm Mát (Lv 10 - 18):* `enemy_forest_fire_wolf`, `enemy_forest_night_owl`, `enemy_forest_thorn_snake`, `enemy_forest_thunder_beetle`, `enemy_forest_dark_boar`, `enemy_forest_spore_mushroom`, `enemy_forest_fire_cat`, `enemy_forest_thunder_eagle`, `enemy_forest_fire_mantis`, `enemy_forest_snow_leopard`, `enemy_forest_pure_deer`, `enemy_forest_treant`.
    - *1 Boss Khu Vực Rừng Rậm (Lv 20):* `boss_black_demon_ape` (Hắc Diệu Ma Viên / Obsidian Demon Ape, HP 650, ATK 28, DEF 12, Area Boss & World Boss).
  * **3 Quái vật cơ bản kế thừa từ Sprint 1:** `Enemy_Slime.asset`, `Enemy_Wolf.asset`, `Boss_WolfKing.asset`.

#### 3. Khôi Phục Toàn Vẹn Tài Liệu WORKLOG.md:
- Đã khôi phục hoàn chỉnh mục **Giai đoạn 5** (Kiểm tra tính đồng nhất logic gameplay, EventBus, Keybinds và Test Suite 5 bài).
- Đã khôi phục đoạn mô tả bị đứt gãy về Boss Hắc Diệu Ma Viên và đầy đủ 7 bài test của Sprint 2.
- Bảo toàn tuyệt đối lịch sử phát triển qua các giai đoạn theo phương thức bổ sung (Append/Refine).
- Đã đối chiếu và kiểm tra trạng thái: Toàn bộ project biên dịch sạch sẽ, **0 lỗi biên dịch (0 compilation errors)**.

---

### SPRINT 5: TRIỂN KHAI HỆ THỐNG SAVE/LOAD BẢO MẬT: JSON + AES-256 + SHA-256 (ASSETS/SCRIPTS/SAVE/)
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn thành 100% ✅ (0 Compilation Errors)

#### 1. Cấu Trúc Dữ Liệu Lưu Trữ Toàn Diện ([`SaveData.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Save/SaveData.cs)):
- `[System.Serializable] public class SaveData`: Cấu trúc phân vùng modular, tuần tự hóa JSON chuẩn mực bằng `JsonUtility`.
- **7 Phân Vùng Lưu Trữ**:
  * `playerData` (`PlayerSaveSection`): `level`, `currentExp`, `currentHp`, `currentMp`, `gold`, `currentTier`, `currentClassId`, `lastPosition` ([`SerializableVector3`](file:///d:/Unity/prj1/gam/Assets/Scripts/Save/SaveData.cs#L12-L33)).
  * `attributes` (`AttributesSaveSection`): `baseStr`, `baseDex`, `baseInt`, `baseVit`.
  * `inventoryData` (`InventorySaveSection`): Danh sách `SavedInventoryItem` (`itemId`, `itemName`, `quantity`, `durability`).
  * `equipmentData` (`EquipmentSaveSection`): Danh sách `SavedEquippedSlot` (`slotName`, `itemId`, `itemName`) cho 7 slot trang bị.
  * `petStorageData` (`PetStorageSaveSection`): Danh sách `SavedPetData` (`petId`, `customName`, `level`, `currentHp`, `bondLevel`, `isSummoned`) và `activePetId`.
  * `worldFlags` (`WorldFlagsSaveSection`): `unlockedWaystones`, `completedQuests`, `defeatedBosses`.
  * `metadata` (`MetadataSaveSection`): `saveTimestamp`, `playTime`, `saveVersion`.

#### 2. Module Mã Hóa AES-256 & Chữ Ký Toàn Vẹn SHA-256 ([`SaveSecurity.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Save/SaveSecurity.cs)):
- **Mã Hóa AES-256 (CBC, PKCS7)**:
  * `EncryptString(string plainText, string key, string iv)`: Mã hóa JSON thô sang chuỗi Base64 với Key 256-bit (32 bytes) và IV 128-bit (16 bytes).
  * `DecryptString(string cipherText, string key, string iv)`: Giải mã chuỗi Base64 trở về JSON nguyên bản.
- **Chữ Ký Chống Gian Lận SHA-256 (Anti-Tamper & Integrity Check)**:
  * `ComputeHash(string data)`: Tạo chữ ký băm SHA-256 64 ký tự hex từ chuỗi JSON thô.
  * Khi Lưu: Ghi song song 2 file tại `Application.persistentDataPath`:
    - `savegame.dat`: Chứa chuỗi mã hóa AES-256.
    - `savegame.sig`: Chứa mã băm SHA-256 của dữ liệu JSON.
  * Khi Tải: Đọc `savegame.dat` $\rightarrow$ Giải mã AES-256 $\rightarrow$ Tính toán lại băm SHA-256 $\rightarrow$ So khớp với `savegame.sig`. Nếu phát hiện bất kỳ sai lệch nào (do can thiệp gian lận file save), hệ thống lập tức từ chối tải và phát cảnh báo: `"Dữ liệu lưu đã bị can thiệp!"`.

#### 3. Quản Lý Vòng Đời Save/Load Toàn Game ([`SaveManager.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Save/SaveManager.cs)):
- **Singleton `SaveManager.Instance`**:
  * `SaveGame()`: Thu thập dữ liệu từ `PlayerController` (tọa độ), `CharacterStats` (level, exp, hp, mp, stats), `Inventory` (vàng, túi đồ, trang bị 7 slot), `ClassSystem` (class hiện tại), `PlayerPetRoster` (kho Pet và Pet xuất chiến) $\rightarrow$ Đóng gói $\rightarrow$ Mã hóa AES-256 $\rightarrow$ Ký SHA-256 $\rightarrow$ Lưu ra đĩa.
  * `LoadGame()`: Đọc file $\rightarrow$ Xác thực SHA-256 $\rightarrow$ Giải mã AES-256 $\rightarrow$ Phục hồi tọa độ, chỉ số, class, túi đồ, trang bị, và triệu hồi Pet.
  * `DeleteSave()`: Xóa sạch dữ liệu cũ (`savegame.dat` và `savegame.sig`).
  * Phím tắt Debug tiện lợi: **`[F5]` Quick Save**, **`[F9]` Quick Load**.

#### 4. Bộ Kiểm Thử Tự Động Trong Editor ([`SaveSystemTest.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/SaveSystemTest.cs)):
- Tích hợp Menu Editor: `Tools -> ActionRPG -> Run Save/Load Security Test`.
- Kiểm thử tự động 4 bước đạt **100% PASS**:
  1. Tạo SaveData mẫu với đầy đủ 7 phân vùng $\rightarrow$ Lưu và mã hóa AES-256 thành công.
  2. Đọc và giải mã $\rightarrow$ Xác minh dữ liệu khớp 100% với mẫu ban đầu.
  3. Cố tình sửa 1 ký tự trong file `savegame.dat` và `savegame.sig` $\rightarrow$ Hệ thống chống giả mạo SHA-256 phát hiện và chặn tải thành công.
  4. Báo cáo kết quả chi tiết trên Console với tỷ lệ đạt 100%.

---

### SPRINT 6: SỬA LỖI UI RECTTRANSFORM (SKILLSLOT_1) & TỰ ĐỘNG THANH LỌC HIERARCHY
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn thành 100% ✅ (0 Compilation Errors, 0 MissingComponentException)

#### 1. Nguyên Nhân Gốc Rễ (Root Cause):
- Trong Unity (đặc biệt là cơ chế kiểm tra kiểu nghiêm ngặt của Unity 6), khi khởi tạo GameObject bằng `new GameObject("Name")`, đối tượng mặc định được gắn một `Transform` thông thường.
- Việc gọi `AddComponent<RectTransform>()` sau đó không thể thay thế `Transform` có sẵn trong core Unity C++, dẫn đến việc đối tượng con UI (`SkillSlot_1..8`, `Icon`, `CooldownOverlay`,...) chỉ sở hữu `Transform` thay vì `RectTransform`.
- Khi các component layout của Unity UI (`HorizontalLayoutGroup`), hoặc script `GameplayHUD.EnsureUIHierarchy()` truy xuất `sRt.sizeDelta`, Unity ném ngoại lệ:
  `MissingComponentException: There is no 'RectTransform' attached to the "SkillSlot_1" game object, but a script is trying to access it.`

#### 2. Giải Pháp Triển Khai Toàn Diện:
- **Chuẩn Hóa Khởi Tạo UI bằng `typeof(RectTransform)`:**
  * Thay thế 100% pattern lỗi thời `new GameObject(name); obj.AddComponent<RectTransform>();` trên toàn bộ codebase bằng:
    `new GameObject(name, typeof(RectTransform));`
    kết hợp với `obj.GetComponent<RectTransform>()`.
  * Các file đã chuẩn hóa:
    - [`Assets/Scripts/UI/Gameplay/GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs)
    - [`Assets/Editor/GenerateUIAssets.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/GenerateUIAssets.cs)
    - [`Assets/Editor/GameSceneBuilder.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/GameSceneBuilder.cs)
    - [`Assets/Editor/SceneSetupAndConsistencyFixer.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/SceneSetupAndConsistencyFixer.cs)
    - [`Assets/Scripts/UI/PauseMenuUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/PauseMenuUI.cs)
    - [`Assets/Scripts/UI/ClassDemoSwitcherUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassDemoSwitcherUI.cs)
    - [`Assets/Scripts/UI/ClassSelectionUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassSelectionUI.cs)
    - [`Assets/Scripts/UI/Gameplay/PetHUDWidget.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/PetHUDWidget.cs)
    - [`Assets/Scripts/UI/HUDController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/HUDController.cs)
    - [`Assets/Scripts/UI/Character/SkillsUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Character/SkillsUI.cs)
    - [`Assets/Scripts/Environment/Gathering/GatheringNode.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Environment/Gathering/GatheringNode.cs)
- **Cơ Chế Tự Động Thanh Lọc & Tự Chữa Lành Runtime (`Self-Healing & Auto-Purge`):**
  * Trong `GameplayHUD.cs`: Thêm hàm `CleanNonRectTransformChildren(Transform parent)` quét và tiêu hủy ngay lập tức (`DestroyImmediate` / `Destroy` sau khi `SetParent(null)`) bất kỳ đối tượng con nào chỉ có `Transform` thường tồn đọng từ các lần build scene trước.
  * Trong `EnsureRectTransform(parent, name)`: Nếu phát hiện đối tượng cũ cùng tên nhưng không phải `RectTransform`, lập tức tách khỏi cây hierarchy và tiêu hủy, thay thế bằng GameObject chuẩn `RectTransform`.
- **Kiểm Toán Cú Pháp & Toàn Vẹn Mã Nguồn:**
  * 0 lệnh `AddComponent<RectTransform>` còn sót lại trong toàn bộ project.
  * Đã dọn sạch khối khai báo trùng lặp biến (`skLayout`, `actionArea`, `aart`) trong `ClassDemoSwitcherUI.cs`, giải quyết triệt để lỗi biên dịch CS0128.
  * Tỷ lệ đóng/mở ngoặc `{ }` và `#region / #endregion` đạt cân bằng 100% trên toàn bộ các file `.cs`.

---

### SPRINT 7: CHUẨN HÓA VÙNG AN TOÀN LÀNG TÂN THỦ, KHÓA PET ĐẦU GAME & ĐỒNG BỘ TIẾN HÓA 3 TIER
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn thành 100% ✅ (0 Lỗi Biên Dịch, Đồng bộ cả Unity C# và Web Demo 60 FPS)

#### 1. Loại Bỏ Pet Đầu Game (Khóa Hoàn Toàn Tier 1: Lv 1 – 9):
- **Cơ chế nghiêm ngặt:** Nhân vật khởi đầu tại Cấp 1 thuộc **Tier 1 (Tập Sự - Novice)**. Hệ thống Pet bị khóa hoàn toàn (`isPetUnlocked = false`).
- **Mã nguồn Unity (`PlayerPetRoster.cs`, `PetController.cs`):**
  * `IsPetUnlocked`: Ràng buộc cả 2 điều kiện: `stats.level >= stats.petUnlockLevel && stats.isPetUnlocked`.
  * `PetController.Start()`: Tự động kiểm tra `stats.level < stats.petUnlockLevel || !stats.isPetUnlocked` để vô hiệu hóa (`gameObject.SetActive(false)`), tuyệt đối không cho phép Pet xuất hiện hay chạy theo người chơi trong giai đoạn đầu game.
  * Phím tắt `[T]` bị chặn: Khi bấm phím [T], phát thông báo nổi trên UI: `"Cần Cấp 10 để mở khóa Đồng Hành!"`.
  * Khung Pet trên HUD: Hiển thị biểu tượng Ổ Khóa 🔒 kèm text `"Mở khóa ở Cấp 10"`.
- **Mốc Mở Khóa Cấp 10 (Tier 2: Phái Cơ Bản):**
  * Khi nhân vật đạt Cấp 10: Tự động phát sự kiện `EventBus.OnPetSystemUnlocked`, cấp Pet tân thủ (Hồ Ly Lửa) + 5 Quả Cầu Tinh Linh, kích hoạt phím [T] gọi/thu hồi Pet và mở tính năng thu phục thú rừng khi máu quái <= 30%.

#### 2. Vùng An Toàn Làng Tân Thủ (Oakhaven Safe Zone - Tuyệt Đối Không Có Quái):
- **Bảo vệ Vùng An Toàn (`EnemySpawner.cs`):**
  * Thiết lập `villageSafeCenter = (0, 0, 0)` và `villageSafeRadius = 18f`.
  * Trong `SpawnRandomEnemy()`: Nếu vị trí ngẫu nhiên nằm trong bán kính an toàn của làng, tự động đẩy vị trí sinh quái ra ngoài ranh giới (bán kính >= 20m).
  * Trong Scene `MainGame.unity`: Điều chỉnh tọa độ Spawner 1 dời ra x = 26 (Thảo Nguyên Xanh) và Spawner 2 ra x = 50 (Khu Rừng Rậm), dọn sạch 100% quái vật ra khỏi Làng Tân Thủ.
- **Ranh Giới De-Aggro (`EnemyController.cs`):**
  * Bổ sung cơ chế bảo vệ: Nếu Player chạy vào Làng Tân Thủ (`distToVillage < 16m`) hoặc bản thân quái bị kéo lại gần làng, quái lập tức mất dấu aggro, hủy truy đuổi và quay đầu về điểm tuần tra bên ngoài.

#### 3. Đồng Bộ Toàn Diện Lên Bản Web Demo Trực Tiếp (`web-demo/`):
- **Bản đồ thế giới mở phân vùng rõ rệt:**
  * **Làng Tân Thủ (Oakhaven):** Khu vực an toàn lát đá bán kính 260px, Trụ Đá Waystone phát sáng trung tâm, Lò rèn, Hiệp hội thợ săn, Tiệm thuốc. 0 quái vật xuất hiện.
  * **Thảo Nguyên Xanh (Zone 1: Lv 1 - 10):** Nằm ngoài cổng Làng về phía Đông, có 5 loài quái (Slime Thủy bị động, Nhím Gai, Heo Đất, Mầm Cây, Mini-Boss Cua Bọc Đá với Telegraph đỏ 1.2s) và các bụi Thảo Mộc / Bãi Quặng ([F] thu hoạch).
  * **Đại Lộ Huyết Mạch (Highway):** Đường lát đá nối Làng Tân Thủ tới Cổng Đô Thành Lumina (yêu cầu Cấp 20+).
  * **Khu Rừng Rậm (Zone 2: Lv 10 - 20):** Nằm về phía Bắc, gồm 13 loài bắt Pet (Sói Lửa, Cú Đêm, Rắn Gai, Bọ Sấm, Báo Tuyết, và Boss Hắc Diệu Ma Viên với 2 Phase & combo Telegraph đỏ).
- **Tính năng bắt Pet (Phím C):** Khi đánh quái rừng tụt máu dưới 30% HP, ném Quả Cầu Tinh Linh để thu phục.
- **Thanh công cụ kiểm thử:** Tích hợp các nút dịch chuyển tức thời (`[Về Làng]`, `[Thảo Nguyên]`, `[Rừng Rậm]`) và nút đặt cấp độ nhanh (`[Lv.1 Novice]`, `[Lv.10 Mở Pet]`, `[Lv.20 Lumina]`).

---

### SPRINT 8: CƠ CHẾ HỒI SINH BOSS LIÊN TỤC & HẤP THỤ TINH HOA PHONG CÁCH "TATE NO YUUSHA" VÀ "LOG HORIZON"
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn thành 100% ✅ (0 Lỗi Biên Dịch, Đồng bộ Unity C# và Web Demo 60 FPS)

#### 1. Chuyển Đổi Triết Lý Thiết Kế Trò Chơi:
- **Xóa bỏ triệt để tư duy "Vượt ải" (Stage-Clear):** Trò chơi được định hình rõ nét là một Game Nhập Vai Hành Động Thế Giới Mở Sống Động (Persistent Living Sandbox ARPG), lấy cảm hứng trực tiếp từ hai bộ anime kinh điển:
  * **Sự Trỗi Dậy Của Khiên Anh Hùng (Tate no Yuusha no Nariagari):** Cơ chế hấp thụ tinh hoa quái vật mở khóa vũ khí, ấp trứng nuôi dưỡng thần thú, và Đợt Sóng Tai Ương toàn cầu.
  * **Log Horizon:** Cơ chế Hate/Aggro trong săn Boss dã ngoại, hệ thống nghề phụ (Đầu bếp, Thợ rèn, Luyện dược), và phát triển kinh tế thị trấn.
- **Boss và Quái vật không bao giờ biến mất vĩnh viễn:** Đánh bại Boss không phải là kết thúc màn chơi, mà là một chiến tích trong chu kỳ săn bắn dã ngoại.

#### 2. Cơ Chế Tái Sinh Liên Tục Trong Unity C# ([`EnemySpawner.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Enemies/EnemySpawner.cs)):
- **Cấu hình chu kỳ hồi sinh tự nhiên:**
  * `bossRespawnCooldown = 35.0f` (có thể tinh chỉnh lên 10 - 15 phút cho bản phát hành chính thức).
  * `mobRespawnDelay = 10.0f` đối với quái vật thường ngoài thảo nguyên và rừng rậm.
- **Lắng nghe sự kiện toàn cầu (`EventBus.OnBossDefeated`):**
  * Khi Boss (Hắc Diệu Ma Viên / Vua Sói) bị tiêu diệt, Spawner tự động kích hoạt `isBossAwaitingRespawn = true` và bắt đầu đếm ngược `bossRespawnTimer`.
  * Phát thông báo thế giới: `"👑 Boss đã bị đánh bại! Chu kỳ tái sinh: X giây..."`.
  * Khi hết thời gian: Kích hoạt `SpawnBossDirectly()` tái sinh Boss ngay tại sào huyệt kèm hiệu ứng âm thanh rống vang dội (`boss_roar`) và cảnh báo đỏ: `"⚠️ CẢNH BÁO THẾ GIỚI: Boss ĐÃ TÁI SINH TẠI SÀO HUYỆT!"`.

#### 3. Đồng Bộ Trực Tiếp Trên Web Demo (`web-demo/src/game.js` & `entities.js`):
- **Hàng đợi hồi sinh `this.respawnQueue`:**
  * Tự động ghi nhận vị trí tọa độ gốc `(spawnX, spawnY)` và cấu hình thời gian hồi sinh:
    - Boss Thế Giới (Hắc Diệu Ma Viên Lv 20): **35 giây**.
    - Mini-Boss Tuyến Đường (Cua Bọc Đá Lv 8): **20 giây**.
    - Quái Dã Ngoại thường: **12 giây**.
  * Khi thời gian hồi sinh kết thúc: Kích hoạt hiệu ứng nổ năng lượng màu đỏ/cam và tái tạo lại quái vật nguyên bản, phát thông báo nổi rực rỡ trên màn hình.
- **Cơ Chế "Hấp Thụ Tinh Hoa Quái Vật" (Monster Part Absorption - Phong cách Khiên Hiệp Sĩ):**
  * Quái và Boss khi chết rơi ra các túi tài nguyên: Bình máu, Cầu tinh linh và **Thịt Quái Tươi (`meat`)**.
  * Khi người chơi thu thập, hàm `player.absorbMonsterItem("Thịt Quái Tươi")` được kích hoạt:
    - Cứ mỗi 3 vật phẩm quái vật nhặt được, vũ khí/khiên hấp thụ tinh hoa và tăng vĩnh viễn: **+2 Sức Mạnh Tấn Công (ATK)**, **+1 Phòng Thủ (DEF)**, **+15 Máu Tối Đa (Max HP)**.
    - Phát nổ ánh sáng hoàng kim và thông báo nổi: `"🛡️ HẤP THỤ TINH HOA: +2 ATK, +1 DEF, +15 HP VĨNH VIỄN!"`.

#### 4. Đặc Tả Thiết Kế Dự Án & Kế Hoạch 5 Trụ Cột:
- Đã khởi tạo Artifact chi tiết: [`anime_arpg_evolution_plan.md`](file:///C:/Users/ASUS/.gemini/antigravity-ide/brain/8e10c03f-3399-4a3e-8d1d-67af77892c9a/anime_arpg_evolution_plan.md)
- *Lưu ý:* Toàn bộ các định hướng mở rộng chuyên sâu thuộc lộ trình Sprint 8 - 12 (Cây hấp thụ vũ khí, Đợt sóng tai ương, Thú cưỡi, Nâng cấp làng, Đô thành Lumina) đã được hệ thống hóa và lưu trữ sang tài liệu học thuật độc lập: [Assets/Docs/FUTURE_ROADMAP_AND_EXPANSIONS.md](file:///d:/Unity/prj1/gam/Assets/Docs/FUTURE_ROADMAP_AND_EXPANSIONS.md) theo chỉ thị **Đóng Băng Phạm Vi (Scope Freeze)**.

---

### SPRINT 9: ĐÓNG BĂNG PHẠM VI (SCOPE FREEZE) & THIẾT LẬP BẢN DEMO CỐT LÕI (CORE MVP)
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn tất 100% ✅ (Console Unity 0 Lỗi Biên Dịch, Scene MainGame.unity Chuẩn Hóa Vòng Lặp 10 Phút)

#### 1. Kích Hoạt Chỉ Thị Đóng Băng Phạm Vi (Scope Freeze):
* **Mục tiêu:** Chấm dứt hoàn toàn việc viết thêm code tính năng mới ngoài lề; tránh quá tải mã nguồn và phân tán nguồn lực làm đồ án.
* **Biện pháp:** 
  - Toàn bộ các ý tưởng nâng cao (Cây hấp thụ vũ khí nuốt thịt quái, Đợt sóng tai ương toàn cầu, Ấp trứng thú cưỡi [G], Nghề phụ nấu ăn/chế bẫy, Quản trị nâng cấp Làng Oakhaven, Đô thành Lumina, Hate table đa mục tiêu) được đóng gói và chuyển toàn bộ sang tài liệu phục vụ chương "Hướng Phát Triển Tương Lai" trong Khóa luận tốt nghiệp:
    📄 **[FUTURE_ROADMAP_AND_EXPANSIONS.md](file:///d:/Unity/prj1/gam/Assets/Docs/FUTURE_ROADMAP_AND_EXPANSIONS.md)**
  - Đóng băng mã nguồn, cô lập hoàn toàn các stub/mã rác, tập trung kiểm thử và đánh bóng 4 Trụ Cột Cốt Lõi.

#### 2. Định Nghĩa 4 Trụ Cột Bản Demo Cốt Lõi (Core MVP):
1. **Lõi Nhân Vật & 3 Bậc Tiến Hóa (Progression Loop):**
   - **Tier 1 (Lv 1–9):** Tập sự (Novice) dùng kiếm chém cơ bản, Pet bị khóa chặt chẽ.
   - **Tier 2 (Lv 10–19):** Tiến cấp 3 phái cơ bản (Chiến Binh, Cung Thủ, Pháp Sư) + Mở khóa tính năng Thu phục Pet.
   - **Tier 3 (Lv 20+):** Mở khóa các Chuyên phái Lumina chuyên sâu và 3 Ấn Nội Tại ẩn.
2. **Hệ Thống Pet Dã Ngoại Cơ Bản (Companion System):**
   - Bắt quái dã ngoại khi máu < 30% bằng Quả Cầu Tinh Linh (phím [C] / Item).
   - Triệu hồi / Thu hồi linh hoạt bằng phím [T], AI bám theo chủ nhân và tự động tấn công mục tiêu Player chỉ định.
3. **Môi Trường & Chiến Đấu Dã Ngoại (Living Wilderness):**
   - **Thảo Nguyên Xanh (Zone 1: Lv 1–10):** Quái Slime, Nhím gai + Bụi thảo mộc nhặt hái bằng phím [F].
   - **Khu Rừng Rậm (Zone 2: Lv 10–20):** Quái Sói lửa, Cú đêm + Boss Hắc Diệu Ma Viên / Vua Sói có vệt Telegraph cảnh báo đỏ và cơ chế hồi sinh liên tục tại sào huyệt sau 35s.
4. **Hệ Thống Lưu Trữ An Toàn & Bảo Mật (Secure Persistence):**
   - Đóng gói toàn bộ hồ sơ nhân vật (Level, EXP, HP, MP, Inventory, Vàng, Pet Roster, Class) thành file JSON.
   - Mã hóa AES-256 + Ký chữ ký số SHA-256 chống can thiệp.
   - Phím tắt tiện lợi: **[F5] Quick Save** ↔ **[F9] Quick Load** trong môi trường Editor và Build.

#### 3. Bảng Kiểm Toán Mã Nguồn & Tính Toàn Vẹn Hệ Thống (Audit Checklist):

| Hạng Mục Kiểm Toán | Tiêu Chí Đánh Giá | Kết Quả Thực Tế | Ghi Chú |
| :--- | :--- | :---: | :--- |
| **Lỗi Biên Dịch (Compilation)** | Unity Batchmode Compile 0 Errors | **PASS (0 Lỗi)** ✅ | Khắc phục `EnemySpawner.cs` thiếu namespace `ActionRPG.Core`. Mã nguồn sạch 100%. |
| **Di Chuyển & Điều Khiển** | WASD / Mũi tên + Space lướt né | **PASS** ✅ | `PlayerController.cs` Dynamic Rigidbody2D, BaseSpeed 4.5. |
| **Giao Diện HUD** | Hiển thị Máu, Mana, EXP, Cấp, Pet | **PASS** ✅ | `GameplayHUD.cs` & `Canvas_GameUI` tự động cập nhật từ `EventBus`. |
| **Hệ Thống Pet Tier 1-2** | Khóa Pet Lv 1-9; Mở khóa Lv 10; [T] Gọi/Thu | **PASS** ✅ | `PlayerPetRoster.cs` & `PetController.cs` kiểm tra điều kiện cấp độ nghiêm ngặt. |
| **Vùng An Toàn Làng Tân Thủ** | 0 quái vật trong bán kính 18m của làng | **PASS** ✅ | `EnemySpawner.cs` chặn sinh quái trong Safe Zone, `EnemyController.cs` De-Aggro khi về làng. |
| **Săn Boss & Tái Sinh** | Boss có Telegraph cảnh báo, chết đếm ngược hồi sinh | **PASS** ✅ | `TelegraphIndicator.cs` + `EnemySpawner.cs` (Cooldown 35s hồi sinh liên tục). |
| **Lưu Trữ [F5] / [F9]** | Quick Save AES-256 + Quick Load | **PASS** ✅ | `SaveManager.cs` đã gắn trực tiếp vào `GameSystems` trong scene `MainGame.unity`. |
| **Tính Toàn Vẹn Scene** | Scene `MainGame.unity` nạp và chạy trực tiếp | **PASS** ✅ | Đã chạy `SceneSetupAndConsistencyFixer`, lưu scene chuẩn hóa thành công (Saved=True). |

---

### SPRINT 10: TÁI CẤU TRÚC KIẾN TRÚC PHÂN TÁCH ĐỘC LẬP (CLEAN ARCHITECTURE / MVP - CORE LOGIC VS PRESENTATION)
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn tất 100% ✅ (Phân tách triệt để 2 nhánh thư mục, Triển khai trọn vẹn mô hình Pure C# Logic & Visuals View)

#### 1. Bối Cảnh & Mục Tiêu Kiến Trúc:
* **Vấn đề tồn đọng:** Mã nguồn trước đây bị trộn lẫn giữa Logic số liệu (HP, Stat, Cooldown) và Visual (SpriteRenderer, nhấp nháy đỏ, Animator) trong các lớp MonoBehaviour như `Hurtbox2D.cs`, `CharacterStats.cs`, `PlayerController.cs`. Điều này gây khó khăn khi tùy biến đồ họa 2D Pixel Art và không thể viết Unit Test độc lập.
* **Mục tiêu chuyển đổi:** Tách biệt dự án thành 2 phân nhánh chuyên trách:
  1. **Nhánh 1: Core Logic & Cầu Nối Unity (Backend / Pure C#):** Chỉ chịu trách nhiệm thuật toán, quy tắc game, số liệu và bắn C# Events. Hạn chế tối đa phụ thuộc vào MonoBehaviour.
  2. **Nhánh 2: Presentation & Visuals (Frontend / 2D Pixel Art & UI):** Chỉ lắng nghe tín hiệu từ Logic để đổi Sprite, chạy Animation, chớp đỏ, cập nhật UI Slider/TMP.

#### 2. Cấu Trúc Thư Mục Mới Được Thiết Lập:
* **[Assets/Scripts/01_Core_And_Logic/](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic):**
  * `Domain/Interfaces/`: [`IHealthModel.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Domain/Interfaces/IHealthModel.cs), [`IMovementModel.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Domain/Interfaces/IMovementModel.cs).
  * `Domain/Models/`: 
    * [`PlayerHealthModel.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Domain/Models/PlayerHealthModel.cs) — Pure C# Class quản lý HP, sát thương, trạng thái tử trận.
    * [`PlayerMovementModel.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Domain/Models/PlayerMovementModel.cs) — Pure C# Class tính toán vận tốc, chuẩn hóa vector, lướt Dash, hướng quay mặt.
  * `Domain/ValueObjects/`: [`DamageData.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Domain/ValueObjects/DamageData.cs) — Pure C# Struct DTO truyền dữ liệu sát thương.
  * `Services/`: [`DamageCalculationService.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Services/DamageCalculationService.cs) — Dịch vụ tính toán giảm trừ giáp, sát thương chí mạng.
  * `Bridges/`: [`PlayerPresenter.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Bridges/PlayerPresenter.cs) — Lớp MonoBehaviour điều phối MVP (Input -> Pure Models -> Views/UI).
* **[Assets/Scripts/02_Presentation_Visuals/](file:///d:/Unity/prj1/gam/Assets/Scripts/02_Presentation_Visuals):**
  * `Views/`:
    * [`CharacterVisualView.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/02_Presentation_Visuals/Views/CharacterVisualView.cs) — View chuyên trách SpriteRenderer (FlipX), Animator và SpriteAnimator2D.
    * [`HitFlashEffectView.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/02_Presentation_Visuals/Views/HitFlashEffectView.cs) — View xử lý hiệu ứng nhấp nháy đỏ/trắng pixel art khi nhận đòn.
    * [`HealthBarView.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/02_Presentation_Visuals/Views/HealthBarView.cs) — View hiển thị thanh máu mượt mà kèm TMP text.

#### 3. Bóc Tách & Tương Thích Ngược Trong Hệ Thống Chiến Đấu:
* **Refactor [`Hurtbox2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/Hurtbox2D.cs):**
  * Giải phóng `Hurtbox2D` khỏi việc trực tiếp can thiệp đổi màu `spriteRenderer.color`.
  * Bổ sung C# Event `OnDamageTaken`.
| **Refactor Hurtbox2D** | Bóc tách Visual khỏi Combat & hỗ trợ Presenter | **PASS** ✅ | `Hurtbox2D.cs` chuyển giao hiệu ứng nhấp nháy đỏ cho `HitFlashEffectView` và nhận diện `PlayerPresenter`. |

---

### SPRINT 11: TÍCH HỢP BẢN ĐỒ THẾ GIỚI, HỆ THỐNG PHÂN VÙNG VÀ SPAWNER DÃ NGOẠI
> **Thời gian thực hiện:** Tháng 10 Năm 2026  
> **Trạng thái:** Hoàn tất 100% ✅ (ZoneManager, SafeZoneTrigger, Spawner Phân Vùng, Waystone Hồi Sinh & Tool Auto Layout)

#### 1. Quản Lý Phân Vùng & Vùng An Toàn (Zone System & Safe Zone):
* **[`ZoneManager.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/World/ZoneManager.cs):**
  - Quản lý `GameZone`: `Village_Oakhaven` (Làng Tân Thủ), `Grassland_Lv1_10` (Thảo Nguyên), `Highway_Road` (Đại Lộ), `DeepForest_Lv10_20` (Rừng Rậm), `BossArena` (Sào Huyệt Boss).
  - Bắn sự kiện tĩnh `OnZoneChanged(GameZone newZone)` và thông báo banner nổi giao diện `EventBus.OnShowUIMessage`.
* **[`SafeZoneTrigger.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/World/SafeZoneTrigger.cs):**
  - Gắn tại Làng Tân Thủ (`BoxCollider2D Trigger`).
  - Trong Làng: Tự động bật `isSafeZone = true` trên `PlayerPresenter` và `PlayerController`. Khóa toàn bộ thao tác vung kiếm, tấn công chuột trái hoặc dùng kỹ năng chiến đấu (báo thông báo UI bảo vệ).
  - Ra ngoài Cổng Làng: Tắt cờ `isSafeZone = false`, tự động chuyển phân vùng sang `Grassland_Lv1_10` và mở lại toàn bộ lệnh chiến đấu.

#### 2. Bộ Điều Phối Spawner Quái Vật Theo Phân Vùng (Zone Spawners):
* **[`ZoneEnemySpawner.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/World/Spawners/ZoneEnemySpawner.cs):**
  - Chứa hồ bơi quái vật (`enemyPool`) lấy dữ liệu `EnemyData` tương ứng phân vùng.
  - Cấu hình `maxEnemiesAlive`, bán kính `spawnRadius` và thời gian đếm ngược hồi sinh `respawnDelay` (20s - 30s sau khi quái chết).
  - Tự động bù đắp quái theo chu kỳ liên tục, hỗ trợ vẽ Gizmos theo mã màu phân vùng trong Scene View.

#### 3. Trụ Đá Dịch Chuyển & Điểm Hồi Sinh (Waystone Pillars):
* **[`Waystone.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/World/Interactables/Waystone.cs):**
  - Đặt tại Làng Tân Thủ (`Waystone_Village`) và Bìa Rừng Rậm (`Waystone_Forest`).
  - Khi Player đến gần bấm **[F]**: Kích hoạt mở khóa trụ đá, đổi màu vệt sáng phát quang (Glow), lưu tọa độ làm điểm hồi sinh mặc định.
  - Tích hợp tự động vào [`PlayerPresenter.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/Bridges/PlayerPresenter.cs): Khi Player tử trận (`HealthModel.OnDied`), sau 2.5s đếm ngược sẽ tự động hồi đầy máu và dịch chuyển Player an toàn về Trụ Đá tích cực gần nhất.

#### 4. Công Cụ Tự Động Bố Trí Scene (World Map Layout Tool):
* **[`WorldMapSceneBuilder.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/WorldMapSceneBuilder.cs):**
  - Tích hợp Menu Editor: **`Tools -> Setup Lumina World Layout`** (hoặc `Tools/Lumina/Setup Lumina World Layout`).
  - Tự động dựng toàn bộ hệ thống thế giới trong `MainGame.unity`:
    + `Zone_01_Village`: SafeZoneTrigger (28x24m), Trụ đá Làng Oakhaven, Điểm spawn Player.
    + `Zone_02_Grassland`: 3 cụm Spawner quái Lv 1-10 (Slime, Mầm Cây, Nhím Gai, Heo Rừng, Cua Đá) + 4 Node thu thập nguyên liệu (2 Bụi Thảo Dược, 2 Bãi Khoáng Thạch).
    + `Zone_03_Forest`: Trụ đá Bìa Rừng, 4 Spawner thú rừng Lv 10-20 (Sói Lửa, Cú Đêm, Báo Tuyết, Lợn Rừng Đen, Cổ Thụ, Nấm Độc) + Cổng Hang Boss.
    + `Boss_Arena`: Vùng đấu trường Boss, Boss Hắc Diệu Ma Viên (Obsidian Demon Ape Lv 20) tích hợp Telegraph Indicator.
    + Tự động gắn kết cấu trúc Clean Architecture (`PlayerPresenter`, `CharacterVisualView`, `HitFlashEffectView`).

---

* **Khắc Phục Kích Thước Sprite (PPU 64), Animation Di Chuyển & Hệ Thống Chiến Đấu 2D 360 Độ (13:25):**
  - **1. Chuẩn Hóa Tỷ Lệ Màn Hình & PPU (Pixel-Perfect):**
    + Điều chỉnh chuẩn Pixels Per Unit (PPU) từ `16` lên `64` trong [`config.json`](file:///d:/Unity/prj1/gam/Tools/PixelLab/config.json), toàn bộ 12 tệp metadata JSON trong [`Assets/Art/Spritesheets/`](file:///d:/Unity/prj1/gam/Assets/Art/Spritesheets/) và [`PixelLabAnimationImporter.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/PixelLabAnimationImporter.cs).
    + Kích thước frame $128 \times 128$ px chia cho PPU 64 tương ứng chiều cao nhân vật đúng **2.0 Unity Units**, đạt tỷ lệ vàng cân đối với Orthographic Camera và môi trường màn hình.
    + Khôi phục `Transform.localScale` của Player giữ nguyên `(1, 1, 1)`, loại bỏ triệt để hành vi lật bằng scale âm.
  - **2. Khắc Phục Lỗi Animation & Di Chuyển Player:**
    + Cập nhật [`PlayerController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/PlayerController.cs):
      * Thêm đồng bộ tham số bool `IsMoving` vào Animator và `PlayerVisualSwitcher` (trước đó chỉ cập nhật float `Speed` khiến Animator State Machine không chuyển từ `Idle` sang `Walk`).
      * Lật mặt nhân vật bằng `SpriteRenderer.flipX` thay vì lật ngược `localScale.x`.
      * Gỡ bỏ thành phần dư thừa `SpriteAnimator2D` trên Player để tránh xung đột đè sprite tĩnh lên Animator native.
  - **3. Xây Dựng Hệ Thống Chiến Đấu & Vung Chiêu Thức 2D 360 Độ Dựa Trên Tên Kỹ Năng:**
    + **Tạo [`PlayerAimController2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/PlayerAimController2D.cs):**
      * Xoay tâm ngắm `AimPivot` 360° theo con trỏ chuột trong World Space.
      * Tự động lật `spriteRenderer.flipX` theo hướng chuột.
      * Cơ chế chống lộn ngược vũ khí (chống lật ngược trục khi góc quay $> 90^\circ$ hoặc $< -90^\circ$ bằng cách lật trục Y của AimPivot).
    + **Tạo [`ProjectileBehavior.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/ProjectileBehavior.cs):**
      * Điều khiển quỹ đạo đạn/mũi tên/cầu phép 360° với `Rigidbody2D`, va chạm Trigger và gây sát thương chính xác cho tầng `Enemy` (`Hurtbox2D` / `EnemyStats`).
    + **Tạo [`PlayerSkillCombat2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/PlayerSkillCombat2D.cs):**
      * Đánh thường: Phím `[J]` hoặc `[Chuột Trái]`.
      * Kỹ năng 1, 2, 3: Phím `[K]`, `[L]`, `[O]` (hỗ trợ cả layout `[Q]`, `[E]`, `[R]` hoặc `[1]`, `[2]`, `[3]`).
      * Phân tích tên kỹ năng (String Parsing):
        - Từ khóa [Slash, Strike, Chém, Trảm, Cleave, Slam]: Sinh hiệu ứng `SlashArc_VFX` hình cung 2D quét 360° theo chuột, gây sát thương diện rộng (`Physics2D.OverlapCircleAll`).
        - Từ khóa [Shot, Arrow, Bắn, Tiễn, Snipe]: Phóng mũi tên 360° với `ProjectileBehavior`, hỗ trợ bắn tỏa 3 mũi tên (Tam Liên Tiễn).
        - Từ khóa [Fire, Ice, Magic, Phép, Cầu, Nova, Blast, Trận]: Phóng cầu ma pháp 360° hoặc tạo vòng tròn ma pháp AOE nổ dưới đất tại vị trí chuột.
        - Từ khóa [Heal, Aid, Skin, Giáp, Hồi]: Kích hoạt hồi máu/buff phòng thủ.
  - **4. Sinh Bộ Sprite & Prefab VFX 2D Pixel Art:**
    + Dùng PIL sinh trọn bộ ảnh pixel art chuẩn 16-bit tại [`Assets/Art/VFX/`](file:///d:/Unity/prj1/gam/Assets/Art/VFX/):
      * `vfx_slash_arc.png` ($96 \times 96$): Vệt chém bán nguyệt phát sáng.
      * `vfx_arrow.png` ($48 \times 16$): Mũi tên gỗ bọc sắt và lông vũ.
      * `vfx_magic_orb.png` ($48 \times 48$): Cầu ma pháp năng lượng.
      * `vfx_magic_aoe.png` ($128 \times 128$): Vòng tròn ấn chú ma thuật dưới đất.
      * `vfx_hit_spark.png` ($32 \times 32$): Tia lửa va chạm bạo kích.
    + Tự động đóng gói thành 5 Prefab hoàn chỉnh tại [`Assets/Prefabs/VFX/`](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/).
* **Triển Khai Hệ Thống Modular 2D Equipment & Weapon Socket - Paperdoll (16:15):**
  - **1. Kiến Trúc Paperdoll (Tách Biệt Lớp Hình Ảnh):**
    + Nâng cấp ScriptableObject `EquipmentData.cs` với các thông số hiển thị trực quan: `visualSprite`, `weaponGripOffset` (độ lệch tâm), `weaponRotationAngle` (góc nghỉ).
    + Tạo view `PlayerEquipmentVisualView.cs` quản lý hệ thống phân tầng (Helmet, Armor, Gloves, Boots). Tự động lắng nghe `EventBus.OnEquipmentChanged` kết xuất từ `Inventory`.
  - **2. Cơ Chế Vũ Khí Động (Weapon Socket & Slash Animation):**
    + Phát triển `WeaponSocketView.cs` gắn vào `Weapon_Holder`. Hỗ trợ chuyển đổi nhanh sprite vũ khí.
    + Thêm tính năng chém vũ khí (`PlayAttackSlashAnimation`) bằng Coroutine thông qua việc thao tác Local Rotation, tự động đồng bộ góc xoay theo phương lật (FlipX) của nhân vật.
  - **3. Cầu Nối MVC & Tool Sinh Code Tự Động:**
    + Gắn kết 2 Views vào cầu nối logic `PlayerPresenter.cs`, kích hoạt animation chém ngay khi nhận Input tấn công (Chuột trái hoặc J).
    + Viết Editor tool `SetupModularPlayerVisuals.cs` (Menu: Tools -> Lumina -> Setup Modular Visual Layers) tự động khởi tạo cây GameObject `Body` và các Renderer có sẵn Sorting Order chuẩn xác 100%.

* **Kiểm Toán (Audit) & Tái Cấu Trúc Toàn Diện Item System Architecture (17:30):**
  - **1. Hoàn Thiện Cấu Trúc Scriptable Object:**
    + **`ItemData.cs`**: Chuẩn hóa lớp gốc chứa cơ chế cộng dồn (`isStackable`, `maxStack = 99`).
    + **`EquipmentData.cs`**: Kế thừa ItemData, lưu các thuộc tính Paperdoll.
    + **`ConsumableItemData.cs`**: Kế thừa ItemData, chứa cơ chế `healHpAmount`, `restoreMpAmount`, `cooldownTime`.
  - **2. Bổ Sung Hai Nhánh Thiếu Cực Kỳ Quan Trọng:**
    + **`MaterialItemData.cs`**: Cấu trúc mới hỗ trợ các nguyên liệu rời (`Herb`, `Ore`, `MonsterDrop`, `Fabric`, `Catalyst`) với `maxStack = 999` và `elementalAffinity`.
    + **`CraftingRecipe.cs`**: Cấu trúc Công thức Chế tạo linh hoạt, tổ hợp danh sách thành phần nguyên liệu đầu vào (`ItemData` + số lượng) và thành phẩm đầu ra.
  - Đảm bảo 100% các lớp logic kho đồ (`Inventory.cs`), rơi đồ ngẫu nhiên (`LootDropper.cs`) và UI (`EquipmentUI.cs`) tương thích hoàn toàn, không phát sinh lỗi biên dịch.


* **Dựng Forest Prototype & Demo Map 3 Khu (00:40 - 01:25):**
  - **1. Sửa Lỗi Chặn Biên Dịch:**
    + Lỗi Burst `Byte array for GUID must be exactly 16 bytes long` (cache `Library/BurstCache` hỏng, nguồn từ Unity.InferenceEngine): hướng dẫn tắt `Jobs > Burst > Enable Compilation` hoặc xóa `Library/BurstCache` khi đóng Unity.
    + Hộp thoại "Enter Safe Mode?": nguyên nhân thật là `error CS0308` ở `SetupForestPrototypeScene.cs` (gọi `AssetDatabase.LoadAllAssetsAtPath<Sprite>` — hàm này không có bản generic). Đã sửa `SetSprite()` duyệt mảng `Object[]` và lấy Sprite đầu tiên.
  - **2. Tool Dựng Map Tự Động [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs) (Menu: Tools -> Lumina -> Generate Forest Demo Map):**
    + Tạo scene mới `Assets/Scenes/Forest_Demo_Map.unity` (60x30 ô, X -22..38), không ghi đè scene đang mở.
    + `Grid` gồm `Tilemap_Ground` (Order -10) và `Tilemap_Obstacles` (`TilemapCollider2D` + `CompositeCollider2D` + Rigidbody2D Static, viền tường quanh map). Đại lộ Huyết Mạch là dải đường 2 ô chạy ngang map.
    + Tile 16x16 được cắt tự động từ `tileset.png` (đất, đường, cỏ, tường) ra `Assets/Sprites/ForestPack/Tiles/`, Tile asset lưu tại `Assets/Tilemaps/ForestDemo/`. Không có `map.json` trong project nên dùng tileset.
    + `Zone_01_Village`: Waystone (-18,0), biển chỉ dẫn (-6,-1), cổng làng 2 cột gỗ (X=-5), `PlayerSpawn` (-15,0).
    + `Zone_02_Grassland`: bụi `bush` (-1,2), `bush-tall` (8,-2), `Enemy_Mole` (3,1) và (10,3).
    + `Zone_03_DeepForest`: hàng cây khô chắn ranh giới X=15 (chừa lối đại lộ), 3 cây cam + 3 cây hồng có collider chân cây, thác nước 3 frame, `Enemy_Treant` (28,0).
    + Gắn `Player_Novice_Tier1` (tag Player) vào điểm spawn, Camera dùng `CameraFollow2D` có giới hạn biên map, BGM `ancient_path.ogg` phát lặp.
  - **3. Script Mới:** [`SpriteFrameAnimator.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Environment/SpriteFrameAnimator.cs) (namespace `ActionRPG.Environment`) phát animation lặp cho thác nước.
  - **4. Giới Hạn / Việc Tồn Đọng:**
    + Chưa chạy thử trong Editor, chưa xác nhận biên dịch sạch (Unity chưa recompile sau lần sửa cuối).
    + Chưa tạo Tile Palette; chưa dùng Cinemachine (project chưa cài); chưa dùng `EnemySpawner` vì thiếu asset `EnemyData` (đặt thẳng prefab quái).
    + Props đang dùng Sorting Order cố định (thấp hơn Player) nên cây chưa che Player khi đi phía sau.
    + Tông màu tileset là mùa thu (olive), chưa có xanh lá thuần.

* **Nâng Cấp Va Chạm 2D, Animation 4 Hướng & Phân Khu Map Chuẩn Xác (01:40 - 02:00):**
  - **1. Đại Tu Cơ Chế Va Chạm (Foot Colliders) & Dynamic Y-Sorting:**
    + **Foot Colliders:** Sửa toàn bộ Collider của Player, Quái vật và Cây cối về vị trí chân tiếp đất. Player dùng `CapsuleCollider2D` (size 0.55x0.35, offset -0.4f). Cây cối (`tree-orange`, `tree-pink`, `tree-dried`) chỉ đặt collider ở gốc cây (size 1.2x0.6, offset -1.8f), cho phép Player đi xuyên sau tán cây mà không bị kẹt.
    + **Tạo [`YSortOrder2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/YSortOrder2D.cs):** Tự động tính toán `sortingOrder = -footY * 32` theo thời gian thực (Player/Quái) hoặc lúc khởi tạo (Cây/Đá tĩnh). Giải quyết triệt để vấn đề tầng lớp hiển thị đè trước/sau chuẩn phong cách Zelda Top-Down.
  - **2. Hoạt Ảnh Bước Đi 4 Hướng [`DirectionalWalkAnimator.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/DirectionalWalkAnimator.cs):**
    + Điều khiển tự động hoạt ảnh 4 hướng (Front / Back / Left / Right) dựa theo vận tốc di chuyển hoặc input.
    + Tự động lật mặt (`flipX`), chạy đúng các bộ frame có sẵn trong `ForestPack`:
      * Hero: `hero-walk-front-1..6`, `hero-walk-back-1..6`, `hero-walk-side-1..6`, các pose `hero-idle`.
      * Mole: `mole-walk-front-1..4`, `mole-walk-back-1..4`, `mole-walk-side-1..4`, các pose `mole-idle`.
      * Treant: `treant-walk-front-1..4`, `treant-walk-back-1..4`, `treant-walk-side-1..4`, các pose `treant-idle`.
  - **3. Bổ Sung Các Loài Mới Sẵn Có Trong Pack & Dự Án:**
    + **`Enemy_Mole_Leader` (Chuột Chũi Đầu Đàn - Lv 5):** Kích thước 1.35x, tint đỏ dũng mãnh, HP 220, sát thương 18, trấn giữ chặng cuối Thảo Nguyên.
    + **`Pet_Treant_Sprout` (Búp Cổ Thụ / Tinh Linh Cây Con - Lv 8):** Scale 0.85x, ánh ngọc lục bảo tươi mát, HP 130, phân bố tại vùng đệm rừng sâu.
    + **`NPC_VillageElder` (Trưởng Làng Eldrin):** Đứng cạnh Cổng Làng với script [`NPCVillageElder.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/01_Core_And_Logic/World/Interactables/NPCVillageElder.cs), hỗ trợ tương tác [F] hiển thị hội thoại chỉ đường.
  - **4. Phân Khu Rõ Rệt & Đại Lộ Huyết Mạch (The Grand Highway):**
    + **Đại Lộ Huyết Mạch:** Trục đường lát đá sỏi rộng 3 ô (Y: -6..-4) chạy xuyên suốt từ X: -26 đến X: 40 qua cả 3 phân khu.
    + **Khu 1 - Làng Tân Thủ (X: -26..-7):** Nền đất nện `Tile_Dirt`, Trụ Đá Waystone (-20, 2), hàng rào cọc gỗ bao quanh, Cổng Làng 2 cột trụ gỗ to kèm biển báo (-8.5, -3.2) và NPC Trưởng Làng (-9.5, -2.5).
    + **Khu 2 - Thảo Nguyên Săn (X: -7..16):** Nền cỏ tươi sáng `Tile_Grass`, 3 bụi thu hoạch (`GatheringNode_Bush`), 3 bãi tuần tra Chuột Chũi và Boss Chuột Chũi Đầu Đàn.
    + **Khu 3 - Bìa Rừng Cổ Linh (X: 16..42):** Nền rêu rừng thẫm, Bức Tường Cây Khô ranh giới tự nhiên tại X=16 (chỉ mở đúng khe cửa Đại Lộ), biển cảnh báo nguy hiểm, Thác Nước Động 3 frames tại (36, 6), 2 Búp Cổ Thụ, Đại Cổ Thụ Trùm Rừng Sâu Lv 18 tại (32, -1) và rừng cây cam hồng cổ thụ.
  - **5. Sửa Lỗi Biên Dịch CS0246 (PlayerPresenter):**
    + Thêm `using ActionRPG.Core.Bridges;` vào [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs) để giải quyết lỗi namespace `PlayerPresenter`, đưa dự án về 0 compile error hoàn toàn.

* **Sửa Triệt Để Lỗi Mất Khung Ảnh Nhân Vật (Sorting Order & Sprite Fallback) (02:20):**
  - **1. Nguyên Nhân:**
    + Khi nhân vật di chuyển lên nửa phía Bắc của bản đồ ($Y > 0.7$), công thức $sortingOrder = -footY \times 32$ bị âm sâu (xuống $-32, -100, -300$).
    + Do `Tilemap_Ground` (nền cỏ/đất) có Sorting Order cố định là $-10$, khi nhân vật có Order nhỏ hơn $-10$, **Tilemap nền đất đã vẽ đè lên trên nhân vật**, khiến khung ảnh nhân vật bị chìm/biến mất ở một số khu vực phía Bắc.
  - **2. Khắc Phục:**
    + **Nâng cấp [`YSortOrder2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/YSortOrder2D.cs):**
      * Đặt `baseSortingOffset = 5000`. Giờ đây toàn bộ nhân vật, quái vật, NPC và cây cối luôn có Sorting Order nằm trong dải $[4500 .. 5500]$, luôn cao hơn nền đất hàng ngàn đơn vị.
      * Thêm `syncChildRenderers = true` để tự động đồng bộ Sorting Order cho cả vũ khí, trang bị, bóng râm con gắn trên nhân vật.
    + **Điều chỉnh [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs):**
      * Đặt `OrderGround = 0`, `OrderObstacle = 10`. Nền đất và viền tường luôn ở lớp nền dưới cùng.
    + **Cơ chế Fallback trong [`DirectionalWalkAnimator.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/DirectionalWalkAnimator.cs):**
      * Thêm cơ chế chuyển frame tức thì khi đổi hướng di chuyển, và luôn fallback về `idleFront` nếu gặp frame thiếu, tuyệt đối không để `sr.sprite` bị gán `null`.

* **Loại Bỏ Hoàn Toàn Tích Hợp PixelLab & Dọn Dẹp Môi Trường Python Tooling (08:50):**
  - Đã loại bỏ tệp cấu hình template PixelLab API: .env.example.
  - Đã xóa tệp phụ thuộc Python 
requirements.txt và toàn bộ môi trường ảo .venv (giải phóng dung lượng ổ đĩa, loại bỏ toàn bộ file .py dư thừa).
  - Đã xóa thư mục rỗng Tools/.
  - Đảm bảo dự án Unity tinh gọn, biên dịch 100% sạch (0 Error), không còn bất kỳ liên kết hay phụ thuộc nào vào PixelLab API.

* **Gỡ Bỏ Hoàn Toàn Phân Hệ Web Demo (09:02):**
  - Đã xóa toàn bộ thư mục web-demo/ (HTML, CSS và JavaScript mô phỏng web).
  - Dự án trở về trạng thái tinh gọn 100% thuần Unity Client (C#) kết hợp Backend Server (Node.js/Express + MongoDB).
  - Toàn bộ tài nguyên hình ảnh Map, Pet, Animation và Audio nằm an toàn trong Assets/, kiểm tra biên dịch đạt 0 Warning, 0 Error.

---
* **mô tả map cơ bản:**
  - tạo lại dùm tôi làng tâng thủ là 1 khối hình chữ nhật lớn(gấp 3 lần cái cũ), thu nhỏ bức tường cho giống cổng thành và đá, cây thì xắp xếp lại cho hợp lý hơn sao nữa sẽ làm các tòa nhà, cổng làng nằm ở bên phải sao khi ra cổng làng thì có 1 con đường đi dọc xuống. phía bên phải con đường là 1 thảo nguyên với 1 số quái vật cơ bản cho tân thủ cài những cấp độ đầu tiên, xắp xếp cây, đá và vũng nước hoặc nước cho quái hệ thủy, sẽ cho vào sao. tieps phía bên phải thảo nguyê là khu rừng rậm với quái cấp độ cao không thích hợp cho tân thủ, và khu rừng đấy có 4 con boss với 4 vùng khác nhau gồm, núi lửa, 1 cái hồ lớn, 1 cây cổ thụ lớn và 1 đỉnh núi cao.(đừng xóa)

---

* **Rà Soát Kiến Trúc Công Nghệ & Trạng Thái Cơ Sở Dữ Liệu MongoDB (09:15):**
  - **1. Ngôn ngữ & Công nghệ chính thức của dự án:**
    + **Client Game:** Unity C#, Shader (HLSL/ShaderLab).
    + **Backend Service:** Node.js + Express.js.
    + **Cơ sở dữ liệu:** MongoDB (chạy tại cổng mặc định `27017`).
    + **Tự động hóa / Quản trị hệ điều hành:** PowerShell (Windows).
    + *(Đã loại bỏ hoàn toàn: Web Demo, Python Tooling và PixelLab API, đảm bảo toàn bộ tài nguyên game được bảo toàn nguyên vẹn).*
  - **2. Điểm kết nối MongoDB:**
    + Đã xác minh tại `server/server.js` thông qua hàm `connectDB()` kết nối trực tiếp đến URI `mongodb://127.0.0.1:27017/gam`.
    + Dữ liệu ban đầu đã nạp thành công 42 bản ghi nội dung (Vật phẩm, Kỹ năng, Quái vật) và 1 bản ghi Player Save thử nghiệm.

* **Tái Cấu Trúc & Mở Rộng Toàn Diện Bản Đồ Thế Giới Siêu Lớn (Grand World V2) (09:30 - 10:15):**
  - **1. Kiến Trúc Phạm Vi Tổng Thể Bản Đồ:**
    + Nâng cấp toàn diện bộ sinh map tự động trong [`Assets/Editor/ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs).
    + Kích thước thế giới mở rộng vượt bậc: **160 x 64 ô gạch** (Tọa độ $X \in [-65, 95]$, $Y \in [-32, 32]$), chia thành 4 phân vùng địa lý liền mạch từ Tây sang Đông:
  - **2. Khu 1: Làng Tân Thủ Oakhaven (Hình Chữ Nhật Siêu Lớn - Gấp 3 Lần Cũ):**
    + **Quy mô:** Tọa độ $X \in [-62, -18]$, $Y \in [-18, 18]$ (Kích thước $44 \times 36 = 1584$ ô gạch, gấp gần 3 lần ngôi làng cũ trước đó).
    + **Tường thành & Cổng thành:** Toàn bộ viền bao bọc bởi tường đá thành trì kiên cố (`Tile_CastleWall`), có Collider đặc ngăn ranh giới ngoài.
    + **Cổng Làng Hướng Đông (Bên Phải):** Tọa độ $X = -18, Y \in [-2, 2]$. Hai bên cổng có 2 tháp canh đá sừng sững, biển chỉ dẫn đường ra thảo nguyên.
    + **Quy hoạch 4 Lô Đất Tòa Nhà Tương Lai (Building Plots):** Dựng sẵn móng đá tự nhiên và biển tên phân định rõ ràng:
      * `Plot_Blacksmith`: Lò Rèn Trang Bị ($X = -50, Y = 8$).
      * `Plot_Alchemist`: Tiệm Thuốc Giả Kim ($X = -32, Y = 8$).
      * `Plot_AdventurerGuild`: Hội Mạo Hiểm Giả ($X = -50, Y = -8$).
      * `Plot_GeneralStore`: Cửa Hàng Bách Hóa ($X = -32, Y = -8$).
    + **Cảnh quan & An toàn:** Sắp xếp lại mật độ cây cối, đá phong thủy, hoa cỏ thông thoáng. Đặt Bia Đá Cổ (`Waystone_Monument`) tại $(-37, 0)$, Trưởng Làng (`NPC_VillageElder`) đón khách tại $(-21, 0)$ và vùng an toàn `SafeZoneTrigger` bao phủ toàn bộ lòng làng.
  - **3. Khu 2: Con Đường Huyết Mạch Dọc Xuống Phía Nam (Southbound Highway):**
    + Từ Cổng Làng ($X = -18$), con đường lát đá sỏi chạy ngang nối ra $X = -11$, sau đó bẻ góc chạy dọc thẳng xuống phía Nam ($X \in [-13, -10]$, $Y$ kéo từ $1$ xuống $-28$).
    + Ranh giới tự nhiên phân tách rõ ràng giữa khu vực làng mạc văn minh và vùng thảo nguyên hoang dã.
  - **4. Khu 3: Thảo Nguyên Tân Thủ (Emerald Steppe):**
    + **Vị trí:** Nằm trọn vẹn ở phía **Bên Phải** của con đường dọc ($X \in [-9, 25]$, $Y \in [-28, 15]$).
    + **Bãi quái cơ bản cày cấp:** Chuột Chũi (`Enemy_Mole` Lv 1-3) và Chuột Chũi Thủ Lĩnh (`Enemy_Mole_Leader` Lv 5).
    + **Vũng Nước Thảo Nguyên (Steppe Water Pond):** Tọa độ $X \in [4, 16], Y \in [-18, -8]$. Hồ nước trong lành dùng `Tile_Water`, bao quanh bởi cỏ nước, đá cuội và bố trí sẵn 2 quái hệ Thủy (`Enemy_Water_Slime` Lv 3).
    + **Tài nguyên thu thập:** Phân bổ các bụi thảo dược (`GatheringNode_Bush`) và mỏ quặng thô cho người chơi khai thác ban đầu.
  - **5. Khu 4: Rừng Rậm Nguy Hiểm & 4 Vực Lãnh Địa Boss Cấp Cao (Deep Forest):**
    + Nằm tiếp giáp về phía bên phải Thảo Nguyên (từ $X = 25$ đến $X = 95$), ngăn cách bằng bức tường Cây Khô Cổ Thụ dày đặc (`tree-dried`) cùng biển cảnh báo nguy hiểm cấp cao.
    + Chia thành 4 lãnh địa độc lập ở 4 góc với địa hình và Boss tương ứng:
      * **Góc Tây Nam (SW) - Vùng Núi Lửa (Volcano Territory):** $X \in [30, 56], Y \in [-28, -8]$, nền dung nham đỏ rực `Tile_Lava`, đá cháy xém, trấn giữ bởi Đại Trùm Lửa `Boss_Volcano_DemonApe` (Hỏa Vượn Lv 20).
      * **Góc Đông Nam (SE) - Vùng Hồ Lớn & Thác Nước (Great Lake & Waterfall):** $X \in [64, 90], Y \in [-28, -6]$, mặt nước mênh mông `Tile_Water`, thác nước hoạt ảnh 3-frame sinh động, trấn giữ bởi Thủy Quái Hồ `Boss_Lake_Leviathan` (Leviathan Lv 22).
      * **Góc Tây Bắc (NW) - Vùng Cây Cổ Thụ Ngàn Năm (Giant Ancient Tree Domain):** $X \in [30, 56], Y \in [8, 28]$, thảm cỏ rậm ngàn năm `Tile_DeepGrass`, rừng cây đại thụ rậm rạp, trấn giữ bởi Đại Cổ Thụ `Boss_Treant_Ancient` (Mộc Tinh Thụ Nhân Lv 22).
      * **Góc Đông Bắc (NE) - Vùng Đỉnh Núi Cao (High Mountain Summit):** $X \in [64, 90], Y \in [8, 28]$, vách đá núi dựng đứng `Tile_MountainRock`, đỉnh phong lộng gió, trấn giữ bởi Lang Vương Đỉnh Núi `Boss_Summit_WolfKing` (Lv 25 - Hệ Phong/Lôi).
  - **6. Hệ Thống Tài Nguyên Đồ Họa Mới & Menu Editor:**
    + **Tile Đồ Họa Tự Tạo:** Tạo các mẫu Tile Pixel Art tại `Assets/Sprites/ForestPack/Tiles/` gồm `Tile_CastleWall.png`, `Tile_Water.png`, `Tile_Lava.png`, `Tile_MountainRock.png`, `Tile_DeepGrass.png` kèm các Scriptable Tile `.asset` tương ứng trong `Assets/Tilemaps/ForestDemo/`.
    + **Menu Công Cụ Unity:**
      * `Tools > Lumina > Generate Forest Demo Map`: Sinh bản đồ ra scene độc lập `Assets/Scenes/Forest_Demo_Map.unity`.
      * `Tools > Lumina > Generate World Map to MainGame`: Sinh và ghi đè trực tiếp toàn bộ phân khu vào scene chính `Assets/Scenes/MainGame.unity`.
    + **Tình trạng biên dịch C#:** Kiểm tra toàn diện, 0 Lỗi (0 Error).
  - **7. Danh Mục Tài Nguyên Cần Bổ Sung (Báo Cáo Người Dùng):**
    + *Hiện tại bản đồ đã hoàn thiện 100% về mặt địa hình, va chạm, phân khu quái và 4 Boss.*
    + *Tài nguyên còn thiếu để hoàn thiện mỹ thuật sau này:*
      1. Sprite đồ họa các căn nhà cụ thể (Nhà Lò Rèn, Nhà Giả Kim/Thuốc, Nhà Hội Mạo Hiểm Giả, Cửa Hàng Bách Hóa) để gắn lên 4 móng nền đã quy hoạch.
      2. Sprite cầu gỗ bắc qua suối (nếu muốn làm cầu trang trí).
      3. Sprite khói lửa / bọt dung nham động cho khu vực Núi Lửa.

* **Nâng Cấp & Tinh Chỉnh Mỹ Thuật Toàn Diện Bản Đồ Thế Giới (Visual & Aesthetics Overhaul) (13:35 - 13:55):**
  - **1. Tinh Chỉnh Kích Thước Đá Nhỏ Tinh Tế (Pebbles & Small Stones):**
    + Khắc phục triệt để tình trạng đá thô cứng, quá khổ (trước đây dùng nguyên sprite 31x29px ~ 2x2 ô gạch làm vướng lối đi).
    + Xây dựng hàm [`PlaceRock`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs#L663-L670) với scale mặc định cho đá cuội nhỏ là `0.45f - 0.55f` ($\approx 14 \times 13$ pixel, vừa vặn 1 ô gạch tự nhiên).
    + Tự động thu nhỏ Footprint Collider ở phần chân (`0.75f * scale, 0.4f * scale`) với Y-sorting mượt mà, giúp nhân vật và quái vật không bị vấp va chạm vô lý.
    + Bố trí đá cuội nhỏ tự nhiên dọc mép hồ, lề đường huyết mạch Nam, khuôn viên làng và 4 góc móng nền tòa nhà quy hoạch (`scale = 0.55f`).
  - **2. Phân Cấp Cây Cối Đa Tầng & Các Cụm Rừng Tự Nhiên (Tree Hierarchy & Clusters):**
    + Phân cấp kích thước cây rõ ràng:
      * **Cây Đại Cổ Thụ (Giant Ancient Trees - `scale = 1.4f - 1.65f`):** Tán rộng bóng mát, bố trí tại Quảng trường làng cạnh Waystone (`Village_Plaza_SacredTree`), bao quanh Đấu trường Boss Treant và 2 cây khổng lồ gác Cổng Rừng Sâu.
      * **Cây Trưởng Thành (Standard Trees - `scale = 1.0f - 1.15f`):** Đan xen giữa lá phong cam (`tree-orange`) và hoa anh đào hồng (`tree-pink`).
      * **Cây Con / Cây Non (Saplings - `scale = 0.65f - 0.8f`):** Cây non nhỏ xinh mọc ven các lối dạo và thảo nguyên.
      * **Cây Khô Gai Góc (Dried Trees - `scale = 0.85f - 1.35f`):** Ranh giới rừng tại $X = 25$ được uốn lượn ziczac 2 lớp có chiều sâu, xen kẽ gốc cây rêu mục (`trunk`).
    + Hàm [`PlaceTreeCluster`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs#L681-L698) tạo cụm rừng tự nhiên: 1 cây lớn ở tâm + 1 cây non bên cạnh + 2 bụi cây rậm rạp + 1 viên đá cuội nhỏ nép dưới gốc. Phân bổ tại 4 góc làng Oakhaven và thảo nguyên.
    + Bổ sung thảm thực vật đa dạng với bụi cây tròn xum xuê (`bush`) và lau sậy ven nước (`bush-tall`).
  - **3. Tái Thiết Kế Ao Hồ & Hệ Thống Vũng Nước Tự Nhiên (Organic Ponds & Satellite Wetlands):**
    + Loại bỏ hoàn toàn hình khối chữ nhật thô cứng cũ.
    + **Ao Nước Thảo Nguyên Chính (Organic Steppe Pond):** Ứng dụng thuật toán elip uốn lượn tự nhiên quanh tâm $(10, -13)$ với bán kính $R_x = 5.6, R_y = 4.2$.
    + **Dải Bờ Đất Sỏi (Shoreline Transition):** Viền quanh mép nước được tự động trải 1 dải đất sỏi [`Tile_Dirt`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs#L557) mềm mại, tạo bãi bồi cát sỏi tự nhiên trước khi tiếp giáp cỏ xanh.
    + **2 Vũng Nước Nông Vệ Tinh (Satellite Puddles):** Bổ sung Vũng Nước Tây Bắc $(3.5, -9.0)$ và Vũng Nước Đông Nam $(17.0, -17.0)$, tạo hệ sinh thái đầm nước hoàn chỉnh cho Thủy Slime sinh sống.
    + Hàm [`PlacePondShoreCluster`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs#L703-L710) rải đá cuội nhỏ, lau sậy soi bóng nước ven bờ ao.
    + Đại Hồ Nước Sâu (Boss Leviathan) và Miệng Núi Lửa (Boss Demon Ape) cũng được chuyển sang hình học lòng chảo oval uốn lượn mềm mại.
  - **4. Nâng Cấp Chất Lượng Đồ Họa Tile Pixel Art Chuyên Nghiệp:**
    + Viết thuật toán thủ tục Pixel Art cao cấp, tự động ghi đè và import trực tiếp vào Unity:
      * **[`Tile_Water.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/ForestPack/Tiles/Tile_Water.png):** Nước xanh lam ngọc trong trẻo với các dải gợn sóng ngang lấp lánh (highlights trắng xanh và bóng nước sâu liền mạch).
      * **[`Tile_Lava.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/ForestPack/Tiles/Tile_Lava.png):** Dòng magma đỏ cam rực lửa có lõi vàng và vảy xỉ than bazan đen nổi trên mặt.
      * **[`Tile_MountainRock.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/ForestPack/Tiles/Tile_MountainRock.png):** Phiến đá núi trầm tích góc cạnh 3D sắc nét.
      * **[`Tile_DeepGrass.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/ForestPack/Tiles/Tile_DeepGrass.png):** Thảm rêu rừng cổ thụ sẫm màu điểm xuyết các đốm hoa rừng li ti.
      * **[`Tile_CastleWall.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/ForestPack/Tiles/Tile_CastleWall.png):** Gạch đá thành trì kiên cố so le với mạch vữa rõ nét và cạnh nổi khối 3D.
---

### 🎮 SPRINT 5: HỆ THỐNG PIXEL ART HUD & 5 KỸ NĂNG TÂN THỦ CẤP 1 (Hoàn thành 100% ✅)
- **Thời gian thực hiện:** 2026-10-12 15:00 - 16:25 (Local Time)
- **1. Cân Bằng & Triển Khai 5 Kỹ Năng Tân Thủ Cấp 1 (Tier 1 Novice):**
  + *Ô 1 - Đoạn Phong Trảm (`novice_fan_slash`):* Quét kiếm hình quạt 90° cự ly 2.8m, gây 140% sát thương, đẩy lùi 5.5m (CD 3.0s, tiêu hao 10 MP).
  + *Ô 2 - Loạn Vũ Liên Trảm (`novice_flurry_slash`):* Kích hoạt chuỗi 6 nhịp chém liên tục trong đúng 2.0 giây, dồn DPS cực cao (CD 7.0s, tiêu hao 25 MP).
  + *Ô 3 - Kim Cang Giáp (`novice_aegis_barrier`):* Tăng 15 điểm giáp và kích hoạt 50 điểm Máu Ảo (Barrier) hấp thụ sát thương trong 5.0s (CD 14.0s, tiêu hao 20 MP).
  + *Ô 4 - Tứ Bộ Du Long (`novice_quad_dash`):* Lướt tới 4 đoạn ngắn liên tiếp cách nhau 0.08s với tốc độ 14m/s, né đòn cơ động (CD 10.0s, tiêu hao 15 MP).
  + *Ô 5 - Ánh Sáng Phục Hồi (`novice_holy_heal`):* Kêu gọi ánh sáng hồi phục tức thì 60 HP (CD 22.0s, tiêu hao 35 MP).
- **2. Cơ Chế Máu Ảo (Barrier / Temporary Shield) trong [`CharacterStats.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/CharacterStats.cs):**
  + Biến `temporaryShieldHp` quản lý lượng khiên ảo.
  + Hàm `AddBarrier(float amount, float duration)` cấp khiên và tự tiêu biến khi hết hạn.
  + Trong `TakeDamage()`: Ưu tiên hấp thụ và trừ triệt để vào `temporaryShieldHp` trước khi làm sụt giảm máu thật `currentHp`.
- **3. Bộ Tạo Procedural Pixel Art Asset [`LuminaHUDAssetGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/LuminaHUDAssetGenerator.cs):**
  + Tự động sinh runtime/editor toàn bộ Sprite 16-bit PNG:
    * 5 Skill Icons (32x32): `icon_skill_fan_slash`, `icon_skill_flurry_slash`, `icon_skill_aegis_barrier`, `icon_skill_quad_dash`, `icon_skill_holy_heal`.
    * Khung & Thanh Trượt (Frames): `frame_skill_slot` (9-sliced 4,4,4,4), `frame_hud_bar_bg`, `fill_hp_bar`, `fill_mp_bar`, `fill_exp_bar`, `frame_portrait`, `frame_boss_bar`.
    * Menu Icons (24x24): `icon_backpack`, `icon_character`, `icon_settings`, `icon_demo_class`.
  + Tự động cấu hình `TextureImporter`: Filter Point (no filter), PPU 32, Sprite Mode Single, uncompressed.
- **4. Nâng Cấp Skill Hotbar & Đếm Ngược CD Trên [`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs):**
  + [`SkillSlotUI`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs#L885) hỗ trợ Text đếm ngược từng giây (`cdText`) khi kỹ năng đang hồi chiêu.
  + Hỗ trợ phím nóng `[1]`, `[2]`, `[3]`, `[4]`, `[5]`, `[Q]`.
  + Tự động gắn viền `frame_skill_slot` và thanh nền `frame_hud_bar_bg` theo chuẩn Pixel Art.
- **5. Kiểm Thử Toàn Diện Sprint 5 ([`Sprint5HUDAndNoviceSkillsTest.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/Sprint5HUDAndNoviceSkillsTest.cs)):**
  + Bộ kiểm thử tự động 5 bài: Sprite Assets & Validation, Dữ liệu Novice ScriptableObjects, Máu Ảo Barrier Absorption, Thi triển Combat 2D, Hierarchy GameplayHUD & Skill Slots.
  + Biên dịch: Hoàn hảo 0 lỗi biên dịch (**0 compilation errors**).
- **6. Khắc Phục Triệt Để Sự Cố HUD Chưa Hiển Thị Trên Màn Hình (Hotfix ScreenSpace HUD):**
  + **Nguyên nhân gốc rễ:** Trong Scene `Forest_Demo_Map.unity`, do chưa có Canvas `GameUIRoot` lưu sẵn, cơ chế nạp runtime `RuntimeHUDAutoLoader` đã gọi `Object.FindAnyObjectByType<Canvas>()` và bắt nhầm Canvas WorldSpace nội bộ của Bụi Dược Liệu (`HerbBush`) hoặc Mỏ Quặng (`OreVein`). Hậu quả là `GameplayHUD` bị nhét vào bên trong đối tượng bụi cây ở tọa độ thế giới (kích thước siêu nhỏ), không hiển thị trên màn hình người chơi, đồng thời gây lỗi `MissingReferenceException` trong `ClassDemoSwitcherUI`.
  + **Các bước xử lý dứt điểm:**
    1. [`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs): Tái cấu trúc `RuntimeHUDAutoLoader.EnsureHUDInScene()`. Chỉ nhắm mục tiêu vào GameObject `"GameUIRoot"` hoặc Canvas có `renderMode == RenderMode.ScreenSpaceOverlay`. Nếu chưa có, tự động khởi tạo mới một `GameUIRoot` chuẩn ScreenSpaceOverlay (1920x1080), gắn `GraphicRaycaster` và `EventSystem`.
    2. [`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs#L165): Thêm fallback đa tầng trong `Start()`: nếu chưa có `ClassSystem`, tự động nạp ngay `Class_Novice.asset` để hiển thị lập tức 5 icon kỹ năng Tân Thủ trên HUD.
    3. [`ClassDemoSwitcherUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassDemoSwitcherUI.cs#L650) & [`PauseMenuUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/PauseMenuUI.cs#L148): Chuẩn hóa `EnsureUIHierarchy()`, loại bỏ việc truy cập `transform.parent` sai lệch làm lỗi Transform của đối tượng thế giới.
    4. [`PlayerController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/PlayerController.cs#L78): Tự động `AddComponent<ClassSystem>()` trong `Awake()` nếu Player chưa có.
    5. [`SceneSetupAndConsistencyFixer.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/SceneSetupAndConsistencyFixer.cs#L22): Cung cấp tính năng `FixAllScenes()`, hỗ trợ bake trực tiếp hệ thống Canvas `GameUIRoot` + `GameplayHUD` vào file scene [`Forest_Demo_Map.unity`](file:///d:/Unity/prj1/gam/Assets/Scenes/Forest_Demo_Map.unity) và [`MainGame.unity`](file:///d:/Unity/prj1/gam/Assets/Scenes/MainGame.unity).
    6. Cả 2 project `Assembly-CSharp.csproj` và `Assembly-CSharp-Editor.csproj` biên dịch hoàn hảo với **0 Errors**.

---

### 🎨 SPRINT 5 REMAKE: NÂNG CẤP TOÀN DIỆN ĐỒ HỌA FANTASY RPG HUD & 5 KỸ NĂNG TÂN THỦ (Hoàn thành 100% ✅)
- **Thời gian thực hiện:** 2026-10-12 19:15 - 19:25 (Local Time)
- **Mục tiêu:** Thay thế toàn bộ hình vẽ hộp/canvas đơn sơ trước đó bằng đồ họa Fantasy Action RPG chuyên nghiệp, cực kỳ sắc nét, sang trọng và chuẩn game 2D thương mại, đồng thời bảo toàn trọn vẹn 100% logic của Sprint 5.
- **1. Thiết Kế & Nâng Cấp Toàn Bộ Sprite Assets (Siêu Lấy Mẫu Supersampling 4x):**
  + **5 Skill Icons (64x64):**
    * *Đoạn Phong Trảm ([`icon_skill_fan_slash.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Skills/icon_skill_fan_slash.png)):* Lưỡi kiếm Katana sáng chói vung vệt quạt gió lam ngọc, hiệu ứng hào quang lốc xoáy và đốm sáng năng lượng.
    * *Loạn Vũ Liên Trảm ([`icon_skill_flurry_slash.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Skills/icon_skill_flurry_slash.png)):* Vệt chém đan chéo rực lửa bùng nổ tia lửa cam - đỏ với tâm va chạm phát sáng cực mạnh.
    * *Kim Cang Giáp ([`icon_skill_aegis_barrier.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Skills/icon_skill_aegis_barrier.png)):* Khiên tháp hoàng kim viền ngọc bích ma thuật, hào quang thánh giáp phản chiếu năng lượng hấp thụ đòn đánh.
    * *Tứ Bộ Du Long ([`icon_skill_quad_dash.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Skills/icon_skill_quad_dash.png)):* Đôi giày lông vũ thần tốc với 4 tàn ảnh phân thân chuyển màu xanh lơ lướt gió.
    * *Ánh Sáng Phục Hồi ([`icon_skill_holy_heal.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Skills/icon_skill_holy_heal.png)):* Thánh giá ngọc lục bảo phát quang thiên giới, luồng hào quang hồi phục rạng rỡ và tinh cầu sinh mệnh.
  + **Hệ Thống Khung & Thanh Trạng Thái (Frames & Bars):**
    * *[`frame_skill_slot.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Frames/frame_skill_slot.png):* Khung viền mạ vàng vát góc 3D, đinh tán kim hoàn, nền đá obsidian sẫm (9-sliced 8,8,8,8).
    * *[`frame_portrait.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Frames/frame_portrait.png):* Khung chân dung hoàng gia viền chạm khắc ma thuật gothic đính hồng ngọc ở 4 góc.
    * *[`frame_hud_bar_bg.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Frames/frame_hud_bar_bg.png):* Vỏ bọc thanh rãnh kim loại rèn viền vàng sang trọng.
    * *[`fill_hp_bar.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Frames/fill_hp_bar.png):* Dòng huyết dịch ngọc ruby bóng kính lấp lánh có viền sáng phản chiếu.
    * *[`fill_mp_bar.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Frames/fill_mp_bar.png):* Dòng năng lượng ma thuật lam bảo thạch phát quang rực rỡ.
    * *[`fill_exp_bar.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Frames/fill_exp_bar.png):* Dòng kinh nghiệm ngọc lục bảo mạ ánh sáng tươi tắn.
  + **Menu Icons & Quick Slot Potions:**
    * *[`icon_potion_hp.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Icons/icon_potion_hp.png) & [`icon_potion_mp.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Icons/icon_potion_mp.png):* Bình thuốc thủy tinh cổ điển có nút bần và dung dịch phát quang ma thuật cho phím [E] và [R].
    * *[`icon_demo_class.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Icons/icon_demo_class.png), [`icon_character.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Icons/icon_character.png), [`icon_backpack.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Icons/icon_backpack.png), [`icon_settings.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Icons/icon_settings.png):* Bộ icon menu chi tiết bóng bẩy.
- **2. Tái Thiết Kế Visual Cây Phân Cấp Canvas Trong [`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs):**
  + **Cụm Top-Left Player Info:**
    * Thêm khung chân dung `Portrait_Frame` (`frame_portrait.png`) chứa ảnh `Avatar_Image` nhân vật sắc nét.
    * Tên người chơi màu vàng kim sáng bóng, tag Lv.1 màu hổ phách phong cách RPG.
    * Thanh HP, MP, EXP được lồng khớp hoàn hảo vào khung `frame_hud_bar_bg` với padding viền chuẩn 9-slice và dòng fill bóng kính.
    * Hiển thị trực quan lượng Máu Ảo (Barrier) trên thanh HP khi kích hoạt Kim Cang Giáp: `100 (+50) / 100` với màu xanh băng rực rỡ.
  + **Cụm Bottom-Right Skill Bar (Hotbar):**
    * 6 Slot kỹ năng [1], [2], [3], [4], [5], [Q] dùng khung `frame_skill_slot` 9-slice kích thước 60x60.
    * Tag phím tắt `[1]`, `[2]`... góc trên bên trái màu vàng kim.
    * Tag năng lượng tiêu hao (Mana Cost) góc dưới bên phải màu lam sáng rực rỡ.
    * Vòng quay hồi chiêu `CooldownOverlay` (Radial360) mượt mà cùng số giây đếm ngược `cdText` cỡ lớn ở tâm slot.
  + **Cụm Quick Slots [E] & [R] (Bình Máu & Mana):**
    * Nâng cấp từ các ô vuông phẳng thành lọ bình thủy tinh phát quang sinh động (`icon_potion_hp`, `icon_potion_mp`), hiển thị số lượng và overlay hồi chiêu độc lập 8s.
  + **Cụm Bottom-Left Menu Navigation:**
    * 4 nút Đổi Class [F1], Nhân Vật [U], Túi Đồ [I], Cài Đặt [ESC] được trang bị icon tương ứng bên cạnh chữ.
- **3. Biên Dịch & Đảm Bảo Tính Toàn Vẹn:**
  + Biên dịch sạch: **0 Errors** trên cả `Assembly-CSharp.csproj` và `Assembly-CSharp-Editor.csproj`.
  + Tương thích 100% với toàn bộ 5 bài test trong [`Sprint5HUDAndNoviceSkillsTest.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/Sprint5HUDAndNoviceSkillsTest.cs).
- **4. Tự Động Bake & Cập Nhật Vào Toàn Bộ Scenes & Prefabs ([`BakeNewFantasyHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/BakeNewFantasyHUD.cs)):**
  + Tự động cấu hình Sprite Importer (Bilinear filter, 9-slice borders, single sprite mode).
  + Liên kết biểu tượng hiệp sĩ vào `Class_Novice.asset` và 5 sprite kỹ năng vào `SkillData`.
  + Tạo Prefab chuẩn UI Fantasy tại [`Assets/Prefabs/UI/GameUIRoot.prefab`](file:///d:/Unity/prj1/gam/Assets/Prefabs/UI/GameUIRoot.prefab).
  + Nạp và bake sẵn sàng vào hai scene chính: [`Forest_Demo_Map.unity`](file:///d:/Unity/prj1/gam/Assets/Scenes/Forest_Demo_Map.unity) và [`MainGame.unity`](file:///d:/Unity/prj1/gam/Assets/Scenes/MainGame.unity).

---

### ⚔️ SPRINT 6: CHIÊU THỨC 3 CHỨC NGHIỆP CƠ BẢN, MỞ KHÓA THU PHỤC PET & GIAO DIỆN PET ĐỒNG HÀNH (Hoàn thành 100% ✅)
- **Thời gian thực hiện:** 2026-10-12 19:35 - 20:40 (Local Time)
- **Mục tiêu:**
  1. Triển khai trọn vẹn 15 chiêu thức chiến đấu (5 chiêu/class, phím Q, W, E, R, T) cho 3 chức nghiệp cơ bản: Đấu Sĩ (Warrior), Pháp Sư (Mage), Xạ Thủ (Archer).
  2. Mở khóa cơ chế thu phục Pet (Pet Taming) khi người chơi chuyển chức sang bất kỳ hệ nào trong 3 hệ cơ bản (tặng 5 Quả Cầu Tinh Linh `Spirit Orb`, cho phép thu phục quái < 30% HP bằng phím [C]).
  3. Thêm giao diện Pet Đồng Hành (`PetHUDWidget`) hiển thị Avatar, HP, Hệ nguyên tố, Thanh thân mật (Bond) và phím tắt gọi/thu hồi Pet [P].

- **1. Triển Khai Bộ 15 Kỹ Năng Chuẩn ([`PlayerSkillCombat2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/PlayerSkillCombat2D.cs)):**
  + **Đấu Sĩ (Warrior / Brawler) - Cận chiến, kiểm soát nhịp độ, giáp và phản đòn:**
    * **Q (Chiêu 1) - Trảm Kích (`warrior_cleave`):** Vung vũ khí hình bán nguyệt 180° phía trước cự ly 2.8m, gây sát thương vật lý và đẩy lùi nhẹ quái trúng đòn.
    * **W (Chiêu 2) - Xung Phong (`warrior_vanguard_charge`):** Lao thẳng 5.5m về hướng chỉ định; nếu va chạm kẻ địch đầu tiên sẽ gây choáng 0.75s và đẩy lùi.
    * **E (Chiêu 3) - Thiết Giáp (`warrior_iron_bastion`):** Tạo 60 điểm Giáp Ảo (Barrier) hấp thụ sát thương trong 3s đồng thời giảm 20% sát thương nhận vào trong thời gian này.
    * **R (Chiêu 4) - Địa Chấn (`warrior_tremor_slam`):** Đập mạnh vũ khí nứt đất hình nón 65°, gây sát thương vật lý và làm chậm 40% tốc chạy của kẻ địch trong 2s.
    * **T (Chiêu 5) - Cuồng Nộ Kích (`warrior_frenzied_strike`):** Tích lực thoáng chốc giáng đòn chí mạng uy lực cực đại, sát thương tăng thêm tỉ lệ nghịch với lượng máu hiện tại của bản thân (tối đa +150% dmg khi cận tử).
  + **Pháp Sư (Mage / Elementalist) - Tầm xa, sốc sát thương nguyên tố và khống chế:**
    * **Q (Chiêu 1) - Hỏa Cầu (`mage_fire_orb`):** Phóng quả cầu lửa bay đường thẳng; phát nổ khi chạm mục tiêu gây sát thương phép AoE diện rộng 2.6m.
    * **W (Chiêu 2) - Băng Phong Thược (`mage_frost_spikes`):** Triệu hồi gai băng nhô lên tại vùng chỉ định sau 0.5s delay, gây sát thương phép và trói chân (Root) kẻ địch 1.5s.
    * **E (Chiêu 3) - Thiểm Dịch (`mage_blink`):** Biến ảnh tức thời 5.5m về hướng con trỏ chuột, để lại ảo ảnh gây chậm 35% tốc chạy của kẻ địch xung quanh vị trí cũ trong 2s.
    * **R (Chiêu 4) - Lôi Điện Tỏa (`mage_chain_lightning`):** Phóng tia sét giật vào mục tiêu rồi nảy sang tối đa 3 kẻ địch lân cận, sát thương giảm dần 15% sau mỗi nhịp nảy.
    * **T (Chiêu 5) - Nguyên Tố Hộ Thể (`mage_mana_shield`):** Kích hoạt khiên ma pháp hấp thụ 70 sát thương và chuyển hóa 30% giá trị sát thương nhận vào thành Mana.
  + **Xạ Thủ (Archer / Marksman) - Tầm xa, duy trì khoảng cách (kiting), dứt điểm:**
    * **Q (Chiêu 1) - Xuyên Vân Tiễn (`archer_piercing_shot`):** Bắn phát tiễn uy lực bay xuyên qua mọi kẻ địch trên đường thẳng (`pierceCount = 99`), sát thương vật lý giảm nhẹ qua từng mục tiêu.
    * **W (Chiêu 2) - Mưa Tên (`archer_volley`):** Bắn chùm 5 mũi tên hình nón về phía trước, gây sát thương và làm chậm đối thủ 30% trong 1.5s để giữ cự ly.
    * **E (Chiêu 3) - Nhào Lộn Thoát Thân (`archer_tumble`):** Lộn nhào 4.5m về hướng chỉ định, cường hóa tăng tầm đánh và tốc đánh cho đòn đánh tiếp theo trong 3s.
    * **R (Chiêu 4) - Bẫy Dây (`archer_snare_trap`):** Đặt bẫy tàng hình tại vị trí chỉ định; kích nổ gây sát thương, làm lộ diện và trói chân mục tiêu 1.25s khi kẻ địch giẫm phải.
    * **T (Chiêu 5) - Ngắm Chuẩn Dứt Điểm (`archer_deadeye`):** Khóa mục tiêu đơn lẻ 1s để khai hỏa đòn bắn cực đại, sát thương tăng tỉ lệ thuận theo lượng máu đã mất của quái.

- **2. Bổ Sung Cơ Chế Debuff Cho AI Quái Vật ([`EnemyController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Enemies/EnemyController.cs)):**
  + Tích hợp đầy đủ các hàm xử lý trạng thái bất lợi: `ApplyStun(duration)`, `ApplySlow(percent, duration)`, `ApplyRoot(duration)`.
  + Tự động khóa di chuyển / giảm tốc trong cả hai trạng thái Tuần Tra (`ExecutePatrol`) và Truy Đuổi (`ExecuteChase`), kèm hiệu ứng màu sắc phản hồi trực quan.

- **3. Cơ Chế Mở Khóa Thu Phục Pet Khi Chuyển Chức ([`ClassSystem.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/ClassSystem.cs)):**
  + Trong hàm `PromoteTo()`: Khi chuyển sang bất kỳ class nào trong `Warrior`, `Mage`, `Archer`, tự động kích hoạt `stats.isPetUnlocked = true`.
  + Tặng ngay cho người chơi **5 Quả Cầu Tinh Linh (`item_spirit_orb`)** vào túi đồ để bắt đầu hành trình thu phục Pet.
  + Phát sự kiện toàn cục `EventBus.OnPetSystemUnlocked` và hiển thị Toast thông báo chúc mừng trên màn hình.
  + [`PlayerPetRoster.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Pets/PlayerPetRoster.cs) & [`PetController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Pets/PetController.cs): Cập nhật điều kiện mở khóa `isPetUnlocked || level >= 5`, đổi phím gọi/thu hồi Pet từ [T] sang **[P]** (để phím [T] dành riêng cho Kỹ năng 5).
  + Đổi phím tương tác NPC từ [E] sang **[F]** trong [`PlayerController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Characters/PlayerController.cs) để giải phóng [E] cho Kỹ năng 3.

- **4. Giao Diện Pet Đồng Hành (`PetHUDWidget`) & Cập Nhật HUD ([`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs) & [`PetHUDWidget.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/PetHUDWidget.cs)):**
  + Widget Pet Đồng Hành được đặt trang nhã ở góc trên bên trái màn hình (ngay dưới cụm HP/MP nhân vật).
  + Hiển thị: Avatar Pet trong khung viền kim loại, Tên Pet, Level, Biểu tượng Hệ Nguyên Tố, Thanh HP Pet, Thanh Điểm Thân Mật (Bond Bar), Nút bấm gọi/thu hồi phím tắt **[P]**.
  + Tự động ẩn khi người chơi chưa mở khóa Pet hoặc chưa có Pet trong đội hình.
  + Skill Hotbar cập nhật nhãn phím hiển thị rõ ràng: **[Q]**, **[W]**, **[E]**, **[R]**, **[T]**.

- **5. Tạo Sprite Mới & Tự Động Bake Vào Database ([`LuminaClassAssetDatabase.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/LuminaClassAssetDatabase.cs) & [`BakeNewFantasyHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/BakeNewFantasyHUD.cs)):**
  + Sinh và cấu hình 15 Icon kỹ năng chi tiết và 2 Icon Pet: `icon_spirit_orb.png`, `icon_pet_wolf.png`.
  + Tự động đồng bộ vào toàn bộ ScriptableObjects `SkillData` và `CharacterClassData` tại `Assets/ScriptableObjects/`.
  + Bake thành công vào prefab `GameUIRoot.prefab` và 2 scene `Forest_Demo_Map.unity`, `MainGame.unity`.
  + Biên dịch sạch: **0 Errors**, 0 lỗi logic.

---

### 🖋️ SPRINT 7: CHUẨN HÓA FONT CHỮ DỰ ÁN SANG TIMES NEW ROMAN (Hoàn thành 100% ✅)
- **Thời gian thực hiện:** 2026-10-12 22:10 - 22:25 (Local Time)
- **Mục tiêu:** Chuyển đổi toàn bộ phông chữ của dự án sang **Times New Roman** chuẩn Serif để tăng tính trang trọng, rõ ràng, dễ đọc và hiển thị hoàn hảo tiếng Việt có dấu.
- **1. Nhập & Cấu Hình Font TrueType ([`TimesNewRoman.ttf`](file:///d:/Unity/prj1/gam/Assets/Fonts/TimesNewRoman.ttf)):**
  + Trích xuất và nhập cả 2 biến thể: `TimesNewRoman.ttf` (Regular) và `TimesNewRoman_Bold.ttf` (Bold) vào `Assets/Fonts/` và `Assets/Resources/Fonts/`.
  + Cấu hình metadata `TrueTypeFontImporter` nhận diện tên font `Times New Roman`.
- **2. Bộ Quản Lý Font Hệ Thống Runtime ([`FontManager.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Core/FontManager.cs)):**
  + Cung cấp hàm nạp tự động `FontManager.GetTimesNewRomanTMP()` cho TextMeshPro và `FontManager.GetTimesNewRomanLegacy()` cho Text cổ điển.
  + Hỗ trợ `ApplyFont(TMP_Text)` và `ApplyFont(Text)` giúp toàn bộ component UI tự động chuyển font nhanh chóng.
- **3. Tích Hợp Vào Toàn Bộ Hệ Thống UI & HUD:**
  + [`GameplayHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs): Tự động duyệt và áp dụng font Times New Roman cho toàn bộ text (HP, MP, EXP, Level, Tên nhân vật, Slot phím tắt, Cooldown đếm ngược...).
  + [`ClassDemoSwitcherUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassDemoSwitcherUI.cs): Áp dụng Times New Roman cho danh sách 14 Class, 3 Ấn nội tại, chỉ số và danh sách kỹ năng [Q, W, E, R, T].
  + [`PetHUDWidget.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/PetHUDWidget.cs): Áp dụng Times New Roman cho Tên Pet, Level, Hệ nguyên tố, Thanh máu và Điểm thân mật.
  + [`PauseMenuUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/PauseMenuUI.cs), [`ClassSelectionUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/ClassSelectionUI.cs), [`InventoryUI.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/InventoryUI.cs), [`HUDController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/HUDController.cs).
- **4. Công Cụ Editor Đồng Bộ Tự Động ([`TimesNewRomanFontSetup.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/TimesNewRomanFontSetup.cs)):**
  + Tự động tạo `TMP_FontAsset` chế độ **Dynamic (AtlasPopulationMode.Dynamic)** giải quyết triệt để lỗi thiếu glyph khi hiển thị tiếng Việt có dấu.
  + Cập nhật `TMP Settings.asset` đặt Times New Roman làm Font Asset mặc định và đầu danh sách Fallback.
  + Menu thao tác một chạm: `Tools -> Lumina -> Apply Times New Roman Font To Whole Project`.
  + Tích hợp trực tiếp vào quy trình bake của [`BakeNewFantasyHUD.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/BakeNewFantasyHUD.cs).
- **5. Kiểm Tra Biên Dịch:**
  + `dotnet build Assembly-CSharp.csproj`: **0 Errors**.
  + `dotnet build Assembly-CSharp-Editor.csproj`: **0 Errors**.



