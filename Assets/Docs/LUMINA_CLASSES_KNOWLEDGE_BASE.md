# LUMINA: HỆ THỐNG CLASS NHÂN VẬT VÀ CƠ CHẾ CHIẾN ĐẤU TOÀN DIỆN
> **Tài Liệu Cơ Sở Tri Thức (Knowledge Base) Dành Cho Đội Ngũ Phát Triển & AI**  
> **Nguồn Dữ Liệu:** Trích xuất từ 13 tài liệu thiết kế gốc trong `D:\gam\class gam\`  
> **Động cơ:** Unity 2D + C# | Thể loại: Real-time Action RPG kết hợp Pet System  

---

## 1. QUY TẮC PHÁT TRIỂN & TIẾN HÓA NHÂN VẬT
- **Tiến trình phát triển:**
  $$\text{Nhân Vật} \longrightarrow \text{Class Cơ Bản} \longrightarrow \text{Tiến Hóa Class} \longrightarrow \text{Chức Nghiệp Chuyên Môn}$$
- **Nguyên tắc cốt lõi:**
  - Không tăng chỉ số đơn thuần (+10% sát thương, +20% HP).
  - Phần quan trọng nhất là: **"Class vận hành khác biệt như thế nào sau khi tiến hóa?"**
  - **Class:** Quyết định cách nhân vật chiến đấu trên chiến trường.
  - **Profession:** Quyết định hướng chuyên môn hóa, chế tác và kinh tế (Alchemist chế dược phẩm, Blacksmith rèn đúc và chế tạo mod).

---

## 2. MA TRẬN 8 NGUYÊN TỐ & ELEMENTAL REACTION TOÀN GAME
- **8 Hệ Nguyên Tố:** Hỏa (Fire), Thủy (Water), Điện (Electric), Thổ (Earth), Phong (Wind), Độc (Poison), Thánh (Angel), Ma (Demon).
- **Vòng Tròn Khắc Chế:**  
  $$\text{Hỏa} \longrightarrow \text{Phong} \longrightarrow \text{Thổ} \longrightarrow \text{Điện} \longrightarrow \text{Thủy} \longrightarrow \text{Hỏa}$$
  $$\text{Thánh} \longleftrightarrow \text{Ma} \quad \text{(Đôi bên khắc chế lẫn nhau)}$$
- **Hệ số sát thương:**
  - Tấn công trúng hệ bị khắc chế: **$\times 2$ Sát thương**.
  - Tấn công hệ khắc ngược: Giảm nhẹ hiệu lực sát thương.
- **Elemental Reaction Matrix:** Hệ thống dùng chung toàn game. Mọi nguồn nguyên tố tác động lên mục tiêu đều kiểm tra Reaction Matrix chung.

---

## 3. PHÁI CẬN CHIẾN: CẬN CHIẾN TỰ DO (PET HÓA VŨ KHÍ)
- **Class duy nhất:** Cận Chiến Tự Do. Không bị khóa vũ khí cố định.
- **Cơ chế cốt lõi:** **Pet biến thành Vũ khí**
  - *Hình dạng/loài Pet:* Quyết định 1 trong 11 loại vũ khí và nhịp combo.
  - *Element của Pet:* Gắn hiệu ứng nguyên tố lên vũ khí.
- **Thời gian đổi Pet/Vũ khí:** Cooldown **5 giây**.
- **11 Loại Vũ Khí Theo 4 Nhóm Loài Pet:**
  1. *Thú to/nặng (Gấu, Rùa, Hổ):*
     - **Búa Lớn (Great Hammer):** Chậm, nện choáng, phá giáp. (Skill 3: Địa Chấn Nện, Skill 4: Búa Tạ Phá Giáp, Skill 5: Khai Sơn Bạt Địa - Tuyệt Kỹ).
     - **Đại Kiếm (Greatsword):** Quét bán nguyệt rộng, gồng lực Charge 1.5s x2 sát thương. (Skill 3: Đoạn Phong Trảm, Skill 4: Nghịch Kiếm Hất Tung, Skill 5: Khí Kiếm Trảm Nguyệt - Tuyệt Kỹ).
     - **Kiếm & Khiên (Sword & Shield):** Công thủ toàn diện, chặn đòn phản công chuẩn. (Skill 3: Húc Khiên Choáng Váng, Skill 4: Khiên Chắn Phản Đòn, Skill 5: Trận Địa Pháo Đài - Tuyệt Kỹ).
  2. *Thú dài/mảnh (Rắn, Cá, Thằn Lằn):*
     - **Trường Giáo (Spear):** Tầm đâm cực xa, xuyên hàng dọc. (Skill 3: Liên Hoàn Thương Kích, Skill 4: Chống Giáo Đạp Lưng, Skill 5: Long Thương Xuất Động - Tuyệt Kỹ).
     - **Lưỡi Hái Lớn (Great Scythe):** Tầm quét rộng, móc quái về trước mặt. (Skill 3: Lưỡi Quét Tử Thần, Skill 4: Trảm Câu Kéo Quái, Skill 5: Hư Không Xoáy Lốc - Tuyệt Kỹ).
     - **Roi Ma Pháp (Whip):** Vũ khí dẻo, quất ngã quái vật. (Skill 3: Roi Quất Xé Gió, Skill 4: Quấn Chân Quật Ngã, Skill 5: Vũ Điệu Hỗn Loạn - Tuyệt Kỹ).
  3. *Thú nhỏ/nhanh (Chim, Mèo, Chuột):*
     - **Dao Găm / Song Đao (Dual Daggers):** Tốc độ cực cao, luồn sau lưng, chí mạng. (Skill 3: Đoản Đao Xé Thịt, Skill 4: Bộ Pháp Bóng Ma, Skill 5: Vũ Điệu Tuyệt Sát - Tuyệt Kỹ).
     - **Móng Vuốt (Claws):** Tầm sát người, cào xé gây DoT. (Skill 3: Cào Xé Dã Tính, Skill 4: Vồ Mồi Áp Đảo, Skill 5: Cuồng Nộ Thú Tính - Tuyệt Kỹ).
     - **Kiếm Ném / Phi Tiêu (Throwing Darts):** Cận chiến tầm trung, ném boomerang hồi mã thương. (Skill 3: Phi Đao Tản Xạ, Skill 4: Kiếm Khí Hồi Mã, Skill 5: Vạn Tiêu Trận Địa - Tuyệt Kỹ).
  4. *Thú bay (Bướm, Tiên Linh):*
     - **Quạt Chiến (War Fan):** Múa lốc, lướt giật lùi, kiểm soát gió. (Skill 3: Phong Ba Phiến, Skill 4: Vũ Điệu Khinh Linh, Skill 5: Phong Quyển Tàn Hoa - Tuyệt Kỹ).
     - **Dây Xích Ma Thuật (Chains):** Kéo quái lại hoặc đu người tới quái to, giam cầm. (Skill 3: Móc Sắt Tầm Xa, Skill 4: Xích Quét Trùng Phùng, Skill 5: Tỏa Ngục Trầm Uyên - Tuyệt Kỹ).
- **3 Ấn Nội Tại:**
  - *Ấn 1: Binh Khí Đồng Thể:* Đổi Pet (CD 5s) nhận buff *Cường Lực Hoán Vũ* tăng +25% sát thương đòn đầu tiên trong 3s.
  - *Ấn 2: Phủ Ngấn Nguyên Tố:* Chuyển 3 chiêu thức vũ khí sang hệ nguyên tố Pet, hưởng đầy đủ hiệu ứng lõi và x2 sát thương khắc chế.
  - *Ấn 3: Kế Thừa Linh Tính:* Kế thừa 100% chỉ số từ Hidden Passive của Pet biến hình.

---

## 4. PHÁI PHÁP SƯ (5 LỚP CHUYÊN BIỆT)

### 4.1. Pháp Sư Nguyên Tố (Elemental Mage)
- **Cơ chế cốt lõi:** **Hàng đợi 2 ô nguyên tố (Queue 2 slots)**.
  - Ô 1: Nguyên tố Chủ Thể (dạng thức chiêu thức).
  - Ô 2: Nguyên tố Xúc Tác (kích phát reaction).
  - Tạo nên **64 kỹ năng:** 8 Đơn Hệ (tăng +25% dmg và AoE) và 56 Đa Hệ Phản Ứng (kéo dài debuff +1.5s).
- **Thiên Tai (Cataclysm):** Đạt 3 chuỗi combo thành công $\rightarrow$ mở Thiên Tai AoE toàn màn hình mang hệ nguyên tố cuối cùng.
- **3 Ấn Nội Tại:** Lưỡng Nghi Hợp Thành, Mạch Chuỗi Thiên Tai, Linh Thú Xúc Tác.

### 4.2. Pháp Sư Triệu Hồi (Summoner)
- **Cơ chế cốt lõi:** **Song Hành (Dual Summon) – Điều khiển cùng lúc 2 Pet Active**.
  - HUD 6 ô kỹ năng động: Pet 1 cung cấp Slot 1-3, Pet 2 cung cấp Slot 4-6.
  - Mỗi Pet sở hữu 3 kỹ năng chuẩn hóa: Skill 1 (Nhịp nhanh 3-5s), Skill 2 (Role Core 8-12s), Skill 3 (Ultimate 18-25s).
- **Cơ chế HP Pet & Đổi Pet Dự Bị:**
  - Pet có HP riêng; khi về 0 rơi vào trạng thái *Nguy Kịch (Downed)* và khóa 3 ô chiêu tương ứng.
  - Người chơi thao tác nhanh (phím Z/X) thu hồi Pet bị gục và gọi Pet Dự Bị (Reserve) ra sân.
  - *Cân bằng:* Pet gục bị Thời Gian Dưỡng Thương 45–60s; Pet mới ra sân chịu Trễ Nhịp Xuất Hiện 1.0–1.5s; Giới hạn đội hình 2 Active + 2–3 Reserve.
- **3 Ấn Nội Tại:** Cộng Hưởng Lưỡng Thú (+20% reaction dmg), Phân Bổ Chiến Thuật (-15% dmg cho Pet cận chiến, +15% tầm cho Pet tầm xa), Tiếp Ứng Khẩn Cấp (Tạo giáp 20% HP, sóng xung kích 3m khi đổi Pet).

### 4.3. Pháp Sư Thời Không (Chrono Mage)
- **Cơ chế cốt lõi:** Thao túng thời gian, trạng thái và không gian qua bộ 3 cơ chế:
  - *Slow:* Giảm tốc độ di chuyển và tốc độ ra chiêu.
  - *Stasis (Ngưng đọng):* Khóa cứng mọi hành động của mục tiêu; sát thương nén lại và bùng nổ khi Stasis kết thúc.
  - *Reverse (Nghịch chuyển):* Khôi phục vị trí của mục tiêu hoặc bản thân về tọa độ 3 giây trước.
- **Skill Pool 10 Chiêu (Trang bị 5 Slot):** Chrono Bolt, Time Dilation Field, Temporal Stasis, Spatial Rewind, Phase Blink, Gravitational Singularity, Echo Impact, Temporal Trap, Resonance Surge, Chrono Rupture (x2 sát thương lên mục tiêu đang bị Slow/Stasis/Reverse).
- **3 Ấn Nội Tại:** Sụp Đổ Tuyến Tính (bùng nổ thêm 15% tổng sát thương đã nhận), Nghịch Đảo Nhân Quả (+20% MoveSpeed, hoàn 15% mana khi Reverse), Linh Thú Hiệp Đồng Thời Gian (+1.5s Slow, -0.5s cooldown).

### 4.4. Mục Sư Thánh – Ma (Holy / Dark Priest)
- **Cơ chế cốt lõi:** **Song Ấn Thánh / Ma – Hai Form, Hai Bộ Kỹ Năng Độc Lập**.
  - *Angel Form:* Hỗ trợ, hồi máu, tạo khiên, thanh tẩy (Cleanse).
  - *Demon Form:* Áp chế, nguyền rủa (Curse), hút máu, bộc phát sát thương.
  - Nút chuyển Form (Stance Toggle): Thay đổi toàn bộ 5 ô kỹ năng trên HUD tức thì.
- **Skill Pool 10 Chiêu Theo Cặp Đối Xứng:** Thánh Kích/Ma Kích, Cầu Phúc/Nguyền Ấn, Quang Thuẫn/Hắc Thuẫn, Thanh Tẩy/Huyết Khế, Thánh Vực/Ma Vực, Thánh Liên/Hắc Liên, Cứu Rỗi/Đoạt Hồn, Màn Thánh/Màn Hắc, Quang Bùng Nổ/Hắc Bộc Phát, Phán Xét/Kết Tội.

### 4.5. Luyện Dược Sư Chum Ma Pháp (Cauldron Alchemist)
- **Cơ chế cốt lõi:** Đặt Chum $\longrightarrow$ Ném Nguyên Liệu $\longrightarrow$ Delay/Ủ $\longrightarrow$ Kích Hoạt Phản Ứng $\longrightarrow$ Hoàn Nguyên.
  - Nếu bị ngắt chiêu / khống chế trong lúc đặt chum, **hoàn trả 100% nguyên liệu**, không tạo phế phẩm.
- **3 Cấp Chất Lượng Nguyên Liệu (Tỷ lệ drop cơ bản 60% Thấp, 30% Vừa, 10% Cao):**
  - Thấp = 1 điểm, Vừa = 2 điểm, Cao = 3 điểm.
  - Ngưỡng trung bình 3-5 nguyên liệu: 1.00–1.49 = Thấp, 1.50–2.49 = Vừa, 2.50–3.00 = Cao (Chất lượng Cao tăng +30% hiệu số).
- **Tỷ Lệ Hoàn Nguyên Theo Progression:** Lv 1–15 đạt **5%**, Lv 16–29 đạt **10%**, Lv 30+ đạt **15%**.
- **3 Ấn Nội Tại:** Quy Hoàn Dược Lý, Xúc Tác Linh Thú, Thang Điểm Tinh Khiết.
- **Nghề Alchemist:** Chế thuốc hồi phục, buff combat, kháng nguyên tố, thuốc tăng tỷ lệ săn/bắt Pet.

---

## 5. PHÁI XẠ THỦ (4 LỚP CHUYÊN BIỆT)

### 5.1. Xạ Thủ Súng (Gunner) & 2 Chức Nghiệp Ẩn
- **Cơ chế cốt lõi:** **Thanh Nhiệt Lượng (Heat) & Overheat**.
  - Không dùng đạn truyền thống. Bắn tích lũy nhiệt lượng.
  - Nhiệt cao (70–99%): Tăng +20% sát thương đạn và +1.5m tầm bắn.
  - Overheat (100%): Kẹt nòng 3.5s, bung van xả khí nóng đẩy lùi 3m và nhận khiên 15% HP.

#### Hướng 1: Cơ Khí Sư (Machinist / Engineer)
- **Tối đa 3 Trụ Súng Tự Động:** Trụ Liên Thanh (Gatling dồn dame), Trụ Laser (xuyên giáp), Trụ Điện Từ (EMP làm chậm và choáng diện rộng).
- **Tài nguyên Linh Kiện Cơ Khí:** Rơi từ quái vật để đặt và sửa trụ.
- **Pet – Trạm Bảo Trì Di Động:** Tự động bay đến sửa trụ thấp máu, hồi HP và tăng 25% tốc bắn cho trụ.
- **Nghề Phụ Cơ Khí Rèn:** Chế súng/giáp, rã phế liệu lấy hợp kim, chế chip tản nhiệt và mod gia cố trụ.

#### Hướng 2: Sát Thủ Bóng Đêm (Shadow Gunner)
- **Tàng Hình Chủ Động (Stealth):** Ẩn nấp 5–10 giây, quái mất khả năng khóa mục tiêu.
- **Đòn Mở Đầu (Ambush Shot):** Phát bắn đầu tiên phá tàng hình nhận **$\times 200\% - 300\%$ Sát thương chí mạng**.
- **Nội Tại Phía Sau Lưng (Backstab):** Bắn vào góc $180^\circ$ sau lưng quái nhận **+35% sát thương và +25% crit rate**.
  - *(Tàng hình + Sau lưng + Khắc chế nguyên tố = Nhân sát thương cực hạn x8).*
- **Pet – Vật Thế Thân (Decoy):** Pet tạo phân thân Decoy khiêu khích toàn bộ quái vật quay đầu tấn công Decoy, mở toang góc lưng cho xạ thủ dứt điểm.

### 5.2. Xạ Thủ Nguyên Tố (Elemental Archer)
- **Cơ chế cốt lõi: Cố Định Tốc Bắn 1.0 hit/s & Chuyển Hóa Chỉ Số:**
  - Tốc độ tấn công bị khóa cứng ở 1.0 phát/giây.
  - Tốc độ đánh từ trang bị chuyển thành Sát Thương Nguyên Tố theo tỷ lệ:  
    $$\text{1\% Attack Speed} \longrightarrow \text{1.5\% Sát Thương Nguyên Tố (hoặc Hồi Mana)}$$
- **4 Loại Mũi Tên Nguyên Tố:**
  - *Hỏa Tên:* Nổ lan 2.5m thiêu đốt (Burn DoT); Kích hoạt tạo vũng nham thạch.
  - *Thủy Tên:* Làm chậm 35% MoveSpeed và 20% AtkSpeed; Kích hoạt đóng băng 1.5s.
  - *Phong Tên:* Đẩy lùi 2.5m và ngắt chiêu; Kích hoạt bắn lốc cuốn xa 8m.
  - *Thổ Tên:* Bỏ qua 40% giáp; Kích hoạt gây sát thương chuẩn theo % giáp tối đa của quái.
- **3 Ấn Nội Tại:** Chuẩn Xác Tuyệt Đối (bắn 3 phát cùng hệ thì phát thứ 4 chắc chắn bạo kích), Khắc Chế Điểm Yếu (x2 dmg và x2 thời gian hiệu lực), Dẫn Tiễn Hiệp Đồng (mũi tên gắn ấn, đòn đánh Pet kích nổ reaction).

### 5.3. Xạ Thủ Tinh Linh (Spirit Archer)
- **Cơ chế cốt lõi: 3 Điểm Charge Linh Thể (Spirit Charge):**
  - Pet cấp 3 Charge, tự hồi 1 Charge mỗi 4 giây.
  - *Nhấn Tấn Công:* Tiêu hao 1 Charge gọi 1 Linh Thể tự bay đuổi quái và tấn công liên tục 8–10 giây.
  - *Giữ Tấn Công (3 Charge):* Kích hoạt **Hợp Thể (Spirit Fusion)** tiêu cả 3 Charge tạo Linh Thể khổng lồ x3 sát thương.
- **Kế Thừa Hệ Của Pet:**
  - Hỏa: Tích lũy Burn DoT.
  - Thủy: Tích lũy tầng Slow.
  - Điện: Xác suất giật choáng (Stun).
  - Thổ: Bào mòn giáp (Armor Shred).
  - Thiên Thần: Bay quanh đồng đội thấp máu nhất để hồi máu.
  - Ác Quỷ: Hút máu (Lifesteal) hồi cho chủ nhân.
- **3 Ấn Nội Tại:** Tuần Hoàn Tinh Phách (rút ngắn hồi Charge còn 2s nếu quái chết, Fusion tăng 20% MoveSpeed), Đồng Điệu Linh Tố (+25% hiệu số nguyên tố, kích nổ Reaction), Kế Thừa Linh Thú (thừa hưởng 100% Hidden Passive của Pet).

---

## 6. DANH SÁCH 3 ẤN NỘI TẠI (PASSIVE SEALS) TOÀN BỘ CÁC CLASS

| Tên Class | Ấn 1 (Passive Seal 1) | Ấn 2 (Passive Seal 2) | Ấn 3 (Passive Seal 3) |
|---|---|---|---|
| **Cận Chiến Tự Do** | **Binh Khí Đồng Thể** (+25% dmg đòn đầu trong 3s sau đổi Pet) | **Phủ Ngấn Nguyên Tố** (3 chiêu vũ khí mang hệ Pet, x2 khắc chế) | **Kế Thừa Linh Tính** (100% Hidden Passive Pet, cường hóa Tuyệt kỹ) |
| **Pháp Sư Nguyên Tố** | **Lưỡng Nghi Hợp Thành** (+25% dmg đơn hệ, +1.5s debuff đa hệ) | **Mạch Chuỗi Thiên Tai** (Tích 3 combo mở Thiên Tai AoE) | **Linh Thú Xúc Tác** (Pet tự mồi hạt nguyên tố vào queue 25%) |
| **Pháp Sư Triệu Hồi** | **Cộng Hưởng Lưỡng Thú** (2 Pet cùng đánh kích nổ +20% Reaction) | **Phân Bổ Chiến Thuật** (Melee Pet -15% dmg nhận, Ranged +15% tầm) | **Tiếp Ứng Khẩn Cấp** (Đổi Pet tạo giáp 20% HP & sóng xung kích 3m) |
| **Pháp Sư Thời Không** | **Sụp Đổ Tuyến Tính** (Mục tiêu Slow/Stasis nổ thêm 15% dmg đã nhận) | **Nghịch Đảo Nhân Quả** (Reverse tăng 20% MoveSpeed & hoàn 15% mana) | **Linh Thú Hiệp Đồng** (Pet đánh kéo dài +1.5s Slow, giảm 0.5s CD) |
| **Mục Sư Thánh - Ma** | **Song Ấn Hỗ Sinh** (Chuyển Form hoàn trả mana, bảo lưu ấn ký) | **Thánh Ma Lưỡng Hóa** (Thánh hồi máu tạo khiên; Ma hút máu chuyển buff) | **Đồng Điều Linh Giới** (Đọc Element và Hidden Passive Pet theo Form) |
| **Luyện Dược Sư Chum** | **Quy Hoàn Dược Lý** (Hoàn nguyên 5%/10%/15%, ngắt đòn hoàn 100%) | **Xúc Tác Linh Thú** (Đồng hóa Pet Synergy theo hệ, kích hoạt hidden) | **Thang Điểm Tinh Khiết** (Chất lượng Cao tăng +30% hiệu số, mở hiệu ứng phụ) |
| **Xạ Thủ Súng (Cơ Khí)** | **Xung Áp Quá Nhiệt** (Nhiệt cao +20% dmg; Overheat tạo khiên 15% HP) | **Trạm Cơ Động Hiệp Đồng** (Pet tự bay sửa trụ, +35% drop linh kiện) | **Thợ Rèn Chế Tác** (Trang bị tự rèn thêm khe mod, giảm 50% chi phí trụ) |
| **Xạ Thủ Súng (Sát Thủ)** | **Ám Ảnh Trảm Kích** (Bắn góc 180° sau lưng +35% dmg, +25% crit) | **Ảnh Phách Thế Thân** (Tàng hình Pet tạo Decoy khiêu khích 6m) | **Thợ Săn Bóng Đêm** (Ambush Shot x200-300% crit; diệt quái giảm 50% CD) |
| **Xạ Thủ Nguyên Tố** | **Chuẩn Xác Tuyệt Đối** (1% AtkSpd -> 1.5% ElemDmg, 3 phát cùng hệ bạo kích) | **Khắc Chế Điểm Yếu** (Bắn trúng hệ khắc chế x2 dmg & x2 thời gian hiệu ứng) | **Dẫn Tiễn Hiệp Đồng** (Mũi tên gắn ấn dẫn đường, Pet đánh kích nổ Reaction) |
| **Xạ Thủ Tinh Linh** | **Tuần Hoàn Tinh Phách** (Quái chết rút ngắn hồi Charge còn 2s, Fusion +20% spd) | **Đồng Điệu Linh Tố** (+25% hiệu số hiệu ứng hệ Pet, kích nổ Reaction) | **Kế Thừa Linh Thú** (100% Hidden Passive Pet, bảo toàn hệ Linh Thể đang bay) |
