using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Captovix;
internal static class Program
{
    [STAThread]private static int Main()
    {
        int checks=0;void Check(bool yes,string label){if(!yes)throw new Exception(label);checks++;Console.WriteLine("PASS: "+label);}
        try{
            const int width=240,height=180;var original=new byte[width*height*4];for(int y=0;y<height;y++)for(int x=0;x<width;x++){int n=(y*width+x)*4;original[n]=(byte)(x*5%256);original[n+1]=(byte)(y*7%256);original[n+2]=(byte)((x+y)%2*240);original[n+3]=255;}var source=ImageTransfer.Bitmap(width,height,original);
            foreach(var kind in Enum.GetValues<AnnotationKind>()){
                var a=new Annotation{Kind=kind,X1=60,Y1=50,X2=160,Y2=110,TailX=120,TailY=140,Text="Chữ",FontSize=18,FillEnabled=true,FillColor="#008080",Rotation=30};if(kind==AnnotationKind.Freehand){a.Points=[new(60,50),new(100,90),new(160,110)];}
                var doc=new EditorDocument{OriginalWidth=width,OriginalHeight=height,Annotations=[a]};var result=EditorRenderer.Render(source,doc);var pixels=ImageTransfer.Bgra(result);
                Check(result.PixelWidth==width&&result.PixelHeight==height&&!pixels.SequenceEqual(original),"Rotated object renders without resizing the file: "+kind);
                Check(pixels.AsSpan(0,width*4).SequenceEqual(original.AsSpan(0,width*4)),"Untouched row retains exact pixels: "+kind);
                Check(ImageTransfer.Bgra(EditorRenderer.Render(source,EditorDocument.Deserialize(doc.Serialize()))).SequenceEqual(pixels),"Rotated object reopens pixel-identically: "+kind);
                a.Visible=false;Check(ImageTransfer.Bgra(EditorRenderer.Render(source,doc)).SequenceEqual(original),"Hidden object is excluded from export: "+kind);
            }
            var rectangle=new Annotation{Kind=AnnotationKind.Rectangle,X1=60,Y1=50,X2=160,Y2=110,Rotation=45,FillEnabled=true};var center=new Point(110,80);
            Check(EditorRenderer.Hit(rectangle,EditorRenderer.WorldPoint(rectangle,new(65,55)),1),"Hit testing inverse-transforms a rotated filled region");
            Check(!EditorRenderer.Hit(rectangle,new(55,45),1),"Rotated shape does not select its empty AABB corner");var unlockedPixels=ImageTransfer.Bgra(EditorRenderer.Render(source,new(){OriginalWidth=width,OriginalHeight=height,Annotations=[rectangle]}));rectangle.Locked=true;Check(unlockedPixels.SequenceEqual(ImageTransfer.Bgra(EditorRenderer.Render(source,new(){OriginalWidth=width,OriginalHeight=height,Annotations=[rectangle]}))),"Lock changes edit permission without changing exported appearance");Check(!EditorRenderer.Hit(rectangle,center,1),"Locked object cannot be hit on canvas");
            var scene=new EditorDocument{OriginalWidth=width,OriginalHeight=height,Annotations=[rectangle,new(){Kind=AnnotationKind.Ellipse,X1=30,Y1=30,X2=50,Y2=50,Visible=false},new(){Kind=AnnotationKind.Rectangle,X1=170,Y1=120,X2=200,Y2=150}]};var session=new EditorSession(scene);var surface=new EditorSurface();surface.Load(source,session);surface.SelectIds([rectangle.Id]);string locked=scene.Serialize();surface.Nudge(1,1);surface.DeleteSelected();Check(session.Document.Serialize()==locked&&!session.CanUndo,"Explicit layer selection cannot nudge or delete a locked object");surface.SelectAll();Check(surface.SelectedIds.Count==1&&surface.SelectedIds.Contains(scene.Annotations[2].Id),"Select all skips locked and hidden canvas objects");
            rectangle.Locked=false;scene.Annotations[1].Visible=true;surface.SelectIds([rectangle.Id]);var before=ImageTransfer.Bgra(surface.Flatten());surface.SetZoom(.25);Check(ImageTransfer.Bgra(surface.Flatten()).SequenceEqual(before),"Rotated selection handles and zoom do not enter export");
            // A 90-degree solid mask must protect its destination, not rotated source pixels.
            var mask=new Annotation{Kind=AnnotationKind.Cover,X1=70,Y1=70,X2=150,Y2=110,Rotation=90,Color="#123456"};var masked=new EditorDocument{OriginalWidth=width,OriginalHeight=height,Annotations=[mask]};var bytes=ImageTransfer.Bgra(EditorRenderer.Render(source,masked));int inside=(60*width+110)*4,outside=(90*width+75)*4;Check(bytes[inside]==0x56&&bytes[inside+1]==0x34&&bytes[inside+2]==0x12,"Rotated cover fills the correct world-space region");Check(bytes.AsSpan(outside,4).SequenceEqual(original.AsSpan(outside,4)),"Rotated cover leaves the former unrotated-only region untouched");
            var exactLine=new Annotation{Kind=AnnotationKind.Line,X1=10,Y1=20,X2=110,Y2=20,Rotation=90};var lineBounds=EditorRenderer.LocalBounds(exactLine);var turnedStart=EditorRenderer.WorldPoint(exactLine,new(10,20));var turnedEnd=EditorRenderer.WorldPoint(exactLine,new(110,20));Check(lineBounds.Height==0&&Math.Abs(turnedStart.X-60)<1e-8&&Math.Abs(turnedStart.Y+30)<1e-8&&Math.Abs(turnedEnd.X-60)<1e-8&&Math.Abs(turnedEnd.Y-70)<1e-8,"Horizontal line rotates around the exact midpoint with no half-pixel pivot offset");
            var app=new App();app.InitializeComponent();var window=new MainWindow();var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;object? Invoke(string name,params object[] values)=>typeof(MainWindow).GetMethod(name,flags)!.Invoke(window,values);
            var editor=(EditorSurface)window.FindName("Surface");typeof(MainWindow).GetField("_session",flags)!.SetValue(window,session);editor.Load(source,session);editor.SelectIds([rectangle.Id]);Invoke("SyncProperties");
            var rows=(ListBox)window.FindName("LayerList");Check(rows.Items.Count==3,"Layers panel shows all editable document objects");
            ((TextBox)window.FindName("LayerName")).Text="Khung chính";Invoke("RenameLayer_Click",window,new RoutedEventArgs());Check(rectangle.Name=="Khung chính","Layer name applies to the selected object");
            Invoke("LayerToggle_Click",new Button{Tag="Lock",CommandParameter=rectangle.Id},new RoutedEventArgs());Check(rectangle.Locked&&!((StackPanel)window.FindName("EditableObjectProperties")).IsEnabled,"Locking a layer disables style editing");
            Invoke("LayerToggle_Click",new Button{Tag="Lock",CommandParameter=rectangle.Id},new RoutedEventArgs());Invoke("LayerToggle_Click",new Button{Tag="Visible",CommandParameter=rectangle.Id},new RoutedEventArgs());Check(!rectangle.Visible&&!((StackPanel)window.FindName("ObjectFrameFields")).IsEnabled,"Hidden layer disables geometric edits and remains in the list");
            Invoke("LayerToggle_Click",new Button{Tag="Visible",CommandParameter=rectangle.Id},new RoutedEventArgs());editor.SelectIds([rectangle.Id]);
            ((CheckBox)window.FindName("ObjectKeepRatio")).IsChecked=false;((TextBox)window.FindName("ObjectX")).Text="40";((TextBox)window.FindName("ObjectY")).Text="45";((TextBox)window.FindName("ObjectWidth")).Text="110";((TextBox)window.FindName("ObjectHeight")).Text="55";((TextBox)window.FindName("ObjectAngle")).Text="30";
            Check((bool)Invoke("ApplyObjectFrame")!&&rectangle.X1==40&&rectangle.Y1==45&&rectangle.Width==110&&rectangle.Height==55&&rectangle.Rotation==30,"Frame inspector applies exact source coordinates and absolute angle");var saved=session.Document.Serialize();((TextBox)window.FindName("ObjectAngle")).Text="NaN";Check(!(bool)Invoke("ApplyObjectFrame")!&&session.Document.Serialize()==saved,"Invalid numeric input does not change layer geometry");
            session.Undo();editor.Refresh();Invoke("SyncProperties");Check(session.Document.Annotations[0].Rotation==45,"Numeric transform is one undo step");session.Redo();editor.Refresh();Invoke("SyncProperties");Check(session.Document.Annotations[0].Rotation==30,"Numeric transform redo restores the exact angle");
            Check(window.Title.Contains("V"+VersionInfo.Current)&&window.WindowState==WindowState.Maximized,"Unified Editor keeps current identity and opens maximized");window.Close();
            Console.WriteLine($"{checks} object Windows renderer/UI checks passed; native capture, real pointer and DPI matrix remain manual.");return 0;
        }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
