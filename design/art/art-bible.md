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
