# ASSET LOG — The Last God

This document maintains a running inventory of all external and prototype assets imported into **The Last God** project. Every external asset must be logged here with its source, license, and usage details so placeholder assets can be easily identified and replaced prior to release.

---

## External & Active Prototype Asset Log

| Asset Name | Source / Search Query | License | Used For | File Location |
|---|---|---|---|---|
| **Aeron Character Concept Art** | User Provided Concept Art | Proprietary / Project Asset | Official character design artwork for Aeron (A-07 suit, glowing eye, arm bandage) | `Assets/Art/Sprites/Aeron_Concept.png` |
| **Aeron Sprite Sheet** | Processed from User Artwork (32x32 GBA Sprite Sheet) | Project Asset | Legacy reference sprite | `Assets/Art/Sprites/Aeron_Spritesheet.png` |
| **Cyber Guard Sprite Sheet** | 32x32 Sci-Fi Soldier/Guard (`"pixel soldier enemy 32x32 free"`) | CC0 / Prototype Asset | Legacy reference enemy placeholder | `Assets/Art/Sprites/Guard_Spritesheet.png` |
| **Laser Bullet Sprite** | 16x16 Energy Projectile (`"pixel laser bullet 16x16"`) | CC0 / Prototype Asset | Guard projectile sprite reference | `Assets/Art/Sprites/Bullet.png` |
| **Dark Vignette Overlay** | 240x160 Cutout Vignette Texture | CC0 / Prototype Asset | Dark lab silhouette mask reference | `Assets/Art/Sprites/DarkVignette.png` |
| **Heartbeat SFX** | Freesound.org (`"low heartbeat pulse thumping 8bit"`) | CC0 / Royalty-Free | Low thumping pulse sound during intro sequence | `Assets/Audio/AudioClips/heartbeat.wav` |
| **Machine Hum Ambient** | Freesound.org (`"sci-fi ambient drone machine hum loop"`) | CC0 / Royalty-Free | Looping background lab machine hum sound | `Assets/Audio/AudioClips/machine_hum.wav` |
| **Glass Shatter SFX** | Freesound.org (`"glass break crash crunch 8bit"`) | CC0 / Royalty-Free | Audio burst sound effect | `Assets/Audio/AudioClips/glass_shatter.wav` |
| **Alarm Siren SFX** | Freesound.org (`"bitcrushed alarm siren GBA synth"`) | CC0 / Royalty-Free | Warning siren sound | `Assets/Audio/AudioClips/alarm.wav` |
| **Gunshot SFX** | Freesound.org (`"laser rifle gunshot burst 8bit"`) | CC0 / Royalty-Free | Guard firing sound effect | `Assets/Audio/AudioClips/gunshot.wav` |
| **Slash SFX** | Freesound.org (`"sword slash blade swing 8bit"`) | CC0 / Royalty-Free | Melee attack sound effect | `Assets/Audio/AudioClips/slash.wav` |
| **Aeron Awakening Power Sheet** | Nano Banana AI Generator | Project Asset | Visual FX reference for cyan eye flare | `Assets/Art/Sprites/Aeron_Awakening_Sheet.png` |
| **Cyber Guard Combat Sheet** | Nano Banana AI Generator | Project Asset | Visual reference for Guard posture | `Assets/Art/Sprites/Guard_Spritesheet.png` |
| **Chronos Aura Temporal FX** | Nano Banana AI Generator | Project Asset | Temporal distortion glyph ring VFX reference | `Assets/Art/Sprites/Chronos_Aura_FX.png` |
| **Laser Bullet Projectile FX** | Nano Banana AI Generator | Project Asset | Plasma beam projectile reference | `Assets/Art/Sprites/Laser_Bullet_FX.png` |

## 2D Platformer Environment Assets (Act 1 Laboratory)

