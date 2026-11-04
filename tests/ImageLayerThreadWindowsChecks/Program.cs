using Captovix;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program
{
 static int count;static void Check(bool ok,string text){if(!ok)throw new Exception(text);count++;Console.WriteLine("PASS: "+text);}
 [STAThread]static int Main(){var folder=Path.Combine(Path.GetTempPath(),"Captovix-thread-checks-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);MediaItem? item=null;
 try{
 var pixels=new byte[24*18*4];for(int i=0;i<24*18;i++){pixels[i*4]=(byte)(i*17);pixels[i*4+1]=(byte)(i*31);pixels[i*4+2]=(byte)(i*7);pixels[i*4+3]=(byte)(i%4*85);}var pasted=ImageTransfer.Bitmap(24,18,pixels);string data=RasterPixels.Encode(pasted);
 RasterPixels.ClearCache();var foreign=Task.Run(()=>RasterPixels.Decode(data)).GetAwaiter().GetResult();Check(foreign.IsFrozen&&foreign is not BitmapFrame,"Worker cache publishes detached frozen pixels, never decoder-backed BitmapFrame");Check(!foreign.IsDownloading&&foreign.PixelWidth==24&&foreign.PixelHeight==18,"UI can query image state/dimensions after worker decode");Check(ImageTransfer.Bgra(foreign).SequenceEqual(pixels),"Detached BGRA preserves alpha and color pixels");
 var cacheHits=RasterPixels.CacheStats.Hits;Check(ReferenceEquals(foreign,RasterPixels.Decode(data))&&RasterPixels.CacheStats.Hits>cacheHits,"Same PNG cache entry is reused safely across threads");
 var backgroundPng=StaImageWorker.Run(()=>{var encoded=ImageTransfer.Png(foreign);if(encoded.Length==0)throw new Exception("Empty background PNG");});backgroundPng.GetAwaiter().GetResult();Check(true,"STA encoder can reuse shared detached pixels");
 var basePixels=Enumerable.Repeat((byte)255,96*72*4).ToArray();var original=ImageTransfer.Bitmap(96,72,basePixels);string path=Path.Combine(folder,"photo1.png");ImageTransfer.Save(original,path);item=new MediaItem{Id=Guid.NewGuid().ToString("N"),FilePath=path};var session=DocumentStore.Load(item,original,out _);
 session.Change(d=>d.Annotations.Add(new(){Kind=AnnotationKind.ImageLayer,Name="Ảnh 2 dán vào ảnh 1",X1=13,Y1=11,X2=61,Y2=47,PixelPng=data,ImageStyle=new(){Mask=LayerMask.Rounded,Radius=4,Opacity=.8}}));
 var baseline=ImageTransfer.Bgra(EditorRenderer.Render(original,session.Document));var exactData=session.Document.Annotations[0].PixelPng;
 var snapshot=new EditorSession(session.Document.Snapshot());var savedItem=new MediaItem{Id=item.Id,FilePath=item.FilePath};StaImageWorker.Run(()=>{try{DocumentStore.Save(savedItem,snapshot,original);}finally{EditorRenderer.ReleaseImageCaches(original);}}).GetAwaiter().GetResult();Check(File.Exists(DocumentStore.EditPath(item.Id)),"Background autosave retains sidecar after pasting image");Check(savedItem.Thumbnail!=null&&savedItem.Thumbnail.IsFrozen&&!savedItem.Thumbnail.IsDownloading,"Thumbnail returned to UI is frozen and cross-thread safe");
 var sidecar=File.ReadAllText(DocumentStore.EditPath(item.Id));
 for(int i=0;i<25;i++){
  // Unload image1, display another image, and reopen image1 on the production MTA load path.
  var surface=new EditorSurface();surface.Load(original,session);surface.Load(null,null);surface.Load(pasted,new EditorSession(new(){OriginalWidth=24,OriginalHeight=18}));surface.Load(null,null);
  var loaded=Task.Run(()=>{var image=MediaStore.LoadImage(path);var editing=DocumentStore.Load(item,image,out var warning);if(warning!=null)throw new Exception(warning);return(image,editing);}).GetAwaiter().GetResult();
  string payload=loaded.editing.Document.Annotations.Single().PixelPng!;Check(payload==exactData,"Reopen preserves pasted PNG data: "+i);
  var raster=RasterPixels.Decode(payload);Check(raster.IsFrozen&&raster is not BitmapFrame&&!raster.IsDownloading,"UI consumes PNG validated on MTA safely: "+i);
  var group=new DrawingGroup();using(var dc=group.Open())EditorRenderer.Draw(dc,loaded.image,loaded.editing.Document);Check(group.CanFreeze,"UI scene drawing can freeze after background decode: "+i);group.Freeze();
  surface.Load(loaded.image,loaded.editing);Check(ImageTransfer.Bgra(surface.Flatten()).SequenceEqual(baseline),"Reopened composite pixels match saved image: "+i);
  var editing=loaded.editing;StaImageWorker.Run(()=>{try{var preview=EditorRenderer.Render(loaded.image,editing.Document.Snapshot(),240);if(!preview.IsFrozen)throw new Exception("Preview not frozen");}finally{EditorRenderer.ReleaseImageCaches(loaded.image);}}).GetAwaiter().GetResult();
  surface.Load(null,null);
 }
 Check(File.ReadAllText(DocumentStore.EditPath(item.Id))==sidecar,"Switch/render/cache cleanup does not mutate saved edits");
 Check(EditorDocument.Deserialize(sidecar).Annotations.Single().PixelPng==data,"Existing sidecar PNG data needs no migration or deletion");
 Console.WriteLine(count+" image layer thread Windows checks passed. Physical library/clipboard/IME/DPI acceptance remains separate.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}
 finally{StaImageWorker.Stop();if(item!=null){File.Delete(DocumentStore.EditPath(item.Id));File.Delete(DocumentStore.ThumbnailPath(item.Id));File.Delete(DocumentStore.EditPath(item.Id)+".bak");}Directory.Delete(folder,true);}
 }
}
