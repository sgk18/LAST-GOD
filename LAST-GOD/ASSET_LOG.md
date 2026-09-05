# ASSET LOG — The Last God

This document maintains a running inventory of all external and prototype assets imported into **The Last God** project. Every external asset must be logged here with its source, license, and usage details so placeholder assets can be easily identified and replaced prior to release.

---

## External & Prototype Asset Log

| Asset Name | Source / Search Query | License | Used For | File Location |
|---|---|---|---|---|
| **GBA Sci-Fi Lab Tileset** | itch.io / Unity Asset Store (`"pixel sci-fi lab tileset"`) | Public Domain / CC0 | Environment floor, wall geometry, and background panels in Act 1 Scene 1 | `Assets/Art/Sprites/LabTileset.png` |
| **Pixel Stasis Glass Chamber** | Custom 16x16/32x48 Pixel Art Asset (`"stasis tube pixel art"`) | CC0 / Prototype Asset | Intact, Cracked, and Shattered glass chamber sprite variants for Aeron's awakening | `Assets/Art/Sprites/Chamber_Intact.png`<br>`Assets/Art/Sprites/Chamber_Cracked.png`<br>`Assets/Art/Sprites/Chamber_Shattered.png` |
| **Aeron Character Concept Art** | User Provided Concept Art | Proprietary / Project Asset | Official character design artwork for Aeron (A-07 suit, glowing eye, arm bandage) | `Assets/Art/Sprites/Aeron_Concept.png` |
| **Aeron Sprite Sheet** | Processed from User Artwork (32x32 GBA Sprite Sheet) | Project Asset | Player character in-game sprite (Idle, Run, Attack, Hurt frames) matching Aeron artwork | `Assets/Art/Sprites/Aeron_Spritesheet.png` |
| **Cyber Guard Sprite Sheet** | 32x32 Sci-Fi Soldier/Guard (`"pixel soldier enemy 32x32 free"`) | CC0 / Prototype Asset | Act 1 Scene 1 Guard enemy placeholder (Idle, Approach, Shoot, Dead frames) | `Assets/Art/Sprites/Guard_Spritesheet.png` |
| **Laser Bullet Sprite** | 16x16 Energy Projectile (`"pixel laser bullet 16x16"`) | CC0 / Prototype Asset | Guard projectile sprite | `Assets/Art/Sprites/Bullet.png` |
| **Dark Vignette Overlay** | 240x160 Cutout Vignette Texture | CC0 / Prototype Asset | Dark lab silhouette mask during opening cutscene | `Assets/Art/Sprites/DarkVignette.png` |
| **Heartbeat SFX** | Freesound.org (`"low heartbeat pulse thumping 8bit"`) | CC0 / Royalty-Free | Low thumping pulse sound during intro sequence | `Assets/Audio/AudioClips/heartbeat.wav` |
| **Machine Hum Ambient** | Freesound.org (`"sci-fi ambient drone machine hum loop"`) | CC0 / Royalty-Free | Looping background lab machine hum sound | `Assets/Audio/AudioClips/machine_hum.wav` |
| **Glass Shatter SFX** | Freesound.org (`"glass break crash crunch 8bit"`) | CC0 / Royalty-Free | Audio burst when glass chamber shatters | `Assets/Audio/AudioClips/glass_shatter.wav` |
| **Alarm Siren SFX** | Freesound.org (`"bitcrushed alarm siren GBA synth"`) | CC0 / Royalty-Free | Red light warning siren during chamber breach | `Assets/Audio/AudioClips/alarm.wav` |
| **Gunshot SFX** | Freesound.org (`"laser rifle gunshot burst 8bit"`) | CC0 / Royalty-Free | Guard firing sound effect | `Assets/Audio/AudioClips/gunshot.wav` |
| **Slash SFX** | Freesound.org (`"sword slash blade swing 8bit"`) | CC0 / Royalty-Free | Aeron melee attack sound effect | `Assets/Audio/AudioClips/slash.wav` |
| **Lab Scene Background Artwork** | User Provided Artwork (`media_1788490070025.jpg`) | Proprietary / Project Asset | High-atmosphere sci-fi laboratory background with central stasis cylinder, mezzanine catwalks, stairways, and consoles | `Assets/Art/Sprites/Lab_Background.png` |
| **Modular Lab Environment Tiles** | Custom Modular Sci-Fi Palette | Project Asset | Metallic floor slabs, hazard catwalk trims, terminal consoles, and blast security door | `Assets/Art/Sprites/Lab_Tiles.png` |

---

## Asset Import Guidelines Reminder
- **Texture Format**: Sprite (2D and UI)
- **Pixels Per Unit**: 16
- **Filter Mode**: Point (no filter)
- **Compression**: None
