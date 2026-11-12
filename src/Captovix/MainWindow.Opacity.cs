using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Captovix;

public partial class MainWindow
{
    private readonly PercentField _imageObjectOpacity=new(){Label="Opacity (%)"};
    private void InitializeOpacityControls()
    {
        void Pair(TextBox input)
        {
            if(input.Parent is not Panel parent)return;
            int index=parent.Children.IndexOf(input);
            string label=index>0&&parent.Children[index-1] is TextBlock caption?caption.Text:"Opacity (%)";
            parent.Children.RemoveAt(index);
            if(index>0&&parent.Children[index-1] is TextBlock)parent.Children.RemoveAt(--index);
            parent.Children.Insert(index,new PercentField(input){Label=label});
        }
        foreach(var field in new[]{ShapeStrokeOpacity,ShapeFillOpacity,TextBackgroundOpacity,FocusOpacityField,ObjectShadowOpacity,ShadowOpacity})
            Pair(field);
        _imageObjectOpacity.Visibility=Visibility.Collapsed;
        EditorProperties.Children.Insert(0,_imageObjectOpacity);
        _imageObjectOpacity.ValueChanged+=(_,_)=>ScheduleProperty("Opacity",false);
        _imageObjectOpacity.LostKeyboardFocus+=(_,_)=>FlushAutoProperties();
    }

    private void ApplyImageObjectOpacity()
    {
        if(_session==null||Surface.SelectedIds.Count==0||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return;
        var value=_imageObjectOpacity.Value;
        if(_session.Document.Annotations.Where(item=>Surface.SelectedIds.Contains(item.Id)).All(item=>item.ObjectOpacity==value))return;
        ChangeSelected(item=>item.ObjectOpacity=value);
        Surface.Refresh();
    }

    private void SyncImageObjectOpacity()
    {
        bool show=_session!=null&&Surface.SelectedIds.Count>0;
        _imageObjectOpacity.Visibility=show?Visibility.Visible:Visibility.Collapsed;
        if(show){
            _imageObjectOpacity.IsEnabled=ObjectControl.CanEdit(_session!.Document,Surface.SelectedIds);
            if(!_imageObjectOpacity.IsKeyboardFocusWithin&&!_imageObjectOpacity.IsMouseCaptureWithin)
                _imageObjectOpacity.Value=Surface.Selected?.ObjectOpacity??100;
        }
    }
}
