using System.IO;
using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
internal static class Program
{
    static int n;static void Check(bool ok,string label){if(!ok)throw new Exception(label);n++;Console.WriteLine("PASS: "+label);}
    static void Pump(){var f=new DispatcherFrame();Dispatcher.CurrentDispatcher.BeginInvoke(()=>f.Continue=false,DispatcherPriority.ContextIdle);Dispatcher.PushFrame(f);}
    [STAThread]static int Main(){App? app=null;Window? owner=null;MainWindow? main=null;try{
        app=new App();app.InitializeComponent();app.ShutdownMode=ShutdownMode.OnExplicitShutdown;
        var button=new Button{Content="Hybrid Save",Width=140,Height=40};owner=new Window{Title="Hybrid UI probe fixture",Width=440,Height=260,Content=button,Topmost=true,ShowInTaskbar=false};owner.Show();Pump();
        foreach(bool dark in new[]{false,true}){
            app.Resources["Surface"]=dark?Brushes.Black:Brushes.White;
            var menu=new ContextMenu{PlacementTarget=button};var order=new MenuItem{Header="Sắp xếp lớp"};foreach(string title in new[]{"Lên trước cùng","Lên một lớp","Xuống một lớp","Xuống sau cùng"})order.Items.Add(new MenuItem{Header=title});menu.Items.Add(new MenuItem{Header="Mở vị trí tệp"});menu.Items.Add(order);button.ContextMenu=menu;
            for(int i=0;i<4;i++){menu.IsOpen=true;Pump();Check(menu.IsOpen&&LogicalTreeHelper.GetParent(menu) is not Border,"ContextMenu is not wrapped in an illegal parent: "+dark+" / "+i);order.IsSubmenuOpen=true;Pump();Check(order.IsSubmenuOpen&&order.Items.Count==4,"Layer submenu opens with four commands");order.IsSubmenuOpen=false;menu.IsOpen=false;Pump();}
            var tip=new ToolTip{Content="Ảnh trong thư viện",PlacementTarget=button};button.ToolTip=tip;for(int i=0;i<3;i++){tip.IsOpen=true;Pump();Check(tip.IsOpen&&LogicalTreeHelper.GetParent(tip) is not Border,"ToolTip retains WPF popup ownership");tip.IsOpen=false;Pump();}
        }
        // Keep the dispatcher pumping while UIA inspects this live test window.
        var hwnd=new WindowInteropHelper(owner).Handle;var q=new HybridQuery([new((long)hwnd,(uint)Environment.ProcessId,new(0,0,32767,32767))],Native.Displays().Select(d=>new HybridDisplay(d.Bounds,d.DpiX,d.DpiY)).ToArray(),-1,(long)hwnd);
        string dotnet=Path.Combine(Environment.GetEnvironmentVariable("DOTNET_ROOT")??Path.GetDirectoryName(Environment.ProcessPath!)!,"dotnet.exe");if(!File.Exists(dotnet))dotnet="dotnet";
        var query=new HybridProbeClient(dotnet,[typeof(App).Assembly.Location,"--hybrid-ui-probe"]).QueryAsync(q,default);while(!query.IsCompleted)Pump();var nodes=query.GetAwaiter().GetResult();Check(nodes.Any(x=>x.Type=="Button"&&x.Name=="Hybrid Save"),"UIA worker reads visible live WPF button without blocking dispatcher");var node=nodes.First(x=>x.Type=="Button"&&x.Name=="Hybrid Save");var point=button.PointToScreen(new(0,0));Check(Math.Abs(node.Bounds.X-point.X)<2&&Math.Abs(node.Bounds.Y-point.Y)<2,"UIA rectangle agrees with physical WPF screen coordinates");
        owner.Close();owner=null;
        main=new MainWindow();var flags=BindingFlags.Instance|BindingFlags.NonPublic;var surface=(EditorSurface)typeof(MainWindow).GetField("Surface",flags)!.GetValue(main)!;
        var doc=new EditorDocument{OriginalWidth=300,OriginalHeight=180};var lower=new Annotation{Kind=AnnotationKind.Rectangle,X1=20,Y1=20,X2=120,Y2=90,FillEnabled=true};var upper=new Annotation{Kind=AnnotationKind.Rectangle,X1=150,Y1=20,X2=250,Y2=90,FillEnabled=true};doc.Annotations.AddRange([lower,upper]);var session=new EditorSession(doc);
        typeof(MainWindow).GetField("_session",flags)!.SetValue(main,session);typeof(MainWindow).GetField("_editorReady",flags)!.SetValue(main,true);surface.Load(ImageTransfer.Bitmap(300,180,Enumerable.Repeat((byte)255,300*180*4).ToArray()),session);surface.PrepareContextSelection(new(180,40));Check(surface.SelectedIds.Contains(upper.Id),"Right click selects the object under pointer");
        typeof(MainWindow).GetMethod("BuildCanvasMenu",flags)!.Invoke(main,null);var layer=surface.ContextMenu.Items.OfType<MenuItem>().Single(m=>m.Header as string=="Sắp xếp lớp");Check(layer.IsEnabled&&layer.Items.Count==4,"Editor context menu restores enabled layer ordering for selected image/object");
        var before=doc.Annotations.Select(a=>a.Id).ToArray();((MenuItem)layer.Items[3]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));Check(session.Document.Annotations[0].Id==upper.Id,"Send-to-back menu actually changes document stacking order");session.Undo();Check(session.Document.Annotations.Select(a=>a.Id).SequenceEqual(before),"Layer order command is undoable");
        main.Close();main=null;Console.WriteLine(n+" Hybrid UI/popup/canvas checks passed on Windows.");return 0;
    }catch(Exception e){Console.Error.WriteLine(e);return 1;}finally{main?.Close();owner?.Close();app?.Shutdown();}}
}
