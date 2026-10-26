using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private bool _libraryBottom;
    private double _libraryWidth=190,_filmstripHeight=160,_inspectorWidth=250;
    private void Sidebar_Resized(object sender,System.Windows.Controls.Primitives.DragCompletedEventArgs e){if(!_libraryBottom)_libraryWidth=LibraryColumn.ActualWidth;else _filmstripHeight=FilmstripRow.ActualHeight;if(InspectorPane.Visibility==Visibility.Visible)_inspectorWidth=InspectorColumn.ActualWidth;SaveLibraryLayout();if(_session!=null){_fitMode=true;_actualPixels=false;FitImage();}}
    private Style? _mediaCardStyle;
    private Window? _settingsDialog;
    private void InitializeShell()
    {
        try{var path=Path.Combine(App.DataRoot,"editor-layout.json");if(File.Exists(path)){using var d=JsonDocument.Parse(File.ReadAllText(path));_libraryBottom=d.RootElement.TryGetProperty("LibraryBottom",out var b)&&b.GetBoolean();if(d.RootElement.TryGetProperty("LibraryWidth",out var w))_libraryWidth=Math.Clamp(w.GetDouble(),140,480);if(d.RootElement.TryGetProperty("FilmstripHeight",out var h))_filmstripHeight=Math.Clamp(h.GetDouble(),110,400);if(d.RootElement.TryGetProperty("InspectorWidth",out var i))_inspectorWidth=Math.Clamp(i.GetDouble(),210,520);InspectorColumn.Width=new(_inspectorWidth);}}catch(Exception ex){App.Log(ex);}
        MediaLocationButton.ToolTip="Lưu media mới: "+_store.Root+"\nBấm để chọn thư mục khác.";ApplyLibraryLayout();LibraryMorePopup.Child.AddHandler(Button.ClickEvent,new RoutedEventHandler((_,_)=>LibraryMorePopup.IsOpen=false));
    }
    private void ApplyLibraryLayout()
    {
        LibraryColumn.MinWidth=_libraryBottom?0:140;LibraryColumn.MaxWidth=_libraryBottom?0:480;LibraryColumn.Width=new(_libraryBottom?0:_libraryWidth);FilmstripRow.MinHeight=_libraryBottom?110:0;FilmstripRow.MaxHeight=_libraryBottom?400:0;FilmstripRow.Height=new(_libraryBottom?_filmstripHeight:0);
        LibrarySplitter.Visibility=_libraryBottom?Visibility.Collapsed:Visibility.Visible;FilmstripSplitter.Visibility=_libraryBottom?Visibility.Visible:Visibility.Collapsed;
        InspectorColumn.MinWidth=InspectorPane.Visibility==Visibility.Visible?210:0;InspectorColumn.MaxWidth=520;MediaList.Tag=_libraryBottom?Math.Clamp(_filmstripHeight-104,45,220):76d;
        Grid.SetRow(LibraryPane,_libraryBottom?2:0);Grid.SetColumn(LibraryPane,0);Grid.SetColumnSpan(LibraryPane,_libraryBottom?3:1);
        Grid.SetColumn(EditorPane,_libraryBottom?0:2);Grid.SetColumnSpan(EditorPane,_libraryBottom?3:1);
        DockPanel.SetDock(LibraryHeader,Dock.Top);LibraryHeader.Width=double.NaN;LibraryHeader.Visibility=_libraryBottom?Visibility.Collapsed:Visibility.Visible;
        var factory=new FrameworkElementFactory(typeof(VirtualizingStackPanel));factory.SetValue(VirtualizingStackPanel.OrientationProperty,_libraryBottom?Orientation.Horizontal:Orientation.Vertical);MediaList.ItemsPanel=new ItemsPanelTemplate(factory);
        _mediaCardStyle??=MediaList.ItemContainerStyle;var style=new Style(typeof(ListBoxItem),_mediaCardStyle);style.Setters.Add(new Setter(FrameworkElement.WidthProperty,_libraryBottom?Math.Clamp((_filmstripHeight-104)*1.65,120,350):double.NaN));MediaList.ItemContainerStyle=style;
        ScrollViewer.SetHorizontalScrollBarVisibility(MediaList,_libraryBottom?ScrollBarVisibility.Auto:ScrollBarVisibility.Disabled);ScrollViewer.SetVerticalScrollBarVisibility(MediaList,_libraryBottom?ScrollBarVisibility.Disabled:ScrollBarVisibility.Auto);
        Dispatcher.BeginInvoke(()=>{if(_fitMode&&_session!=null)FitImage();if(Selected!=null)MediaList.ScrollIntoView(Selected);});
    }
    private void LibraryLayout_Click(object sender,RoutedEventArgs e){_libraryBottom=(sender as MenuItem)?.Tag?.ToString()=="Bottom";SaveLibraryLayout();}
    private void ToggleLibrary_Click(object sender,RoutedEventArgs e){_libraryBottom=!_libraryBottom;SaveLibraryLayout();}
    private void SaveLibraryLayout(){ApplyLibraryLayout();try{MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"editor-layout.json"),JsonSerializer.Serialize(new{LibraryBottom=_libraryBottom,LibraryWidth=_libraryWidth,FilmstripHeight=_filmstripHeight,InspectorWidth=_inspectorWidth}));}catch(Exception ex){App.Log(ex);SetStatus("Chưa lưu được bố cục thư viện: "+ex.Message);}}
    private void ToggleInspector_Click(object sender,RoutedEventArgs e){bool hide=InspectorPane.Visibility==Visibility.Visible;InspectorPane.Visibility=hide?Visibility.Collapsed:Visibility.Visible;InspectorColumn.MinWidth=hide?0:210;InspectorColumn.Width=new(hide?0:_inspectorWidth);if(_fitMode)FitImage();}
    private void ExitMenu_Click(object sender,RoutedEventArgs e)=>RequestExit();
    private void SettingsMenu_Click(object sender,RoutedEventArgs e)=>OpenSettings((sender as MenuItem)?.Tag?.ToString()??"Capture");
    private void OpenSettings(string selected)
    {
        if(_settingsDialog!=null){_settingsDialog.Activate();return;}if(_busy){SetStatus("Kết thúc phiên chụp/quay trước khi mở thiết lập.");return;}
        FlushAutoProperties();
        var dialog=new Window{Owner=this,Title="Captovix · Thiết lập",Width=Math.Min(880,SystemParameters.WorkArea.Width-32),Height=Math.Min(660,SystemParameters.WorkArea.Height-32),MinWidth=Math.Min(720,SystemParameters.WorkArea.Width-32),MinHeight=Math.Min(440,SystemParameters.WorkArea.Height-32),WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false};_settingsDialog=dialog;
        var layout=new Grid{Margin=new(12)};layout.ColumnDefinitions.Add(new(){Width=new(192)});layout.ColumnDefinitions.Add(new());
        var tree=new TreeView{BorderThickness=new(0),Margin=new(0,0,12,0)};var host=new ContentControl();Grid.SetColumn(host,1);layout.Children.Add(tree);layout.Children.Add(host);dialog.Content=layout;
        var views=new Dictionary<string,FrameworkElement>{{"Capture",CaptureSettingsView},{"Hotkeys",HotkeysSettingsView},{"Updates",UpdatesExpander},{"Diagnostics",DiagnosticsSettingsView},{"Metadata",MetadataSettingsView}};
        views["Appearance"]=BuildAppearanceSettings();
        FrameworkElement? mounted=null;
        void Show(string key){if(mounted!=null){if(host.Content is ScrollViewer previous)previous.Content=null;host.Content=null;if(mounted!=views["Appearance"])SettingsStaging.Children.Add(mounted);}mounted=views[key];SettingsStaging.Children.Remove(mounted);if(mounted is Expander ex)ex.IsExpanded=true;host.Content=new ScrollViewer{Content=mounted,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};}
        foreach(var group in new[]{("Chụp & quay",new[]{("Capture","Chụp & quay"),("Hotkeys","Phím tắt & chạy nền")}), ("Hệ thống",new[]{("Updates","Cập nhật ứng dụng"),("Diagnostics","Máy & màn hình")}), ("Nội dung",new[]{("Metadata","Thông tin ảnh / video")})}){
            var node=new TreeViewItem{Header=group.Item1,IsExpanded=true,Margin=new(0,5,0,0)};foreach(var item in group.Item2){var child=new TreeViewItem{Header=item.Item2,Tag=item.Item1,Padding=new(4,7,2,7)};child.Selected+=(_,e)=>{if(e.OriginalSource==child){Show((string)child.Tag);e.Handled=true;}};node.Items.Add(child);if(item.Item1==selected)child.IsSelected=true;}tree.Items.Add(node);
        }
        var appearance=new TreeViewItem{Header="Giao diện · màu sắc",Tag="Appearance",Padding=new(4,7,2,7)};appearance.Selected+=(_,e)=>{if(e.OriginalSource==appearance){Show("Appearance");e.Handled=true;}};tree.Items.Add(appearance);if(selected=="Appearance")appearance.IsSelected=true;
        if(mounted==null)Show(views.ContainsKey(selected)?selected:"Capture");
        dialog.Closing+=(_,_)=>{FlushAutoProperties();if(mounted!=null){if(host.Content is ScrollViewer scroll)scroll.Content=null;host.Content=null;if(mounted!=views["Appearance"])SettingsStaging.Children.Add(mounted);mounted=null;}_settingsDialog=null;Dispatcher.BeginInvoke(RenderUpdateUi);};dialog.ShowDialog();
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root){foreach(var child in System.Windows.LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>()){yield return child;foreach(var next in Descendants(child))yield return next;}}
}
