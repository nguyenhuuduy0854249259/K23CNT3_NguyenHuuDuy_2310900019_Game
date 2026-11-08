import math
import os
from PIL import Image, ImageDraw, ImageFilter

HUD_DIR = r"d:\Unity\prj1\gam\Assets\Sprites\UI\HUD"
SKILLS_DIR = os.path.join(HUD_DIR, "Skills")
ICONS_DIR = os.path.join(HUD_DIR, "Icons")

os.makedirs(SKILLS_DIR, exist_ok=True)
os.makedirs(ICONS_DIR, exist_ok=True)

def create_supersampled(size=(64, 64), scale=4):
    w, h = size
    img = Image.new("RGBA", (w * scale, h * scale), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img), scale

def finalize(img, scale=4):
    w, h = img.size[0] // scale, img.size[1] // scale
    return img.resize((w, h), Image.Resampling.LANCZOS)

def draw_icon_base(draw, S, border_color=(180, 140, 60), inner_color=(15, 18, 26)):
    pad = 4 * S
    draw.rounded_rectangle([pad, pad, 64*S - pad, 64*S - pad], radius=6*S, fill=inner_color)
    draw.rounded_rectangle([pad + 2*S, pad + 2*S, 64*S - pad - 2*S, 64*S - pad - 2*S], radius=5*S, outline=(8, 10, 16, 255), width=2*S)
    draw.rounded_rectangle([pad, pad, 64*S - pad, 64*S - pad], radius=6*S, outline=border_color, width=2*S)
    for cx, cy in [(pad+3*S, pad+3*S), (64*S-pad-3*S, pad+3*S), (pad+3*S, 64*S-pad-3*S), (64*S-pad-3*S, 64*S-pad-3*S)]:
        draw.ellipse([cx-1.5*S, cy-1.5*S, cx+1.5*S, cy+1.5*S], fill=(240, 210, 120))

# ==============================================================================
# 1. WARRIOR SKILLS (ĐẤU SĨ)
# ==============================================================================

# Q: Trảm Kích (Cleave) - Vung vũ khí bán nguyệt 180 độ
def gen_warrior_cleave():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(220, 130, 40), inner_color=(25, 14, 10))
    cx, cy = 32 * S, 36 * S
    # Bán nguyệt quét 180 độ
    for r in range(26, 14, -2):
        draw.arc([cx - r*S, cy - r*S, cx + r*S, cy + r*S], start=180, end=360, fill=(255, 140, 30, 160), width=3*S)
    draw.arc([cx - 24*S, cy - 24*S, cx + 24*S, cy + 24*S], start=185, end=355, fill=(255, 230, 150), width=4*S)
    # Lưỡi đại đao chém
    draw.line([(14*S, 48*S), (46*S, 20*S)], fill=(230, 240, 255), width=4*S)
    draw.polygon([(46*S, 20*S), (54*S, 14*S), (40*S, 26*S)], fill=(255, 255, 255))
    return finalize(img, S)

# W: Xung Phong (Vanguard Charge) - Lao tới đẩy lùi choáng
def gen_warrior_charge():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(240, 160, 50), inner_color=(22, 16, 12))
    # Mũi tên xung kích vàng rực + khiên húc
    shield = [(20*S, 16*S), (38*S, 16*S), (44*S, 32*S), (38*S, 48*S), (20*S, 48*S)]
    draw.polygon(shield, fill=(210, 160, 40))
    draw.polygon([(23*S, 19*S), (36*S, 19*S), (41*S, 32*S), (36*S, 45*S), (23*S, 45*S)], fill=(70, 45, 20))
    # Vệt gió gia tốc đẩy tới
    for y in [24, 32, 40]:
        draw.line([(8*S, y*S), (22*S, y*S)], fill=(255, 220, 100, 220), width=3*S)
        draw.line([(44*S, y*S), (56*S, (y-2)*S)], fill=(255, 255, 200, 240), width=3*S)
    return finalize(img, S)

