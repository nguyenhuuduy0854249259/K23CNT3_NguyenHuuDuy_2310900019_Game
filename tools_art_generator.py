import math
import os
from PIL import Image, ImageDraw, ImageFilter

HUD_DIR = r"d:\Unity\prj1\gam\Assets\Sprites\UI\HUD"
SKILLS_DIR = os.path.join(HUD_DIR, "Skills")
FRAMES_DIR = os.path.join(HUD_DIR, "Frames")
ICONS_DIR = os.path.join(HUD_DIR, "Icons")

os.makedirs(SKILLS_DIR, exist_ok=True)
os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(ICONS_DIR, exist_ok=True)

# Helper: Create super-sampled image (render at 4x then downscale with high-quality Lanczos for buttery smooth crisp edges)
def create_supersampled(size, scale=4):
    w, h = size
    img = Image.new("RGBA", (w * scale, h * scale), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img), scale

def finalize(img, scale=4):
    w, h = img.size[0] // scale, img.size[1] // scale
    return img.resize((w, h), Image.Resampling.LANCZOS)

# ---------------------------------------------------------------------------
# 1. SKILL ICONS (64x64 resolution, downsampled from 256x256)
# ---------------------------------------------------------------------------

def draw_icon_base(draw, S, border_color=(180, 140, 60), inner_color=(15, 18, 26)):
    # Dark beveled background with metallic border
    pad = 4 * S
    draw.rounded_rectangle([pad, pad, 64*S - pad, 64*S - pad], radius=6*S, fill=inner_color)
    # Inner dark vignette
    draw.rounded_rectangle([pad + 2*S, pad + 2*S, 64*S - pad - 2*S, 64*S - pad - 2*S], radius=5*S, outline=(8, 10, 16, 255), width=2*S)
    # Outer gold/steel border
    draw.rounded_rectangle([pad, pad, 64*S - pad, 64*S - pad], radius=6*S, outline=border_color, width=2*S)
    # Corner rivets
    for cx, cy in [(pad+3*S, pad+3*S), (64*S-pad-3*S, pad+3*S), (pad+3*S, 64*S-pad-3*S), (64*S-pad-3*S, 64*S-pad-3*S)]:
        draw.ellipse([cx-1.5*S, cy-1.5*S, cx+1.5*S, cy+1.5*S], fill=(240, 210, 120))

def generate_fan_slash():
    img, draw, S = create_supersampled((64, 64))
    draw_icon_base(draw, S, border_color=(100, 170, 230), inner_color=(10, 16, 28))
    
    # Glowing wind arc
    cx, cy = 16 * S, 48 * S
    # Multiple layered arcs for energy glow
    for r in range(40, 12, -2):
        alpha = int(40 + (40 - r) * 4)
        alpha = max(10, min(200, alpha))
        draw.arc([cx - r*S, cy - r*S, cx + r*S, cy + r*S], start=280, end=370, fill=(30, 140, 240, alpha), width=3*S)
    
    # Sharp primary blade slash arc
    draw.arc([cx - 32*S, cy - 32*S, cx + 32*S, cy + 32*S], start=285, end=365, fill=(180, 235, 255, 255), width=4*S)
    draw.arc([cx - 32*S, cy - 32*S, cx + 32*S, cy + 32*S], start=295, end=355, fill=(255, 255, 255, 255), width=2*S)
    
    # Secondary inner slash
    draw.arc([cx - 22*S, cy - 22*S, cx + 22*S, cy + 22*S], start=290, end=360, fill=(100, 200, 255, 220), width=3*S)
    
    # Steel katana blade diagonal
    blade_poly = [
        (12*S, 52*S),
        (18*S, 46*S),
        (52*S, 14*S),
        (54*S, 12*S),
        (50*S, 18*S),
        (16*S, 54*S)
    ]
    draw.polygon(blade_poly, fill=(220, 235, 255))
    draw.line([(18*S, 46*S), (54*S, 12*S)], fill=(255, 255, 255), width=2*S)
    # Hilt / guard
    draw.line([(13*S, 45*S), (21*S, 53*S)], fill=(230, 180, 50), width=3*S)
    draw.line([(9*S, 51*S), (15*S, 57*S)], fill=(80, 40, 20), width=4*S)
    
    # Sparkle motes
    for sx, sy in [(44, 20), (32, 14), (52, 28), (24, 22), (48, 40)]:
        draw.ellipse([sx*S-1.5*S, sy*S-1.5*S, sx*S+1.5*S, sy*S+1.5*S], fill=(220, 245, 255, 230))
        
    return finalize(img, S)

