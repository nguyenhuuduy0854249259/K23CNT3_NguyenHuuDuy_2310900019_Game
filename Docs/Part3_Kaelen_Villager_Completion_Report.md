# Báo Cáo Hoàn Thiện Phần 3/3: Quản Thú Kaelen + Chim Ưng (`Pet_Falcon`) & Cư Dân Oakhaven (`NPC_Villager_Wanderer`)

Báo cáo này tổng hợp toàn bộ quy trình thiết kế đồ họa Pixel Art Chibi $32 \times 32$ ($\text{PPU} = 16$), tách lớp đạo cụ độc lập và lập trình hệ thống C# tương tác cho 2 NPC cuối cùng trong bộ **7 NPC Oakhaven**, kế thừa 100% chuẩn [`PixelArtShadingHelper`](file:///d:/Unity/prj1/gam/Assets/Scripts/Visuals/PixelArtShadingHelper.cs) từ Phần 1 và Phần 2.

---

## 1. Bảng So Sánh Trực Quan (Visual Showcase)

### 1.1. Chi Tiết Phần 3/3: Kaelen, Chim Ưng (`Pet_Falcon`) & Cư Dân Oakhaven (Đội Mũ / Bê Giỏ / Tháo Mũ Tay Không / Đạo Cụ Đặt Đất)
![Part 3 Kaelen, Falcon & Villager Comparison](file:///C:/Users/ASUS/.gemini/antigravity/brain/d3b29ec0-6ca1-430f-8d2d-9199280d206b/part3_kaelen_falcon_villager_comparison.png)

### 1.2. Bảng Tổng Hợp Toàn Bộ 7 NPC Chibi ($32 \times 32$, $\text{PPU}=16$) So Sánh Với Player Class
![All 7 Modular NPCs vs Player Class Master Comparison](file:///C:/Users/ASUS/.gemini/antigravity/brain/d3b29ec0-6ca1-430f-8d2d-9199280d206b/all_7_npcs_vs_player_master_comparison.png)

---

## 2. Chi Tiết Kỹ Thuật 2 NPC & Hệ Thống Tương Tác Đạo Cụ

### 2.1. Quản Thú Kaelen ([`NPC_PetMaster_Kaelen.prefab`](file:///d:/Unity/prj1/gam/Assets/Prefabs/ForestPrototype/NPC_PetMaster_Kaelen.prefab)) + Thực Thể Chim Ưng ([`Pet_Falcon`](file:///d:/Unity/prj1/gam/Assets/Prefabs/ForestPrototype/Pet_Falcon.prefab))
- **Thư mục Sprite Kaelen:** [`Assets/Sprites/NPCs/Modular/Kaelen/`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/Modular/Kaelen) (24 PNGs $32 \times 32$, $\text{PPU}=16$, `FilterMode.Point`).
  - **Ngoại hình Chibi $32 \times 32$:** Đội mũ da thú rừng có **2 tai lông nhỏ** nhô rõ trên đỉnh đầu (`Y=7..9`), viền băng lông trắng kem (`#DCE2EA` / `#F5F8FC`) quanh trán, áo gilê da hươu cổ lông trắng, tay trái đeo găng da chim ưng dày (`GAUNTLET_RAMP`) tại đúng cao độ `PerchPoint`.
  - **Đạo cụ tách lớp (`Prop_FeedPouch`):** Túi thức ăn da đeo bên hông có miệng dây rút xanh lá, tự động đảo `sortingOrder` theo 4 hướng đi (`Down`, `Up`, `Right`, `Left`).
- **Thư mục Sprite Chim Ưng (`Pet_Falcon`):** [`Assets/Sprites/NPCs/Modular/Falcon/`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/Modular/Falcon) (6 PNGs $32 \times 32$, $\text{PPU}=16$, `FilterMode.Point`):
  - `falcon_idle_perched_1.png`, `falcon_idle_perched_2.png`: Tư thế đậu khép cánh trên găng tay, mỏ vàng cong sắc nét, ngực trắng kem đan hạt dithering, mắt có điểm sáng.
  - `falcon_fly_loop_1.png` .. `falcon_fly_loop_4.png`: Chu kỳ 4 frame vỗ cánh bay lượn (cánh nâng cao $\rightarrow$ vỗ ngang $\rightarrow$ đập xuống $\rightarrow$ thu lướt).
- **Script Máy Trạng Thái [`FalconBehaviorController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Town/FalconBehaviorController.cs):**
  - **`FalconState.Perched`:** Đậu yên trên cổ tay trái của Kaelen tại `PerchPoint`, bám sát toạ độ găng tay ngay cả khi Kaelen bước đi, chạy hoạt ảnh `idlePerchedFrames` và đồng bộ `sortingOrder = kaelenRenderer.sortingOrder + 2`.
  - **`FalconState.FlyAround`:** Cất cánh tách khỏi tay, bay lượn vòng tròn quanh Kaelen bằng quỹ đạo lượng giác `Mathf.Cos(orbitAngle) * orbitRadiusX` và `Mathf.Sin(orbitAngle) * orbitRadiusY`, nâng độ cao theo đường cong mượt `Mathf.Sin(0..π)`, tự động `flipX` theo vận tốc ngang và nhấp nhô theo nhịp vỗ cánh.
  - **`FalconState.Landing`:** Tự động thu hẹp khoảng cách bằng `Vector3.MoveTowards` kèm quỹ đạo đáp parabol nhẹ, khớp chính xác về `PerchPoint` (`<= 0.04f` đơn vị) và chuyển lại sang trạng thái `Perched`.

### 2.2. Cư Dân Oakhaven ([`NPC_Villager_Wanderer.prefab`](file:///d:/Unity/prj1/gam/Assets/Prefabs/ForestPrototype/NPC_Villager_Wanderer.prefab)) + Cơ Chế Đặt/Nhặt Đạo Cụ
- **Thư mục Sprite:** [`Assets/Sprites/NPCs/Modular/Villager/`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/Modular/Villager) (50 PNGs $32 \times 32$, $\text{PPU}=16$, `FilterMode.Point`).
  - **Nhân vật gốc (Unarmed & Carrying):**
    - Đầu nhân vật được vẽ **đầy đủ mái tóc nâu hạt dẻ hoàn chỉnh** có đổ bóng 4 sắc độ và đan caro dithering (`npc_villager_body_...` & `npc_villager_carry_...`), đảm bảo khi gọi `TakeOffHat()` nhân vật có mái tóc tự nhiên, tuyệt đối không bị hói.
    - Trang phục áo vải thô màu be (`BEIGE_SHIRT_RAMP`), quần xanh lam (`BLUE_PANTS_RAMP`), giày da nâu và bóng đổ `DropShadow` dưới bàn chân.
    - Hỗ trợ 2 bộ hoạt ảnh 4 hướng (40 sprites thân): bộ đi bộ khi **tay không** (`npc_villager_body_idle/walk_...`) và bộ đi bộ khi **hai tay nâng giỏ trước ngực/hông** (`npc_villager_carry_idle/walk_...`).
  - **2 Child Objects Độc Lập:**
    - `Prop_StrawHat`: Mũ rơm vàng đan caro dithering thắt ruy băng đỏ (`prop_villager_hat_{front,back,side,left,world}.png`).
    - `Prop_FruitBasket`: Giỏ mây đan đầy táo đỏ, cam vàng và nho xanh (`prop_villager_basket_{front,back,side,left,world}.png`).
- **Script Điều Khiển Tương Tác [`VillagerPropInteract.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Town/VillagerPropInteract.cs):**
  - `DropBasket()` / `DropBasket(Vector3 worldPos)`: Tách `Prop_FruitBasket` khỏi Parent (`transform.SetParent(null, true)`), đặt xuống mặt đất (World Space), chuyển sang sprite `prop_villager_basket_world.png` (có bóng đổ dưới đáy giỏ), bật `YSortOrder2D` độc lập và lập tức chuyển bộ hoạt ảnh của NPC sang tư thế **tay không** (`Unarmed`).
  - `PickUpBasket()`: Gắn `Prop_FruitBasket` trở lại vào Parent của NPC (`transform.SetParent(transform, false)`), khôi phục vị trí neo trên tay, chuyển NPC về bộ hoạt ảnh **bê giỏ** (`Carrying`) và đồng bộ lại hướng nhìn.
  - `TakeOffHat()` / `TakeOffHat(Vector3 worldPos)`: Tách `Prop_StrawHat` khỏi Parent (`transform.SetParent(null, true)`), đặt mũ rơm xuống mặt đất tại World Space với sprite `prop_villager_hat_world.png`, để lộ mái tóc nâu đầy đủ của NPC.
  - `PutOnHat()`: Đội `Prop_StrawHat` trở lại lên đầu NPC (`transform.SetParent(transform, false)`), khôi phục `hatEquippedLocalPos` và đồng bộ `sortingOrder` phía trên đầu.

---

## 3. Danh Mục File Đã Tạo & Cập Nhật

1. **Scripts C# (100% Full Code, Không Viết Tắt):**
   - [`Assets/Scripts/Town/FalconBehaviorController.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Town/FalconBehaviorController.cs) — Máy trạng thái 3 pha (`Perched`, `FlyAround`, `Landing`) với quỹ đạo `Mathf.Sin` / `Mathf.Cos` cho Chim Ưng.
   - [`Assets/Scripts/Town/VillagerPropInteract.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Town/VillagerPropInteract.cs) — Cơ chế `DropBasket()`, `PickUpBasket()`, `TakeOffHat()`, `PutOnHat()` tách/gộp Parent (`World Space` $\leftrightarrow$ `Child Transform`) và chuyển đổi bộ hoạt ảnh `Carrying` / `Unarmed`.
   - [`Assets/Scripts/Town/VillagerPropHandler.cs`](file:///d:/Unity/prj1/gam/Assets/Scripts/Town/VillagerPropHandler.cs) — Bổ sung các hàm alias tương thích ngược.
   - [`Assets/Editor/ForestMapGenerator.cs`](file:///d:/Unity/prj1/gam/Assets/Editor/ForestMapGenerator.cs) — Cập nhật trình tạo Prefab cho Kaelen (`FalconBehaviorController`) và Villager (`VillagerPropInteract`).
2. **Scripts Python Sinh Đồ Họa, Cập Nhật Prefab & Kiểm Định QA:**
   - [`Assets/Sprites/NPCs/pixel_art_shading_helper.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/pixel_art_shading_helper.py)
   - [`Assets/Sprites/NPCs/generate_part3_npcs_sprites.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/generate_part3_npcs_sprites.py)
   - [`Assets/Sprites/NPCs/update_part3_prefabs_yaml.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/update_part3_prefabs_yaml.py)
   - [`Assets/Sprites/NPCs/verify_all_7_npcs_qa.py`](file:///d:/Unity/prj1/gam/Assets/Sprites/NPCs/verify_all_7_npcs_qa.py)
3. **Prefabs Đã Đồng Bộ Hoạt Ảnh & Đạo Cụ:**
   - [`Assets/Prefabs/ForestPrototype/NPC_PetMaster_Kaelen.prefab`](file:///d:/Unity/prj1/gam/Assets/Prefabs/ForestPrototype/NPC_PetMaster_Kaelen.prefab)
   - [`Assets/Prefabs/ForestPrototype/Pet_Falcon.prefab`](file:///d:/Unity/prj1/gam/Assets/Prefabs/ForestPrototype/Pet_Falcon.prefab)
   - [`Assets/Prefabs/ForestPrototype/NPC_Villager_Wanderer.prefab`](file:///d:/Unity/prj1/gam/Assets/Prefabs/ForestPrototype/NPC_Villager_Wanderer.prefab)
