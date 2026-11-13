"""Procedural 32x32 character sprites: Aeron, Guard, Dr. Voss."""
import math
from PIL import Image, ImageDraw
from core import R, new, outline, flip

BASE = dict(hip=(16, 20), top=(16, 12), head=(1, -4), hl=(13, 19), hr=(19, 19),
            fl=(14, 29), fr=(18, 29), eye=1, gun=False, tablet=False)


def pose(**k):
    b = dict(BASE)
    b.update(k)
    return b


OUTFITS = {
    "aeron": dict(shirt=5, belt=4, pants=3, pants_s=2, boot=2, skin=11, skin_s=12, sleeve=11,
                  sleeve_s=12, hair=10),
    "guard": dict(shirt=3, hi=4, belt=1, pants=2, pants_s=1, boot=1, skin=11, sleeve=3,
                  sleeve_s=2, helmet=3, visor=8),
    "voss": dict(shirt=14, belt=4, pants=3, pants_s=2, boot=2, skin=11, skin_s=12, sleeve=6,
                 sleeve_s=5, hair=10, coat=6, coat_s=5, glasses=15, bob=True),
}


def figure_raw(p, o):
    im = new(32, 32)
    d = ImageDraw.Draw(im)
    hx, hy = p["hip"]
    tx, ty = p["top"]

    def limb(a, b, col, w=2):
        d.line([a, b], fill=R(col))
        if w == 2:
            d.line([(a[0] + 1, a[1]), (b[0] + 1, b[1])], fill=R(col))

    def leg(f, far):
        limb((hx, hy), f, o["pants_s"] if far else o["pants"])
        d.rectangle([f[0], f[1], f[0] + 2, f[1] + 1], fill=R(o["boot"]))

    def arm(h, far):
        limb((tx, ty + 1), h, o["sleeve_s"] if far else o["sleeve"], w=1 if far else 2)
        d.point(h, fill=R(o["skin"]))

    leg(p["fl"], True)
    arm(p["hl"], True)
    d.polygon([(tx - 2, ty), (tx + 2, ty), (hx + 2, hy), (hx - 2, hy)], fill=R(o["shirt"]))
    if "hi" in o:
        d.line([(tx - 2, ty), (tx + 2, ty)], fill=R(o["hi"]))
    d.line([(hx - 2, hy), (hx + 2, hy)], fill=R(o["belt"]))
    leg(p["fr"], False)
    if "coat" in o:
        d.polygon([(hx - 3, hy - 3), (hx + 3, hy - 3), (hx + 4, hy + 7), (hx - 4, hy + 7)], fill=R(o["coat"]))
        d.line([(hx - 3, hy - 3), (hx - 4, hy + 7)], fill=R(o["coat_s"]))
        d.line([(tx, ty), (tx, hy + 7)], fill=R(o["coat_s"]))
    arm(p["hr"], False)
    if p["tablet"]:
        x, y = p["hr"]
        d.rectangle([x, y - 3, x + 3, y + 1], fill=R(4))
        d.rectangle([x + 1, y - 2, x + 2, y], fill=R(15))
    if p["gun"]:
        x, y = p["hr"]
        d.line([(x - 2, y), (x + 8, y - 1)], fill=R(2))
        d.line([(x - 2, y + 1), (x + 8, y)], fill=R(1))
        d.point((x + 3, y - 1), fill=R(4))
    cx, cy = tx + p["head"][0], ty + p["head"][1]
    d.point((tx, ty - 1), fill=R(o["skin_s"] if "skin_s" in o else o["skin"]))
    d.rectangle([cx - 3, cy - 3, cx + 2, cy + 2], fill=R(o["skin"]))
    if "helmet" in o:
        d.rectangle([cx - 3, cy - 3, cx + 2, cy], fill=R(o["helmet"]))
        d.rectangle([cx, cy, cx + 2, cy + 1], fill=R(o["visor"]))
        d.line([(cx - 3, cy - 3), (cx + 2, cy - 3)], fill=R(4))
    else:
        d.rectangle([cx - 3, cy - 3, cx + 2, cy - 2], fill=R(o["hair"]))
        d.rectangle([cx - 3, cy - 3, cx - 2, cy + (3 if o.get("bob") else 1)], fill=R(o["hair"]))
        e = p["eye"]
        if e == 0:
            d.point((cx + 1, cy), fill=R(12))
            d.point((cx + 2, cy), fill=R(12))
        elif e == 1:
            d.point((cx + 1, cy), fill=R(1))
        else:
            d.point((cx + 1, cy), fill=R(15))
            d.point((cx + 2, cy), fill=R(15))
        if "glasses" in o:
            d.line([(cx, cy), (cx + 2, cy)], fill=R(o["glasses"]))
    return im