# E: Thiết Giáp (Iron Bastion) - Giáp ảo hấp thụ 3s
def gen_warrior_iron_bastion():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(200, 210, 225), inner_color=(15, 20, 28))
    # Khiên thép nguyên khối dày dặn
    pts = [(16*S, 14*S), (48*S, 14*S), (48*S, 34*S), (32*S, 52*S), (16*S, 34*S)]
    draw.polygon(pts, fill=(160, 175, 195))
    inner = [(20*S, 18*S), (44*S, 18*S), (44*S, 32*S), (32*S, 47*S), (20*S, 32*S)]
    draw.polygon(inner, fill=(50, 65, 85))
    # Hào quang phòng ngự kim cang
    draw.rectangle([28*S, 20*S, 36*S, 42*S], fill=(220, 240, 255))
    draw.rectangle([20*S, 28*S, 44*S, 34*S], fill=(220, 240, 255))
    return finalize(img, S)

# R: Địa Chấn (Tremor Slam) - Đập đất nứt nẻ hình nón
def gen_warrior_tremor_slam():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(210, 110, 30), inner_color=(26, 14, 10))
    # Búa tạ đập đất
    draw.line([(18*S, 16*S), (38*S, 36*S)], fill=(180, 130, 60), width=5*S)
    draw.rectangle([34*S, 30*S, 48*S, 44*S], fill=(140, 150, 165))
    # Vết nứt đất tỏa năng lượng cam đỏ
    cracks = [
        [(32*S, 46*S), (20*S, 54*S)],
        [(36*S, 46*S), (36*S, 56*S)],
        [(40*S, 46*S), (52*S, 54*S)],
        [(36*S, 46*S), (48*S, 50*S)]
    ]
    for c in cracks:
        draw.line(c, fill=(255, 120, 30), width=4*S)
        draw.line(c, fill=(255, 230, 120), width=1*S)
    return finalize(img, S)

# T: Cuồng Nộ Kích (Frenzied Strike) - Chí mạng tăng theo máu mất
def gen_warrior_frenzied_strike():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(235, 40, 40), inner_color=(32, 10, 10))
    # Đôi mắt dã thú cuồng nộ / kiếm đẫm huyết khí
    draw.polygon([(14*S, 50*S), (50*S, 14*S), (54*S, 18*S), (18*S, 54*S)], fill=(255, 50, 50))
    draw.line([(20*S, 44*S), (48*S, 16*S)], fill=(255, 230, 230), width=2*S)
    # Tia máu/lửa cuồng bạo xung quanh
    for sx, sy in [(22, 20), (44, 40), (14, 30), (40, 12), (32, 48)]:
        draw.ellipse([sx*S-2*S, sy*S-2*S, sx*S+2*S, sy*S+2*S], fill=(255, 20, 20))
    return finalize(img, S)

# ==============================================================================
# 2. MAGE SKILLS (PHÁP SƯ)
# ==============================================================================

# Q: Hỏa Cầu (Fire Orb) - Cầu lửa nổ AOE
def gen_mage_fire_orb():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(245, 90, 25), inner_color=(30, 12, 10))
    cx, cy = 34 * S, 32 * S
    for r in range(22, 6, -2):
        draw.ellipse([cx - r*S, cy - r*S, cx + r*S, cy + r*S], fill=(255, 80 + r*6, 20, 150))
    draw.ellipse([cx - 8*S, cy - 8*S, cx + 8*S, cy + 8*S], fill=(255, 240, 140))
    # Đuôi lửa bay từ trái sang
    draw.polygon([(12*S, 32*S), (26*S, 20*S), (26*S, 44*S)], fill=(255, 120, 30))
    return finalize(img, S)

