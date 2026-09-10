import re

with open('Assets/Scenes/2.5D_Lab_Scene.unity', 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')
print(f"Total YAML documents: {len(docs)}")

type_counts = {}
for doc in docs[1:]:
    header, _, body = doc.partition('\n')
    m = re.match(r'(\d+) &(\d+)', header.strip())
    if m:
        t, fid = m.groups()
        type_counts[t] = type_counts.get(t, 0) + 1

print("Document types:", type_counts)

# Print PrefabInstances
for doc in docs[1:]:
    if doc.startswith('1001 '):
        lines = doc.strip().split('\n')
        print("PrefabInstance header:", lines[0])
        for l in lines[:25]:
            if 'm_SourcePrefab' in l or 'm_RootGameObject' in l or 'm_Name' in l:
                print("  ", l)
