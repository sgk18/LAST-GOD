# ADR 0002: Shared IDamageable Combat Contract

## Status
Accepted

## Context
Combat interactions across players, guards, hazards, and breakable environment objects need a decoupled, unified contract to avoid circular dependencies between combat modules, actor scripts, and projectile classes.

## Decision
All damage-dealing scripts must communicate exclusively through the `IDamageable` interface:
```csharp
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 knockbackDir);
    bool IsDead { get; }
}
```
Actors attach the modular `Health.cs` component to manage HP pools, invulnerability frames, and `OnDamaged` / `OnDeath` event dispatches. No script may implement custom HP fields or bypass `IDamageable`.

## Consequences
- **Positive**: Complete decoupling between attackers and targets; bullets and melee hitboxes can hit players, guards, or breakable props without knowing their concrete classes; zero circular assembly dependencies.
- **Negative**: Knockback physics calculations must accept unified parameters.
