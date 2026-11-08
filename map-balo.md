# NHẬT KÝ PHÁT TRIỂN: HỆ THỐNG BẢNG CHỈ SỐ [U], BALO TÚI ĐỒ [I] VÀ MINIMAP 2D

- **Ngày thực hiện:** 21/10/2026
- **Dự án:** Action RPG Lumina
- **Trạng thái:** ✅ Hoàn thành 100% – 0 lỗi biên dịch (0 Compilation Errors)

---

## 1. MỤC TIÊU NHIỆM VỤ
Tự động sinh giao diện (Procedural UI Generation) hoàn chỉnh trong game mà không cần kéo thả thủ công trên Unity Editor. Đảm bảo khi bấm Play ở bất kỳ Scene nào hoặc khi sử dụng công cụ Editor 1-Click, hệ thống sẽ tự động cấu hình đầy đủ cả 3 giao diện:
1. **Bảng Chỉ Số Nhân Vật [U]** (bên trái màn hình).
2. **Balo Túi Đồ 20 Ô [I]** (bên phải màn hình).
3. **Bản Đồ Nhỏ Minimap 2D** (góc trên bên phải màn hình).

---

## 2. CHI TIẾT CÁC HỆ THỐNG ĐÃ XÂY DỰNG

### 2.1. Bảng Chỉ Số Nhân Vật – Phím [U] (`CharacterStatusUI.cs`)
- **Vị trí & Kích thước:** Neo ở cạnh trái màn hình (`anchorMin = (0, 0.5)`, `sizeDelta = (340, 530)` px, cách lề trái 25 px).
- **Phong cách đồ họa:** Nền tối huyền bí (`#12141C`) phối viền vàng đồng (`#D9B340`) sắc nét, đồng bộ chuẩn Pixel Art 16-bit.
- **Cơ chế Bật/Tắt:**
  - Bấm phím **`U`** trên bàn phím.
  - Hoặc click vào nút **`Nhân vật [U]`** ở cụm menu góc dưới bên trái HUD.
  - Có nút **`[X]`** màu đỏ góc trên phải để đóng bảng.
