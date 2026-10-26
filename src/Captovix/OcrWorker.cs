using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using Tesseract;
namespace Captovix;
internal static class OcrWorker
{
    // Isolated process: cancellation/timeout terminates native OCR without blocking the Editor.
    internal static int Run(string[] args)
    {
        if(args.Length!=4||args[0]!="--ocr-worker"||!Guid.TryParseExact(args[1],"N",out var job))return 2;
        string folder=Path.Combine(App.DataRoot,"OcrJobs",job.ToString("N"));
        string output=Path.Combine(folder,"result.json");
        try{
            string language=args[2];if(!int.TryParse(args[3],out int mode))return 2;
            string input=Path.Combine(folder,"input.png");
            if(!File.Exists(input)||new FileInfo(input).Length>100_000_000)throw new InvalidDataException("Thiếu hoặc vượt giới hạn ảnh OCR.");
            string data=Path.Combine(AppContext.BaseDirectory,"tessdata");
            using var manifest=JsonDocument.Parse(File.ReadAllText(Path.Combine(data,"models.json")));
            if(!OcrContract.Languages.Contains(language))throw new InvalidDataException("Ngôn ngữ không hỗ trợ.");
            foreach(var lang in language.Split('+')){
                string path=Path.Combine(data,lang+".traineddata");
                if(!File.Exists(path))throw new FileNotFoundException("Thiếu bộ nhận dạng "+lang+". Hãy cài lại Captovix.");
                using var stream=File.OpenRead(path);string hash=Convert.ToHexString(SHA256.HashData(stream));
                if(!hash.Equals(manifest.RootElement.GetProperty("models").GetProperty(lang).GetProperty("sha256").GetString(),StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Bộ nhận dạng "+lang+" bị thay đổi. Hãy cài lại Captovix.");
            }
            using(var png=File.OpenRead(input)){
                Span<byte> header=stackalloc byte[24];png.ReadExactly(header);
                if(!header[..8].SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}))throw new InvalidDataException("Ảnh OCR không phải PNG.");
                int w=System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.Slice(16,4)),h=System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.Slice(20,4));OcrContract.Validate(w,h,language,mode);
            }
            using var image=Pix.LoadFromFile(input);OcrContract.Validate(image.Width,image.Height,language,mode);
            using var engine=new TesseractEngine(data,language,EngineMode.LstmOnly);
            using var page=engine.Process(image,(PageSegMode)mode);
            var lines=new List<OcrTextLine>();using(var iterator=page.GetIterator()){iterator.Begin();do{if(lines.Count>=500)break;if(iterator.TryGetBoundingBox(PageIteratorLevel.TextLine,out var box)){string text=OcrContract.Normalize(iterator.GetText(PageIteratorLevel.TextLine)??"");if(text.Length>0)lines.Add(new(new(box.X1,box.Y1,box.Width,box.Height),text));}}while(iterator.Next(PageIteratorLevel.TextLine));}
            var result=new OcrResult(OcrContract.Normalize(page.GetText()),Math.Clamp(page.GetMeanConfidence(),0,1),Lines:lines.ToArray());
            File.WriteAllText(output,JsonSerializer.Serialize(result));return 0;
        }catch(Exception ex){
            try{File.WriteAllText(output,JsonSerializer.Serialize(new OcrResult(Error:"Chưa nhận dạng được chữ. Kiểm tra bộ nhận dạng và Visual C++ Runtime, hoặc cài lại Captovix. Chi tiết: "+ex.Message)));}catch{}
            return 1;
        }
    }
}
