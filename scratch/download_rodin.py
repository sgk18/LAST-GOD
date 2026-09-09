import requests
import json
import os

task_uuid = "9ad43926-575c-478e-9f1e-cf978a9c7f5c"
api_key = "vibecoding"

response = requests.post(
    "https://hyperhuman.deemos.com/api/v2/download",
    headers={
        "Authorization": f"Bearer {api_key}",
    },
    json={
        'task_uuid': task_uuid
    }
)

print("Status:", response.status_code)
data = response.json()
print("Data:", json.dumps(data, indent=2))

out_dir = os.path.abspath("Assets/Characters/Aeron/Rodin")
os.makedirs(out_dir, exist_ok=True)

if "list" in data:
    for item in data["list"]:
        name = item["name"]
        url = item["url"]
        out_path = os.path.join(out_dir, name)
        print(f"Downloading {name}...")
        r = requests.get(url, stream=True)
        r.raise_for_status()
        with open(out_path, 'wb') as f:
            for chunk in r.iter_content(chunk_size=8192):
                f.write(chunk)
        print(f"Saved: {out_path} ({os.path.getsize(out_path)} bytes)")
