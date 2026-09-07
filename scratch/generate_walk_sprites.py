import os
import numpy as np
from PIL import Image

src_img = Image.open(r"C:\Users\Surya VM\Downloads\walk.png").convert("RGBA")
src_arr = np.array(src_img)

# Badges to erase: (x_min, x_max, y_min, y_max)
badges = [
    (235, 275, 403, 415),
    (505, 545, 403, 415),
    (735, 775, 403, 415),
    (985, 1025, 403, 415),
    (1238, 1278, 403, 415),
    (1500, 1542, 403, 415),
    (1738, 1776, 403, 415),
    (1980, 2020, 403, 415),
]

cleaned_arr = src_arr.copy()
for bx1, bx2, by1, by2 in badges:
    cleaned_arr[by1:by2+1, bx1:bx2+1] = [0, 0, 0, 0]

# Zero out left text for Frame 1
cleaned_arr[:430, :175] = [0, 0, 0, 0]

# Frame X bounding boxes
frame_bounds = [
    (175, 395),
    (405, 649),
    (655, 895),
    (905, 1145),
    (1155, 1395),
    (1405, 1645),
    (1655, 1895),
    (1900, 2150)
]

OUTPUT_DIRS = [
    r"c:\projects\LAST-GOD\Assets\Characters\Aeron\Sprites\Walk",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Characters\Aeron\Sprites\Walk"
]

for d in OUTPUT_DIRS:
    os.makedirs(d, exist_ok=True)

CANVAS_WIDTH = 448
CANVAS_HEIGHT = 704
FEET_BASELINE_Y = 688  # 16px from bottom
SCALE = 643.0 / 393.0  # 1.636132

for i, (x1, x2) in enumerate(frame_bounds):
    frame_crop = cleaned_arr[:415, x1:x2+1, :]
    ys, xs = np.where(frame_crop[:, :, 3] > 20)
    if len(ys) == 0:
        continue
    
    crop_ymin, crop_ymax = ys.min(), ys.max()
    crop_xmin, crop_xmax = xs.min(), xs.max()
    
    char_crop = frame_crop[crop_ymin:crop_ymax+1, crop_xmin:crop_xmax+1, :]
    char_img = Image.fromarray(char_crop, "RGBA")
    
    new_w = int(round(char_img.width * SCALE))
    new_h = int(round(char_img.height * SCALE))
    scaled_img = char_img.resize((new_w, new_h), Image.Resampling.LANCZOS)
    scaled_arr = np.array(scaled_img)
    
    canvas = np.zeros((CANVAS_HEIGHT, CANVAS_WIDTH, 4), dtype=np.uint8)
    
    # Place bottom of feet at FEET_BASELINE_Y
    paste_y = FEET_BASELINE_Y - new_h
    # Center horizontally on CANVAS_WIDTH // 2
    paste_x = (CANVAS_WIDTH - new_w) // 2
    
    for sy in range(new_h):
        cy = paste_y + sy
        if 0 <= cy < CANVAS_HEIGHT:
            for sx in range(new_w):
                cx = paste_x + sx
                if 0 <= cx < CANVAS_WIDTH:
                    canvas[cy, cx] = scaled_arr[sy, sx]
                    
    out_img = Image.fromarray(canvas, "RGBA")
    fname = f"Aeron_Walk_{i+1:02d}.png"
    for d in OUTPUT_DIRS:
        p = os.path.join(d, fname)
        out_img.save(p)
    print(f"Generated {fname}: placed at ({paste_x}, {paste_y}), size=({new_w}, {new_h})")
