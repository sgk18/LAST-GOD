"""
LabCanvas_MCP.py — Run this via Blender MCP execute_blender_code
Paste the content of this file as the 'code' parameter.

Compatible with Blender 4.0+ (auto-detects GP v3 API for 4.3+)

Creates the full LAST-GOD Lab Canvas setup in Blender:
 - 480×160 canvas, 12fps, GP layers, 4 animated props
 - Reference images loaded as traceable guides
 - Saves to c:/projects/LAST-GOD/design/LabCanvas.blend
"""

import bpy
import os
import math

OUTPUT_DIR = r"c:/projects/LAST-GOD/Assets/Art/Backgrounds/Lab_Animated"
BLEND_PATH = r"c:/projects/LAST-GOD/design/LabCanvas.blend"

# Detect Blender version
BL_VER = bpy.app.version  # e.g. (4, 3, 0)
USE_GP_V3 = BL_VER >= (4, 3, 0)
print(f"Blender {BL_VER} — GP API: {'v3' if USE_GP_V3 else 'legacy'}")

def hex_rgb(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i+2], 16)/255.0 for i in (0,2,4))

def new_gp_data(name):
    """Create a Grease Pencil datablock — handles both legacy and v3 API."""
    if USE_GP_V3:
        return bpy.data.grease_pencil_v3.new(name)
    return bpy.data.grease_pencils.new(name)

def new_gp_layer(gp_data, name):
    """Add a layer — v3 uses gp.layers.new(), legacy same."""
    return gp_data.layers.new(name, set_active=True)

def add_gp_material(gp_data, name, color_hex):
    """Create a GP stroke+fill material and append to the GP datablock."""
    mat = bpy.data.materials.new(name)
    bpy.data.materials.create_gpencil_data(mat)
    r, g, b = hex_rgb(color_hex)
    mat.grease_pencil.color = (r, g, b, 1.0)
    mat.grease_pencil.fill_color = (r, g, b, 1.0)
    mat.grease_pencil.show_stroke = True
    mat.grease_pencil.show_fill = True
    gp_data.materials.append(mat)
    return mat

# 1. Scene
bpy.context.scene.name = "LabCanvas"
bpy.context.scene.render.resolution_x = 480
bpy.context.scene.render.resolution_y = 160
bpy.context.scene.render.fps = 12
bpy.context.scene.frame_start = 1
bpy.context.scene.frame_end = 96
bpy.context.scene.render.film_transparent = True
bpy.context.scene.render.image_settings.file_format = 'PNG'
bpy.context.scene.render.image_settings.color_mode = 'RGBA'

# Camera → orthographic
for o in bpy.data.objects:
    if o.type == 'CAMERA':
        o.data.type = 'ORTHO'
        o.data.ortho_scale = 1.6
        o.location = (0, 0, 10)

print("✅ Scene configured")

# 2. Main Grease Pencil Object
gp = new_gp_data("LabBG")
gp_obj = bpy.data.objects.new("LabBG", gp)
bpy.context.scene.collection.objects.link(gp_obj)

layer_defs = [
    ("BG_Far",        "#0a0a1a"),
    ("BG_Mid",        "#1a2a3a"),
    ("FG_Silhouette", "#050508"),
    ("Props_Monitors","#00ff41"),
    ("Props_Flasks",  "#00d4ff"),
    ("Props_Alarms",  "#ff6600"),
]

for lname, color in layer_defs:
    new_gp_layer(gp, lname)
    add_gp_material(gp, lname, color)
    print(f"  Layer: {lname}")

print("✅ GP layers created")

# 3. Reference Images
refs = [
    (r"c:/projects/LAST-GOD/act1-labbg sprite/frame-1.png", (-2.4, 0, -1)),
    (r"c:/projects/LAST-GOD/act1-labbg sprite/frame-2.png", ( 2.4, 0, -1)),
]
for path, loc in refs:
    if os.path.exists(path):
        bpy.ops.object.load_reference_image(filepath=path)
        ref = bpy.context.active_object
        if ref:
            ref.location = loc
            ref.empty_image_opacity = 0.3
            ref.name = "REF_" + os.path.basename(path)
            print(f"  Reference: {ref.name}")
    else:
        print(f"  ⚠ Reference missing: {path}")

# 4. Animated prop helper
def make_gp_prop(name, loc, color, loop_frames):
    d = new_gp_data(name)
    o = bpy.data.objects.new(name, d)
    bpy.context.scene.collection.objects.link(o)
    o.location = loc
    new_gp_layer(d, "main")
    add_gp_material(d, name + "_mat", color)
    return o, d, d.layers[0]

# Monitor flicker — 8-frame loop
mon, mon_gp, mon_layer = make_gp_prop("Prop_Monitor", (-1.8, 0, 0.2), "#00ff41", 8)
flicker = [1.0, 0.9, 0.3, 1.0, 0.8, 0.1, 0.95, 1.0]
for i, a in enumerate(flicker):
    mon_gp.layers[0].opacity = a
    mon_gp.layers[0].keyframe_insert("opacity", frame=i+1)
print("✅ Monitor prop animated")

# Flask bubble — 16-frame loop
flask, flask_gp, flask_layer = make_gp_prop("Prop_Flask", (0, 0, 0), "#00d4ff", 16)
for f in range(1, 17):
    t = (f-1)/15.0
    s = 1.0 + 0.08 * math.sin(t * math.pi * 2)
    flask.scale = (1.0, 1.0, s)
    flask.keyframe_insert("scale", frame=f)
print("✅ Flask prop animated")

# Alarm blink — 6-frame loop
alarm, alarm_gp, alarm_layer = make_gp_prop("Prop_Alarm", (1.8, 0, 0.4), "#ff6600", 6)
blink = [1.0, 1.0, 1.0, 0.0, 0.0, 0.0]
for i, a in enumerate(blink):
    alarm_gp.layers[0].opacity = a
    alarm_gp.layers[0].keyframe_insert("opacity", frame=i+1)
print("✅ Alarm prop animated")

# Steam vent — 24-frame loop
steam, steam_gp, steam_layer = make_gp_prop("Prop_Steam", (0.6, 0, -0.3), "#8aacb8", 24)
for f in range(1, 25):
    t = (f-1)/23.0
    a = (t/0.8)*0.7 if t < 0.8 else ((1.0-(t-0.8)/0.2)*0.7)
    steam_gp.layers[0].opacity = a
    steam_gp.layers[0].keyframe_insert("opacity", frame=f)
print("✅ Steam prop animated")

# 5. Render output
os.makedirs(OUTPUT_DIR, exist_ok=True)
bpy.context.scene.render.filepath = OUTPUT_DIR + "/lab_####"

# 6. Save
os.makedirs(os.path.dirname(BLEND_PATH), exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"\n✅ LabCanvas.blend saved: {BLEND_PATH}")
print("   Next: Render → Render Animation to export PNG sequence")
