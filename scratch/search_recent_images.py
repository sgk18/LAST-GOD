import os
import time

now = time.time()
search_roots = [
    r"c:\projects\LAST-GOD",
    r"C:\Users\Surya VM\Downloads",
    r"C:\Users\Surya VM\Desktop",
    r"C:\Users\Surya VM\.gemini",
    r"C:\Users\Surya VM\AppData\Local\Temp"
]

found = []
for root_path in search_roots:
    if not os.path.exists(root_path):
        continue
    for root, dirs, files in os.walk(root_path):
        # Skip Library and node_modules
        if "Library" in root or ".git" in root or "PackageCache" in root:
            continue
        for f in files:
            if f.lower().endswith((".png", ".jpg", ".jpeg", ".webp", ".ase", ".aseprite", ".psd")):
                full_path = os.path.join(root, f)
                try:
                    mtime = os.path.getmtime(full_path)
                    if now - mtime < 7200: # last 2 hours
                        size = os.path.getsize(full_path)
                        found.append((mtime, size, full_path))
                except Exception:
                    pass

found.sort(reverse=True)
print(f"Found {len(found)} images in last 2 hours:")
for mtime, size, p in found[:30]:
    t_str = time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(mtime))
    print(f"  [{t_str}] {size:>9} bytes : {p}")
