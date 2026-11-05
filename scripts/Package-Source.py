#!/usr/bin/env python3
"""Create the versioned clean source ZIP. Never package runtime/dev secrets."""
from pathlib import Path
import zipfile,xml.etree.ElementTree as ET,argparse
root=Path(__file__).resolve().parent.parent
project=ET.parse(root/'src/Captovix/Captovix.csproj')
version=project.findtext('.//Version')
title=project.findtext('.//ReleaseTitle') or 'Performance & Responsiveness'
excluded={'bin','obj','artifacts','.git','.vs','__pycache__','node_modules','secrets','prerequisites'}
files=[p for p in root.rglob('*') if p.is_file() and not any(x in excluded for x in p.relative_to(root).parts) and '_wpftmp' not in p.name and p.suffix not in {'.pdb','.tmp','.log','.exe','.deb','.pyc'}]
for p in files:
    if p.name in {'server.env','edge-secrets.env','captovix-signing.pem','.env','.env.local'} or p.name.endswith('.private.pem'):
        raise SystemExit('Refusing to package a secret file: '+p.relative_to(root).as_posix())
parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path);args=parser.parse_args()
target=args.output or root/'artifacts'/f'Captovix V{version} — {title}.zip';target.parent.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED,6) as z:
    for p in files:z.write(p,f'Captovix-v{version}/'+p.relative_to(root).as_posix())
print(target)
