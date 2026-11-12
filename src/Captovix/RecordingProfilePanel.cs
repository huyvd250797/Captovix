using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Captovix;

internal sealed class RecordingProfilePanel : StackPanel
{
    private readonly ComboBox _preset=new(){ItemsSource=new[]{"Nhẹ","Bình thường","Chất lượng cao","Tùy chỉnh"}};
    private readonly ComboBox _resolution=new(){ItemsSource=new[]{"720p","1080p","1440p","2160p","Theo nguồn"}};
    private readonly ComboBox _fps=new(){ItemsSource=new[]{"15 FPS","24 FPS","30 FPS","60 FPS"}};
    private readonly TextBlock _summary=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,4,4,8)};
    private static readonly int[] Resolutions=[720,1080,1440,2160,0], FrameRates=[15,24,30,60];
    private bool _updating;

    internal RecordingProfilePanel(RecordingProfile saved)
    {
        Add("Preset quay",_preset);
        var row=new UniformGrid{Columns=2};
        row.Children.Add(Field("Giới hạn độ phân giải",_resolution));
        row.Children.Add(Field("Tốc độ khung hình",_fps));
        Children.Add(row);
        Children.Add(_summary);
        _preset.SelectedIndex=Math.Clamp(saved.Preset,0,3);
        _resolution.SelectedIndex=IndexOf(Resolutions,saved.Resolution,1);
        _fps.SelectedIndex=IndexOf(FrameRates,saved.Fps,2);
        _preset.SelectionChanged+=(_,_)=>{
            if(_updating)return;
            if(_preset.SelectedIndex is >=0 and <=2){
                var profile=RecordingProfile.ForPreset(_preset.SelectedIndex);
                _updating=true;
                _resolution.SelectedIndex=Array.IndexOf(Resolutions,profile.Resolution);
                _fps.SelectedIndex=Array.IndexOf(FrameRates,profile.Fps);
                _updating=false;
            }
            Changed?.Invoke();
        };
        _resolution.SelectionChanged+=(_,_)=>Customize();
        _fps.SelectionChanged+=(_,_)=>Customize();
    }

    internal event Action? Changed;
    private void Customize()
    {
        if(_updating)return;
        _updating=true;
        _preset.SelectedIndex=3;
        _updating=false;
        Changed?.Invoke();
    }

    internal RecordingProfile Read()=>new(_preset.SelectedIndex,Resolutions[_resolution.SelectedIndex],FrameRates[_fps.SelectedIndex]);

    internal void Update(PixelRect reference,bool exact,bool mic,bool system)
    {
        var profile=Read();
        var frame=profile.Frame(reference);
        double size=profile.BytesPerMinute(profile.Bitrate(frame),mic,system)/1_048_576d;
        _summary.Text=$"{(exact?"Đầu ra":"Ước tính theo màn hình tham chiếu")} · {frame.Width} × {frame.Height} px · {profile.Fps} FPS\nKhoảng {size:0.0} MB/phút, gồm các tệp âm thanh riêng.";
    }

    private static int IndexOf(int[] values,int value,int fallback)
    {
        int index=Array.IndexOf(values,value);
        return index>=0?index:fallback;
    }
    private void Add(string label,ComboBox field)=>Children.Add(Field(label,field));
    private static StackPanel Field(string label,ComboBox field)
    {
        var panel=new StackPanel();
        panel.Children.Add(new TextBlock{Text=label,Margin=new(4,5,4,0)});
        System.Windows.Automation.AutomationProperties.SetName(field,label);
        panel.Children.Add(field);
        return panel;
    }
}
