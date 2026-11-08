"""
Master Pixel Art Generator for Kaelen, Falcon & Villager
---------------------------------------------------------
1. Quản Thú Kaelen (NPC_PetMaster_Kaelen)
2. Chim Ưng (Pet_Falcon)
3. Cư Dân Oakhaven (NPC_Villager_Wanderer)

Strictly adhering to:
- Dark Tinted Outlines (1px crisp contour)
- 3-Tone Shading (Highlight, Base, Shadow + Dithering)
- Modular Layer Separation
- Grounded Drop Shadow
- Full 4-Direction Walk Cycles (Front, Back, Side Right, Side Left)
- Canvas: 32x32, PPU = 16 (Falcon is 24x24 / 32x32)
"""

import os
from PIL import Image
import numpy as np
from pixel_art_shading_helper import PixelCanvas, ColorRamp, hex_to_rgba

PALETTES = {
    # === KAELEN (Beastmaster) ===
    'KAELEN_SKIN': ColorRamp('#fff0e6', '#fcd5be', '#d99e80', '#9c664c', '#472a1c'),
    'KAELEN_HAIR': ColorRamp('#784c35', '#573320', '#3b1f11', '#241107', '#120702'),
    'KAELEN_HAT': ColorRamp('#4d7c5a', '#335c3e', '#1e3d26', '#122617', '#08140b'),
    'KAELEN_FUR': ColorRamp('#ffffff', '#f1f5f9', '#cbd5e1', '#94a3b8', '#334155'),
    'KAELEN_VEST': ColorRamp('#b47238', '#854d1d', '#542e0d', '#331a06', '#1a0b02'),
    'KAELEN_TUNIC': ColorRamp('#22c55e', '#16a34a', '#15803d', '#14532d', '#052e16'),
    'KAELEN_GLOVE': ColorRamp('#a16207', '#78350f', '#451a03', '#2b1001', '#140700'),
    'KAELEN_BOOTS': ColorRamp('#61422d', '#422a1b', '#29180e', '#170b05', '#0a0402'),

    # === FALCON ===
    'FALCON_WHITE': ColorRamp('#ffffff', '#f8fafc', '#cbd5e1', '#94a3b8', '#475569'),
    'FALCON_BROWN': ColorRamp('#a16207', '#78350f', '#451a03', '#2e1101', '#140600'),
    'FALCON_BEAK': ColorRamp('#fde047', '#eab308', '#ca8a04', '#854d0e', '#3a1f02'),

    # === VILLAGER (Wanderer) ===
    'VILLAGER_SKIN': ColorRamp('#fff0e6', '#fcd5be', '#d99e80', '#9c664c', '#472a1c'),
    'VILLAGER_HAIR': ColorRamp('#854d27', '#5e3214', '#3d1c08', '#260f03', '#120601'),
    'VILLAGER_SHIRT': ColorRamp('#ffffff', '#f1f5f9', '#e2e8f0', '#94a3b8', '#475569'),
    'VILLAGER_PANTS': ColorRamp('#2dd4bf', '#0d9488', '#0f766e', '#115e59', '#042f2e'),
    'VILLAGER_BOOTS': ColorRamp('#784c28', '#543216', '#361e0b', '#211105', '#0e0601'),
    'STRAW_HAT': ColorRamp('#fef08a', '#eab308', '#ca8a04', '#854d0e', '#3a1f02'),
    'FRUIT_BASKET': ColorRamp('#d97706', '#b45309', '#78350f', '#451a03', '#1e0a00'),
}


def draw_drop_shadow(canvas, center_x=16, center_y=28, rx=6, ry=2, alpha=110):
    for y in range(center_y - ry, center_y + ry + 1):
        for x in range(center_x - rx, center_x + rx + 1):
            dx = (x - center_x) / float(rx)
            dy = (y - center_y) / float(ry)
            d2 = dx * dx + dy * dy
            if d2 <= 0.7:
                canvas.set_pixel(x, y, (14, 18, 24, alpha))
            elif d2 <= 1.0:
                canvas.set_pixel(x, y, (14, 18, 24, int(alpha * 0.6)))
            elif d2 <= 1.35 and (x + y) % 2 == 0:
                canvas.set_pixel(x, y, (14, 18, 24, int(alpha * 0.35)))


def flip_canvas_horizontally(canvas):
    flipped = PixelCanvas(canvas.width, canvas.height)
    flipped.buffer = np.fliplr(canvas.buffer)
    return flipped


