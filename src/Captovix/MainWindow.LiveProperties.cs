using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
namespace Captovix;
public partial class MainWindow
{
    private readonly DispatcherTimer _propertyTimer=new(){Interval=TimeSpan.FromMilliseconds(400)};
    private string? _pendingSection,_pendingTarget;
    private bool _autoApplying,_syncCaptureStyle;
    private string PropertyTarget()=>System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_session??(object)this)+"|"+Surface.Tool+"|"+string.Join(",",Surface.SelectedIds.Order());
    private void InitializeLiveProperties()
    {
        _propertyTimer.Tick+=(_,_)=>FlushAutoProperties();
        void Wire(string section,params string[] names){foreach(var name in names){if(FindName(name) is not FrameworkElement control)continue;
            if(control is TextBox text){text.TextChanged+=(_,_)=>ScheduleProperty(section,false);text.LostKeyboardFocus+=(_,_)=>FlushAutoProperties();}
            else if(control is ComboBox combo){combo.SelectionChanged+=(_,_)=>ScheduleProperty(section,true);if(combo.IsEditable){combo.AddHandler(TextBoxBase.TextChangedEvent,new TextChangedEventHandler((_,_)=>ScheduleProperty(section,false)));combo.LostKeyboardFocus+=(_,_)=>FlushAutoProperties();}}
            else if(control is CheckBox check){check.Checked+=(_,_)=>ScheduleProperty(section,true);check.Unchecked+=(_,_)=>ScheduleProperty(section,true);}
        }}
        Wire("Shape","ShapeStrokeHex","ShapeFillHex","ShapeThickness","ShapeStrokeOpacity","ShapeFillOpacity","ShapeStrokeEnabled","ShapeFillEnabled","ShapeDash","ShapeRadius","ShapeStarPoints","ShapeStarInset","ShapeArrowTip","ShapeDoubleArrow","ShapeArrowSize");
        Wire("Text","TextFontPicker","TextBold","TextItalic","TextUnderline","TextForeground","TextAlignmentPicker","TextVerticalPicker","TextPaddingField","TextWrapWidth","TextSpacing","TextAutoFit","TextBackgroundHex","TextBackgroundOpacity","TextBorderToggle","CalloutKindPicker","StepKindPicker");
        Wire("Focus","FocusRegionPicker","FocusColorField","FocusOpacityField","FocusZoomField","FocusBorder","FocusBorderWidth","FocusMaskKind","FocusStrength");
        Wire("Object","ObjectX","ObjectY","ObjectWidth","ObjectHeight","ObjectAngle");Wire("Step","StepNumber");Wire("Name","LayerName");
        Wire("Resize","ResizeW","ResizeH");Wire("Canvas","CustomCanvas","CanvasW","CanvasH","CanvasAnchor");
        Wire("Frame","FramePadding","FrameBorder","FrameBorderColor","TransparentBackground","FrameBackground","ShadowEnabled","ShadowBlur","ShadowOpacity","ShadowX","ShadowY");
        Wire("Shadow","ObjectShadowEnabled","ObjectShadowDistance","ObjectShadowBlur","ObjectShadowOpacity");Wire("CaptureStyle","CaptureBorderEnabled","CaptureBorderWidth");
    }
    private void ScheduleProperty(string section,bool immediate)
    {
        if(!_editorReady||_syncProperties||_syncImage||_syncLayers||_editingObjectRatio||_autoApplying||_syncCaptureStyle||Surface.IsInteracting)return;
        if(section=="CaptureStyle"&&(!_loaded||_workflowReadOnly))return;
        if(_pendingSection!=null&&_pendingSection!=section)FlushAutoProperties();
        _pendingSection=section;_pendingTarget=PropertyTarget();_propertyTimer.Stop();if(immediate)FlushAutoProperties();else _propertyTimer.Start();
    }
    private void FlushAutoProperties()
    {
        _propertyTimer.Stop();if(_pendingSection==null||_autoApplying)return;var section=_pendingSection;_pendingSection=null;if(_pendingTarget!=PropertyTarget())return;
        var focused=Keyboard.FocusedElement as TextBox;string? text=focused?.Text;int caret=focused?.SelectionStart??0,length=focused?.SelectionLength??0;
        _autoApplying=true;try{
            if(section=="Focus")FocusError.Text="";if(section=="Shape")ShapeStyleError.Text="";if(section=="Text")TextStudioError.Text="";if(section=="Object")ObjectFrameError.Text="";if(section=="Shadow")VisualStudioError.Text="";
            switch(section){case "Shape":ApplyShapeStyle();break;case "Text":ApplyTextStudio();break;case "Focus":ApplyFocus_Click(this,new());break;case "Object":ApplyObjectFrame();break;case "Step":CommitStep();break;case "Name":RenameLayer_Click(this,new());break;case "Resize":ApplyResize_Click(this,new());break;case "Canvas":ApplyCanvas_Click(this,new());break;case "Frame":ApplyFrame_Click(this,new());break;case "Shadow":ApplyObjectShadow();break;case "CaptureStyle":SaveCaptureBorder();break;}
        }finally{var errors=new[]{ShapeStyleError,TextStudioError,ObjectFrameError,VisualStudioError,FocusError}.Select(b=>(Box:b,Text:b.Text)).ToArray();_autoApplying=false;SyncProperties();foreach(var error in errors)if(!string.IsNullOrEmpty(error.Text))error.Box.Text=error.Text;if(focused!=null&&focused.IsKeyboardFocusWithin&&text!=null&&!(focused==ObjectAngle&&(Surface.SelectedIds.Count>1||Surface.Selected is {} a&&ConnectorStudio.IsPath(a)))){bool old=_syncProperties;_syncProperties=true;_syncImage=true;try{focused.Text=text;focused.Select(Math.Min(caret,text.Length),Math.Min(length,text.Length-Math.Min(caret,text.Length)));}finally{_syncProperties=old;_syncImage=false;}}}
    }
    private void SyncCaptureBorder()
    {
        _syncCaptureStyle=true;try{CaptureBorderEnabled.IsChecked=_workflow.PhotoStyle.BorderEnabled;CaptureBorderWidth.Text=_workflow.PhotoStyle.BorderWidth.ToString();PaintButton(CaptureBorderColor,"Viền chụp",_workflow.PhotoStyle.BorderEnabled?_workflow.PhotoStyle.BorderColor:null);}finally{_syncCaptureStyle=false;}
    }
    private void SaveCaptureBorder()
    {
        try{var c=_workflow.Clone();c.PhotoStyle.BorderEnabled=CaptureBorderEnabled.IsChecked==true;c.PhotoStyle.BorderWidth=ImageNumber(CaptureBorderWidth,1,128);SaveWorkflow(c);SyncCaptureBorder();WorkflowStatus.Text="Đã lưu kiểu chụp; ảnh mới nhận viền và có thể sửa lại trong Editor.";}catch(Exception e){WorkflowStatus.Text=e.Message;}
    }
    private void CaptureBorderColor_Click(object sender,RoutedEventArgs e)=>OpenPaint(CaptureBorderColor,color=>{
        if(_workflowReadOnly)return;var c=_workflow.Clone();c.PhotoStyle.BorderEnabled=color!=null;if(color!=null)c.PhotoStyle.BorderColor=color;try{SaveWorkflow(c);SyncCaptureBorder();}catch(Exception ex){WorkflowStatus.Text=ex.Message;}
    });
}
