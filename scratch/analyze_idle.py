from PIL import Image

with Image.open(r"C:\Users\Surya VM\Downloads\idle.png") as img:
    print("idle.png size:", img.size, "mode:", img.mode)
    # Check if there are separate frames or transparent backgrounds
    # 2172 / 8 = 271.5, or 2172 / 7 = 310.28, or let's check non-empty bounding box
    bbox = img.getbbox()
    print("idle.png bounding box:", bbox)

with Image.open(r"C:\Users\Surya VM\Downloads\prompt.png") as pimg:
    print("prompt.png size:", pimg.size, "mode:", pimg.mode)
