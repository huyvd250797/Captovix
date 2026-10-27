using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation;

internal static class Program
{
    private sealed record Row(string Mã,string HọTên,string Lớp,int Số);
    [STAThread]private static void Main()
    {
        var app=new Application();var panel=new DockPanel{Margin=new(24)};
        var button=new Button{Content="Mở modal để thử chụp phần tử",Padding=new(12),HorizontalAlignment=HorizontalAlignment.Left};DockPanel.SetDock(button,Dock.Top);panel.Children.Add(button);
        panel.Children.Add(new TextBlock{Text="Nền cửa sổ / khối cha\n\nMở Captovix → Chụp phần tử. Rê lên bảng, dùng con lăn đi từ bảng tới nhóm và cửa sổ.\nClick khóa, kéo cạnh chỉnh, Enter chụp. Tab chọn lại.\nKéo trực tiếp trên khối đang gợi ý để thử vùng thủ công.\nThử ở DPI100–200% và trên màn hình có tọa độ âm.",FontSize=18,Margin=new(0,24,0,0)});
        var main=new Window{Title="Captovix · Smart Capture — Windows acceptance fixture",Width=1050,Height=760,Content=panel,Background=Brushes.AliceBlue};
        button.Click+=(_,_)=>{
            var grid=new DataGrid{AutoGenerateColumns=true,IsReadOnly=true,ItemsSource=Enumerable.Range(1,20).Select(i=>new Row("CV"+i.ToString("000"),"Nguyễn Văn "+i,"D20TA1",i)).ToArray(),Margin=new(8)};AutomationProperties.SetName(grid,"Bảng dữ liệu kiểm thử");
            var group=new GroupBox{Header="Khối chứa bảng",Content=grid,Margin=new(20)};AutomationProperties.SetName(group,"Nhóm bảng");
            var modal=new Window{Owner=main,Title="Modal kiểm thử · chọn khối cha/con",Width=720,Height=480,Content=group,WindowStartupLocation=WindowStartupLocation.CenterOwner};modal.ShowDialog();
        };
        app.Run(main);
    }
}
