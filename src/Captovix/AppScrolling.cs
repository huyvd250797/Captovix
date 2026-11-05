using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
namespace Captovix;
internal static class AppScrolling
{
    public static readonly DependencyProperty CustomZoomProperty=DependencyProperty.RegisterAttached("CustomZoom",typeof(bool),typeof(AppScrolling),new PropertyMetadata(false));
    private sealed class State {public double Zoom=1;public Transform? Original;public bool Captured;public double MaxWidth;public int WheelRemainder;}
    private static readonly ConditionalWeakTable<ScrollViewer,State> States=new();
    public static void Initialize()=>EventManager.RegisterClassHandler(typeof(ScrollViewer),Mouse.PreviewMouseWheelEvent,new MouseWheelEventHandler(Wheel));
    private static DependencyObject? Parent(DependencyObject item)=>item is Visual or Visual3D?VisualTreeHelper.GetParent(item):LogicalTreeHelper.GetParent(item);
    private static bool IsItemsViewer(ScrollViewer viewer){for(DependencyObject? p=viewer;p!=null;p=Parent(p))if(p is ItemsControl)return true;return viewer.TemplatedParent is ItemsControl;}
    private static void Wheel(object sender,MouseWheelEventArgs e)
    {
        if(e.Handled||sender is not ScrollViewer viewer||viewer.Name=="ImageScroll")return;
        // Nested viewers consume only their own gesture; outer settings stay still.
        ScrollViewer? closest=null;for(var node=e.OriginalSource as DependencyObject;node!=null;node=Parent(node))if(node is ScrollViewer scroll){closest=scroll;break;}
        if(closest!=viewer)return;
        if(IsItemsViewer(viewer)){
            var state=States.GetOrCreateValue(viewer);state.WheelRemainder+=e.Delta;int ticks=state.WheelRemainder/120;state.WheelRemainder%=120;
            bool horizontal=Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)||viewer.ScrollableHeight<=0&&viewer.ScrollableWidth>0;
            int count=Math.Min(24,Math.Abs(ticks)*Math.Max(1,SystemParameters.WheelScrollLines));
            for(int i=0;i<count;i++){if(SystemParameters.WheelScrollLines<0){if(horizontal){if(ticks>0)viewer.PageLeft();else viewer.PageRight();}else{if(ticks>0)viewer.PageUp();else viewer.PageDown();}}else if(horizontal){if(ticks>0)viewer.LineLeft();else viewer.LineRight();}else{if(ticks>0)viewer.LineUp();else viewer.LineDown();}}
            e.Handled=true;return;
        }
        var modifiers=Keyboard.Modifiers;
        if(modifiers.HasFlag(ModifierKeys.Control)){
            if((bool)viewer.GetValue(CustomZoomProperty))return;
            if(viewer.Content is not FrameworkElement content)return;
            var state=States.GetOrCreateValue(viewer);if(!state.Captured){state.Original=content.LayoutTransform;state.MaxWidth=content.MaxWidth;state.Captured=true;if(double.IsInfinity(content.MaxWidth)&&content.ActualWidth>0)content.MaxWidth=content.ActualWidth;viewer.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty,ScrollBarVisibility.Auto);}
            double old=state.Zoom;state.Zoom=Math.Clamp(old*Math.Pow(1.1,e.Delta/120d),.65,2);
            var transforms=new TransformGroup();if(state.Original!=null)transforms.Children.Add(state.Original);transforms.Children.Add(new ScaleTransform(state.Zoom,state.Zoom));content.LayoutTransform=transforms;
            e.Handled=true;return;
        }
        var distance=e.Delta/120d*Math.Max(1,SystemParameters.WheelScrollLines)*16;
        if(modifiers.HasFlag(ModifierKeys.Shift)||viewer.ScrollableHeight<=0&&viewer.ScrollableWidth>0)viewer.ScrollToHorizontalOffset(Math.Clamp(viewer.HorizontalOffset-distance,0,viewer.ScrollableWidth));
        else viewer.ScrollToVerticalOffset(Math.Clamp(viewer.VerticalOffset-distance,0,viewer.ScrollableHeight));
        e.Handled=true;
    }
}
