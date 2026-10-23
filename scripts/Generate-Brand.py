#!/usr/bin/env python3
"""Deterministic production trace of the approved compact Captovix concept.
Requires cairosvg and Pillow; not needed to build because assets are checked in.
"""
from pathlib import Path
from io import BytesIO
import cairosvg
from PIL import Image
assets=Path(__file__).resolve().parent.parent/'src/Captovix/Assets'
mark=(assets/'Captovix-mark.svg').read_text()
cairosvg.svg2png(bytestring=mark.encode(),write_to=str(assets/'Captovix-mark.png'),output_width=256,output_height=256)
cairosvg.svg2png(bytestring=mark.replace('#7C3AED','#D6C5FF').encode(),write_to=str(assets/'Captovix-mark-light.png'),output_width=256,output_height=256)
white=mark[mark.index('<g '):mark.index('</svg>')].replace('#7C3AED','#FFFFFF')
tile='<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256"><rect x="0" y="0" width="256" height="256" rx="48" fill="#7C3AED"/><g transform="translate(47 47) scale(.633)">'+white+'</g></svg>'
(assets/'Captovix-app.svg').write_text(tile)
png=cairosvg.svg2png(bytestring=tile.encode(),output_width=1024,output_height=1024)
image=Image.open(BytesIO(png)).convert('RGBA')
image.resize((256,256),Image.Resampling.LANCZOS).save(assets/'Captovix-app.png')
image.save(assets/'Captovix.ico',format='ICO',sizes=[(n,n) for n in (16,20,24,32,40,48,64,128,256)])
print('Brand PNG, SVG, and multi-resolution ICO generated.')
