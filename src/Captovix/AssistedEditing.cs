using System.Windows;
using System.Windows.Media.Imaging;
namespace Captovix;
public sealed record AssistRequest(PixelRect Region,string Text,string Background,string Foreground,double FontSize,bool Extract);
internal static class AssistedEditing
{
    internal static Annotation[] Objects(AssistRequest request,ImageLayout layout,EditorDocument doc,BitmapSource snapshot)
    {
        var b=request.Region;
        if(b.Width<2||b.Height<2||(long)b.Width*b.Height>20_000_000||request.Text.Length>20000||request.FontSize<8||request.FontSize>240)throw new ArgumentException("Vùng/chữ vượt giới hạn chỉnh sửa hỗ trợ.");
        if(Math.Abs(layout.A-1)>.00001||Math.Abs(layout.D-1)>.00001||Math.Abs(layout.B)>.00001||Math.Abs(layout.C)>.00001)throw new InvalidOperationException("Ảnh đang xoay/lật/resize: tạo bản ảnh mới từ kết quả hiển thị trước khi sửa hỗ trợ.");
        var p=layout.ToSource(b.X,b.Y);int x=(int)Math.Round(p.X),y=(int)Math.Round(p.Y);var view=doc.Viewport;
        if(x<view.X||y<view.Y||x+b.Width>view.Right||y+b.Height>view.Bottom)throw new ArgumentException("Chọn vùng nằm trong ảnh, không chọn phần đệm/khung ngoài ảnh.");
        _=EditorRenderer.Ink(request.Background);_=EditorRenderer.Ink(request.Foreground);
        var background=new Annotation{Kind=AnnotationKind.Rectangle,Name="Nền phục hồi · màu phẳng",X1=x,Y1=y,X2=x+b.Width,Y2=y+b.Height,FillEnabled=true,FillColor=request.Background,FillOpacity=100,StrokeEnabled=false};
        Annotation content;
        if(request.Extract){
            var image=new CroppedBitmap(snapshot,new Int32Rect(b.X,b.Y,b.Width,b.Height));image.Freeze();
            content=new(){Kind=AnnotationKind.ImageLayer,ImageStyle=new(),Name="Vùng giao diện đã tách",X1=x,Y1=y,X2=x+b.Width,Y2=y+b.Height,PixelPng=RasterPixels.Encode(image)};
        }else{
            if(string.IsNullOrWhiteSpace(request.Text))throw new ArgumentException("Nhập chữ thay thế trước khi xem trước.");
            content=new(){Kind=AnnotationKind.Text,Name="Chữ chỉnh sửa hỗ trợ",X1=x,Y1=y,X2=x+b.Width,Y2=y+b.Height,Text=request.Text,TextColor=request.Foreground,Color=request.Foreground,FontFamily="Segoe UI",FontSize=request.FontSize,TextWidth=b.Width,TextPadding=0,FillEnabled=false};
            var text=EditorRenderer.TextLayout(content);if(text.Height>b.Height+1||text.WidthIncludingTrailingWhitespace>b.Width+1)throw new ArgumentException("Chữ chưa vừa vùng chọn. Giảm cỡ chữ hoặc mở rộng vùng trước khi áp dụng.");
        }
        return [background,content];
    }
    internal static BitmapSource Preview(BitmapSource snapshot,AssistRequest request)
    {
        var doc=new EditorDocument{OriginalWidth=snapshot.PixelWidth,OriginalHeight=snapshot.PixelHeight};var layout=new ImageLayout(doc.Viewport,doc.Appearance);doc.Annotations.AddRange(Objects(request,layout,doc,snapshot));return EditorRenderer.Render(snapshot,doc);
    }
}
