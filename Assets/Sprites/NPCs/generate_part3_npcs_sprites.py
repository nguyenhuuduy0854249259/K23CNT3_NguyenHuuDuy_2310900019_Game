"""
Master Pixel Art Generator for Part 3/3 Modular NPCs & Interactive Props
------------------------------------------------------------------------
1. Quản Thú Kaelen (NPC_PetMaster_Kaelen.prefab) + Thực thể Chim Ưng (Pet_Falcon)
   - Nhân vật Kaelen: Đội nón da thú có 2 tai lông nhỏ, áo gilê da hươu cổ lông trắng,
     găng tay da bảo hộ trên tay trái (PerchPoint), đeo túi thức ăn bên hông (Prop_FeedPouch).
   - Thực thể Chim Ưng (Pet_Falcon): Đậu yên (Idle_Perched 2 frames) và Vỗ cánh bay (Fly_Loop 4 frames).
2. Cư Dân Oakhaven (NPC_Villager_Wanderer.prefab) + Cơ chế Đặt/Nhặt Đạo Cụ
   - Nhân vật gốc: Áo vải thô màu be, quần xanh, đầu vẽ đầy đủ mái tóc nâu hoàn chỉnh (không bị hói khi tháo mũ).
   - Hỗ trợ 2 bộ Animation 4 hướng:
     + Bộ đi bộ tay không (Unarmed Walk/Idle)
     + Bộ đi bộ hai tay bê giỏ (Carrying Walk/Idle)
   - Tách rời 2 Child Objects:
     + Mũ rơm thắt ruy băng đỏ (Prop_StrawHat): 4 hướng + bản đặt dưới đất (World Space)
     + Giỏ hoa quả tươi (Prop_FruitBasket): 4 hướng + bản đặt dưới đất (World Space)

Kế thừa 100% PixelArtShadingHelper (cùng giải phẫu Chibi 32x32 với Phần 1 & Phần 2):
- Đầu to Y=9..20 (mũ/tóc Y=8..16, mặt Y=17..20, mắt 2px dọc tại X=13, 18 với điểm sáng trắng #F7EFEB tại Y=19)
- Thân Y=21..25, 2 tay tách nhẹ ở X=10..11 và X=20..21 có khe nách tối 1px ở X=12, 19
- 2 chân tách rõ ở X=11..15 và X=16..20 (Y=26..29) + bóng mờ DropShadow dưới chân
"""

import os
import sys
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

from PIL import Image
from pixel_art_shading_helper import (
    PixelArtShadingHelper,
    ColorRamp4,
    hex_to_rgba,
    STANDARD_PPU,
)

BASE_MODULAR_DIR = "Assets/Sprites/NPCs/Modular"
FLAT_NPCS_DIR = "Assets/Sprites/NPCs"

# ==============================================================================
# BẢNG MÀU 4 SẮC ĐỘ (4-SHADE COLOR RAMPS)
# ==============================================================================

# --- 1. QUẢN THÚ KAELEN ---
KAELEN_SKIN = ColorRamp4("#36140E", "#8F4C23", "#D48046", "#F5AC78")
KAELEN_HAIR = ColorRamp4("#1F1008", "#422413", "#6E3F23", "#9E603B")
KAELEN_PELT_HAT = ColorRamp4("#241208", "#5E3219", "#94542E", "#C97D50")
KAELEN_EAR_INNER = hex_to_rgba("#E8A598")
KAELEN_FUR_COLLAR = ColorRamp4("#263240", "#64748B", "#CBD5E1", "#FFFFFF")
KAELEN_VEST = ColorRamp4("#2B1608", "#6B3A18", "#A35C2B", "#D4884E")
KAELEN_TUNIC = ColorRamp4("#0E291B", "#1E5236", "#348257", "#59B380")
KAELEN_GAUNTLET = ColorRamp4("#1F0E05", "#4D2510", "#7D401D", "#AD6234")
KAELEN_BOOTS = ColorRamp4("#1A0E07", "#382010", "#59351C", "#7D4D2B")
KAELEN_POUCH = ColorRamp4("#291306", "#633212", "#9C5221", "#CF793C")
GOLD_TRIM = ColorRamp4("#361E06", "#7A490F", "#C98A24", "#FCE258")

# --- 2. CHIM ƯNG (PET_FALCON) ---
FALCON_WING = ColorRamp4("#1C0D06", "#4A2511", "#804622", "#B86F3E")
FALCON_BREAST = ColorRamp4("#3B2313", "#8C6239", "#D9B382", "#FFF5E4")
FALCON_BEAK = ColorRamp4("#361E06", "#8F580A", "#EAB308", "#FEF08A")
FALCON_TAIL = ColorRamp4("#140904", "#361B0C", "#5C3118", "#8A4F2B")

# --- 3. CƯ DÂN OAKHAVEN (VILLAGER WANDERER) ---
VILLAGER_SKIN = ColorRamp4("#36140E", "#9E6347", "#DB9E81", "#FFE0D1")
VILLAGER_HAIR = ColorRamp4("#211108", "#4D2612", "#804320", "#B56B43")
VILLAGER_SHIRT = ColorRamp4("#2B2118", "#735D46", "#B89B7A", "#E6CDB0")
VILLAGER_PANTS = ColorRamp4("#0F1D2E", "#1E3A5F", "#345F94", "#5B89C2")
VILLAGER_BOOTS = ColorRamp4("#1C0F08", "#3D2212", "#633920", "#8A5434")

# Đạo cụ Cư Dân: Mũ rơm (Prop_StrawHat) & Giỏ hoa quả (Prop_FruitBasket)
STRAW_HAT = ColorRamp4("#3B2506", "#8F5E14", "#DEC949", "#FFF394")
HAT_RIBBON = ColorRamp4("#38060B", "#7F111B", "#C81E2B", "#FF5A67")
WICKER_BASKET = ColorRamp4("#2B1606", "#663814", "#A66329", "#DBA25E")
FRUIT_APPLE = ColorRamp4("#450A0A", "#991B1B", "#EF4444", "#FCA5A5")
FRUIT_ORANGE = ColorRamp4("#431407", "#9A3412", "#F97316", "#FDBA74")
FRUIT_GRAPE = ColorRamp4("#2E1065", "#5B21B6", "#9333EA", "#D8B4FE")
FRUIT_LEAF = ColorRamp4("#052E16", "#166534", "#22C55E", "#86EFAC")

EYE_PUPIL = hex_to_rgba("#361313")
EYE_GLINT = hex_to_rgba("#F7EFEB")


def compose_layers(base_img: Image.Image, overlays_with_front_flag: list) -> Image.Image:
    out = PixelArtShadingHelper.create_canvas(32, 32)
    for prop_img, is_front in overlays_with_front_flag:
        if not is_front and prop_img is not None:
            out.alpha_composite(prop_img)
    out.alpha_composite(base_img)
    for prop_img, is_front in overlays_with_front_flag:
        if is_front and prop_img is not None:
            out.alpha_composite(prop_img)
    return out


# ==============================================================================
# 1. QUẢN THÚ KAELEN (NPC_PetMaster_Kaelen)
#    - Đội nón da thú có 2 tai lông nhỏ ở đỉnh đầu
#    - Áo gilê da hươu cổ lông trắng
#    - Tay trái đeo găng tay da bảo hộ cho Chim Ưng đậu
#    - Đeo túi thức ăn bên hông (Prop_FeedPouch)
# ==============================================================================

