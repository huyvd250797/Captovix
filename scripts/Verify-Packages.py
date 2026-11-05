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
p.add_argument('--migration-txt',type=Path)
p.add_argument('--release-dir',type=Path)
p.add_argument('--payload-dir',type=Path)
p.add_argument('--setup-wrapper',type=Path)
a=p.parse_args();r=a.root.resolve();rel=a.release_dir.resolve() if a.release_dir else r/'artifacts/releases';payload=a.payload_dir.resolve() if a.payload_dir else r/'artifacts/win-x64'
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
 for name in ['Assets/Captovix-brand-v117.ico','Captovix.dll','Captovix.exe','ScreenRecorderLib.dll','Velopack.dll','Captovix.runtimeconfig.json','update-source.json','README.md','CHANGELOG.md','ROADMAP.md','docs/WINDOWS-ACCEPTANCE-V145.md','release-quality.json','Recovery.cmd','Recovery.ps1','docs/WINDOWS-ACCEPTANCE-V1441.md','docs/WINDOWS-ACCEPTANCE-V144.md','docs/MODEL-EVALUATION-V144.md','docs/PERFORMANCE-M6-V144.json','docs/WINDOWS-ACCEPTANCE-V143.md','docs/WINDOWS-ACCEPTANCE-V142.md','docs/PERFORMANCE-M5-V142.json','docs/PAYLOAD-CLEANUP-V142.json','docs/STORAGE-AUDIT-V142.json','docs/STARTUP-POPUP-RECOVERY-V1411.md','docs/GROUPS-LAYOUT-EDITING-V141.md','docs/MOVE-BACKGROUND-RECOVERY-V140.md','docs/ELEMENT-SCAN-REVIEW-V139.md','docs/ACTIVATION-METADATA-V139.md','docs/MOCKUP-M1-V138.md','docs/IMAGE-COMPOSITION-TEMPLATES.md','docs/SCROLLING-CAPTURE-APPEARANCE.md','docs/WINDOWS-TEST-V1.14.md','docs/VALIDATION.md','docs/CAPTURE-RELIABILITY.md','docs/OCR-TEXT-EXTRACTION.md','docs/ASSISTED-IMAGE-EDITING.md','docs/IMAGE-LIBRARY-PROJECTS.md','docs/BATCH-EXPORT-DOCUMENTATION.md','docs/IMAGE-RELEASE-PERFORMANCE.md','docs/LAYOUT-PRECISION.md','docs/OBJECT-EDITING-PRECISION.md','docs/TEXT-CALLOUT-REFINEMENT.md','docs/CAPTURE-PRECISION.md','docs/SCROLLING-CAPTURE-QUALITY.md','docs/SMART-ELEMENT-CAPTURE.md','docs/CAPTURE-IMAGE-STUDIO.md','docs/COLOR-PICKER-STUDIO-ACCESS.md','docs/LICENSING-GUIDE.md','docs/SERVERLESS-LICENSING.md','docs/CAPTURE-FIDELITY-RELIABILITY.md','docs/QUICK-CAPTURE-EXPERIENCE.md','docs/capture-pixel-test.html','Licensing.Shared.dll','docs/PERFORMANCE-BENCHMARK.json','Tesseract.dll','x64/tesseract50.dll','x64/leptonica-1.82.0.dll','tessdata/eng.traineddata','tessdata/vie.traineddata','tessdata/models.json','docs/RELEASE-NOTES.md','docs/PERFORMANCE-RESPONSIVENESS-V137.md','docs/PERFORMANCE-V137-before.json','docs/PERFORMANCE-V137-after.json']:
  check(z.read(entry(name))==(payload/name).read_bytes(),'Packaged payload matches published '+name)
 check(z.read(entry('Licensing.Shared.dll'))==(r/'src/Licensing.Shared/bin/x64/Release/net10.0/Licensing.Shared.dll').read_bytes(),'Packaged licensing protocol is the current build with activatedAt support')
 check(version.encode('utf-16le') in z.read(entry('Captovix.exe')),'Native application host version matches release')
 deps=json.loads(z.read(entry('Captovix.deps.json')));check('Captovix/'+version in deps['libraries'],'Application dependency version matches release')
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
setup=rel/'HuyVo.Captovix-win-Setup.exe';check(setup.read_bytes()[:2]==b'MZ' and setup.read_bytes()==(a.setup_wrapper if a.setup_wrapper else r/f'artifacts/Captovix-v{version}-Setup.exe').read_bytes(),'Canonical release setup is the migration/directory wrapper')
check(set(pe_icons(setup.read_bytes()))==set(ico_images((r/'src/Captovix/Assets/Captovix.ico').read_bytes())),'Setup wrapper icon is approved artwork; MUI2 cannot override it')
expected={pkg.name,'HuyVo.Captovix-win-Setup.exe','releases.win.json','assets.win.json','RELEASES'}
check({x.name for x in rel.iterdir() if x.is_file() and not x.name.startswith(".")}==expected,'Exactly five release assets')
manifest=json.loads((rel/'assets.win.json').read_text());check({x['RelativeFileName'] for x in manifest}=={pkg.name,setup.name},'Assets manifest points to canonical package and installer')
if a.baseline_source:
 with zipfile.ZipFile(a.baseline_source) as old:
  for path in ['src/Captovix/CaptureEngine.cs','installer/InstallPath.nsh']:
   prior=old.read(next(n for n in old.namelist() if n.endswith('/'+path)));check(prior==(r/path).read_bytes(),'Protected foundation unchanged: '+path)
  prior=old.read(next(n for n in old.namelist() if n.endswith('/src/Captovix/Program.cs'))).decode();current=(r/'src/Captovix/Program.cs').read_text()
  normalized=re.sub(r'        if\(args.Length>0&&args\[0\]=="--restore-backup"[^\n]+\n','',current).replace('+"\\nChạy Recovery.cmd trong thư mục cài để mở công cụ cứu hộ độc lập."','');check(prior==normalized,'Startup foundation retained; isolated restore worker before WPF and rescue guidance added')
  check('catch(Exception error)' in current and 'App.Log(new InvalidOperationException' in current and 'System.Windows.MessageBox.Show' in current and 'Environment.ExitCode=1' in current,'Startup failure has independent native fallback and diagnostic log')
  prior=old.read(next(n for n in old.namelist() if n.endswith('/installer/Captovix.nsi'))).decode();prior=re.sub(r'(!define APPVERSION ")[0-9.]+(")',lambda m:m[1]+version+m[2],prior);check(prior==(r/'installer/Captovix.nsi').read_text(),'Installer changes only version; core arguments and approved MUI icon retained')

