from PIL import Image
import numpy as np

img = Image.open(r"C:\Users\Surya VM\Downloads\idle.png")
arr = np.array(img)
alpha = arr[:, :, 3]

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
    
    y_min, y_max = np.min(ys), np.max(ys)
    x_min, x_max = np.min(xs), np.max(xs)
    
    # Global coordinates
    gx1 = x1 + x_min
    gx2 = x1 + x_max
    gy1 = y_min
    gy2 = y_max
    
    # Foot ground contact: pixels with y within 5px of gy2
    foot_pixels = np.where((sub_alpha > 10) & (arr[:, x1:x2+1, 3] > 10) & (np.arange(arr.shape[0])[:, None] >= gy2 - 5))
    foot_x_global = x1 + np.mean(foot_pixels[1])
    
    print(f"Frame {idx+1}:")
    print(f"  Global Box: X=[{gx1}..{gx2}] (w={gx2-gx1+1}), Y=[{gy1}..{gy2}] (h={gy2-gy1+1})")
    print(f"  Foot Base Y: {gy2}")
    print(f"  Foot Center X: {foot_x_global:.2f}")
