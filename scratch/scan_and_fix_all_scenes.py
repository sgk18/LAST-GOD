import os
import re

project_root = r"C:\projects\LAST-GOD"

# Step 1: Collect valid GUIDs from all .meta files (including Library/PackageCache)
guid_to_path = {}
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
                        guid_to_path[guid] = meta_path[:-5]
            except Exception:
                pass

print(f"Total valid GUIDs in project + packages: {len(guid_to_path)}")

# Step 2: Define fixes for known dummy/fake GUIDs
guid_replacements = {
    "073797305484433d9692482318e8749a": "073797afb82c5a1438f328866b10b3f0", # Light2D
    "a860cb24a0cc4735a80140457adc180b": "6a160d838ff8b4b4693ac20007e008c7", # PixelPerfectCamera
    "6a1523450901e4a4eb8b8098c764e526": "6a160d838ff8b4b4693ac20007e008c7", # PixelPerfectCamera
    "a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2": "2b966ba5223494842b8bef66df566975", # Health.cs
    "6055be8ebefd69e48b49212b09b47b2f": "06eed5e9c7a77c64e9637efc06e0b51c", # PlayerController.cs
    "2bafac87e7f4b9b418d9448d219b01ab": "6eddb6523950d104492ba93573f8f405", # CameraFollow.cs
}

# Scan all scene and prefab files
affected_files = {}

for root, dirs, files in os.walk(project_root):
    if any(ignore in root for ignore in ['.git', 'Library', 'Temp', 'Logs', 'scratch', '.vscode', '.antigravitycli', 'Packages']):
        continue
    for file in files:
        if file.endswith(('.unity', '.prefab')):
            file_path = os.path.join(root, file)
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            
            new_content = content
            replaced_in_file = []
            
            for old_guid, new_guid in guid_replacements.items():
                if old_guid in new_content:
                    count = new_content.count(old_guid)
                    new_content = new_content.replace(old_guid, new_guid)
                    replaced_in_file.append((old_guid, new_guid, count))
            
            if replaced_in_file:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                affected_files[file_path] = replaced_in_file

print(f"\n--- FIXED SCENES AND PREFABS ---")
for file_path, reps in affected_files.items():
    print(f"\nFile: {file_path}")
    for old_g, new_g, count in reps:
        target_script = os.path.basename(guid_to_path.get(new_g, "Unknown"))
        print(f"  Replaced {old_g} -> {new_g} ({target_script}) x{count}")
