using System.Windows.Controls;

namespace Captovix;

public partial class MainWindow
{
    private enum PercentageChannel { ShapeStroke, ShapeFill, TextStroke, TextFill, Image }
    private PercentField _shapeStrokePercent=null!,_shapeFillPercent=null!;
    private PercentageDrag? _percentageDrag;

    private sealed record PercentageDrag(EditorSession Session,PercentField Field,Annotation[] Targets);

    private void InitializeOpacityControls()
    {
        PercentField Pair(TextBox input)
        {
            var parent=(Panel)input.Parent;
            int index=parent.Children.IndexOf(input);
            var caption=(TextBlock)parent.Children[index-1];
            parent.Children.RemoveAt(index);
            parent.Children.RemoveAt(index-1);
            var field=new PercentField(input){Label=caption.Text};
            parent.Children.Insert(index-1,field);
            return field;
        }

        _shapeStrokePercent=Pair(ShapeStrokeOpacity);
        _shapeFillPercent=Pair(ShapeFillOpacity);
        foreach(var input in new[]{FocusOpacityField,ObjectShadowOpacity,ShadowOpacity})Pair(input);

        Wire(_shapeStrokePercent,PercentageChannel.ShapeStroke);
        Wire(_shapeFillPercent,PercentageChannel.ShapeFill);
        Wire(TextStrokePercent,PercentageChannel.TextStroke);
        Wire(TextFillPercent,PercentageChannel.TextFill);
        Wire(_layerOpacity,PercentageChannel.Image);

        void Wire(PercentField field,PercentageChannel channel)
        {
            field.DragValueChanged+=(_,_)=>PreviewPercentage(field,channel);
            field.DragFinished+=(_,args)=>{
                if(_percentageDrag?.Field==field)FinishPercentageDrag(args.Canceled);
            };
            field.EditCommitted+=(_,_)=>CommitPercentage(field,channel);
        }
    }

    private static bool MatchesPercentage(AnnotationKind kind,PercentageChannel channel)=>channel switch{
        PercentageChannel.ShapeStroke=>ShapeStudio.HasStroke(kind),
        PercentageChannel.ShapeFill=>ShapeStudio.IsClosed(kind),
        PercentageChannel.TextStroke=>kind is AnnotationKind.Callout or AnnotationKind.Step,
        PercentageChannel.TextFill=>TextStudio.IsText(kind),
        _=>kind==AnnotationKind.ImageLayer
    };

    private static double ReadPercentage(Annotation a,PercentageChannel channel)=>channel switch{
        PercentageChannel.ShapeStroke or PercentageChannel.TextStroke=>a.StrokeOpacity,
        PercentageChannel.Image=>(a.ImageStyle?.Opacity??1)*a.ObjectOpacity,
        _=>a.FillOpacity
    };

    private static void SetPercentage(Annotation a,PercentageChannel channel,double value)
    {
        switch(channel){
            case PercentageChannel.ShapeStroke:
            case PercentageChannel.TextStroke:
                a.StrokeOpacity=value;
                if(a.Kind==AnnotationKind.Step)a.StepStrokeColor??="#FFFFFF";
                break;
            case PercentageChannel.Image:
                (a.ImageStyle??=new()).Opacity=value/100;
                a.ObjectOpacity=100;
                break;
            default:
                a.FillOpacity=value;
                if(a.Kind==AnnotationKind.Step)a.StepFillVisible??=true;
                break;
        }
    }

    private Annotation[] PercentageTargets(EditorSession session,PercentageChannel channel)
    {
        var ids=Surface.SelectedIds;
        if(!ObjectControl.CanEdit(session.Document,ids))return [];
        return session.Document.Annotations.Where(a=>ids.Contains(a.Id)&&MatchesPercentage(a.Kind,channel)).ToArray();
    }

    private void PreviewPercentage(PercentField field,PercentageChannel channel)
    {
        if(!_editorReady||_syncProperties||Surface.IsInteracting||_session is not {} session)return;
        if(_percentageDrag==null){
            double requested=field.Value;
            FlushAutoProperties();
            EndInlineText(true);
            if(!ReferenceEquals(session,_session))return;
            var targets=PercentageTargets(session,channel);
            if(targets.Length==0||targets.All(a=>ReadPercentage(a,channel)==requested)||!RequireLicense()||!field.IsDragging)return;
            session.Commit();
            session.Begin();
            _percentageDrag=new(session,field,targets);
        }
        if(_percentageDrag.Field!=field||_percentageDrag.Session!=session)return;
        foreach(var target in _percentageDrag.Targets)SetPercentage(target,channel,field.Value);
        Surface.InvalidateScenePaint();
    }

    private void FinishPercentageDrag(bool canceled=false)
    {
        if(_percentageDrag is not {} drag)return;
        _percentageDrag=null;
        if(canceled){
            drag.Session.Cancel();
            Surface.Refresh();
            SyncProperties();
            return;
        }
        drag.Session.Commit();
    }

    private void CommitPercentage(PercentField field,PercentageChannel channel)
    {
        if(!_editorReady||_syncProperties||Surface.IsInteracting||_session is not {} session)return;
        double value=field.Value;
        FlushAutoProperties();
        if(!ReferenceEquals(session,_session)||!RequireLicense())return;
        EndInlineText(true);
        var targets=PercentageTargets(session,channel);
        if(targets.Any(a=>ReadPercentage(a,channel)!=value)){
            session.Change(_=>{foreach(var target in targets)SetPercentage(target,channel,value);});
        }else if(Surface.SelectedIds.Count==0){
            var defaults=channel switch{
                PercentageChannel.TextFill or PercentageChannel.TextStroke=>Surface.TextDefaultsFor(CurrentTextKind()),
                PercentageChannel.ShapeFill or PercentageChannel.ShapeStroke=>Surface.ShapeDefaults,
                _=>null
            };
            if(defaults!=null)SetPercentage(defaults,channel,value);
        }
    }
}