# W: Băng Phong Thược (Frost Spikes) - Gai băng trồi lên trói chân
def gen_mage_frost_spikes():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(80, 190, 245), inner_color=(10, 20, 30))
    # 3 ngọn gai băng nhọn hoắt
    spikes = [
        [(18*S, 48*S), (24*S, 24*S), (30*S, 48*S)],
        [(28*S, 52*S), (36*S, 14*S), (44*S, 52*S)],
        [(40*S, 48*S), (48*S, 28*S), (54*S, 48*S)],
    ]
    for sp in spikes:
        draw.polygon(sp, fill=(130, 220, 255))
        draw.line([sp[0], sp[1]], fill=(255, 255, 255), width=2*S)
    return finalize(img, S)

# E: Thiểm Dịch (Blink / Arcane Shift) - Biến ảnh dịch chuyển
def gen_mage_blink():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(180, 90, 240), inner_color=(20, 10, 30))
    # Vòng tròn dịch chuyển ma pháp tím
    cx, cy = 40 * S, 28 * S
    draw.ellipse([cx - 16*S, cy - 16*S, cx + 16*S, cy + 16*S], outline=(200, 120, 255), width=3*S)
    draw.ellipse([cx - 6*S, cy - 6*S, cx + 6*S, cy + 6*S], fill=(255, 255, 255))
    # Bóng ảo mờ phía sau
    draw.ellipse([20*S - 10*S, 42*S - 10*S, 20*S + 10*S, 42*S + 10*S], outline=(140, 60, 200, 120), width=2*S)
    draw.line([(20*S, 42*S), (cx, cy)], fill=(220, 160, 255, 160), width=2*S)
    return finalize(img, S)

# R: Lôi Điện Tỏa (Chain Lightning) - Tia sét nảy 3 mục tiêu
def gen_mage_chain_lightning():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(240, 220, 60), inner_color=(24, 22, 10))
    # Tia sét gãy khúc ziczac
    pts = [(16*S, 16*S), (32*S, 26*S), (24*S, 34*S), (46*S, 48*S)]
    draw.line(pts, fill=(255, 245, 120), width=5*S)
    draw.line(pts, fill=(255, 255, 255), width=2*S)
    # Các nhánh sét phụ nảy ra
    draw.line([(32*S, 26*S), (48*S, 20*S)], fill=(230, 210, 80), width=3*S)
    draw.line([(24*S, 34*S), (14*S, 46*S)], fill=(230, 210, 80), width=3*S)
    return finalize(img, S)

# T: Nguyên Tố Hộ Thể (Mana Shield) - Hấp thụ sát thương hồi Mana
def gen_mage_mana_shield():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(60, 160, 240), inner_color=(12, 22, 34))
    cx, cy = 32 * S, 32 * S
    for r in range(24, 8, -3):
        draw.ellipse([cx - r*S, cy - r*S, cx + r*S, cy + r*S], fill=(50, 150, 255, 60))
    # Quả cầu khiên mana có hạt tinh thể
    draw.ellipse([cx - 18*S, cy - 18*S, cx + 18*S, cy + 18*S], outline=(160, 225, 255), width=3*S)
    draw.rectangle([cx - 5*S, cy - 10*S, cx + 5*S, cy + 10*S], fill=(220, 250, 255))
    draw.rectangle([cx - 10*S, cy - 5*S, cx + 10*S, cy + 5*S], fill=(220, 250, 255))
    return finalize(img, S)

# ==============================================================================
# 3. ARCHER SKILLS (XẠ THỦ)
# ==============================================================================

# Q: Xuyên Vân Tiễn (Piercing Shot) - Mũi tên xuyên thấu
def gen_archer_piercing_shot():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(60, 200, 160), inner_color=(10, 26, 20))
    # Mũi tên xanh ngọc lao xuyên
    draw.line([(14*S, 48*S), (50*S, 14*S)], fill=(220, 255, 235), width=4*S)
    draw.polygon([(50*S, 14*S), (54*S, 24*S), (40*S, 18*S)], fill=(80, 240, 180))
    # Luồng xoáy gió xuyên thấu
    cx, cy = 32*S, 31*S
    draw.arc([cx - 14*S, cy - 14*S, cx + 14*S, cy + 14*S], start=120, end=300, fill=(100, 240, 190, 180), width=3*S)
    return finalize(img, S)

