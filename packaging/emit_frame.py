import sys, os, io, base64
from PIL import Image

name, path, prefix, quality, width = sys.argv[1], sys.argv[2], sys.argv[3], int(sys.argv[4]), int(sys.argv[5])
if not os.path.exists(path):
    print("missing frame: " + path)
    raise SystemExit(0)

# Headless Skia frames contain transparent pixels whose RGB channels hold
# stale framebuffer data. Composite onto white before any resize/compression,
# otherwise those bytes show up as coloured blocks and ghost text.
rgba = Image.open(path).convert("RGBA")
flat = Image.new("RGBA", rgba.size, (255, 255, 255, 255))
flat.alpha_composite(rgba)
source = flat.convert("RGB")
best = None
for w in (width, int(width * 0.8), int(width * 0.65)):
    for q in (quality, quality - 12, 58, 46):
        image = source
        if w > 0 and image.width > w:
            image = image.resize((w, int(image.height * w / image.width)), Image.LANCZOS)
        buf = io.BytesIO()
        image.save(buf, "JPEG", quality=q, optimize=True)
        blob = base64.b64encode(buf.getvalue()).decode()
        chunks = [blob[i:i + 4000] for i in range(0, len(blob), 5000)]
        if len(chunks) <= 9 and len(prefix) + len(name) + 6 + 4000 <= 4096:
            best = (blob, chunks, q, w)
            break
    if best:
        break

if not best:
    print("frame too large: " + name)
    raise SystemExit(0)

blob, chunks, q, w = best
for i, chunk in enumerate(chunks):
    print("::warning::%s:%s:%d:%s" % (prefix, name, i, chunk))
print("%s_LEN:%s:%d:%d:%d:%d" % (prefix, name, len(blob), len(chunks), q, w))
