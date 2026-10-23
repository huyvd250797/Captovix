using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
namespace Captovix;
public partial class MainWindow
{
    private readonly StackPanel _imageLayerPanel=new(){Margin=new(0,4,0,10)};
    private readonly ComboBox _layerFit=new(){ItemsSource=new[]{"Vừa ảnh · giữ toàn bộ","Lấp đầy ô · cắt phần thừa"}};
    private readonly Slider _layerPanX=new(){Minimum=0,Maximum=1,TickFrequency=.01},_layerPanY=new(){Minimum=0,Maximum=1,TickFrequency=.01};
    private readonly TextBox _layerRadius=new(),_layerBorder=new();
    private readonly Button _layerColor=new(){Content="Màu viền"};
    private readonly CheckBox _panImageMode=new(){Content="Kéo ảnh bên trong ô"};
    private readonly CheckBox _layerShadow=new(){Content="Đổ bóng"};
    private bool _syncComposition;
    private MediaItem? _compositionDropTarget;
    private Point _libraryDragStart;
    private static Button ActionButton(string text,Action action){var b=new Button{Content=text};b.Click+=(_,_)=>action();return b;}
    private void InitializeComposition()
    {
        _imageLayerPanel.Children.Add(new TextBlock{Text="Lớp ảnh",FontWeight=FontWeights.SemiBold,FontSize=16});
        _imageLayerPanel.Children.Add(_layerFit);_imageLayerPanel.Children.Add(_panImageMode);_panImageMode.Checked+=(_,_)=>Surface.ContentPanEnabled=true;_panImageMode.Unchecked+=(_,_)=>Surface.ContentPanEnabled=false;_imageLayerPanel.Children.Add(new TextBlock{Text="Vị trí ảnh trong ô · ngang / dọc"});_imageLayerPanel.Children.Add(_layerPanX);_imageLayerPanel.Children.Add(_layerPanY);
        _imageLayerPanel.Children.Add(new TextBlock{Text="Bo góc · px"});_imageLayerPanel.Children.Add(_layerRadius);_imageLayerPanel.Children.Add(new TextBlock{Text="Dày viền · 0 = không viền"});_imageLayerPanel.Children.Add(_layerBorder);_imageLayerPanel.Children.Add(_layerColor);_imageLayerPanel.Children.Add(_layerShadow);
        _imageLayerPanel.Children.Add(ActionButton("Thay ảnh trong ô…",ReplaceLayerImage));_imageLayerPanel.Children.Add(ActionButton("Cắt riêng ảnh này…",CropLayerImage));_imageLayerPanel.Children.Add(ActionButton("Khôi phục vùng ảnh",()=>LayerChange(a=>(a.ImageStyle??=new()).Crop=null)));_imageLayerPanel.Children.Add(ActionButton("Đổi chỗ hai ảnh đã chọn",SwapImages));EditorProperties.Children.Insert(0,_imageLayerPanel);
        _layerFit.SelectionChanged+=(_,_)=>ApplyLayerControls();_layerPanX.ValueChanged+=(_,_)=>ApplyLayerControls();_layerPanY.ValueChanged+=(_,_)=>ApplyLayerControls();_layerShadow.Checked+=(_,_)=>ApplyLayerControls();_layerShadow.Unchecked+=(_,_)=>ApplyLayerControls();
        foreach(var text in new[]{_layerRadius,_layerBorder}){text.LostKeyboardFocus+=(_,_)=>ApplyLayerControls();text.KeyDown+=(_,e)=>{if(e.Key==Key.Enter)ApplyLayerControls();};}
        _layerColor.Click+=(_,_)=>OpenPaint(_layerColor,c=>{if(c!=null)LayerChange(a=>(a.ImageStyle??=new()).BorderColor=c);},false);
        EditorPane.AllowDrop=true;EditorPane.DragOver+=(_,e)=>{e.Effects=e.Data.GetDataPresent(DataFormats.FileDrop)||e.Data.GetDataPresent("Captovix.LibraryImage")?DragDropEffects.Copy:DragDropEffects.None;e.Handled=true;};EditorPane.Drop+=CompositionDrop;
        MediaList.PreviewMouseLeftButtonDown+=(_,e)=>{_libraryDragStart=e.GetPosition(MediaList);_compositionDropTarget=_activeItem;};
        MediaList.PreviewMouseMove+=(_,e)=>{if(e.LeftButton!=MouseButtonState.Pressed||Math.Abs(e.GetPosition(MediaList).X-_libraryDragStart.X)+Math.Abs(e.GetPosition(MediaList).Y-_libraryDragStart.Y)<12)return;if(MediaList.SelectedItem is MediaItem{IsVideo:false} item){var data=new DataObject("Captovix.LibraryImage",item);DragDrop.DoDragDrop(MediaList,data,DragDropEffects.Copy);}};
        SyncCompositionControls();
    }
    private Annotation? SelectedImage()=>_session?.Document.Annotations.Where(a=>Surface.SelectedIds.Contains(a.Id)&&a.Kind==AnnotationKind.ImageLayer).SingleOrDefaultSafe();
    private void SyncCompositionControls()
    {
        if(_imageLayerPanel.Parent==null)return;var a=SelectedImage();_imageLayerPanel.Visibility=a==null?Visibility.Collapsed:Visibility.Visible;if(a==null)return;_imageLayerPanel.IsEnabled=_session!=null&&ObjectControl.CanEdit(_session.Document,Surface.SelectedIds);_syncComposition=true;
        try{var style=a.ImageStyle??new();_layerFit.SelectedIndex=(int)style.Fit;_layerPanX.Value=style.PanX;_layerPanY.Value=style.PanY;_layerRadius.Text=ShapeNumber(style.Radius);_layerBorder.Text=ShapeNumber(style.Border);_layerShadow.IsChecked=a.Shadow.Enabled;PaintButton(_layerColor,"Viền",style.Border>0?style.BorderColor:null);}finally{_syncComposition=false;}
    }
    private void LayerChange(Action<Annotation> change){if(_session==null||SelectedImage() is not {} a||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return;try{EndInlineText(true);Surface.CancelInteraction();_session.Change(d=>{change(d.Annotations.First(x=>x.Id==a.Id));d.Validate(d.OriginalWidth,d.OriginalHeight,d.SourceHash);});Surface.Refresh();SyncProperties();}catch(Exception ex){SetStatus("Chưa chỉnh được lớp ảnh: "+ex.Message);}}
    private void ApplyLayerControls(){if(_syncComposition)return;try{double radius=ShapeValue(_layerRadius,"Bo góc",0,1000),border=ShapeValue(_layerBorder,"Viền",0,40);int fit=_layerFit.SelectedIndex;double x=_layerPanX.Value,y=_layerPanY.Value;bool shadow=_layerShadow.IsChecked==true;LayerChange(a=>{var s=a.ImageStyle??=new();s.Fit=(ImageFit)fit;s.PanX=x;s.PanY=y;s.Radius=radius;s.Border=border;a.Shadow.Enabled=shadow;});}catch(Exception ex){SetStatus(ex.Message);}}
    private void AddImages_Click(object sender,RoutedEventArgs e){var picker=new OpenFileDialog{Filter="Ảnh|*.png;*.jpg;*.jpeg;*.bmp",Multiselect=true,Title="Thêm ảnh vào bản ghép"};if(picker.ShowDialog(this)==true)ImportCompositionFiles(picker.FileNames);}
    private void ImportCompositionFiles(string[] paths)
    {
        try{if(_busy||!FlushEdits())return;if(paths.Length==0||paths.Length>16)throw new InvalidDataException("Thêm tối đa 16 ảnh mỗi lần.");
            var images=paths.Select(path=>{if(new FileInfo(path).Length>72_000_000)throw new InvalidDataException("File ảnh vượt 72 MB.");return(Name:Path.GetFileNameWithoutExtension(path),Image:(BitmapSource)MediaStore.LoadImage(path));}).ToArray();
            if(_session==null)CreateCompositionCanvas(new());AddCompositionImages(images);
        }catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Thêm ảnh",MessageBoxButton.OK,MessageBoxImage.Warning);}
    }
    private void AddCompositionImages(IEnumerable<(string Name,BitmapSource Image)> values)
    {
        if(_session==null)return;var layers=values.Select(v=>{var png=RasterPixels.Encode(v.Image);return new Annotation{Kind=AnnotationKind.ImageLayer,Name=v.Name[..Math.Min(80,v.Name.Length)],PixelPng=png,ImageStyle=new(),X1=40,Y1=40,X2=40+v.Image.PixelWidth,Y2=40+v.Image.PixelHeight};}).ToList();
        EndInlineText(true);Surface.CancelInteraction();var selected=new List<string>();_session.Change(d=>{foreach(var layer in layers){var placeholder=d.Annotations.FirstOrDefault(a=>a.Kind==AnnotationKind.ImageLayer&&a.ImagePlaceholder);if(placeholder!=null){if(!ObjectControl.CanEdit(d,[placeholder.Id]))throw new InvalidOperationException("Mở khóa ô ảnh trước khi điền ảnh vào mẫu.");placeholder.PixelPng=layer.PixelPng;placeholder.ImagePlaceholder=false;placeholder.Name=layer.Name;(placeholder.ImageStyle??=new()).Crop=null;selected.Add(placeholder.Id);}else{d.Annotations.Add(layer);selected.Add(layer.Id);}}d.Validate(d.OriginalWidth,d.OriginalHeight,d.SourceHash);});SelectTool(EditorTool.Select);Surface.SelectIds(selected);Surface.Refresh();SyncProperties();FitImage();SetStatus("Đã thêm lớp ảnh. Menu Bố cục → Sắp xếp ảnh để chọn mẫu ghép.");
    }
    private void CompositionDrop(object sender,DragEventArgs e)
    {
        e.Handled=true;try{if(_busy)return;if(e.Data.GetData("Captovix.LibraryImage") is MediaItem item){if(!FlushEdits())return;var original=MediaStore.LoadImage(item.FilePath);var doc=DocumentStore.Load(item,original,out _);var rendered=EditorRenderer.Render(original,doc.Document);if(_compositionDropTarget!=null&&_compositionDropTarget!=item)MediaList.SelectedItem=_compositionDropTarget;if(_session==null)CreateCompositionCanvas(new());AddCompositionImages([(item.Title,rendered)]);}
            else if(e.Data.GetData(DataFormats.FileDrop) is string[] files)ImportCompositionFiles(files);
        }catch(Exception ex){App.Log(ex);SetStatus("Không thêm được ảnh: "+ex.Message);}
    }
    private void CreateCompositionCanvas(CompositionOptions o)
    {
        o.Validate();var bitmap=ImageTransfer.Bitmap(o.Width,o.Height,new byte[checked(o.Width*o.Height*4)]);var path=_store.NewPath(false);File.WriteAllBytes(path,ImageTransfer.Png(bitmap));var item=new MediaItem{FilePath=path,Title="Bản ghép "+DateTime.Now.ToString("dd-MM HH:mm"),Width=o.Width,Height=o.Height,Source="Canvas ghép ảnh"};
        _store.Add(item);SearchBox.Clear();MediaList.SelectedItem=item;if(_session==null)throw new InvalidOperationException("Chưa mở được canvas.");_session.Change(d=>{d.Appearance.BackgroundColor=o.Background;d.Appearance.Transparent=false;});
    }
    private void NewComposition_Click(object sender,RoutedEventArgs e)=>CompositionOptionsDialog(true);
    private void LayoutComposition_Click(object sender,RoutedEventArgs e)=>CompositionOptionsDialog(false);
    private void CompositionOptionsDialog(bool create)
    {
        if(_busy||!FlushEdits())return;if(!create&&_session==null){SetStatus("Tạo bản ghép hoặc mở ảnh trước.");return;}
        var o=new CompositionOptions{Width=create?1600:_session!.Document.OriginalWidth,Height=create?1000:_session!.Document.OriginalHeight,Title=create?"":_session!.Document.Annotations.FirstOrDefault(a=>a.Name=="@composition:title")?.Text??"",Footer=create?"":_session!.Document.Annotations.FirstOrDefault(a=>a.Name=="@composition:footer")?.Text??""};var form=new StackPanel{Margin=new(18)};var dialog=FormDialog(create?"Tạo bản ghép mới":"Sắp xếp ảnh",form,600,660);
        var width=Input(form,"Rộng canvas · px",o.Width.ToString());var height=Input(form,"Cao canvas · px",o.Height.ToString());width.IsEnabled=height.IsEnabled=create;
        var margin=Input(form,"Đệm ngoài · px","40");var gap=Input(form,"Khoảng cách · px","24");var title=Input(form,"Tiêu đề",o.Title);var footer=Input(form,"Ghi chú cuối",o.Footer);var captions=new CheckBox{Content="Caption dưới mỗi ảnh",IsChecked=true,Margin=new(4,8,4,8)};form.Children.Add(captions);
        var background=new Button{Content="Nền canvas · trắng"};background.Click+=(_,_)=>OpenPaint(background,c=>{if(c!=null){o.Background=c;background.Content="Nền · "+c;}},false);form.Children.Add(background);
        var gallery=new System.Windows.Controls.Primitives.UniformGrid{Columns=3};string[] labels=["2 ảnh ngang","2 ảnh dọc","3 ảnh ngang","3 ảnh dọc","1 lớn + 2 nhỏ","Lưới 2 × 2","Tự do"];
        foreach(var layout in Enum.GetValues<CompositionLayout>()){var b=new Button{Content=new StackPanel(),Padding=new(6),Margin=new(3)};var row=(StackPanel)b.Content;row.Children.Add(LayoutPreview(layout));row.Children.Add(new TextBlock{Text=labels[(int)layout],TextAlignment=TextAlignment.Center,FontSize=11});b.Click+=(_,_)=>{o.Layout=layout;foreach(var other in gallery.Children.OfType<Button>())other.SetResourceReference(Control.BackgroundProperty,"Surface");b.SetResourceReference(Control.BackgroundProperty,"Soft");};gallery.Children.Add(b);}form.Children.Add(gallery);
        form.Children.Add(ActionButton(create?"Tạo canvas":"Áp dụng bố cục",()=>{try{o.Width=int.Parse(width.Text);o.Height=int.Parse(height.Text);o.Margin=int.Parse(margin.Text);o.Gap=int.Parse(gap.Text);o.Title=title.Text;o.Footer=footer.Text;o.Captions=captions.IsChecked==true;o.Validate();if(!create)CompositionStudio.Slots(o,_session!.Document.Annotations.Count(a=>a.Kind==AnnotationKind.ImageLayer));if(create){CreateCompositionCanvas(o);dialog.Close();AddImages_Click(this,new());if(_session?.Document.Annotations.Any(a=>a.Kind==AnnotationKind.ImageLayer)==true)_session.Change(d=>CompositionStudio.Apply(d,o));}else{Surface.CancelInteraction();_session!.Change(d=>CompositionStudio.Apply(d,o));dialog.Close();}Surface.Refresh();SyncProperties();FitImage();}catch(Exception ex){MessageBox.Show(dialog,ex.Message,"Bố cục",MessageBoxButton.OK,MessageBoxImage.Warning);}}));dialog.ShowDialog();
    }
    private static FrameworkElement LayoutPreview(CompositionLayout layout){var canvas=new Canvas{Width=104,Height=58};var o=new CompositionOptions{Width=1040,Height=580,Margin=20,Gap=20,Layout=layout,Captions=false};int count=layout is CompositionLayout.ThreeAcross or CompositionLayout.ThreeDown or CompositionLayout.LargeAndTwo?3:layout==CompositionLayout.Grid?4:2;foreach(var r in CompositionStudio.Slots(o,count)){var box=new Border{Width=r.Width/10,Height=r.Height/10,Background=new SolidColorBrush(Color.FromRgb(205,233,235)),BorderBrush=new SolidColorBrush(Color.FromRgb(0,139,146)),BorderThickness=new(1),CornerRadius=new(2)};Canvas.SetLeft(box,r.X/10);Canvas.SetTop(box,r.Y/10);canvas.Children.Add(box);}return canvas;}
    private Window FormDialog(string title,StackPanel content,double width=480,double height=530)=>new(){Owner=this,Title="Captovix · "+title,Width=Math.Min(width,SystemParameters.WorkArea.Width-32),Height=Math.Min(height,SystemParameters.WorkArea.Height-32),WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false,Content=new ScrollViewer{Content=content,VerticalScrollBarVisibility=ScrollBarVisibility.Auto}};
    private static TextBox Input(Panel form,string label,string value){form.Children.Add(new TextBlock{Text=label,Margin=new(4,5,4,0)});var input=new TextBox{Text=value,Margin=new(4,3,4,3)};form.Children.Add(input);return input;}
    private void ReplaceLayerImage(){if(SelectedImage()==null)return;var picker=new OpenFileDialog{Filter="Ảnh|*.png;*.jpg;*.jpeg;*.bmp"};if(picker.ShowDialog(this)!=true)return;try{var image=MediaStore.LoadImage(picker.FileName);var data=RasterPixels.Encode(image);LayerChange(a=>{a.PixelPng=data;a.ImagePlaceholder=false;(a.ImageStyle??=new()).Crop=null;});}catch(Exception ex){SetStatus(ex.Message);}}
    private void CropLayerImage(){if(SelectedImage() is not {} a||a.PixelPng==null)return;var image=RasterPixels.Decode(a.PixelPng);var crop=a.ImageStyle?.Crop??new(0,0,image.PixelWidth,image.PixelHeight);var form=new StackPanel{Margin=new(18)};var dialog=FormDialog("Cắt lớp ảnh · pixel nguồn",form);var x=Input(form,"X",crop.X.ToString());var y=Input(form,"Y",crop.Y.ToString());var w=Input(form,"Rộng",crop.Width.ToString());var h=Input(form,"Cao",crop.Height.ToString());form.Children.Add(ActionButton("Cắt ảnh · giữ nguồn",()=>{try{var c=new PixelRect(int.Parse(x.Text),int.Parse(y.Text),int.Parse(w.Text),int.Parse(h.Text));new LayerImageStyle{Crop=c}.Validate(image.PixelWidth,image.PixelHeight);LayerChange(item=>(item.ImageStyle??=new()).Crop=c);dialog.Close();}catch(Exception ex){MessageBox.Show(dialog,ex.Message);}}));dialog.ShowDialog();}
    private void SwapImages_Click(object sender,RoutedEventArgs e)=>SwapImages();
    private void SwapImages(){if(_session==null)return;var images=_session.Document.Annotations.Where(a=>Surface.SelectedIds.Contains(a.Id)&&a.Kind==AnnotationKind.ImageLayer).ToArray();if(images.Length!=2||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)){SetStatus("Ctrl+click chọn đúng hai lớp ảnh (hoặc hai nhóm ảnh + caption).");return;}_session.Change(d=>{var a=images[0];var b=images[1];(a.PixelPng,b.PixelPng)=(b.PixelPng,a.PixelPng);(a.ImageStyle,b.ImageStyle)=(b.ImageStyle,a.ImageStyle);(a.Name,b.Name)=(b.Name,a.Name);});Surface.Refresh();SyncProperties();}
    private void SaveTemplate_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||!FlushEdits())return;try{var template=CompositionTemplate.From(_session.Document);string json=JsonSerializer.Serialize(template,EditorDocument.JsonOptions);if(System.Text.Encoding.UTF8.GetByteCount(json)>4_000_000)throw new InvalidDataException("Mẫu bố cục vượt giới hạn 4 MB.");var picker=new SaveFileDialog{Filter="Mẫu Captovix|*.cvtemplate",FileName="Mau-bo-cuc.cvtemplate"};if(picker.ShowDialog(this)==true)MediaStore.AtomicWrite(picker.FileName,json);}catch(Exception ex){SetStatus("Không lưu được mẫu: "+ex.Message);}
    }
    private void OpenTemplate_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||!FlushEdits())return;var picker=new OpenFileDialog{Filter="Mẫu Captovix|*.cvtemplate"};if(picker.ShowDialog(this)!=true)return;try{if(new FileInfo(picker.FileName).Length>4_000_000)throw new InvalidDataException("Mẫu quá lớn.");var template=JsonSerializer.Deserialize<CompositionTemplate>(File.ReadAllText(picker.FileName),EditorDocument.JsonOptions)??throw new InvalidDataException("Mẫu trống.");template.Validate();var marker=RasterPixels.Encode(ImageTransfer.Bitmap(2,2,new byte[16]));var doc=new EditorDocument{OriginalWidth=template.Width,OriginalHeight=template.Height,Crop=template.Crop,Appearance=template.Appearance,Annotations=template.Objects};foreach(var a in doc.Annotations.Where(a=>a.Kind==AnnotationKind.ImageLayer)){a.PixelPng=marker;(a.ImageStyle??=new()).Crop=null;}doc.Validate(template.Width,template.Height,"");CreateCompositionCanvas(new(){Width=template.Width,Height=template.Height});_session!.Change(d=>{d.Annotations=doc.Annotations;d.Appearance=doc.Appearance;d.Crop=doc.Crop;});Surface.Refresh();SyncProperties();SetStatus("Đã mở mẫu. Thêm ảnh sẽ điền lần lượt các ô; hoặc chọn ô trong Layers → Thay ảnh.");}catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Mở mẫu");}
    }
    private void SaveProject_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||Surface.Original==null||!FlushEdits())return;var picker=new SaveFileDialog{Filter="Dự án Captovix|*.cvproject",FileName="Captovix-project.cvproject"};if(picker.ShowDialog(this)!=true)return;try{CompositionProject.Save(picker.FileName,ImageTransfer.Png(Surface.Original),_session.Document);SetStatus("Đã lưu dự án kèm toàn bộ ảnh và đối tượng.");}catch(Exception ex){SetStatus("Chưa lưu được dự án: "+ex.Message);}
    }
    private void OpenProject_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||!FlushEdits())return;var picker=new OpenFileDialog{Filter="Dự án Captovix|*.cvproject"};if(picker.ShowDialog(this)!=true)return;
        try{var project=CompositionProject.Load(picker.FileName);var png=project.Original;var doc=project.Document;var original=RasterPixels.Decode(Convert.ToBase64String(png));RasterPixels.ValidateDecoded(doc);
            var path=_store.NewPath(false);File.WriteAllBytes(path,png);var item=new MediaItem{FilePath=path,Title=Path.GetFileNameWithoutExtension(picker.FileName),Width=original.PixelWidth,Height=original.PixelHeight,Source="Dự án Captovix"};DocumentStore.Save(item,new(doc),original);_store.Add(item);SearchBox.Clear();MediaList.SelectedItem=item;SetStatus("Đã mở dự án cùng ảnh nguồn và đối tượng.");
        }catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Mở dự án",MessageBoxButton.OK,MessageBoxImage.Warning);}
    }
}
internal static class CompositionEnumerable
{
    public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> values) where T:class{var a=values.Take(2).ToArray();return a.Length==1?a[0]:null;}
}
