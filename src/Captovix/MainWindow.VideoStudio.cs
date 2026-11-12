using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Captovix;

public partial class MainWindow
{
    private bool _videoStudio;
    private (WindowStyle Style,ResizeMode Resize,WindowState State,Rect Bounds)? _videoStudioWindow;
    private (Visibility Visibility,GridLength Width,double Minimum)? _videoInspectorBefore;

    private void UpdateVideoWorkspace()
    {
        bool video=_videoItem!=null;
        bool studio=video&&_videoStudio;
        VideoTools.Visibility=video?Visibility.Visible:Visibility.Collapsed;
        VideoMarkTools.Visibility=VideoInspector.Visibility=VideoStudioTimeline.Visibility=VideoStudioCommands.Visibility=
            studio?Visibility.Visible:Visibility.Collapsed;
        VideoStudioButton.Content=AppIcons.Content(studio?"Close":"Record",studio?"Thoát Studio":"Studio");
        MainChrome.Visibility=CaptureActions.Visibility=LibraryPane.Visibility=studio?Visibility.Collapsed:Visibility.Visible;
        if(video){
            _videoInspectorBefore??=(InspectorPane.Visibility,InspectorColumn.Width,InspectorColumn.MinWidth);
            InspectorPane.Visibility=studio?Visibility.Visible:Visibility.Collapsed;
            InspectorColumn.MinWidth=studio?210:0;
            InspectorColumn.Width=studio?new GridLength(Math.Max(260,_inspectorWidth)):new GridLength(0);
            MainInspectorSplitter.Visibility=studio?Visibility.Visible:Visibility.Collapsed;
        }else if(_videoInspectorBefore is {} previous){
            _videoInspectorBefore=null;
            InspectorPane.Visibility=previous.Visibility;
            InspectorColumn.MinWidth=previous.Minimum;
            InspectorColumn.Width=previous.Width;
            MainInspectorSplitter.Visibility=Visibility.Visible;
        }
        if(studio){
            LibraryColumn.MinWidth=0;
            LibraryColumn.Width=new(0);
            FilmstripRow.MinHeight=0;
            FilmstripRow.Height=new(0);
            FilmstripSplitter.Visibility=LibrarySplitter.Visibility=Visibility.Collapsed;
            Grid.SetColumn(EditorPane,0);
            Grid.SetColumnSpan(EditorPane,3);
        }else ApplyLibraryLayout();
        UpdateVideoMarks();
    }

    private void VideoStudio_Click(object sender,RoutedEventArgs e)
    {
        if(_videoStudio){ExitVideoStudio();return;}
        if(!RequireLicense()||!VideoCanEdit||_videoFullscreen!=null)return;
        PauseVideoForEditing();
        _videoStudio=true;
        var bounds=RestoreBounds;
        _videoStudioWindow=(WindowStyle,ResizeMode,WindowState,bounds);
        WindowState=WindowState.Normal;
        WindowStyle=WindowStyle.None;
        ResizeMode=ResizeMode.NoResize;
        WindowState=WindowState.Maximized;
        UpdateVideoWorkspace();
        RefreshVideoControls();
        RequestVideoAudio();
        VideoStudioButton.Focus();
    }

    private void ExitVideoStudio()
    {
        if(!_videoStudio||_exportInProgress)return;
        PauseVideoForEditing();
        _videoStudio=false;
        SetVideoTool(null);
        ClearVideoTrackSelection();
        if(_videoStudioWindow is {} saved){
            WindowState=WindowState.Normal;
            WindowStyle=saved.Style;
            ResizeMode=saved.Resize;
            if(!saved.Bounds.IsEmpty&&saved.Bounds.Width>0&&saved.Bounds.Height>0){
                Left=saved.Bounds.Left;Top=saved.Bounds.Top;Width=saved.Bounds.Width;Height=saved.Bounds.Height;
            }
            WindowState=saved.State;
        }
        _videoStudioWindow=null;
        UpdateVideoWorkspace();
    }

    private void InitializeVideoTools()
    {
        VideoInkFont.ItemsSource=System.Windows.Media.Fonts.SystemFontFamilies.Select(font=>font.Source).Order().ToArray();
        foreach(var button in VideoMarkTools.Children.OfType<Button>()){
            var key=(string)button.Tag;
            button.Content=AppIcons.Content(key,EditorToolLabel(key));
            button.SetResourceReference(StyleProperty,"ToolbarButton");
        }
    }

    private static string EditorToolLabel(string key)=>key switch{
        "Select"=>"Chọn","Shape"=>"Khung","Callout"=>"Callout","Arrow"=>"Mũi tên",
        "Text"=>"Chữ","Step"=>"Bước","Blur"=>"Làm mờ","Highlight"=>"Highlight","Spotlight"=>"Spotlight",
        "Magnifier"=>"Phóng đại","Freehand"=>"Vẽ tự do","Crop"=>"Crop","PixelRectangle"=>"Chọn vùng","PixelEllipse"=>"Ellipse","PixelLasso"=>"Lasso",_=>key
    };
}
