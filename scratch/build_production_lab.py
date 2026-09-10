import bpy
import bmesh
import math
import os
from mathutils import Vector, Euler

def clear_all():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for m in list(bpy.data.materials):
        bpy.data.materials.remove(m)
    for me in list(bpy.data.meshes):
        bpy.data.meshes.remove(me)

def get_or_create_material(name, color=(0.15, 0.18, 0.22, 1.0), roughness=0.8, metallic=0.2, emission=(0,0,0,1), emission_strength=0.0):
    mat = bpy.data.materials.get(name)
    if not mat:
        mat = bpy.data.materials.new(name=name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs['Base Color'].default_value = color
            bsdf.inputs['Roughness'].default_value = roughness
            bsdf.inputs['Metallic'].default_value = metallic
            if emission_strength > 0:
                bsdf.inputs['Emission Color'].default_value = emission
                bsdf.inputs['Emission Strength'].default_value = emission_strength
    return mat

def init_palette_materials():
    return {
        'MetalDark': get_or_create_material("MAT_Lab_Metal_Dark", color=(0.106, 0.141, 0.188, 1.0), roughness=0.85, metallic=0.30),
        'MetalWorn': get_or_create_material("MAT_Lab_Metal_Worn", color=(0.145, 0.176, 0.212, 1.0), roughness=0.88, metallic=0.20),
        'Floor':     get_or_create_material("MAT_Lab_Floor",      color=(0.039, 0.047, 0.063, 1.0), roughness=0.82, metallic=0.15),
        'Grating':   get_or_create_material("MAT_Lab_Grating",    color=(0.106, 0.141, 0.188, 1.0), roughness=0.88, metallic=0.40),
        'Glass':     get_or_create_material("MAT_Lab_Glass",      color=(0.435, 0.890, 1.000, 0.25), roughness=0.35, metallic=0.0, emission=(0.218, 0.445, 0.500, 1.0), emission_strength=1.5),
        'CyanEmiss': get_or_create_material("MAT_Lab_CyanEmission", color=(0.435, 0.890, 1.000, 1.0), roughness=0.1, emission=(0.435, 0.890, 1.000, 1.0), emission_strength=2.5),
        'Console':   get_or_create_material("MAT_Lab_Console",    color=(0.063, 0.094, 0.125, 1.0), roughness=0.80, metallic=0.20),
        'Cable':     get_or_create_material("MAT_Lab_Cable",      color=(0.039, 0.047, 0.063, 1.0), roughness=0.92, metallic=0.0),
        'Concrete':  get_or_create_material("MAT_Lab_Concrete",   color=(0.102, 0.118, 0.141, 1.0), roughness=0.95, metallic=0.0),
        'Warning':   get_or_create_material("MAT_Lab_Warning",    color=(0.769, 0.314, 0.180, 1.0), roughness=0.90, metallic=0.0, emission=(0.769, 0.314, 0.180, 1.0), emission_strength=0.8),
    }

def create_box(name, size, offset=(0,0,0), mat=None):
    mesh = bpy.data.meshes.new(name + "_mesh")
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co.x = v.co.x * size[0] + offset[0]
        v.co.y = v.co.y * size[1] + offset[1]
        v.co.z = v.co.z * size[2] + offset[2]
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    if mat:
        obj.data.materials.append(mat)
    bpy.context.scene.collection.objects.link(obj)
    return obj

def create_cylinder(name, radius, depth, offset=(0,0,0), segments=24, mat=None):
    mesh = bpy.data.meshes.new(name + "_mesh")
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments, radius1=radius, radius2=radius, depth=depth)
    for v in bm.verts:
        v.co.x = v.co.x + offset[0]
        v.co.y = v.co.y + offset[1]
        v.co.z = v.co.z + offset[2]
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    if mat:
        obj.data.materials.append(mat)
    bpy.context.scene.collection.objects.link(obj)
    return obj

def join_parts(parts, final_name, do_uv=True):
    for p in parts:
        if p.name not in bpy.context.scene.collection.objects:
            bpy.context.scene.collection.objects.link(p)
    bpy.ops.object.select_all(action='DESELECT')
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = final_name
    if do_uv:
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.01)
        bpy.ops.object.mode_set(mode='OBJECT')
    return obj

# ═══════════════════════════════════════════════════════════════════════════
# MODULAR ASSET BUILDERS
# ═══════════════════════════════════════════════════════════════════════════

def build_floor_a(mats):
    parts = []
    # Main slab: 4m x 4m x 0.3m, top at Z = 0
    main = create_box("FloorA_Main", (4.0, 4.0, 0.3), (0, 0, -0.15), mats['Floor'])
    parts.append(main)
    # Perimeter rim bevel seam
    rim_n = create_box("FloorA_RimN", (3.96, 0.08, 0.02), (0, 1.96, 0.01), mats['MetalDark'])
    rim_s = create_box("FloorA_RimS", (3.96, 0.08, 0.02), (0, -1.96, 0.01), mats['MetalDark'])
    rim_e = create_box("FloorA_RimE", (0.08, 3.96, 0.02), (1.96, 0, 0.01), mats['MetalDark'])
    rim_w = create_box("FloorA_RimW", (0.08, 3.96, 0.02), (-1.96, 0, 0.01), mats['MetalDark'])
    parts.extend([rim_n, rim_s, rim_e, rim_w])
    # Seam divider cross
    cross_x = create_box("FloorA_CrossX", (3.92, 0.04, 0.01), (0, 0, 0.005), mats['MetalDark'])
    cross_y = create_box("FloorA_CrossY", (0.04, 3.92, 0.01), (0, 0, 0.005), mats['MetalDark'])
    parts.extend([cross_x, cross_y])
    return join_parts(parts, "Lab_Floor_A")

