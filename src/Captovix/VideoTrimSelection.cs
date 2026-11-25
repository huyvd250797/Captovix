namespace Captovix;

internal readonly record struct VideoRange(TimeSpan Start,TimeSpan End)
{
    public int Number {get;init;}
    public VideoClipLayout? Layout {get;init;}
    public int SourceIndex {get;init;}
    public VideoMark[]? Marks {get;init;}
    public VideoMotion? Motion {get;init;}
    internal VideoMotion Timing=>Motion??VideoMotion.Default;
    internal TimeSpan SourceDuration=>End-Start;
    internal TimeSpan MovingDuration=>Timing.Playback(SourceDuration);
    internal TimeSpan Duration=>MovingDuration+Timing.Hold;
    internal TimeSpan SourceAt(TimeSpan local)=>local>=MovingDuration?End:Start+Timing.Source(TimeSpan.FromTicks(Math.Max(0,local.Ticks)));
    internal double Fraction(TimeSpan local)=>Math.Clamp(local.TotalSeconds/MovingDuration.TotalSeconds,0,1);
    internal TimeSpan LocalAt(TimeSpan source)=>Timing.Playback(TimeSpan.FromTicks(Math.Clamp((source-Start).Ticks,0,SourceDuration.Ticks)));


    internal bool TrySplit(TimeSpan position,TimeSpan minimum,out VideoRange left,out VideoRange right)
    {
        left=this with{End=position};
        right=this with{Start=position};
        if(left.SourceDuration<=TimeSpan.Zero||right.SourceDuration<=TimeSpan.Zero)return false;
        if(Motion!=null){
            double fraction=left.SourceDuration.TotalSeconds/SourceDuration.TotalSeconds;
            left=left with{Motion=Timing.Slice(0,fraction,false,true,false)};
            right=right with{Motion=Timing.Slice(fraction,1,true,false,true)};
        }
        return left.MovingDuration>=minimum&&right.MovingDuration>=minimum;
    }
}

internal enum VideoMarkKind { Frame, Callout, Arrow, Text, Step, Blur, Highlight, Spotlight, Magnifier, Freehand, Title }

internal sealed record VideoInkPoint(double X,double Y)
{
    internal bool IsValid()=>X is >=0 and <=1&&Y is >=0 and <=1;
}

internal sealed record VideoInkStyle
{
    internal static readonly VideoInkStyle Default=new();
    public double Opacity {get;init;}=100;
    public double StrokeOpacity {get;init;}=100;
    public double FillOpacity {get;init;}=100;
    public bool StrokeEnabled {get;init;}=true;
    public bool? FillEnabled {get;init;}
    public string Fill {get;init;}="#343A48";
    public string TextColor {get;init;}="#FFFFFF";
    public string Font {get;init;}="Segoe UI";
    public bool Bold {get;init;}
    public bool Italic {get;init;}
    public bool Underline {get;init;}
    public int Shape {get;init;}
    public int Dash {get;init;}
    public int ArrowHead {get;init;}
    public bool DoubleArrow {get;init;}
    public int Callout {get;init;}
    public int Step {get;init;}
    public int Effect {get;init;}
    public int Strength {get;init;}=24;
    public double Radius {get;init;}=16;
    public double Angle {get;init;}
    public double Zoom {get;init;}=2;
    public bool Ellipse {get;init;}
    public int Alignment {get;init;}
    public double Padding {get;init;}=6;
    public double Focus {get;init;}=45;
    public int Vertical {get;init;}
    public double LineSpacing {get;init;}
    public double ArrowSize {get;init;}
    public int StarPoints {get;init;}=5;
    public double StarInset {get;init;}=.45;
    public int Route {get;init;}
    public bool ShadowEnabled {get;init;}
    public int ShadowDirection {get;init;}=7;
    public double ShadowDistance {get;init;}=8;
    public double ShadowBlur {get;init;}=8;
    public double ShadowOpacity {get;init;}=25;
    public string ShadowColor {get;init;}="#000000";
    public string? StepStrokeColor {get;init;}
    public bool? StepFillVisible {get;init;}

