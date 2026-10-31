# THE LAST GOD — Act 1 / Scene 1 prototype (2D)

**Logline:** Engineered to eliminate seven failed gods, a silent hunter begins to lose his humanity as he realizes he was never meant to save the world but to become its last god.

## Scope of this milestone
Everything up to **Aeron's awakening** (Act 1 — Origin, scenes 1–3 + combat-awakening intro):
lab at night → Voice wakes him → chamber shatters → guards fire, bullets slow near Aeron → raw first fight → "They will fear you. They should." → cut to black.

## Art direction (locked)
- Low-res retro pixel art, Hyper Light Drifter feel. **Characters 32×32**, tiles 16×16, scenes 320×180.
- **One shared 16-colour palette** ([palette.gpl](../Assets/Art/Act1/palette.gpl)); every scene is quantized to it.
- Colour language: cold slate/steel = the lab, **teal = Aeron / the Voice / liquid**, **red = alarm / danger / guards' visors**, yellow = gunfire & hazard.
- Storytelling through light: scene 1 starts almost black (only the chamber glows), alarm turns the room red, the aftermath is the coldest and emptiest.

## Storytelling beats → assets
| Beat | Script | Assets used |
|---|---|---|
| 1 Darkness, heartbeat | Pitch black, hum, Aeron suspended | `stasis_chamber/intact`, `aeron/suspended`, `fx/heartbeat` |
| 2 "Wake up." | Voice (V.O.), eyes snap open | `aeron/wake`, `ui/portraits/voice`, `fx/alarm_light` |
| 3 Control room | Voss: "He's awake…" (fear) | `dr_voss/idle,afraid`, `control_room_background` |
| 4 Glass cracks | Alarms, red light | `stasis_chamber/crack,drain`, `fx/sparks`, `fx/liquid_splash` |
| 5 Shatter + guards | "Contain the subject!" bullets slow | `stasis_chamber/shattered`, `aeron/collapse`, `guard/shoot`, `fx/muzzle_flash`, `fx/bullet(slowed)`, `fx/time_ripple` |
| 6 Combat awakening | Raw, instinctive fighting | `aeron/attack,hurt`, `guard/hurt,dead`, `fx/slash_arc` |
| 7 Aftermath | Looks at his hands, Voice line | `aeron/look_hands`, `guard/dead`, portraits |

Storyboard: `Assets/Art/Act1/storyboard/contact_sheet_2x.png` (+ individual panels).

## Animation tags (frame = 32×32, fps)
- **aeron**: idle(4) walk(6) suspended(4) wake(4) collapse(8) look_hands(6) attack(6) hurt(3) — faces right
- **guard** (faces left): idle(4) aim(2) shoot(3) hurt(2) dead(4)
- **dr_voss**: idle(4) afraid(2)
- **stasis_chamber** (48×80): intact, crack(3), drain(4), shattered
- **fx**: alarm_light, sparks, liquid_splash, muzzle_flash, bullet(normal/slowed), slash_arc, time_ripple, heartbeat
- **ui**: dialog_box (288×44, 9-slice border 4), portraits (aeron, aeron_awake, voss, voice, guard), next_arrow

Each sheet has `name.png` (horizontal strip), `name.json` (tags/fps) and `name.aseprite` (tags + durations) in `Assets/Art/Act1/<folder>/`.

## Gameplay intro notes (for the Unity/Unreal port)
- Player control starts after the shatter: raw, no skill tree — only move, strike, hurt-react.
- **Time-slow field** around Aeron: swap bullets to the `slowed` sprite inside `time_ripple` radius; this is the first hint he is "not human".
- Eyes glow teal (`eye=2`) from the moment he wakes; keep this as a permanent tell that grows with power.

## Regenerate
```
python tools/act1/build.py
```
Requires Python + Pillow + numpy; Aseprite CLI path is set in `tools/act1/core.py`.


## Playable build
See [game/act1/README.md](../game/act1/README.md) - open game/act1/index.html in a browser. Includes parallax backgrounds (skyline + server racks), animated props, lighting, cinematic intro, 3 combat waves and the Take/Cut-down humanity mechanic.
