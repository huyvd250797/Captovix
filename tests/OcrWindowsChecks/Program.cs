using Captovix;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tesseract;
internal static class Program
{
 [STAThread]static int Main(){string folder=Path.Combine(Path.GetTempPath(),"Captovix-ocr-test-"+Guid.NewGuid().ToString("N"));try{
  Directory.CreateDirectory(folder);int n=0;void Check(bool ok,string s){if(!ok)throw new Exception(s);n++;Console.WriteLine("PASS: "+s);}
  var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawRectangle(Brushes.White,null,new System.Windows.Rect(0,0,1200,180));dc.DrawText(new FormattedText("CAPTOVIX OCR TEST 2026",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),48,Brushes.Black,1),new Point(30,50));}
  var bitmap=new RenderTargetBitmap(1200,180,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();string image=Path.Combine(folder,"sample.png");ImageTransfer.Save(bitmap,image);var before=File.ReadAllBytes(image);
  foreach(string language in new[]{"eng","vie+eng"}){
   using var engine=new TesseractEngine(Path.Combine(AppContext.BaseDirectory,"tessdata"),language,EngineMode.LstmOnly);using var pix=Pix.LoadFromFile(image);using var page=engine.Process(pix,PageSegMode.SingleBlock);string text=page.GetText();Check(text.Contains("CAPTOVIX")&&text.Contains("2026"),"Real native recognition: "+language);Check(page.GetMeanConfidence() is >=0 and <=1,"Confidence is bounded");
  }
  Check(before.SequenceEqual(File.ReadAllBytes(image)),"Recognition never alters input PNG");
  var session=new EditorSession(new EditorDocument{OriginalWidth=1200,OriginalHeight=180});var surface=new EditorSurface();surface.Load(bitmap,session);surface.StartCrop();Check(surface.CropDraft==session.Document.Viewport,"Crop starts around the full current image");Check(!session.Dirty,"Starting crop is non-destructive");Check(surface.SetCropDraft(new(30,20,600,100))&&surface.ApplyCropDraft(),"Crop handles/numeric draft can apply");Check(surface.Flatten().PixelWidth==600&&surface.Flatten().PixelHeight==100,"Crop exports exact selected pixels");
  Console.WriteLine($"{n} Windows OCR/crop checks passed. Live IME/titlebar/cancel still require desktop acceptance.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}}
}
