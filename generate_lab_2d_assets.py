"""
generate_lab_2d_assets.py
Procedurally synthesizes the complete modular hand-crafted 2D pixel art asset library
for The Last God (Act 1: The Laboratory).
Adheres strictly to the 10-color master palette, 32 PPU, and exact pixel geometry.
"""

import os
from PIL import Image, ImageDraw

# Master Palette Colors (RGBA)
C_DARK_BASE      = (8, 11, 15, 255)       # #080B0F - Deep void, shadows
C_DEEP_CHARCOAL  = (17, 22, 28, 255)      # #11161C - Primary silhouette, heavy armor
C_DARK_BLUE_GREY = (27, 36, 48, 255)      # #1B2430 - Wall panels, ambient tone
C_IND_GREY       = (48, 56, 65, 255)      # #303841 - Floors, platforms, machinery
C_LIGHT_METAL    = (74, 83, 92, 255)      # #4A535C - Gratings, edge rims, highlights
C_DARK_CYAN      = (36, 91, 112, 255)     # #245B70 - Fluid tanks, low power conduits
C_PRIMARY_CYAN   = (111, 227, 255, 255)   # #6FE3FF - Active energy, displays, screens
C_BRIGHT_CYAN    = (207, 244, 255, 255)   # #CFF4FF - Specular emission, core arcs
C_WARN_ORANGE    = (196, 80, 46, 255)     # #C4502E - Hazard latches, warning diodes
C_IND_YELLOW     = (179, 154, 69, 255)    # #B39A45 - Hazard stripes, stencils
C_CLEAR          = (0, 0, 0, 0)

BASE_DIR = "Assets/Art"

