# MongoDB Backend cho Action RPG (Kiến trúc CSDL Mở)

API này kết nối Unity với MongoDB, cung cấp 2 tính năng chính:
1. **Quản lý dữ liệu người chơi (`PlayerSaveData`)** - Collection `playersaves`: Hỗ trợ lưu trữ đám mây (Cloud Save) với các thuộc tính mở rộng linh hoạt (`extraData`).
2. **Hệ thống dữ liệu game mở (`GameContent`)** - Collection `gamecontents`: Lưu trữ nội dung game (classes, skills, items, pets, quests, recipes,...) dạng Polymorphic / Flexible Schema để sau này có thể cập nhật thêm vũ khí, nhiệm vụ, NPC, màn chơi mới bất kỳ lúc nào mà không cần sửa cấu trúc CSDL!

---

## 1. Trạng thái CSDL trên máy
- **MongoDB Server**: Đang chạy cục bộ tại `mongodb://127.0.0.1:27017/action_rpg`.
- **MongoDB Compass**: Đã có sẵn trên máy, mở ứng dụng và dán `mongodb://127.0.0.1:27017` để xem dữ liệu dạng giao diện trực quan.

## 2. Cấu hình và chạy Backend API
Thư mục: `server/`

```powershell
# Chạy server
npm start

# Hoặc chế độ dev tự reload khi sửa code
npm run dev

# Nạp dữ liệu mẫu ban đầu (42 nội dung game)
npm run seed
```

Kiểm tra API:
- Kiểm tra server: `GET http://localhost:3000/health`
- Tổng quan CSDL: `GET http://localhost:3000/api/stats/overview`

---

## 3. Danh sách API (Endpoints)

### A. Dữ liệu Game Mở (`GameContent`)
- `GET /api/content`: Lấy tất cả nội dung game (hỗ trợ lọc theo tag `?tag=fire`).
- `GET /api/content/:contentType`: Lấy nội dung theo phân loại (ví dụ: `item`, `skill`, `class`, `pet`, `quest`, `achievement`...).
- `GET /api/content/:contentType/:contentId`: Lấy chi tiết một tài nguyên (ví dụ: `/api/content/item/item_potion_hp`).
- `POST /api/content/:contentType`: **Thêm mới hoặc cập nhật động** một tài nguyên game bất kỳ (CSDL Mở).
  ```json
  {
    "contentId": "item_fire_sword",
    "name": "Hỏa Long Kiếm",
    "description": "Thanh kiếm mang sức mạnh rồng lửa.",
    "tags": ["weapon", "fire", "legendary"],
    "data": {
      "attack": 120,
      "critRate": 0.25,
      "element": "Fire"
    }
  }
  ```

### B. Lưu trữ Người Chơi (`PlayerSave`)
- `GET /api/players/:playerId/save`: Lấy hồ sơ lưu trữ của người chơi.
- `PUT /api/players/:playerId/save`: Tạo hoặc cập nhật tiến trình chơi. Hỗ trợ trường `extraData` để lưu trữ thêm bất kỳ dữ liệu mới nào trong tương lai.

