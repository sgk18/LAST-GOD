# ADR 0004: Awakening Sequence and Power Unlock State Integration

## Status
Accepted

## Context
When Aeron breaks out of the stasis chamber in Act 1 Scene 1, he experiences an awakening surge that unlocks his first divine ability (Chronos Aura). Bolting this on as a separate manager or ad-hoc reflection/`SendMessage` controller creates brittle wiring, timing bugs, and breaks state invariants.

## Decision
The awakening sequence and power unlock are integrated directly into `PlayerController` as a formal state: `PlayerState.Awakening`.
1. During `Awakening`, gameplay input is strictly gated.
2. The controller plays the awakening animation/surge, activates the cyan eye glow / aura VFX, and enables Chronos Aura passive bullet deceleration.
3. Upon completion of the sequence (or callback from cutscene director), Aeron transitions cleanly to `PlayerState.Idle`, unlocking player locomotion and melee combat.

## Consequences
- **Positive**: Strongly typed lifecycle; no race conditions where the player moves during the cinematic awakening; zero reflection or `SendMessage` coupling.
- **Negative**: Adds a dedicated state to the player state machine.
