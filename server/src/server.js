require('dotenv').config();

const express = require('express');
const mongoose = require('mongoose');
const PlayerSave = require('./models/PlayerSave');
const GameContent = require('./models/GameContent');

const app = express();
const port = Number(process.env.PORT || 3000);
const mongoUri = process.env.MONGODB_URI;

if (!mongoUri) {
  throw new Error('Thiếu MONGODB_URI. Hãy tạo file .env từ .env.example.');
}

app.use(express.json({ limit: '64kb' }));

app.get('/health', (_req, res) => res.json({ ok: true, status: 'MongoDB Backend Running' }));

// Lấy tổng quan tình trạng CSDL
app.get('/api/stats/overview', async (_req, res, next) => {
  try {
    const totalContent = await GameContent.countDocuments();
    const totalPlayers = await PlayerSave.countDocuments();
    const categories = await GameContent.distinct('contentType');
    res.json({
      database: mongoose.connection.name,
      totalContent,
      totalPlayers,
      contentTypes: categories
    });
  } catch (error) {
    next(error);
  }
});

// Lấy danh sách tất cả các loại nội dung (hoặc lọc theo tag ?tag=...)
app.get('/api/content', async (req, res, next) => {
  try {
    const { tag } = req.query;
    const filter = { enabled: true };
    if (tag) filter.tags = tag;
    const contents = await GameContent.find(filter).sort({ contentType: 1, contentId: 1 }).lean();
    res.json(contents);
  } catch (error) {
    next(error);
  }
});

app.get('/api/content/:contentType', async (req, res, next) => {
  try {
    const content = await GameContent.find({ contentType: req.params.contentType.toLowerCase(), enabled: true })
      .sort({ contentId: 1 })
      .lean();
    res.json(content);
  } catch (error) {
    next(error);
  }
});

app.get('/api/content/:contentType/:contentId', async (req, res, next) => {
  try {
    const content = await GameContent.findOne({
      contentType: req.params.contentType.toLowerCase(),
      contentId: req.params.contentId,
      enabled: true
    }).lean();
    if (!content) return res.status(404).json({ message: 'Không tìm thấy nội dung game.' });
    res.json(content);
  } catch (error) {
    next(error);
  }
});

// Thêm mới hoặc cập nhật động một nội dung game bất kỳ (Hệ thống dữ liệu mở)
app.post('/api/content/:contentType', async (req, res, next) => {
  try {
    const contentType = req.params.contentType.toLowerCase();
    const { contentId, name, description, version, enabled, tags, data } = req.body;

    if (!contentId || !name) {
      return res.status(400).json({ message: 'Thiếu contentId hoặc name.' });
    }

    const item = await GameContent.findOneAndUpdate(
      { contentType, contentId },
      { contentType, contentId, name, description, version: version || 1, enabled: enabled !== false, tags: tags || [], data: data || {} },
      { new: true, upsert: true, setDefaultsOnInsert: true }
    ).lean();

    res.json({ message: 'Lưu nội dung game thành công.', data: item });
  } catch (error) {
    next(error);
  }
});


app.get('/api/players/:playerId/save', async (req, res, next) => {
  try {
    const save = await PlayerSave.findOne({ playerId: req.params.playerId }).lean();
    if (!save) return res.status(404).json({ message: 'Chưa có dữ liệu lưu cho người chơi này.' });
    res.json(save);
  } catch (error) {
    next(error);
  }
});

app.put('/api/players/:playerId/save', async (req, res, next) => {
  try {
    const { inventoryItemIds = [], inventoryItemCounts = [] } = req.body;
    if (!Array.isArray(inventoryItemIds) || !Array.isArray(inventoryItemCounts) || inventoryItemIds.length !== inventoryItemCounts.length) {
      return res.status(400).json({ message: 'inventoryItemIds và inventoryItemCounts phải là hai mảng có cùng số phần tử.' });
    }

    const save = await PlayerSave.findOneAndUpdate(
      { playerId: req.params.playerId },
      { ...req.body, playerId: req.params.playerId },
      { new: true, upsert: true, runValidators: true, setDefaultsOnInsert: true }
    ).lean();
    res.json(save);
  } catch (error) {
    next(error);
  }
});

app.use((error, _req, res, _next) => {
  console.error(error);
  res.status(500).json({ message: 'Không thể xử lý dữ liệu lưu.' });
});

mongoose.connect(mongoUri)
  .then(() => app.listen(port, () => console.log(`API đang chạy tại http://localhost:${port}`)))
  .catch((error) => {
    console.error('Kết nối MongoDB thất bại:', error.message);
    process.exit(1);
  });
