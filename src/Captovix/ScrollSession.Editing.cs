using System.IO;
namespace Captovix;
public sealed partial class ScrollSession
{
    public void RemoveFrame(int index)
    {
        if(Count<=1||index<0||index>=Count)throw new InvalidOperationException("Cần giữ ít nhất một khung.");
        var revision=Path.Combine(Root,"revision-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(revision);var retained=Enumerable.Range(0,Count).Where(i=>i!=index).ToArray();
        for(int i=0;i<retained.Length;i++){Load(retained[i]);File.Copy(FramePath(retained[i]),Path.Combine(revision,$"frame-{i:D3}.png"));}
        var next=new ScrollSession{Schema=Schema,Horizontal=Horizontal,Header=Header,Footer=Footer,KeepHeader=KeepHeader,Timing=Timing,Width=Width,Height=Height,Count=retained.Length,Topology=Topology,Region=Region,Folder=revision};
        next.Hashes=Enumerable.Range(0,next.Count).Select(i=>Hash(next.FramePath(i))).ToList();
        for(int i=1;i<next.Count;i++){var match=ScrollStitching.Match(next.Load(i-1),next.Load(i));next.Joins.Add(new(){Overlap=Math.Clamp(match.Overlap,1,WorkHeight-1),Uncertain=true,Message="Đã loại khung. Kiểm tra lại mối nối: "+match.Message});}
        // Only switch the active revision after all copies, joins and the manifest commit succeed.
        next.Save();Folder=next.Folder;Count=next.Count;Hashes=next.Hashes;Joins=next.Joins;
    }
}