def generate_flurry_slash():
    img, draw, S = create_supersampled((64, 64))
    draw_icon_base(draw, S, border_color=(235, 110, 30), inner_color=(28, 12, 10))
    
    # Multi-directional fiery cross-slashes
    slashes = [
        ((12*S, 16*S), (52*S, 48*S), (255, 80, 20)),
        ((14*S, 48*S), (50*S, 14*S), (255, 140, 30)),
        ((8*S, 32*S), (56*S, 30*S), (255, 200, 40)),
        ((22*S, 10*S), (42*S, 54*S), (255, 60, 20)),
        ((16*S, 24*S), (48*S, 40*S), (255, 240, 100))
    ]
    
    # Glow pass
    for p1, p2, col in slashes:
        draw.line([p1, p2], fill=col + (100,), width=7*S)
    # Sharp pass
    for p1, p2, col in slashes:
        draw.line([p1, p2], fill=col + (230,), width=3*S)
    # Core white hot centers
    for p1, p2, _ in slashes[:3]:
        draw.line([p1, p2], fill=(255, 255, 240, 240), width=1*S)
        
    # Central impact explosion burst
    cx, cy = 32*S, 31*S
    for r in range(12, 1, -2):
        draw.ellipse([cx - r*S, cy - r*S, cx + r*S, cy + r*S], fill=(255, 180 + r*5, 50, 140))
    draw.ellipse([cx - 3*S, cy - 3*S, cx + 3*S, cy + 3*S], fill=(255, 255, 255, 255))
    
    # Fiery embers
    for ex, ey, er in [(18, 18, 1.5), (46, 18, 2), (48, 44, 1.5), (20, 42, 2), (32, 12, 2.5), (32, 50, 2)]:
        draw.ellipse([ex*S-er*S, ey*S-er*S, ex*S+er*S, ey*S+er*S], fill=(255, 220, 90, 240))
        
    return finalize(img, S)

def generate_aegis_barrier():
    img, draw, S = create_supersampled((64, 64))
    draw_icon_base(draw, S, border_color=(230, 190, 60), inner_color=(14, 18, 28))
    
    # Radiant holy halo behind shield
    cx, cy = 32*S, 32*S
    for r in range(24, 12, -2):
        draw.ellipse([cx - r*S, cy - r*S, cx + r*S, cy + r*S], fill=(70, 160, 240, 30))
        
    # Majestic Shield Outline & Body
    shield_pts = [
        (18*S, 16*S), # top left
        (46*S, 16*S), # top right
        (46*S, 34*S), # mid right
        (32*S, 52*S), # bottom tip
        (18*S, 34*S), # mid left
    ]
    # Gold rim
    draw.polygon(shield_pts, fill=(225, 175, 45))
    # Inner steel plate
    inner_shield = [
        (21*S, 19*S),
        (43*S, 19*S),
        (43*S, 33*S),
        (32*S, 48*S),
        (21*S, 33*S),
    ]
    draw.polygon(inner_shield, fill=(28, 48, 75))
    
    # Shield shading (split left light, right dark)
    half_shield = [
        (21*S, 19*S),
        (32*S, 19*S),
        (32*S, 48*S),
        (21*S, 33*S),
    ]
    draw.polygon(half_shield, fill=(38, 68, 105))
    
    # Glowing sapphire rune / cross in center
    draw.line([(32*S, 22*S), (32*S, 42*S)], fill=(120, 220, 255), width=3*S)
    draw.line([(24*S, 29*S), (40*S, 29*S)], fill=(120, 220, 255), width=3*S)
    # Core gem
    gem = [(32*S, 26*S), (35*S, 29*S), (32*S, 32*S), (29*S, 29*S)]
    draw.polygon(gem, fill=(220, 245, 255))
    
    # Outer barrier energy aura (hexagonal ring or arcs)
    draw.arc([10*S, 10*S, 54*S, 54*S], start=30, end=150, fill=(100, 200, 255, 160), width=2*S)
    draw.arc([10*S, 10*S, 54*S, 54*S], start=210, end=330, fill=(100, 200, 255, 160), width=2*S)
    
    return finalize(img, S)

