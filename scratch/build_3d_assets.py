import bpy
import bmesh
import math
import os
from mathutils import Vector, Euler

def reset_blender():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1.0

def create_box(name, size, loc):
    mesh = bpy.data.meshes.new(name)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co.x *= size[0]
        v.co.y *= size[1]
        v.co.z *= size[2]
        v.co.x += loc[0]
        v.co.y += loc[1]
        v.co.z += loc[2]
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return obj

def create_cylinder(name, radius, depth, loc, segments=16):
    mesh = bpy.data.meshes.new(name)
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments, radius1=radius, radius2=radius, depth=depth)
    for v in bm.verts:
        v.co.x += loc[0]
        v.co.y += loc[1]
        v.co.z += loc[2]
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return obj

def apply_modifier(obj, mod_name):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=mod_name)

def build_aeron():
    reset_blender()
    parts = []

    # Head & Hair (Z: 1.58 to 1.80)
    head = create_box("Head", (0.18, 0.20, 0.22), (0, 0, 1.69))
    parts.append(head)
    hair = create_box("Hair", (0.20, 0.22, 0.12), (0, -0.02, 1.76))
    parts.append(hair)
    eye = create_box("CyanEye", (0.04, 0.02, 0.04), (0.05, -0.10, 1.69))
    parts.append(eye)

    # Neck & Collar (Z: 1.44 to 1.58)
    neck = create_cylinder("Neck", 0.08, 0.14, (0, 0, 1.51), segments=12)
    parts.append(neck)
    collar = create_box("Collar", (0.34, 0.30, 0.08), (0, 0, 1.48))
    parts.append(collar)

    # Torso (Chest: 1.25 - 1.45, Abdomen: 1.05 - 1.25, Hips: 0.90 - 1.05)
    chest = create_box("Chest", (0.42, 0.24, 0.22), (0, 0, 1.34))
    parts.append(chest)
    abdomen = create_box("Abdomen", (0.34, 0.22, 0.20), (0, 0, 1.15))
    parts.append(abdomen)
    hips = create_box("Hips", (0.36, 0.24, 0.16), (0, 0, 0.98))
    parts.append(hips)

    # Shoulders & Arms
    for side, sign in [("_L", 1), ("_R", -1)]:
        sh = create_box(f"Shoulder{side}", (0.12, 0.14, 0.12), (sign * 0.26, 0, 1.40))
        uarm = create_cylinder(f"UpperArm{side}", 0.065, 0.28, (sign * 0.32, 0, 1.24), segments=10)
        farm = create_cylinder(f"Forearm{side}", 0.055, 0.26, (sign * 0.34, 0, 0.96), segments=10)
        hand = create_box(f"Hand{side}", (0.08, 0.10, 0.12), (sign * 0.35, 0, 0.77))
        parts.extend([sh, uarm, farm, hand])

    # Legs & Feet
    for side, sign in [("_L", 1), ("_R", -1)]:
        thigh = create_cylinder(f"Thigh{side}", 0.09, 0.42, (sign * 0.12, 0, 0.69), segments=12)
        knee = create_box(f"Knee{side}", (0.10, 0.12, 0.08), (sign * 0.12, -0.04, 0.48))
        shin = create_cylinder(f"Shin{side}", 0.075, 0.40, (sign * 0.12, 0, 0.28), segments=12)
        foot = create_box(f"Foot{side}", (0.11, 0.24, 0.10), (sign * 0.12, -0.04, 0.05))
        parts.extend([thigh, knee, shin, foot])

    # Join all parts
    bpy.ops.object.select_all(action='DESELECT')
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    aeron = bpy.context.active_object
    aeron.name = "Aeron_Mesh"

    # Subdivide & bevel
    mod_sub = aeron.modifiers.new(name="Subsurf", type='SUBSURF')
    mod_sub.levels = 1
    apply_modifier(aeron, "Subsurf")

    mod_bev = aeron.modifiers.new(name="Bevel", type='BEVEL')
    mod_bev.width = 0.012
    mod_bev.segments = 2
    apply_modifier(aeron, "Bevel")

    mod_tri = aeron.modifiers.new(name="Triangulate", type='TRIANGULATE')
    apply_modifier(aeron, "Triangulate")

    # Decimate to exact budget (~4,200 tris, target: 3,000 - 6,000)
    mod_dec = aeron.modifiers.new(name="Decimate", type='DECIMATE')
    mod_dec.ratio = 0.42
    apply_modifier(aeron, "Decimate")

    tri_count = len(aeron.data.polygons)
    print(f"[Aeron] Final poly/tri count: {tri_count} (Budget: 3,000 - 6,000)")

    # UV Unwrap
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')

    # Create Armature
    bpy.ops.object.armature_add(enter_editmode=True, align='WORLD', location=(0, 0, 0))
    arm_obj = bpy.context.active_object
    arm_obj.name = "Aeron_Armature"
    eb = arm_obj.data.edit_bones
    eb.remove(eb[0])

    bone_defs = [
        ("Hips", (0, 0, 0.90), (0, 0, 1.05), None),
        ("Spine", (0, 0, 1.05), (0, 0, 1.25), "Hips"),
        ("Chest", (0, 0, 1.25), (0, 0, 1.50), "Spine"),
        ("Neck", (0, 0, 1.50), (0, 0, 1.58), "Chest"),
        ("Head", (0, 0, 1.58), (0, 0, 1.80), "Neck"),
        # Left Arm
        ("Shoulder_L", (0.08, 0, 1.45), (0.24, 0, 1.42), "Chest"),
        ("UpperArm_L", (0.24, 0, 1.42), (0.34, 0, 1.15), "Shoulder_L"),
        ("Forearm_L", (0.34, 0, 1.15), (0.35, 0, 0.88), "UpperArm_L"),
        ("Hand_L", (0.35, 0, 0.88), (0.36, 0, 0.72), "Forearm_L"),
        # Right Arm
        ("Shoulder_R", (-0.08, 0, 1.45), (-0.24, 0, 1.42), "Chest"),
        ("UpperArm_R", (-0.24, 0, 1.42), (-0.34, 0, 1.15), "Shoulder_R"),
        ("Forearm_R", (-0.34, 0, 1.15), (-0.35, 0, 0.88), "UpperArm_R"),
        ("Hand_R", (-0.35, 0, 0.88), (-0.36, 0, 0.72), "Forearm_R"),
        # Left Leg
        ("Thigh_L", (0.12, 0, 0.90), (0.12, 0, 0.48), "Hips"),
        ("Shin_L", (0.12, 0, 0.48), (0.12, 0, 0.10), "Thigh_L"),
        ("Foot_L", (0.12, 0, 0.10), (0.12, -0.15, 0.0), "Shin_L"),
        # Right Leg
        ("Thigh_R", (-0.12, 0, 0.90), (-0.12, 0, 0.48), "Hips"),
        ("Shin_R", (-0.12, 0, 0.48), (-0.12, 0, 0.10), "Thigh_R"),
        ("Foot_R", (-0.12, 0, 0.10), (-0.12, -0.15, 0.0), "Shin_R"),
    ]

    created_bones = {}
    for name, head_pos, tail_pos, parent_name in bone_defs:
        b = eb.new(name)
        b.head = Vector(head_pos)
        b.tail = Vector(tail_pos)
        created_bones[name] = b

    for name, head_pos, tail_pos, parent_name in bone_defs:
        if parent_name:
            created_bones[name].parent = created_bones[parent_name]

    bpy.ops.object.mode_set(mode='OBJECT')

    # Skinning
    bpy.ops.object.select_all(action='DESELECT')
    aeron.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')

    # Ensure every vertex has at least one bone assignment (avoids FBX importer warning)
    hips_vg = aeron.vertex_groups.get("Hips")
    if hips_vg:
        for v in aeron.data.vertices:
            if len(v.groups) == 0:
                hips_vg.add([v.index], 1.0, 'REPLACE')


    # Animation
    scene = bpy.context.scene
    scene.frame_start = 1
    scene.frame_end = 60
    scene.render.fps = 30

    arm_obj.animation_data_create()
    action = bpy.data.actions.new(name="Aeron_Idle")
    arm_obj.animation_data.action = action

    bpy.ops.object.mode_set(mode='POSE')
    pose_bones = arm_obj.pose.bones

    chest_bone = pose_bones.get("Chest")
    spine_bone = pose_bones.get("Spine")
    sh_l = pose_bones.get("Shoulder_L")
    sh_r = pose_bones.get("Shoulder_R")

    for f in [1, 60]:
        scene.frame_set(f)
        if chest_bone:
            chest_bone.location = (0, 0, 0)
            chest_bone.rotation_euler = (0, 0, 0)
            chest_bone.keyframe_insert(data_path="location", frame=f)
            chest_bone.keyframe_insert(data_path="rotation_euler", frame=f)
        if spine_bone:
            spine_bone.rotation_euler = (0, 0, 0)
            spine_bone.keyframe_insert(data_path="rotation_euler", frame=f)
        if sh_l:
            sh_l.location = (0, 0, 0)
            sh_l.keyframe_insert(data_path="location", frame=f)
        if sh_r:
            sh_r.location = (0, 0, 0)
            sh_r.keyframe_insert(data_path="location", frame=f)

    scene.frame_set(30)
    if chest_bone:
        chest_bone.location = (0, -0.005, 0.018)
        chest_bone.rotation_euler = (0.028, 0, 0)
        chest_bone.keyframe_insert(data_path="location", frame=30)
        chest_bone.keyframe_insert(data_path="rotation_euler", frame=30)
    if spine_bone:
        spine_bone.rotation_euler = (0.012, 0, 0)
        spine_bone.keyframe_insert(data_path="rotation_euler", frame=30)
    if sh_l:
        sh_l.location = (0, 0, 0.008)
        sh_l.keyframe_insert(data_path="location", frame=30)
    if sh_r:
        sh_r.location = (0, 0, 0.008)
        sh_r.keyframe_insert(data_path="location", frame=30)

    bpy.ops.object.mode_set(mode='OBJECT')

    # Material
    mat = bpy.data.materials.new(name="M_Aeron")
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Roughness'].default_value = 0.85
        bsdf.inputs['Metallic'].default_value = 0.10
        tex_path = os.path.abspath("Assets/Characters/Aeron/Aeron_Albedo.png")
        if os.path.exists(tex_path):
            tex_img = bpy.data.images.load(tex_path)
            node_tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
            node_tex.image = tex_img
            mat.node_tree.links.new(node_tex.outputs['Color'], bsdf.inputs['Base Color'])
    aeron.data.materials.append(mat)

    # Export FBX
    bpy.ops.object.select_all(action='DESELECT')
    aeron.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj

    out_fbx = os.path.abspath("Assets/Characters/Aeron/Aeron.fbx")
    bpy.ops.export_scene.fbx(
        filepath=out_fbx,
        check_existing=False,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_anim=True,
        bake_anim_use_all_actions=False,
        bake_anim_use_nla_strips=False,
        bake_anim_step=1.0,
        add_leaf_bones=False
    )
    print(f"[Aeron] Successfully exported to {out_fbx} (Tris: {tri_count})")


