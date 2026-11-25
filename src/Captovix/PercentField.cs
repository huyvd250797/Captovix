using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Captovix;

public sealed class PercentField : StackPanel
{
    private readonly TextBlock _caption=new(){Margin=new(4,1,4,1)};
    private readonly Slider _slider=new(){Minimum=0,Maximum=100,TickFrequency=1,IsSnapToTickEnabled=true,SmallChange=1,LargeChange=10,
        VerticalAlignment=VerticalAlignment.Center,Margin=new(4,0,9,0),IsMoveToPointEnabled=true};
    private readonly TextBox _input;
    private bool _sync;
    private double _committed=100;

    public PercentField():this(new TextBox()){}
    internal PercentField(TextBox input)
    {
        _input=input;
        _slider.SetResourceReference(StyleProperty,"PercentageSlider");
        Children.Add(_caption);
        var grid=new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new(64)});
        grid.Children.Add(_slider);
        Grid.SetColumn(input,1);
        input.MinWidth=0;
        input.Margin=new(0,2,4,3);
        input.Padding=new(5,4,7,4);
        input.MinHeight=30;
        input.SetResourceReference(Control.BackgroundProperty,"Soft");
        input.VerticalAlignment=VerticalAlignment.Center;
        input.HorizontalContentAlignment=HorizontalAlignment.Right;
        grid.Children.Add(input);
        Children.Add(grid);

        _slider.AddHandler(Thumb.DragStartedEvent,new DragStartedEventHandler((_,_)=>IsDragging=true),true);
        _slider.AddHandler(Thumb.DragCompletedEvent,new DragCompletedEventHandler((_,args)=>{
            IsDragging=false;
            DragFinished?.Invoke(this,args);
            if(args.Canceled)Value=_committed;
            else CommitValue();
        }),true);
        _slider.ValueChanged+=(_,_)=>{
            if(_sync)return;
            SetText(Math.Round(_slider.Value));
            if(IsDragging)DragValueChanged?.Invoke(this,EventArgs.Empty);
            else CommitValue();
        };
        input.TextChanged+=(_,_)=>{
            if(_sync||!TryValue(out var value))return;
            _sync=true;
            try{_slider.Value=value;}finally{_sync=false;}
            ValueChanged?.Invoke(this,EventArgs.Empty);
        };
        input.LostKeyboardFocus+=(_,_)=>CommitValue();
        input.PreviewKeyDown+=(_,e)=>{
            if(e.Key!=Key.Enter||EditCommitted==null)return;
            CommitValue();
            e.Handled=true;
        };
        Value=TryValue(out var initial)?initial:100;
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
            if(IsDragging)return;
            _sync=true;
            try{
                _slider.Value=Math.Clamp(value,0,Maximum);
                _input.Text=Format(_slider.Value);
                _committed=_slider.Value;
            }finally{_sync=false;}
        }
    }

    internal bool IsDragging {get;private set;}
    internal event EventHandler? ValueChanged,DragValueChanged,EditCommitted;
    internal event DragCompletedEventHandler? DragFinished;
    private bool TryValue(out double value)=>(double.TryParse(_input.Text,NumberStyles.Float,CultureInfo.CurrentCulture,out value)
        ||double.TryParse(_input.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out value))&&value>=0&&value<=Maximum;
    private static string Format(double value)=>value.ToString("0.##",CultureInfo.CurrentCulture);

    private void CommitValue()
    {
        if(IsDragging)return;
        if(!TryValue(out var value)){Value=EditCommitted==null?_slider.Value:_committed;return;}
        if(value==_committed)return;
        _committed=value;
        EditCommitted?.Invoke(this,EventArgs.Empty);
    }

    private void SetText(double value)
    {
        var text=Format(value);
        if(_input.Text!=text)_input.Text=text;
    }
}