# ==============================================================================
# 1. QUẢN THÚ KAELEN (NPC_PetMaster_Kaelen)
# ==============================================================================
def build_kaelen_front(walk_phase=0):
    body = PixelCanvas(32, 32)
    bob = 1 if walk_phase in (2, 4) else 0
    sway = -1 if walk_phase == 1 else (1 if walk_phase == 3 else 0)
    draw_drop_shadow(body, 16, 28, 6, 2, 110)

    # Boots
    boots = PALETTES['KAELEN_BOOTS']
    l_foot_y = 27 + (1 if walk_phase == 1 else 0) + bob
    r_foot_y = 27 + (1 if walk_phase == 3 else 0) + bob
    body.set_pixel(13, l_foot_y, boots.base)
    body.set_pixel(14, l_foot_y, boots.shadow)
    body.set_pixel(17, r_foot_y, boots.base)
    body.set_pixel(18, r_foot_y, boots.shadow)

    # Forest Green Tunic Skirt & Deer Leather Coat (Y=18..26 + bob)
    tunic = PALETTES['KAELEN_TUNIC']
    vest = PALETTES['KAELEN_VEST']

    for y in range(18 + bob, 27 + bob):
        rel_y = y - (18 + bob)
        w = 8 + int(rel_y * 0.5)
        min_x = 16 - w // 2 + (sway if rel_y > 4 else 0)
        max_x = min_x + w - 1
        for x in range(min_x, max_x + 1):
            if x in (15, 16) and y < 24 + bob:
                body.set_pixel(x, y, tunic.base if x == 15 else tunic.shadow) # Green inner tunic
            else:
                rel_x = x - min_x
                if rel_x <= 1:
                    body.set_pixel(x, y, vest.highlight)
                elif rel_x >= w - 2:
                    body.set_pixel(x, y, vest.shadow)
                else:
                    body.set_pixel(x, y, vest.base)

    # Belt with gold buckle at Y=19 + bob
    for x in range(13, 19):
        body.set_pixel(x, 19 + bob, vest.deep_shadow)
    body.set_pixel(15, 19 + bob, '#fde047')
    body.set_pixel(16, 19 + bob, '#ca8a04')

    # Fluffy White Fur Mantle / Boa on Shoulders (Y=14..18 + bob)
    fur = PALETTES['KAELEN_FUR']
    for y in range(14 + bob, 18 + bob):
        for x in range(11, 21):
            rel_x = x - 11
            if rel_x in (0, 9) and y in (14 + bob, 17 + bob):
                continue
            if y == 14 + bob or rel_x <= 2:
                body.set_pixel(x, y, fur.highlight)
            elif rel_x >= 7:
                body.set_pixel(x, y, fur.shadow)
            else:
                body.set_pixel(x, y, fur.base if (x + y) % 2 == 0 else fur.highlight)
    # Gold jewel clasp in center of fur
    body.set_pixel(15, 16 + bob, '#facc15')
    body.set_pixel(16, 16 + bob, '#eab308')

    # Right Arm (viewer's left X=9..11, Y=16..19 + bob)
    skin = PALETTES['KAELEN_SKIN']
    body.set_pixel(10, 16 + bob, tunic.base)
    body.set_pixel(10, 17 + bob, tunic.shadow)
    body.set_pixel(10, 18 + bob, skin.base)
    body.set_pixel(10, 19 + bob, skin.shadow)

    # Left Arm: Heavy Falconer's Training Gauntlet (viewer's right X=20..24, Y=16..19 + bob)
    glove = PALETTES['KAELEN_GLOVE']
    for y in range(16 + bob, 20 + bob):
        body.set_pixel(20, y, glove.highlight)
        body.set_pixel(21, y, glove.base)
        body.set_pixel(22, y, glove.shadow)
        body.set_pixel(23, y, glove.deep_shadow)
    # Glove cuff fur trim
    body.set_pixel(20, 16 + bob, fur.highlight)
    body.set_pixel(21, 16 + bob, fur.base)
    body.set_pixel(22, 16 + bob, fur.shadow)

    # Face & Neck (Y=9..13 + bob)
    body.set_pixel(15, 12 + bob, skin.highlight)
    body.set_pixel(16, 12 + bob, skin.base)
    body.set_pixel(15, 13 + bob, skin.base)
    body.set_pixel(16, 13 + bob, skin.shadow)

    for y in range(9 + bob, 12 + bob):
        for x in range(13, 19):
            body.set_pixel(x, y, skin.highlight if x <= 15 else skin.base)

    # Eyes & Cheerful expression
    body.set_pixel(14, 10 + bob, '#14532d') # Forest green eyes
    body.set_pixel(17, 10 + bob, '#14532d')
    body.set_pixel(15, 10 + bob, skin.highlight)
    body.set_pixel(16, 10 + bob, skin.base)
    body.set_pixel(13, 11 + bob, '#fda4af')
    body.set_pixel(18, 11 + bob, '#fb7185')

    # Brown Hair under hat
    hair = PALETTES['KAELEN_HAIR']
    body.set_pixel(13, 9 + bob, hair.base)
    body.set_pixel(18, 9 + bob, hair.shadow)
    body.set_pixel(12, 10 + bob, hair.base)
    body.set_pixel(19, 10 + bob, hair.shadow)

    # Beastmaster Hat with Fox/Wolf Animal Ears (Y=5..9 + bob)
    hat = PALETTES['KAELEN_HAT']
    # Hat brim (X=11..20, Y=8 + bob)
    for x in range(11, 21):
        body.set_pixel(x, 8 + bob, hat.highlight if x <= 14 else (hat.base if x <= 17 else hat.shadow))
    # Hat crown (Y=6..7 + bob)
    for y in range(6 + bob, 8 + bob):
        for x in range(13, 19):
            body.set_pixel(x, y, hat.highlight if x <= 15 else hat.shadow)

    # Animal Ears sticking up from hat (fox fur ears at X=13 and X=18, Y=5..7 + bob)
    # Left ear
    body.set_pixel(13, 5 + bob, '#d97706') # Tip
    body.set_pixel(12, 6 + bob, '#b45309')
    body.set_pixel(13, 6 + bob, '#fde0cb') # Inner fluff
    body.set_pixel(14, 6 + bob, '#d97706')
    # Right ear
    body.set_pixel(18, 5 + bob, '#d97706')
    body.set_pixel(17, 6 + bob, '#d97706')
    body.set_pixel(18, 6 + bob, '#fde0cb')
    body.set_pixel(19, 6 + bob, '#9a3412')

    body.apply_dark_outline(outline_color='#0c1a0e')
    return body


