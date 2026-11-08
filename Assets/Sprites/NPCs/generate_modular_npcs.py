import os
import hashlib
from PIL import Image, ImageDraw

BASE_DIR = "Assets/Sprites/NPCs/Modular"
os.makedirs(BASE_DIR, exist_ok=True)

# Helper function to generate Unity .meta file with PPU = 16
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
  spritePixelsToUnits: 16
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

CLR_TRANSPARENT = (0, 0, 0, 0)

def draw_pixel(im, x, y, color):
    if 0 <= x < 32 and 0 <= y < 32:
        im.putpixel((x, y), color)

def fill_rect(im, x1, y1, x2, y2, color):
    for x in range(x1, x2 + 1):
        for y in range(y1, y2 + 1):
            draw_pixel(im, x, y, color)

def save_sprite(im, subfolder, filename):
    folder = os.path.join(BASE_DIR, subfolder)
    os.makedirs(folder, exist_ok=True)
    path = os.path.join(folder, filename)
    im.save(path)
    make_meta(path)

# =========================================================================
# 1. TRƯỞNG LÀNG ELDRIN (BODY + OAK STAFF + ORB GLOW)
# =========================================================================
ELDRIN_PAL = {
    "skin": (245, 215, 190, 255),
    "skin_shadow": (210, 175, 150, 255),
    "hair": (248, 250, 255, 255),
    "hair_shadow": (185, 195, 210, 255),
    "robe": (28, 59, 130, 255),
    "robe_shadow": (14, 34, 84, 255),
    "robe_highlight": (59, 130, 246, 255),
    "gold": (245, 158, 11, 255),
    "gold_bright": (253, 224, 71, 255),
    "staff_wood": (120, 53, 15, 255),
    "staff_wood_light": (180, 83, 9, 255),
    "gem": (56, 189, 248, 255),
    "gem_glow": (165, 243, 252, 255),
    "gem_deep": (2, 132, 199, 255),
    "shoes": (70, 50, 38, 255),
    "eyes": (30, 50, 90, 255)
}

