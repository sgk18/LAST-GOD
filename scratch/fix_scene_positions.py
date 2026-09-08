import os, re

SCENE_PATHS = [
    r"c:\projects\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Scene1.unity"
]

for p in SCENE_PATHS:
    with open(p, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Update Camera: Zoom out (orthographic size from 5 to 7.5) and lower position Y to -14.0
    content = re.sub(r'orthographic size:\s*5(\.0*)?', 'orthographic size: 7.5', content)
    content = content.replace(
        "m_LocalPosition: {x: 0, y: -9.5, z: -10}",
        "m_LocalPosition: {x: 5, y: -14.0, z: -10}"
    )

    # 2. Lower Collider_MainDeck so floor top is at y = -17.10 (actual walkway floor, below computer)
    # Previously y = -15.28 with height 4.5 -> top was -13.03.
    # Now y = -19.35 with height 4.5 -> top is -17.10.
    content = content.replace(
        "--- !u!4 &4200002\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 4200001}\n  serializedVersion: 2\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: -15.28, z: 0}",
        "--- !u!4 &4200002\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 4200001}\n  serializedVersion: 2\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: -19.35, z: 0}"
    )

    # 3. Position Player on the open floor at x = 5.0, y = -17.10 (not on the computer!)
    content = content.replace(
        "--- !u!4 &3000002\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 3000001}\n  serializedVersion: 2\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: -13.03, z: 0}",
        "--- !u!4 &3000002\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 3000001}\n  serializedVersion: 2\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 5, y: -17.10, z: 0}"
    )

    # 4. Attach Animator with Aeron.controller to Player in Act1_Scene1 if missing
    if "guid: 5cf7de8c63c5d78494648cab33915b53" not in content:
        # Add component to Player GameObject
        content = content.replace(
            "  m_Component:\n  - component: {fileID: 3000002}\n  - component: {fileID: 3000003}\n  - component: {fileID: 3000004}\n  - component: {fileID: 3000005}\n  - component: {fileID: 3000006}\n  - component: {fileID: 3000007}",
            "  m_Component:\n  - component: {fileID: 3000002}\n  - component: {fileID: 3000003}\n  - component: {fileID: 3000008}\n  - component: {fileID: 3000004}\n  - component: {fileID: 3000005}\n  - component: {fileID: 3000006}\n  - component: {fileID: 3000007}"
        )
        # Append Animator component block
        animator_block = """--- !u!95 &3000008
Animator:
  serializedVersion: 5
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 3000001}
  m_Enabled: 1
  m_Avatar: {fileID: 0}
  m_Controller: {fileID: 9100000, guid: 5cf7de8c63c5d78494648cab33915b53, type: 2}
"""
        content += "\n" + animator_block

    # Update SpriteRenderer to Aeron_Idle_01
    content = content.replace(
        "guid: a071de01a1c54b2d8e3b123456780001",
        "guid: d42314df21e915c4d02bcbcec172bbd0"
    )

    with open(p, "w", encoding="utf-8") as f:
        f.write(content)
    print("Updated:", p)
