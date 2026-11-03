using Captovix;
int count=0;void Check(bool x,string label){if(!x)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
foreach(var screen in new[]{new PixelRect(0,0,1920,1080),new PixelRect(-1920,-300,1920,1080),new PixelRect(0,0,300,180)}){
    var selected=new PixelRect(screen.X+30,screen.Y+30,screen.Width/3,screen.Height/3);
    var toolbar=QuickCaptureLayout.Toolbar(screen,selected,270,32,selected.Right,selected.Bottom);
    Check(toolbar.X>=screen.X&&toolbar.Y>=screen.Y&&toolbar.Right<=screen.Right&&toolbar.Bottom<=screen.Bottom,"Toolbar stays on monitor, including negative origin");
    Check(QuickCaptureLayout.Overlap(toolbar,selected)==0,"Toolbar uses outside when space exists");
    var corner=QuickCaptureLayout.Toolbar(screen,screen,140,28,screen.Right,screen.Bottom);
    Check(corner.X==screen.X&&corner.Y==screen.Y,"Full-screen fallback uses corner away from release point");
    var keyboard=QuickCaptureLayout.KeyboardRegion(screen,screen.Right,screen.Bottom);Check(CaptureSelection.Valid(keyboard)&&keyboard.Right<=screen.Right&&keyboard.Bottom<=screen.Bottom,"Keyboard seed has valid pixel size and stays on desktop");
}
Check(ReleaseNotesPresentation.Text("# V1.31\n\n- **Chụp nhanh**\n- `Enter` một lần\n[Tài liệu](https://example.com)")=="V1.31\n\n• Chụp nhanh\n• Enter một lần\nTài liệu","Both update surfaces receive readable identical notes");
Check(ReleaseNotesPresentation.Text("").Length>0,"Empty release notes have helpful fallback");
Check(ReleaseNotesPresentation.Text(new string('x',6000)).Length==5001,"Notes are bounded for both UI surfaces");
Check(StudioToolCatalog.All.Select(t=>t.Command).Distinct().Count()==StudioToolCatalog.All.Length,"All recent tool commands are unambiguous");
Check(StudioToolCatalog.All.Select(t=>t.Icon).Distinct().Count()==StudioToolCatalog.All.Length,"Each Studio tool has its own icon");
Check(StudioToolCatalog.Find("invalid")==null&&StudioToolCatalog.Find("Stamp")?.Icon=="Stamp","Persisted recent tool validates command and supports Stamp");
Check(StampCatalog.All.Length>=10&&StampCatalog.All.Any(s=>s.Id=="check"&&s.Color=="#159568")&&StampCatalog.All.Any(s=>s.Id=="cross"&&s.Color=="#E4495A"),"Stamp gallery includes green check, red cross and new vector artwork");
var gesture=new CaptureGesture(default,CaptureHandle.New,10,10);var region=gesture.Apply(110,70,false);region=CaptureSelection.Drag(region,CaptureHandle.Right,110,40,135,40,false);Check(region==new PixelRect(10,10,125,60),"Mouse drag then boundary adjustment shares one final capture rectangle");
Console.WriteLine($"{count} quick capture logic checks passed. Native overlay/input acceptance requires Windows.");
