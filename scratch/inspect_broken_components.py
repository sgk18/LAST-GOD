import os
import re

project_root = r"C:\projects\LAST-GOD"

# Build complete GUID dictionary
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
                        guid_to_info[guid] = meta_path[:-5]
            except Exception as e:
                pass

scenes = [
    r"C:\projects\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"C:\projects\LAST-GOD\Assets\Scenes\Act1_Level1_Village.unity",
    r"C:\projects\LAST-GOD\Assets\Scenes\Aeron_Idle_Test.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Scene1.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Act1_Level1_Village.unity",
    r"C:\projects\LAST-GOD\LAST-GOD\Assets\Scenes\Aeron_Idle_Test.unity",
]

for sc in scenes:
    if not os.path.exists(sc):
        continue
    print(f"\n=======================================================")
    print(f"FILE: {sc}")
    with open(sc, 'r', encoding='utf-8', errors='ignore') as f:
        lines = f.readlines()
    
    # Parse YAML documents
    doc_lines = []
    current_header = ""
    
    for i, line in enumerate(lines):
        if line.startswith('--- !u!'):
            if doc_lines:
                doc_text = "".join(doc_lines)
                m_script = re.search(r'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([a-f0-9]{32})', doc_text)
                if m_script:
                    guid = m_script.group(1)
                    if guid not in guid_to_info:
                        # Find GameObject reference inside MonoBehaviour
                        m_go = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc_text)
                        go_id = m_go.group(1) if m_go else None
                        
                        # Find GameObject name
                        go_name = "Unknown"
                        if go_id:
                            pattern = r'--- !u!1 &' + go_id + r'\b.*?\nm_Name:\s*([^\r\n]+)'
                            m_name = re.search(pattern, "".join(lines), re.DOTALL)
                            if m_name:
                                go_name = m_name.group(1)
                        
                        print(f"Line {doc_start_line}: GameObject '{go_name}' | Bad GUID: {guid}")
                        print("  Doc Snippet:")
                        for dl in doc_lines[:15]:
                            print("    " + dl.strip())
            doc_lines = [line]
            doc_start_line = i + 1
        else:
            doc_lines.append(line)
            
    if doc_lines:
        doc_text = "".join(doc_lines)
        m_script = re.search(r'm_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([a-f0-9]{32})', doc_text)
        if m_script:
            guid = m_script.group(1)
            if guid not in guid_to_info:
                print(f"Line {doc_start_line}: Bad GUID: {guid}")
