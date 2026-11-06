using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
internal static class Program
{
 static int n;static void Check(bool value,string title){if(!value)throw new Exception(title);n++;Console.WriteLine("PASS: "+title);}
 [STAThread]static int Main(){App? app=null;Window? owner=null;try{
  app=new App();app.InitializeComponent();app.ShutdownMode=ShutdownMode.OnExplicitShutdown;
  var themes=app.Resources.MergedDictionaries.Single();Check(themes[typeof(TreeViewItem)] is Style,"Compiled application BAML loads with TreeViewItem style");
  foreach(var key in themes.Keys.Cast<object>().ToArray())_=themes[key];Check(true,"All compiled theme resources materialize");
  AppearanceManager.Apply();
  Check(app.Resources["BrandMark"] is System.Windows.Media.Imaging.BitmapSource { PixelWidth:128, PixelHeight:128 },"Default brand artwork loads on startup");
  var brandImage=typeof(AppearanceManager).GetMethod("BrandImage",BindingFlags.Static|BindingFlags.NonPublic)!;
  var custom=(System.Windows.Media.Imaging.BitmapSource)brandImage.Invoke(null,[new AppearanceSettings(IconBackground:"#244A69")])!;
  Check(custom.PixelWidth==128&&custom.PixelHeight==128,"Custom palette loads the mark resource from the app assembly");
  var chrome=typeof(MainWindow).Assembly.GetType("Captovix.WindowTheme",true)!;chrome.GetMethod("Initialize",BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,null);
  owner=new Window{Title="Startup regression fixture",Width=420,Height=300,Content=new TextBlock{Text="Owner"},ShowInTaskbar=false};owner.Show();Check(owner.Content is Border,"Normal window gets a frame without double-parent exception");
  foreach(bool dark in new[]{false,true}){
   app.Resources["Surface"]=dark?Brushes.Black:Brushes.White;app.Resources["Text"]=dark?Brushes.White:Brushes.Black;
   var content=new StackPanel();var item=new TreeViewItem{Header="Group",IsExpanded=true};item.Items.Add(new TreeViewItem{Header="Label"});var tree=new TreeView();tree.Items.Add(item);content.Children.Add(tree);
   var modal=new Window{Owner=owner,Title=dark?"Dark fixture":"Light fixture",Width=360,Height=220,Content=content,ShowInTaskbar=false};
   modal.Loaded+=(_,_)=>modal.Dispatcher.BeginInvoke(()=>{Check(modal.Content is Border,"Modal frame loaded: "+dark);Check(content.Parent is ContentControl,"Original modal content retains one logical parent");tree.ApplyTemplate();item.ApplyTemplate();Check(item.Template!=null,"TreeViewItem template loads in modal: "+dark);modal.Close();},DispatcherPriority.ContextIdle);
   modal.ShowDialog();Check(!modal.IsVisible&&owner.IsEnabled,"Modal closes and restores owner interaction: "+dark);
  }
  var child=new Border{Child=new TextBlock{Text="Popup"}};var popup=new Popup{PlacementTarget=owner,Child=child,AllowsTransparency=true};PopupPresentation.Attach(popup);popup.IsOpen=true;
  var frame=new DispatcherFrame();Dispatcher.CurrentDispatcher.BeginInvoke(()=>frame.Continue=false,DispatcherPriority.ContextIdle);Dispatcher.PushFrame(frame);
  Check(popup.Child is Border {Tag:"@popup-frame"}&&ReferenceEquals(child.Parent,popup.Child),"Popup surface wraps content once");Check(child.BorderThickness==new Thickness(0),"Popup has no second content border");Check(((Border)popup.Child).BorderThickness==new Thickness(1),"Popup outline is one DIP");var first=popup.Child;popup.IsOpen=false;popup.IsOpen=true;Check(ReferenceEquals(first,popup.Child),"Reopening popup does not reparent twice");popup.IsOpen=false;
  owner.Close();owner=null;Console.WriteLine(n+" native startup/popup checks passed. Full app profile/capture/updater acceptance separate.");return 0;
 }catch(Exception error){Console.Error.WriteLine(error);return 1;}finally{owner?.Close();app?.Shutdown();}}
}