def lay_down(im):
    rot = im.rotate(-90, resample=Image.NEAREST)
    bb = rot.getbbox()
    out = new(32, 32)
    out.paste(rot, (0, 31 - bb[3]), rot)
    return out


def render(p, o, lie=False):
    im = figure_raw(p, o)
    if lie:
        im = lay_down(im)
    return outline(im)


def aeron():
    o = OUTFITS["aeron"]
    A = lambda p, lie=False: render(p, o, lie)
    idle = [A(pose(top=(16, 12 + b), hl=(13, 19 + b), hr=(19, 19 + b))) for b in (0, 1, 1, 0)]
    walk = []
    for i in range(6):
        t = i / 6 * 2 * math.pi
        s, c = math.sin(t), math.cos(t)
        walk.append(A(pose(top=(16, 12 + (1 if i % 3 == 0 else 0)),
                           fr=(17 + round(4 * s), 29 - max(0, round(2 * c))),
                           fl=(15 - round(4 * s), 29 - max(0, round(-2 * c))),
                           hr=(18 - round(3 * s), 19), hl=(14 + round(3 * s), 19))))
    susp = [A(pose(hip=(16, 19 + b), top=(16, 11 + b), head=(0, -4), hl=(12, 23 + b), hr=(20, 23 + b),
                   fl=(14, 28 + b), fr=(17, 29 + b), eye=0)) for b in (0, 1, 2, 1)]
    wake = [A(pose(hip=(16, 19), top=(16, 11), head=(0, -4), hl=(13, 21), hr=(19, 21), fl=(14, 28), fr=(17, 29), eye=0)),
            A(pose(hip=(16, 19), top=(16, 11), head=(0, -4), hl=(13, 18), hr=(19, 18), fl=(14, 28), fr=(17, 29), eye=2)),
            A(pose(hip=(16, 19), top=(16, 11), head=(1, -4), hl=(12, 16), hr=(20, 16), fl=(13, 28), fr=(18, 29), eye=2)),
            A(pose(hip=(16, 19), top=(16, 11), head=(1, -4), hl=(12, 17), hr=(20, 17), fl=(14, 28), fr=(17, 29), eye=2))]
    slump = pose(hip=(14, 25), top=(18, 21), head=(2, -2), hl=(15, 29), hr=(22, 29), fl=(8, 29), fr=(11, 29), eye=2)
    collapse = [
        A(pose(top=(15, 12), hl=(12, 19), hr=(20, 18), eye=2)),
        A(pose(hip=(16, 23), top=(15, 15), head=(1, -3), hl=(12, 22), hr=(20, 22), fl=(12, 29), fr=(19, 29), eye=2)),
        A(pose(hip=(16, 25), top=(15, 18), head=(1, -3), hl=(12, 28), hr=(21, 28), fl=(10, 29), fr=(19, 29), eye=2)),
        A(slump),
        A(dict(slump, top=(18, 22))),
        A(pose(hip=(16, 20), top=(16, 12), head=(2, -3), hl=(13, 19), hr=(19, 19), eye=2), lie=True),
        A(pose(hip=(16, 20), top=(16, 12), head=(2, -3), hl=(13, 20), hr=(19, 20), eye=2), lie=True),
        A(pose(hip=(16, 20), top=(16, 13), head=(2, -3), hl=(13, 20), hr=(19, 20), eye=2), lie=True),
    ]
    look = [A(pose(head=(2, -3), hl=(13, 19), hr=(19, 19), eye=2)),
            A(pose(head=(2, -3), hl=(15, 17), hr=(21, 17), eye=2)),
            A(pose(head=(2, -3), hl=(20, 16), hr=(22, 15), eye=2)),
            A(pose(head=(2, -3), hl=(20, 16), hr=(22, 15), eye=2)),
            A(pose(head=(2, -3), hl=(20, 17), hr=(22, 14), eye=2)),
            A(pose(head=(2, -3), hl=(20, 17), hr=(22, 14), eye=2))]
    attack = [A(pose(top=(15, 12), hr=(11, 16), hl=(18, 17), fr=(20, 29), eye=2)),
              A(pose(top=(14, 13), hr=(9, 15), hl=(18, 17), fl=(12, 29), fr=(21, 29), eye=2)),
              A(pose(top=(19, 12), head=(2, -4), hr=(27, 14), hl=(14, 18), fl=(14, 29), fr=(22, 29), eye=2)),
              A(pose(top=(20, 12), head=(2, -4), hr=(28, 14), hl=(14, 18), fl=(14, 29), fr=(22, 29), eye=2)),
              A(pose(top=(18, 12), hr=(24, 17), hl=(15, 18), eye=2)),
              A(pose(eye=2))]
    hurt = [A(pose(top=(14, 13), head=(-1, -4), hr=(22, 16), hl=(11, 17), eye=1)),
            A(pose(top=(13, 13), head=(-1, -4), hr=(22, 15), hl=(10, 17), eye=1)),
            A(pose(top=(15, 12), eye=1))]
    return {"idle": (idle, 4), "walk": (walk, 8), "suspended": (susp, 3), "wake": (wake, 4),
            "collapse": (collapse, 6), "look_hands": (look, 4), "attack": (attack, 12), "hurt": (hurt, 8)}


