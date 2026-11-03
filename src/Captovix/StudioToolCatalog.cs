namespace Captovix;
public sealed record StudioTool(string Command,string Label,string Icon);
public static class StudioToolCatalog
{
    public static readonly StudioTool[] All=[
        new("Editor:Select","Chọn đối tượng","Select"),new("Editor:Magnifier","Phóng đại","Magnifier"),new("Editor:Spotlight","Spotlight","Spotlight"),new("Editor:Highlight","Highlight","Highlight"),new("Editor:Freehand","Vẽ tự do","Freehand"),new("Canvas","Canvas","Expand"),new("Stamp","Stamp","Stamp"),new("Color","Màu & làm nét","StudioColor"),new("Pick","Lấy màu từ ảnh","Eyedropper"),
        new("Clone","Clone","StudioClone"),new("Perspective","Phối cảnh","StudioPerspective"),new("Rotate","Chỉnh nghiêng","StudioRotate"),
        new("Background","Tách nền theo màu","StudioBackground"),new("Mask","Cắt theo hình","StudioMask"),new("Comparison","Trước / Sau","StudioComparison"),
        new("Watermark","Watermark","StudioWatermark"),new("ResetBase","Khôi phục nền gốc","StudioReset"),new("CustomImage","Thêm logo / ảnh…","ImageLayer")];
    public static StudioTool? Find(string? command)=>All.FirstOrDefault(t=>t.Command==command);
}
public sealed record StampSpec(string Id,string Label,string Color,string Path,string Category="Đánh dấu & phản hồi",string? Caption=null);
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
        new("idea","Ý tưởng","#D99516","M8,17 C8,14 5,13 5,9 A7,7 0 0 1 19,9 C19,13 16,14 16,17 Z M9,20 H15 M10,23 H14"),
        new("bug","Lỗi cần sửa","#DC4C57","M8,8 H16 V18 C16,23 8,23 8,18 Z M10,8 V5 H14 V8 M3,10 H8 M16,10 H21 M3,16 H8 M16,16 H21 M5,23 L9,20 M19,23 L15,20","Kiểm tra & xử lý"),
        new("review","Rà soát","#397CCB","M10,2 A8,8 0 1 1 9.99,2 M16,16 L23,23 M6,10 L9,13 L14,7","Kiểm tra & xử lý"),
        new("pending","Chờ xử lý","#D99516","M6,2 H18 M6,22 H18 M8,2 C8,9 16,15 16,22 M16,2 C16,9 8,15 8,22","Kiểm tra & xử lý"),
        new("done","Hoàn tất","#159568","M19,5 V21 H3 V5 H7 M8,3 H14 V7 H8 Z M7,14 L10,17 L17,10","Kiểm tra & xử lý"),
        new("reject","Từ chối","#DC4C57","M7,2 H17 L23,8 V17 L17,23 H7 L1,17 V8 Z M8,8 L16,16 M16,8 L8,16","Kiểm tra & xử lý"),
        new("refresh","Cập nhật","#397CCB","M20,9 A8,8 0 0 0 5,6 M4,2 V8 H10 M4,15 A8,8 0 0 0 19,18 M20,22 V16 H14","Kiểm tra & xử lý"),
        new("priority","Ưu tiên cao","#DC4C57","M5,14 L12,7 L19,14 M5,21 L12,14 L19,21 M12,2 V3","Kiểm tra & xử lý"),
        new("stop","Dừng tại đây","#DC4C57","M8,2 H16 L22,8 V16 L16,22 H8 L2,16 V8 Z M7,12 H17","Kiểm tra & xử lý"),
        new("click","Click tại đây","#397CCB","M6,3 L6,20 L11,15 L15,23 L19,21 L15,14 L22,14 Z M3,1 L1,0 M2,6 H0 M10,2 L12,0","Hướng dẫn thao tác"),
        new("doubleclick","Nhấp đúp","#8651CB","M8,9 V22 L12,17 L16,23 L19,21 L15,15 L22,15 Z M5,8 A5,5 0 0 1 13,4 M2,9 A8,8 0 0 1 15,2","Hướng dẫn thao tác"),
        new("keyboard","Bàn phím","#397CCB","M2,5 H22 V20 H2 Z M6,9 H7 M11,9 H12 M16,9 H17 M6,13 H7 M11,13 H12 M16,13 H17 M7,17 H17","Hướng dẫn thao tác"),
        new("hand","Chọn mục này","#397CCB","M8,14 V4 C8,1 12,1 12,4 V11 M12,8 C12,5 16,5 16,8 V12 M16,10 C16,7 20,7 20,10 V18 L17,23 H9 L3,15 C1,12 4,10 8,14","Hướng dẫn thao tác"),
        new("download","Tải xuống","#159568","M12,2 V16 M6,10 L12,16 L18,10 M3,18 V23 H21 V18","Hướng dẫn thao tác"),
        new("upload","Tải lên","#397CCB","M12,17 V3 M6,9 L12,3 L18,9 M3,18 V23 H21 V18","Hướng dẫn thao tác"),
        new("link","Liên kết","#397CCB","M9,15 L15,9 M7,13 L4,16 A4,4 0 0 0 10,22 L13,19 M11,5 L14,2 A4,4 0 0 1 20,8 L17,11","Hướng dẫn thao tác"),
        new("cursor","Vị trí chọn","#D99516","M12,1 V7 M12,17 V23 M1,12 H7 M17,12 H23 M12,8 A4,4 0 1 1 11.99,8","Hướng dẫn thao tác"),
        new("lock","Bảo mật","#8651CB","M5,10 H19 V23 H5 Z M8,10 V6 A4,4 0 0 1 16,6 V10 M12,16 V19","Bảo mật & thông tin"),
        new("shield","Đã kiểm tra","#159568","M12,2 L22,6 V12 C22,18 17,22 12,24 C7,22 2,18 2,12 V6 Z M6,12 L10,16 L18,8","Bảo mật & thông tin"),
        new("private","Thông tin riêng","#DC4C57","M2,12 C6,3 18,3 22,12 C18,21 6,21 2,12 Z M12,8 A4,4 0 1 1 11.99,8 M2,2 L22,22","Bảo mật & thông tin"),
        new("calendar","Ngày hẹn","#397CCB","M3,5 H21 V23 H3 Z M3,10 H21 M8,2 V7 M16,2 V7 M7,15 H10 M14,15 H17 M7,19 H10","Bảo mật & thông tin"),
        new("clock","Thời gian","#D99516","M12,2 A10,10 0 1 1 11.99,2 M12,6 V12 L17,15","Bảo mật & thông tin"),
        new("seal-approved","Đã duyệt","#159568","M2,2 H22 V22 H2 Z","Dấu trạng thái","ĐÃ DUYỆT"),
        new("seal-revision","Cần sửa","#DC4C57","M2,2 H22 V22 H2 Z","Dấu trạng thái","CẦN SỬA"),
        new("seal-draft","Bản nháp","#D99516","M2,2 H22 V22 H2 Z","Dấu trạng thái","BẢN NHÁP"),
        new("seal-verified","Đã kiểm tra","#397CCB","M2,2 H22 V22 H2 Z","Dấu trạng thái","ĐÃ KIỂM TRA"),
        new("seal-private","Bảo mật","#8651CB","M2,2 H22 V22 H2 Z","Dấu trạng thái","BẢO MẬT"),
        new("seal-important","Quan trọng","#DC4C57","M2,2 H22 V22 H2 Z","Dấu trạng thái","QUAN TRỌNG")];
}
