import json

log_path = r"C:\Users\Surya VM\.gemini\antigravity-ide\brain\0f1140e2-afca-47d8-9ec8-d30f22d917c9\.system_generated\logs\transcript_full.jsonl"

with open(log_path, "r", encoding="utf-8") as f:
    for line in f:
        data = json.loads(line)
        if data.get("type") == "USER_INPUT":
            last_user = data

print("Keys:", list(last_user.keys()))
for k, v in last_user.items():
    if k == "content":
        if isinstance(v, list):
            print("Content is list of length", len(v))
            for item in v:
                print("  Item type:", type(item), "keys:", getattr(item, 'keys', lambda: None)())
                if isinstance(item, dict):
                    for ik, iv in item.items():
                        if ik != "text":
                            print("    ", ik, ":", str(iv)[:200])
        else:
            print("Content is str of len", len(v))
    else:
        print(f"{k}: {str(v)[:200]}")
