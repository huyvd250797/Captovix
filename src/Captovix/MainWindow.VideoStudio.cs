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
        ImageInspectorHeading.Visibility=video?Visibility.Collapsed:Visibility.Visible;
        VideoTools.Visibility=video?Visibility.Visible:Visibility.Collapsed;
        VideoMarkTools.Visibility=VideoInspector.Visibility=VideoStudioTimeline.Visibility=VideoStudioCommands.Visibility=
            studio?Visibility.Visible:Visibility.Collapsed;
        VideoStudioBack.Visibility=studio?Visibility.Visible:Visibility.Collapsed;
        VideoStudioButton.Visibility=VideoViewerSeek.Visibility=studio?Visibility.Collapsed:Visibility.Visible;
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
        if(_videoDecoderSuspended)OpenVideoMedia();
        else CancelVideoPoster();
        var bounds=RestoreBounds;
        _videoStudioWindow=(WindowStyle,ResizeMode,WindowState,bounds);
        WindowState=WindowState.Normal;
        WindowStyle=WindowStyle.None;
        ResizeMode=ResizeMode.NoResize;
        WindowState=WindowState.Maximized;
        UpdateVideoWorkspace();
        RefreshVideoControls();
        RequestVideoAudio();
        VideoStudioBack.Focus();
    }

    private void ExitVideoStudio()
    {
        if(!_videoStudio||_exportInProgress)return;
        PauseVideoForEditing();
        _videoStudio=false;
        ClearVideoAudio();
        VideoStrip.Show(null,null);
        VideoFavoriteTiles.Children.Clear();
        VideoCalloutTiles.Children.Clear();
        _videoFavoriteKey=_videoCalloutGalleryKey=null;
        _videoShownFavorites=null;
        _videoMotionShownClip=null;
        _previousVideoTracks=null;
        ClearVideoVisuals();
        ReleaseCameraPreview();

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
        SuspendVideoDecoder();
    }

    private void InitializeVideoTools()
    {
        InitializeCameraPreview();
        Panel.SetZIndex(_cueLayer,5);
        VideoOverlay.Children.Add(_cueLayer);
        VideoInkFont.DropDownOpened+=(_,_)=>{
            VideoInkFont.ItemsSource??=System.Windows.Media.Fonts.SystemFontFamilies.Select(font=>font.Source).Order().ToArray();
        };
        void Icon(Button button,string key,string label,bool toolbar=true){
            button.Content=AppIcons.Content(key,label);
            if(toolbar)button.SetResourceReference(StyleProperty,"ToolbarButton");
        }
        Icon(VideoStudioBack,"Back","Quay lại");
        Icon(VideoStudioButton,"StudioVideo","Vào Studio");
        Icon(VideoThumbnailButton,"Thumbnail","Ảnh Thư viện");
        Icon(VideoAddClipButton,"Add","Thêm clip");
        Icon(VideoSplitButton,"SplitVideo","Split");
        Icon(VideoDeleteClipButton,"Trash","Xóa đoạn");
        Icon(VideoUndoButton,"Undo","Hoàn tác");
        Icon(VideoRedoButton,"Redo","Làm lại");
        Icon(VideoExportClipButton,"Save","Xuất đoạn");
        Icon(VideoExportButton,"Export","Xuất MP4",false);
        Icon(VideoMusicButton,"MusicVideo","Nhạc nền");
        Icon(VideoOpenProjectButton,"FolderLocation","Mở dự án");
        Icon(VideoSaveProjectButton,"Save","Lưu dự án");
        Icon(VideoRelinkButton,"FolderLocation","Liên kết nguồn");
        Icon(VideoCancelExportButton,"Close","Hủy xuất");
        Icon(VideoMotionApply,"Settings","Áp dụng cho đoạn");
        Icon(VideoMotionAdd,"KeyframeVideo","Lưu keyframe");
        Icon(VideoMotionRemove,"Trash","Xóa keyframe");
        foreach(var button in VideoMarkTools.Children.OfType<Button>()){
            var key=(string)button.Tag;
            button.Content=AppIcons.Content(key,EditorToolLabel(key));
            button.SetResourceReference(StyleProperty,"ToolbarButton");
        }
    }

    private static string EditorToolLabel(string key)=>key switch{
        "Select"=>"Chọn","Shape"=>"Khung","Callout"=>"Callout","Arrow"=>"Mũi tên",
        "Text"=>"Chữ","Title"=>"Tiêu đề","Step"=>"Bước","Blur"=>"Làm mờ","Highlight"=>"Highlight","Spotlight"=>"Spotlight",
        "Magnifier"=>"Phóng đại","Freehand"=>"Vẽ tự do","Crop"=>"Crop","PixelRectangle"=>"Chọn vùng","PixelEllipse"=>"Ellipse","PixelLasso"=>"Lasso",_=>key
    };
}
