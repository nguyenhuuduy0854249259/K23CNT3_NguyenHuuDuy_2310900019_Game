import os
from PIL import Image
from pixel_art_shading_helper import (
    PixelArtShadingHelper,
    ColorRamp4,
    hex_to_rgba,
    STANDARD_PPU,
)

MODULAR_ELDRIN_DIR = "Assets/Sprites/NPCs/Modular/Eldrin"
FLAT_NPCS_DIR = "Assets/Sprites/NPCs"

# ==============================================================================
# BẢNG MÀU 4 SẮC ĐỘ CHUẨN PIXEL ART (OUTLINE -> SHADOW -> BASE -> HIGHLIGHT)
# ==============================================================================
HAIR_RAMP = ColorRamp4("#2B3440", "#7F92A6", "#CAD8E6", "#FFFFFF")
SKIN_RAMP = ColorRamp4("#361313", "#875A2D", "#B8815B", "#FFC69E")
ROBE_RAMP = ColorRamp4("#12172E", "#1A2D5E", "#2B4C8C", "#4A78D4")
GOLD_RAMP = ColorRamp4("#3B2008", "#996212", "#DEA02C", "#FFF078")
BOOT_RAMP = ColorRamp4("#261111", "#361313", "#4A372C", "#6E5242")
WOOD_RAMP = ColorRamp4("#24120B", "#5E2E14", "#964B1E", "#C97636")
GEM_RAMP = ColorRamp4("#0B3B59", "#0E78A8", "#2CD5FF", "#E8FFFF")

EYE_PUPIL = hex_to_rgba("#361313")
EYE_GLINT = hex_to_rgba("#F7EFEB")


