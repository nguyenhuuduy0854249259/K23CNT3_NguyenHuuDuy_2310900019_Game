"""
Master Pixel Art Generator for 4 Modular NPCs - Part 2/3
---------------------------------------------------------
1. Thợ Rèn Boris (NPC_Blacksmith_Barnaby.prefab) -> Prop_Hammer
2. Dược Sĩ Elena (NPC_Alchemist_Elena.prefab) -> Prop_HerbBag
3. Hội Trưởng Carlos (NPC_GuildMaster_Carlos.prefab) -> Prop_KnightSword & Prop_CrimsonCape
4. Thương Nhân Lina (NPC_Merchant_Lina.prefab) -> Prop_Basket & Prop_CoinPouch

Kế thừa 100% hàm PixelArtShadingHelper:
- Bảng màu 4 sắc độ ColorRamp4 (Outline -> Shadow -> Base -> Highlight)
- Đan caro Dithering (x + y) % 2 == 0 trên trang phục, tóc, áo choàng, tạp dề, giáp kim loại
- Giải phẫu Chibi 32x32 đồng bộ chuẩn Player Class (đầu to Y=8..17, mắt có điểm sáng trắng #F7EFEB,
  2 tay tách nhẹ 2 bên hông có viền tối 1px khe nách, 2 chân tách rõ và có bóng mờ DropShadow dưới chân)
- Đạo cụ tách lớp vẽ trực tiếp trên cùng lưới 32x32 (PPU = 16, No Mixels)
"""

import os
import sys
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')
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
# BẢNG MÀU 4 SẮC ĐỘ CHO 4 NHÂN VẬT
# ==============================================================================
# --- BORIS ---
BORIS_SKIN = ColorRamp4("#36140B", "#8F4C23", "#D48046", "#F5AC78")
BORIS_HAIR = ColorRamp4("#170C06", "#3B1E0D", "#5E3216", "#8C522B")
BORIS_APRON = ColorRamp4("#240E05", "#5E260D", "#9E4516", "#CF6829")
BORIS_PANTS = ColorRamp4("#0E131A", "#1F2A38", "#384A5E", "#58718F")
BORIS_BOOTS = ColorRamp4("#1C0F08", "#331C0E", "#4D2D19", "#6B4228")
HAMMER_STEEL = ColorRamp4("#141E26", "#384C5C", "#738E9E", "#E8F4FA")
HAMMER_WOOD = ColorRamp4("#211208", "#4D2B14", "#7E4B25", "#B07240")

# --- ELENA ---
ELENA_SKIN = ColorRamp4("#381515", "#9E6347", "#DB9E81", "#FFE0D1")
ELENA_HAIR = ColorRamp4("#21110A", "#4A2616", "#7D4327", "#B56B43")
ELENA_FLOWER = ColorRamp4("#0E2942", "#165380", "#2991DE", "#8CE2FF")
ELENA_DRESS = ColorRamp4("#0A2E20", "#1B5E43", "#38A87A", "#73E0B1")
ELENA_APRON = ColorRamp4("#22303D", "#647B91", "#B8CAD9", "#FFFFFF")
ELENA_BAG = ColorRamp4("#241107", "#572D14", "#8C4E28", "#C27A4A")
ELENA_POTION_PURPLE = ColorRamp4("#3B0842", "#7A198A", "#C93AF0", "#F9B8FF")
ELENA_POTION_CYAN = ColorRamp4("#05333B", "#0E6D7D", "#1CD0ED", "#C7FFFF")
ELENA_BOOTS = ColorRamp4("#1F100A", "#3B2114", "#613923", "#8C5738")

# --- CARLOS ---
CARLOS_SKIN = ColorRamp4("#36140E", "#8F4C23", "#D48046", "#F5AC78")
CARLOS_HAIR = ColorRamp4("#261608", "#5E3917", "#9C642B", "#DEA04E")
CARLOS_ARMOR = ColorRamp4("#161B24", "#3E4957", "#7E8D9E", "#FFFFFF")
CARLOS_CAPE = ColorRamp4("#2B0808", "#5E1212", "#A82020", "#E84343")
CARLOS_SWORD_STEEL = ColorRamp4("#141A21", "#3B4857", "#8294A6", "#FFFFFF")
CARLOS_SWORD_GOLD = ColorRamp4("#361E06", "#7A490F", "#C98A24", "#FCE258")
CARLOS_BOOTS = ColorRamp4("#1A0E08", "#361E12", "#573320", "#804F33")

# --- LINA ---
LINA_SKIN = ColorRamp4("#361313", "#9E6347", "#DB9E81", "#FFE0D1")
LINA_HAIR = ColorRamp4("#241108", "#4D2613", "#824424", "#B86739")
LINA_SCARF = ColorRamp4("#3B1C05", "#8F4C0D", "#DE871B", "#FFD042")
LINA_DRESS = ColorRamp4("#290B04", "#6E2611", "#BD4D24", "#F07E51")
LINA_APRON = ColorRamp4("#22303D", "#697E91", "#BDCCD9", "#FFFFFF")
LINA_BASKET = ColorRamp4("#2B1607", "#633816", "#A3642E", "#D6914D")
LINA_POUCH = ColorRamp4("#361F06", "#7A4B10", "#C98A24", "#FFDC4A")
LINA_BOOTS = ColorRamp4("#1C0F08", "#361E12", "#573320", "#7D4C30")

EYE_PUPIL = hex_to_rgba("#361313")
EYE_GLINT = hex_to_rgba("#F7EFEB")


