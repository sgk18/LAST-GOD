# THE LAST GOD — ACT 1: THE LABORATORY
## 2D Platformer Visual Bible & Production Standards

### 1. Vision & Core Philosophy
The laboratory in Act 1 (*Origin*) is not merely scenery; it is a primary storytelling engine.
- **Narrative Premise**: Aeron (Subject A-07) awakens inside an underground, black-budget research facility. The laboratory represents human scientific hubris attempting to synthesize and control divine power.
- **Atmospheric Arc**:
  - *Immediate impression*: Advanced, expensive, sterile, functional, clinical engineering.
  - *Prolonged observation*: Chilling, unnatural, containment fractures, biological restraint armatures, hurried evacuations, and decaying artificial life support.
- **Stylistic Identity**:
  - Hand-crafted 2D atmospheric platformer.
  - Dark sci-fi and industrial horror.
  - Painterly pixel art with strong geometric silhouettes.
  - Deep multi-plane parallax with cold, selective cinematic lighting.
  - **NOT** neon cyberpunk. **NOT** fantasy dungeon. **NOT** photorealistic. **NOT** a Hollow Knight clone.

---

### 2. Master Color System & Hierarchy

#### Palette Swatches
- **DARK BASE**: `#080B0F` — Deep void, occluded corners, background recesses.
- **DEEP CHARCOAL**: `#11161C` — Heavy structural framing, bulkheads, armor plating.
- **DARK BLUE-GREY**: `#1B2430` — Wall cladding, secondary structural silhouettes, ambient shadow.
- **INDUSTRIAL GREY**: `#303841` — Floors, platforms, machinery chassis, conduits.
- **LIGHT METAL**: `#4A535C` — Walkway gratings, handrails, worn metal edges, highlight rims.
- **DARK CYAN**: `#245B70` — Stasis fluid volume, powered-down conduits, sub-surface glow.
- **PRIMARY CYAN**: `#6FE3FF` — Active stasis fields, diagnostic terminals, energized power lines.
- **BRIGHT CYAN**: `#CFF4FF` — High-voltage electrical arcs, central stasis core, specular peaks.
- **WARNING ORANGE**: `#C4502E` — Hazard latches, airlock indicators, emergency valves.
- **INDUSTRIAL YELLOW**: `#B39A45` — Muted floor hazard stripes, restriction stencils.

#### Distribution Rule
- **60% Dark Neutral** (`#080B0F`, `#11161C`) — Establishes oppressive gloom and negative space.
- **20% Dark Blue-Grey** (`#1B2430`) — Shapes the architectural enclosure.
- **10% Industrial Grey & Light Metal** (`#303841`, `#4A535C`) — Identifies walkable platforms and mechanical details.
- **7% Cyan Accent** (`#245B70`, `#6FE3FF`, `#CFF4FF`) — Denotes synthetic divine power and active tech.
- **3% Warning / Accent** (`#C4502E`, `#B39A45`) — Directs attention to hazards and access thresholds.

---

### 3. Pixel & Technical Specifications
- **Base Native Resolution**: `384 × 216` (16:9 widescreen, integer 5x to 1080p, 10x to 4K).
- **Pixels Per Unit (PPU)**: `32`.
- **Tile Unit**: `32 × 32` pixels (1 meter × 1 meter in Unity coordinates).
- **Camera Orthographic Size**: `3.375` (`216 / (2 * 32) = 3.375`).
- **Texture Import**:
  - `Sprite Mode`: Single / Multiple
  - `Filter Mode`: Point (no filter)
  - `Compression`: None / High Quality uncompressed
  - `Generate Mip Maps`: Disabled.

---

### 4. Layered Environmental Composition & Parallax

| Layer | Sorting Layer | Parallax X | Visual Role | Contrast / Readability |
|---|---|---|---|---|
| **0: Far Background** | `Background_Far` | 0.15 | Distant structural silos, massive cooling ducts, deep darkness | Low contrast, deep charcoal `#11161C` |
| **1: Background** | `Background` | 0.40 | Structural walls, heavy pillars (A-7, B-3), conduit runs | Muted blue-grey `#1B2430` |
| **2: Midground** | `Midground` | 0.75 | Secondary containment units, server towers, consoles, piping | Medium detail, selective occlusion |
| **3: Gameplay** | `Gameplay` | 1.00 | Walkways, platforms, floor gratings, **Hero Stasis Chamber** | Sharp silhouettes, high edge contrast |
| **4: Player** | `Player` | 1.00 | Temporary silhouette proxy / future player character | Protected negative space behind avatar |
| **5: Foreground** | `Foreground` | 1.25 | Massive descending conduits, structural ceiling ribs | Framing only; never occludes platforms |
| **6: FX / Lighting** | `Foreground_FX` | 1.10 | Steam vents, sparks, dust motes, cyan volumetric rays | Subtly layered depth enhancement |

---

### 5. Hero Focal Point: The Aeron Stasis Chamber
- **Dimensions**: ~2 tiles wide × 3 tiles high (64 × 96 pixels).
- **Structure**:
  1. *Base & Crown*: Heavy industrial alloy collars clamped into the floor and ceiling conduits.
  2. *Chamber*: Reinforced transparent cylindrical tube with internal fluid meniscus.
  3. *Core*: Soft, pulsating cyan stasis energy (`#6FE3FF` to `#CFF4FF`), trailing thin energetic filaments.
- **Narrative Purpose**: Ground zero for Act 1. It must look like an exorbitantly expensive, military-funded containment apparatus built to restrain something far stronger than a mortal man.