def draw_eldrin_front_body(step=0):
    """
    Vẽ thân Trưởng Làng Eldrin hướng Nhìn Xuống (Down / Front) theo đúng giải phẫu Chibi 32x32
    của Player Class (player_novice_4direction):
    - Đầu to Y=8..17 (rộng 12px tại X=10..21), tóc bạc đan caro Dithering + đổ bóng từng lọn.
    - Mặt Chibi Y=18..20 (X=12..19), mắt có điểm sáng trắng #F7EFEB tại (13, 19) và (18, 19).
    - Râu bạc dài chia 3 lọn đổ bóng 4 sắc độ Y=20..24.
    - 2 tay Chibi tách nhẹ 2 bên hông (X=10..11 và X=20..21) có khe tối 1px tại X=12 và X=19.
    - 2 chân ủng tách rõ (X=12..14 và X=17..19) có khe 2px ở giữa (X=15..16) và DropShadow dưới chân.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0
    arm_l_swing = 1 if step == 3 else (-1 if step == 1 else 0)
    arm_r_swing = -1 if step == 3 else (1 if step == 1 else 0)

    # 1. Bóng đen mờ dưới bàn chân (DropShadow)
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2. Hai chân / ủng Chibi tách rõ (X=12..14 và X=17..19, Y=26..29)
    l_top = 26 - leg_l_lift
    l_bot = 29 - leg_l_lift
    for y in range(l_top, l_bot + 1):
        put(img, 11, y, BOOT_RAMP.outline)
        put(img, 12, y, BOOT_RAMP.outline if y == l_bot else BOOT_RAMP.shadow)
        put(img, 13, y, BOOT_RAMP.outline if y == l_bot else (BOOT_RAMP.highlight if y == l_top + 1 else BOOT_RAMP.base))
        put(img, 14, y, BOOT_RAMP.outline if y == l_bot else BOOT_RAMP.base)
        put(img, 15, y, BOOT_RAMP.outline if y < l_bot else (0, 0, 0, 0))

    r_top = 26 - leg_r_lift
    r_bot = 29 - leg_r_lift
    for y in range(r_top, r_bot + 1):
        put(img, 16, y, BOOT_RAMP.outline if y < r_bot else (0, 0, 0, 0))
        put(img, 17, y, BOOT_RAMP.outline if y == r_bot else BOOT_RAMP.base)
        put(img, 18, y, BOOT_RAMP.outline if y == r_bot else (BOOT_RAMP.highlight if y == r_top + 1 else BOOT_RAMP.base))
        put(img, 19, y, BOOT_RAMP.outline if y == r_bot else BOOT_RAMP.shadow)
        put(img, 20, y, BOOT_RAMP.outline)

    # 3. Thân áo choàng chàm sâu viền vàng hoàng kim (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            is_side_edge = (x == 12 or x == 19)
            if y_rel == 25:
                # Gấu áo viền vàng hoàng kim
                if is_side_edge:
                    put(img, x, y, GOLD_RAMP.outline)
                elif x in (14, 17):
                    put(img, x, y, GOLD_RAMP.highlight)
                elif x in (15, 16):
                    put(img, x, y, GOLD_RAMP.base)
                else:
                    put(img, x, y, GOLD_RAMP.shadow)
            elif y_rel == 23:
                # Đai lưng hoàng kim ngang bụng
                if is_side_edge:
                    put(img, x, y, ROBE_RAMP.outline)
                elif x in (15, 16):
                    put(img, x, y, GOLD_RAMP.highlight)
                else:
                    put(img, x, y, GOLD_RAMP.base)
            else:
                # Vạt áo chàm đan caro Dithering + nẹp vàng dọc thân
                if is_side_edge:
                    put(img, x, y, ROBE_RAMP.outline)
                elif x in (15, 16) and y_rel == 24:
                    put(img, x, y, GOLD_RAMP.base if x == 15 else GOLD_RAMP.shadow)
                else:
                    checker = ((x + y) & 1) == 0
                    if x in (13, 18):
                        put(img, x, y, ROBE_RAMP.shadow)
                    else:
                        put(img, x, y, ROBE_RAMP.highlight if checker else ROBE_RAMP.base)

    # 4. Hai cánh tay Chibi tách nhẹ 2 bên hông (X=10..11 và X=20..21) + khe tối 1px tại X=12, 19
    # Tay bên trái khung hình (Tay phải cầm trượng của Eldrin - giữ ổn định vị trí nắm trượng)
    yl = 21 + bob
    put(img, 11, yl, GOLD_RAMP.base)
    put(img, 12, yl, GOLD_RAMP.highlight)
    put(img, 10, yl + 1, ROBE_RAMP.outline)
    put(img, 11, yl + 1, ROBE_RAMP.highlight)
    put(img, 12, yl + 1, ROBE_RAMP.shadow)
    put(img, 10, yl + 2, SKIN_RAMP.highlight)
    put(img, 11, yl + 2, SKIN_RAMP.base)
    put(img, 12, yl + 2, ROBE_RAMP.outline)  # Khe tối 1px tách tay khỏi thân
    put(img, 10, yl + 3, SKIN_RAMP.shadow)
    put(img, 11, yl + 3, SKIN_RAMP.highlight)

    # Tay bên phải khung hình (Tay trái của Eldrin - vung nhẹ theo bước chân)
    yr = 21 + bob + (0 if step in (0, 2) else (1 if arm_r_swing > 0 else 0))
    put(img, 19, 21 + bob, GOLD_RAMP.highlight)
    put(img, 20, 21 + bob, GOLD_RAMP.base)
    put(img, 19, yr + 1, ROBE_RAMP.shadow)
    put(img, 20, yr + 1, ROBE_RAMP.base)
    put(img, 21, yr + 1, ROBE_RAMP.outline)
    put(img, 19, yr + 2, ROBE_RAMP.outline)  # Khe tối 1px tách tay khỏi thân
    put(img, 20, yr + 2, SKIN_RAMP.base)
    put(img, 21, yr + 2, SKIN_RAMP.highlight)
    put(img, 20, yr + 3, SKIN_RAMP.highlight)
    put(img, 21, yr + 3, SKIN_RAMP.shadow)

    # 5. Vòm đầu to Chibi (Y=8..17, X=10..21) với tóc bạc đan caro Dithering & chia lọn
    put(img, 19, 8 + bob, HAIR_RAMP.outline)
    crown_spans = [
        (9, 13, 19),
        (10, 12, 20),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 10, 21),
        (17, 11, 20),
    ]
    for y_rel, x0, x1 in crown_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel == 9 or x == x0 or x == x1:
                put(img, x, y, HAIR_RAMP.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, HAIR_RAMP.shadow)
            else:
                checker = ((x + y) & 1) == 0
                tone = HAIR_RAMP.highlight if checker else HAIR_RAMP.base
                # Rãnh chia lọn tóc bạc tự nhiên
                if x in (13, 18) and y_rel >= 12:
                    tone = HAIR_RAMP.shadow
                elif x in (15, 16) and y_rel in (11, 12, 13):
                    tone = HAIR_RAMP.highlight if ((x + y) & 1) == 0 else HAIR_RAMP.base
                put(img, x, y, tone)

    # Lọn tóc mai bạc rủ hai bên thái dương (Y=18..20, X=10..11 và X=20..21)
    for y_rel in (18, 19, 20):
        y = y_rel + bob
        put(img, 10, y, HAIR_RAMP.outline)
        put(img, 11, y, HAIR_RAMP.base if y_rel < 20 else HAIR_RAMP.shadow)
        put(img, 20, y, HAIR_RAMP.base if y_rel < 20 else HAIR_RAMP.shadow)
        put(img, 21, y, HAIR_RAMP.outline)

    # 6. Chân mày bạc rậm & Đôi mắt Chibi có điểm sáng trắng (#F7EFEB) giống hệt Player Class
    y17 = 17 + bob
    put(img, 12, y17, HAIR_RAMP.shadow)
    put(img, 13, y17, HAIR_RAMP.highlight)
    put(img, 14, y17, HAIR_RAMP.base)
    put(img, 15, y17, SKIN_RAMP.shadow)
    put(img, 16, y17, SKIN_RAMP.shadow)
    put(img, 17, y17, HAIR_RAMP.base)
    put(img, 18, y17, HAIR_RAMP.highlight)
    put(img, 19, y17, HAIR_RAMP.shadow)

    y18 = 18 + bob
    put(img, 12, y18, HAIR_RAMP.outline)
    put(img, 13, y18, EYE_PUPIL)
    put(img, 14, y18, SKIN_RAMP.shadow)
    put(img, 15, y18, SKIN_RAMP.highlight)
    put(img, 16, y18, SKIN_RAMP.highlight)
    put(img, 17, y18, SKIN_RAMP.shadow)
    put(img, 18, y18, EYE_PUPIL)
    put(img, 19, y18, HAIR_RAMP.outline)

    y19 = 19 + bob
    put(img, 12, y19, HAIR_RAMP.outline)
    put(img, 13, y19, EYE_GLINT)  # Điểm sáng trắng trong mắt trái
    put(img, 14, y19, SKIN_RAMP.base)
    put(img, 15, y19, SKIN_RAMP.highlight)
    put(img, 16, y19, SKIN_RAMP.base)
    put(img, 17, y19, SKIN_RAMP.base)
    put(img, 18, y19, EYE_GLINT)  # Điểm sáng trắng trong mắt phải
    put(img, 19, y19, HAIR_RAMP.outline)

    # 7. Hai gò má Chibi & Bộ râu bạc dài chia 3 lọn đổ bóng (Y=20..24)
    y20 = 20 + bob
    put(img, 12, y20, HAIR_RAMP.outline)
    put(img, 13, y20, SKIN_RAMP.highlight)
    put(img, 14, y20, HAIR_RAMP.highlight)  # Ria mép trái sáng
    put(img, 15, y20, HAIR_RAMP.base)
    put(img, 16, y20, HAIR_RAMP.highlight)
    put(img, 17, y20, HAIR_RAMP.base)       # Ria mép phải
    put(img, 18, y20, SKIN_RAMP.highlight)
    put(img, 19, y20, HAIR_RAMP.outline)

    # Thân râu bạc dài (Y=21..24) với rãnh tối chia lọn (shadow) + điểm sáng (highlight)
    y21 = 21 + bob
    put(img, 13, y21, HAIR_RAMP.outline)
    put(img, 14, y21, HAIR_RAMP.base)
    put(img, 15, y21, HAIR_RAMP.highlight)
    put(img, 16, y21, HAIR_RAMP.base)
    put(img, 17, y21, HAIR_RAMP.shadow)
    put(img, 18, y21, HAIR_RAMP.outline)

    y22 = 22 + bob
    put(img, 13, y22, HAIR_RAMP.outline)
    put(img, 14, y22, HAIR_RAMP.shadow)
    put(img, 15, y22, HAIR_RAMP.highlight)
    put(img, 16, y22, HAIR_RAMP.highlight)
    put(img, 17, y22, HAIR_RAMP.base)
    put(img, 18, y22, HAIR_RAMP.outline)

    y23 = 23 + bob
    put(img, 14, y23, HAIR_RAMP.outline)
    put(img, 15, y23, HAIR_RAMP.base)
    put(img, 16, y23, HAIR_RAMP.highlight)
    put(img, 17, y23, HAIR_RAMP.outline)

    y24 = 24 + bob
    put(img, 15, y24, HAIR_RAMP.outline)
    put(img, 16, y24, HAIR_RAMP.shadow)

    return img


def draw_eldrin_back_body(step=0):
    """
    Vẽ thân Trưởng Làng Eldrin hướng Nhìn Lên (Up / Back) theo đúng giải phẫu Chibi 32x32:
    - Mái tóc bạc dài phía sau chia lọn và đan caro Dithering (Y=8..21, X=10..21).
    - Áo choàng chàm sâu có cổ hoàng kim và hoa văn vàng sau lưng (Y=21..25).
    - 2 tay tách nhẹ 2 bên hông (X=10..11 và X=20..21), 2 chân tách rõ (X=12..14 và X=17..19) + DropShadow.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)
    put = PixelArtShadingHelper.put_px

    bob = 1 if step in (1, 3) else 0
    leg_l_lift = 1 if step == 1 else 0
    leg_r_lift = 1 if step == 3 else 0

    # 1. Bóng đen mờ dưới bàn chân (DropShadow)
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2. Hai chân / ủng Chibi tách rõ (X=12..14 và X=17..19, Y=26..29)
    l_top = 26 - leg_l_lift
    l_bot = 29 - leg_l_lift
    for y in range(l_top, l_bot + 1):
        put(img, 11, y, BOOT_RAMP.outline)
        put(img, 12, y, BOOT_RAMP.outline if y == l_bot else BOOT_RAMP.shadow)
        put(img, 13, y, BOOT_RAMP.outline if y == l_bot else BOOT_RAMP.base)
        put(img, 14, y, BOOT_RAMP.outline if y == l_bot else BOOT_RAMP.shadow)
        put(img, 15, y, BOOT_RAMP.outline if y < l_bot else (0, 0, 0, 0))

    r_top = 26 - leg_r_lift
    r_bot = 29 - leg_r_lift
    for y in range(r_top, r_bot + 1):
        put(img, 16, y, BOOT_RAMP.outline if y < r_bot else (0, 0, 0, 0))
        put(img, 17, y, BOOT_RAMP.outline if y == r_bot else BOOT_RAMP.shadow)
        put(img, 18, y, BOOT_RAMP.outline if y == r_bot else BOOT_RAMP.base)
        put(img, 19, y, BOOT_RAMP.outline if y == r_bot else BOOT_RAMP.shadow)
        put(img, 20, y, BOOT_RAMP.outline)

    # 3. Thân áo choàng chàm phía sau (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(img, x, y, GOLD_RAMP.outline if y_rel == 25 else ROBE_RAMP.outline)
            elif y_rel == 25:
                put(img, x, y, GOLD_RAMP.highlight if x in (15, 16) else GOLD_RAMP.base)
            elif y_rel == 23:
                put(img, x, y, GOLD_RAMP.base if x in (14, 15, 16, 17) else ROBE_RAMP.shadow)
            else:
                checker = ((x + y) & 1) == 0
                if x in (15, 16):
                    put(img, x, y, GOLD_RAMP.base if y_rel == 22 else ROBE_RAMP.highlight)
                else:
                    put(img, x, y, ROBE_RAMP.base if checker else ROBE_RAMP.shadow)

    # 4. Hai tay Chibi 2 bên hông (X=10..11 và X=20..21)
    yl = 21 + bob
    put(img, 11, yl, GOLD_RAMP.base)
    put(img, 10, yl + 1, ROBE_RAMP.outline)
    put(img, 11, yl + 1, ROBE_RAMP.base)
    put(img, 10, yl + 2, SKIN_RAMP.base)
    put(img, 11, yl + 2, SKIN_RAMP.shadow)
    put(img, 12, yl + 2, ROBE_RAMP.outline)
    put(img, 10, yl + 3, SKIN_RAMP.shadow)
    put(img, 11, yl + 3, SKIN_RAMP.base)

    # Tay phải của Eldrin ở phía bên phải khung hình khi nhìn từ sau lưng (X=20..21)
    yr = 21 + bob
    put(img, 20, yr, GOLD_RAMP.base)
    put(img, 20, yr + 1, ROBE_RAMP.base)
    put(img, 21, yr + 1, ROBE_RAMP.outline)
    put(img, 19, yr + 2, ROBE_RAMP.outline)
    put(img, 20, yr + 2, SKIN_RAMP.base)
    put(img, 21, yr + 2, SKIN_RAMP.highlight)
    put(img, 20, yr + 3, SKIN_RAMP.shadow)
    put(img, 21, yr + 3, SKIN_RAMP.base)

    # 5. Mái tóc bạc dài sau lưng chia lọn + đan caro Dithering (Y=9..21, X=10..21)
    back_hair_spans = [
        (9, 13, 18),
        (10, 12, 19),
        (11, 11, 20),
        (12, 11, 20),
        (13, 10, 21),
        (14, 10, 21),
        (15, 10, 21),
        (16, 10, 21),
        (17, 10, 21),
        (18, 11, 20),
        (19, 11, 20),
        (20, 12, 19),
        (21, 13, 18),
    ]
    for y_rel, x0, x1 in back_hair_spans:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if y_rel in (9, 21) or x == x0 or x == x1:
                put(img, x, y, HAIR_RAMP.outline)
            elif x == x0 + 1 or x == x1 - 1:
                put(img, x, y, HAIR_RAMP.shadow)
            elif x in (13, 16, 18) and y_rel >= 14:
                # Rãnh chia các lọn tóc bạc dài sau lưng
                put(img, x, y, HAIR_RAMP.shadow)
            else:
                checker = ((x + y) & 1) == 0
                if y_rel <= 15:
                    put(img, x, y, HAIR_RAMP.highlight if checker else HAIR_RAMP.base)
                else:
                    put(img, x, y, HAIR_RAMP.base if checker else HAIR_RAMP.shadow)

    return img