license_source=(r/'src/Captovix/LicenseRuntime.cs').read_text()
check('GetManifestResourceStream("Captovix.license-config.json")' in license_source and 'Verify(reply.Certificate)' in license_source,'License trust configuration is embedded and certificates require signature verification')
check(not any(name in p.name for p in payload.rglob('*') if p.is_file() for name in ['captovix-signing.pem','server.env','edge-secrets.env']),'No server signing key or admin token environment in desktop payload')
if a.baseline_source:
 with zipfile.ZipFile(a.baseline_source) as basezip:
  for name in ['src/Captovix/license-config.json','supabase/functions/licensing/index.ts','admin/config.json']:
   check(basezip.read(next(n for n in basezip.namelist() if n.endswith('/'+name)))==(r/name).read_bytes(),'License/backend unchanged from supplied source: '+name)
main=(r/'src/Captovix/MainWindow.xaml.cs').read_text()
check('Task.Run(()=>frozen.Save(region,working)' in main and 'frozen?.Dispose()' in main,'Frozen capture saves retained physical pixels and releases snapshot')
capture=(r/'src/Captovix/DesktopPhotoCapture.cs').read_text()
check('SetThreadDpiAwarenessContext(previousDpi)' in capture and 'pixels.AsSpan().SequenceEqual(ImageTransfer.Bgra(decoded))' in capture,'GDI worker restores DPI context and validates PNG pixel roundtrip')
check('StretchBlt(' not in capture and 'BitBlt(dc,part.X,part.Y,part.Crop.Width,part.Crop.Height' in capture,'Desktop photos retain unscaled physical pixel blits')

old_sql=(r/'supabase/migrations/202610280003_serverless.sql').read_text();start=old_sql.index('create or replace function public.cvx_license_operation(');end=old_sql.index('create or replace function public.cvx_license_admin(',start);old_function=old_sql[start:end].strip()
new_sql=(r/'supabase/migrations/202611040001_activation_timestamp.sql').read_text();new_function=new_sql[new_sql.index('create or replace function'):new_sql.rindex('commit;')].strip()
check(new_function.replace("'activatedAt',d.created_at,",'')==old_function,'Migration changes only signed activation metadata, preserving policy and protections')
if a.migration_txt:check(a.migration_txt.read_bytes()==(r/'supabase/migrations/202611040001_activation_timestamp.sql').read_bytes(),'Migration TXT exactly matches complete source SQL')

for path in [a.source_zip,a.update_zip]:
 if path:
  with zipfile.ZipFile(path) as z:
   check(z.testzip() is None,'ZIP CRC: '+path.name)
   if path==a.source_zip:
    files=[x for x in r.rglob('*') if x.is_file() and not any(part in {'bin','obj','artifacts','.git','.vs','__pycache__','node_modules'} for part in x.relative_to(r).parts) and '_wpftmp' not in x.name and x.suffix not in {'.pdb','.tmp','.log','.exe','.deb','.pyc'}]
    check({x.relative_to(r).as_posix() for x in files}=={n.split('/',1)[1] for n in z.namelist() if not n.endswith('/')},'Source ZIP contains complete clean tree')
    check(all(z.read('Captovix-v'+version+'/'+x.relative_to(r).as_posix())==x.read_bytes() for x in files),'Source ZIP byte comparison')
   else:
    check(set(z.namelist())==expected|{'README-PUBLISH.txt'},'Update ZIP includes five assets and publishing instructions')
    check(all(z.read(name)==(rel/name).read_bytes() for name in expected),'Update ZIP assets match verified release bytes')
print(f'{checks} artifact integrity checks passed. Windows install/render/input acceptance is pending.')