# W: Mưa Tên (Volley / Barrage) - Chùm 5 tên nón làm chậm
def gen_archer_volley():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(120, 210, 80), inner_color=(14, 28, 12))
    # 5 mũi tên xòe hình quạt
    angles = [-30, -15, 0, 15, 30]
    cx, cy = 32 * S, 48 * S
    for a in angles:
        rad = math.radians(a - 90)
        x2 = cx + math.cos(rad) * 36 * S
        y2 = cy + math.sin(rad) * 36 * S
        draw.line([(cx, cy), (x2, y2)], fill=(210, 255, 140), width=2*S)
        draw.ellipse([x2 - 2*S, y2 - 2*S, x2 + 2*S, y2 + 2*S], fill=(255, 255, 200))
    return finalize(img, S)

# E: Nhào Lộn Thoát Thân (Tumble) - Lộn nhanh buff tốc đánh
def gen_archer_tumble():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(230, 190, 60), inner_color=(24, 20, 10))
    # Vòng tròn xoay lộn nhào 360
    cx, cy = 32 * S, 32 * S
    draw.arc([cx - 18*S, cy - 18*S, cx + 18*S, cy + 18*S], start=40, end=320, fill=(240, 210, 70), width=4*S)
    # Đầu mũi tên xoay
    draw.polygon([(46*S, 18*S), (54*S, 24*S), (44*S, 30*S)], fill=(255, 240, 140))
    # Giày linh hoạt
    draw.line([(24*S, 32*S), (38*S, 32*S)], fill=(255, 255, 255), width=3*S)
    return finalize(img, S)

# R: Bẫy Dây (Snare Trap) - Bẫy tàng hình trói chân
def gen_archer_snare_trap():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(160, 150, 140), inner_color=(18, 18, 20))
    # Bẫy gấu răng cưa kim loại
    cx, cy = 32 * S, 34 * S
    draw.ellipse([cx - 18*S, cy - 14*S, cx + 18*S, cy + 14*S], outline=(180, 190, 205), width=4*S)
    # Răng cưa nhọn
    for i in range(-14, 15, 6):
        draw.polygon([(cx + i*S, cy - 12*S), (cx + (i+3)*S, cy - 4*S), (cx + (i+6)*S, cy - 12*S)], fill=(220, 230, 245))
        draw.polygon([(cx + i*S, cy + 12*S), (cx + (i+3)*S, cy + 4*S), (cx + (i+6)*S, cy + 12*S)], fill=(220, 230, 245))
    return finalize(img, S)

# T: Ngắm Chuẩn Dứt Điểm (Deadeye) - Tâm ngắm tỉa kết liễu
def gen_archer_deadeye():
    img, draw, S = create_supersampled()
    draw_icon_base(draw, S, border_color=(235, 60, 60), inner_color=(28, 12, 12))
    cx, cy = 32 * S, 32 * S
    # Tâm ngắm đỏ rực
    draw.ellipse([cx - 20*S, cy - 20*S, cx + 20*S, cy + 20*S], outline=(255, 60, 60), width=3*S)
    draw.ellipse([cx - 12*S, cy - 12*S, cx + 12*S, cy + 12*S], outline=(255, 120, 120), width=2*S)
    draw.line([(cx - 24*S, cy), (cx + 24*S, cy)], fill=(255, 60, 60), width=2*S)
    draw.line([(cx, cy - 24*S), (cx, cy + 24*S)], fill=(255, 60, 60), width=2*S)
    draw.ellipse([cx - 3*S, cy - 3*S, cx + 3*S, cy + 3*S], fill=(255, 255, 255))
    return finalize(img, S)

