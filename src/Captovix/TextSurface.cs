namespace Captovix;
public sealed partial class EditorSurface
{
    public Annotation TextDefaults {get;}=new(){Kind=AnnotationKind.Text};
    public Annotation CalloutDefaults {get;}=new(){Kind=AnnotationKind.Callout,FillEnabled=true,FillColor="#FFFFFF"};
    public Annotation StepDefaults {get;}=new(){Kind=AnnotationKind.Step};
    public Annotation TextDefaultsFor(AnnotationKind kind)=>kind==AnnotationKind.Callout?CalloutDefaults:kind==AnnotationKind.Step?StepDefaults:TextDefaults;
    private void InitializeTextObject(Annotation a)
    {
        if(!TextStudio.IsText(a.Kind))return;var style=TextDefaultsFor(a.Kind);if(a.Kind==AnnotationKind.Callout)ShapeStudio.CopyStyle(style,a);TextStudio.CopyStyle(style,a);a.FillEnabled=style.FillEnabled;a.FillColor=style.FillColor;a.FillOpacity=style.FillOpacity;
        if(a.Kind==AnnotationKind.Callout)a.StrokeEnabled=style.StrokeEnabled;
    }
    public bool UsesSelectionGesture(bool hit,bool handle,bool forceNew)=>Tool==EditorTool.Select||Tool!=EditorTool.Crop&&!forceNew&&(hit||handle);
}
