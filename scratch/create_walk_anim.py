import os, hashlib

ANIM_TEMPLATE = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Aeron_Walk
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - curve:
"""

FRAME_GUIDS = [
    "97111b25c5ba76fed0be6a7ab9cb8738", # 1
    "4bbe8661db631f887434442079e6ebe6", # 2
    "c0e076397be1ca6f72ba7e6806ff008e", # 3
    "9cbb9e62cb9b89e9dafc13bcc8a7026e", # 4
    "d351d65297a1d637a0b9e776cd6c591f", # 5
    "6f63b59d7331b1a73b4481545f0fa17f", # 6
    "bf1e3f79546da7bfe7f30cbc5c6e5faf", # 7
    "9bb0cc9037285ed4b9e729fa9d49888b", # 8
]

FRAME_TIME = 0.10

anim_content = ANIM_TEMPLATE
for i, g in enumerate(FRAME_GUIDS):
    t = i * FRAME_TIME
    anim_content += f"    - time: {t:.2f}\n      value: {{fileID: 21300000, guid: {g}, type: 3}}\n"

anim_content += f"""    attribute: m_Sprite
    path: 
    classID: 212
    script: {{fileID: 0}}
  m_SampleRate: 10
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 0
      script: {{fileID: 0}}
      typeID: 212
      customType: 23
      isPPtrCurve: 1
    pptrCurveMapping:
"""

for g in FRAME_GUIDS:
    anim_content += f"    - {{fileID: 21300000, guid: {g}, type: 3}}\n"

stop_time = len(FRAME_GUIDS) * FRAME_TIME
anim_content += f"""  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: {stop_time:.2f}
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: 1
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_GenerateMotionCurves: 0
  m_Events: []
"""

ANIM_DIRS = [
    r"c:\projects\LAST-GOD\Assets\Characters\Aeron\Animations\Walk",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Characters\Aeron\Animations\Walk"
]

WALK_ANIM_GUID = "e1f2a3b4c5d6e7f8091a2b3c4d5e6f7a"

for d in ANIM_DIRS:
    os.makedirs(d, exist_ok=True)
    # folder meta
    parent = os.path.dirname(d)
    folder_meta = os.path.join(parent, "Walk.meta")
    if not os.path.exists(folder_meta):
        fh = hashlib.md5(b"Walk_Anim_Folder_Guid").hexdigest()
        with open(folder_meta, "w", encoding="utf-8") as f:
            f.write(f"fileFormatVersion: 2\nguid: {fh}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
            
    anim_file = os.path.join(d, "Aeron_Walk.anim")
    with open(anim_file, "w", encoding="utf-8") as f:
        f.write(anim_content)
        
    meta_file = anim_file + ".meta"
    with open(meta_file, "w", encoding="utf-8") as f:
        f.write(f"fileFormatVersion: 2\nguid: {WALK_ANIM_GUID}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 7400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
    print(f"Generated {anim_file} (guid: {WALK_ANIM_GUID})")
