import os
from PIL import Image, ImageDraw

tex_dir = r"C:\projects\LAST-GOD\Assets\Art\UI\Textures"
os.makedirs(tex_dir, exist_ok=True)

# 1. UI_Speaker_Silhouette_01.png (128x128) - Dr. Malkhov / Classified Scientist
def create_scientist_silhouette():
    img = Image.new("RGBA", (128, 128), (17, 22, 28, 255)) # Deep Charcoal #11161C
    draw = ImageDraw.Draw(img)
    
    # 2px border
    draw.rectangle([1, 1, 126, 126], outline=(74, 83, 92, 255), width=2)
    # Corner registration ticks
    draw.line([(1, 10), (10, 1)], fill=(111, 227, 255, 255), width=2)
    draw.line([(117, 1), (126, 10)], fill=(111, 227, 255, 255), width=2)
    draw.line([(1, 117), (10, 126)], fill=(111, 227, 255, 255), width=2)
    draw.line([(117, 126), (126, 117)], fill=(111, 227, 255, 255), width=2)

    # Dark ink head and shoulders silhouette
    # Head
    draw.ellipse([46, 26, 82, 68], fill=(8, 11, 15, 255))
    # Angular jaw / chin
    draw.polygon([(46, 52), (82, 52), (72, 74), (56, 74)], fill=(8, 11, 15, 255))
    # High collar coat / shoulders
    draw.polygon([(20, 125), (42, 80), (86, 80), (108, 125)], fill=(8, 11, 15, 255))
    # Glasses / optical rim line (Cyan tech accent)
    draw.line([(52, 46), (62, 46)], fill=(111, 227, 255, 220), width=2)
    draw.line([(66, 46), (76, 46)], fill=(111, 227, 255, 220), width=2)
    draw.line([(62, 46), (66, 46)], fill=(111, 227, 255, 180), width=1)
    
    # Ink hatching lines across coat
    for y in range(86, 120, 6):
        draw.line([(32 + (y-86), y), (46 + (y-86), y+4)], fill=(48, 56, 65, 200), width=1)
        
    img.save(os.path.join(tex_dir, "UI_Speaker_Silhouette_01.png"))
    print("Created UI_Speaker_Silhouette_01.png")

# 2. UI_Speaker_Silhouette_02.png (128x128) - Security Overwatch / Tactical Drone
def create_overwatch_silhouette():
    img = Image.new("RGBA", (128, 128), (17, 22, 28, 255))
    draw = ImageDraw.Draw(img)
    
    # 2px border
    draw.rectangle([1, 1, 126, 126], outline=(74, 83, 92, 255), width=2)
    # Tactical helmet silhouette
    draw.polygon([(64, 20), (90, 36), (90, 72), (64, 88), (38, 72), (38, 36)], fill=(8, 11, 15, 255))
    # Tactical visor slit (Warning Orange #C4502E)
    draw.line([(44, 52), (84, 52)], fill=(196, 80, 46, 255), width=3)
    # Heavy armor shoulders
    draw.polygon([(16, 125), (32, 92), (96, 92), (112, 125)], fill=(8, 11, 15, 255))
    # Chevrons on collar
    draw.line([(58, 102), (64, 108), (70, 102)], fill=(196, 80, 46, 220), width=2)
    draw.line([(58, 110), (64, 116), (70, 110)], fill=(196, 80, 46, 220), width=2)

    img.save(os.path.join(tex_dir, "UI_Speaker_Silhouette_02.png"))
    print("Created UI_Speaker_Silhouette_02.png")

# 3. UI_Bullet_Icon.png (16x32)
def create_bullet_icon():
    img = Image.new("RGBA", (16, 32), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Bullet projectile tip
    draw.polygon([(8, 2), (13, 10), (3, 10)], fill=(111, 227, 255, 255))
    # Casing body
    draw.rectangle([3, 11, 13, 27], fill=(48, 56, 65, 255), outline=(74, 83, 92, 255), width=1)
    # Extractor groove & rim
    draw.rectangle([2, 28, 14, 30], fill=(111, 227, 255, 200))
    img.save(os.path.join(tex_dir, "UI_Bullet_Icon.png"))
    print("Created UI_Bullet_Icon.png")

# 4. UI_Slider_Handle.png (16x24)
def create_slider_handle():
    img = Image.new("RGBA", (16, 24), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Rectangular brutalist handle
    draw.rectangle([1, 1, 14, 22], fill=(17, 22, 28, 255), outline=(207, 244, 255, 255), width=2)
    # Center cyan vertical notch
    draw.line([(8, 5), (8, 18)], fill=(111, 227, 255, 255), width=2)
    img.save(os.path.join(tex_dir, "UI_Slider_Handle.png"))
    print("Created UI_Slider_Handle.png")

# 5. UI_Checkbox_Frame.png (24x24)
def create_checkbox_frame():
    img = Image.new("RGBA", (24, 24), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([1, 1, 22, 22], fill=(17, 22, 28, 255), outline=(74, 83, 92, 255), width=2)
    # Corner registration ticks
    draw.point([(1, 1), (22, 1), (1, 22), (22, 22)], fill=(111, 227, 255, 255))
    img.save(os.path.join(tex_dir, "UI_Checkbox_Frame.png"))
    print("Created UI_Checkbox_Frame.png")

# 6. UI_Check_Mark.png (24x24)
def create_check_mark():
    img = Image.new("RGBA", (24, 24), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Heavy brutalist cross / fill block
    draw.rectangle([5, 5, 18, 18], fill=(111, 227, 255, 255))
    img.save(os.path.join(tex_dir, "UI_Check_Mark.png"))
    print("Created UI_Check_Mark.png")

create_scientist_silhouette()
create_overwatch_silhouette()
create_bullet_icon()
create_slider_handle()
create_checkbox_frame()
create_check_mark()
print("All procedural Phase 02 sprites generated.")
