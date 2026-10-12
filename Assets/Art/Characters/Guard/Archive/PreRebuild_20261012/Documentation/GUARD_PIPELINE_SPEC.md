# THE LAST GOD — ACT 1: ORIGIN
## CANONICAL 2D GUARD CHARACTER & ANIMATION PIPELINE SPECIFICATION

### 1. Character Identity & Art Direction Lock
- **Role**: Laboratory Security Operative / Covert Investigator (Playable character in Act 1 Prologue before discovering Aeron).
- **Visual Style**: Dark Ink Graphic Novel + Industrial Sci-Fi + Heavy Shadows + Limited Palette + Hand-Illustrated 2D Character Art.
- **Physical Build**: Adult male, athletic/lean build, height 1.78m (grounded, practical boots, no superhuman bulk or armor).
- **Personality**: Alert, professional, cautious, suspicious, human, vulnerable compared to the towering laboratory.
- **Anatomy & Features**:
  - Locked skull, sharp jawline, defined brow, controlled graphic novel eyes (zero anime or chibi styling).
  - Hairstyle: Canonical short dark tapered cut with subtle side part, perfectly consistent across all 8 frames.
  - Costume: Dark charcoal tactical fatigue trousers & long sleeves, navy blue-grey modular tactical vest with shoulder radio microphone, heavy duty utility belt with pouches and side holster, laced combat boots.
  - Weapon: `GUARD_PISTOL_MASTER` — compact utilitarian semi-automatic pistol, 4.0" barrel, squared trigger guard, tritium sights, matte black steel slide.
  - Flashlight: `GUARD_FLASHLIGHT_MASTER` — knurled tactical aluminum LED flashlight with front bezel and tail switch.

---

### 2. Master Color System (Palette Compliance)
| Palette Key | Hex Code | Role in Character | Usage Distribution |
|---|---|---|---|
| **Dark Base** | `#080B0F` | Deepest shadows, occlusions, background | Baseline / Void |
| **Dark Charcoal** | `#11161C` | Uniform trousers & sleeves, ink outlines | 60% Body Base |
| **Secondary Blue-Grey** | `#1B2430` | Tactical vest, harness, shoulder mic | 20% Uniform Accent |
| **Industrial Grey** | `#303841` | Duty belt, equipment pouches, holster | 10% Gear |
| **Light Metal** | `#4A535C` | Pistol slide, belt buckles, flashlight bezel | 5% Hardware |
| **Dark Cyan** | `#245B70` | Cold environmental shadow falloff | Environmental Tint |
| **Cyan Accent** | `#6FE3FF` | Cold rim lighting on shoulder/edges | Key Accent 5% |
| **Bright Cyan** | `#CFF4FF` | Specular glint from lab stasis apparatus | Hotspot <2% |
| **Warning Orange** | `#C4502E` | Radio status LED, emergency warning accents | Indicator <1% |
| **Muted Skin Midtone** | `#B88B6E` | Graphic novel inked facial and hand flesh | Anatomy Midtone |
| **Muted Skin Shadow** | `#8A5F48` | Inked facial core shadow and jaw contour | Anatomy Shadow |
| **Deep Skin Shadow** | `#5A3D2E` | Occlusion under brow, jawline, and collar | Anatomy Occlusion |

---

### 3. Master Sprite Metrics & World Alignment
- **Master Canvas Size**: `128 × 192` pixels (constant across all animations).
- **Character Visible Height**: `168` pixels (~87.5% of vertical frame height, within the mandated 80–90% range).
- **Margins**: Top margin 14px (hair clearance), bottom margin 10px (soles grounded at `y = 182`), lateral margins 30px on each side (weapon/elbow clearance).
- **Pixels Per Unit (PPU)**: `96 PPU`.
- **Character World Scale**: `168 / 96 = 1.75` Unity units (meters), matching standard humanoid scale and the Act 1 Laboratory benchmark.
- **Pivot Alignment**: **Bottom-Center** (`x = 0.50`, `y = 0.052` at soles line). Ground contact remains fixed across all frames with zero sliding, floating, or foot jitter.

---

