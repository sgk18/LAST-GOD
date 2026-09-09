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
