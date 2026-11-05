#!/usr/bin/env python3
"""Read-only file sizes and exact duplicate groups. Never deletes user media."""
from pathlib import Path
import argparse,hashlib,json,collections
p=argparse.ArgumentParser();p.add_argument('folder',type=Path);p.add_argument('--output',type=Path);a=p.parse_args();root=a.folder.resolve();files=[];warnings=[]
for f in root.rglob('*'):
    if f.is_symlink() or any(x.is_symlink() for x in f.parents if x!=root):continue
    if f.is_file():
        try:files.append((f,f.stat().st_size))
        except OSError as e:warnings.append(str(e))
groups=collections.defaultdict(list)
for f,size in files:groups[size].append(f)
duplicates=[]
for size,paths in groups.items():
    if size==0 or len(paths)<2:continue
    hashes=collections.defaultdict(list)
    for f in paths:
        try:
            with f.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
            hashes[digest].append(str(f.relative_to(root)))
        except OSError as e:warnings.append(str(e))
    for digest,items in hashes.items():
        if len(items)>1:duplicates.append({'bytes_each':size,'sha256':digest,'paths':items,'action':'review only; paths may be runtime contracts'})
result={'folder':str(root),'file_count':len(files),'bytes':sum(s for _,s in files),'largest':[{'path':str(f.relative_to(root)),'bytes':s} for f,s in sorted(files,key=lambda v:v[1],reverse=True)[:20]],'duplicates':duplicates,'warnings':warnings}
text=json.dumps(result,indent=2,ensure_ascii=False)
if a.output:a.output.write_text(text,encoding='utf-8')
print(text)
