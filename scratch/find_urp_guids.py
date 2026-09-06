import os, re

with open('LAST-GOD/Assets/Scenes/Act1_Level1_Village.unity', 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Look for Light2D or PixelPerfectCamera
for m in re.finditer(r'MonoBehaviour:[^\n]*\n(?:[^\n]*\n){1,10}?[^\n]*guid: ([a-f0-9]+)', content):
    full_block = m.group(0)
    if 'PixelPerfect' in full_block or 'Light' in full_block or 'Camera' in full_block:
        print(full_block)
        print("="*40)