def build_kaelen_back(walk_phase=0):
    body = PixelCanvas(32, 32)
    bob = 1 if walk_phase in (2, 4) else 0
    sway = -1 if walk_phase == 1 else (1 if walk_phase == 3 else 0)
    draw_drop_shadow(body, 16, 28, 6, 2, 110)

    boots = PALETTES['KAELEN_BOOTS']
    body.set_pixel(13, 27 + bob, boots.base)
    body.set_pixel(14, 27 + bob, boots.shadow)
    body.set_pixel(17, 27 + bob, boots.base)
    body.set_pixel(18, 27 + bob, boots.shadow)

    vest = PALETTES['KAELEN_VEST']
    for y in range(18 + bob, 27 + bob):
        rel_y = y - (18 + bob)
        w = 8 + int(rel_y * 0.5)
        min_x = 16 - w // 2 + (sway if rel_y > 4 else 0)
        max_x = min_x + w - 1
        for x in range(min_x, max_x + 1):
            body.set_pixel(x, y, vest.base if x <= 16 else vest.shadow)

    # Fluffy Fur Mantle Back
    fur = PALETTES['KAELEN_FUR']
    for y in range(14 + bob, 18 + bob):
        for x in range(11, 21):
            body.set_pixel(x, y, fur.base if (x + y) % 2 == 0 else fur.shadow)

    # Hair on back
    hair = PALETTES['KAELEN_HAIR']
    for y in range(8 + bob, 14 + bob):
        for x in range(13, 19):
            body.set_pixel(x, y, hair.highlight if x <= 15 else hair.shadow)

    # Hat & Ears Back
    hat = PALETTES['KAELEN_HAT']
    for x in range(11, 21):
        body.set_pixel(x, 8 + bob, hat.base)
    for y in range(6 + bob, 8 + bob):
        for x in range(13, 19):
            body.set_pixel(x, y, hat.base if x <= 16 else hat.shadow)

    # Ears Back
    body.set_pixel(13, 5 + bob, '#b45309')
    body.set_pixel(18, 5 + bob, '#b45309')
    body.set_pixel(13, 6 + bob, '#9a3412')
    body.set_pixel(18, 6 + bob, '#9a3412')

    body.apply_dark_outline(outline_color='#0c1a0e')
    return body


def build_kaelen_side(walk_phase=0):
    body = PixelCanvas(32, 32)
    bob = 1 if walk_phase in (2, 4) else 0
    stride = 2 if walk_phase == 1 else (-2 if walk_phase == 3 else 0)
    draw_drop_shadow(body, 16, 28, 5, 2, 110)

    boots = PALETTES['KAELEN_BOOTS']
    body.set_pixel(17 + stride, 27 + bob, boots.base)
    body.set_pixel(14 - stride, 27 + bob, boots.shadow)

    vest = PALETTES['KAELEN_VEST']
    for y in range(18 + bob, 27 + bob):
        body.set_pixel(17, y, vest.highlight)
        body.set_pixel(16, y, vest.base)
        body.set_pixel(15, y, vest.base)
        body.set_pixel(14, y, vest.shadow)

    fur = PALETTES['KAELEN_FUR']
    for y in range(14 + bob, 18 + bob):
        body.set_pixel(17, y, fur.highlight)
        body.set_pixel(16, y, fur.highlight)
        body.set_pixel(15, y, fur.base)
        body.set_pixel(14, y, fur.shadow)

    # Gauntlet held forward
    glove = PALETTES['KAELEN_GLOVE']
    gx = 18 + stride // 2
    for y in range(16 + bob, 20 + bob):
        body.set_pixel(gx, y, glove.highlight)
        body.set_pixel(gx + 1, y, glove.base)
        body.set_pixel(gx + 2, y, glove.shadow)

    skin = PALETTES['KAELEN_SKIN']
    hair = PALETTES['KAELEN_HAIR']
    body.set_pixel(16, 12 + bob, skin.base)
    body.set_pixel(16, 13 + bob, skin.shadow)

    for y in range(9 + bob, 12 + bob):
        body.set_pixel(14, y, hair.base)
        body.set_pixel(15, y, skin.highlight)
        body.set_pixel(16, y, skin.highlight)
        body.set_pixel(17, y, skin.base)
    body.set_pixel(17, 10 + bob, '#14532d')
    body.set_pixel(18, 10 + bob, skin.highlight)

    hat = PALETTES['KAELEN_HAT']
    for x in range(12, 19):
        body.set_pixel(x, 8 + bob, hat.highlight if x <= 15 else hat.base)
    for y in range(6 + bob, 8 + bob):
        for x in range(13, 17):
            body.set_pixel(x, y, hat.highlight if x <= 14 else hat.base)
    body.set_pixel(15, 5 + bob, '#d97706')
    body.set_pixel(15, 6 + bob, '#fde0cb')

    body.apply_dark_outline(outline_color='#0c1a0e')
    return body


