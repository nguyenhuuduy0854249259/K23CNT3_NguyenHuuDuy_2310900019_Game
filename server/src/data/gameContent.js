const content = (contentType, contentId, name, description, data, tags = []) => ({
  contentType,
  contentId,
  name,
  description,
  data,
  tags
});

module.exports = [
  content('element', 'none', 'Không nguyên tố', 'Sát thương vật lý cơ bản.', { strongAgainst: [], weakAgainst: [] }),
  content('element', 'fire', 'Lửa', 'Sát thương theo thời gian và cộng hưởng diện rộng.', { strongAgainst: ['wind'], weakAgainst: ['water'] }),
  content('element', 'water', 'Nước', 'Làm chậm và khống chế mục tiêu.', { strongAgainst: ['fire'], weakAgainst: ['earth'] }),
  content('element', 'earth', 'Thổ', 'Phòng thủ và tạo địa chấn.', { strongAgainst: ['water'], weakAgainst: ['wind'] }),
  content('element', 'wind', 'Phong', 'Tăng tốc và khuếch đại phạm vi.', { strongAgainst: ['earth'], weakAgainst: ['fire'] }),
  content('element', 'lightning', 'Điện', 'Sát thương nhanh và hiệu ứng choáng.', { strongAgainst: [], weakAgainst: [] }),
  content('element', 'poison', 'Độc', 'Gây độc kéo dài.', { strongAgainst: [], weakAgainst: [] }),

  content('class', 'novice', 'Tân Thủ (Novice)', 'Lớp khởi đầu cân bằng.', { branch: 'Novice', tier: 0, requiredLevelForPromotion: 1, baseAtk: 15, baseDef: 5, baseHp: 100, baseMp: 50, baseSpeed: 4.5, skillIds: [] }, ['starter']),
  content('class', 'archer_spirit', 'Xạ Thủ Tinh Linh', 'Nhập hồn pet vào cung để tạo mũi tên theo hệ của pet.', { branch: 'Archer', tier: 1, requiredLevelForPromotion: 10, baseAtk: 18, baseDef: 4, baseHp: 90, baseMp: 60, baseSpeed: 5.5, mechanic: 'soul_infusion', skillIds: ['skill_spirit_arrow'] }, ['archer', 'pet-synergy']),
  content('class', 'archer_elemental', 'Xạ Thủ Nguyên Tố', 'Tốc độ bắn cố định, chọn hệ để khắc chế kẻ địch.', { branch: 'Archer', tier: 2, requiredLevelForPromotion: 30, baseAtk: 24, baseDef: 4, baseHp: 95, baseMp: 70, baseSpeed: 5, mechanic: 'elemental_quiver', skillIds: ['skill_elemental_arrow'] }, ['archer', 'element']),
  content('class', 'archer_engineer', 'Cơ Khí Sư', 'Đặt tối đa ba trụ tự động, pet hỗ trợ bảo trì.', { branch: 'Archer', tier: 2, requiredLevelForPromotion: 30, baseAtk: 22, baseDef: 8, baseHp: 110, baseMp: 50, baseSpeed: 4.2, mechanic: 'turret_system', skillIds: ['skill_deploy_turret'] }, ['archer', 'turret']),
  content('class', 'archer_shadow', 'Sát Thủ Bóng Đêm', 'Tàng hình và dứt điểm từ phía sau.', { branch: 'Archer', tier: 2, requiredLevelForPromotion: 30, baseAtk: 30, baseDef: 2, baseHp: 80, baseMp: 55, baseSpeed: 6, mechanic: 'stealth_mark', skillIds: ['skill_cloak'] }, ['archer', 'stealth']),
  content('class', 'warrior_guardian', 'Kiếm và Khiên', 'Chặn chủ động và phản đòn.', { branch: 'Warrior', tier: 1, requiredLevelForPromotion: 10, baseAtk: 16, baseDef: 14, baseHp: 140, baseMp: 35, baseSpeed: 3.8, mechanic: 'active_block', skillIds: ['skill_parry'] }, ['warrior', 'tank']),
  content('class', 'warrior_slayer', 'Đại Kiếm', 'Tích lực để gây sát thương diện rộng.', { branch: 'Warrior', tier: 1, requiredLevelForPromotion: 10, baseAtk: 32, baseDef: 7, baseHp: 125, baseMp: 35, baseSpeed: 3.2, mechanic: 'charged_attack', skillIds: ['skill_great_sweep'] }, ['warrior', 'aoe']),
  content('class', 'warrior_duelist', 'Kiếm Đơn', 'Combo liên hoàn và lướt phản công.', { branch: 'Warrior', tier: 1, requiredLevelForPromotion: 10, baseAtk: 22, baseDef: 6, baseHp: 100, baseMp: 40, baseSpeed: 5.8, mechanic: 'combo_chain', skillIds: ['skill_dash'] }, ['warrior', 'combo']),
  content('class', 'warrior_assassin', 'Sát Thủ Dao Găm', 'Backstab và gây xuất huyết.', { branch: 'Warrior', tier: 2, requiredLevelForPromotion: 30, baseAtk: 28, baseDef: 3, baseHp: 85, baseMp: 45, baseSpeed: 6.2, mechanic: 'backstab_bleed', skillIds: ['skill_backstab'] }, ['warrior', 'assassin']),
  content('class', 'mage_elemental', 'Pháp Sư Nguyên Tố', 'Kết hợp hai nguyên tố để tạo kỹ năng cộng hưởng.', { branch: 'Mage', tier: 1, requiredLevelForPromotion: 10, baseAtk: 28, baseDef: 3, baseHp: 85, baseMp: 100, baseSpeed: 4, mechanic: 'element_combo', skillIds: ['skill_element_combo'] }, ['mage', 'element']),
  content('class', 'mage_priest', 'Mục Sư Thánh - Ma', 'Chuyển đổi giữa hỗ trợ Thiên Thần và tấn công Ác Quỷ.', { branch: 'Mage', tier: 1, requiredLevelForPromotion: 10, baseAtk: 18, baseDef: 6, baseHp: 100, baseMp: 110, baseSpeed: 4, mechanic: 'stance_switch', skillIds: ['skill_angel_heal', 'skill_demon_curse'] }, ['mage', 'support']),
  content('class', 'mage_summoner', 'Pháp Sư Triệu Hồi', 'Điều khiển đồng thời tối đa hai pet.', { branch: 'Mage', tier: 2, requiredLevelForPromotion: 30, baseAtk: 20, baseDef: 4, baseHp: 95, baseMp: 120, baseSpeed: 3.8, mechanic: 'dual_summon', maxActivePets: 2, skillIds: ['skill_dual_summon'] }, ['mage', 'pet-synergy']),
  content('class', 'mage_chrono', 'Pháp Sư Thời Không', 'Làm chậm, ngưng đọng và đảo ngược vị trí.', { branch: 'Mage', tier: 2, requiredLevelForPromotion: 30, baseAtk: 24, baseDef: 4, baseHp: 90, baseMp: 125, baseSpeed: 4, mechanic: 'time_control', skillIds: ['skill_time_freeze'] }, ['mage', 'debuffer']),

  content('skill', 'skill_spirit_arrow', 'Mũi Tên Linh Hồn', 'Mũi tên nhận hiệu ứng theo hệ pet đang nhập hồn.', { targetType: 'Projectile', manaCost: 8, cooldownSeconds: 0.5, damageMultiplier: 1.3, range: 8, element: 'pet_element' }),
  content('skill', 'skill_elemental_arrow', 'Mũi Tên Nguyên Tố', 'Bắn một mũi tên hệ đã chọn.', { targetType: 'Projectile', manaCost: 12, cooldownSeconds: 1, damageMultiplier: 1.8, range: 9, element: 'selected' }),
  content('skill', 'skill_deploy_turret', 'Đặt Trụ Tự Động', 'Triệu hồi trụ tấn công mục tiêu gần nhất.', { targetType: 'PetCommand', manaCost: 20, cooldownSeconds: 12, durationSeconds: 30, maxTurrets: 3 }),
  content('skill', 'skill_cloak', 'Tàng Hình', 'Ẩn khỏi tầm nhìn địch; đòn đầu tiên gây chí mạng.', { targetType: 'SelfBuff', manaCost: 18, cooldownSeconds: 20, durationSeconds: 8, firstHitMultiplier: 2.5 }),
  content('skill', 'skill_parry', 'Phản Đòn', 'Chặn đúng thời điểm để làm choáng và phản công.', { targetType: 'SelfBuff', manaCost: 8, cooldownSeconds: 4, frontDamageReduction: 0.8, statusEffect: 'Stun' }),
  content('skill', 'skill_element_combo', 'Cộng Hưởng Hai Hệ', 'Nạp hai nguyên tố để tạo hiệu ứng kết hợp.', { targetType: 'AreaOfEffect', manaCost: 25, cooldownSeconds: 8, comboSlots: 2, examples: { fire_wind: 'area_burn', water_earth: 'mud_stun' } }),
  content('skill', 'skill_angel_heal', 'Thánh Ấn', 'Hồi máu và giảm sát thương cho đồng đội.', { targetType: 'AreaOfEffect', manaCost: 30, cooldownSeconds: 10, stance: 'Angel', healPercent: 0.2, damageReduction: 0.2 }),
  content('skill', 'skill_demon_curse', 'Ma Ấn', 'Nguyền rủa, giảm phòng thủ và gây sát thương theo thời gian.', { targetType: 'AreaOfEffect', manaCost: 30, cooldownSeconds: 10, stance: 'Demon', defenseReduction: 0.2, statusEffect: 'Burn' }),
  content('skill', 'skill_time_freeze', 'Ngưng Đọng Thời Gian', 'Đóng băng mục tiêu trong thời gian ngắn.', { targetType: 'AreaOfEffect', manaCost: 35, cooldownSeconds: 15, durationSeconds: 3, statusEffect: 'Stun', damageImmuneWhileFrozen: true }),

  content('pet', 'pet_firefox', 'Cáo Lửa', 'Pet DPS hệ Lửa, có thể cộng hưởng với mũi tên.', { element: 'Fire', role: 'DPS', evolutionStage: 1, maxHp: 80, attackPower: 12, defense: 4, moveSpeed: 5, activeSkillId: 'skill_pet_fire_breath', maxEvolutionStage: 3 }, ['fire', 'dps']),
  content('pet', 'pet_wind_hawk', 'Chim Săn Mồi', 'Thu hồi mũi tên và tăng phạm vi kỹ năng.', { element: 'Wind', role: 'Support', evolutionStage: 1, maxHp: 70, attackPower: 10, defense: 3, moveSpeed: 7, passive: 'retrieve_arrows', maxEvolutionStage: 3 }, ['wind', 'support']),
  content('pet', 'pet_mecha', 'Cơ Khí Robot', 'Bảo trì trụ và có thể biến thành turret.', { element: 'Lightning', role: 'Support', evolutionStage: 1, maxHp: 100, attackPower: 14, defense: 8, moveSpeed: 4, passive: 'repair_turret', maxEvolutionStage: 3 }, ['lightning', 'support']),
  content('pet', 'pet_angel', 'Tinh Linh Thiên Thần', 'Tăng hiệu lực hồi máu và tạo hào quang phòng thủ.', { element: 'Light', role: 'Support', evolutionStage: 1, maxHp: 85, attackPower: 8, defense: 5, moveSpeed: 5, passive: 'heal_aura', maxEvolutionStage: 3 }, ['light', 'support']),

  content('item', 'item_capture_scroll', 'Giấy Thu Phục', 'Dùng khi quái đã yếu để thu phục pet.', { itemType: 'CaptureScroll', rarity: 'Common', buyPrice: 50, sellPrice: 10, isStackable: true, maxStack: 99 }, ['capture']),
  content('item', 'item_potion_hp', 'Thuốc Hồi Máu', 'Hồi phục HP.', { itemType: 'Potion', rarity: 'Common', buyPrice: 20, sellPrice: 8, isStackable: true, maxStack: 99, healHp: 50 }, ['consumable']),
  content('item', 'item_evolution_stone', 'Đá Tiến Hóa', 'Nguyên liệu nâng cấp pet.', { itemType: 'Material', rarity: 'Rare', buyPrice: 250, sellPrice: 80, isStackable: true, maxStack: 99 }, ['pet', 'evolution']),
  content('item', 'item_fire_arrow', 'Mũi Tên Lửa', 'Mũi tên gây hiệu ứng thiêu đốt.', { itemType: 'Material', rarity: 'Uncommon', element: 'Fire', statusEffect: 'Burn', isStackable: true, maxStack: 999 }, ['arrow', 'crafting']),
  content('item', 'item_ice_arrow', 'Mũi Tên Băng', 'Mũi tên làm chậm mục tiêu.', { itemType: 'Material', rarity: 'Uncommon', element: 'Water', statusEffect: 'Slow', isStackable: true, maxStack: 999 }, ['arrow', 'crafting']),

  content('quest', 'quest_slime_hunt', 'Quét sạch Slime Rừng', 'Tiêu diệt 5 Slime để nhận thưởng.', { targetEnemyId: 'enemy_slime', requiredKills: 5, rewardExp: 80, rewardGold: 50 }, ['guild', 'starter']),
  content('quest', 'quest_wolf_hunt', 'Bảo vệ Làng', 'Tiêu diệt 3 Sói Hoang.', { targetEnemyId: 'enemy_wolf', requiredKills: 3, rewardExp: 120, rewardGold: 80 }, ['guild']),

  content('recipe', 'recipe_fire_arrow', 'Chế tạo Mũi Tên Lửa', 'Biến nguyên liệu thành mũi tên nguyên tố.', { outputItemId: 'item_fire_arrow', outputQuantity: 5, ingredients: [{ itemId: 'material_wood', quantity: 2 }, { itemId: 'material_fire_core', quantity: 1 }] }, ['crafting']),
  content('recipe', 'recipe_ice_arrow', 'Chế tạo Mũi Tên Băng', 'Biến nguyên liệu thành mũi tên làm chậm.', { outputItemId: 'item_ice_arrow', outputQuantity: 5, ingredients: [{ itemId: 'material_wood', quantity: 2 }, { itemId: 'material_ice_core', quantity: 1 }] }, ['crafting'])
];
