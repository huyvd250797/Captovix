#!/usr/bin/env python3
"""Verify local build artifacts; no Windows executable is run. Python 3 stdlib only."""
from pathlib import Path
import argparse, hashlib, json, zipfile, re, xml.etree.ElementTree as ET
from brand_resources import ico_images,pe_icons
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--root',type=Path,default=Path(__file__).resolve().parent.parent)
p.add_argument('--baseline-source',type=Path)
p.add_argument('--source-zip',type=Path)
p.add_argument('--update-zip',type=Path)
a=p.parse_args();r=a.root.resolve();rel=r/'artifacts/releases';payload=r/'artifacts/win-x64'
version=ET.parse(r/'src/Captovix/Captovix.csproj').findtext('.//Version');app='HuyVo.Captovix';checks=0

def check(ok,label):
 global checks
 if not ok:raise SystemExit('FAIL: '+label)
 checks+=1;print('PASS: '+label)
def digest(data,kind='sha256'):return hashlib.new(kind,data).hexdigest().upper()
feed=json.loads((rel/'releases.win.json').read_text());assets=feed['Assets'];full=next(x for x in assets if x['Type']=='Full' and x['PackageId']==app and x['Version']==version)
pkg=rel/full['FileName'];raw=pkg.read_bytes()
check(len(raw)==full['Size'] and digest(raw)==full['SHA256'] and digest(raw,'sha1')==full['SHA1'],'Feed full package size/SHA256/SHA1 match bytes')
releases=(rel/'RELEASES').read_text(encoding='utf-8-sig').split();check(releases[0].upper()==digest(raw,'sha1') and releases[1]==pkg.name and int(releases[2])==len(raw),'Legacy RELEASES matches same full package')
with zipfile.ZipFile(pkg) as z:
 check(z.testzip() is None,'Nupkg CRC verified')
 nuspec=ET.fromstring(z.read(next(n for n in z.namelist() if n.endswith('.nuspec'))));fields={e.tag.rsplit('}',1)[-1]:e.text for e in nuspec.iter()}
 check(fields['id']==app and fields['version']==version,'Nuspec AppId/version verified')
 def entry(name):return next(n for n in z.namelist() if n.startswith('lib/') and n.endswith('/'+name))
 for name in ['Assets/Captovix-brand-v117.ico','Captovix.dll','Captovix.exe','ScreenRecorderLib.dll','Velopack.dll','Captovix.runtimeconfig.json','update-source.json','README.md','CHANGELOG.md','ROADMAP.md','docs/IMAGE-COMPOSITION-TEMPLATES.md','docs/SCROLLING-CAPTURE-APPEARANCE.md','docs/WINDOWS-TEST-V1.14.md','docs/VALIDATION.md','docs/CAPTURE-RELIABILITY.md','docs/OCR-TEXT-EXTRACTION.md','docs/ASSISTED-IMAGE-EDITING.md','docs/IMAGE-LIBRARY-PROJECTS.md','docs/BATCH-EXPORT-DOCUMENTATION.md','docs/IMAGE-RELEASE-PERFORMANCE.md','docs/LAYOUT-PRECISION.md','docs/OBJECT-EDITING-PRECISION.md','docs/TEXT-CALLOUT-REFINEMENT.md','docs/CAPTURE-PRECISION.md','docs/SCROLLING-CAPTURE-QUALITY.md','docs/SMART-ELEMENT-CAPTURE.md','docs/PERFORMANCE-BENCHMARK.json','Tesseract.dll','x64/tesseract50.dll','x64/leptonica-1.82.0.dll','tessdata/eng.traineddata','tessdata/vie.traineddata','tessdata/models.json','docs/RELEASE-NOTES.md']:
  check(z.read(entry(name))==(payload/name).read_bytes(),'Packaged payload matches published '+name)
 runtime=json.loads(z.read(entry('Captovix.runtimeconfig.json')));check(all(f['version']=='10.0.13' for f in runtime['runtimeOptions']['includedFrameworks']),'.NET10.0.13 self-contained runtime declared')
 source=json.loads(z.read(entry('update-source.json')));check(source['SourceKind']=='Https' and source['SourceUrl']=='https://github.com/huyvd250797/captovix-releases/releases/latest/download','Stable source baked into package')
 check(z.read(entry('Assets/Captovix-brand-v117.ico'))==(r/'src/Captovix/Assets/Captovix.ico').read_bytes(),'Runtime icon equals approved source artwork')
 models=json.loads(z.read(entry('tessdata/models.json')))
 for lang,meta in models['models'].items():
  data=z.read(entry('tessdata/'+meta['file']));check(len(data)==meta['bytes'] and digest(data)==meta['sha256'].upper(),'Bundled OCR model hash and size: '+lang)
 check(not any('/x86/' in n for n in z.namelist()),'Windows x64 OCR package has no x86 native payload')
 for file in ['Captovix.exe','Squirrel.exe']:
  check(set(pe_icons(z.read(entry(file))))==set(ico_images((r/'src/Captovix/Assets/Captovix.ico').read_bytes())),'Approved icon in all PE sizes: '+file)
 check(not any(n.endswith('.pdb') for n in z.namelist()),'No debug symbols in release payload')
