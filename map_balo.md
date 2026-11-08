# 🎒 NHẬT KÝ KỸ THUẬT: HỆ THỐNG BẢN ĐỒ & GIAO DIỆN BALO / UI CANVAS PIXEL-PERFECT (DỰ ÁN LUMINA)

**Cập nhật lần cuối:** 14:51 — Ngày 30/10/2026  
**Phiên bản Engine:** Unity `6000.5.10f1` | Universal Render Pipeline (`URP 17.6.0`) | Unity UI (`uGUI 2.5.0`)  
**Nguyên tắc bảo toàn:** Tách biệt độc lập 100% giữa **Dữ liệu Bản đồ (Map 1, Map 2, Map 3)** và **Hệ thống Chức năng Giao diện (`GameUIRoot` Canvas)** — tuyệt đối không ghi đè hay làm xáo trộn các đối tượng Tilemap/Grid trên Scene.

---

## PHẦN I. TRẠNG THÁI BẢN ĐỒ THẾ GIỚI & KHO SAO LƯU HIỆN TẠI

### 1. Thông Số 3 Khu Vực Bản Đồ Trong `MainGame.unity`
- **Tệp Scene chính:** `Assets/Scenes/MainGame.unity` (`14,928,439 bytes` ~ **14.93 MB**)
- **Cấu trúc phân vùng thế giới (`32 PPU`):**
  1. **Map 1 (`Grid_World` - Làng Oakhaven):** Khu vực khởi đầu, NPC hướng dẫn, cổng dịch chuyển sang Map 2.
  2. **Map 2 (`Map_Zone_2` - Rừng Oakhaven Woods & Boss Arena):** Kích thước `60 x 60 tiles`, tọa độ gốc `(-60.9, 33.9, 0)`. Đã tái thiết kế toàn diện theo đúng ảnh mẫu tham chiếu 16-bit: Tấm nền pixel-perfect `map2_ground_plate.png` (PPU 16), Đấu trường Boss Treant khổng lồ trên đài đá tròn, 3 ngôi nhà thời trung cổ góc dưới-trái nối đường đá, núi quặng kim loại và mỏ quặng góc giữa-phải, rừng pha lê phát sáng góc trên-trái, hố đá trũng góc dưới-phải, vòng xoay lối vào có hàng rào gỗ, và bảo tồn đồng bộ 5 Linh Thú (Pets).
  3. **Map 3 (`Map_Zone_3` - Khu vực mở rộng):** Kích thước `120 x 60 tiles` (trục X dài gấp đôi Map 2: `X: 140 .. 260`, `Y: -30 .. 30`), gồm 5 lớp Tilemap chuẩn (`Tilemap_Ground`, `Tilemap_Water` có `CompositeCollider2D IsTrigger`, `Tilemap_Cliffs`, `Tilemap_Bridge`, `Tilemap_Props_Foreground`), tường bao vật lý `Map3_BoundaryWalls` và `CameraConfiner_Map3` (`PolygonCollider2D IsTrigger`).
- **Kho sao lưu an toàn mới nhất (Ca 26 - 31/10/2026):**
  - Bản sao nội bộ: `Assets/Scenes/Backups/MainGame_Backup_20261031_RedesignedMap2.unity` & `Assets/Scenes/Backups/MainGame_SafeBackup.unity` (`16.74 MB`).
  - Kho lưu trữ độc lập: `Backups/Maps_Backup_20261030_141729/` (125 files, ~19.95 MB, kèm `BACKUP_MANIFEST.json` kiểm định mã SHA-256).
  - Gói nén dự phòng: `Backups/Maps_Backup_20261030_141729.zip` (`737,935 bytes`).

---

## PHẦN II. HỆ THỐNG GIAO DIỆN UNITY UI CANVAS – PIXEL PERFECT & BALO 20 Ô

### 1. Chuẩn Thiết Lập Canvas & Hiển Thị Sắc Nét
- **Canvas gốc (`GameUIRoot`):**
  - `Render Mode`: `Screen Space - Overlay`
  - `Canvas Scaler`: `Scale With Screen Size` | `Reference Resolution = 1920 x 1080` | `Screen Match Mode = Match Width Or Height (0.5)`
  - `Graphic Raycaster`: `Enabled`