def build_floor_grate(mats):
    parts = []
    # Frame: 4m x 1m x 0.3m, top at Z = 0
    frame_l = create_box("Grate_FrameL", (0.1, 4.0, 0.3), (-0.45, 0, -0.15), mats['MetalDark'])
    frame_r = create_box("Grate_FrameR", (0.1, 4.0, 0.3), (0.45, 0, -0.15), mats['MetalDark'])
    trough  = create_box("Grate_Trough", (0.8, 4.0, 0.1), (0, 0, -0.25), mats['Floor'])
    mesh_surf = create_box("Grate_Mesh", (0.8, 3.96, 0.02), (0, 0, -0.01), mats['Grating'])
    parts.extend([frame_l, frame_r, trough, mesh_surf])
    # 8 cross bars
    for i in range(8):
        y = -1.75 + i * 0.5
        bar = create_box(f"Grate_Bar_{i}", (0.8, 0.04, 0.03), (0, y, 0.0), mats['MetalDark'])
        parts.append(bar)
    return join_parts(parts, "Lab_Floor_Grate")

def build_wall_a(mats):
    # 4m wide x 8m tall x 0.4m deep
    parts = []
    panel = create_box("WallA_Panel", (4.0, 0.3, 8.0), (0, 0.15, 4.0), mats['MetalDark'])
    parts.append(panel)
    # 3 vertical structural ribs
    for i, x in enumerate([-1.5, 0.0, 1.5]):
        rib = create_box(f"WallA_Rib_{i}", (0.2, 0.25, 8.0), (x, -0.12, 4.0), mats['MetalWorn'])
        cap_t = create_box(f"WallA_RibCapT_{i}", (0.35, 0.35, 0.4), (x, -0.12, 7.8), mats['MetalDark'])
        cap_b = create_box(f"WallA_RibCapB_{i}", (0.35, 0.35, 0.4), (x, -0.12, 0.2), mats['MetalDark'])
        parts.extend([rib, cap_t, cap_b])
    # Lower cable access chase
    chase = create_box("WallA_Chase", (4.0, 0.15, 0.6), (0, -0.05, 0.5), mats['MetalWorn'])
    parts.append(chase)
    return join_parts(parts, "Lab_Wall_A")

def build_wall_b(mats):
    # 4m x 8m with ventilation louvers and conduits
    parts = []
    panel = create_box("WallB_Panel", (4.0, 0.3, 8.0), (0, 0.15, 4.0), mats['MetalDark'])
    parts.append(panel)
    # Recessed maintenance hatch
    hatch = create_box("WallB_Hatch", (1.6, 0.1, 2.2), (0, -0.02, 1.8), mats['MetalWorn'])
    frame = create_box("WallB_HatchFrame", (1.8, 0.15, 2.4), (0, 0.02, 1.8), mats['MetalDark'])
    handle = create_box("WallB_Handle", (0.1, 0.08, 0.4), (0.6, -0.1, 1.8), mats['MetalDark'])
    parts.extend([frame, hatch, handle])
    # Ventilation louver box at top
    vent_box = create_box("WallB_VentBox", (2.8, 0.2, 1.4), (0, -0.05, 6.2), mats['MetalDark'])
    vent_mesh = create_box("WallB_VentMesh", (2.6, 0.05, 1.2), (0, -0.16, 6.2), mats['Grating'])
    parts.extend([vent_box, vent_mesh])
    # Conduit pipe
    conduit = create_cylinder("WallB_Conduit", 0.06, 7.6, (-1.6, -0.1, 4.0), segments=12, mat=mats['Cable'])
    parts.append(conduit)
    return join_parts(parts, "Lab_Wall_B")

def build_wall_damaged(mats):
    # 4m x 8m panel with impact stress and exposed wiring
    parts = []
    panel = create_box("WallD_Panel", (4.0, 0.3, 8.0), (0, 0.15, 4.0), mats['MetalWorn'])
    parts.append(panel)
    # 2 standard ribs, 1 buckled rib
    rib1 = create_box("WallD_Rib1", (0.2, 0.25, 8.0), (-1.5, -0.12, 4.0), mats['MetalWorn'])
    rib2 = create_box("WallD_Rib2", (0.2, 0.25, 8.0), (1.5, -0.12, 4.0), mats['MetalWorn'])
    parts.extend([rib1, rib2])
    # Buckled center rib
    rib_b_low = create_box("WallD_RibBLow", (0.2, 0.25, 3.2), (0, -0.12, 1.6), mats['MetalWorn'])
    rib_b_top = create_box("WallD_RibBTop", (0.2, 0.25, 3.2), (0, -0.12, 6.4), mats['MetalWorn'])
    rib_bend  = create_box("WallD_RibBend",  (0.24, 0.35, 1.6), (0.1, -0.22, 4.0), mats['MetalDark'])
    rib_bend.rotation_euler = (0, 0.08, 0)
    parts.extend([rib_b_low, rib_b_top, rib_bend])
    # Exposed conduit loop
    cable_exposed = create_box("WallD_ExposedCable", (0.08, 0.15, 1.4), (0.2, -0.25, 4.0), mats['Cable'])
    parts.append(cable_exposed)
    return join_parts(parts, "Lab_Wall_Damaged")

def build_pillar_heavy(mats):
    # Heavy octagonal structural column (B-3 / A-7 designation)
    parts = []
    # Base plinth
    base = create_cylinder("PillarH_Base", 0.75, 0.6, (0, 0, 0.3), segments=8, mat=mats['Concrete'])
    parts.append(base)
    # Main column shaft: 8m tall
    shaft = create_cylinder("PillarH_Shaft", 0.55, 7.4, (0, 0, 4.3), segments=8, mat=mats['MetalDark'])
    parts.append(shaft)
    # 3 reinforcing collar rings
    for i, z in enumerate([2.0, 4.0, 6.0]):
        ring = create_cylinder(f"PillarH_Ring_{i}", 0.68, 0.35, (0, 0, z), segments=8, mat=mats['MetalWorn'])
        parts.append(ring)
    # Stenciled ID flange bracket (B-3 / A-7)
    flange = create_box("PillarH_IDFlange", (0.45, 0.1, 0.7), (0, -0.58, 2.0), mats['MetalDark'])
    flange_plate = create_box("PillarH_IDPlate", (0.35, 0.04, 0.5), (0, -0.64, 2.0), mats['MetalWorn'])
    cyan_strip = create_box("PillarH_CyanStrip", (0.35, 0.03, 0.06), (0, -0.65, 2.3), mats['CyanEmiss'])
    parts.extend([flange, flange_plate, cyan_strip])
    # Top ceiling cap bracket
    cap = create_cylinder("PillarH_Cap", 0.75, 0.6, (0, 0, 7.7), segments=8, mat=mats['MetalDark'])
    parts.append(cap)
    # Vertical conduit pipe along back
    pipe = create_cylinder("PillarH_Pipe", 0.08, 7.6, (0, 0.55, 4.1), segments=12, mat=mats['Cable'])
    parts.append(pipe)
    return join_parts(parts, "Lab_Pillar_Heavy")