# ==============================================================================
# 2. CHIM ƯNG (Pet_Falcon)
# ==============================================================================
def build_falcon_perched(phase=0):
    canvas = PixelCanvas(32, 32)
    bob = 1 if phase == 1 else 0

    fw = PALETTES['FALCON_WHITE']
    fb = PALETTES['FALCON_BROWN']
    beak = PALETTES['FALCON_BEAK']

    # Perched falcon sits compactly at X=21..24, Y=11..18 (matching gauntlet socket)
    # Head & Beak at Y=10..12 + bob
    canvas.set_pixel(22, 10 + bob, fw.highlight)
    canvas.set_pixel(23, 10 + bob, fw.base)
    canvas.set_pixel(24, 10 + bob, beak.highlight) # Golden hooked beak
    canvas.set_pixel(24, 11 + bob, beak.base)
    canvas.set_pixel(22, 11 + bob, fw.highlight)
    canvas.set_pixel(23, 11 + bob, '#18181b') # Hawk eye

    # White breast & neck
    for y in range(12 + bob, 16 + bob):
        canvas.set_pixel(22, y, fw.highlight)
        canvas.set_pixel(23, y, fw.base)

    # Folded Brown Wings
    for y in range(12 + bob, 17 + bob):
        canvas.set_pixel(20, y, fb.highlight if y < 15 + bob else fb.base)
        canvas.set_pixel(21, y, fb.base if y < 15 + bob else fb.shadow)

    # Tail feathers (Y=16..18 + bob)
    canvas.set_pixel(20, 17 + bob, fb.shadow)
    canvas.set_pixel(21, 17 + bob, fb.deep_shadow)
    canvas.set_pixel(20, 18 + bob, fb.deep_shadow)

    # Talons grasping gauntlet at Y=16 + bob
    canvas.set_pixel(22, 16 + bob, beak.highlight)
    canvas.set_pixel(23, 16 + bob, beak.base)

    canvas.apply_dark_outline(outline_color='#1a0b02')
    return canvas


def build_falcon_flying(flight_phase=0):
    canvas = PixelCanvas(32, 32)
    fw = PALETTES['FALCON_WHITE']
    fb = PALETTES['FALCON_BROWN']
    beak = PALETTES['FALCON_BEAK']

    # 4 Flight Frames: 0=Wings Up, 1=Glide Flat, 2=Wings Down, 3=Upstroke
    cx, cy = 16, 14

    # Hawk Body & Head
    canvas.set_pixel(cx, cy, fw.highlight)
    canvas.set_pixel(cx + 1, cy, fw.base)
    canvas.set_pixel(cx + 2, cy - 1, beak.highlight)
    canvas.set_pixel(cx + 1, cy - 1, fw.highlight)
    canvas.set_pixel(cx + 1, cy, '#18181b') # Eye
    canvas.set_pixel(cx - 1, cy + 1, fb.base) # Tail
    canvas.set_pixel(cx - 2, cy + 2, fb.shadow)

    if flight_phase == 0:
        # Wings High Up
        for i in range(1, 6):
            # Left wing angled up-left
            canvas.set_pixel(cx - i, cy - i, fb.highlight)
            canvas.set_pixel(cx - i, cy - i + 1, fb.base)
            # Right wing angled up-right
            canvas.set_pixel(cx + i, cy - i, fb.highlight)
            canvas.set_pixel(cx + i, cy - i + 1, fb.base)
    elif flight_phase == 1:
        # Glide Flat (Soaring wingspan)
        for i in range(1, 7):
            canvas.set_pixel(cx - i, cy, fb.highlight)
            canvas.set_pixel(cx - i, cy + 1, fb.base)
            canvas.set_pixel(cx + i, cy, fb.highlight)
            canvas.set_pixel(cx + i, cy + 1, fb.base)
    elif flight_phase == 2:
        # Wings Powered Down
        for i in range(1, 6):
            canvas.set_pixel(cx - i, cy + i - 1, fb.base)
            canvas.set_pixel(cx - i, cy + i, fb.shadow)
            canvas.set_pixel(cx + i, cy + i - 1, fb.base)
            canvas.set_pixel(cx + i, cy + i, fb.shadow)
    elif flight_phase == 3:
        # Upstroke Transition
        for i in range(1, 5):
            canvas.set_pixel(cx - i, cy - 1, fb.highlight)
            canvas.set_pixel(cx - i, cy, fb.base)
            canvas.set_pixel(cx + i, cy - 1, fb.highlight)
            canvas.set_pixel(cx + i, cy, fb.base)

    canvas.apply_dark_outline(outline_color='#1a0b02')
    return canvas


