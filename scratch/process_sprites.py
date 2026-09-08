import os
import shutil
import numpy as np
from PIL import Image

def remove_magenta_bg(img, tolerance=40):
    rgba = img.convert("RGBA")
    arr = np.array(rgba, dtype=np.float32)
    r, g, b = arr[:, :, 0], arr[:, :, 1], arr[:, :, 2]
    # Magenta target: (255, 0, 255)
    dist = np.sqrt((r - 255)**2 + (g - 0)**2 + (b - 255)**2)
    # Mask where distance is small
    alpha = np.clip((dist - 15) / float(tolerance), 0, 1) * 255.0
    arr[:, :, 3] = np.minimum(arr[:, :, 3], alpha)
    return Image.fromarray(arr.astype(np.uint8))

brain_dir = r"C:\Users\Surya VM\.gemini\antigravity-ide\brain\142d7740-0700-4fab-bd10-b847b06cc0c7"
idle_path = os.path.join(brain_dir, "cyber_guard_idle_1788870320331.jpg")
walk_path = os.path.join(brain_dir, "cyber_guard_walk_1788870340907.jpg")
shoot_path = os.path.join(brain_dir, "cyber_guard_shoot_1788870363370.jpg")
fireball_path = os.path.join(brain_dir, "aeron_fireball_fx_1788870382077.jpg")

os.makedirs("Assets/Art/Sprites/Guard", exist_ok=True)
os.makedirs("Assets/Art/Sprites/Combat", exist_ok=True)
os.makedirs("LAST-GOD/Assets/Art/Sprites/Guard", exist_ok=True)
os.makedirs("LAST-GOD/Assets/Art/Sprites/Combat", exist_ok=True)

# 1. Process Fireball
fb_raw = Image.open(fireball_path)
fb_clean = remove_magenta_bg(fb_raw)
# Crop to bounding box of non-zero alpha
bbox = fb_clean.getbbox()
if bbox:
    fb_cropped = fb_clean.crop(bbox)
else:
    fb_cropped = fb_clean

fb_dest1 = "Assets/Art/Sprites/Combat/Aeron_Fireball_FX.png"
fb_dest2 = "LAST-GOD/Assets/Art/Sprites/Combat/Aeron_Fireball_FX.png"
fb_cropped.save(fb_dest1)
fb_cropped.save(fb_dest2)
print("Saved Fireball FX:", fb_cropped.size)

# 2. Process Guard Walk (2x2 grid)
walk_raw = Image.open(walk_path)
walk_clean = remove_magenta_bg(walk_raw)
w, h = walk_clean.size
hw, hh = w // 2, h // 2
# 4 quadrants: (0, 0, hw, hh), (hw, 0, w, hh), (0, hh, hw, h), (hw, hh, w, h)
walk_frames = [
    walk_clean.crop((0, 0, hw, hh)),
    walk_clean.crop((hw, 0, w, hh)),
    walk_clean.crop((0, hh, hw, h)),
    walk_clean.crop((hw, hh, w, h))
]

# Find common bounding box size to align frames
max_w = 0
max_h = 0
cropped_walks = []
for f in walk_frames:
    b = f.getbbox()
    if b:
        c = f.crop(b)
        cropped_walks.append(c)
        max_w = max(max_w, c.width)
        max_h = max(max_h, c.height)
    else:
        cropped_walks.append(f)

