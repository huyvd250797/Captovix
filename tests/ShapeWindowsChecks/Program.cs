using System.Windows;
using System.Windows.Media;
using Captovix;
internal static class Program
{
    [STAThread]private static int Main()
    {
        int checks=0;void Check(bool yes,string text){if(!yes)throw new Exception(text);checks++;Console.WriteLine("PASS: "+text);}
        try{
            const int w=200,h=160;var bytes=Enumerable.Repeat((byte)255,w*h*4).ToArray();var source=ImageTransfer.Bitmap(w,h,bytes);
            foreach(var kind in ShapeStudio.Shapes){
                var a=new Annotation{Kind=kind,X1=20,Y1=20,X2=160,Y2=140,Color="#FF0000",FillColor="#008000",FillEnabled=true,FillOpacity=50,Stroke=4,CornerRadius=25};
                var doc=new EditorDocument{OriginalWidth=w,OriginalHeight=h,Annotations=[a]};var output=EditorRenderer.Render(source,doc);var pixels=ImageTransfer.Bgra(output);int center=(80*w+90)*4;
                Check(pixels[center]<150&&pixels[center+1]>170&&pixels[center+1]<210&&pixels[center+2]<150,"Independent 50% fill is composited: "+kind);
                Check(EditorRenderer.Hit(a,new(90,80),1),"Filled interior is selectable: "+kind);
                a.FillEnabled=false;Check(!EditorRenderer.Hit(a,new(90,80),1),"Hollow interior passes selection through: "+kind);
                a.FillEnabled=true;var before=ImageTransfer.Bgra(EditorRenderer.Render(source,doc));var surface=new EditorSurface();var session=new EditorSession(doc);surface.Load(source,session);surface.SelectIds([a.Id]);surface.Nudge(2,3);
                Check(a.X1==22&&a.Y1==23&&a.Kind==kind&&a.FillOpacity==50,"Surface selection/nudge retains new shape styling: "+kind);
                session.Undo();surface.SetZoom(.25);Check(ImageTransfer.Bgra(surface.Flatten()).SequenceEqual(before),"Undo/zoom/export restore identical pixels: "+kind);
                var again=EditorDocument.Deserialize(session.Document.Serialize());Check(ImageTransfer.Bgra(EditorRenderer.Render(source,again)).SequenceEqual(before),"New shape reopen renders identically: "+kind);
                Check(before.AsSpan(0,w*4).SequenceEqual(bytes.AsSpan(0,w*4)),"Untouched source row stays byte-identical: "+kind);
            }
            var line=new Annotation{Kind=AnnotationKind.Line,X1=20,Y1=80,X2=180,Y2=80,Color="#FF0000",Stroke=4};var lines=new EditorDocument{OriginalWidth=w,OriginalHeight=h,Annotations=[line]};
            var solid=ImageTransfer.Bgra(EditorRenderer.Render(source,lines));
            foreach(var dash in new[]{StrokePattern.Dash,StrokePattern.Dot,StrokePattern.DashDot}){line.Dash=dash;var pixels=ImageTransfer.Bgra(EditorRenderer.Render(source,lines));Check(!pixels.SequenceEqual(solid),"Dash pattern differs from solid in export: "+dash);Check(Enumerable.Range(25,150).Any(x=>pixels[(80*w+x)*4]>240)&&Enumerable.Range(25,150).Any(x=>pixels[(80*w+x)*4]<40),"Dash pattern has both ink and gaps: "+dash);Check(EditorRenderer.Hit(line,new(80,80),2),"Selection follows dashed line through gaps: "+dash);}
            line.StrokeEnabled=false;Check(ImageTransfer.Bgra(EditorRenderer.Render(source,lines)).SequenceEqual(bytes),"No stroke produces no accidental line");
            foreach(var tip in Enum.GetValues<ArrowTip>())foreach(bool both in new[]{false,true}){var a=new Annotation{Kind=AnnotationKind.Arrow,X1=25,Y1=80,X2=175,Y2=80,Tip=tip,DoubleArrow=both,ArrowHeadSize=30,Stroke=3,Color="#FF0000"};var d=new EditorDocument{OriginalWidth=w,OriginalHeight=h,Annotations=[a]};var output=ImageTransfer.Bgra(EditorRenderer.Render(source,d));Check(!output.SequenceEqual(bytes)&&EditorRenderer.Hit(a,new(173,80),2),$"Arrow renders and its head is selectable: {tip}/{both}");}
            var invisible=new Annotation{Kind=AnnotationKind.Rectangle,X1=10,Y1=10,X2=80,Y2=80,StrokeEnabled=false,FillEnabled=true,FillOpacity=0};Check(ImageTransfer.Bgra(EditorRenderer.Render(source,new(){OriginalWidth=w,OriginalHeight=h,Annotations=[invisible]})).SequenceEqual(bytes),"Zero opacity preserves all underlying pixels");
            var round=new Annotation{Kind=AnnotationKind.RoundedRectangle,X1=20,Y1=20,X2=120,Y2=100,CornerRadius=1000};Check(!EditorRenderer.ShapeGeometry(round).FillContains(new Point(21,21))&&EditorRenderer.ShapeGeometry(round).FillContains(new Point(70,60)),"Oversized corner radius clamps to shape bounds");
            Console.WriteLine($"{checks} shape Windows renderer checks passed. Real mouse/DPI and capture remain manual.");return 0;
        }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