# ==============================================================================
# 3. CƯ DÂN OAKHAVEN (NPC_Villager_Wanderer - Full Hair & Detachable Props)
# ==============================================================================
def build_villager_front(walk_phase=0):
    body = PixelCanvas(32, 32)
    hat = PixelCanvas(32, 32)
    basket = PixelCanvas(32, 32)
    comp = PixelCanvas(32, 32)

    bob = 1 if walk_phase in (2, 4) else 0
    sway = -1 if walk_phase == 1 else (1 if walk_phase == 3 else 0)
    draw_drop_shadow(body, 16, 28, 5, 2, 110)

    # Boots
    boots = PALETTES['VILLAGER_BOOTS']
    l_foot_y = 27 + (1 if walk_phase == 1 else 0) + bob
    r_foot_y = 27 + (1 if walk_phase == 3 else 0) + bob
    body.set_pixel(13, l_foot_y, boots.base)
    body.set_pixel(14, l_foot_y, boots.shadow)
    body.set_pixel(17, r_foot_y, boots.base)
    body.set_pixel(18, r_foot_y, boots.shadow)

    # Teal/Green-Blue Trousers (Y=20..26 + bob)
    pants = PALETTES['VILLAGER_PANTS']
    for y in range(20 + bob, 27 + bob):
        rel_y = y - (20 + bob)
        w = 8 + int(rel_y * 0.45)
        min_x = 16 - w // 2 + (sway if rel_y > 4 else 0)
        max_x = min_x + w - 1
        for x in range(min_x, max_x + 1):
            body.set_pixel(x, y, pants.base if x <= 16 else pants.shadow)

    # Cream Linen Shirt & Suspenders (Y=14..20 + bob)
    shirt = PALETTES['VILLAGER_SHIRT']
    skin = PALETTES['VILLAGER_SKIN']
    hair = PALETTES['VILLAGER_HAIR']

    # Neck connecting chin (Y=12..14 + bob)
    body.set_pixel(15, 12 + bob, skin.highlight)
    body.set_pixel(16, 12 + bob, skin.base)
    body.set_pixel(15, 13 + bob, skin.base)
    body.set_pixel(16, 13 + bob, skin.shadow)

    for y in range(14 + bob, 21 + bob):
        for x in range(12, 20):
            if x in (13, 17):
                body.set_pixel(x, y, '#78350f') # Brown leather suspenders
            else:
                body.set_pixel(x, y, shirt.highlight if x <= 15 else shirt.base)

    # Arms & Hands (ready to hold basket in front or rest naturally)
    body.set_pixel(11, 15 + bob, shirt.highlight)
    body.set_pixel(11, 16 + bob, shirt.base)
    body.set_pixel(11, 17 + bob, skin.highlight)
    body.set_pixel(12, 17 + bob, skin.base)

    body.set_pixel(20, 15 + bob, shirt.base)
    body.set_pixel(20, 16 + bob, shirt.shadow)
    body.set_pixel(20, 17 + bob, skin.base)
    body.set_pixel(19, 17 + bob, skin.shadow)

    # Face & FULL BROWN HAIR (Artisan Tousled Locks - completely drawn when hatless)
    for y in range(9 + bob, 12 + bob):
        for x in range(13, 19):
            body.set_pixel(x, y, skin.highlight if x <= 15 else skin.base)

    # Friendly blue/dark eyes & smile
    body.set_pixel(14, 10 + bob, '#0284c7')
    body.set_pixel(17, 10 + bob, '#0284c7')
    body.set_pixel(15, 10 + bob, skin.highlight)
    body.set_pixel(16, 10 + bob, skin.base)
    body.set_pixel(15, 12 + bob, '#b91c1c') # Subtle friendly mouth

    # Full Tousled Brown Hair Crown & Locks (Y=6..10 + bob)
    # Crown top
    for x in range(14, 18):
        body.set_pixel(x, 6 + bob, hair.highlight if x <= 15 else hair.base)
    for x in range(13, 19):
        body.set_pixel(x, 7 + bob, hair.highlight if x <= 14 else (hair.base if x <= 16 else hair.shadow))
    for x in range(12, 20):
        body.set_pixel(x, 8 + bob, hair.highlight if x <= 14 else (hair.base if x <= 16 else hair.shadow))

    # Bangs & Side Locks
    body.set_pixel(13, 9 + bob, hair.highlight)
    body.set_pixel(14, 9 + bob, hair.base)
    body.set_pixel(17, 9 + bob, hair.base)
    body.set_pixel(18, 9 + bob, hair.shadow)
    body.set_pixel(12, 10 + bob, hair.base)
    body.set_pixel(19, 10 + bob, hair.shadow)
    body.set_pixel(12, 11 + bob, hair.shadow)
    body.set_pixel(19, 11 + bob, hair.deep_shadow)

    body.apply_dark_outline(outline_color='#120601')

    # Prop 1: Straw Hat (Worn on head at X=10..21, Y=5..9 + bob)
    shat = PALETTES['STRAW_HAT']
    # Hat Brim (wide oval)
    for x in range(10, 22):
        hat.set_pixel(x, 7 + bob, shat.highlight if x <= 14 else (shat.base if x <= 17 else shat.shadow))
    # Hat Crown
    for y in range(4 + bob, 7 + bob):
        for x in range(13, 19):
            hat.set_pixel(x, y, shat.highlight if x <= 15 else shat.shadow)
    # Blue Ribbon Band at Y=6 + bob
    for x in range(13, 19):
        hat.set_pixel(x, 6 + bob, '#2563eb' if x <= 15 else '#1d4ed8')

    hat.apply_dark_outline(outline_color='#3a1f02')

    # Prop 2: Fruit Basket (Held in front of chest/waist at X=13..19, Y=16..20 + bob)
    bask = PALETTES['FRUIT_BASKET']
    bx, by = 13, 17 + bob

    # Woven basket body
    for y in range(by, by + 4):
        for x in range(bx, bx + 7):
            basket.set_pixel(x, y, bask.highlight if (x + y) % 2 == 0 else bask.shadow)
    # Fruits overflowing on top
    basket.set_pixel(bx + 1, by - 1, '#ef4444') # Red apple
    basket.set_pixel(bx + 2, by - 1, '#f97316') # Orange
    basket.set_pixel(bx + 3, by - 1, '#a855f7') # Grapes
    basket.set_pixel(bx + 4, by - 1, '#22c55e') # Green pear
    basket.set_pixel(bx + 5, by - 1, '#ef4444') # Red apple

    basket.apply_dark_outline(outline_color='#1e0a00')

    # Composite: Body -> Basket -> Hat
    comp.composite(body)
    comp.composite(basket)
    # Fingers holding basket
    comp.set_pixel(bx, by, skin.highlight)
    comp.set_pixel(bx + 6, by, skin.shadow)
    comp.composite(hat)

    return body, hat, basket, comp


