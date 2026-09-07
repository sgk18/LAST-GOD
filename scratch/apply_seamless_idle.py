import os

ANIM_PATHS = [
    r"c:\projects\LAST-GOD\Assets\Characters\Aeron\Animations\Idle\Aeron_Idle.anim",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Characters\Aeron\Animations\Idle\Aeron_Idle.anim"
]

F01 = "d42314df21e915c4d02bcbcec172bbd0"
F02 = "aa4e0f42ed01981f508f1663955d8e60"
F03 = "7962f63c9aa06e5e366497f42be8a92c"
F04 = "3a1f3a0011ed283f26f8819f6bffe014"
F05 = "de644496bde4c5c1d0e2ae1dd227844c"
F06 = "1a547c2e8a88d28bfc7b5b7dbbad9d57"
F07 = "30934504e52a96a72d2bedcd97201615"
F08 = "2862cfaa58216fc81e71dd9bd957fa96"

# Fluid, continuous breathing curve with NO artificial freeze:
# Total loop = 3.20 seconds
# Frame 1: 0.00s -> 0.24s (0.24s)
# Frame 2: 0.24s -> 0.46s (0.22s)
# Frame 3: 0.46s -> 0.67s (0.21s)
# Frame 4: 0.67s -> 0.87s (0.20s)
# Frame 5: 0.87s -> 1.07s (0.20s)
# Frame 6: 1.07s -> 1.28s (0.21s)
# Frame 7: 1.28s -> 1.50s (0.22s)
# Frame 8: 1.50s -> 1.74s (0.24s) [Apex]
# Frame 7: 1.74s -> 1.96s (0.22s)
# Frame 6: 1.96s -> 2.17s (0.21s)
# Frame 5: 2.17s -> 2.37s (0.20s)
# Frame 4: 2.37s -> 2.57s (0.20s)
# Frame 3: 2.57s -> 2.78s (0.21s)
# Frame 2: 2.78s -> 3.00s (0.22s)
# Frame 1: 3.00s -> 3.20s (0.20s) [seamless connection to 0.00s]

timeline = [
    (0.00, F01),
    (0.24, F02),
    (0.46, F03),
    (0.67, F04),
    (0.87, F05),
    (1.07, F06),
    (1.28, F07),
    (1.50, F08),
    (1.74, F07),
    (1.96, F06),
    (2.17, F05),
    (2.37, F04),
    (2.57, F03),
    (2.78, F02),
    (3.00, F01),
    (3.20, F01),
]

curve_lines = []
mapping_lines = []

for t, guid in timeline:
    curve_lines.append(f"    - time: {t:.2f}\n      value: {{fileID: 21300000, guid: {guid}, type: 3}}")
    mapping_lines.append(f"    - {{fileID: 21300000, guid: {guid}, type: 3}}")

curve_str = "\n".join(curve_lines)
mapping_str = "\n".join(mapping_lines)

anim_yaml = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: Aeron_Idle
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
{curve_str}
    attribute: m_Sprite
    path: 
    classID: 212
    script: {{fileID: 0}}
  m_SampleRate: 50
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
{mapping_str}
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: 3.20
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

for p in ANIM_PATHS:
    if os.path.exists(os.path.dirname(p)):
        with open(p, "w", encoding="utf-8") as f:
            f.write(anim_yaml)
        print(f"Written: {p}")