| Asset Name | Source / Tool | License | Description / Dimensions | File Location |
|---|---|---|---|---|
| **Visual Bible** | Visual Bible Document | Project Asset | Complete art bible, palette hierarchy, and lighting rules | `Assets/Art/References/VISUAL_BIBLE.md` |
| **Master Color Palette** | Palette Exporter | Project Asset | 10-color master palette (.hex, .gpl, .json) | `Assets/Art/References/palette.*` |
| **Lab Reference Suite** | AI Image Generator (`generate_image`) | Project Asset | Concept references: wide lab, side-scroll, hero pod, machinery | `Assets/Art/References/ref_*.jpg` |
| **Blender 2D Master Scene** | Blender 5.2.1 LTS | Project Asset | 2D planning scene with 8 Grease Pencil / depth collections | `Assets/Blender/Lab_2D/LAB_2D_MASTER.blend` |
| **2D Composition Study** | Blender 5.2.1 LTS | Project Asset | Rendered 2D orthographic composition study image | `Assets/Blender/Lab_2D/Lab_2D_Composition_Study.png` |
| **Modular Tile Library (12)** | Aseprite / PIL Generator | Project Asset | 32x32 tiles (Floor straight/grating/damaged/hazard, Platform surface/edges/support, Walls, Ceiling) | `Assets/Art/Tiles/LAB_Tile_*.png` |
| **Structural Sprites (6)** | Aseprite / PIL Generator | Project Asset | Wall A/B (32x64), Pillar A-7/B-3 (32x128), Beam A (64x16), Door A (32x64) | `Assets/Art/Environment/Background/`, `Props/` |
| **Hero Stasis Chamber** | Aseprite / PIL Generator | Project Asset | 64x96 hero stasis apparatus with transparent tube and cyan stasis glow | `Assets/Art/Environment/Containment/LAB_CONTAINMENT_Main.png` |
| **Secondary Pods (2)** | Aseprite / PIL Generator | Project Asset | Inactive pod (32x64), Damaged cracked tank with fluid puddle (48x80) | `Assets/Art/Environment/Containment/LAB_Containment_*.png` |
| **Machinery & Consoles (3)**| Aseprite / PIL Generator | Project Asset | Console A (64x32), PowerUnit A (48x48), ServerRack A (32x80) | `Assets/Art/Environment/Machinery/LAB_*.png` |
| **Piping & Hardware (5)** | Aseprite / PIL Generator | Project Asset | Pipe A/B (32x32), Pipe Junction (32x32), Valve A (32x32), Railing A (32x16) | `Assets/Art/Environment/Props/LAB_*.png` |
| **Decal Library (3)** | Aseprite / PIL Generator | Project Asset | Stencils: A-7 (32x16), B-3 (32x16), Restricted (32x16) | `Assets/Art/Environment/Decals/LAB_DECAL_*.png` |
| **VFX Sprites (4)** | Aseprite / PIL Generator | Project Asset | Steam puff (32x32), Spark (16x16), Cyan energy mote (16x16), Dust mote (8x8) | `Assets/Art/Environment/VFX/FX_*.png` |
| **Player Silhouette Proxy** | Aseprite / PIL Generator | Project Asset | 32x64 temporary 1.8m silhouette for platform scale and negative space testing | `Assets/Art/Environment/Gameplay/Player_Silhouette_Proxy.png` |
| **Native Aseprite Source (140)** | Aseprite CLI | Project Asset | Raw editable .aseprite source files | `Assets/Aseprite/Lab/*.aseprite` |
| **Parallax Component** | C# Engine Script | Project Asset | Multi-plane parallax controller with pixel snapping | `Assets/Scripts/2D/ParallaxLayer.cs` |
| **Act 1 2D Lab Scene** | Unity 6.5 / URP 2D | Project Asset | Assembled production scene with 3 Tilemaps, 5 Parallax planes, 4 Lights, VFX | `Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D.unity` |
| **In-Engine Camera Capture** | Unity 6.5 In-Engine Camera | Project Asset | High-resolution 1920x1080 camera screenshot of the live 2D scene | `Assets/Scenes/Act1_Lab_2D/Act1_Lab_2D_Screenshot.png` |

---



| Asset Name | Source / Tool | License | Description / Polycount | File Location |
|---|---|---|---|---|
| **Lab Trim Sheet** | Procedural Texture Generator | Project Asset | 2048×2048 master trim sheet (panels, grating, hazard stripes, UI) | `Assets/Environment/Lab/Lab_TrimSheet.png` |
| **Lab Modular Kit (21 Modules)** | Blender 5.2.1 LTS | Project Asset | 21 modular structural FBX assets (floors, stairs, pillars, pods, consoles, doors, pipes) | `Assets/Environment/Lab/Modules/MOD_*.fbx` |
| **Lab Production FBX** | Blender 5.2.1 LTS | Project Asset | Master assembled production environment mesh (~14k tris) | `Assets/Environment/Lab/Lab_Environment_Production.fbx` |
| **Lab Production .blend** | Blender 5.2.1 LTS | Project Asset | Master source Blender scene with modular collections & UVs | `Assets/Environment/Lab/Lab_Production.blend` |
| **Lab URP Lit Materials (10)** | Unity URP Shader Pipeline | Project Asset | Materials: Floor, Grating, MetalDark, MetalWorn, Glass, CyanEmission, Console, Cable, Concrete, Warning | `Assets/Materials/Environment/MAT_Lab_*.mat` |
| **Lab Modular Prefabs (21)** | Unity Prefab Pipeline | Project Asset | Configured modular prefabs with exact multi-submesh material slot mappings | `Assets/Prefabs/Environment/Lab/PF_*.prefab` |