- **Dữ liệu thời gian thực (Real-time Stats):**
  - **Thông tin cơ bản:** Khung avatar chân dung + Sprite class, Tên nhân vật (*Hiệp Sĩ Lumina*), Hệ phái (*Tân Thủ / Xạ Thủ / Pháp Sư / Chiến Binh...*), Cấp độ (*Lv. X*), Kinh nghiệm (*EXP: X / Y*) kèm thanh tiến trình EXP Bar mini.
  - **6 Chỉ số chiến đấu:**
    - Sinh Lực (HP): Cập nhật theo `CharacterStats.currentHp` / `maxHp`.
    - Năng Lượng (MP): Cập nhật theo `CharacterStats.currentMp` / `maxMp`.
    - Sát Thương (ATK): Cập nhật theo `CharacterStats.TotalAtk` + hiển thị chỉ số cộng thêm từ Vũ Khí `(+12)`.
    - Phòng Thủ (DEF): Cập nhật theo `CharacterStats.TotalDef` + hiển thị chỉ số cộng thêm từ Giáp & Mũ `(+13)`.
    - Tốc Độ Di Chuyển (Speed): Cập nhật theo `CharacterStats.TotalMoveSpeed` + hiển thị chỉ số cộng từ Giày `(+1.0)`.
    - Tỷ Lệ Chí Mạng (Crit Rate): Mặc định hiển thị `15%` (hoặc cao hơn theo trang bị).
  - **4 Ô Trang Bị Tương Tác Hai Chiều (Click để tháo vào Balo):**
    - Ô 1: **Vũ Khí** (Mặc định tự động nhận diện vũ khí phù hợp theo nhánh: **Kiếm Sắt** cho Cận Chiến/Tân Thủ, **Cung Săn Rừng** cho Xạ Thủ, **Gậy Pháp Sư** cho Pháp Sư; click vào ô để tháo vũ khí cất lại vào Balo).
    - Ô 2: **Áo Giáp** (Mặc định trang bị **Giáp Da Tân Thủ** +8 DEF; click vào ô để tháo).
    - Ô 3: **Mũ** (Mặc định trang bị **Mũ Da Tân Thủ** +5 DEF; click vào ô để tháo).
    - Ô 4: **Giày** (Mặc định trang bị **Giày Da Tân Thủ** +1.0 Tốc độ; click vào ô để tháo).
    - Hiển thị đầy đủ Tên trang bị màu vàng kim, chỉ số phụ màu xanh ngọc (`+12 Sát Thương [Cận Chiến]`, `+10 Sát Thương [Xạ Thủ]`, `+10 Sát Thương [Pháp Sư]`, `+8 Phòng Thủ`...) hoặc nhãn `[Trống - Click Balo để mặc]`.
  - **Cơ Chế Phân Chia Vũ Khí Theo Nhánh Hệ Phái (Class Branch Weapon Restrictions):**
    - **Gậy Pháp Sư (Staff):** Chỉ dành riêng cho nhánh **Pháp Sư (Mage)**. Nếu nhánh khác cố gắng trang bị sẽ bị chặn với thông báo: *"Gậy Pháp Sư chỉ có thể sử dụng cho nhánh Pháp Sư!"* và phát âm thanh lỗi.
    - **Cung (Bow):** Chỉ dành riêng cho nhánh **Xạ Thủ (Archer)**. Nếu nhánh khác cố gắng trang bị sẽ bị chặn với thông báo: *"Cung chỉ có thể sử dụng cho nhánh Xạ Thủ!"*.
    - **Kiếm (Sword):** Dành riêng cho nhánh còn lại: **Cận Chiến (Chiến Binh & Tân Thủ)**. Pháp Sư và Xạ Thủ không thể trang bị kiếm.
    - **Tự động thích ứng khi đổi Class:** Khi người chơi chuyển nghề hoặc đổi class qua phím [F1], nếu vũ khí đang cầm trên tay không tương thích với nhánh mới, hệ thống sẽ tự động cất vũ khí cũ vào Balo và trang bị vũ khí chuẩn của nhánh mới.

---

### 2.2. Balo / Túi Đồ 20 Ô – Phím [I] (`InventoryBagUI.cs`)
- **Vị trí & Kích thước:** Neo ở cạnh phải màn hình (`anchorMin = (1, 0.5)`, `sizeDelta = (380, 520)` px, cách lề phải 25 px).
- **Phong cách đồ họa:** Nền tối viền vàng đồng bộ, thanh tiêu đề `BALO HÀNH TRANG (20 Ô)`, nút `[X]` đóng bảng.
- **Cơ chế Bật/Tắt:**
  - Bấm phím **`I`** hoặc **`B`** trên bàn phím.
  - Hoặc click vào nút **`Túi đồ [I]`** ở cụm menu góc dưới bên trái HUD.
- **Lưới 20 Ô Hành Trang:**
  - Sử dụng `GridLayoutGroup` chia thành 5 cột x 4 hàng.
  - Mỗi ô vuông kích thước `58x58` px, khung viền pixel art (`frame_skill_slot.png`), icon vật phẩm ở giữa và số lượng `Quantity Text` ở góc dưới phải.
  - Các ô vũ khí hiển thị rõ nhãn phân loại: **KIẾM** (vàng), **CUNG** (xanh lá), **GẬY** (xanh lam).
