#!/usr/bin/env python3
"""Re-run the checked-in synthetic OCR fixtures with an installed Tesseract CLI.
Print a JSON report. Linux CLI results are NOT Windows native runtime acceptance.
Python standard library only. Usage: python scripts/Benchmark-Ocr.py
"""
from pathlib import Path
import argparse,json,subprocess,unicodedata
p=argparse.ArgumentParser(description=__doc__);p.add_argument('--tesseract',default='tesseract');a=p.parse_args()
r=Path(__file__).resolve().parent.parent
def normalize(s):return ' '.join(unicodedata.normalize('NFC',s).split())
def distance(x,y):
 row=list(range(len(y)+1))
 for i,c in enumerate(x,1):
  nxt=[i]
  for j,d in enumerate(y,1):nxt.append(min(nxt[-1]+1,row[j]+1,row[j-1]+(c!=d)))
  row=nxt
 return row[-1]
rows=[]
for name in ['english','vietnamese','small-text','dark-ui']:
 expected=(r/'tests/OcrFixtures'/f'{name}.txt').read_text(encoding='utf-8');language='eng' if name=='english' else 'vie+eng'
 result=subprocess.run([a.tesseract,str(r/'tests/OcrFixtures'/f'{name}.png'),'stdout','--tessdata-dir',str(r/'src/Captovix/tessdata'),'-l',language,'--psm','6'],check=True,capture_output=True,text=True,encoding='utf-8')
 actual=result.stdout.strip();length=len(normalize(expected));errors=distance(normalize(expected),normalize(actual));rows.append(dict(fixture=name,language=language,characters=length,errors=errors,CER=round(errors/max(1,length),4),recognized=actual))
version=subprocess.run([a.tesseract,'--version'],capture_output=True,text=True,check=True).stdout.splitlines()[0]
print(json.dumps(dict(engine=version,cases=rows,note='Synthetic fixtures only; platform/runtime must be recorded separately.'),ensure_ascii=False,indent=2))
