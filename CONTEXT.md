# CONTEXT — The Last God (Unity 6.5 Prototype)

## Project Summary
**The Last God** is a 2D URP pixel-art combat platformer prototype built with a GBA-authentic aesthetic (240×160 reference resolution, Pixel Perfect Camera, 16 Pixels Per Unit, Point/no-filter texture sampling). Tonally inspired by the atmospheric world design of *Hollow Knight* / *Silksong* and the raw, visceral weight of *God of War: Sons of Sparta*, the project is a scoped-down vertical slice executing a multi-act script.

---

## Current Scope & Status (Single Source of Truth)

### ✅ Built So Far
- **Foundation Core & Architecture**:
  - `PlayerController`: Single-enum state machine (`Idle`, `Run`, `Jump`, `Climb`, `Attack`, `Hurt`, `Dead`, `Awakening`), horizontal movement, variable jump height, dash with i-frames, facing direction flip, and 3-hit melee combo hit detection.
  - `Chronos Aura & Awakening`: `TriggerAwakening()` initiates cinematic surge, cyan eye flare, rotating `Chronos_Aura_FX` ring visual, temporal bullet deceleration aura, and unlock of `PrototypePowerType.TimeSlow`.
  - `IDamageable` & `Health`: Decoupled health pool component with `OnDamaged(int)` and `OnDeath` UnityEvents, knockback impulse logic.
  - `PlayerInputActions`: New Input System integration configured for Gamepad and Keyboard.
  - `CameraFollow` & `CameraShake2D`: Pixel-perfect camera tracking and screen-shake system.
  - Architecture Records: Formal ADRs accepted (`0001` through `0004`) in `docs/adr/`.
- **Act 1, Scene 1 (INT. LAB – NIGHT) Vertical Slice**:
  - Canonical GDD: [`design/gdd/gdd-act1-scene1.md`](file:///C:/projects/LAST-GOD/design/gdd/gdd-act1-scene1.md).
  - Art Bible: [`design/art/art-bible.md`](file:///C:/projects/LAST-GOD/design/art/art-bible.md).
  - `Act1_Scene1.unity`: Constructed via `Act1Scene1Builder.cs` with catwalk deck at visual floor Y = -17.10, glass chamber at Pillar B-3, Aeron starting at X = -31.5, 2 Cyber Guards at X = -14.0 and X = -6.0, solid opaque bedrock backdrop with ambient flickering laboratory interior (`frame-1.png` / `frame-2.png`), camera follow + shake, and Cutscene UI Canvas.
  - `Act1Scene1SequenceManager`: Master coroutine director driving the complete 6-step vertical slice flow:
    1. Pitch darkness, ambient machine hum loop, thumping heartbeat audio pulses.
    2. V.O. Dialogue: *"WAKE UP."* → *"YOU WERE NOT MADE TO SLEEP."* (typewriter text via TextMeshPro).
    3. Chamber rupture: glass cracks → shatters with glass shard particle burst, glass shatter SFX, blaring alarm siren, red light flicker, screen shake.
    4. Awakening & Power Unlock: Aeron triggers `PlayerState.Awakening`, cyan eye flare pulses, Chronos Aura ring emerges, temporal bullet deceleration active, controls unlock into `PlayerState.Idle`.
    5. Tactical Guard Encounter: 2 Cyber Guards approach using `EnemyAI.cs`, firing plasma laser bullets. Bullets smoothly decelerate to 2.0 speed inside Aeron's Chronos Aura. Aeron defeats guards with 3-hit melee combo; guard bodies stay on ground upon death.
    6. Scene Resolution: Input locks on Aeron (`LockControls(true)`), alarm klaxon stops, concluding V.O. dialogue: *"THEY WILL FEAR YOU."* → *"THEY SHOULD."* → Fade to black.

### ⏳ Not Built Yet
- **Act 1, Scene 2**: Lab Corridor escape sequence.
- **Act 1, Scene 3**: Elia encounter.
- **Act 1, Scene 4**: The Wrath boss fight.
- **Acts 2 & 3**: Remaining story beats, extended combat skills, and level areas.

---

## Folder Structure Reference

```
Assets/
├── Art/
│   ├── Sprites/          ← Character sheets, tilesets, overlays (PPU=16, Point filter, No compression)
│   ├── Animations/       ← Animation clips & Animator Controllers
│   └── Tilemaps/         ← Tile assets and Palette definitions
├── Audio/
│   └── AudioClips/       ← 8-bit / bitcrushed WAV files (heartbeat, hum, shatter, alarm, etc.)
├── Prefabs/
│   ├── Player/           ← Aeron player prefabs
│   ├── Enemies/          ← Guard enemy prefabs & projectiles
│   └── Environment/     ← Chamber, room objects, props
├── Scenes/               ← Scene files (Act1_Scene1.unity, TestScene.unity)
├── Scripts/
│   ├── Core/             ← IDamageable.cs, Health.cs, CutsceneUIController.cs, Act1Scene1SequenceManager.cs
│   ├── Player/           ← PlayerController.cs, CameraFollow.cs, CameraShake2D.cs
│   ├── Combat/           ← Bullet.cs, Hitboxes, Hurtboxes
│   └── Enemies/          ← EnemyAI.cs, Boss controllers
└── Settings/
    └── Input/            ← PlayerInputActions.inputactions
```

### File Placement Rules:
- Put all shared interfaces, managers, and base utilities in `Scripts/Core/`.
- Put player-specific movement, camera, and input scripts in `Scripts/Player/`.
- Put weapon, bullet, hitbox, and damage scripts in `Scripts/Combat/`.
- Put enemy and boss AI scripts in `Scripts/Enemies/`.
- Always verify PPU=16 and Point (no filter) texture settings on newly added art assets in `Art/`.
