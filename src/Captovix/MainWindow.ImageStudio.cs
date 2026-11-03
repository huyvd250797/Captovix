using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
namespace Captovix;
public partial class MainWindow
{
    private void ImageStudio_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        if(_session==null||Surface.Original==null||_busy)return;
        try{if(!FlushEdits())return;EndInlineText(true);Surface.CancelInteraction();var command=(sender as MenuItem)?.Tag?.ToString()??"Color";
            if(command=="ResetBase"){_session.Change(d=>d.PixelBasePng=null);Surface.Refresh();SyncProperties();return;}
            if(command=="Stamp"){StampStudio();return;}if(command=="Comparison"){CompareStudio();return;}if(command=="Watermark"){WatermarkStudio();return;}if(command=="Mask"){MaskStudio();return;}
            if(command=="Pick"){var choose=new Window{Owner=this,Title="Lấy màu · áp dụng cho",SizeToContent=SizeToContent.WidthAndHeight,WindowStartupLocation=WindowStartupLocation.CenterOwner};var choices=new StackPanel{Margin=new(18)};choose.Content=choices;foreach(var target in new[]{"Viền","Nền","Chữ"}){var b=new Button{Content=target,MinWidth=180};choices.Children.Add(b);b.Click+=(_,_)=>{choose.Close();var sample=new ImageWorkshop(EditorRenderer.Render(Surface.Original,_session.Document),5){Owner=this};if(sample.ShowDialog()==true&&sample.PickedColor is {} color){if(target=="Chữ"){Surface.TextDefaultsFor(CurrentTextKind()).TextColor=color;ChangeSelected(a=>{if(TextStudio.IsText(a.Kind))a.TextColor=color;});}else SetObjectPaint(color,target=="Nền");Surface.Refresh();SyncProperties();}};}choose.ShowDialog();return;}
            int mode=command switch{"Clone"=>1,"Perspective"=>2,"Rotate"=>3,"Background"=>4,_=>0};var session=_session;
            var dialog=new ImageWorkshop(RasterPixels.Source(Surface.Original,session.Document),mode){Owner=this};
            if(dialog.ShowDialog()==true&&dialog.Result is {} image){string data=RasterPixels.Encode(image);session.Change(d=>{d.PixelBasePng=data;PixelStudio.Validate(d);});Surface.Refresh();SyncProperties();SetStatus("Đã áp dụng lên nền ảnh · chú thích giữ nguyên vị trí · Ctrl+Z hoàn tác.");}
        }catch(Exception ex){App.Log(ex);SetStatus("Chưa thực hiện: "+ex.Message);}
    }
    private void AddStudioObjects(params Annotation[] items){if(_session==null)return;_session.Change(d=>{d.Annotations.AddRange(items);d.Validate(d.OriginalWidth,d.OriginalHeight,d.SourceHash);});SelectTool(EditorTool.Select);Surface.SelectIds(items.Select(a=>a.Id));Surface.Refresh();SyncProperties();}
    private void StampStudio()=>StudioMore_Click(StudioMoreButton,new());
    private void MaskStudio()
    {

        var dialog=new Window{Owner=this,Title="Cắt lớp ảnh theo hình",SizeToContent=SizeToContent.WidthAndHeight,WindowStartupLocation=WindowStartupLocation.CenterOwner};var row=new StackPanel{Margin=new(18)};dialog.Content=row;
        foreach(var item in new[]{("Chữ nhật",LayerMask.Rectangle),("Ellipse / tròn",LayerMask.Ellipse),("Bo góc",LayerMask.Rounded)}){var b=new Button{Content=item.Item1,MinWidth=200};row.Children.Add(b);b.Click+=async(_,_)=>{try{if(SelectedImage()!=null)LayerChange(a=>{a.ImageStyle??=new();a.ImageStyle.Mask=item.Item2;a.ImageStyle.Radius=item.Item2==LayerMask.Rounded?Math.Min(a.Width,a.Height)*.12:0;});else{var session=_session!;var source=RasterPixels.Source(Surface.Original!,session.Document);var raw=ImageTransfer.Bgra(source);int w=source.PixelWidth,h=source.PixelHeight;dialog.IsEnabled=false;var result=await Task.Run(()=>{var pixels=(byte[])raw.Clone();double radius=Math.Min(w,h)*.12;for(int y=0;y<h;y++)for(int x=0;x<w;x++){bool inside=true;if(item.Item2==LayerMask.Ellipse)inside=Math.Pow((x+.5-w/2d)/(w/2d),2)+Math.Pow((y+.5-h/2d)/(h/2d),2)<=1;else if(item.Item2==LayerMask.Rounded){double dx=Math.Max(0,Math.Abs(x+.5-w/2d)-(w/2d-radius)),dy=Math.Max(0,Math.Abs(y+.5-h/2d)-(h/2d-radius));inside=dx*dx+dy*dy<=radius*radius;}if(!inside)pixels[(y*w+x)*4+3]=0;}return pixels;});string data=RasterPixels.Encode(ImageTransfer.Bitmap(w,h,result));session.Change(d=>d.PixelBasePng=data);Surface.Refresh();SyncProperties();}dialog.Close();}catch(Exception ex){dialog.IsEnabled=true;SetStatus(ex.Message);}};}dialog.ShowDialog();
    }
    private void CompareStudio()
    {
        var picker=new OpenFileDialog{Title="Chọn ảnh Sau để so sánh với ảnh đang mở",Filter="Ảnh|*.png;*.jpg;*.jpeg;*.bmp"};if(picker.ShowDialog(this)!=true)return;
        var before=EditorRenderer.Render(Surface.Original!,_session!.Document);var after=MediaStore.LoadImage(picker.FileName);int cellWidth=Math.Max(before.PixelWidth,after.PixelWidth),height=Math.Max(before.PixelHeight,after.PixelHeight);var o=new CompositionOptions{Width=cellWidth*2+72,Height=height+110,Margin=24,Gap=24,Captions=false};o.Validate();CreateCompositionCanvas(o);AddCompositionImages([("Trước",before),("Sau",after)]);_session!.Change(d=>{var layers=d.Annotations.Where(a=>a.Kind==AnnotationKind.ImageLayer).ToArray();for(int i=0;i<2;i++){var a=layers[i];a.X1=24+i*(cellWidth+24);a.Y1=70;a.X2=a.X1+cellWidth;a.Y2=70+height;d.Annotations.Add(new(){Kind=AnnotationKind.Text,Text=i==0?"Trước":"Sau",X1=a.X1,Y1=24,X2=a.X2,Y2=58,FontSize=26,TextColor="#263445"});}});Surface.Refresh();FitImage();
    }
    private void WatermarkStudio()
    {
        var dialog=new Window{Owner=this,Title="Captovix · Watermark",Width=400,Height=360,WindowStartupLocation=WindowStartupLocation.CenterOwner};var form=new StackPanel{Margin=new(18)};dialog.Content=form;var text=new TextBox{Text="© HuyVo",MaxLength=80};var positions=new ComboBox{ItemsSource=new[]{"Trên trái","Trên giữa","Trên phải","Giữa trái","Chính giữa","Giữa phải","Dưới trái","Dưới giữa","Dưới phải"},SelectedIndex=8};var opacity=new Slider{Minimum=10,Maximum=100,Value=60};form.Children.Add(new TextBlock{Text="Nội dung"});form.Children.Add(text);form.Children.Add(positions);form.Children.Add(new TextBlock{Text="Độ hiện · 10–100%"});form.Children.Add(opacity);
        void Place(Annotation a){var v=_session!.Document.Viewport;double w=Math.Min(a.Width,v.Width*.8),h=Math.Min(a.Height,v.Height*.5);int pos=positions.SelectedIndex;a.X1=v.X+Math.Max(0,(v.Width-w-24)*(pos%3)/2d)+12;a.Y1=v.Y+Math.Max(0,(v.Height-h-24)*(pos/3)/2d)+12;a.X2=a.X1+w;a.Y2=a.Y1+h;a.Name="Watermark";}
        var add=new Button{Content="Thêm chữ"};form.Children.Add(add);add.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(text.Text))return;var a=new Annotation{Kind=AnnotationKind.Text,Text=text.Text,TextColor="#263445",X2=280,Y2=48,FontSize=28,StrokeOpacity=opacity.Value};var formatted=new FormattedText(text.Text,System.Globalization.CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),28,Brushes.SlateGray,1);int width=(int)Math.Ceiling(formatted.WidthIncludingTrailingWhitespace)+8,height=(int)Math.Ceiling(formatted.Height)+8;var visual=new DrawingVisual();using(var dc=visual.RenderOpen())dc.DrawText(formatted,new Point(4,4));var rendered=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);rendered.Render(visual);a=new Annotation{Kind=AnnotationKind.ImageLayer,PixelPng=RasterPixels.Encode(rendered),X2=width,Y2=height,ImageStyle=new(){Opacity=opacity.Value/100}};Place(a);AddStudioObjects(a);dialog.Close();};
        var logo=new Button{Content="Thêm logo…"};form.Children.Add(logo);logo.Click+=(_,_)=>{var file=new OpenFileDialog{Filter="Logo|*.png;*.jpg;*.jpeg"};if(file.ShowDialog(dialog)!=true)return;var image=MediaStore.LoadImage(file.FileName);var a=new Annotation{Kind=AnnotationKind.ImageLayer,PixelPng=RasterPixels.Encode(image),X2=Math.Min(240,image.PixelWidth),Y2=Math.Min(240,image.PixelWidth)*image.PixelHeight/(double)image.PixelWidth,ImageStyle=new(){Opacity=opacity.Value/100}};Place(a);AddStudioObjects(a);dialog.Close();};dialog.ShowDialog();
    }
}
