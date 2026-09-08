# ADR 0001: Enum-Based State Machine Pattern

## Status
Accepted

## Context
Actors in *The Last God* (Aeron, Guards, Bosses) have mutually exclusive behavioral states (e.g., Idle, Run, Jump, Attack, Hurt, Dead). Boolean flags (`isJumping`, `isAttacking`, `isDead`) lead to boolean soup, race conditions, invalid concurrent states, and unmaintainable transition logic.

## Decision
Every behavioral actor must maintain state using an explicit C# `enum` and a single private `_state` member variable. Transitions must occur exclusively through a dedicated `SetState(NewState)` method that validates transitions, fires `OnStateChanged` events, and manages enter/exit logic.

## Consequences
- **Positive**: Zero impossible concurrent states (e.g. attacking while dead); centralized event dispatching; clean animator parameter synchronization.
- **Negative**: Requires explicit enum definitions and state transition handlers per actor.