- **Danh sách vật phẩm & Trang bị khởi đầu:**
  - **Ô 1:** Bình Máu Đỏ (x5) – click vào tự động hồi +50 HP.
  - **Ô 2:** Bình Mana Xanh (x5) – click vào tự động hồi +40 MP.
  - **Ô 3:** Thảo Mộc Xanh (x12).
  - **Ô 4:** Quặng Đá Tím (x8).
  - **Ô 5:** Quả Cầu Tinh Linh (x3).
  - **Ô 6:** **Kiếm Sắt Tân Thủ** [KIẾM - Nhánh Cận Chiến] (ATK +12) – Click để trang bị (chỉ cho Tân Thủ / Chiến Binh).
  - **Ô 7:** **Cung Săn Rừng** [CUNG - Nhánh Xạ Thủ] (ATK +10, Tốc độ +0.5) – Click để trang bị (chỉ cho Xạ Thủ).
  - **Ô 8:** **Gậy Pháp Sư** [GẬY - Nhánh Pháp Sư] (ATK +10, MP +30) – Click để trang bị (chỉ cho Pháp Sư).
  - **Ô 9:** **Giáp Da Thợ Săn** [ÁO GIÁP] (DEF +8, HP +40) – Click để trang bị / hoán đổi giáp.
  - **Ô 10:** **Mũ Da Thợ Săn** [MŨ] (DEF +5) – Click để trang bị / hoán đổi mũ.
  - **Ô 11:** **Giày Da Tân Thủ** [GIÀY] (Tốc độ +1.0, DEF +3) – Click để trang bị / hoán đổi giày.
  - **Ô 12 đến 20:** Ô trống sẵn sàng nhận trang bị và vật phẩm rơi.
- **Cơ chế Trang Bị / Tháo / Hoán Đổi (Equip & Swap có kiểm tra Nhánh):**
  - Click vào bất kỳ món Vũ Khí / Trang Bị nào trong Balo sẽ kiểm tra điều kiện nhánh trước khi trang bị.
  - Nếu đúng nhánh: Tự động mặc lên người, nếu đã có vũ khí cũ thì hoán đổi (Swap) về lại Balo.
  - Nếu sai nhánh: Chặn ngay lập tức, phát âm thanh lỗi và hiện thông báo từ chối rõ ràng.
- **Thanh dưới cùng (Bottom Bar):**
  - Hiển thị tiền tệ: **`Vàng: 1,500 G`** (màu vàng kim rực rỡ).
  - Nút **`[Sắp Xếp]`**: Gom tất cả các ô có trang bị và vật phẩm lên đầu (ưu tiên Trang bị -> Bình thuốc -> Nguyên liệu), dọn các ô trống xuống dưới và hiển thị thông báo HUD: *"Đã sắp xếp lại túi đồ gọn gàng!"*.

---

### 2.3. Bản Đồ Nhỏ Minimap 2D Góc Trên Phải (`MinimapSystem2D.cs`)
- **Vị trí & Kích thước:** Khung vuông chuẩn `170x170` px neo cố định ở **Góc Trên Bên Phải (Top-Right)** của Canvas (`anchorMin = (1, 1)`, `anchorMax = (1, 1)`).
- **Khung & Tiêu đề địa danh:** Viền vàng đồng pixel art dày 2px, bên dưới gắn khung chữ tiêu đề: **`"Làng Oakhaven"`**.
- **Cơ chế RenderTexture & Minimap Camera bằng code:**
  - Tự động tạo `RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)` trong bộ nhớ và gán trực tiếp vào `RawImage`.
  - Tự động tạo `GameObject` tên `Minimap_Camera` với `Camera` chiếu Orthographic từ trên cao xuống (`Z = -30f`).
  - `cullingMask` được cấu hình loại trừ UI Layer 5 (`~(1 << 5)`) để camera minimap không tự soi lại giao diện Canvas.
  - Camera tự động tìm và bám sát vị trí nhân vật chính (`Player`) trong hàm `LateUpdate()`.
- **Điểm đánh dấu nhân vật (Player Marker):**
  - Một chấm tròn màu **Xanh Neon Sáng** (`#33FF73`) kích thước `10x10` px có viền trắng đặt tại chính giữa Minimap, giúp người chơi định vị rõ ràng trên mọi địa hình.
