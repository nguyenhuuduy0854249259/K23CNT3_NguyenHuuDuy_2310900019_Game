const mongoose = require('mongoose');

/**
 * GameContent: Mô hình dữ liệu mở (Open / Extensible Content Schema).
 * Cho phép mở rộng thêm bất kỳ tài nguyên game nào trong tương lai
 * (item, skill, class, pet, quest, recipe, enemy, map, achievement, npc, event, dialogue, v.v.)
 * mà không cần sửa cấu trúc schema hay migrate database.
 */
const gameContentSchema = new mongoose.Schema(
  {
    contentType: {
      type: String,
      required: true,
      trim: true,
      lowercase: true,
      maxlength: 64
    },
    contentId: { type: String, required: true, trim: true, maxlength: 128 },
    name: { type: String, required: true, trim: true, maxlength: 160 },
    description: { type: String, default: '' },
    version: { type: Number, default: 1, min: 1 },
    enabled: { type: Boolean, default: true },
    tags: { type: [String], default: [] },
    // Dữ liệu tùy biến không giới hạn (Mixed): chứa chỉ số, thuộc tính riêng của từng loại
    data: { type: mongoose.Schema.Types.Mixed, default: {} }
  },
  { timestamps: true, versionKey: false, strict: false }
);

gameContentSchema.index({ contentType: 1, contentId: 1 }, { unique: true });
gameContentSchema.index({ contentType: 1, enabled: 1 });
gameContentSchema.index({ tags: 1 });

module.exports = mongoose.model('GameContent', gameContentSchema);