def build_pillar_slender(mats):
    parts = []
    base = create_box("PillarS_Base", (0.7, 0.7, 0.4), (0, 0, 0.2), mats['Concrete'])
    shaft = create_box("PillarS_Shaft", (0.5, 0.5, 7.6), (0, 0, 4.2), mats['MetalWorn'])
    cap = create_box("PillarS_Cap", (0.65, 0.65, 0.4), (0, 0, 7.8), mats['MetalDark'])
    parts.extend([base, shaft, cap])
    for i, z in enumerate([2.5, 5.0]):
        ring = create_box(f"PillarS_Ring_{i}", (0.58, 0.58, 0.25), (0, 0, z), mats['MetalDark'])
        parts.append(ring)
    return join_parts(parts, "Lab_Pillar_Slender")

def build_platform_deck(mats):
    parts = []
    # 4m wide x 3m deep deck slab at Z = 0 (parented in Unity at Y = 2.5m)
    deck = create_box("Plat_Deck", (4.0, 3.0, 0.25), (0, 0, -0.125), mats['Grating'])
    parts.append(deck)
    # Outer steel frame
    frame_front = create_box("Plat_FrameFront", (4.0, 0.15, 0.35), (0, -1.45, -0.15), mats['MetalDark'])
    hazard_trim = create_box("Plat_HazardTrim", (3.96, 0.05, 0.15), (0, -1.53, -0.12), mats['Warning'])
    frame_back  = create_box("Plat_FrameBack",  (4.0, 0.15, 0.35), (0,  1.45, -0.15), mats['MetalDark'])
    frame_left  = create_box("Plat_FrameLeft",  (0.15, 3.0, 0.35), (-1.95, 0, -0.15), mats['MetalDark'])
    frame_right = create_box("Plat_FrameRight", (0.15, 3.0, 0.35), ( 1.95, 0, -0.15), mats['MetalDark'])
    parts.extend([frame_front, hazard_trim, frame_back, frame_left, frame_right])
    # Under-truss cross beams
    truss_1 = create_box("Plat_Truss1", (3.8, 0.1, 0.2), (0, -0.5, -0.22), mats['MetalWorn'])
    truss_2 = create_box("Plat_Truss2", (3.8, 0.1, 0.2), (0,  0.5, -0.22), mats['MetalWorn'])
    parts.extend([truss_1, truss_2])
    # 4 heavy support legs (2.5m down)
    for i, x in enumerate([-1.8, 1.8]):
        for j, y in enumerate([-1.3, 1.3]):
            leg = create_box(f"Plat_Leg_{i}_{j}", (0.2, 0.2, 2.5), (x, y, -1.5), mats['MetalDark'])
            foot = create_box(f"Plat_Foot_{i}_{j}", (0.35, 0.35, 0.15), (x, y, -2.65), mats['Concrete'])
            parts.extend([leg, foot])
    return join_parts(parts, "Lab_Platform_Deck")

def build_staircase(mats):
    # 5-step industrial staircase: width 2.4m, total height 2.5m, run 3.0m
    parts = []
    num_steps = 5
    step_h = 2.5 / num_steps
    step_d = 3.0 / num_steps
    step_w = 2.4

    for i in range(num_steps):
        # Step block
        z = step_h * (i + 0.5)
        y = step_d * (i + 0.5) - 1.5
        tread = create_box(f"Stair_Tread_{i}", (step_w, step_d, step_h * 0.5), (0, y, z), mats['MetalDark'])
        anti_slip = create_box(f"Stair_Grate_{i}", (step_w - 0.2, step_d - 0.08, 0.02), (0, y, z + step_h * 0.25 + 0.01), mats['Grating'])
        parts.extend([tread, anti_slip])

    # Left & Right heavy diagonal stringers
    str_l = create_box("Stair_StringerL", (0.15, 3.6, 0.4), (-step_w * 0.5 - 0.075, 0, 1.25), mats['MetalDark'])
    str_l.rotation_euler = (-math.atan2(2.5, 3.0), 0, 0)
    str_r = create_box("Stair_StringerR", (0.15, 3.6, 0.4), ( step_w * 0.5 + 0.075, 0, 1.25), mats['MetalDark'])
    str_r.rotation_euler = (-math.atan2(2.5, 3.0), 0, 0)
    parts.extend([str_l, str_r])

    return join_parts(parts, "Lab_Staircase_Industrial")

def build_railing(mats):
    # 2.0m modular steel railing
    parts = []
    top_bar = create_box("Rail_Top", (2.0, 0.08, 0.06), (0, 0, 1.0), mats['MetalDark'])
    mid_bar = create_box("Rail_Mid", (2.0, 0.05, 0.04), (0, 0, 0.5), mats['MetalDark'])
    kick_plate = create_box("Rail_Kick", (2.0, 0.04, 0.12), (0, 0, 0.06), mats['MetalWorn'])
    parts.extend([top_bar, mid_bar, kick_plate])
    for i, x in enumerate([-0.95, 0.0, 0.95]):
        post = create_box(f"Rail_Post_{i}", (0.07, 0.07, 1.0), (x, 0, 0.5), mats['MetalDark'])
        mount = create_box(f"Rail_Mount_{i}", (0.14, 0.14, 0.04), (x, 0, 0.02), mats['MetalDark'])
        parts.extend([post, mount])
    return join_parts(parts, "Lab_Railing_Segment")