def draw_kaelen_front_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots (X=11..15 and X=16..20, Y=26..29)
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, KAELEN_BOOTS.outline)
        put(img, 12, y, KAELEN_BOOTS.outline if y == 29 - leg_l_lift else KAELEN_BOOTS.shadow)
        put(img, 13, y, KAELEN_BOOTS.outline if y == 29 - leg_l_lift else KAELEN_BOOTS.base)
        put(img, 14, y, KAELEN_BOOTS.outline if y == 29 - leg_l_lift else KAELEN_BOOTS.base)
        put(img, 15, y, KAELEN_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, KAELEN_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, KAELEN_BOOTS.outline if y == 29 - leg_r_lift else KAELEN_BOOTS.base)
        put(img, 18, y, KAELEN_BOOTS.outline if y == 29 - leg_r_lift else KAELEN_BOOTS.base)
        put(img, 19, y, KAELEN_BOOTS.outline if y == 29 - leg_r_lift else KAELEN_BOOTS.shadow)
        put(img, 20, y, KAELEN_BOOTS.outline)

    # Áo gilê da hươu cổ lông trắng & áo xanh rêu bên trong (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, KAELEN_VEST.outline)
            elif y_rel == 21:
                # Cổ áo lông thú trắng dày ấm áp quanh vai và ngực
                put(img, x, y, KAELEN_FUR_COLLAR.highlight if x in (13, 14, 17, 18) else KAELEN_FUR_COLLAR.base)
            elif y_rel == 22 and x in (13, 18):
                put(img, x, y, KAELEN_FUR_COLLAR.base)
            elif y_rel == 24:
                # Đai lưng thợ săn & khóa đồng
                put(img, x, y, GOLD_TRIM.highlight if x in (15, 16) else KAELEN_GAUNTLET.shadow)
            elif x in (15, 16):
                # Áo xanh rêu rừng lộ ở giữa gilê
                put(img, x, y, KAELEN_TUNIC.highlight if x == 15 else KAELEN_TUNIC.base)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, KAELEN_VEST.highlight if checker else KAELEN_VEST.base)

    # Tay phải (viewer's left: X=10..11) + khe nách 1px ở X=12
    yl = 21 + bob
    put(img, 11, yl, KAELEN_TUNIC.shadow)
    put(img, 10, yl + 1, KAELEN_TUNIC.outline)
    put(img, 11, yl + 1, KAELEN_TUNIC.base)
    put(img, 10, yl + 2, KAELEN_SKIN.highlight)
    put(img, 11, yl + 2, KAELEN_SKIN.base)
    put(img, 12, yl + 2, KAELEN_VEST.outline)  # Khe nách 1px
    put(img, 10, yl + 3, KAELEN_SKIN.shadow)
    put(img, 11, yl + 3, KAELEN_SKIN.highlight)

    # Tay trái đeo găng tay da bảo hộ dày cho Chim Ưng đậu (viewer's right: X=20..22)
    yr = 21 + bob
    put(img, 20, yr, KAELEN_GAUNTLET.shadow)
    put(img, 20, yr + 1, KAELEN_GAUNTLET.highlight)
    put(img, 21, yr + 1, KAELEN_GAUNTLET.base)
    put(img, 22, yr + 1, KAELEN_GAUNTLET.outline)
    put(img, 19, yr + 2, KAELEN_VEST.outline)  # Khe nách 1px
    put(img, 20, yr + 2, GOLD_TRIM.highlight)  # Viền đồng găng tay
    put(img, 21, yr + 2, KAELEN_GAUNTLET.highlight)
    put(img, 22, yr + 2, KAELEN_GAUNTLET.outline)
    put(img, 20, yr + 3, KAELEN_GAUNTLET.base)
    put(img, 21, yr + 3, KAELEN_GAUNTLET.shadow)

    # Nón da thú có 2 tai lông nhỏ trên đỉnh đầu (Y=7..16, X=10..21)
    # 2 tai lông thú nhỏ nhô lên ở X=11..13 và X=18..20, Y=7..9
    for ex0, ex1 in ((11, 13), (18, 20)):
        put(img, ex0, 7 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0 + 1, 7 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex1, 7 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0, 8 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0 + 1, 8 + bob, KAELEN_EAR_INNER)  # Lòng tai hồng kem
        put(img, ex1, 8 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0, 9 + bob, KAELEN_PELT_HAT.shadow)
        put(img, ex0 + 1, 9 + bob, KAELEN_PELT_HAT.highlight)
        put(img, ex1, 9 + bob, KAELEN_PELT_HAT.shadow)

    crown_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 11, 20),
    ]
    for y_rel, x0, x1 in crown_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 and x in (13, 18):
                put(img, x, y, KAELEN_PELT_HAT.base)
            elif y_rel == 9 or x == x0 or x == x1:
                put(img, x, y, KAELEN_PELT_HAT.outline)
            elif y_rel == 16:
                # Viền lông trắng nhẹ dưới vành mũ da thú
                put(img, x, y, KAELEN_FUR_COLLAR.highlight if ((x + y) & 1) == 0 else KAELEN_FUR_COLLAR.base)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, KAELEN_PELT_HAT.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, KAELEN_PELT_HAT.highlight if checker else KAELEN_PELT_HAT.base)

    # Khuôn mặt Chibi, mắt 2px dọc có điểm sáng trắng #F7EFEB tại Y=19
    y17 = 17 + bob
    put(img, 11, y17, KAELEN_PELT_HAT.outline)
    put(img, 12, y17, KAELEN_PELT_HAT.base)  # Vạt mũ che tai hai bên
    put(img, 13, y17, KAELEN_HAIR.shadow)
    put(img, 14, y17, KAELEN_SKIN.base)
    put(img, 15, y17, KAELEN_SKIN.shadow)
    put(img, 16, y17, KAELEN_SKIN.shadow)
    put(img, 17, y17, KAELEN_SKIN.base)
    put(img, 18, y17, KAELEN_HAIR.shadow)
    put(img, 19, y17, KAELEN_PELT_HAT.base)
    put(img, 20, y17, KAELEN_PELT_HAT.outline)

    y18 = 18 + bob
    put(img, 11, y18, KAELEN_PELT_HAT.outline)
    put(img, 12, y18, KAELEN_PELT_HAT.shadow)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, KAELEN_SKIN.shadow)
    put(img, 15, y18, KAELEN_SKIN.highlight)
    put(img, 16, y18, KAELEN_SKIN.highlight)
    put(img, 17, y18, KAELEN_SKIN.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, KAELEN_PELT_HAT.shadow)
    put(img, 20, y18, KAELEN_PELT_HAT.outline)

    y19 = 19 + bob
    put(img, 12, y19, KAELEN_HAIR.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng trắng mắt trái
    put(img, 14, y19, KAELEN_SKIN.base)
    put(img, 15, y19, KAELEN_SKIN.highlight)
    put(img, 16, y19, KAELEN_SKIN.base)
    put(img, 17, y19, KAELEN_SKIN.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng trắng mắt phải
    put(img, 19, y19, KAELEN_HAIR.outline)

    y20 = 20 + bob
    put(img, 12, y20, KAELEN_HAIR.outline)
    put(img, 13, y20, KAELEN_SKIN.base)
    put(img, 14, y20, KAELEN_SKIN.highlight)
    put(img, 15, y20, KAELEN_SKIN.highlight)
    put(img, 16, y20, KAELEN_SKIN.highlight)
    put(img, 17, y20, KAELEN_SKIN.highlight)
    put(img, 18, y20, KAELEN_SKIN.base)
    put(img, 19, y20, KAELEN_HAIR.outline)

    return img


def draw_kaelen_back_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, KAELEN_BOOTS.outline)
        put(img, 12, y, KAELEN_BOOTS.outline if y == 29 - leg_l_lift else KAELEN_BOOTS.shadow)
        put(img, 13, y, KAELEN_BOOTS.outline if y == 29 - leg_l_lift else KAELEN_BOOTS.base)
        put(img, 14, y, KAELEN_BOOTS.outline if y == 29 - leg_l_lift else KAELEN_BOOTS.shadow)
        put(img, 15, y, KAELEN_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, KAELEN_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, KAELEN_BOOTS.outline if y == 29 - leg_r_lift else KAELEN_BOOTS.shadow)
        put(img, 18, y, KAELEN_BOOTS.outline if y == 29 - leg_r_lift else KAELEN_BOOTS.base)
        put(img, 19, y, KAELEN_BOOTS.outline if y == 29 - leg_r_lift else KAELEN_BOOTS.shadow)
        put(img, 20, y, KAELEN_BOOTS.outline)

    # Lưng áo gilê da hươu & Cổ lông trắng (Y=20..25, X=12..19)
    for y_rel in range(20, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, KAELEN_VEST.outline)
            elif y_rel in (20, 21):
                put(img, x, y, KAELEN_FUR_COLLAR.highlight if ((x + y) & 1) == 0 else KAELEN_FUR_COLLAR.base)
            elif y_rel == 24:
                put(img, x, y, KAELEN_GAUNTLET.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, KAELEN_VEST.highlight if checker else KAELEN_VEST.base)

    # Tay trái đeo găng tay chim ưng (viewer's left khi nhìn từ lưng: X=9..11)
    yl = 21 + bob
    put(img, 11, yl, KAELEN_GAUNTLET.shadow)
    put(img, 9, yl + 1, KAELEN_GAUNTLET.outline)
    put(img, 10, yl + 1, KAELEN_GAUNTLET.highlight)
    put(img, 11, yl + 1, KAELEN_GAUNTLET.base)
    put(img, 9, yl + 2, KAELEN_GAUNTLET.outline)
    put(img, 10, yl + 2, KAELEN_GAUNTLET.base)
    put(img, 11, yl + 2, GOLD_TRIM.highlight)
    put(img, 12, yl + 2, KAELEN_VEST.outline)
    put(img, 10, yl + 3, KAELEN_GAUNTLET.shadow)
    put(img, 11, yl + 3, KAELEN_GAUNTLET.base)

    # Tay phải (viewer's right: X=20..21)
    yr = 21 + bob
    put(img, 20, yr, KAELEN_TUNIC.shadow)
    put(img, 20, yr + 1, KAELEN_TUNIC.base)
    put(img, 21, yr + 1, KAELEN_TUNIC.outline)
    put(img, 19, yr + 2, KAELEN_VEST.outline)
    put(img, 20, yr + 2, KAELEN_SKIN.base)
    put(img, 21, yr + 2, KAELEN_SKIN.highlight)
    put(img, 20, yr + 3, KAELEN_SKIN.shadow)
    put(img, 21, yr + 3, KAELEN_SKIN.base)

    # Sau đầu: Nón da thú có 2 tai lông nhỏ
    for ex0, ex1 in ((11, 13), (18, 20)):
        put(img, ex0, 7 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0 + 1, 7 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex1, 7 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0, 8 + bob, KAELEN_PELT_HAT.outline)
        put(img, ex0 + 1, 8 + bob, KAELEN_PELT_HAT.highlight)
        put(img, ex1, 8 + bob, KAELEN_PELT_HAT.outline)

    back_hat_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 11, 20),
        (17, 12, 19),
        (18, 13, 18),
    ]
    for y_rel, x0, x1 in back_hat_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel in (9, 18) or x == x0 or x == x1:
                put(img, x, y, KAELEN_PELT_HAT.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, KAELEN_PELT_HAT.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, KAELEN_PELT_HAT.highlight if checker else KAELEN_PELT_HAT.base)

    return img