- **Nút Zoom Bản Đồ:**
  - Cặp nút nhỏ **`[-]`** và **`[+]`** ở góc trên Minimap.
  - Bấm `[+]`: Thu hẹp tầm nhìn (Zoom In, giảm `orthographicSize`).
  - Bấm `[-]`: Mở rộng tầm nhìn (Zoom Out, tăng `orthographicSize`, tối đa 28).

---

### 2.4. Tích Hợp Hệ Thống HUD & Tự Động Hóa (`GameplayHUD.cs`)
- Cập nhật hàm `EnsureUIHierarchy()` để tự động gọi khởi tạo cả 3 panel:
  - `CharacterStatusUI.EnsureUI(parent)`
  - `InventoryBagUI.EnsureUI(parent)`
  - `MinimapSystem2D.EnsureUI(parent)`
- Cập nhật hàm `HookButtons()`:
  - Nút `Btn_Character` ("Nhân Vật [U]") nối trực tiếp với `CharacterStatusUI.Instance.Toggle()`.
  - Nút `Btn_Inventory` ("Túi Đồ [I]") nối trực tiếp với `InventoryBagUI.Instance.Toggle()`.
- Cập nhật `RuntimeHUDAutoLoader.EnsureHUDInScene()`: Khi vào bất kỳ Scene nào ở Play Mode, hệ thống HUD và cả 3 giao diện sẽ tự động được sinh ra trong `GameUIRoot` nếu chưa có.

---

### 2.5. Công Cụ Editor 1-Click & Bộ Kiểm Thử Tự Động
1. **Menu Editor 1-Click:**
   - **Đường dẫn menu:** `Tools -> Lumina -> Setup Stats, Inventory & Minimap UI`
   - **Tác vụ:** Tự động kiểm tra Canvas, GameplayHUD và khởi tạo sẵn 3 Panel trên Hierarchy, lưu dirty Scene chỉ bằng một click chuột.
2. **Bộ Kiểm Thử Toàn Diện (Unit Test):**
   - **Đường dẫn menu:** `Tools -> Lumina -> Run Stats, Inventory & Minimap UI Test`
   - **Nội dung kiểm tra:**
     - Test 1: Kiểm tra sự tồn tại của toàn bộ Sprite tài nguyên.
     - Test 2: Kiểm tra cấu trúc phân cấp và hiển thị thời gian thực của `CharacterStatusUI`.
     - Test 3: Kiểm tra cấu trúc 20 ô và 5 vật phẩm mẫu của `InventoryBagUI`.
     - Test 4: Kiểm tra khung 170x170 px, RenderTexture và Camera của `MinimapSystem2D`.
     - Test 5: Kiểm tra liên kết sự kiện nút bấm trong `GameplayHUD`.

---

### 2.6. Tích Hợp Icon Pixel Art Mới Cho Vật Phẩm Balo
- **Vật phẩm 1: Quặng Đá Tím (Purple Ore Crystal):**
  - Đã lưu file ảnh pixel art gốc vào: `Assets/Sprites/Items/icon_material_ore_purple.png` và đè lên `Assets/Sprites/Items/icon_material_ore_chunk.png`.
  - Cấu hình file `.meta` chuẩn Sprite 2D (Single sprite, `filterMode: 0` - Point filter sắc nét, `pixelsPerUnit: 32`, không nén).
  - Gán vào Ô 4 trong Balo [I] với số lượng x8.
- **Vật phẩm 2: Thảo Mộc Xanh (Blue Jade Herb):**
  - Đã lưu file ảnh pixel art gốc vào: `Assets/Sprites/Items/icon_material_herb_blue.png` và đè lên `Assets/Sprites/Items/icon_material_herb_leaf.png`.
  - Cấu hình file `.meta` chuẩn Sprite 2D (Single sprite, `filterMode: 0` - Point filter sắc nét, `pixelsPerUnit: 32`, không nén).
  - Gán vào Ô 3 trong Balo [I] với số lượng x12.
