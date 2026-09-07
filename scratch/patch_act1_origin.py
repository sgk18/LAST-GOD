import os

SCENE_PATHS = [
    r"c:\projects\LAST-GOD\Assets\Scenes\Act1_Origin.unity",
    r"c:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Origin.unity",
]

for p in SCENE_PATHS:
    if not os.path.exists(p):
        continue
    with open(p, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Fix lighting and endTerminalMonitor field targets in Act1OriginDirector
    content = content.replace("  lighting: {fileID: 108}", "  lighting: {fileID: 109}")
    content = content.replace("  endTerminalMonitor: {fileID: 227}", "  endTerminalMonitor: {fileID: 231}")

    # 2. Configure UniversalAdditionalCameraData component 273
    old_cam_data = """--- !u!114 &273
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 268}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: cb64b7bf8bbbd229e65ce6832fc1d5e7, type: 3}
  m_Name: 
  m_EditorClassIdentifier: """

    new_cam_data = """--- !u!114 &273
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 268}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: cb64b7bf8bbbd229e65ce6832fc1d5e7, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_RenderShadows: 1
  m_RequiresDepthTextureOption: 2
  m_RequiresOpaqueTextureOption: 2
  m_CameraType: 0
  m_Cameras: []
  m_RendererIndex: 1
  m_VolumeLayerMask:
    serializedVersion: 2
    m_Bits: 1
  m_VolumeTrigger: {fileID: 0}
  m_VolumeFrameworkUpdateModeOption: 2
  m_RenderPostProcessing: 0
  m_Antialiasing: 0
  m_AntialiasingQuality: 2
  m_StopNaN: 0
  m_Dithering: 0
  m_ClearDepth: 1
  m_AllowXRRendering: 1
  m_AllowHDROutput: 1"""

    if old_cam_data in content:
        content = content.replace(old_cam_data, new_cam_data)

    # 3. Ensure Camera clear color is not pitch black
    content = content.replace("  m_BackGroundColor: {r: 0.02, g: 0.03, b: 0.05, a: 0}",
                              "  m_BackGroundColor: {r: 0.08, g: 0.10, b: 0.14, a: 1}")

    with open(p, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Patched Act1_Origin: {p}")
