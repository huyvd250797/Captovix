namespace Captovix;
public sealed partial class EditorSurface
{
    public Annotation HighlightDefaults {get;}=new(){Kind=AnnotationKind.Highlight};
    public Annotation SpotlightDefaults {get;}=new(){Kind=AnnotationKind.Spotlight,FocusOpacity=70};
    public Annotation MagnifierDefaults {get;}=new(){Kind=AnnotationKind.Magnifier,RegionShape=RegionShape.Ellipse};
    public Annotation PrivacyDefaults {get;}=new(){Kind=AnnotationKind.Cover};
    public Annotation FocusDefaultsFor(AnnotationKind kind)=>kind==AnnotationKind.Highlight?HighlightDefaults:kind==AnnotationKind.Spotlight?SpotlightDefaults:kind==AnnotationKind.Magnifier?MagnifierDefaults:PrivacyDefaults;
    private void InitializeFocusObject(Annotation a){if(FocusStudio.Supports(a.Kind)){var style=FocusDefaultsFor(a.Kind);FocusStudio.CopyStyle(style,a);if(a.Kind==AnnotationKind.Magnifier){a.StrokeEnabled=style.StrokeEnabled;a.StrokeOpacity=style.StrokeOpacity;a.Dash=style.Dash;}}}
}