def draw_eldrin_side_body(step=0, facing_left=False):
    """
    Vẽ thân Trưởng Làng Eldrin hướng Nhìn Ngang (Side: Right hoặc Left) theo đúng giải phẫu Chibi 32x32
    của Player Class (player_novice_4direction / player_mage_4direction):
    - Đầu nghiêng Chibi Y=8..20 (X=10..21), mắt nghiêng có điểm sáng trắng #F7EFEB.
    - Râu bạc nhô ra phía trước cằm chia lọn sáng/tối.
    - Tay cầm trượng đưa nhẹ ra phía trước nắm sát thân trượng.
    - 2 chân bước nhịp nhàng có độ tương phản chân gần / chân xa và DropShadow dưới bàn chân.
    """
    img = PixelArtShadingHelper.create_canvas(32, 32)

    def put(x, y, c):
        tx = (31 - x) if facing_left else x
        PixelArtShadingHelper.put_px(img, tx, y, c)

    bob = 1 if step in (1, 3) else 0

    # 1. Bóng đen mờ dưới bàn chân (DropShadow)
    PixelArtShadingHelper.draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1)

    # 2. Hai chân / ủng nhìn ngang (tách bước khi đi bộ)
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
                    put(x, y, BOOT_RAMP.outline)
                else:
                    put(x, y, (BOOT_RAMP.highlight if (is_near and y == y0 + 1) else (BOOT_RAMP.base if is_near else BOOT_RAMP.shadow)))

    # 3. Thân áo choàng chàm nhìn ngang (Y=21..25, X=12..19)
    for y_rel in range(21, 26):
        y = y_rel + bob
        for x in range(12, 20):
            if x == 12 or x == 19:
                put(x, y, GOLD_RAMP.outline if y_rel == 25 else ROBE_RAMP.outline)
            elif y_rel == 25:
                put(x, y, GOLD_RAMP.highlight if x in (15, 16) else GOLD_RAMP.base)
            elif y_rel == 23:
                put(x, y, GOLD_RAMP.highlight if x >= 16 else GOLD_RAMP.base)
            else:
                checker = ((x + y) & 1) == 0
                put(x, y, ROBE_RAMP.highlight if (checker and x >= 15) else (ROBE_RAMP.base if x >= 14 else ROBE_RAMP.shadow))

    # 4. Vòm đầu & Tóc bạc nhìn ngang (Y=8..20, X=9..21) giống hệt form nghiêng của Player Class
    put(10, 8 + bob, HAIR_RAMP.outline)
    put(9, 9 + bob, HAIR_RAMP.outline)
    put(10, 9 + bob, HAIR_RAMP.highlight)
    put(11, 9 + bob, HAIR_RAMP.outline)

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
                put(x, y, HAIR_RAMP.outline)
            elif x == x0 + 1 or (y_rel == 15 and x >= 16):
                put(x, y, HAIR_RAMP.shadow)
            else:
                checker = ((x + y) & 1) == 0
                put(x, y, HAIR_RAMP.highlight if checker else HAIR_RAMP.base)

    # Lọn tóc bạc rủ mềm sau gáy + Tai Chibi (X=10..15, Y=16..20)
    for y_rel in range(16, 21):
        y = y_rel + bob
        x_start = 10 if y_rel <= 18 else 11
        put(x_start, y, HAIR_RAMP.outline)
        put(x_start + 1, y, HAIR_RAMP.shadow)
        put(12, y, HAIR_RAMP.highlight if (y_rel % 2 == 0) else HAIR_RAMP.base)
        put(13, y, HAIR_RAMP.base if y_rel <= 18 else HAIR_RAMP.shadow)
        if y_rel == 20:
            put(12, y, HAIR_RAMP.outline)
            put(13, y, HAIR_RAMP.outline)

    # Tai Chibi hồng ấm tại X=14, Y=16..17 + Chân mày bạc & Mắt có điểm sáng trắng (#F7EFEB)
    y16 = 16 + bob
    put(14, y16, SKIN_RAMP.base)       # Đỉnh tai
    put(15, y16, HAIR_RAMP.shadow)     # Tóc mai trước tai
    put(16, y16, HAIR_RAMP.highlight)  # Chân mày bạc
    put(17, y16, HAIR_RAMP.highlight)
    put(18, y16, HAIR_RAMP.base)
    put(19, y16, HAIR_RAMP.outline)

    y17 = 17 + bob
    put(14, y17, SKIN_RAMP.highlight)  # Dái tai sáng
    put(15, y17, SKIN_RAMP.shadow)
    put(16, y17, SKIN_RAMP.base)
    put(17, y17, EYE_GLINT)            # Điểm sáng trắng mắt nghiêng (#F7EFEB)
    put(18, y17, EYE_PUPIL)            # Con ngươi mắt nghiêng (#361313)
    put(19, y17, SKIN_RAMP.highlight)
    put(20, y17, SKIN_RAMP.outline)

    y18 = 18 + bob
    put(14, y18, HAIR_RAMP.shadow)
    put(15, y18, SKIN_RAMP.base)
    put(16, y18, SKIN_RAMP.highlight)
    put(17, y18, SKIN_RAMP.highlight)
    put(18, y18, SKIN_RAMP.highlight)
    put(19, y18, SKIN_RAMP.base)
    put(20, y18, SKIN_RAMP.outline)

    # Râu bạc nhô ra phía trước cằm (Y=19..24, X=15..19) rõ nét không bị trượng che
    beard_side_rows = [
        (19, 15, 19),
        (20, 15, 19),
        (21, 15, 19),
        (22, 16, 19),
        (23, 16, 18),
        (24, 17, 18),
    ]
    for y_rel, x0, x1 in beard_side_rows:
        y = y_rel + bob
        for x in range(x0, x1 + 1):
            if x == x0 or x == x1 or y_rel == 24:
                put(x, y, HAIR_RAMP.outline)
            elif x == x1 - 1:
                put(x, y, HAIR_RAMP.highlight)
            else:
                put(x, y, HAIR_RAMP.base if ((x + y) & 1) == 0 else HAIR_RAMP.shadow)

    # 5. Tay cầm trượng đưa nhẹ ra phía trước (X=15..20, Y=21..24, bàn tay tại X=18..19, Y=23..24)
    ya = 21 + bob
    put(15, ya, GOLD_RAMP.base)
    put(16, ya, GOLD_RAMP.highlight)
    put(15, ya + 1, ROBE_RAMP.outline)
    put(16, ya + 1, ROBE_RAMP.highlight)
    put(17, ya + 1, ROBE_RAMP.base)
    put(18, ya + 1, ROBE_RAMP.outline)
    put(17, ya + 2, ROBE_RAMP.outline)
    put(18, ya + 2, SKIN_RAMP.highlight)
    put(19, ya + 2, SKIN_RAMP.base)
    put(18, ya + 3, SKIN_RAMP.shadow)
    put(19, ya + 3, SKIN_RAMP.highlight)

    return img


