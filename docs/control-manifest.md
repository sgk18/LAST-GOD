# Control Manifest — The Last God (Developer Rules Sheet)

Every script and asset added or modified in *The Last God* must comply with the following constraints:

## 1. Actor Architecture & State Machines (ADR 0001)
- Explicit `enum` per actor (Player: `PlayerState`, Enemy: `EnemyState`).
- Private `_state` field only; NO boolean flags for state (`isJumping`, `isAttacking`, `isDead`).
- State transitions must occur only via `SetState(NewState)`.

## 2. Damage & Combat Contract (ADR 0002)
- All damage interactions must invoke `IDamageable.TakeDamage(int amount, Vector2 knockbackDir)`.
- Use `Health.cs` for health pool, invincibility frames, and `OnDamaged` / `OnDeath` event dispatches.
- Do not write custom HP variables on individual actor or enemy controllers.

## 3. Pixel Pipeline & Parallax (ADR 0003)
- Reference resolution: 240×160.
- PPU = 16 (16 pixels = 1 Unity unit).
- Texture Import Settings: Filter Mode = Point (no filter), Compression = None, Sprite (2D and UI).
- Parallax layers must use camera delta tracking with pixel-snapping in `LateUpdate`.

## 4. Awakening & Ability System (ADR 0004)
- Aeron's awakening is driven by `PlayerState.Awakening`.
- Chronos Aura bullet-slow operates via proximity check (radius 2.5 units, speed lerps down to 25%).
- No reflection or string-based `SendMessage` calls between sequence directors and player components.

## 5. Code Cleanliness & Inspector Tuning
- Single-line XML `<summary>` on every class and interface.
- Zero magic numbers: all speeds, forces, durations, ranges, and cooldowns exposed via `[SerializeField]` with `[Tooltip]`.
- Naming: PascalCase for types/methods/properties, camelCase with `_` prefix for private fields.