# Pad and align at feet bottom
cell_w = max_w + 16
cell_h = max_h + 16
walk_sheet = Image.new("RGBA", (cell_w * 4, cell_h), (0, 0, 0, 0))
for i, c in enumerate(cropped_walks):
    px = i * cell_w + (cell_w - c.width) // 2
    py = cell_h - c.height - 4
    walk_sheet.paste(c, (px, py))
    frame_dest = f"Assets/Art/Sprites/Guard/Guard_Walk_{i}.png"
    c_aligned = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
    c_aligned.paste(c, ((cell_w - c.width) // 2, cell_h - c.height - 4))
    c_aligned.save(frame_dest)
    shutil.copyfile(frame_dest, f"LAST-GOD/{frame_dest}")

walk_sheet.save("Assets/Art/Sprites/Guard/Guard_Walk_Sheet.png")
shutil.copyfile("Assets/Art/Sprites/Guard/Guard_Walk_Sheet.png", "LAST-GOD/Assets/Art/Sprites/Guard/Guard_Walk_Sheet.png")
print("Saved Guard Walk Sheet:", walk_sheet.size)

# 3. Process Guard Shoot (2x2 grid)
shoot_raw = Image.open(shoot_path)
shoot_clean = remove_magenta_bg(shoot_raw)
shoot_frames = [
    shoot_clean.crop((0, 0, hw, hh)),
    shoot_clean.crop((hw, 0, w, hh)),
    shoot_clean.crop((0, hh, hw, h)),
    shoot_clean.crop((hw, hh, w, h))
]

cropped_shoots = []
s_max_w = 0
s_max_h = 0
for f in shoot_frames:
    b = f.getbbox()
    if b:
        c = f.crop(b)
        cropped_shoots.append(c)
        s_max_w = max(s_max_w, c.width)
        s_max_h = max(s_max_h, c.height)
    else:
        cropped_shoots.append(f)

s_cell_w = s_max_w + 16
s_cell_h = s_max_h + 16
shoot_sheet = Image.new("RGBA", (s_cell_w * 4, s_cell_h), (0, 0, 0, 0))
for i, c in enumerate(cropped_shoots):
    px = i * s_cell_w + (s_cell_w - c.width) // 2
    py = s_cell_h - c.height - 4
    shoot_sheet.paste(c, (px, py))
    frame_dest = f"Assets/Art/Sprites/Guard/Guard_Shoot_{i}.png"
    c_aligned = Image.new("RGBA", (s_cell_w, s_cell_h), (0, 0, 0, 0))
    c_aligned.paste(c, ((s_cell_w - c.width) // 2, s_cell_h - c.height - 4))
    c_aligned.save(frame_dest)
    shutil.copyfile(frame_dest, f"LAST-GOD/{frame_dest}")

shoot_sheet.save("Assets/Art/Sprites/Guard/Guard_Shoot_Sheet.png")
shutil.copyfile("Assets/Art/Sprites/Guard/Guard_Shoot_Sheet.png", "LAST-GOD/Assets/Art/Sprites/Guard/Guard_Shoot_Sheet.png")
print("Saved Guard Shoot Sheet:", shoot_sheet.size)

# 4. Process Guard Idle (4 columns, 2 rows)
idle_raw = Image.open(idle_path)
idle_clean = remove_magenta_bg(idle_raw)
cols = 4
rows = 2
fw = idle_clean.width // cols
fh = idle_clean.height // rows
idle_frames = []
for r in range(rows):
    for c in range(cols):
        idle_frames.append(idle_clean.crop((c * fw, r * fh, (c + 1) * fw, (r + 1) * fh)))

cropped_idles = []
i_max_w = 0
i_max_h = 0
for f in idle_frames[:4]: # Use top row of 4 idle cycle frames
    b = f.getbbox()
    if b:
        c = f.crop(b)
        cropped_idles.append(c)
        i_max_w = max(i_max_w, c.width)
        i_max_h = max(i_max_h, c.height)
    else:
        cropped_idles.append(f)

i_cell_w = i_max_w + 16
i_cell_h = i_max_h + 16
idle_sheet = Image.new("RGBA", (i_cell_w * 4, i_cell_h), (0, 0, 0, 0))
for i, c in enumerate(cropped_idles):
    px = i * i_cell_w + (i_cell_w - c.width) // 2
    py = i_cell_h - c.height - 4
    idle_sheet.paste(c, (px, py))
    frame_dest = f"Assets/Art/Sprites/Guard/Guard_Idle_{i}.png"
    c_aligned = Image.new("RGBA", (i_cell_w, i_cell_h), (0, 0, 0, 0))
    c_aligned.paste(c, ((i_cell_w - c.width) // 2, i_cell_h - c.height - 4))
    c_aligned.save(frame_dest)
    shutil.copyfile(frame_dest, f"LAST-GOD/{frame_dest}")

idle_sheet.save("Assets/Art/Sprites/Guard/Guard_Idle_Sheet.png")
shutil.copyfile("Assets/Art/Sprites/Guard/Guard_Idle_Sheet.png", "LAST-GOD/Assets/Art/Sprites/Guard/Guard_Idle_Sheet.png")
print("Saved Guard Idle Sheet:", idle_sheet.size)
