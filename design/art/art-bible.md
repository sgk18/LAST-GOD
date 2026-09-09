# Art Bible — The Last God

## 1. Visual Pillars
- **GBA Authenticity**: Rendered within a 240×160 native viewport reference at 16 Pixels Per Unit (PPU). Clean pixel boundaries without bilinear blur or sub-pixel shimmering.
- **Painterly "Sons of Sparta" Treatment**: Gritty, atmospheric, high-density detailing. Dark, industrial sci-fi aesthetic with heavy silhouettes, textural grime, cable clutter, and metallic paneling.
- **Focused Color Temperature & Saturated Accents**:
  - **Dominant Base (80%)**: Cold, desaturated slate blues (`#0f172a`), deep gunmetal grey (`#1e293b`), and near-black backdrop shadows (`#020617`).
  - **Hero Accent (10%)**: Ethereal glowing cyan (`#38bdf8` / `#06b6d4`) on Aeron's eyes, arm runes, and the stasis fluid.
  - **Hazard Accent (10%)**: Urgent crimson red (`#ef4444`) on alarm beacons, security strobes, and blood/damage impacts.

---

## 2. Character Rendering Specifications
### Aeron (The Awakening God / A-07)
- **Visual Description**: Barefoot, tattered dark containment suit, cybernetic collar with loose severed conduits, pale skin with dark disheveled hair. A single piercing cyan eye-glow shines through the shadows. Bandaged right arm inscribed with ancient divine circuitry.
- **Sprite Dimensions**: Standard 32×48 pixel canvas (or 64×64 frame box with bottom-centered 16px baseline margin).
- **Pivot**: Bottom center `{x: 0.5, y: 0.0227}` matching ground contact.
- **Required Animations**:
  - `Awaken`: Stasis rupture frame, eyes snapping open with cyan lens flare.
  - `Idle`: 8 frames, gentle 10 FPS ping-pong breathing cycle, capelet/hair micro-sway.
  - `Walk`: 8 frames, grounded low-center-of-gravity tactical advance.
  - `Attack`: 3-frame combo (unarmed palm strike, elbow rush, cyan-imbued roundhouse/slash).
  - `Hurt`: Recoil/flinch frame with cyan particle spark.
  - `PostFight`: Somber silhouette staring at glowing hands.

### Laboratory Guard (Facility Security Sentry)
- **Visual Description**: Heavy armored tactical hazmat armor, angular dark navy visor with faint orange HUD indicator, compact energy pulse rifle with sling, magnetic combat boots.
- **Sprite Dimensions**: Standard 32×48 pixel canvas.
- **Pivot**: Bottom center `{x: 0.5, y: 0.0}`.
- **Required Animations**:
  - `Idle`: Stately alert stance, weapon held at ready.
  - `Walk`: Deliberate forward march.
  - `Attack`: Rifle aimed with muzzle flash.
  - `Hurt`: Flinch backward with spark/armor chip.
  - `Death`: Permanent collapse onto ground.

---

## 3. Environment & Props
- **Chamber Pod**: Vertical glass stasis cylinder with heavy metallic hydraulic cap and base dais. Intact, fractured/cracked, and shattered glass states.
- **Lab Catwalk**: Heavy industrial grating with yellow/black hazard trim, handrails, computer monitors with glowing terminal displays.
- **Lighting**: 2D Point and Global lights casting moody chiaroscuro contrast.

---

## 4. 3D Asset Standards (True 2.5D Pipeline)

### Poly Budgets
- **Character Models**: `3,000 – 6,000` triangles per character model (stylized low-poly, clean silhouette-focused topology, not high-fidelity).
- **Environment Block-Out**: `1,500 – 3,000` triangles for the minimal laboratory room block-out geometry.

### Texture Resolution & Packing
- **Characters**: `1024×1024` per character:
  - `[Character]_Albedo.png`: Base color and accent highlights.
  - `[Character]_RoughMetal.png`: Single packed map (Metallic / Occlusion / Detail / Smoothness).
  - Normal maps: None required for this prototype pass.
- **Environment**: `2048×2048` shared Trim Sheet texture (`Lab_TrimSheet.png`) covering wall panels, floor grating, and pillar fluting.

### Material & Surface Language
- **Shader Model**: Unity Universal Render Pipeline (URP) `Universal Render Pipeline/Lit`.
- **Smoothness**: Kept low (`0.1 – 0.3`) across all materials — creating a tactile, matte, industrial, non-reflective surface language.
- **Color Palette & Accents**:
  - Cold desaturated base tones:
    - Greys: `#3A3F47`
    - Dark blues: `#1B2430`
    - Near-black shadows: `#0A0C10`
  - Exactly one distinctive accent color per character:
    - **Aeron**: Pale cyan-white glow `#CFF4FF` at the eyes only.
    - **Guard**: Dim amber/red accent `#C4502E` on a status light or chest marking to visually differentiate threat from hero at a glance.

