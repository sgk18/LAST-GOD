# The Last God — 2D Pixel-Art Combat Platformer

GBA-authentic combat platformer prototype in the style of Hollow Knight / Silksong.
Built with **Unity 6.5 (6000.5.x) · Universal 2D (URP) template · C# · New Input System**.

---

## Repo Structure

```
LAST-GOD/                        ← git repo root
├── .gitignore
├── README.md
└── LAST-GOD/                    ← actual Unity project (open THIS in Unity Hub)
    ├── Assets/
    │   ├── Art/
    │   │   ├── Sprites/
    │   │   ├── Animations/
    │   │   └── Tilemaps/
    │   ├── Prefabs/
    │   │   ├── Player/
    │   │   └── Environment/
    │   ├── Scenes/              ← create TestScene.unity here
    │   ├── Scripts/
    │   │   ├── Core/            IDamageable.cs, Health.cs
    │   │   ├── Player/          PlayerController.cs, CameraFollow.cs
    │   │   ├── Combat/          (Prompt 2)
    │   │   └── Enemies/         (Prompt 3)
    │   └── Settings/
    │       └── Input/           PlayerInputActions.inputactions
    ├── Packages/
    │   └── manifest.json        ← Pixel Perfect + Input System already added
    └── ProjectSettings/
        ├── Physics2DSettings    ← Gravity Y = -30
        └── TagManager           ← Ground (8), Player (9) layers added
```

> **Open in Unity Hub**: Add → Browse → select `LAST-GOD/LAST-GOD/` (the inner folder).

---

## Project Setup — What's Already Done

These settings are **already baked into the repo files** — you don't need to do them manually:

| Setting | Value | File |
|---|---|---|
| Gravity Y | **-30** | `ProjectSettings/Physics2DSettings.asset` |
| Layer 8 | **Ground** | `ProjectSettings/TagManager.asset` |
| Layer 9 | **Player** | `ProjectSettings/TagManager.asset` |
| 2D Pixel Perfect package | **5.2.0** | `Packages/manifest.json` |
| Input System package | **1.20.0** | `Packages/manifest.json` |

---

## Remaining Manual Steps (one-time, in the Unity Editor)

### Step 1 — Enable New Input System backend
On first open, Unity will ask to switch to the New Input System — **accept & restart**.

### Step 2 — Generate Input Actions C# class
- Click `Assets/Settings/Input/PlayerInputActions.inputactions`
- Inspector → tick **Generate C# Class**
- Namespace: `LastGod.Input` (optional)
- Click **Apply** → Unity generates `PlayerInputActions.cs`

### Step 3 — Add Pixel Perfect Camera component
On **Main Camera** in the scene:
- Add Component → **Pixel Perfect Camera**
- Reference Resolution: **240 × 160**
- **Upscale Render Texture**: ✅
- **Pixel Snapping**: ✅

### Step 4 — Sprite Import Defaults
**Edit → Project Settings → Editor → Default Texture Settings:**

| Setting | Value |
|---|---|
| Filter Mode | **Point (no filter)** |
| Compression | **None** |
| Pixels Per Unit | **16** |
| Generate Mip Maps | **Off** |

> ⚠️ Every sprite imported into this project MUST match these settings. Point filter prevents the bilinear blur that destroys pixel art.

### Step 5 — Create TestScene
1. `File → New Scene → Basic 2D` → Save as `Assets/Scenes/TestScene.unity`
2. **Ground floor**: Create Empty → Add `BoxCollider2D` → Scale `(30,1,1)` → Position `(0,-3,0)` → Layer: **Ground**
3. **Player**: Create Empty → Add `Rigidbody2D` (Dynamic, Freeze Rotation Z), `CapsuleCollider2D` (Vertical, size 0.5×1), `Health`, `PlayerController` → set Ground Layer mask → drag to `Prefabs/Player/Player.prefab`
4. **Main Camera**: Add `Pixel Perfect Camera` (see Step 3) + `CameraFollow` → assign Player as Target

### Step 6 — Play!
| Key | Gamepad | Action |
|---|---|---|
| A / D | Left Stick | Run |
| Space | South (A/Cross) | Jump |
| J | West (X/Square) | Attack (debug log) |
| Left Shift | East (B/Circle) | Dash |

---

## Architecture

```
PlayerController  ──state machine──►  PlayerState { Idle, Run, Jump, Attack, Hurt, Dead }
      │
      ├── reads   PlayerInputActions   (New Input System)
      ├── owns    Rigidbody2D · CapsuleCollider2D
      └── uses    Health (IDamageable)
                    ├── event OnDamaged(int remainingHP)
                    └── event OnDeath
```

## Coding Conventions
- **Namespaces**: `LastGod.Core` · `LastGod.Player` · `LastGod.Combat` · `LastGod.Enemies`
- **One state field**: `PlayerState _state` — no boolean soup
- **Events over polling**: UnityEvent / C# events for cross-component comms
- **PPU = 16**: all physics constants tuned to this scale
- **Assembly definitions**: four separate `.asmdef` files enforcing layered dependencies

---

## Roadmap
| Prompt | Feature |
|---|---|
| ✅ **1** | Foundation — pixel pipeline, player controller, health system |
| **2** | Combat hitboxes, attack animation, hit-stop |
| **3** | EnemyBase class, first patrol/charge enemy |
| **4** | Tilemap level geometry, camera bounds |
| **5** | Juice — screen shake, particles, SFX hooks |
