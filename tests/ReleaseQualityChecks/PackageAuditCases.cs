using Captovix;
using System.IO.Compression;
static class PackageAuditCases
{
    public static void Run(string package)
    {
        ReleasePackageAudit.Validate(package,"HuyVo.Captovix","1.46.1");
        Console.WriteLine("PASS Production package passes release quality audit");
        void Reject(Action action,string name){try{action();throw new Exception("Not rejected: "+name);}catch(InvalidDataException){Console.WriteLine("PASS "+name);}}
        Reject(()=>ReleasePackageAudit.Validate(package,"HuyVo.Other","1.46.1"),"Wrong AppId rejected");
        Reject(()=>ReleasePackageAudit.Validate(package,"HuyVo.Captovix","9.99.0"),"Wrong version rejected");
        string temp=Path.Combine(Path.GetTempPath(),"CaptovixAudit-"+Guid.NewGuid().ToString("N")+".nupkg");
        try {
            void Mutate(Action<ZipArchive> change){File.Copy(package,temp,true);using var zip=ZipFile.Open(temp,ZipArchiveMode.Update);change(zip);}
            Mutate(z=>z.Entries.Single(e=>e.FullName.EndsWith("/release-quality.json")).Delete());
            Reject(()=>ReleasePackageAudit.Validate(temp,"HuyVo.Captovix","1.46.1"),"Missing release quality stamp rejected");
            Mutate(z=>{var e=z.Entries.Single(e=>e.FullName.EndsWith("/Captovix.dll"));var name=e.FullName;e.Delete();using var stream=z.CreateEntry(name).Open();stream.WriteByte(0);});
            Reject(()=>ReleasePackageAudit.Validate(temp,"HuyVo.Captovix","1.46.1"),"Altered runtime bytes rejected by stamp");
            Mutate(z=>z.Entries.Single(e=>e.FullName.EndsWith("/tessdata/vie.traineddata")).Delete());
            Reject(()=>ReleasePackageAudit.Validate(temp,"HuyVo.Captovix","1.46.1"),"Missing OCR component rejected");
            Mutate(z=>z.CreateEntry("../outside.dll"));
            Reject(()=>ReleasePackageAudit.Validate(temp,"HuyVo.Captovix","1.46.1"),"Traversal entry rejected");
        } finally {File.Delete(temp);}
        Console.WriteLine("7 real package quality checks passed.");
    }
}