def generate_quad_dash():
    img, draw, S = create_supersampled((64, 64))
    draw_icon_base(draw, S, border_color=(60, 210, 210), inner_color=(10, 22, 26))
    
    # 4 Ghostly motion blur silhouettes advancing diagonally
    dash_steps = [
        (12, 40, 0.25, (30, 100, 140)),
        (20, 32, 0.45, (40, 150, 190)),
        (28, 24, 0.70, (60, 210, 240)),
        (36, 16, 1.00, (220, 255, 255))
    ]
    
    # Speed wind lines
    for ly in [16, 26, 36, 46]:
        draw.line([(10*S, ly*S), (34*S, (ly-4)*S)], fill=(60, 190, 220, 90), width=2*S)
        
    for bx, by, alpha_scale, col in dash_steps:
        # Draw winged boot profile
        pts = [
            ((bx + 2)*S, (by + 16)*S),
            ((bx + 18)*S, (by + 16)*S),
            ((bx + 20)*S, (by + 12)*S),
            ((bx + 12)*S, (by + 8)*S),
            ((bx + 10)*S, (by + 2)*S),
            ((bx + 4)*S, (by + 2)*S),
            ((bx + 4)*S, (by + 10)*S),
        ]
        alpha = int(255 * alpha_scale)
        draw.polygon(pts, fill=col + (alpha,))
        
        # Wing feathers on boot
        wing = [
            ((bx + 2)*S, (by + 4)*S),
            ((bx - 6)*S, (by - 2)*S),
            ((bx + 2)*S, (by + 1)*S),
            ((bx - 4)*S, (by + 5)*S),
            ((bx + 3)*S, (by + 6)*S),
        ]
        draw.polygon(wing, fill=(col[0], col[1], min(255, col[2]+30), alpha))

    # Lead boot detail & highlight
    draw.line([(38*S, 32*S), (54*S, 32*S)], fill=(255, 255, 255), width=2*S)
    draw.ellipse([46*S, 20*S, 49*S, 23*S], fill=(255, 255, 255, 240))
    
    return finalize(img, S)