---

## Aeron Character 2.5D Production Assets (True 2.5D Hybrid)

| Asset Name | Source / Tool | License | Description / Specs | File Location |
|---|---|---|---|---|
| **Aeron Turnaround Sheet** | Image Generation Model | Project Asset | Full-body turnaround reference (Front, Side, 3/4) | `Assets/Art/Reference/Characters/Aeron_Turnaround_Production.png` |
| **Aeron Portrait Reference** | Image Generation Model | Project Asset | Close-up face portrait (tired expression, cyan eye glow) | `Assets/Art/Reference/Characters/Aeron_Face_Portrait.png` |
| **Aeron Idle Sprite (Quantized)** | Aseprite CLI / Python Pipeline | Project Asset | 256×512 master sprite, palette-quantized, grounded at Y=492 | `Assets/Characters/Aeron/Sprites/Aeron_Idle_0.png` |
| **Aeron Base Sprite** | Python Sprite Processor | Project Asset | 256×512 segmented front-view idle sprite | `Assets/Characters/Aeron/Sprites/Aeron_Base_256.png` |
| **Aeron Emission Mask** | Python Sprite Processor | Project Asset | 256×512 isolated cyan eye and neck glow emission texture | `Assets/Characters/Aeron/Sprites/Aeron_Emission_256.png` |
| **Aeron Contact Shadow** | Python Texture Generator | Project Asset | 128×128 soft elliptical foot ambient shadow texture | `Assets/Characters/Aeron/Sprites/Aeron_ContactShadow.png` |
| **Aeron 2.5D Card Mesh** | Blender 5.2.1 LTS | Project Asset | 0.9m × 1.8m quad card mesh, bottom pivot, 12 tris, cylindrical curve | `Assets/Characters/Aeron/Meshes/Aeron_Card.fbx` |
| **Aeron Sprite Material** | Unity URP Shader Pipeline | Project Asset | URP Lit alpha-clipped double-sided material with cyan emission | `Assets/Characters/Aeron/Materials/MAT_Aeron_Sprite.mat` |
| **Aeron Shadow Material** | Unity URP Shader Pipeline | Project Asset | URP Lit transparent contact shadow material | `Assets/Characters/Aeron/Materials/MAT_Aeron_Shadow.mat` |
| **Aeron Player Prefab** | Unity Prefab Pipeline | Project Asset | Complete 2.5D player prefab (Mesh, Billboard, Collider, Health, Light) | `Assets/Characters/Aeron/Prefabs/Aeron_Player.prefab` |

---

## Full 3D First-Person Aeron Character Assets (Current Production)