- **Cơ chế Fallback Runtime Siêu Bền (Ultra-Reliable Fallback):**
  - Cả `InventoryBagUI.cs` và `CharacterStatusUI.cs` đều được trang bị cơ chế nạp Sprite thông minh: Ưu tiên `AssetDatabase.LoadAssetAtPath<Sprite>` trong Editor, đồng thời tích hợp fallback đọc trực tiếp file byte PNG bằng `System.IO.File.ReadAllBytes` -> `Texture2D.LoadImage` -> `Sprite.Create(..., FilterMode.Point)`.
  - Đảm bảo 100% hình ảnh icon luôn hiển thị ngay lập tức, không bị mờ và không phụ thuộc vào trạng thái import của Editor.

---

## 3. DANH SÁCH FILE THAY ĐỔI VÀ TẠO MỚI

| STT | Đường Dẫn File | Trạng Thái | Mô Tả |
|:---:|:---|:---:|:---|
| 1 | `Assets/Scripts/UI/Character/CharacterStatusUI.cs` | **Tạo mới / Cập nhật** | Script điều khiển Bảng chỉ số nhân vật [U] & fallback load icon |
| 2 | `Assets/Scripts/UI/InventoryBagUI.cs` | **Tạo mới / Cập nhật** | Script điều khiển Balo túi đồ 20 ô [I] & fallback load icon |
| 3 | `Assets/Scripts/UI/MinimapSystem2D.cs` | **Tạo mới** | Script điều khiển Bản đồ nhỏ Minimap 2D |
| 4 | `Assets/Scripts/UI/Gameplay/GameplayHUD.cs` | **Cập nhật** | Nối nút bấm góc dưới trái & tích hợp tự sinh 3 giao diện |
| 5 | `Assets/Editor/SetupLuminaStatsInventoryMinimapUI.cs` | **Tạo mới** | Menu Editor 1-Click Setup trong Unity |
| 6 | `Assets/Editor/StatsInventoryMinimapTest.cs` | **Tạo mới / Cập nhật** | Bộ Unit Test tự động kiểm thử toàn diện |
| 7 | `Assets/Sprites/Items/icon_material_ore_purple.png` | **Thêm mới** | Icon Pixel Art Quặng Đá Tím |
| 8 | `Assets/Sprites/Items/icon_material_herb_blue.png` | **Thêm mới** | Icon Pixel Art Thảo Mộc Xanh |
| 9 | `map-balo.md` | **Cập nhật** | Nhật ký chi tiết tiến độ phát triển |

---

## 4. HƯỚNG DẪN KIỂM TRA TRONG UNITY

1. **Cách 1 – Sử dụng ngay khi Play game:**
   - Mở bất kỳ Scene nào (ví dụ: `Forest_Demo_Map` hoặc `MainGame`).
   - Bấm nút **Play**.
   - Bấm phím **`U`** để bật/tắt Bảng chỉ số nhân vật.
   - Bấm phím **`I`** để bật/tắt Balo túi đồ (quan sát thấy ngay icon sắc nét của Quặng Đá Tím ở Ô 4 và Thảo Mộc Xanh ở Ô 3).
   - Quan sát góc trên bên phải để xem **Minimap 2D** theo sát bước chân nhân vật, bấm `[+]`/`[-]` để zoom.
2. **Cách 2 – Dựng sẵn trên Hierarchy qua Editor Menu:**
   - Trên thanh menu Unity, chọn: `Tools -> Lumina -> Setup Stats, Inventory & Minimap UI`.
   - Một bảng thông báo xác nhận thành công sẽ hiện lên và 3 giao diện được đưa vào Scene ngay lập tức.
3. **Cách 3 – Chạy kiểm thử tự động:**
   - Trên thanh menu Unity, chọn: `Tools -> Lumina -> Run Stats, Inventory & Minimap UI Test`.
   - Xem kết quả các bài kiểm thử Pass hoàn toàn trong Console log.