def build_aeron_chamber(mats):
    # PRIMARY AERON STASIS CHAMBER (Real 3D multi-part mesh)
    # Diameter: 2.8m, Total Height: 4.5m
    parts = []
    radius = 1.4
    height = 3.8

    # 1. Octagonal concrete base plinth
    base = create_cylinder("AeronChamber_Plinth", 1.8, 0.45, (0, 0, 0.225), segments=8, mat=mats['Concrete'])
    parts.append(base)

    # 2. Lower mechanical housing collar (dark metal, conduit sockets)
    lower_ring = create_cylinder("AeronChamber_LowerCollar", 1.55, 0.65, (0, 0, 0.725), segments=24, mat=mats['MetalDark'])
    lower_trim = create_cylinder("AeronChamber_LowerTrim", 1.62, 0.15, (0, 0, 1.05), segments=24, mat=mats['MetalWorn'])
    parts.extend([lower_ring, lower_trim])

    # 3. Status indicator pods on lower ring (3 cyan lights)
    for i in range(3):
        angle = math.radians(i * 120.0 + 30.0)
        lx = math.sin(angle) * 1.58
        ly = math.cos(angle) * 1.58
        pod = create_box(f"AeronChamber_StatusPod_{i}", (0.16, 0.12, 0.12), (lx, ly, 0.75), mats['MetalDark'])
        lens = create_box(f"AeronChamber_StatusLens_{i}", (0.10, 0.04, 0.06), (lx, ly, 0.75), mats['CyanEmiss'])
        pod.rotation_euler = (0, 0, -angle)
        lens.rotation_euler = (0, 0, -angle)
        parts.extend([pod, lens])

    # 4. Transparent stasis glass (double-walled cylinder for authentic depth)
    glass_outer = create_cylinder("AeronChamber_GlassOuter", radius, height, (0, 0, 1.05 + height * 0.5), segments=32, mat=mats['Glass'])
    glass_inner = create_cylinder("AeronChamber_GlassInner", radius - 0.05, height - 0.1, (0, 0, 1.05 + height * 0.5), segments=28, mat=mats['Glass'])
    parts.extend([glass_outer, glass_inner])

    # 5. Upper mechanical collar ring with 8 radial hydraulic lock bolts
    upper_ring = create_cylinder("AeronChamber_UpperRing", 1.55, 0.50, (0, 0, 1.05 + height + 0.25), segments=24, mat=mats['MetalDark'])
    upper_trim = create_cylinder("AeronChamber_UpperTrim", 1.62, 0.12, (0, 0, 1.05 + height + 0.06), segments=24, mat=mats['MetalWorn'])
    parts.extend([upper_ring, upper_trim])

    for i in range(8):
        angle = math.radians(i * 45.0)
        bx = math.sin(angle) * 1.58
        by = math.cos(angle) * 1.58
        bolt = create_box(f"AeronChamber_LockBolt_{i}", (0.14, 0.16, 0.22), (bx, by, 1.05 + height + 0.25), mats['MetalWorn'])
        bolt.rotation_euler = (0, 0, -angle)
        parts.append(bolt)

    # 6. Top hydraulic cap with central manifold
    top_cap = create_cylinder("AeronChamber_TopCap", 1.35, 0.40, (0, 0, 1.05 + height + 0.70), segments=16, mat=mats['MetalDark'])
    manifold = create_cylinder("AeronChamber_Manifold", 0.60, 0.35, (0, 0, 1.05 + height + 0.95), segments=12, mat=mats['MetalWorn'])
    parts.extend([top_cap, manifold])

    # 7. 4 heavy vertical perimeter guide pylons
    for i in range(4):
        angle = math.radians(i * 90.0 + 45.0)
        px = math.sin(angle) * (radius + 0.22)
        py = math.cos(angle) * (radius + 0.22)
        pylon = create_box(f"AeronChamber_Pylon_{i}", (0.18, 0.18, height + 1.2), (px, py, 1.05 + height * 0.5), mats['MetalDark'])
        bracket_b = create_box(f"AeronChamber_PylonB_{i}", (0.28, 0.28, 0.35), (px, py, 0.7), mats['MetalWorn'])
        bracket_t = create_box(f"AeronChamber_PylonT_{i}", (0.28, 0.28, 0.35), (px, py, 1.05 + height + 0.25), mats['MetalWorn'])
        parts.extend([pylon, bracket_b, bracket_t])

    # 8. Attached diagnostic console station
    con_desk = create_box("AeronChamber_ConsoleDesk", (0.9, 0.6, 0.9), (radius + 0.55, -0.6, 0.45), mats['MetalDark'])
    con_screen_mount = create_box("AeronChamber_ConsoleMount", (0.8, 0.15, 0.6), (radius + 0.55, -0.5, 1.2), mats['MetalDark'])
    con_screen_mount.rotation_euler = (-0.26, 0, 0)
    con_screen = create_box("AeronChamber_ConsoleScreen", (0.7, 0.04, 0.48), (radius + 0.55, -0.55, 1.2), mats['CyanEmiss'])
    con_screen.rotation_euler = (-0.26, 0, 0)
    parts.extend([con_desk, con_screen_mount, con_screen])

    # 9. Coolant and energy conduit pipes running into floor
    conduit_l = create_cylinder("AeronChamber_ConduitL", 0.10, 2.2, (-radius - 0.25, 0.8, 0.45), segments=12, mat=mats['Cable'])
    conduit_l.rotation_euler = (math.radians(90.0), 0, 0)
    conduit_r = create_cylinder("AeronChamber_ConduitR", 0.10, 2.2, (radius + 0.25, 0.8, 0.45), segments=12, mat=mats['Cable'])
    conduit_r.rotation_euler = (math.radians(90.0), 0, 0)
    parts.extend([conduit_l, conduit_r])

    return join_parts(parts, "Lab_Aeron_Chamber")

