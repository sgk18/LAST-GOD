import os
import re

project_root = r"C:\projects\LAST-GOD"

meta_files = []
for root, dirs, files in os.walk(project_root):
    if any(ignore in root for ignore in ['.git', 'Temp', 'Logs', 'scratch', '.vscode', '.antigravitycli']):
        continue
    for file in files:
        if file.endswith('.meta'):
            meta_files.append(os.path.join(root, file))

print(f"Searching {len(meta_files)} meta files...")

targets = [
    'Light2D',
    'UniversalAdditionalCameraData',
    'PixelPerfectCamera',
    'CameraFollow',
    'PlayerController',
    'Health',
    'Act1Scene1SequenceManager',
    'CutsceneUIController',
    'CameraShake2D',
    'InteractiveDummy',
    'HazardSpike',
    'InteractiveChest'
]

results = {}

for meta in meta_files:
    target_name = meta[:-5]
    base_name = os.path.basename(target_name)
    name_no_ext = os.path.splitext(base_name)[0]
    
    for t in targets:
        if name_no_ext.lower() == t.lower():
            try:
                with open(meta, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                    m = re.search(r'guid:\s*([a-f0-9]{32})', content)
                    if m:
                        guid = m.group(1)
                        if t not in results:
                            results[t] = []
                        results[t].append((guid, meta))
            except Exception as e:
                pass

for t, hits in results.items():
    print(f"\nTarget: {t}")
    for guid, path in hits:
        print(f"  GUID: {guid} -> {path}")
