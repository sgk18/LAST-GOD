# CONTEXT — The Last God (Unity 6.5 Prototype)

## 1. Project Summary & Identity
**The Last God** is a dark, atmospheric 2D URP pixel-art action-combat platformer built with an authentic Game Boy Advance (GBA) aesthetic.
- **Reference Viewport**: 240 × 160 native resolution framed by Unity's **Pixel Perfect Camera**.
- **Pixel Grid & PPU**: Strict **16 Pixels Per Unit (PPU)** (16px = 1.0 Unity world unit).
- **Texture Sampling**: **Point (no filter)**, no texture compression, true pixel boundaries without bilinear blurring or sub-pixel shimmering.
- **Tonal Identity**: Merges the haunting atmospheric world design and environmental storytelling of *Hollow Knight* / *Silksong* with the raw, visceral weight and kinetic combat of *God of War: Sons of Sparta*.
- **Narrative Premise**: You are Aeron (Subject A-07), an ancient divine entity awakening from an industrial stasis chamber in a subterranean bio-mechanical research facility. Severed from memories and with divine powers suppressed by laboratory dampeners, Aeron must harness temporal distortion (Chronos Aura) and visceral melee strikes to breach containment.

---

## 2. Agent Skills Policy & Universal Availability
> [!IMPORTANT]
> **Universal Skills Access**: The agent has full authority and capability to utilize **all available skills**—including both local Unity Editor MCP tools and global developer skills. Whenever implementing features, tuning components, inspecting assets, running tests, or diagnosing issues, relevant specialized skills **must** be actively leveraged rather than resorting to manual workarounds or guesswork.

---

## 3. Comprehensive Skills Catalog

The project environment is equipped with a wide array of specialized agent skills categorized below.

### A. Unity MCP Engine & Editor Skills (Local Project Tools)

