using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;

public static partial class EditorRenderer
{
    internal static string FlattenImageLayer(BitmapSource original, EditorDocument document, string id)
    {
        var layer = document.Annotations.First(a => a.Id == id);
        if (layer.Kind != AnnotationKind.ImageLayer || layer.PixelPng == null)
            throw new InvalidDataException("Chọn một lớp ảnh hợp lệ.");
        var viewport = document.Viewport;
        var clip = new Rect(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        var layerBounds = ContentBounds(layer);
        var affected = Rect.Intersect(layerBounds, clip);
        if (affected.IsEmpty || affected.Width <= 0 || affected.Height <= 0)
            throw new InvalidDataException("Lớp ảnh đang nằm ngoài vùng ảnh hiển thị.");
        if (document.Appearance.DrawOutsideImage && !clip.Contains(layerBounds))
            throw new InvalidDataException("Đưa lớp ảnh vào bên trong ảnh nền trước khi hợp nhất để giữ phần đang hiển thị.");
        if (document.Recoveries.Any(p => affected.IntersectsWith(new Rect(p.Bounds.X, p.Bounds.Y, p.Bounds.Width, p.Bounds.Height))) ||
            document.Annotations.Any(a => a.Visible && a.Kind == AnnotationKind.Spotlight))
            throw new InvalidDataException("Vùng ảnh đang có hiệu ứng hoặc mặt nạ nền ảnh hưởng đến thứ tự hiển thị.");
        if (document.Annotations.TakeWhile(a => a.Id != id).Any(a => a.Visible && !a.IsRedaction && affected.IntersectsWith(ContentBounds(a))))
            throw new InvalidDataException("Đưa lớp ảnh xuống dưới các đối tượng đang chồng lên nó rồi thử lại.");
        if (document.Annotations.Any(a => a.StartAnchor?.TargetId == id || a.EndAnchor?.TargetId == id))
            throw new InvalidDataException("Tháo liên kết đường nối khỏi lớp ảnh trước khi hợp nhất.");
        if ((long)document.OriginalWidth * document.OriginalHeight > PixelStudio.MaxPixels)
            throw new InvalidDataException("Hợp nhất hỗ trợ ảnh nền tối đa 40 triệu pixel.");

        var visual = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);
        using (var drawing = visual.RenderOpen())
        {
            drawing.DrawImage(RasterPixels.Source(original, document), new Rect(0, 0, document.OriginalWidth, document.OriginalHeight));
            drawing.PushClip(new RectangleGeometry(clip));
            DrawAnnotation(drawing, layer);
            drawing.Pop();
        }
        var bitmap = new RenderTargetBitmap(document.OriginalWidth, document.OriginalHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return RasterPixels.Encode(bitmap);
    }
}
