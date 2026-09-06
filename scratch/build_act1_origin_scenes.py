import os
import re

SCENE_ACT1_PATH = r"c:\projects\LAST-GOD\Assets\Scenes\Act1_Origin.unity"
SCENE_ACT1_PATH_INNER = r"c:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Origin.unity"
SCENE_MENU_PATH = r"c:\projects\LAST-GOD\Assets\Scenes\MainMenu_Origin.unity"
SCENE_MENU_PATH_INNER = r"c:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\MainMenu_Origin.unity"

def get_guid(meta_path):
    with open(meta_path, 'r', encoding='utf-8') as f:
        content = f.read()
    m = re.search(r'guid:\s*([0-9a-fA-F]{32})', content)
    return m.group(1) if m else None

# Fetch GUIDs of scripts
BASE = r"c:\projects\LAST-GOD\Assets"
GUIDS = {
    'CombatManager': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Combat/CombatManager.cs.meta")),
    'DamageSystem': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Combat/DamageSystem.cs.meta")),
    'AscensionSurge': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Combat/AscensionSurge.cs.meta")),
    'SlowMotionBullet': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Combat/SlowMotionBullet.cs.meta")),
    'ThirdPersonPlayerInput': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Player/ThirdPersonPlayerInput.cs.meta")),
    'ThirdPersonPlayerController': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Player/ThirdPersonPlayerController.cs.meta")),
    'ThirdPersonCameraController': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Player/ThirdPersonCameraController.cs.meta")),
    'ThirdPersonPlayerAnimator': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Player/ThirdPersonPlayerAnimator.cs.meta")),
    'GuardAI': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/AI/GuardAI.cs.meta")),
    'GuardWeapon': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/AI/GuardWeapon.cs.meta")),
    'DialogueSystem': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Dialogue/DialogueSystem.cs.meta")),
    'Act1OriginDirector': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Cinematics/Act1OriginDirector.cs.meta")),
    'StasisChamber': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Environment/StasisChamber.cs.meta")),
    'LabLightingController': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Environment/LabLightingController.cs.meta")),
    'InteractiveMonitor': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Environment/InteractiveMonitor.cs.meta")),
    'LabDoor': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Environment/LabDoor.cs.meta")),
    'VitalStabilityHUD': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/UI/VitalStabilityHUD.cs.meta")),
    'PauseMenuUI': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/UI/PauseMenuUI.cs.meta")),
    'MainMenuUI': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/UI/MainMenuUI.cs.meta")),
    'AudioManager': get_guid(os.path.join(BASE, "Scripts/ThirdPerson/Audio/AudioManager.cs.meta")),
}

print("Loaded script GUIDs:")
for k, v in GUIDS.items():
    print(f"  {k}: {v}")

