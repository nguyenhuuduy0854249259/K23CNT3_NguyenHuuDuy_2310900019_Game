# KỊCH BẢN GAME HỒI 1 & NHẬT KÝ CÔNG VIỆC DỰ ÁN LUMINA

> **Tài liệu tham chiếu cốt truyện:** `kich-ban.md` (Phần I & Phần II: Hồi 1 – Lời Thề Tân Binh)  
> **Phạm vi trọng tâm:** Hồi 1 (Cấp độ 1 – 9: Làng Khởi Nguyên Oakhaven & Thảo Nguyên Xanh)  
> **Nguyên tắc làm việc:** Toàn bộ kịch bản chi tiết, lộ trình công việc kỹ thuật (Task Breakdown), danh sách đầu việc cần làm (Checklist), và nhật ký tiến độ (Work Log) từ nay sẽ được quản lý và ghi nhận trực tiếp tại file này.

---

## PHẦN I. CỐT TRUYỆN CHI TIẾT & ĐẶC TẢ THIẾT KẾ HỒI 1

### 1. Thông Tin Tổng Quan Hồi 1
* **Tên hồi:** Hồi 1 – LỜI THỀ TÂN BINH (The Novice's Oath)
* **Khung cấp độ:** Cấp 1 – 9
* **Khu vực hoạt động:**
  * **Làng Oakhaven:** Khu vực an toàn (SafeZone) – Không cho phép rút vũ khí hay tấn công NPC.
  * **Thảo Nguyên Xanh (Green Steppes):** Vùng dã ngoại nguy hiểm ngoài cổng làng. Nguồn nước bị ô nhiễm Tinh Tố bởi Vết Nứt Hắc Ám (*The Umbral Rift*).
* **Mục tiêu cốt lõi:**
  1. Thức tỉnh sau biến cố ma khí, làm quen bối cảnh thế giới Lumina và mảnh phù văn bí ẩn.
  2. Nắm vững cụm điều khiển cơ bản: Di chuyển 8 hướng (WASD/phím mũi tên), ngắm đánh 360° theo chuột, đòn đánh cơ bản (Chuột trái / J).
  3. Trải nghiệm hệ thống trang bị trực quan theo thời gian thực (Paperdoll Modular / `WeaponSocketView`).
  4. Thu thập thảo dược/khoáng thạch dã ngoại (`GatheringNode`), nhặt tài nguyên tự động (`Loot Magnet`).
  5. Đối đầu quái vật sơ cấp: Slime Nước (Lv.1) và Chuột Chũi Đất (Lv.3).
  6. Rèn luyện đạt Cấp 10 để mở khóa Lễ Khảo Hạch Thợ Săn (chuyển sang Hồi 2: chọn 3 nhánh cơ bản Chiến Binh / Xạ Thủ / Pháp Sư và nhận Quả Cầu Tinh Linh thu phục Pet).

---

### 2. Tiến Trình Nhiệm Vụ & Tuyến Nhân Vật (Hồi 1)

#### 🔹 Cảnh Mở Đầu (Prologue Cutscene / Intro)
* Màn hình mờ dần (Fade In). Ánh nắng chiếu vào phòng xá nhỏ tại Làng Oakhaven.
* Nhân vật chính tỉnh lại trên giường bệnh, mặc quần áo vải thô, mất trí nhớ, trong tay cầm một mảnh phù văn cổ vỡ vụn phát sáng mờ nhạt.
* **NPC Dược Sĩ Elena:** Chăm sóc và giải thích tình hình: Đội tuần tra tìm thấy nhân vật ngất lịm ở bìa rừng khi ma khí từ Vết Nứt Hắc Ám tràn xuống. Khuyên nhân vật tìm Hội Trưởng Carlos để gia nhập Hiệp Hội Thợ Săn.

#### 🔹 Nhiệm Vụ 1: Lời Chào Của Hiệp Hội Thợ Săn
* **NPC giao:** Dược Sĩ Elena  
* **NPC nhận:** Hội Trưởng Carlos (Trụ sở Hiệp Hội Thợ Săn Oakhaven)  
* **Mục tiêu:** Di chuyển theo bảng chỉ dẫn / HUD đến gặp Carlos.  
* **Phần thưởng:** 50 EXP, 20 Vàng, Vũ khí khởi đầu: **Kiếm Sắt Rỉ** (`Starter_Sword.asset`).  
* **Cơ chế giới thiệu:**
  * Bấm phím mở Túi đồ & Trang bị.
  * Kéo Kiếm Sắt Rỉ vào ô Weapon.
  * Paperdoll kích hoạt: Thanh kiếm hiển thị ngay trên tay nhân vật thông qua `WeaponSocketView`.

#### 🔹 Nhiệm Vụ 2: Dọn Dẹp Hồ Nước Thảo Nguyên
* **NPC giao:** Hội Trưởng Carlos  
* **Mục tiêu:**
  * Bước qua cổng làng ra Thảo Nguyên Xanh (thoát khỏi SafeZone, nhạc nền đổi sang dã ngoại, rút vũ khí sẵn sàng chiến đấu).
  * Tiêu diệt **5 Slime Nước (Cấp 1)** quanh hồ.
  * Tiêu diệt **3 Chuột Chũi Đất (Cấp 3)** gần bìa rừng.
  * Quay về báo cáo Carlos.  
* **Phần thưởng:** 150 EXP, 80 Vàng, 3 Bình Máu Nhỏ (`potion_small_hp`).  
* **Cơ chế giới thiệu:**
  * Chiến đấu đánh thường thời gian thực 360°, hit flash nháy đỏ khi quái trúng đòn.
  * Hiệu ứng khói pixel khi quái chết (`enemy-death.png`).
  * Đồng xu văng ra và hút tự động về phía người chơi trong bán kính 2.5m (Loot Magnet).

#### 🔹 Nhiệm Vụ 3: Bài Học Của Dược Sư (Thu Thập Thảo Mộc)
* **NPC giao:** Dược Sĩ Elena  
* **Mục tiêu:**
  * Đến các bụi cây thảo dược ngoài thảo nguyên (`GatheringNode_Bush`).
  * Nhấn giữ/tương tác phím `F` (thời gian thu hoạch 1.0 giây).
  * Thu thập **3 Bó Thảo Mộc Xanh**.
  * Giao lại cho Elena.  
* **Phần thưởng:** 100 EXP, 50 Vàng, 5 Bình Máu Nhỏ, Mở khóa công thức chế thuốc sơ cấp.  
* **Cơ chế giới thiệu:**
  * Hệ thống khai thác tài nguyên sống (`GatheringNode`), thanh tiến trình thu hoạch (Harvest Channeling bar).

#### 🔹 Nhiệm Vụ 4: Lớp Phòng Ngự Đầu Tiên
* **NPC giao:** Thợ Rèn Boris  
* **Mục tiêu:** Dùng da thú thu được từ quái vật thảo nguyên để Boris gia công bộ giáp cơ bản.  
* **Phần thưởng:** 120 EXP, **Giáp Da Thô** (`armor_leather_novice`).  
* **Cơ chế giới thiệu:**
  * Trang bị áo giáp thay đổi diện mạo nhân vật trên hệ thống Paperdoll (`PlayerEquipmentVisualView`).
  * Chỉ số Giáp (DEF) tăng lên, giảm sát thương nhận vào từ quái vật.

#### 🔹 Cuộc Gặp Với Quản Thú Kaelen & Khép Lại Hồi 1
* Nhân vật gặp **Quản Thú Kaelen**: Được hé lộ về nghệ thuật giao cảm với linh thú Tinh Tố.
* Kaelen cảnh báo: *"Chưa thấu hiểu nhịp thở của tự nhiên thì chớ vội bắt Pet. Hãy rèn luyện đạt Cấp 10 rồi trở lại tìm ta!"*
* Khi người chơi tích lũy đủ EXP đạt **Cấp 10**:
  * Chuông thông báo vang lên: *"Bạn đã đủ điều kiện tham gia Lễ Khảo Hạch Thợ Săn!"*
  * Hoàn thành Hồi 1, chuẩn bị bước vào Hồi 2 (Chọn Class & Nhận Quả Cầu Tinh Linh).

---

## PHẦN II. KỊCH BẢN CÔNG VIỆC CẦN LÀM CHO DỰ ÁN (TASK BREAKDOWN)

> ⚠️ **ĐIỀU CHỈNH CHIẾN LƯỢC MỚI:** Tạm hoãn lập trình nhiệm vụ/cốt truyện. **Ưu tiên số 1:** Thiết kế hoàn thiện toàn bộ hệ thống hình ảnh Pixel Art (Spritesheet/Icon) cho Vũ khí, Trang bị (Mũ, Áo giáp, Găng, Giày), Vật phẩm tiêu hao, Nguyên liệu thu thập, và Bộ công trình kiến trúc Làng Oakhaven (như Lò Rèn của Boris). Sau khi hoàn tất kho tài nguyên hình ảnh sẽ tiến hành ghép vào bản đồ và triển khai cốt truyện/nhiệm vụ.

### 🎨 Giai đoạn 1: Thiết Kế & Chuẩn Hóa Toàn Bộ Đồ Họa Trang Bị & Vật Phẩm Hồi 1
- [x] **Task 1.1: Bộ Icon Vật Phẩm Túi Đồ & Trang Bị (Inventory Icons):**
  - Đã trích xuất & làm trong suốt trực tiếp từ bản vẽ thiết kế chuẩn của người dùng:
    - **Vũ khí:** Kiếm Sắt Rỉ (`icon_weapon_starter_sword.png`).
    - **Trang bị:** Giáp Da Thô (`icon_armor_leather_novice.png`), Mũ Da Thợ Săn (`icon_helmet_leather_novice.png`), Găng Da Bền (`icon_gloves_leather_novice.png`), Ủng Da Đi Rừng (`icon_boots_leather_novice.png`).
    - **Vật phẩm tiêu hao & Thu thập:** Bình Máu Nhỏ (`icon_potion_small_hp.png`), Bình Mana Nhỏ (`icon_potion_small_mp.png`), Thảo Mộc Xanh (`icon_material_herb_leaf.png`), Quặng Đồng Thô (`icon_material_ore_chunk.png`), Quặng Sắt Thô, Da Thú Rừng (`icon_material_animal_hide.png`), Mảnh Phù Văn Vỡ (`icon_quest_broken_rune.png`).
  - *Tự động sinh toàn bộ ScriptableObject `ItemData` & `EquipmentData` qua `Act1ItemCreator.cs`.*

- [x] **Task 1.2: Thiết kế Bãi Thu Thập Dã Ngoại Dưới Đất (Ground Gathering Nodes):**
  - Đã trích xuất và tích hợp 3 bãi tài nguyên cắm rải rác trên Thảo Nguyên:
    - Bụi Thảo Mộc Xanh nở hoa trắng trên nền đất (`ground_herb_green_leaf.png`).
    - Bãi Quặng Đồng Thô sắc đỏ đồng (`ground_ore_copper_chunk.png`).
    - Bãi Quặng Sắt Thô vách đá ánh xám kim (`ground_ore_iron_chunk.png`).
  - Tích hợp vào Prefab `GatheringNode_Bush`, `GatheringNode_CopperOre`, `GatheringNode_Ore` trong [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs).

- [ ] **Task 1.3: Cấu Trúc Hệ Thống Ngoại Trang (Costume & Skin System) Riêng Biệt:**
  - **Quy tắc thiết kế mới:** Trang bị của Class (Giáp, Mũ, Găng, Giày) **chỉ cộng chỉ số (Stats/Attributes)**, không thay đổi sprite trực tiếp trên người để tối ưu hiệu năng và linh hoạt thiết kế.
  - Ngoại hình trang phục bên ngoài của nhân vật được tách thành **Hệ thống Ngoại Trang / Skin (Costume System)** riêng (ví dụ: Trang phục Thợ Săn, Trang phục Tân Thủ, Đồ Lễ Hội...).
  - Riêng **Vũ Khí** vẫn gắn trên tay nhân vật thông qua `WeaponSocketView` để xoay và vung chém theo hướng ngắm.

- [ ] **Task 1.4: Quy Hoạch Tài Nguyên Skins Vũ Khí từ Gói `RPG_skins_V1.1` (Backlog - Để Làm Sau Này):**
  - **Mục tiêu:** Sử dụng bộ tài nguyên pixel chuẩn 32x32 tại [Assets/RPG_skins_V1.1](file:///d:/Unity/prj1/gam/Assets/RPG_skins_V1.1/) làm nguồn dữ liệu chính cho hệ thống Ngoại Trang Vũ Khí (Weapon Skins).
  - **Phân loại kho vũ khí 32x32 (hơn 224+ mẫu sẵn có):**
    - **Kiếm (Swords - 88 mẫu):** Dành cho Chiến Binh / Tân Thủ / Kiếm sĩ.
    - **Gậy phép / Pháp trượng (Staffs - 40 mẫu):** Dành cho Pháp Sư.
    - **Rìu chiến (Axes - 40 mẫu):** Dành cho Đấu sĩ / Berserker.
    - **Đinh ba / Thương dài (Tridents - 24 mẫu):** Dành cho Đấu sĩ tầm xa.
    - **Súng cao su (Slingshots - 16 mẫu) & Quạt chiến (Fans - 16 mẫu):** Vũ khí phụ / Biến thể đặc thù.
    - **Tài nguyên bổ trợ:** 48 mẫu Nhẫn/Dây chuyền và 24 mẫu Bình dược phẩm (hỗ trợ trang sức & tiêu hao sau này).
  - **Chiến lược:** Tạm lưu trữ vào Backlog để làm sau này, tập trung hoàn thiện các tính năng cốt lõi của Hồi 1 trước; sẽ tích hợp vào Tủ Đồ Ngoại Trang (Wardrobe/Transmog System) hoặc Cửa Hàng Skin ở giai đoạn sau.

### 🏛️ Giai đoạn 2: Tích Hợp Đồ Họa Công Trình & Kiến Trúc Làng Oakhaven
- [x] **Task 2.1: Tích hợp Spritesheet Lò Rèn của Boris (Blacksmith Forge):**
  - Đã nạp Spritesheet Lò Rèn của người dùng vào `Assets/Sprites/Buildings/blacksmith_forge_sheet.png`.
  - Cắt thành 8 frame animation khói bốc nghi ngút & lửa than bập bùng tại `Assets/Sprites/Buildings/ForgeFrames/`.
  - Tạo cấu hình Prefab `Building_Blacksmith_Forge` tích hợp `SpriteFrameAnimator` (6 FPS loop), `BoxCollider2D` chân tường và `YSortOrder2D` (chiều sâu isometric).
- [x] **Task 2.2: Quy hoạch mặt bằng Kiến Trúc Làng Oakhaven:**
  - Quy hoạch Lò Rèn Barnaby tại tọa độ Tây Bắc làng (`X: -49, Y: 9.5`), kèm NPC Thợ Rèn Barnaby đứng trước đe rèn (`X: -47.2, Y: 7.8`).
  - Bố trí các khu đất quy hoạch: Dược Quán Elena (`X: -49, Y: -9`), Hội Thám Hiểm (`X: -27, Y: 9`), Cửa Tiệm Tạp Hóa (`X: -27, Y: -9`).
  - Bố trí cổng làng phía Đông (`X: -18, Y: 0`) có Trưởng Làng Eldrin và đường dẫn ra Thảo Nguyên Xanh.

### ⚔️ Giai đoạn 3: Hệ Thống Hoạt Họa 4 Hướng (Characters & Weapons) & Dựng Khung Hình 15 Kỹ Năng Pixel (VFX & Combat)
- [x] **Task 3.1: Hoạt họa nhân vật 3 Class cơ bản 4 hướng (Character 4-Direction Spritesheet):**
  - Đã thiết kế và dựng chuẩn **42 khung hình (frames)** cho mỗi Class nhân vật:
    - **Chiến Binh (Warrior):** [`player_warrior_4direction.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/player_warrior_4direction.png) (42 frames)
    - **Pháp Sư (Mage):** [`player_mage_4direction.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/player_mage_4direction.png) (42 frames)
    - **Xạ Thủ (Archer):** [`player_archer_4direction.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/player_archer_4direction.png) (42 frames)
  - **Trích dẫn chi tiết phân bổ khung hình 4 hướng (Down, Up, Right, Left):**
    - **Idle (Đứng thở - 2 frames/hướng):** `_idle_<dir>_0..1` (cơ thể nhấp nhô 1-2px, chu kỳ 4 FPS).
    - **Walk (Bước đi - 6 frames/hướng):** `_walk_<dir>_0..5` (chân bước so le, vung tay nhịp nhàng theo chu kỳ bước đi, 8 FPS).
    - **Attack (Đánh thường - 3 frames/hướng):** `_attack_<dir>_0..2` (f0: vung lùi lấy đà -> f1: vung chém/bắn hết lực -> f2: thu đà kết thúc, 12 FPS).
    - **Cast (Tung chiêu/Niệm chú - 2 frames/hướng):** `_cast_<dir>_0..1` (f0: giơ tay/vũ khí tụ năng lượng -> f1: phóng xuất kỹ năng theo hướng ngắm chuột, 10 FPS).
  - Trích dẫn mã nguồn điều khiển: [`LuminaFourDirectionCharacterAnimator.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/LuminaFourDirectionCharacterAnimator.cs#L42-L47) quản lý mảng 2 chiều `idleFrames[4][]`, `walkFrames[4][]`, `attackFrames[4][]`, `castFrames[4][]`.

- [x] **Task 3.2: Bộ hình ảnh vũ khí 4 hướng cầm tay không bị lệch tâm & mất chi tiết:**
  - Khắc phục triệt để lỗi lật trục âm (`scale.x = -1`) gây ngược bóng và lệch vị trí tay bằng cách vẽ riêng biệt 12 sprite chuẩn 32x32 pixel:
    - **Chiến Binh (Kiếm Thép):** [`weapon_warrior_sword_down.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_down.png), [`_up.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_up.png), [`_right.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_right.png), [`_left.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_left.png).
    - **Pháp Sư (Gậy Phép):** [`weapon_mage_staff_down.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_down.png), [`_up.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_up.png), [`_right.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_right.png), [`_left.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_left.png).
    - **Xạ Thủ (Cung Tên):** [`weapon_archer_bow_down.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_down.png), [`_up.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_up.png), [`_right.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_right.png), [`_left.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_left.png).
  - Trích dẫn mã nguồn sắp lớp: [``WeaponSocketView`.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Presentation/Views/`WeaponSocketView`.cs#L88-L105) tự động gán sorting order: hướng Up hiển thị sau lưng (`bodySortingOrder - 1`), hướng Down/Right/Left hiển thị phía trước (`bodySortingOrder + 2`).

- [x] **Task 3.3: Dựng toàn bộ khung hình hoạt họa (Animation Frames) & Prefab cho 15 kỹ năng của 3 Class:**
  - Hoàn tất 15 Sprite Sheet pixel VFX với tổng cộng **82 khung hình độc lập**.
  - Cắt lát Sprite (Multiple Sprite Mode) đồng bộ GUID và FileID với Texture Importer.
  - Đóng gói 16 Prefab hiệu ứng hoạt họa vận hành bởi [`SimpleFrameAnimator2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs#L10-L24).
  - Liên kết toàn diện vào 15 ScriptableObject [`SkillData`](file:///d:/Unity/prj1/gam/Assets/ScriptableObjects/Skills/) và cơ chế tung chiêu thời gian thực [`PlayerSkillCombat2D.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/PlayerSkillCombat2D.cs#L225-L270).

- [x] **Task 3.4: Tự động hiển thị thanh máu (HP Bar) & Cấp độ (Lv.) trên đầu toàn bộ quái vật (EnemyOverheadUI):**
  - Tự động tính toán chiều cao quái vật dựa trên `SpriteRenderer.bounds.extents.y + 0.35f` đặt thanh máu trên đỉnh đầu.
  - Tạo Canvas con `RenderMode.WorldSpace` với `sortingOrder = 500` không bao giờ bị che khuất bởi mặt đất hoặc cây cối.
  - Hiển thị nhãn Level & Tên quái vật `"Lv.X [Tên Quái]"` (ví dụ: `"Lv.3 Sói Xám"`, `"Lv.1 Bù Nhìn Luyện Võ"`) màu vàng sáng viền đen `Outline` sắc nét.
  - Khung nền đen mờ (`90x12`), thanh máu đỏ co giãn theo tỷ lệ `currentHp / maxHp` kèm hiệu ứng `Damage Ease Lag` rút từ từ.
  - Khóa xoay `Quaternion.identity` và triệt tiêu lật ngược tỷ lệ (`0.01f * Mathf.Sign(scale.x)`) trong `LateUpdate()`.
  - Tự động gắn vào quái trong `Start()` / `OnEnable()` của `EnemyStats` & `EnemyController`, kèm Menu Editor `Tools -> Lumina -> Attach Overhead HP Bar To All Enemies`.


---

## PHẦN III. NHẬT KÝ CÔNG VIỆC (WORK LOG)

*(Toàn bộ hoạt động thực hiện cho dự án từ thời điểm này sẽ được ghi nhận chi tiết tại đây)*

### 📅 Ngày 2026-10-13

#### 🟢 Ca làm việc 1: Khởi tạo Kịch bản Hồi 1 & Thiết lập Quy trình mới
* **Thời gian:** 01:15 (Giờ hệ thống)
* **Nội dung công việc thực hiện:**
  1. Khảo sát tài liệu `kich-ban.md`, trích xuất toàn bộ cốt truyện và cơ chế Hồi 1.
  2. Tạo lập tài liệu trung tâm `kich-ban-game-hoi-1.md`.
  3. Thống nhất quy ước ghi chú tập trung vào file này.

#### 🟢 Ca làm việc 2: Điều chỉnh lộ trình - Ưu tiên Thiết kế Đồ họa (Assets & Art First) & Quy hoạch Lò Rèn
* **Thời gian:** 03:00 (Giờ hệ thống)
* **Quyết định điều chỉnh:**
  1. Tạm hoãn khâu lập trình cốt truyện/nhiệm vụ.
  2. Tập trung tối đa vào thiết kế tất cả hình ảnh liên quan đến vũ khí (Kiếm Sắt Rỉ), giáp (Giáp Da Thô), mũ, găng, giày, vật phẩm (bình máu, mana, thảo dược, quặng, phù văn cổ) của Hồi 1.
  3. Tiếp nhận hình ảnh Spritesheet **Lò Rèn Hoạt Họa 8 Frame (Blacksmith Forge)** từ người dùng. Lên phương án cắt sprite, tạo animation khói bốc/lửa rực, đóng gói Prefab có Collider móng và cắm vào tọa độ quy hoạch Làng Oakhaven.

#### 🟢 Ca làm việc 3: Triển khai Đồ Họa Vật Phẩm Hồi 1 & Tích hợp Lò Rèn Làng Oakhaven
* **Thời gian:** 03:15 (Giờ hệ thống)
* **Nội dung đã hoàn thành:**
  1. **Tích hợp Tòa Nhà Lò Rèn (Blacksmith Forge):**
     * Đã nhập tấm ảnh spritesheet 8-frame lò rèn người dùng cung cấp vào dự án: [`Assets/Sprites/Buildings/blacksmith_forge_sheet.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Buildings/blacksmith_forge_sheet.png).
     * Đã cắt chính xác 8 frame hoạt ảnh độc lập (kích thước 160x160 mỗi frame) vào thư mục [`Assets/Sprites/Buildings/ForgeFrames/`](file:///d:/Unity/prj1/gam/Assets/Sprites/Buildings/ForgeFrames/).
     * Khởi tạo Prefab `Building_Blacksmith_Forge` trong [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs): Gắn hoạt ảnh `SpriteFrameAnimator` (6 FPS loop khói đen bốc nghi ngút + lửa than hồng rực rỡ), chân móng `BoxCollider2D` kích thước `(4.5, 2.2)`, và bộ sắp lớp chiều sâu `YSortOrder2D` (`footOffsetY = -2.2f`).
     * Cắm tòa nhà Lò Rèn tại khu đất Tây Bắc Làng Oakhaven `(-49, 9.5)` và đặt NPC Thợ Rèn Barnaby đứng làm việc ngay trước lò rèn `(-47.2, 7.8)`.

#### 🟢 Ca làm việc 4: Tích Hợp Chuẩn Art Gốc Người Dùng (11 Icon & 3 Bãi Đất) & Tách Biệt Hệ Thống Ngoại Trang (Costume)
* **Thời gian:** 03:25 (Giờ hệ thống)
* **Quyết định kiến trúc quan trọng từ người dùng:**
  * **Trang bị Class (Stats Equipment):** Các món trang bị (Giáp da, Mũ, Găng, Giày) sẽ **chỉ gia tăng thuộc tính chiến đấu (HP, DEF, ATK, Tốc độ)** chứ **không bắt buộc ghép đè sprite lên nhân vật**.
  * **Hệ thống Ngoại Trang Độc Lập (Costume / Skin System):** Diện mạo trang phục của nhân vật sẽ do một hệ thống Skin/Costume riêng quản lý (ví dụ: Bộ đồ Tân thủ, Bộ đồ Lính Hoàng Gia, Bộ đồ Lễ Hội...). Cách tiếp cận này giúp đồ họa nhất quán, tránh lỗi lệch frame animation khi đi/chạy/tấn công và dễ mở rộng về sau.
  * **Vũ khí cầm tay:** Vẫn giữ nguyên hiển thị trên tay xoay theo góc ngắm chuột (`WeaponSocketView`).
* **Nội dung kỹ thuật đã hoàn thành:**
  1. **Trích xuất & Tách Nền Trong Suốt Cho Toàn Bộ Bộ Icon Gốc:**
     * Từ ảnh mẫu giao diện người dùng cung cấp, đã cắt chính xác từng icon và khử nền tối thành nền trong suốt tại [`Assets/Sprites/Items/`](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/):
       * `icon_weapon_starter_sword.png`
       * `icon_armor_leather_novice.png`, `icon_helmet_leather_novice.png`, `icon_gloves_leather_novice.png`, `icon_boots_leather_novice.png`
       * `icon_potion_small_hp.png`, `icon_potion_small_mp.png`, `icon_material_herb_leaf.png`
       * `icon_material_ore_chunk.png` (Quặng Đồng & Sắt), `icon_material_animal_hide.png`, `icon_quest_broken_rune.png`
  2. **Trích xuất 3 Điểm Tài Nguyên Trên Mặt Đất (Ground Nodes):**
     * Đã tách nền trong suốt và lưu tại [`Assets/Sprites/Gathering/`](file:///d:/Unity/prj1/gam/Assets/Sprites/Gathering/):
       * `ground_herb_green_leaf.png`: Khóm Thảo Mộc Xanh hoa trắng mọc giữa nền đất cỏ.
       * `ground_ore_copper_chunk.png`: Mỏ Quặng Đồng Thô màu nâu đỏ.
       * `ground_ore_iron_chunk.png`: Mỏ Quặng Sắt Thô màu xám ánh kim.
     * Cập nhật [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs) để Prefab `GatheringNode_Bush`, `GatheringNode_CopperOre`, `GatheringNode_Ore` tự động nhận đúng Sprite mỹ thuật mới này và rải đều trên Thảo Nguyên Xanh.
  3. **Cập nhật Act1ItemCreator:** Hỗ trợ tạo ScriptableObject cho cả 2 loại quặng Đồng và Sắt.
  4. **Nhật ký:** Cập nhật đồng bộ các thay đổi vào [`kich-ban-game-hoi-1.md`](file:///d:/Unity/prj1/gam/kich-ban-game-hoi-1.md).

#### 🟢 Ca làm việc 5: Hoàn Tất Sinh Toàn Bộ 12 ScriptableObject Tài Nguyên & Đấu Nối Vào Hệ Thống Thu Thập
* **Thời gian:** 03:30 (Giờ hệ thống)
* **Nội dung công việc thực hiện:**
  1. **Đấu nối tài nguyên vào game engine (Unity Native Asset):**
     * Đã khởi tạo thư mục và sinh toàn bộ **12 file ScriptableObject** chuẩn trong [`Assets/Resources/Items/`](file:///d:/Unity/prj1/gam/Assets/Resources/Items/):
       * `Starter_Sword.asset`: Kiếm Sắt Rỉ (ATK +8, Tỉ lệ Chí Mạng +5%).
       * `armor_leather_novice.asset`: Giáp Da Thô (DEF +6, HP +30).
       * `helmet_leather_novice.asset`: Mũ Da Thợ Săn (DEF +3, HP +15).
       * `gloves_leather_novice.asset`: Găng Da Bền (ATK +2, DEF +2, HP +10).
       * `boots_leather_novice.asset`: Ủng Da Đi Rừng (DEF +2, HP +10, Tốc độ chạy +0.5).
       * `potion_small_hp.asset`: Bình Máu Nhỏ (Hồi phục HP).
       * `potion_small_mp.asset`: Bình Mana Nhỏ (Hồi phục MP).
       * `material_herb_leaf.asset`: Thảo Mộc Xanh Oakhaven.
       * `material_copper_ore.asset`: Mẩu Quặng Đồng Thô.
       * `material_iron_ore.asset`: Mẩu Quặng Sắt Thô.
       * `material_animal_hide.asset`: Tấm Da Thú Rừng.
       * `quest_broken_rune.asset`: Mảnh Phù Văn Vỡ Vụn.
  2. **Tách biệt Icon Quặng Đồng và Quặng Sắt tỉ mỉ:**
     * Trích xuất trực tiếp icon Quặng Đồng nâu đỏ và Quặng Sắt xám kim từ ô xem trước trên ảnh của người dùng và lưu thành:
       * `icon_material_copper_ore.png`
       * `icon_material_iron_ore.png`
  3. **Đấu nối trực tiếp vào các Bãi Tài Nguyên Dưới Đất (`GatheringNode`):**
     * Prefab `GatheringNode_Bush` tự động rơi ra từ 1 đến 3 `material_herb_leaf` khi người chơi bấm tương tác [F].
     * Prefab `GatheringNode_Ore` tự động rơi ra từ 1 đến 2 `material_iron_ore`.
     * Prefab `GatheringNode_CopperOre` tự động rơi ra từ 1 đến 2 `material_copper_ore`.
     * Cả 3 bãi tài nguyên trên đều có cơ chế channeling 1.0s và tự động hồi lại sau 30 giây.

#### 🟢 Ca làm việc 6: Tách Nền Sạch Cho 3 Bãi Tài Nguyên & Sửa Lỗi Biên Dịch Test Game
* **Thời gian:** 12:15 (Giờ hệ thống)
* **Nội dung công việc hoàn thành:**
  1. **Bóc tách nền sạch (Chỉ giữ lại cây và đá ở giữa):**
     * **Bụi Thảo Mộc Xanh:** Loại bỏ toàn bộ khoanh thảm cỏ tròn ngoài cùng, chỉ giữ lại bụi cây thảo mộc xanh vươn cao với hoa cúc trắng nhụy vàng và thân cành tại [`Assets/Sprites/Gathering/ground_herb_green_leaf.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Gathering/ground_herb_green_leaf.png).
     * **Mỏ Quặng Đồng Thô:** Loại bỏ thảm cỏ nền, chỉ giữ lại cụm khối đá quặng màu cam/nâu đỏ đồng tại [`Assets/Sprites/Gathering/ground_ore_copper_chunk.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Gathering/ground_ore_copper_chunk.png).
     * **Mỏ Quặng Sắt Thô:** Loại bỏ thảm cỏ nền, chỉ giữ lại khối đá quặng sắt xám bạc ánh vàng tại [`Assets/Sprites/Gathering/ground_ore_iron_chunk.png`](file:///d:/Unity/prj1/gam/Assets/Sprites/Gathering/ground_ore_iron_chunk.png).
     * Điều chỉnh `Pixels Per Unit (PPU) = 64` giúp kích thước sprite hiển thị tự nhiên, hài hòa và sắc nét trên mặt đất của thế giới game.
  2. **Kiểm tra và Sửa lỗi Biên Dịch Playmode:**
     * Phát hiện và khắc phục lỗi thiếu `using ActionRPG.Items;` trong [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs).
     * Đã build kiểm tra toàn bộ solution: **0 Error(s)**. Dự án đã hoàn toàn thông suốt, người chơi có thể bấm Play trong Unity để test game bình thường.

#### 🟢 Ca làm việc 7: Dựng Khung Hình Hoạt Họa 15 Kỹ Năng Pixel, Nhân Vật 4 Hướng & Đồng Bộ Vũ Khí
* **Thời gian:** 13:40 (Giờ hệ thống)
* **Nội dung công việc thực hiện:**
  1. **Thiết kế & Hoàn tất 15 Icon Pixel HUD Kỹ Năng Chuẩn 32x32:**
     * Lưu trữ tại [Assets/Sprites/UI/HUD/Skills/](file:///d:/Unity/prj1/gam/Assets/Sprites/UI/HUD/Skills/):
       * **Chiến Binh:** icon_skill_warrior_cleave.png, icon_skill_warrior_vanguard_charge.png, icon_skill_warrior_iron_bastion.png, icon_skill_warrior_tremor_slam.png, icon_skill_warrior_frenzied_strike.png.
       * **Pháp Sư:** icon_skill_mage_fire_orb.png, icon_skill_mage_frost_spikes.png, icon_skill_mage_blink.png, icon_skill_mage_chain_lightning.png, icon_skill_mage_mana_shield.png.
       * **Xạ Thủ:** icon_skill_archer_piercing_shot.png, icon_skill_archer_volley.png, icon_skill_archer_tumble.png, icon_skill_archer_snare_trap.png, icon_skill_archer_deadeye.png.
  2. **Dựng Sprite Sheet Hoạt Họa & Bộ Điều Khiển Nhân Vật 4 Hướng (LuminaFourDirectionCharacterAnimator):**
     * Hoàn tất 3 Sprite Sheet 42 frames mỗi class tại [Assets/Sprites/](file:///d:/Unity/prj1/gam/Assets/Sprites/): [player_warrior_4direction.png](file:///d:/Unity/prj1/gam/Assets/Sprites/player_warrior_4direction.png), [player_mage_4direction.png](file:///d:/Unity/prj1/gam/Assets/Sprites/player_mage_4direction.png), [player_archer_4direction.png](file:///d:/Unity/prj1/gam/Assets/Sprites/player_archer_4direction.png).
     * Trích dẫn mã nguồn bộ điều khiển hoạt ảnh: [LuminaFourDirectionCharacterAnimator.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/LuminaFourDirectionCharacterAnimator.cs):
       * [ApplyDirection](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/LuminaFourDirectionCharacterAnimator.cs#L159-L170): Tự động đổi hướng nhìn (Down=0, Up=1, Right=2, Left=3).
       * [TriggerAttack](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/LuminaFourDirectionCharacterAnimator.cs#L189-L198): Kích hoạt cử động chém/bắn 3 frames theo góc nhắm.
       * [TriggerCast](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/LuminaFourDirectionCharacterAnimator.cs#L173-L184): Kích hoạt tư thế giơ vũ khí tụ năng lượng 2 frames theo hướng chuột.
  3. **Bộ Sprite Vũ Khí 4 Hướng & Đồng Bộ Lớp Hiển Thị (`WeaponSocketView`):**
     * Thiết kế 12 sprite vũ khí cầm tay 32x32 không bị ngược bóng:
       * Kiếm Chiến Binh: [weapon_warrior_sword_down.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_down.png), [_up.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_up.png), [_right.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_right.png), [_left.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_warrior_sword_left.png).
       * Trượng Pháp Sư: [weapon_mage_staff_down.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_down.png), [_up.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_up.png), [_right.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_right.png), [_left.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_mage_staff_left.png).
       * Cung Xạ Thủ: [weapon_archer_bow_down.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_down.png), [_up.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_up.png), [_right.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_right.png), [_left.png](file:///d:/Unity/prj1/gam/Assets/Sprites/Items/weapon_archer_bow_left.png).
     * Trích dẫn mã nguồn sắp lớp: [`WeaponSocketView`.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/Presentation/Views/`WeaponSocketView`.cs#L88-L105) đảm bảo quay lên trên thì giấu vũ khí sau lưng (sortingOrder - 1), quay xuống hoặc sang bên thì đưa vũ khí ra trước mặt (sortingOrder + 2).
  4. **Dựng Trọn Bộ 82 Khung Hình Hoạt Họa & 16 Unity Prefab Cho 15 Kỹ Năng:**
     * Toàn bộ 15 kỹ năng được dựng sprite sheet độc lập tại [Assets/Sprites/VFX/](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/) và đóng gói Prefab tại [Assets/Prefabs/VFX/](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/) kèm component [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs).

### 📖 Bảng Trích Dẫn Chi Tiết Toàn Bộ Khung Hình Animation (Frame Citations) 15 Kỹ Năng:

#### 1. Chiến Binh (Warrior)
* **Q. Trảm Kích (Cleave):**
  * Sprite Sheet: [vfx_warrior_cleave.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_warrior_cleave.png) (6 frames: vfx_warrior_cleave_0 .. _5).
  * Prefab: [VFX_Warrior_Cleave.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Warrior_Cleave.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 16, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 vệt sáng khởi động -> f1-3 vòng bán nguyệt 180° quét rộng màu đỏ rực -> f4-5 tàn khí tan dần.
* **W. Xung Phong (Vanguard Charge):**
  * Sprite Sheet: [vfx_warrior_vanguard_charge.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_warrior_vanguard_charge.png) (6 frames: vfx_warrior_vanguard_charge_0 .. _5).
  * Prefab: [VFX_Warrior_Vanguard_Charge.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Warrior_Vanguard_Charge.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 14, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 nón xung kích hình thành -> f1-3 vệt gió tốc hành lướt tới cực mạnh -> f4-5 bùng nổ choáng tại điểm va chạm.
* **E. Thiết Giáp (Iron Bastion):**
  * Sprite Sheet: [vfx_warrior_iron_bastion.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_warrior_iron_bastion.png) (6 frames: vfx_warrior_iron_bastion_0 .. _5).
  * Prefab: [VFX_Warrior_Iron_Bastion.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Warrior_Iron_Bastion.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 10, loop: true, destroyOnFinish: false).
  * Diễn biến khung hình: f0-5 vòng kim giáp hoàng kim xoay tròn liên tục quanh thân nhân vật, bảo hộ giảm sát thương.
* **R. Địa Chấn (Tremor Slam):**
  * Sprite Sheet: [vfx_warrior_tremor_slam.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_warrior_tremor_slam.png) (6 frames: vfx_warrior_tremor_slam_0 .. _5).
  * Prefab: [VFX_Warrior_Tremor_Slam.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Warrior_Tremor_Slam.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 12, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 nứt đất sơ khởi -> f1-3 các mảng đá tảng bung lên hình nón theo hướng đập -> f4-5 bụi đất lắng xuống, tạo vùng làm chậm 40%.
* **T. Cuồng Nộ Kích (Frenzied Strike):**
  * Sprite Sheet: [vfx_warrior_frenzied_strike.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_warrior_frenzied_strike.png) (6 frames: vfx_warrior_frenzied_strike_0 .. _5).
  * Prefab: [VFX_Warrior_Frenzied_Strike.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Warrior_Frenzied_Strike.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 15, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 tụ khí huyết nộ -> f1-3 song kiếm trảm chữ X bộc phát sáng lóa -> f4-5 tia máu tan biến, gây sát thương cực lớn.

#### 2. Pháp Sư (Mage)
* **Q. Hỏa Cầu (Fire Orb):**
  * Sprite Sheet: [vfx_mage_fire_orb.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_mage_fire_orb.png) (8 frames: vfx_mage_fire_orb_0 .. _7).
  * Prefab Đạn Bay: [FireOrb_Projectile.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/FireOrb_Projectile.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (frames: f0-3 xoay cuộn quả cầu lửa, FPS: 12, loop: true).
  * Prefab Vụ Nổ AOE: [FireOrb_Explosion_VFX.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/FireOrb_Explosion_VFX.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (frames: f4-7 sóng xung kích lửa bùng nổ, FPS: 14, loop: false, destroyOnFinish: true).
* **W. Băng Phong Thược (Frost Spikes):**
  * Sprite Sheet: [vfx_mage_frost_spikes.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_mage_frost_spikes.png) (6 frames: vfx_mage_frost_spikes_0 .. _5).
  * Prefab: [VFX_Mage_Frost_Spikes.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Mage_Frost_Spikes.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 11, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 vòng ma pháp băng lam xuất hiện -> f1-3 ba trụ gai băng nhọn hoắt mọc vọt lên khỏi mặt đất trói chân 1.5s -> f4-5 gai băng vỡ thành hoa tuyết.
* **E. Thiểm Dịch (Blink / Arcane Shift):**
  * Sprite Sheet: [vfx_mage_blink.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_mage_blink.png) (6 frames: vfx_mage_blink_0 .. _5).
  * Prefab: [VFX_Mage_Blink.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Mage_Blink.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 14, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0-2 vòng xoáy hư không co rút cuốn lấy nhân vật -> f3-5 bùng nổ hư không tại điểm đến để lại dư ảnh làm chậm kẻ địch.
* **R. Lôi Điện Tỏa (Chain Lightning):**
  * Sprite Sheet: [vfx_mage_chain_lightning.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_mage_chain_lightning.png) (6 frames: vfx_mage_chain_lightning_0 .. _5).
  * Prefab: [VFX_Mage_Chain_Lightning.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Mage_Chain_Lightning.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 15, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 tia sét phóng xuất -> f1-3 phân nhánh ziczac giật điện liên hoàn sang 3 mục tiêu xung quanh -> f4-5 tia điện ion hóa tàn dần.
* **T. Nguyên Tố Hộ Thể (Mana Shield):**
  * Sprite Sheet: [vfx_mage_mana_shield.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_mage_mana_shield.png) (6 frames: vfx_mage_mana_shield_0 .. _5).
  * Prefab: [VFX_Mage_Mana_Shield.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Mage_Mana_Shield.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 10, loop: true, destroyOnFinish: false).
  * Diễn biến khung hình: f0-5 kết giới lục giác ma pháp lam xoay chuyển quanh cơ thể, chuyển 30% sát thương nhận vào thành tiêu hao Mana.

#### 3. Xạ Thủ (Archer)
* **Q. Xuyên Vân Tiễn (Piercing Shot):**
  * Sprite Sheet: [vfx_archer_piercing_shot.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_archer_piercing_shot.png) (4 frames: vfx_archer_piercing_shot_0 .. _3).
  * Prefab: [VFX_Archer_Piercing_Shot.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Archer_Piercing_Shot.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 15, loop: true).
  * Diễn biến khung hình: f0-3 mũi tên phát sáng ánh lục tốc độ cao 20 m/s với luồng khí cuộn xoáy quanh thân tên, đâm xuyên không giới hạn số quái.
* **W. Mưa Tên (Volley / Barrage):**
  * Sprite Sheet: [vfx_archer_volley.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_archer_volley.png) (6 frames: vfx_archer_volley_0 .. _5).
  * Prefab: [VFX_Archer_Volley_Arrow.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Archer_Volley_Arrow.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 12, loop: true).
  * Diễn biến khung hình: f0-5 chùm mũi tên xé gió phóng theo hình quạt 36°, tạo vệt khí động học làm chậm kẻ địch chạm phải.
* **E. Nhào Lộn Thoát Thân (Tumble):**
  * Sprite Sheet: [vfx_archer_tumble.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_archer_tumble.png) (4 frames: vfx_archer_tumble_0 .. _3).
  * Prefab: [VFX_Archer_Tumble.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Archer_Tumble.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 12, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình: f0 vệt lốc thoảng khởi động -> f1-2 cuộn tròn lộn nhào né đòn kèm bụi đất -> f3 luồng phong tức tan biến, kích hoạt buff tăng tốc và tầm đánh 3s.
* **R. Bẫy Dây (Snare Trap):**
  * Sprite Sheet: [vfx_archer_snare_trap.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_archer_snare_trap.png) (4 frames: vfx_archer_snare_trap_0 .. _3).
  * Prefab: [VFX_Archer_Snare_Trap.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Archer_Snare_Trap.prefab#L109-L116) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 10, loop: false, destroyOnFinish: true).
  * Diễn biến khung hình:
    * Frame 0: vfx_archer_snare_trap_0 — Bẫy sắt cơ học đặt trên mặt đất, 2 ngàm mở rộng chờ kích hoạt.
    * Frame 1: vfx_archer_snare_trap_1 — Kẻ địch chạm vào, lò xo bung và 2 ngàm bập sập vào giữa.
    * Frame 2: vfx_archer_snare_trap_2 — Cụm gai sắt và dây chằng búng vọt lên trói chân mục tiêu.
    * Frame 3: vfx_archer_snare_trap_3 — Dây thép gai siết chặt, lóe tia kim loại giam giữ đối thủ trong 1.25s.
* **T. Ngắm Chuẩn Dứt Điểm (Deadeye):**
  * Sprite Sheet: [vfx_archer_deadeye.png](file:///d:/Unity/prj1/gam/Assets/Sprites/VFX/vfx_archer_deadeye.png) (4 frames: vfx_archer_deadeye_0 .. _3).
  * Prefab: [VFX_Archer_Deadeye.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/VFX/VFX_Archer_Deadeye.prefab) — [SimpleFrameAnimator2D](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/SimpleFrameAnimator2D.cs) (FPS: 10, loop: true, destroyOnFinish: false).
  * Diễn biến khung hình: f0-3 hồng tâm laser ngắm bắn nhấp nháy xoay tròn, khóa chặt mục tiêu trước khi phóng phát đạn bạo kích cực đại.

  5. **Kiểm Thử Toàn Diện & Đấu Nối ScriptableObject:**
     * Toàn bộ 15 ScriptableObject [SkillData](file:///d:/Unity/prj1/gam/Assets/ScriptableObjects/Skills/) đã được liên kết đúng Prefab (hitVfxPrefab / projectilePrefab).
     * Cơ chế tung chiêu thời gian thực [PlayerSkillCombat2D.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/Combat/PlayerSkillCombat2D.cs#L225-L270) đã tự động xoay hướng nhân vật, phát animation vung vũ khí / niệm phép, và sinh VFX tương ứng.
     * Chạy script tự động kiểm tra hệ thống: **66/66 checks PASSED (100%)**.

---

### 📅 Ngày 2026-10-15

#### 🟢 Ca làm việc 8: Cải tiến Cơ Chế Cầm Nắm Vũ Khí – Ẩn Vũ Khí Khi Đứng Yên/Di Chuyển, Chỉ Hiện Khi Tấn Công/Tung Chiêu
* **Thời gian:** 16:30 (Giờ hệ thống)
* **Mục tiêu:** Khắc phục tình trạng vũ khí luôn hiển thị trên tay nhân vật lúc đứng yên hoặc đi bộ gây rối mắt; đồng bộ hiển thị vũ khí chỉ xuất hiện khi vung đòn hoặc niệm phép rồi tự động ẩn lại.
* **Nội dung công việc hoàn thành:**
  1. **Chỉnh sửa [`WeaponSocketView`.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/02_Presentation_Visuals/Views/`WeaponSocketView`.cs):**
     * Trạng thái mặc định (`Awake`, `Start`, `OnEnable`): Đặt `weaponRenderer.enabled = false;` để vũ khí luôn ẩn khi nhân vật đứng yên hoặc chạy bộ.
     * Khi thay đổi trang bị (`OnEquipmentChanged`, `SetWeaponSprite`): Cập nhật `weaponRenderer.sprite` nhưng giữ `weaponRenderer.enabled = false;`.
     * Khi tấn công thường (`PlayAttackSlashAnimation`): Bật `weaponRenderer.enabled = true;`, thực hiện chuỗi xoay/chém, sau đó tự động tắt `weaponRenderer.enabled = false;`.
     * Khi tung chiêu (`PlayCastAnimation`): Bật hiển thị trong suốt thời gian niệm phép rồi tự động ẩn lại.
  2. **Đồng bộ hóa Animation 4 hướng [LuminaFourDirectionCharacterAnimator.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/LuminaFourDirectionCharacterAnimator.cs):**
     * Tích hợp gọi `weaponSocketView.PlayAttackSlashAnimation(...)` và `weaponSocketView.PlayCastAnimation(...)` khi nhân vật kích hoạt animation Attack hoặc Cast.

#### 🟢 Ca làm việc 9: Đồng Bộ Hóa Đồ Họa & Hoạt Họa Nhân Vật Class Tân Thủ (Novice)
* **Thời gian:** 17:10 (Giờ hệ thống)
* **Mục tiêu:** Chuẩn hóa nhân vật Class Tân thủ (Novice Tier 1) đồng bộ về kích thước hình ảnh, pivot, sprite sheet 4 hướng và animation với 3 class nghề nghiệp cơ bản (Chiến Binh, Pháp Sư, Xạ Thủ).
* **Nội dung công việc hoàn thành:**
  1. Cấu hình lại sprite nhân vật Tân thủ theo đúng chuẩn kích thước và tỉ lệ của 3 class cơ bản.
  2. Đồng bộ hóa component `LuminaFourDirectionCharacterAnimator` và `WeaponSocketView` trên prefab `Player_Novice_Tier1.prefab`.

#### 🟢 Ca làm việc 10: Khắc Phục Triệt Để Lỗi Thanh Kỹ Năng Bị Đen Trống & Missing Script trên HUD
* **Thời gian:** 18:35 (Giờ hệ thống)
* **Mục tiêu:** Sửa lỗi thanh 6 ô kỹ năng trên HUD bị đen trống, không hiển thị icon và không thể bấm phím 1..6; giải quyết lỗi Unity Console cảnh báo `The referenced script on this Behaviour (Game Object 'SkillSlot_3') is missing` và tương tự với `SkillSlot_6`.
* **Phân tích nguyên nhân gốc rễ:**
  1. Lớp `SkillSlotUI` trước đây nằm chung bên trong `GameplayHUD.cs`. Trong Unity, các class phụ trong cùng một file script không được tạo MonoScript asset (`.cs.meta`), dẫn đến việc cả 6 slot trong `GameUIRoot.prefab` bị gán `m_Script: {fileID: 0}` (Missing Script).
  2. Prefab người chơi `Player_Novice_Tier1.prefab` có trường `currentClass` bị gán `{fileID: 0}` (null), khiến kỹ năng Tân Thủ không được nạp tự động vào `GameplayHUD` khi vừa vào game.
  3. Xung đột input phím `1..6` giữa `PlayerController.cs` và `PlayerSkillCombat2D.cs` gây tình trạng trừ mana trùng lặp.
* **Nội dung công việc hoàn thành:**
  1. **Tách riêng script [SkillSlotUI.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/SkillSlotUI.cs):**
     * Khởi tạo file độc lập trong `Assets/Scripts/UI/Gameplay/SkillSlotUI.cs` kèm meta file GUID `f2a4c8e1d3b5790246813579bdf02468`.
     * Tích hợp hàm `EnsureReferences()` tự động tìm kiếm các GameObject con (`Icon`, `CooldownOverlay`, `Key`, `ManaCost`, `CDText`) mà không cần kéo thả thủ công trên Inspector.
     * Xóa bỏ khai báo trùng trong `GameplayHUD.cs` và đăng ký file vào `Assembly-CSharp.csproj`.
  2. **Khôi phục tham chiếu trong [GameUIRoot.prefab](file:///d:/Unity/prj1/gam/Assets/Prefabs/UI/GameUIRoot.prefab):**
     * Khôi phục tham chiếu `m_Script` của cả 6 slot (`SkillSlot_1` đến `SkillSlot_6`) trỏ chính xác về GUID `f2a4c8e1d3b5790246813579bdf02468`.
     * Bảo toàn 100% dữ liệu liên kết RectTransform, Image, Button và Text của toàn bộ 6 slot.
  3. **Tự động hóa hoàn toàn tham chiếu UI trong [GameplayHUD.cs](file:///d:/Unity/prj1/gam/Assets/Scripts/UI/Gameplay/GameplayHUD.cs):**
     * Nâng cấp `AutoFindSkillSlots()`: Nếu mảng `skillSlots` bị rỗng hoặc mất liên kết, tự động quét container `BottomRight_SkillBar`, tìm kiếm theo tên `SkillSlot_1..6` và tự động gắn `AddComponent<SkillSlotUI>()` nếu bị thiếu.
     * Cập nhật hiển thị an toàn: Khi có kỹ năng, gán `iconImage.sprite = spr`, bật `iconImage.enabled = true` và `iconImage.color = Color.white` (Alpha = 1). Nếu ô không có kỹ năng (ô số 6), tự động ẩn icon và làm mờ phím tắt.
  4. **Đồng bộ luồng dữ liệu Class Tân Thủ & Quản lý Phím bấm:**
     * Gán `currentClass` trong `Player_Novice_Tier1.prefab` trỏ tới `Class_Novice.asset`.
     * Kích hoạt sự kiện `EventBus.OnClassApplied` và `EventBus.RaiseSkillChanged` trong `ClassSystem.cs` ngay khi khởi tạo.
     * Cập nhật `PlayerSkillCombat2D.cs`: Mở rộng nạp đủ 6 slot trong `ApplySkills`, tích hợp kiểm tra Vùng an toàn (`isSafeZone`), lắng nghe `EventBus.OnSkillRequested` để click chuột vào ô HUD cũng tung được chiêu.
     * Cập nhật `PlayerController.cs`: Ủy quyền thi triển chiêu sang `PlayerSkillCombat2D.CastSkillBySlot`, đảm bảo phím di chuyển WASD độc lập hoàn toàn và loại bỏ triệt để double-cast.
  5. **Kiểm tra biên dịch:**
     * `dotnet build Assembly-CSharp.csproj` ➔ **0 Error(s)**.
     * `dotnet build Assembly-CSharp-Editor.csproj` ➔ **0 Error(s)**.



---

### 📅 Ngày 2026-10-16

#### 🟢 Ca làm việc 11: Tiếp Nhận & Quy Hoạch Bộ Tài Nguyên RPG_skins_V1.1 Làm Skins Vũ Khí (Lưu Kế Hoạch Làm Sau Này)
* **Thời gian:** 23:10 (Giờ hệ thống)
* **Mục tiêu:** Khảo sát, định danh và quy hoạch gói tài nguyên [Assets/RPG_skins_V1.1](file:///d:/Unity/prj1/gam/Assets/RPG_skins_V1.1/) làm nguồn skin vũ khí cho các Class nhân vật, ghi nhận vào kịch bản Hồi 1 và lưu vào Backlog để triển khai sau này.
* **Nội dung công việc hoàn thành:**
  1. **Khảo sát cấu trúc tài nguyên:**
     - Bộ tài nguyên nằm tại [Assets/RPG_skins_V1.1](file:///d:/Unity/prj1/gam/Assets/RPG_skins_V1.1/).
     - Kích thước chuẩn: **32x32 pixel**, khớp hoàn toàn với quy chuẩn sprite vũ khí của `WeaponSocketView` và `PlayerVisualView`.
     - Bao gồm 2 bộ định dạng: `Icons_no_background/` (tách nền trong suốt, sẵn sàng gắn trực tiếp vào tay nhân vật) và `Icons_background/` (kèm khung nền, sẵn sàng hiển thị trên UI).
  2. **Thống kê chi tiết danh mục Skins Vũ Khí (Tổng cộng 224+ mẫu vũ khí):**
     - **Kiếm (Swords):** 88 mẫu phong phú (từ kiếm gỗ, kiếm sắt, đoản kiếm đến đại kiếm phát sáng hào quang nguyên tố). Phù hợp làm skin kiếm cho Chiến Binh và Tân Thủ.
     - **Gậy phép / Pháp trượng (Staffs):** 40 mẫu gậy pháp thuật (gậy ngọc, gậy nguyên tố Lửa/Băng/Sét). Phù hợp làm skin vũ khí Pháp Sư.
     - **Rìu chiến (Axes):** 40 mẫu rìu 1 lưỡi và 2 lưỡi (thô sơ đến thần thoại).
     - **Đinh ba / Thương (Tridents):** 24 mẫu vũ khí cán dài uy lực.
     - **Súng cao su (Slingshots):** 16 mẫu vũ khí cơ động tầm xa.
     - **Quạt chiến (Fans):** 16 mẫu vũ khí phong cách đặc thù.
     - **Tài nguyên bổ trợ:** 48 mẫu nhẫn & dây chuyền (`rings_and_necklaces`), 24 mẫu bình thuốc tiêu hao (`consumables`).
  3. **Quyết định lộ trình triển khai (Lưu lại để làm sau này):**
     - Giai đoạn hiện tại của Hồi 1 tiếp tục ưu tiên hoàn thiện gameplay cơ bản, chuỗi nhiệm vụ tân binh và tính năng chiến đấu cốt lõi.
     - Bộ tài nguyên RPG_skins_V1.1 được lưu trữ an toàn trong dự án và đưa vào kế hoạch sau này: sẽ thiết kế hệ thống Tủ Đồ Ngoại Trang (Weapon Transmog/Skin System), cho phép người chơi thay đổi diện mạo vũ khí mà không làm mất chỉ số trang bị gốc.

#### 🟢 Ca làm việc 12: Cài Đặt & Nâng Cấp Bộ Công Cụ Agent Sprite Forge v0.4.0 (Skill Tạo Ảnh 2D & Tạo Hoạt Ảnh Animation)
* **Thời gian:** 23:55 (Giờ hệ thống)
* **Kho lưu trữ nguồn:** `https://github.com/0x0funky/agent-sprite-forge.git`
* **Mục tiêu:** Cài đặt toàn diện bộ kỹ năng Agent Sprite Forge v0.4.0 (The Big Upgrade) vào `.agents/skills/`, nâng cấp quy trình tạo ảnh 2D và dựng hoạt ảnh animation mượt mà chuyên nghiệp cho dự án game Lumina.
* **Nội dung công việc hoàn thành:**
  1. **Nâng cấp toàn bộ 5 kỹ năng lên bản v0.4.0:**
     - `generate2dsprite`: Chuyên biệt tạo ảnh 2D (Master Still nhân vật, Sprite Sheet quái vật/vật phẩm, Icon UI, FX).
     - `video2dsprite`: Chuyên biệt tạo Animation nhân vật từ 1 ảnh tĩnh (Master Still) -> sinh chuỗi hành động thời gian thực (Idle, Walk, Run, Attack, Cast, Jump, Hurt) với công nghệ Soft-matte keying khử viền tím, Color Lock giữ nguyên bảng màu, Foot Registration chống trượt chân, Gait Loop bắt chu kỳ bước đi và Auto-retime đòn đánh đạt tốc độ game.
     - `generate2dmedia`: Cổng điều phối đa nền tảng kết nối API / CLI sinh ảnh và video.
     - `generate2dmap`: Thiết kế bản đồ 2D, terrain, tileset, prop pack và parallax background.
     - `codeart2d`: Phương án dự phòng vẽ sprite/FX thuần mã nguồn khi không dùng model AI.
     - Đã đồng bộ 164 files vào `.agents/skills/` và tạo manifest xác thực SHA256 an toàn (`in-sync`).
  2. **Cài đặt & kiểm thử môi trường Python:**
     - Đã cài đặt đầy đủ: `numpy 2.5.4`, `Pillow 12.3.0`, `scipy 1.18.1`, `resvg-py 0.5.0`.
     - Đã cấu hình bộ giải mã `ffmpeg 7.1` vào PATH hệ thống để phục vụ render và giải nén frame animation.
     - Đã chạy kiểm tra hệ thống `forge_doctor.py`: Toàn bộ 5 skills và thư viện phụ thuộc đã sẵn sàng hoạt động.

---
### 📅 Ngày 2026-10-21

#### 🟢 Ca làm việc 13: Xây Dựng & Tích Hợp Hệ Thống Hiển Thị Thanh Máu (HP Bar) & Cấp Độ (Lv.) Tự Động Cho Toàn Bộ Quái Vật (EnemyOverheadUI)
* **Thời gian:** 18:50 (Giờ hệ thống)
* **Mục tiêu:** Tự động hiển thị thanh máu Procedural World-Space và nhãn "Lv.X [Tên Quái]" trên đỉnh đầu tất cả quái vật (Sói Trắng, Bù Nhìn Luyện Võ, Quái Rừng, Boss...) mà không cần tạo Prefab thủ công, đảm bảo không bị che khuất và không bị xoay/lộn ngược chữ khi quái lật mặt.
* **Nội dung công việc hoàn thành:**
  1. **Khảo sát cấu trúc chỉ số quái vật (EnemyStats & EnemyController):**
     * Đồng bộ chính xác các thuộc tính `currentHp`, `maxHp`, `level`, `enemyName` và cấu trúc nhận sát thương tương khắc hệ Lumina x2.0.
  2. **Xây dựng Script `Assets/Scripts/UI/EnemyOverheadUI.cs`:**
     * Tự động tính toán chiều cao đỉnh đầu dựa trên `SpriteRenderer.bounds.extents.y + 0.35f` để định vị thanh máu chính xác tuyệt đối trên đầu mọi kích thước quái vật.
     * Tự động sinh `Canvas` con ở chế độ `RenderMode.WorldSpace`, ép `sortingOrder = 500` để không bao giờ bị địa hình, mặt đất hay tán cây che khuất. Kích thước tỷ lệ `localScale = new Vector3(0.01f, 0.01f, 1.0f)` chuẩn pixel art.
     * Tự động sinh nhãn cấp độ & tên (`TextMeshProUGUI`) định dạng `"Lv.X [Tên Quái]"` (ví dụ: `"Lv.3 Sói Xám"`, `"Lv.1 Bù Nhìn Luyện Võ"`), màu vàng rực rỡ kèm component `Outline` viền đen sắc nét.
     * Tự động sinh khung nền thanh máu đen mờ 85% (`width: 90, height: 12`) và thanh máu đỏ (`Image.Type.Filled`, Horizontal) co giãn theo tỷ lệ `currentHp / maxHp`.
     * Tích hợp thanh `Damage Ease Lag` (cam/vàng) rút từ từ sau delay 0.35s tạo phản hồi trúng đòn đã tay.
     * Khóa góc xoay `Quaternion.identity` và triệt tiêu đảo trục `0.01f * Mathf.Sign(transform.lossyScale.x)` trong `LateUpdate()`, giúp chữ không bao giờ bị lộn ngược khi quái đổi hướng (`flipX` hoặc `localScale.x = -1`).
     * Tự động ẩn khi quái vật bị tiêu diệt (`currentHp <= 0`).
  3. **Tự động kích hoạt & Quét toàn diện trên Scene & Prefabs:**
     * Thêm cơ chế tự kiểm tra và gắn tự động `if (GetComponent<EnemyOverheadUI>() == null) gameObject.AddComponent<EnemyOverheadUI>();` trong `Start()` và `OnEnable()` của `EnemyStats.cs` và `EnemyController.cs`.
     * Đồng bộ trong `ZoneEnemySpawner.cs` và `ClassDemoSwitcherUI.cs` (quái test Bù nhìn).
     * Tạo công cụ Editor Menu tại `Assets/Editor/AttachEnemyOverheadUIMenu.cs`: **`Tools -> Lumina -> Attach Overhead HP Bar To All Enemies`** quét và gắn sẵn vào toàn bộ quái vật/bù nhìn trên Scene đang mở và cập nhật sẵn toàn bộ Enemy Prefabs trong `Assets/Prefabs/ForestPrototype/`.
  4. **Kiểm thử tự động (Unit Test Suite):**
     * Viết và chạy thành công 7 bài test trong `Assets/Editor/EnemyOverheadUITest.cs` (**7/7 PASSED**):
       - Test 1: Khởi tạo World-Space Canvas với sortingOrder = 500.
       - Test 2: Định dạng nhãn Level & Tên quái vật.
       - Test 3: Kích thước thanh máu 90x12 và thanh đỏ Filled.
       - Test 4: Co giãn thanh máu khi quái nhận sát thương.
       - Test 5: Khóa xoay và chống lộn ngược chữ khi quái flip.
       - Test 6: Tự động gắn EnemyOverheadUI trong Start/OnEnable của EnemyStats & EnemyController.
       - Test 7: Menu Tools quét và tự động cập nhật Prefabs quái vật.
     * Đảm bảo **0 lỗi biên dịch (0 compilation errors)** trên toàn bộ dự án.


#### 🟢 Ca làm việc 14: Cải Tiến Toàn Diện Động Vật Tự Nhiên (Hươu, Thỏ) - Tiêu Diệt Rơi Da Thú & Thu Phục Làm Pet Đồng Hành
* **Thời gian:** 19:15 (Giờ hệ thống)
* **Mục tiêu:** Mở rộng cơ chế thế giới mở sinh động: Cho phép toàn bộ động vật tự nhiên trong rừng và quanh làng (Hươu Đực Có Gạc, Thỏ Trắng, Thỏ Nâu) có thể bị tấn công, hiển thị thanh máu Overhead, hoảng loạn bỏ chạy khi trúng đòn, bị tiêu diệt rơi Da Thú Rừng (`material_animal_hide`) + EXP, và đặc biệt có thể dùng Quả Cầu Tinh Linh / Phù Chú Thu Phục để biến thành Pet cưng khi máu yếu (< 30%).
* **Nội dung công việc hoàn thành:**
  1. **Đấu Nối Sát Thương Trực Tiếp (`Hurtbox2D.cs` & `EnemyStats.cs`):**
     * Trước đây `Hurtbox2D` chỉ ủy quyền nhận sát thương qua `EnemyController`. Vì động vật rừng là sinh vật hòa bình (sử dụng `NPCWanderController2D`), sát thương của người chơi không được áp dụng vào `EnemyStats`.
     * Đã bổ sung cơ chế fallback trực tiếp: Nếu thực thể trúng đòn không có `EnemyController`, `Hurtbox2D` sẽ tự động chuyển phát sát thương và hiệu ứng đẩy lùi (knockback) thẳng vào `EnemyStats.TakeDamage(damage, isCrit, attacker)`.
     * Trong `EnemyStats.Awake()`, tự động đăng ký `hurtbox.OnTakeDamageCustom += TakeDamage` khi không tìm thấy `EnemyController`.
  2. **Hành Vi Sinh Tồn: Động Vật Hoảng Loạn Bỏ Chạy (`NPCWanderController2D.cs`):**
     * Thêm phương thức `public void FleeFrom(Vector2 dangerSource)`: Khi động vật bị đánh trúng hoặc bị dính knockback, thay vì đứng yên chịu trận hay lao vào cắn trả, động vật sẽ kích hoạt trạng thái hoảng loạn (`isFleeing = true`), tăng tốc độ di chuyển gấp 2.5 lần và tháo chạy ngược hướng người chơi trong 4.0 giây trước khi ổn định lại tâm lý.
  3. **Phần Thưởng Tiêu Diệt & Rơi Vật Phẩm Da Thú Rừng (`EnemyStats.cs`):**
     * Khi HP rơi về 0, động vật tự động kích hoạt logic `Die()`, cộng điểm kinh nghiệm (EXP) và vàng cho người chơi.
     * Bổ sung cơ chế rơi vật phẩm fallback: Nếu động vật chưa có cấu hình Drop Table, game sẽ tự động sinh và nạp `material_animal_hide` (Da Thú Rừng Oakhaven) rơi ra đất với tỷ lệ 85% kèm âm thanh và hiệu ứng rơi đồ.
  4. **Nâng Cấp Hệ Thống Bắt Pet (`PetCaptureSystem.cs` & `PlayerController.cs`):**
     * Bổ sung hàm nạp chồng `public bool TryCaptureEnemy(EnemyStats targetEnemy, ItemData scrollOrOrbItem)`.
     * Hỗ trợ trích xuất thông tin động vật thành `PetData` / `PetInstance`: Lấy đúng Sprite hiện tại từ `SpriteRenderer` của Hươu/Thỏ, suy luận hệ nguyên tố tự nhiên phù hợp (Hươu Sao -> Hệ Thổ, Thỏ Rừng -> Hệ Phong).
     * Điều kiện bắt giữ cân bằng: Động vật phải bị suy yếu còn dưới 30% HP (`currentHp / maxHp <= 0.3f`), người chơi sở hữu Quả Cầu Tinh Linh hoặc Phù Chú Thu Phục trong túi đồ.
     * Khi bắt thành công, động vật tan biến vào hào quang phép thuật, thú cưng được thêm ngay vào `PlayerPetRoster` và sẵn sàng gọi ra (`SummonPet`) để chạy lon ton theo sau người chơi.
     * Nếu bắt trượt, động vật sẽ hoảng sợ bỏ chạy thục mạng (`FleeFrom`) thay vì kích hoạt nộ khí (`Enrage`) như quái vật hung hãn.
     * Cập nhật phím tắt `[C]` trong `PlayerController.cs`: Quét cả Layer `Enemy` và `Default` trong bán kính 3.0m để người chơi có thể bắt cả quái vật lẫn động vật hoang dã.
  5. **Cấu Hình Prefabs & Scene Toàn Diện (`ForestMapGenerator.cs` & `AttachEnemyOverheadUIMenu.cs`):**
     * Thiết lập lại toàn bộ Prefabs Động vật rừng (`Animal_Deer_Stag`, `Animal_Rabbit_Brown`, `Animal_Rabbit_White`):
       - Chuyển Layer sang `Enemy`, Tag sang `Enemy`.
       - Chuyển `Rigidbody2D` sang `Dynamic` (`gravityScale = 0`, `freezeRotation = true`) để nhận va chạm và lực đẩy đồng bộ.
       - Tự động gắn thanh máu `EnemyOverheadUI` ("Lv.1 Hươu Rừng", "Lv.1 Thỏ Trắng").
     * Thêm công cụ Menu Editor tại `Assets/Editor/AttachEnemyOverheadUIMenu.cs`: **`Tools -> Lumina -> Cấu Hình Động Vật Có Thể Tiêu Diệt & Thu Phục Pet`** quét và tự động cập nhật cả Prefabs lẫn các thực thể động vật trên Scene hiện tại.
  6. **Bộ Kiểm Thử Động Vật Tự Động (`AnimalKillAndCaptureTest.cs`):**
     * Xây dựng 6 bài kiểm thử tự động tại menu `Tools -> Lumina -> Chạy Kiểm Thử Động Vật Bị Tiêu Diệt & Thu Phục`:
       - Test 1: Động vật (Hươu/Thỏ) nhận sát thương chuẩn xác qua Hurtbox2D.
       - Test 2: Động vật hoảng loạn bỏ chạy (FleeFrom) khi bị người chơi tấn công.
       - Test 3: Động vật bị tiêu diệt khi HP <= 0 và rơi Da Thú Rừng.
       - Test 4: Thu phục Động vật thành công khi HP < 30% và khởi tạo PetInstance đầy đủ Sprite & Hệ nguyên tố.
       - Test 5: Từ chối thu phục khi máu Động vật còn sung mãn (> 30%).
       - Test 6: Kiểm tra cấu hình toàn bộ Prefabs Động vật rừng trên ổ đĩa.
     * Dự án đạt **0 lỗi biên dịch (0 compilation errors)**.

---


---

#### 🟢 Ca làm việc 15: Sao Lưu Toàn Diện Toàn Bộ Các Map Dự Án Lumina
* **Thời gian:** 17:34 (Giờ hệ thống ngày 2026-10-28)
* **Mục tiêu:** Sao lưu dự phòng đa tầng toàn bộ các Scene, Tilemaps, MapData và Editor Scripts để bảo vệ tuyệt đối các map.
* **Nội dung công việc hoàn thành:**
  1. Sao lưu nội bộ Scenes: `Assets/Scenes/Backups/MainGame_Backup_20261028_173423.unity`.
  2. Sao lưu kho lưu trữ: `Backups/Maps_Backup_20261028_173423/` (38 tệp, 10,720,988 bytes).
  3. Đóng gói nén: `Backups/Maps_Backup_20261028_173423.zip`.
  4. Dự án đạt 0 lỗi biên dịch.

---

#### 🟢 Ca làm việc 16: Sao Lưu Tiếp Toàn Diện Bản Đồ & Đồng Bộ Khôi Phục Scene An Toàn
* **Thời gian:** 22:34 (Giờ hệ thống ngày 2026-10-28)
* **Mục tiêu:** Sao lưu trạng thái hoàn thiện mới nhất của bản đồ thế giới và cập nhật Scene Safe Backup.
* **Nội dung công việc hoàn thành:**
  1. Đồng bộ `Assets/Scenes/MainGame.unity` (6,249,357 bytes).
  2. Tạo bản lưu `Backups/Maps_Backup_20261028_223458/` và file nén `Backups/Maps_Backup_20261028_223458.zip`.
  3. Dự án đạt 0 lỗi biên dịch.

---

#### 🟢 Ca làm việc 17: Thiết Lập Khung Kỹ Thuật Chuẩn Độc Lập Cho Map 3 (Trục X Dài Gấp Đôi Map 2: 120 x 60)
* **Thời gian:** 01:18 (Giờ hệ thống ngày 2026-10-29)
* **Mục tiêu:** Thiết lập khung kỹ thuật không gian trống cho Map 3: Grid 2D, các Tilemap con trống rỗng phân layer chuẩn xác, Polygon Collider 2D giới hạn camera Cinemachine Confiner, tường bao ranh giới, hệ thống chuyển Scene (LuminaSceneManager) và đăng ký Scenes In Build. Bảo vệ tuyệt đối 100% không đụng chạm đến Map 1 và Map 2 cũ.
* **Nội dung công việc hoàn thành:**
  1. Tạo Scene độc lập `Assets/Scenes/Map3.unity` và Prefab `Assets/Prefabs/Map3/Map_Zone_3.prefab`.
  2. Thiết lập kích thước trục X = 120, Y = 60 (gấp đôi Map 2).
  3. Phân layer Tilemaps: `Tilemap_Ground`, `Tilemap_Water` (Is Trigger), `Tilemap_Walls`, `Tilemap_Cliffs`, `Tilemap_Bridge`, `Tilemap_Props`, `Tilemap_Foreground`.
  4. Giới hạn camera: `Cinemachine_Confiner_Map3` (PolygonCollider2D Is Trigger: [0,0] đến [120,60]).
  5. Cổng dịch chuyển và quản lý Scene: `LuminaSceneManager.cs` và `PortalTrigger.cs`.
  6. Dự án đạt 0 lỗi biên dịch.

---

#### 🟢 Ca làm việc 18: Sao Lưu Toàn Diện Bản Đồ Hiện Tại (Tích Hợp Map 1, Map 2 & Khung Map 3)
* **Thời gian:** 15:33 (Giờ hệ thống ngày 2026-10-29)
* **Mục tiêu:** Thực hiện sao lưu đa tầng (Multi-tier snapshot) bảo vệ toàn vẹn tuyệt đối toàn bộ cấu trúc bản đồ hiện tại của dự án Lumina sau khi người dùng chèn khung Map 3 và lưu Scene `MainGame.unity` (kích thước hiện tại ~11.97 MB tích hợp cả Map 1 `Grid_World`, Map 2 `Map_Zone_2` và Map 3 `Map_Zone_3`), cùng Scene `Map3.unity` và Prefab `Map_Zone_3.prefab`.
* **Nội dung công việc hoàn thành:**
  1. **Kiểm tra trạng thái bản đồ hiện tại trên đĩa:**
     * `Assets/Scenes/MainGame.unity`: 11,974,016 bytes (11.97 MB) - Chứa đầy đủ Map 1 (Làng Oakhaven), Map 2 (Rừng Oakhaven Woods) và Khung kỹ thuật Map 3 (Map_Zone_3 kích thước 120x60).
     * `Assets/Scenes/Map3.unity`: 56,825 bytes - Scene độc lập của Map 3.
     * `Assets/Prefabs/Map3/Map_Zone_3.prefab`: 47,422 bytes - Prefab khung kỹ thuật Map 3.
     * `Assets/Scenes/Forest_Demo_Map.unity`: 4,063,771 bytes - Bản đồ demo dự phòng.
  2. **Thực thi sao lưu 3 tầng bảo vệ dự án:**
     * **Tầng 1 - Sao lưu nội bộ Scenes (`Assets/Scenes/Backups/`):**
       - Tạo bản sao timestamp: `Assets/Scenes/Backups/MainGame_Backup_20261029_153317.unity` (11.97 MB).
       - Tạo bản sao timestamp: `Assets/Scenes/Backups/Map3_Backup_20261029_153317.unity`.
       - Đồng bộ bản an toàn tuyệt đối: `Assets/Scenes/Backups/MainGame_SafeBackup.unity` (11.97 MB).
     * **Tầng 2 - Kho lưu trữ độc lập dự án (`Backups/Maps_Backup_20261029_153317/`):**
       - Sao lưu toàn bộ 125 tệp tin (tổng dung lượng giải nén: 17,000,464 bytes ~ 17 MB).
       - Phân loại rõ ràng vào các thư mục con: `Scenes/`, `Prefabs/`, `Tilemaps/`, `MapData/`, `EditorScripts/`.
       - Tự động sinh tệp kiểm định tính toàn vẹn `BACKUP_MANIFEST.json` ghi lại kích thước từng byte và mã SHA-256 của từng file.
       - Mã SHA-256 của `MainGame.unity`: `89258D45CC1C62B2B27CA68C9612FCBEB873033669386C5C63215A14D57AFF4B`.
       - Mã SHA-256 của `Map3.unity`: `381B2BCE2B3E14BC32C66986F9A0835B796CDFE2F4257EE33AEBD7C62E4DF837`.
       - Mã SHA-256 của `Map_Zone_3.prefab`: `FC921EEFBF1FE44F6955CF115049DDC06771D0A3528A4DFE3C8D066BE0297072`.
     * **Tầng 3 - Đóng gói nén an toàn (`Backups/Maps_Backup_20261029_153317.zip`):**
       - Đóng gói toàn bộ kho lưu trữ thành tệp ZIP nén độc lập (dung lượng nén: 693,256 bytes), sẵn sàng phục hồi bất kỳ lúc nào chỉ bằng một thao tác.
  3. **Kiểm tra biên dịch dự án:**
     * `dotnet build Assembly-CSharp.csproj` ➔ **0 Error(s)**.
     * `dotnet build Assembly-CSharp-Editor.csproj` ➔ **0 Error(s)**.
     * Toàn bộ các map và hệ thống hoạt động trơn tru, an toàn tuyệt đối 100%.

---

#### 🟢 Ca làm việc 19: Sao Lưu Tiếp Tục Bản Đồ Thế Giới Sau Khi Vẽ & Mở Rộng Map 3
* **Thời gian:** 14:17 (Giờ hệ thống ngày 2026-10-30)
* **Mục tiêu:** Sao lưu đa tầng (Multi-tier snapshot) toàn bộ dữ liệu bản đồ vừa được người dùng tự tay thiết kế và lưu lại trên Unity Editor (`MainGame.unity` đã tăng dung lượng từ `11.97 MB` lên `14.93 MB` lúc 14:11 ngày 2026-10-30), bảo vệ tuyệt đối tiến độ xây dựng Map 1, Map 2 và Map 3.
* **Nội dung công việc hoàn thành:**
  1. **Kiểm tra trạng thái bản đồ mới nhất trên đĩa:**
     * `Assets/Scenes/MainGame.unity`: `14,928,439 bytes` (~**14.93 MB**, tăng thêm ~2.95 MB dữ liệu thiết kế Map mới so với Ca 18) - Lưu trữ trọn vẹn Map 1 (`Grid_World`), Map 2 (`Map_Zone_2`) và Map 3 (`Map_Zone_3`).
     * `Assets/Scenes/Map3.unity`: `56,825 bytes` - Scene độc lập của Map 3.
     * `Assets/Prefabs/Map3/Map_Zone_3.prefab`: `47,422 bytes` - Prefab khung kỹ thuật Map 3.
     * `Assets/Scenes/Forest_Demo_Map.unity`: `4,063,771 bytes` - Bản đồ demo dự phòng.
  2. **Thực thi sao lưu 3 tầng bảo vệ dự án:**
     * **Tầng 1 - Sao lưu nội bộ Scenes (`Assets/Scenes/Backups/`):**
       - Tạo bản sao timestamp: `Assets/Scenes/Backups/MainGame_Backup_20261030_141729.unity` (`14,928,439 bytes` ~ 14.93 MB).
       - Tạo bản sao timestamp: `Assets/Scenes/Backups/Map3_Backup_20261030_141729.unity` (`56,825 bytes`).
       - Đồng bộ bản an toàn tuyệt đối: `Assets/Scenes/Backups/MainGame_SafeBackup.unity` (`14,928,439 bytes` ~ 14.93 MB).
     * **Tầng 2 - Kho lưu trữ độc lập dự án (`Backups/Maps_Backup_20261030_141729/`):**
       - Sao lưu toàn bộ 125 tệp tin (tổng dung lượng giải nén: `19,954,887 bytes` ~ **19.95 MB**).
       - Sinh tệp kiểm định tính toàn vẹn `BACKUP_MANIFEST.json` với mã SHA-256 của `MainGame.unity`: `31185625B1856A979890592AE10586A501744186AFEED86F77A0F179A05A6D04`.
     * **Tầng 3 - Đóng gói nén an toàn (`Backups/Maps_Backup_20261030_141729.zip`):**
       - Đóng gói toàn bộ kho lưu trữ thành tệp ZIP nén độc lập (`737,935 bytes`).