def build_villager_back(walk_phase=0):
    body = PixelCanvas(32, 32)
    hat = PixelCanvas(32, 32)
    basket = PixelCanvas(32, 32)
    comp = PixelCanvas(32, 32)

    bob = 1 if walk_phase in (2, 4) else 0
    sway = -1 if walk_phase == 1 else (1 if walk_phase == 3 else 0)
    draw_drop_shadow(body, 16, 28, 5, 2, 110)

    boots = PALETTES['VILLAGER_BOOTS']
    body.set_pixel(13, 27 + bob, boots.base)
    body.set_pixel(14, 27 + bob, boots.shadow)
    body.set_pixel(17, 27 + bob, boots.base)
    body.set_pixel(18, 27 + bob, boots.shadow)

    pants = PALETTES['VILLAGER_PANTS']
    for y in range(20 + bob, 27 + bob):
        rel_y = y - (20 + bob)
        w = 8 + int(rel_y * 0.45)
        min_x = 16 - w // 2 + (sway if rel_y > 4 else 0)
        max_x = min_x + w - 1
        for x in range(min_x, max_x + 1):
            body.set_pixel(x, y, pants.base if x <= 16 else pants.shadow)

    shirt = PALETTES['VILLAGER_SHIRT']
    for y in range(14 + bob, 21 + bob):
        for x in range(12, 20):
            if x in (13, 17):
                body.set_pixel(x, y, '#78350f') # Suspenders crossed
            else:
                body.set_pixel(x, y, shirt.base if x <= 16 else shirt.shadow)

    # Full Hair Back
    hair = PALETTES['VILLAGER_HAIR']
    for y in range(6 + bob, 14 + bob):
        w = 6 if y < 9 + bob else 8
        min_x = 16 - w // 2
        for x in range(min_x, min_x + w):
            body.set_pixel(x, y, hair.highlight if x <= 15 else hair.shadow)

    body.apply_dark_outline(outline_color='#120601')

    # Hat Back
    shat = PALETTES['STRAW_HAT']
    for x in range(10, 22):
        hat.set_pixel(x, 7 + bob, shat.base if x <= 16 else shat.shadow)
    for y in range(4 + bob, 7 + bob):
        for x in range(13, 19):
            hat.set_pixel(x, y, shat.base if x <= 16 else shat.shadow)
    for x in range(13, 19):
        hat.set_pixel(x, 6 + bob, '#1d4ed8')
    hat.apply_dark_outline(outline_color='#3a1f02')

    comp.composite(body)
    comp.composite(hat)
    return body, hat, basket, comp