def generate_holy_heal():
    img, draw, S = create_supersampled((64, 64))
    draw_icon_base(draw, S, border_color=(80, 210, 110), inner_color=(10, 26, 18))
    
    cx, cy = 32*S, 32*S
    # Divine radial bloom
    for r in range(24, 6, -2):
        draw.ellipse([cx - r*S, cy - r*S, cx + r*S, cy + r*S], fill=(50, 220, 120, 35))
        
    # Divine rays
    for angle in range(0, 360, 45):
        rad = math.radians(angle)
        x2 = cx + math.cos(rad) * 26 * S
        y2 = cy + math.sin(rad) * 26 * S
        draw.line([(cx, cy), (x2, y2)], fill=(120, 255, 170, 70), width=2*S)

    # Holy Cross shape with ornate beveled tips
    cross_w = 6 * S
    cross_h = 28 * S
    # Vertical arm
    draw.rounded_rectangle([cx - cross_w, cy - cross_h//2, cx + cross_w, cy + cross_h//2], radius=2*S, fill=(35, 160, 85))
    # Horizontal arm
    draw.rounded_rectangle([cx - cross_h//2, cy - cross_w, cx + cross_h//2, cy + cross_w], radius=2*S, fill=(35, 160, 85))
    
    # Inner bright holy green core
    in_w = 3 * S
    in_h = 24 * S
    draw.rounded_rectangle([cx - in_w, cy - in_h//2, cx + in_w, cy + in_h//2], radius=1*S, fill=(120, 245, 160))
    draw.rounded_rectangle([cx - in_h//2, cy - in_w, cx + in_h//2, cy + in_w], radius=1*S, fill=(120, 245, 160))
    
    # Brilliant white core sparkle
    draw.ellipse([cx - 4*S, cy - 4*S, cx + 4*S, cy + 4*S], fill=(255, 255, 255, 255))
    
    # Floating healing orbs
    for ox, oy, orad in [(20, 18, 2.5), (44, 18, 2.5), (18, 44, 2), (46, 44, 2), (32, 10, 3)]:
        draw.ellipse([ox*S-orad*S, oy*S-orad*S, ox*S+orad*S, oy*S+orad*S], fill=(160, 255, 200, 230))
        draw.ellipse([ox*S-1*S, oy*S-1*S, ox*S+1*S, oy*S+1*S], fill=(255, 255, 255, 255))
        
    return finalize(img, S)

# ---------------------------------------------------------------------------
# 2. FRAMES & BARS (Sliced Frames with 9-Slice Friendly Borders)
# ---------------------------------------------------------------------------

def generate_frame_skill_slot():
    # 64x64 slot frame, border 8px on all sides
    img, draw, S = create_supersampled((64, 64))
    
    # Outer dark shadow
    draw.rectangle([0, 0, 64*S, 64*S], fill=(0, 0, 0, 0))
    # Dark slate background fill (center area will be covered or transparent)
    draw.rectangle([6*S, 6*S, 58*S, 58*S], fill=(18, 20, 28, 240))
    
    # Metallic gold frame bevel (outer ring)
    # Top border
    draw.rectangle([2*S, 2*S, 62*S, 8*S], fill=(180, 140, 50))
    # Left border
    draw.rectangle([2*S, 2*S, 8*S, 62*S], fill=(160, 120, 40))
    # Right border
    draw.rectangle([56*S, 2*S, 62*S, 62*S], fill=(120, 90, 30))
    # Bottom border
    draw.rectangle([2*S, 56*S, 62*S, 62*S], fill=(100, 75, 25))
    
    # Inner dark groove (shadow)
    draw.rectangle([8*S, 8*S, 56*S, 11*S], fill=(10, 12, 18))
    draw.rectangle([8*S, 8*S, 11*S, 56*S], fill=(10, 12, 18))
    draw.rectangle([53*S, 8*S, 56*S, 56*S], fill=(40, 45, 60))
    draw.rectangle([8*S, 53*S, 56*S, 56*S], fill=(40, 45, 60))
    
    # Corner rivets / gems
    corners = [(4*S, 4*S), (60*S, 4*S), (4*S, 60*S), (60*S, 60*S)]
    for cx, cy in corners:
        draw.rectangle([cx-2*S, cy-2*S, cx+2*S, cy+2*S], fill=(240, 210, 110))
        draw.point((cx, cy), fill=(255, 255, 255))
        
    return finalize(img, S)

def generate_frame_hud_bar_bg():
    # 64x24 bar background casing (border 6px)
    img, draw, S = create_supersampled((64, 24))
    
    # Recessed trench background
    draw.rectangle([0, 0, 64*S, 24*S], fill=(12, 14, 18, 255))
    # Dark cavity
    draw.rectangle([4*S, 4*S, 60*S, 20*S], fill=(6, 7, 10, 255))
    
    # Wrought iron border
    draw.rectangle([0, 0, 64*S, 3*S], fill=(70, 80, 95)) # top rim
    draw.rectangle([0, 21*S, 64*S, 24*S], fill=(30, 35, 45)) # bottom rim
    draw.rectangle([0, 0, 3*S, 24*S], fill=(60, 70, 85)) # left rim
    draw.rectangle([61*S, 0, 64*S, 24*S], fill=(35, 40, 50)) # right rim
    
    # Gold decorative corner brackets
    for x in [1*S, 2*S, 3*S, 61*S, 62*S, 63*S]:
        draw.line([(x, 1*S), (x, 5*S)], fill=(190, 155, 55))
        draw.line([(x, 19*S), (x, 23*S)], fill=(150, 120, 40))

    return finalize(img, S)

def generate_bar_fill(color_top, color_mid, color_bot):
    # 64x20 gradient bar fill with top specular reflection
    img, draw, S = create_supersampled((64, 20))
    for y in range(20):
        t = y / 19.0
        if y < 4:
            # Glossy highlight on top
            r = int(color_top[0] + (color_mid[0] - color_top[0]) * (y / 4.0))
            g = int(color_top[1] + (color_mid[1] - color_top[1]) * (y / 4.0))
            b = int(color_top[2] + (color_mid[2] - color_top[2]) * (y / 4.0))
        else:
            t_sub = (y - 4) / 15.0
            r = int(color_mid[0] + (color_bot[0] - color_mid[0]) * t_sub)
            g = int(color_mid[1] + (color_bot[1] - color_mid[1]) * t_sub)
            b = int(color_mid[2] + (color_bot[2] - color_mid[2]) * t_sub)
        draw.line([(0, y*S), (64*S, y*S)], fill=(r, g, b, 255), width=S)
        
    return finalize(img, S)

def generate_frame_portrait():
    # 64x64 portrait frame with royal gothic crest corners
    img, draw, S = create_supersampled((64, 64))
    
    # Dark velvet background
    draw.rectangle([4*S, 4*S, 60*S, 60*S], fill=(16, 14, 24, 255))
    
    # Heavy ornate golden rim
    draw.rectangle([2*S, 2*S, 62*S, 6*S], fill=(225, 185, 70))
    draw.rectangle([2*S, 2*S, 6*S, 62*S], fill=(195, 155, 50))
    draw.rectangle([58*S, 2*S, 62*S, 62*S], fill=(145, 115, 35))
    draw.rectangle([2*S, 58*S, 62*S, 62*S], fill=(125, 95, 25))
    
    # Gemstone corner studs (ruby in gold setting)
    for cx, cy in [(6*S, 6*S), (58*S, 6*S), (6*S, 58*S), (58*S, 58*S)]:
        draw.ellipse([cx-4*S, cy-4*S, cx+4*S, cy+4*S], fill=(240, 200, 80))
        draw.ellipse([cx-2.5*S, cy-2.5*S, cx+2.5*S, cy+2.5*S], fill=(220, 40, 50))
        draw.point((cx-1*S, cy-1*S), fill=(255, 255, 255))
        
    return finalize(img, S)

def generate_frame_boss_bar():
    # 64x24 boss bar with abyssal dark purple trim and dragon horn studs
    img, draw, S = create_supersampled((64, 24))
    draw.rectangle([0, 0, 64*S, 24*S], fill=(14, 8, 20, 255))
    draw.rectangle([4*S, 4*S, 60*S, 20*S], fill=(6, 4, 10, 255))
    
    # Void rune border
    draw.rectangle([0, 0, 64*S, 3*S], fill=(150, 60, 200))
    draw.rectangle([0, 21*S, 64*S, 24*S], fill=(70, 20, 100))
    draw.rectangle([0, 0, 3*S, 24*S], fill=(120, 40, 170))
    draw.rectangle([61*S, 0, 64*S, 24*S], fill=(70, 20, 100))
    
    return finalize(img, S)

# ---------------------------------------------------------------------------
# 3. MENU ICONS (48x48 resolution)
# ---------------------------------------------------------------------------

def generate_icon_backpack():
    img, draw, S = create_supersampled((48, 48))
    # Rich brown leather explorer satchel
    draw.rounded_rectangle([10*S, 14*S, 38*S, 42*S], radius=4*S, fill=(135, 75, 35))
    # Flap
    draw.rounded_rectangle([10*S, 12*S, 38*S, 26*S], radius=3*S, fill=(160, 95, 45))
    # Leather straps
    draw.rectangle([15*S, 12*S, 19*S, 38*S], fill=(95, 48, 20))
    draw.rectangle([29*S, 12*S, 33*S, 38*S], fill=(95, 48, 20))
    # Gold buckles
    draw.rectangle([14*S, 24*S, 20*S, 29*S], fill=(235, 190, 60))
    draw.rectangle([28*S, 24*S, 34*S, 29*S], fill=(235, 190, 60))
    # Top carry loop
    draw.arc([18*S, 6*S, 30*S, 16*S], start=180, end=360, fill=(95, 48, 20), width=3*S)
    return finalize(img, S)

def generate_icon_character():
    img, draw, S = create_supersampled((48, 48))
    # Knight helmet profile
    # Helmet dome
    draw.ellipse([14*S, 8*S, 34*S, 28*S], fill=(190, 205, 225))
    # Golden crest on top
    draw.polygon([(24*S, 4*S), (21*S, 12*S), (27*S, 12*S)], fill=(240, 195, 60))
    # Visor slit
    draw.rectangle([16*S, 18*S, 32*S, 22*S], fill=(20, 25, 35))
    # Glowing eye slit
    draw.line([(18*S, 20*S), (30*S, 20*S)], fill=(80, 200, 255), width=2*S)
    # Bevel jaw guard
    jaw = [(15*S, 22*S), (33*S, 22*S), (29*S, 38*S), (19*S, 38*S)]
    draw.polygon(jaw, fill=(150, 165, 185))
    # Shoulder pauldrons
    draw.polygon([(8*S, 44*S), (24*S, 34*S), (40*S, 44*S)], fill=(120, 135, 155))
    return finalize(img, S)

def generate_icon_settings():
    img, draw, S = create_supersampled((48, 48))
    # Polished brass cog / gear
    cx, cy = 24*S, 24*S
    # 8 teeth
    for i in range(8):
        ang = i * (math.pi / 4.0)
        tx = cx + math.cos(ang) * 16 * S
        ty = cy + math.sin(ang) * 16 * S
        draw.rectangle([tx - 4*S, ty - 4*S, tx + 4*S, ty + 4*S], fill=(190, 150, 50))
    # Gear outer circle
    draw.ellipse([cx - 15*S, cy - 15*S, cx + 15*S, cy + 15*S], fill=(225, 180, 60))
    # Inner dark hole
    draw.ellipse([cx - 7*S, cy - 7*S, cx + 7*S, cy + 7*S], fill=(20, 22, 28))
    # Center pin
    draw.ellipse([cx - 3*S, cy - 3*S, cx + 3*S, cy + 3*S], fill=(240, 215, 120))
    return finalize(img, S)

def generate_icon_demo_class():
    img, draw, S = create_supersampled((48, 48))
    # Crossed dual broadswords with blazing golden blades
    # Blade 1 (top-left to bottom-right)
    draw.line([(10*S, 10*S), (38*S, 38*S)], fill=(225, 240, 255), width=3*S)
    draw.line([(8*S, 12*S), (14*S, 6*S)], fill=(240, 190, 50), width=3*S)
    draw.line([(38*S, 38*S), (42*S, 42*S)], fill=(140, 80, 30), width=3*S)
    
    # Blade 2 (top-right to bottom-left)
    draw.line([(38*S, 10*S), (10*S, 38*S)], fill=(225, 240, 255), width=3*S)
    draw.line([(34*S, 6*S), (40*S, 12*S)], fill=(240, 190, 50), width=3*S)
    draw.line([(10*S, 38*S), (6*S, 42*S)], fill=(140, 80, 30), width=3*S)
    
    # Center clash spark
    draw.ellipse([21*S, 21*S, 27*S, 27*S], fill=(255, 240, 160, 240))
    return finalize(img, S)

# ---------------------------------------------------------------------------
# 4. POTION ICONS FOR QUICK SLOTS
# ---------------------------------------------------------------------------

def generate_potion(liquid_col, glow_col):
    img, draw, S = create_supersampled((48, 48))
    # Glass flask shape: neck + round bulb
    cx, cy = 24*S, 27*S
    # Liquid glow
    draw.ellipse([cx - 15*S, cy - 13*S, cx + 15*S, cy + 17*S], fill=glow_col + (90,))
    # Liquid body
    draw.ellipse([cx - 13*S, cy - 11*S, cx + 13*S, cy + 15*S], fill=liquid_col + (240,))
    # Flask neck
    draw.rectangle([cx - 5*S, 8*S, cx + 5*S, cy - 6*S], fill=(160, 200, 230, 150))
    # Cork stopper
    draw.rectangle([cx - 4*S, 5*S, cx + 4*S, 9*S], fill=(160, 110, 60))
    # Glass highlight reflection
    draw.arc([cx - 11*S, cy - 9*S, cx + 11*S, cy + 13*S], start=140, end=240, fill=(255, 255, 255, 220), width=2*S)
    return finalize(img, S)

def main():
    print("Generating ultra-polished Lumina Fantasy RPG HUD assets...")
    
    # 1. Skills
    generate_fan_slash().save(os.path.join(SKILLS_DIR, "icon_skill_fan_slash.png"))
    generate_flurry_slash().save(os.path.join(SKILLS_DIR, "icon_skill_flurry_slash.png"))
    generate_aegis_barrier().save(os.path.join(SKILLS_DIR, "icon_skill_aegis_barrier.png"))
    generate_quad_dash().save(os.path.join(SKILLS_DIR, "icon_skill_quad_dash.png"))
    generate_holy_heal().save(os.path.join(SKILLS_DIR, "icon_skill_holy_heal.png"))
    print("-> 5 Skill Icons saved.")
    
    # 2. Frames
    generate_frame_skill_slot().save(os.path.join(FRAMES_DIR, "frame_skill_slot.png"))
    generate_frame_hud_bar_bg().save(os.path.join(FRAMES_DIR, "frame_hud_bar_bg.png"))
    generate_bar_fill((255, 120, 130), (220, 45, 55), (130, 15, 20)).save(os.path.join(FRAMES_DIR, "fill_hp_bar.png"))
    generate_bar_fill((140, 220, 255), (35, 140, 240), (15, 60, 160)).save(os.path.join(FRAMES_DIR, "fill_mp_bar.png"))
    generate_bar_fill((160, 255, 170), (45, 210, 85), (15, 110, 40)).save(os.path.join(FRAMES_DIR, "fill_exp_bar.png"))
    generate_frame_portrait().save(os.path.join(FRAMES_DIR, "frame_portrait.png"))
    generate_frame_boss_bar().save(os.path.join(FRAMES_DIR, "frame_boss_bar.png"))
    print("-> 7 UI Frames & Fills saved.")
    
    # 3. Menu Icons
    generate_icon_backpack().save(os.path.join(ICONS_DIR, "icon_backpack.png"))
    generate_icon_character().save(os.path.join(ICONS_DIR, "icon_character.png"))
    generate_icon_settings().save(os.path.join(ICONS_DIR, "icon_settings.png"))
    generate_icon_demo_class().save(os.path.join(ICONS_DIR, "icon_demo_class.png"))
    print("-> 4 Menu Icons saved.")
    
    # 4. Potions for Quick Slots
    generate_potion((220, 40, 50), (255, 90, 100)).save(os.path.join(ICONS_DIR, "icon_potion_hp.png"))
    generate_potion((35, 130, 240), (80, 180, 255)).save(os.path.join(ICONS_DIR, "icon_potion_mp.png"))
    print("-> 2 Potion Quick Slot Icons saved.")

if __name__ == "__main__":
    main()
