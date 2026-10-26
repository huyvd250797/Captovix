using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
internal static class Program
{
 [STAThread]static int Main(){try{int n=0;void Check(bool b,string s){if(!b)throw new Exception(s);n++;Console.WriteLine("PASS: "+s);}var app=new App();app.InitializeComponent();
 var source=ImageTransfer.Bitmap(120,80,Enumerable.Repeat((byte)255,120*80*4).ToArray());var bytes=ImageTransfer.Bgra(source);var same=BatchRendering.Transform(source,new());Check(ReferenceEquals(same,source),"Native/no-watermark preset reuses exact render pixels");var resized=BatchRendering.Transform(source,new(LongEdge:60));Check(resized.PixelWidth==60&&resized.PixelHeight==40,"Batch resize exact dimensions");var watermarked=BatchRendering.Transform(source,new(Watermark:"Captovix",WatermarkSize:20));Check(watermarked.PixelWidth==120&&watermarked.PixelHeight==80,"Watermark preserves geometry");Check(!ImageTransfer.Bgra(watermarked).SequenceEqual(bytes)&&ImageTransfer.Bgra(source).SequenceEqual(bytes),"Watermark changes output only");
 foreach(var kind in new[]{AnnotationKind.Step,AnnotationKind.Callout,AnnotationKind.Text,AnnotationKind.Magnifier,AnnotationKind.Blur}){var a=StudioStyle.Sample(new Annotation{Kind=kind,FillEnabled=true,Text="Aa",FontSize=18});var drawing=ToolPreview.Create(a);Check(!drawing.Bounds.IsEmpty&&drawing.Bounds.Width>0&&drawing.Bounds.Height>0,"Sample has actual visible bounds: "+kind);Check(drawing.ClipGeometry==null,"Sample has no origin clip: "+kind);}
 var paths=(Dictionary<string,string>)typeof(AppIcons).GetField("Paths",BindingFlags.Static|BindingFlags.NonPublic)!.GetValue(null)!;foreach(var pair in paths){var geometry=SoftIcon.Create(pair.Value);Check(!geometry.Bounds.IsEmpty&&double.IsFinite(geometry.Bounds.Width)&&geometry.IsFrozen,"Rounded icon valid/frozen: "+pair.Key);}
 var pdf=BatchRendering.Pdf(ImageTransfer.Bitmap(1,1,[0,0,255,255]));using(var z=new System.IO.Compression.ZLibStream(new System.IO.MemoryStream(pdf.DeflatedRgb),System.IO.Compression.CompressionMode.Decompress)){var rgb=new byte[3];z.ReadExactly(rgb);Check(rgb.SequenceEqual(new byte[]{255,0,0}),"PDF converts BGRA to RGB without channel reversal");}
 Console.WriteLine($"{n} batch/preview/icon Windows checks passed; desktop interaction acceptance remains pending.");return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
