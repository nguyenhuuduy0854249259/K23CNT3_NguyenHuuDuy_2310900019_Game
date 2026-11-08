import os
import uuid

dirs = [r'd:\Unity\prj1\gam\Assets\Resources', r'd:\Unity\prj1\gam\Assets\Resources\Items']
for d in dirs:
    os.makedirs(d, exist_ok=True)
    meta = d + '.meta'
    if not os.path.exists(meta):
        guid = uuid.uuid4().hex
        with open(meta, 'w', encoding='utf-8') as f:
            f.write(f'fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')

items = [
    ('Starter_Sword', 'Kiếm Sắt Rỉ', 0, 0, 'Thanh kiếm sắt cơ bản phát cho lính mới. Tuy hoen rỉ nhưng lưỡi kiếm vẫn đủ bén để hạ gục lũ quái vật cấp thấp.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_weapon_starter_sword.png', 15, 8, True, 1, 0, 8.0, 0.0, 0.0, 0.0, 0.0, 0.05),
    ('armor_leather_novice', 'Áo Giáp Da Thô', 0, 0, 'Áo giáp làm từ da thú thường khâu lại. Cung cấp khả năng bảo vệ cơ bản trước móng vuốt dã thú.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_armor_leather_novice.png', 20, 10, True, 1, 1, 0.0, 6.0, 30.0, 0.0, 0.0, 0.0),
    ('helmet_leather_novice', 'Mũ Da Thợ Săn', 0, 0, 'Mũ trùm đầu làm bằng da mềm, nhẹ và thoáng khí, giúp người đội dễ dàng quan sát trong rừng rậm.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_helmet_leather_novice.png', 15, 7, True, 1, 2, 0.0, 3.0, 15.0, 0.0, 0.0, 0.0),
    ('gloves_leather_novice', 'Găng Tay Da Bền', 0, 0, 'Đôi găng tay da ôm sát lòng bàn tay, hỗ trợ cầm chắc chuôi kiếm và kéo cung mà không lo trầy xước.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_gloves_leather_novice.png', 12, 6, True, 1, 5, 2.0, 2.0, 10.0, 0.0, 0.0, 0.0),
    ('boots_leather_novice', 'Ủng Da Đi Rừng', 0, 0, 'Đôi ủng đế cao su dày chống trơn trượt trên bùn lầy, tăng tốc độ di chuyển khi thám hiểm thảo nguyên.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_boots_leather_novice.png', 15, 7, True, 1, 6, 0.0, 2.0, 10.0, 0.0, 0.5, 0.0),
    ('potion_small_hp', 'Bình Máu Nhỏ', 2, 0, 'Chất lỏng màu đỏ chiết xuất từ thảo mộc tươi. Lập tức hồi phục 60 HP khi uống.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_potion_small_hp.png', 10, 5, False, 1, 0, 0, 0, 0, 0, 0, 0),
    ('potion_small_mp', 'Bình Năng Lượng Nhỏ', 2, 0, 'Dược thủy màu lam phát sáng lấp lánh. Lập tức hồi phục 40 MP giúp thi triển kỹ năng liên tục.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_potion_small_mp.png', 12, 6, False, 1, 0, 0, 0, 0, 0, 0, 0),
    ('material_herb_leaf', 'Thảo Mộc Xanh Oakhaven', 3, 0, 'Lá thảo mộc mọc nhiều ven hồ và bờ suối thảo nguyên, nguyên liệu chính để chế tạo bình hồi máu.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_material_herb_leaf.png', 5, 2, False, 1, 0, 0, 0, 0, 0, 0, 0),
    ('material_copper_ore', 'Mẩu Quặng Đồng Thô', 3, 0, 'Khoáng thạch đồng đỏ khai thác từ các vách đá thảo nguyên. Mang cho Thợ Rèn Barnaby để chế tạo hợp kim.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_material_copper_ore.png', 6, 3, False, 1, 0, 0, 0, 0, 0, 0, 0),
    ('material_iron_ore', 'Mẩu Quặng Sắt Thô', 3, 0, 'Khoáng thạch khai thác từ các vách đá thảo nguyên. Mang cho Thợ Rèn Barnaby để tinh luyện vũ khí.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_material_iron_ore.png', 8, 4, False, 1, 0, 0, 0, 0, 0, 0, 0),
    ('material_animal_hide', 'Tấm Da Thú Rừng', 3, 0, 'Lớp da bền thu hoạch từ Dã Lang hoặc Chuột Chũi. Dùng để may vá mũ, áo giáp và ủng đi rừng.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_material_animal_hide.png', 6, 3, False, 1, 0, 0, 0, 0, 0, 0, 0),
    ('quest_broken_rune', 'Mảnh Phù Văn Vỡ Vụn', 5, 2, 'Mảnh đá cổ khắc cổ tự phát sáng ánh tím bí ẩn rơi ra từ Chúa Tể Sói Bóng Tối. Chứa manh mối Hồi 2.', r'd:\Unity\prj1\gam\Assets\Sprites\Items\icon_quest_broken_rune.png', 0, 0, False, 1, 0, 0, 0, 0, 0, 0, 0)
]

def get_guid(path):
    meta_path = path + '.meta'
    if os.path.exists(meta_path):
        with open(meta_path, 'r', encoding='utf-8') as f:
            for line in f:
                if line.startswith('guid:'):
                    return line.split(':')[1].strip()
    return ''

for it in items:
    iid, name, itype, rarity, desc, icon_path, buy, sell, is_equip, req_lv, slot, atk, _def, hp, mp, spd, crit = it
    asset_file = os.path.join(dirs[1], f'{iid}.asset')
    meta_file = asset_file + '.meta'
    if not os.path.exists(meta_file):
        guid = uuid.uuid4().hex
        with open(meta_file, 'w', encoding='utf-8') as f:
            f.write(f'fileFormatVersion: 2\nguid: {guid}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    
    icon_guid = get_guid(icon_path)
    icon_ref = f'{{fileID: 21300000, guid: {icon_guid}, type: 3}}' if icon_guid else '{{fileID: 0}}'
    
    if is_equip:
        content = f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: e9215371cbee6da429ae08b281f1d951, type: 3}}
  m_Name: {iid}
  itemId: {iid}
  itemName: "{name}"
  itemType: 0
  rarity: {rarity}
  description: "{desc}"
  icon: {icon_ref}
  buyPrice: {buy}
  sellPrice: {sell}
  isStackable: 0
  maxStack: 1
  equipSlot: {slot}
  requiredLevel: {req_lv}
  requiredTier: 0
  isSpecializedWeapon: 0
  bonusAtk: {atk}
  bonusDef: {_def}
  bonusHp: {hp}
  bonusMp: {mp}
  bonusSpeed: {spd}
  bonusCritRate: {crit}
  weaponElement: 0
  visualSprite: {icon_ref}
  weaponGripOffset: {{x: 0, y: 0}}
  weaponRotationAngle: 0
  isUnique: 0
  uniqueEffect: 0
  uniqueEffectName: ""
  uniqueEffectDescription: ""
'''
    else:
        content = f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 38016e11326294e4f91711e2d475bceb, type: 3}}
  m_Name: {iid}
  itemId: {iid}
  itemName: "{name}"
  itemType: {itype}
  rarity: {rarity}
  description: "{desc}"
  icon: {icon_ref}
  buyPrice: {buy}
  sellPrice: {sell}
  isStackable: 1
  maxStack: 99
'''
    with open(asset_file, 'w', encoding='utf-8') as f:
        f.write(content)

print('Generated all 12 ScriptableObjects in Assets/Resources/Items!')
