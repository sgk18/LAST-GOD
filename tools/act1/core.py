"""Core helpers for THE LAST GOD Act 1 pixel-art generator (16-colour palette)."""
import json
import os
import subprocess
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "Art", "Act1")
ASEPRITE = r"C:\projects\aseprite\build\bin\aseprite.exe"

HEX = {
    1: "#0b0d14", 2: "#1c2233", 3: "#2f3b52", 4: "#4f6a85", 5: "#8fb3c4",
    6: "#d7eef0", 7: "#5a1f2b", 8: "#b3263e", 9: "#ff5a5f", 10: "#3a2a24",
    11: "#c89b7b", 12: "#8c6450", 13: "#e8c34a", 14: "#2f8f83", 15: "#7fe3d0",
}


def rgb(i):
    h = HEX[i]
    return (int(h[1:3], 16), int(h[3:5], 16), int(h[5:7], 16))


def R(i, a=255):
    return rgb(i) + (a,)


def new(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def outline(im, col=1):
    w, h = im.size
    src = im.load()
    out = im.copy()
    o = out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 0:
                        o[x, y] = R(col)
                        break
    return out


def flip(im):
    return im.transpose(Image.FLIP_LEFT_RIGHT)


def path(*parts):
    p = os.path.join(OUT, *parts)
    os.makedirs(os.path.dirname(p), exist_ok=True)
    return p


def write_palette():
    with open(path("palette.gpl"), "w") as f:
        f.write("GIMP Palette\nName: LAST-GOD Act1 16\nColumns: 4\n#\n")
        for i in range(1, 16):
            r, g, b = rgb(i)
            f.write(f"{r:3d} {g:3d} {b:3d}\tcolor{i}\n")
    sw = new(15 * 8, 8)
    d = ImageDraw.Draw(sw)
    for i in range(1, 16):
        d.rectangle([(i - 1) * 8, 0, i * 8 - 1, 7], fill=R(i))
    sw.save(path("palette.png"))


def quantize(im):
    pal = Image.new("P", (1, 1))
    flat = []
    for i in range(1, 16):
        flat += list(rgb(i))
    flat += [0] * (768 - len(flat))
    pal.putpalette(flat)
    return im.convert("RGB").quantize(palette=pal, dither=Image.Dither.NONE).convert("RGBA")


LUA = """
local png=app.params["png"]; local out=app.params["out"]
local fw=tonumber(app.params["fw"]); local fh=tonumber(app.params["fh"]); local n=tonumber(app.params["n"])
local tags=app.params["tags"]
local src=Image{fromFile=png}
local spr=Sprite(fw,fh,ColorMode.RGB)
for i=2,n do spr:newEmptyFrame() end
for i=1,n do
  local img=Image(fw,fh,ColorMode.RGB)
  img:drawImage(src,Point(-(i-1)*fw,0))
  spr:newCel(spr.layers[1],spr.frames[i],img,Point(0,0))
end
for t in string.gmatch(tags,"[^,]+") do
  local name,a,b,fps=string.match(t,"([^:]+):(%d+):(%d+):(%d+)")
  local tag=spr:newTag(tonumber(a)+1,tonumber(b)+1); tag.name=name
  for i=tonumber(a)+1,tonumber(b)+1 do spr.frames[i].duration=1/tonumber(fps) end
end
spr:saveAs(out)
"""

SHEETS = []


def sheet(folder, name, anims, to_aseprite=True):
    """anims: ordered dict name -> (list of equal-size RGBA frames, fps)."""
    frames, tags, idx = [], {}, 0
    for aname, (fr, fps) in anims.items():
        tags[aname] = {"from": idx, "to": idx + len(fr) - 1, "fps": fps}
        frames += fr
        idx += len(fr)
    fw, fh = frames[0].size
    st = new(fw * len(frames), fh)
    for i, f in enumerate(frames):
        st.paste(f, (i * fw, 0), f)
    png = path(folder, name + ".png")
    st.save(png)
    meta = {"image": name + ".png", "frame_w": fw, "frame_h": fh,
            "frame_count": len(frames), "tags": tags}
    with open(path(folder, name + ".json"), "w") as f:
        json.dump(meta, f, indent=2)
    if to_aseprite:
        SHEETS.append((png, path(folder, name + ".aseprite"), fw, fh, len(frames),
                       ",".join(f"{k}:{v['from']}:{v['to']}:{v['fps']}" for k, v in tags.items())))
    return st


def build_aseprite():
    lua = path("_export.lua")
    with open(lua, "w") as f:
        f.write(LUA)
    ok = 0
    for png, out, fw, fh, n, tags in SHEETS:
        r = subprocess.run([ASEPRITE, "-b", "--script-param", f"png={png}", "--script-param", f"out={out}",
                            "--script-param", f"fw={fw}", "--script-param", f"fh={fh}",
                            "--script-param", f"n={n}", "--script-param", f"tags={tags}",
                            "--script", lua], capture_output=True, text=True)
        if os.path.exists(out):
            ok += 1
        else:
            print("aseprite failed:", out, r.stdout[-200:], r.stderr[-200:])
    os.remove(lua)
    print(f"aseprite files written: {ok}/{len(SHEETS)}")

