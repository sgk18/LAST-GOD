"""Build all Act 1 / Scene 1 assets:  python tools/act1/build.py"""
import json
import numpy as np
from PIL import Image, ImageDraw
import core
from core import R, rgb, new, quantize, sheet, path, flip
import characters as ch
import world as wd

W, H = 320, 180
OY = 4
FLOOR_Y = 134
CH_X, CH_Y = 64, 48


def frames_of(d, tag):
    return d[tag][0]


def lab_base(T, screens_on=True, variant=0):
    im = Image.new("RGBA", (W, H), R(1))
    def put(name, c, r):
        t = T[name]
        im.paste(t, (c * 16, OY + r * 16), t)
    for c in range(20):
        put("ceiling_light" if c % 5 == 2 else "ceiling", c, 0)
        put("ceiling", c, 1)
        for r in range(2, 7):
            put("wall_pipes" if c % 6 == 0 else "vent" if (c % 6 == 3 and r == 5) else "wall_panel", c, r)
        for r in range(7, 11):
            n = "floor_grate" if (c + r) % 6 == 0 else "floor_cable" if (c * 3 + r) % 11 == 0 else "floor_a"
            put(n, c, r)
        put("floor_hazard", c, 7) if 7 <= c <= 12 and False else None
    for c in (3, 4, 14, 15, 16):
        put(f"wall_screen_{(c + variant) % 2}" if screens_on else "wall_panel", c, 3)
    put("door", 18, 5); put("door", 19, 5)
    for c in range(8, 14):
        put("floor_hazard", c, 10)
    return im


def control_base(T, variant=0):
    im = Image.new("RGBA", (W, H), R(1))
    def put(name, c, r):
        t = T[name]
        im.paste(t, (c * 16, OY + r * 16), t)
    for c in range(20):
        put("ceiling", c, 0); put("ceiling", c, 1)
        for r in range(2, 7):
            put("wall_panel", c, r)
        for r in range(7, 11):
            put("floor_a", c, r)
    for c in range(2, 18):
        for r in (2, 3, 4):
            put(f"wall_screen_{(c + r + variant) % 2}", c, r)
    for c in range(3, 17):
        put(f"console_{(c + variant) % 2}", c, 6)
    return im


def light(im, lights, base, tint=1, red=0.0):
    a = np.array(im.convert("RGB")).astype(float)
    h, w = a.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w]
    if red:
        a = a * (1 - red) + np.array(rgb(8), float) * red
    k = np.full((h, w), float(base))
    for cx, cy, r, s in lights:
        dist = np.sqrt((xx - cx) ** 2 + (yy - cy) ** 2)
        k = np.maximum(k, s * np.clip(1 - dist / r, 0, 1))
    dark = np.array(rgb(tint), float)
    out = dark + (a - dark) * k[..., None]
    return quantize(Image.fromarray(out.clip(0, 255).astype("uint8")))


def pp(im, spr, x, y):
    im.paste(spr, (x, y), spr)


def place(im, spr, x, feet=FLOOR_Y):
    pp(im, spr, x, feet - 31)


def dialogue(im, speaker, text, port, color=13):
    box = wd.dialog_box()
    pp(im, box, 16, 6)
    pp(im, port, 22, 12)
    d = ImageDraw.Draw(im)
    d.fontmode = "1"
    d.text((62, 12), speaker, font=wd.font(8), fill=R(color))
    d.text((62, 25), text, font=wd.font(8), fill=R(6))


def caption(im, text):
    d = ImageDraw.Draw(im)
    d.fontmode = "1"
    d.rectangle([0, H - 14, W, H], fill=R(1))
    d.line([(0, H - 15), (W, H - 15)], fill=R(3))
    d.text((6, H - 12), text, font=wd.font(8), fill=R(5))


