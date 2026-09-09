"""
LabCanvas.py — Blender Grease Pencil 2D Lab Scene Setup
Run this in Blender's scripting workspace (Text Editor → Run Script)

Creates:
  - 480×160 GBA-style render canvas
  - Named Grease Pencil layers: BG_Far, BG_Mid, FG_Silhouette, Props_Monitors, Props_Flasks, Props_Alarms
  - Colour palette materials matching the LAST-GOD Lab aesthetic
  - Skeleton keyframe animations for each prop
  - Render output to: <project>/Assets/Art/Backgrounds/Lab_Animated/
"""

import bpy
import os
import math

# ── Config ────────────────────────────────────────────────────────────────────
OUTPUT_DIR = r"c:\projects\LAST-GOD\Assets\Art\Backgrounds\Lab_Animated"
CANVAS_W   = 480
CANVAS_H   = 160
FPS        = 12
TOTAL_FRAMES = 96   # 8-second loop at 12fps

# LAST-GOD Lab palette (hex → linear RGB)
def hex_to_linear(hex_str):
    h = hex_str.lstrip('#')
    r, g, b = (int(h[i:i+2], 16) / 255.0 for i in (0, 2, 4))
    return (r ** 2.2, g ** 2.2, b ** 2.2, 1.0)  # gamma to linear

PALETTE = {
    "DarkWall":    "#0a0a1a",
    "SteelGray":   "#2a2a3a",
    "CyanGlow":    "#00d4ff",
    "AmberWarn":   "#ff6600",
    "MonitorGreen":"#00ff41",
    "ShadowBlack": "#050508",
    "MidGray":     "#1a2a3a",
    "WarnRed":     "#cc2200",
}

# ── 1. Scene Setup ────────────────────────────────────────────────────────────
def setup_scene():
    scene = bpy.context.scene
    scene.name = "LabCanvas"
    scene.render.resolution_x = CANVAS_W
    scene.render.resolution_y = CANVAS_H
    scene.render.fps          = FPS
    scene.frame_start         = 1
    scene.frame_end           = TOTAL_FRAMES

    # Pixel art render: no anti-aliasing, point sampling
    scene.render.engine            = 'BLENDER_EEVEE_NEXT'
    scene.render.filter_size       = 0.0
    scene.render.use_compositing   = False
    scene.eevee.taa_render_samples = 1

    # Transparent background
    scene.render.film_transparent  = True
    scene.render.image_settings.file_format       = 'PNG'
    scene.render.image_settings.color_mode        = 'RGBA'
    scene.render.image_settings.compression       = 0

    # Camera: orthographic 2D view
    for obj in bpy.data.objects:
        if obj.type == 'CAMERA':
            cam = obj.data
            cam.type              = 'ORTHO'
            cam.ortho_scale       = CANVAS_H / 100.0
            obj.location          = (0, 0, 10)
            obj.rotation_euler    = (0, 0, 0)
            break

    # World: black background
    world = bpy.data.worlds.get("World") or bpy.data.worlds.new("World")
    scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs["Color"].default_value = (*hex_to_linear("#050508")[:3], 1)
        bg.inputs["Strength"].default_value = 1.0

    print("[LabCanvas] Scene configured: {}×{} @ {}fps".format(CANVAS_W, CANVAS_H, FPS))


# ── 2. Grease Pencil Object + Layers ─────────────────────────────────────────
def create_gp_object(name, location=(0, 0, 0)):
    gp_data = bpy.data.grease_pencils.new(name)
    gp_obj  = bpy.data.objects.new(name, gp_data)
    bpy.context.scene.collection.objects.link(gp_obj)
    gp_obj.location = location
    return gp_obj, gp_data

def add_gp_layer(gp_data, name, color_hex, opacity=1.0):
    layer  = gp_data.layers.new(name, set_active=True)
    layer.opacity = opacity
    mat = bpy.data.materials.new(name=name + "_mat")
    bpy.data.materials.create_gpencil_data(mat)
    mat.grease_pencil.show_stroke = True
    mat.grease_pencil.show_fill   = True
    col = hex_to_linear(color_hex)
    mat.grease_pencil.color       = col
    mat.grease_pencil.fill_color  = col
    gp_data.materials.append(mat)
    return layer, mat

def setup_lab_gp_object():
    """Create the main lab Grease Pencil canvas with all layers."""
    gp_obj, gp_data = create_gp_object("LabBackground")

    layers = [
        ("BG_Far",          "#0a0a1a", 1.0),
        ("BG_Mid",          "#1a2a3a", 1.0),
        ("FG_Silhouette",   "#050508", 1.0),
        ("Props_Monitors",  "#00ff41", 1.0),
        ("Props_Flasks",    "#00d4ff", 1.0),
        ("Props_Alarms",    "#ff6600", 1.0),
    ]

    for lname, color, opacity in layers:
        add_gp_layer(gp_data, lname, color, opacity)
        print("[LabCanvas] Layer added: {}".format(lname))

    return gp_obj, gp_data


# ── 3. Reference Images ───────────────────────────────────────────────────────
def import_reference_images():
    refs = [
        (r"c:\projects\LAST-GOD\act1-labbg sprite\frame-1.png", (-2.4, 0, -1)),
        (r"c:\projects\LAST-GOD\act1-labbg sprite\frame-2.png", ( 2.4, 0, -1)),
    ]
    for path, loc in refs:
        if not os.path.exists(path):
            print("[LabCanvas] WARNING: reference not found: {}".format(path))
            continue
        bpy.ops.object.load_reference_image(filepath=path)
        ref = bpy.context.active_object
        if ref:
            ref.location = loc
            ref.name = "REF_" + os.path.basename(path)
            # Make semi-transparent so it's just a tracing guide
            ref.empty_image_opacity = 0.35
            print("[LabCanvas] Reference loaded: {}".format(ref.name))


