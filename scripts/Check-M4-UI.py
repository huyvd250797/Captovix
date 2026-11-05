from pathlib import Path
import xml.etree.ElementTree as E
r=Path(__file__).resolve().parent.parent;d=r/'src/Captovix';n=0
def check(ok,msg):
 global n
 assert ok,msg
 n+=1;print('PASS:',msg)
s=(d/'MainWindow.StudioAccess.cs').read_text();check('UniformGrid{Columns=2}' in s and 'Width=174' not in s,'More grid uses two real columns rather than overflowing wrap tiles')
check('Studio:Groups' not in s and 'command=="Groups"' in s,'Groups runs through the normal dynamic Studio command path')
x=E.parse(d/'MainWindow.xaml');w='{http://schemas.microsoft.com/winfx/2006/xaml/presentation}';ns='{http://schemas.microsoft.com/winfx/2006/xaml}'
check(any(a.get(ns+'Name')=='ObjectTree' for a in x.iter(w+'TreeView')),'Inspector exposes hierarchy tree')
check(any(a.get('PreviewMouseRightButtonDown')=='MediaRight_Click' for a in x.iter(w+'ListBox')),'Library tile right click routes to its actual source item')
cs=(d/'MainWindow.GroupLayout.cs').read_text();check('ArgumentList.Add' in cs and 'File.Exists(item.FilePath)' in cs,'Open location validates file and passes Explorer arguments separately')
p=(d/'PopupPresentation.cs').read_text();check('DoubleAnimationUsingKeyFrames' in p and 'Visibility.Collapsed' not in p and 'Hide()' not in p,'Outside click feedback animates outline only')
check('RemoveHook' in p and 'w.Closed' in p,'Modal hooks are detached on close')
check('SetResourceReference(Border.BorderBrushProperty,"PopupLine")' in p,'Dialog and popup outline follow current theme')
check('PopupPresentation.DecorateWindow(w,root)' in (d/'WindowChromeStudio.cs').read_text(),'Normal Captovix dialogs share common chrome')
check(all('MessageBox.Show(' not in f.read_text() for f in d.glob('*.cs') if f.name not in {'AppDialog.cs','Program.cs','RecoveryWorker.cs'}),'Normal app dialogs share common form; fatal startup/isolated restore worker retain native fallback')
a=(d/'AppDialog.cs').read_text();check(a.index('Button("Hủy"')<a.index('Button(buttons') and 'Dock.Right' in a,'Cancel remains left and confirmation right')
print(n,'M4 UI contract checks passed; visual/native acceptance pending.')
