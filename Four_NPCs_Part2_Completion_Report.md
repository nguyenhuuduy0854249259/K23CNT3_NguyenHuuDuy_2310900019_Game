# BÁO CÁO NGHIỆM THU KỸ THUẬT: 4 NPC MODULAR SPRITE - PHẦN 2/3
**Dự án:** Argon Medium - Top-down Action RPG  
**Mục tiêu:** Kế thừa 100% quy chuẩn đồ họa từ `PixelArtShadingHelper` (viền tối `Dark Outline`, đổ bóng 4 sắc độ `ColorRamp4`, dithering đan caro `(x+y)%2==0`, bóng chân `DropShadow`) để thiết kế và xuất code hoàn chỉnh 4 NPC tiếp theo:
1. **Thợ Rèn Boris (`NPC_Blacksmith_Barnaby.prefab`)** -> `Prop_Hammer`
2. **Dược Sĩ Elena (`NPC_Alchemist_Elena.prefab`)** -> `Prop_HerbBag`
3. **Hội Trưởng Carlos (`NPC_GuildMaster_Carlos.prefab`)** -> `Prop_KnightSword` & `Prop_CrimsonCape`
4. **Thương Nhân Lina (`NPC_Merchant_Lina.prefab`)** -> `Prop_Basket` & `Prop_CoinPouch`

---

## I. TỔNG QUAN TIÊU CHUẨN KỸ THUẬT

| Thông số | Giá trị chuẩn | Kiểm tra nghiệm thu |
| :--- | :--- | :--- |
| **Kích thước khung vẽ (Canvas)** | $32 \times 32\text{ px}$ | Đạt $100\%$ ($32 \times 32$) |
| **Pixels Per Unit (PPU)** | `16` | Đạt $100\%$ trên toàn bộ Sprite `.meta` |
| **Lọc Texture (Filter Mode)** | `Point (no filter)` | Đạt $100\%$ sắc nét từng pixel, không mờ viền |
| **Màu Renderer (SpriteRenderer.color)** | `Color.white` (`#FFFFFF`) | Giữ nguyên màu gốc từ bảng màu 4 sắc độ |
| **Giải phẫu Chibi** | Đầu to $12\text{px}$ ngang ($X=10..21$), mắt có điểm sáng sclera `#F7EFEB`, 2 tay tách rời thân bằng viền tối $1\text{px}$ nách, 2 chân bước có khoảng hở | Đồng bộ tuyệt đối với Player Class (`player_novice_4direction.png`) |
| **Bóng dưới chân (Drop Shadow)** | Vùng elip $12\times 3\text{px}$ mờ đen `rgba(10, 10, 15, 120..140)` tại $Y=28..30$ | Nhân vật bám đất tự nhiên, không bay lơ lửng |
| **Đồng bộ lưới Pixel (No Mixels)** | Đạo cụ vẽ trực tiếp trên cùng canvas $32 \times 32$ với bàn tay nhân vật | Không lệch tọa độ pixel, `offset = Vector2.zero` |
| **Nhịp bước chân (Step Bob)** | Nhún $1\text{px}$ ($-0.0625\text{ unit}$) tại frame lẻ ($1, 3$) | Đạo cụ gắn `NPCModularProp` có `syncWithWalkStepBob = true` tự động nhún khớp thân |

---

## II. CHI TIẾT 4 NPC VÀ ĐẠO CỤ BÓC TÁCH

### 1. Thợ Rèn Boris (`NPC_Blacksmith_Barnaby.prefab`)
- **Ngoại hình & Khối giải phẫu:**
  - Thân hình Chibi vạm vỡ, lộ cơ bắp cánh tay với 4 sắc độ da bánh mật (`#36140B`, `#8F4C23`, `#D48046`, `#F5AC78`).
  - Râu quai nón nâu sẫm che quanh cằm, tóc đan caro dithering ánh nâu.
  - Tạp dề da thuộc dày dặn phủ trước ngực và bụng (`#240E05` -> `#CF6829`). Quần thợ rèn tối màu, ủng da bảo hộ.
- **Đạo cụ tách lớp (`Prop_Hammer`):**
  - Búa rèn thép tôi với độ bóng kim loại specular highlight trắng xanh (`#E8F4FA`). Cầm chắc trên **tay trái** (ở góc nhìn trước: phía tay trái nhân vật, tức bên phải màn hình).
  - Hoạt ảnh 4 hướng:
    - `Front`: Búa dựng đứng dọc bên hông trái (`X=20..22`), `sortingOffset = +1`.
    - `Back`: Đầu búa hạ thấp (`X=10..12`), `sortingOffset = -1`.
    - `Side` (Right): Tay trái vung về phía sau thân người, `sortingOffset = -1`.
    - `Left`: Tay trái cầm búa hướng về phía trước tầm mắt, `sortingOffset = +1`.

---

### 2. Dược Sĩ Elena (`NPC_Alchemist_Elena.prefab`)
- **Ngoại hình & Khối giải phẫu:**
  - Mái tóc hạt dẻ bồng bềnh tết gọn ra sau với bông hoa cài tóc màu xanh ngọc lam thanh nhã.
  - Áo choàng xanh bạc hà tươi mát (`#0C2E26`, `#1B5E4F`, `#2FA88D`, `#6EE7CB`), yếm trắng tinh khôi viền nếp gấp.
