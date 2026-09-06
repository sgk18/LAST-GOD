from PIL import Image, ImageDraw
import numpy as np

img = Image.open('Assets/Art/Backgrounds/1.png').convert('RGBA')
w, h = img.size

# Pivot is (w/2, h/2) = (1044.5, 376.5)
# Unity X = (pixel_x - 1044.5) / 16.0
# Unity Y = (376.5 - pixel_y) / 16.0
# pixel_x = 1044.5 + Unity_X * 16.0
# pixel_y = 376.5 - Unity_Y * 16.0

print(f"Total width in Unity units: {w/16.0:.2f} (from {-w/32.0:.2f} to {w/32.0:.2f})")
print(f"Total height in Unity units: {h/16.0:.2f} (from {-h/32.0:.2f} to {h/32.0:.2f})")

# Let's find the exact Y of the floor across X:
# Walkway runs horizontally across the scene.
# Let's find the top surface of the metallic deck at various X positions:
# e.g. x_unity from -55 to +55 in steps of 5 units:
results = []
for x_u in range(-60, 61, 5):
    px = int(round(1044.5 + x_u * 16.0))
    if 0 <= px < w:
        # Search vertically between Unity Y = -11 and -15 (pixel Y = 550 to 620)
        # The metallic floor top has a sharp contrast / highlight
        slice_col = np.array([img.getpixel((px, py))[:3] for py in range(540, 640)])
        # Find where the bright edge is:
        # Let's find row with maximum horizontal or brightness transition
        brightness = slice_col.mean(axis=1)
        # Floor edge usually corresponds to the top edge of the bright metallic rim
        max_b_idx = np.argmax(brightness)
        py_edge = 540 + max_b_idx
        yu_edge = (376.5 - py_edge) / 16.0
        results.append((x_u, px, py_edge, yu_edge, brightness[max_b_idx]))

for r in results:
    print(f"Unity X = {r[0]:4d} | Pixel X = {r[1]:4d} | Pixel Y = {r[2]:3d} | Unity Y = {r[3]:6.3f} | Brightness = {r[4]:5.1f}")
