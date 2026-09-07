import os

logs = [
    r'C:\projects\LAST-GOD\Logs\Editor.log',
    r'C:\projects\LAST-GOD\LAST-GOD\Logs\Editor.log'
]

for l in logs:
    if os.path.exists(l):
        print(f"=== {l} (size: {os.path.getsize(l)}) ===")
        with open(l, 'r', encoding='utf-8', errors='ignore') as f:
            lines = f.readlines()
        print(f"Total lines: {len(lines)}")
        errs = [line.strip() for line in lines if 'error cs' in line.lower() or 'compilation' in line.lower()]
        print(f"Found {len(errs)} mentions:")
        for err in errs[-15:]:
            print("  ", err)