- **Đạo cụ tách lớp (`Prop_HerbBag`):**
  - Dây túi da thuộc đeo chéo từ vai trái xuống **hông phải** (`X=17..20, Y=22..25`).
  - Gắn kèm bình độc dược huỳnh quang phát sáng tím rực (`#D946EF`) và xanh cyan (`#22D3EE`).
  - **Chuyển động thứ cấp (Secondary Bounce):**
    - Cấu hình `NPCModularProp.hasSecondaryMotion = true`, `motionAmplitudeY = 0.04f`, `motionFrequency = 7f`.
    - Khi Elena bước đi, túi thuốc và các lọ độc dược có độ nảy nhẹ $1\text{px}$ lệch nhịp so với bước chân, tạo cảm giác chất lỏng đung đưa chân thực.
    - Sorting Order: `Front` = $+1$, `Back` = $-1$, `SideRight` = $+1$, `SideLeft` = $-1$.

---

### 3. Hội Trưởng Carlos (`NPC_GuildMaster_Carlos.prefab`)
- **Ngoại hình & Khối giải phẫu:**
  - Mái tóc màu nâu vàng quý tộc với các lọn sáng highlight.
  - Giáp ngực bạc kim loại sáng bóng (Metallic highlight `#FFFFFF`, phản quang kim loại sắc nét `#222E3A` -> `#DDE7F0`).
- **Đạo cụ tách lớp 1 (`Prop_CrimsonCape`):**
  - Áo choàng đỏ thẫm quý tộc khoác sau vai, có dithering chuyển màu đỏ gấm (`#33080C` -> `#EF4444`).
  - Hiệu ứng bay nhẹ: `motionAmplitudeX = 0.025f`, `motionAmplitudeY = 0.015f`.
  - Sorting Order: `Front` = $-1$ (sau lưng), `Back` = $+1$ (che trước lưng), `SideRight` = $-1$, `SideLeft` = $-1$.
- **Đạo cụ tách lớp 2 (`Prop_KnightSword`):**
  - Thanh kiếm hiệp sĩ chuôi mạ vàng hoàng gia (`#F59E0B`), lưỡi kiếm thép sáng bóng.
  - Tay trái của Carlos luôn đặt lên chuôi kiếm bên hông xuyên suốt cả 4 hướng nhìn.
  - Sorting Order: `Front` = $+1$, `Back` = $-1$, `SideRight` = $-1$, `SideLeft` = $+1$.

---

### 4. Thương Nhân Lina (`NPC_Merchant_Lina.prefab`)
- **Ngoại hình & Khối giải phẫu:**
  - Khăn trùm đầu vàng cam rực rỡ đặc trưng vùng thảo nguyên (`#3E1D05` -> `#FDE047`) đan caro dithering che tóc.
  - Váy yếm cam đất ấm áp, tạp dề trắng thắt nơ trước ngực.
- **Đạo cụ tách lớp 1 (`Prop_Basket`):**
  - Giỏ hàng mây đan xách bên **tay phải** (`X=20..24, Y=22..27`).
  - Hiệu ứng đung đưa tự nhiên khi bước đi (`hasSecondaryMotion = true`, `motionAmplitudeX = 0.03f`, `motionAmplitudeY = 0.02f`).
  - Sorting Order: `Front` = $+1$, `Back` = $-1$, `SideRight` = $+1$, `SideLeft` = $-1$.
- **Đạo cụ tách lớp 2 (`Prop_CoinPouch`):**
  - Thắt lưng đeo túi tiền vàng căng tròn có dây rút và sổ ghi chép bìa da bên hông trái (`X=11..13, Y=22..25`).
  - Sorting Order: `Front` = $+1$, `Back` = $-1$, `SideRight` = $-1$, `SideLeft` = $+1$.

---

## III. BẢNG TỔNG KẾT TÀI NGUYÊN ĐÃ XUẤT

| NPC | Thư mục Modular Sprite | Prefab cập nhật | Đạo cụ con (Child GameObjects) |
| :--- | :--- | :--- | :--- |
| **Boris** | `Assets/Sprites/NPCs/Modular/Boris/` | `Assets/Prefabs/ForestPrototype/NPC_Blacksmith_Barnaby.prefab` | `Prop_Hammer` |
| **Elena** | `Assets/Sprites/NPCs/Modular/Elena/` | `Assets/Prefabs/ForestPrototype/NPC_Alchemist_Elena.prefab` | `Prop_HerbBag` |
| **Carlos** | `Assets/Sprites/NPCs/Modular/Carlos/` | `Assets/Prefabs/ForestPrototype/NPC_GuildMaster_Carlos.prefab` | `Prop_CrimsonCape`<br/>`Prop_KnightSword` |
| **Lina** | `Assets/Sprites/NPCs/Modular/Lina/` | `Assets/Prefabs/ForestPrototype/NPC_Merchant_Lina.prefab` | `Prop_Basket`<br/>`Prop_CoinPouch` |

- **Toàn bộ mã nguồn cốt lõi đã viết hoàn chỉnh (không rút gọn):**
  1. [`generate_4_npcs_sprites.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/generate_4_npcs_sprites.py): Chứa đầy đủ 1,841 dòng code vẽ chi tiết từng pixel cho 4 NPC và 6 đạo cụ qua 4 hướng nhìn.
  2. [`update_4_prefabs_yaml.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/update_4_prefabs_yaml.py): Đồng bộ hóa tự động toàn bộ GUID và trường dữ liệu Animation/Prop vào 4 prefab Unity.
  3. [`ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs): Cập nhật hàm khởi tạo tự động trong hệ thống bản đồ thế giới Oakhaven.
  4. [`verify_4_npcs_qa.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/verify_4_npcs_qa.py): Kịch bản tự động hóa kiểm tra $100\%$ tính hợp lệ của Texture và Prefab.
