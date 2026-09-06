from PIL import Image, ImageDraw

# Background 1.png: 2089 x 753 pixels, PPU = 16
# Pivot (0.5, 0.5) -> pixel (1044.5, 376.5)
img = Image.open('Assets/Art/Backgrounds/1.png').convert('RGBA')
w, h = img.size

def unity_to_pixel(ux, uy):
    px = 1044.5 + ux * 16.0
    py = 376.5 - uy * 16.0
    return px, py

draw = ImageDraw.Draw(img)

# 1. Collider_MainDeck: pos = (0, -15.28), size = (131, 4.5)
# min_x = -65.5, max_x = 65.5
# min_y = -17.53, max_y = -13.03
x0, y0 = unity_to_pixel(-65.5, -13.03) # top-left
x1, y1 = unity_to_pixel(65.5, -17.53)  # bottom-right
draw.rectangle([x0, y0, x1, y1], outline=(0, 255, 0, 255), width=3)

# 2. Collider_LeftWall: pos = (-65.5, 0), size = (2, 45)
# min_x = -66.5, max_x = -64.5, min_y = -22.5, max_y = 22.5
x0, y0 = unity_to_pixel(-66.5, 22.5)
x1, y1 = unity_to_pixel(-64.5, -22.5)
draw.rectangle([x0, y0, x1, y1], outline=(0, 200, 255, 255), width=2)

# 3. Collider_RightWall: pos = (65.5, 0), size = (2, 45)
x0, y0 = unity_to_pixel(64.5, 22.5)
x1, y1 = unity_to_pixel(66.5, -22.5)
draw.rectangle([x0, y0, x1, y1], outline=(0, 200, 255, 255), width=2)

# 4. Glass_Chamber: pos = (-34.0, -11.53), size = (2.2, 2.7), offset = (0, 0.125)
# center_x = -34.0, center_y = -11.53 + 0.125 = -11.405
# min_x = -34.0 - 1.1 = -35.1, max_x = -34.0 + 1.1 = -32.9
# min_y = -11.405 - 1.35 = -12.755, max_y = -11.405 + 1.35 = -10.055
x0, y0 = unity_to_pixel(-35.1, -10.055)
x1, y1 = unity_to_pixel(-32.9, -12.755)
draw.rectangle([x0, y0, x1, y1], outline=(255, 255, 0, 255), width=2)

# 5. Player: pos = (0, -13.03), Capsule offset = (0, 0.7), size = (0.5, 1.4)
# min_x = -0.25, max_x = 0.25
# min_y = -13.03, max_y = -13.03 + 1.4 = -11.63
x0, y0 = unity_to_pixel(-0.5, -11.63)
x1, y1 = unity_to_pixel(0.5, -13.03)
draw.rectangle([x0, y0, x1, y1], outline=(255, 0, 0, 255), width=2)

img.save('scratch/colliders_verification.png')
print('Saved scratch/colliders_verification.png successfully!')
