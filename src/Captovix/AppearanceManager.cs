using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
namespace Captovix;
public static class AppearanceManager
{
    public static AppearanceSettings Current {get;private set;}=new();
    public static event Action? Applied;
    public static string? Warning {get;private set;}
    public static void Initialize(){Current=AppearanceSettings.Load(App.DataRoot,out var warning);Warning=warning;Apply();SystemEvents.UserPreferenceChanged+=Changed;}
    public static void Stop()=>SystemEvents.UserPreferenceChanged-=Changed;
    private static void Changed(object sender,UserPreferenceChangedEventArgs e){if(Current.Mode==AppearanceMode.System)Application.Current.Dispatcher.BeginInvoke(Apply);}
    public static void RefreshSystem(){if(Current.Mode==AppearanceMode.System)Apply();}
    public static void Set(AppearanceSettings settings){settings.Validate();settings.Save(App.DataRoot);Current=settings;Warning=null;Apply();}
    private static bool SystemLight(){try{return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize","AppsUseLightTheme",1) is not int v||v!=0;}catch{return true;}}
    internal static Brush Paint(string start,string end,bool gradient,double angle){var first=(Color)ColorConverter.ConvertFromString(start);if(!gradient){var solid=new SolidColorBrush(first);solid.Freeze();return solid;}var brush=new LinearGradientBrush(first,(Color)ColorConverter.ConvertFromString(end),angle);brush.Freeze();return brush;}
    internal static System.Windows.Media.Imaging.BitmapSource BrandImage(AppearanceSettings settings)
    {
        const double size = 128;
        const double scale = size / 256;
        var foreground = settings.IconUsesAccent ? settings.Accent : settings.IconColor;
        var foregroundEnd = settings.IconUsesAccent ? settings.AccentEnd : settings.IconEnd;
        var gradient = settings.IconUsesAccent ? settings.AccentGradient : settings.IconGradient;
        var originalPalette = !gradient && foreground.Equals("#FFFFFF", StringComparison.OrdinalIgnoreCase)
            && settings.IconBackground.Equals("#7C3AED", StringComparison.OrdinalIgnoreCase);
        var asset = originalPalette ? "Captovix-app.png" : "Captovix-mark.png";
        var assemblyName = typeof(AppearanceManager).Assembly.GetName().Name;
        var artwork = new System.Windows.Media.Imaging.BitmapImage(
            new Uri($"pack://application:,,,/{assemblyName};component/Assets/{asset}"));
        var visual = new DrawingVisual();
        var bounds = new Rect(0, 0, size, size);
        using (var dc = visual.RenderOpen())
        {
            if (originalPalette)
            {
                dc.DrawImage(artwork, bounds);
            }
            else
            {
                var background = Paint(settings.IconBackground, settings.IconBackground, false, 0);
                dc.DrawRoundedRectangle(background, null, bounds, 48 * scale, 48 * scale);
                // Placement from the original Captovix-app.svg.
                var logo = new Rect(47 * scale, 47 * scale, size * .633, size * .633);
                dc.PushOpacityMask(new ImageBrush(artwork));
                dc.DrawRectangle(Paint(foreground, foregroundEnd, gradient, settings.GradientAngle), null, logo);
                dc.Pop();
            }
        }
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)size, (int)size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
    public static void Apply()
    {
        bool dark=AppearanceSettings.UseDark(Current.Mode,SystemLight());var r=Application.Current.Resources;
        void Brush(string key,string light,string night){var value=(Color)ColorConverter.ConvertFromString(dark?night:light);var brush=new SolidColorBrush(value);brush.Freeze();r[key]=brush;}
        Brush("Surface", "#FFFFFF", "#27282D");
        Brush("AppBackground", "#F1F2F5", "#202126");
        Brush("CaptionSurface", "#E8E9F0", "#28292F");
        Brush("Text", "#202129", "#F0F0F3");
        Brush("Muted", "#575A65", "#B9BBC5");
        Brush("Line", "#D7D8DF", "#45464F");
        Brush("Soft", "#EEF0F5", "#303139");
        Brush("Hover", "#E3E5EC", "#393B44");
        Brush("Workspace", "#E3E5EC", "#191A1F");
        Brush("Danger", "#B42342", "#F29AA9");
        Brush("Warning", "#835300", "#DFC28B");
        Brush("PopupLine", "#92949E", "#6E707B");
        Brush("RecordBackground", "#D82038", "#D82038");
        Brush("RecordText", "#FFFFFF", "#FFFFFF");
        r["Accent"]=Paint(Current.Accent,Current.AccentEnd,Current.AccentGradient,Current.GradientAngle);Brush("OnAccent",AppearanceSettings.WhiteText(Current.Accent)?"#FFFFFF":"#000000",AppearanceSettings.WhiteText(Current.Accent)?"#FFFFFF":"#000000");
        // A readable accent label/selection outline is distinct from the user's exact button paint.
        Brush("AccentText",AppearanceSettings.WhiteText(Current.Accent)?Current.Accent:"#5925AE",AppearanceSettings.WhiteText(Current.Accent)?"#C4A5FF":Current.Accent);
        r["BrandMark"]=BrandImage(Current);
        foreach(Window window in Application.Current.Windows)window.SetResourceReference(Window.IconProperty,"BrandMark");
        var checker=new DrawingGroup();checker.Children.Add(new GeometryDrawing((Brush)r["Surface"],null,Geometry.Parse("M0,0 H16 V16 H0 Z")));checker.Children.Add(new GeometryDrawing((Brush)r["Soft"],null,Geometry.Parse("M0,0 H8 V8 H0 Z M8,8 H16 V16 H8 Z")));var grid=new DrawingBrush(checker){TileMode=TileMode.Tile,Viewport=new Rect(0,0,16,16),ViewportUnits=BrushMappingMode.Absolute};grid.Freeze();r["TransparencyGrid"]=grid;
        foreach(var key in new[]{SystemColors.WindowBrushKey,SystemColors.ControlBrushKey,SystemColors.MenuBrushKey})r[key]=r["Surface"];
        foreach(var key in new[]{SystemColors.WindowTextBrushKey,SystemColors.ControlTextBrushKey,SystemColors.MenuTextBrushKey,SystemColors.HighlightTextBrushKey,SystemColors.InactiveSelectionHighlightTextBrushKey})r[key]=r["Text"];
        r[SystemColors.HighlightBrushKey]=r["Soft"];r[SystemColors.InactiveSelectionHighlightBrushKey]=r["Soft"];r[SystemColors.GrayTextBrushKey]=r["Muted"];WindowTheme.ApplyAll();Applied?.Invoke();
    }
}
