# Captovix V1.17.0 — Assisted Image Editing

## 1. Sửa chữ và tách vùng giao diện

Mở ảnh → **Hỗ trợ** trên toolbar hoặc **Công cụ → Chỉnh sửa ảnh hỗ trợ…**.

1. Zoom và kéo chọn vùng chữ/nút cần sửa. Ctrl+con lăn hoặc +/− để zoom, 100% để kiểm pixel; Space+kéo/chuột giữa để kéo khung nhìn; bật Kính lúp3× để canh vùng nhỏ.
2. Bấm Nhận chữ. Có thể chọn một dòng trong danh sách gợi ý do OCR trả về; vùng và chữ được điền theo dòng đó. Gợi ý không phải phát hiện mọi thành phần giao diện.
3. Sửa chữ trong ô kết quả; chọn cỡ chữ, màu nền và màu chữ. App gợi ý màu từ viền vùng, có báo nền không đồng nhất. Có thể chọn màu thủ công.
4. Chọn **Thay chữ trên nền màu phẳng** hoặc **Tách vùng thành lớp để di chuyển**.
5. Bấm **Xem trước thay đổi**, kiểm tra, rồi **Áp dụng vào Editor**. Thay đổi vùng/chữ/style sẽ vô hiệu hóa preview cũ, yêu cầu xem lại.
6. Kết quả là hai đối tượng: nền màu phẳng và chữ/lớp ảnh. Chỉ đối tượng nội dung được chọn để kéo; nền ở vị trí cũ. Một Undo hoàn tác toàn bộ lần áp dụng; các lớp vẫn sửa/lưu/mở lại được.

Tách vùng tạo lớp tại vị trí ban đầu; sau khi áp dụng, kéo lớp đó trong Editor. Khi vùng tách còn tại vị trí cũ, nó che lớp nền nên preview chưa thể hiện chỗ trống phía sau. Đây là thao tác có kiểm soát, không phải tự chuyển mọi screenshot thành layout editable.

Giới hạn: tối đa20MP/vùng,20.000ký tự, cỡ8–240px và phải vừa vùng. Font mặc định Segoe UI, chưa đoán chính xác font gốc. Nền ảnh/gradient/hoa văn không được inpaint tự động; thay nền bằng một màu có thể lộ dấu chỉnh. Chưa có phát hiện thông tin nhạy cảm tự động. Nếu ảnh đang xoay/lật/resize, app tạo bản ảnh đã render để chỉnh, giữ dự án cũ; với ảnh tương thích, có thể chọn chỉnh bản hiện tại hoặc tạo bản mới. Chọn vùng bên trong ảnh, không chọn phần đệm ngoài ảnh.

OCR và gợi ý chỉ xử lý snapshot đã render, giữ phần che đang thấy; không đọc pixel gốc bị che. Project vẫn giữ nguồn để chỉnh lại; chia sẻ ảnh xuất đã render khi cần che thông tin.

## 2. Popup cập nhật

Khi kiểm tra phát hiện release mới: popup có **Cập nhật** và **Để sau**. Cập nhật mở phần tiến trình và bắt đầu tải; vẫn cần chọn khởi động lại sau khi tải/kiểm tra xong. Portable mở trang phát hành.

Để sau hoặc đóng bằngX: thẻ thông báo thu nhỏ/bay về nút cập nhật góc phải dưới, badge phiên bản vẫn giữ. Nếu Windows tắt animation thì chỉ giữ chỉ báo, không chạy chuyển động. Mỗi version thông báo một lần trong phiên app; không bật popup lúc đang chụp/quay, mở thiết lập hoặc app không ở trước. Khi trở lại Editor sẽ kiểm điều kiện hiển thị.

Lịch kiểm tra tự động giữ cơ chế hiện có: tối đa một lần/24giờ/nguồn khi bật AutoCheck; timer kiểm điều kiện mỗi30phút. Có thể bấm Kiểm tra cập nhật thủ công. Không phải push tức thời từ server.

**Popup mới chỉ có từ app V1.17 trở đi.** App1.16 cần cập nhật lên1.17 bằng cơ chế/nút hiện có trước.

## 3. Chụp vùng và phần tử

- Overlay đen bán trong suốt trên màn hình; vùng kéo chọn/element được nhận diện là lỗ trong suốt, không có lớp màu đen hay màu fill bên trong. Viền và hướng dẫn vẫn hiện.
- Mỗi màn hình có overlay riêng theo tọa độ pixel; không thay pipeline PNG native.
- Element dùng UI Automation: bỏ overlay của Captovix, ưu tiên nhánh nhỏ nhất chứa con trỏ, chuẩn hóa leaf→container→window, giữ ý nghĩa container khi trùng bounds.
- Kiểm tra lại cửa sổ sau probe và trước xác nhận click. Khi cửa sổ đổi vị trí, bỏ khung cũ và dò lại. Provider chậm bị kết thúc trong helper riêng.
- Con lăn chọn cha/con; click xác nhận; giữ+kéo làm vùng thủ công; phải/Esc hủy. Overlay đóng trước khi lấy ảnh.