def guard():
    o = OUTFITS["guard"]
    G = lambda p, lie=False: flip(render(p, o, lie))  # guards face left (towards Aeron)
    g = dict(gun=True, hl=(21, 19), hr=(19, 18))
    idle = [G(pose(top=(16, 12 + b), **g)) for b in (0, 0, 1, 1)]
    aim = [G(pose(top=(17, 12), hr=(20, 17), hl=(22, 18), gun=True, fl=(13, 29), fr=(19, 29))),
           G(pose(top=(17, 12), hr=(20, 17), hl=(22, 18), gun=True, fl=(13, 29), fr=(19, 29)))]
    shoot = [G(pose(top=(17, 12), hr=(20, 17), hl=(22, 18), gun=True)),
             G(pose(top=(15, 12), hr=(18, 18), hl=(20, 18), gun=True)),
             G(pose(top=(16, 12), hr=(19, 17), hl=(21, 18), gun=True))]
    hurt = [G(pose(top=(14, 13), head=(-1, -4), hr=(17, 20), hl=(12, 18), gun=True)),
            G(pose(top=(13, 14), head=(-1, -3), hr=(16, 21), hl=(11, 19), gun=False))]
    dead = [G(pose(top=(14, 13), head=(-1, -4), hr=(16, 21), hl=(11, 19))),
            G(pose(hip=(16, 25), top=(15, 19), head=(1, -2), hl=(12, 28), hr=(18, 28), fl=(10, 29), fr=(19, 29))),
            G(pose(hip=(16, 20), top=(16, 12), head=(2, -3), hl=(13, 20), hr=(19, 20)), lie=True),
            G(pose(hip=(16, 20), top=(16, 12), head=(2, -3), hl=(13, 20), hr=(19, 20)), lie=True)]
    return {"idle": (idle, 4), "aim": (aim, 6), "shoot": (shoot, 10), "hurt": (hurt, 8), "dead": (dead, 6)}


def voss():
    o = OUTFITS["voss"]
    V = lambda p: render(p, o)
    idle = [V(pose(top=(16, 12 + b), hl=(14, 18 + b), hr=(18, 18 + b), tablet=True)) for b in (0, 0, 1, 1)]
    react = [V(pose(top=(16, 12), head=(0, -4), hl=(17, 11), hr=(18, 10), eye=1)),
             V(pose(top=(15, 12), head=(-1, -4), hl=(17, 11), hr=(18, 10), eye=1))]
    return {"idle": (idle, 4), "afraid": (react, 6)}

