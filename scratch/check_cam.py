with open("Assets/Scenes/Act1_Origin.unity", "r", encoding="utf-8") as f:
    text = f.read()

docs = text.split("--- !u!")
for doc in docs:
    if "1000000" in doc or "1000001" in doc or "1000002" in doc or "1000003" in doc or "1000004" in doc or "1000005" in doc:
        print("--- !u!" + doc)
