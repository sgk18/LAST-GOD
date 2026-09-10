import os
from PIL import Image, ImageDraw, ImageFont

def generate_lab_trim_sheet():
    W, H = 2048, 2048
    im = Image.new("RGBA", (W, H), (16, 20, 28, 255)) # #10141C base
    draw = ImageDraw.Draw(im)

    # ── 1. BULKHEAD PANELS (Top half: Y = 0 to 800) ──
    # Large dark metallic panels with recessed seams
    for x in range(0, W, 256):
        draw.rectangle([x, 0, x + 252, 400], fill=(27, 36, 48, 255)) # #1B2430
        draw.rectangle([x, 404, x + 252, 800], fill=(37, 45, 54, 255)) # #252D36
        # Edge bevels
        draw.line([x, 0, x + 252, 0], fill=(58, 63, 71, 255), width=3)
        draw.line([x, 0, x, 400], fill=(58, 63, 71, 255), width=3)
        draw.line([x + 252, 0, x + 252, 400], fill=(10, 12, 16, 255), width=3)
        draw.line([x, 400, x + 252, 400], fill=(10, 12, 16, 255), width=3)
        # Hex bolts at corners
        for bx, by in [(x + 16, 16), (x + 236, 16), (x + 16, 384), (x + 236, 384)]:
            draw.ellipse([bx - 6, by - 6, bx + 6, by + 6], fill=(58, 63, 71, 255), outline=(10, 12, 16, 255), width=2)

    # ── 2. FLOOR & GRATING SLABS (Y = 800 to 1300) ──
    # Floor plates
    draw.rectangle([0, 800, W, 1050], fill=(10, 12, 16, 255)) # #0A0C10
    for x in range(0, W, 128):
        draw.line([x, 800, x, 1050], fill=(27, 36, 48, 255), width=4)
    for y in range(800, 1050, 64):
        draw.line([0, y, W, y], fill=(27, 36, 48, 255), width=4)

    # Metal Grating / Slats (Y = 1050 to 1300)
    draw.rectangle([0, 1050, W, 1300], fill=(20, 25, 33, 255))
    for x in range(0, W, 32):
        draw.rectangle([x + 4, 1060, x + 28, 1290], fill=(10, 12, 16, 255), outline=(45, 52, 65, 255), width=2)
        # Slat horizontal lines
        for sy in range(1070, 1280, 20):
            draw.line([x + 6, sy, x + 26, sy], fill=(58, 63, 71, 255), width=2)

    # ── 3. HAZARD WARNING STRIPES (Y = 1300 to 1500) ──
    draw.rectangle([0, 1300, W, 1500], fill=(196, 80, 46, 255)) # Warning red-orange #C4502E
    # Angled black warning chevrons
    for x in range(-200, W + 200, 80):
        points = [(x, 1500), (x + 50, 1500), (x + 120, 1300), (x + 70, 1300)]
        draw.polygon(points, fill=(10, 12, 16, 255))

    # ── 4. CONSOLE & TERMINAL MONITORS (Y = 1500 to 1800) ──
    draw.rectangle([0, 1500, W, 1800], fill=(8, 13, 20, 255))
    # Draw screen panels with cyan interfaces
    for sx in range(0, W, 512):
        draw.rectangle([sx + 16, 1516, sx + 496, 1784], fill=(12, 20, 32, 255), outline=(45, 52, 65, 255), width=4)
        # Grid lines
        for gx in range(sx + 32, sx + 480, 32):
            draw.line([gx, 1530, gx, 1770], fill=(20, 35, 50, 255), width=1)
        for gy in range(1530, 1770, 30):
            draw.line([sx + 32, gy, sx + 480, gy], fill=(20, 35, 50, 255), width=1)
        # Oscilloscope / Cyan waveform
        wave_pts = []
        for i in range(sx + 40, sx + 470, 10):
            import math
            wy = 1650 + int(math.sin((i - sx) * 0.05) * 35.0)
            wave_pts.append((i, wy))
        if len(wave_pts) > 1:
            draw.line(wave_pts, fill=(111, 227, 255, 255), width=3) # #6FE3FF
        # Status text lines (simulated pixel rows)
        draw.rectangle([sx + 40, 1540, sx + 220, 1555], fill=(207, 244, 255, 255)) # #CFF4FF
        draw.rectangle([sx + 40, 1565, sx + 180, 1575], fill=(111, 227, 255, 200))
        draw.rectangle([sx + 40, 1585, sx + 260, 1595], fill=(111, 227, 255, 180))

    # ── 5. FACILITY STENCIL MARKINGS (Y = 1800 to 2048) ──
    draw.rectangle([0, 1800, W, 2048], fill=(27, 36, 48, 255))
    try:
        font_lg = ImageFont.truetype("arial.ttf", 64)
        font_md = ImageFont.truetype("arial.ttf", 36)
        font_sm = ImageFont.truetype("arial.ttf", 24)
    except Exception:
        font_lg = font_md = font_sm = ImageFont.load_default()

    # Stencil Text
    draw.text((64, 1840), "B-3", fill=(207, 244, 255, 240), font=font_lg)
    draw.text((320, 1840), "A-7", fill=(207, 244, 255, 240), font=font_lg)
    draw.text((580, 1850), "SECTOR B // RESTRICTED", fill=(207, 244, 255, 220), font=font_md)
    draw.text((1100, 1850), "PROJECT ASCENSION: SUBJECT A-07", fill=(111, 227, 255, 240), font=font_md)
    draw.text((64, 1940), "CONTAINMENT PROTOCOL LEVEL 5", fill=(196, 80, 46, 255), font=font_md)
    draw.text((800, 1940), "WARNING: BIO-DIVINE ENTITY SUSPENDED", fill=(196, 80, 46, 255), font=font_md)

    out_path = os.path.abspath("Assets/Environment/Lab/Lab_TrimSheet.png")
    im.save(out_path)
    print(f"Generated 2048x2048 modular trim sheet: {out_path}")

if __name__ == "__main__":
    generate_lab_trim_sheet()