def build_secondary_containment(name, status, mats):
    # Secondary background containment cylinder (dia 1.8m, height 3.6m)
    parts = []
    r = 0.9
    h = 3.2
    base = create_cylinder(f"{name}_Base", r + 0.25, 0.35, (0, 0, 0.175), segments=8, mat=mats['Concrete'])
    collar_b = create_cylinder(f"{name}_CollarB", r + 0.12, 0.45, (0, 0, 0.575), segments=20, mat=mats['MetalDark'])
    parts.extend([base, collar_b])

    glass_mat = mats['Glass'] if status == 'active' else mats['Console']
    glass = create_cylinder(f"{name}_Glass", r, h, (0, 0, 0.8 + h * 0.5), segments=24, mat=glass_mat)
    parts.append(glass)

    collar_t = create_cylinder(f"{name}_CollarT", r + 0.12, 0.40, (0, 0, 0.8 + h + 0.2), segments=20, mat=mats['MetalDark'])
    cap = create_cylinder(f"{name}_Cap", r - 0.1, 0.30, (0, 0, 0.8 + h + 0.55), segments=16, mat=mats['MetalWorn'])
    parts.extend([collar_t, cap])

    # 3 perimeter struts
    for i in range(3):
        ang = math.radians(i * 120.0)
        sx = math.sin(ang) * (r + 0.14)
        sy = math.cos(ang) * (r + 0.14)
        strut = create_box(f"{name}_Strut_{i}", (0.12, 0.12, h + 0.8), (sx, sy, 0.8 + h * 0.5), mats['MetalWorn'])
        parts.append(strut)

    # Status light
    stat_mat = mats['CyanEmiss'] if status == 'active' else (mats['Warning'] if status == 'malfunction' else mats['MetalDark'])
    stat_light = create_box(f"{name}_StatusLight", (0.08, 0.08, 0.08), (0, -r - 0.15, 0.6), stat_mat)
    parts.append(stat_light)

    return join_parts(parts, name)

def build_security_blast_door(mats):
    # 3m wide x 4m tall heavy blast door with segmented vertical slabs
    parts = []
    # Heavy outer door frame: 3.8m wide, 4.4m tall, 0.5m deep
    frame_l = create_box("SecDoor_JambL", (0.4, 0.5, 4.4), (-1.7, 0, 2.2), mats['MetalDark'])
    frame_r = create_box("SecDoor_JambR", (0.4, 0.5, 4.4), ( 1.7, 0, 2.2), mats['MetalDark'])
    frame_t = create_box("SecDoor_Header", (3.8, 0.5, 0.4), (0, 0, 4.2), mats['MetalDark'])
    parts.extend([frame_l, frame_r, frame_t])

    # Split door leaves (Left & Right halves)
    slab_l = create_box("SecDoor_SlabL", (1.48, 0.25, 4.0), (-0.74, 0, 2.0), mats['MetalWorn'])
    slab_r = create_box("SecDoor_SlabR", (1.48, 0.25, 4.0), ( 0.74, 0, 2.0), mats['MetalWorn'])
    parts.extend([slab_l, slab_r])

    # Horizontal reinforcing ribs
    for i, z in enumerate([0.8, 2.0, 3.2]):
        rib = create_box(f"SecDoor_Rib_{i}", (2.9, 0.08, 0.18), (0, -0.15, z), mats['MetalDark'])
        parts.append(rib)

    # Central heavy hydraulic locking box
    lock_box = create_box("SecDoor_LockBox", (0.5, 0.16, 0.7), (0, -0.18, 2.0), mats['MetalDark'])
    lock_pin = create_cylinder("SecDoor_LockPin", 0.08, 0.8, (0, -0.22, 2.0), segments=12, mat=mats['Warning'])
    parts.extend([lock_box, lock_pin])

    # Warning chevrons at door base
    hazard_base = create_box("SecDoor_HazardBase", (2.9, 0.05, 0.25), (0, -0.14, 0.2), mats['Warning'])
    parts.append(hazard_base)

    # Status beacon atop header
    beacon_mount = create_box("SecDoor_BeaconMount", (0.3, 0.15, 0.15), (0, -0.28, 4.3), mats['MetalDark'])
    beacon_light = create_box("SecDoor_BeaconLight", (0.18, 0.08, 0.10), (0, -0.32, 4.3), mats['CyanEmiss'])
    parts.extend([beacon_mount, beacon_light])

    return join_parts(parts, "Lab_Door_Security")

def build_maintenance_door(mats):
    parts = []
    # 1.8m wide x 3.2m tall recessed door
    frame_l = create_box("MaintDoor_JambL", (0.25, 0.35, 3.2), (-0.85, 0, 1.6), mats['MetalDark'])
    frame_r = create_box("MaintDoor_JambR", (0.25, 0.35, 3.2), ( 0.85, 0, 1.6), mats['MetalDark'])
    frame_t = create_box("MaintDoor_Header", (1.95, 0.35, 0.25), (0, 0, 3.1), mats['MetalDark'])
    parts.extend([frame_l, frame_r, frame_t])

    panel = create_box("MaintDoor_Panel", (1.45, 0.15, 2.95), (0, 0.02, 1.5), mats['MetalWorn'])
    wheel_hub = create_cylinder("MaintDoor_WheelHub", 0.18, 0.12, (0.4, -0.10, 1.5), segments=16, mat=mats['MetalDark'])
    wheel_rim = create_cylinder("MaintDoor_WheelRim", 0.25, 0.04, (0.4, -0.16, 1.5), segments=16, mat=mats['Warning'])
    warning_strip = create_box("MaintDoor_WarnStrip", (1.4, 0.04, 0.18), (0, -0.06, 0.3), mats['Warning'])
    parts.extend([panel, wheel_hub, wheel_rim, warning_strip])

    return join_parts(parts, "Lab_Door_Maintenance")

