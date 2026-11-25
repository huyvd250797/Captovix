#!/usr/bin/env python3
"""Build checks sequentially; execute WPF only on Windows. Never pass MSBuild switches as test arguments."""
import os, subprocess, sys, argparse
from pathlib import Path
root=Path(__file__).resolve().parent.parent
dotnet=os.environ.get('DOTNET_EXE','dotnet')
parser=argparse.ArgumentParser();parser.add_argument('--from-suite',default='');options=parser.parse_args()
for project in sorted((root/'tests').glob('*/*.csproj')):
    if options.from_suite and project.parent.name<options.from_suite:continue
    windows='UseWPF>true' in project.read_text()
    print('SUITE '+project.parent.name+' · build',flush=True)
    subprocess.run([dotnet,'build',str(project),'-c','Release','-p:Platform=x64','-m:1','-p:BuildInParallel=false','--nologo'],cwd=root,check=True)
    if windows and sys.platform!='win32':
        print('WINDOWS_RUNTIME_PENDING '+project.parent.name,flush=True)
        continue
    args=[dotnet,'run','--no-build','--project',str(project),'-c','Release','-p:Platform=x64']
    if project.parent.name=='InstallerContractChecks':args+=['--',str(root)]
    subprocess.run(args,cwd=root,check=True)
print('ALL_CHECKS_OK · Windows-only suites are compile-only on non-Windows hosts.',flush=True)
