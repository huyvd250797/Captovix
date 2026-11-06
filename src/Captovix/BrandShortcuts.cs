using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Velopack.Locators;
using Velopack.Windows;
namespace Captovix;
internal static class BrandShortcuts
{
    private static string PinnedDirectory=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");

    internal static bool RefreshPinned(string icon)
    {
        try{
            string? root=VelopackLocator.Current.RootAppDir;
            if(!Directory.Exists(PinnedDirectory))return true;
            if(root==null)return false;
            bool updated=true;
            foreach(var file in Directory.EnumerateFiles(PinnedDirectory,"Captovix*.lnk")){
                try{
                    using var link=new ShellLink(file);
                    if(!BrandOwnership.IsOwned(root,link.Target))continue;
                    if(link.IconPath==icon&&link.IconIndex==0)continue;
                    link.IconPath=icon;
                    link.IconIndex=0;
                    link.Save(file);
                    SHChangeNotify(0x2000,0x5,file,0);
                }catch(Exception ex){updated=false;App.Log(ex);}
            }
            return updated;
        }catch(Exception ex){App.Log(ex);return false;}
    }
    internal static void Repair()
    {
        try{
            string source=Path.Combine(AppContext.BaseDirectory,"Assets","Captovix-brand-v117.ico");if(!File.Exists(source))return;
            string folder=Path.Combine(App.DataRoot,"Brand");Directory.CreateDirectory(folder);string icon=Path.Combine(folder,"Captovix-brand-v117.ico");var bytes=File.ReadAllBytes(source);
            if(!File.Exists(icon)||!File.ReadAllBytes(icon).SequenceEqual(bytes))File.WriteAllBytes(icon,bytes);
            string? root=VelopackLocator.Current.RootAppDir;int changed=0;
            if(root!=null&&File.Exists(Path.Combine(root,"Update.exe"))){
                StartupRecoveryLauncher.EnsureShortcut(root,icon);
                var dirs=new[]{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),Environment.GetFolderPath(Environment.SpecialFolder.Programs),PinnedDirectory};
                foreach(string directory in dirs.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))foreach(string file in Directory.EnumerateFiles(directory,"Captovix*.lnk",SearchOption.TopDirectoryOnly)){
                    try{var shortcutIcon=directory==PinnedDirectory&&ShellBrandIcon.ShortcutPath is {} personal?personal:icon;using var link=new ShellLink(file);if(!BrandOwnership.IsOwned(root,link.Target))continue;bool oldDirect=Path.GetFileName(link.Target).Equals("Captovix.exe",StringComparison.OrdinalIgnoreCase)&&string.IsNullOrWhiteSpace(link.Arguments);if(link.IconPath==shortcutIcon&&link.IconIndex==0&&!oldDirect)continue;if(oldDirect){link.Target=Path.Combine(root,"Update.exe");link.Arguments="start Captovix.exe";link.WorkingDirectory=root;}link.IconPath=shortcutIcon;link.IconIndex=0;link.SetAppUserModelId("velopack."+VersionInfo.AppId);link.Save(file);SHChangeNotify(0x2000,0x5,file,0);changed++;}catch(Exception ex){App.Log(ex);}
                }
            }
            MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"branding.log"),$"V{VersionInfo.Current}\nEXE: {Environment.ProcessPath}\nIcon: {icon}\nSHA256: {Convert.ToHexString(SHA256.HashData(bytes))}\nOwned shortcuts refreshed: {changed}\n");
        }catch(Exception ex){App.Log(ex);}
    }
    [DllImport("shell32.dll",CharSet=CharSet.Unicode)]private static extern void SHChangeNotify(uint events,uint flags,string path,nint unused);
}
