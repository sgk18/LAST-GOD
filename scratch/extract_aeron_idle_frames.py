import os
import numpy as np
from PIL import Image

SRC_PATH = r"C:\Users\Surya VM\Downloads\idle.png"
OUTPUT_DIRS = [
    r"c:\projects\LAST-GOD\Assets\Characters\Aeron\Sprites\Idle",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Characters\Aeron\Sprites\Idle"
]

CANVAS_WIDTH = 320
CANVAS_HEIGHT = 704
FOOT_BASELINE_CANVAS_Y = 688  # From top (leaving 16px bottom margin)

# Segments and foot midpoints from previous analysis
FRAME_DATA = [
    # (x1, x2, foot_mid_x)
    (37, 260, 138.0),
    (289, 532, 406.5),
    (561, 793, 670.5),
    (816, 1053, 932.0),
    (1075, 1318, 1200.0),
    (1348, 1599, 1468.0),
    (1618, 1861, 1738.5),
    (1893, 2130, 2018.0)
]

for d in OUTPUT_DIRS:
    os.makedirs(d, exist_ok=True)

src_img = Image.open(SRC_PATH).convert("RGBA")
src_arr = np.array(src_img)

extracted_images = []

for i, (x1, x2, foot_mid_x) in enumerate(FRAME_DATA):
    frame_num = i + 1
    # Create empty canvas with alpha=0
    canvas = np.zeros((CANVAS_HEIGHT, CANVAS_WIDTH, 4), dtype=np.uint8)
    
    # Calculate offset
    # foot_mid_x in src should land on CANVAS_WIDTH // 2 (160)
    # FOOT_BASELINE_Y (688 in src) should land on FOOT_BASELINE_CANVAS_Y (688)
    shift_x = int(round((CANVAS_WIDTH / 2.0) - foot_mid_x))
    shift_y = FOOT_BASELINE_CANVAS_Y - 688 # 0
    
    # Copy pixels from src_arr to canvas
    for sy in range(src_arr.shape[0]):
        cy = sy + shift_y
        if 0 <= cy < CANVAS_HEIGHT:
            for sx in range(x1, x2 + 1):
                cx = sx + shift_x
                if 0 <= cx < CANVAS_WIDTH:
                    canvas[cy, cx] = src_arr[sy, sx]
                    
    frame_img = Image.fromarray(canvas, "RGBA")
    extracted_images.append(frame_img)
    
    filename = f"Aeron_Idle_{frame_num:02d}.png"
    for d in OUTPUT_DIRS:
        out_path = os.path.join(d, filename)
        frame_img.save(out_path, "PNG")
    print(f"Saved {filename} ({CANVAS_WIDTH}x{CANVAS_HEIGHT})")

# Also create composite spritesheet (8 * 320 = 2560 x 704)
sheet = Image.new("RGBA", (CANVAS_WIDTH * len(extracted_images), CANVAS_HEIGHT), (0, 0, 0, 0))
for idx, f_img in enumerate(extracted_images):
    sheet.paste(f_img, (idx * CANVAS_WIDTH, 0))

for d in OUTPUT_DIRS:
    sheet_path = os.path.join(d, "Aeron_Idle_Sheet.png")
    sheet.save(sheet_path, "PNG")
print(f"Saved composite sheet: {sheet.size}")