def draw_elder_staff_and_glow(direction):
    """
    Vẽ đạo cụ tách lớp Quyền Trượng Gỗ Sồi Ngọc Lam (Prop_ElderStaff) và Lớp Phát Sáng (Prop_OrbGlow)
    trên cùng lưới Pixel 32x32 (PPU = 16, No Mixels):
    - Độ dày thân trượng 2px có viền tối + đổ bóng gỗ sồi + đế bọc vàng.
    - Bàn tay phải của Eldrin luôn nắm khít vào thân trượng ở cả 4 hướng mà không che khuất khuôn mặt:
      + front (Down): Tay phải của Eldrin ở bên trái khung hình (X=10..11, Y=23..24) -> Trượng tại X=8..9.
      + back (Up): Tay phải của Eldrin ở bên phải khung hình (X=20..21, Y=23..24) -> Trượng tại X=22..23.
      + side (Right): Tay phải của Eldrin đưa ra trước (X=18..19, Y=23..24) -> Trượng tại X=21..22.
      + left (Left): Trượng đối xứng chuẩn xác tại X=9..10 (khớp tay X=12..13).
    """
    staff_img = PixelArtShadingHelper.create_canvas(32, 32)
    glow_img = PixelArtShadingHelper.create_canvas(32, 32)
    put_s = PixelArtShadingHelper.put_px
    put_g = PixelArtShadingHelper.put_px

    if direction == "front":
        shaft_x = 8
        grip_x = 10
        crook_dir = -1
    elif direction == "back":
        shaft_x = 22
        grip_x = 21
        crook_dir = 1
    elif direction == "side":
        shaft_x = 21
        grip_x = 20
        crook_dir = 1
    else:  # left
        shaft_x = 9
        grip_x = 11
        crook_dir = -1

    # 1. Thân trượng gỗ sồi 2px (X = shaft_x .. shaft_x+1, Y = 11..29) + viền tối bao quanh
    for y in range(11, 30):
        if y == 11 or y == 29:
            put_s(staff_img, shaft_x, y, WOOD_RAMP.outline)
            put_s(staff_img, shaft_x + 1, y, WOOD_RAMP.outline)
        elif y in (12, 13, 27, 28):
            # Khớp nối bọc vàng hoàng kim ở cổ trượng và chân trượng
            put_s(staff_img, shaft_x - 1, y, GOLD_RAMP.outline)
            put_s(staff_img, shaft_x, y, GOLD_RAMP.highlight)
            put_s(staff_img, shaft_x + 1, y, GOLD_RAMP.base)
            put_s(staff_img, shaft_x + 2, y, GOLD_RAMP.outline)
        else:
            put_s(staff_img, shaft_x - 1, y, WOOD_RAMP.outline)
            put_s(staff_img, shaft_x, y, WOOD_RAMP.highlight if (y % 2 == 0) else WOOD_RAMP.base)
            put_s(staff_img, shaft_x + 1, y, WOOD_RAMP.shadow if (y % 2 == 0) else WOOD_RAMP.base)
            put_s(staff_img, shaft_x + 2, y, WOOD_RAMP.outline)

    # 2. Đai tay cầm nối liền sát vào bàn tay phải của Eldrin (Y = 23..24)
    put_s(staff_img, grip_x, 22, GOLD_RAMP.outline)
    put_s(staff_img, grip_x, 23, GOLD_RAMP.highlight)
    put_s(staff_img, grip_x, 24, GOLD_RAMP.base)
    put_s(staff_img, grip_x, 25, GOLD_RAMP.outline)

    # 3. Đầu trượng gỗ sồi uốn cong & Đài vàng ôm viên Ngọc Lam (Y = 6..11)
    gem_cx = shaft_x + (1 if crook_dir > 0 else 0)
    gem_cy = 8

    # Vòng gỗ sồi & ngạnh vàng nâng viên ngọc
    for dy in range(-3, 4):
        for dx in range(-3, 4):
            px = gem_cx + dx
            py = gem_cy + dy
            dist_sq = dx * dx + dy * dy
            if 5 <= dist_sq <= 10:
                if dy >= 1:
                    put_s(staff_img, px, py, GOLD_RAMP.highlight if dx <= 0 else GOLD_RAMP.base)
                elif (crook_dir < 0 and dx <= -1) or (crook_dir > 0 and dx >= 1):
                    put_s(staff_img, px, py, WOOD_RAMP.highlight if dy < 0 else WOOD_RAMP.base)

    # Viền tối quanh đầu trượng trước khi đặt viên ngọc lam
    staff_img = PixelArtShadingHelper.apply_dark_outline(staff_img, WOOD_RAMP.outline)

    # 4. Viên Ngọc Lam (Turquoise Orb) 3x3 tại tâm (gem_cx, gem_cy) đổ bóng 4 sắc độ
    for dy in range(-1, 2):
        for dx in range(-1, 2):
            px = gem_cx + dx
            py = gem_cy + dy
            if dx == -1 and dy == -1:
                put_s(staff_img, px, py, GEM_RAMP.highlight)
            elif dx + dy <= 0:
                put_s(staff_img, px, py, GEM_RAMP.base)
            elif dx == 1 and dy == 1:
                put_s(staff_img, px, py, GEM_RAMP.outline)
            else:
                put_s(staff_img, px, py, GEM_RAMP.shadow)

    # 5. Lớp Hiệu Ứng Phát Sáng Tách Riêng (Prop_OrbGlow) khớp 100% tọa độ (gem_cx, gem_cy)
    for dy in range(-4, 5):
        for dx in range(-4, 5):
            px = gem_cx + dx
            py = gem_cy + dy
            d2 = dx * dx + dy * dy
            if d2 == 0:
                put_g(glow_img, px, py, (255, 255, 255, 255))
            elif d2 <= 2:
                put_g(glow_img, px, py, (165, 250, 255, 235))
            elif d2 <= 5:
                put_g(glow_img, px, py, (56, 220, 255, 165))
            elif d2 <= 9:
                if ((px + py) & 1) == 0:
                    put_g(glow_img, px, py, (32, 195, 255, 110))
                else:
                    put_g(glow_img, px, py, (32, 195, 255, 65))
            elif d2 in (10, 13) and (dx == 0 or dy == 0):
                # Tia sáng chữ thập lấp lánh quanh đỉnh trượng
                put_g(glow_img, px, py, (180, 250, 255, 150))

    return staff_img, glow_img


