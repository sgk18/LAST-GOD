from PIL import Image
import numpy as np

im = Image.open(r"C:\Users\Surya VM\Downloads\walk.png")
arr = np.array(im)[:430, :, :]

# Character 1: x from ~180 to 400
# Character 2: x from ~400 to 650
# Character 3: x from ~650 to 900
# Character 4: x from ~900 to 1150
# Character 5: x from ~1150 to 1400
# Character 6: x from ~1400 to 1650
# Character 7: x from ~1650 to 1900
# Character 8: x from ~1900 to 2150

# Let's inspect each range in detail
ranges = [
    (180, 400),
    (400, 650),
    (650, 900),
    (900, 1150),
    (1150, 1400),
    (1400, 1650),
    (1650, 1900),
    (1900, 2160)
]

for idx, (x_min, x_max) in enumerate(ranges):
    sub = arr[:, x_min:x_max, :]
    # look at foot area: y between 340 and 405
    # find where badge is vs foot
    print(f"=== Frame {idx+1} (x in {x_min}..{x_max}) ===")
    ys, xs = np.where(sub[:, :, 3] > 20)
    if len(ys) > 0:
        global_xs = xs + x_min
        print(f"  Overall bounds: x=[{global_xs.min()}..{global_xs.max()}], y=[{ys.min()}..{ys.max()}]")
        
        # Look for badge at bottom: badges are typically around y=400..420
        # Character feet are around y=380..400
        # Let's print the bottom 30 rows of non-empty pixels
        for y in range(ys.max()-25, ys.max()+1):
            row_xs = global_xs[ys == y]
            if len(row_xs) > 0:
                print(f"    y={y}: count={len(row_xs)}, x_span=[{row_xs.min()}..{row_xs.max()}]")
