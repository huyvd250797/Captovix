using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Captovix;

internal sealed class RecordingAudioPanel : StackPanel, IDisposable
{
    private readonly SourceControl _microphone,_system;
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(80)};

    internal RecordingAudioPanel(AppSettings preferences)
    {
        _microphone=new("Micro",false,preferences.Mic,preferences.MicDevice);
        _system=new("Âm thanh máy",true,preferences.SystemAudio,preferences.SystemAudioDevice);
        Children.Add(_microphone.Panel);
        Children.Add(_system.Panel);
        _microphone.Changed+=()=>SourcesChanged?.Invoke();
        _system.Changed+=()=>SourcesChanged?.Invoke();
        var refresh=new Button{Content="Làm mới thiết bị",HorizontalAlignment=HorizontalAlignment.Left};
        refresh.Click+=(_,_)=>{_microphone.Refresh();_system.Refresh();};
        Children.Add(refresh);
        _timer.Tick+=(_,_)=>{_microphone.UpdateLevel();_system.UpdateLevel();};
        _timer.Start();
    }

    internal event Action? SourcesChanged;
    internal bool MicrophoneEnabled=>_microphone.Enabled;
    internal bool SystemEnabled=>_system.Enabled;

    internal RecordingAudioSettings Read()=>new(_microphone.Read(),_system.Read());

    public void Dispose()
    {
        _timer.Stop();
        _microphone.Dispose();
        _system.Dispose();
    }

    private sealed class SourceControl : IDisposable
    {
        internal event Action? Changed;
        internal bool Enabled=>_enabled.IsChecked==true;
        internal StackPanel Panel {get;}=new(){Margin=new Thickness(0,8,0,3)};
        private readonly CheckBox _enabled;
        private readonly ComboBox _devices=new(){DisplayMemberPath=nameof(AudioEndpoint.Name)};
        private readonly ProgressBar _meter=new(){Height=7,Maximum=1,Margin=new Thickness(4,3,4,3),Focusable=false};
        private readonly TextBlock _status=new(){Margin=new Thickness(4,0,4,4),TextWrapping=TextWrapping.Wrap};
        private readonly bool _system;
        private readonly string _name;
        private string? _preferred;
        private bool _refreshing;
        private AudioInput? _input;

        internal SourceControl(string name,bool system,bool enabled,string? preferred)
        {
            _system=system;
            _name=name;
            _preferred=preferred;
            _enabled=new CheckBox{Content=name,IsChecked=enabled,FontWeight=FontWeights.SemiBold};
            System.Windows.Automation.AutomationProperties.SetName(_devices,$"Thiết bị · {name}");
            System.Windows.Automation.AutomationProperties.SetName(_meter,$"Mức âm · {name}");
            Panel.Children.Add(_enabled);
            Panel.Children.Add(_devices);
            Panel.Children.Add(_meter);
            Panel.Children.Add(_status);
            _enabled.Checked+=(_,_)=>{OpenPreview();Changed?.Invoke();};
            _enabled.Unchecked+=(_,_)=>{OpenPreview();Changed?.Invoke();};
            _devices.SelectionChanged+=(_,_)=>OpenPreview();
            Refresh();
        }

        internal void Refresh()
        {
            var selected=(_devices.SelectedItem as AudioEndpoint)?.Id??_preferred;
            _refreshing=true;
            try{
                var (devices,defaultId)=RecordingAudio.FindEndpoints(_system);
                _devices.ItemsSource=devices;
                _devices.SelectedItem=devices.FirstOrDefault(d=>d.Id==(selected??defaultId));
                if(_devices.SelectedItem==null&&selected==null)_devices.SelectedIndex=devices.Length==0?-1:0;
            }catch(Exception error){
                App.Log(error);
                _devices.ItemsSource=null;
            }finally{_refreshing=false;}
            OpenPreview();
        }

        private void OpenPreview()
        {
            if(_refreshing)return;
            Dispose();
            _meter.Value=0;
            _devices.IsEnabled=_enabled.IsChecked==true;
            if(!_devices.IsEnabled){_status.Text="Đã tắt";return;}
            if(_devices.SelectedItem is not AudioEndpoint endpoint){
                _status.Text="Chọn thiết bị đang kết nối hoặc tắt nguồn này.";
                return;
            }
            _preferred=endpoint.Id;
            try{
                _input=new AudioInput(endpoint,_system);
                _status.Text=_system?"Phát thử âm thanh trên thiết bị đã chọn.":"Nói thử để kiểm tra micro.";
            }catch(Exception error){_status.Text=error.Message;}
        }

        internal void UpdateLevel()
        {
            _meter.Value=Math.Sqrt(_input?.Level??0);
            if(_input?.Warning is {} warning)_status.Text=warning;
        }

        internal AudioEndpoint? Read()
        {
            if(_enabled.IsChecked!=true)return null;
            if(_devices.SelectedItem is not AudioEndpoint endpoint||_input==null||_input.Warning!=null)
                throw new InvalidOperationException($"Kiểm tra nguồn {_name}, chọn lại thiết bị hoặc tắt nguồn trước khi tiếp tục.");
            return endpoint;
        }

        public void Dispose(){_input?.Dispose();_input=null;}
    }
}
