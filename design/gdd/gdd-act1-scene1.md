# GDD — Act 1 Scene 1: Laboratory Awakening & Initial Breach

## 1. Executive Summary
- **Game Title**: The Last God
- **Genre**: 2D Action-Platformer
- **Target Aesthetic**: GBA-authentic 240×160 reference framing, 16 PPU, Point/no-filter texture sampling, blended with high-density painterly sprite art (*Sons of Sparta* style).
- **Scope of Document**: Act 1 Scene 1 (INT. LAB – NIGHT) Vertical Slice.

---

## 2. Narrative Beat & Sequence Flow
1. **Cold Open**: Total blackness. Low, rhythmic heartbeat audio pulses accompanied by deep ambient machine hum.
2. **The Voice (V.O.)**: Subtitle typewriter text appears:
   - *"WAKE UP."* (Pause 1.5s)
   - *"YOU WERE NOT MADE TO SLEEP."*
3. **Chamber Rupture**:
   - Laboratory lights flicker with an electronic hum (`AmbientBackgroundFlicker`).
   - Central stasis chamber cracks (`Chamber_Cracked.png`) with stress sound effect.
   - Glass shatters violently (`Chamber_Shattered.png`), bursting water/glass particles, blaring alarm sirens, screen shake, and flashing red emergency lighting.
4. **Awakening & Power Surge**:
   - Aeron is revealed standing on the chamber dais platform.
   - Eyes snap open with piercing cyan luminescence.
   - Aeron emits a localized shockwave: **Chronos Aura (Temporal Distortion)**.
   - Player control is granted.
5. **Tactical Guard Incursion**:
   - Laboratory security doors open. Two guards enter from opposite sides (catwalk & deck).
   - Guards raise laser rifles and open fire.
   - Bullets entering within 2.5 units of Aeron visibly decelerate (speed drops 75%), giving the player reactive time to dodge or jump over projectiles.
6. **Visceral Combat**:
   - Aeron closes distance and eliminates the guards using 3-hit unarmed/melee strikes.
   - Guards recoil from hits and collapse permanently on the ground upon defeat.
7. **Resolution & Aftermath**:
   - As the last guard falls, input locks. Camera softly pans and focuses on Aeron staring down at his glowing hands in stunned realization.
   - The Voice speaks again:
     - *"THEY WILL FEAR YOU."* (Pause 1.5s)
     - *"THEY SHOULD."*
   - Screen fades cleanly to black.

---

## 3. Core Mechanics & Character Moveset
- **Horizontal Movement**: 6.5 units/sec, responsive acceleration with snappy turnaround.
- **Jump & Fall**: Variable height jump (tap = short hop, hold = full vertical reach), fall gravity multiplier (3.0x) for weight.
- **Combat Roll / Dash**: Invincibility frames (0.25s) with dust VFX, used to bypass slowed bullets.
- **Melee Combat**: 3-hit combo dealing damage via `IDamageable.TakeDamage(int amount, Vector2 knockbackDir)`.
- **Chronos Aura (First Power)**: Passive proximity time-dilation bubble (radius: 2.5 units) slowing projectile velocities by 75%.

---

## 4. Combat Metrics & Encounter Parameters
- **Aeron**:
  - Max HP: 10
  - Attack 1: 2 dmg, 1.2 unit range, knockback 2.5
  - Attack 2: 3 dmg, 1.3 unit range, knockback 3.5
  - Attack 3: 5 dmg, 1.5 unit range, knockback 5.0
- **Lab Guard**:
  - Max HP: 6 (takes a full 3-hit combo to eliminate)
  - Move Speed: 3.5 units/sec
  - Attack Range: 5.0 units
  - Fire Cooldown: 1.8 seconds
  - Bullet Speed: 8.0 units/sec (slows to 2.0 units/sec within Aeron's aura)
  - Bullet Damage: 1 HP

---

## 5. Visual & Audio Guidelines
- **Camera**: Orthographic, size 7.5 (240x160 viewport equivalent), Pixel Perfect snapping at 16 PPU.
- **Palette**: Slate blues (`#0f172a`), deep charcoal (`#020617`), vibrant cyan stasis fluid (`#38bdf8`), crimson emergency strobe (`#ef4444`).
- **Audio**: Low-pass filtered heartbeat, 8-bit crushed alarm siren, crisp glass shattering, resonant synth bass pulse on awakening.