def draw_eldrin_body(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = ELDRIN_PAL
    bob = 1 if step in (1, 3) else 0

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
    # Gold trims & belt
    fill_rect(im, 15, by, 16, 25, p["gold"])
    fill_rect(im, 13, by + 4, 18, by + 5, p["gold"]) # belt
    fill_rect(im, 12, 26, 19, 26, p["gold_bright"]) # bottom trim

    # Head
    hy = 9 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    # Hair & Hood
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])
    fill_rect(im, 11, hy + 1, 12, hy + 5, p["hair"])
    fill_rect(im, 19, hy + 1, 20, hy + 5, p["hair"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        # Big white beard reaching chest
        fill_rect(im, 13, hy + 5, 18, hy + 9, p["hair"])
        fill_rect(im, 14, hy + 9, 17, hy + 11, p["hair"])
        fill_rect(im, 15, hy + 11, 16, hy + 12, p["hair_shadow"])
        # Right arm & hand (free, positioned ready to grasp staff)
        fill_rect(im, 20, by, 22, by + 2, p["robe"])
        draw_pixel(im, 22, by + 2, p["skin"])
        # Left arm
        fill_rect(im, 9, by, 11, by + 2, p["robe"])
        draw_pixel(im, 9, by + 2, p["skin"])
    elif facing == "back":
        # White hair flowing down
        fill_rect(im, 12, hy, 19, hy + 8, p["hair"])
        fill_rect(im, 13, hy + 8, 18, hy + 10, p["hair_shadow"])
        # Sleeves
        fill_rect(im, 20, by, 22, by + 2, p["robe"])
        fill_rect(im, 9, by, 11, by + 2, p["robe"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])
        fill_rect(im, 15, hy + 5, 18, hy + 9, p["hair"])
        fill_rect(im, 16, hy + 9, 18, hy + 11, p["hair"])
        fill_rect(im, 18, by, 20, by + 2, p["robe"])
        draw_pixel(im, 20, by + 2, p["skin"])

    return im

def draw_eldrin_staff(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = ELDRIN_PAL
    # Staff in right hand (x=23 in front, x=22 in side, x=23 in back)
    sx = 23 if facing != "side" else 22
    for y in range(11, 29):
        draw_pixel(im, sx, y, p["staff_wood"])
    # Curved crown holding gem
    draw_pixel(im, sx - 1, 10, p["staff_wood_light"])
    draw_pixel(im, sx + 1, 10, p["staff_wood_light"])
    draw_pixel(im, sx - 1, 9, p["staff_wood_light"])
    draw_pixel(im, sx, 10, p["gem_deep"])
    return im

def draw_eldrin_orb_glow():
    # 32x32 image with the glowing cyan orb positioned exactly at staff head (x=23, y=8-9)
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = ELDRIN_PAL
    cx, cy = 23, 9
    # Radial aura glow
    fill_rect(im, cx - 2, cy - 2, cx + 2, cy + 2, (56, 189, 248, 90))
    fill_rect(im, cx - 1, cy - 1, cx + 1, cy + 1, p["gem"])
    draw_pixel(im, cx, cy, p["gem_glow"])
    return im

# =========================================================================
# 2. THỢ RÈN BORIS (BODY + HEAVY SLEDGEHAMMER)
# =========================================================================
BORIS_PAL = {
    "skin": (245, 175, 125, 255),
    "skin_shadow": (195, 125, 80, 255),
    "hair": (69, 26, 3, 255),
    "hair_shadow": (45, 15, 2, 255),
    "shirt": (215, 210, 200, 255),
    "apron": (120, 53, 15, 255),
    "apron_shadow": (92, 38, 7, 255),
    "apron_straps": (60, 24, 5, 255),
    "pants": (55, 45, 40, 255),
    "gloves": (60, 50, 45, 255),
    "hammer_head": (148, 163, 184, 255),
    "hammer_steel": (241, 245, 249, 255),
    "hammer_shadow": (51, 65, 85, 255),
    "hammer_handle": (113, 63, 18, 255),
    "shoes": (45, 35, 30, 255),
    "eyes": (40, 25, 18, 255)
}

def draw_boris_body(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = BORIS_PAL
    bob = 1 if step in (1, 3) else 0

    # Boots & Pants
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

    # Muscular Torso & Leather Apron
    ty = 16 + bob
    fill_rect(im, 10, ty, 21, ty + 7, p["shirt"])
    fill_rect(im, 12, ty + 1, 19, ty + 7, p["apron"])
    # Apron straps & belt
    draw_pixel(im, 13, ty, p["apron_straps"])
    draw_pixel(im, 18, ty, p["apron_straps"])
    fill_rect(im, 12, ty + 4, 19, ty + 4, p["apron_straps"])

    # Bare muscular arms
    fill_rect(im, 8, ty + 1, 11, ty + 4, p["skin"])
    fill_rect(im, 20, ty + 1, 23, ty + 4, p["skin"])
    # Heavy leather gloves
    fill_rect(im, 8, ty + 4, 11, ty + 6, p["gloves"])
    fill_rect(im, 20, ty + 4, 23, ty + 6, p["gloves"])

    # Head & Beard
    hy = 8 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
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

def draw_boris_hammer(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = BORIS_PAL
    # Held in left hand (x=7 in front, x=6 in side)
    hx = 7 if facing != "side" else 8
    # Handle
    for y in range(16, 27):
        draw_pixel(im, hx, y, p["hammer_handle"])
    # Heavy sledge head
    fill_rect(im, hx - 3, 13, hx + 1, 16, p["hammer_head"])
    fill_rect(im, hx - 2, 13, hx, 14, p["hammer_steel"])
    draw_pixel(im, hx - 3, 16, p["hammer_shadow"])
    return im

# =========================================================================
# 3. DƯỢC SĨ ELENA (BODY + CROSSBODY POUCH & POTIONS)
# =========================================================================
ELENA_PAL = {
    "skin": (255, 225, 205, 255),
    "skin_shadow": (225, 185, 165, 255),
    "hair": (120, 53, 15, 255),
    "hair_shadow": (92, 38, 7, 255),
    "flower": (34, 197, 94, 255),
    "flower_center": (254, 240, 138, 255),
    "robe": (45, 212, 191, 255),
    "robe_shadow": (15, 118, 110, 255),
    "apron": (248, 250, 252, 255),
    "apron_shadow": (203, 213, 225, 255),
    "pouch_leather": (146, 64, 14, 255),
    "pouch_strap": (120, 53, 15, 255),
    "potion_purple": (168, 85, 247, 255),
    "potion_green": (52, 211, 153, 255),
    "cork": (217, 119, 6, 255),
    "shoes": (95, 65, 50, 255),
    "eyes": (13, 148, 136, 255)
}

def draw_elena_body(facing, step):
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

    # Robe / Mint Coat & White Apron
    by = 17 + bob
    fill_rect(im, 12, by, 19, 26, p["robe"])
    fill_rect(im, 14, by + 1, 17, 25, p["apron"])
    # Sleeves
    fill_rect(im, 10, by, 11, by + 4, p["robe"])
    fill_rect(im, 20, by, 21, by + 4, p["robe"])
    draw_pixel(im, 10, by + 4, p["skin"])
    draw_pixel(im, 21, by + 4, p["skin"])

    # Head & Chestnut Hair
    hy = 9 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])
    fill_rect(im, 11, hy + 2, 13, hy + 6, p["hair"])
    fill_rect(im, 18, hy + 2, 20, hy + 6, p["hair"])
    # Flower hairpin
    draw_pixel(im, 12, hy + 1, p["flower"])
    draw_pixel(im, 12, hy, p["flower_center"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        # Side braid resting on shoulder
        fill_rect(im, 11, hy + 6, 12, hy + 10, p["hair"])
    elif facing == "back":
        fill_rect(im, 12, hy, 19, hy + 8, p["hair"])
        fill_rect(im, 14, hy + 8, 16, hy + 11, p["hair"]) # braid down back
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])
        fill_rect(im, 13, hy + 6, 14, hy + 10, p["hair"])

    return im

def draw_elena_pouch(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = ELENA_PAL
    # Crossbody strap from left shoulder (x=13, y=17) to right hip (x=19, y=21)
    if facing == "front":
        draw_pixel(im, 13, 17, p["pouch_strap"])
        draw_pixel(im, 15, 19, p["pouch_strap"])
        draw_pixel(im, 17, 21, p["pouch_strap"])
        # Satchel pouch on right hip
        fill_rect(im, 19, 21, 22, 25, p["pouch_leather"])
        # Potions attached to pouch: purple & green
        draw_pixel(im, 19, 20, p["cork"])
        draw_pixel(im, 19, 21, p["potion_purple"])
        draw_pixel(im, 21, 20, p["cork"])
        draw_pixel(im, 21, 21, p["potion_green"])
    elif facing == "back":
        draw_pixel(im, 19, 17, p["pouch_strap"])
        draw_pixel(im, 17, 19, p["pouch_strap"])
        draw_pixel(im, 15, 21, p["pouch_strap"])
        fill_rect(im, 13, 22, 16, 25, p["pouch_leather"])
    else: # side
        fill_rect(im, 16, 21, 19, 25, p["pouch_leather"])
        draw_pixel(im, 17, 20, p["cork"])
        draw_pixel(im, 17, 21, p["potion_purple"])
    return im

# =========================================================================
# 4. HỘI TRƯỞNG CARLOS (BODY + CRIMSON CAPE + KNIGHT SWORD)
# =========================================================================
CARLOS_PAL = {
    "skin": (245, 195, 160, 255),
    "skin_shadow": (205, 150, 115, 255),
    "hair": (217, 119, 6, 255),
    "hair_shadow": (180, 83, 9, 255),
    "armor_bright": (248, 250, 252, 255),
    "armor_base": (226, 232, 240, 255),
    "armor_shadow": (148, 163, 184, 255),
    "armor_dark": (71, 85, 105, 255),
    "gold": (245, 158, 11, 255),
    "cape": (220, 38, 38, 255),
    "cape_shadow": (153, 27, 27, 255),
    "sword_scabbard": (30, 41, 59, 255),
    "sword_steel": (248, 250, 252, 255),
    "boots": (71, 85, 105, 255),
    "eyes": (30, 41, 59, 255)
}

def draw_carlos_body(facing, step):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = CARLOS_PAL
    bob = 1 if step in (1, 3) else 0

    # Boots
    if facing == "front" or facing == "back":
        l_off = 1 if step == 1 else 0
        r_off = 1 if step == 3 else 0
        fill_rect(im, 12, 24, 14, 28 + l_off, p["boots"])
        fill_rect(im, 17, 24, 19, 28 + r_off, p["boots"])
    else: # side
        fill_rect(im, 14, 24, 17, 28, p["boots"])

    # Plate Armor
    cy = 16 + bob
    fill_rect(im, 11, cy, 20, cy + 7, p["armor_base"])
    fill_rect(im, 12, cy, 19, cy + 2, p["armor_bright"])
    # Gold Crest
    draw_pixel(im, 15, cy + 3, p["gold"])
    draw_pixel(im, 16, cy + 3, p["gold"])
    # Arms in plate
    fill_rect(im, 9, cy, 10, cy + 4, p["armor_shadow"])
    fill_rect(im, 21, cy, 22, cy + 4, p["armor_shadow"])

    # Head & Golden Brown Hair
    hy = 8 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    fill_rect(im, 12, hy - 1, 19, hy + 2, p["hair"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        # Left hand resting at waist/scabbard
        draw_pixel(im, 9, cy + 4, p["skin"])
    elif facing == "back":
        fill_rect(im, 12, hy, 19, hy + 5, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])

    return im

def draw_carlos_cape(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = CARLOS_PAL
    if facing == "front":
        # Cape edges visible on both sides
        fill_rect(im, 8, 16, 10, 26, p["cape"])
        fill_rect(im, 21, 16, 23, 26, p["cape"])
    elif facing == "back":
        # Full crimson cape covering back
        fill_rect(im, 10, 16, 21, 26, p["cape"])
        fill_rect(im, 11, 18, 20, 26, p["cape_shadow"])
    else: # side
        fill_rect(im, 9, 16, 12, 26, p["cape"])
        fill_rect(im, 8, 19, 10, 26, p["cape_shadow"])
    return im

def draw_carlos_sword(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = CARLOS_PAL
    # Knight Sword on left hip (x=8 in front, x=10 in side, x=22 in back)
    if facing == "front":
        # Scabbard & Golden Hilt
        fill_rect(im, 8, 19, 9, 26, p["sword_scabbard"])
        fill_rect(im, 7, 18, 10, 18, p["gold"]) # Crossguard
        draw_pixel(im, 8, 17, p["gold"])        # Pommel
    elif facing == "back":
        fill_rect(im, 22, 19, 23, 26, p["sword_scabbard"])
        fill_rect(im, 21, 18, 24, 18, p["gold"])
    else: # side
        fill_rect(im, 11, 19, 12, 26, p["sword_scabbard"])
        fill_rect(im, 10, 18, 13, 18, p["gold"])
    return im

# =========================================================================
# 5. QUẢN THÚ KAELEN & CHIM ƯNG (BODY + PET FALCON)
# =========================================================================
KAELEN_PAL = {
    "skin": (245, 200, 160, 255),
    "skin_shadow": (205, 155, 120, 255),
    "hair": (140, 85, 45, 255),
    "fur_hat": (21, 128, 61, 255),
    "fur_ear": (180, 83, 9, 255),
    "fur_collar": (248, 250, 252, 255),
    "vest": (146, 64, 14, 255),
    "shirt": (22, 101, 52, 255),
    "glove": (120, 53, 15, 255),
    "pouch": (217, 119, 6, 255),
    "pants": (80, 65, 55, 255),
    "boots": (50, 40, 35, 255),
    "eyes": (22, 101, 52, 255)
}

FALCON_PAL = {
    "feathers_brown": (120, 53, 15, 255),
    "feathers_dark": (69, 26, 3, 255),
    "chest_white": (248, 250, 252, 255),
    "beak_gold": (245, 158, 11, 255),
    "eyes": (15, 23, 42, 255)
}

def draw_kaelen_body(facing, step):
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

    # Hunter Vest & Green Shirt
    vy = 16 + bob
    fill_rect(im, 11, vy, 20, vy + 7, p["shirt"])
    fill_rect(im, 12, vy + 1, 19, vy + 7, p["vest"])
    # Fluffy Fur Collar
    fill_rect(im, 12, vy, 19, vy + 1, p["fur_collar"])
    # Treat Pouch
    fill_rect(im, 19, vy + 4, 21, vy + 6, p["pouch"])

    # Left Arm with thick falconer glove (ready for falcon to perch)
    fill_rect(im, 8, vy + 1, 10, vy + 3, p["shirt"])
    fill_rect(im, 7, vy + 3, 10, vy + 5, p["glove"])
    # Right Arm
    fill_rect(im, 21, vy + 1, 22, vy + 4, p["shirt"])

    # Animal Fur Hat with Ears
    hy = 8 + bob
    fill_rect(im, 13, hy + 1, 18, hy + 7, p["skin"])
    fill_rect(im, 11, hy - 1, 20, hy + 2, p["fur_hat"])
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

def draw_falcon_perched(frame_idx):
    # 24x24 sprite positioned so it perches on Kaelen's glove
    im = Image.new("RGBA", (24, 24), CLR_TRANSPARENT)
    p = FALCON_PAL
    # Falcon body centered around x=12, y=14
    fill_rect(im, 10, 10, 15, 17, p["feathers_brown"])
    fill_rect(im, 11, 12, 14, 16, p["chest_white"])
    # Head & beak
    fill_rect(im, 11, 7, 14, 9, p["feathers_brown"])
    draw_pixel(im, 15, 8, p["beak_gold"]) # Beak facing right
    draw_pixel(im, 13, 8, p["eyes"])
    # Tail
    draw_pixel(im, 9, 18, p["feathers_dark"])
    draw_pixel(im, 10, 19, p["feathers_dark"])
    # Talons gripping glove
    draw_pixel(im, 11, 17, p["beak_gold"])
    draw_pixel(im, 14, 17, p["beak_gold"])

    if frame_idx == 2:
        # Subtle head glance or wing tuck
        draw_pixel(im, 10, 11, p["feathers_dark"])
    return im

def draw_falcon_fly(frame_idx):
    # 24x24 flying animation
    im = Image.new("RGBA", (24, 24), CLR_TRANSPARENT)
    p = FALCON_PAL
    # Body
    fill_rect(im, 10, 10, 14, 14, p["chest_white"])
    # Head & beak
    fill_rect(im, 15, 9, 17, 11, p["feathers_brown"])
    draw_pixel(im, 18, 10, p["beak_gold"])
    draw_pixel(im, 16, 10, p["eyes"])

    # Flapping wings in 4 frames
    if frame_idx == 1: # Wings Up
        fill_rect(im, 8, 4, 13, 9, p["feathers_brown"])
        fill_rect(im, 7, 3, 10, 6, p["feathers_dark"])
    elif frame_idx == 2: # Wings Mid-glide
        fill_rect(im, 5, 9, 12, 12, p["feathers_brown"])
        fill_rect(im, 3, 9, 6, 11, p["feathers_dark"])
    elif frame_idx == 3: # Wings Down
        fill_rect(im, 8, 13, 13, 17, p["feathers_brown"])
        fill_rect(im, 7, 16, 11, 19, p["feathers_dark"])
    else: # Wings Recover
        fill_rect(im, 7, 7, 12, 11, p["feathers_brown"])

    # Tail feathers
    fill_rect(im, 6, 12, 9, 14, p["feathers_dark"])
    return im

# =========================================================================
# 6. THƯƠNG NHÂN LINA (BODY + MARKET BASKET + WAIST POUCH)
# =========================================================================
LINA_PAL = {
    "skin": (255, 220, 195, 255),
    "skin_shadow": (225, 180, 155, 255),
    "hair": (185, 100, 40, 255),
    "kerchicf": (245, 158, 11, 255),
    "kerchicf_trim": (254, 243, 199, 255),
    "dress": (234, 88, 12, 255),
    "dress_shadow": (194, 65, 12, 255),
    "apron": (248, 250, 252, 255),
    "gold_bag": (253, 224, 71, 255),
    "wicker": (217, 119, 6, 255),
    "wicker_dark": (146, 64, 14, 255),
    "apples": (239, 68, 68, 255),
    "shoes": (105, 60, 45, 255),
    "eyes": (70, 40, 25, 255)
}

def draw_lina_body(facing, step):
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

    # Arms (Right arm ready to carry basket)
    fill_rect(im, 20, dy, 22, dy + 3, (255, 255, 255, 255))
    draw_pixel(im, 21, dy + 3, p["skin"])
    fill_rect(im, 9, dy, 11, dy + 3, (255, 255, 255, 255))
    draw_pixel(im, 10, dy + 3, p["skin"])

    # Golden Yellow Kerchief Headscarf
    hy = 9 + bob
    fill_rect(im, 13, hy, 18, hy + 7, p["skin"])
    fill_rect(im, 11, hy - 1, 20, hy + 3, p["kerchicf"])
    fill_rect(im, 11, hy + 3, 13, hy + 5, p["kerchicf_trim"])
    fill_rect(im, 18, hy + 3, 20, hy + 5, p["kerchicf_trim"])

    if facing == "front":
        draw_pixel(im, 14, hy + 3, p["eyes"])
        draw_pixel(im, 17, hy + 3, p["eyes"])
        draw_pixel(im, 15, hy + 5, p["dress"])
        draw_pixel(im, 16, hy + 5, p["dress"])
    elif facing == "back":
        fill_rect(im, 12, hy + 2, 19, hy + 7, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 3, p["eyes"])

    return im

def draw_lina_basket(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = LINA_PAL
    # Held on right arm (x=21 in front, x=20 in side, x=9 in back)
    bx = 21 if facing != "back" else 9
    fill_rect(im, bx - 2, 20, bx + 2, 24, p["wicker"])
    fill_rect(im, bx - 1, 21, bx + 1, 23, p["wicker_dark"])
    # Handle over arm
    draw_pixel(im, bx, 19, p["wicker"])
    draw_pixel(im, bx, 18, p["wicker"])
    # Red apples inside basket
    draw_pixel(im, bx - 1, 20, p["apples"])
    draw_pixel(im, bx + 1, 20, p["apples"])
    return im

def draw_lina_pouch(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = LINA_PAL
    # Golden money pouch & book at belt
    if facing == "front":
        fill_rect(im, 13, 20, 15, 22, p["gold_bag"])
    elif facing == "side":
        fill_rect(im, 14, 20, 16, 22, p["gold_bag"])
    return im

# =========================================================================
# 7. CƯ DÂN OAKHAVEN (BODY WITH FULL HAIR + STRAW HAT + FRUIT BASKET)
# =========================================================================
VILLAGER_PAL = {
    "skin": (245, 205, 165, 255),
    "skin_shadow": (210, 165, 125, 255),
    "hair": (120, 53, 15, 255), # Full Chestnut Brown Hair
    "hair_highlight": (180, 83, 9, 255),
    "shirt": (254, 243, 199, 255), # Beige
    "shirt_shadow": (253, 230, 138, 255),
    "pants": (2, 132, 199, 255), # Teal Blue
    "pants_shadow": (3, 105, 161, 255),
    "hat_straw": (253, 224, 71, 255),
    "hat_ribbon": (239, 68, 68, 255),
    "hat_shadow": (217, 119, 6, 255),
    "basket": (180, 83, 9, 255),
    "apples": (239, 68, 68, 255),
    "oranges": (245, 158, 11, 255),
    "shoes": (95, 65, 50, 255),
    "eyes": (30, 41, 59, 255)
}

def draw_villager_body(facing, step):
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

    # Shirt & Arms
    sy = 16 + bob
    fill_rect(im, 11, sy, 20, sy + 6, p["shirt"])
    # Arms ready to hold basket or swing
    fill_rect(im, 9, sy + 1, 10, sy + 4, p["shirt"])
    draw_pixel(im, 10, sy + 4, p["skin"])
    fill_rect(im, 21, sy + 1, 22, sy + 4, p["shirt"])
    draw_pixel(im, 21, sy + 4, p["skin"])

    # Head & FULL CHESTNUT BROWN HAIR (Complete hair drawn when hat is off!)
    hy = 8 + bob
    fill_rect(im, 13, hy + 2, 18, hy + 7, p["skin"])
    # Fluffy chestnut hair
    fill_rect(im, 12, hy - 1, 19, hy + 3, p["hair"])
    fill_rect(im, 11, hy + 2, 13, hy + 5, p["hair"])
    fill_rect(im, 18, hy + 2, 20, hy + 5, p["hair"])
    draw_pixel(im, 15, hy - 1, p["hair_highlight"])
    draw_pixel(im, 16, hy - 1, p["hair_highlight"])

    if facing == "front":
        draw_pixel(im, 14, hy + 4, p["eyes"])
        draw_pixel(im, 17, hy + 4, p["eyes"])
    elif facing == "back":
        fill_rect(im, 12, hy, 19, hy + 6, p["hair"])
    else: # side
        draw_pixel(im, 17, hy + 4, p["eyes"])

    return im

def draw_villager_straw_hat(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = VILLAGER_PAL
    hy = 8
    # Broad straw brim & red ribbon
    fill_rect(im, 10, hy + 1, 21, hy + 2, p["hat_straw"])
    fill_rect(im, 12, hy - 1, 19, hy + 1, p["hat_straw"])
    fill_rect(im, 12, hy + 1, 19, hy + 1, p["hat_ribbon"]) # Red Ribbon
    return im

def draw_villager_fruit_basket(facing):
    im = Image.new("RGBA", (32, 32), CLR_TRANSPARENT)
    p = VILLAGER_PAL
    # Fruit basket held in front of chest/hands (x=14..18, y=18..23)
    fill_rect(im, 13, 19, 18, 23, p["basket"])
    # Apples & Oranges
    draw_pixel(im, 14, 18, p["apples"])
    draw_pixel(im, 16, 18, p["oranges"])
    draw_pixel(im, 17, 18, p["apples"])
    return im

def draw_villager_hat_world():
    # Mũ rơm đặt nghiêng trên mặt đất làm World Object
    im = Image.new("RGBA", (24, 24), CLR_TRANSPARENT)
    p = VILLAGER_PAL
    fill_rect(im, 4, 12, 19, 15, p["hat_straw"])
    fill_rect(im, 7, 9, 16, 12, p["hat_straw"])
    fill_rect(im, 7, 12, 16, 12, p["hat_ribbon"])
    return im

def draw_villager_basket_world():
    # Giỏ hoa quả đặt trên mặt đất làm World Object
    im = Image.new("RGBA", (24, 24), CLR_TRANSPARENT)
    p = VILLAGER_PAL
    fill_rect(im, 5, 11, 18, 18, p["basket"])
    fill_rect(im, 6, 13, 17, 17, p["hat_shadow"])
    # Handle arch
    for y in range(7, 12):
        draw_pixel(im, 6, y, p["basket"])
        draw_pixel(im, 17, y, p["basket"])
    fill_rect(im, 7, 7, 16, 7, p["basket"])
    # Fresh fruit inside
    draw_pixel(im, 8, 10, p["apples"])
    draw_pixel(im, 10, 10, p["oranges"])
    draw_pixel(im, 12, 9, p["apples"])
    draw_pixel(im, 14, 10, p["oranges"])
    draw_pixel(im, 15, 10, p["apples"])
    return im

# =========================================================================
# MAIN GENERATION PIPELINE
# =========================================================================
def generate_all_modular():
    print("=== Generating Modular NPC Sprites (PPU = 16) ===")

    # 1. Eldrin
    print("Generating Eldrin...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_eldrin_body(dir, 0), "Eldrin", f"npc_eldrin_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_eldrin_body(dir, step), "Eldrin", f"npc_eldrin_body_walk_{dir}_{step}.png")
        save_sprite(draw_eldrin_staff(dir), "Eldrin", f"prop_eldrin_staff_{dir}.png")
    save_sprite(draw_eldrin_orb_glow(), "Eldrin", "prop_eldrin_orb_glow.png")

    # 2. Boris
    print("Generating Boris...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_boris_body(dir, 0), "Boris", f"npc_boris_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_boris_body(dir, step), "Boris", f"npc_boris_body_walk_{dir}_{step}.png")
        save_sprite(draw_boris_hammer(dir), "Boris", f"prop_boris_hammer_{dir}.png")

    # 3. Elena
    print("Generating Elena...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_elena_body(dir, 0), "Elena", f"npc_elena_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_elena_body(dir, step), "Elena", f"npc_elena_body_walk_{dir}_{step}.png")
        save_sprite(draw_elena_pouch(dir), "Elena", f"prop_elena_pouch_{dir}.png")

    # 4. Carlos
    print("Generating Carlos...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_carlos_body(dir, 0), "Carlos", f"npc_carlos_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_carlos_body(dir, step), "Carlos", f"npc_carlos_body_walk_{dir}_{step}.png")
        save_sprite(draw_carlos_cape(dir), "Carlos", f"prop_carlos_cape_{dir}.png")
        save_sprite(draw_carlos_sword(dir), "Carlos", f"prop_carlos_sword_{dir}.png")

    # 5. Kaelen & Falcon
    print("Generating Kaelen & Falcon...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_kaelen_body(dir, 0), "Kaelen", f"npc_kaelen_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_kaelen_body(dir, step), "Kaelen", f"npc_kaelen_body_walk_{dir}_{step}.png")
    # Falcon independent animations
    save_sprite(draw_falcon_perched(1), "Falcon", "falcon_idle_perched_1.png")
    save_sprite(draw_falcon_perched(2), "Falcon", "falcon_idle_perched_2.png")
    for f in range(1, 5):
        save_sprite(draw_falcon_fly(f), "Falcon", f"falcon_fly_loop_{f}.png")

    # 6. Lina
    print("Generating Lina...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_lina_body(dir, 0), "Lina", f"npc_lina_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_lina_body(dir, step), "Lina", f"npc_lina_body_walk_{dir}_{step}.png")
        save_sprite(draw_lina_basket(dir), "Lina", f"prop_lina_basket_{dir}.png")
        save_sprite(draw_lina_pouch(dir), "Lina", f"prop_lina_pouch_{dir}.png")

    # 7. Villager
    print("Generating Villager...")
    for dir in ["front", "back", "side"]:
        save_sprite(draw_villager_body(dir, 0), "Villager", f"npc_villager_body_idle_{dir}.png")
        for step in range(1, 5):
            save_sprite(draw_villager_body(dir, step), "Villager", f"npc_villager_body_walk_{dir}_{step}.png")
        save_sprite(draw_villager_straw_hat(dir), "Villager", f"prop_villager_hat_{dir}.png")
        save_sprite(draw_villager_fruit_basket(dir), "Villager", f"prop_villager_basket_{dir}.png")
    save_sprite(draw_villager_hat_world(), "Villager", "prop_villager_hat_world.png")
    save_sprite(draw_villager_basket_world(), "Villager", "prop_villager_basket_world.png")

    print("=== Successfully Generated All Modular NPC Sprites! ===")

if __name__ == "__main__":
    generate_all_modular()