- **Chuẩn tọa độ Số Nguyên (Integer Pixel-Perfect):**
  - Toàn bộ `RectTransform` (`anchoredPosition`, `sizeDelta`, `offsetMin`, `offsetMax`) được làm tròn thành **số nguyên (Integer)** qua `FontManager.SnapRectToIntegers()` và `FontManager.SanitizeUIHierarchy()`.
  - Mọi đối tượng UI và Text đều khóa `localScale = (1, 1, 1)`.
- **Tối ưu Sprite & Khung 9-Slice:**
  - Toàn bộ Sprite UI trong `Assets/Sprites/UI` được cấu hình: `Texture Type = Sprite (2D and UI)`, `Filter Mode = Point (No Filter)`, `Compression = None (Uncompressed)`, `Generate Mip Maps = Disabled`.
  - Toàn bộ khung bảng (Panel), ô chứa (Slot) và nút bấm sử dụng `Image.Type.Sliced` (9-Slice Border `4px` – `8px`) để giữ viền sắc nét ở mọi kích thước.
  - Mọi Icon vật phẩm, trang bị, kỹ năng đều bật `preserveAspect = true`.
- **Tối ưu Font Chữ & Dấu Tiếng Việt (`FontManager.cs` & `TimesNewRomanFontSetup.cs`):**
  - Sử dụng `TextMeshProUGUI` với `TimesNewRoman SDF` (Dynamic SDF, **Font Atlas `2048 x 2048`**) kết hợp tự động `LiberationSans SDF` trong `fallbackFontAssetTable`.
  - Tự động bật `extraPadding = true` trên tất cả `TMP_Text` để không bị cắt dấu tiếng Việt ở rìa khung.
  - Phân cấp cỡ chữ chuẩn:
    - Tiêu đề bảng (Header): `18 - 20 pt` (Bold, Vàng đồng `#FFE066`)
    - Tên mục / Chỉ số: `13 - 14 pt` (Trắng ngà `#F7F2E6` / Xanh ngọc `#00E5FF`)
    - Mô tả chi tiết / Số lượng: `11 - 12 pt`

---

### 2. Bảng Balo Hành Trang 20 Ô (`InventoryBagUI.cs`) — Xử Lý Dứt Điểm Lỗi Ô Trắng
- **Vị trí & Kích thước:**
  - Neo **Bên Phải (Middle-Right)**: `Anchor = (1, 0.5)`, `Pivot = (1, 0.5)`, `anchoredPosition = (-16, -24)`, `sizeDelta = 268 x 436 px`.
  - Nằm gọn trong khoảng trống bên dưới khung Minimap (`Y = -16 .. -186`) và phía trên cụm 6 ô kỹ năng (`Y = 16 .. 72`).
  - Phím tắt đóng/mở: **`[I]`** hoặc **`[B]`** hoặc nút **`Túi Đồ [I]`** ở góc dưới trái.
- **Cấu trúc Lưới Vật Phẩm (`GridLayoutGroup`):**
  - Chia đúng **4 cột x 5 hàng = 20 ô** (`constraint = FixedColumnCount`, `constraintCount = 4`).
  - Kích thước ô vuông đồng nhất: `cellSize = 52 x 52 px`, khoảng cách `Spacing = (6, 6)`, `Padding = (10, 10, 8, 8)`.
- **Nguyên nhân & Cách khắc phục triệt để lỗi ô trắng xóa:**
  - *Nguyên nhân cũ:* Các danh sách `slotIconImages`, `slotQuantityTexts`, `slotButtons` trước đây để `private` không có `[SerializeField]`. Khi `GameUIRoot` đã lưu trong Scene chạy vào Play Mode, `EnsureUI` bỏ qua bước tạo lại khiến danh sách tham chiếu bị rỗng (`Count = 0`), làm các `Image` con `Icon` không có Sprite hiển thị màu trắng mặc định (`Color.white`).
  - *Giải pháp đã triển khai:*
    1. Thêm `[SerializeField]` cho `slotIconImages`, `slotQuantityTexts`, `slotButtons` và bổ sung hàm `EnsureSlotReferencesBound()` tự động quét và gắn lại 100% tham chiếu từ 20 ô con bất cứ khi nào cần.
    2. Gán Sprite nền 9-Slice tối màu (`FontManager.SlotEmptyBgColor`) cho toàn bộ 20 `ItemSlot_00` .. `ItemSlot_19`.
    3. Khi ô trống hoặc chưa có vật phẩm: ép `iconImg.sprite = null; iconImg.color = Color.clear; iconImg.enabled = false;` để tuyệt đối không bao giờ hiện hình vuông trắng.
