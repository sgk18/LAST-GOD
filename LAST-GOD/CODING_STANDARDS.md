# CODING STANDARDS — The Last God

All code written for **The Last God** must strictly adhere to the following standards.

---

## 1. State Machines (No Boolean Soup)
- Every actor with distinct behavioral states (Player, Enemy, Boss, Cutscene Director) **must** use a single explicit `enum` and a single `_state` field.
- **Never** use boolean flags for state (`isJumping`, `isAttacking`, `isHurt`, `isDead`, `isFalling`).
- State transitions must occur via an explicit `SetState(NewState)` method to fire transition events (`OnStateChanged`) and execute enter/exit logic cleanly.

```csharp
// CORRECT
public enum EnemyState { Idle, Approach, Attack, Hurt, Dead }
private EnemyState _state = EnemyState.Idle;

private void SetState(EnemyState next)
{
    if (_state == next) return;
    EnemyState prev = _state;
    _state = next;
    OnStateChanged?.Invoke(prev, next);
}
```

---

## 2. Shared Damage Interface (`IDamageable`)
- **`IDamageable` is the single authoritative damage pipeline in the project.**
- Every damageable entity (Player, Guards, Bosses, Breakables) must use `Health.cs` or implement `IDamageable`.
- **Never** write custom damage or HP handling logic on individual actor scripts.
- Attacks must invoke `IDamageable.TakeDamage(int amount, Vector2 knockbackDir)`.

```csharp
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 knockbackDir);
    bool IsDead { get; }
}
```

---

## 3. Naming Conventions
- **PascalCase**: Classes, Structs, Enums, Interfaces (`IDamageable`), Methods (`TakeDamage`), Public Fields, Properties (`CurrentHP`), Events (`OnDamaged`).
- **camelCase with Leading Underscore (`_`)**: Private and Protected member fields (`_health`, `_rb`, `_state`).
- **camelCase**: Method parameters (`amount`, `knockbackDir`) and local variables (`distToPlayer`).
- **NO Hungarian Notation**: Avoid prefixes like `m_pHealth`, `iDamage`, `strName`.

---

## 4. Single Responsibility & XML Documentation
- Every `MonoBehaviour`, `ScriptableObject`, and `Interface` **must** include a one-line XML summary describing its single responsibility:

```csharp
/// <summary>
/// Controls horizontal movement, variable jumps, and state machine for Aeron.
/// </summary>
public class PlayerController : MonoBehaviour
```

> ⚠️ **Rule of Thumb**: If you cannot describe a script's sole responsibility in one sentence, the script is doing too much and must be refactored into smaller components.

---

## 5. No Magic Numbers in Gameplay Code
- All speed, force, duration, damage, range, and cooldown values **must** be exposed via `[SerializeField]` with a helpful `[Tooltip]`.
- No hardcoded numbers inside `Update()`, `FixedUpdate()`, or collision handlers.

```csharp
// INCORRECT
if (dist < 5.0f) { _rb.velocity = dir * 3.5f; }

// CORRECT
[Header("Movement")]
[Tooltip("Movement speed in Unity units/sec. At PPU=16, ~3.5 = Guard pace.")]
[SerializeField] private float moveSpeed = 3.5f;
[Tooltip("Distance at which the enemy initiates an attack.")]
[SerializeField] private float attackRange = 5.0f;
```

---

## 6. Composition Over Monoliths
- Build small, decoupled components that communicate via references or C#/Unity events (`Health`, `PlayerController`, `CameraShake2D`, `CutsceneUIController`).
- Avoid creating giant "God Objects" that control physics, health, UI, and audio in a single file.

---

## 7. Pixel-Pipeline Safety
- **Pixel Scale**: All physics and movement values must target **PPU = 16** (16 pixels = 1 Unity unit).
- **Pixel Snapping**: Never set sub-pixel offsets manually in code. Rely on **Pixel Perfect Camera** snapping.
- **Import Rules**: Texture imports must strictly maintain:
  - **Filter Mode**: Point (no filter)
  - **Compression**: None
  - **Pixels Per Unit**: 16
