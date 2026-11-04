using System.Reflection;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captovix;
internal static class Program
{
    private static int count;
    private static void Check(bool ok,string label){if(!ok)throw new Exception(label);count++;Console.WriteLine("PASS: "+label);}
    private static byte[] Pixels(BitmapSource bitmap){var p=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];bitmap.CopyPixels(p,bitmap.PixelWidth*4,0);return p;}
    [STAThread]private static int Main()
    {
        try{
            var raw=Enumerable.Repeat((byte)255,800*600*4).ToArray();var source=BitmapSource.Create(800,600,96,96,PixelFormats.Bgra32,null,raw,800*4);source.Freeze();
            var doc=new EditorDocument{OriginalWidth=800,OriginalHeight=600};var session=new EditorSession(doc);var surface=new EditorSurface();surface.Load(source,session);surface.Tool=EditorTool.Rectangle;
            surface.PointerDown(new(40,50));surface.PointerMove(new(240,180));surface.PointerUp(new(240,180));var rect=surface.Selected!;var rectId=rect.Id;Check(doc.Annotations.Count==1&&rect.Kind==AnnotationKind.Rectangle&&surface.Tool==EditorTool.Rectangle,"Drawing a rectangle retains the current drawing tool");
            surface.SelectIds([]);surface.Tool=EditorTool.Step;surface.PointerDown(new(340,250));surface.PointerDown(new(420,250));Check(doc.Annotations.Count==3&&doc.Annotations.Where(a=>a.Kind==AnnotationKind.Step).Select(a=>a.Number).SequenceEqual(new[]{1,2}),"Repeated empty clicks create sequential steps without selecting tool again");
            surface.PointerDown(new(40,100));surface.PointerMove(new(80,125));surface.PointerUp(new(80,125));Check(surface.SelectedId==rectId&&rect.X1==80&&rect.Y1==75&&doc.Annotations.Count==3&&surface.Tool==EditorTool.Step,"While drawing steps, clicking rectangle border selects and drags it without creating a step");
            session.Undo();surface.Refresh();Check(session.Document.Annotations.First(a=>a.Id==rectId).X1==40,"Smart-target drag is one undo transaction");session.Redo();surface.Refresh();rect=session.Document.Annotations.First(a=>a.Id==rectId);rect.FillEnabled=true;
            surface.SelectIds([]);surface.Tool=EditorTool.Arrow;surface.PointerDown(new(100,100));surface.PointerMove(new(120,120));surface.PointerUp(new(120,120));Check(surface.SelectedId==rectId&&session.Document.Annotations.Count==3,"Filled object is directly editable from a different drawing tool");
            surface.Tool=EditorTool.Rectangle;surface.SelectIds([]);surface.PointerDown(new(140,130),ModifierKeys.Alt);surface.PointerMove(new(200,170),ModifierKeys.Alt);surface.PointerUp(new(200,170),ModifierKeys.Alt);Check(session.Document.Annotations.Count==4,"Alt-drag can draw a new overlapping rectangle");
            var text=new Annotation{Kind=AnnotationKind.Text,X1=40,Y1=280,Text="Tiếng Việt: Ắ ằ ữ ệ\nDòng chữ dài để kiểm tra",FontSize=28,FontFamily="Arial",Bold=true,Italic=true,Underline=true,TextWidth=260,TextPadding=8,TextAlignment=TextAlign.Center,FillEnabled=true,FillColor="#FFE28A"};var textDoc=new EditorDocument{OriginalWidth=800,OriginalHeight=600,Annotations=[text]};
            var layout=EditorRenderer.TextLayout(text);Check(layout.TextAlignment==TextAlignment.Center&&layout.Height>text.FontSize,"Wrapped Vietnamese formatted text uses centered multiline layout");var glyphs=layout.BuildGeometry(EditorRenderer.TextOrigin(text,layout)).Bounds;Check(EditorRenderer.LocalBounds(text).Contains(glyphs),"Text selection bounds contain italic glyph overhang and underline");
            var exported=EditorRenderer.Render(source,textDoc);var reopened=EditorDocument.Deserialize(textDoc.Serialize());reopened.Validate(800,600,"");Check(Pixels(exported).SequenceEqual(Pixels(EditorRenderer.Render(source,reopened))),"Styled text output is identical after save/reopen");var textSurface=new EditorSurface();textSurface.Load(source,new EditorSession(textDoc));var original=Pixels(textSurface.Flatten());textSurface.SetZoom(.25);Check(Pixels(textSurface.Flatten()).SequenceEqual(original),"Text export does not change with display zoom");
            var polished=new Annotation{Kind=AnnotationKind.Callout,CalloutBodyPivot=true,AutoFitText=true,TextPadding=30,VerticalAlignment=TextVertical.Center,Text="Tiếng Việt",FontSize=28};
            foreach(var delta in new[]{new Point(260,100),new Point(-260,100),new Point(260,-100),new Point(-260,-100)}){CalloutGesture.Place(polished,400,300,400+delta.X,300+delta.Y);Check(polished.TailX==400&&polished.TailY==300&&polished.Width==242&&polished.Height==82,"Initial pointer remains outside body as fixed tail in quadrant"+delta);}
            polished.X1=100;polished.Y1=80;polished.X2=480;polished.Y2=300;polished.TailX=60;polished.TailY=160;polished.Rotation=25;
            var tail=EditorRenderer.WorldPoint(polished,new(polished.TailX,polished.TailY));var width=polished.Width;var height=polished.Height;
            EditorRenderer.FitText(polished,new(0,0,800,600),true);Check(polished.Width==width&&polished.Height==height,"Short text keeps drawn callout dimensions");
            polished.Text=string.Join(" ",Enumerable.Repeat("Tiếng Việt Ắ ằ ữ ệ",300));EditorRenderer.FitText(polished,new(0,0,800,600),true);
            Check(polished.Height>height&&polished.Width==width&&EditorRenderer.TextContent(polished).Height+1>=EditorRenderer.TextLayout(polished).Height,"Long text grows live vertically and retains drawn width");Check((EditorRenderer.WorldPoint(polished,new(polished.TailX,polished.TailY))-tail).Length<.001,"Auto-grow pins rotated world-space tail");
            var cached=EditorRenderer.TextLayout(polished);polished.Translate(35.12345,42.67891);Check(ReferenceEquals(cached,EditorRenderer.TextLayout(polished)),"Moving long text reuses layout");polished.Text+=" mới";Check(!ReferenceEquals(cached,EditorRenderer.TextLayout(polished)),"Text changes invalidate layout cache");
            var sized=polished.Copy();sized.Text="Ngắn";var moved=new AnnotationBox(sized.Left,sized.Top,sized.Width+80,sized.Height);AnnotationEditing.Transform(sized,polished,new(polished.Left,polished.Top,polished.Width,polished.Height),moved);Check(sized.TextPadding==30&&sized.FontSize==polished.FontSize,"Width resize preserves callout font and padding");
            var replay=polished.Copy();replay.Text="Nội dung kiểm tra cache";replay.Rotation=0;replay.X1=100;replay.Y1=100;replay.X2=480;replay.Y2=300;replay.TailX=60;replay.TailY=160;replay.FillEnabled=true;replay.FillColor="#FFFFFF";
            var replayDoc=new EditorDocument{OriginalWidth=800,OriginalHeight=600,Annotations=[replay]};var replayPixels=Pixels(EditorRenderer.Render(source,replayDoc));replay.Translate(20,30);EditorRenderer.Render(source,replayDoc);replay.Translate(-20,-30);Check(replayPixels.SequenceEqual(Pixels(EditorRenderer.Render(source,replayDoc))),"Cached drawing returns exact pixels after move back");replay.FillColor="#FFCC33";Check(!replayPixels.SequenceEqual(Pixels(EditorRenderer.Render(source,replayDoc))),"Changing fill invalidates drawing without stale pixels");
            replay.Text=new string('a',20000);EditorRenderer.FitText(replay,new(0,0,800,600),true);var watch=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<120;i++){replay.Translate(1,0);var drawing=new DrawingGroup();using(var dc=drawing.Open())EditorRenderer.DrawAnnotation(dc,replay);}watch.Stop();Console.WriteLine($"BENCHMARK 20k-char callout,120 moves: {watch.Elapsed.TotalMilliseconds:0.0}ms; native machine measurement, no universal zero-lag claim.");
            foreach(var style in Enum.GetValues<CalloutStyle>()){
                var callout=text.Copy();callout.Kind=AnnotationKind.Callout;callout.X1=50;callout.Y1=30;callout.X2=340;callout.Y2=90;callout.TailX=400;callout.TailY=180;callout.CalloutStyle=style;callout.FillEnabled=true;callout.TextPadding=12;EditorRenderer.FitText(callout,new(0,0,800,600));
                Check(EditorRenderer.TextContent(callout).Height+1>=EditorRenderer.TextLayout(callout).Height,"Auto-fit callout includes all wrapped lines: "+style);
                var d=new EditorDocument{OriginalWidth=800,OriginalHeight=600,Annotations=[callout]};var before=Pixels(EditorRenderer.Render(source,d));d.Validate(800,600,"");var after=EditorDocument.Deserialize(d.Serialize());Check(before.SequenceEqual(Pixels(EditorRenderer.Render(source,after))),"Callout body/tail/text roundtrip: "+style);
            }
            foreach(var style in Enum.GetValues<StepStyle>()){var step=new Annotation{Kind=AnnotationKind.Step,X1=150,Y1=150,Number=9999,FontSize=32,StepStyle=style};var number=EditorRenderer.TextLayout(step,"9999");var bounds=EditorRenderer.StepGeometry(step).Bounds;Check(bounds.Width>=number.WidthIncludingTrailingWhitespace&&bounds.Height>=number.Height,"Marker fits four-digit number: "+style);}
            var fallback=text.Copy();fallback.FontFamily="MissingFontForCaptovixTest";Check(EditorRenderer.TextFont(fallback).Source=="Segoe UI","Unavailable font uses explicit local fallback");
            var data=new DataObject();string copied=AnnotationClipboard.Copy(textDoc,[text.Id]);data.SetData(AnnotationClipboard.Format,new MemoryStream(Encoding.UTF8.GetBytes(copied)),false);var stream=(MemoryStream)data.GetData(AnnotationClipboard.Format,false)!;Check(AnnotationClipboard.Read(Encoding.UTF8.GetString(stream.ToArray())).Objects[0].Bold,"WPF custom data object preserves editable typography payload");
            var app=new App();app.InitializeComponent();var window=new MainWindow();var flags=BindingFlags.Instance|BindingFlags.NonPublic;typeof(MainWindow).GetField("_session",flags)!.SetValue(window,session);var editor=(EditorSurface)window.FindName("Surface");editor.Load(source,session);editor.SelectIds([rectId]);
            ((CheckBox)window.FindName("ShapeFillEnabled")).IsChecked=false;Check(!session.Document.Annotations.First(a=>a.Id==rectId).FillEnabled,"Unchecking fill updates selected shape immediately without Apply");
            typeof(MainWindow).GetMethod("SelectTool",flags)!.Invoke(window,new object[]{EditorTool.Step});editor.PointerDown(new(700,500));editor.PointerDown(new(100,100));editor.PointerUp(new(100,100));Check(editor.SelectedId==rectId&&((TextBlock)window.FindName("PropertyTitle")).Text.Contains("chữ nhật",StringComparison.OrdinalIgnoreCase),"Smart targeting updates MainWindow properties to actual rectangle while Step remains drawing tool");
            Check(window.Title.Contains("V"+VersionInfo.Current)&&window.WindowState==WindowState.Maximized,"Current Editor initializes successfully and opens maximized");window.Close();
            Check(Pixels(source).SequenceEqual(raw),"All text and pointer operations preserve original image pixels");Console.WriteLine($"{count} text studio Windows checks passed. Real IME, OS clipboard, DPI and pointer devices remain manual acceptance.");return 0;
        }catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
}
