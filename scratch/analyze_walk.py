from PIL import Image
import numpy as np

im = Image.open(r'C:\Users\Surya VM\Downloads\walk.png')
print('Walk.png size:', im.size)
arr = np.array(im)

# Top half is the 8 frames
top_half = arr[:430, :, :]
col_alpha = (top_half[:, :, 3] > 20).sum(axis=0)

in_frame = False
spans = []
start = 0
for x in range(len(col_alpha)):
    if col_alpha[x] > 10 and not in_frame:
        in_frame = True
        start = x
    elif col_alpha[x] <= 10 and in_frame:
        in_frame = False
        if x - start > 40:
            spans.append((start, x))

print('Found spans in top half:', len(spans))
for i, (s, e) in enumerate(spans):
    print(f'Span {i}: {s} to {e} (width {e-s})')
