using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Captovix;

public sealed class PercentField : StackPanel
{
    private readonly TextBlock _caption=new();
    private readonly Slider _slider=new(){Minimum=0,Maximum=100,TickFrequency=1,SmallChange=1,LargeChange=10,VerticalAlignment=VerticalAlignment.Center,Margin=new(3,0,5,0)};
    private readonly TextBox _input;
    private bool _sync;
    public PercentField():this(new TextBox()){}
    internal PercentField(TextBox input)
    {
        _input=input;
        Children.Add(_caption);
        var grid=new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new(64)});
        grid.Children.Add(_slider);
        Grid.SetColumn(input,1);
        input.MinWidth=0;
        input.HorizontalContentAlignment=HorizontalAlignment.Right;
        grid.Children.Add(input);
        Children.Add(grid);
        _slider.ValueChanged+=(_,_)=>{
            if(_sync)return;
            SetValueText(Math.Round(_slider.Value));
        };
        input.TextChanged+=(_,_)=>{
            if(_sync||!TryValue(out var value))return;
            _sync=true;
            try{_slider.Value=value;}finally{_sync=false;}
            ValueChanged?.Invoke(this,EventArgs.Empty);
        };
        input.LostKeyboardFocus+=(_,_)=>Normalize();
        input.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Enter)Normalize();};
        if(string.IsNullOrEmpty(input.Text))Value=100;
    }

    public string Label
    {
        get=>_caption.Text;
        set{
            _caption.Text=value;
            System.Windows.Automation.AutomationProperties.SetName(_input,value);
            System.Windows.Automation.AutomationProperties.SetName(_slider,value);
        }
    }

    public double Maximum
    {
        get=>_slider.Maximum;
        set=>_slider.Maximum=value;
    }

    public double Value
    {
        get=>TryValue(out var value)?value:_slider.Value;
        set{
            _sync=true;
            try{_slider.Value=Math.Clamp(value,0,Maximum);_input.Text=Format(_slider.Value);}
            finally{_sync=false;}
        }
    }
    internal event EventHandler? ValueChanged;
    private bool TryValue(out double value)=>(double.TryParse(_input.Text,NumberStyles.Float,CultureInfo.CurrentCulture,out value)
        ||double.TryParse(_input.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out value))&&value>=0&&value<=Maximum;
    private static string Format(double value)=>value.ToString("0.##",CultureInfo.CurrentCulture);
    private void Normalize()
    {
        if(!TryValue(out _))SetValueText(_slider.Value);
    }
    private void SetValueText(double value)
    {
        var text=Format(value);
        if(_input.Text!=text)_input.Text=text;
    }
}