def save_sprite(img, rel_path):
    full_path = os.path.join(BASE_DIR, rel_path)
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    img.save(full_path, format="PNG")
    print(f"Saved: {full_path} ({img.width}x{img.height})")

    # Generate Unity .meta file
    meta_path = full_path + ".meta"
    # Simple GUID generation from path hash
    import hashlib
    h = hashlib.md5(rel_path.encode('utf-8')).hexdigest()
    meta_content = f"""fileFormatVersion: 2
guid: {h}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 32
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)

# ----------------------------------------------------------------------------
# 1. TILES (32x32)
# ----------------------------------------------------------------------------

def create_tiles():
    # 1.1 Floor Straight
    im = Image.new("RGBA", (32, 32), C_IND_GREY)
    draw = ImageDraw.Draw(im)
    # Top highlight bevel
    draw.line([(0, 0), (31, 0)], fill=C_LIGHT_METAL)
    draw.line([(0, 1), (31, 1)], fill=C_LIGHT_METAL)
    # Bottom shadow bevel
    draw.line([(0, 31), (31, 31)], fill=C_DARK_BASE)
    draw.line([(0, 30), (31, 30)], fill=C_DEEP_CHARCOAL)
    # Vertical expansion seam
    draw.line([(15, 2), (15, 29)], fill=C_DEEP_CHARCOAL)
    draw.line([(16, 2), (16, 29)], fill=C_DARK_BLUE_GREY)
    # Rivets
    rivet_pos = [(4, 5), (27, 5), (4, 25), (27, 25)]
    for rx, ry in rivet_pos:
        im.putpixel((rx, ry), C_LIGHT_METAL)
        im.putpixel((rx+1, ry), C_DEEP_CHARCOAL)
        im.putpixel((rx, ry+1), C_DARK_BASE)
    save_sprite(im, "Tiles/LAB_Tile_Floor_Straight.png")

    # 1.2 Floor Grating
    im = Image.new("RGBA", (32, 32), C_DARK_BASE)
    draw = ImageDraw.Draw(im)
    # Steel frame
    draw.rectangle([0, 0, 31, 31], outline=C_LIGHT_METAL)
    draw.rectangle([1, 1, 30, 30], outline=C_IND_GREY)
    # Grating bars
    for x in range(4, 28, 4):
        draw.line([(x, 3), (x, 28)], fill=C_LIGHT_METAL)
        draw.line([(x+1, 3), (x+1, 28)], fill=C_IND_GREY)
    save_sprite(im, "Tiles/LAB_Tile_Floor_Grating.png")

    # 1.3 Floor Damaged
    im = Image.new("RGBA", (32, 32), C_IND_GREY)
    draw = ImageDraw.Draw(im)
    draw.line([(0, 0), (31, 0)], fill=C_LIGHT_METAL)
    draw.line([(0, 31), (31, 31)], fill=C_DARK_BASE)
    # Impact fracture
    crack_pts = [(16, 0), (14, 8), (18, 14), (11, 22), (15, 31)]
    draw.line(crack_pts, fill=C_DARK_BASE, width=1)
    # Exposed wire (cyan spark hint)
    draw.line([(13, 15), (15, 17)], fill=C_PRIMARY_CYAN)
    save_sprite(im, "Tiles/LAB_Tile_Floor_Damaged.png")

    # 1.4 Floor Hazard Stripe
    im = Image.new("RGBA", (32, 32), C_IND_GREY)
    draw = ImageDraw.Draw(im)
    draw.line([(0, 0), (31, 0)], fill=C_LIGHT_METAL)
    # Hazard band on top 10 pixels
    draw.rectangle([0, 1, 31, 10], fill=C_DEEP_CHARCOAL)
    for x in range(-16, 48, 8):
        draw.polygon([(x, 10), (x+6, 1), (x+10, 1), (x+4, 10)], fill=C_IND_YELLOW)
    draw.line([(0, 11), (31, 11)], fill=C_DARK_BASE)
    save_sprite(im, "Tiles/LAB_Tile_Floor_Hazard.png")

    # 1.5 Platform Surface
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Catwalk deck (top 14 pixels)
    draw.rectangle([0, 0, 31, 10], fill=C_IND_GREY)
    draw.line([(0, 0), (31, 0)], fill=C_LIGHT_METAL)
    draw.line([(0, 1), (31, 1)], fill=C_LIGHT_METAL)
    # Perforated tread dots
    for x in range(2, 30, 4):
        for y in range(4, 9, 3):
            im.putpixel((x, y), C_DEEP_CHARCOAL)
            im.putpixel((x+1, y), C_LIGHT_METAL)
    draw.line([(0, 10), (31, 10)], fill=C_DARK_BASE)
    # Underside truss flange
    draw.rectangle([0, 11, 31, 14], fill=C_DARK_BLUE_GREY)
    save_sprite(im, "Tiles/LAB_Tile_Platform_Surface.png")

    # 1.6 Platform Edge Left
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([4, 0, 31, 10], fill=C_IND_GREY)
    draw.line([(4, 0), (31, 0)], fill=C_LIGHT_METAL)
    # Rounded left lip
    draw.polygon([(0, 2), (4, 0), (4, 12), (1, 10)], fill=C_LIGHT_METAL)
    draw.line([(4, 10), (31, 10)], fill=C_DARK_BASE)
    draw.rectangle([4, 11, 31, 14], fill=C_DARK_BLUE_GREY)
    save_sprite(im, "Tiles/LAB_Tile_Platform_Edge_L.png")

    # 1.7 Platform Edge Right
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([0, 0, 27, 10], fill=C_IND_GREY)
    draw.line([(0, 0), (27, 0)], fill=C_LIGHT_METAL)
    # Rounded right lip
    draw.polygon([(27, 0), (31, 2), (30, 10), (27, 12)], fill=C_LIGHT_METAL)
    draw.line([(0, 10), (27, 10)], fill=C_DARK_BASE)
    draw.rectangle([0, 11, 27, 14], fill=C_DARK_BLUE_GREY)
    save_sprite(im, "Tiles/LAB_Tile_Platform_Edge_R.png")

    # 1.8 Platform Support Strut
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Center vertical column
    draw.rectangle([11, 0, 20, 31], fill=C_DARK_BLUE_GREY)
    draw.rectangle([13, 0, 18, 31], fill=C_DEEP_CHARCOAL)
    draw.line([(11, 0), (11, 31)], fill=C_LIGHT_METAL)
    # Gusset brackets
    draw.polygon([(3, 0), (11, 0), (11, 12)], fill=C_IND_GREY)
    draw.polygon([(28, 0), (20, 0), (20, 12)], fill=C_IND_GREY)
    save_sprite(im, "Tiles/LAB_Tile_Platform_Support.png")

    # 1.9 Wall Straight
    im = Image.new("RGBA", (32, 32), C_DARK_BLUE_GREY)
    draw = ImageDraw.Draw(im)
    draw.line([(0, 0), (0, 31)], fill=C_LIGHT_METAL)
    draw.line([(31, 0), (31, 31)], fill=C_DARK_BASE)
    draw.line([(1, 15), (30, 15)], fill=C_DEEP_CHARCOAL)
    draw.line([(1, 16), (30, 16)], fill=C_DARK_BASE)
    save_sprite(im, "Tiles/LAB_Tile_Wall_Straight.png")

    # 1.10 Wall Panel (with status diode)
    im = Image.new("RGBA", (32, 32), C_DARK_BLUE_GREY)
    draw = ImageDraw.Draw(im)
    draw.rectangle([4, 4, 27, 27], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    # Cyan status diode
    im.putpixel((16, 10), C_PRIMARY_CYAN)
    im.putpixel((15, 10), C_BRIGHT_CYAN)
    save_sprite(im, "Tiles/LAB_Tile_Wall_Panel.png")

    # 1.11 Ceiling Straight
    im = Image.new("RGBA", (32, 32), C_DEEP_CHARCOAL)
    draw = ImageDraw.Draw(im)
    draw.line([(0, 0), (31, 0)], fill=C_DARK_BASE)
    draw.rectangle([0, 24, 31, 31], fill=C_IND_GREY)
    draw.line([(0, 31), (31, 31)], fill=C_LIGHT_METAL)
    save_sprite(im, "Tiles/LAB_Tile_Ceiling_Straight.png")

    # 1.12 Ceiling Pipes
    im = Image.new("RGBA", (32, 32), C_DEEP_CHARCOAL)
    draw = ImageDraw.Draw(im)
    # Upper heavy horizontal conduit
    draw.rectangle([0, 8, 31, 18], fill=C_IND_GREY)
    draw.line([(0, 9), (31, 9)], fill=C_LIGHT_METAL)
    draw.line([(0, 18), (31, 18)], fill=C_DARK_BASE)
    # Thin secondary cyan fluid tube
    draw.rectangle([0, 22, 31, 26], fill=C_DARK_CYAN)
    draw.line([(0, 23), (31, 23)], fill=C_PRIMARY_CYAN)
    save_sprite(im, "Tiles/LAB_Tile_Ceiling_Pipes.png")

# ----------------------------------------------------------------------------
# 2. STRUCTURE (Large Modular Sprites)
# ----------------------------------------------------------------------------

def create_structures():
    # 2.1 Wall A (32x64)
    im = Image.new("RGBA", (32, 64), C_DARK_BLUE_GREY)
    draw = ImageDraw.Draw(im)
    draw.rectangle([0, 0, 31, 63], outline=C_DEEP_CHARCOAL)
    draw.line([(0, 0), (0, 63)], fill=C_LIGHT_METAL)
    draw.line([(31, 0), (31, 63)], fill=C_DARK_BASE)
    # Conduit raceway down center
    draw.rectangle([13, 0, 18, 63], fill=C_DEEP_CHARCOAL)
    draw.line([(15, 0), (15, 63)], fill=C_DARK_CYAN)
    save_sprite(im, "Environment/Background/LAB_Wall_A.png")

    # 2.2 Wall B (32x64) - Reinforced blast panel with viewing slot
    im = Image.new("RGBA", (32, 64), C_DEEP_CHARCOAL)
    draw = ImageDraw.Draw(im)
    draw.rectangle([2, 2, 29, 61], fill=C_DARK_BLUE_GREY, outline=C_IND_GREY)
    # Narrow reinforced observation glass
    draw.rectangle([8, 20, 23, 28], fill=C_DARK_CYAN, outline=C_DARK_BASE)
    draw.line([(9, 21), (22, 21)], fill=C_PRIMARY_CYAN)
    save_sprite(im, "Environment/Background/LAB_Wall_B.png")

    # 2.3 Pillar A-7 (32x128)
    im = Image.new("RGBA", (32, 128), C_DEEP_CHARCOAL)
    draw = ImageDraw.Draw(im)
    # Base collar
    draw.rectangle([2, 108, 29, 127], fill=C_IND_GREY, outline=C_LIGHT_METAL)
    # Top capital
    draw.rectangle([2, 0, 29, 18], fill=C_IND_GREY, outline=C_LIGHT_METAL)
    # Main column body
    draw.rectangle([5, 19, 26, 107], fill=C_DARK_BLUE_GREY)
    draw.line([(5, 19), (5, 107)], fill=C_LIGHT_METAL)
    draw.line([(26, 19), (26, 107)], fill=C_DARK_BASE)
    # Hydraulic cylinder clamps
    draw.rectangle([3, 45, 28, 52], fill=C_IND_GREY, outline=C_DEEP_CHARCOAL)
    draw.rectangle([3, 85, 28, 92], fill=C_IND_GREY, outline=C_DEEP_CHARCOAL)
    # Stencil "A-7" in muted yellow
    # Simple 5x7 bitmap letters
    # 'A' at x=10, y=65
    for px, py in [(12,65),(11,66),(13,66),(10,67),(14,67),(10,68),(11,68),(12,68),(13,68),(14,68),(10,69),(14,69),(10,70),(14,70)]:
        im.putpixel((px, py), C_IND_YELLOW)
    # '-' at x=16, y=67
    im.putpixel((16, 67), C_IND_YELLOW)
    im.putpixel((17, 67), C_IND_YELLOW)
    # '7' at x=19, y=65
    for px, py in [(19,65),(20,65),(21,65),(22,65),(23,65),(23,66),(22,67),(21,68),(21,69),(21,70)]:
        im.putpixel((px, py), C_IND_YELLOW)
    save_sprite(im, "Environment/Background/LAB_Pillar_A.png")

    # 2.4 Pillar B-3 (32x128)
    im = Image.new("RGBA", (32, 128), C_DEEP_CHARCOAL)
    draw = ImageDraw.Draw(im)
    draw.rectangle([2, 108, 29, 127], fill=C_IND_GREY, outline=C_LIGHT_METAL)
    draw.rectangle([2, 0, 29, 18], fill=C_IND_GREY, outline=C_LIGHT_METAL)
    draw.rectangle([5, 19, 26, 107], fill=C_DARK_BLUE_GREY)
    draw.line([(5, 19), (5, 107)], fill=C_LIGHT_METAL)
    draw.line([(26, 19), (26, 107)], fill=C_DARK_BASE)
    # Cable harness wrap
    for cy in range(30, 100, 14):
        draw.line([(5, cy), (26, cy+3)], fill=C_DARK_BASE, width=2)
    # Stencil "B-3"
    for px, py in [(11,65),(12,65),(13,65),(11,66),(14,66),(11,67),(12,67),(13,67),(11,68),(14,68),(11,69),(12,69),(13,69)]:
        im.putpixel((px, py), C_IND_YELLOW)
    im.putpixel((16, 67), C_IND_YELLOW)
    im.putpixel((17, 67), C_IND_YELLOW)
    for px, py in [(19,65),(20,65),(21,65),(22,65),(22,66),(20,67),(21,67),(22,68),(22,69),(19,70),(20,70),(21,70)]:
        im.putpixel((px, py), C_IND_YELLOW)
    save_sprite(im, "Environment/Background/LAB_Pillar_B.png")

    # 2.5 Beam A (64x16)
    im = Image.new("RGBA", (64, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([0, 0, 63, 15], fill=C_IND_GREY)
    draw.line([(0, 0), (63, 0)], fill=C_LIGHT_METAL)
    draw.line([(0, 15), (63, 15)], fill=C_DARK_BASE)
    # Triangular lightening cutouts
    for x in range(4, 60, 12):
        draw.polygon([(x+2, 12), (x+5, 3), (x+8, 12)], fill=C_DARK_BASE)
    save_sprite(im, "Environment/Background/LAB_Beam_A.png")

    # 2.6 Security Door A (32x64)
    im = Image.new("RGBA", (32, 64), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Heavy outer door frame
    draw.rectangle([0, 0, 31, 63], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    # Recessed sliding door slabs
    draw.rectangle([4, 6, 27, 63], fill=C_IND_GREY, outline=C_LIGHT_METAL)
    draw.line([(15, 6), (15, 63)], fill=C_DARK_BASE)
    draw.line([(16, 6), (16, 63)], fill=C_DEEP_CHARCOAL)
    # Hydraulic overhead locks
    draw.rectangle([10, 8, 21, 14], fill=C_DARK_BLUE_GREY)
    # Warning indicator above door
    im.putpixel((15, 3), C_WARN_ORANGE)
    im.putpixel((16, 3), C_WARN_ORANGE)
    save_sprite(im, "Environment/Props/LAB_Door_A.png")

# ----------------------------------------------------------------------------
# 3. HERO CONTAINMENT & SECONDARY STASIS PODS
# ----------------------------------------------------------------------------

def create_containment():
    # 3.1 HERO STASIS CHAMBER (64x96) - Primary Focal Point of Act 1
    im = Image.new("RGBA", (64, 96), C_CLEAR)
    draw = ImageDraw.Draw(im)

    # Base Collar (bottom 20 pixels: y=76 to 95)
    draw.rectangle([6, 76, 57, 95], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    draw.rectangle([10, 80, 53, 93], fill=C_IND_GREY)
    draw.line([(10, 80), (53, 80)], fill=C_LIGHT_METAL)
    # Cable feeds clamping into floor
    draw.line([(12, 90), (4, 95)], fill=C_DEEP_CHARCOAL, width=2)
    draw.line([(51, 90), (59, 95)], fill=C_DEEP_CHARCOAL, width=2)

    # Top Collar (top 18 pixels: y=0 to 17)
    draw.rectangle([8, 0, 55, 17], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    draw.rectangle([12, 4, 51, 15], fill=C_IND_GREY)
    draw.line([(12, 15), (51, 15)], fill=C_LIGHT_METAL)
    # Overhead hydraulic manifold
    draw.line([(24, 0), (24, 4)], fill=C_LIGHT_METAL, width=2)
    draw.line([(39, 0), (39, 4)], fill=C_LIGHT_METAL, width=2)

    # Transparent Cylinder (y=18 to 75, x=12 to 51)
    # Outer glass wall
    draw.rectangle([12, 18, 51, 75], fill=C_DARK_CYAN, outline=C_LIGHT_METAL)
    # Inner fluid volume with cyan gradient
    for y in range(19, 75):
        factor = (y - 19) / 56.0
        # Color interpolation from primary cyan glow at center to deep cyan
        if 32 <= y <= 62:
            fluid_c = C_PRIMARY_CYAN
        else:
            fluid_c = C_DARK_CYAN
        draw.line([(14, y), (49, y)], fill=fluid_c)

    # Suspended Divine Silhouette (Aeron's embryonic form)
    # Head
    draw.ellipse([30, 28, 34, 33], fill=C_DARK_BASE)
    # Torso
    draw.line([(32, 34), (32, 48)], fill=C_DARK_BASE, width=3)
    # Limbs trailing loosely in suspension
    draw.line([(31, 37), (27, 44)], fill=C_DARK_BASE, width=2) # Left arm
    draw.line([(33, 37), (37, 44)], fill=C_DARK_BASE, width=2) # Right arm
    draw.line([(31, 48), (29, 62)], fill=C_DARK_BASE, width=2) # Left leg
    draw.line([(33, 48), (35, 62)], fill=C_DARK_BASE, width=2) # Right leg

    # Specular Core Arcs & Energy Glow (#CFF4FF)
    draw.line([(30, 24), (34, 24)], fill=C_BRIGHT_CYAN)
    draw.line([(22, 40), (24, 42)], fill=C_BRIGHT_CYAN)
    draw.line([(40, 50), (42, 52)], fill=C_BRIGHT_CYAN)

    # Vertical structural glass struts (left and right)
    draw.rectangle([10, 18, 14, 75], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    draw.rectangle([49, 18, 53, 75], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)

    # Diagnostic Mini-Terminal mounted on bottom right base
    draw.rectangle([44, 72, 60, 84], fill=C_DEEP_CHARCOAL, outline=C_LIGHT_METAL)
    draw.rectangle([46, 74, 58, 80], fill=C_PRIMARY_CYAN) # Glowing screen
    im.putpixel((58, 82), C_WARN_ORANGE) # Amber warning diode

    save_sprite(im, "Environment/Containment/LAB_CONTAINMENT_Main.png")

    # 3.2 Secondary Containment Pod (32x64)
    im = Image.new("RGBA", (32, 64), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([4, 52, 27, 63], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    draw.rectangle([4, 0, 27, 12], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    # Murky inactive fluid
    draw.rectangle([7, 13, 24, 51], fill=C_DARK_BLUE_GREY, outline=C_DARK_BASE)
    draw.line([(10, 15), (21, 15)], fill=C_DARK_CYAN)
    save_sprite(im, "Environment/Containment/LAB_Containment_Small.png")

    # 3.3 Damaged / Cracked Containment (48x80)
    im = Image.new("RGBA", (48, 80), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([6, 64, 41, 79], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    draw.rectangle([6, 0, 41, 14], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    draw.rectangle([10, 15, 37, 63], fill=C_DARK_BLUE_GREY, outline=C_LIGHT_METAL)
    # Severe impact spiderweb crack
    draw.line([(24, 38), (14, 25)], fill=C_LIGHT_METAL)
    draw.line([(24, 38), (34, 30)], fill=C_LIGHT_METAL)
    draw.line([(24, 38), (20, 52)], fill=C_LIGHT_METAL)
    draw.line([(24, 38), (30, 58)], fill=C_LIGHT_METAL)
    # Leaked fluid pool on floor
    draw.ellipse([2, 75, 45, 79], fill=C_DARK_CYAN)
    save_sprite(im, "Environment/Containment/LAB_Containment_Damaged.png")

# ----------------------------------------------------------------------------
# 4. MACHINERY & CONSOLES
# ----------------------------------------------------------------------------

def create_machinery():
    # 4.1 Console A (64x32) - Large Research Terminal
    im = Image.new("RGBA", (64, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Lower desk chassis
    draw.rectangle([4, 16, 59, 31], fill=C_IND_GREY, outline=C_DEEP_CHARCOAL)
    draw.line([(4, 16), (59, 16)], fill=C_LIGHT_METAL)
    # Angled keyboard shelf
    draw.rectangle([8, 17, 55, 21], fill=C_DEEP_CHARCOAL)
    # Left Monitor
    draw.rectangle([6, 2, 23, 15], fill=C_DEEP_CHARCOAL, outline=C_LIGHT_METAL)
    draw.rectangle([8, 4, 21, 13], fill=C_PRIMARY_CYAN)
    # Center Wide Monitor
    draw.rectangle([25, 0, 45, 15], fill=C_DEEP_CHARCOAL, outline=C_LIGHT_METAL)
    draw.rectangle([27, 2, 43, 13], fill=C_PRIMARY_CYAN)
    # Right Angled Monitor
    draw.rectangle([47, 2, 61, 15], fill=C_DEEP_CHARCOAL, outline=C_LIGHT_METAL)
    draw.rectangle([49, 4, 59, 13], fill=C_DARK_CYAN)
    # Status buttons
    im.putpixel((12, 26), C_WARN_ORANGE)
    im.putpixel((15, 26), C_IND_YELLOW)
    save_sprite(im, "Environment/Machinery/LAB_Console_A.png")

    # 4.2 High-Voltage Power Unit (48x48)
    im = Image.new("RGBA", (48, 48), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([4, 6, 43, 47], fill=C_IND_GREY, outline=C_DEEP_CHARCOAL)
    draw.line([(4, 6), (43, 6)], fill=C_LIGHT_METAL)
    # Cooling fins
    for y in range(12, 38, 4):
        draw.line([(8, y), (22, y)], fill=C_DEEP_CHARCOAL)
    # Transformer coil core
    draw.rectangle([26, 12, 39, 32], fill=C_DEEP_CHARCOAL, outline=C_DARK_CYAN)
    draw.line([(28, 22), (37, 22)], fill=C_PRIMARY_CYAN)
    # Warning lightning decal
    draw.polygon([(14, 40), (18, 40), (15, 43), (19, 43), (13, 47), (15, 44), (12, 44)], fill=C_IND_YELLOW)
    save_sprite(im, "Environment/Machinery/LAB_PowerUnit_A.png")

    # 4.3 Server Rack (32x80)
    im = Image.new("RGBA", (32, 80), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([2, 0, 29, 79], fill=C_DEEP_CHARCOAL, outline=C_IND_GREY)
    # Server blade trays
    for y in range(4, 76, 8):
        draw.rectangle([5, y, 26, y+5], fill=C_DARK_BLUE_GREY, outline=C_DARK_BASE)
        # Status blinking LEDs
        im.putpixel((7, y+2), C_PRIMARY_CYAN)
        im.putpixel((10, y+2), C_IND_YELLOW)
        im.putpixel((13, y+2), C_WARN_ORANGE)
    save_sprite(im, "Environment/Machinery/LAB_ServerRack_A.png")

# ----------------------------------------------------------------------------
# 5. INDUSTRIAL PIPING & PROPS
# ----------------------------------------------------------------------------

def create_props():
    # 5.1 Pipe Horizontal (32x32)
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([0, 10, 31, 22], fill=C_IND_GREY)
    draw.line([(0, 10), (31, 10)], fill=C_LIGHT_METAL)
    draw.line([(0, 11), (31, 11)], fill=C_LIGHT_METAL)
    draw.line([(0, 22), (31, 22)], fill=C_DARK_BASE)
    # Flange ring
    draw.rectangle([13, 8, 18, 24], fill=C_LIGHT_METAL, outline=C_DEEP_CHARCOAL)
    save_sprite(im, "Environment/Props/LAB_Pipe_A.png")

    # 5.2 Pipe Vertical (32x32)
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([10, 0, 22, 31], fill=C_IND_GREY)
    draw.line([(10, 0), (10, 31)], fill=C_LIGHT_METAL)
    draw.line([(11, 0), (11, 31)], fill=C_LIGHT_METAL)
    draw.line([(22, 0), (22, 31)], fill=C_DARK_BASE)
    draw.rectangle([8, 13, 24, 18], fill=C_LIGHT_METAL, outline=C_DEEP_CHARCOAL)
    save_sprite(im, "Environment/Props/LAB_Pipe_B.png")

    # 5.3 Pipe T-Junction with Pressure Gauge (32x32)
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Main horizontal run
    draw.rectangle([0, 16, 31, 28], fill=C_IND_GREY)
    draw.line([(0, 16), (31, 16)], fill=C_LIGHT_METAL)
    draw.line([(0, 28), (31, 28)], fill=C_DARK_BASE)
    # Vertical pipe rising
    draw.rectangle([12, 6, 20, 16], fill=C_IND_GREY)
    draw.line([(12, 6), (12, 16)], fill=C_LIGHT_METAL)
    draw.line([(20, 6), (20, 16)], fill=C_DARK_BASE)
    # Round Pressure Gauge dial
    draw.ellipse([8, 0, 24, 12], fill=C_LIGHT_METAL, outline=C_DEEP_CHARCOAL)
    draw.ellipse([10, 2, 22, 10], fill=C_DARK_BLUE_GREY)
    # Needle
    draw.line([(16, 6), (19, 4)], fill=C_WARN_ORANGE)
    save_sprite(im, "Environment/Props/LAB_Pipe_Junction.png")

    # 5.4 Wheel Valve A (32x32)
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([0, 16, 31, 26], fill=C_IND_GREY)
    # Red/Orange round valve wheel
    draw.ellipse([8, 2, 24, 14], fill=C_CLEAR, outline=C_WARN_ORANGE, width=2)
    draw.line([(16, 2), (16, 14)], fill=C_WARN_ORANGE)
    draw.line([(8, 8), (24, 8)], fill=C_WARN_ORANGE)
    draw.rectangle([14, 12, 18, 16], fill=C_LIGHT_METAL)
    save_sprite(im, "Environment/Props/LAB_Valve_A.png")

    # 5.5 Safety Railing (32x16)
    im = Image.new("RGBA", (32, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Top handrail
    draw.line([(0, 2), (31, 2)], fill=C_LIGHT_METAL, width=2)
    # Mid-height safety wire
    draw.line([(0, 8), (31, 8)], fill=C_IND_GREY)
    # Vertical stanchions
    draw.line([(2, 2), (2, 15)], fill=C_LIGHT_METAL, width=2)
    draw.line([(30, 2), (30, 15)], fill=C_LIGHT_METAL, width=2)
    save_sprite(im, "Environment/Props/LAB_Railing_A.png")

# ----------------------------------------------------------------------------
# 6. DECALS (Transparent Stencils)
# ----------------------------------------------------------------------------

def create_decals():
    # 6.1 Decal A-7 (32x16)
    im = Image.new("RGBA", (32, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # 'A'
    for px, py in [(8,4),(7,5),(9,5),(6,6),(10,6),(6,7),(7,7),(8,7),(9,7),(10,7),(6,8),(10,8),(6,9),(10,9)]:
        im.putpixel((px, py), C_IND_YELLOW)
    # '-'
    im.putpixel((13, 7), C_IND_YELLOW)
    im.putpixel((14, 7), C_IND_YELLOW)
    # '7'
    for px, py in [(17,4),(18,4),(19,4),(20,4),(21,4),(21,5),(20,6),(19,7),(19,8),(19,9)]:
        im.putpixel((px, py), C_IND_YELLOW)
    save_sprite(im, "Environment/Decals/LAB_DECAL_A7.png")

    # 6.2 Decal B-3 (32x16)
    im = Image.new("RGBA", (32, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # 'B'
    for px, py in [(8,4),(9,4),(10,4),(8,5),(11,5),(8,6),(9,6),(10,6),(8,7),(11,7),(8,8),(11,8),(8,9),(9,9),(10,9)]:
        im.putpixel((px, py), C_IND_YELLOW)
    # '-'
    im.putpixel((13, 7), C_IND_YELLOW)
    im.putpixel((14, 7), C_IND_YELLOW)
    # '3'
    for px, py in [(17,4),(18,4),(19,4),(20,4),(20,5),(18,6),(19,6),(20,7),(20,8),(17,9),(18,9),(19,9)]:
        im.putpixel((px, py), C_IND_YELLOW)
    save_sprite(im, "Environment/Decals/LAB_DECAL_B3.png")

    # 6.3 Decal Restricted (32x16)
    im = Image.new("RGBA", (32, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.rectangle([1, 2, 30, 13], outline=C_WARN_ORANGE)
    draw.line([(4, 5), (27, 5)], fill=C_WARN_ORANGE)
    draw.line([(4, 10), (27, 10)], fill=C_WARN_ORANGE)
    save_sprite(im, "Environment/Decals/LAB_DECAL_Restricted.png")

# ----------------------------------------------------------------------------
# 7. VFX SPRITES
# ----------------------------------------------------------------------------

def create_vfx():
    # 7.1 Steam Puff (32x32)
    im = Image.new("RGBA", (32, 32), C_CLEAR)
    draw = ImageDraw.Draw(im)
    steam_c = (74, 83, 92, 120)
    draw.ellipse([8, 8, 24, 24], fill=steam_c)
    draw.ellipse([12, 4, 20, 16], fill=(111, 227, 255, 60))
    save_sprite(im, "Environment/VFX/FX_Steam_A.png")

    # 7.2 Spark (16x16)
    im = Image.new("RGBA", (16, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.line([(8, 1), (8, 14)], fill=C_BRIGHT_CYAN)
    draw.line([(1, 8), (14, 8)], fill=C_BRIGHT_CYAN)
    draw.rectangle([7, 7, 8, 8], fill=(255, 255, 255, 255))
    save_sprite(im, "Environment/VFX/FX_Spark_A.png")

    # 7.3 Cyan Energy Mote (16x16)
    im = Image.new("RGBA", (16, 16), C_CLEAR)
    draw = ImageDraw.Draw(im)
    draw.polygon([(8, 2), (13, 8), (8, 13), (3, 8)], fill=C_DARK_CYAN)
    draw.polygon([(8, 4), (11, 8), (8, 11), (5, 8)], fill=C_PRIMARY_CYAN)
    im.putpixel((8, 8), C_BRIGHT_CYAN)
    save_sprite(im, "Environment/VFX/FX_CyanParticle_A.png")

    # 7.4 Dust Mote (8x8)
    im = Image.new("RGBA", (8, 8), C_CLEAR)
    dust_c = (207, 244, 255, 140)
    im.putpixel((3, 3), dust_c)
    im.putpixel((4, 3), dust_c)
    im.putpixel((3, 4), dust_c)
    im.putpixel((4, 4), dust_c)
    save_sprite(im, "Environment/VFX/FX_Dust_A.png")

# ----------------------------------------------------------------------------
# 8. TEMPORARY PLAYER SILHOUETTE PROXY (32x64)
# ----------------------------------------------------------------------------

def create_player_proxy():
    im = Image.new("RGBA", (32, 64), C_CLEAR)
    draw = ImageDraw.Draw(im)
    # Head (height ~56 to 62)
    draw.ellipse([12, 6, 19, 14], fill=C_DARK_BASE)
    # Hood / Collar rim in faint cyan
    draw.line([(11, 13), (20, 13)], fill=C_DARK_CYAN)
    # Torso & Coat
    draw.polygon([(11, 15), (20, 15), (23, 44), (8, 44)], fill=C_DARK_BASE)
    # Faint rim highlight on left edge to pop against dark backgrounds
    draw.line([(8, 20), (8, 44)], fill=C_DARK_CYAN)
    # Legs (boots grounded at bottom)
    draw.rectangle([10, 45, 14, 61], fill=C_DARK_BASE)
    draw.rectangle([17, 45, 21, 61], fill=C_DARK_BASE)
    # Boots
    draw.rectangle([8, 59, 14, 62], fill=C_DEEP_CHARCOAL)
    draw.rectangle([17, 59, 23, 62], fill=C_DEEP_CHARCOAL)
    save_sprite(im, "Environment/Gameplay/Player_Silhouette_Proxy.png")

if __name__ == "__main__":
    print("=== SYNTHESIZING LAST GOD 2D ASSET LIBRARY ===")
    create_tiles()
    create_structures()
    create_containment()
    create_machinery()
    create_props()
    create_decals()
    create_vfx()
    create_player_proxy()
    print("=== ALL ASSETS GENERATED SUCCESSFULLY ===")
