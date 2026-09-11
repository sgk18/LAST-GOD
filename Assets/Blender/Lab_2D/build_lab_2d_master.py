"""
build_lab_2d_master.py
Automated generator for LAB_2D_MASTER.blend
Constructs the 2D planning scene from Lab_Production.blend spatial data with:
- 8 dedicated collections
- Grease Pencil 2D architectural blocking
- Orthographic 2D platformer camera framing
- Multi-layer depth and parallax guides
- Rendered composition study
"""

import bpy
import math
from mathutils import Vector

def run():
    print("=== BUILDING LAB_2D_MASTER.blend ===")

    # Reset to empty scene
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.name = "LAB_2D_MASTER"

    # Setup render settings for 2D platformer (384x216 scaled 5x to 1920x1080)
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'

    # Ensure World with dark neutral background (#080B0F)
    if not scene.world:
        scene.world = bpy.data.worlds.new("Lab_2D_World")
    scene.world.use_nodes = True
    bg_node = scene.world.node_tree.nodes.get("Background")
    if bg_node:
        bg_node.inputs['Color'].default_value = (0.031, 0.043, 0.059, 1.0) # #080B0F
        bg_node.inputs['Strength'].default_value = 0.5

    # 1. Create Required Collections
    collection_names = [
        "GP_Background_Far",
        "GP_Background",
        "GP_Midground",
        "GP_Gameplay",
        "GP_Foreground",
        "GP_Architecture_Guide",
        "GP_Lighting_Guide",
        "GP_Annotation"
    ]

    collections = {}
    master_col = scene.collection
    for name in collection_names:
        col = bpy.data.collections.new(name)
        master_col.children.link(col)
        collections[name] = col
        print(f"Created collection: {name}")

    # Helper: Create materials for 2D visualization
    def get_or_create_mat(name, color, roughness=0.8, emission=None):
        if name in bpy.data.materials:
            return bpy.data.materials[name]
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs['Base Color'].default_value = color
            bsdf.inputs['Roughness'].default_value = roughness
            if emission:
                bsdf.inputs['Emission Color'].default_value = emission[0]
                bsdf.inputs['Emission Strength'].default_value = emission[1]
        return mat

    mat_dark = get_or_create_mat("M_DarkBase", (0.031, 0.043, 0.059, 1.0))
    mat_charcoal = get_or_create_mat("M_DeepCharcoal", (0.067, 0.086, 0.110, 1.0))
    mat_bluegrey = get_or_create_mat("M_DarkBlueGrey", (0.106, 0.141, 0.188, 1.0))
    mat_metal = get_or_create_mat("M_IndustrialGrey", (0.188, 0.220, 0.255, 1.0))
    mat_lightmetal = get_or_create_mat("M_LightMetal", (0.290, 0.325, 0.361, 1.0))
    mat_cyan_emit = get_or_create_mat("M_CyanEmission", (0.435, 0.890, 1.0, 1.0), emission=((0.435, 0.890, 1.0, 1.0), 3.0))
    mat_warning = get_or_create_mat("M_WarningOrange", (0.769, 0.314, 0.180, 1.0), emission=((0.769, 0.314, 0.180, 1.0), 1.5))
    mat_yellow = get_or_create_mat("M_IndustrialYellow", (0.702, 0.604, 0.271, 1.0))

    # Helper: Create 2D plane/mesh guide
    def create_block(name, col, loc, size, mat):
        mesh = bpy.data.meshes.new(name + "_Mesh")
        obj = bpy.data.objects.new(name, mesh)
        col.objects.link(obj)
        obj.location = loc

        # Create vertices for a box/plane along X and Z (where Z is 2D height, Y is depth)
        sx, sy, sz = size[0] / 2.0, size[1] / 2.0, size[2] / 2.0
        verts = [
            (-sx, -sy, -sz), (sx, -sy, -sz), (sx, sy, -sz), (-sx, sy, -sz),
            (-sx, -sy, sz), (sx, -sy, sz), (sx, sy, sz), (-sx, sy, sz)
        ]
        faces = [
            (0, 1, 2, 3), (4, 5, 6, 7), (0, 1, 5, 4),
            (2, 3, 7, 6), (0, 3, 7, 4), (1, 2, 6, 5)
        ]
        mesh.from_pydata(verts, [], faces)
        mesh.update()
        if mat:
            obj.data.materials.append(mat)
        return obj

    # 2. Populate GP_Background_Far (Depth Y = 10.0m)
    create_block("Far_Silo_Left", collections["GP_Background_Far"], (-6.0, 10.0, 4.0), (3.0, 0.2, 8.0), mat_dark)
    create_block("Far_Silo_Right", collections["GP_Background_Far"], (6.0, 10.0, 4.0), (3.0, 0.2, 8.0), mat_dark)
    create_block("Far_Duct_Center", collections["GP_Background_Far"], (0.0, 10.0, 6.5), (14.0, 0.2, 1.5), mat_charcoal)

    # 3. Populate GP_Background (Depth Y = 5.0m)
    # Heavy back walls and primary pillars (A-7, B-3) from Lab_Production.blend
    create_block("Wall_Panel_Back", collections["GP_Background"], (0.0, 5.0, 4.0), (16.0, 0.2, 8.0), mat_bluegrey)
    create_block("Pillar_B3_Left", collections["GP_Background"], (-6.0, 4.8, 4.0), (1.5, 0.4, 8.0), mat_charcoal)
    create_block("Pillar_A7_Right", collections["GP_Background"], (6.0, 4.8, 4.0), (1.5, 0.4, 8.0), mat_charcoal)
    create_block("Pillar_Slender", collections["GP_Background"], (-3.0, 4.8, 4.0), (0.7, 0.4, 8.0), mat_charcoal)
    create_block("Overhead_CableTray", collections["GP_Background"], (0.0, 4.5, 7.2), (15.0, 0.4, 0.5), mat_metal)

    # 4. Populate GP_Midground (Depth Y = 2.0m)
    # Secondary containment pods (01, 02, 03, 04), server racks, power units
    create_block("Containment_Pod_01", collections["GP_Midground"], (-4.5, 2.0, 2.35), (1.6, 0.6, 4.7), mat_metal)
    create_block("Containment_Pod_02", collections["GP_Midground"], (4.8, 2.0, 2.35), (1.6, 0.6, 4.7), mat_metal)
    create_block("ServerRacks_Right", collections["GP_Midground"], (7.2, 2.0, 1.2), (1.2, 0.6, 2.4), mat_charcoal)
    create_block("PowerUnit_Left", collections["GP_Midground"], (-7.2, 2.0, 1.0), (1.4, 0.6, 2.0), mat_metal)
    create_block("Midground_Conduit_Run", collections["GP_Midground"], (0.0, 2.0, 3.8), (14.0, 0.3, 0.3), mat_charcoal)

    # 5. Populate GP_Gameplay (Depth Y = 0.0m)
    # Ground floor, catwalks, stairs, consoles, HERO CONTAINMENT CHAMBER
    # Floor: Y=0 in 2D is Z=0 in Blender (height)
    create_block("Floor_Main", collections["GP_Gameplay"], (0.0, 0.0, -0.2), (16.0, 1.0, 0.4), mat_metal)
    create_block("Floor_Hazard_Stripe", collections["GP_Gameplay"], (0.0, -0.05, 0.02), (16.0, 0.1, 0.04), mat_yellow)
    
    # Raised platform (Right catwalk from Lab_Production.blend at Y=2.5m height)
    create_block("Platform_Right_Deck", collections["GP_Gameplay"], (5.5, 0.0, 2.5), (4.5, 1.0, 0.3), mat_lightmetal)
    create_block("Platform_Stairs_Incline", collections["GP_Gameplay"], (2.8, 0.0, 1.25), (2.0, 1.0, 2.5), mat_metal)
    create_block("Catwalk_Left_Deck", collections["GP_Gameplay"], (-5.5, 0.0, 2.5), (3.5, 1.0, 0.3), mat_lightmetal)

    # Consoles and Terminals
    create_block("ControlStation_Console", collections["GP_Gameplay"], (-3.8, 0.0, 0.8), (1.8, 0.6, 1.6), mat_metal)

    # HERO STASIS CHAMBER (Central focal point at X=-0.2, height Z=0 to 3.6m)
    create_block("Aeron_Chamber_Base", collections["GP_Gameplay"], (-0.2, 0.0, 0.3), (2.0, 0.8, 0.6), mat_charcoal)
    create_block("Aeron_Chamber_Core", collections["GP_Gameplay"], (-0.2, 0.0, 1.8), (1.4, 0.6, 2.4), mat_cyan_emit)
    create_block("Aeron_Chamber_Top", collections["GP_Gameplay"], (-0.2, 0.0, 3.3), (1.8, 0.8, 0.6), mat_charcoal)

    # 6. Populate GP_Foreground (Depth Y = -2.0m)
    # Descending thick foreground pipes, framing structural arch
    create_block("FG_Pipe_Left", collections["GP_Foreground"], (-7.8, -2.0, 4.0), (1.2, 0.8, 8.0), mat_charcoal)
    create_block("FG_Pipe_Right", collections["GP_Foreground"], (7.8, -2.0, 4.0), (1.2, 0.8, 8.0), mat_charcoal)
    create_block("FG_Overhead_Girder", collections["GP_Foreground"], (0.0, -2.0, 7.8), (16.0, 0.8, 1.0), mat_dark)

    # 7. Lighting Guide & Lights
    # Key cyan stasis light
    cyan_light_data = bpy.data.lights.new(name="Light_Cyan_Stasis", type='POINT')
    cyan_light_data.energy = 500
    cyan_light_data.color = (0.435, 0.890, 1.0)
    cyan_light_obj = bpy.data.objects.new("Light_Cyan_Stasis", cyan_light_data)
    cyan_light_obj.location = (-0.2, -0.5, 1.8)
    collections["GP_Lighting_Guide"].objects.link(cyan_light_obj)

    # Ambient cold blue fill
    fill_light_data = bpy.data.lights.new(name="Light_Cold_Fill", type='SUN')
    fill_light_data.energy = 0.5
    fill_light_data.color = (0.106, 0.141, 0.188)
    fill_light_obj = bpy.data.objects.new("Light_Cold_Fill", fill_light_data)
    fill_light_obj.rotation_euler = (math.radians(45), 0, 0)
    collections["GP_Lighting_Guide"].objects.link(fill_light_obj)

    # Warning amber point light
    warn_light_data = bpy.data.lights.new(name="Light_Warning_Amber", type='POINT')
    warn_light_data.energy = 80
    warn_light_data.color = (0.769, 0.314, 0.180)
    warn_light_obj = bpy.data.objects.new("Light_Warning_Amber", warn_light_data)
    warn_light_obj.location = (5.5, -0.2, 2.8)
    collections["GP_Lighting_Guide"].objects.link(warn_light_obj)

    # 8. Setup Orthographic 2D Platformer Camera
    cam_data = bpy.data.cameras.new(name="Camera_2D_Platformer")
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = 13.5  # Frames 16m wide lab room with margin
    cam_data.clip_start = 0.1
    cam_data.clip_end = 50.0

    cam_obj = bpy.data.objects.new("Camera_2D_Platformer", cam_data)
    # Looking down Y axis towards positive Y (2D side-scrolling platformer perspective)
    cam_obj.location = (0.0, -8.0, 3.2)
    cam_obj.rotation_euler = (math.radians(90), 0, 0)
    master_col.objects.link(cam_obj)
    scene.camera = cam_obj

    # Save Blend file
    blend_path = "Assets/Blender/Lab_2D/LAB_2D_MASTER.blend"
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    print(f"Saved {blend_path} successfully!")

    # Render 2D Composition Study
    render_output = "Assets/Blender/Lab_2D/Lab_2D_Composition_Study.png"
    scene.render.filepath = render_output
    bpy.ops.render.render(write_still=True)
    print(f"Rendered composition study to {render_output} successfully!")

if __name__ == "__main__":
    run()
