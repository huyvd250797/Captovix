using Captovix;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program
{
 static int count;static void Check(bool ok,string message){if(!ok)throw new Exception(message);count++;Console.WriteLine("PASS: "+message);}
 [STAThread]static int Main(){MediaItem? item=null;var folder=Path.Combine(Path.GetTempPath(),"Captovix-mockup-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
 try{
 const int w=320,h=180;var pixels=Enumerable.Repeat((byte)255,w*h*4).ToArray();var area=new PixelRect(30,40,140,40);
 // Black simulated old glyphs on a flat white form, with untouched nearby border.
 for(int y=48;y<72;y++)for(int x=40;x<155;x++)if(x%8<3){int n=(y*w+x)*4;pixels[n]=pixels[n+1]=pixels[n+2]=0;}
 var original=ImageTransfer.Bitmap(w,h,pixels);var d=new EditorDocument{OriginalWidth=w,OriginalHeight=h};var session=new EditorSession(d);var a=MockupStudio.Create(area,new(0,0,w,h),new("#FFFFFF","#D94155","#FFFFFF",1));a.Mockup!.Type=MockupKind.Button;a.Text="";a.CornerRadius=0;
 var before=EditorRenderer.Render(original,d);session.Change(doc=>MockupStudio.Apply(doc,a));var covered=ImageTransfer.Bgra(EditorRenderer.Render(original,d));
 bool clean=true,outside=true;for(int y=0;y<h;y++)for(int x=0;x<w;x++){int n=(y*w+x)*4;bool inside=x>=area.X&&x<area.Right&&y>=area.Y&&y<area.Bottom;if(inside)clean &= covered[n]==85&&covered[n+1]==65&&covered[n+2]==217;else for(int c=0;c<4;c++)outside &= covered[n+c]==pixels[n+c];}
 Check(clean,"Opaque replacement covers every old glyph in the selected flat region");Check(outside,"Pixels outside the selected region remain unchanged");Check(ImageTransfer.Bgra(original).SequenceEqual(pixels),"Original image pixels are immutable");
 session.Undo();Check(ImageTransfer.Bgra(EditorRenderer.Render(original,session.Document)).SequenceEqual(ImageTransfer.Bgra(before)),"Undo restores exact original rendered form");session.Redo();Check(ImageTransfer.Bgra(EditorRenderer.Render(original,session.Document)).SequenceEqual(covered),"Redo restores exact replacement pixels");
 var edited=session.Document.Annotations[0].Copy();edited.Text="Tìm kiếm mới";edited.FontSize=18;edited.TextPadding=3;edited.TextAlignment=TextAlign.Center;edited.CornerRadius=8;edited.StrokeEnabled=true;edited.Color="#26364B";edited.Stroke=2;session.Change(doc=>MockupStudio.Apply(doc,edited,edited.Id));var expected=ImageTransfer.Bgra(EditorRenderer.Render(original,session.Document));Check(!expected.SequenceEqual(covered),"Vietnamese text, radius and border affect renderer");
 BitmapSource? snapshot=null;var frozen=StaImageWorker.Run(()=>snapshot=EditorRenderer.MockupSnapshot(original,session.Document.Snapshot(),new(26,36,148,48)));frozen.GetAwaiter().GetResult();Check(snapshot!.IsFrozen&&snapshot is not BitmapFrame&&!snapshot.IsDownloading,"Worker mockup snapshot is detached and cross-thread readable");
 var preview=EditorRenderer.MockupPreview(snapshot!,new(26,36,148,48),edited);Check(preview.IsFrozen&&preview.PixelWidth<=900&&preview.PixelHeight<=480,"Live preview is bounded and frozen");
 string imagePath=Path.Combine(folder,"form.png");ImageTransfer.Save(original,imagePath);item=new(){Id=Guid.NewGuid().ToString("N"),FilePath=imagePath};var loaded=DocumentStore.Load(item,original,out _);DocumentStore.Save(item,loaded,original);var oldBytes=File.ReadAllBytes(DocumentStore.EditPath(item.Id));loaded.Change(doc=>MockupStudio.Apply(doc,edited));DocumentStore.Save(item,loaded,original);Check(File.ReadAllBytes(DocumentStore.EditPath(item.Id)+".pre-v1.38.bak").SequenceEqual(oldBytes),"First schema18 save preserves exact pre-upgrade sidecar");
 var reopened=Task.Run(()=>DocumentStore.Load(item,original,out _)).GetAwaiter().GetResult();Check(reopened.Document.Annotations.Single().Mockup!=null&&ImageTransfer.Bgra(EditorRenderer.Render(original,reopened.Document)).SequenceEqual(expected),"Background sidecar reload preserves editable text/button pixels");
 string project=Path.Combine(folder,"form.captovix");var png=ImageTransfer.Png(original);CompositionProject.Save(project,png,session.Document);var unpacked=CompositionProject.Load(project);Check(unpacked.Original.SequenceEqual(png)&&ImageTransfer.Bgra(EditorRenderer.Render(original,unpacked.Document)).SequenceEqual(expected),"Portable project keeps original PNG and editable mockup");
 var surface=new EditorSurface{Tool=EditorTool.Mockup,SnapEnabled=false};surface.Load(original,new EditorSession(new(){OriginalWidth=w,OriginalHeight=h}));surface.SetZoom(1);PixelRect? requested=null;surface.MockupRequested+=r=>requested=r;surface.PointerDown(new(25,35));surface.PointerUp(new(25,35));Check(requested==null&&!surface.Session!.CanUndo&&surface.Session.Document.Annotations.Count==0,"Single click creates no mockup, history or dialog");
 surface.PointerDown(new(25,35));surface.PointerMove(new(170,80));surface.PointerUp(new(170,80));Check(requested==new PixelRect(25,35,145,45)&&surface.Tool==EditorTool.Mockup&&!surface.Session!.CanUndo,"Drag requests bounded region, retains tool and waits for Apply");
 surface.Load(original,session);surface.Tool=EditorTool.Select;surface.SelectIds([edited.Id]);surface.PointerDown(new(60,60));surface.PointerMove(new(200,130));surface.PointerUp(new(200,130));Check(session.Document.Annotations.Single().Mockup!.SourceBounds==area,"Moving editable object leaves original erasure anchored");
 Console.WriteLine(count+" mockup Windows render/persistence/interaction checks passed. Physical IME/OCR/installer acceptance remains separate.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
 finally{StaImageWorker.Stop();if(item!=null)foreach(var p in new[]{DocumentStore.EditPath(item.Id),DocumentStore.ThumbnailPath(item.Id),DocumentStore.EditPath(item.Id)+".bak",DocumentStore.EditPath(item.Id)+".pre-v1.38.bak",DocumentStore.EditPath(item.Id)+".pre-v1.13.bak"})File.Delete(p);Directory.Delete(folder,true);}
 }
}
