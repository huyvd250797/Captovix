#!/usr/bin/env python3
from pathlib import Path
import argparse,json,hashlib,xml.etree.ElementTree as E
p=argparse.ArgumentParser();p.add_argument('payload',type=Path);a=p.parse_args();root=Path(__file__).resolve().parent.parent;version=E.parse(root/'src/Captovix/Captovix.csproj').findtext('.//Version')
files=['Captovix.exe','Captovix.dll','Licensing.Shared.dll','Captovix.runtimeconfig.json','Velopack.dll','ScreenRecorderLib.dll','x64/tesseract50.dll','x64/leptonica-1.82.0.dll','tessdata/eng.traineddata','tessdata/vie.traineddata']
for name in files:
 if not (a.payload/name).is_file():raise SystemExit('Missing runtime file: '+name)
data={'version':version,'files':{name:hashlib.sha256((a.payload/name).read_bytes()).hexdigest() for name in files},'purpose':'Consistency stamp; not a publisher signature or Windows runtime QA approval'}
(a.payload/'release-quality.json').write_text(json.dumps(data,indent=2));print('Stamped',version,len(files),'files')
