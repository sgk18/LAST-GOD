import os
import re

project_root = r"C:\projects\LAST-GOD"

# Build complete GUID dictionary including Library/PackageCache
guid_to_info = {}

for root, dirs, files in os.walk(project_root):
    if any(ignore in root for ignore in ['.git', 'Temp', 'Logs', 'scratch', '.vscode', '.antigravitycli']):
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
                        guid_to_info[guid] = target_file
            except Exception as e:
                pass

print(f"Total GUIDs mapped (including PackageCache): {len(guid_to_info)}")

# Find scenes in Assets (excluding Samples, Packages, etc)
project_scenes = [
    r"C:\projects\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"C:\projects\LAST-GOD\Assets\Scenes\Act1_Level1_Village.unity",
    r"C:\projects\LAST-GOD\Assets\Scenes\Act1_Origin.unity",
    r"C:\projects\LAST-GOD\Assets\Scenes\Aeron_Idle_Test.unity",
    r"C:\projects\LAST-GOD\Assets\Scenes\MainMenu_Origin.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Level1_Village.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Origin.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Aeron_Idle_Test.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\MainMenu_Origin.unity",
]

for scene_path in project_scenes:
    if not os.path.exists(scene_path):
        continue
    
    with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    
    # parse blocks
    blocks = content.split('--- !u!')
    
    missing_in_scene = []
    
    for b in blocks:
        if not b.startswith('114 ') and 'MonoBehaviour:' not in b:
            continue
        
        # Get GameObject name
        go_name = "Unknown GameObject"
        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
        if m_go:
            go_id = m_go.group(1)
            # Find GameObject block with id &go_id
            m_goname = re.search(r'--- !u!1 &' + go_id + r'\b.*?\nm_Name:\s*([^\r\n]+)', content, re.DOTALL)
            if m_goname:
                go_name = m_goname.group(1)
        
        m_script = re.search(r'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([a-f0-9]{32})', b)
        if m_script:
            guid = m_script.group(1)
            if guid not in guid_to_info:
                missing_in_scene.append((go_name, guid))
            else:
                script_path = guid_to_info[guid]
                # print(f"Valid: {go_name} -> {os.path.basename(script_path)}")
    
    print(f"\n==========================================")
    print(f"Scene: {scene_path}")
    print(f"Total invalid/missing script GUIDs: {len(missing_in_scene)}")
    for go_name, guid in missing_in_scene:
        print(f"  [MISSING] GameObject: '{go_name}' | GUID: {guid}")
