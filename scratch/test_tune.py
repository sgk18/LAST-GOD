import bpy

def test_decimate():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.mesh.primitive_cube_add()
    obj = bpy.context.active_object
    sub = obj.modifiers.new("Sub", 'SUBSURF')
    sub.levels = 3
    bpy.ops.object.modifier_apply(modifier="Sub")
    bev = obj.modifiers.new("Bev", 'BEVEL')
    bev.segments = 2
    bpy.ops.object.modifier_apply(modifier="Bev")
    tri = obj.modifiers.new("Tri", 'TRIANGULATE')
    bpy.ops.object.modifier_apply(modifier="Tri")
    print("Pre-decimate tris:", len(obj.data.polygons))

    dec = obj.modifiers.new("Dec", 'DECIMATE')
    dec.ratio = 0.40
    bpy.ops.object.modifier_apply(modifier="Dec")
    print("Post-decimate tris:", len(obj.data.polygons))

test_decimate()
