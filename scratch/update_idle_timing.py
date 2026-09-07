import os

ANIM_PATH = r"c:\projects\LAST-GOD\Assets\Characters\Aeron\Animations\Idle\Aeron_Idle.anim"
ANIM_PATH_INNER = r"c:\projects\LAST-GOD\LAST-GOD\Assets\Characters\Aeron\Animations\Idle\Aeron_Idle.anim"

F01 = "d42314df21e915c4d02bcbcec172bbd0"
F02 = "aa4e0f42ed01981f508f1663955d8e60"
F03 = "7962f63c9aa06e5e366497f42be8a92c"
F04 = "3a1f3a0011ed283f26f8819f6bffe014"
F05 = "de644496bde4c5c1d0e2ae1dd227844c"
F06 = "1a547c2e8a88d28bfc7b5b7dbbad9d57"
F07 = "30934504e52a96a72d2bedcd97201615"
F08 = "2862cfaa58216fc81e71dd9bd957fa96"

# Design:
# 0.00s to 2.00s: Base rest on Frame 01 (over 2 seconds of stillness)
# 2.00s to 3.26s: Smooth progressive inhale 1 -> 8 (~0.18s per frame)
# 3.26s to 3.70s: Apex pause on Frame 08 (~0.44s hold at peak inhale)
# 3.70s to 4.96s: Smooth progressive exhale 8 -> 1 (~0.18s per frame)
# 4.96s to 5.00s: Frame 01 seamless loop point
timeline = [
    (0.00, F01),
    (2.00, F01),
    (2.18, F02),
    (2.36, F03),
    (2.54, F04),
    (2.72, F05),
    (2.90, F06),
    (3.08, F07),
    (3.26, F08),
    (3.70, F08),
    (3.88, F07),
    (4.06, F06),
    (4.24, F05),
    (4.42, F04),
    (4.60, F03),
    (4.78, F02),
    (4.96, F01),
    (5.00, F01),
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
    m_StopTime: 5.0
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

for p in [ANIM_PATH, ANIM_PATH_INNER]:
    if os.path.exists(os.path.dirname(p)):
        with open(p, "w", encoding="utf-8") as f:
            f.write(anim_yaml)
        print(f"Updated: {p}")