def draw_kaelen_side_body(step=0, facing_left=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    if step == 0:
        legs = [(13, 16, 26, 29, False), (15, 18, 26, 29, True)]
    elif step == 1:
        legs = [(11, 14, 26, 28, False), (16, 19, 26, 29, True)]
    elif step == 2:
        legs = [(13, 16, 26, 29, False), (15, 18, 26, 29, True)]
    else:
        legs = [(16, 19, 26, 28, False), (12, 15, 26, 29, True)]

    for x0, x1, y0, y1, is_near in legs:
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if x == x0 or x == x1 or y == y1:
                    put(x, y, KAELEN_BOOTS.outline)
                else:
                    put(x, y, KAELEN_BOOTS.highlight if (is_near and y == y0 + 1) else (KAELEN_BOOTS.base if is_near else KAELEN_BOOTS.shadow))

    # Thân gilê da hươu & Cổ lông trắng nhìn ngang (Y=20..25, X=12..19)
    for y_rel in range(20, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, KAELEN_VEST.outline)
            elif y_rel == 20:
                put(x, y, KAELEN_FUR_COLLAR.highlight if ((x + y) & 1) == 0 else KAELEN_FUR_COLLAR.base)
            elif y_rel == 24:
                put(x, y, KAELEN_GAUNTLET.shadow)
            else:
                put(x, y, KAELEN_VEST.highlight if ((x + y) & 1) == 0 else KAELEN_VEST.base)

    # Tai thú nhô trên đỉnh mũ nhìn nghiêng (X=14..16, Y=7..8)
    put(14, 7 + bob, KAELEN_PELT_HAT.outline)
    put(15, 7 + bob, KAELEN_PELT_HAT.outline)
    put(16, 7 + bob, KAELEN_PELT_HAT.outline)
    put(14, 8 + bob, KAELEN_PELT_HAT.outline)
    put(15, 8 + bob, KAELEN_EAR_INNER)
    put(16, 8 + bob, KAELEN_PELT_HAT.outline)

    # Đầu & Nón da thú nhìn ngang
    side_head_spans = [
        (9, 12, 18),
        (10, 11, 19),
        (11, 10, 20),
        (12, 10, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 20),
        (16, 10, 15),
        (17, 10, 14),
    ]
    for y_rel, x0, x1 in side_head_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(x, y, KAELEN_PELT_HAT.outline)
            elif x == x0 + 1:
                put(x, y, KAELEN_PELT_HAT.shadow)
            else:
                put(x, y, KAELEN_PELT_HAT.highlight if ((x + y) & 1) == 0 else KAELEN_PELT_HAT.base)

    # Mặt nghiêng & Mắt có điểm sáng trắng #F7EFEB
    y16 = 16 + bob
    put(15, y16, KAELEN_FUR_COLLAR.base)
    put(16, y16, KAELEN_FUR_COLLAR.highlight)
    put(17, y16, KAELEN_FUR_COLLAR.base)
    put(18, y16, KAELEN_FUR_COLLAR.highlight)
    put(19, y16, KAELEN_PELT_HAT.outline)

    y17 = 17 + bob
    put(15, y17, KAELEN_SKIN.shadow)
    put(16, y17, KAELEN_SKIN.base)
    put(17, y17, EYE_GLINT)
    put(18, y17, EYE_PUPIL)
    put(19, y17, KAELEN_SKIN.highlight)
    put(20, y17, KAELEN_SKIN.outline)

    y18 = 18 + bob
    put(15, y18, KAELEN_SKIN.base)
    put(16, y18, KAELEN_SKIN.highlight)
    put(17, y18, KAELEN_SKIN.highlight)
    put(18, y18, KAELEN_SKIN.highlight)
    put(19, y18, KAELEN_SKIN.base)
    put(20, y18, KAELEN_SKIN.outline)

    y19 = 19 + bob
    put(14, y19, KAELEN_SKIN.outline)
    put(15, y19, KAELEN_SKIN.base)
    put(16, y19, KAELEN_SKIN.highlight)
    put(17, y19, KAELEN_SKIN.highlight)
    put(18, y19, KAELEN_SKIN.base)
    put(19, y19, KAELEN_SKIN.outline)

    # Tay găng da bảo hộ đưa ra phía trước đón chim ưng
    ya = 21 + bob
    put(15, ya, KAELEN_GAUNTLET.shadow)
    put(16, ya, KAELEN_GAUNTLET.highlight)
    put(15, ya + 1, KAELEN_GAUNTLET.outline)
    put(16, ya + 1, KAELEN_GAUNTLET.highlight)
    put(17, ya + 1, GOLD_TRIM.highlight)
    put(18, ya + 1, KAELEN_GAUNTLET.base)
    put(19, ya + 1, KAELEN_GAUNTLET.outline)
    put(17, ya + 2, KAELEN_GAUNTLET.outline)
    put(18, ya + 2, KAELEN_GAUNTLET.highlight)
    put(19, ya + 2, KAELEN_GAUNTLET.base)
    put(20, ya + 2, KAELEN_GAUNTLET.outline)

    return img


def draw_kaelen_feed_pouch(direction: str) -> Image.Image:
    """Túi thức ăn da thú đeo bên hông phải của Kaelen (Prop_FeedPouch)."""
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    if direction == "front":
        for y in range(22, 26):
            for x in range(11, 15):
                if x == 11 or x == 14 or y == 22 or y == 25:
                    put(img, x, y, KAELEN_POUCH.outline)
                elif y == 23 and x == 12:
                    put(img, x, y, GOLD_TRIM.highlight)
                else:
                    put(img, x, y, KAELEN_POUCH.highlight if ((x + y) & 1) == 0 else KAELEN_POUCH.base)
    elif direction == "back":
        for y in range(22, 26):
            for x in range(17, 21):
                if x == 17 or x == 20 or y == 22 or y == 25:
                    put(img, x, y, KAELEN_POUCH.outline)
                else:
                    put(img, x, y, KAELEN_POUCH.base)
    elif direction == "side":
        for y in range(22, 26):
            for x in range(13, 17):
                if x == 13 or x == 16 or y == 22 or y == 25:
                    put(img, x, y, KAELEN_POUCH.outline)
                elif y == 23 and x == 15:
                    put(img, x, y, GOLD_TRIM.highlight)
                else:
                    put(img, x, y, KAELEN_POUCH.highlight if ((x + y) & 1) == 0 else KAELEN_POUCH.base)
    else:  # left
        for y in range(23, 26):
            put(img, 15, y, KAELEN_POUCH.outline)
            put(img, 16, y, KAELEN_POUCH.base)
            put(img, 17, y, KAELEN_POUCH.outline)
    return img


# ==============================================================================
# 2. THỰC THỂ CHIM ƯNG (PET_FALCON) - 32x32 PPU=16
# ==============================================================================

def draw_falcon_perched(frame: int) -> Image.Image:
    """
    Chim Ưng đậu yên (Idle_Perched 1..2):
    Khung 32x32 PPU=16, móng vuốt ở trung tâm (X=14..17, Y=20) bám sát PerchPoint trên găng tay Kaelen.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    head_bob = 0 if frame == 1 else 1

    # Đuôi chim ưng rủ xuống dưới (Y=20..24)
    PixelArtShadingHelper.draw_shaded_rect(
        img, 13, 20, 16, 24, FALCON_TAIL, light_dir=(-1, -1), dither_highlight=False
    )

    # Thân & Ức lông vũ kem đan caro (X=12..19, Y=14..20)
    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, 12, 14 + head_bob, 19, 20, FALCON_BREAST, corner_cut=1, light_dir=(-1, -1), dither=True
    )

    # Cánh nâu đại bàng khép gọn hai bên (X=11..14 và X=17..20, Y=14..20)
    PixelArtShadingHelper.draw_shaded_rect(
        img, 11, 14 + head_bob, 14, 20, FALCON_WING, light_dir=(-1, -1), dither_highlight=True
    )
    PixelArtShadingHelper.draw_shaded_rect(
        img, 17, 14 + head_bob, 20, 20, FALCON_WING, light_dir=(-1, -1), dither_highlight=True
    )

    # Đầu chim ưng (X=13..18, Y=10..15)
    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, 13, 10 + head_bob, 18, 15 + head_bob, FALCON_WING, corner_cut=1, light_dir=(-1, -1), dither=True
    )
    # Mắt sắc lẹm & Mỏ vàng khoằm
    put(img, 14, 12 + head_bob, EYE_PUPIL)
    put(img, 14, 11 + head_bob, EYE_GLINT)
    put(img, 16, 12 + head_bob, FALCON_BEAK.highlight)
    put(img, 17, 12 + head_bob, FALCON_BEAK.base)
    put(img, 17, 13 + head_bob, FALCON_BEAK.shadow)

    # 2 chân móng vuốt vàng bám vào điểm đậu tại Y=20
    put(img, 14, 20, FALCON_BEAK.base)
    put(img, 15, 20, FALCON_BEAK.highlight)
    put(img, 17, 20, FALCON_BEAK.base)
    put(img, 18, 20, FALCON_BEAK.highlight)

    return img


def draw_falcon_flying(frame: int) -> Image.Image:
    """Chim Ưng vỗ cánh bay lượn (Fly_Loop 1..4)."""
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    body_y = 14 if frame in (1, 2) else 13

    # Đuôi xòe phía sau (X=8..12, Y=body_y+1..body_y+4)
    PixelArtShadingHelper.draw_shaded_rect(
        img, 8, body_y + 1, 12, body_y + 4, FALCON_TAIL, light_dir=(-1, -1), dither_highlight=False
    )

    # Thân chim bay ngang hướng sang phải (X=11..20, Y=body_y..body_y+4)
    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, 11, body_y, 20, body_y + 4, FALCON_BREAST, corner_cut=1, light_dir=(-1, -1), dither=True
    )

    # Đầu & Mỏ vươn về phía trước (X=18..23, Y=body_y-1..body_y+3)
    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, 18, body_y - 1, 22, body_y + 3, FALCON_WING, corner_cut=1, light_dir=(-1, -1), dither=False
    )
    put(img, 20, body_y, EYE_GLINT)
    put(img, 21, body_y, EYE_PUPIL)
    put(img, 23, body_y + 1, FALCON_BEAK.highlight)
    put(img, 24, body_y + 1, FALCON_BEAK.base)
    put(img, 23, body_y + 2, FALCON_BEAK.outline)

    # Đôi cánh sải rộng theo 4 pha vỗ cánh
    if frame == 1:
        PixelArtShadingHelper.draw_rounded_shaded_box(
            img, 11, 6, 19, body_y + 1, FALCON_WING, corner_cut=2, light_dir=(-1, -1), dither=True
        )
        put(img, 12, 5, FALCON_WING.outline)
        put(img, 14, 5, FALCON_WING.outline)
        put(img, 16, 5, FALCON_WING.outline)
    elif frame == 2:
        PixelArtShadingHelper.draw_rounded_shaded_box(
            img, 9, 10, 20, body_y + 2, FALCON_WING, corner_cut=1, light_dir=(-1, -1), dither=True
        )
    elif frame == 3:
        PixelArtShadingHelper.draw_rounded_shaded_box(
            img, 11, body_y + 1, 19, 23, FALCON_WING, corner_cut=2, light_dir=(-1, -1), dither=True
        )
    else:
        PixelArtShadingHelper.draw_rounded_shaded_box(
            img, 11, 9, 19, body_y + 3, FALCON_WING, corner_cut=1, light_dir=(-1, -1), dither=True
        )

    return img


# ==============================================================================
# 3. CƯ DÂN OAKHAVEN (NPC_Villager_Wanderer)
#    - Áo vải thô màu be, quần xanh
#    - Đầu vẽ đầy đủ mái tóc nâu hoàn chỉnh (để khi tháo mũ không bị hói!)
#    - Có cả anim đi bộ khi bê đồ (Carrying) và khi tay không (Unarmed)
# ==============================================================================

def draw_villager_front_body(step=0, carrying=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Quần xanh chàm & Giày vải nâu (Y=26..29)
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, VILLAGER_BOOTS.outline)
        if y == 26 - leg_l_lift:
            put(img, 12, y, VILLAGER_PANTS.shadow)
            put(img, 13, y, VILLAGER_PANTS.highlight)
            put(img, 14, y, VILLAGER_PANTS.base)
        else:
            put(img, 12, y, VILLAGER_BOOTS.outline if y == 29 - leg_l_lift else VILLAGER_BOOTS.shadow)
            put(img, 13, y, VILLAGER_BOOTS.outline if y == 29 - leg_l_lift else VILLAGER_BOOTS.base)
            put(img, 14, y, VILLAGER_BOOTS.outline if y == 29 - leg_l_lift else VILLAGER_BOOTS.base)
        put(img, 15, y, VILLAGER_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, VILLAGER_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        if y == 26 - leg_r_lift:
            put(img, 17, y, VILLAGER_PANTS.base)
            put(img, 18, y, VILLAGER_PANTS.highlight)
            put(img, 19, y, VILLAGER_PANTS.shadow)
        else:
            put(img, 17, y, VILLAGER_BOOTS.outline if y == 29 - leg_r_lift else VILLAGER_BOOTS.base)
            put(img, 18, y, VILLAGER_BOOTS.outline if y == 29 - leg_r_lift else VILLAGER_BOOTS.base)
            put(img, 19, y, VILLAGER_BOOTS.outline if y == 29 - leg_r_lift else VILLAGER_BOOTS.shadow)
        put(img, 20, y, VILLAGER_BOOTS.outline)

    # Áo vải thô màu be (Y=21..24) & Lưng quần xanh chàm (Y=25)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, VILLAGER_SHIRT.outline)
            elif y_rel == 25:
                put(img, x, y, VILLAGER_PANTS.highlight if x in (14, 15, 16) else VILLAGER_PANTS.base)
            elif y_rel == 24:
                put(img, x, y, VILLAGER_BOOTS.shadow)  # Đai vải nâu
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, VILLAGER_SHIRT.highlight if checker else VILLAGER_SHIRT.base)

    if not carrying:
        # Trạng thái tay không: 2 tay tách nhẹ 2 bên hông có khe nách 1px
        yl = 21 + bob + (1 if step == 1 else (-1 if step == 3 else 0))
        put(img, 11, yl, VILLAGER_SHIRT.shadow)
        put(img, 10, yl + 1, VILLAGER_SHIRT.outline)
        put(img, 11, yl + 1, VILLAGER_SHIRT.base)
        put(img, 10, yl + 2, VILLAGER_SKIN.highlight)
        put(img, 11, yl + 2, VILLAGER_SKIN.base)
        put(img, 12, yl + 2, VILLAGER_SHIRT.outline)  # Khe nách 1px
        put(img, 10, yl + 3, VILLAGER_SKIN.shadow)
        put(img, 11, yl + 3, VILLAGER_SKIN.highlight)

        yr = 21 + bob + (-1 if step == 1 else (1 if step == 3 else 0))
        put(img, 20, yr, VILLAGER_SHIRT.shadow)
        put(img, 20, yr + 1, VILLAGER_SHIRT.base)
        put(img, 21, yr + 1, VILLAGER_SHIRT.outline)
        put(img, 19, yr + 2, VILLAGER_SHIRT.outline)  # Khe nách 1px
        put(img, 20, yr + 2, VILLAGER_SKIN.base)
        put(img, 21, yr + 2, VILLAGER_SKIN.highlight)
        put(img, 20, yr + 3, VILLAGER_SKIN.highlight)
        put(img, 21, yr + 3, VILLAGER_SKIN.shadow)
    else:
        # Trạng thái bê giỏ hoa quả: 2 tay co về phía trước bụng ôm giữ giỏ
        ya = 21 + bob
        put(img, 11, ya, VILLAGER_SHIRT.shadow)
        put(img, 10, ya + 1, VILLAGER_SHIRT.outline)
        put(img, 11, ya + 1, VILLAGER_SHIRT.highlight)
        put(img, 11, ya + 2, VILLAGER_SKIN.highlight)
        put(img, 12, ya + 2, VILLAGER_SKIN.base)

        put(img, 20, ya, VILLAGER_SHIRT.shadow)
        put(img, 20, ya + 1, VILLAGER_SHIRT.highlight)
        put(img, 21, ya + 1, VILLAGER_SHIRT.outline)
        put(img, 19, ya + 2, VILLAGER_SKIN.base)
        put(img, 20, ya + 2, VILLAGER_SKIN.highlight)

    # Đầu to Chibi & Mái tóc nâu hạt dẻ đầy đặn hoàn chỉnh (Y=8..16, X=10..21)
    # Có lọn tóc mái bồng bềnh để khi tháo mũ rơm tuyệt đối không bị hói!
    put(img, 16, 8 + bob, VILLAGER_HAIR.highlight)
    put(img, 17, 8 + bob, VILLAGER_HAIR.outline)
    crown_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 11, 20),
    ]
    for y_rel, x0, x1 in crown_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(img, x, y, VILLAGER_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, VILLAGER_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, VILLAGER_HAIR.highlight if checker else VILLAGER_HAIR.base)

    # Mặt Chibi (Y=17..20), mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(img, 12, y17, VILLAGER_HAIR.outline)
    put(img, 13, y17, VILLAGER_HAIR.shadow)
    put(img, 14, y17, VILLAGER_HAIR.base)
    put(img, 15, y17, VILLAGER_SKIN.shadow)
    put(img, 16, y17, VILLAGER_SKIN.shadow)
    put(img, 17, y17, VILLAGER_HAIR.base)
    put(img, 18, y17, VILLAGER_HAIR.shadow)
    put(img, 19, y17, VILLAGER_HAIR.outline)

    y18 = 18 + bob
    put(img, 12, y18, VILLAGER_HAIR.outline)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, VILLAGER_SKIN.shadow)
    put(img, 15, y18, VILLAGER_SKIN.highlight)
    put(img, 16, y18, VILLAGER_SKIN.highlight)
    put(img, 17, y18, VILLAGER_SKIN.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, VILLAGER_HAIR.outline)

    y19 = 19 + bob
    put(img, 12, y19, VILLAGER_HAIR.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng trắng mắt trái
    put(img, 14, y19, VILLAGER_SKIN.base)
    put(img, 15, y19, VILLAGER_SKIN.highlight)
    put(img, 16, y19, VILLAGER_SKIN.base)
    put(img, 17, y19, VILLAGER_SKIN.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng trắng mắt phải
    put(img, 19, y19, VILLAGER_HAIR.outline)

    y20 = 20 + bob
    put(img, 12, y20, VILLAGER_HAIR.outline)
    put(img, 13, y20, VILLAGER_SKIN.base)
    put(img, 14, y20, VILLAGER_SKIN.highlight)
    put(img, 15, y20, VILLAGER_SKIN.highlight)
    put(img, 16, y20, VILLAGER_SKIN.highlight)
    put(img, 17, y20, VILLAGER_SKIN.highlight)
    put(img, 18, y20, VILLAGER_SKIN.base)
    put(img, 19, y20, VILLAGER_HAIR.outline)

    return img


def draw_villager_back_body(step=0, carrying=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, VILLAGER_BOOTS.outline)
        if y == 26 - leg_l_lift:
            put(img, 12, y, VILLAGER_PANTS.shadow)
            put(img, 13, y, VILLAGER_PANTS.base)
            put(img, 14, y, VILLAGER_PANTS.shadow)
        else:
            put(img, 12, y, VILLAGER_BOOTS.outline if y == 29 - leg_l_lift else VILLAGER_BOOTS.shadow)
            put(img, 13, y, VILLAGER_BOOTS.outline if y == 29 - leg_l_lift else VILLAGER_BOOTS.base)
            put(img, 14, y, VILLAGER_BOOTS.outline if y == 29 - leg_l_lift else VILLAGER_BOOTS.shadow)
        put(img, 15, y, VILLAGER_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, VILLAGER_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        if y == 26 - leg_r_lift:
            put(img, 17, y, VILLAGER_PANTS.shadow)
            put(img, 18, y, VILLAGER_PANTS.base)
            put(img, 19, y, VILLAGER_PANTS.shadow)
        else:
            put(img, 17, y, VILLAGER_BOOTS.outline if y == 29 - leg_r_lift else VILLAGER_BOOTS.shadow)
            put(img, 18, y, VILLAGER_BOOTS.outline if y == 29 - leg_r_lift else VILLAGER_BOOTS.base)
            put(img, 19, y, VILLAGER_BOOTS.outline if y == 29 - leg_r_lift else VILLAGER_BOOTS.shadow)
        put(img, 20, y, VILLAGER_BOOTS.outline)

    # Lưng áo vải thô màu be & Quần xanh (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, VILLAGER_SHIRT.outline)
            elif y_rel == 25:
                put(img, x, y, VILLAGER_PANTS.base if x in (14, 15, 16, 17) else VILLAGER_PANTS.shadow)
            elif y_rel == 24:
                put(img, x, y, VILLAGER_BOOTS.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, VILLAGER_SHIRT.highlight if checker else VILLAGER_SHIRT.base)

    # 2 tay từ sau lưng
    yl = 21 + bob
    put(img, 11, yl, VILLAGER_SHIRT.shadow)
    put(img, 10, yl + 1, VILLAGER_SHIRT.outline)
    put(img, 11, yl + 1, VILLAGER_SHIRT.base)
    put(img, 10, yl + 2, VILLAGER_SKIN.base)
    put(img, 11, yl + 2, VILLAGER_SKIN.shadow)
    put(img, 12, yl + 2, VILLAGER_SHIRT.outline)
    put(img, 10, yl + 3, VILLAGER_SKIN.shadow)
    put(img, 11, yl + 3, VILLAGER_SKIN.base)

    yr = 21 + bob
    put(img, 20, yr, VILLAGER_SHIRT.shadow)
    put(img, 20, yr + 1, VILLAGER_SHIRT.base)
    put(img, 21, yr + 1, VILLAGER_SHIRT.outline)
    put(img, 19, yr + 2, VILLAGER_SHIRT.outline)
    put(img, 20, yr + 2, VILLAGER_SKIN.base)
    put(img, 21, yr + 2, VILLAGER_SKIN.highlight)
    put(img, 20, yr + 3, VILLAGER_SKIN.shadow)
    put(img, 21, yr + 3, VILLAGER_SKIN.base)

    # Tóc nâu hạt dẻ phủ kín sau gáy
    back_hair_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 11, 20),
        (17, 12, 19),
        (18, 13, 18),
    ]
    for y_rel, x0, x1 in back_hair_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel in (9, 18) or x == x0 or x == x1:
                put(img, x, y, VILLAGER_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, VILLAGER_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, VILLAGER_HAIR.highlight if checker else VILLAGER_HAIR.base)

    return img


def draw_villager_side_body(step=0, facing_left=False, carrying=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    if step == 0:
        legs = [(13, 16, 26, 29, False), (15, 18, 26, 29, True)]
    elif step == 1:
        legs = [(11, 14, 26, 28, False), (16, 19, 26, 29, True)]
    elif step == 2:
        legs = [(13, 16, 26, 29, False), (15, 18, 26, 29, True)]
    else:
        legs = [(16, 19, 26, 28, False), (12, 15, 26, 29, True)]

    for x0, x1, y0, y1, is_near in legs:
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if x == x0 or x == x1 or y == y1:
                    put(x, y, VILLAGER_BOOTS.outline)
                elif y == y0:
                    put(x, y, VILLAGER_PANTS.highlight if is_near else VILLAGER_PANTS.shadow)
                else:
                    put(x, y, VILLAGER_BOOTS.highlight if (is_near and y == y0 + 1) else (VILLAGER_BOOTS.base if is_near else VILLAGER_BOOTS.shadow))

    # Áo vải thô màu be & quần xanh nhìn ngang (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, VILLAGER_SHIRT.outline)
            elif y_rel == 25:
                put(x, y, VILLAGER_PANTS.base)
            elif y_rel == 24:
                put(x, y, VILLAGER_BOOTS.shadow)
            else:
                put(x, y, VILLAGER_SHIRT.highlight if ((x + y) & 1) == 0 else VILLAGER_SHIRT.base)

    # Đầu & Tóc nâu hạt dẻ nhìn ngang đầy đặn
    side_head_spans = [
        (9, 12, 18),
        (10, 11, 19),
        (11, 10, 20),
        (12, 10, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 19),
        (16, 10, 15),
        (17, 11, 14),
    ]
    for y_rel, x0, x1 in side_head_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(x, y, VILLAGER_HAIR.outline)
            elif x == x0 + 1:
                put(x, y, VILLAGER_HAIR.shadow)
            else:
                put(x, y, VILLAGER_HAIR.highlight if ((x + y) & 1) == 0 else VILLAGER_HAIR.base)

    # Mặt nghiêng & Mắt có điểm sáng trắng #F7EFEB
    y16 = 16 + bob
    put(15, y16, VILLAGER_HAIR.shadow)
    put(16, y16, VILLAGER_SKIN.shadow)
    put(17, y16, VILLAGER_SKIN.base)
    put(18, y16, VILLAGER_HAIR.base)
    put(19, y16, VILLAGER_HAIR.outline)

    y17 = 17 + bob
    put(15, y17, VILLAGER_SKIN.shadow)
    put(16, y17, VILLAGER_SKIN.base)
    put(17, y17, EYE_GLINT)
    put(18, y17, EYE_PUPIL)
    put(19, y17, VILLAGER_SKIN.highlight)
    put(20, y17, VILLAGER_SKIN.outline)

    y18 = 18 + bob
    put(15, y18, VILLAGER_SKIN.base)
    put(16, y18, VILLAGER_SKIN.highlight)
    put(17, y18, VILLAGER_SKIN.highlight)
    put(18, y18, VILLAGER_SKIN.highlight)
    put(19, y18, VILLAGER_SKIN.base)
    put(20, y18, VILLAGER_SKIN.outline)

    y19 = 19 + bob
    put(14, y19, VILLAGER_SKIN.outline)
    put(15, y19, VILLAGER_SKIN.base)
    put(16, y19, VILLAGER_SKIN.highlight)
    put(17, y19, VILLAGER_SKIN.highlight)
    put(18, y19, VILLAGER_SKIN.base)
    put(19, y19, VILLAGER_SKIN.outline)

    # Cánh tay nhìn ngang:
    ya = 21 + bob
    if not carrying:
        # Tay buông tự nhiên bên sườn
        put(15, ya, VILLAGER_SHIRT.shadow)
        put(16, ya, VILLAGER_SHIRT.highlight)
        put(15, ya + 1, VILLAGER_SHIRT.outline)
        put(16, ya + 1, VILLAGER_SHIRT.highlight)
        put(17, ya + 1, VILLAGER_SHIRT.base)
        put(18, ya + 1, VILLAGER_SHIRT.outline)
        put(15, ya + 2, VILLAGER_SKIN.outline)
        put(16, ya + 2, VILLAGER_SKIN.highlight)
        put(17, ya + 2, VILLAGER_SKIN.base)
        put(16, ya + 3, VILLAGER_SKIN.shadow)
    else:
        # Tay đưa ra phía trước ôm giỏ hoa quả
        put(15, ya, VILLAGER_SHIRT.shadow)
        put(16, ya, VILLAGER_SHIRT.highlight)
        put(16, ya + 1, VILLAGER_SHIRT.highlight)
        put(17, ya + 1, VILLAGER_SHIRT.base)
        put(18, ya + 1, VILLAGER_SHIRT.base)
        put(19, ya + 1, VILLAGER_SKIN.highlight)
        put(18, ya + 2, VILLAGER_SKIN.base)
        put(19, ya + 2, VILLAGER_SKIN.highlight)

    return img


# ==============================================================================
# ĐẠO CỤ CƯ DÂN OAKHAVEN: MŨ RƠM (Prop_StrawHat) & GIỎ HOA QUẢ (Prop_FruitBasket)
# ==============================================================================

def draw_villager_straw_hat(direction: str) -> Image.Image:
    """
    Mũ rơm vàng thắt ruy băng đỏ (Prop_StrawHat) trên lưới 32x32 (PPU=16).
    - Khi đội trên đầu: Đặt ở Y=6..13 ôm vừa vặn đỉnh đầu Chibi (Y=9..16).
    - Khi tháo đặt dưới đất ('world'): Đặt ở tâm dưới khung kèm bóng đổ DropShadow.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    hy = 15 if direction == "world" else 6

    if direction == "world":
        PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=23, rx=7, ry=2)

    # Chóp mũ rơm tròn đan caro dithering (X=10..21, Y=hy..hy+5)
    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, 10, hy, 21, hy + 5, STRAW_HAT, corner_cut=2, light_dir=(-1, -1), dither=True
    )

    # Dải ruy băng đỏ thắt ngang chân chóp mũ (Y=hy+4)
    for x in range(10, 22):
        if x in (10, 21):
            put(img, x, hy + 4, HAT_RIBBON.outline)
        else:
            c = HAT_RIBBON.highlight if x in (13, 14, 15) else HAT_RIBBON.base
            put(img, x, hy + 4, c)

    # Vành mũ rơm rộng che nắng (X=7..24, Y=hy+5..hy+7)
    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, 7, hy + 5, 24, hy + 7, STRAW_HAT, corner_cut=1, light_dir=(-1, -1), dither=True
    )

    if direction == "back":
        put(img, 15, hy + 5, HAT_RIBBON.highlight)
        put(img, 16, hy + 5, HAT_RIBBON.highlight)
        put(img, 14, hy + 6, HAT_RIBBON.base)
        put(img, 17, hy + 6, HAT_RIBBON.base)

    return img