def build_guard():
    reset_blender()
    parts = []

    # Guard height target: 1.98m
    # Head & Helmet (Z: 1.72 to 1.98)
    helmet = create_box("Helmet", (0.24, 0.26, 0.26), (0, 0, 1.85))
    parts.append(helmet)
    visor = create_box("Visor", (0.18, 0.05, 0.08), (0, -0.14, 1.86))
    parts.append(visor)

    # Collar & Cowl (Z: 1.60 to 1.72)
    cowl = create_box("Cowl", (0.42, 0.36, 0.12), (0, 0, 1.66))
    parts.append(cowl)

    # Heavy Chest & Torso (Z: 1.15 to 1.62)
    chest = create_box("Chest", (0.54, 0.32, 0.28), (0, 0, 1.48))
    parts.append(chest)
    chest_plate = create_box("ChestPlate", (0.42, 0.10, 0.24), (0, -0.15, 1.50))
    parts.append(chest_plate)
    abdomen = create_box("Abdomen", (0.44, 0.28, 0.22), (0, 0, 1.25))
    parts.append(abdomen)
    belt = create_box("Belt", (0.46, 0.30, 0.12), (0, 0, 1.10))
    parts.append(belt)
    for sign in [1, -1]:
        pouch = create_box(f"Pouch_{sign}", (0.10, 0.12, 0.10), (sign * 0.22, -0.14, 1.10))
        parts.append(pouch)

    # Heavy Armored Shoulders & Arms
    for side, sign in [("_L", 1), ("_R", -1)]:
        pauldron = create_box(f"Pauldron{side}", (0.22, 0.24, 0.18), (sign * 0.38, 0, 1.55))
        uarm = create_cylinder(f"UpperArm{side}", 0.09, 0.30, (sign * 0.40, 0, 1.34), segments=12)
        bracer = create_box(f"Bracer{side}", (0.14, 0.14, 0.28), (sign * 0.42, 0, 1.02))
        gauntlet = create_box(f"Gauntlet{side}", (0.12, 0.14, 0.14), (sign * 0.43, 0, 0.80))
        parts.extend([pauldron, uarm, bracer, gauntlet])

    # Pelvis & Armored Legs (Hips: 1.05 - 0.90, Thighs: 0.90 - 0.50, Shins: 0.50 - 0.14, Boots: 0.14 - 0.0)
    hips = create_box("Hips", (0.44, 0.30, 0.18), (0, 0, 0.98))
    parts.append(hips)
    for side, sign in [("_L", 1), ("_R", -1)]:
        thigh = create_cylinder(f"Thigh{side}", 0.12, 0.44, (sign * 0.15, 0, 0.72), segments=12)
        knee_plate = create_box(f"KneePlate{side}", (0.14, 0.14, 0.12), (sign * 0.15, -0.08, 0.50))
        shin = create_cylinder(f"Shin{side}", 0.10, 0.40, (sign * 0.15, 0, 0.30), segments=12)
        boot = create_box(f"Boot{side}", (0.16, 0.30, 0.14), (sign * 0.15, -0.05, 0.07))
        parts.extend([thigh, knee_plate, shin, boot])

    # Join parts
    bpy.ops.object.select_all(action='DESELECT')
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    guard = bpy.context.active_object
    guard.name = "Guard_Mesh"

    # Subsurf & Bevel
    mod_sub = guard.modifiers.new(name="Subsurf", type='SUBSURF')
    mod_sub.levels = 1
    apply_modifier(guard, "Subsurf")

    mod_bev = guard.modifiers.new(name="Bevel", type='BEVEL')
    mod_bev.width = 0.015
    mod_bev.segments = 2
    apply_modifier(guard, "Bevel")

    mod_tri = guard.modifiers.new(name="Triangulate", type='TRIANGULATE')
    apply_modifier(guard, "Triangulate")

    # Decimate to exact budget (~4,700 tris, target: 3,000 - 6,000)
    mod_dec = guard.modifiers.new(name="Decimate", type='DECIMATE')
    mod_dec.ratio = 0.45
    apply_modifier(guard, "Decimate")

    tri_count = len(guard.data.polygons)
    print(f"[Guard] Final poly/tri count: {tri_count} (Budget: 3,000 - 6,000)")

    # UV Unwrap
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')

    # Armature
    bpy.ops.object.armature_add(enter_editmode=True, align='WORLD', location=(0, 0, 0))
    arm_obj = bpy.context.active_object
    arm_obj.name = "Guard_Armature"
    eb = arm_obj.data.edit_bones
    eb.remove(eb[0])

    bone_defs = [
        ("Hips", (0, 0, 0.95), (0, 0, 1.15), None),
        ("Spine", (0, 0, 1.15), (0, 0, 1.38), "Hips"),
        ("Chest", (0, 0, 1.38), (0, 0, 1.65), "Spine"),
        ("Neck", (0, 0, 1.65), (0, 0, 1.74), "Chest"),
        ("Head", (0, 0, 1.74), (0, 0, 1.98), "Neck"),
        # Arms
        ("Shoulder_L", (0.12, 0, 1.55), (0.34, 0, 1.52), "Chest"),
        ("UpperArm_L", (0.34, 0, 1.52), (0.42, 0, 1.20), "Shoulder_L"),
        ("Forearm_L", (0.42, 0, 1.20), (0.43, 0, 0.92), "UpperArm_L"),
        ("Hand_L", (0.43, 0, 0.92), (0.44, 0, 0.76), "Forearm_L"),
        ("Shoulder_R", (-0.12, 0, 1.55), (-0.34, 0, 1.52), "Chest"),
        ("UpperArm_R", (-0.34, 0, 1.52), (-0.42, 0, 1.20), "Shoulder_R"),
        ("Forearm_R", (-0.42, 0, 1.20), (-0.43, 0, 0.92), "UpperArm_R"),
        ("Hand_R", (-0.43, 0, 0.92), (-0.44, 0, 0.76), "Forearm_R"),
        # Legs
        ("Thigh_L", (0.15, 0, 0.95), (0.15, 0, 0.50), "Hips"),
        ("Shin_L", (0.15, 0, 0.50), (0.15, 0, 0.14), "Thigh_L"),
        ("Foot_L", (0.15, 0, 0.14), (0.15, -0.18, 0.0), "Shin_L"),
        ("Thigh_R", (-0.15, 0, 0.95), (-0.15, 0, 0.50), "Hips"),
        ("Shin_R", (-0.15, 0, 0.50), (-0.15, 0, 0.14), "Thigh_R"),
        ("Foot_R", (-0.15, 0, 0.14), (-0.15, -0.18, 0.0), "Shin_R"),
    ]

    created_bones = {}
    for name, head_pos, tail_pos, parent_name in bone_defs:
        b = eb.new(name)
        b.head = Vector(head_pos)
        b.tail = Vector(tail_pos)
        created_bones[name] = b

    for name, head_pos, tail_pos, parent_name in bone_defs:
        if parent_name:
            created_bones[name].parent = created_bones[parent_name]

    bpy.ops.object.mode_set(mode='OBJECT')

    # Skinning
    bpy.ops.object.select_all(action='DESELECT')
    guard.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')

    # Ensure every vertex has at least one bone assignment (avoids FBX importer warning)
    hips_vg = guard.vertex_groups.get("Hips")
    if hips_vg:
        for v in guard.data.vertices:
            if len(v.groups) == 0:
                hips_vg.add([v.index], 1.0, 'REPLACE')


    # Animation
    scene = bpy.context.scene
    scene.frame_start = 1
    scene.frame_end = 60
    scene.render.fps = 30

    arm_obj.animation_data_create()
    action = bpy.data.actions.new(name="Guard_Idle")
    arm_obj.animation_data.action = action

    bpy.ops.object.mode_set(mode='POSE')
    pose_bones = arm_obj.pose.bones
    head_bone = pose_bones.get("Head")
    chest_bone = pose_bones.get("Chest")

    for f in [1, 60]:
        scene.frame_set(f)
        if head_bone:
            head_bone.rotation_euler = (0, 0, 0)
            head_bone.keyframe_insert(data_path="rotation_euler", frame=f)
        if chest_bone:
            chest_bone.location = (0, 0, 0)
            chest_bone.rotation_euler = (0, 0, 0)
            chest_bone.keyframe_insert(data_path="location", frame=f)
            chest_bone.keyframe_insert(data_path="rotation_euler", frame=f)

    scene.frame_set(30)
    if head_bone:
        head_bone.rotation_euler = (0, 0.015, -0.025)
        head_bone.keyframe_insert(data_path="rotation_euler", frame=30)
    if chest_bone:
        chest_bone.location = (0, -0.003, 0.008)
        chest_bone.rotation_euler = (0.012, 0, 0)
        chest_bone.keyframe_insert(data_path="location", frame=30)
        chest_bone.keyframe_insert(data_path="rotation_euler", frame=30)

    bpy.ops.object.mode_set(mode='OBJECT')

    # Material
    mat = bpy.data.materials.new(name="M_Guard")
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Roughness'].default_value = 0.80
        bsdf.inputs['Metallic'].default_value = 0.25
        tex_path = os.path.abspath("Assets/Characters/Guard/Guard_Albedo.png")
        if os.path.exists(tex_path):
            tex_img = bpy.data.images.load(tex_path)
            node_tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
            node_tex.image = tex_img
            mat.node_tree.links.new(node_tex.outputs['Color'], bsdf.inputs['Base Color'])
    guard.data.materials.append(mat)

    # Export FBX
    bpy.ops.object.select_all(action='DESELECT')
    guard.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj

    out_fbx = os.path.abspath("Assets/Characters/Guard/Guard.fbx")
    bpy.ops.export_scene.fbx(
        filepath=out_fbx,
        check_existing=False,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_anim=True,
        bake_anim_use_all_actions=False,
        bake_anim_use_nla_strips=False,
        bake_anim_step=1.0,
        add_leaf_bones=False
    )
    print(f"[Guard] Successfully exported to {out_fbx} (Tris: {tri_count})")


