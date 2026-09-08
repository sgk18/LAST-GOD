import re

with open("Assets/Scenes/Act1_Origin.unity", "r", encoding="utf-8") as f:
    text = f.read()

docs = text.split("--- !u!")

# Find Environment_Laboratory (3000000) and children
env_ids = ["3000000", "3000010", "3000020", "3000030", "3000040", "3000050", "3000060"]

print("=== CHECKING ENVIRONMENT_LABORATORY HIERARCHY ===")
for doc in docs:
    for env_id in env_ids:
        if f"&{env_id}\n" in doc or f"&{env_id} " in doc or f"m_GameObject: {{fileID: {env_id}}}" in doc:
            lines = doc.strip().split("\n")
            print(f"--- {lines[0]}")
            for l in lines[1:8]:
                print(f"    {l}")