def draw_villager_fruit_basket(direction: str) -> Image.Image:
    """
    Giỏ hoa quả mây đan đầy ắp trái cây tươi (Prop_FruitBasket) trên lưới 32x32 (PPU=16).
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "back":
        # Nhìn từ sau lưng: giỏ ở phía trước bụng, chỉ lộ nhẹ 2 mép bên hông
        PixelArtShadingHelper.draw_shaded_rect(img, 10, 22, 11, 25, WICKER_BASKET, dither_highlight=False)
        PixelArtShadingHelper.draw_shaded_rect(img, 20, 22, 21, 25, WICKER_BASKET, dither_highlight=False)
        return img

    if direction == "world":
        bx0, bx1, by0, by1 = 10, 21, 18, 25
        PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=25, rx=6, ry=2)
    elif direction == "front":
        bx0, bx1, by0, by1 = 11, 20, 21, 26
    elif direction == "side":
        bx0, bx1, by0, by1 = 16, 24, 21, 26
    else:  # left
        bx0, bx1, by0, by1 = 7, 15, 21, 26

    mid_x = (bx0 + bx1) // 2
    for x in range(bx0 + 2, bx1 - 1):
        put(img, x, by0 - 2, WICKER_BASKET.outline)
        put(img, x, by0 - 1, WICKER_BASKET.highlight if x == mid_x else WICKER_BASKET.base)

    PixelArtShadingHelper.draw_rounded_shaded_box(
        img, bx0, by0 + 1, bx1, by1, WICKER_BASKET, corner_cut=1, light_dir=(-1, -1), dither=True
    )

    # Hoa quả tươi đầy ắp bên trên miệng giỏ: Táo đỏ, Cam vàng, Nho tím, Lá xanh
    put(img, bx0 + 2, by0, FRUIT_APPLE.highlight)
    put(img, bx0 + 3, by0, FRUIT_APPLE.base)
    put(img, bx0 + 2, by0 + 1, FRUIT_APPLE.base)
    put(img, bx0 + 3, by0 + 1, FRUIT_APPLE.shadow)
    put(img, bx0 + 4, by0 - 1, FRUIT_LEAF.highlight)
    put(img, bx0 + 4, by0, FRUIT_LEAF.base)
    put(img, mid_x, by0, FRUIT_ORANGE.highlight)
    put(img, mid_x + 1, by0, FRUIT_ORANGE.base)
    put(img, mid_x, by0 + 1, FRUIT_ORANGE.base)
    put(img, bx1 - 3, by0, FRUIT_GRAPE.highlight)
    put(img, bx1 - 2, by0, FRUIT_GRAPE.base)
    put(img, bx1 - 2, by0 + 1, FRUIT_GRAPE.shadow)

    return img


# ==============================================================================
# HÀM CHÍNH: XUẤT TOÀN BỘ SPRITE & META CHO KAELEN, FALCON, VILLAGER
# ==============================================================================

def generate_part3_all():
    # 1. KAELEN
    kaelen_dir = os.path.join(BASE_MODULAR_DIR, "Kaelen")
    os.makedirs(kaelen_dir, exist_ok=True)

    kaelen_pouches = {}
    for d in ("front", "back", "side", "left"):
        pouch_img = draw_kaelen_feed_pouch(d)
        kaelen_pouches[d] = pouch_img
        PixelArtShadingHelper.save_sprite_with_meta(
            pouch_img, os.path.join(kaelen_dir, f"prop_kaelen_pouch_{d}.png")
        )

    for d, fn, pouch_front in [
        ("front", draw_kaelen_front_body, True),
        ("back", draw_kaelen_back_body, False),
        ("side", lambda st: draw_kaelen_side_body(st, False), True),
        ("left", lambda st: draw_kaelen_side_body(st, True), False),
    ]:
        idle_body = fn(0)
        idle_with_pouch = compose_layers(idle_body, [(kaelen_pouches[d], pouch_front)])
        PixelArtShadingHelper.save_sprite_with_meta(
            idle_with_pouch, os.path.join(kaelen_dir, f"npc_kaelen_body_idle_{d}.png")
        )
        PixelArtShadingHelper.save_sprite_with_meta(
            idle_with_pouch, os.path.join(FLAT_NPCS_DIR, f"npc_kaelen_idle_{d}.png")
        )

        for st in range(1, 5):
            walk_body = fn(st)
            bob = 1 if st in (1, 3) else 0
            pouch_shifted = PixelArtShadingHelper.create_canvas(32, 32)
            pouch_shifted.paste(kaelen_pouches[d], (0, bob))
            walk_with_pouch = compose_layers(walk_body, [(pouch_shifted, pouch_front)])
            PixelArtShadingHelper.save_sprite_with_meta(
                walk_with_pouch, os.path.join(kaelen_dir, f"npc_kaelen_body_walk_{d}_{st}.png")
            )
            PixelArtShadingHelper.save_sprite_with_meta(
                walk_with_pouch, os.path.join(FLAT_NPCS_DIR, f"npc_kaelen_walk_{d}_{st}.png")
            )

    print("Kaelen (PetMaster) done!")

    # 2. PET_FALCON
    falcon_dir = os.path.join(BASE_MODULAR_DIR, "Falcon")
    os.makedirs(falcon_dir, exist_ok=True)

    for i in (1, 2):
        perched_img = draw_falcon_perched(i)
        PixelArtShadingHelper.save_sprite_with_meta(
            perched_img, os.path.join(falcon_dir, f"falcon_idle_perched_{i}.png")
        )

    for i in (1, 2, 3, 4):
        fly_img = draw_falcon_flying(i)
        PixelArtShadingHelper.save_sprite_with_meta(
            fly_img, os.path.join(falcon_dir, f"falcon_fly_loop_{i}.png")
        )

    print("Pet_Falcon done!")

    # 3. VILLAGER OAKHAVEN
    villager_dir = os.path.join(BASE_MODULAR_DIR, "Villager")
    os.makedirs(villager_dir, exist_ok=True)

    hats = {}
    baskets = {}
    for d in ("front", "back", "side", "left", "world"):
        h_img = draw_villager_straw_hat(d)
        b_img = draw_villager_fruit_basket(d)
        hats[d] = h_img
        baskets[d] = b_img
        PixelArtShadingHelper.save_sprite_with_meta(
            h_img, os.path.join(villager_dir, f"prop_villager_hat_{d}.png")
        )
        PixelArtShadingHelper.save_sprite_with_meta(
            b_img, os.path.join(villager_dir, f"prop_villager_basket_{d}.png")
        )

    for d, fn_unarmed, fn_carrying, basket_in_front in [
        ("front", lambda st: draw_villager_front_body(st, False), lambda st: draw_villager_front_body(st, True), True),
        ("back", lambda st: draw_villager_back_body(st, False), lambda st: draw_villager_back_body(st, True), False),
        ("side", lambda st: draw_villager_side_body(st, False, False), lambda st: draw_villager_side_body(st, False, True), True),
        ("left", lambda st: draw_villager_side_body(st, True, False), lambda st: draw_villager_side_body(st, True, True), True),
    ]:
        idle_unarmed = fn_unarmed(0)
        PixelArtShadingHelper.save_sprite_with_meta(
            idle_unarmed, os.path.join(villager_dir, f"npc_villager_body_idle_{d}.png")
        )

        idle_carry = fn_carrying(0)
        PixelArtShadingHelper.save_sprite_with_meta(
            idle_carry, os.path.join(villager_dir, f"npc_villager_carry_idle_{d}.png")
        )

        full_idle = compose_layers(idle_carry, [(baskets[d], basket_in_front), (hats[d], True)])
        PixelArtShadingHelper.save_sprite_with_meta(
            full_idle, os.path.join(FLAT_NPCS_DIR, f"npc_villager_idle_{d}.png")
        )

        for st in range(1, 5):
            walk_unarmed = fn_unarmed(st)
            PixelArtShadingHelper.save_sprite_with_meta(
                walk_unarmed, os.path.join(villager_dir, f"npc_villager_body_walk_{d}_{st}.png")
            )

            walk_carry = fn_carrying(st)
            PixelArtShadingHelper.save_sprite_with_meta(
                walk_carry, os.path.join(villager_dir, f"npc_villager_carry_walk_{d}_{st}.png")
            )

            bob = 1 if st in (1, 3) else 0
            h_sh = PixelArtShadingHelper.create_canvas(32, 32)
            b_sh = PixelArtShadingHelper.create_canvas(32, 32)
            h_sh.paste(hats[d], (0, bob))
            b_sh.paste(baskets[d], (0, bob))
            full_walk = compose_layers(walk_carry, [(b_sh, basket_in_front), (h_sh, True)])
            PixelArtShadingHelper.save_sprite_with_meta(
                full_walk, os.path.join(FLAT_NPCS_DIR, f"npc_villager_walk_{d}_{st}.png")
            )

    print("Villager (Oakhaven) done!")
    print("PART 3/3 SPRITES GENERATED SUCCESSFULLY!")


if __name__ == "__main__":
    generate_part3_all()