def build_lab_environment():
    reset_blender()

    # Floor (12m x 6m x 0.2m, top surface at Z=0.0m)
    floor_obj = create_box("Floor", (12.0, 6.0, 0.2), (0, 0, -0.1))
    
    # Subdivide floor with bmesh directly
    bm_f = bmesh.new()
    bm_f.from_mesh(floor_obj.data)
    bmesh.ops.subdivide_edges(bm_f, edges=bm_f.edges, cuts=6, use_grid_fill=True)
    bmesh.ops.triangulate(bm_f, faces=bm_f.faces)
    bm_f.to_mesh(floor_obj.data)
    bm_f.free()

    # Back Wall (12m wide, 0.4m thick, 5.5m high, at Y = 2.8m)
    back_wall = create_box("BackWall", (12.0, 0.4, 5.5), (0, 2.8, 2.75))

    # 4x Industrial Pillars along Back Wall (X = -4.5, -1.5, 1.5, 4.5)
    env_parts = [back_wall]
    for x in [-4.5, -1.5, 1.5, 4.5]:
        pillar = create_box(f"Pillar_{x}", (0.8, 0.7, 5.5), (x, 2.5, 2.75))
        flange_b = create_box(f"PillarFlangeB_{x}", (1.1, 0.9, 0.6), (x, 2.5, 0.3))
        flange_t = create_box(f"PillarFlangeT_{x}", (1.1, 0.9, 0.6), (x, 2.5, 5.2))
        pipe = create_cylinder(f"PillarPipe_{x}", 0.08, 5.2, (x + 0.35, 2.1, 2.75), segments=16)
        env_parts.extend([pillar, flange_b, flange_t, pipe])

    # 3x Containment Tank Cylinders in Background (X = -3.0, 0.0, +3.0)
    for x in [-3.0, 0.0, 3.0]:
        tank = create_cylinder(f"Tank_{x}", 0.55, 3.2, (x, 2.3, 1.8), segments=24)
        cap_b = create_cylinder(f"TankCapB_{x}", 0.65, 0.3, (x, 2.3, 0.35), segments=24)
        cap_t = create_cylinder(f"TankCapT_{x}", 0.65, 0.3, (x, 2.3, 3.25), segments=24)
        ring_m = create_cylinder(f"TankRing_{x}", 0.60, 0.15, (x, 2.3, 1.8), segments=24)
        env_parts.extend([tank, cap_b, cap_t, ring_m])

    # Join background architectural parts into WallStructures
    bpy.ops.object.select_all(action='DESELECT')
    for p in env_parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = env_parts[0]
    bpy.ops.object.join()
    wall_structs = bpy.context.active_object
    wall_structs.name = "Wall_Structures"

    # Triangulate wall structures with bmesh directly
    bm_w = bmesh.new()
    bm_w.from_mesh(wall_structs.data)
    bmesh.ops.triangulate(bm_w, faces=bm_w.faces)
    bm_w.to_mesh(wall_structs.data)
    bm_w.free()

    tri_floor = len(floor_obj.data.polygons)
    tri_wall = len(wall_structs.data.polygons)
    total_env_tris = tri_floor + tri_wall
    print(f"[Lab] Floor tris: {tri_floor}, Wall tris: {tri_wall}, Total: {total_env_tris} (Budget: 1,500 - 3,000)")

    # Material
    mat = bpy.data.materials.new(name="M_Lab_Trim")
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Roughness'].default_value = 0.85
        bsdf.inputs['Metallic'].default_value = 0.20
        tex_path = os.path.abspath("Assets/Environment/Lab/Lab_TrimSheet.png")
        if os.path.exists(tex_path):
            tex_img = bpy.data.images.load(tex_path)
            node_tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
            node_tex.image = tex_img
            mat.node_tree.links.new(node_tex.outputs['Color'], bsdf.inputs['Base Color'])

    floor_obj.data.materials.append(mat)
    wall_structs.data.materials.append(mat)

    # UV Unwrap both
    for obj in [floor_obj, wall_structs]:
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.01)
        bpy.ops.object.mode_set(mode='OBJECT')

    # Select both Floor and Wall_Structures for export
    bpy.ops.object.select_all(action='DESELECT')
    floor_obj.select_set(True)
    wall_structs.select_set(True)
    bpy.context.view_layer.objects.active = floor_obj

    out_fbx = os.path.abspath("Assets/Environment/Lab/Lab_BlockOut.fbx")
    bpy.ops.export_scene.fbx(
        filepath=out_fbx,
        check_existing=False,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        bake_anim=False
    )
    print(f"[Lab] Successfully exported to {out_fbx}")


if __name__ == "__main__":
    build_aeron()
    build_guard()
    build_lab_environment()
    print("ALL 3D ASSETS BUILT AND EXPORTED SUCCESSFULLY!")