# ==============================================================================
# 4. PET & SPIRIT ORB ICONS
# ==============================================================================
def gen_spirit_orb():
    img, draw, S = create_supersampled((48, 48))
    cx, cy = 24 * S, 24 * S
    # Quả cầu tinh linh thu phục
    for r in range(18, 4, -2):
        draw.ellipse([cx - r*S, cy - r*S, cx + r*S, cy + r*S], fill=(80, 180, 255, 100))
    draw.ellipse([cx - 14*S, cy - 14*S, cx + 14*S, cy + 14*S], fill=(30, 120, 230))
    # Vành đai hoàng kim
    draw.ellipse([cx - 16*S, cy - 8*S, cx + 16*S, cy + 8*S], outline=(255, 215, 60), width=3*S)
    draw.ellipse([cx - 4*S, cy - 4*S, cx + 4*S, cy + 4*S], fill=(255, 255, 255))
    return finalize(img, S)

def gen_pet_avatar_wolf():
    img, draw, S = create_supersampled((48, 48))
    # Đầu sói tuyết / linh thú
    draw.polygon([(10*S, 14*S), (18*S, 8*S), (24*S, 18*S)], fill=(180, 200, 220)) # tai T
    draw.polygon([(38*S, 14*S), (30*S, 8*S), (24*S, 18*S)], fill=(180, 200, 220)) # tai P
    draw.ellipse([14*S, 16*S, 34*S, 38*S], fill=(220, 235, 250)) # mặt
    draw.ellipse([18*S, 22*S, 22*S, 26*S], fill=(40, 160, 240)) # mắt T
    draw.ellipse([26*S, 22*S, 30*S, 22*S], fill=(40, 160, 240)) # mắt P
    draw.polygon([(22*S, 30*S), (26*S, 30*S), (24*S, 34*S)], fill=(40, 50, 60)) # mũi
    return finalize(img, S)

def main():
    print("Generating 15 Class Skills + Pet Icons...")
    # Warrior
    gen_warrior_cleave().save(os.path.join(SKILLS_DIR, "icon_skill_warrior_cleave.png"))
    gen_warrior_charge().save(os.path.join(SKILLS_DIR, "icon_skill_warrior_vanguard_charge.png"))
    gen_warrior_iron_bastion().save(os.path.join(SKILLS_DIR, "icon_skill_warrior_iron_bastion.png"))
    gen_warrior_tremor_slam().save(os.path.join(SKILLS_DIR, "icon_skill_warrior_tremor_slam.png"))
    gen_warrior_frenzied_strike().save(os.path.join(SKILLS_DIR, "icon_skill_warrior_frenzied_strike.png"))

    # Mage
    gen_mage_fire_orb().save(os.path.join(SKILLS_DIR, "icon_skill_mage_fire_orb.png"))
    gen_mage_frost_spikes().save(os.path.join(SKILLS_DIR, "icon_skill_mage_frost_spikes.png"))
    gen_mage_blink().save(os.path.join(SKILLS_DIR, "icon_skill_mage_blink.png"))
    gen_mage_chain_lightning().save(os.path.join(SKILLS_DIR, "icon_skill_mage_chain_lightning.png"))
    gen_mage_mana_shield().save(os.path.join(SKILLS_DIR, "icon_skill_mage_mana_shield.png"))

    # Archer
    gen_archer_piercing_shot().save(os.path.join(SKILLS_DIR, "icon_skill_archer_piercing_shot.png"))
    gen_archer_volley().save(os.path.join(SKILLS_DIR, "icon_skill_archer_volley.png"))
    gen_archer_tumble().save(os.path.join(SKILLS_DIR, "icon_skill_archer_tumble.png"))
    gen_archer_snare_trap().save(os.path.join(SKILLS_DIR, "icon_skill_archer_snare_trap.png"))
    gen_archer_deadeye().save(os.path.join(SKILLS_DIR, "icon_skill_archer_deadeye.png"))

    # Pet & Orb
    gen_spirit_orb().save(os.path.join(ICONS_DIR, "icon_spirit_orb.png"))
    gen_pet_avatar_wolf().save(os.path.join(ICONS_DIR, "icon_pet_wolf.png"))

    print("-> 15 Class Skills and Pet Icons generated successfully!")

if __name__ == "__main__":
    main()
