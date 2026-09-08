import re

with open("Assets/Scenes/Act1_Origin.unity", "r", encoding="utf-8") as f:
    text = f.read()

docs = text.split("--- !u!")

print("=== DIAGNOSING ALL RENDERERS IN Act1_Origin.unity ===")
for doc in docs:
    if not doc.strip(): continue
    first = doc.strip().split("\n")[0]
    if "m_Materials:" in doc or "m_Sprite:" in doc:
        go_id = re.search(r"m_GameObject:\s*\{fileID:\s*(\d+)\}", doc)
        go_name = "Unknown"
        if go_id:
            for d2 in docs:
                if f"1 &{go_id.group(1)}" in d2:
                    nm = re.search(r"m_Name:\s*(.*)", d2)
                    if nm: go_name = nm.group(1).strip()
        mat = re.findall(r"m_Materials:\n((?:\s*-\s*\{[^\}]+\}\n?)+)", doc)
        sprite = re.search(r"m_Sprite:\s*\{fileID:\s*(\d+),\s*guid:\s*([a-f0-9]+),\s*type:\s*(\d+)\}", doc)
        print(f"GameObject: \"{go_name}\" ({first})")
        if mat:
            print(f"  Materials:\n    {mat[0].strip()}")
        if sprite:
            print(f"  Sprite: fileID={sprite.group(1)}, guid={sprite.group(2)}")