| Asset Name | Source / Tool | License | Description / Specs | File Location |
|---|---|---|---|---|
| **Aeron 3D Front View** | Image Generation Model | Project Asset | Front full-body A-pose turnaround reference | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_Front.png` |
| **Aeron 3D Side View** | Image Generation Model | Project Asset | Side profile turnaround reference | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_Side.png` |
| **Aeron 3D Three-Quarter View** | Image Generation Model | Project Asset | 3/4 dynamic perspective reference | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_ThreeQuarter.png` |
| **Aeron 3D Back View** | Image Generation Model | Project Asset | Back view reference with cybernetic spine ports | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_Back.png` |
| **Aeron 3D Face Portrait** | Image Generation Model | Project Asset | Cinematic face close-up (gaunt jaw, cyan left eye `#6FE3FF`) | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_Face_Closeup.png` |
| **Aeron 3D Clothing Detail** | Image Generation Model | Project Asset | Dark fabric weave, A-07 barcode, forearm gauze, boots | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_Clothing_Detail.png` |
| **Aeron 3D First-Person Hands** | Image Generation Model | Project Asset | First-person view of hands, fingers, and veins | `Assets/Art/Reference/Characters/3D_Aeron/Aeron_3D_Hands_FirstPerson.png` |
| **Aeron 3D Albedo Map** | Texture Synthesis Pipeline | Project Asset | 2048×2048 sRGB albedo map with skin tones, suit weave, boots | `Assets/Characters/Aeron/Textures/Aeron_3D_Albedo.png` |
| **Aeron 3D Masks Map** | Texture Synthesis Pipeline | Project Asset | 2048×2048 Linear map: R=Metallic, G=Occlusion, A=Smoothness | `Assets/Characters/Aeron/Textures/Aeron_3D_Masks.png` |
| **Aeron 3D Normal Map** | Texture Synthesis Pipeline | Project Asset | 2048×2048 tangent space normal map for folds, seams, and muscles | `Assets/Characters/Aeron/Textures/Aeron_3D_Normal.png` |
| **Aeron 3D Emission Map** | Texture Synthesis Pipeline | Project Asset | 2048×2048 cyan emission map (`#6FE3FF`) for left eye & spine | `Assets/Characters/Aeron/Textures/Aeron_3D_Emission.png` |
| **Aeron Full Body 3D FBX** | Blender 5.2.1 LTS | Project Asset | 1.80m tall humanoid, 50+ bone rig, 5 fingers/hand, Idle/Walk/Run | `Assets/Characters/Aeron/Meshes/Aeron_FullBody_3D.fbx` |
| **Aeron First-Person Arms FBX**| Blender 5.2.1 LTS | Project Asset | Camera-local arms mesh with articulated fingers & gauze wraps | `Assets/Characters/Aeron/Meshes/Aeron_FirstPerson_Arms.fbx` |
| **Aeron 3D Production .blend** | Blender 5.2.1 LTS | Project Asset | Master Blender source file with complete rig and animation actions | `Assets/Characters/Aeron/Aeron_3D_Production.blend` |
| **Aeron 3D Head Material** | Unity URP Shader Pipeline | Project Asset | URP Lit matte skin/hair material (Smoothness 0.30) | `Assets/Characters/Aeron/Materials/MAT_Aeron_3D_Head.mat` |
| **Aeron 3D Suit Material** | Unity URP Shader Pipeline | Project Asset | URP Lit matte tactical suit material (Smoothness 0.15) | `Assets/Characters/Aeron/Materials/MAT_Aeron_3D_Suit.mat` |
| **Aeron 3D FP Arms Material** | Unity URP Shader Pipeline | Project Asset | URP Lit first-person arms material (Smoothness 0.25) | `Assets/Characters/Aeron/Materials/MAT_Aeron_3D_FPArms.mat` |
| **Aeron 3D Eyes Material** | Unity URP Shader Pipeline | Project Asset | URP Lit glowing eye material (`#6FE3FF`, intensity 1.5) | `Assets/Characters/Aeron/Materials/MAT_Aeron_3D_Eyes.mat` |
| **Aeron First-Person Prefab** | Unity Prefab Pipeline | Project Asset | Complete First-Person player prefab (CharacterController, Camera, Arms, Body) | `Assets/Prefabs/Player/Aeron_FirstPerson_Player.prefab` |

---

## Superseded — 2D Pipeline (retained in _Recovery)

| Asset Name | Source / Original Location | Status & Archival Reason | Backup Location |
|---|---|---|---|
| **GBA Sci-Fi Lab Tileset** | `Assets/Art/Sprites/LabTileset.png` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/` |
| **Pixel Stasis Glass Chamber** | `Assets/Art/Sprites/Chamber_Intact.png`<br>`Assets/Art/Sprites/Chamber_Cracked.png`<br>`Assets/Art/Sprites/Chamber_Shattered.png` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/Act1_Origin_StasisChamber_Root.prefab` |
| **Lab Scene Background Artwork** | `Assets/Art/Sprites/Lab_Background.png` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/` |
| **Modular Lab Environment Tiles** | `Assets/Art/Sprites/Lab_Tiles.png` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/Act1_Origin_Environment_Laboratory.prefab` |
| **Aeron Post-Chamber Awakening Idle Sprite** | `Assets/Art/Sprites/aeron onside idle/01.png`<br>`Assets/Art/Sprites/aeron outside idle/01.png` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/` |
| **Solid Dark Backdrop** | `Assets/Art/Backgrounds/Backdrop_Solid_Dark.png` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/` |
| **Parallax System Hierarchy** | `Assets/Scenes/Act1_Origin.unity` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/Act1_Origin_Parallax_System.prefab` |
| **Environment Laboratory Hierarchy** | `Assets/Scenes/Act1_Origin.unity` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/Act1_Origin_Environment_Laboratory.prefab` |
| **Stasis Chamber Hierarchy** | `Assets/Scenes/Act1_Origin.unity` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Prefabs/Act1_Origin_StasisChamber_Root.prefab` |
| **AmbientBackgroundFlicker & ParallaxLayer Scripts** | `Assets/Scripts/Core/` | superseded by 2.5D pivot, 2026-09-09. | `Assets/_Recovery/2D_Deprecated/Scripts/` |

---

## 3D Asset Technical Standards (True 2.5D Pipeline)
- **Unit Scale**: 1 Unity unit = 1.0 meter
- **Character Poly Budget**: 3,000 – 6,000 triangles per character
- **Environment Poly Budget**: 1,500 – 3,000 triangles for room block-outs
- **Texture Resolutions**:
  - Characters: 1024×1024 (Albedo, packed Roughness/Metallic)
  - Environment: 2048×2048 shared Trim Sheet
- **Shader**: Universal Render Pipeline / Lit, Smoothness 0.1 – 0.3 (matte)
