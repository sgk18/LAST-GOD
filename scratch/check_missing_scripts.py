import os
import re

project_root = r"C:\projects\LAST-GOD"

# Step 1: Collect all GUIDs from all .meta files
guid_to_meta = {}
cs_file_to_guid = {}

for root, dirs, files in os.walk(project_root):
    # skip .git, Library, Temp, etc.
    if any(ignore in root for ignore in ['.git', 'Library', 'Temp', 'Logs', 'scratch', '.vscode', '.antigravitycli']):
        continue
    for file in files:
        if file.endswith('.meta'):
            meta_path = os.path.join(root, file)
            try:
                with open(meta_path, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                    m = re.search(r'guid:\s*([a-f0-9]{32})', content)
                    if m:
                        guid = m.group(1)
                        target_file = meta_path[:-5]
                        guid_to_meta[guid] = target_file
                        if target_file.endswith('.cs'):
                            cs_file_to_guid[target_file] = guid
            except Exception as e:
                pass

print(f"Total .meta GUIDs found: {len(guid_to_meta)}")
print(f"Total .cs files found: {len(cs_file_to_guid)}")

# Step 2: Scan scenes and prefabs for m_Script references
missing_report = []

scene_prefab_extensions = ('.unity', '.prefab')

for root, dirs, files in os.walk(project_root):
    if any(ignore in root for ignore in ['.git', 'Library', 'Temp', 'Logs', 'scratch', '.vscode', '.antigravitycli']):
        continue
    for file in files:
        if file.endswith(scene_prefab_extensions):
            asset_path = os.path.join(root, file)
            try:
                with open(asset_path, 'r', encoding='utf-8', errors='ignore') as f:
                    lines = f.readlines()
                
                current_gameobject = None
                current_component = None
                
                for i, line in enumerate(lines):
                    if line.startswith('--- !u!1 '): # GameObject
                        current_gameobject = line.strip()
                    elif line.startswith('--- !u!114 '): # MonoBehaviour
                        current_component = line.strip()
                    
                    m = re.search(r'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([a-f0-9]{32})', line)
                    if m:
                        guid = m.group(1)
                        if guid not in guid_to_meta:
                            missing_report.append({
                                'file': asset_path,
                                'line_no': i + 1,
                                'guid': guid,
                                'raw_line': line.strip()
                            })
            except Exception as e:
                print(f"Error reading {asset_path}: {e}")

print(f"\n--- MISSING SCRIPT GUID SUMMARY ({len(missing_report)} occurrences) ---")
missing_guids = set()
for item in missing_report:
    missing_guids.add(item['guid'])
    print(f"File: {item['file']}:{item['line_no']} | GUID: {item['guid']}")

print(f"\nUnique missing GUIDs: {len(missing_guids)}")
for g in missing_guids:
    print(f"Missing GUID: {g}")
