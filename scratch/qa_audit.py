import os
from PIL import Image

def audit_assets():
    print("=== ASSET AUDIT ===")
    textures = [
        ("Assets/Characters/Aeron/Aeron_Albedo.png", (1024, 1024)),
        ("Assets/Characters/Aeron/Aeron_RoughMetal.png", (1024, 1024)),
        ("Assets/Characters/Guard/Guard_Albedo.png", (1024, 1024)),
        ("Assets/Characters/Guard/Guard_RoughMetal.png", (1024, 1024)),
        ("Assets/Environment/Lab/Lab_TrimSheet.png", (2048, 2048)),
    ]

    all_passed = True
    for path, expected_res in textures:
        if not os.path.exists(path):
            print(f"[FAIL] Missing file: {path}")
            all_passed = False
            continue
        im = Image.open(path)
        if im.size == expected_res:
            print(f"[PASS] {path}: Resolution {im.size[0]}x{im.size[1]} matches {expected_res[0]}x{expected_res[1]}")
        else:
            print(f"[FAIL] {path}: Resolution {im.size} != {expected_res}")
            all_passed = False

    fbx_files = [
        "Assets/Characters/Aeron/Aeron.fbx",
        "Assets/Characters/Guard/Guard.fbx",
        "Assets/Environment/Lab/Lab_BlockOut.fbx"
    ]
    for f in fbx_files:
        if os.path.exists(f):
            size_kb = os.path.getsize(f) / 1024
            print(f"[PASS] FBX exists: {f} ({size_kb:.1f} KB)")
        else:
            print(f"[FAIL] Missing FBX: {f}")
            all_passed = False

    if all_passed:
        print("=== ASSET AUDIT COMPLETED: ALL CHECKS PASSED ===")
    else:
        print("=== ASSET AUDIT FAILED ===")

if __name__ == "__main__":
    audit_assets()