setup=rel/'HuyVo.Captovix-win-Setup.exe';check(setup.read_bytes()[:2]==b'MZ' and setup.read_bytes()==(r/f'artifacts/Captovix-v{version}-Setup.exe').read_bytes(),'Canonical release setup is the migration/directory wrapper')
check(set(pe_icons(setup.read_bytes()))==set(ico_images((r/'src/Captovix/Assets/Captovix.ico').read_bytes())),'Setup wrapper icon is approved artwork; MUI2 cannot override it')
expected={pkg.name,'HuyVo.Captovix-win-Setup.exe','releases.win.json','assets.win.json','RELEASES'}
check({x.name for x in rel.iterdir() if x.is_file() and not x.name.startswith(".")}==expected,'Exactly five release assets')
manifest=json.loads((rel/'assets.win.json').read_text());check({x['RelativeFileName'] for x in manifest}=={pkg.name,setup.name},'Assets manifest points to canonical package and installer')
if a.baseline_source:
 with zipfile.ZipFile(a.baseline_source) as old:
  for path in ['src/Captovix/Native.cs','src/Captovix/CaptureEngine.cs','src/Captovix/ImageTransfer.cs','src/Captovix/CaptureKeys.cs','installer/InstallPath.nsh']:
   prior=old.read(next(n for n in old.namelist() if n.endswith('/'+path)));check(prior==(r/path).read_bytes(),'Protected foundation unchanged: '+path)
  prior=old.read(next(n for n in old.namelist() if n.endswith('/src/Captovix/Program.cs'))).decode();check(prior.replace('VelopackApp.Build().SetAutoApplyOnStartup','VelopackApp.Build().SetAppUserModelId("velopack."+VersionInfo.AppId).SetAutoApplyOnStartup')==(r/'src/Captovix/Program.cs').read_text(),'Program changes only stable AppID; updater and worker ordering preserved')
  prior=old.read(next(n for n in old.namelist() if n.endswith('/installer/Captovix.nsi'))).decode();prior=re.sub(r'(!define APPVERSION ")[0-9.]+(")',lambda m:m[1]+version+m[2],prior);check(prior==(r/'installer/Captovix.nsi').read_text(),'Installer changes only version; core arguments and approved MUI icon retained')

capture=(r/'src/Captovix/DesktopPhotoCapture.cs').read_text()
check('SetThreadDpiAwarenessContext(previousDpi)' in capture and 'pixels.AsSpan().SequenceEqual(ImageTransfer.Bgra(decoded))' in capture,'GDI worker restores DPI context and validates PNG pixel roundtrip')
check('StretchBlt(' not in capture and 'BitBlt(dc,part.X,part.Y,part.Crop.Width,part.Crop.Height' in capture,'Desktop photos retain unscaled physical pixel blits')

for path in [a.source_zip,a.update_zip]:
 if path:
  with zipfile.ZipFile(path) as z:
   check(z.testzip() is None,'ZIP CRC: '+path.name)
   if path==a.source_zip:
    files=[x for x in r.rglob('*') if x.is_file() and not any(part in {'bin','obj','artifacts','.git','.vs','__pycache__'} for part in x.relative_to(r).parts) and '_wpftmp' not in x.name and x.suffix not in {'.pdb','.tmp','.log','.exe','.deb','.pyc'}]
    check({x.relative_to(r).as_posix() for x in files}=={n.split('/',1)[1] for n in z.namelist() if not n.endswith('/')},'Source ZIP contains complete clean tree')
    check(all(z.read(r.name+'/'+x.relative_to(r).as_posix())==x.read_bytes() for x in files),'Source ZIP byte comparison')
   else:
    check(set(z.namelist())==expected|{'README-PUBLISH.txt'},'Update ZIP includes five assets and publishing instructions')
    check(all(z.read(name)==(rel/name).read_bytes() for name in expected),'Update ZIP assets match verified release bytes')
print(f'{checks} artifact integrity checks passed. Windows install/render/input acceptance is pending.')
