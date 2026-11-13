"""Tiles, chamber, FX and UI sprites."""
import math
import random
from PIL import Image, ImageDraw, ImageFont
from core import R, new, outline


# ---------------- tiles (16x16) ----------------
def _t(fill):
    im = new(16, 16)
    ImageDraw.Draw(im).rectangle([0, 0, 15, 15], fill=R(fill))
    return im


def tiles():
    rnd = random.Random(7)
    T = {}
    im = _t(3); d = ImageDraw.Draw(im)
    d.line([(0, 0), (15, 0)], fill=R(2))
    for _ in range(6):
        d.point((rnd.randint(0, 15), rnd.randint(1, 15)), fill=R(4))
        d.point((rnd.randint(0, 15), rnd.randint(1, 15)), fill=R(2))
    T["floor_a"] = im
    im = _t(2); d = ImageDraw.Draw(im)
    for x in range(0, 16, 4):
        d.line([(x, 0), (x, 15)], fill=R(1))
    for y in (3, 7, 11, 15):
        d.line([(0, y), (15, y)], fill=R(3))
    T["floor_grate"] = im
    im = _t(1); d = ImageDraw.Draw(im)
    for y in range(16):
        for x in range(16):
            if ((x + y) // 4) % 2 == 0:
                d.point((x, y), fill=R(13))
    T["floor_hazard"] = im
    im = _t(2); d = ImageDraw.Draw(im)
    d.line([(0, 0), (15, 0)], fill=R(1)); d.line([(0, 0), (0, 15)], fill=R(1))
    d.rectangle([3, 3, 12, 12], outline=R(3))
    for p in ((2, 2), (13, 2), (2, 13), (13, 13)):
        d.point(p, fill=R(4))
    T["wall_panel"] = im
    im = _t(2); d = ImageDraw.Draw(im)
    for y0 in (2, 9):
        d.rectangle([0, y0, 15, y0 + 3], fill=R(3))
        d.line([(0, y0), (15, y0)], fill=R(4))
        d.line([(0, y0 + 3), (15, y0 + 3)], fill=R(1))
    d.rectangle([6, 0, 7, 15], fill=R(1))
    T["wall_pipes"] = im
    for n in (0, 1):
        im = _t(1); d = ImageDraw.Draw(im)
        d.rectangle([1, 2, 14, 13], fill=R(2))
        for k, y in enumerate(range(3, 13, 2)):
            ln = 4 + ((k * 5 + n * 3) % 8)
            d.line([(2, y), (2 + ln, y)], fill=R(15 if k % 2 == n else 14))
        d.point((13, 3 + n * 8), fill=R(9))
        T[f"wall_screen_{n}"] = im
    im = _t(1); d = ImageDraw.Draw(im)
    d.rectangle([0, 11, 15, 15], fill=R(2)); d.line([(0, 12), (15, 12)], fill=R(3))
    T["ceiling"] = im
    im = _t(2); d = ImageDraw.Draw(im)
    d.line([(0, 0), (15, 0)], fill=R(1))
    d.rectangle([3, 6, 12, 8], fill=R(5)); d.line([(3, 6), (12, 6)], fill=R(6))
    T["ceiling_light"] = im
    im = _t(2); d = ImageDraw.Draw(im)
    d.rectangle([2, 1, 13, 15], fill=R(3)); d.rectangle([2, 1, 13, 15], outline=R(1))
    for y in (4, 8, 12):
        d.line([(4, y), (7, y - 2)], fill=R(4)); d.line([(8, y - 2), (11, y)], fill=R(4))
    T["door"] = im
    for n in (0, 1):
        im = new(16, 16); d = ImageDraw.Draw(im)
        d.rectangle([0, 0, 15, 4], fill=R(1))
        d.rectangle([1, 1, 14, 3], fill=R(15 if n else 14))
        d.rectangle([0, 5, 15, 7], fill=R(4)); d.line([(0, 5), (15, 5)], fill=R(5))
        d.rectangle([0, 8, 15, 15], fill=R(3)); d.line([(0, 8), (15, 8)], fill=R(1))
        for k, x in enumerate((2, 5, 8, 11)):
            d.rectangle([x, 10, x + 1, 11], fill=R(9 if (k + n) % 2 else 15))
        T[f"console_{n}"] = im
    im = _t(2); d = ImageDraw.Draw(im)
    for y in range(2, 14, 3):
        d.line([(2, y), (13, y)], fill=R(1))
    d.rectangle([1, 1, 14, 14], outline=R(3))
    T["vent"] = im
    im = _t(3); d = ImageDraw.Draw(im)
    d.line([(0, 0), (15, 0)], fill=R(2))
    for x in range(16):
        d.point((x, 6 + round(3 * math.sin(x / 2.5))), fill=R(1))
        d.point((x, 7 + round(3 * math.sin(x / 2.5))), fill=R(2))
    T["floor_cable"] = im
    return T


def tileset_frames(T):
    names = list(T)
    return names, [T[n] for n in names]


# ---------------- chamber (48x80) ----------------
CRACKS = [
    [(24, 10), (22, 16), (26, 22), (23, 29)],
    [(26, 22), (31, 26), (30, 34), (34, 40)],
    [(23, 29), (17, 34), (19, 43), (14, 50)],
    [(34, 40), (36, 48), (33, 56)],
    [(14, 50), (18, 58), (15, 65)],
]


def chamber(level=1.0, cracks=0, shattered=False, drip=False):
    im = new(48, 80)
    d = ImageDraw.Draw(im)
    # base + cap machinery
    d.rectangle([0, 68, 47, 79], fill=R(3)); d.rectangle([0, 68, 47, 70], fill=R(4))
    d.rectangle([0, 76, 47, 79], fill=R(2))
    for x in (4, 12, 36, 43):
        d.rectangle([x, 72, x + 1, 73], fill=R(15 if x < 20 else 9))
    if not shattered:
        d.rectangle([0, 0, 47, 7], fill=R(3)); d.rectangle([0, 0, 47, 1], fill=R(4))
        d.rectangle([20, 6, 27, 9], fill=R(2))
    d.rectangle([2, 0, 4, 67], fill=R(2)); d.rectangle([43, 0, 45, 67], fill=R(2))  # posts
    if shattered:
        d.rectangle([2, 40, 4, 67], fill=R(2)); d.rectangle([2, 0, 4, 39], fill=(0, 0, 0, 0))
        d.rectangle([43, 0, 45, 30], fill=(0, 0, 0, 0))
        rnd = random.Random(3)
        for _ in range(26):
            x, y = rnd.randint(5, 42), rnd.randint(64, 67)
            d.point((x, y), fill=R(rnd.choice((5, 6, 15))))
        d.line([(6, 68), (41, 68)], fill=R(15))
    else:
        top = 8 + int((1 - level) * 58)
        for y in range(top, 67):
            for x in range(6, 42):
                if (x + y) % 2 == 0:
                    d.point((x, y), fill=R(14))
        d.line([(6, top), (41, top)], fill=R(15))
        rnd = random.Random(5)
        for _ in range(7):
            x, y = rnd.randint(8, 40), rnd.randint(top + 2, 65)
            d.point((x, y), fill=R(15))
        # glass edges + highlight
        d.line([(5, 8), (5, 67)], fill=R(5)); d.line([(42, 8), (42, 67)], fill=R(5))
        d.line([(8, 12), (8, 50)], fill=R(6))
        for c in CRACKS[:cracks]:
            d.line(c, fill=R(6))
    if drip:
        d.line([(26, 66), (26, 74)], fill=R(15))
    return outline(im)


def chamber_frames():
    intact = [chamber()]
    crack = [chamber(cracks=n) for n in (1, 3, 5)]
    drain = [chamber(level=l, cracks=5, drip=True) for l in (0.85, 0.6, 0.35, 0.1)]
    return {"intact": (intact, 1), "crack": (crack, 6), "drain": (drain, 8),
            "shattered": ([chamber(shattered=True)], 1)}


# ---------------- FX ----------------
def alarm():
    fr = []
    for i in range(4):
        im = new(16, 16); d = ImageDraw.Draw(im)
        d.rectangle([5, 11, 10, 14], fill=R(3)); d.rectangle([5, 11, 10, 11], fill=R(4))
        d.rectangle([6, 7, 9, 10], fill=R(9 if i % 2 == 0 else 7))
        a = math.radians(i * 45 - 90)
        if i % 2 == 0:
            for k in (-0.5, 0.5):
                d.line([(8, 8), (8 + 7 * math.cos(a + k), 8 + 7 * math.sin(a + k))], fill=R(8))
        fr.append(outline(im))
    return {"spin": (fr, 8)}


def sparks():
    fr = []
    rnd = random.Random(11)
    v = [(rnd.uniform(-1.4, 1.4), rnd.uniform(-2.4, -0.8)) for _ in range(7)]
    for f in range(5):
        im = new(16, 16); d = ImageDraw.Draw(im)
        for (vx, vy) in v:
            x, y = 8 + vx * f * 1.5, 12 + vy * f * 1.4 + 0.6 * f * f
            if 0 <= x < 16 and 0 <= y < 16:
                d.point((int(x), int(y)), fill=R(6 if f < 2 else 13 if f < 4 else 8))
        fr.append(im)
    return {"burst": (fr, 14)}


def splash():
    fr = []
    for f in range(6):
        im = new(32, 16); d = ImageDraw.Draw(im)
        w = 4 + f * 4
        d.line([(16 - w // 2, 15), (16 + w // 2, 15)], fill=R(14))
        d.line([(16 - w // 2 + 1, 14), (16 + w // 2 - 1, 14)], fill=R(15))
        for k in range(-3, 4):
            x = 16 + k * (1 + f)
            y = 14 - int(f * 2.4 - (f * abs(k) * 0.35)) + max(0, f - 3) * 2
            if 0 <= x < 32 and 0 <= y < 16:
                d.point((x, y), fill=R(15 if k % 2 == 0 else 14))
        fr.append(im)
    return {"splash": (fr, 12)}


def muzzle():
    fr = []
    for r in (2, 4, 3):
        im = new(16, 16); d = ImageDraw.Draw(im)
        d.ellipse([8 - r, 8 - r // 2, 8 + r, 8 + r // 2], fill=R(13))
        d.line([(8 - r - 2, 8), (8 + r + 2, 8)], fill=R(6))
        d.point((8, 8), fill=R(6))
        fr.append(im)
    return {"flash": (fr, 24)}


def bullets():
    a = new(8, 3); d = ImageDraw.Draw(a)
    d.rectangle([0, 1, 5, 1], fill=R(13)); d.point((6, 1), fill=R(6)); d.point((7, 1), fill=R(6))
    s = new(24, 3); d = ImageDraw.Draw(s)
    for x in range(24):
        d.point((x, 1), fill=R(3 if x < 8 else 14 if x < 16 else 15))
    d.rectangle([20, 0, 22, 2], fill=R(15)); d.point((23, 1), fill=R(6))
    return {"normal": ([a], 1), "slowed": ([s], 1)}


def slash():
    fr = []
    for f in range(4):
        im = new(32, 32); d = ImageDraw.Draw(im)
        r = 8 + f * 3
        d.arc([16 - r, 16 - r, 16 + r, 16 + r], -70 + f * 8, 60 + f * 5, fill=R(6 if f < 3 else 15), width=3 - f // 2)
        d.arc([16 - r + 3, 16 - r + 3, 16 + r - 3, 16 + r - 3], -55, 50, fill=R(15), width=1)
        fr.append(im)
    return {"arc": (fr, 18)}


def ripple():
    fr = []
    for f in range(6):
        im = new(32, 32); d = ImageDraw.Draw(im)
        r = 3 + f * 2.6
        tmp = new(32, 32); td = ImageDraw.Draw(tmp)
        td.ellipse([16 - r, 16 - r, 16 + r, 16 + r], outline=R(15 if f < 4 else 14))
        px, tp = im.load(), tmp.load()
        for y in range(32):
            for x in range(32):
                if tp[x, y][3] and (x + y + f) % 2 == 0:
                    px[x, y] = tp[x, y]
        fr.append(im)
    return {"pulse": (fr, 10)}


def heartbeat():
    fr = []
    for f in range(6):
        im = new(32, 16); d = ImageDraw.Draw(im)
        pts = [(0, 8), (8, 8), (10, 8 - (6 if f % 6 in (0, 1) else 2)), (12, 12), (14, 8), (31, 8)]
        d.line(pts, fill=R(9))
        fr.append(im)
    return {"beat": (fr, 6)}


# ---------------- UI ----------------
def dialog_box():
    im = new(288, 44); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 287, 43], fill=R(1))
    d.rectangle([2, 2, 285, 41], fill=R(2))
    d.rectangle([1, 1, 286, 42], outline=R(4))
    d.line([(2, 2), (285, 2)], fill=R(5))
    for p in ((0, 0), (287, 0), (0, 43), (287, 43)):
        d.point(p, fill=(0, 0, 0, 0))
    return im


def next_arrow():
    fr = []
    for f in range(2):
        im = new(8, 8); d = ImageDraw.Draw(im)
        d.polygon([(1, 1 + f), (6, 1 + f), (3.5, 5 + f)], fill=R(13))
        fr.append(im)
    return {"blink": (fr, 3)}


def portrait(kind):
    im = new(32, 32); d = ImageDraw.Draw(im)
    if kind == "guard":
        d.rectangle([6, 25, 25, 31], fill=R(3)); d.rectangle([8, 5, 23, 24], fill=R(11))
        d.rectangle([8, 5, 23, 14], fill=R(3)); d.line([(8, 5), (23, 5)], fill=R(4))
        d.rectangle([12, 13, 23, 17], fill=R(8)); d.rectangle([14, 14, 21, 14], fill=R(9))
        d.line([(14, 21), (18, 21)], fill=R(12))
        return outline(im)
    if kind == "voice":
        d.ellipse([2, 2, 29, 29], fill=R(1)); d.ellipse([5, 5, 26, 26], outline=R(15))
        d.ellipse([9, 9, 22, 22], outline=R(14)); d.rectangle([15, 10, 16, 21], fill=R(6))
        return im
    d.rectangle([6, 25, 25, 31], fill=R(5 if kind.startswith("aeron") else 6))  # shoulders
    d.rectangle([8, 5, 23, 24], fill=R(11))
    d.rectangle([8, 5, 23, 9], fill=R(10)); d.rectangle([8, 5, 10, 18], fill=R(10)); d.rectangle([21, 5, 23, 12], fill=R(10))
    d.rectangle([12, 22, 19, 25], fill=R(12))
    if kind == "voss":
        d.rectangle([8, 5, 10, 24], fill=R(10)); d.rectangle([21, 5, 23, 24], fill=R(10))
        d.rectangle([11, 14, 14, 16], outline=R(15)); d.rectangle([17, 14, 20, 16], outline=R(15))
        d.line([(15, 15), (16, 15)], fill=R(15))
    else:
        glow = kind == "aeron_awake"
        c = 15 if glow else 1
        d.rectangle([12, 15, 14, 16], fill=R(c)); d.rectangle([18, 15, 20, 16], fill=R(c))
        if not glow:
            d.line([(12, 14), (14, 14)], fill=R(12)); d.line([(18, 14), (20, 14)], fill=R(12))
    d.line([(14, 21), (18, 21)], fill=R(12))
    return outline(im)


def portraits():
    return {k: ([portrait(k)], 1) for k in ("aeron", "aeron_awake", "voss", "voice", "guard")}


def font(size=8):
    return ImageFont.load_default(size=size)