def compose_preview(body_img, staff_img, glow_img, staff_in_front=True):
    """Ghép các lớp Modular (Thân + Trượng + Hào quang) để xuất bản preview đầy đủ."""
    out = PixelArtShadingHelper.create_canvas(32, 32)
    if staff_in_front:
        out = Image.alpha_composite(out, body_img)
        out = Image.alpha_composite(out, staff_img)
        out = Image.alpha_composite(out, glow_img)
    else:
        out = Image.alpha_composite(out, staff_img)
        out = Image.alpha_composite(out, glow_img)
        out = Image.alpha_composite(out, body_img)
    return out


def generate_all_eldrin_assets():
    os.makedirs(MODULAR_ELDRIN_DIR, exist_ok=True)
    os.makedirs(FLAT_NPCS_DIR, exist_ok=True)

    # 1. Tạo các lớp Đạo cụ Quyền trượng & Hào quang Ngọc lam cho cả 4 hướng
    staff_layers = {}
    glow_layers = {}
    for d in ("front", "back", "side", "left"):
        s_img, g_img = draw_elder_staff_and_glow(d)
        staff_layers[d] = s_img
        glow_layers[d] = g_img
        PixelArtShadingHelper.save_sprite_with_meta(
            s_img, os.path.join(MODULAR_ELDRIN_DIR, f"prop_eldrin_staff_{d}.png"), ppu=STANDARD_PPU
        )
        PixelArtShadingHelper.save_sprite_with_meta(
            g_img, os.path.join(MODULAR_ELDRIN_DIR, f"prop_eldrin_orb_glow_{d}.png"), ppu=STANDARD_PPU
        )

    # Giữ file prop_eldrin_orb_glow.png mặc định trùng với hướng front để bảo toàn GUID
    PixelArtShadingHelper.save_sprite_with_meta(
        glow_layers["front"], os.path.join(MODULAR_ELDRIN_DIR, "prop_eldrin_orb_glow.png"), ppu=STANDARD_PPU
    )

    # 2. Tạo toàn bộ các frame Thân nhân vật (Idle + Walk 1..4) cho đủ 4 hướng (front, back, side, left)
    directions = [
        ("front", lambda st: draw_eldrin_front_body(st), True),
        ("back", lambda st: draw_eldrin_back_body(st), False),
        ("side", lambda st: draw_eldrin_side_body(st, facing_left=False), True),
        ("left", lambda st: draw_eldrin_side_body(st, facing_left=True), False),
    ]

    for dir_name, builder_fn, staff_front in directions:
        # Idle frame (step = 0)
        idle_body = builder_fn(0)
        PixelArtShadingHelper.save_sprite_with_meta(
            idle_body, os.path.join(MODULAR_ELDRIN_DIR, f"npc_eldrin_body_idle_{dir_name}.png"), ppu=STANDARD_PPU
        )
        idle_comp = compose_preview(idle_body, staff_layers[dir_name], glow_layers[dir_name], staff_front)
        PixelArtShadingHelper.save_sprite_with_meta(
            idle_comp, os.path.join(FLAT_NPCS_DIR, f"npc_eldrin_idle_{dir_name}.png"), ppu=STANDARD_PPU
        )

        # 4 Walk frames (step = 1..4)
        for st in range(1, 5):
            walk_body = builder_fn(st)
            PixelArtShadingHelper.save_sprite_with_meta(
                walk_body,
                os.path.join(MODULAR_ELDRIN_DIR, f"npc_eldrin_body_walk_{dir_name}_{st}.png"),
                ppu=STANDARD_PPU,
            )

            # Khi bước đi (step 1 và 3), đạo cụ nhún xuống 1px cùng nhịp với bàn tay
            bob = 1 if st in (1, 3) else 0
            staff_shifted = PixelArtShadingHelper.create_canvas(32, 32)
            glow_shifted = PixelArtShadingHelper.create_canvas(32, 32)
            staff_shifted.paste(staff_layers[dir_name], (0, bob))
            glow_shifted.paste(glow_layers[dir_name], (0, bob))

            walk_comp = compose_preview(walk_body, staff_shifted, glow_shifted, staff_front)
            PixelArtShadingHelper.save_sprite_with_meta(
                walk_comp, os.path.join(FLAT_NPCS_DIR, f"npc_eldrin_walk_{dir_name}_{st}.png"), ppu=STANDARD_PPU
            )

    print("Successfully generated all Chibi 32x32 4-direction modular sprites for Village Elder Eldrin!")


if __name__ == "__main__":
    generate_all_eldrin_assets()
