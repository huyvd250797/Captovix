using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Captovix;
// One overlay and one gesture model for suggested elements and manual regions.
public sealed class RegionPicker : Window
{
    internal static CaptureChoice Choice {get;private set;}=CaptureChoice.Photo;
    private sealed class Session : IDisposable
    {
        public readonly List<RegionPicker> Windows=[];
        public readonly IReadOnlyList<DisplayInfo> Displays;
        public readonly FrozenDesktop? Frozen;
        public bool CrosshairVisible=QuickCapturePreferences.Load().Crosshair;
        public bool KeyboardSelecting;public Native.POINT KeyboardPoint;
        public void SavePreferences()=>new QuickCapturePreferences(MagnifierVisible,CrosshairVisible).Save();
        public bool MagnifierVisible=QuickCapturePreferences.Load().Magnifier;
        private readonly Dictionary<nint,PixelRect> _frozenWindows=[];
        public void Pointer(Native.POINT p){foreach(var w in Windows){w.UpdateMagnifier(p);w._crosshair.Update(w.PointFromScreen(new(p.X,p.Y)),CrosshairVisible&&!w.OverToolbar(p));}}
        public readonly TaskCompletionSource<PixelRect?> Done=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly ElementSuggestion Suggestion=new();
        private readonly ElementProbeClient _probe=new();
        private ElementCandidate[] _hierarchy=[];private long _cacheUntil;private int _queryX=int.MinValue,_queryY=int.MinValue;private long _revision;private CancellationTokenSource? _pendingProbe;private int _requestX,_requestY;
        private readonly CancellationTokenSource _cancel=new();
        private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(40)};
        public PixelRect Current,Before;
        public CaptureHandle Handle;
        public Native.POINT Start;
        public bool Locked,Dragging,Smart;
        private CaptureGesture? _gesture;
        private bool _querying,_disposed;
        private int _x=int.MinValue,_y=int.MinValue;
        private long _stableSince,_lastQuery,_lastWindows;
        private List<WindowInfo> _windows=[];
        private nint _target,_lockedTarget;
        private PixelRect _targetBounds,_lockedBounds;
        private string _status="Kéo chọn vùng · Esc hủy";
        public string Status=>Locked?$"Đã chọn · {Current.Width} × {Current.Height} px · Enter chụp":Suggestion.Current is {} c?$"{c.Label.Split('·')[0].Trim()} · {c.Bounds.Width} × {c.Bounds.Height} · {Suggestion.Index+1}/{Suggestion.Candidates.Length}":_status;
        public Session(IReadOnlyList<DisplayInfo> displays,bool smart,FrozenDesktop? frozen){Displays=displays;Frozen=frozen;Smart=smart;_status=smart?"Rê chọn khối · Kéo chọn vùng":"Kéo chọn vùng · Esc hủy";if(frozen!=null){_windows=Native.Windows(true);foreach(var w in _windows)if(Native.GetWindowRect(w.Handle,out var bounds))_frozenWindows[w.Handle]=bounds.Pixels;}_timer.Tick+=async(_,_)=>await Poll();}
        public void StartPolling()=>_timer.Start();
        public void Draw(){foreach(var w in Windows)w.Draw();}
        public void Reset(){_pendingProbe?.Cancel();_revision++;_hierarchy=[];Locked=false;_lockedTarget=0;Current=default;Suggestion.Clear();_x=int.MinValue;Draw();}
        public void ToggleSmart(){Smart=!Smart;Reset();_status=Smart?"Rê chuột chọn khối · Kéo chọn vùng":"Kéo chọn vùng · Esc hủy";}
        public void Cycle(int wheel){if(Locked||Dragging||!Smart)return;Suggestion.Cycle(wheel);Current=Suggestion.Current?.Bounds??default;Draw();}
        private async Task Poll()
        {
            if(_disposed||!Native.GetCursorPos(out var p))return;
            if(KeyboardSelecting)p=KeyboardPoint;Pointer(p);if(Locked||Dragging||!Smart)return;
            if(Windows.Any(w=>w.OverToolbar(p)))return;
            long now=Environment.TickCount64;
            if(Math.Abs((long)p.X-_x)>3||Math.Abs((long)p.Y-_y)>3){_x=p.X;_y=p.Y;_stableSince=now;}
            if(_querying&&(Math.Abs((long)p.X-_requestX)>8||Math.Abs((long)p.Y-_requestY)>8))_pendingProbe?.Cancel();
            // Reuse the known hierarchy immediately while the pointer stays in its window.
            if(_target!=0&&ElementSelection.Contains(_targetBounds,p.X,p.Y)&&Suggestion.Candidates.Length>0){
                var cached=(_hierarchy.Length>0?_hierarchy:Suggestion.Candidates).Where(c=>ElementSelection.Contains(c.Bounds,p.X,p.Y)).ToArray();
                if(cached.Length>0&&Suggestion.Offer(cached,p.X,p.Y,16)){var rect=Suggestion.Current!.Bounds;if(rect!=Current){Current=rect;Draw();}}
            }
            if(!ElementSelection.Contains(Current,p.X,p.Y)&&!Current.IsEmpty){Suggestion.Clear();Current=default;Draw();}
            if(_querying||now-_stableSince<65||now-_lastQuery<120)return;
            if(now<_cacheUntil&&Math.Abs((long)p.X-_queryX)<4&&Math.Abs((long)p.Y-_queryY)<4)return;
            _lastQuery=now;
            _querying=true;
            try{
                if(Frozen==null&&now-_lastWindows>300){_windows=Native.Windows(true);_lastWindows=now;}
                var target=_windows.FirstOrDefault(w=>Frozen!=null?_frozenWindows.TryGetValue(w.Handle,out var stored)&&ElementSelection.Contains(stored,p.X,p.Y):Native.GetWindowRect(w.Handle,out var r)&&ElementSelection.Contains(r.Pixels,p.X,p.Y));
                if(target==null){Suggestion.Clear();Current=default;_status="Không có khối · Kéo chọn vùng";Draw();return;}
                if(!Native.GetWindowRect(target.Handle,out var bounds))return;
                if(Frozen!=null&&(!_frozenWindows.TryGetValue(target.Handle,out var storedBounds)||bounds.Pixels!=storedBounds)){_status="Khối đã thay đổi · Kéo chọn trên ảnh đã giữ";Suggestion.Clear();Current=default;Draw();return;}
                if(_target!=target.Handle||_targetBounds!=bounds.Pixels){Suggestion.Clear();_hierarchy=[];_revision++;_target=target.Handle;_targetBounds=bounds.Pixels;Current=bounds.Pixels;Suggestion.Offer([new(bounds.Pixels,"Cửa sổ",true)],p.X,p.Y,16);Draw();}
                var revision=_revision;_requestX=p.X;_requestY=p.Y;using var pending=CancellationTokenSource.CreateLinkedTokenSource(_cancel.Token);_pendingProbe=pending;var list=await _probe.QueryAsync(new((long)target.Handle,p.X,p.Y,target.ProcessId==Environment.ProcessId?-1:Environment.ProcessId),pending.Token);
                if(_disposed||revision!=_revision||Locked||Dragging||!Smart||!Native.GetCursorPos(out var fresh)||Math.Abs(fresh.X-p.X)>8||Math.Abs(fresh.Y-p.Y)>8)return;
                if(!Native.GetWindowRect(target.Handle,out var check)||check.Pixels!=bounds.Pixels){Suggestion.Clear();Current=default;_x=int.MinValue;Draw();return;}
                var desktop=new PixelRect(Displays.Min(d=>d.Bounds.X),Displays.Min(d=>d.Bounds.Y),Displays.Max(d=>d.Bounds.Right)-Displays.Min(d=>d.Bounds.X),Displays.Max(d=>d.Bounds.Bottom)-Displays.Min(d=>d.Bounds.Y));
                var normalized=ElementSelection.Normalize(list,p.X,p.Y,bounds.Pixels.Intersect(desktop));
                _hierarchy=normalized;_queryX=p.X;_queryY=p.Y;_cacheUntil=Environment.TickCount64+700;
                int tolerance=(int)Math.Round(16*(Displays.FirstOrDefault(d=>ElementSelection.Contains(d.Bounds,p.X,p.Y))?.DpiX??96)/96d);
                if(Suggestion.Offer(normalized,p.X,p.Y,tolerance)){Current=Suggestion.Current?.Bounds??default;Draw();}
                else if(!ElementSelection.Contains(Current,p.X,p.Y)){Current=default;Draw();}
            }catch(OperationCanceledException){}catch(Exception ex){if(!_disposed){App.Log(ex);_status="Kéo chọn vùng thủ công";}}
            finally{_pendingProbe=null;_querying=false;}
        }
        public void Begin(Native.POINT p,int tolerance)
        {
            _pendingProbe?.Cancel();_revision++;Dragging=true;Start=p;Before=Current;bool suggestedClick=false;
            // Click a suggestion to lock. A drag before locking always starts a manual region.
            if(!Locked){bool inside=ElementSelection.Contains(Current,p.X,p.Y)&&Suggestion.Current!=null;Handle=inside?CaptureHandle.Move:CaptureHandle.New;suggestedClick=inside;_lockedTarget=inside?_target:0;_lockedBounds=_targetBounds;Locked=true;}
            else Handle=CaptureSelection.Hit(Current,p.X,p.Y,tolerance);
            _gesture=new(Before,Handle,p.X,p.Y,suggestedClick);
            if(Handle==CaptureHandle.New){_lockedTarget=0;Current=default;}
            Draw();
        }
        public void Move(Native.POINT p,bool shift){
            if(!Dragging||_gesture==null)return;
            Current=_gesture.Apply(p.X,p.Y,shift);if(_gesture.Handle==CaptureHandle.New)_lockedTarget=0;Draw();
        }
        public void End(){Dragging=false;Draw();}
        public void Finish(CaptureChoice choice=CaptureChoice.Photo)
        {
            if(!Locked||Dragging||!CaptureSelection.Valid(Current))return;
            if(Frozen==null&&_lockedTarget!=0&&(!Native.GetWindowRect(_lockedTarget,out var fresh)||fresh.Pixels!=_lockedBounds)){
                Reset();_status="Cửa sổ đã di chuyển · Chọn lại";Draw();return;
            }
            if(Frozen==null&&!Locked&&Smart&&(_target==0||!Native.GetWindowRect(_target,out var live)||live.Pixels!=_targetBounds)){Reset();return;}
            Choice=choice;Close(Current);
        }
        public void Close(PixelRect? result){if(_disposed)return;Dispose();Done.TrySetResult(result);}
        public void Dispose(){if(_disposed)return;_disposed=true;_timer.Stop();_cancel.Cancel();_probe.Dispose();foreach(var w in Windows){w._selection.BeginAnimation(Shape.StrokeDashOffsetProperty,null);w.Background=null;w.Content=null;w._canvas.Children.Clear();w.Close();}Windows.Clear();_cancel.Dispose();}
    }
    private readonly Session _session;
    private readonly DisplayInfo _display;
    private readonly Canvas _canvas=new();
    private readonly CaptureShade _shade=new();
    private readonly CaptureCrosshair _crosshair=new();
    private readonly Rectangle _contrast=new(){Stroke=Brushes.Black,StrokeThickness=2,Fill=Brushes.Transparent,IsHitTestVisible=false,Visibility=Visibility.Collapsed};
    private readonly Rectangle _selection=new(){Stroke=Brushes.White,StrokeThickness=1,Fill=Brushes.Transparent,Visibility=Visibility.Collapsed,IsHitTestVisible=false};
    private readonly Border _toolbar;
    private readonly TextBlock _label=new(){Foreground=Brushes.White,FontSize=10,MaxWidth=125,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center,Margin=new(4,0,7,0)};
    private readonly Button _shoot,_smart;
    private readonly List<Ellipse> _handles=[];
    private readonly CaptureMagnifier? _magnifier;
    private PixelRect _toolbarBounds;
    private bool _placing;
    public static async Task<PixelRect?> PickAsync(IReadOnlyList<DisplayInfo> displays,CancellationToken cancellation=default,bool allowScroll=false,bool smart=false,FrozenDesktop? frozen=null)
    {
        cancellation.ThrowIfCancellationRequested();if(displays.Count==0)throw new InvalidOperationException("Không tìm thấy màn hình.");
        Choice=CaptureChoice.Photo;using var session=new Session(displays,smart,frozen);
        foreach(var d in displays)session.Windows.Add(new(d,session,allowScroll,smart));
        foreach(var w in session.Windows)w.Show();
        session.Draw();session.StartPolling();
        using var registration=cancellation.Register(()=>Application.Current.Dispatcher.BeginInvoke(()=>session.Close(null)));
        return await session.Done.Task;
    }
    private RegionPicker(DisplayInfo display,Session session,bool allowScroll,bool smartAvailable)
    {
        _display=display;_session=session;_contrast.StrokeThickness=2/(display.DpiX/96d);_selection.StrokeThickness=1/(display.DpiX/96d);
        WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;Background=Brushes.Transparent;Topmost=true;ShowInTaskbar=false;Cursor=Cursors.Cross;
        if(session.Frozen!=null){Background=session.Frozen.Background(display.Bounds);RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.NearestNeighbor);_magnifier=new(session.Frozen){Visibility=Visibility.Collapsed};_canvas.Children.Add(_magnifier);Panel.SetZIndex(_magnifier,20);}
        Content=_canvas;_canvas.Children.Add(_shade);_canvas.Children.Add(_contrast);_canvas.Children.Add(_selection);_canvas.Children.Add(_crosshair);
        _selection.StrokeDashArray=new DoubleCollection{4,3};_selection.BeginAnimation(Shape.StrokeDashOffsetProperty,new DoubleAnimation(0,-14,TimeSpan.FromSeconds(2.4)){RepeatBehavior=RepeatBehavior.Forever});
        for(int i=0;i<8;i++){var handle=new Ellipse{Width=7,Height=7,Fill=Brushes.White,Stroke=Brushes.MediumPurple,StrokeThickness=1,IsHitTestVisible=false,Visibility=Visibility.Collapsed};_handles.Add(handle);_canvas.Children.Add(handle);}
        var actions=new WrapPanel{Orientation=Orientation.Horizontal};
        _toolbar=new Border{Background=new SolidColorBrush(Color.FromRgb(29,32,40)),BorderBrush=new SolidColorBrush(Color.FromRgb(87,78,110)),BorderThickness=new(1),CornerRadius=new(6),Padding=new(2),Child=actions,ToolTip="Kéo vùng / cạnh để chỉnh · Enter chụp · Esc hủy · M kính lúp · H tọa độ · Space đặt góc rồi phím mũi tên",ClipToBounds=true};Panel.SetZIndex(_toolbar,10);_canvas.Children.Add(_toolbar);
        Button Add(string icon,string label,string help,Action action){var button=new Button{Content=AppIcons.Content(icon,label,14),ToolTip=help,Foreground=Brushes.White,Background=Brushes.Transparent,Padding=new(4,2,4,2),Margin=new(1),MinWidth=24,MinHeight=24,BorderThickness=new(0),Template=OverlayButtonTemplate(),Focusable=false};button.Click+=(_,e)=>{e.Handled=true;action();};actions.Children.Add(button);return button;}
        actions.Children.Add(_label);
        if(session.Frozen!=null)Add("Magnifier","","Kính lúp pixel · M bật/tắt · M ẩn/hiện",()=>{session.MagnifierVisible=!session.MagnifierVisible;session.SavePreferences();Native.GetCursorPos(out var pointer);session.Pointer(pointer);});
        Add("Crosshair","","Đường tọa độ X/Y · H bật/tắt",()=>{session.CrosshairVisible=!session.CrosshairVisible;session.SavePreferences();Native.GetCursorPos(out var pointer);session.Pointer(pointer);});
        _smart=Add("Element","","Nhận diện phần tử · S bật/tắt\nRê để gợi ý · ↑/↓: cha/con\nClick khóa · Kéo cạnh chỉnh · Tab: chọn lại",()=>{session.ToggleSmart();session.Draw();});_smart.Visibility=smartAvailable?Visibility.Visible:Visibility.Collapsed;
        if(allowScroll){Add("ScrollDown","","Chụp cuộn dọc",()=>session.Finish(CaptureChoice.Vertical));Add("ScrollRight","","Chụp cuộn ngang",()=>session.Finish(CaptureChoice.Horizontal));}
        Add("Close","","Hủy · Esc",()=>session.Close(null));
        _shoot=Add("Camera","Enter","Chụp ảnh · Enter",()=>session.Finish());_shoot.Background=new SolidColorBrush(Color.FromRgb(111,55,220));
        _toolbar.SizeChanged+=(_,_)=>Draw();
        SizeChanged+=(_,_)=>{_shade.Width=_crosshair.Width=ActualWidth;_shade.Height=_crosshair.Height=ActualHeight;Draw();};
        void Position(){var h=new WindowInteropHelper(this).Handle;Native.SetWindowPos(h,-1,display.Bounds.X,display.Bounds.Y,display.Bounds.Width,display.Bounds.Height,0x40);Native.SetWindowDisplayAffinity(h,0x11);}
        SourceInitialized+=(_,_)=>Position();Loaded+=(_,_)=>{Position();Focus();};
        PreviewKeyDown+=KeyDownOverlay;
        MouseLeftButtonDown+=(_,e)=>{if(IsToolbarSource(e.OriginalSource as DependencyObject))return;if(session.KeyboardSelecting){session.KeyboardSelecting=false;session.End();session.Reset();}Native.GetCursorPos(out var p);session.Begin(p,Tolerance);CaptureMouse();e.Handled=true;};
        MouseMove+=(_,_)=>{Native.GetCursorPos(out var p);session.Pointer(p);if(session.Dragging){session.Move(p,(Keyboard.Modifiers&ModifierKeys.Shift)!=0);return;}Cursor=CursorAt(p);};
        MouseLeftButtonUp+=(_,e)=>{if(!session.Dragging)return;Native.GetCursorPos(out var p);session.Move(p,(Keyboard.Modifiers&ModifierKeys.Shift)!=0);ReleaseMouseCapture();session.End();e.Handled=true;};
        MouseWheel+=(_,e)=>{e.Handled=true;};MouseRightButtonDown+=(_,_)=>session.Close(null);
    }
    private void UpdateMagnifier(Native.POINT p)
    {
        if(_magnifier==null)return;
        bool show=_session.MagnifierVisible&&ElementSelection.Contains(_display.Bounds,p.X,p.Y)&&!OverToolbar(p);
        _magnifier.Visibility=show?Visibility.Visible:Visibility.Collapsed;if(!show)return;
        var point=PointFromScreen(new(p.X,p.Y));double left=point.X+24,top=point.Y+24;
        if(left+_magnifier.Width>ActualWidth-4)left=point.X-_magnifier.Width-24;
        if(top+_magnifier.Height>ActualHeight-4)top=point.Y-_magnifier.Height-24;
        Canvas.SetLeft(_magnifier,Math.Max(4,left));Canvas.SetTop(_magnifier,Math.Max(4,top));_magnifier.Update(p.X,p.Y);
    }
    private static ControlTemplate OverlayButtonTemplate()
    {
        var border=new FrameworkElementFactory(typeof(Border));border.Name="Chrome";border.SetValue(Border.CornerRadiusProperty,new CornerRadius(4));
        border.SetBinding(Border.BackgroundProperty,new System.Windows.Data.Binding("Background"){RelativeSource=System.Windows.Data.RelativeSource.TemplatedParent});
        border.SetBinding(Border.PaddingProperty,new System.Windows.Data.Binding("Padding"){RelativeSource=System.Windows.Data.RelativeSource.TemplatedParent});
        var content=new FrameworkElementFactory(typeof(ContentPresenter));content.SetValue(ContentPresenter.HorizontalAlignmentProperty,HorizontalAlignment.Center);content.SetValue(ContentPresenter.VerticalAlignmentProperty,VerticalAlignment.Center);border.AppendChild(content);
        var template=new ControlTemplate(typeof(Button)){VisualTree=border};
        var hover=new Trigger{Property=IsMouseOverProperty,Value=true};hover.Setters.Add(new Setter(Border.BackgroundProperty,new SolidColorBrush(Color.FromRgb(72,60,98)),"Chrome"));template.Triggers.Add(hover);
        var disabled=new Trigger{Property=IsEnabledProperty,Value=false};disabled.Setters.Add(new Setter(OpacityProperty,.4));template.Triggers.Add(disabled);return template;
    }
    private int Tolerance=>(int)Math.Round(8*_display.DpiX/96d);
    private bool IsToolbarSource(DependencyObject? item){while(item!=null){if(item==_toolbar)return true;item=VisualTreeHelper.GetParent(item);}return false;}
    private Cursor CursorAt(Native.POINT p)=>!_session.Locked?Cursors.Cross:CaptureSelection.Hit(_session.Current,p.X,p.Y,Tolerance) switch{CaptureHandle.Left or CaptureHandle.Right=>Cursors.SizeWE,CaptureHandle.Top or CaptureHandle.Bottom=>Cursors.SizeNS,CaptureHandle.TopLeft or CaptureHandle.BottomRight=>Cursors.SizeNWSE,CaptureHandle.TopRight or CaptureHandle.BottomLeft=>Cursors.SizeNESW,CaptureHandle.Move=>Cursors.SizeAll,_=>Cursors.Cross};
    private void KeyDownOverlay(object? sender,KeyEventArgs e)
    {
        var s=_session;if(e.Key==Key.M&&_magnifier!=null){s.MagnifierVisible=!s.MagnifierVisible;s.SavePreferences();Native.GetCursorPos(out var point);s.Pointer(point);}else if(e.Key==Key.Escape)s.Close(null);else if(e.Key==Key.H){s.CrosshairVisible=!s.CrosshairVisible;s.SavePreferences();Native.GetCursorPos(out var point);s.Pointer(point);}
        else if(e.Key==Key.Space&&!s.Locked){Native.GetCursorPos(out var point);s.Begin(point,Tolerance);s.KeyboardPoint=point;s.KeyboardSelecting=true;}
        else if(e.Key==Key.Enter){if(s.KeyboardSelecting){s.KeyboardSelecting=false;s.End();}s.Finish();}else if(e.Key==Key.Tab){s.Reset();}else if(e.Key==Key.S&&_smart.Visibility==Visibility.Visible){s.ToggleSmart();s.Draw();}
        else if(e.Key is Key.Up or Key.Down or Key.Left or Key.Right){
            if(s.KeyboardSelecting){int n=Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?10:1;var point=s.KeyboardPoint;point.X+=e.Key==Key.Left?-n:e.Key==Key.Right?n:0;point.Y+=e.Key==Key.Up?-n:e.Key==Key.Down?n:0;var desktop=new PixelRect(s.Displays.Min(d=>d.Bounds.X),s.Displays.Min(d=>d.Bounds.Y),s.Displays.Max(d=>d.Bounds.Right)-s.Displays.Min(d=>d.Bounds.X),s.Displays.Max(d=>d.Bounds.Bottom)-s.Displays.Min(d=>d.Bounds.Y));point.X=Math.Clamp(point.X,desktop.X,desktop.Right);point.Y=Math.Clamp(point.Y,desktop.Y,desktop.Bottom);s.KeyboardPoint=point;s.Move(point,false);s.Pointer(point);}
            else if(!s.Locked&&!s.Smart){Native.GetCursorPos(out var point);var desktop=new PixelRect(s.Displays.Min(d=>d.Bounds.X),s.Displays.Min(d=>d.Bounds.Y),s.Displays.Max(d=>d.Bounds.Right)-s.Displays.Min(d=>d.Bounds.X),s.Displays.Max(d=>d.Bounds.Bottom)-s.Displays.Min(d=>d.Bounds.Y));s.Current=QuickCaptureLayout.KeyboardRegion(desktop,point.X,point.Y);s.Locked=true;s.Draw();}
            else if(!s.Locked)s.Cycle(e.Key is Key.Up or Key.Left?120:-120);else{int step=(Keyboard.Modifiers&ModifierKeys.Shift)!=0?10:1;s.Current=CaptureSelection.Nudge(s.Current,e.Key==Key.Left?-step:e.Key==Key.Right?step:0,e.Key==Key.Up?-step:e.Key==Key.Down?step:0,(Keyboard.Modifiers&ModifierKeys.Control)!=0);s.Draw();}}else return;e.Handled=true;
    }
    private bool OverToolbar(Native.POINT p)=>_toolbar.Visibility==Visibility.Visible&&ElementSelection.Contains(_toolbarBounds,p.X,p.Y);
    private void Draw()
    {
        if(_placing||!IsLoaded)return;_placing=true;
        try{
            var s=_session;var r=s.Current.Intersect(_display.Bounds);var valid=CaptureSelection.Valid(s.Current);
            _contrast.Visibility=_selection.Visibility=r.IsEmpty?Visibility.Collapsed:Visibility.Visible;
            foreach(var h in _handles)h.Visibility=Visibility.Collapsed;
            if(r.IsEmpty)_shade.Reveal(null);else{
                var a=PointFromScreen(new(r.X,r.Y));var b=PointFromScreen(new(r.Right,r.Bottom));_shade.Reveal(new Rect(a,b));Canvas.SetLeft(_selection,a.X);Canvas.SetTop(_selection,a.Y);_selection.Width=Math.Max(0,b.X-a.X);_selection.Height=Math.Max(0,b.Y-a.Y);Canvas.SetLeft(_contrast,a.X);Canvas.SetTop(_contrast,a.Y);_contrast.Width=_selection.Width;_contrast.Height=_selection.Height;
                if(s.Locked&&valid){var c=s.Current;int[] xs=[c.X,c.X+c.Width/2,c.Right,c.Right,c.Right,c.X+c.Width/2,c.X,c.X];int[] ys=[c.Y,c.Y,c.Y,c.Y+c.Height/2,c.Bottom,c.Bottom,c.Bottom,c.Y+c.Height/2];for(int i=0;i<8;i++)if(ElementSelection.Contains(_display.Bounds,xs[i],ys[i])){var p=PointFromScreen(new(xs[i],ys[i]));Canvas.SetLeft(_handles[i],p.X-3.5);Canvas.SetTop(_handles[i],p.Y-3.5);_handles[i].Visibility=Visibility.Visible;}}
            }
            Native.GetCursorPos(out var cursor);var owner=valid?s.Displays.FirstOrDefault(d=>ElementSelection.Contains(d.Bounds,s.Current.X+s.Current.Width/2,s.Current.Y+s.Current.Height/2)):s.Displays.FirstOrDefault(d=>ElementSelection.Contains(d.Bounds,cursor.X,cursor.Y));owner??=s.Displays[0];
            _toolbar.Visibility=s.Dragging||owner!=_display?Visibility.Collapsed:Visibility.Visible;if(_toolbar.Visibility!=Visibility.Visible)return;
            _label.Text=s.Status;_shoot.IsEnabled=valid&&s.Locked;_smart.Opacity=s.Smart?1:.45;
            _toolbar.MaxWidth=Math.Max(80,ActualWidth-8);_label.MaxWidth=Math.Max(60,Math.Min(125,ActualWidth-230));_toolbar.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));var size=_toolbar.DesiredSize;double scale=VisualTreeHelper.GetDpi(this).DpiScaleX;
            var screen=_display.Bounds;var anchor=valid?s.Current:new PixelRect(screen.X+screen.Width/2,screen.Bottom,1,1);
            _toolbarBounds=QuickCaptureLayout.Toolbar(screen,anchor,(int)Math.Ceiling(size.Width*scale),(int)Math.Ceiling(size.Height*scale),cursor.X,cursor.Y);var point=PointFromScreen(new(_toolbarBounds.X,_toolbarBounds.Y));Canvas.SetLeft(_toolbar,point.X);Canvas.SetTop(_toolbar,point.Y);
        }finally{_placing=false;}
    }
}
