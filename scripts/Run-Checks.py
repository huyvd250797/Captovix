#!/usr/bin/env python3
"""Run pure checks; cross-build WPF on Linux, execute WPF checks on Windows."""
import os, subprocess, sys
from pathlib import Path
root=Path(__file__).resolve().parent.parent
dotnet=os.environ.get('DOTNET_EXE','dotnet')
for project in sorted((root/'tests').glob('*/*.csproj')):
    windows='UseWPF>true' in project.read_text()
    mode='build' if windows and sys.platform!='win32' else 'run'
    args=[dotnet,mode,'--project',str(project)] if mode=='run' else [dotnet,mode,str(project)]
    args+=['-c','Release','-p:Platform=x64']
    if mode=='build':args+=['--nologo']
    if project.parent.name=='InstallerContractChecks':args+=['--',str(root)]
    print('\nSUITE '+project.parent.name+' · '+mode,flush=True)
    subprocess.run(args,cwd=root,check=True)
print('ALL_CHECKS_OK · Windows-only suites are compile-only on non-Windows hosts.',flush=True)
