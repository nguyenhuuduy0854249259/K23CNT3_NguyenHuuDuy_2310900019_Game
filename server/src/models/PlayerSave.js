const mongoose = require('mongoose');

/**
 * PlayerSave: Hồ sơ lưu trữ nhân vật người chơi trên MongoDB.
 * Hỗ trợ các trường cốt lõi và trường mở rộng `extraData` linh hoạt
 * để lưu thêm bất kỳ thông tin nào trong tương lai (thành tựu, trang bị, quest progress, skin, v.v.)
 */
const playerSaveSchema = new mongoose.Schema(
  {
    playerId: { type: String, required: true, trim: true, unique: true, maxlength: 128 },
    level: { type: Number, default: 1, min: 1 },
    currentExp: { type: Number, default: 0, min: 0 },
    gold: { type: Number, default: 100, min: 0 },
    currentClassName: { type: String, default: 'Tân Thủ (Novice)', maxlength: 100 },
    hunger: { type: Number, default: 100, min: 0, max: 100 },
    currentHp: { type: Number, default: 100, min: 0 },
    currentMp: { type: Number, default: 50, min: 0 },
    activePetId: { type: String, default: '', maxlength: 100 },
    inventoryItemIds: { type: [String], default: [] },
    inventoryItemCounts: { type: [Number], default: [] },
    // Dữ liệu mở rộng tự do: có thể lưu bất kỳ đối tượng JSON nào
    extraData: { type: mongoose.Schema.Types.Mixed, default: {} }
  },
  { timestamps: true, versionKey: false, strict: false }
);

module.exports = mongoose.model('PlayerSave', playerSaveSchema);

