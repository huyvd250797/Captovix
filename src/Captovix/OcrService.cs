using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
internal static class OcrService
{
    internal static async Task<OcrResult> RecognizeAsync(BitmapSource source,PixelRect region,string language,int mode,CancellationToken cancellation)
    {
        OcrContract.Validate(region.Width,region.Height,language,mode);
        if(region.X<0||region.Y<0||region.Right>source.PixelWidth||region.Bottom>source.PixelHeight)throw new ArgumentException("Vùng nằm ngoài ảnh.");
        string id=Guid.NewGuid().ToString("N"),folder=Path.Combine(App.DataRoot,"OcrJobs",id);Directory.CreateDirectory(folder);
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);deadline.CancelAfter(TimeSpan.FromSeconds(120));
        Process? process=null;
        try{
            // Only the rendered, visible snapshot is supplied. Never read hidden original pixels.
            await Task.Run(()=>{
                cancellation.ThrowIfCancellationRequested();var crop=new CroppedBitmap(source,new(region.X,region.Y,region.Width,region.Height));
                var rgb=ImageFidelity.WhiteRgb(ImageTransfer.Bgra(crop));var opaque=BitmapSource.Create(region.Width,region.Height,96,96,PixelFormats.Bgr24,null,rgb,checked(region.Width*3));opaque.Freeze();
                ImageTransfer.Save(opaque,Path.Combine(folder,"input.png"));
            },deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            var start=new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory,"Captovix.exe")){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=AppContext.BaseDirectory};
            foreach(string value in new[]{"--ocr-worker",id,language,mode.ToString(System.Globalization.CultureInfo.InvariantCulture)})start.ArgumentList.Add(value);
            process=Process.Start(start)??throw new IOException("Không mở được tiến trình OCR.");
            await process.WaitForExitAsync(deadline.Token);deadline.Token.ThrowIfCancellationRequested();
            string result=Path.Combine(folder,"result.json");if(!File.Exists(result)||new FileInfo(result).Length>4_000_000)throw new IOException("OCR không trả kết quả hợp lệ. Hãy cài lại Captovix nếu thiếu thư viện.");
            return JsonSerializer.Deserialize<OcrResult>(await File.ReadAllTextAsync(result,deadline.Token))??throw new IOException("Không đọc được kết quả OCR.");
        }catch(OperationCanceledException)when(!cancellation.IsCancellationRequested){throw new TimeoutException("OCR quá 120 giây. Chọn vùng nhỏ hơn rồi thử lại.");}
        finally{
            if(process!=null){try{if(!process.HasExited){process.Kill(true);await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));}}catch(Exception ex){App.Log(ex);}finally{process.Dispose();}}
            try{Directory.Delete(folder,true);}catch(Exception ex){App.Log(ex);}
        }
    }
}