- **Hiển thị Số Lượng & Footer Balo:**
  - Nhãn số lượng (`x1`, `x5`) đặt ở góc dưới phải của ô (`TextAlignmentOptions.BottomRight`, cỡ chữ `11`, Bold), gắn `Outline` đen mảnh `(1, -1)` nổi bật trên nền icon.
  - Thanh **Footer** dưới đáy bảng: Bên trái hiển thị **`Vàng: 1,500 G`** (căn trái), bên phải là nút **`[Sắp Xếp]`** (căn phải, tự động gom nhóm và dồn các ô vật phẩm lên đầu).

---

### 3. Bố Cục Tổng Thể Toàn Màn Hình `1920 x 1080` (Không Chồng Lấn)

#### A. 4 Cụm HUD Cố Định Sát 4 Góc Màn Hình (Cách lề `16px`)
1. **Góc Trên - Trái (`TopLeft_PlayerInfo`):**
   - Vị trí `(16, -16)`, kích thước `350 x 108 px`. Gồm khung Avatar `72 x 72`, Tên nhân vật, Cấp độ và 3 thanh HP (Đỏ), MP (Xanh lam), EXP (Xanh lá).
   - Cụm `TopLeft_PetCompanionWidget` được đặt ở `(374, -16)` ngay bên phải `TopLeft_PlayerInfo` để không che khuất Bảng Chỉ Số Nhân Vật.
2. **Góc Trên - Phải (`MinimapSystem2D_Frame`):**
   - Vị trí `(-16, -16)`, kích thước `170 x 170 px`, `RenderTexture` dùng `FilterMode.Point` sắc nét, nút phóng to `[+]` / thu nhỏ `[-]` và nhãn tọa độ `(X, Y)`.
3. **Góc Dưới - Trái (`BottomLeft_MenuCluster`):**
   - Vị trí `(16, 16)`, kích thước `528 x 38 px`, dùng `HorizontalLayoutGroup` (`Spacing = 8px`), 4 nút kích thước đồng nhất `126 x 38 px`:
     - `Đổi Class [K]`
     - `Nhân Vật [C]`
     - `Túi Đồ [I]`
     - `Cài Đặt [ESC]`
4. **Góc Dưới - Giữa (`QuickSlots_Container`) & Góc Dưới - Phải (`BottomRight_SkillBar`):**
   - Cụm 2 ô bình máu/năng lượng (`[E]`, `[R]`) ở giữa đáy `(0, 16)` và cụm 6 ô kỹ năng chiến đấu (`[1]`–`[6]`) ở góc dưới phải `(-16, 16)`.
   - Toàn bộ 8 ô đều có kích thước vuông bằng nhau **`56 x 56 px`**, khoảng cách `Spacing = 8px`, hiển thị rõ Icon (`preserveAspect = true`), phím tắt, MP tiêu hao và vòng xoay hồi chiêu (`Radial360`).

#### B. 3 Bảng Chức Năng Chính Mở Cùng Lúc Không Chồng Lấn
| Tên Bảng Chức Năng | Script Điều Khiển | Phím Tắt | Vị Trí `anchoredPosition` | Kích Thước `sizeDelta` | Vùng Tọa Độ Ngang (`1920px`) |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **1. Bảng Chỉ Số Nhân Vật** | `CharacterStatusUI.cs` | `[C]` / `[U]` | `(16, -18)` (Middle-Left) | `350 x 596 px` | `X = 16 .. 366` |
| **2. Bảng Chuyển Đổi 14 Class** | `ClassDemoSwitcherUI.cs` | `[K]` / `[F1]` | `(40, 14)` (Center) | `1210 x 720 px` | `X = 395 .. 1605` |
| **3. Bảng Balo Hành Trang 20 Ô** | `InventoryBagUI.cs` | `[I]` / `[B]` | `(-16, -24)` (Middle-Right) | `268 x 436 px` | `X = 1636 .. 1904` |

> **Khoảng hở ngăn cách khi mở cả 3 bảng cùng lúc:**
> - Giữa **Bảng Chỉ Số Nhân Vật** (`X = 366`) và **Bảng Chuyển Đổi 14 Class** (`X = 395`): Cách nhau **`29 px`** thoáng đãng.
> - Giữa **Bảng Chuyển Đổi 14 Class** (`X = 1605`) và **Bảng Balo 20 Ô** (`X = 1636`): Cách nhau **`31 px`** thoáng đãng.