def build():
    core.write_palette()
    T = wd.tiles()
    names, tl = wd.tileset_frames(T)
    sheet("tiles", "lab_tileset", {n: ([t], 1) for n, t in T.items()})
    with open(path("tiles", "lab_tileset_index.json"), "w") as f:
        json.dump({n: i for i, n in enumerate(names)}, f, indent=2)

    A, G, V = ch.aeron(), ch.guard(), ch.voss()
    sheet("characters", "aeron", A)
    sheet("characters", "guard", G)
    swap = {rgb(3): rgb(7), rgb(2): rgb(1), rgb(4): rgb(8), rgb(8): rgb(13), rgb(9): rgb(6)}
    def recolor(im):
        out = im.copy(); px = out.load()
        for y in range(out.height):
            for x in range(out.width):
                r, g_, b_, a = px[x, y]
                if a and (r, g_, b_) in swap:
                    px[x, y] = swap[(r, g_, b_)] + (a,)
        return out
    sheet("characters", "guard_elite", {k: ([recolor(f) for f in v[0]], v[1]) for k, v in G.items()})
    sheet("characters", "dr_voss", V)
    CH = wd.chamber_frames()
    sheet("environment", "stasis_chamber", CH)
    for name, fn in (("alarm_light", wd.alarm), ("sparks", wd.sparks), ("liquid_splash", wd.splash),
                     ("muzzle_flash", wd.muzzle), ("bullet", wd.bullets), ("slash_arc", wd.slash),
                     ("time_ripple", wd.ripple), ("heartbeat", wd.heartbeat)):
        sheet("fx", name, fn())
    sheet("ui", "portraits", wd.portraits())
    sheet("ui", "next_arrow", wd.next_arrow())
    wd.dialog_box().save(path("ui", "dialog_box.png"))

    FX = {k: v for k, v in {
        "ripple": wd.ripple()["pulse"][0], "flash": wd.muzzle()["flash"][0],
        "sparks": wd.sparks()["burst"][0], "alarm": wd.alarm()["spin"][0],
        "bullet": wd.bullets()}.items()}
    P = {k: v[0][0] for k, v in wd.portraits().items()}

    def lab(chamber_frame, alarm=False, shade=0.35, lights=None, variant=0, aeron=None, cx=CH_X):
        im = lab_base(T, variant=variant)
        if aeron is not None:
            pp(im, aeron, cx + 8, 66)
        pp(im, chamber_frame, cx, CH_Y)
        return im

    def finish(im, base, lights, red=0.0):
        return light(im, lights, base, red=red)

    panels = []
    chamber_glow = (CH_X + 24, 90, 70, 0.95)
    # 1 darkness
    im = lab(CH["intact"][0][0], aeron=A["suspended"][0][0])
    im = finish(im, 0.16, [(CH_X + 24, 90, 80, 1.0)])
    caption(im, "1. INT. LAB - NIGHT.  Pitch darkness. A faint heartbeat. Machines hum like something alive.")
    pp(im, wd.heartbeat()["beat"][0][0], 270, 8)
    panels.append(("01_darkness_heartbeat", im))
    # 2 wake up
    im = lab(CH["intact"][0][0], aeron=A["wake"][0][2], variant=1)
    im = finish(im, 0.30, [chamber_glow, (60, 60, 90, 0.7)])
    dialogue(im, "VOICE (V.O.)", "YOU WERE NOT MADE TO SLEEP.", P["voice"], 15)
    pp(im, FX["alarm"][0], 232, 56)
    caption(im, "Aeron's eyes snap open. The liquid drains violently. Alarms blare.")
    panels.append(("02_eyes_open", im))
    # 3 control room
    im = control_base(T)
    place(im, V["afraid"][0][0], 150, feet=FLOOR_Y)
    im = finish(im, 0.45, [(160, 90, 120, 0.9)], red=0.12)
    dialogue(im, "DR. ILYA VOSS", "(whispers)  He's awake...", P["voss"], 15)
    caption(im, "2. INT. LAB CONTROL ROOM - CONTINUOUS.  Fear. Not excitement.")
    panels.append(("03_control_room_voss", im))
    # 4 glass cracks
    im = lab(CH["crack"][0][2], aeron=A["wake"][0][3], variant=0)
    pp(im, wd.sparks()["burst"][0][2], 200, 70)
    im = finish(im, 0.45, [chamber_glow], red=0.28)
    caption(im, "3. INT. LAB - CONTINUOUS.  Red lights flicker. The glass begins to crack.")
    panels.append(("04_glass_cracks", im))
    # 5 shatter + guards + slowed bullets
    im = lab(CH["shattered"][0][0], variant=1)
    pp(im, wd.splash()["splash"][0][4], CH_X + 8, FLOOR_Y - 12)
    place(im, A["collapse"][0][7], 112)
    place(im, G["shoot"][0][1], 236)
    place(im, G["shoot"][0][0], 266)
    pp(im, FX["flash"][0], 214, FLOOR_Y - 18); pp(im, FX["flash"][0], 244, FLOOR_Y - 18)
    sl = flip(FX["bullet"]["slowed"][0][0]); nb = flip(FX["bullet"]["normal"][0][0])
    pp(im, nb, 206, FLOOR_Y - 20); pp(im, nb, 224, FLOOR_Y - 22)
    pp(im, sl, 150, FLOOR_Y - 14); pp(im, sl, 160, FLOOR_Y - 11)
    pp(im, FX["ripple"][3], 118, FLOOR_Y - 26)
    im = finish(im, 0.60, [(120, 110, 80, 1.0)], red=0.2)
    dialogue(im, "GUARD", "CONTAIN THE SUBJECT!", P["guard"], 9)
    caption(im, "The chamber shatters. They fire. Bullets slow... slightly... near Aeron.")
    panels.append(("05_guards_fire_bullets_slow", im))
    # 6 aftermath
    im = lab(CH["shattered"][0][0], variant=0)
    place(im, A["look_hands"][0][3], 120)
    place(im, G["dead"][0][3], 232); place(im, G["dead"][0][2], 262)
    pp(im, wd.sparks()["burst"][0][3], 100, 70)
    im = finish(im, 0.55, [(135, 100, 90, 1.0)], red=0.08)
    dialogue(im, "VOICE (V.O.)", "THEY WILL FEAR YOU...  THEY SHOULD.", P["voice"], 15)
    caption(im, "After the fight. Bodies lie still. Aeron looks at his hands.   CUT TO BLACK.")
    panels.append(("06_aftermath_hands", im))

    # title card
    tc = Image.new("RGBA", (W, H), R(1))
    d = ImageDraw.Draw(tc); d.fontmode = "1"
    d.text((W // 2, 70), "THE LAST GOD", font=wd.font(28), fill=R(9), anchor="mm")
    d.text((W // 2, 98), "ACT 1 - ORIGIN", font=wd.font(10), fill=R(5), anchor="mm")
    pp(tc, A["suspended"][0][0], W // 2 - 16, 112)
    panels.insert(0, ("00_title", tc))

    for n, im in panels:
        im.save(path("storyboard", n + ".png"))
    cols = 3
    rows = (len(panels) + cols - 1) // cols
    cs = Image.new("RGBA", (W * cols + 4 * (cols + 1), H * rows + 4 * (rows + 1)), R(1))
    for i, (n, im) in enumerate(panels):
        cs.paste(im, (4 + (i % cols) * (W + 4), 4 + (i // cols) * (H + 4)))
    cs.save(path("storyboard", "contact_sheet.png"))
    cs.resize((cs.width * 2, cs.height * 2), Image.NEAREST).save(path("storyboard", "contact_sheet_2x.png"))
    # clean room backgrounds for the engine
    lab_base(T).save(path("scenes", "lab_background.png"))
    control_base(T).save(path("scenes", "control_room_background.png"))
    # animation preview sheet
    prev = Image.new("RGBA", (32 * 8 * 3, 32 * 3 * 3 + 8), R(2))
    y = 0
    for name, d_ in (("aeron", A), ("guard", G), ("voss", V)):
        x = 0
        for tag, (fr, fps) in d_.items():
            for f in fr:
                prev.paste(f.resize((96, 96), Image.NEAREST), (x % (96 * 8), y + (x // (96 * 8)) * 0), f.resize((96, 96), Image.NEAREST))
                x += 96
                if x >= 96 * 8:
                    break
        y += 96
    prev.save(path("characters", "preview_x3.png"))
    import env
    env.build_env()
    core.build_aseprite()
    export_game()


def export_game():
    import glob, os, shutil
    dst = os.path.join(core.ROOT, "game", "act1", "assets")
    os.makedirs(dst, exist_ok=True)
    meta = {}
    for js in glob.glob(os.path.join(core.OUT, "*", "*.json")):
        m = json.load(open(js))
        if "image" not in m:
            continue
        name = os.path.splitext(os.path.basename(js))[0]
        shutil.copy(os.path.join(os.path.dirname(js), m["image"]), os.path.join(dst, m["image"]))
        meta[name] = m
    for n in ("dialog_box.png",):
        shutil.copy(os.path.join(core.OUT, "ui", n), os.path.join(dst, n))
    for n in ("lab_background.png", "control_room_background.png"):
        shutil.copy(os.path.join(core.OUT, "scenes", n), os.path.join(dst, n))
    with open(os.path.join(dst, "meta.js"), "w") as f:
        f.write("window.META = " + json.dumps(meta) + ";")
    print("game assets exported:", len(meta))


if __name__ == "__main__":
    build()




