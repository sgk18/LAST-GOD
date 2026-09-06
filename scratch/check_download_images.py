from PIL import Image
import os

paths = [
    r"C:\Users\Surya VM\Downloads\prompt.png",
    r"C:\Users\Surya VM\Downloads\idle.png",
    r"C:\Users\Surya VM\Downloads\aeron-idle-01.png",
    r"C:\Users\Surya VM\Downloads\ChatGPT Image Sep 6, 2026, 06_27_03 PM.png"
]

for p in paths:
    if os.path.exists(p):
        with Image.open(p) as img:
            print(f"{os.path.basename(p)}: size={img.size}, format={img.format}, mode={img.mode}")
