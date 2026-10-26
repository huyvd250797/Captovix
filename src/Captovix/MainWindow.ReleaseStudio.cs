using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private void LibraryMore_Click(object sender,RoutedEventArgs e)=>LibraryMorePopup.IsOpen=!LibraryMorePopup.IsOpen;
    private void ExtendCanvas_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||_busy)return;EndInlineText(true);Surface.CancelInteraction();
        var session=_session;var w=new Window{Owner=this,Title="Canvas · Lề và nền",Width=460,Height=500,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner};
        var root=new StackPanel{Margin=new(20)};w.Content=root;
        root.Children.Add(new TextBlock{Text="Không gian cho ý tưởng",FontSize=21,FontWeight=FontWeights.SemiBold});
        root.Children.Add(new TextBlock{Text="Thêm phần trống quanh canvas hiện tại, không resize ảnh gốc.",TextWrapping=TextWrapping.Wrap,Margin=new(0,6,0,16)});
        var fields=new System.Windows.Controls.Primitives.UniformGrid{Columns=2};root.Children.Add(fields);var values=new List<TextBox>();
        foreach(var name in new[]{"Trái (px)","Trên (px)","Phải (px)","Dưới (px)"}){var cell=new StackPanel{Margin=new(0,0,10,8)};cell.Children.Add(new TextBlock{Text=name});var input=new TextBox{Text="0",MaxLength=5};values.Add(input);cell.Children.Add(input);fields.Children.Add(cell);}
        var error=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(0,8,0,6)};error.SetResourceReference(TextBlock.ForegroundProperty,"Warning");
        var add=new Button{Content="Thêm phần mở rộng",HorizontalAlignment=HorizontalAlignment.Left};root.Children.Add(add);
        add.Click+=(_,_)=>{try{var n=values.Select(v=>int.Parse(v.Text,System.Globalization.CultureInfo.InvariantCulture)).ToArray();session.Change(d=>{d.Appearance=CanvasEditing.Expand(d.Viewport,d.Appearance,n[0],n[1],n[2],n[3]);d.Version=14;});foreach(var v in values)v.Text="0";Surface.Refresh();SyncProperties();FitImage();error.Text="Đã mở rộng · Ctrl+Z để hoàn tác.";}catch(Exception ex){error.Text=ex.Message;}};
        var auto=new CheckBox{Content="Tự mở rộng khi vẽ sát mép",IsChecked=session.Document.Appearance.AutoExpand,Margin=new(2,14,2,8)};root.Children.Add(auto);
        auto.Click+=(_,_)=>{session.Change(d=>{d.Appearance.AutoExpand=auto.IsChecked==true;d.Version=14;});SyncProperties();};
        var color=new Button{Content=AppIcons.Content("Fill","Nền canvas · chọn màu / trong suốt"),HorizontalAlignment=HorizontalAlignment.Left};root.Children.Add(color);
        color.Click+=(_,_)=>OpenPaint(color,value=>{session.Change(d=>{d.Appearance.Transparent=value==null;if(value!=null)d.Appearance.BackgroundColor=value;});Surface.Refresh();SyncProperties();});
        root.Children.Add(new TextBlock{Text="Có thể vẽ trực tiếp trong khoảng trống quanh ảnh. Crop dùng tọa độ đầu ra, bao gồm cả chú thích. Nền và tự mở rộng áp dụng ngay.",TextWrapping=TextWrapping.Wrap,Margin=new(0,12,0,0)});root.Children.Add(error);
        var close=new Button{Content="Đóng",HorizontalAlignment=HorizontalAlignment.Right};close.Click+=(_,_)=>w.Close();root.Children.Add(close);w.ShowDialog();
    }

    private void PerformanceReport_Click(object sender,RoutedEventArgs e)
    {
        var report=new{Version=VersionInfo.Current,Runtime=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,MemoryManaged=GC.GetTotalMemory(false),MemoryProcess=Environment.WorkingSet,Media=_store.Items.Count,Recent=_recentItems.Count,Thumbnails=_store.ThumbnailCacheStats,Decoded=RasterPixels.CacheStats,Renders=RenderMetrics.Snapshot(),History=_session?.HistoryStats,WindowsDesktopAcceptance="Chưa tự kết luận; xem checklist V1.21"};string json=JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true});var box=new TextBox{Text=json,IsReadOnly=true,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};var w=new Window{Owner=this,Title="Captovix · Hiệu năng thực tế trên máy này",Width=660,Height=540,WindowStartupLocation=WindowStartupLocation.CenterOwner};var panel=new DockPanel{Margin=new(14)};w.Content=panel;var copy=new Button{Content="Copy báo cáo",HorizontalAlignment=HorizontalAlignment.Left};DockPanel.SetDock(copy,Dock.Bottom);panel.Children.Add(copy);copy.Click+=(_,_)=>Clipboard.SetText(json);panel.Children.Add(box);w.ShowDialog();
    }
}
