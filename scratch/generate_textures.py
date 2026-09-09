import os
from PIL import Image, ImageDraw

def create_aeron_textures():
    # 1024x1024 Aeron_Albedo
    w, h = 1024, 1024
    albedo = Image.new("RGBA", (w, h), (27, 36, 48, 255)) # #1B2430 dark slate
    draw = ImageDraw.Draw(albedo)

    # Torso / Armor plates
    for y in range(0, 512, 64):
        draw.rectangle([64, y + 8, 448, y + 56], fill=(58, 63, 71, 255)) # #3A3F47
        draw.line([64, y + 8, 448, y + 8], fill=(75, 82, 92, 255), width=2)
        draw.line([64, y + 56, 448, y + 56], fill=(10, 12, 16, 255), width=2) # #0A0C10

    # Limbs / Legs section
    for y in range(512, 1024, 64):
        draw.rectangle([64, y + 8, 448, y + 56], fill=(35, 42, 54, 255))
        draw.line([64, y + 32, 448, y + 32], fill=(10, 12, 16, 255), width=3)

    # Collar & Heavy Hardware (Right half, top)
    draw.rectangle([512, 0, 1024, 384], fill=(10, 12, 16, 255)) # #0A0C10 near-black
    for x in range(540, 1000, 48):
        draw.rectangle([x, 40, x + 32, 340], fill=(30, 35, 45, 255))

    # Head / Face region
    draw.rectangle([512, 384, 800, 768], fill=(212, 197, 185, 255)) # pale skin
    draw.rectangle([512, 384, 800, 500], fill=(25, 20, 20, 255)) # dark hair

    # Pale cyan-white glowing eye (#CFF4FF)
    draw.ellipse([620, 540, 680, 600], fill=(207, 244, 255, 255)) # #CFF4FF core
    draw.ellipse([635, 555, 665, 585], fill=(245, 252, 255, 255)) # white-hot center
    draw.ellipse([600, 520, 700, 620], outline=(111, 227, 255, 120), width=4) # faint corona

    # Divine Circuitry / Arm Glyphs
    for i in range(8):
        draw.line([840 + i*16, 400, 840 + i*16, 750], fill=(207, 244, 255, 180), width=3)

    albedo_path = "Assets/Characters/Aeron/Aeron_Albedo.png"
    albedo.save(albedo_path)
    print(f"Saved {albedo_path}")

    # Aeron_RoughMetal (Packed: R=Metallic, G=AO, B=Detail, A=Smoothness)
    # Metallic=25 (0.10), AO=235 (0.92), B=0, Smoothness=48 (0.19)
    rough_metal = Image.new("RGBA", (w, h), (25, 235, 0, 48))
    rm_path = "Assets/Characters/Aeron/Aeron_RoughMetal.png"
    rough_metal.save(rm_path)
    print(f"Saved {rm_path}")


def create_guard_textures():
    # 1024x1024 Guard_Albedo
    w, h = 1024, 1024
    albedo = Image.new("RGBA", (w, h), (34, 38, 46, 255)) # #22262E carbon grey
    draw = ImageDraw.Draw(albedo)

    # Heavy armor plates
    for y in range(0, 512, 64):
        draw.rectangle([64, y + 6, 448, y + 58], fill=(58, 63, 71, 255)) # #3A3F47
        draw.rectangle([80, y + 16, 432, y + 48], fill=(27, 36, 48, 255)) # #1B2430 navy inset
        draw.line([64, y + 6, 448, y + 6], fill=(80, 88, 98, 255), width=2)
        draw.line([64, y + 58, 448, y + 58], fill=(10, 12, 16, 255), width=3)

    # Greaves / Boots / Harness
    draw.rectangle([512, 0, 1024, 512], fill=(10, 12, 16, 255)) # #0A0C10 near black
    for y in range(32, 480, 48):
        draw.rectangle([540, y, 996, y + 28], fill=(42, 48, 58, 255))

    # Helmet / Visor region
    draw.rectangle([512, 512, 1024, 1024], fill=(27, 36, 48, 255))
    draw.rectangle([540, 540, 996, 750], fill=(10, 12, 16, 255)) # dark helmet face

    # Visor amber/red status slit (#C4502E)
    draw.rectangle([600, 620, 940, 670], fill=(196, 80, 46, 255)) # #C4502E
    draw.rectangle([640, 635, 900, 655], fill=(240, 120, 70, 255)) # bright core
    draw.line([600, 620, 940, 620], fill=(255, 160, 100, 255), width=2)

    # Chest status indicator badge
    draw.rectangle([180, 700, 320, 840], fill=(10, 12, 16, 255))
    draw.rectangle([200, 720, 300, 820], fill=(196, 80, 46, 255))
    draw.ellipse([230, 750, 270, 790], fill=(255, 140, 80, 255))

    albedo_path = "Assets/Characters/Guard/Guard_Albedo.png"
    albedo.save(albedo_path)
    print(f"Saved {albedo_path}")

    # Guard_RoughMetal (Packed: R=Metallic 0.28, G=AO 0.94, B=0, A=Smoothness 0.22)
    rough_metal = Image.new("RGBA", (w, h), (71, 240, 0, 56))
    rm_path = "Assets/Characters/Guard/Guard_RoughMetal.png"
    rough_metal.save(rm_path)
    print(f"Saved {rm_path}")