def build_villager_side(walk_phase=0):
    body = PixelCanvas(32, 32)
    hat = PixelCanvas(32, 32)
    basket = PixelCanvas(32, 32)
    comp = PixelCanvas(32, 32)

    bob = 1 if walk_phase in (2, 4) else 0
    stride = 2 if walk_phase == 1 else (-2 if walk_phase == 3 else 0)
    draw_drop_shadow(body, 16, 28, 5, 2, 110)

    boots = PALETTES['VILLAGER_BOOTS']
    body.set_pixel(17 + stride, 27 + bob, boots.base)
    body.set_pixel(14 - stride, 27 + bob, boots.shadow)

    pants = PALETTES['VILLAGER_PANTS']
    for y in range(20 + bob, 27 + bob):
        body.set_pixel(17, y, pants.highlight)
        body.set_pixel(16, y, pants.base)
        body.set_pixel(15, y, pants.base)
        body.set_pixel(14, y, pants.shadow)

    shirt = PALETTES['VILLAGER_SHIRT']
    for y in range(14 + bob, 21 + bob):
        body.set_pixel(17, y, shirt.highlight)
        body.set_pixel(16, y, '#78350f') # Suspender
        body.set_pixel(15, y, shirt.base)
        body.set_pixel(14, y, shirt.shadow)

    skin = PALETTES['VILLAGER_SKIN']
    hair = PALETTES['VILLAGER_HAIR']
    body.set_pixel(16, 12 + bob, skin.base)
    body.set_pixel(16, 13 + bob, skin.shadow)

    for y in range(9 + bob, 12 + bob):
        body.set_pixel(14, y, hair.base)
        body.set_pixel(15, y, skin.highlight)
        body.set_pixel(16, y, skin.highlight)
        body.set_pixel(17, y, skin.base)
    body.set_pixel(17, 10 + bob, '#0284c7')
    body.set_pixel(18, 10 + bob, skin.highlight)

    # Hair profile
    for x in range(13, 17):
        body.set_pixel(x, 6 + bob, hair.highlight)
    for y in range(7 + bob, 13 + bob):
        body.set_pixel(13, y, hair.base)
        body.set_pixel(14, y, hair.shadow)

    body.apply_dark_outline(outline_color='#120601')

    # Hat Side
    shat = PALETTES['STRAW_HAT']
    for x in range(11, 21):
        hat.set_pixel(x, 7 + bob, shat.highlight if x <= 15 else shat.base)
    for y in range(4 + bob, 7 + bob):
        for x in range(13, 18):
            hat.set_pixel(x, y, shat.highlight if x <= 15 else shat.base)
    hat.apply_dark_outline(outline_color='#3a1f02')

    # Basket Side (held forward)
    bask = PALETTES['FRUIT_BASKET']
    bx = 18 + stride // 2
    by = 17 + bob
    for y in range(by, by + 4):
        for x in range(bx, bx + 5):
            basket.set_pixel(x, y, bask.highlight if x == bx else bask.base)
    basket.set_pixel(bx + 1, by - 1, '#ef4444')
    basket.set_pixel(bx + 2, by - 1, '#22c55e')
    basket.apply_dark_outline(outline_color='#1e0a00')

    comp.composite(body)
    comp.composite(basket)
    comp.composite(hat)
    return body, hat, basket, comp


def build_world_props():
    """Generates ground world item sprites for Straw Hat & Fruit Basket."""
    # World Straw Hat (Placed on ground)
    w_hat = PixelCanvas(32, 32)
    draw_drop_shadow(w_hat, 16, 22, 6, 2, 100)
    shat = PALETTES['STRAW_HAT']
    # Oval brim resting on grass
    for y in range(18, 23):
        w = 12 if y in (19, 20, 21) else 8
        min_x = 16 - w // 2
        for x in range(min_x, min_x + w):
            w_hat.set_pixel(x, y, shat.highlight if y < 20 else shat.base)
    # Crown in center
    for y in range(16, 19):
        for x in range(14, 19):
            w_hat.set_pixel(x, y, shat.highlight if x <= 15 else shat.shadow)
    for x in range(14, 19):
        w_hat.set_pixel(x, 18, '#2563eb')
    w_hat.apply_dark_outline(outline_color='#3a1f02')

    # World Fruit Basket (Placed on ground)
    w_bask = PixelCanvas(32, 32)
    draw_drop_shadow(w_bask, 16, 24, 6, 2, 110)
    bask = PALETTES['FRUIT_BASKET']
    for y in range(19, 25):
        w = 10 if y < 23 else 8
        min_x = 16 - w // 2
        for x in range(min_x, min_x + w):
            w_bask.set_pixel(x, y, bask.highlight if (x + y) % 2 == 0 else bask.shadow)
    # Fruits on top
    w_bask.set_pixel(13, 18, '#ef4444')
    w_bask.set_pixel(14, 18, '#f97316')
    w_bask.set_pixel(15, 17, '#ef4444')
    w_bask.set_pixel(16, 17, '#a855f7')
    w_bask.set_pixel(17, 18, '#22c55e')
    w_bask.set_pixel(18, 18, '#ef4444')
    w_bask.apply_dark_outline(outline_color='#1e0a00')

    return w_hat, w_bask


