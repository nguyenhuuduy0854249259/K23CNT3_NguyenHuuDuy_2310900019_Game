require('dotenv').config();

const mongoose = require('mongoose');
const GameContent = require('./models/GameContent');
const content = require('./data/gameContent');

const mongoUri = process.env.MONGODB_URI;

if (!mongoUri) {
  throw new Error('Thiếu MONGODB_URI. Hãy tạo file .env từ .env.example.');
}

async function seed() {
  await mongoose.connect(mongoUri);

  const operations = content.map((entry) => ({
    updateOne: {
      filter: { contentType: entry.contentType, contentId: entry.contentId },
      update: { $set: entry, $setOnInsert: { version: 1 } },
      upsert: true
    }
  }));

  const result = await GameContent.bulkWrite(operations, { ordered: false });
  console.log(`Đã nạp ${content.length} nội dung game (thêm mới: ${result.upsertedCount}, cập nhật: ${result.modifiedCount}).`);
}

seed()
  .catch((error) => {
    console.error('Nạp dữ liệu game thất bại:', error.message);
    process.exitCode = 1;
  })
  .finally(async () => {
    await mongoose.disconnect();
  });
