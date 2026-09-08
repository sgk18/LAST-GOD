import subprocess
import json
import base64

res = subprocess.run(["cmd.exe", "/c", "npx", "unity-mcp-cli", "run-tool", "screenshot-game-view", "--path", "c:/projects/LAST-GOD"], capture_output=True, text=True, check=True)
output = res.stdout

start_idx = output.find('{')
end_idx = output.rfind('}')
if start_idx != -1 and end_idx != -1:
    data = json.loads(output[start_idx:end_idx+1])
    content = data.get("content", [])
    if len(content) > 1:
        img_b64 = content[1]["data"]
        img_bytes = base64.b64decode(img_b64)
        dest = "scratch/latest_game_view.png"
        with open(dest, "wb") as f:
            f.write(img_bytes)
        print("Successfully saved screenshot to", dest, "size:", len(img_bytes), "bytes")
