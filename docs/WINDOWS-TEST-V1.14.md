# Nghiệm thu Windows thật — V1.14

Chưa thực hiện tại môi trường Linux. Đánh dấu máy, Windows build, SDR/HDR, scale và app đích trước mỗi lần thử; không đánh dấu đạt chỉ vì build thành công.

## Cài và cập nhật

- Máy cài mới: giải nén Update-Packages, chạy HuyVo.Captovix-win-Setup.exe, chọnD:\Captovix\Captovix. Kiểm tra iconexe/start/taskbar/tray mới và app mở maximized.
- Nâng managed install cũ tại ổD: đóng cả tray, chạy setup/cập nhật trong app; giữ đúng installroot, ảnh/chú thích/preset/bố cục và appearance-settings. Không chọn thư mục dữ liệu làm thư mục cài. Kiểm tra thư mục current không bị lồng.
- Nguồn ổn định giữ sẵn; kiểm tra app nhận1.14.0 sau khi publisher tự đăng5assets lên stable latest Release. Setup chưa ký bằng chứng thư nhà phát hành; không hướng dẫn bỏ qua cảnh báo Windows một cách mù quáng.
- Chạy scripts/Build-Windows.ps1 để thực thi các WPFcheck trên máy kiểm thử. Thử lại đổi ảnh, inline text, Ctrl+C/V, xóa/khôi phục ảnh và export thuộc tính cũ.

## Giao diện

- Sáng→Tối→System mở/đóng app mỗi lần; đổi màu tím/xanh/đen/trắng/vàng bằng palette và picker. Kiểm tra chữ, nút disabled/focus/selected/hover, ComboBox popup, menu/context menu, cây thiết lập, tooltip, lớp, gallery, inspector, cập nhật và scroll preview đọc được.
- MáyA chọntím+Tối, máyB chọnxanh+Sáng; mở lại phải giữ riêng. Tài khoản Windows khác trên cùng máy cũng tách cấu hình. Update không reset.
- System: đổi Windows AppsUseLightTheme trong Personalization khi app đang mở/còntray; đổi ngay, không mất accent. Light/Dark: Windows đổi không ép theme.
- Tạo ảnh có nền/chữ màu trước khi đổi theme; hash PNG xuất trước/sau phải bằng nhau. Theme không đổi style ảnh, mẫu đã lưu hay project.
- Khi app đóng, sao lưu appearance-settings.json rồi đưa JSON lỗi; app mở được mặc định, thông báo trong thiết lập, lưu mới có backup. Khôi phục file ban đầu sau test.

## Chụp cuộn — mẫu xác định

Dùng docs/scroll-capture-test.html mở offline ở browser100%,SDR. Chọn vùng chỉ chứa khối nội dung, tránh chrome/scrollbar/header. Browser và cửa sổ không đổi kích thước trong phiên.

- Lấy khung đầu, cuộn xuống30–60% vùng, lấy6–10khung qua cả đoạn chữ và ô ảnh. Xem mối nối100%, không lặp/mất hàng đánh số, chữ không đứt hoặc nhảy ngang.
- Lấy lại khi chưa cuộn: không tăngkhung. Tới cuối trang lấy thêm: không appendtrùng. Cuộn ngược, lệch ngang, đổi zoom, trang có animation: kết quả không chắc phải hiện rõ; người dùng không được tin auto-match nếu nội dung sai. Nơi không thể ghép chính xác phải lấy lại vùng/khung.
- Sửa overlap sai, cập nhật; chọn xác nhận từng điểm chưa chắc; PNG xuất không có đường tím. So kích thước đầu ra với h+(h-overlap1)+…; ghép không nội suy pixel.
- Dùng Ctrl+Shift+F10/F11 khi focusbrowser. Mở app khác chiếm hotkey rồi bắt đầuphiên: có cảnhbáo, nút vẫn dùngđược. Toolbar không hiện trongPNG kể cả excludeAPI thấtbại.
- Esc lúc chọnvùng, lúctrước chụp, lúcđang chụp; đóngpreviewđang ghép. Không apphiddenmãi, hotkeyđượcgiải phóng, lầnchụptiếptheohoạtđộng. Giữnguồnđãcommit.
- Hủy sau3khung→khôi phục session.json→ghép lại. Thử filePNGthiếu/hashđổi và JSONlỗi; từchối rõ, khôngxóanguồn. Khôi phụcchỉghépkhungcũ, khônghòanhậpcửa sổ đích cũ.
- Vượt24khung/4MPkhung/40MPoutput/32768h/64MPnguồn: chặnrõ, khôngcrash, vẫn xem/lưuphầnđãthu. Thử ổítchỗ/khôngquyềnghi trongmáytest; ảnhđãghép vànguồncũvẫncòn.
- Kiểm scale100/125/150/175/200%, mànphụtọađộâm;đổimàn/DPIgiữaphiênphảidừngappend. ThửWord/PDF/browsernội dungtĩnhriêng; ghinhận appđíchhỗtrợvàhạnchế. KhôngtuyênbốHDRđạtthếSDR.

Ghi kết quả+log `%LOCALAPPDATA%\HuyVo\Captovix\errors.log`, ảnhghép và nguồn test vào báo cáo nghiệm thu; không gửi ảnh công việc nhạy cảm kèm source.
