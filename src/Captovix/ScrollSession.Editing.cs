using System.IO;
namespace Captovix;
public sealed partial class ScrollSession
{
    public void RemoveFrame(int index)
    {
        if(Count<=1||index<0||index>=Count)throw new InvalidOperationException("Cần giữ ít nhất một khung.");
        var revision=Path.Combine(Root,"revision-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(revision);var retained=Enumerable.Range(0,Count).Where(i=>i!=index).ToArray();
        for(int i=0;i<retained.Length;i++){Load(retained[i]);File.Copy(FramePath(retained[i]),Path.Combine(revision,$"frame-{i:D3}.png"));}
        Folder=revision;Count=retained.Length;Hashes=Enumerable.Range(0,Count).Select(i=>Hash(FramePath(i))).ToList();Joins=[];
        for(int i=1;i<Count;i++){var match=ScrollStitching.Match(Load(i-1),Load(i));Joins.Add(new(){Overlap=Math.Clamp(match.Overlap,1,WorkHeight-1),Uncertain=true,Message="Đã loại khung. Kiểm tra lại mối nối: "+match.Message});}Save();
    }
}