---

### 4. Danh Mục Prefab Giao Diện & Công Cụ Editor Đã Thiết Lập
- **Các Prefab UI tái sử dụng (`Assets/Prefabs/UI/`):**
  - `Assets/Prefabs/UI/GameUIRoot.prefab` — Prefab Canvas tổng hợp đầy đủ HUD và 3 bảng chức năng.
  - `Assets/Prefabs/UI/ClassSlot_Prefab.prefab` — Ô danh sách Class (`320 x 50 px`, 9-Slice, viền sáng xanh ngọc).
  - `Assets/Prefabs/UI/ItemSlot_Prefab.prefab` — Ô vật phẩm Balo (`52 x 52 px`, 9-Slice, Icon `preserveAspect`, số lượng góc dưới phải có viền đen).
  - `Assets/Prefabs/UI/SkillSlot_Prefab.prefab` — Ô kỹ năng chiến đấu (`56 x 56 px`, gắn `SkillSlotUI`).
  - `Assets/Prefabs/UI/EquipmentSlot_Prefab.prefab` — Ô trang bị nhân vật (`156 x 76 px`, Icon `44 x 44` bên trái, chữ thông số `Word Wrapping` bên phải).
- **Menu Công cụ Editor an toàn (Không đụng đến Map):**
  - `Tools -> Lumina -> Upgrade UI Canvas Quality & Layout (Safe - No Map Touch)` (trong `Assets/Editor/BakeNewFantasyHUD.cs`).

---

### 5. Báo Cáo Sửa Lỗi Không Thể Mở UI (Hotkeys & HUD Buttons Fix)
- **Nguyên nhân dứt điểm:**
  1. *Vòng đời MonoBehaviour khi GameObject bị Inactive:* `CharacterStatusUI` và `InventoryBagUI` trước đây được gắn trực tiếp trên GameObject Panel con (`CharacterStatusUI_Panel` và `InventoryBagUI_Panel`). Khi gọi `panelRoot.SetActive(false)`, toàn bộ script bị vô hiệu hóa, khiến hàm `Update()` không bao giờ được gọi -> Mất toàn bộ phím tắt `[C]`, `[U]`, `[I]`, `[B]`.
  2. *Xung đột phím tắt `[C]` trong `PlayerController.cs`:* `PlayerController.HandleInputs()` bắt cứng phím `[C]` để thu phục quái thú (`TryCaptureNearbyEnemy()`), in ra cảnh báo yêu cầu Lv.10 và chặn không cho mở bảng Nhân Vật.
  3. *Điều phối tập trung tại `GameplayHUD.cs`:* `GameplayHUD` luôn luôn Active trên Canvas nhưng trước đây hàm `Update()` chưa bắt phím tắt của 4 bảng chức năng.
- **Giải pháp kỹ thuật đã triển khai:**
  1. Đưa `CharacterStatusUI` và `InventoryBagUI` lên Canvas cha (`GameUIRoot`), chỉ bật/tắt GameObject con `panelRoot`. Nhờ đó MonoBehaviour luôn chạy `Update()` 24/7.
  2. Cập nhật `GameplayHUD.Update()` bắt tập trung: `[C]`/`[U]` (Nhân vật), `[I]`/`[B]` (Balo), `[K]`/`[F1]` (Đổi Class), `[ESC]` (Pause/Cài đặt).
  3. Gắn lại sự kiện `onClick` cho 4 nút menu dưới-trái (`Btn_Character`, `Btn_Inventory`, `Btn_DemoClass`, `Btn_Menu`).
  4. Cập nhật `PlayerController.cs`: Phím `[C]` chỉ can thiệp thu phục khi thực sự có quái thú máu < 35% trong tầm 3m; bình thường nhường trọn vẹn cho bảng Nhân Vật `[C]`. Bổ sung phím `[V]` cho tính năng Thu phục độc lập.
  5. Cập nhật `PauseMenuUI.cs`: Phím `[ESC]` ưu tiên đóng các bảng chức năng đang mở trước khi kích hoạt Pause Menu.
- **Bảo toàn Map:** Tuyệt đối không đụng chạm đến bất kỳ Map hay Tilemap nào trong Scene (`MainGame.unity` 14.93 MB giữ nguyên 100%).