# ==============================================================================
# MAIN BATCH EXPORT
# ==============================================================================
def export_all():
    base_modular = 'Assets/Sprites/NPCs/Modular'
    base_monolithic = 'Assets/Sprites/NPCs'

    print("=== EXPORTING KAELEN, FALCON & VILLAGER ===")

    # 1. KAELEN
    k_dir = f'{base_modular}/Kaelen'
    os.makedirs(k_dir, exist_ok=True)
    kb = build_kaelen_front(0)
    kb.save(f'{k_dir}/npc_kaelen_body_idle_front.png')
    kb.save(f'{base_monolithic}/npc_kaelen_idle_front.png')
    for i in range(1, 5):
        wb = build_kaelen_front(i)
        wb.save(f'{k_dir}/npc_kaelen_body_walk_front_{i}.png')
        wb.save(f'{base_monolithic}/npc_kaelen_walk_front_{i}.png')

    kbb = build_kaelen_back(0)
    kbb.save(f'{k_dir}/npc_kaelen_body_idle_back.png')
    kbb.save(f'{base_monolithic}/npc_kaelen_idle_back.png')
    for i in range(1, 5):
        wb = build_kaelen_back(i)
        wb.save(f'{k_dir}/npc_kaelen_body_walk_back_{i}.png')
        wb.save(f'{base_monolithic}/npc_kaelen_walk_back_{i}.png')

    kbs = build_kaelen_side(0)
    kbs.save(f'{k_dir}/npc_kaelen_body_idle_side.png')
    kbs.save(f'{base_monolithic}/npc_kaelen_idle_side.png')
    for i in range(1, 5):
        wb = build_kaelen_side(i)
        wb.save(f'{k_dir}/npc_kaelen_body_walk_side_{i}.png')
        wb.save(f'{base_monolithic}/npc_kaelen_walk_side_{i}.png')

    flip_canvas_horizontally(kbs).save(f'{k_dir}/npc_kaelen_body_idle_left.png')
    flip_canvas_horizontally(kbs).save(f'{base_monolithic}/npc_kaelen_idle_left.png')
    for i in range(1, 5):
        wb = build_kaelen_side(i)
        flip_canvas_horizontally(wb).save(f'{k_dir}/npc_kaelen_body_walk_left_{i}.png')
        flip_canvas_horizontally(wb).save(f'{base_monolithic}/npc_kaelen_walk_left_{i}.png')

    # 2. FALCON
    f_dir = f'{base_modular}/Falcon'
    os.makedirs(f_dir, exist_ok=True)
    # Perched frames
    build_falcon_perched(0).save(f'{f_dir}/falcon_idle_perched_1.png')
    build_falcon_perched(1).save(f'{f_dir}/falcon_idle_perched_2.png')
    # Flight frames
    for i in range(4):
        build_falcon_flying(i).save(f'{f_dir}/falcon_fly_loop_{i+1}.png')

    # 3. VILLAGER
    v_dir = f'{base_modular}/Villager'
    os.makedirs(v_dir, exist_ok=True)
    # Front
    vb, vh, vk, vc = build_villager_front(0)
    vb.save(f'{v_dir}/npc_villager_body_idle_front.png')
    vh.save(f'{v_dir}/prop_villager_hat_front.png')
    vk.save(f'{v_dir}/prop_villager_basket_front.png')
    vc.save(f'{base_monolithic}/npc_villager_idle_front.png')
    for i in range(1, 5):
        wb, wh, wk, wc = build_villager_front(i)
        wb.save(f'{v_dir}/npc_villager_body_walk_front_{i}.png')
        wc.save(f'{base_monolithic}/npc_villager_walk_front_{i}.png')

    # Back
    vbb, vbh, vbk, vbc = build_villager_back(0)
    vbb.save(f'{v_dir}/npc_villager_body_idle_back.png')
    vbh.save(f'{v_dir}/prop_villager_hat_back.png')
    vbk.save(f'{v_dir}/prop_villager_basket_back.png')
    vbc.save(f'{base_monolithic}/npc_villager_idle_back.png')
    for i in range(1, 5):
        wb, _, _, wc = build_villager_back(i)
        wb.save(f'{v_dir}/npc_villager_body_walk_back_{i}.png')
        wc.save(f'{base_monolithic}/npc_villager_walk_back_{i}.png')

    # Side (Right)
    vbs, vsh, vsk, vsc = build_villager_side(0)
    vbs.save(f'{v_dir}/npc_villager_body_idle_side.png')
    vsh.save(f'{v_dir}/prop_villager_hat_side.png')
    vsk.save(f'{v_dir}/prop_villager_basket_side.png')
    vsc.save(f'{base_monolithic}/npc_villager_idle_side.png')
    for i in range(1, 5):
        wb, _, _, wc = build_villager_side(i)
        wb.save(f'{v_dir}/npc_villager_body_walk_side_{i}.png')
        wc.save(f'{base_monolithic}/npc_villager_walk_side_{i}.png')

    # Side (Left)
    flip_canvas_horizontally(vbs).save(f'{v_dir}/npc_villager_body_idle_left.png')
    flip_canvas_horizontally(vsh).save(f'{v_dir}/prop_villager_hat_left.png')
    flip_canvas_horizontally(vsk).save(f'{v_dir}/prop_villager_basket_left.png')
    flip_canvas_horizontally(vsc).save(f'{base_monolithic}/npc_villager_idle_left.png')
    for i in range(1, 5):
        wb, _, _, wc = build_villager_side(i)
        flip_canvas_horizontally(wb).save(f'{v_dir}/npc_villager_body_walk_left_{i}.png')
        flip_canvas_horizontally(wc).save(f'{base_monolithic}/npc_villager_walk_left_{i}.png')

    # World Props
    w_hat, w_bask = build_world_props()
    w_hat.save(f'{v_dir}/prop_villager_hat_world.png')
    w_bask.save(f'{v_dir}/prop_villager_basket_world.png')

    print("KAELEN, FALCON & VILLAGER GENERATED WITH ARTISAN PERFECTION!")


if __name__ == '__main__':
    export_all()