#### 1. Particle Systems & VFX
- [`particle-system-get`](file:///C:/projects/LAST-GOD/.agent/skills/particle-system-get/SKILL.md): Deep inspection of `UnityEngine.ParticleSystem` components (playing state, particle counts, burst configs, and 24+ modular subsystems: Emission, Shape, Velocity, Noise, Collision, Sub-Emitters, Renderer).
- [`particle-system-modify`](file:///C:/projects/LAST-GOD/.agent/skills/particle-system-modify/SKILL.md): Modify particle system modules (burst rates, start speed/lifetime, shape dimensions, color over lifetime, materials) with targeted serialized diffs.

#### 2. Animation & Animator Controllers
- [`animation-create`](file:///C:/projects/LAST-GOD/.agent/skills/animation-create/SKILL.md): Create empty `AnimationClip` assets at `Assets/...` paths.
- [`animation-get-data`](file:///C:/projects/LAST-GOD/.agent/skills/animation-get-data/SKILL.md): Inspect `AnimationClip` curves, keyframes, events, frame rate, and wrap modes.
- [`animation-modify`](file:///C:/projects/LAST-GOD/.agent/skills/animation-modify/SKILL.md): Apply float curves, sprite reference curves, and animation events to clips.
- [`animator-create`](file:///C:/projects/LAST-GOD/.agent/skills/animator-create/SKILL.md): Create empty `AnimatorController` state machine assets.
- [`animator-get-data`](file:///C:/projects/LAST-GOD/.agent/skills/animator-get-data/SKILL.md): Inspect layers, parameters, states, transitions, blend trees, and conditions.
- [`animator-modify`](file:///C:/projects/LAST-GOD/.agent/skills/animator-modify/SKILL.md): Add/modify animator parameters, states, transitions, exit times, and motion clips.

#### 3. Cinemachine & Camera Control
- [`cinemachine-brain-ensure`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-brain-ensure/SKILL.md): Ensure `CinemachineBrain` exists on the rendering Camera.
- [`cinemachine-camera-create`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-camera-create/SKILL.md): Create virtual cameras (`CinemachineCamera`) in the active scene.
- [`cinemachine-camera-get`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-camera-get/SKILL.md): Inspect virtual camera lens, follow/lookAt targets, and pipeline extensions.
- [`cinemachine-camera-list`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-camera-list/SKILL.md): Enumerate all virtual cameras and identify live cameras.
- [`cinemachine-modify`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-modify/SKILL.md): Generic serialized member modification on Cinemachine components.
- [`cinemachine-set-aim`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-aim/SKILL.md): Configure Aim pipelines (RotationComposer, HardLookAt, PanTilt).
- [`cinemachine-set-body`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-body/SKILL.md): Configure Body positioning (Follow, OrbitalFollow, PositionComposer, HardLock).
- [`cinemachine-set-default-blend`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-default-blend/SKILL.md): Adjust cutscene/gameplay blend curves and transition durations.
- [`cinemachine-set-lens`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-lens/SKILL.md): Set orthographic size (7.5 default), FoV, near/far clipping planes.
- [`cinemachine-set-noise`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-noise/SKILL.md): Apply procedural 2D/3D camera shake (Perlin noise profiles, amplitude, frequency).
- [`cinemachine-set-priority`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-priority/SKILL.md): Manage camera transitions by altering virtual camera priorities.
- [`cinemachine-set-targets`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-set-targets/SKILL.md): Bind or unbind `Follow` and `LookAt` transforms (e.g., Aeron or Boss).
- [`cinemachine-add-extension`](file:///C:/projects/LAST-GOD/.agent/skills/cinemachine-add-extension/SKILL.md): Attach confiners, deoccluders, and impulse listeners.

#### 4. Scene & GameObject Hierarchy Management
- [`scene-create`](file:///C:/projects/LAST-GOD/.agent/skills/scene-create/SKILL.md), [`scene-open`](file:///C:/projects/LAST-GOD/.agent/skills/scene-open/SKILL.md), [`scene-save`](file:///C:/projects/LAST-GOD/.agent/skills/scene-save/SKILL.md), [`scene-get-data`](file:///C:/projects/LAST-GOD/.agent/skills/scene-get-data/SKILL.md), [`scene-list-opened`](file:///C:/projects/LAST-GOD/.agent/skills/scene-list-opened/SKILL.md), [`scene-set-active`](file:///C:/projects/LAST-GOD/.agent/skills/scene-set-active/SKILL.md), [`scene-unload`](file:///C:/projects/LAST-GOD/.agent/skills/scene-unload/SKILL.md).
- [`gameobject-create`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-create/SKILL.md), [`gameobject-destroy`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-destroy/SKILL.md), [`gameobject-duplicate`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-duplicate/SKILL.md), [`gameobject-find`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-find/SKILL.md), [`gameobject-modify`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-modify/SKILL.md), [`gameobject-set-parent`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-set-parent/SKILL.md).
- [`gameobject-component-add`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-component-add/SKILL.md), [`gameobject-component-destroy`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-component-destroy/SKILL.md), [`gameobject-component-get`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-component-get/SKILL.md), [`gameobject-component-list-all`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-component-list-all/SKILL.md), [`gameobject-component-modify`](file:///C:/projects/LAST-GOD/.agent/skills/gameobject-component-modify/SKILL.md).
- [`object-get-data`](file:///C:/projects/LAST-GOD/.agent/skills/object-get-data/SKILL.md), [`object-modify`](file:///C:/projects/LAST-GOD/.agent/skills/object-modify/SKILL.md).

#### 5. Asset Database & Prefab Pipeline
- [`assets-find`](file:///C:/projects/LAST-GOD/.agent/skills/assets-find/SKILL.md), [`assets-find-built-in`](file:///C:/projects/LAST-GOD/.agent/skills/assets-find-built-in/SKILL.md), [`assets-get-data`](file:///C:/projects/LAST-GOD/.agent/skills/assets-get-data/SKILL.md), [`assets-modify`](file:///C:/projects/LAST-GOD/.agent/skills/assets-modify/SKILL.md), [`assets-copy`](file:///C:/projects/LAST-GOD/.agent/skills/assets-copy/SKILL.md), [`assets-move`](file:///C:/projects/LAST-GOD/.agent/skills/assets-move/SKILL.md), [`assets-delete`](file:///C:/projects/LAST-GOD/.agent/skills/assets-delete/SKILL.md), [`assets-create-folder`](file:///C:/projects/LAST-GOD/.agent/skills/assets-create-folder/SKILL.md), [`assets-refresh`](file:///C:/projects/LAST-GOD/.agent/skills/assets-refresh/SKILL.md).
- [`assets-material-create`](file:///C:/projects/LAST-GOD/.agent/skills/assets-material-create/SKILL.md), [`assets-shader-get-data`](file:///C:/projects/LAST-GOD/.agent/skills/assets-shader-get-data/SKILL.md), [`assets-shader-list-all`](file:///C:/projects/LAST-GOD/.agent/skills/assets-shader-list-all/SKILL.md).
- [`assets-prefab-create`](file:///C:/projects/LAST-GOD/.agent/skills/assets-prefab-create/SKILL.md), [`assets-prefab-open`](file:///C:/projects/LAST-GOD/.agent/skills/assets-prefab-open/SKILL.md), [`assets-prefab-save`](file:///C:/projects/LAST-GOD/.agent/skills/assets-prefab-save/SKILL.md), [`assets-prefab-close`](file:///C:/projects/LAST-GOD/.agent/skills/assets-prefab-close/SKILL.md), [`assets-prefab-instantiate`](file:///C:/projects/LAST-GOD/.agent/skills/assets-prefab-instantiate/SKILL.md).

#### 6. Tilemaps & 2D Environment Construction
- [`tilemap-create`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-create/SKILL.md), [`tilemap-list`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-list/SKILL.md), [`tilemap-get`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-get/SKILL.md), [`tilemap-modify`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-modify/SKILL.md), [`tilemap-get-tile`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-get-tile/SKILL.md), [`tilemap-set-tile`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-set-tile/SKILL.md), [`tilemap-box-fill`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-box-fill/SKILL.md), [`tilemap-clear`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-clear/SKILL.md), [`tilemap-set-collider-type`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-set-collider-type/SKILL.md), [`tilemap-set-orientation`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-set-orientation/SKILL.md), [`tilemap-set-tile-flags`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-set-tile-flags/SKILL.md), [`tilemap-create-tile-asset`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-create-tile-asset/SKILL.md), [`tilemap-create-rule-tile`](file:///C:/projects/LAST-GOD/.agent/skills/tilemap-create-rule-tile/SKILL.md).

#### 7. Input System Management
- [`inputsystem-get`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-get/SKILL.md), [`inputsystem-asset-create`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-asset-create/SKILL.md), [`inputsystem-actionmap-add`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-actionmap-add/SKILL.md), [`inputsystem-actionmap-remove`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-actionmap-remove/SKILL.md), [`inputsystem-action-add`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-action-add/SKILL.md), [`inputsystem-action-remove`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-action-remove/SKILL.md), [`inputsystem-binding-add`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-binding-add/SKILL.md), [`inputsystem-binding-composite-add`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-binding-composite-add/SKILL.md), [`inputsystem-binding-remove`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-binding-remove/SKILL.md), [`inputsystem-binding-set`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-binding-set/SKILL.md), [`inputsystem-controlscheme-add`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-controlscheme-add/SKILL.md), [`inputsystem-modify`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-modify/SKILL.md), [`inputsystem-save`](file:///C:/projects/LAST-GOD/.agent/skills/inputsystem-save/SKILL.md).

#### 8. Editor State, Console Logs & Playmode
- [`editor-application-get-state`](file:///C:/projects/LAST-GOD/.agent/skills/editor-application-get-state/SKILL.md): Read editor playmode, paused state, compile state.
- [`editor-application-set-state`](file:///C:/projects/LAST-GOD/.agent/skills/editor-application-set-state/SKILL.md): Toggle playmode to test sequences and physics live.
- [`editor-selection-get`](file:///C:/projects/LAST-GOD/.agent/skills/editor-selection-get/SKILL.md), [`editor-selection-set`](file:///C:/projects/LAST-GOD/.agent/skills/editor-selection-set/SKILL.md): Manage editor object selections.
- [`console-get-logs`](file:///C:/projects/LAST-GOD/.agent/skills/console-get-logs/SKILL.md), [`console-clear-logs`](file:///C:/projects/LAST-GOD/.agent/skills/console-clear-logs/SKILL.md): Inspect and filter Unity debug warnings, exceptions, and assertions.

#### 9. Dynamic Execution & Reflection
- [`script-execute`](file:///C:/projects/LAST-GOD/.agent/skills/script-execute/SKILL.md): Compiles and executes C# snippets on-the-fly via Roslyn inside the Unity Editor.
- [`reflection-method-find`](file:///C:/projects/LAST-GOD/.agent/skills/reflection-method-find/SKILL.md), [`reflection-method-call`](file:///C:/projects/LAST-GOD/.agent/skills/reflection-method-call/SKILL.md): Discover and call internal/private methods across loaded assemblies.
- [`type-get-json-schema`](file:///C:/projects/LAST-GOD/.agent/skills/type-get-json-schema/SKILL.md): Inspect C# reflection models.
- [`script-read`](file:///C:/projects/LAST-GOD/.agent/skills/script-read/SKILL.md), [`script-update-or-create`](file:///C:/projects/LAST-GOD/.agent/skills/script-update-or-create/SKILL.md), [`script-delete`](file:///C:/projects/LAST-GOD/.agent/skills/script-delete/SKILL.md): Write, update, or remove script assets with compilation verification.

#### 10. Visual Inspection & Screenshots
- [`screenshot-game-view`](file:///C:/projects/LAST-GOD/.agent/skills/screenshot-game-view/SKILL.md): Direct framebuffer grab from Game View.
- [`screenshot-scene-view`](file:///C:/projects/LAST-GOD/.agent/skills/screenshot-scene-view/SKILL.md): Capture Scene View camera composition.
- [`screenshot-camera`](file:///C:/projects/LAST-GOD/.agent/skills/screenshot-camera/SKILL.md): Capture output from any specific scene camera.
- [`screenshot-isolated`](file:///C:/projects/LAST-GOD/.agent/skills/screenshot-isolated/SKILL.md): Render isolated GameObject from angles (Front/Right/Composite).

#### 11. Testing & Profiling
- [`tests-run`](file:///C:/projects/LAST-GOD/.agent/skills/tests-run/SKILL.md): Run Unity EditMode and PlayMode automated test suites.
- [`profiler-start`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-start/SKILL.md), [`profiler-stop`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-stop/SKILL.md), [`profiler-get-status`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-get-status/SKILL.md), [`profiler-capture-frame`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-capture-frame/SKILL.md), [`profiler-get-rendering-stats`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-get-rendering-stats/SKILL.md), [`profiler-get-memory-stats`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-get-memory-stats/SKILL.md), [`profiler-get-script-stats`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-get-script-stats/SKILL.md), [`profiler-list-modules`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-list-modules/SKILL.md), [`profiler-enable-module`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-enable-module/SKILL.md), [`profiler-clear-data`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-clear-data/SKILL.md), [`profiler-save-data`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-save-data/SKILL.md), [`profiler-load-data`](file:///C:/projects/LAST-GOD/.agent/skills/profiler-load-data/SKILL.md).

#### 12. Package & Custom Skill Tooling
- [`package-list`](file:///C:/projects/LAST-GOD/.agent/skills/package-list/SKILL.md), [`package-search`](file:///C:/projects/LAST-GOD/.agent/skills/package-search/SKILL.md), [`package-add`](file:///C:/projects/LAST-GOD/.agent/skills/package-add/SKILL.md), [`package-remove`](file:///C:/projects/LAST-GOD/.agent/skills/package-remove/SKILL.md).
- [`unity-tool-list`](file:///C:/projects/LAST-GOD/.agent/skills/unity-tool-list/SKILL.md), [`tool-set-enabled-state`](file:///C:/projects/LAST-GOD/.agent/skills/tool-set-enabled-state/SKILL.md), [`unity-initial-setup`](file:///C:/projects/LAST-GOD/.agent/skills/unity-initial-setup/SKILL.md), [`unity-skill-create`](file:///C:/projects/LAST-GOD/.agent/skills/unity-skill-create/SKILL.md), [`unity-skill-generate`](file:///C:/projects/LAST-GOD/.agent/skills/unity-skill-generate/SKILL.md), [`ping`](file:///C:/projects/LAST-GOD/.agent/skills/ping/SKILL.md).

---

### B. Global Assistant & Engineering Skills
- [`code-review`](file:///C:/Users/Surya%20VM/.gemini/config/skills/code-review/SKILL.md): Parallel multi-axis review checking code standards and spec compliance.
- [`codebase-design`](file:///C:/Users/Surya%20VM/.gemini/config/skills/codebase-design/SKILL.md): Deep-module interface architecture, abstraction layers, and testable seams.
- [`diagnosing-bugs`](file:///C:/Users/Surya%20VM/.gemini/config/skills/diagnosing-bugs/SKILL.md): Systematic scientific loop for tracking down regressions, exceptions, and edge cases.
- [`domain-modeling`](file:///C:/Users/Surya%20VM/.gemini/config/skills/domain-modeling/SKILL.md): Ubiquitous vocabulary alignment, ADR management, and architecture documentation.
- [`tdd`](file:///C:/Users/Surya%20VM/.gemini/config/skills/tdd/SKILL.md): Test-driven red-green-refactor loop for robust game mechanics.
- [`prototype`](file:///C:/Users/Surya%20VM/.gemini/config/skills/prototype/SKILL.md): Rapid spike / prototype generation to validate mechanic feel.
- [`research`](file:///C:/Users/Surya%20VM/.gemini/config/skills/research/SKILL.md): Primary-source investigation and technical discovery.
- [`grilling`](file:///C:/Users/Surya%20VM/.gemini/config/skills/grilling/SKILL.md): Rigorous design challenge and edge-case stress testing.
- [`git-guardrails-claude-code`](file:///C:/Users/Surya%20VM/.gemini/config/skills/git-guardrails-claude-code/SKILL.md): Destructive git operation prevention.
- [`setup-pre-commit`](file:///C:/Users/Surya%20VM/.gemini/config/skills/setup-pre-commit/SKILL.md): Automated quality gates and linting hooks.
- [`scaffold-exercises`](file:///C:/Users/Surya%20VM/.gemini/config/skills/scaffold-exercises/SKILL.md), [`migrate-to-shoehorn`](file:///C:/Users/Surya%20VM/.gemini/config/skills/migrate-to-shoehorn/SKILL.md), [`wizard`](file:///C:/Users/Surya%20VM/.gemini/config/skills/wizard/SKILL.md), [`writing-for-agents`](file:///C:/Users/Surya%20VM/.gemini/config/skills/writing-for-agents/SKILL.md).
- [`antigravity-guide`](file:///C:/Users/Surya%20VM/.gemini/antigravity-cli/builtin/skills/antigravity_guide/SKILL.md), [`agy-customizations`](file:///C:/Users/Surya%20VM/.gemini/antigravity-cli/builtin/skills/agy-customizations/SKILL.md).

---

## 4. Deep Gameplay & Architectural Analysis

### A. Player Actor & State Machine (`PlayerController.cs`)
Located at [`Assets/Scripts/Player/PlayerController.cs`](file:///C:/projects/LAST-GOD/Assets/Scripts/Player/PlayerController.cs).
- **Explicit Enum State Machine**:
  ```csharp
  public enum PlayerState { Idle, Run, Jump, Climb, Attack, Hurt, Dead, Awakening }
  ```
  Strictly avoids boolean soups. Transitions trigger `OnStateChanged(prev, next)` events.
- **Locomotion Tuning**:
  - `moveSpeed = 6.5f` units/s (PPU 16 equivalent = 104 px/s).
  - Frictionless 2D Physics Material prevents catching on vertical walls and catwalk lips.
  - Continuous collision detection with interpolated Rigidbody2D.
- **Vertical Mobility**:
  - Tap vs hold variable jump cut (`jumpForce = 13.5f`, `fallMultiplier = 3.0f`).
  - Double jump support (`maxJumps = 2`, `doubleJumpForce = 12.0f`).
  - 140ms **Coyote Time** and 140ms **Jump Buffering** for crisp, platformer responsiveness.
  - Ladder climbing (`climbSpeed = 4.5f`) and wall sliding (`wallSlideSpeed = 2.0f`).
- **Combat & Evasive Maneuvers**:
  - **Dash / Dodge Roll**: 16.0 units/s dash over 0.22s with 0.25s invincibility frames (i-frames) and `SlideDust` VFX emission at player feet.
  - **Melee Combo System**: 3-stage combo with attack buffering:
    - Strike 1: Quick palm thrust (2 dmg, 1.2 unit range, knockback 2.5).
    - Strike 2: Elbow rush (3 dmg, 1.3 unit range, knockback 3.5).
    - Strike 3: Heavy divine roundhouse / slash (5 dmg, 1.5 unit range, knockback 5.0).
  - **Shield Block**: Defensive guard stance mitigating incoming impact forces.
  - **Dual Input Layer**: Seamlessly supports Unity's New Input System (`PlayerInputActions`) with resilient legacy input fallbacks.

### B. Temporal Dilation: The Chronos Aura
- **Concept**: A proximity time-distortion bubble radiating 2.5 units around Aeron.
- **Implementation**:
  - `chronosAuraVisual`: Rotating glyph ring (`Chronos_Aura_FX.png`) attached to Aeron.
  - Bullet Deceleration: When guard projectiles (`Bullet.cs` / `Laser_Bullet_FX.png`) enter the 2.5-unit radius, their speed is dramatically decelerated by 75% (8.0 units/s down to 2.0 units/s).
  - Gives the player reaction time to jump, parry, or dodge roll through projectile lines.

### C. Prototype Divine Powers Matrix (`PrototypePowerController.cs`)
Located at [`Assets/Scripts/Player/PrototypePowerController.cs`](file:///C:/projects/LAST-GOD/Assets/Scripts/Player/PrototypePowerController.cs).
- **Post-Chamber Stasis Dampening**:
  - Upon shattering out of containment, Aeron's full divine reservoir is dampened by facility suppression fields.
  - **Always Unlocked (Physical Tier)**: `BasicMovement`, `BasicAttack`, `DodgeRoll`, `ShieldBlock`.
  - **Suppressed Divine Powers (Escalation Path)**:
    1. `EnergyWave`: Ranged slash / crescent wave.
    2. `Teleport`: Phase shift / spatial blink.
    3. `MagicShield`: Barrier forcefield.
    4. `TimeSlow`: Global / expanded chrono dilation.
    5. `StealthMode`: Light-bending cloaking.
    6. `PowerBoost`: Overdrive physical damage surge.
    7. `WindPower`: Aerial updraft propulsion.
    8. `EnergyCharge`: Stasis energy siphon.
    9. `LightEmission`: Blinding flash stun.
    10. `SummonAlly`: Phantom manifestation.
    11. `HealthRegeneration`: Nanite biological repair.
    12. `Hacking`: Cybernetic terminal breach.
    13. `Resurrection`: Phoenix reboot protocol.
    14. `AerialCharge`: Downward diving strike.
    15. `BulletDodge`: Matrix-style reflex evasion.
- **Player Feedback**: Attempting a locked power triggers warning audio (`powerLockedSFX`) and renders a customized gold/red framed OnGUI banner notification explaining the dampener reason.

### D. Single Authoritative Damage Pipeline (`IDamageable`)
- Defined in [`Assets/Scripts/Core/IDamageable.cs`](file:///C:/projects/LAST-GOD/Assets/Scripts/Core/IDamageable.cs):
  ```csharp
  public interface IDamageable {
      void TakeDamage(int amount, Vector2 knockbackDir);
      bool IsDead { get; }
  }
  ```
- Backed by [`Health.cs`](file:///C:/projects/LAST-GOD/Assets/Scripts/Core/Health.cs) on all entities (Aeron, Guards, Bringer of Death, Breakables).
- Decouples combat sources from damage targets. Never bypasses this pipeline.

---

## 5. Particle Systems & Visual Effects (VFX) Architecture

Visual effects in **The Last God** reinforce the high-contrast GBA aesthetic, combining crisp pixel sprites with punchy dynamic particle emitters:

| VFX System | Component / Script Location | Visual Behavior & Purpose |
|---|---|---|
| **Glass Chamber Rupture** | `Act1Scene1SequenceManager` / `Act1OriginSceneBuilder` | Box-shaped emitter (1.5 × 2.5), 35–60 burst count, ice/cyan tint (`#9AE6FF`), 0.8s lifetime, velocity 6.0, `stopAction = Destroy`. Shatters when pod breaks. |
| **Stasis Fluid Liquid Burst** | `Act1OriginSceneBuilder` / `StasisChamber.cs` | Sphere burst emitter, 80 particles, high-velocity cyan droplets (`#1ACCE6`), simulates pressurized stasis fluid splashing catwalk. |
| **Chamber Ambient Bubbles** | `Act1OriginSceneBuilder` / `StasisChamber.cs` | Circle emitter at pod bottom, gentle upward continuous drift of translucent cyan micro-bubbles while stasis is intact. |
| **Locomotion Slide Dust** | `dustSpawnPoint` on `PlayerController` | Spawns `SlideDustPrefab` at character footing during Dodge Roll / Dash execution and hard landings. |
| **Chronos Aura Ring** | Child object `Chronos_Aura_FX` on Aeron | Rotating temporal ring with smooth alpha pulsation indicating the 2.5m time-slow zone. |
| **Awakening Surge & Eye Flare** | `AscensionSurge.cs` | Cyan eye-flare flash (`#38BDF8`) and expanding radial shockwave upon stasis release. |
| **Rifle Muzzle Flash** | `GuardWeapon.cs` / `Laser_Bullet_FX` | Brief magenta/red energy muzzle emission when Cyber Guards fire plasma rounds. |
| **Impact Sparks & Blood** | `DamageSystem.cs` / `DarkMagicProjectile.cs` | High-speed spark burst on armored hits, dark crimson splatter on flesh impacts, purple necrotic dissipation on dark magic strikes. |
| **Chest Opening Sparks** | `InteractiveChest.cs` | Golden sparkle burst (`openParticles`) when player touches or interacts with Cainos treasure chests. |
| **Ash Dissolve Death** | `BringerOfDeathAI.cs` | Dissolves enemy sprites into floating dark ash particles upon zero HP. |

---

## 6. Enemy & Encounter AI Architectures

### 1. Cyber Guard (`EnemyAI.cs`)
- **Role**: Frontline facility sentry.
- **States**: `Idle`, `Approach`, `Attack`, `Hurt`, `Dead`.
- **Behavior**: Detects Aeron, advances to tactical distance (5.0 units), fires plasma bullets (`gunshotSFX`), recoils on hit, and permanently remains on the catwalk deck upon death to establish visceral aftermath.

### 2. Bringer of Death (`BringerOfDeathAI.cs`)
- **Role**: Elite boss / shadow harbinger.
- **States**: `Idle`, `Walk`, `MeleeAttack`, `CastSpell`, `Hurt`, `Dead`.
- **Behavior**:
  - Hovering advance towards player.
  - Multi-hit scythe cleave combo at close range (`meleeDamage = 3`).
  - Mid-range spellcast firing homing necrotic orbs (`DarkMagicProjectile.cs`).
  - Hurt flash reaction (`hurtFlashColor = #FF4D4D`).
  - Ash-dissolve death sequence with dedicated SFX.

### 3. Interactive Dummy & Environmental Hazards
- **Interactive Dummy (`InteractiveDummy.cs`)**: Non-lethal training target with sine-wave rotational spring wobble responding to melee impacts.
- **Hazard Spikes (`HazardSpike.cs`)**: Environmental floor traps dealing damage and upward knockback (`knockbackDirection = (0, 1)`) with cooldown throttling.

---

## 7. Audio & Soundscape Pipeline

- **Aesthetic**: Gritty, low-pass filtered, bitcrushed 8-bit / 16-bit sound design reflecting the industrial cyberpunk atmosphere.
- **Key Audio Assets**:
  - Ambient Hum (`machine_hum.wav`): Looping low industrial drone.
  - Heartbeat (`heartbeat.wav`): Rhythmic low-pass pulse setting dramatic tension.
  - Glass Shatter (`glass_shatter.wav`): Crisp, explosive crunch.
  - Alarm Siren (`alarm.wav`): Bitcrushed klaxon loop.
  - Combat Slashes (`slash.wav`, `slash1`, `slash2`, `slash3`): Varied blade/fist impacts.
  - Projectile & Gunfire (`gunshot.wav`): Heavy pulsed laser discharge.
  - UI Audio Feedback: Distinct stasis dampener lock and unlock chimes.

---

## 8. Act 1 Scene 1 (INT. LAB – NIGHT) Vertical Slice Flow

Orchestrated by [`Act1Scene1SequenceManager.cs`](file:///C:/projects/LAST-GOD/Assets/Scripts/Core/Act1Scene1SequenceManager.cs):
1. **Cold Open**: Pitch darkness, looping ambient machine hum, 2 spaced thumping heartbeat pulses.
2. **The Voice (V.O.)**: Subtitle typewriter display: *"WAKE UP."* → *"YOU WERE NOT MADE TO SLEEP."*
3. **Chamber Rupture**: Background lights flicker (`AmbientBackgroundFlicker`), pod glass cracks (`Chamber_Cracked`), violently shatters (`Chamber_Shattered`) with 35+ glass shard particles, glass shatter SFX, screen shake (`CameraShake2D`), and red alarm strobe.
4. **Awakening**: Aeron emerges, eyes flare cyan, Chronos Aura ring ignites, player controls unlock.
5. **Tactical Guard Breach**: 2 Cyber Guards spawn on catwalk wings, open fire. Bullets visibly slow to 25% speed within Aeron's aura. Aeron eliminates guards with 3-hit melee strikes; guard corpses stay grounded.
6. **Resolution & Aftermath**: Controls freeze, klaxon halts, camera frames Aeron staring at glowing hands. The Voice speaks: *"THEY WILL FEAR YOU."* → *"THEY SHOULD."* → Fade to black.

---

## 9. Folder Structure & Directory Governance

```
Assets/
├── Art/
│   ├── Backgrounds/      ← Layer1 Far, Layer2 Mid, Layer3 Fore, Solid Dark Backdrop
│   ├── Sprites/          ← Character sheets, tilesets, overlays (PPU=16, Point, No compression)
│   ├── Animations/       ← Animation clips & Animator Controllers
│   └── Tilemaps/         ← Tile assets and Palette definitions
├── Audio/
│   └── AudioClips/       ← 8-bit / bitcrushed WAV files (heartbeat, hum, shatter, alarm, etc.)
├── Characters/
│   └── Aeron/            ← Specific Aeron rigs, textures, animations, scripts
├── Prefabs/
│   ├── Player/           ← Aeron player prefabs & aura attachments
│   ├── Enemies/          ← Cyber Guards, Bringer of Death, projectiles
│   └── Environment/     ← Glass chamber, monitors, catwalks, hazards
├── Scenes/               ← Act1_Scene1.unity, TestScene.unity
├── Scripts/
│   ├── Core/             ← IDamageable.cs, Health.cs, CutsceneUIController.cs, Act1Scene1SequenceManager.cs
│   ├── Player/           ← PlayerController.cs, PrototypePowerController.cs, CameraFollow.cs
│   ├── Combat/           ← Bullet.cs, DarkMagicProjectile.cs, HazardSpike.cs, InteractiveChest.cs
│   └── Enemies/          ← EnemyAI.cs, BringerOfDeathAI.cs
└── Settings/
    └── Input/            ← PlayerInputActions.inputactions
```

### Architectural Rules:
1. **No Monolithic Boolean Soup**: Always use explicit enum state machines (`PlayerState`, `EnemyState`, `BringerState`).
2. **Universal Damage Protocol**: Always route damage through `IDamageable.TakeDamage()`.
3. **Particle Hygiene**: Particle bursts during one-shot events must set `main.stopAction = ParticleSystemStopAction.Destroy` or reuse pooled emitters.
4. **Cinemachine First**: Camera tracking and screen shakes must leverage Cinemachine 3.x and Pixel Perfect Camera framing.
5. **PPU Integrity**: All art imports must remain 16 PPU, Point filter, No compression.
