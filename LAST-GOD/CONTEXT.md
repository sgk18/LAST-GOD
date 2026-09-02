# CONTEXT — The Last God (Unity 6.5 Prototype)

## Project Summary
**The Last God** is a 2D URP pixel-art combat platformer prototype built with a GBA-authentic aesthetic (240×160 reference resolution, Pixel Perfect Camera, 16 Pixels Per Unit, Point/no-filter texture sampling). Tonally inspired by the atmospheric world design of *Hollow Knight* / *Silksong* and the raw, visceral weight of *God of War: Sons of Sparta*, the project is a scoped-down vertical slice executing a multi-act script.

---

## Current Scope & Status (Single Source of Truth)

### ✅ Built So Far
- **Foundation Core**:
  - `PlayerController`: Enum state machine (`Idle`, `Run`, `Jump`, `Attack`, `Hurt`, `Dead`), horizontal movement, variable height jump, dash with i-frames, facing direction, and melee combat hit detection.
  - `IDamageable` & `Health`: Decoupled health pool component with `OnDamaged(int)` and `OnDeath` UnityEvents, knockback impulse logic.
  - `PlayerInputActions`: New Input System integration configured for Gamepad and Keyboard.
  - `CameraFollow` & `CameraShake2D`: Pixel-perfect camera tracking and screen-shake system.
- **Act 1, Scene 1 (INT. LAB – NIGHT) Playable Beat**:
  - `Act1_Scene1.unity`: Scene setup with 240x160 resolution, dark lab room geometry, center glass chamber, and spawn points.
  - `Act1Scene1SequenceManager`: Master coroutine director driving the opening flow.
  - **Sequence Flow**:
    1. Pitch darkness, ambient machine hum loop, thumping heartbeat audio.
    2. V.O. Dialogue: *"WAKE UP."* → *"YOU WERE NOT MADE TO SLEEP."* (typewriter text).
    3. Chamber rupture: glass cracks → shatters with particle burst, glass shatter SFX, blaring alarm siren, red light flicker, screen shake.
    4. Aeron prefab spawns, camera locks on Aeron, player input unlocks.
    5. 2–3 Guard enemies spawn using `EnemyAI` (`Idle`, `Approach`, `Attack`, `Hurt`, `Dead`).
    6. Guards fire `Bullet` projectiles that slow down near Aeron ("bullets slow near Aeron").
    7. Raw instinctive combat: player defeats guards, guard bodies stay on ground upon death.
    8. Input locks on final death, camera holds on Aeron.
    9. Ending V.O. Dialogue: *"THEY WILL FEAR YOU."* → *"THEY SHOULD."* → Cut to black.

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
