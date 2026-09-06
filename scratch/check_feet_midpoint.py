from PIL import Image
import numpy as np

img = Image.open(r"C:\Users\Surya VM\Downloads\idle.png")
arr = np.array(img)

segments = [
    (37, 260),
    (289, 532),
    (561, 793),
    (816, 1053),
    (1075, 1318),
    (1348, 1599),
    (1618, 1861),
    (1893, 2130)
]

for idx, (x1, x2) in enumerate(segments):
    sub = arr[:, x1:x2+1, :]
    sub_alpha = sub[:, :, 3]
    ys, xs = np.where(sub_alpha > 10)
    y_max = np.max(ys)
    
    # Check lowest 25 rows (feet)
    feet_mask = (sub_alpha > 10) & (np.arange(arr.shape[0])[:, None] >= y_max - 25)
    f_ys, f_xs = np.where(feet_mask)
    f_min_x = x1 + np.min(f_xs)
    f_max_x = x1 + np.max(f_xs)
    f_mid_x = (f_min_x + f_max_x) / 2.0
    f_width = f_max_x - f_min_x + 1
    
    print(f"Frame {idx+1}: Feet X=[{f_min_x}..{f_max_x}] width={f_width}, Feet Midpoint={f_mid_x:.1f}")