def build_console_station(mats):
    parts = []
    # Dual terminal workstation: 1.8m wide, 1.0m deep, 1.6m tall
    desk = create_box("Console_Desk", (1.8, 0.9, 0.12), (0, 0, 0.85), mats['MetalDark'])
    ped_l = create_box("Console_PedL", (0.35, 0.8, 0.85), (-0.75, 0, 0.425), mats['MetalDark'])
    ped_r = create_box("Console_PedR", (0.35, 0.8, 0.85), ( 0.75, 0, 0.425), mats['MetalDark'])
    parts.extend([desk, ped_l, ped_r])

    # Slanted monitor bay
    back_chassis = create_box("Console_MonitorBay", (1.6, 0.2, 0.75), (0, 0.25, 1.35), mats['MetalDark'])
    back_chassis.rotation_euler = (-0.26, 0, 0)
    screen_l = create_box("Console_ScreenL", (0.68, 0.04, 0.55), (-0.4, 0.18, 1.38), mats['Console'])
    screen_l.rotation_euler = (-0.26, 0, 0)
    screen_r = create_box("Console_ScreenR", (0.68, 0.04, 0.55), ( 0.4, 0.18, 1.38), mats['CyanEmiss'])
    screen_r.rotation_euler = (-0.26, 0, 0)
    parts.extend([back_chassis, screen_l, screen_r])

    # Keyboard deck
    kb = create_box("Console_Keyboard", (1.2, 0.32, 0.04), (0, -0.15, 0.93), mats['MetalWorn'])
    parts.append(kb)
    return join_parts(parts, "Lab_Console_A")

def build_server_rack(mats):
    parts = []
    # 0.9m wide x 0.8m deep x 2.4m tall
    chassis = create_box("Server_Chassis", (0.9, 0.8, 2.4), (0, 0, 1.2), mats['MetalDark'])
    door_frame = create_box("Server_DoorFrame", (0.84, 0.06, 2.25), (0, -0.42, 1.2), mats['MetalWorn'])
    parts.extend([chassis, door_frame])
    # 8 server blade slots
    for i in range(8):
        z = 0.35 + i * 0.24
        slot = create_box(f"Server_Blade_{i}", (0.72, 0.04, 0.18), (0, -0.44, z), mats['MetalDark'])
        led_color = mats['CyanEmiss'] if i % 2 == 0 else mats['Warning']
        led = create_box(f"Server_LED_{i}", (0.04, 0.02, 0.04), (0.3, -0.47, z), led_color)
        parts.extend([slot, led])
    return join_parts(parts, "Lab_Server_Rack")

def build_power_unit(mats):
    parts = []
    # Heavy transformer cabinet: 1.4m wide x 0.9m deep x 1.8m tall
    body = create_box("Power_Body", (1.4, 0.9, 1.8), (0, 0, 0.9), mats['MetalDark'])
    top_vent = create_box("Power_TopVent", (1.2, 0.7, 0.2), (0, 0, 1.9), mats['Grating'])
    meter_panel = create_box("Power_MeterPanel", (0.8, 0.06, 0.5), (0, -0.48, 1.3), mats['MetalWorn'])
    meter_dial = create_box("Power_MeterDial", (0.35, 0.03, 0.25), (0, -0.52, 1.3), mats['CyanEmiss'])
    parts.extend([body, top_vent, meter_panel, meter_dial])
    return join_parts(parts, "Lab_Power_Unit")

def build_industrial_crate(mats):
    parts = []
    # 1.0m x 0.8m x 0.8m reinforced crate
    body = create_box("Crate_Body", (1.0, 0.8, 0.8), (0, 0, 0.4), mats['MetalWorn'])
    lid = create_box("Crate_Lid", (1.04, 0.84, 0.12), (0, 0, 0.82), mats['MetalDark'])
    parts.extend([body, lid])
    # Corner protective bumpers
    for x in [-0.48, 0.48]:
        for y in [-0.38, 0.38]:
            bumper = create_box("Crate_Bumper", (0.12, 0.12, 0.78), (x, y, 0.4), mats['MetalDark'])
            parts.append(bumper)
    return join_parts(parts, "Lab_Industrial_Crate")

def build_pipe_network(mats):
    parts = []
    # Overhead pipe runs with elbows and flanges
    pipe_main = create_cylinder("Pipe_Main", 0.16, 12.0, (0, 0, 0), segments=16, mat=mats['MetalDark'])
    pipe_main.rotation_euler = (math.radians(90.0), 0, 0)
    parts.append(pipe_main)
    for y in [-4.0, -1.0, 2.0, 5.0]:
        flange = create_cylinder("Pipe_Flange", 0.24, 0.08, (0, y, 0), segments=16, mat=mats['MetalWorn'])
        flange.rotation_euler = (math.radians(90.0), 0, 0)
        parts.append(flange)
    return join_parts(parts, "Lab_Pipe_Network")

def build_cable_tray(mats):
    parts = []
    # 4m cable tray segment
    mesh_tray = create_box("CableTray_Mesh", (0.5, 4.0, 0.08), (0, 0, 0), mats['Grating'])
    rail_l = create_box("CableTray_RailL", (0.05, 4.0, 0.18), (-0.25, 0, 0.06), mats['MetalDark'])
    rail_r = create_box("CableTray_RailR", (0.05, 4.0, 0.18), ( 0.25, 0, 0.06), mats['MetalDark'])
    cables = create_cylinder("CableTray_Bundles", 0.14, 3.9, (0, 0, 0.08), segments=10, mat=mats['Cable'])
    cables.rotation_euler = (math.radians(90.0), 0, 0)
    parts.extend([mesh_tray, rail_l, rail_r, cables])
    return join_parts(parts, "Lab_CableTray_A")

def build_warning_light(mats):
    parts = []
    bracket = create_box("WarnLight_Bracket", (0.16, 0.12, 0.16), (0, 0, 0.08), mats['MetalDark'])
    dome = create_cylinder("WarnLight_Dome", 0.10, 0.18, (0, 0, 0.25), segments=16, mat=mats['Warning'])
    parts.extend([bracket, dome])
    return join_parts(parts, "Lab_Warning_Light")

def export_single_object(obj, filepath):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=filepath,
        check_existing=False,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_space_transform=True,
        bake_anim=False
    )
    print(f"[Export] Saved: {filepath}")

