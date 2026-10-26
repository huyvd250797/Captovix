using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captovix;
internal static class Program
{
    private static int checks;
    private static void Check(bool value,string label){if(!value)throw new Exception(label);checks++;Console.WriteLine("PASS: "+label);}
    private static byte[] Pixels(BitmapSource image){var bytes=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(bytes,image.PixelWidth*4,0);return bytes;}
    [STAThread] private static int Main()
    {
        try{
            var raw=Enumerable.Repeat((byte)255,640*480*4).ToArray();var image=BitmapSource.Create(640,480,96,96,PixelFormats.Bgra32,null,raw,640*4);image.Freeze();
            var doc=new EditorDocument{OriginalWidth=640,OriginalHeight=480,SourceHash="fixture"};
            var left=new Annotation{Kind=AnnotationKind.Rectangle,X1=30,Y1=60,X2=130,Y2=160};var right=new Annotation{Kind=AnnotationKind.Ellipse,X1=400,Y1=230,X2=550,Y2=370};
            var arrow=new Annotation{Kind=AnnotationKind.Arrow,Route=ConnectorRoute.Curve,Control=new(320,30),Color="#FF0000",DoubleArrow=true,StartAnchor=new(left.Id,1,.5),EndAnchor=new(right.Id,0,.5)};doc.Annotations.AddRange([left,right,arrow]);ConnectorStudio.Resolve(doc);
            foreach(var route in Enum.GetValues<ConnectorRoute>()){
                arrow.Route=route;var output=EditorRenderer.Render(image,doc);Check(output.PixelWidth==640&&output.PixelHeight==480,"Path export keeps source pixel dimensions: "+route);
                var path=ConnectorStudio.Samples(arrow);var middle=path[path.Length/2];Check(EditorRenderer.Hit(arrow,new(middle.X,middle.Y),3),"WPF hit test matches routed drawing: "+route);
                Check(!Pixels(output).SequenceEqual(raw),"Rendered connector and heads produce visible pixels: "+route);
                var roundtrip=EditorDocument.Deserialize(doc.Serialize());roundtrip.Validate(640,480,"fixture");Check(Pixels(output).SequenceEqual(Pixels(EditorRenderer.Render(image,roundtrip))),"WPF exported pixels survive save/reopen: "+route);
            }
            arrow.Route=ConnectorRoute.Curve;var session=new EditorSession(doc);var surface=new EditorSurface();surface.Load(image,session);surface.SelectIds([arrow.Id]);var before=Pixels(surface.Flatten());surface.SetZoom(.25);Check(Pixels(surface.Flatten()).SequenceEqual(before),"Changing display zoom does not alter connector export");Check(Pixels(image).SequenceEqual(raw),"Connector editing never mutates original source image");
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;var handles=(Point[])typeof(EditorSurface).GetMethod("ObjectHandles",flags)!.Invoke(surface,null)!;Check(handles.Length==3,"Curve exposes two endpoints and one control handle");
            session.Change(d=>left.Translate(40,20));surface.Refresh();Check(arrow.X1==170&&arrow.Y1==130,"Live WPF surface follows moved target");surface.SetConnectorRoute(ConnectorRoute.Elbow);Check(arrow.Route==ConnectorRoute.Elbow&&arrow.StartAnchor!=null,"Changing routing keeps existing target link");surface.DetachConnectors();Check(arrow.StartAnchor==null&&arrow.EndAnchor==null,"Detach control keeps path geometry and removes anchors");session.Undo();surface.Refresh();Check(surface.Selected!.StartAnchor!=null,"Undo detach restores the link");
            var plain=new Annotation{Kind=AnnotationKind.Arrow,X1=250,Y1=30,X2=330,Y2=60,Rotation=25};session.Document.Annotations.Add(plain);surface.SelectIds([plain.Id]);var oldStart=EditorRenderer.WorldPoint(plain,new(plain.X1,plain.Y1));
            typeof(EditorSurface).GetField("_originals",flags)!.SetValue(surface,new[]{plain.Copy()});typeof(EditorSurface).GetField("_handle",flags)!.SetValue(surface,1);
            session.Change(d=>typeof(EditorSurface).GetMethod("ResizeObjectSelection",flags)!.Invoke(surface,new object[]{new Point(90,130),false}));
            Check(plain.EndAnchor?.TargetId==left.Id&&plain.Rotation==0&&Math.Abs(plain.X1-oldStart.X)<1e-7&&Math.Abs(plain.Y1-oldStart.Y)<1e-7,"Previously unlinked rotated straight arrow can attach an endpoint without moving the opposite end");
            typeof(EditorSurface).GetField("_originals",flags)!.SetValue(surface,Array.Empty<Annotation>());surface.SelectIds([arrow.Id]);
            var app=new App();app.InitializeComponent();var window=new MainWindow();Check(window.WindowState==WindowState.Maximized&&window.Title.Contains("V1.17.0"),"MainWindow initializes successfully and opens maximized");
            typeof(MainWindow).GetField("_session",flags)!.SetValue(window,session);var editor=(EditorSurface)window.FindName("Surface");editor.Load(image,session);editor.SelectIds([arrow.Id]);
            object? Invoke(string name,params object[] args)=>typeof(MainWindow).GetMethod(name,flags)!.Invoke(window,args);
            Invoke("BuildCanvasMenu");var menu=editor.ContextMenu;var order=menu.Items.OfType<MenuItem>().Single(m=>Equals(m.Header,"Sắp xếp lớp"));Check(order.IsEnabled&&order.Items.OfType<MenuItem>().Count()==4,"Canvas right-click menu has four functional layer commands");
            Check(window.FindName("OrderPanel")==null&&window.FindName("AlignmentPanel")==null,"Right inspector no longer contains ordering or alignment button panels");
            order.Items.OfType<MenuItem>().Single(m=>Equals(m.Header,"Xuống sau cùng")).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));Check(session.Document.Annotations[0].Id==arrow.Id,"Context menu sends selected arrow to back");
            session.Undo();editor.Refresh();editor.SelectIds([arrow.Id]);Invoke("BuildCanvasMenu");menu.Items.OfType<MenuItem>().Single(m=>Equals(m.Header,"Khóa đối tượng")).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));Check(editor.Selected!.Locked,"Context menu lock changes the selected object");Invoke("BuildCanvasMenu");Check(!menu.Items.OfType<MenuItem>().Single(m=>Equals(m.Header,"Sắp xếp lớp")).IsEnabled,"Locked selection disables ordering actions");
            menu.Items.OfType<MenuItem>().Single(m=>Equals(m.Header,"Mở khóa đối tượng")).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));Check(!editor.Selected!.Locked,"Context menu can unlock a layer-selected object");
            editor.SelectIds([]);Invoke("BuildCanvasMenu");Check(!menu.Items.OfType<MenuItem>().Single(m=>Equals(m.Header,"Sắp xếp lớp")).IsEnabled,"Empty canvas selection cannot reorder unrelated objects");
            window.Close();Console.WriteLine($"{checks} connector Windows checks passed. Global hotkeys, accessibility providers, hook input and mixed-DPI capture require desktop acceptance.");return 0;
        }catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
}