def create_lab_trimsheet():
    # 2048x2048 Lab_TrimSheet
    w, h = 2048, 2048
    trim = Image.new("RGBA", (w, h), (20, 24, 32, 255)) # #141820
    draw = ImageDraw.Draw(trim)

    # 1. Wall Panels (Top 40%: Y: 0 to 820)
    for y in range(0, 820, 164):
        draw.line([0, y, w, y], fill=(10, 12, 16, 255), width=6)
        draw.line([0, y+4, w, y+4], fill=(45, 52, 65, 255), width=2)
        for x in range(0, w, 256):
            draw.line([x, y, x, y + 164], fill=(10, 12, 16, 255), width=4)
            # Rivets
            draw.ellipse([x + 16, y + 16, x + 26, y + 26], fill=(58, 63, 71, 255))
            draw.ellipse([x + 230, y + 16, x + 240, y + 26], fill=(58, 63, 71, 255))
            draw.ellipse([x + 16, y + 138, x + 26, y + 148], fill=(58, 63, 71, 255))
            draw.ellipse([x + 230, y + 138, x + 240, y + 148], fill=(58, 63, 71, 255))

    # 2. Floor Grating (Middle 35%: Y: 820 to 1536)
    draw.rectangle([0, 820, w, 1536], fill=(26, 32, 42, 255))
    # Diamond / hex grating pattern
    step = 32
    for y in range(820, 1536, step):
        for x in range(0, w, step):
            draw.rectangle([x + 4, y + 4, x + step - 4, y + step - 4], fill=(16, 20, 28, 255))
            draw.line([x, y, x + step, y + step], fill=(42, 48, 58, 255), width=1)
            draw.line([x + step, y, x, y + step], fill=(42, 48, 58, 255), width=1)

    # Industrial hazard border stripe (Y: 820 to 860 and 1496 to 1536)
    for y_stripe in [820, 1496]:
        for x in range(0, w, 64):
            draw.polygon([(x, y_stripe), (x + 32, y_stripe), (x + 16, y_stripe + 40), (x - 16, y_stripe + 40)], fill=(75, 70, 50, 255))

    # 3. Pillar & Conduit Fluting (Bottom 25%: Y: 1536 to 2048)
    draw.rectangle([0, 1536, w, 2048], fill=(14, 20, 32, 255))
    for x in range(0, w, 128):
        # Heavy vertical structural casing
        draw.rectangle([x + 12, 1536, x + 116, 2048], fill=(32, 38, 48, 255))
        draw.line([x + 12, 1536, x + 12, 2048], fill=(60, 70, 85, 255), width=3)
        draw.line([x + 116, 1536, x + 116, 2048], fill=(10, 12, 16, 255), width=4)
        # Conduit pipe
        draw.rectangle([x + 48, 1536, x + 80, 2048], fill=(45, 52, 65, 255))
        # Subtle cold cyan status seam line (#206080)
        draw.line([x + 64, 1536, x + 64, 2048], fill=(32, 96, 128, 255), width=3)

    trim_path = "Assets/Environment/Lab/Lab_TrimSheet.png"
    trim.save(trim_path)
    print(f"Saved {trim_path}")

if __name__ == "__main__":
    create_aeron_textures()
    create_guard_textures()
    create_lab_trimsheet()
    print("All textures created successfully!")
