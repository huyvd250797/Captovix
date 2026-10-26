using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static class SoftIcon
{
    public static Geometry Create(string path)
    {
        var flattened=Geometry.Parse(path).GetFlattenedPathGeometry(.04,ToleranceType.Absolute);var result=new StreamGeometry{FillRule=flattened.FillRule};
        using(var c=result.Open())foreach(var f in flattened.Figures){var p=new List<Point>{f.StartPoint};foreach(var segment in f.Segments){if(segment is LineSegment line)p.Add(line.Point);else if(segment is PolyLineSegment poly)p.AddRange(poly.Points);}if(p.Count<2)continue;if(f.IsClosed&&(p[^1]-p[0]).Length<.01)p.RemoveAt(p.Count-1);if(p.Count<2)continue;
            Point Before(int i){if(!f.IsClosed&&(i==0||i==p.Count-1))return p[i];var d=p[(i+p.Count-1)%p.Count]-p[i];double n=Math.Min(1.5,Math.Min(d.Length,(p[(i+1)%p.Count]-p[i]).Length)*.35);return d.Length==0?p[i]:p[i]+d*(n/d.Length);}
            Point After(int i){if(!f.IsClosed&&(i==0||i==p.Count-1))return p[i];var d=p[(i+1)%p.Count]-p[i];double n=Math.Min(1.5,Math.Min(d.Length,(p[(i+p.Count-1)%p.Count]-p[i]).Length)*.35);return d.Length==0?p[i]:p[i]+d*(n/d.Length);}
            c.BeginFigure(f.IsClosed?After(0):p[0],f.IsFilled,f.IsClosed);for(int i=1;i<p.Count;i++){c.LineTo(Before(i),true,false);if(f.IsClosed||i<p.Count-1)c.QuadraticBezierTo(p[i],After(i),true,false);}if(f.IsClosed){c.LineTo(Before(0),true,false);c.QuadraticBezierTo(p[0],After(0),true,false);}
        }result.Freeze();return result;
    }
}
