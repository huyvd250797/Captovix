using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
namespace Captovix;
public partial class MainWindow
{
    private void SearchLibrary_Click(object sender,RoutedEventArgs e){LibrarySearchPopup.IsOpen=!LibrarySearchPopup.IsOpen;if(LibrarySearchPopup.IsOpen)Dispatcher.BeginInvoke(()=>SearchBox.Focus());}
    private void ChooseMediaFolder_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||_updateApplying)return;
        var picker=new OpenFolderDialog{Title="Chọn thư mục lưu ảnh và video mới",Multiselect=false};
        if(picker.ShowDialog(this)!=true)return;
        try{_store.SetMediaRoot(picker.FolderName);MediaLocationButton.ToolTip="Lưu media mới: "+_store.Root+"\nBấm để chọn thư mục khác.";SetStatus("Media mới sẽ lưu tại "+_store.Root+". Ảnh/video cũ giữ nguyên vị trí.");}
        catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Chưa đổi được thư mục media");}
    }
}
