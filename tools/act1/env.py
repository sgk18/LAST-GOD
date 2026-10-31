"""Parallax backgrounds and environment props for the Act 1 lab."""
import math
import random
from PIL import Image, ImageDraw
from core import R, new, outline, sheet, path


def bg_far():
    """Night skyline seen through the lab's observation windows (320x180, seamless)."""
    im = Image.new("RGBA", (320, 180), R(2)); d = ImageDraw.Draw(im)
    for y in range(0, 110):  # sky bands
        d.line([(0, y), (319, y)], fill=R(1 if y < 30 else 2 if y < 70 else 3))
    rnd = random.Random(21)
    x, first = 0, None
    while x < 320:
        w = rnd.randint(18, 36)
        h = rnd.randint(30, 80)
        if first is None:
            first = h
        if x + w >= 320:
            w, h = 320 - x, first
        top = 112 - h
        d.rectangle([x, top, x + w - 1, 112], fill=R(2))
        d.line([(x, top), (x + w - 1, top)], fill=R(3))
        if rnd.random() < 0.4:  # broken roof
            d.polygon([(x + w // 2, top), (x + w, top), (x + w, top + 8)], fill=R(1))
        for wy in range(top + 6, 110, 8):
            for wx in range(x + 3, x + w - 3, 6):
                if rnd.random() < 0.12:
                    d.point((wx, wy), fill=R(13 if rnd.random() < 0.8 else 8))
        if rnd.random() < 0.3:
            d.point((x + w // 2, top - 2), fill=R(9))
        x += w
    d.rectangle([0, 108, 319, 112], fill=R(1))
    for mx in range(0, 320, 64):  # window mullions
        d.rectangle([mx, 20, mx + 3, 120], fill=R(1))
        d.line([(mx + 1, 20), (mx + 1, 120)], fill=R(3))
    d.rectangle([0, 20, 319, 23], fill=R(1)); d.rectangle([0, 118, 319, 124], fill=R(1))
    d.rectangle([0, 0, 319, 19], fill=R(1)); d.rectangle([0, 125, 319, 179], fill=R(2))
    for y in range(126, 180, 12):
        d.line([(0, y), (319, y)], fill=R(1))
    return im


def bg_mid():
    """Server racks + pipes behind the action (2 blink frames, 320x180, seamless)."""
    frames = []
    for f in range(2):
        im = new(320, 180); d = ImageDraw.Draw(im)
        rnd = random.Random(33)
        for x in range(8, 320, 40):
            top = 62 + rnd.randint(0, 6)
            d.rectangle([x, top, x + 27, 126], fill=R(2)); d.rectangle([x, top, x + 27, 126], outline=R(1))
            d.line([(x + 1, top + 1), (x + 26, top + 1)], fill=R(3))
            for ry in range(top + 5, 122, 7):
                d.rectangle([x + 3, ry, x + 24, ry + 4], fill=R(1))
                for lx in range(x + 5, x + 22, 4):
                    on = rnd.random() < 0.5
                    if (f and rnd.random() < 0.3):
                        on = not on
                    d.point((lx, ry + 2), fill=R(15 if on else 3))
        for y in (30, 40):  # overhead pipes
            d.rectangle([0, y, 319, y + 4], fill=R(3)); d.line([(0, y), (319, y)], fill=R(4))
            d.line([(0, y + 4), (319, y + 4)], fill=R(1))
            for bx in range(10, 320, 54):
                d.rectangle([bx, y - 1, bx + 2, y + 5], fill=R(1))
        frames.append(im)
    return frames


def fg_overhead():
    im = new(320, 48); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 319, 6], fill=R(1))
    rnd = random.Random(2)
    for x in range(12, 320, 38):
        l = rnd.randint(14, 40)
        pts = [(x + int(2 * math.sin(i / 3)), 7 + i) for i in range(l)]
        d.line(pts, fill=R(1))
        d.point(pts[-1], fill=R(9 if rnd.random() < 0.5 else 15))
    return im


def fg_floor():
    im = new(320, 14); d = ImageDraw.Draw(im)
    d.rectangle([0, 7, 319, 13], fill=R(1))
    for x in range(0, 320, 5):
        d.point((x, 7 + (x * 7) % 5), fill=R(2))
    for x in range(30, 320, 90):
        d.rectangle([x, 3, x + 10, 8], fill=R(1))
    return im


def crate():
    im = new(24, 20); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 23, 19], fill=R(3)); d.rectangle([0, 0, 23, 19], outline=R(1))
    d.line([(0, 0), (23, 19)], fill=R(2)); d.line([(23, 0), (0, 19)], fill=R(2))
    d.rectangle([2, 2, 21, 17], outline=R(4))
    d.rectangle([8, 8, 15, 11], fill=R(13)); d.point((11, 9), fill=R(1))
    return im


def server_rack():
    fr = []
    for f in range(2):
        im = new(20, 44); d = ImageDraw.Draw(im)
        d.rectangle([0, 0, 19, 43], fill=R(3))
        for y in range(3, 41, 6):
            d.rectangle([2, y, 17, y + 3], fill=R(1))
            for k, x in enumerate((4, 7, 10)):
                d.point((x, y + 1), fill=R(15 if (k + y // 6 + f) % 3 else 9))
        fr.append(outline(im))
    return fr


def console_desk():
    fr = []
    for f in range(2):
        im = new(40, 22); d = ImageDraw.Draw(im)
        d.polygon([(4, 0), (36, 0), (38, 9), (2, 9)], fill=R(1))
        d.polygon([(6, 1), (34, 1), (36, 8), (4, 8)], fill=R(14 if f else 15))
        for k in range(4):
            d.line([(8, 2 + k * 2), (8 + 6 + (k * 5 + f * 3) % 14, 2 + k * 2)], fill=R(6))
        d.rectangle([0, 10, 39, 21], fill=R(3)); d.line([(0, 10), (39, 10)], fill=R(4))
        for k, x in enumerate(range(3, 37, 5)):
            d.rectangle([x, 13, x + 2, 14], fill=R(9 if (k + f) % 2 else 15))
        fr.append(outline(im))
    return fr


def lamp():
    fr = []
    for f in range(3):
        im = new(12, 30); d = ImageDraw.Draw(im)
        d.rectangle([5, 6, 6, 29], fill=R(3)); d.rectangle([2, 26, 9, 29], fill=R(2))
        d.rectangle([1, 0, 10, 5], fill=R(4))
        d.rectangle([2, 4, 9, 5], fill=[R(6), R(13), R(5)][f])
        fr.append(outline(im))
    return fr


def steam():
    fr = []
    for f in range(6):
        im = new(16, 24); d = ImageDraw.Draw(im)
        d.rectangle([3, 20, 12, 23], fill=R(3)); d.line([(3, 20), (12, 20)], fill=R(4))
        for k in range(5):
            y = 19 - (f * 3 + k * 4) % 20
            r = 1 + (19 - y) // 6
            x = 8 + int(2 * math.sin((f + k) * 1.3))
            d.ellipse([x - r, y - r, x + r, y + r], fill=R(5 if y > 10 else 4))
        fr.append(im)
    return fr


def monitor_broken():
    fr = []
    for f in range(2):
        im = new(16, 16); d = ImageDraw.Draw(im)
        d.rectangle([1, 2, 14, 11], fill=R(1)); d.rectangle([2, 3, 13, 10], fill=R(2))
        d.line([(3, 3), (9, 10)], fill=R(6)); d.line([(9, 10), (12, 5)], fill=R(6))
        d.rectangle([6, 12, 9, 14], fill=R(3))
        if f:
            d.point((4, 2), fill=R(13)); d.point((12, 1), fill=R(6)); d.point((8, 0), fill=R(13))
        fr.append(outline(im))
    return fr


def gurney():
    im = new(40, 20); d = ImageDraw.Draw(im)
    d.rectangle([2, 6, 37, 10], fill=R(5)); d.line([(2, 6), (37, 6)], fill=R(6))
    d.rectangle([2, 10, 37, 11], fill=R(4))
    for x in (6, 32):
        d.line([(x, 12), (x, 17)], fill=R(3)); d.rectangle([x - 2, 17, x + 2, 18], fill=R(1))
    d.rectangle([2, 3, 9, 6], fill=R(6))
    return outline(im)


def sign():
    fr = []
    for f in range(2):
        im = new(18, 12); d = ImageDraw.Draw(im)
        d.rectangle([0, 0, 17, 11], fill=R(1))
        d.rectangle([1, 1, 16, 10], fill=R(8 if f else 7))
        d.line([(4, 9), (9, 2)], fill=R(13)); d.line([(9, 2), (14, 9)], fill=R(13)); d.line([(4, 9), (14, 9)], fill=R(13))
        d.point((9, 6), fill=R(13))
        fr.append(im)
    return fr


def debris():
    im = new(24, 8); d = ImageDraw.Draw(im)
    rnd = random.Random(8)
    for _ in range(18):
        x, y = rnd.randint(1, 21), rnd.randint(2, 6)
        d.rectangle([x, y, x + rnd.randint(0, 2), y + rnd.randint(0, 1)], fill=R(rnd.choice((3, 4, 5, 2))))
    return outline(im)


def build_env():
    sheet("environment", "bg_far", {"bg": ([bg_far()], 1)}, to_aseprite=False)
    sheet("environment", "bg_mid", {"blink": (bg_mid(), 2)}, to_aseprite=False)
    sheet("environment", "fg_overhead", {"fg": ([fg_overhead()], 1)}, to_aseprite=False)
    sheet("environment", "fg_floor", {"fg": ([fg_floor()], 1)}, to_aseprite=False)
    P = "props"
    sheet(P, "crate", {"idle": ([crate()], 1)})
    sheet(P, "server_rack", {"blink": (server_rack(), 3)})
    sheet(P, "console_desk", {"blink": (console_desk(), 2)})
    sheet(P, "lamp", {"flicker": (lamp(), 6)})
    sheet(P, "steam_vent", {"steam": (steam(), 8)})
    sheet(P, "broken_monitor", {"spark": (monitor_broken(), 3)})
    sheet(P, "gurney", {"idle": ([gurney()], 1)})
    sheet(P, "warning_sign", {"blink": (sign(), 2)})
    sheet(P, "debris", {"idle": ([debris()], 1)})