    internal bool IsValid()=>Percent(Opacity)&&Percent(StrokeOpacity)&&Percent(FillOpacity)&&Percent(Focus)
        &&VideoCanvasLayout.IsColor(Fill)&&VideoCanvasLayout.IsColor(TextColor)&&Font is {Length:>0 and <=100}
        &&Shape is >=0 and <=9&&Dash is >=0 and <=3&&ArrowHead is >=0 and <=4&&Callout is >=0 and <=9
        &&Step is >=0 and <=2&&Effect is >=0 and <=2&&Strength is >=4 and <=64&&Zoom is >=1.25 and <=8
        &&Vertical is >=0 and <=2&&LineSpacing is >=0 and <=3&&(LineSpacing==0||LineSpacing>=.8)
        &&ArrowSize is >=0 and <=160&&StarPoints is >=3 and <=20&&StarInset is >=.1 and <=.9&&Route is >=0 and <=2
        &&ShadowDirection is >=0 and <=8&&ShadowDistance is >=0 and <=80&&ShadowBlur is >=0 and <=40&&Percent(ShadowOpacity)
        &&VideoCanvasLayout.IsColor(ShadowColor)&&(StepStrokeColor==null||VideoCanvasLayout.IsColor(StepStrokeColor))
        &&Radius is >=0 and <=1000&&Angle is >=-360 and <=360&&Alignment is >=0 and <=2&&Padding is >=0 and <=200;
    private static bool Percent(double value)=>value is >=0 and <=100;
}

internal sealed record VideoMark(VideoMarkKind Kind,double X1,double Y1,double X2,double Y2,string Text,int Number)
{
    public string? Color {get;init;}
    public double FontSize {get;init;}=20;
    public double Stroke {get;init;}=3;
    public VideoInkStyle Style {get;init;}=VideoInkStyle.Default;
    public VideoInkPoint[]? Points {get;init;}
    internal bool SamplesVideo=>Kind is VideoMarkKind.Blur or VideoMarkKind.Magnifier;
    public VideoInkPoint? Tail {get;init;}
    public bool TextFrame {get;init;}
    internal bool HasText=>Kind is VideoMarkKind.Text or VideoMarkKind.Title or VideoMarkKind.Callout or VideoMarkKind.Step;
    public string? StepLabel {get;init;}
    internal string Label=>Kind switch{
        VideoMarkKind.Frame=>"Khung",VideoMarkKind.Callout=>"Callout",VideoMarkKind.Arrow=>"Mũi tên",
        VideoMarkKind.Text=>"Chữ",VideoMarkKind.Title=>"Tiêu đề",VideoMarkKind.Step=>"Bước",VideoMarkKind.Highlight=>"Highlight",
        VideoMarkKind.Spotlight=>"Spotlight",VideoMarkKind.Magnifier=>"Phóng đại",VideoMarkKind.Freehand=>"Vẽ tự do",_=>"Làm mờ"
    };
    internal bool IsValid()=>Enum.IsDefined(Kind)&&Style!=null&&Style.IsValid()&&(Points==null||Points.Length<=2048
        &&Points.All(point=>point!=null&&point.IsValid()))&&FontSize is >=8 and <=240&&Stroke is >=1 and <=40
        &&(Color==null||VideoCanvasLayout.IsColor(Color))&&(StepLabel==null||StepLabel.Length<=12)&&Text is {Length:<=500}&&Number is >=0 and <=9999
        &&Valid(X1)&&Valid(Y1)&&Valid(X2)&&Valid(Y2)&&(Tail==null||Tail.IsValid());

    private static bool Valid(double value)=>value is >=0 and <=1;
}


internal sealed record VideoMarkTrack(Guid Id,TimeSpan Start,TimeSpan End,VideoMark Mark)
{
    internal TimeSpan Length=>End-Start;
    internal bool IsActive(TimeSpan time)=>Start<=time&&time<End;
    internal VideoMarkTrack? Slice(TimeSpan start,TimeSpan end)
    {
        var left=Start>start?Start:start;
        var right=End<end?End:end;
        return right>left?this with{Start=left-start,End=right-start}:null;
    }
}
