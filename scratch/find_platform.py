from PIL import Image, ImageDraw
import numpy as np

img = Image.open('Assets/Art/Backgrounds/1.png').convert('RGBA')
w, h = img.size
print(f"Image size: {w} x {h}")

# The platform floor in the screenshot is below the blue circle (y_center=376.5) and above the dark silhouette pipes at the bottom.
# Let's crop a column slice in the middle (e.g. x=800 to 1200) to find the walking deck surface.
# Notice in screenshot:
# There's computers, consoles, railing, stasis pod.
# Below them is a metallic horizontal line with grating/plate surface.
# Below that is hazard stripes / under-deck beams, then black silhouettes of pipes.

# Let's sample a vertical slice at x=1044 (center) from y=350 to 700:
arr = np.array(img)

# Let's inspect RGB values along x=1000 from y=350 to 650
print("Row samples near center:")
for y in range(400, 600, 10):
    # average RGB over x=1000..1050
    avg_rgb = arr[y, 1000:1050, :3].mean(axis=0).astype(int)
    unity_y = (376.5 - y) / 16.0
    print(f"y={y}, Unity Y={unity_y:.3f}, RGB={avg_rgb}")

# Let's also check where the hazard stripe (yellow/black) or deck edge is
# And draw lines on a downscaled image to verify where unity Y values map to the image
debug_img = img.copy()
draw = ImageDraw.Draw(debug_img)

# Let's test lines at different Unity Y values: e.g. -5, -6, -7, -8, -9, -10, -11, -12, -13, -14, -15
for unity_y in [-5, -6, -7, -8, -9, -10, -11, -12, -13, -14, -15]:
    # unity_y = (376.5 - y) / 16.0 => y = 376.5 - unity_y * 16.0
    y_pix = int(round(376.5 - unity_y * 16.0))
    if 0 <= y_pix < h:
        draw.line([(0, y_pix), (w, y_pix)], fill=(255, 0, 0, 255) if unity_y == -14 else (0, 255, 0, 255), width=2)
        draw.text((50, y_pix - 15), f"Unity Y = {unity_y}", fill=(255, 255, 255, 255))

debug_img.save('scratch/annotated_grid.png')
print("Saved scratch/annotated_grid.png")

