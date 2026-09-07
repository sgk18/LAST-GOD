import os
import re

project_root = r"C:\projects\LAST-GOD"

guid_to_meta = {}
guid_to_path = {}

for root, dirs, files in os.walk(project_root):
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
                        guid_to_meta[guid] = meta_path
                        guid_to_path[guid] = target_file
            except Exception as e:
                pass

scene_files = []
for root, dirs, files in os.walk(project_root):
    if any(ignore in root for ignore in ['.git', 'Library', 'Temp', 'Logs', 'scratch', '.vscode', '.antigravitycli']):
        continue
    for file in files:
        if file.endswith(('.unity', '.prefab')):
            scene_files.append(os.path.join(root, file))

results = []

for sfile in scene_files:
    try:
        with open(sfile, 'r', encoding='utf-8', errors='ignore') as f:
            lines = f.readlines()
        
        # Parse YAML objects
        yaml_objects = {}
        current_id = None
        current_block = []
        
        for line in lines:
            if line.startswith('--- !u!'):
                if current_id:
                    yaml_objects[current_id] = current_block
                current_id = line.strip().split()[-1] # e.g. &12345
                current_block = [line]
            else:
                if current_id:
                    current_block.append(line)
        if current_id:
            yaml_objects[current_id] = current_block
            
        # Find MonoBehaviour blocks with missing GUIDs
        for obj_id, block_lines in yaml_objects.items():
            block_text = "".join(block_lines)
            if 'MonoBehaviour:' in block_lines[0] or '!u!114' in block_lines[0]:
                m = re.search(r'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([a-f0-9]{32})', block_text)
                if m:
                    guid = m.group(1)
                    if guid not in guid_to_meta:
                        # Find GameObject name if possible
                        go_name = "Unknown GameObject"
                        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', block_text)
                        if go_m:
                            go_id = "&" + go_m.group(1)
                            if go_id in yaml_objects:
                                go_block = "".join(yaml_objects[go_id])
                                name_m = re.search(r'm_Name:\s*(.*)', go_block)
                                if name_m:
                                    go_name = name_m.group(1).strip()
                        
                        # Find component name / fields in block
                        fields = []
                        for l in block_lines:
                            if ':' in l and not l.strip().startswith('m_'):
                                fields.append(l.strip())
                        
                        results.append({
                            'scene': sfile,
                            'go_name': go_name,
                            'guid': guid,
                            'fields': fields[:10]
                        })
    except Exception as e:
        print(f"Error reading {sfile}: {e}")

print("=== DETAILED MISSING SCRIPT USAGE ===")
for r in results:
    print(f"Scene: {r['scene']}")
    print(f"  GameObject: {r['go_name']}")
    print(f"  Missing GUID: {r['guid']}")
    print(f"  Sample Fields: {r['fields'][:5]}")
    print("-" * 50)