def main():
    clear_all()
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1.0

    mats = init_palette_materials()
    modules_dir = os.path.abspath("Assets/Environment/Lab/Modules")
    os.makedirs(modules_dir, exist_ok=True)

    print("=== 1. BUILDING & EXPORTING INDIVIDUAL MODULES ===")
    builders = [
        ("Lab_Floor_A", build_floor_a),
        ("Lab_Floor_Grate", build_floor_grate),
        ("Lab_Wall_A", build_wall_a),
        ("Lab_Wall_B", build_wall_b),
        ("Lab_Wall_Damaged", build_wall_damaged),
        ("Lab_Pillar_A", build_pillar_heavy),
        ("Lab_Pillar_B", build_pillar_slender),
        ("Lab_Platform_A", build_platform_deck),
        ("Lab_Stairs_A", build_staircase),
        ("Lab_Railing_A", build_railing),
        ("Lab_Containment_A", build_aeron_chamber),
        ("Lab_Containment_B", lambda m: build_secondary_containment("Lab_Containment_B", "active", m)),
        ("Lab_Door_Security", build_security_blast_door),
        ("Lab_Door_Maintenance", build_maintenance_door),
        ("Lab_Console_A", build_console_station),
        ("Lab_Server_Rack", build_server_rack),
        ("Lab_Power_Unit", build_power_unit),
        ("Lab_Industrial_Crate", build_industrial_crate),
        ("Lab_Pipe_Network", build_pipe_network),
        ("Lab_CableTray_A", build_cable_tray),
        ("Lab_Warning_Light", build_warning_light),
    ]

    for name, func in builders:
        obj = func(mats)
        fbx_path = os.path.join(modules_dir, f"{name}.fbx")
        export_single_object(obj, fbx_path)
        # Delete after export to keep clean
        bpy.data.objects.remove(obj, do_unlink=True)

    print("=== 2. ASSEMBLING COMPLETE LABORATORY ENVIRONMENT IN BLENDER ===")
    # ── Floors: 4x3 grid of FloorA (16m x 12m) with central Grate strip ──
    assembled_objects = []
    for ix, x in enumerate([-6.0, -2.0, 2.0, 6.0]):
        for iy, y in enumerate([-2.0, 2.0, 6.0]):
            fl = build_floor_a(mats)
            fl.name = f"Floor_Plate_{ix}_{iy}"
            fl.location = Vector((x, y, 0))
            assembled_objects.append(fl)

    # Central drainage grate along Y = -4 to +8
    for i, y in enumerate([-2.0, 2.0, 6.0]):
        gr = build_floor_grate(mats)
        gr.name = f"Floor_Grate_{i}"
        gr.location = Vector((0, y, 0))
        assembled_objects.append(gr)

    # ── Walls ──
    # Back Wall (4 panels along Y = 8.0)
    w_types = [build_wall_a, build_wall_damaged, build_wall_b, build_wall_a]
    for i, x in enumerate([-6.0, -2.0, 2.0, 6.0]):
        w = w_types[i](mats)
        w.name = f"Wall_Back_{i}"
        w.location = Vector((x, 8.0, 0))
        assembled_objects.append(w)

    # Left Wall (3 panels along X = -8.0)
    for i, y in enumerate([-2.0, 2.0, 6.0]):
        w = build_wall_a(mats) if i != 1 else build_wall_b(mats)
        w.name = f"Wall_Left_{i}"
        w.location = Vector((-8.0, y, 0))
        w.rotation_euler = (0, 0, math.radians(-90.0))
        assembled_objects.append(w)

    # Right Wall (3 panels along X = 8.0)
    for i, y in enumerate([-2.0, 2.0, 6.0]):
        w = build_wall_b(mats) if i != 0 else build_wall_a(mats)
        w.name = f"Wall_Right_{i}"
        w.location = Vector((8.0, y, 0))
        w.rotation_euler = (0, 0, math.radians(90.0))
        assembled_objects.append(w)

    # ── Pillars ──
    p_b3 = build_pillar_heavy(mats)
    p_b3.name = "Pillar_B3_Left"
    p_b3.location = Vector((-6.0, -1.0, 0))
    assembled_objects.append(p_b3)

    p_rf = build_pillar_heavy(mats)
    p_rf.name = "Pillar_Right_Front"
    p_rf.location = Vector((6.0, -1.0, 0))
    assembled_objects.append(p_rf)

    p_a7 = build_pillar_heavy(mats)
    p_a7.name = "Pillar_A7_Left"
    p_a7.location = Vector((-6.0, 5.0, 0))
    assembled_objects.append(p_a7)

    p_rb = build_pillar_heavy(mats)
    p_rb.name = "Pillar_Right_Back"
    p_rb.location = Vector((6.0, 5.0, 0))
    assembled_objects.append(p_rb)

    # Secondary slender pillars
    p_s1 = build_pillar_slender(mats)
    p_s1.name = "Pillar_Slender_Left"
    p_s1.location = Vector((-3.0, 7.6, 0))
    assembled_objects.append(p_s1)

    # ── Platform & Catwalks ──
    plat_main = build_platform_deck(mats)
    plat_main.name = "MainPlatform_Deck"
    plat_main.location = Vector((5.5, 3.5, 2.5))
    assembled_objects.append(plat_main)

    catwalk_left = build_platform_deck(mats)
    catwalk_left.name = "Catwalk_Left_Deck"
    catwalk_left.location = Vector((-5.5, 4.0, 2.5))
    assembled_objects.append(catwalk_left)

    stairs = build_staircase(mats)
    stairs.name = "Platform_Stairs"
    stairs.location = Vector((3.2, 2.2, 0))
    assembled_objects.append(stairs)

    # Railings
    rail1 = build_railing(mats)
    rail1.name = "Rail_Main_Front"
    rail1.location = Vector((5.5, 1.95, 2.5))
    assembled_objects.append(rail1)

    rail2 = build_railing(mats)
    rail2.name = "Rail_Left_Front"
    rail2.location = Vector((-5.5, 2.45, 2.5))
    assembled_objects.append(rail2)

    # ── Containment Chambers ──
    # Primary Aeron Chamber: center-midground behind Aeron
    aeron_ch = build_aeron_chamber(mats)
    aeron_ch.name = "Aeron_Chamber"
    aeron_ch.location = Vector((-1.8, 3.6, 0))
    assembled_objects.append(aeron_ch)

    # Secondary Containment Pods
    c1 = build_secondary_containment("Containment_01", "active", mats)
    c1.location = Vector((-4.5, 6.5, 0))
    c2 = build_secondary_containment("Containment_02", "inactive", mats)
    c2.location = Vector((4.8, 6.8, 0))
    c3 = build_secondary_containment("Containment_03", "malfunction", mats)
    c3.location = Vector((-6.8, 4.8, 0))
    c4 = build_secondary_containment("Containment_04", "inactive", mats)
    c4.location = Vector((6.8, 5.5, 0))
    assembled_objects.extend([c1, c2, c3, c4])

    # ── Doors ──
    door_sec = build_security_blast_door(mats)
    door_sec.name = "SecurityDoor"
    door_sec.location = Vector((5.5, 7.8, 0))
    assembled_objects.append(door_sec)

    door_maint = build_maintenance_door(mats)
    door_maint.name = "MaintenanceDoor"
    door_maint.location = Vector((-7.8, 2.0, 0))
    door_maint.rotation_euler = (0, 0, math.radians(90.0))
    assembled_objects.append(door_maint)

    # ── Machinery & Props ──
    for i in range(3):
        rack = build_server_rack(mats)
        rack.name = f"ServerRack_{i}"
        rack.location = Vector((7.4, -1.0 + i * 1.5, 0))
        rack.rotation_euler = (0, 0, math.radians(90.0))
        assembled_objects.append(rack)

    for i in range(2):
        pwr = build_power_unit(mats)
        pwr.name = f"PowerUnit_{i}"
        pwr.location = Vector((-7.3, 2.5 + i * 2.2, 0))
        pwr.rotation_euler = (0, 0, math.radians(-90.0))
        assembled_objects.append(pwr)

    con1 = build_console_station(mats)
    con1.name = "ControlStation_1"
    con1.location = Vector((-3.8, -1.5, 0))
    con1.rotation_euler = (0, 0, math.radians(15.0))
    assembled_objects.append(con1)

    crate1 = build_industrial_crate(mats)
    crate1.name = "Crate_01"
    crate1.location = Vector((6.8, -2.2, 0))
    crate2 = build_industrial_crate(mats)
    crate2.name = "Crate_02"
    crate2.location = Vector((6.8, -2.2, 0.82))
    assembled_objects.extend([crate1, crate2])

    pipe_net = build_pipe_network(mats)
    pipe_net.name = "Overhead_Pipes"
    pipe_net.location = Vector((0, 2.0, 7.2))
    assembled_objects.append(pipe_net)

    print(f"Assembled {len(assembled_objects)} modular elements in Blender!")

    # ── Export Assembled Environment FBX ──
    bpy.ops.object.select_all(action='SELECT')
    out_assembled = os.path.abspath("Assets/Environment/Lab/Lab_Environment_Production.fbx")
    bpy.ops.export_scene.fbx(
        filepath=out_assembled,
        check_existing=False,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_space_transform=True,
        bake_anim=False
    )
    print(f"[Export] Assembled environment FBX exported to: {out_assembled}")

    # ── 3. Add Character References & 2.5D Camera for Live Viewport Inspection ──
    # Aeron Ref (Cyan, 1.8m)
    aeron_box = create_box("Ref_Aeron_1.8m", (0.4, 0.25, 1.80), (-1.2, 0.5, 0.9), mats['CyanEmiss'])
    # Guard Ref (Hazard Orange, 1.98m)
    guard_box = create_box("Ref_Guard_1.98m", (0.5, 0.35, 1.98), (1.6, 0.5, 0.99), mats['Warning'])

    # Camera configured to match Unity 2.5D Camera (Vertical FOV = 27°)
    cam_data = bpy.data.cameras.new("Ref_2.5D_Camera")
    cam_data.lens_unit = 'FOV'
    cam_data.sensor_fit = 'VERTICAL'
    cam_data.angle = math.radians(27.0)
    cam_data.clip_start = 0.1
    cam_data.clip_end = 100.0
    cam_obj = bpy.data.objects.new("Ref_2.5D_Camera", cam_data)
    cam_obj.location = Vector((0.0, -9.5, 2.2))
    cam_obj.rotation_euler = Euler((math.radians(87.0), 0.0, 0.0), 'XYZ')
    bpy.context.collection.objects.link(cam_obj)
    scene.camera = cam_obj

    # Lighting
    light_key = bpy.data.lights.new(name="Key_Light", type='SUN')
    light_key.color = (0.435, 0.890, 1.0)
    light_key.energy = 2.0
    light_key_obj = bpy.data.objects.new(name="Key_Light", object_data=light_key)
    light_key_obj.rotation_euler = (math.radians(35.0), math.radians(-40.0), 0.0)
    bpy.context.collection.objects.link(light_key_obj)

    light_fill = bpy.data.lights.new(name="Fill_Light", type='SUN')
    light_fill.color = (0.055, 0.078, 0.125)
    light_fill.energy = 0.5
    light_fill_obj = bpy.data.objects.new(name="Fill_Light", object_data=light_fill)
    bpy.context.collection.objects.link(light_fill_obj)

    # Set viewport to camera view & material shading
    for window in bpy.context.window_manager.windows:
        for area in window.screen.areas:
            if area.type == 'VIEW_3D':
                for space in area.spaces:
                    if space.type == 'VIEW_3D':
                        space.region_3d.view_perspective = 'CAMERA'
                        space.shading.type = 'MATERIAL'
                        space.shading.use_scene_lights = True

    # Save blend file
    blend_path = os.path.abspath("Assets/Environment/Lab/Lab_Production.blend")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    print(f"[Save] Saved production blend file to: {blend_path}")

    print("=== BLENDER PRODUCTION LAB ENVIRONMENT COMPLETED SUCCESSFULLY ===")

if __name__ == "__main__":
    main()
