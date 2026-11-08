import os
import hashlib
from PIL import Image, ImageDraw

OUTPUT_DIR = "Assets/Sprites/NPCs"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Helper function to generate Unity .meta file
def make_meta(png_path):
    rel_path = png_path.replace("\\", "/")
    # Use deterministic guid based on path
    guid = hashlib.md5(rel_path.encode('utf-8')).hexdigest()
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 32
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
"""
    with open(png_path + ".meta", "w", encoding="utf-8") as f:
        f.write(meta_content)

# Color palettes (RGBA tuples)
CLR_TRANSPARENT = (0, 0, 0, 0)
CLR_OUTLINE = (24, 20, 32, 255)

# 1. Trưởng Làng Eldrin (Cụ già thông thái râu dài, áo chàm viền vàng, trượng ngọc)
ELDRIN_PAL = {
    "skin": (240, 210, 185, 255),
    "skin_shadow": (205, 170, 145, 255),
    "hair": (245, 248, 255, 255),
    "hair_shadow": (180, 190, 205, 255),
    "robe": (42, 48, 88, 255),
    "robe_shadow": (28, 30, 60, 255),
    "gold": (235, 180, 45, 255),
    "gold_bright": (255, 230, 110, 255),
    "staff_wood": (115, 75, 45, 255),
    "gem": (50, 230, 215, 255),
    "gem_glow": (160, 255, 245, 255),
    "shoes": (70, 50, 38, 255),
    "eyes": (35, 45, 75, 255)
}

# 2. Thợ Rèn Boris (Cơ bắp, tóc ngắn nâu, râu quai nón, tạp dề da thuộc, búa rèn)
BORIS_PAL = {
    "skin": (225, 155, 110, 255),
    "skin_shadow": (180, 115, 75, 255),
    "hair": (65, 40, 25, 255),
    "hair_shadow": (40, 25, 15, 255),
    "shirt": (210, 205, 195, 255),
    "apron": (130, 65, 30, 255),
    "apron_shadow": (85, 40, 18, 255),
    "pants": (55, 45, 40, 255),
    "gloves": (60, 50, 45, 255),
    "hammer_head": (155, 160, 170, 255),
    "hammer_steel": (215, 220, 230, 255),
    "hammer_handle": (100, 60, 35, 255),
    "shoes": (45, 35, 30, 255),
    "eyes": (40, 25, 18, 255)
}

# 3. Dược Sĩ Elena (Nữ dược sư, tóc nâu tết lá xanh, áo choàng xanh ngọc, túi dược liệu)
ELENA_PAL = {
    "skin": (255, 225, 205, 255),
    "skin_shadow": (225, 185, 165, 255),
    "hair": (115, 60, 35, 255),
    "hair_shadow": (80, 40, 22, 255),
    "flower": (80, 225, 140, 255),
    "flower_center": (255, 235, 100, 255),
    "robe": (45, 165, 150, 255),
    "robe_shadow": (28, 110, 100, 255),
    "apron": (245, 252, 250, 255),
    "pouch": (145, 90, 55, 255),
    "potion_green": (60, 240, 120, 255),
    "potion_red": (255, 75, 100, 255),
    "shoes": (95, 65, 50, 255),
    "eyes": (45, 120, 80, 255)
}

# 4. Hội Trưởng Carlos (Giáp kim loại sáng bạc, áo choàng đỏ thẫm mạo hiểm giả, kiếm dài)
CARLOS_PAL = {
    "skin": (238, 185, 145, 255),
    "skin_shadow": (195, 140, 105, 255),
    "hair": (160, 120, 55, 255),
    "hair_shadow": (115, 80, 35, 255),
    "armor_bright": (230, 240, 250, 255),
    "armor_base": (175, 185, 200, 255),
    "armor_shadow": (110, 120, 135, 255),
    "cape": (180, 35, 45, 255),
    "cape_shadow": (120, 20, 28, 255),
    "gold": (240, 190, 50, 255),
    "sword_blade": (220, 230, 245, 255),
    "sword_hilt": (235, 185, 45, 255),
    "scabbard": (80, 50, 40, 255),
    "boots": (65, 55, 55, 255),
    "eyes": (35, 45, 65, 255)
}

# 5. Quản Thú Kaelen (Mũ tai thú, áo gilê lông thú ấm, găng huấn luyện, túi mồi)
KAELEN_PAL = {
    "skin": (240, 195, 155, 255),
    "skin_shadow": (200, 150, 115, 255),
    "hair": (135, 80, 45, 255),
    "hair_shadow": (95, 50, 28, 255),
    "fur_hat": (145, 95, 55, 255),
    "fur_ear": (90, 55, 30, 255),
    "fur_trim": (245, 240, 230, 255),
    "vest": (120, 75, 45, 255),
    "shirt": (65, 105, 55, 255),
    "glove": (75, 55, 40, 255),
    "pouch": (210, 145, 75, 255),
    "pants": (80, 65, 55, 255),
    "boots": (50, 40, 35, 255),
    "eyes": (50, 80, 45, 255)
}

# 6. Thương Nhân Lina (Khăn trùm vàng cam, yếm cam đất, tạp dề trắng, túi vàng)
LINA_PAL = {
    "skin": (255, 220, 195, 255),
    "skin_shadow": (225, 180, 155, 255),
    "hair": (185, 100, 40, 255),
    "hair_shadow": (135, 65, 25, 255),
    "kerchicf": (245, 160, 35, 255),
    "kerchicf_trim": (255, 245, 225, 255),
    "dress": (195, 85, 45, 255),
    "dress_shadow": (145, 55, 28, 255),
    "apron": (248, 248, 245, 255),
    "gold_bag": (250, 205, 45, 255),
    "shoes": (105, 60, 45, 255),
    "eyes": (70, 40, 25, 255)
}

# 7. Cư Dân Oakhaven (Nông dân thôn dã mộc mạc, mũ rơm, quần xanh mộc)
VILLAGER_PAL = {
    "skin": (240, 190, 150, 255),
    "skin_shadow": (200, 145, 110, 255),
    "hair": (90, 60, 40, 255),
    "hair_shadow": (60, 38, 24, 255),
    "hat": (220, 185, 110, 255),
    "hat_ribbon": (160, 50, 40, 255),
    "shirt": (215, 195, 160, 255),
    "pants": (55, 85, 110, 255),
    "pants_shadow": (35, 55, 75, 255),
    "basket": (160, 110, 60, 255),
    "apples": (225, 45, 45, 255),
    "shoes": (75, 55, 45, 255),
    "eyes": (40, 30, 25, 255)
}

def draw_pixel(im, x, y, color):
    if 0 <= x < 32 and 0 <= y < 32:
        im.putpixel((x, y), color)

def fill_rect(im, x1, y1, x2, y2, color):
    for x in range(x1, x2 + 1):
        for y in range(y1, y2 + 1):
            draw_pixel(im, x, y, color)

def draw_eldrin(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = ELDRIN_PAL
    bob = 1 if step in (1, 3) else 0

    # Staff (Right side or left depending on facing)
    staff_x = 23 if facing != "side" else 22
    for sy in range(11 + bob, 29):
        draw_pixel(im, staff_x, sy, p["staff_wood"])
    # Gem atop staff
    fill_rect(im, staff_x - 1, 9 + bob, staff_x + 1, 10 + bob, p["gem"])
    draw_pixel(im, staff_x, 9 + bob, p["gem_glow"])

    # Feet
    if facing == "front" or facing == "back":
        leg_l = 27 + (1 if step == 1 else 0)
        leg_r = 27 + (1 if step == 3 else 0)
        fill_rect(im, 13, 26, 14, leg_l, p["shoes"])
        fill_rect(im, 17, 26, 18, leg_r, p["shoes"])
    else: # side
        leg_f = 27 + (1 if step in (1, 2) else 0)
        leg_b = 27 + (1 if step in (3, 4) else 0)
        fill_rect(im, 15, 26, 17, leg_f, p["shoes"])
        fill_rect(im, 13, 26, 15, leg_b, p["shoes"])

    # Robe body
    by = 17 + bob
    fill_rect(im, 12, by, 19, 26, p["robe"])
    fill_rect(im, 12, 25, 19, 26, p["robe_shadow"])
    # Gold trims
    fill_rect(im, 15, by, 16, 25, p["gold"])
    fill_rect(im, 12, 26, 19, 26, p["gold_bright"])

    # Head
    hy = 9 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    # Hair & Hood
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])
    fill_rect(im, 11, hy + 1, 12, hy + 5, p["hair"])
    fill_rect(im, 19, hy + 1, 20, hy + 5, p["hair"])

    if facing == "front":
        # Eyes
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        # Big white beard reaching chest
        fill_rect(im, 13, hy + 5, 18, hy + 9, p["hair"])
        fill_rect(im, 14, hy + 9, 17, hy + 11, p["hair"])
        fill_rect(im, 15, hy + 11, 16, hy + 12, p["hair_shadow"])
        # Hand holding staff
        draw_pixel(im, 21, 19 + bob, p["skin"])
        draw_pixel(im, 22, 19 + bob, p["skin"])
    elif facing == "back":
        # Full white hair flowing down
        fill_rect(im, 12, hy, 19, hy + 8, p["hair"])
        fill_rect(im, 13, hy + 8, 18, hy + 10, p["hair_shadow"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])
        fill_rect(im, 15, hy + 5, 18, hy + 9, p["hair"])
        fill_rect(im, 16, hy + 9, 18, hy + 11, p["hair"])
        draw_pixel(im, 20, 19 + bob, p["skin"])

    return im

def draw_boris(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = BORIS_PAL
    bob = 1 if step in (1, 3) else 0

    # Massive Blacksmith Hammer
    ham_x = 24 if facing != "side" else 22
    for hy in range(13 + bob, 27):
        draw_pixel(im, ham_x, hy, p["hammer_handle"])
    # Hammer head
    fill_rect(im, ham_x - 2, 10 + bob, ham_x + 2, 13 + bob, p["hammer_head"])
    fill_rect(im, ham_x - 1, 10 + bob, ham_x + 1, 11 + bob, p["hammer_steel"])

    # Boots & Pants
    by = 24
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 12, 23, 14, 26 + l_off, p["pants"])
        fill_rect(im, 17, 23, 19, 26 + r_off, p["pants"])
        fill_rect(im, 12, 27 + l_off, 14, 28 + l_off, p["shoes"])
        fill_rect(im, 17, 27 + r_off, 19, 28 + r_off, p["shoes"])
    else: # side
        fill_rect(im, 14, 23, 17, 26, p["pants"])
        fill_rect(im, 14, 27, 18, 28, p["shoes"])

    # Broad Muscular Torso & Leather Apron
    ty = 16 + bob
    fill_rect(im, 10, ty, 21, ty + 7, p["shirt"])
    # Leather Apron
    fill_rect(im, 12, ty + 1, 19, ty + 7, p["apron"])
    # Straps
    draw_pixel(im, 12, ty, p["apron_shadow"])
    draw_pixel(im, 19, ty, p["apron_shadow"])
    # Strong bare arms
    fill_rect(im, 9, ty + 1, 11, ty + 5, p["skin"])
    fill_rect(im, 20, ty + 1, 22, ty + 5, p["skin"])
    # Heavy gloves
    fill_rect(im, 9, ty + 5, 11, ty + 7, p["gloves"])
    fill_rect(im, 20, ty + 5, 23, ty + 7, p["gloves"])

    # Head & Beard
    hy = 8 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    # Short dark hair
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        # Full thick beard & mustache
        fill_rect(im, 13, hy + 4, 18, hy + 7, p["hair"])
        draw_pixel(im, 15, hy + 5, p["hair_shadow"])
        draw_pixel(im, 16, hy + 5, p["hair_shadow"])
    elif facing == "back":
        fill_rect(im, 12, hy, 19, hy + 5, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])
        fill_rect(im, 15, hy + 4, 18, hy + 7, p["hair"])

    return im

def draw_elena(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = ELENA_PAL
    bob = 1 if step in (1, 3) else 0

    # Feet
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 13, 27 + l_off, 14, 28 + l_off, p["shoes"])
        fill_rect(im, 17, 27 + r_off, 18, 28 + r_off, p["shoes"])
    else: # side
        fill_rect(im, 15, 27, 17, 28, p["shoes"])

    # Robe / Dress
    by = 17 + bob
    fill_rect(im, 12, by, 19, 26, p["robe"])
    # White Alchemist Apron
    fill_rect(im, 14, by + 1, 17, 25, p["apron"])
    # Herb Pouch & Potions on hip
    fill_rect(im, 19, by + 4, 21, by + 7, p["pouch"])
    draw_pixel(im, 20, by + 3, p["potion_green"])
    draw_pixel(im, 21, by + 3, p["potion_red"])

    # Head & Braided Hair
    hy = 9 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    # Chestnut hair
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])
    fill_rect(im, 11, hy + 2, 13, hy + 6, p["hair"])
    fill_rect(im, 18, hy + 2, 20, hy + 6, p["hair"])
    # Green Flower hairpin
    draw_pixel(im, 12, hy + 1, p["flower"])
    draw_pixel(im, 12, hy, p["flower_center"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        draw_pixel(im, 15, hy + 5, p["skin_shadow"])
    elif facing == "back":
        fill_rect(im, 12, hy, 19, hy + 8, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])

    return im

def draw_carlos(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = CARLOS_PAL
    bob = 1 if step in (1, 3) else 0

    # Crimson Cape behind
    cy = 16 + bob
    if facing == "front" or facing == "side":
        fill_rect(im, 9, cy, 11, 26, p["cape"])
        fill_rect(im, 20, cy, 22, 26, p["cape"])
    else: # back cape fully visible
        fill_rect(im, 10, cy, 21, 26, p["cape"])
        fill_rect(im, 11, cy + 2, 20, 26, p["cape_shadow"])

    # Boots & Greaves
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 12, 24, 14, 28 + l_off, p["boots"])
        fill_rect(im, 17, 24, 19, 28 + r_off, p["boots"])
    else: # side
        fill_rect(im, 14, 24, 17, 28, p["boots"])

    # Plate Armor (Bright Silver & Gold Guild Emblem)
    fill_rect(im, 11, cy, 20, cy + 7, p["armor_base"])
    fill_rect(im, 12, cy, 19, cy + 2, p["armor_bright"])
    # Gold Crest
    draw_pixel(im, 15, cy + 3, p["gold"])
    draw_pixel(im, 16, cy + 3, p["gold"])

    # Knight Sword on left hip
    sw_x = 9 if facing != "side" else 11
    fill_rect(im, sw_x, cy + 3, sw_x + 1, cy + 9, p["scabbard"])
    fill_rect(im, sw_x - 1, cy + 2, sw_x + 2, cy + 2, p["sword_hilt"])

    # Head & Golden Brown Hair
    hy = 8 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        draw_pixel(im, 15, hy + 5, p["skin_shadow"])
    elif facing == "back":
        fill_rect(im, 12, hy, 19, hy + 5, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])

    return im

def draw_kaelen(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = KAELEN_PAL
    bob = 1 if step in (1, 3) else 0

    # Boots & Pants
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 13, 24, 14, 28 + l_off, p["boots"])
        fill_rect(im, 17, 24, 18, 28 + r_off, p["boots"])
    else: # side
        fill_rect(im, 14, 24, 17, 28, p["boots"])

    # Fur-trimmed vest & Green hunter shirt
    vy = 16 + bob
    fill_rect(im, 11, vy, 20, vy + 7, p["shirt"])
    fill_rect(im, 12, vy + 1, 19, vy + 7, p["vest"])
    # Fur Collar
    fill_rect(im, 12, vy, 19, vy + 1, p["fur_trim"])
    # Pet treat pouch
    fill_rect(im, 19, vy + 4, 21, vy + 6, p["pouch"])

    # Animal Fur Hat with Ears
    hy = 8 + bob
    fill_rect(im, 13, hy + 1, 18, hy + 7, p["skin"])
    # Hat base
    fill_rect(im, 11, hy - 1, 20, hy + 2, p["fur_hat"])
    # Fur ears
    draw_pixel(im, 12, hy - 2, p["fur_ear"])
    draw_pixel(im, 19, hy - 2, p["fur_ear"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
    elif facing == "back":
        fill_rect(im, 12, hy + 2, 19, hy + 6, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])

    return im

def draw_lina(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = LINA_PAL
    bob = 1 if step in (1, 3) else 0

    # Shoes
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 13, 27 + l_off, 14, 28 + l_off, p["shoes"])
        fill_rect(im, 17, 27 + r_off, 18, 28 + r_off, p["shoes"])
    else: # side
        fill_rect(im, 15, 27, 17, 28, p["shoes"])

    # Merchant Dress & Apron
    dy = 17 + bob
    fill_rect(im, 12, dy, 19, 26, p["dress"])
    fill_rect(im, 13, dy + 2, 18, 25, p["apron"])
    # Shiny gold purse
    fill_rect(im, 19, dy + 3, 21, dy + 5, p["gold_bag"])

    # Golden Yellow Kerchief Headscarf
    hy = 9 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    fill_rect(im, 11, hy - 1, 20, hy + 3, p["kerchicf"])
    fill_rect(im, 11, hy + 3, 13, hy + 5, p["kerchicf_trim"])
    fill_rect(im, 18, hy + 3, 20, hy + 5, p["kerchicf_trim"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        # Cute smile
        draw_pixel(im, 15, hy + 5, p["dress"])
        draw_pixel(im, 16, hy + 5, p["dress"])
    elif facing == "back":
        fill_rect(im, 12, hy + 2, 19, hy + 7, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])

    return im

def draw_villager(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = VILLAGER_PAL
    bob = 1 if step in (1, 3) else 0

    # Pants & Shoes
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 13, 23, 14, 27 + l_off, p["pants"])
        fill_rect(im, 17, 23, 18, 27 + r_off, p["pants"])
        fill_rect(im, 13, 27 + l_off, 14, 28 + l_off, p["shoes"])
        fill_rect(im, 17, 27 + r_off, 18, 28 + r_off, p["shoes"])
    else: # side
        fill_rect(im, 14, 23, 17, 27, p["pants"])
        fill_rect(im, 15, 27, 17, 28, p["shoes"])

    # Peasant Shirt & Basket of Apples
    sy = 16 + bob
    fill_rect(im, 11, sy, 20, sy + 6, p["shirt"])
    # Straw basket
    fill_rect(im, 20, sy + 2, 23, sy + 6, p["basket"])
    draw_pixel(im, 21, sy + 2, p["apples"])
    draw_pixel(im, 22, sy + 3, p["apples"])

    # Straw Hat with Red Ribbon
    hy = 8 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    # Broad straw brim
    fill_rect(im, 10, hy + 1, 21, hy + 2, p["hat"])
    fill_rect(im, 12, hy - 1, 19, hy + 1, p["hat"])
    fill_rect(im, 12, hy + 1, 19, hy + 1, p["hat_ribbon"])

    if facing == "front":
        draw_pixel(im, 14, hy + 4, p["eyes"])
        draw_pixel(im, 17, hy + 4, p["eyes"])
    elif facing == "back":
        fill_rect(im, 13, hy + 2, 18, hy + 6, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 4, p["eyes"])

    return im

# Master Generation Dispatcher
NPC_GENERATORS = {
    "eldrin": draw_eldrin,
    "boris": draw_boris,
    "elena": draw_elena,
    "carlos": draw_carlos,
    "kaelen": draw_kaelen,
    "lina": draw_lina,
    "villager": draw_villager
}

def generate_all():
    print("Generating distinct NPC sprites in", OUTPUT_DIR)
    for npc_key, draw_fn in NPC_GENERATORS.items():
        # 1. Idle Sprites (Front, Back, Side)
        for direction in ["front", "back", "side"]:
            img = draw_fn(direction, 0)
            png_name = f"npc_{npc_key}_idle_{direction}.png"
            path = os.path.join(OUTPUT_DIR, png_name)
            img.save(path)
            make_meta(path)

        # 2. Walk Sprites (4 frames for each direction)
        for direction in ["front", "back", "side"]:
            for frame_idx in range(1, 5):
                img = draw_fn(direction, frame_idx)
                png_name = f"npc_{npc_key}_walk_{direction}_{frame_idx}.png"
                path = os.path.join(OUTPUT_DIR, png_name)
                img.save(path)
                make_meta(path)

    print("Successfully generated all distinct NPC sprites and .meta files!")

if __name__ == "__main__":
    generate_all()