# ==============================================================================
# 1. THỢ RÈN BORIS (NPC_Blacksmith_Barnaby)
# ==============================================================================
def draw_boris_front_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots (X=12..14 and X=17..19)
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, BORIS_BOOTS.outline)
        put(img, 12, y, BORIS_BOOTS.outline if y == 29 - leg_l_lift else BORIS_BOOTS.shadow)
        put(img, 13, y, BORIS_BOOTS.outline if y == 29 - leg_l_lift else BORIS_BOOTS.base)
        put(img, 14, y, BORIS_BOOTS.outline if y == 29 - leg_l_lift else BORIS_BOOTS.base)
        put(img, 15, y, BORIS_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, BORIS_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, BORIS_BOOTS.outline if y == 29 - leg_r_lift else BORIS_BOOTS.base)
        put(img, 18, y, BORIS_BOOTS.outline if y == 29 - leg_r_lift else BORIS_BOOTS.base)
        put(img, 19, y, BORIS_BOOTS.outline if y == 29 - leg_r_lift else BORIS_BOOTS.shadow)
        put(img, 20, y, BORIS_BOOTS.outline)

    # Pants & Leather Apron (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, BORIS_APRON.outline)
            elif y_rel == 25:
                # Quần thợ rèn lòi ra dưới chân tạp dề
                put(img, x, y, BORIS_PANTS.base if x in (13, 14, 17, 18) else BORIS_PANTS.shadow)
            elif y_rel in (23, 24):
                # Tạp dề da thuộc đan caro Dithering
                checker = ((x + y) & 1) == 0
                put(img, x, y, BORIS_APRON.highlight if (checker and x in (14, 15, 16)) else BORIS_APRON.base)
            else:
                # Đai da và yếm ngực
                put(img, x, y, BORIS_APRON.highlight if x in (15, 16) else BORIS_APRON.base)

    # 2 Tay cơ bắp cuồn cuộn có khối cơ + khe nách tối 1px
    # Tay phải (viewer's left: X=10..11)
    yl = 21 + bob
    put(img, 11, yl, BORIS_SKIN.shadow)
    put(img, 10, yl + 1, BORIS_SKIN.outline)
    put(img, 11, yl + 1, BORIS_SKIN.highlight)
    put(img, 10, yl + 2, BORIS_SKIN.highlight)
    put(img, 11, yl + 2, BORIS_SKIN.base)
    put(img, 12, yl + 2, BORIS_SKIN.outline)  # Khe nách 1px
    put(img, 10, yl + 3, BORIS_SKIN.shadow)
    put(img, 11, yl + 3, BORIS_SKIN.highlight)

    # Tay trái cầm búa (viewer's right: X=20..21) - nắm chắc búa rèn
    yr = 21 + bob
    put(img, 20, yr, BORIS_SKIN.shadow)
    put(img, 20, yr + 1, BORIS_SKIN.base)
    put(img, 21, yr + 1, BORIS_SKIN.outline)
    put(img, 19, yr + 2, BORIS_SKIN.outline)  # Khe nách 1px
    put(img, 20, yr + 2, BORIS_SKIN.base)
    put(img, 21, yr + 2, BORIS_SKIN.highlight)
    put(img, 20, yr + 3, BORIS_SKIN.highlight)
    put(img, 21, yr + 3, BORIS_SKIN.shadow)

    # Đầu to Chibi & Tóc nâu ngắn (Y=8..17, X=10..21)
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
                put(img, x, y, BORIS_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, BORIS_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, BORIS_HAIR.highlight if checker else BORIS_HAIR.base)

    # Mặt Chibi, mắt có điểm sáng trắng (#F7EFEB) và râu quai nón nâu sẫm bao quanh cằm
    y17 = 17 + bob
    put(img, 12, y17, BORIS_HAIR.outline)
    put(img, 13, y17, BORIS_HAIR.shadow)  # Lông mày rậm
    put(img, 14, y17, BORIS_HAIR.base)
    put(img, 15, y17, BORIS_SKIN.shadow)
    put(img, 16, y17, BORIS_SKIN.shadow)
    put(img, 17, y17, BORIS_HAIR.base)
    put(img, 18, y17, BORIS_HAIR.shadow)
    put(img, 19, y17, BORIS_HAIR.outline)

    y18 = 18 + bob
    put(img, 12, y18, BORIS_HAIR.outline)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, BORIS_SKIN.shadow)
    put(img, 15, y18, BORIS_SKIN.highlight)
    put(img, 16, y18, BORIS_SKIN.highlight)
    put(img, 17, y18, BORIS_SKIN.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, BORIS_HAIR.outline)

    y19 = 19 + bob
    put(img, 12, y19, BORIS_HAIR.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng mắt trái
    put(img, 14, y19, BORIS_SKIN.base)
    put(img, 15, y19, BORIS_SKIN.highlight)
    put(img, 16, y19, BORIS_SKIN.base)
    put(img, 17, y19, BORIS_SKIN.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng mắt phải
    put(img, 19, y19, BORIS_HAIR.outline)

    # Râu quai nón nâu sẫm (Y=20..22, X=12..19)
    y20 = 20 + bob
    put(img, 12, y20, BORIS_HAIR.outline)
    put(img, 13, y20, BORIS_HAIR.base)     # Quai nón bên má
    put(img, 14, y20, BORIS_HAIR.highlight) # Ria mép
    put(img, 15, y20, BORIS_HAIR.base)
    put(img, 16, y20, BORIS_HAIR.base)
    put(img, 17, y20, BORIS_HAIR.highlight)
    put(img, 18, y20, BORIS_HAIR.base)
    put(img, 19, y20, BORIS_HAIR.outline)

    y21_b = 21 + bob
    put(img, 13, y21_b, BORIS_HAIR.outline)
    put(img, 14, y21_b, BORIS_HAIR.shadow)
    put(img, 15, y21_b, BORIS_HAIR.base)
    put(img, 16, y21_b, BORIS_HAIR.base)
    put(img, 17, y21_b, BORIS_HAIR.shadow)
    put(img, 18, y21_b, BORIS_HAIR.outline)

    y22_b = 22 + bob
    put(img, 14, y22_b, BORIS_HAIR.outline)
    put(img, 15, y22_b, BORIS_HAIR.shadow)
    put(img, 16, y22_b, BORIS_HAIR.shadow)
    put(img, 17, y22_b, BORIS_HAIR.outline)

    return img


def draw_boris_back_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, BORIS_BOOTS.outline)
        put(img, 12, y, BORIS_BOOTS.outline if y == 29 - leg_l_lift else BORIS_BOOTS.shadow)
        put(img, 13, y, BORIS_BOOTS.outline if y == 29 - leg_l_lift else BORIS_BOOTS.base)
        put(img, 14, y, BORIS_BOOTS.outline if y == 29 - leg_l_lift else BORIS_BOOTS.shadow)
        put(img, 15, y, BORIS_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, BORIS_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, BORIS_BOOTS.outline if y == 29 - leg_r_lift else BORIS_BOOTS.shadow)
        put(img, 18, y, BORIS_BOOTS.outline if y == 29 - leg_r_lift else BORIS_BOOTS.base)
        put(img, 19, y, BORIS_BOOTS.outline if y == 29 - leg_r_lift else BORIS_BOOTS.shadow)
        put(img, 20, y, BORIS_BOOTS.outline)

    # Lưng áo và dây tạp dề da bắt chéo (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, BORIS_PANTS.outline)
            elif (x == y_rel - 8 or x == 27 - y_rel) and y_rel <= 23:
                # Dây đai da bắt chéo lưng
                put(img, x, y, BORIS_APRON.highlight)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, BORIS_PANTS.base if checker else BORIS_PANTS.shadow)

    # 2 tay từ sau lưng
    yl = 21 + bob
    put(img, 11, yl, BORIS_SKIN.shadow)
    put(img, 10, yl + 1, BORIS_SKIN.outline)
    put(img, 11, yl + 1, BORIS_SKIN.base)
    put(img, 10, yl + 2, BORIS_SKIN.base)
    put(img, 11, yl + 2, BORIS_SKIN.shadow)
    put(img, 12, yl + 2, BORIS_SKIN.outline)
    put(img, 10, yl + 3, BORIS_SKIN.shadow)
    put(img, 11, yl + 3, BORIS_SKIN.base)

    yr = 21 + bob
    put(img, 20, yr, BORIS_SKIN.shadow)
    put(img, 20, yr + 1, BORIS_SKIN.base)
    put(img, 21, yr + 1, BORIS_SKIN.outline)
    put(img, 19, yr + 2, BORIS_SKIN.outline)
    put(img, 20, yr + 2, BORIS_SKIN.base)
    put(img, 21, yr + 2, BORIS_SKIN.highlight)
    put(img, 20, yr + 3, BORIS_SKIN.shadow)
    put(img, 21, yr + 3, BORIS_SKIN.base)

    # Tóc sau gáy (Y=9..20, X=10..21)
    back_hair_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 10, 21),
        (17, 11, 20),
        (18, 12, 19),
        (19, 13, 18),
        (20, 14, 17),
    ]
    for y_rel, x0, x1 in back_hair_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel in (9, 20) or x == x0 or x == x1:
                put(img, x, y, BORIS_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, BORIS_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, BORIS_HAIR.highlight if checker else BORIS_HAIR.base)

    return img