Độ chính xác phụ thuộc UIA của ứng dụng nguồn. Canvas/game/remote desktop hoặc trình duyệt không cung cấp cây accessibility có thể chỉ nhận cửa sổ; app báo fallback và cho kéo vùng. Chưa công bố ứng dụng cụ thể nào đã nghiệm thu Windows.

## 4. Giao diện và media

- Context menu có template riêng và icon vector trước tên; phím tắt ở cột riêng. Áp dụng cả menu ảnh và tray, tương thích Light/Dark, không dùng gutter trắng của theme Windows.
- Đồng bộ icon nét, checkbox/radio/expander, textbox bo góc và scrollbar. Giữ native titlebar/các hộp chọn file và thư mục Windows.
- Bỏ ô tìm kiếm thường trực: bấm kính lúp cùng hàng Xóa/Khôi phục/Mở thư mục/Bố cục. Bộ lọc vẫn giữ khi đóng popup; icon đổi màu khi đang lọc.
- Nút chọn thư mục media ở cùng hàng. Chỉ **ảnh/video mới** lưu ở thư mục mới; media cũ và đường dẫn metadata giữ nguyên, không tự di chuyển/xóa dữ liệu.
- Cấu hình tại `%LOCALAPPDATA%\HuyVo\Captovix\media-location.json`. Có kiểm tra ghi trước khi lưu. Thư mục không khả dụng: báo lỗi, không âm thầm ghi nơi khác; thư viện cũ vẫn mở theo đường dẫn còn truy cập được. Không lưu media bên trong thư mục chương trình hiện tại vì update có thể thay thế nó.
- Chọn folder có file khác không tự nhập tất cả file đó. Khôi phục orphan chỉ xét tên media do Captovix tạo. Metadata, Edits, theme và project vẫn ở DataRoot; chọn folder không phải chuyển toàn bộ thư viện sang máy khác.

## 5. Logo sau update

Kiểm tra gói1.16 cho thấy Captovix.exe và Squirrel.exe đã có artwork tím đúng; chưa có bằng chứng Windows runtime để khẳng định nguyên nhân duy nhất. Một khả năng là shortcut/pin hoặc updater gốc còn đường dẫn/icon cũ.

Đã xác nhận thêm lỗi riêng của Setup wrapper: MUI2 ghi đè lệnh Icon bằng icon NSIS mặc định. Bản1.17 thêm MUI_ICON trước macro giao diện và kiểm trực tiếp PE icon của Setup; đây không phải bằng chứng duy nhất cho lỗi icon taskbar khi app chạy.

V1.17 dùng ICO mới `Captovix-brand-v117.ico` cùng artwork đã chốt cho EXE, cửa sổ, tray, Setup và updater. Khi mở bản managed install, app sao chép icon vào DataRoot/Brand rồi làm mới shortcut tên **Captovix*** ở Desktop/Programs/Taskbar, chỉ khi target thật nằm trong installroot của Captovix. Shortcut launcher thông thường trỏ trực tiếp app sẽ chuyển sang updater launcher ổn định; các arguments tùy chỉnh được giữ. AppID tiến trình và shortcut thống nhất.

Không quét sâu thư mục, không sửa shortcut app khác, không xóa icon cache hệ thống và không restart Explorer. Shortcut đã đổi tên khác hoặc pin do Windows quản lý khác có thể phải bỏ ghim/ghim lại. `branding.log` ghi version, EXE thực chạy, đường dẫn/hash icon và số shortcut đã làm mới; `errors.log` ghi lỗi nếu Windows từ chối. Đây là chẩn đoán/cải thiện có giới hạn, không tuyên bố đã thử update/taskbar trên Windows.

## 6. Kiểm thử Windows cần hoàn tất

Kiểm tra13 bộ WPF trên Windows và thao tác thật: assist text/preview/Undo/lưu mở lại/crop/rotate/newcopy/mask; OCR zoom/pan/lens/cancel; popup update Later/X/Download/Restart/capture busy/reduced motion; menu keyboard/icon/dark/light; chụp vùng qua màn có DPI khác; UIA modal/grid, di chuyển nguồn và fallback; folderD/mất quyền/ổ mất kết nối; update từ1.16 và icon Desktop/Start/taskbar/tray. Logic/cross-build không thay cho những ca này.

V1.18 dự kiến — Image Library & Projects: tag/bộ sưu tập/yêu thích, lọc nhanh, quản lý dự án và backup/import giữ đủ assets. Chọn thư mục lưu media đã có ở1.17 sẽ được kế thừa.
