import os, re

files = [
    "Assets/Art/Backgrounds/Layer1_FarBackground.png.meta",
    "Assets/Art/Backgrounds/Layer2_Midground.png.meta",
    "Assets/Art/Backgrounds/Layer3_Foreground.png.meta",
    "Assets/Characters/Aeron/Sprites/Idle/Aeron_Idle_01.png.meta"
]

for f in files:
    if os.path.exists(f):
        with open(f, "r", encoding="utf-8") as meta_f:
            c = meta_f.read()
        guid = re.search(r"guid:\s*([a-f0-9]+)", c).group(1)
        ttype = re.search(r"textureType:\s*(\d+)", c)
        filterMode = re.search(r"filterMode:\s*(\d+)", c)
        ppu = re.search(r"spritePixelsToUnits:\s*(\d+)", c)
        comp = re.search(r"textureCompression:\s*(\d+)", c)
        ttype_val = ttype.group(1) if ttype else "?"
        fmode_val = filterMode.group(1) if filterMode else "?"
        ppu_val = ppu.group(1) if ppu else "?"
        comp_val = comp.group(1) if comp else "?"
        print(f"{f}:")
        print(f"  GUID: {guid}")
        print(f"  textureType: {ttype_val} (8=Sprite)")
        print(f"  filterMode: {fmode_val} (0=Point)")
        print(f"  PPU: {ppu_val}")
        print(f"  compression: {comp_val} (0=None)")