# ── 4. Animated Prop Objects ─────────────────────────────────────────────────
def create_monitor_prop():
    """Monitor screen: flickering cyan/green glow — 8-frame loop."""
    gp_obj, gp_data = create_gp_object("Prop_Monitor", location=(-1.8, 0, 0))
    layer, mat = add_gp_layer(gp_data, "Screen", "#00ff41")

    # Simple rectangle stroke for the screen face
    frame = gp_data.layers[0].frames.new(1)
    stroke = frame.strokes.new()
    stroke.display_mode = '3DSPACE'
    stroke.points.add(count=5)
    w, h = 0.3, 0.2
    pts = [(-w, 0, -h), (w, 0, -h), (w, 0, h), (-w, 0, h), (-w, 0, -h)]
    for i, (x, y, z) in enumerate(pts):
        stroke.points[i].co        = (x, y, z)
        stroke.points[i].pressure  = 1.0

    # Keyframe opacity animation: flicker pattern over 8 frames
    gp_data.layers[0].opacity = 1.0
    flicker_pattern = [1.0, 0.9, 0.3, 1.0, 0.8, 0.1, 0.95, 1.0]  # 8-frame flicker
    for i, alpha in enumerate(flicker_pattern):
        bpy.context.scene.frame_set(i + 1)
        gp_data.layers[0].opacity = alpha
        gp_data.layers[0].keyframe_insert(data_path="opacity", frame=i + 1)

    print("[LabCanvas] Monitor prop created with flicker animation")
    return gp_obj

def create_flask_prop():
    """Bubbling flask: pulsing scale + color shift — 16-frame loop."""
    gp_obj, gp_data = create_gp_object("Prop_Flask", location=(0, 0, 0))
    add_gp_layer(gp_data, "Liquid", "#00d4ff")

    # Animate object scale to simulate bubbling (scale Y oscillates)
    for frame in range(1, 17):
        bpy.context.scene.frame_set(frame)
        t = (frame - 1) / 15.0
        # Sine wave: slight up-down scale
        scale_y = 1.0 + 0.08 * math.sin(t * math.pi * 2)
        gp_obj.scale = (1.0, 1.0, scale_y)
        gp_obj.keyframe_insert(data_path="scale", frame=frame)

    print("[LabCanvas] Flask prop created with bubble animation")
    return gp_obj

def create_alarm_prop():
    """Alarm light: hard on/off blink — 6-frame loop."""
    gp_obj, gp_data = create_gp_object("Prop_Alarm", location=(1.8, 0, 0.4))
    layer, mat = add_gp_layer(gp_data, "Light", "#ff6600")

    # Blink: on for 3 frames, off for 3 frames
    blink_pattern = [1.0, 1.0, 1.0, 0.0, 0.0, 0.0]
    for i, alpha in enumerate(blink_pattern):
        gp_data.layers[0].opacity = alpha
        gp_data.layers[0].keyframe_insert(data_path="opacity", frame=i + 1)

    # NLA push-down to make it loop
    print("[LabCanvas] Alarm prop created with blink animation")
    return gp_obj

def create_steam_prop():
    """Steam vent: slow rise, fast fade — 24-frame loop using GP layer opacity."""
    gp_obj, gp_data = create_gp_object("Prop_Steam", location=(0.6, 0, -0.3))
    add_gp_layer(gp_data, "Steam", "#8aacb8", opacity=0.0)

    # Saw-wave: slow fade in (19 frames), fast fade out (5 frames)
    for frame in range(1, 25):
        t = (frame - 1) / 23.0
        if t < 0.8:
            alpha = t / 0.8 * 0.7  # fade in to 0.7
        else:
            alpha = (1.0 - (t - 0.8) / 0.2) * 0.7  # fast fade out
        bpy.context.scene.frame_set(frame)
        gp_data.layers[0].opacity = alpha
        gp_data.layers[0].keyframe_insert(data_path="opacity", frame=frame)

    print("[LabCanvas] Steam prop created with pulse animation")
    return gp_obj


# ── 5. Render Setup ───────────────────────────────────────────────────────────
def configure_render_output():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    scene = bpy.context.scene
    scene.render.filepath = os.path.join(OUTPUT_DIR, "lab_anim_####")
    print("[LabCanvas] Render output set to: {}".format(OUTPUT_DIR))
    print("[LabCanvas] Run Render → Render Animation to export PNG sequence")


# ── MAIN ──────────────────────────────────────────────────────────────────────
def main():
    print("\n" + "="*60)
    print("  LAST-GOD :: LabCanvas Setup")
    print("="*60)

    setup_scene()
    gp_obj, gp_data = setup_lab_gp_object()
    import_reference_images()

    # Animated props
    create_monitor_prop()
    create_flask_prop()
    create_alarm_prop()
    create_steam_prop()

    configure_render_output()

    # Save the blend file
    blend_path = r"c:\projects\LAST-GOD\design\LabCanvas.blend"
    os.makedirs(os.path.dirname(blend_path), exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    print("\n✅  LabCanvas.blend saved to: {}".format(blend_path))
    print("    Next steps:")
    print("    1. Trace the background layers in the Grease Pencil editor")
    print("    2. Run Render → Render Animation to export PNG sequences")
    print("    3. Unity will auto-import from Assets/Art/Backgrounds/Lab_Animated/")
    print("="*60 + "\n")


main()
