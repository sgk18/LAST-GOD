import os

SCENE_PATHS = [
    r"c:\projects\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"c:\projects\LAST-GOD\Assets\Scenes\Act1_Scene1_Lab.unity",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Scene1_Lab.unity"
]

FRAME1_GUID = "a1b2c3d4e5f60718293a4b5c6d7e8f90"
FRAME2_GUID = "b2c3d4e5f6a708192a3b4c5d6e7f8a91"
CHAMBER_GUID = "9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d"
PLAYER_GUID = "a071de01a1c54b2d8e3b123456780001"

for p in SCENE_PATHS:
    if not os.path.exists(p):
        continue
    with open(p, "r", encoding="utf-8") as f:
        content = f.read()

    # Replace invalid background sprite GUIDs
    content = content.replace("guid: c1d2e3f4a5b60718293a4b5c6d7e8f91", f"guid: {FRAME1_GUID}")
    content = content.replace("guid: d2e3f4a5b6c708192a3b4c5d6e7f8a92", f"guid: {FRAME2_GUID}")

    # Set background sorting layer to Default (-10) so it's lit by Global Light 2D
    content = content.replace("m_SortingLayerID: 1001\n  m_SortingLayer: 1001\n  m_SortingOrder: 0",
                              "m_SortingLayerID: 0\n  m_SortingLayer: 0\n  m_SortingOrder: -10")

    # Ensure camera background is not pitch black
    content = content.replace("m_BackGroundColor: {r: 0, g: 0, b: 0, a: 1}",
                              "m_BackGroundColor: {r: 0.08, g: 0.10, b: 0.14, a: 1}")

    with open(p, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Patched: {p}")

