namespace Captovix;
public sealed record StudioTool(string Command,string Label,string Icon);
public static class StudioToolCatalog
{
    public static readonly StudioTool[] All=[
        new("Stamp","Stamp","Stamp"),new("Color","Màu & làm nét","StudioColor"),new("Pick","Lấy màu từ ảnh","Eyedropper"),
        new("Clone","Clone","StudioClone"),new("Perspective","Phối cảnh","StudioPerspective"),new("Rotate","Chỉnh nghiêng","StudioRotate"),
        new("Background","Tách nền theo màu","StudioBackground"),new("Mask","Cắt theo hình","StudioMask"),new("Comparison","Trước / Sau","StudioComparison"),
        new("Watermark","Watermark","StudioWatermark"),new("ResetBase","Khôi phục nền gốc","StudioReset"),new("CustomImage","Thêm logo / ảnh…","ImageLayer")];
    public static StudioTool? Find(string? command)=>All.FirstOrDefault(t=>t.Command==command);
}
public sealed record StampSpec(string Id,string Label,string Color,string Path);
public static class StampCatalog
{
    // Original vector drawings for Captovix; no third-party stamp assets.
    public static readonly StampSpec[] All=[
        new("check","Đúng","#159568","M4,12 L9,17 L20,5"),new("cross","Sai","#E4495A","M5,5 L19,19 M19,5 L5,19"),
        new("approved","Đã duyệt","#159568","M12,2 L15,4 L19,4 L20,8 L22,12 L20,16 L19,20 L15,20 L12,22 L9,20 L5,20 L4,16 L2,12 L4,8 L5,4 L9,4 Z M7,12 L10,15 L17,8"),
        new("warning","Lưu ý","#D99516","M12,3 L22,21 H2 Z M12,9 V14 M12,17 V17.3"),new("info","Thông tin","#397CCB","M12,2 A10,10 0 1 1 11.99,2 M12,10 V17 M12,6.5 V6.8"),
        new("question","Cần kiểm tra","#8651CB","M12,2 A10,10 0 1 1 11.99,2 M8.5,8 C8.5,4 16,4 16,8 C16,11 12,10 12,14 M12,17 V17.2"),
        new("star","Nổi bật","#D99516","M12,2 L15,8 L22,9 L17,14 L18,21 L12,18 L6,21 L7,14 L2,9 L9,8 Z"),
        new("heart","Yêu thích","#D84A73","M12,21 C-5,10 4,-2 12,7 C20,-2 29,10 12,21 Z"),
        new("flag","Đánh dấu","#E4495A","M5,22 V3 M5,4 C10,0 15,8 21,3 V14 C15,19 10,11 5,15"),
        new("target","Mục tiêu","#397CCB","M12,2 A10,10 0 1 1 11.99,2 M12,7 A5,5 0 1 1 11.99,7 M12,11 V13"),
        new("pin","Ghim","#8651CB","M8,3 H16 L15,9 L19,13 H5 L9,9 Z M12,13 V22"),
        new("idea","Ý tưởng","#D99516","M8,17 C8,14 5,13 5,9 A7,7 0 0 1 19,9 C19,13 16,14 16,17 Z M9,20 H15 M10,23 H14")];
}
