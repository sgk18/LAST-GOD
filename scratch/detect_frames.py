from PIL import Image
import numpy as np

img = Image.open(r"C:\Users\Surya VM\Downloads\idle.png")
arr = np.array(img)
alpha = arr[:, :, 3]

# Projection along columns
col_has_pixels = (alpha > 10).any(axis=0)

# Find contiguous segments of columns
segments = []
in_seg = False
start = 0
for col_idx, has_pix in enumerate(col_has_pixels):
    if has_pix and not in_seg:
        in_seg = True
        start = col_idx
    elif not has_pix and in_seg:
        in_seg = False
        segments.append((start, col_idx - 1))
if in_seg:
    segments.append((start, len(col_has_pixels) - 1))

print(f"Found {len(segments)} segments in idle.png:")
for i, (s, e) in enumerate(segments):
    w = e - s + 1
    # Check vertical bounds for this segment
    sub_alpha = alpha[:, s:e+1]
    row_has_pixels = (sub_alpha > 10).any(axis=1)
    y_indices = np.where(row_has_pixels)[0]
    y_min, y_max = y_indices[0], y_indices[-1]
    h = y_max - y_min + 1
    print(f"  Frame {i+1}: X=[{s}..{e}] (width {w}), Y=[{y_min}..{y_max}] (height {h})")
