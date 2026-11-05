using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
namespace Captovix;
public static class PopupPresentation
{
 public static readonly DependencyProperty EnabledProperty=DependencyProperty.RegisterAttached("Enabled",typeof(bool),typeof(PopupPresentation),new PropertyMetadata(false,(d,e)=>{if(d is Popup p&&(bool)e.NewValue)Attach(p);}));
 public static void SetEnabled(DependencyObject d,bool value)=>d.SetValue(EnabledProperty,value);
 public static bool GetEnabled(DependencyObject d)=>(bool)d.GetValue(EnabledProperty);
 private static readonly ConditionalWeakTable<Popup,object> Attached=new();
 public static void Attach(Popup popup){if(Attached.TryGetValue(popup,out _))return;Attached.Add(popup,new());popup.Opened+=(_,_)=>{if(popup.Child is not FrameworkElement child||child.Tag as string=="@popup-frame"||popup.TemplatedParent is ContextMenu)return;if(child is Border inner){inner.BorderThickness=new(0);inner.Effect=null;}
 popup.Child=null;var border=new Border{Tag="@popup-frame",Child=child,Padding=new(1),Margin=new(12),CornerRadius=new(9),BorderThickness=new(1),Effect=Shadow()};border.SetResourceReference(Border.BackgroundProperty,"Surface");border.SetResourceReference(Border.BorderBrushProperty,"PopupLine");popup.AllowsTransparency=true;popup.Child=border;};}
 internal static DropShadowEffect Shadow()=>new(){Color=Colors.Black,BlurRadius=18,ShadowDepth=3,Opacity=.38};
 internal static void DecorateWindow(Window w,FrameworkElement root)
 {
  if(w is MainWindow)return;
  // Detach from Window before parenting this content under the common frame.
  if(ReferenceEquals(w.Content,root))w.Content=null;
  var original=root;var border=new Border{Child=original,BorderThickness=new(1),Effect=Shadow()};border.SetResourceReference(Border.BorderBrushProperty,"PopupLine");border.SetResourceReference(Border.BackgroundProperty,"AppBackground");w.Content=border;
  // Inactive-modal feedback animates only its outline; the window never hides.
  var glow=new DropShadowEffect{Color=Colors.Silver,BlurRadius=20,ShadowDepth=0,Opacity=0};var overlay=new Border{IsHitTestVisible=false,BorderThickness=new(1),Opacity=0,Effect=glow};overlay.SetResourceReference(Border.BorderBrushProperty,"PopupLine");
  border.Child=null;var layer=new Grid();layer.Children.Add(original);layer.Children.Add(overlay);border.Child=layer;
  DateTime last=DateTime.MinValue;void Pulse(){if(!w.IsVisible||!w.IsEnabled||DateTime.UtcNow-last<TimeSpan.FromMilliseconds(600))return;last=DateTime.UtcNow;overlay.BeginAnimation(UIElement.OpacityProperty,new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromMilliseconds(650),KeyFrames={new LinearDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.Zero)),new LinearDoubleKeyFrame(1,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(140))),new LinearDoubleKeyFrame(.2,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(290))),new LinearDoubleKeyFrame(.9,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(420))),new LinearDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(650)))}});glow.BeginAnimation(DropShadowEffect.OpacityProperty,new DoubleAnimation(.85,0,TimeSpan.FromMilliseconds(650)));}
  var owner=w.Owner;var ownerHandle=owner==null?0:new WindowInteropHelper(owner).Handle;var source=ownerHandle==0?null:HwndSource.FromHwnd(ownerHandle);var self=HwndSource.FromHwnd(new WindowInteropHelper(w).Handle);
  nint Hook(nint hwnd,int msg,nint wp,nint lp,ref bool handled){int mouse=(int)((long)lp>>16)&0xffff;if(ownerHandle!=0&&!IsWindowEnabled(ownerHandle)&&(msg==0x21||msg==0x20&&mouse is 0x201 or 0x204 or 0x207))w.Dispatcher.BeginInvoke(Pulse);return 0;}
  nint SelfHook(nint hwnd,int msg,nint wp,nint lp,ref bool handled){if(ownerHandle!=0&&!IsWindowEnabled(ownerHandle)&&msg==0x86&&wp==0)w.Dispatcher.BeginInvoke(Pulse);return 0;}
  source?.AddHook(Hook);self?.AddHook(SelfHook);w.Closed+=(_,_)=>{source?.RemoveHook(Hook);self?.RemoveHook(SelfHook);overlay.BeginAnimation(UIElement.OpacityProperty,null);};
 }
 [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)]private static extern bool IsWindowEnabled(nint hwnd);
}
