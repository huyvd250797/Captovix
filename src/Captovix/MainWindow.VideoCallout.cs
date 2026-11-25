using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private static readonly (int X,int Y)[] VideoCalloutEdges=
        [(0,0),(1,0),(2,0),(2,1),(2,2),(1,2),(0,2),(0,1)];

    private VideoMark PlaceVideoCallout(VideoMark mark,Point start,Point end)
    {
        var tail=VideoNormalized(start);
        var drag=VideoNormalized(end);
        double width=540*_videoBounds.Width/_videoBounds.Height;
        var a=VideoMarkRendering.AnnotationFor(mark,width,540);
        CalloutGesture.Place(a,tail.X*width,tail.Y*540,drag.X*width,drag.Y*540);
        double w=Math.Min(1,a.Width/width),h=Math.Min(1,a.Height/540);
        double x=Math.Clamp(a.Left/width,0,1-w),y=Math.Clamp(a.Top/540,0,1-h);
        return mark with{X1=x,Y1=y,X2=x+w,Y2=y+h,Tail=new(tail.X,tail.Y)};
    }

    private IEnumerable<(int Handle,Point Point)> VideoCalloutHandles(VideoMark mark)
    {
        var a=VideoMarkRendering.AnnotationFor(mark,_videoBounds.Width,_videoBounds.Height);
        var origin=new Vector(_videoBounds.X,_videoBounds.Y);
        var bounds=EditorRenderer.LocalBox(a);
        yield return (3,EditorRenderer.WorldPoint(a,new(a.TailX,a.TailY))+origin);
        yield return (4,new Point(bounds.CenterX,bounds.CenterY)+origin);
        for(int i=0;i<VideoCalloutEdges.Length;i++){
            var (x,y)=VideoCalloutEdges[i];
            yield return (5+i,EditorRenderer.WorldPoint(a,new(bounds.X+bounds.Width*x/2,bounds.Y+bounds.Height*y/2))+origin);
        }
    }

    private int FindCalloutHandle(VideoMark mark,Point position)
    {
        if(mark.Kind!=VideoMarkKind.Callout)return 0;
        return VideoCalloutHandles(mark).Where(anchor=>(anchor.Point-position).Length<=9)
            .OrderBy(anchor=>(anchor.Point-position).Length).FirstOrDefault().Handle;
    }

    private VideoMark MoveVideoCallout(VideoMark mark,Point from,Point to)
    {
        var a=VideoMarkRendering.AnnotationFor(mark,_videoBounds.Width,_videoBounds.Height);
        var pin=new VideoInkPoint(Math.Clamp(a.TailX/_videoBounds.Width,0,1),Math.Clamp(a.TailY/_videoBounds.Height,0,1));
        var origin=new Vector(_videoBounds.X,_videoBounds.Y);
        var local=EditorRenderer.LocalPoint(a,to-origin);
        if(_videoDragHandle==3)
            return mark with{Tail=new(Math.Clamp(local.X/_videoBounds.Width,0,1),Math.Clamp(local.Y/_videoBounds.Height,0,1))};
        double x1=Math.Min(mark.X1,mark.X2),y1=Math.Min(mark.Y1,mark.Y2),x2=Math.Max(mark.X1,mark.X2),y2=Math.Max(mark.Y1,mark.Y2);
        if(_videoDragHandle>=5){
            var (x,y)=VideoCalloutEdges[_videoDragHandle-5];
            var minimumX=Math.Min(x2-x1,CalloutGesture.MinimumSize/540*_videoBounds.Height/_videoBounds.Width);
            var minimumY=Math.Min(y2-y1,CalloutGesture.MinimumSize/540);
            if(x==0)x1=Math.Clamp(local.X/_videoBounds.Width,0,x2-minimumX);
            if(x==2)x2=Math.Clamp(local.X/_videoBounds.Width,x1+minimumX,1);
            if(y==0)y1=Math.Clamp(local.Y/_videoBounds.Height,0,y2-minimumY);
            if(y==2)y2=Math.Clamp(local.Y/_videoBounds.Height,y1+minimumY,1);
            return mark with{X1=x1,Y1=y1,X2=x2,Y2=y2,Tail=pin};
        }
        var start=VideoNormalized(from);
        var end=VideoNormalized(to);
        bool bodyOnly=_videoDragHandle==4;
        double minX=bodyOnly?x1:Math.Min(x1,pin.X),minY=bodyOnly?y1:Math.Min(y1,pin.Y);
        double maxX=bodyOnly?x2:Math.Max(x2,pin.X),maxY=bodyOnly?y2:Math.Max(y2,pin.Y);
        double dx=Math.Clamp(end.X-start.X,-minX,1-maxX),dy=Math.Clamp(end.Y-start.Y,-minY,1-maxY);
        return mark with{X1=x1+dx,Y1=y1+dy,X2=x2+dx,Y2=y2+dy,
            Tail=bodyOnly?pin:new(Math.Clamp(pin.X+dx,0,1),Math.Clamp(pin.Y+dy,0,1))};
    }

    private void ShowVideoCalloutDraft(VideoMark mark)
    {
        if(_videoCalloutDraft==null){
            _videoCalloutDraft=new Image{IsHitTestVisible=false,Stretch=Stretch.Fill,Width=_videoBounds.Width,Height=_videoBounds.Height};
            Canvas.SetLeft(_videoCalloutDraft,_videoBounds.X);
            Canvas.SetTop(_videoCalloutDraft,_videoBounds.Y);
            VideoOverlay.Children.Add(_videoCalloutDraft);
        }
        _videoCalloutDraft.Source=VideoMarkRendering.Draw([mark],_videoBounds.Width,_videoBounds.Height);
    }
}
