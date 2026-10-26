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
        LibraryMorePopup.IsOpen=false;if(_session==null||_busy)return;Surface.CancelInteraction();EndInlineText(true);try{_session.Change(d=>{d.Appearance=CanvasEditing.Expand(d.Viewport,d.Appearance);d.Version=13;});Surface.Refresh();FitImage();SetStatus("Đã nới canvas160px mỗi cạnh. Có thể vẽ khung/callout ngoài ảnh; số pixel xuất tăng, ảnh gốc không resize. Ctrl+Z để hoàn tác.");}catch(Exception ex){SetStatus(ex.Message);}
    }
    private void PerformanceReport_Click(object sender,RoutedEventArgs e)
    {
        var report=new{Version=VersionInfo.Current,Runtime=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,MemoryManaged=GC.GetTotalMemory(false),MemoryProcess=Environment.WorkingSet,Media=_store.Items.Count,Recent=_recentItems.Count,Thumbnails=_store.ThumbnailCacheStats,Decoded=RasterPixels.CacheStats,Renders=RenderMetrics.Snapshot(),History=_session?.HistoryStats,WindowsDesktopAcceptance="Chưa tự kết luận; xem checklist V1.20"};string json=JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true});var box=new TextBox{Text=json,IsReadOnly=true,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};var w=new Window{Owner=this,Title="Captovix · Hiệu năng thực tế trên máy này",Width=660,Height=540,WindowStartupLocation=WindowStartupLocation.CenterOwner};var panel=new DockPanel{Margin=new(14)};w.Content=panel;var copy=new Button{Content="Copy báo cáo",HorizontalAlignment=HorizontalAlignment.Left};DockPanel.SetDock(copy,Dock.Bottom);panel.Children.Add(copy);copy.Click+=(_,_)=>Clipboard.SetText(json);panel.Children.Add(box);w.ShowDialog();
    }
}