class SceneBuilder:
    def __init__(self):
        self.file_id = 100
        self.blocks = []

    def next_id(self):
        self.file_id += 1
        return self.file_id

    def add_header(self, fog_color="0.04, 0.05, 0.08"):
        header = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 1
  m_FogColor: {{r: 0.04, g: 0.05, b: 0.08, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.035
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {{r: 0.08, g: 0.10, b: 0.14, a: 1}}
  m_AmbientEquatorColor: {{r: 0.06, g: 0.08, b: 0.10, a: 1}}
  m_AmbientGroundColor: {{r: 0.02, g: 0.02, b: 0.03, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.478, b: 0.627, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {{fileID: 0}}
  m_SpotCookie: {{fileID: 0}}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {{fileID: 0}}
  m_Sun: {{fileID: 0}}
  m_IndirectSpecularColor: {{r: 0, g: 0, b: 0, a: 1}}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {{fileID: 0}}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {{fileID: 0}}
  m_LightingSettings: {{fileID: 0}}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 2
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    accuratePlacement: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {{fileID: 0}}"""
        self.blocks.append(header)

    def create_gameobject(self, name, tag="Untagged", layer=0, pos=(0,0,0), rot=(0,0,0,1), scale=(1,1,1), parent=None):
        go_id = self.next_id()
        tr_id = self.next_id()
        go_data = {
            'go_id': go_id,
            'tr_id': tr_id,
            'name': name,
            'tag': tag,
            'layer': layer,
            'pos': pos,
            'rot': rot,
            'scale': scale,
            'parent': parent,
            'components': [tr_id],
            'children': []
        }
        if parent:
            parent['children'].append(tr_id)
        return go_data

    def add_primitive_mesh(self, go_data, prim_type="Cube"):
        # standard unity meshes: Cube=10202, Cylinder=10206, Sphere=10207, Capsule=10208
        mesh_id = {
            "Cube": 10202,
            "Cylinder": 10206,
            "Sphere": 10207,
            "Capsule": 10208
        }.get(prim_type, 10202)
        
        mf_id = self.next_id()
        mr_id = self.next_id()
        col_id = self.next_id()
        go_data['components'].extend([mf_id, mr_id, col_id])

        mf_block = f"""--- !u!33 &{mf_id}
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Mesh: {{fileID: {mesh_id}, guid: 0000000000000000e000000000000000, type: 0}}"""

        mr_block = f"""--- !u!23 &{mr_id}
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Enabled: 1
  m_CastShadows: 1
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 2
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10303, guid: 0000000000000000f000000000000000, type: 0}}"""

        if prim_type == "Cube":
            col_block = f"""--- !u!65 &{col_id}
BoxCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Size: {{x: 1, y: 1, z: 1}}
  m_Center: {{x: 0, y: 0, z: 0}}"""
        else:
            col_block = f"""--- !u!136 &{col_id}
CapsuleCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 2
  m_Radius: 0.5
  m_Height: 2
  m_Direction: 1
  m_Center: {{x: 0, y: 0, z: 0}}"""

        self.blocks.extend([mf_block, mr_block, col_block])

    def add_light(self, go_data, color=(1,1,1), intensity=1.5, range_val=12, light_type=2):
        l_id = self.next_id()
        go_data['components'].append(l_id)
        block = f"""--- !u!108 &{l_id}
Light:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Enabled: 1
  serializedVersion: 10
  m_Type: {light_type}
  m_Shape: 0
  m_Color: {{r: {color[0]}, g: {color[1]}, b: {color[2]}, a: 1}}
  m_Intensity: {intensity}
  m_Range: {range_val}
  m_SpotAngle: 30
  m_InnerSpotAngle: 21.80208
  m_CookieSize: 10
  m_Shadows:
    m_Type: 0
    m_Resolution: -1
    m_CustomResolution: -1
    m_Strength: 1
    m_Bias: 0.05
    m_NormalBias: 0.4
    m_NearPlane: 0.2
    m_CullingMatrixOverride:
      e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
    m_UseCullingMatrixOverride: 0
  m_Cookie: {{fileID: 0}}
  m_DrawHalo: 0
  m_Flare: {{fileID: 0}}
  m_RenderMode: 0
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingLayerMask: 1
  m_Lightmapping: 4
  m_LightShadowCasterMode: 0
  m_AreaSize: {{x: 1, y: 1}}
  m_BounceIntensity: 1
  m_ColorTemperature: 6570
  m_UseColorTemperature: 0
  m_BoundingSphereOverride: {{x: 0, y: 0, z: 0, w: 0}}
  m_UseBoundingSphereOverride: 0
  m_UseViewFrustumForShadowCasterCull: 1
  m_ShadowRadius: 0
  m_ShadowAngle: 0"""
        self.blocks.append(block)
        return l_id

    def add_camera(self, go_data, fov=60):
        c_id = self.next_id()
        al_id = self.next_id()
        go_data['components'].extend([c_id, al_id])

        cam_block = f"""--- !u!20 &{c_id}
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 1
  m_BackGroundColor: {{r: 0.02, g: 0.03, b: 0.05, a: 0}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {{x: 2, y: 11}}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.1
  far clip plane: 500
  field of view: {fov}
  orthographic: 0
  orthographic size: 5
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022"""

        al_block = f"""--- !u!81 &{al_id}
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Enabled: 1"""

        self.blocks.extend([cam_block, al_block])
        return c_id

    def add_monobehaviour(self, go_data, script_key, fields=""):
        guid = GUIDS.get(script_key, "")
        mb_id = self.next_id()
        go_data['components'].append(mb_id)
        block = f"""--- !u!114 &{mb_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
{fields}"""
        self.blocks.append(block)
        return mb_id

    def add_character_controller(self, go_data, height=1.9, radius=0.45):
        cc_id = self.next_id()
        go_data['components'].append(cc_id)
        block = f"""--- !u!143 &{cc_id}
CharacterController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Height: {height}
  m_Radius: {radius}
  m_SlopeLimit: 45
  m_StepOffset: 0.3
  m_SkinWidth: 0.08
  m_MinMoveDistance: 0.001
  m_Center: {{x: 0, y: {height*0.5}, z: 0}}"""
        self.blocks.append(block)
        return cc_id

    def finalize_gameobject(self, go_data):
        comps_str = "\n".join([f"  - component: {{fileID: {c}}}" for c in go_data['components']])
        children_str = "\n".join([f"  - {{fileID: {c}}}" for c in go_data['children']])
        parent_ref = f"{{fileID: {go_data['parent']['tr_id']}}}" if go_data['parent'] else "{fileID: 0}"

        go_block = f"""--- !u!1 &{go_data['go_id']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{comps_str}
  m_Layer: {go_data['layer']}
  m_Name: {go_data['name']}
  m_TagString: {go_data['tag']}
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1"""

        pos = go_data['pos']
        rot = go_data['rot']
        scale = go_data['scale']

        tr_block = f"""--- !u!4 &{go_data['tr_id']}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_data['go_id']}}}
  serializedVersion: 2
  m_LocalRotation: {{x: {rot[0]}, y: {rot[1]}, z: {rot[2]}, w: {rot[3]}}}
  m_LocalPosition: {{x: {pos[0]}, y: {pos[1]}, z: {pos[2]}}}
  m_LocalScale: {{x: {scale[0]}, y: {scale[1]}, z: {scale[2]}}}
  m_ConstrainProportionsScale: 0
  m_Children:
{children_str}
  m_Father: {parent_ref}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}"""

        self.blocks.extend([go_block, tr_block])

    def serialize(self):
        return "\n".join(self.blocks) + "\n"

def build_act1_origin_scene():
    sb = SceneBuilder()
    sb.add_header()

    all_gos = []

    # 1. Core Managers
    go_mgr = sb.create_gameobject("=== CORE MANAGERS ===")
    sb.add_monobehaviour(go_mgr, 'CombatManager')
    sb.add_monobehaviour(go_mgr, 'DialogueSystem')
    sb.add_monobehaviour(go_mgr, 'AudioManager')
    sb.add_monobehaviour(go_mgr, 'PauseMenuUI')
    all_gos.append(go_mgr)

    # 2. Lighting System
    go_light = sb.create_gameobject("Lighting_System")
    sb.add_monobehaviour(go_light, 'LabLightingController')
    all_gos.append(go_light)

    # Overhead sterile luminaires
    for i, (lx, ly, lz) in enumerate([(-6, 5.8, -6), (6, 5.8, -6), (-6, 5.8, 6), (6, 5.8, 6), (25, 4.8, 0), (45, 5.5, 0), (65, 4.5, 0), (82, 5.5, 0)]):
        gl = sb.create_gameobject(f"Light_Sterile_{i+1}", pos=(lx, ly, lz), parent=go_light)
        sb.add_light(gl, color=(0.7, 0.85, 1.0), intensity=1.8, range_val=14)
        all_gos.append(gl)

    # Emergency Red Beacons
    for i, (ex, ey, ez) in enumerate([(0, 6.0, 0), (-10, 5.5, 0), (10, 5.5, 0), (25, 5.0, 0), (45, 5.5, 0)]):
        ge = sb.create_gameobject(f"Light_Emergency_Red_{i+1}", pos=(ex, ey, ez), parent=go_light)
        sb.add_light(ge, color=(1.0, 0.05, 0.05), intensity=0.0, range_val=16)
        all_gos.append(ge)

    # 3. Environment Laboratory Root
    go_lab = sb.create_gameobject("Environment_Laboratory")
    all_gos.append(go_lab)

    # Area A: Containment Chamber Arena (30x30 floor, 6.5h walls)
    f_a = sb.create_gameobject("Floor_AreaA", pos=(0, -0.25, 0), scale=(30, 0.5, 30), parent=go_lab)
    sb.add_primitive_mesh(f_a, "Cube")
    all_gos.append(f_a)

    c_a = sb.create_gameobject("Ceiling_AreaA", pos=(0, 6.5, 0), scale=(30, 0.5, 30), parent=go_lab)
    sb.add_primitive_mesh(c_a, "Cube")
    all_gos.append(c_a)

    w_south = sb.create_gameobject("Wall_South", pos=(0, 3.25, -15), scale=(30, 6.5, 0.8), parent=go_lab)
    sb.add_primitive_mesh(w_south, "Cube")
    all_gos.append(w_south)

    w_west = sb.create_gameobject("Wall_West", pos=(-15, 3.25, 0), scale=(0.8, 6.5, 30), parent=go_lab)
    sb.add_primitive_mesh(w_west, "Cube")
    all_gos.append(w_west)

    w_east = sb.create_gameobject("Wall_East", pos=(15, 3.25, 0), scale=(0.8, 6.5, 30), parent=go_lab)
    sb.add_primitive_mesh(w_east, "Cube")
    all_gos.append(w_east)

    # Central Dais Platform
    dais = sb.create_gameobject("Central_Dais_Platform", pos=(0, 0.3, 0), scale=(6, 0.6, 6), parent=go_lab)
    sb.add_primitive_mesh(dais, "Cube")
    all_gos.append(dais)

    # Overhead Apparatus
    apparatus = sb.create_gameobject("Overhead_Stasis_Apparatus", pos=(0, 5.2, 0), scale=(3.2, 2.0, 3.2), parent=go_lab)
    sb.add_primitive_mesh(apparatus, "Cube")
    all_gos.append(apparatus)

    # Area B: Elevated Control Room
    f_b = sb.create_gameobject("Floor_AreaB", pos=(0, 2.5, 20), scale=(20, 0.5, 10), parent=go_lab)
    sb.add_primitive_mesh(f_b, "Cube")
    all_gos.append(f_b)

    c_b = sb.create_gameobject("Ceiling_AreaB", pos=(0, 7.5, 20), scale=(20, 0.5, 10), parent=go_lab)
    sb.add_primitive_mesh(c_b, "Cube")
    all_gos.append(c_b)

    glass_b = sb.create_gameobject("ObservationWindow_Glass", pos=(0, 4.2, 15), scale=(16, 3.2, 0.3), parent=go_lab)
    sb.add_primitive_mesh(glass_b, "Cube")
    all_gos.append(glass_b)

    # Area C: Security Corridor
    f_c = sb.create_gameobject("Floor_AreaC", pos=(25, -0.25, 0), scale=(20, 0.5, 8), parent=go_lab)
    sb.add_primitive_mesh(f_c, "Cube")
    all_gos.append(f_c)

    c_c = sb.create_gameobject("Ceiling_AreaC", pos=(25, 5.5, 0), scale=(20, 0.5, 8), parent=go_lab)
    sb.add_primitive_mesh(c_c, "Cube")
    all_gos.append(c_c)

    # Area D: Medical Bay
    f_d = sb.create_gameobject("Floor_AreaD", pos=(45, -0.25, 0), scale=(20, 0.5, 30), parent=go_lab)
    sb.add_primitive_mesh(f_d, "Cube")
    all_gos.append(f_d)

    # Area E & F: Escape Corridor & Airlock
    f_e = sb.create_gameobject("Floor_AreaE", pos=(65, -0.25, 0), scale=(20, 0.5, 8), parent=go_lab)
    sb.add_primitive_mesh(f_e, "Cube")
    all_gos.append(f_e)

    f_f = sb.create_gameobject("Floor_AreaF", pos=(82, -0.25, 0), scale=(14, 0.5, 18), parent=go_lab)
    sb.add_primitive_mesh(f_f, "Cube")
    all_gos.append(f_f)

    # Interactive Terminal in Area F
    term_stand = sb.create_gameobject("Terminal_ProjectAscension", pos=(86, 1.2, 0), parent=go_lab)
    sb.add_primitive_mesh(term_stand, "Cube")
    sb.add_monobehaviour(term_stand, 'InteractiveMonitor', "  content: 4\n")
    all_gos.append(term_stand)

    # 4. Stasis Chamber
    go_ch = sb.create_gameobject("StasisChamber_Root", pos=(0, 0.7, 0))
    mb_ch = sb.add_monobehaviour(go_ch, 'StasisChamber')
    ch_glass = sb.create_gameobject("GlassCylinder", pos=(0, 1.4, 0), scale=(2.2, 1.4, 2.2), parent=go_ch)
    sb.add_primitive_mesh(ch_glass, "Cylinder")
    all_gos.append(ch_glass)

    ch_liquid = sb.create_gameobject("StasisLiquid", pos=(0, 1.3, 0), scale=(2.1, 1.3, 2.1), parent=go_ch)
    sb.add_primitive_mesh(ch_liquid, "Cylinder")
    all_gos.append(ch_liquid)

    ch_light = sb.create_gameobject("Chamber_Internal_Light", pos=(0, 1.4, 0), parent=go_ch)
    sb.add_light(ch_light, color=(0.15, 0.9, 1.0), intensity=2.5, range_val=7)
    all_gos.append(ch_light)
    all_gos.append(go_ch)

    # 5. Aeron Protagonist
    go_aeron = sb.create_gameobject("Aeron_Protagonist", tag="Player", pos=(0, 0.7, 0))
    sb.add_character_controller(go_aeron, height=1.9, radius=0.42)
    sb.add_monobehaviour(go_aeron, 'ThirdPersonPlayerInput')
    sb.add_monobehaviour(go_aeron, 'DamageSystem')
    sb.add_monobehaviour(go_aeron, 'AscensionSurge')
    mb_aeron = sb.add_monobehaviour(go_aeron, 'ThirdPersonPlayerController')
    
    aeron_vis = sb.create_gameobject("Aeron_Visual", pos=(0, 0, 0), parent=go_aeron)
    aeron_torso = sb.create_gameobject("Torso", pos=(0, 0.95, 0), scale=(0.55, 0.55, 0.35), parent=aeron_vis)
    sb.add_primitive_mesh(aeron_torso, "Capsule")
    all_gos.append(aeron_torso)

    aeron_head = sb.create_gameobject("Head", pos=(0, 1.62, 0), scale=(0.32, 0.35, 0.32), parent=aeron_vis)
    sb.add_primitive_mesh(aeron_head, "Sphere")
    all_gos.append(aeron_head)

    sb.add_monobehaviour(aeron_vis, 'ThirdPersonPlayerAnimator')
    all_gos.append(aeron_vis)
    all_gos.append(go_aeron)

    # 6. Main Camera
    go_cam = sb.create_gameobject("Main Camera", tag="MainCamera", pos=(0, 2.2, -4.5))
    sb.add_camera(go_cam, fov=60)
    mb_cam = sb.add_monobehaviour(go_cam, 'ThirdPersonCameraController')
    sb.add_monobehaviour(go_cam, 'VitalStabilityHUD')
    all_gos.append(go_cam)

    # 7. Dr. Ilya Voss NPC
    go_voss = sb.create_gameobject("NPC_Dr_Ilya_Voss", pos=(0, 2.75, 18))
    voss_body = sb.create_gameobject("Body", pos=(0, 0.85, 0), scale=(0.48, 0.55, 0.32), parent=go_voss)
    sb.add_primitive_mesh(voss_body, "Capsule")
    all_gos.append(voss_body)
    all_gos.append(go_voss)

    # 8. Sliding Security Blast Door
    go_door = sb.create_gameobject("SecurityDoor_AreaA_to_AreaC", pos=(15, 1.5, 0))
    mb_door = sb.add_monobehaviour(go_door, 'LabDoor')
    door_l = sb.create_gameobject("DoorPanel_Left", pos=(0, 0, -1.8), scale=(0.4, 3.2, 3.6), parent=go_door)
    sb.add_primitive_mesh(door_l, "Cube")
    all_gos.append(door_l)
    door_r = sb.create_gameobject("DoorPanel_Right", pos=(0, 0, 1.8), scale=(0.4, 3.2, 3.6), parent=go_door)
    sb.add_primitive_mesh(door_r, "Cube")
    all_gos.append(door_r)
    all_gos.append(go_door)

    # 9. Security Guards (5 guards)
    guard_gos = []
    spawns = [(18, 0.5, -1.5), (20, 0.5, 1.5), (23, 0.5, -2.5), (26, 0.5, 2.0), (28, 0.5, 0.0)]
    for i, sp in enumerate(spawns):
        g = sb.create_gameobject(f"Guard_Security_{i+1}", tag="Enemy", pos=sp)
        sb.add_character_controller(g, height=1.9, radius=0.45)
        sb.add_monobehaviour(g, 'DamageSystem')
        sb.add_monobehaviour(g, 'GuardWeapon')
        sb.add_monobehaviour(g, 'GuardAI')
        g_body = sb.create_gameobject("TacticalArmor", pos=(0, 0.95, 0), scale=(0.6, 0.6, 0.45), parent=g)
        sb.add_primitive_mesh(g_body, "Capsule")
        all_gos.append(g_body)
        g_helm = sb.create_gameobject("TacticalHelmet", pos=(0, 1.65, 0), scale=(0.38, 0.38, 0.38), parent=g)
        sb.add_primitive_mesh(g_helm, "Sphere")
        all_gos.append(g_helm)
        all_gos.append(g)
        guard_gos.append(g)

    # 10. Master Act 1 Sequence Director
    go_dir = sb.create_gameobject("=== ACT 1 ORIGIN DIRECTOR ===")
    dir_fields = f"""  player: {{fileID: {mb_aeron}}}
  cameraController: {{fileID: {mb_cam}}}
  chamber: {{fileID: {mb_ch}}}
  lighting: {{fileID: {go_light['components'][0]}}}
  endTerminalMonitor: {{fileID: {term_stand['components'][0]}}}
  securityDoor: {{fileID: {mb_door}}}
  drVossNPC: {{fileID: {go_voss['tr_id']}}}"""
    sb.add_monobehaviour(go_dir, 'Act1OriginDirector', dir_fields)
    all_gos.append(go_dir)

    for g in all_gos:
        sb.finalize_gameobject(g)

    return sb.serialize()

def build_main_menu_scene():
    sb = SceneBuilder()
    sb.add_header()

    all_gos = []

    # Camera with MainMenuUI
    go_cam = sb.create_gameobject("Main Camera", tag="MainCamera", pos=(0, 2.0, -4.5), rot=(0.1045, 0, 0, 0.9945))
    sb.add_camera(go_cam, fov=60)
    sb.add_monobehaviour(go_cam, 'MainMenuUI')
    all_gos.append(go_cam)

    # Background silhouette dais & pod
    dais = sb.create_gameobject("Dais_Silhouette", pos=(0, 0.3, 0), scale=(4, 0.6, 4))
    sb.add_primitive_mesh(dais, "Cube")
    all_gos.append(dais)

    pod = sb.create_gameobject("Stasis_Pod_Silhouette", pos=(0, 1.6, 0), scale=(1.8, 1.4, 1.8))
    sb.add_primitive_mesh(pod, "Cylinder")
    all_gos.append(pod)

    p_light = sb.create_gameobject("Pod_Glow_Light", pos=(0, 1.6, 0))
    sb.add_light(p_light, color=(0.2, 0.85, 1.0), intensity=3.0, range_val=8)
    all_gos.append(p_light)

    for g in all_gos:
        sb.finalize_gameobject(g)

    return sb.serialize()

if __name__ == "__main__":
    act1_yaml = build_act1_origin_scene()
    with open(SCENE_ACT1_PATH, "w", encoding="utf-8") as f:
        f.write(act1_yaml)
    with open(SCENE_ACT1_PATH_INNER, "w", encoding="utf-8") as f:
        f.write(act1_yaml)
    print(f"Generated {SCENE_ACT1_PATH} ({len(act1_yaml)} bytes)")

    menu_yaml = build_main_menu_scene()
    with open(SCENE_MENU_PATH, "w", encoding="utf-8") as f:
        f.write(menu_yaml)
    with open(SCENE_MENU_PATH_INNER, "w", encoding="utf-8") as f:
        f.write(menu_yaml)
    print(f"Generated {SCENE_MENU_PATH} ({len(menu_yaml)} bytes)")