### 4. 8-Frame Idle Animation Architecture
- **Sprite Sheet**: `Assets/Art/Characters/Guard/Sprites/Guard_Idle_8F.png` (`1024 × 192` pixels, 8 horizontal frames).
- **Frame Rate**: `12 FPS` (`83.33ms` per frame, total loop duration `0.667s`).
- **Looping**: Seamless cyclic loop (`Frame 1 = Frame 8` loop closure).
- **Choreography**:
  - `Frame 1` (0.000s): Neutral baseline alert standing pose.
  - `Frame 2` (0.083s): Very slight breathing expansion begins (chest rises +0.4px).
  - `Frame 3` (0.167s): Peak inhalation (chest reaches apex +0.8px, head follows smoothly +0.6px).
  - `Frame 4` (0.250s): Apex hold and subtle weight shift (belt +0.2px lateral shift).
  - `Frame 5` (0.333s): Inhalation reversal (chest begins descending toward baseline).
  - `Frame 6` (0.417s): Mid-exhalation and shoulder settling.
  - `Frame 7` (0.500s): Easing back toward neutral baseline.
  - `Frame 8` (0.583s): Return exactly to neutral state, closing the loop.
- **Identity Preservation**:
  - Face, skull, jaw, eyes, and hair silhouette are pixel-for-pixel consistent.
  - Pistol on hip holster remains completely steady.
  - Boots and lower legs are locked to ground datum with zero jitter.
  - Continuous mathematical displacement field guarantees zero seams, tearing, or gaps.

---

### 5. Production Toolchain & Deliverables
1. **Authoritative Master References** (`Assets/Art/Characters/Guard/References/`):
   - `GUARD_MASTER_REFERENCE.png`: 8 canonical views (Front, Side Profile, 3/4 Front, Back, Sentry Profile, Weapon Master, Equipment Master, Combat Stance).
   - `GUARD_COLOR_REFERENCE.png`: Full palette swatches, hex codes, and usage rules.
   - `GUARD_SILHOUETTE_REFERENCE.png`: Silhouette verified on Light, Dark, Cyan, and Lab backgrounds.
   - `GUARD_PISTOL_MASTER.png`: Canonical pistol reference.
   - `GUARD_FLASHLIGHT_MASTER.png`: Canonical flashlight reference.
   - `GUARD_COMBAT_REACTIONS_REFERENCE.png`: Takedowns, hurt reactions, and death poses.
2. **Blender 2D Production Scene** (`Assets/Art/Characters/Guard/Blender/`):
   - `Guard_2D_Production.blend`: Orthographic 2D camera, 16-bone anatomical rig, dual studio lighting rig (Key + Cyan Rim), and ground floor reference plane.
3. **Aseprite Source** (`Assets/Art/Characters/Guard/Aseprite/`):
   - `Guard_Idle.aseprite`: Compiled 8-frame native Aseprite source.
4. **Unity Engine Integration** (`Assets/Art/Characters/Guard/`):
   - `Sprites/Guard_Idle_8F.png` + `GUARD_IDLE_01..08.png` (96 PPU, Point filter, Uncompressed).
   - `Animations/Guard_Idle.anim` (12 FPS, 8 keyframes, Loop Time = true).
   - `Animations/Guard.controller` (10 production states: Idle, Walk, Run, Aim, Fire, Reload, Flashlight, Hurt, Death, Takedown).
   - `Prefabs/PF_Guard.prefab` (SpriteRenderer, Animator, Rigidbody2D, CapsuleCollider2D, CharacterController2D, GuardController, WeaponController, SecurityPistol, FlashlightController2D, WeaponAnchor, FlashlightAnchor, GroundCheckPoint, InteractionAnchor).
5. **Quality Assurance & Verification** (`Assets/Art/Characters/Guard/Documentation/`):
   - `GUARD_IDLE_COMPARISON_BOARD.png`
   - `GUARD_SILHOUETTE_BOARD.png`
   - `GUARD_PALETTE_BOARD.png`
   - `GUARD_QA_REPORT.md`
   - In-Engine Screenshots: `Act1_Lab_2D_Screenshot.png` and `Act1_Lab_2D_Guard_Zoom.png`.
