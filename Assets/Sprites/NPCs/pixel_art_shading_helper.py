import os
import hashlib
from PIL import Image

CANVAS_SIZE = 32
STANDARD_PPU = 16
CLR_CLEAR = (0, 0, 0, 0)


def hex_to_rgba(hex_str, alpha=255):
    """Chuyển mã màu Hex (#RRGGBB) sang tuple RGBA (R, G, B, A)."""
    h = hex_str.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), alpha)


class ColorRamp4:
    """
    Bảng màu 4 sắc độ bắt buộc cho mọi vùng màu Pixel Art:
    Outline (viền tối bao quanh) -> Shadow (vùng tối) -> Base (màu gốc) -> Highlight (điểm sáng / đan caro dithering).
    Tuyệt đối cấm tô mảng phẳng đơn sắc (FillRect).
    """
    def __init__(self, outline_hex, shadow_hex, base_hex, highlight_hex):
        self.outline = hex_to_rgba(outline_hex)
        self.shadow = hex_to_rgba(shadow_hex)
        self.base = hex_to_rgba(base_hex)
        self.base_color = self.base
        self.highlight = hex_to_rgba(highlight_hex)


class PixelArtShadingHelper:
    """
    Bộ hàm lõi vẽ Pixel Art Chibi 32x32 (PPU = 16) đồng bộ 100% với Player Class (player_novice_4direction):
    1. Bảng màu 4 sắc độ (Outline -> Shadow -> Base -> Highlight) kèm đan caro Dithering (x + y) % 2 == 0.
    2. Giải phẫu Chibi 32x32: Đầu to (Y=8..20, rộng 12px), mắt có điểm sáng trắng (#F7EFEB),
       2 tay tách nhẹ 2 bên hông có khe tối 1px, 2 chân tách rõ (X=12..14 và X=17..19) và có bóng mờ DropShadow.
    3. Đồng bộ lưới Pixel (No Mixels): Đạo cụ vẽ trên cùng lưới 32x32 với độ dày 1-2px, gắn sát vào tay nhân vật.
    """

    @staticmethod
    def create_canvas(width=CANVAS_SIZE, height=CANVAS_SIZE):
        return Image.new("RGBA", (width, height), CLR_CLEAR)

    @staticmethod
    def blend_pixel(dst, src):
        if src[3] == 0:
            return dst
        if src[3] == 255 or dst[3] == 0:
            return src
        sa = src[3] / 255.0
        da = dst[3] / 255.0
        out_a = sa + da * (1.0 - sa)
        if out_a <= 1e-5:
            return CLR_CLEAR
        out_r = int((src[0] * sa + dst[0] * da * (1.0 - sa)) / out_a)
        out_g = int((src[1] * sa + dst[1] * da * (1.0 - sa)) / out_a)
        out_b = int((src[2] * sa + dst[2] * da * (1.0 - sa)) / out_a)
        return (
            max(0, min(255, out_r)),
            max(0, min(255, out_g)),
            max(0, min(255, out_b)),
            max(0, min(255, int(out_a * 255.0))),
        )

    @staticmethod
    def put_px(img, x, y, color):
        if 0 <= x < img.width and 0 <= y < img.height and color is not None:
            if len(color) == 4 and color[3] < 255:
                existing = img.getpixel((x, y))
                img.putpixel((x, y), PixelArtShadingHelper.blend_pixel(existing, color))
            else:
                img.putpixel((x, y), color)

    @staticmethod
    def set_px(img, x, y, color):
        PixelArtShadingHelper.put_px(img, x, y, color)

    @staticmethod
    def draw_shaded_rect(img, x0, y0, x1, y1, ramp, light_dir=(-1, -1), dither_highlight=True):
        """
        Vẽ hình chữ nhật có viền tối (Dark Outline) bao quanh và đổ bóng 4 sắc độ + Dithering bên trong.
        """
        if x1 < x0 or y1 < y0:
            return
        w = max(1.0, float(x1 - x0))
        h = max(1.0, float(y1 - y0))
        lx = 0.25 if light_dir[0] < 0 else (0.75 if light_dir[0] > 0 else 0.5)
        ly = 0.25 if light_dir[1] < 0 else (0.75 if light_dir[1] > 0 else 0.5)

        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                is_edge = (x == x0 or x == x1 or y == y0 or y == y1)
                if is_edge and (w >= 2 and h >= 2):
                    PixelArtShadingHelper.put_px(img, x, y, ramp.outline)
                else:
                    nx = (x - x0) / w
                    ny = (y - y0) / h
                    dist = ((nx - lx) ** 2 + (ny - ly) ** 2) ** 0.5
                    light = max(0.05, min(0.95, 0.88 - dist * 0.85))
                    if dither_highlight:
                        c = PixelArtShadingHelper.sample_4tone_dither(ramp, light, x, y, False)
                    else:
                        if light >= 0.70:
                            c = ramp.highlight
                        elif light >= 0.38:
                            c = ramp.base
                        else:
                            c = ramp.shadow
                    PixelArtShadingHelper.put_px(img, x, y, c)

    @staticmethod
    def draw_rounded_shaded_box(img, x0, y0, x1, y1, ramp, corner_cut=1, light_dir=(-1, -1), dither=True):
        """
        Vẽ khối bo góc Chibi (đầu, mũ, thân áo, giỏ) có viền tối (Dark Outline) và đổ bóng 4 sắc độ + Dithering.
        """
        if x1 < x0 or y1 < y0:
            return
        w = max(1.0, float(x1 - x0))
        h = max(1.0, float(y1 - y0))
        lx = 0.30 if light_dir[0] < 0 else (0.70 if light_dir[0] > 0 else 0.5)
        ly = 0.25 if light_dir[1] < 0 else (0.75 if light_dir[1] > 0 else 0.5)

        def inside(px, py):
            if px < x0 or px > x1 or py < y0 or py > y1:
                return False
            dx = min(px - x0, x1 - px)
            dy = min(py - y0, y1 - py)
            if dx + dy < corner_cut:
                return False
            return True

        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                if not inside(x, y):
                    continue
                is_outline = (
                    not inside(x - 1, y)
                    or not inside(x + 1, y)
                    or not inside(x, y - 1)
                    or not inside(x, y + 1)
                )
                if is_outline:
                    PixelArtShadingHelper.put_px(img, x, y, ramp.outline)
                else:
                    nx = (x - x0) / w
                    ny = (y - y0) / h
                    dist = ((nx - lx) ** 2 + (ny - ly) ** 2) ** 0.5
                    light = max(0.05, min(0.95, 0.90 - dist * 0.85))
                    if dither:
                        c = PixelArtShadingHelper.sample_4tone_dither(ramp, light, x, y, False)
                    else:
                        if light >= 0.72:
                            c = ramp.highlight
                        elif light >= 0.38:
                            c = ramp.base
                        else:
                            c = ramp.shadow
                    PixelArtShadingHelper.put_px(img, x, y, c)

    @staticmethod
    def sample_4tone_dither(ramp, light_factor, x, y, is_outline=False):
        if is_outline:
            return ramp.outline
        checker = ((x + y) & 1) == 0
        if light_factor >= 0.80:
            return ramp.highlight
        if light_factor >= 0.56:
            return ramp.highlight if checker else ramp.base
        if light_factor >= 0.36:
            return ramp.base
        if light_factor >= 0.20:
            return ramp.base if checker else ramp.shadow
        return ramp.shadow

    @staticmethod
    def draw_drop_shadow(img, center_x=15, center_y=29, rx=5, ry=1, cx=None, cy=None):
        """
        Vẽ bóng đen mờ hình elip dưới bàn chân (DropShadow) bắt buộc cho mọi nhân vật Chibi 32x32.
        """
        if cx is not None:
            center_x = cx
        if cy is not None:
            center_y = cy
        core_shadow = (16, 10, 22, 140)
        edge_shadow = (16, 10, 22, 75)
        for y in range(center_y - ry - 1, center_y + ry + 2):
            for x in range(center_x - rx - 1, center_x + rx + 3):
                dx = (x - (center_x + 0.5)) / max(1.0, float(rx))
                dy = (y - float(center_y)) / max(1.0, float(ry + 0.5))
                dist_sq = dx * dx + dy * dy
                if dist_sq <= 0.85:
                    PixelArtShadingHelper.put_px(img, x, y, core_shadow)
                elif dist_sq <= 1.25:
                    PixelArtShadingHelper.put_px(img, x, y, edge_shadow)

    @staticmethod
    def paint_shaded_span(img, y, x_start, x_end, ramp, highlight_center=15.0, dither=True, outline_edges=True):
        """
        Vẽ một dải ngang có viền tối 2 đầu và đổ bóng 4 sắc độ + đan caro Dithering ở giữa.
        """
        if x_end < x_start:
            return
        span = max(1.0, float(x_end - x_start))
        for x in range(x_start, x_end + 1):
            if outline_edges and (x == x_start or x == x_end):
                PixelArtShadingHelper.put_px(img, x, y, ramp.outline)
            else:
                dist = abs(x - highlight_center) / (span * 0.55)
                light = max(0.10, min(0.95, 0.82 - dist * 0.58))
                if not dither:
                    if light >= 0.68:
                        c = ramp.highlight
                    elif light >= 0.40:
                        c = ramp.base
                    else:
                        c = ramp.shadow
                else:
                    c = PixelArtShadingHelper.sample_4tone_dither(ramp, light, x, y, False)
                PixelArtShadingHelper.put_px(img, x, y, c)

    @staticmethod
    def apply_dark_outline(img, outline_rgba=(24, 18, 30, 255), alpha_threshold=120):
        """
        Đảm bảo mọi vùng điểm ảnh đặc đều có viền tối (Dark Outline) bao quanh sắc nét.
        """
        w, h = img.size
        src = img.copy()
        out = img.copy()
        for y in range(h):
            for x in range(w):
                r, g, b, a = src.getpixel((x, y))
                if a == 0:
                    has_solid_neighbor = False
                    for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                        nx, ny = x + dx, y + dy
                        if 0 <= nx < w and 0 <= ny < h:
                            if src.getpixel((nx, ny))[3] >= alpha_threshold:
                                has_solid_neighbor = True
                                break
                    if has_solid_neighbor:
                        out.putpixel((x, y), outline_rgba)
        return out

    @staticmethod
    def write_unity_meta(png_path, ppu=STANDARD_PPU):
        """
        Đảm bảo file .meta của Unity luôn giữ đúng GUID hiện có (nếu file đã tồn tại)
        và ép chuẩn PPU = 16, filterMode = 0 (Point), Uncompressed.
        """
        meta_path = png_path + ".meta"
        rel_path = png_path.replace("\\", "/")
        existing_guid = None
        if os.path.exists(meta_path):
            with open(meta_path, "r", encoding="utf-8", errors="ignore") as f:
                for line in f:
                    if line.strip().startswith("guid:"):
                        existing_guid = line.split("guid:")[1].strip()
                        break
        if not existing_guid:
            existing_guid = hashlib.md5(rel_path.encode("utf-8")).hexdigest()

        meta_content = f"""fileFormatVersion: 2
guid: {existing_guid}
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
  spritePixelsToUnits: {ppu}
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
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(meta_content)

    @staticmethod
    def save_sprite_with_meta(img, path, ppu=STANDARD_PPU):
        os.makedirs(os.path.dirname(path), exist_ok=True)
        img.save(path)
        PixelArtShadingHelper.write_unity_meta(path, ppu=ppu)
