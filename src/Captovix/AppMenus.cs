using System.Windows;
using System.Windows.Controls;
namespace Captovix;
internal static class AppMenus
{
    internal static void Decorate(ItemsControl parent)
    {
        if(parent is ContextMenu context)context.MaxHeight=Math.Max(240,SystemParameters.WorkArea.Height-40);
        parent.SetResourceReference(FrameworkElement.StyleProperty,parent is ContextMenu?typeof(ContextMenu):typeof(MenuItem));
        foreach(var item in parent.Items.OfType<MenuItem>()){
            item.SetResourceReference(FrameworkElement.StyleProperty,typeof(MenuItem));
            if(item.Header is string title){var parts=title.Split(" · ",2);if(parts.Length==2&&(parts[1].StartsWith("Ctrl")||parts[1]=="Delete")){item.Header=parts[0];item.InputGestureText=parts[1];}item.Icon??=AppIcons.Content(Icon(title),"");}
            if(item.HasItems)Decorate(item);
        }
    }
    private static string Icon(string title)=>title switch{
        "Trái" or "Giữa ngang" or "Phải" or "Trên" or "Giữa dọc" or "Dưới" or "Vùng ảnh đang thấy" or "Toàn canvas"=>"Align",
        var s when s.StartsWith("Hoàn")=>"Undo",var s when s.StartsWith("Làm lại")=>"Redo",var s when s.StartsWith("Copy")=>"Copy",var s when s.StartsWith("Dán")=>"Paste",
        var s when s.StartsWith("Nhân")=>"Duplicate",var s when s.StartsWith("Bỏ nhóm")=>"Ungroup",var s when s.StartsWith("Nhóm")=>"Group",
        var s when s.Contains("khóa")=>s.StartsWith("Mở")?"Unlock":"Lock",var s when s.StartsWith("Ẩn")=>"EyeOff",var s when s.StartsWith("Xóa")=>"Delete",var s when s.StartsWith("Xuất")=>"Export",
        var s when s.StartsWith("Sắp")||s.Contains("lớp")||s.StartsWith("Chọn đối tượng")=>"Layers",var s when s.StartsWith("Căn")||s.StartsWith("Chia")||s.StartsWith("Tham chiếu")=>"Align",var s when s.StartsWith("Chụp")=>"Camera",var s when s.StartsWith("Quay")=>"Record",
        var s when s.Contains("OCR")=>"Ocr",var s when s.StartsWith("Chỉnh sửa hỗ")=>"Assist",_=>"Settings"};
}
