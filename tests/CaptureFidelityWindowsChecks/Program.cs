using Captovix;
using System.Windows;
using System.Windows.Media.Imaging;
internal static class Program
{
    [STAThread]static int Main(string[] args)
    {
        string dir=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Captovix130-"+Guid.NewGuid());System.IO.Directory.CreateDirectory(dir);
        try{
            var raw=new byte[37*23*4];new Random(130).NextBytes(raw);for(int i=3;i<raw.Length;i+=4)raw[i]=255;
            var source=ImageTransfer.Bitmap(37,23,raw);var session=new EditorSession(new(){OriginalWidth=37,OriginalHeight=23});var surface=new EditorSurface();surface.Load(source,session);
            foreach(var dpi in new[]{1d,1.25,1.5,1.75,2})foreach(var zoom in new[]{.125,.73,1d,1.9}){
                surface.SetDisplayScale(dpi);surface.SetZoom(zoom/dpi);var result=surface.Flatten();if(result.PixelWidth!=37||result.PixelHeight!=23||!ImageTransfer.Bgra(result).SequenceEqual(raw))throw new Exception("Zoom/DPI changed export pixels");
                var path=System.IO.Path.Combine(dir,"test.png");ImageTransfer.Save(result,path);var decoded=MediaStore.LoadImage(path);if(!ImageTransfer.Bgra(decoded).SequenceEqual(raw))throw new Exception("PNG roundtrip differs");
            }
            Console.WriteLine("PASS 20 Editor zoom/DPI combinations and PNG pixel roundtrips (synthetic source, not actual Windows monitor scale)");
            var photo=new FrozenFrame(new(-20,-10,37,23),raw);var a=photo.Crop(new(-15,-7,17,11));var b=photo.Crop(new(-15,-7,17,11));if(!a.SequenceEqual(b)||a.Length!=17*11*4)throw new Exception("Frozen crop differs");
            if(args.Contains("--clipboard")){
                ImageTransfer.Copy(source);if(Clipboard.GetData("PNG") is not System.IO.MemoryStream stream)throw new Exception("No PNG clipboard data");stream.Position=0;var decoded=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];if(!ImageTransfer.Bgra(decoded).SequenceEqual(raw))throw new Exception("Clipboard PNG differs");Console.WriteLine("PASS interactive clipboard pixel roundtrip");
            }else Console.WriteLine("PENDING interactive clipboard; run with --clipboard on a test desktop");
            Console.WriteLine("Real 100/125/150/175/200% monitor DPI, negative monitors, frozen notifications and installer remain manual acceptance.");return 0;
        }catch(Exception e){Console.Error.WriteLine(e);return 1;}finally{System.IO.Directory.Delete(dir,true);}
    }
}
