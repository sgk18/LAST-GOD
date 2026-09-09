import bpy
import os

files = [
    ("Assets/Characters/Aeron/Aeron.fbx", "Assets/Characters/Aeron/Aeron.blend"),
    ("Assets/Characters/Guard/Guard.fbx", "Assets/Characters/Guard/Guard.blend"),
    ("Assets/Environment/Lab/Lab_BlockOut.fbx", "Assets/Environment/Lab/Lab_BlockOut.blend"),
]

for fbx, blend in files:
    bpy.ops.wm.read_homefile(use_empty=True)
    abs_fbx = os.path.abspath(fbx)
    abs_blend = os.path.abspath(blend)
    bpy.ops.import_scene.fbx(filepath=abs_fbx)
    bpy.ops.wm.save_as_mainfile(filepath=abs_blend)
    print(f"Saved: {abs_blend}")
