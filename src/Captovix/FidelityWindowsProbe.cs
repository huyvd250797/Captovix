using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;

public sealed record FidelityResult(string Name,bool Passed,string Detail);
public static class FidelityWindowsProbe
{
    private static BitmapSource Pattern(int width=256,int height=192)
    {
        var pixels=new byte[width*height*4];for(int y=0;y<height;y++)for(int x=0;x<width;x++){
            int p=(y*width+x)*4;pixels[p]=(byte)((x%8<4)?24:224);pixels[p+1]=(byte)((y%8<4)?47:207);pixels[p+2]=(byte)((x+y)%2==0?63:191);pixels[p+3]=255;
        }return ImageTransfer.Bitmap(width,height,pixels);
    }
    public static List<FidelityResult> CheckRendering(string folder,bool clipboard=false)
    {
        Directory.CreateDirectory(folder);var results=new List<FidelityResult>();
        void Check(string name,Action test){try{test();results.Add(new(name,true,"Passed on this Windows run"));}catch(Exception ex){results.Add(new(name,false,ex.Message));}}
        void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
        var original=Pattern(17,11);var originalBytes=ImageTransfer.Bgra(original);
        // Non-96 file DPI is intentionally unrelated to file pixel dimensions.
        var source=BitmapSource.Create(17,11,300,144,PixelFormats.Bgra32,null,originalBytes,17*4);source.Freeze();
        for(int q=0;q<4;q++)for(int fx=0;fx<2;fx++)for(int fy=0;fy<2;fy++){
            var doc=new EditorDocument{OriginalWidth=17,OriginalHeight=11,Crop=new(1,2,13,7),Appearance=new(){QuarterTurns=q,FlipHorizontal=fx==1,FlipVertical=fy==1}};
            Check($"PNG crop/rotation/flip {q}/{fx}/{fy}",()=>{
                var output=EditorRenderer.Render(source,doc);var layout=new ImageLayout(doc.Viewport,doc.Appearance);var expected=ImageFidelity.CopyPixels(originalBytes,17,11,doc.Viewport,layout);
                Require(ImageTransfer.Bgra(output).SequenceEqual(expected),"Rendered pixels differ from exact transform");
                var path=Path.Combine(folder,"renderer-last.png");ImageTransfer.Save(output,path);Require(ImageTransfer.Bgra(MediaStore.LoadImage(path)).SequenceEqual(expected),"PNG codec changed output pixels");
            });
        }
        Check("Transparent BGRA roundtrip",()=>{
            byte[] bytes=[17,29,89,0,9,47,81,1,39,77,121,67,101,29,199,128,12,31,67,255];var image=ImageTransfer.Bitmap(5,1,bytes);var doc=new EditorDocument{OriginalWidth=5,OriginalHeight=1};var output=EditorRenderer.Render(image,doc);
            var path=Path.Combine(folder,"alpha.png");ImageTransfer.Save(output,path);Require(ImageTransfer.Bgra(MediaStore.LoadImage(path)).SequenceEqual(bytes),"Transparent PNG pixels changed");
        });
        Check("Untouched pixels with a redaction overlay",()=>{
            var doc=new EditorDocument{OriginalWidth=17,OriginalHeight=11};doc.Annotations.Add(new(){Kind=AnnotationKind.Cover,X1=4,Y1=3,X2=10,Y2=8,Color="#123456"});var pixels=ImageTransfer.Bgra(EditorRenderer.Render(source,doc));
            foreach(var pos in new[]{0,(10*17+16)*4,(10*17)*4,16*4})Require(pixels.AsSpan(pos,4).SequenceEqual(originalBytes.AsSpan(pos,4)),"Unedited source corner was changed");int inside=(5*17+6)*4;Require(pixels[inside]==0x56&&pixels[inside+1]==0x34&&pixels[inside+2]==0x12&&pixels[inside+3]==255,"Solid cover not present in exported pixels");
        });
        Check("Odd canvas center never resamples source",()=>{
            var doc=new EditorDocument{OriginalWidth=17,OriginalHeight=11,Appearance=new(){CanvasWidth=20,CanvasHeight=14,Padding=2,BorderWidth=1}};var layout=new ImageLayout(doc.Viewport,doc.Appearance);var output=ImageTransfer.Bgra(EditorRenderer.Render(source,doc));var at=layout.ToOutput(.5,.5);int pixel=((int)Math.Floor(at.Y)*layout.Width+(int)Math.Floor(at.X))*4;
            Require(output.AsSpan(pixel,4).SequenceEqual(originalBytes.AsSpan(0,4)),"Odd canvas alignment interpolated source");
        });
        Check("Zoom and model DPI cannot affect PNG dimensions or pixels",()=>{
            var doc=new EditorDocument{OriginalWidth=17,OriginalHeight=11};var surface=new EditorSurface();surface.Load(source,new(doc));
            foreach(var dpi in new[]{1d,1.25,1.5,1.75,2})foreach(var zoom in new[]{.25,.75,1,2}){surface.SetDisplayScale(dpi);surface.SetZoom(zoom);var rendered=surface.Flatten();Require(rendered.PixelWidth==17&&rendered.PixelHeight==11&&ImageTransfer.Bgra(rendered).SequenceEqual(originalBytes),"Viewport leaked into output");}
        });
        Check("Explicit resize is honored, never confused with zoom",()=>{var doc=new EditorDocument{OriginalWidth=17,OriginalHeight=11,Appearance=new(){ResizeWidth=34,ResizeHeight=22}};var output=EditorRenderer.Render(source,doc);Require(output.PixelWidth==34&&output.PixelHeight==22,"Explicit resize dimension mismatch");});
        Check("JPEG uses requested dimensions and white matte",()=>{byte[] bytes=[70,50,30,0,30,40,50,255];var image=ImageTransfer.Bitmap(2,1,bytes);var path=Path.Combine(folder,"matte.jpg");ImageTransfer.Save(image,path,true);var decoded=MediaStore.LoadImage(path);Require(decoded.PixelWidth==2&&decoded.PixelHeight==1,"JPEG changed dimensions");});
        if(clipboard)Check("Clipboard PNG dimensions and pixels",()=>{
            ImageTransfer.Copy(source);if(Clipboard.GetData("PNG") is not MemoryStream stream)throw new IOException("Clipboard consumer did not return the PNG stream");stream.Position=0;var frame=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];Require(frame.PixelWidth==17&&frame.PixelHeight==11&&Math.Abs(frame.DpiX-96)<.01&&Math.Abs(frame.DpiY-96)<.01&&ImageTransfer.Bgra(frame).SequenceEqual(originalBytes),"Clipboard PNG differs from rendered source");
        });
        File.WriteAllText(Path.Combine(folder,"renderer-results.json"),JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));return results;
    }
    public static int RunInteractive()
    {
        if(MessageBox.Show("Kiểm chứng ảnh trên Windows này sẽ hiển thị mẫu, chụp thử trên từng màn hình và kiểm tra render/PNG/clipboard. Clipboard sẽ được thay bằng ảnh mẫu. Kết quả chỉ lưu trên máy.\n\nHãy tắt HDR trên các màn hình trước khi chạy. Chỉ tiếp tục khi đang dùng SDR. Esc để hủy trong lúc thử.\n\nBắt đầu?","Captovix · Kiểm chứng SDR",MessageBoxButton.YesNo,MessageBoxImage.Information)!=MessageBoxResult.Yes)return 0;
        var folder=Path.Combine(App.DataRoot,"FidelityChecks",DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")[..6]);Directory.CreateDirectory(folder);
        var results=new List<FidelityResult>();var observed=new List<object>();var application=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};var image=new Image{Stretch=Stretch.Fill,SnapsToDevicePixels=true};RenderOptions.SetBitmapScalingMode(image,BitmapScalingMode.NearestNeighbor);
        var window=new Window{Title="Captovix · Kiểm chứng pixel · Esc hủy",Content=image,Width=400,Height=320,Topmost=true,UseLayoutRounding=true,WindowStyle=WindowStyle.ToolWindow};using var cancel=new CancellationTokenSource();bool running=true;window.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape)cancel.Cancel();};window.Closing+=(_,e)=>{if(running){e.Cancel=true;cancel.Cancel();}};
        int code=1;bool started=false;window.ContentRendered+=async (_,_)=>{
            if(started)return;started=true;
            try{
                results.AddRange(CheckRendering(folder,true));var displays=Native.Displays();var topology=CaptureWorkflowStore.Topology(displays.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity)));var sample=Pattern();image.Source=sample;
                for(int index=0;index<displays.Count;index++){
                    var display=displays[index];cancel.Token.ThrowIfCancellationRequested();
                    try{
                        if(display.Bounds.Width<400||display.Bounds.Height<320)throw new InvalidOperationException("Màn hình quá nhỏ để đặt mẫu kiểm tra.");
                        Native.SetWindowPos(new WindowInteropHelper(window).Handle,(nint)(-1),display.Bounds.X+20,display.Bounds.Y+20,400,320,0x40);await Task.Delay(500,cancel.Token);
                        var dpi=VisualTreeHelper.GetDpi(window);image.Width=sample.PixelWidth/dpi.DpiScaleX;image.Height=sample.PixelHeight/dpi.DpiScaleY;window.UpdateLayout();await Task.Delay(300,cancel.Token);
                        var point=image.PointToScreen(new Point(0,0));var rect=new PixelRect((int)Math.Round(point.X),(int)Math.Round(point.Y),sample.PixelWidth,sample.PixelHeight);
                        var current=Native.Displays();if(CaptureWorkflowStore.Topology(current.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity)))!=topology)throw new InvalidOperationException("Bố trí/DPI màn hình thay đổi trong lượt thử; chạy lại sau khi ổn định.");
                        var path=Path.Combine(folder,$"pattern-{index}.png");var capture=await Capture(rect,current,path,cancel.Token);RequirePattern(capture,sample);results.Add(new("Live physical pattern "+display.Device,true,$"{capture.PixelWidth}×{capture.PixelHeight} at {rect.X},{rect.Y}; window DPI {dpi.DpiScaleX*100:0.#}%"));
                        // Regress odd source width/height independently and together.
                        foreach(var size in new[]{(255,192),(256,191),(255,191),(1,1)}){
                            var oddRect=rect with{Width=size.Item1,Height=size.Item2};
                            var expected=new CroppedBitmap(sample,new Int32Rect(0,0,size.Item1,size.Item2));expected.Freeze();
                            var odd=await Capture(oddRect,current,Path.Combine(folder,$"odd-{index}-{size.Item1}x{size.Item2}.png"),cancel.Token);
                            RequirePattern(odd,expected);results.Add(new("Odd/one-pixel region "+display.Device,true,$"{size.Item1}×{size.Item2}; every BGRA pixel matched"));
                        }
                        if(display.Bounds.Width>=1467&&display.Bounds.Height>=386){
                            var reported=new PixelRect(display.Bounds.X,display.Bounds.Y,1467,386);
                            var result=await Capture(reported,current,Path.Combine(folder,$"regression-1467x386-{index}.png"),cancel.Token);
                            ImageFidelity.ValidateCapture(reported,result.PixelWidth,result.PixelHeight,false);
                            results.Add(new("Reported 1467×386 regression "+display.Device,true,"Exact output dimensions; no resampling"));
                        }
                        var fullPath=Path.Combine(folder,$"desktop-{index}.pending.png");try{var full=await Capture(display.Bounds,current,fullPath,cancel.Token);ImageFidelity.ValidateCapture(display.Bounds,full.PixelWidth,full.PixelHeight,false);results.Add(new("Full-display dimensions "+display.Device,true,$"{full.PixelWidth}×{full.PixelHeight}"));}finally{if(File.Exists(fullPath))File.Delete(fullPath);}
                        var hwnd=new WindowInteropHelper(window).Handle;if(!Native.GetWindowRect(hwnd,out var nativeRect))throw new InvalidOperationException("Không đọc được native window bounds.");
                        var windowCapture=await Capture(nativeRect.Pixels,current,Path.Combine(folder,$"window-{index}.png"),cancel.Token,new(hwnd,window.Title,(uint)Environment.ProcessId));
                        ImageFidelity.ValidateCapture(nativeRect.Pixels,windowCapture.PixelWidth,windowCapture.PixelHeight,true);
                        if(!ContainsPattern(windowCapture,sample))throw new InvalidOperationException("Ảnh cửa sổ không chứa nguyên mẫu pixel 1:1; kiểm tra window backend/DPI/màu.");
                        results.Add(new("Native-window pixel pattern "+display.Device,true,$"Native output {windowCapture.PixelWidth}×{windowCapture.PixelHeight}; fixture pixels matched without resizing"));
                        observed.Add(new{display.Device,display.Bounds,display.DpiX,display.DpiY,WindowDpiX=dpi.PixelsPerInchX,WindowDpiY=dpi.PixelsPerInchY,PatternRegion=rect,Passed=true});
                    }catch(OperationCanceledException){throw;}catch(Exception ex){App.Log(ex);results.Add(new("Live capture "+display.Device,false,ex.Message));observed.Add(new{display.Device,display.Bounds,display.DpiX,display.DpiY,Passed=false});}
                }
                code=results.All(r=>r.Passed)&&displays.Count>0?0:1;
            }catch(Exception ex){App.Log(ex);results.Add(new("Probe run",false,ex.Message));}
            finally{
                try{File.WriteAllText(Path.Combine(folder,"report.json"),JsonSerializer.Serialize(new{Version=VersionInfo.Current,Time=DateTimeOffset.Now,OS=Environment.OSVersion.ToString(),SDR="Confirmed by user; HDR not programmatically assessed",RequiredScales=new[]{100,125,150,175,200},Note="Each report proves only monitors/scales observed in that run. Repeat at missing scale values. Moving the Editor itself still requires the checklist.",ObservedDisplays=observed,Results=results,PassedThisRun=code==0},new JsonSerializerOptions{WriteIndented=true}));}catch(Exception ex){App.Log(ex);code=1;}
                running=false;window.Close();application.Shutdown();
            }
        };
        application.Run(window);MessageBox.Show((code==0?"Các phép thử của lượt này đã qua.":"Có phép thử chưa qua hoặc đã hủy.")+"\nKết quả: "+folder+"\n\nLượt thử này không chứng minh đủ 5 mức scale. Đổi scale trong Windows rồi chạy lại theo checklist.","Captovix · Kết quả SDR");return code;
    }
    private static void RequirePattern(BitmapSource actual,BitmapSource expected)
    {
        if(actual.PixelWidth!=expected.PixelWidth||actual.PixelHeight!=expected.PixelHeight||!ImageTransfer.Bgra(actual).SequenceEqual(ImageTransfer.Bgra(expected)))throw new InvalidOperationException("Pixel mẫu chụp không khớp tuyệt đối. Kiểm tra HDR, profile màu, scale/layout và ảnh pattern đã lưu; không tự đánh dấu đạt.");
    }
    private static bool ContainsPattern(BitmapSource image,BitmapSource pattern)
    {
        if(image.PixelWidth<pattern.PixelWidth||image.PixelHeight<pattern.PixelHeight)return false;
        var haystack=ImageTransfer.Bgra(image);var needle=ImageTransfer.Bgra(pattern);int stride=image.PixelWidth*4,rowBytes=pattern.PixelWidth*4;
        for(int y=0;y<=image.PixelHeight-pattern.PixelHeight;y++)for(int x=0;x<=image.PixelWidth-pattern.PixelWidth;x++){
            int at=y*stride+x*4;if(!haystack.AsSpan(at,rowBytes).SequenceEqual(needle.AsSpan(0,rowBytes)))continue;bool equal=true;
            for(int row=1;row<pattern.PixelHeight;row++)if(!haystack.AsSpan(at+row*stride,rowBytes).SequenceEqual(needle.AsSpan(row*rowBytes,rowBytes))){equal=false;break;}
            if(equal)return true;
        }return false;
    }
    private static async Task<BitmapSource> Capture(PixelRect region,IReadOnlyList<DisplayInfo> displays,string path,CancellationToken cancellation,WindowInfo? window=null)
    {
        using var engine=new CaptureEngine();try{await engine.StartAsync(new(false,region,window,displays,false,false,false,30,false),path).WaitAsync(cancellation);var actual=await engine.Completion.WaitAsync(TimeSpan.FromSeconds(20),cancellation);return MediaStore.LoadImage(actual);}finally{engine.Stop();}
    }
}
