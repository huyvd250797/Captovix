#!/usr/bin/env python3
"""Remove reproducible debug/build residue only; retain native/runtime/OCR data."""
from pathlib import Path
import argparse,json
p=argparse.ArgumentParser();p.add_argument('payload',type=Path);p.add_argument('--report',type=Path);a=p.parse_args();root=a.payload.resolve()
if not (root/'Captovix.exe').is_file():raise SystemExit('Not a Captovix publish directory')
files=list(root.rglob('*'));before=sum(f.stat().st_size for f in files if f.is_file());removed=[]
for f in files:
    if not f.is_file():continue
    # dotnet copy residue has a leading dot, original file name and random suffix.
    temp=f.name.startswith('.') and '.dll.' in f.name and (f.parent/f.name[1:].rsplit('.',1)[0]).is_file()
    if f.suffix.lower() in {'.pdb','.xml'} or temp:
        removed.append({'path':str(f.relative_to(root)),'bytes':f.stat().st_size});f.unlink()
result={'before_bytes':before,'after_bytes':sum(f.stat().st_size for f in root.rglob('*') if f.is_file()),'removed':removed}
if a.report:a.report.parent.mkdir(parents=True,exist_ok=True);a.report.write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
