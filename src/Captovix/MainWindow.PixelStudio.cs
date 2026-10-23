using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private string _pixelFill="#008B92";
    private void PixelTool_Click(object sender,RoutedEventArgs e){var tag=sender is Button b?b.Tag:sender is MenuItem m?m.Tag:null;if(tag is string text&&Enum.TryParse<EditorTool>(text,out var tool)){if(Surface.Tool!=tool)Surface.ClearPixelRegion();SelectTool(tool);Surface.Focus();}}
    private void SyncPixelStudio(){InkControlsPanel.Visibility=Surface.IsPixelTool||Surface.Selected?.Kind==AnnotationKind.ImageLayer?Visibility.Collapsed:Visibility.Visible;PixelPanel.Visibility=Surface.IsPixelTool?Visibility.Visible:Visibility.Collapsed;PixelSummary.Text=Surface.PixelRegion is {} r?$"{(r.Kind==PixelSelectionKind.Rectangle?"Chữ nhật":r.Kind==PixelSelectionKind.Ellipse?"Ellipse":"Lasso")} · {r.Bounds.Width} × {r.Bounds.Height} px · ({r.Bounds.X}, {r.Bounds.Y})":"Kéo trên ảnh để chọn vùng pixel.";PaintButton(PixelFillPaint,"Màu tô",_pixelFill);}
    private void PixelFillPaint_Click(object sender,RoutedEventArgs e)=>OpenPaint(PixelFillPaint,c=>{if(c!=null){_pixelFill=c;SyncPixelStudio();}},false);
    private void PixelAction_Click(object sender,RoutedEventArgs e){var tag=sender is Button b?b.Tag:sender is MenuItem m?m.Tag:null;if(tag is string action)PixelAction(action);}
    private void PixelAction(string action)
    {
        if(_session==null||Surface.Original==null||_busy)return;
        try{
            FlushAutoProperties();if(action=="Paste"){PastePixelImage();return;}if(action=="Clear"){Surface.ClearPixelRegion();return;}
            if(Surface.PixelRegion is not {} region)throw new InvalidOperationException("Chọn vùng pixel trước.");region.Validate(_session.Document.Viewport);
            if((long)_session.Document.OriginalWidth*_session.Document.OriginalHeight>PixelStudio.MaxPixels)throw new InvalidDataException("Chỉnh pixel hỗ trợ tối đa 40 triệu pixel.");
            var source=RasterPixels.Source(Surface.Original,_session.Document);var raw=ImageTransfer.Bgra(source);var b=region.Bounds;
            if(action is "Copy" or "Cut"){var copyDoc=_session.Document.Clone();copyDoc.Appearance=new();copyDoc.Crop=region.Bounds;var merged=EditorRenderer.Render(Surface.Original,copyDoc);var local=region.Copy();local.Bounds=new(0,0,b.Width,b.Height);local.Points=local.Points.Select(p=>new AnnotationPoint(p.X-b.X,p.Y-b.Y)).ToList();var pixels=PixelStudio.Extract(ImageTransfer.Bgra(merged),b.Width,b.Height,local);ImageTransfer.Copy(ImageTransfer.Bitmap(b.Width,b.Height,pixels));if(action=="Copy"){SetStatus("Đã copy vùng ảnh hiển thị (gồm chú thích), giữ số pixel và nền trong suốt.");return;}}
            if(action is "Cut" or "Erase" or "Fill"){
                var color=(Color)ColorConverter.ConvertFromString(_pixelFill);byte[]? fill=action=="Fill"?[color.B,color.G,color.R,255]:null;
                var edited=PixelStudio.Edit(raw,source.PixelWidth,source.PixelHeight,region,fill);string data=RasterPixels.Encode(ImageTransfer.Bitmap(source.PixelWidth,source.PixelHeight,edited));
                _session.Change(d=>{d.PixelBasePng=data;PixelStudio.Validate(d);});Surface.Refresh();SyncProperties();SetStatus(action=="Fill"?"Đã tô vùng ảnh nền · Ctrl+Z hoàn tác.":"Đã xóa pixel thành trong suốt · Ctrl+Z hoàn tác. PNG gốc được giữ.");
            }
        }catch(Exception ex){App.Log(ex);SetStatus("Chưa thực hiện chỉnh pixel: "+ex.Message);}
    }
    private void PastePixelImage()
    {
        if(_session==null)return;if((long)_session.Document.OriginalWidth*_session.Document.OriginalHeight>PixelStudio.MaxPixels)throw new InvalidDataException("Dán lớp ảnh hỗ trợ canvas tối đa 40 triệu pixel.");BitmapSource? image=null;var value=Clipboard.GetDataObject()?.GetData("PNG");
        if(value is Stream stream){if(stream.Length>72_000_000)throw new InvalidDataException("PNG clipboard quá lớn.");if(stream.CanSeek)stream.Position=0;using var bytes=new MemoryStream();stream.CopyTo(bytes);image=RasterPixels.Decode(Convert.ToBase64String(bytes.ToArray()));}
        image??=Clipboard.GetImage();if(image==null){SetStatus("Clipboard chưa có ảnh. Copy vùng pixel hoặc ảnh từ ứng dụng khác trước.");return;}
        string data=RasterPixels.Encode(image);var view=_session.Document.Viewport;var at=Surface.PixelRegion?.Bounds??view;
        double x=at.X,y=at.Y;var layer=new Annotation{Kind=AnnotationKind.ImageLayer,Name="Ảnh dán",X1=x,Y1=y,X2=x+image.PixelWidth,Y2=y+image.PixelHeight,PixelPng=data};
        _session.Change(d=>{d.Annotations.Add(layer);PixelStudio.Validate(d);});Surface.ClearPixelRegion();SelectTool(EditorTool.Select);Surface.SelectIds([layer.Id]);Surface.Focus();SetStatus($"Đã dán lớp ảnh {image.PixelWidth} × {image.PixelHeight} px; chọn/kéo/resize như đối tượng. Phần ngoài canvas bị cắt khi xuất.");
    }
}
