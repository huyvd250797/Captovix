using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
namespace Captovix;
public static class AppearanceManager
{
    public static AppearanceSettings Current {get;private set;}=new();
    public static string? Warning {get;private set;}
    public static void Initialize(){Current=AppearanceSettings.Load(App.DataRoot,out var warning);Warning=warning;Apply();SystemEvents.UserPreferenceChanged+=Changed;}
    public static void Stop()=>SystemEvents.UserPreferenceChanged-=Changed;
    private static void Changed(object sender,UserPreferenceChangedEventArgs e){if(Current.Mode==AppearanceMode.System)Application.Current.Dispatcher.BeginInvoke(Apply);}
    public static void RefreshSystem(){if(Current.Mode==AppearanceMode.System)Apply();}
    public static void Set(AppearanceSettings settings){settings.Validate();settings.Save(App.DataRoot);Current=settings;Warning=null;Apply();}
    private static bool SystemLight(){try{return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize","AppsUseLightTheme",1) is not int v||v!=0;}catch{return true;}}
    public static void Apply()
    {
        bool dark=AppearanceSettings.UseDark(Current.Mode,SystemLight());var r=Application.Current.Resources;
        void Brush(string key,string light,string night){var value=(Color)ColorConverter.ConvertFromString(dark?night:light);var brush=new SolidColorBrush(value);brush.Freeze();r[key]=brush;}
        Brush("Surface","#FFFFFF","#23262E");Brush("AppBackground","#F3F4F8","#1B1E25");Brush("Text","#243348","#DCE1EA");Brush("Muted","#586679","#A5AFBF");Brush("Line","#DCE3E9","#3B424F");Brush("Soft","#EEF1F7","#303641");Brush("Hover","#E7EBF2","#353D4B");Brush("Workspace","#E9EDF1","#181B21");Brush("Danger","#B42342","#F29AA9");Brush("Warning","#835300","#DFC28B");Brush("RecordBackground","#FFF1F2","#362930");Brush("RecordText","#BE123C","#EBA0B0");
        Brush("Accent",Current.Accent,Current.Accent);Brush("OnAccent",AppearanceSettings.WhiteText(Current.Accent)?"#FFFFFF":"#000000",AppearanceSettings.WhiteText(Current.Accent)?"#FFFFFF":"#000000");
        // A readable accent label/selection outline is distinct from the user's exact button paint.
        Brush("AccentText",AppearanceSettings.WhiteText(Current.Accent)?Current.Accent:"#5925AE",AppearanceSettings.WhiteText(Current.Accent)?"#C4A5FF":Current.Accent);
        r["BrandMark"]=new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Captovix;component/Assets/"+(dark?"Captovix-mark-light.png":"Captovix-mark.png")));
        var checker=new DrawingGroup();checker.Children.Add(new GeometryDrawing((Brush)r["Surface"],null,Geometry.Parse("M0,0 H16 V16 H0 Z")));checker.Children.Add(new GeometryDrawing((Brush)r["Soft"],null,Geometry.Parse("M0,0 H8 V8 H0 Z M8,8 H16 V16 H8 Z")));var grid=new DrawingBrush(checker){TileMode=TileMode.Tile,Viewport=new Rect(0,0,16,16),ViewportUnits=BrushMappingMode.Absolute};grid.Freeze();r["TransparencyGrid"]=grid;
        foreach(var key in new[]{SystemColors.WindowBrushKey,SystemColors.ControlBrushKey,SystemColors.MenuBrushKey})r[key]=r["Surface"];
        foreach(var key in new[]{SystemColors.WindowTextBrushKey,SystemColors.ControlTextBrushKey,SystemColors.MenuTextBrushKey,SystemColors.HighlightTextBrushKey,SystemColors.InactiveSelectionHighlightTextBrushKey})r[key]=r["Text"];
        r[SystemColors.HighlightBrushKey]=r["Soft"];r[SystemColors.InactiveSelectionHighlightBrushKey]=r["Soft"];r[SystemColors.GrayTextBrushKey]=r["Muted"];WindowTheme.ApplyAll();
    }
}
public partial class MainWindow
{
    private FrameworkElement BuildAppearanceSettings()
    {
        var panel=new StackPanel{Margin=new(12)};panel.Children.Add(new TextBlock{Text="Giao diện cá nhân",FontSize=22,FontWeight=FontWeights.SemiBold});
        panel.Children.Add(new TextBlock{Text="Áp dụng ngay · lưu riêng cho tài khoản Windows trên máy này. Không đổi màu ảnh hay chú thích.",TextWrapping=TextWrapping.Wrap,Margin=new(0,10,0,18)});
        var modes=new ComboBox{ItemsSource=new[]{"Theo hệ thống","Sáng","Tối"},SelectedIndex=(int)AppearanceManager.Current.Mode,MaxWidth=280,HorizontalAlignment=HorizontalAlignment.Left};panel.Children.Add(modes);
        var status=new TextBlock{Text=AppearanceManager.Warning??"",TextWrapping=TextWrapping.Wrap,Margin=new(4,12,4,6)};
        void Save(AppearanceSettings settings){try{AppearanceManager.Set(settings);status.Text="Đã lưu giao diện trên máy này.";}catch(Exception ex){App.Log(ex);status.Text="Chưa lưu được: "+ex.Message;}}
        modes.SelectionChanged+=(_,_)=>Save(AppearanceManager.Current with{Mode=(AppearanceMode)modes.SelectedIndex});
        panel.Children.Add(new TextBlock{Text="Màu nhấn",Margin=new(4,18,0,8),FontWeight=FontWeights.SemiBold});var palette=new WrapPanel();panel.Children.Add(palette);
        var selected=new TextBlock{Text=AppearanceManager.Current.Accent,Margin=new(4,8,0,0)};
        foreach(var color in new[]{"#7C3AED","#2563EB","#008B92","#C02667","#D97706","#64748B"}){var b=new Button{Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),Width=38,Height=38,ToolTip=color};b.Click+=(_,_)=>{Save(AppearanceManager.Current with{Accent=color});selected.Text=AppearanceManager.Current.Accent;};palette.Children.Add(b);}
        var custom=new Button{Content="Chọn màu khác…",HorizontalAlignment=HorizontalAlignment.Left};custom.Click+=(_,_)=>{var color=NativeColorPicker.Pick(_settingsDialog??this);if(color!=null){Save(AppearanceManager.Current with{Accent=color});selected.Text=AppearanceManager.Current.Accent;}};panel.Children.Add(custom);panel.Children.Add(selected);
        var reset=new Button{Content="Khôi phục tím Captovix · Theo hệ thống",HorizontalAlignment=HorizontalAlignment.Left,Margin=new(4,22,4,4)};reset.Click+=(_,_)=>{Save(new());modes.SelectedIndex=(int)AppearanceManager.Current.Mode;selected.Text=AppearanceManager.Current.Accent;};panel.Children.Add(reset);panel.Children.Add(status);return panel;
    }
}
