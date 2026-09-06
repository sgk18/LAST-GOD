import os, re

guid_pattern = re.compile(r'm_Script: \{fileID: \d+, guid: ([a-f0-9]+)')

# Collect all guids in project .meta files
known_guids = set()
for root, dirs, files in os.walk('.'):
    if '.git' in root or 'Library' in root or 'Temp' in root:
        continue
    for f in files:
        if f.endswith('.meta'):
            path = os.path.join(root, f)
            with open(path, 'r', encoding='utf-8', errors='ignore') as mf:
                for line in mf:
                    if line.startswith('guid: '):
                        known_guids.add(line.strip().split()[1])
                        break

print(f"Total known meta GUIDs: {len(known_guids)}")

# Now scan Act1_Scene1_Lab.unity
for scene_name in ['Assets/Scenes/Act1_Scene1_Lab.unity', 'LAST-GOD/Assets/Scenes/Act1_Scene1_Lab.unity', 'Assets/Scenes/Act1_Scene1.unity']:
    if not os.path.exists(scene_name):
        continue
    with open(scene_name, 'r', encoding='utf-8', errors='ignore') as sf:
        lines = sf.readlines()
    
    print(f"\n--- Scanning {scene_name} ---")
    current_gameobject = ""
    current_fileid = ""
    for i, line in enumerate(lines):
        if line.startswith('--- !u!1 &'):
            # GameObject header
            current_fileid = line.strip()
        if 'm_Name:' in line:
            current_gameobject = line.strip()
        m = guid_pattern.search(line)
        if m:
            guid = m.group(1)
            if guid not in known_guids:
                print(f"MISSING GUID at line {i+1}: {guid} (near {current_gameobject} / {current_fileid})")
            else:
                print(f"OK GUID at line {i+1}: {guid}")
