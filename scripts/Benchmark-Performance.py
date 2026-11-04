#!/usr/bin/env python3
"""Run comparable logic microbenchmarks. Native UI remains a separate Windows test."""
from pathlib import Path
import os,sys,tempfile,subprocess,json,xml.etree.ElementTree as ET
r=Path(__file__).resolve().parent.parent
with tempfile.TemporaryDirectory(prefix='captovix-benchmark-') as folder:
 p=Path(folder);(p/'Program.cs').write_text((r/'benchmarks/Performance/Program.cs.txt').read_text());(p/'Before.cs').write_text((r/'benchmarks/Performance/BeforeEditorDocument.cs.txt').read_text())
 for variant in ['before','after']:
  tree=ET.parse(r/'tests/EditorCoreChecks/EditorCoreChecks.csproj');root=tree.getroot();g=root.find('ItemGroup')
  for node in root.findall('.//Compile'):
   name=Path(node.get('Include')).name;node.attrib={'Include':str(p/'Before.cs') if variant=='before' and name=='EditorDocument.cs' else str(r/'src/Captovix'/name)}
  E=ET.SubElement(root.find('PropertyGroup'),'EnableDefaultCompileItems');E.text='false';ET.SubElement(g,'Compile',Include=str(p/'Program.cs'));project=p/(variant+'.csproj');tree.write(project)
  result=subprocess.run([os.environ.get('DOTNET_EXE','dotnet'),'run','--project',str(project),'-c','Release'],check=True,capture_output=True,text=True);data=json.loads(result.stdout[result.stdout.index('{'):]);print(variant+': '+json.dumps(data,ensure_ascii=False))