def draw_boris_side_body(step=0, facing_left=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2 Chân bước nhìn ngang
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
                    put(x, y, BORIS_BOOTS.outline)
                else:
                    put(x, y, BORIS_BOOTS.highlight if (is_near and y == y0 + 1) else (BORIS_BOOTS.base if is_near else BORIS_BOOTS.shadow))

    # Thân nhìn ngang (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, BORIS_APRON.outline)
            elif x >= 16:
                put(x, y, BORIS_APRON.highlight if ((x + y) & 1) == 0 else BORIS_APRON.base)
            else:
                put(x, y, BORIS_PANTS.base if ((x + y) & 1) == 0 else BORIS_PANTS.shadow)

    # Đầu & Tóc nâu nhìn ngang
    side_head_spans = [
        (9, 12, 18),
        (10, 11, 19),
        (11, 10, 20),
        (12, 10, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 19),
    ]
    for y_rel, x0, x1 in side_head_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(x, y, BORIS_HAIR.outline)
            elif x == x0 + 1:
                put(x, y, BORIS_HAIR.shadow)
            else:
                put(x, y, BORIS_HAIR.highlight if ((x + y) & 1) == 0 else BORIS_HAIR.base)

    # Tai & Mặt nghiêng có mắt sáng trắng #F7EFEB
    y16 = 16 + bob
    put(14, y16, BORIS_SKIN.base)
    put(15, y16, BORIS_HAIR.shadow)
    put(16, y16, BORIS_HAIR.shadow)
    put(17, y16, BORIS_HAIR.base)
    put(18, y16, BORIS_HAIR.outline)

    y17 = 17 + bob
    put(14, y17, BORIS_SKIN.highlight)
    put(15, y17, BORIS_SKIN.shadow)
    put(16, y17, BORIS_SKIN.base)
    put(17, y17, EYE_GLINT)
    put(18, y17, EYE_PUPIL)
    put(19, y17, BORIS_SKIN.highlight)
    put(20, y17, BORIS_SKIN.outline)

    y18 = 18 + bob
    put(14, y18, BORIS_HAIR.shadow)
    put(15, y18, BORIS_SKIN.base)
    put(16, y18, BORIS_SKIN.highlight)
    put(17, y18, BORIS_SKIN.highlight)
    put(18, y18, BORIS_SKIN.highlight)
    put(19, y18, BORIS_SKIN.base)
    put(20, y18, BORIS_SKIN.outline)

    # Râu quai nón nhô trước cằm
    for y_rel, x0, x1 in [(19, 15, 20), (20, 15, 20), (21, 15, 19), (22, 16, 18)]:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if x == x0 or x == x1:
                put(x, y, BORIS_HAIR.outline)
            else:
                put(x, y, BORIS_HAIR.base if ((x + y) & 1) == 0 else BORIS_HAIR.shadow)

    # Cánh tay cơ bắp vung ra trước
    ya = 21 + bob
    put(15, ya, BORIS_SKIN.shadow)
    put(16, ya, BORIS_SKIN.highlight)
    put(15, ya + 1, BORIS_SKIN.outline)
    put(16, ya + 1, BORIS_SKIN.highlight)
    put(17, ya + 1, BORIS_SKIN.base)
    put(18, ya + 1, BORIS_SKIN.outline)
    put(17, ya + 2, BORIS_SKIN.outline)
    put(18, ya + 2, BORIS_SKIN.highlight)
    put(19, ya + 2, BORIS_SKIN.base)
    put(18, ya + 3, BORIS_SKIN.shadow)
    put(19, ya + 3, BORIS_SKIN.highlight)

    return img


def draw_boris_hammer(direction):
    """
    Búa rèn thép tôi (Prop_Hammer): Tay trái luôn cầm chắc búa rèn có độ bóng kim loại:
    - front: Tay trái Boris ở bên phải khung hình (X=20..21, Y=23..24) -> Búa tại X=21..25, Y=16..28.
    - back: Tay trái Boris ở bên trái khung hình (X=10..11, Y=23..24) -> Búa tại X=7..11, Y=16..28.
    - side (Right): Búa rèn thép bên hông xa X=14..18.
    - left (Left): Búa rèn thép vung mạnh phía trước X=19..23.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "front":
        hx, hy = 22, 17  # Đầu búa thép
        handle_x = 21
    elif direction == "back":
        hx, hy = 8, 17
        handle_x = 10
    elif direction == "side":
        hx, hy = 15, 17
        handle_x = 15
    else:  # left
        hx, hy = 21, 17
        handle_x = 20

    # Cán búa gỗ sồi (Y=18..28)
    for y in range(18, 29):
        put(img, handle_x - 1, y, HAMMER_WOOD.outline)
        put(img, handle_x, y, HAMMER_WOOD.highlight if y % 2 == 0 else HAMMER_WOOD.base)
        put(img, handle_x + 1, y, HAMMER_WOOD.shadow)
        put(img, handle_x + 2, y, HAMMER_WOOD.outline)

    # Đầu búa rèn thép tôi hình khối kim loại sắc nét (4x4 tại hx, hy)
    for dy in range(4):
        for dx in range(5):
            px, py = hx - 2 + dx, hy + dy
            if dx == 0 or dx == 4 or dy == 0 or dy == 3:
                put(img, px, py, HAMMER_STEEL.outline)
            elif dy == 1 and dx in (1, 2):
                put(img, px, py, HAMMER_STEEL.highlight)  # Điểm phản quang kim loại sắc nét
            elif dy == 1 or dx == 1:
                put(img, px, py, HAMMER_STEEL.base)
            else:
                put(img, px, py, HAMMER_STEEL.shadow)

    return img


# ==============================================================================
# 2. DƯỢC SĨ ELENA (NPC_Alchemist_Elena)
# ==============================================================================
def draw_elena_front_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, ELENA_BOOTS.outline)
        put(img, 12, y, ELENA_BOOTS.outline if y == 29 - leg_l_lift else ELENA_BOOTS.shadow)
        put(img, 13, y, ELENA_BOOTS.outline if y == 29 - leg_l_lift else ELENA_BOOTS.base)
        put(img, 14, y, ELENA_BOOTS.outline if y == 29 - leg_l_lift else ELENA_BOOTS.base)
        put(img, 15, y, ELENA_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, ELENA_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, ELENA_BOOTS.outline if y == 29 - leg_r_lift else ELENA_BOOTS.base)
        put(img, 18, y, ELENA_BOOTS.outline if y == 29 - leg_r_lift else ELENA_BOOTS.base)
        put(img, 19, y, ELENA_BOOTS.outline if y == 29 - leg_r_lift else ELENA_BOOTS.shadow)
        put(img, 20, y, ELENA_BOOTS.outline)

    # Áo choàng xanh bạc hà & Yếm trắng (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, ELENA_DRESS.outline)
            elif x in (14, 15, 16, 17) and y_rel >= 22:
                # Yếm trắng thắt nơ đan caro Dithering
                checker = ((x + y) & 1) == 0
                put(img, x, y, ELENA_APRON.highlight if checker else ELENA_APRON.base)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, ELENA_DRESS.highlight if (checker and x >= 15) else ELENA_DRESS.base)

    # 2 tay áo bạc hà thanh tú + khe nách 1px
    yl = 21 + bob
    put(img, 11, yl, ELENA_DRESS.shadow)
    put(img, 10, yl + 1, ELENA_DRESS.outline)
    put(img, 11, yl + 1, ELENA_DRESS.highlight)
    put(img, 10, yl + 2, ELENA_SKIN.highlight)
    put(img, 11, yl + 2, ELENA_SKIN.base)
    put(img, 12, yl + 2, ELENA_DRESS.outline)  # Khe nách 1px
    put(img, 10, yl + 3, ELENA_SKIN.shadow)
    put(img, 11, yl + 3, ELENA_SKIN.highlight)

    yr = 21 + bob
    put(img, 20, yr, ELENA_DRESS.shadow)
    put(img, 20, yr + 1, ELENA_DRESS.base)
    put(img, 21, yr + 1, ELENA_DRESS.outline)
    put(img, 19, yr + 2, ELENA_DRESS.outline)  # Khe nách 1px
    put(img, 20, yr + 2, ELENA_SKIN.base)
    put(img, 21, yr + 2, ELENA_SKIN.highlight)
    put(img, 20, yr + 3, ELENA_SKIN.highlight)
    put(img, 21, yr + 3, ELENA_SKIN.shadow)

    # Đầu to Chibi & Tóc hạt dẻ tết gọn (Y=8..17, X=10..21)
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
                put(img, x, y, ELENA_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, ELENA_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, ELENA_HAIR.highlight if checker else ELENA_HAIR.base)

    # Cài hoa xanh biếc bên tai phải (X=19..21, Y=14..16)
    y14 = 14 + bob
    put(img, 20, y14, ELENA_FLOWER.highlight)
    put(img, 19, y14 + 1, ELENA_FLOWER.base)
    put(img, 20, y14 + 1, (255, 240, 100, 255))  # Nhụy hoa vàng
    put(img, 21, y14 + 1, ELENA_FLOWER.shadow)
    put(img, 20, y14 + 2, ELENA_FLOWER.outline)

    # Mặt Chibi, má hồng, mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(img, 12, y17, ELENA_HAIR.outline)
    put(img, 13, y17, ELENA_HAIR.shadow)
    put(img, 14, y17, ELENA_SKIN.shadow)
    put(img, 15, y17, ELENA_SKIN.highlight)
    put(img, 16, y17, ELENA_SKIN.highlight)
    put(img, 17, y17, ELENA_SKIN.shadow)
    put(img, 18, y17, ELENA_HAIR.shadow)
    put(img, 19, y17, ELENA_HAIR.outline)

    y18 = 18 + bob
    put(img, 12, y18, ELENA_HAIR.outline)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, ELENA_SKIN.shadow)
    put(img, 15, y18, ELENA_SKIN.highlight)
    put(img, 16, y18, ELENA_SKIN.highlight)
    put(img, 17, y18, ELENA_SKIN.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, ELENA_HAIR.outline)

    y19 = 19 + bob
    put(img, 12, y19, ELENA_HAIR.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng mắt trái
    put(img, 14, y19, ELENA_SKIN.base)
    put(img, 15, y19, ELENA_SKIN.highlight)
    put(img, 16, y19, ELENA_SKIN.base)
    put(img, 17, y19, ELENA_SKIN.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng mắt phải
    put(img, 19, y19, ELENA_HAIR.outline)

    y20 = 20 + bob
    put(img, 12, y20, ELENA_HAIR.outline)
    put(img, 13, y20, (255, 170, 180, 255))  # Má hồng Chibi dễ thương
    put(img, 14, y20, ELENA_SKIN.highlight)
    put(img, 15, y20, ELENA_SKIN.highlight)
    put(img, 16, y20, ELENA_SKIN.highlight)
    put(img, 17, y20, ELENA_SKIN.highlight)
    put(img, 18, y20, (255, 170, 180, 255))  # Má hồng
    put(img, 19, y20, ELENA_HAIR.outline)

    return img


def draw_elena_back_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, ELENA_BOOTS.outline)
        put(img, 12, y, ELENA_BOOTS.outline if y == 29 - leg_l_lift else ELENA_BOOTS.shadow)
        put(img, 13, y, ELENA_BOOTS.outline if y == 29 - leg_l_lift else ELENA_BOOTS.base)
        put(img, 14, y, ELENA_BOOTS.outline if y == 29 - leg_l_lift else ELENA_BOOTS.shadow)
        put(img, 15, y, ELENA_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, ELENA_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, ELENA_BOOTS.outline if y == 29 - leg_r_lift else ELENA_BOOTS.shadow)
        put(img, 18, y, ELENA_BOOTS.outline if y == 29 - leg_r_lift else ELENA_BOOTS.base)
        put(img, 19, y, ELENA_BOOTS.outline if y == 29 - leg_r_lift else ELENA_BOOTS.shadow)
        put(img, 20, y, ELENA_BOOTS.outline)

    # Áo choàng bạc hà sau lưng & Nơ yếm trắng (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, ELENA_DRESS.outline)
            elif y_rel == 23 and x in (15, 16):
                put(img, x, y, ELENA_APRON.highlight)  # Nơ buộc yếm trắng sau lưng
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, ELENA_DRESS.base if checker else ELENA_DRESS.shadow)

    # 2 tay từ sau lưng
    yl = 21 + bob
    put(img, 11, yl, ELENA_DRESS.shadow)
    put(img, 10, yl + 1, ELENA_DRESS.outline)
    put(img, 11, yl + 1, ELENA_DRESS.base)
    put(img, 10, yl + 2, ELENA_SKIN.base)
    put(img, 11, yl + 2, ELENA_SKIN.shadow)
    put(img, 12, yl + 2, ELENA_DRESS.outline)
    put(img, 10, yl + 3, ELENA_SKIN.shadow)
    put(img, 11, yl + 3, ELENA_SKIN.base)

    yr = 21 + bob
    put(img, 20, yr, ELENA_DRESS.shadow)
    put(img, 20, yr + 1, ELENA_DRESS.base)
    put(img, 21, yr + 1, ELENA_DRESS.outline)
    put(img, 19, yr + 2, ELENA_DRESS.outline)
    put(img, 20, yr + 2, ELENA_SKIN.base)
    put(img, 21, yr + 2, ELENA_SKIN.highlight)
    put(img, 20, yr + 3, ELENA_SKIN.shadow)
    put(img, 21, yr + 3, ELENA_SKIN.base)

    # Tóc hạt dẻ tết sam dài sau lưng (Braid: Y=9..23, X=13..18)
    back_hair_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 11, 20),
        (16, 12, 19),
        (17, 13, 18),
        (18, 14, 17),
        (19, 14, 17),
        (20, 14, 17),
        (21, 15, 16),
        (22, 15, 16),
    ]
    for y_rel, x0, x1 in back_hair_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel in (9, 22) or x == x0 or x == x1:
                put(img, x, y, ELENA_HAIR.outline)
            elif (y_rel % 2 == 0 and x == 15) or (y_rel % 2 != 0 and x == 16):
                # Rãnh tết tóc sam
                put(img, x, y, ELENA_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, ELENA_HAIR.highlight if checker else ELENA_HAIR.base)

    # Cài hoa xanh nhìn từ sau (X=20, Y=14)
    put(img, 20, 14 + bob, ELENA_FLOWER.highlight)
    put(img, 21, 14 + bob, ELENA_FLOWER.base)

    return img


def draw_elena_side_body(step=0, facing_left=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2 Chân bước
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
                    put(x, y, ELENA_BOOTS.outline)
                else:
                    put(x, y, ELENA_BOOTS.highlight if (is_near and y == y0 + 1) else (ELENA_BOOTS.base if is_near else ELENA_BOOTS.shadow))

    # Thân nhìn ngang (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, ELENA_DRESS.outline)
            elif x >= 17:
                put(x, y, ELENA_APRON.highlight if ((x + y) & 1) == 0 else ELENA_APRON.base)
            else:
                put(x, y, ELENA_DRESS.highlight if ((x + y) & 1) == 0 else ELENA_DRESS.base)

    # Đầu & Tóc hạt dẻ tết gọn nhìn ngang
    side_head_spans = [
        (9, 12, 18),
        (10, 11, 19),
        (11, 10, 20),
        (12, 10, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 19),
    ]
    for y_rel, x0, x1 in side_head_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(x, y, ELENA_HAIR.outline)
            elif x == x0 + 1:
                put(x, y, ELENA_HAIR.shadow)
            else:
                put(x, y, ELENA_HAIR.highlight if ((x + y) & 1) == 0 else ELENA_HAIR.base)

    # Đuôi sam dài rủ sau lưng
    for y_rel, x0, x1 in [(16, 9, 13), (17, 9, 12), (18, 9, 12), (19, 10, 12), (20, 10, 12)]:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            put(x, y, ELENA_HAIR.outline if (x == x0 or x == x1 or y_rel == 20) else ELENA_HAIR.base)

    # Cài hoa xanh nhìn nghiêng
    put(17, 14 + bob, ELENA_FLOWER.highlight)
    put(18, 14 + bob, (255, 240, 100, 255))
    put(17, 15 + bob, ELENA_FLOWER.base)

    # Mặt nghiêng & Mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(15, y17, ELENA_SKIN.shadow)
    put(16, y17, ELENA_SKIN.base)
    put(17, y17, EYE_GLINT)
    put(18, y17, EYE_PUPIL)
    put(19, y17, ELENA_SKIN.highlight)
    put(20, y17, ELENA_SKIN.outline)

    y18 = 18 + bob
    put(15, y18, ELENA_SKIN.base)
    put(16, y18, ELENA_SKIN.highlight)
    put(17, y18, (255, 170, 180, 255))  # Má hồng Chibi
    put(18, y18, ELENA_SKIN.highlight)
    put(19, y18, ELENA_SKIN.base)
    put(20, y18, ELENA_SKIN.outline)

    # Tay áo thanh tú
    ya = 21 + bob
    put(15, ya, ELENA_DRESS.shadow)
    put(16, ya, ELENA_DRESS.highlight)
    put(15, ya + 1, ELENA_DRESS.outline)
    put(16, ya + 1, ELENA_DRESS.highlight)
    put(17, ya + 1, ELENA_DRESS.base)
    put(18, ya + 1, ELENA_DRESS.outline)
    put(17, ya + 2, ELENA_DRESS.outline)
    put(18, ya + 2, ELENA_SKIN.highlight)
    put(19, ya + 2, ELENA_SKIN.base)
    put(18, ya + 3, ELENA_SKIN.shadow)
    put(19, ya + 3, ELENA_SKIN.highlight)

    return img


def draw_elena_herb_bag(direction):
    """
    Túi thuốc đeo chéo & Bình độc dược huỳnh quang (Prop_HerbBag):
    - front: Dây da đeo chéo từ vai trái qua hông phải (X=17..20, Y=22..25).
      Gồm túi da thuộc + bình tím huỳnh quang + bình xanh cyan lấp lánh bên hông.
    - back: Dây da vắt sau lưng X=13..18.
    - side (Right): Túi thuốc lộ rõ ở sườn hông X=16..19.
    - left (Left): Túi thuốc lộ nhẹ sau lưng X=12..15.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "front":
        # Dây đeo chéo
        for i in range(5):
            put(img, 13 + i, 20 + i // 2, ELENA_BAG.outline if i % 2 == 0 else ELENA_BAG.highlight)
        # Túi da thuộc (X=17..20, Y=22..25)
        for y in range(22, 26):
            for x in range(17, 21):
                if x == 17 or x == 20 or y == 22 or y == 25:
                    put(img, x, y, ELENA_BAG.outline)
                elif x == 18 and y == 23:
                    put(img, x, y, (255, 215, 60, 255))  # Khóa đồng vàng óng
                else:
                    put(img, x, y, ELENA_BAG.highlight if ((x + y) & 1) == 0 else ELENA_BAG.base)

        # Bình độc dược tím huỳnh quang treo bên hông túi (X=21..22, Y=23..25)
        put(img, 21, 23, ELENA_POTION_PURPLE.outline)
        put(img, 22, 23, (240, 240, 240, 255))  # Nút bấc
        put(img, 21, 24, ELENA_POTION_PURPLE.highlight)
        put(img, 22, 24, ELENA_POTION_PURPLE.base)
        put(img, 21, 25, ELENA_POTION_PURPLE.shadow)
        put(img, 22, 25, ELENA_POTION_PURPLE.outline)

        # Bình độc dược xanh cyan huỳnh quang treo trước túi (X=16, Y=23..25)
        put(img, 16, 23, (240, 240, 240, 255))  # Nút bấc
        put(img, 16, 24, ELENA_POTION_CYAN.highlight)
        put(img, 16, 25, ELENA_POTION_CYAN.base)

    elif direction == "back":
        # Dây da đeo chéo sau lưng
        for i in range(6):
            put(img, 18 - i, 20 + i // 2, ELENA_BAG.outline if i % 2 == 0 else ELENA_BAG.base)
        # Hông túi peeking phía sau
        for y in range(23, 26):
            put(img, 19, y, ELENA_BAG.base)
            put(img, 20, y, ELENA_BAG.outline)

    elif direction == "side":
        # Túi da lộ rõ bên hông nhìn ngang (X=16..20, Y=22..26)
        for y in range(22, 26):
            for x in range(16, 20):
                if x == 16 or x == 19 or y == 22 or y == 25:
                    put(img, x, y, ELENA_BAG.outline)
                elif x == 17 and y == 23:
                    put(img, x, y, (255, 215, 60, 255))
                else:
                    put(img, x, y, ELENA_BAG.highlight if ((x + y) & 1) == 0 else ELENA_BAG.base)
        # Lọ độc dược huỳnh quang
        put(img, 19, 23, ELENA_POTION_PURPLE.highlight)
        put(img, 20, 23, (240, 240, 240, 255))
        put(img, 19, 24, ELENA_POTION_PURPLE.base)
        put(img, 20, 24, ELENA_POTION_PURPLE.outline)

    else:  # left
        # Túi da ở hông xa
        for y in range(23, 26):
            put(img, 12, y, ELENA_BAG.outline)
            put(img, 13, y, ELENA_BAG.base)
            put(img, 14, y, ELENA_BAG.outline)

    return img


# ==============================================================================
# 3. HỘI TRƯỞNG CARLOS (NPC_GuildMaster_Carlos)
# ==============================================================================
def draw_carlos_front_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots / Greaves
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, CARLOS_BOOTS.outline)
        put(img, 12, y, CARLOS_BOOTS.outline if y == 29 - leg_l_lift else CARLOS_BOOTS.shadow)
        put(img, 13, y, CARLOS_BOOTS.outline if y == 29 - leg_l_lift else CARLOS_BOOTS.base)
        put(img, 14, y, CARLOS_BOOTS.outline if y == 29 - leg_l_lift else CARLOS_BOOTS.base)
        put(img, 15, y, CARLOS_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, CARLOS_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, CARLOS_BOOTS.outline if y == 29 - leg_r_lift else CARLOS_BOOTS.base)
        put(img, 18, y, CARLOS_BOOTS.outline if y == 29 - leg_r_lift else CARLOS_BOOTS.base)
        put(img, 19, y, CARLOS_BOOTS.outline if y == 29 - leg_r_lift else CARLOS_BOOTS.shadow)
        put(img, 20, y, CARLOS_BOOTS.outline)

    # Giáp ngực bạc sáng bóng (Metallic Highlight: #FFFFFF) & Đai hiệp sĩ (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, CARLOS_ARMOR.outline)
            elif y_rel == 25:
                # Viền đai da hiệp sĩ
                put(img, x, y, CARLOS_BOOTS.shadow if x in (13, 18) else CARLOS_BOOTS.base)
            elif y_rel == 24:
                # Thắt lưng & khóa mạ vàng
                put(img, x, y, CARLOS_SWORD_GOLD.highlight if x in (15, 16) else CARLOS_BOOTS.shadow)
            elif y_rel == 22 and x in (14, 15, 16):
                # Điểm chói sáng kim loại cực mạnh (Metallic specular shine)
                put(img, x, y, CARLOS_ARMOR.highlight)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, CARLOS_ARMOR.highlight if (checker and x >= 14) else CARLOS_ARMOR.base)

    # 2 tay giáp bạc + khe nách 1px
    # Tay trái (viewer's left: X=10..11) - đặt lên chuôi kiếm bên hông
    yl = 21 + bob
    put(img, 11, yl, CARLOS_ARMOR.shadow)
    put(img, 10, yl + 1, CARLOS_ARMOR.outline)
    put(img, 11, yl + 1, CARLOS_ARMOR.highlight)
    put(img, 10, yl + 2, CARLOS_SKIN.highlight)
    put(img, 11, yl + 2, CARLOS_SKIN.base)
    put(img, 12, yl + 2, CARLOS_ARMOR.outline)  # Khe nách 1px
    put(img, 10, yl + 3, CARLOS_SKIN.shadow)
    put(img, 11, yl + 3, CARLOS_SKIN.highlight)

    # Tay phải (viewer's right: X=20..21)
    yr = 21 + bob
    put(img, 20, yr, CARLOS_ARMOR.shadow)
    put(img, 20, yr + 1, CARLOS_ARMOR.base)
    put(img, 21, yr + 1, CARLOS_ARMOR.outline)
    put(img, 19, yr + 2, CARLOS_ARMOR.outline)  # Khe nách 1px
    put(img, 20, yr + 2, CARLOS_SKIN.base)
    put(img, 21, yr + 2, CARLOS_SKIN.highlight)
    put(img, 20, yr + 3, CARLOS_SKIN.highlight)
    put(img, 21, yr + 3, CARLOS_SKIN.shadow)

    # Đầu to Chibi & Tóc nâu vàng lãng tử (Y=8..17, X=10..21)
    put(img, 18, 8 + bob, CARLOS_HAIR.highlight)  # Lọn tóc vểnh phía trên
    put(img, 19, 8 + bob, CARLOS_HAIR.outline)
    crown_spans = [
        (9, 13, 19),
        (10, 12, 20),
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
                put(img, x, y, CARLOS_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, CARLOS_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, CARLOS_HAIR.highlight if checker else CARLOS_HAIR.base)

    # Mặt Chibi, chân mày cương nghị, mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(img, 12, y17, CARLOS_HAIR.outline)
    put(img, 13, y17, CARLOS_HAIR.shadow)
    put(img, 14, y17, CARLOS_HAIR.base)
    put(img, 15, y17, CARLOS_SKIN.shadow)
    put(img, 16, y17, CARLOS_SKIN.shadow)
    put(img, 17, y17, CARLOS_HAIR.base)
    put(img, 18, y17, CARLOS_HAIR.shadow)
    put(img, 19, y17, CARLOS_HAIR.outline)

    y18 = 18 + bob
    put(img, 12, y18, CARLOS_HAIR.outline)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, CARLOS_SKIN.shadow)
    put(img, 15, y18, CARLOS_SKIN.highlight)
    put(img, 16, y18, CARLOS_SKIN.highlight)
    put(img, 17, y18, CARLOS_SKIN.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, CARLOS_HAIR.outline)

    y19 = 19 + bob
    put(img, 12, y19, CARLOS_HAIR.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng mắt trái
    put(img, 14, y19, CARLOS_SKIN.base)
    put(img, 15, y19, CARLOS_SKIN.highlight)
    put(img, 16, y19, CARLOS_SKIN.base)
    put(img, 17, y19, CARLOS_SKIN.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng mắt phải
    put(img, 19, y19, CARLOS_HAIR.outline)

    y20 = 20 + bob
    put(img, 12, y20, CARLOS_HAIR.outline)
    put(img, 13, y20, CARLOS_SKIN.base)
    put(img, 14, y20, CARLOS_SKIN.highlight)
    put(img, 15, y20, CARLOS_SKIN.highlight)
    put(img, 16, y20, CARLOS_SKIN.highlight)
    put(img, 17, y20, CARLOS_SKIN.highlight)
    put(img, 18, y20, CARLOS_SKIN.base)
    put(img, 19, y20, CARLOS_HAIR.outline)

    return img


def draw_carlos_back_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, CARLOS_BOOTS.outline)
        put(img, 12, y, CARLOS_BOOTS.outline if y == 29 - leg_l_lift else CARLOS_BOOTS.shadow)
        put(img, 13, y, CARLOS_BOOTS.outline if y == 29 - leg_l_lift else CARLOS_BOOTS.base)
        put(img, 14, y, CARLOS_BOOTS.outline if y == 29 - leg_l_lift else CARLOS_BOOTS.shadow)
        put(img, 15, y, CARLOS_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, CARLOS_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, CARLOS_BOOTS.outline if y == 29 - leg_r_lift else CARLOS_BOOTS.shadow)
        put(img, 18, y, CARLOS_BOOTS.outline if y == 29 - leg_r_lift else CARLOS_BOOTS.base)
        put(img, 19, y, CARLOS_BOOTS.outline if y == 29 - leg_r_lift else CARLOS_BOOTS.shadow)
        put(img, 20, y, CARLOS_BOOTS.outline)

    # Lưng giáp bạc (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, CARLOS_ARMOR.outline)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, CARLOS_ARMOR.base if checker else CARLOS_ARMOR.shadow)

    # 2 tay từ sau lưng
    yl = 21 + bob
    put(img, 11, yl, CARLOS_ARMOR.shadow)
    put(img, 10, yl + 1, CARLOS_ARMOR.outline)
    put(img, 11, yl + 1, CARLOS_ARMOR.base)
    put(img, 10, yl + 2, CARLOS_SKIN.base)
    put(img, 11, yl + 2, CARLOS_SKIN.shadow)
    put(img, 12, yl + 2, CARLOS_ARMOR.outline)
    put(img, 10, yl + 3, CARLOS_SKIN.shadow)
    put(img, 11, yl + 3, CARLOS_SKIN.base)

    yr = 21 + bob
    put(img, 20, yr, CARLOS_ARMOR.shadow)
    put(img, 20, yr + 1, CARLOS_ARMOR.base)
    put(img, 21, yr + 1, CARLOS_ARMOR.outline)
    put(img, 19, yr + 2, CARLOS_ARMOR.outline)
    put(img, 20, yr + 2, CARLOS_SKIN.base)
    put(img, 21, yr + 2, CARLOS_SKIN.highlight)
    put(img, 20, yr + 3, CARLOS_SKIN.shadow)
    put(img, 21, yr + 3, CARLOS_SKIN.base)

    # Tóc nâu vàng sau gáy
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
                put(img, x, y, CARLOS_HAIR.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, CARLOS_HAIR.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, CARLOS_HAIR.highlight if checker else CARLOS_HAIR.base)

    return img


def draw_carlos_side_body(step=0, facing_left=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2 Chân bước
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
                    put(x, y, CARLOS_BOOTS.outline)
                else:
                    put(x, y, CARLOS_BOOTS.highlight if (is_near and y == y0 + 1) else (CARLOS_BOOTS.base if is_near else CARLOS_BOOTS.shadow))

    # Giáp ngực bạc nhìn ngang
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, CARLOS_ARMOR.outline)
            elif x in (16, 17) and y_rel == 22:
                put(x, y, CARLOS_ARMOR.highlight)  # Phản quang kim loại
            else:
                put(x, y, CARLOS_ARMOR.base if ((x + y) & 1) == 0 else CARLOS_ARMOR.shadow)

    # Đầu & Tóc nâu vàng nhìn ngang
    side_head_spans = [
        (9, 12, 18),
        (10, 11, 19),
        (11, 10, 20),
        (12, 10, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 19),
    ]
    for y_rel, x0, x1 in side_head_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(x, y, CARLOS_HAIR.outline)
            elif x == x0 + 1:
                put(x, y, CARLOS_HAIR.shadow)
            else:
                put(x, y, CARLOS_HAIR.highlight if ((x + y) & 1) == 0 else CARLOS_HAIR.base)

    # Mặt nghiêng & Mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(15, y17, CARLOS_SKIN.shadow)
    put(16, y17, CARLOS_SKIN.base)
    put(17, y17, EYE_GLINT)
    put(18, y17, EYE_PUPIL)
    put(19, y17, CARLOS_SKIN.highlight)
    put(20, y17, CARLOS_SKIN.outline)

    y18 = 18 + bob
    put(15, y18, CARLOS_SKIN.base)
    put(16, y18, CARLOS_SKIN.highlight)
    put(17, y18, CARLOS_SKIN.highlight)
    put(18, y18, CARLOS_SKIN.highlight)
    put(19, y18, CARLOS_SKIN.base)
    put(20, y18, CARLOS_SKIN.outline)

    # Tay đặt lên chuôi kiếm bên sườn
    ya = 21 + bob
    put(15, ya, CARLOS_ARMOR.shadow)
    put(16, ya, CARLOS_ARMOR.highlight)
    put(15, ya + 1, CARLOS_ARMOR.outline)
    put(16, ya + 1, CARLOS_ARMOR.highlight)
    put(17, ya + 1, CARLOS_ARMOR.base)
    put(18, ya + 1, CARLOS_ARMOR.outline)
    put(17, ya + 2, CARLOS_ARMOR.outline)
    put(18, ya + 2, CARLOS_SKIN.highlight)
    put(19, ya + 2, CARLOS_SKIN.base)
    put(18, ya + 3, CARLOS_SKIN.shadow)
    put(19, ya + 3, CARLOS_SKIN.highlight)

    return img


def draw_carlos_sword(direction):
    """
    Kiếm hiệp sĩ chuôi mạ vàng (Prop_KnightSword):
    - front: Kiếm đeo bên hông trái (viewer's left: X=9..12, Y=21..28). Chuôi mạ vàng Y=21..23, lưỡi thép Y=24..28.
    - back: Kiếm peeking bên hông trái (viewer's right: X=19..22).
    - side: Kiếm ở hông xa X=13..16.
    - left: Kiếm ở hông gần X=16..19.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "front":
        sx, sy = 10, 21
    elif direction == "back":
        sx, sy = 20, 21
    elif direction == "side":
        sx, sy = 14, 21
    else:  # left
        sx, sy = 17, 21

    # Chuôi kiếm mạ vàng & Quả táo chuôi (Pommel & Crossguard)
    put(img, sx, sy, CARLOS_SWORD_GOLD.highlight)
    put(img, sx + 1, sy, CARLOS_SWORD_GOLD.outline)
    put(img, sx - 1, sy + 1, CARLOS_SWORD_GOLD.outline)
    put(img, sx, sy + 1, CARLOS_SWORD_GOLD.highlight)
    put(img, sx + 1, sy + 1, CARLOS_SWORD_GOLD.base)
    put(img, sx + 2, sy + 1, CARLOS_SWORD_GOLD.outline)

    # Vỏ kiếm / Lưỡi thép tôi sắc lẹm (Y=sy+2..sy+7)
    for y in range(sy + 2, sy + 8):
        put(img, sx - 1, y, CARLOS_SWORD_STEEL.outline)
        put(img, sx, y, CARLOS_SWORD_STEEL.highlight if y % 2 == 0 else CARLOS_SWORD_STEEL.base)
        put(img, sx + 1, y, CARLOS_SWORD_STEEL.shadow)
        put(img, sx + 2, y, CARLOS_SWORD_STEEL.outline)

    return img


def draw_carlos_cape(direction):
    """
    Áo choàng đỏ thẫm bay nhẹ (Prop_CrimsonCape):
    - front: Áo choàng đỏ peeking hai bên vai và sau lưng (X=10..21, Y=22..27).
    - back: Áo choàng phủ trọn lưng Y=20..27.
    - side: Áo choàng bay về sau (X=9..13, Y=21..27).
    - left: Áo choàng bay về sau (X=18..22, Y=21..27).
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "front":
        # Áo choàng lộ ở gấu và sườn hai bên
        for y in range(23, 27):
            put(img, 11, y, CARLOS_CAPE.outline)
            put(img, 12, y, CARLOS_CAPE.base)
            put(img, 19, y, CARLOS_CAPE.base)
            put(img, 20, y, CARLOS_CAPE.outline)

    elif direction == "back":
        # Áo choàng che kín lưng Y=21..27
        for y in range(21, 28):
            for x in range(11, 21):
                if x == 11 or x == 20 or y == 27:
                    put(img, x, y, CARLOS_CAPE.outline)
                elif x in (14, 15, 16):
                    put(img, x, y, CARLOS_CAPE.highlight if ((x + y) & 1) == 0 else CARLOS_CAPE.base)
                else:
                    put(img, x, y, CARLOS_CAPE.shadow)

    elif direction == "side":
        # Bay về phía sau bên trái
        for y in range(22, 28):
            for x in range(9, 14):
                if x == 9 or y == 27:
                    put(img, x, y, CARLOS_CAPE.outline)
                else:
                    put(img, x, y, CARLOS_CAPE.base if ((x + y) & 1) == 0 else CARLOS_CAPE.shadow)

    else:  # left
        # Bay về phía sau bên phải
        for y in range(22, 28):
            for x in range(18, 23):
                if x == 22 or y == 27:
                    put(img, x, y, CARLOS_CAPE.outline)
                else:
                    put(img, x, y, CARLOS_CAPE.base if ((x + y) & 1) == 0 else CARLOS_CAPE.shadow)

    return img


# ==============================================================================
# 4. THƯƠNG NHÂN LINA (NPC_Merchant_Lina)
# ==============================================================================
def draw_lina_front_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, LINA_BOOTS.outline)
        put(img, 12, y, LINA_BOOTS.outline if y == 29 - leg_l_lift else LINA_BOOTS.shadow)
        put(img, 13, y, LINA_BOOTS.outline if y == 29 - leg_l_lift else LINA_BOOTS.base)
        put(img, 14, y, LINA_BOOTS.outline if y == 29 - leg_l_lift else LINA_BOOTS.base)
        put(img, 15, y, LINA_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, LINA_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, LINA_BOOTS.outline if y == 29 - leg_r_lift else LINA_BOOTS.base)
        put(img, 18, y, LINA_BOOTS.outline if y == 29 - leg_r_lift else LINA_BOOTS.base)
        put(img, 19, y, LINA_BOOTS.outline if y == 29 - leg_r_lift else LINA_BOOTS.shadow)
        put(img, 20, y, LINA_BOOTS.outline)

    # Váy yếm cam đất & Tạp dề trắng (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, LINA_DRESS.outline)
            elif x in (14, 15, 16, 17) and y_rel >= 22:
                # Tạp dề trắng xinh xắn
                checker = ((x + y) & 1) == 0
                put(img, x, y, LINA_APRON.highlight if checker else LINA_APRON.base)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, LINA_DRESS.highlight if (checker and x >= 15) else LINA_DRESS.base)

    # 2 tay áo cam đất + khe nách 1px
    yl = 21 + bob
    put(img, 11, yl, LINA_DRESS.shadow)
    put(img, 10, yl + 1, LINA_DRESS.outline)
    put(img, 11, yl + 1, LINA_DRESS.highlight)
    put(img, 10, yl + 2, LINA_SKIN.highlight)
    put(img, 11, yl + 2, LINA_SKIN.base)
    put(img, 12, yl + 2, LINA_DRESS.outline)  # Khe nách 1px
    put(img, 10, yl + 3, LINA_SKIN.shadow)
    put(img, 11, yl + 3, LINA_SKIN.highlight)

    yr = 21 + bob
    put(img, 20, yr, LINA_DRESS.shadow)
    put(img, 20, yr + 1, LINA_DRESS.base)
    put(img, 21, yr + 1, LINA_DRESS.outline)
    put(img, 19, yr + 2, LINA_DRESS.outline)  # Khe nách 1px
    put(img, 20, yr + 2, LINA_SKIN.base)
    put(img, 21, yr + 2, LINA_SKIN.highlight)
    put(img, 20, yr + 3, LINA_SKIN.highlight)
    put(img, 21, yr + 3, LINA_SKIN.shadow)

    # Đầu to Chibi & Khăn trùm đầu vàng cam (Y=8..17, X=10..21)
    put(img, 18, 8 + bob, LINA_SCARF.highlight)  # Nơ buộc khăn trên đỉnh
    put(img, 19, 8 + bob, LINA_SCARF.outline)
    crown_spans = [
        (9, 13, 19),
        (10, 12, 20),
        (11, 11, 20),
        (12, 10, 21),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 11, 20),
    ]
    for y_rel, x0, x1 in crown_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(img, x, y, LINA_SCARF.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, LINA_SCARF.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, LINA_SCARF.highlight if checker else LINA_SCARF.base)

    # Lọn tóc nâu hạt dẻ ló ra dưới mép khăn trùm (Y=16..17)
    y16 = 16 + bob
    put(img, 13, y16, LINA_HAIR.base)
    put(img, 14, y16, LINA_HAIR.highlight)
    put(img, 17, y16, LINA_HAIR.highlight)
    put(img, 18, y16, LINA_HAIR.base)

    # Mặt Chibi, má hồng, mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(img, 12, y17, LINA_SCARF.outline)
    put(img, 13, y17, LINA_HAIR.shadow)
    put(img, 14, y17, LINA_SKIN.shadow)
    put(img, 15, y17, LINA_SKIN.highlight)
    put(img, 16, y17, LINA_SKIN.highlight)
    put(img, 17, y17, LINA_SKIN.shadow)
    put(img, 18, y17, LINA_HAIR.shadow)
    put(img, 19, y17, LINA_SCARF.outline)

    y18 = 18 + bob
    put(img, 12, y18, LINA_SCARF.outline)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, LINA_SKIN.shadow)
    put(img, 15, y18, LINA_SKIN.highlight)
    put(img, 16, y18, LINA_SKIN.highlight)
    put(img, 17, y18, LINA_SKIN.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, LINA_SCARF.outline)

    y19 = 19 + bob
    put(img, 12, y19, LINA_SCARF.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng mắt trái
    put(img, 14, y19, LINA_SKIN.base)
    put(img, 15, y19, LINA_SKIN.highlight)
    put(img, 16, y19, LINA_SKIN.base)
    put(img, 17, y19, LINA_SKIN.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng mắt phải
    put(img, 19, y19, LINA_SCARF.outline)

    y20 = 20 + bob
    put(img, 12, y20, LINA_SCARF.outline)
    put(img, 13, y20, (255, 170, 180, 255))  # Má hồng
    put(img, 14, y20, LINA_SKIN.highlight)
    put(img, 15, y20, LINA_SKIN.highlight)
    put(img, 16, y20, LINA_SKIN.highlight)
    put(img, 17, y20, LINA_SKIN.highlight)
    put(img, 18, y20, (255, 170, 180, 255))  # Má hồng
    put(img, 19, y20, LINA_SCARF.outline)

    return img


def draw_lina_back_body(step=0):
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px
    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # Boots
    for y in range(26 - leg_l_lift, 30 - leg_l_lift):
        put(img, 11, y, LINA_BOOTS.outline)
        put(img, 12, y, LINA_BOOTS.outline if y == 29 - leg_l_lift else LINA_BOOTS.shadow)
        put(img, 13, y, LINA_BOOTS.outline if y == 29 - leg_l_lift else LINA_BOOTS.base)
        put(img, 14, y, LINA_BOOTS.outline if y == 29 - leg_l_lift else LINA_BOOTS.shadow)
        put(img, 15, y, LINA_BOOTS.outline if y < 29 - leg_l_lift else (0, 0, 0, 0))

    for y in range(26 - leg_r_lift, 30 - leg_r_lift):
        put(img, 16, y, LINA_BOOTS.outline if y < 29 - leg_r_lift else (0, 0, 0, 0))
        put(img, 17, y, LINA_BOOTS.outline if y == 29 - leg_r_lift else LINA_BOOTS.shadow)
        put(img, 18, y, LINA_BOOTS.outline if y == 29 - leg_r_lift else LINA_BOOTS.base)
        put(img, 19, y, LINA_BOOTS.outline if y == 29 - leg_r_lift else LINA_BOOTS.shadow)
        put(img, 20, y, LINA_BOOTS.outline)

    # Lưng váy cam & Nơ tạp dề (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, LINA_DRESS.outline)
            elif y_rel == 23 and x in (15, 16):
                put(img, x, y, LINA_APRON.highlight)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, LINA_DRESS.base if checker else LINA_DRESS.shadow)

    # 2 tay từ sau lưng
    yl = 21 + bob
    put(img, 11, yl, LINA_DRESS.shadow)
    put(img, 10, yl + 1, LINA_DRESS.outline)
    put(img, 11, yl + 1, LINA_DRESS.base)
    put(img, 10, yl + 2, LINA_SKIN.base)
    put(img, 11, yl + 2, LINA_SKIN.shadow)
    put(img, 12, yl + 2, LINA_DRESS.outline)
    put(img, 10, yl + 3, LINA_SKIN.shadow)
    put(img, 11, yl + 3, LINA_SKIN.base)

    yr = 21 + bob
    put(img, 20, yr, LINA_DRESS.shadow)
    put(img, 20, yr + 1, LINA_DRESS.base)
    put(img, 21, yr + 1, LINA_DRESS.outline)
    put(img, 19, yr + 2, LINA_DRESS.outline)
    put(img, 20, yr + 2, LINA_SKIN.base)
    put(img, 21, yr + 2, LINA_SKIN.highlight)
    put(img, 20, yr + 3, LINA_SKIN.shadow)
    put(img, 21, yr + 3, LINA_SKIN.base)

    # Khăn trùm đầu vàng cam sau gáy
    back_scarf_spans = [
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
        (19, 14, 17),
    ]
    for y_rel, x0, x1 in back_scarf_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel in (9, 19) or x == x0 or x == x1:
                put(img, x, y, LINA_SCARF.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, LINA_SCARF.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(img, x, y, LINA_SCARF.highlight if checker else LINA_SCARF.base)

    return img


def draw_lina_side_body(step=0, facing_left=False):
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2 Chân bước
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
                    put(x, y, LINA_BOOTS.outline)
                else:
                    put(x, y, LINA_BOOTS.highlight if (is_near and y == y0 + 1) else (LINA_BOOTS.base if is_near else LINA_BOOTS.shadow))

    # Váy nhìn ngang
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, LINA_DRESS.outline)
            elif x >= 17:
                put(x, y, LINA_APRON.highlight if ((x + y) & 1) == 0 else LINA_APRON.base)
            else:
                put(x, y, LINA_DRESS.highlight if ((x + y) & 1) == 0 else LINA_DRESS.base)

    # Khăn trùm đầu vàng cam nhìn ngang
    side_scarf_spans = [
        (9, 12, 18),
        (10, 11, 19),
        (11, 10, 20),
        (12, 10, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 19),
    ]
    for y_rel, x0, x1 in side_scarf_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(x, y, LINA_SCARF.outline)
            elif x == x0 + 1:
                put(x, y, LINA_SCARF.shadow)
            else:
                put(x, y, LINA_SCARF.highlight if ((x + y) & 1) == 0 else LINA_SCARF.base)

    # Mặt nghiêng & Mắt có điểm sáng trắng #F7EFEB
    y17 = 17 + bob
    put(15, y17, LINA_SKIN.shadow)
    put(16, y17, LINA_SKIN.base)
    put(17, y17, EYE_GLINT)
    put(18, y17, EYE_PUPIL)
    put(19, y17, LINA_SKIN.highlight)
    put(20, y17, LINA_SKIN.outline)

    y18 = 18 + bob
    put(15, y18, LINA_SKIN.base)
    put(16, y18, LINA_SKIN.highlight)
    put(17, y18, (255, 170, 180, 255))
    put(18, y18, LINA_SKIN.highlight)
    put(19, y18, LINA_SKIN.base)
    put(20, y18, LINA_SKIN.outline)

    # Tay xách giỏ đưa ra trước
    ya = 21 + bob
    put(15, ya, LINA_DRESS.shadow)
    put(16, ya, LINA_DRESS.highlight)
    put(15, ya + 1, LINA_DRESS.outline)
    put(16, ya + 1, LINA_DRESS.highlight)
    put(17, ya + 1, LINA_DRESS.base)
    put(18, ya + 1, LINA_DRESS.outline)
    put(17, ya + 2, LINA_DRESS.outline)
    put(18, ya + 2, LINA_SKIN.highlight)
    put(19, ya + 2, LINA_SKIN.base)
    put(18, ya + 3, LINA_SKIN.shadow)
    put(19, ya + 3, LINA_SKIN.highlight)

    return img


def draw_lina_basket(direction):
    """
    Giỏ hàng mây đan (Prop_Basket):
    - front: Xách bên tay phải (viewer's right: X=20..24, Y=22..27). Có quai giỏ + vân mây đan chéo + trái cây/bánh mì.
    - back: Quai giỏ peeking sau lưng X=9..13.
    - side: Giỏ mây đan xách trước người X=19..23.
    - left: Giỏ mây đan ở phía sau X=9..13.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "front":
        bx, by = 21, 23
    elif direction == "back":
        bx, by = 9, 23
    elif direction == "side":
        bx, by = 20, 23
    else:  # left
        bx, by = 10, 23

    # Quai giỏ mây (Arch Y=by-2..by)
    put(img, bx + 1, by - 2, LINA_BASKET.highlight)
    put(img, bx, by - 1, LINA_BASKET.base)
    put(img, bx + 2, by - 1, LINA_BASKET.shadow)

    # Thân giỏ mây đan hình chữ nhật bo góc (4x4)
    for dy in range(4):
        for dx in range(4):
            px, py = bx - 1 + dx, by + dy
            if dx == 0 or dx == 3 or dy == 3:
                put(img, px, py, LINA_BASKET.outline)
            elif dy == 0:
                # Đựng bánh mì vàng óng & Táo đỏ tươi
                if dx == 1:
                    put(img, px, py, (250, 60, 60, 255))  # Táo đỏ
                else:
                    put(img, px, py, (255, 210, 110, 255))  # Bánh mì
            else:
                checker = ((px + py) & 1) == 0
                put(img, px, py, LINA_BASKET.highlight if checker else LINA_BASKET.base)

    return img


def draw_lina_coin_pouch(direction):
    """
    Túi tiền vàng & Sổ ghi chép ở thắt lưng (Prop_CoinPouch):
    - front: Đeo bên hông trái (viewer's left: X=11..13, Y=22..25). Gồm túi tiền nhung vàng + sổ da nhỏ.
    - back: Peeking bên hông phải X=18..20.
    - side: Peeking phía xa X=14..16.
    - left: Hiển thị rõ bên hông nhìn ngang X=16..18.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    if direction == "front":
        px, py = 12, 23
    elif direction == "back":
        px, py = 19, 23
    elif direction == "side":
        px, py = 15, 23
    else:  # left
        px, py = 17, 23

    # Túi tiền vàng nhung (3x3)
    put(img, px, py - 1, (240, 240, 240, 255))  # Dây thít túi
    for dy in range(3):
        for dx in range(3):
            rx, ry = px - 1 + dx, py + dy
            if dx == 0 or dx == 2 or dy == 2:
                put(img, rx, ry, LINA_POUCH.outline)
            else:
                put(img, rx, ry, LINA_POUCH.highlight if dx == 1 and dy == 0 else LINA_POUCH.base)

    # Sổ ghi chép nhỏ bìa da nâu cạnh túi
    put(img, px - 2, py, LINA_BOOTS.outline)
    put(img, px - 2, py + 1, (245, 248, 255, 255))  # Mép trang giấy trắng
    put(img, px - 2, py + 2, LINA_BOOTS.base)

    return img


# ==============================================================================
# PIPELINE GENERATOR TOÀN BỘ 4 NHÂN VẬT
# ==============================================================================
def compose_2layers(body_img, prop_img, prop_front=True):
    out = PixelArtShadingHelper.create_canvas(32, 32)
    if prop_front:
        out = Image.alpha_composite(out, body_img)
        out = Image.alpha_composite(out, prop_img)
    else:
        out = Image.alpha_composite(out, prop_img)
        out = Image.alpha_composite(out, body_img)
    return out


def compose_3layers(body_img, prop1, prop2, prop1_front=True, prop2_front=True):
    out = PixelArtShadingHelper.create_canvas(32, 32)
    if not prop1_front:
        out = Image.alpha_composite(out, prop1)
    if not prop2_front:
        out = Image.alpha_composite(out, prop2)
    out = Image.alpha_composite(out, body_img)
    if prop1_front:
        out = Image.alpha_composite(out, prop1)
    if prop2_front:
        out = Image.alpha_composite(out, prop2)
    return out


def generate_all_4_npcs():
    # 1. BORIS (NPC_Blacksmith_Barnaby)
    boris_dir = os.path.join(BASE_MODULAR_DIR, "Boris")
    os.makedirs(boris_dir, exist_ok=True)
    boris_props = {}
    for d in ("front", "back", "side", "left"):
        hp = draw_boris_hammer(d)
        boris_props[d] = hp
        PixelArtShadingHelper.save_sprite_with_meta(hp, os.path.join(boris_dir, f"prop_boris_hammer_{d}.png"))

    for d, fn, in_front in [
        ("front", draw_boris_front_body, True),
        ("back", draw_boris_back_body, False),
        ("side", lambda st: draw_boris_side_body(st, False), True),
        ("left", lambda st: draw_boris_side_body(st, True), False),
    ]:
        idle = fn(0)
        PixelArtShadingHelper.save_sprite_with_meta(idle, os.path.join(boris_dir, f"npc_boris_body_idle_{d}.png"))
        comp = compose_2layers(idle, boris_props[d], in_front)
        PixelArtShadingHelper.save_sprite_with_meta(comp, os.path.join(FLAT_NPCS_DIR, f"npc_boris_idle_{d}.png"))

        for st in range(1, 5):
            walk = fn(st)
            PixelArtShadingHelper.save_sprite_with_meta(walk, os.path.join(boris_dir, f"npc_boris_body_walk_{d}_{st}.png"))
            bob = 1 if st in (1, 3) else 0
            hp_shifted = PixelArtShadingHelper.create_canvas(32, 32)
            hp_shifted.paste(boris_props[d], (0, bob))
            w_comp = compose_2layers(walk, hp_shifted, in_front)
            PixelArtShadingHelper.save_sprite_with_meta(w_comp, os.path.join(FLAT_NPCS_DIR, f"npc_boris_walk_{d}_{st}.png"))

    print("Boris (Thợ Rèn) done!")

    # 2. ELENA (NPC_Alchemist_Elena)
    elena_dir = os.path.join(BASE_MODULAR_DIR, "Elena")
    os.makedirs(elena_dir, exist_ok=True)
    elena_props = {}
    for d in ("front", "back", "side", "left"):
        bp = draw_elena_herb_bag(d)
        elena_props[d] = bp
        PixelArtShadingHelper.save_sprite_with_meta(bp, os.path.join(elena_dir, f"prop_elena_pouch_{d}.png"))

    for d, fn, in_front in [
        ("front", draw_elena_front_body, True),
        ("back", draw_elena_back_body, False),
        ("side", lambda st: draw_elena_side_body(st, False), True),
        ("left", lambda st: draw_elena_side_body(st, True), False),
    ]:
        idle = fn(0)
        PixelArtShadingHelper.save_sprite_with_meta(idle, os.path.join(elena_dir, f"npc_elena_body_idle_{d}.png"))
        comp = compose_2layers(idle, elena_props[d], in_front)
        PixelArtShadingHelper.save_sprite_with_meta(comp, os.path.join(FLAT_NPCS_DIR, f"npc_elena_idle_{d}.png"))

        for st in range(1, 5):
            walk = fn(st)
            PixelArtShadingHelper.save_sprite_with_meta(walk, os.path.join(elena_dir, f"npc_elena_body_walk_{d}_{st}.png"))
            bob = 1 if st in (1, 3) else 0
            bp_shifted = PixelArtShadingHelper.create_canvas(32, 32)
            bp_shifted.paste(elena_props[d], (0, bob))
            w_comp = compose_2layers(walk, bp_shifted, in_front)
            PixelArtShadingHelper.save_sprite_with_meta(w_comp, os.path.join(FLAT_NPCS_DIR, f"npc_elena_walk_{d}_{st}.png"))

    print("Elena (Dược Sĩ) done!")

    # 3. CARLOS (NPC_GuildMaster_Carlos)
    carlos_dir = os.path.join(BASE_MODULAR_DIR, "Carlos")
    os.makedirs(carlos_dir, exist_ok=True)
    carlos_swords = {}
    carlos_capes = {}
    for d in ("front", "back", "side", "left"):
        sp = draw_carlos_sword(d)
        cp = draw_carlos_cape(d)
        carlos_swords[d] = sp
        carlos_capes[d] = cp
        PixelArtShadingHelper.save_sprite_with_meta(sp, os.path.join(carlos_dir, f"prop_carlos_sword_{d}.png"))
        PixelArtShadingHelper.save_sprite_with_meta(cp, os.path.join(carlos_dir, f"prop_carlos_cape_{d}.png"))

    for d, fn, sw_front, cp_front in [
        ("front", draw_carlos_front_body, True, False),
        ("back", draw_carlos_back_body, False, True),
        ("side", lambda st: draw_carlos_side_body(st, False), False, False),
        ("left", lambda st: draw_carlos_side_body(st, True), True, False),
    ]:
        idle = fn(0)
        PixelArtShadingHelper.save_sprite_with_meta(idle, os.path.join(carlos_dir, f"npc_carlos_body_idle_{d}.png"))
        comp = compose_3layers(idle, carlos_swords[d], carlos_capes[d], sw_front, cp_front)
        PixelArtShadingHelper.save_sprite_with_meta(comp, os.path.join(FLAT_NPCS_DIR, f"npc_carlos_idle_{d}.png"))

        for st in range(1, 5):
            walk = fn(st)
            PixelArtShadingHelper.save_sprite_with_meta(walk, os.path.join(carlos_dir, f"npc_carlos_body_walk_{d}_{st}.png"))
            bob = 1 if st in (1, 3) else 0
            sw_sh = PixelArtShadingHelper.create_canvas(32, 32)
            cp_sh = PixelArtShadingHelper.create_canvas(32, 32)
            sw_sh.paste(carlos_swords[d], (0, bob))
            cp_sh.paste(carlos_capes[d], (0, bob))
            w_comp = compose_3layers(walk, sw_sh, cp_sh, sw_front, cp_front)
            PixelArtShadingHelper.save_sprite_with_meta(w_comp, os.path.join(FLAT_NPCS_DIR, f"npc_carlos_walk_{d}_{st}.png"))

    print("Carlos (Hội Trưởng) done!")

    # 4. LINA (NPC_Merchant_Lina)
    lina_dir = os.path.join(BASE_MODULAR_DIR, "Lina")
    os.makedirs(lina_dir, exist_ok=True)
    lina_baskets = {}
    lina_pouches = {}
    for d in ("front", "back", "side", "left"):
        bk = draw_lina_basket(d)
        pc = draw_lina_coin_pouch(d)
        lina_baskets[d] = bk
        lina_pouches[d] = pc
        PixelArtShadingHelper.save_sprite_with_meta(bk, os.path.join(lina_dir, f"prop_lina_basket_{d}.png"))
        PixelArtShadingHelper.save_sprite_with_meta(pc, os.path.join(lina_dir, f"prop_lina_pouch_{d}.png"))

    for d, fn, bk_front in [
        ("front", draw_lina_front_body, True),
        ("back", draw_lina_back_body, False),
        ("side", lambda st: draw_lina_side_body(st, False), True),
        ("left", lambda st: draw_lina_side_body(st, True), False),
    ]:
        idle = fn(0)
        PixelArtShadingHelper.save_sprite_with_meta(idle, os.path.join(lina_dir, f"npc_lina_body_idle_{d}.png"))
        comp = compose_3layers(idle, lina_baskets[d], lina_pouches[d], bk_front, True)
        PixelArtShadingHelper.save_sprite_with_meta(comp, os.path.join(FLAT_NPCS_DIR, f"npc_lina_idle_{d}.png"))

        for st in range(1, 5):
            walk = fn(st)
            PixelArtShadingHelper.save_sprite_with_meta(walk, os.path.join(lina_dir, f"npc_lina_body_walk_{d}_{st}.png"))
            bob = 1 if st in (1, 3) else 0
            bk_sh = PixelArtShadingHelper.create_canvas(32, 32)
            pc_sh = PixelArtShadingHelper.create_canvas(32, 32)
            bk_sh.paste(lina_baskets[d], (0, bob))
            pc_sh.paste(lina_pouches[d], (0, bob))
            w_comp = compose_3layers(walk, bk_sh, pc_sh, bk_front, True)
            PixelArtShadingHelper.save_sprite_with_meta(w_comp, os.path.join(FLAT_NPCS_DIR, f"npc_lina_walk_{d}_{st}.png"))

    print("Lina (Thương Nhân) done!")
    print("ALL 4 NPCS GENERATED SUCCESSFULLY!")


if __name__ == "__main__":
    generate_all_4_npcs()
