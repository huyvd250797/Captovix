# Nghiệm thu V1.11.1 — chưa đánh dấu đạt

- [ ] Mở MainWindow một lần, maximized, không NullReference trước/during InitializeComponent.
- [ ] Chạy StudioReliabilityWindowsChecks và các harness WPF kế thừa qua Build.cmd.
- [ ] Từ v1.9 và v1.11: đọc nguồn stable/hiện version 1.11.1; feed mới nằm ngoài 10 releases đầu vẫn nhận; thử offline/404/thiếu gói/hash sai không báo mới nhất.
- [ ] Cài Setup mới và cập nhật managed app ở D:\Captovix\Captovix; giữ thư viện, phím tắt và chú thích; kiểm tra download/restart/cancel.
- [ ] Xóa ảnh đầu/giữa/cuối, ảnh đang mở/ảnh khác, video; khôi phục nguyên byte; lỗi quyền/file đang khóa không mất metadata.
- [ ] Ctrl+wheel zoom theo chuột; wheel cuộn dọc không zoom; Shift+wheel ngang; kiểm tra mixed-DPI 100–200%, monitor âm.
- [ ] Nhập số nhanh, xóa tạm nội dung, nhập sai/đúng, đổi ảnh/công cụ/selection ngay; live edit không áp sang ảnh/đối tượng khác, Undo theo lần chỉnh.
- [ ] Viền chụp màu/dày 1/4/128; auto-copy có viền, gốc đúng pixel, xuất tăng 2*dày và sửa/bỏ viền được.
- [ ] Bảng Viền/Nền trong suốt và màu tùy chọn; shape/text/callout giữ màu riêng, clear không thành màu đen.
- [ ] Chọn mẫu vuông/bo góc/nét đứt/mũi tên, sao lưu/mở lại/bỏ; chuyển mẫu khi đang chọn đối tượng rồi vẽ tiếp đúng công cụ.
- [ ] Bóng 9 ô, nhất là ô dưới giữa: bóng thẳng xuống; tâm halo; copy/paste/duplicate/Undo/reopen; crop biên, ảnh 4K/8K nhiều đối tượng.
- [ ] Giữ các kiểm tra privacy/protected sampling, IME và clipboard thực tế của V1.11 bên dưới.

# V1.6 — Image Fidelity

Thực hiện matrix trong WINDOWS-FIDELITY-MATRIX.md và hướng dẫn IMAGE-FIDELITY.md. Trạng thái bàn giao: CHƯA nghiệm thu Windows thật; không lấy cross-build/unit tests thay thế. Kiểm chứng SDR có nút trong Editor và report trên máy, lặp đủ 100/125/150/175/200%, monitor âm và kéo Editor giữa màn hình.

# Nghiệm thu hotfix V1.5.1 — Editor Switch Fix

- [ ] Update V1.5.0 → V1.5.1; giữ ảnh gốc, chú thích, thư viện, hotkey/preset, nguồn update.
- [ ] Chạy `dotnet run --project tests/EditorWindowsChecks -c Release -p:Platform=x64` trên Windows; kiểm tra các ca lifecycle mới.
- [ ] Mở ảnh A (có crop/rotate/viền/chữ), chuyển sang ảnh B khác kích thước rồi quay lại A, lặp nhanh 30 vòng; không hiện NullReferenceException, kích thước/crop/style/chú thích đúng từng ảnh.
- [ ] Đang nhập chữ rồi chuyển ảnh: phần đã nhập được lưu; quay lại vẫn sửa được.
- [ ] Chuyển ảnh → video → ảnh; bỏ chọn ảnh, xóa ảnh được chọn, chọn ảnh mới chụp/nhập; trạng thái rỗng không đọc layout cũ.
- [ ] Zoom, undo/redo, export/copy sau chuyển ảnh; không lẫn dữ liệu tài liệu cũ.
- [ ] Thoát/mở lại giữ chỉnh sửa. Xem errors.log theo timestamp mới để phân biệt log V1.5.0 đã lưu từ trước.

# Nghiệm thu Capture Workflow V1.5 (máy Windows thật)

Các mục sau chưa được chạy bằng cross-build Linux. Kiểm tra trước khi phát hành rộng:

- [ ] Cài mới V1.5 tại D:\Captovix\Captovix; mở thủ công Editor phóng to; mặc định X thoát và không có autorun.
- [ ] Upgrade bản V1.4 giữ ảnh, sidecar schema 4, preset chú thích, nguồn cập nhật mặc định; đọc được ảnh đã xoay/crop/style.
- [ ] Nhấp biểu tượng khay mở Editor; menu chuột phải chạy đủ 5 chế độ và preset; ẩn Editor/menu trước ảnh; Restart Explorer khôi phục biểu tượng; notification click mở đúng ảnh.
- [ ] Bật đóng xuống khay rồi X, mở lại từ tray hoặc shortcut lần hai; menu Thoát chấm dứt process và giải phóng hotkey. Tắt tùy chọn rồi X thoát.
- [ ] 5 phím mặc định, đổi phím/tắt phím/lưu/mở lại; thử trùng nội bộ, trùng +2/+3/+4, F12, Win key và xung đột app khác. Thất bại khôi phục phím cũ; ô nhập phím không khởi động capture.
- [ ] Chụp vùng màn hình tọa độ âm, hai màn hình 100/150/200%; chụp lại sau restart. Hủy vùng mới không thay vùng cũ. Rút màn hình/đổi DPI/độ phân giải buộc chọn lại; đổi topology trong đếm ngược phải hủy an toàn.
- [ ] Cửa sổ: chọn lại mỗi lần chụp; đóng/thu nhỏ cửa sổ khi đếm ngược không chụp mục tiêu khác. Quay cửa sổ hiện có vẫn hoạt động.
- [ ] Delay 0/3/5: Esc ở vùng/cửa sổ/countdown; chụp liên tiếp/tray/hotkey không tạo phiên song song; file hủy không đưa vào history.
- [ ] Preset: lưu/áp dụng/xóa, tên trống/trùng/quá dài, tối đa 20, màn hình không còn; preset cửa sổ không dùng HWND cũ. Giữ riêng preset chú thích.
- [ ] Mở Editor sau ảnh; lưu nền không đổi tài liệu đang chỉnh; bật/tắt autocopy độc lập; khóa clipboard hoặc Do Not Disturb không mất ảnh. Khởi động lại thấy ảnh đã chụp nền.
- [ ] Autorun bật/lưu/đăng xuất vào lại: mở Editor hoặc tray đúng lựa chọn, không tự capture; mở shortcut thủ công vẫn vào Editor. Tắt/lưu bỏ HKCU Run; gỡ cài đặt không để lại autorun. Kiểm tra đường dẫn có dấu cách.
- [ ] Thoát khi chụp/countdown hủy an toàn; thoát khi quay dừng và lưu MP4, đủ mic/âm thanh máy; +2/+3/+4 vẫn hoạt động.
- [ ] Cấu hình JSON hỏng/schema 99 được giữ nguyên; Khôi phục mặc định tạo backup rồi lưu mới. Thử thư mục dữ liệu read-only báo lỗi không giả thành công.
- [ ] Update V1.4 → V1.5 bằng feed thật; restart giữ dữ liệu và tray/hotkeys không cản đóng app. V1.5 → bản thử cao hơn: lỗi apply giữ app dùng được; backup có capture-workflow.json. Không publish bản thử lên kênh stable.

## Checklist các phiên bản trước

# Checklist kiểm chứng Windows — Captovix V1.2.0

Trạng thái ban đầu: **CHƯA CHẠY TRÊN WINDOWS THẬT**. Người kiểm thử điền Đạt/Không đạt, máy, GPU, OS, DPI, thiết bị âm thanh và đường dẫn bằng chứng. Không suy ra hiệu năng video từ kết quả build.

## Chuẩn bị

- Dùng Windows x64 với desktop đang đăng nhập; cài qua Setup.exe hoặc giải nén Portable.
- Chụp thông tin máy bằng nút Báo cáo máy & màn hình trong Editor.
- Với lần đầu, thử SDR, một màn hình, 30 FPS; mở Notepad và phát một file âm thanh mẫu.
- Nếu thiếu mic/quyền, mở Windows Settings > Privacy & security > Microphone > Let desktop apps access your microphone. Không cần chạy app bằng quyền admin cho tác vụ thông thường.
- Mở thư mục nội dung để kiểm tra kích thước PNG, phát MP4 trong trình phát độc lập. Đánh dấu âm thanh thực tế, không chỉ dựa vào nhãn Mic bật.

| Mã | Trường hợp | Kết quả mong đợi | Thực tế |
|---|---|---|---|
| INS-01 | Máy chưa có .NET Desktop Runtime; cài Setup | App mở bằng runtime đi kèm, có shortcut và mục gỡ | Chưa chạy |
| INS-02 | Máy thiếu VC Runtime | Bộ cài chạy runtime Microsoft có UAC, hoặc dừng rõ ràng khi hủy/lỗi | Chưa chạy |
| INS-03 | Giải nén Portable | App mở khi đủ VC Runtime; không cần SDK | Chưa chạy |
| INS-04 | Gỡ cài đặt | Xóa app/shortcut; giữ media và cấu hình | Chưa chạy |
| CAP-01 | Vùng 800×600 trên màn chính | PNG 800×600; cạnh vùng khớp nội dung | Chưa chạy |
| CAP-02 | Kéo ngược từ phải dưới sang trái trên | Ảnh đúng vùng, không âm kích thước | Chưa chạy |
| CAP-03 | Esc/chuột phải khi chọn vùng | Hủy, trở về cùng Editor, không thêm file lịch sử | Chưa chạy |
| CAP-04 | Toàn màn hình đã chọn | PNG đúng độ phân giải vật lý màn đó | Chưa chạy |
| CAP-05 | Chọn cửa sổ Notepad | WGC chụp đúng cửa sổ, không lấy cửa sổ Editor | Chưa chạy |
| CAP-06 | Cửa sổ đóng/thu nhỏ trước capture | Thông báo lỗi rõ; không thêm bản thành công | Chưa chạy |
| DPI-01 | Màn hình 100%, 125%, 150%, 200% | Overlay đúng toàn màn; vùng PNG khớp pixel vật lý | Chưa chạy |
| DPI-02 | Hai màn hình khác scale | Chụp riêng từng màn không lệch tọa độ | Chưa chạy |
| DPI-03 | Màn phụ ở bên trái, tọa độ X âm | Chụp đúng vùng trên màn phụ | Chưa chạy |
| DPI-04 | Màn phụ ở trên, Y âm | Chọn vùng chính xác | Chưa chạy |
| DPI-05 | Kéo vùng qua hai màn hình | Các phần ghép đúng vị trí, không nhân scale hai lần | Chưa chạy |
| DPI-06 | Di chuyển Editor qua hai DPI, chọn 100% pixel | Ảnh 100% hiển thị theo pixel màn hiện tại | Chưa chạy |
| REC-01 | Quay 60 giây không âm thanh | MP4 có hình, đúng nguồn, phát được | Chưa chạy |
| REC-02 | Bật mic, tắt tiếng máy | Nghe được giọng nói; không trộn loopback điện tử | Chưa chạy |
| REC-03 | Tắt mic, bật tiếng máy | Nghe được file âm thanh đang phát | Chưa chạy |
| REC-04 | Bật mic + tiếng máy, dùng tai nghe | Hai nguồn nghe được, không lệch tăng dần sau 10 phút | Chưa chạy |
| REC-05 | Không có âm thanh phát trong 30 giây rồi phát lại | Không treo, âm thanh tiếp tục khi có tín hiệu | Chưa chạy |
| REC-06 | Pause 10 giây rồi Resume | Khoảng dừng bị loại; hình/âm không lệch sau resume | Chưa chạy |
| REC-07 | Ctrl+Shift+3 khi Editor ẩn | Dừng, chờ hoàn tất, mở video trong cùng Editor | Chưa chạy |
| REC-08 | Thanh điều khiển nằm trong vùng quay | Thanh không hiện trong video trên Windows hỗ trợ exclusion | Chưa chạy |
| REC-09 | Kích thước vùng lẻ | Xuất kích thước chẵn, không stretch; bỏ tối đa 1 px cạnh phải/dưới | Chưa chạy |
| REC-10 | Tắt GPU encoding rồi quay | Có thể quay software nếu máy đủ khả năng; lỗi thì báo | Chưa chạy |
| REC-11 | Quay 30 phút, ghi CPU/RAM/GPU, FPS thực tế | Kết thúc được, âm/hình đồng bộ; ghi số đo thực tế | Chưa chạy |
| ERR-01 | Đổi/rút thiết bị âm thanh giữa phiên | Kiểm tra hành vi, không tự kết luận hỗ trợ; lưu log nếu lỗi | Chưa chạy |
| ERR-02 | Rút màn hình đang capture | Lỗi được ghi nhận, app trở lại Editor | Chưa chạy |
| ERR-03 | Ổ đĩa đầy hoặc không ghi được | Không báo thành công, có thông tin lỗi | Chưa chạy |
| ERR-04 | Kill app giữa phiên (trên dữ liệu thử) | File pending không được liệt kê như bản hoàn chỉnh khi mở lại | Chưa chạy |
| LIB-01 | Chụp 3 ảnh, quay 2 video, mở lại app | Cùng một Editor; lịch sử/cấu hình còn đúng | Chưa chạy |
| LIB-02 | Tìm tên, đổi tên, xuất JPEG/PNG/MP4 | Kết quả/file xuất đúng; bản gốc giữ nguyên | Chưa chạy |
| LIB-03 | Xóa media có xác nhận | File và metadata được xóa thống nhất | Chưa chạy |
| LIB-04 | File bị xóa bên ngoài | App báo thiếu file và không crash | Chưa chạy |

## Ghi nhận kết quả một ca

Mã ca:
Ngày / Người kiểm thử:
OS / build Windows:
GPU / driver:
Màn hình / độ phân giải / scale:
Nguồn âm thanh:
Đạt / Không đạt:
Mô tả thực tế, độ lệch âm hình nếu có:
File mẫu / báo cáo diagnostics / recorder.log:

Chỉ kết luận bản chạy ổn định sau khi các ca capture/DPI/âm thanh cốt lõi đạt trên máy mục tiêu hoặc có giới hạn đã ghi nhận.

## Bổ sung Unified Editor V1.0

Tất cả ca dưới đây CHƯA được chạy trên Windows desktop trong phiên bàn giao.

| Mã | Thao tác | Kết quả cần đạt | Thực tế |
|---|---|---|---|
| UI-01 | Mở app mới, đóng/mở lại | Cửa sổ luôn mở Maximized, có titlebar và nút Windows | Chưa chạy |
| UI-02 | Chụp/quay từ Editor maximized | Sau capture Editor vẫn maximized | Chưa chạy |
| UI-03 | Thu cửa sổ về Normal rồi chụp | Trở lại Normal theo lựa chọn hiện tại; lần khởi động app mới vẫn Maximized | Chưa chạy |
| EDIT-01 | Vẽ đủ 6 loại chú thích | Hiển thị đúng; từng đối tượng còn chọn/sửa được | Chưa chạy |
| EDIT-02 | Chọn, kéo, kéo tay nắm | Đối tượng/đầu mũi tên/khung di chuyển đúng ở các zoom | Chưa chạy |
| EDIT-03 | Sửa chữ tiếng Việt nhiều dòng | Nội dung và cỡ/màu đúng, gõ không tạo đối tượng ngoài ý muốn | Chưa chạy |
| EDIT-04 | Click đặt nhiều số bước, sửa số | Tự tăng đúng, giá trị 1–9999 | Chưa chạy |
| EDIT-05 | Undo/redo vẽ, kéo, sửa, xóa | Hoàn tác/làm lại đúng từng thao tác, không đổi ảnh gốc | Chưa chạy |
| EDIT-06 | Crop nối tiếp rồi khôi phục khung ảnh | Không lệch tọa độ chú thích; vùng ngoài crop lấy lại được | Chưa chạy |
| EDIT-07 | Đổi ảnh rồi mở lại; đóng/mở app | Crop/chữ/đối tượng đã lưu vẫn sửa được, không chỉ là ảnh phẳng | Chưa chạy |
| EDIT-08 | Copy/xuất PNG/JPEG ở zoom 25%,100%,200% | Kích thước và chú thích xuất giống nhau, không có tay nắm/viền chọn | Chưa chạy |
| EDIT-09 | Xuất vào đường dẫn ảnh gốc trong thư viện | App yêu cầu đường dẫn khác, không ghi đè ảnh gốc | Chưa chạy |
| EDIT-10 | Tạm ngừng gõ 2 giây | Nhãn Đã lưu; mở lại có đủ chữ | Chưa chạy |
| EDIT-11 | Chặn quyền ghi thư mục Edits, thử lưu/chuyển ảnh | Báo lỗi, giữ nội dung trong Editor, không đóng/chuyển mất chỉnh sửa | Chưa chạy |
| EDIT-12 | Sửa file gốc bên ngoài rồi mở lại | Không áp nhầm chú thích, giữ .invalid backup và có cảnh báo | Chưa chạy |
| MIG-01 | Có dữ liệu V0.1, mở Captovix lần đầu | Lịch sử/cấu hình được nhập; file media cũ không di chuyển/sửa | Chưa chạy |
| MIG-02 | Có dữ liệu Captovix rồi mở lại | Không ghi đè bằng thư viện cũ | Chưa chạy |

Chạy kiểm tra renderer tự động trên Windows: `dotnet run --project tests/EditorWindowsChecks/EditorWindowsChecks.csproj -c Release -p:Platform=x64`.


## V1.0.1 — Update Foundation (kiểm thử bổ sung bắt buộc trên Windows)

- [ ] Đóng V1.0, cài Setup V1.0.1, xác nhận một mục Captovix mới trong Apps & Features, shortcut mở bản 1.0.1. Thử cả đường cài V1.0 mặc định và có khoảng trắng.
- [ ] Bộ cài chặn khi V1.0/V1.0.1/Capture Foundation đang mở; không buộc tắt bản đang quay.
- [ ] Ảnh/video/chú thích/settings V1.0 vẫn còn; hash media không đổi. Lịch sử V0.1 còn mở được ở đường cũ.
- [ ] Nếu cài Velopack lỗi/hủy prerequisite, V1.0 chưa bị gỡ. Nếu gỡ V1.0 lỗi, bản mới còn mở được và có thông báo.
- [ ] Máy sạch thiếu VC++: thành phần Microsoft có hỏi UAC; từ chối không báo cài thành công. Máy có VC++: app không đòi admin cho bản cài per-user.
- [ ] Editor Maximized khi mở và sau update; capture rồi khôi phục trạng thái cửa sổ.
- [ ] Chưa có nguồn: trạng thái rõ ràng, không truy vấn URL giả. Nhập nguồn HTTPS/GitHub hợp lệ và lưu.
- [ ] Tắt/tăng thời gian kiểm tra mỗi ngày; không tự tải/cài. Nguồn mới được kiểm tra độc lập; nút thủ công dùng được.
- [ ] Mất mạng, URL404, JSON sai, GitHub giới hạn request: thông báo trong panel và vẫn dùng app; không khóa capture.
- [ ] Nguồn thử riêng có V1.0.2 stable: thấy version/notes. Cùng version/thấp hơn/beta/AppId khác không được áp dụng.
- [ ] Tải nền trong khi sửa ảnh/quay; cancel/retry/hoãn; ấn nhiều lần không tạo hai tác vụ.
- [ ] SHA-256/kích thước sai không chuyển Ready. Không tự áp dụng cache khi đóng/mở app.
- [ ] Đang quay: không áp dụng bản mới; chờ finalize MP4. Không có video pending được báo thành công.
- [ ] Ready: sửa chữ ngay trước khi bấm restart; bản mới mở lại đúng ảnh và chữ vừa lưu. Metadata/Edits backup đọc được, media hash không đổi.
- [ ] Giả lập ổ đầy/không ghi được data hoặc Backups: app không thoát và không gọi áp dụng.
- [ ] Tải xong rồi đóng app: mở lại không tự cập nhật. Kiểm tra/tải lại có kiểm tra integrity, sau đó restart mới áp dụng.
- [ ] Bản mới lỗi/locked file: xem hành vi và log Velopack, không khẳng định rollback metadata tự động.
- [ ] Portable mở nguồn tải thủ công; không có Apply. Install.cmd chỉ chuẩn bị runtime, không cài vào Programs.
- [ ] Gỡ bản Velopack bằng Windows Settings: binaries/shortcut được gỡ, data root còn nguyên.

Nguồn test V1.0.2 chỉ dùng repository thử nghiệm, không publish lên stable của người dùng. Đánh dấu kèm OS, ngày, phiên bản, URL nguồn thử và log khi bàn giao QA. Những ô trên chưa được chạy trong môi trường Linux tạo gói.


## V1.0.2 — Installer Recovery

- [ ] Đóng đúng CoreSetup.exe bị kẹt từ V1.0.1 trước khi chạy bản mới.
- [ ] Cài trực tiếp V1.0.0 → V1.0.2: không còn EXE_ARGS panic, có cửa sổ tiến trình lõi, hoàn tất và mở V1.0.2.
- [ ] Thử máy sạch, máy có V1.0.1 hoàn tất, và đường cài có khoảng trắng/tài khoản Unicode.
- [ ] Log mới ở HuyVo/Captovix/Logs/setup-core.log, log lần trước được giữ.
- [ ] Hủy/thất bại core setup không gỡ V1.0; sau thành công gỡ đúng NSIS cũ, giữ dữ liệu và shortcut bản mới.
- [ ] Khi app đã tự mở, nút Hoàn tất không hiện cảnh báo app đang chạy/mở bản thứ hai.
- [ ] Nâng V1.0.1 → V1.0.2 qua nguồn thử riêng; app vẫn mặc định Maximized, chú thích và media cũ còn nguyên.

Các ô trên cần chạy bằng Windows thật; kiểm tra tĩnh hợp đồng lệnh không thay thế kiểm thử cài đặt.

## V1.0.3 — Default Update Source

- [ ] Máy sạch: cài V1.0.3, mở Cập nhật thấy đúng nguồn huyvd250797/captovix-releases; không phải nhập URL.
- [ ] Máy V1.0.2 đã lưu nguồn chính thức: Publish V1.0.3, kiểm tra/tải/khởi động lại và xác nhận version, thư viện/chú thích.
- [ ] Nguồn tùy chỉnh, AutoCheck=false hoặc nguồn rỗng đã lưu được giữ nguyên.
- [ ] Cùng version báo mới nhất; mất mạng không chặn Editor.

## V1.1 — Image Annotation

- [ ] Máy V1.0.3: Publish V1.1.0 stable, kiểm tra/tải/apply/restart, thư viện cũ còn nguyên và Editor tối đa cửa sổ.
- [ ] Mở sidecar V1.0, thêm 1 hiệu ứng/lưu, đóng mở lại; tài liệu schema 2 và các đối tượng cũ đều còn.
- [ ] Blur/pixel/cover với nền PNG có alpha, vùng sát mép, vùng rộng qua mốc tile 512 px, crop đi qua vùng và zoom 25/100/200%.
- [ ] PNG/JPEG/copy/thumbnail có vùng đã xử lý, không có tay nắm; Cover đè được text và vùng blur chồng lấp.
- [ ] Shift/Ctrl+click, marquee, Ctrl+A; kéo/xóa/nudge/đổi preset/căn nhóm và undo/redo.
- [ ] Click chọn không làm đối tượng tự bắt dính. Escape/mất capture chuột hủy kéo; click nội dung giữa khung có thể chọn đối tượng bên trong.
- [ ] 8 tay nắm; Shift khóa hướng/tỷ lệ; chữ/số kéo góc; mũi tên đổi hai đầu; giới hạn cỡ chữ; DPI 100–200% trên nhiều màn hình.
- [ ] Preset riêng lưu/xóa/mở lại; tên trùng/quá 20/bộ preset lỗi không ghi đè dữ liệu trước.
- [ ] Khi focus ô nhập hoặc ComboBox, phím nhập/chọn không kích hoạt vẽ/nudge ngoài ý muốn.
- [ ] Kiểm tra capture/video/updater hiện có sau các thao tác chỉnh sửa ảnh.

Các kiểm tra Windows này chưa được thực thi trong môi trường Linux đóng gói.

## V1.2 — Image Editor UX / Install location

- [ ] Con lăn zoom tại con trỏ khi ảnh nhỏ/lớn hơn viewport; giữ đúng vùng trên ảnh, giới hạn zoom, scrollbar xuất hiện/ẩn.
- [ ] Space+drag ở vùng trống/ảnh/crop; nhả Space trước chuột, nhả chuột trước Space, Escape, mất focus; không để kẹt cursor/capture. Space trong textbox và wheel bên phải vẫn theo controls.
- [ ] Crop kéo cả 8 tay nắm, move, Shift, nhập số, ảnh đã crop, mép ảnh, kích thước nhỏ, phím mũi tên, X/Y gốc, 100/150/200% DPI.
- [ ] Crop chờ không tự lưu; áp dụng/cancel/undo/redo; Copy/Xuất áp dụng đúng vùng; hủy Save dialog giữ vùng chờ.
- [ ] Alt+click đổi lớp với chữ/hình/che kín chồng nhau; select/move/resize/preset cũ vẫn hoạt động.
- [ ] Cài mới ở D:\Apps\Captovix, D:\Ứng dụng\Captovix: launch/shortcut/uninstall registry đúng thư mục, update tiếp tại D.
- [ ] Gốc ổ, UNC/ổ tháo rời, thư mục không có quyền, thư mục có dữ liệu, thư mục dữ liệu/cha/con đều bị từ chối; không xóa file có sẵn.
- [ ] Bản đã cài ở C được nhận diện; đổi D khi chưa gỡ hiển thị hướng dẫn, chưa tạo bản thứ hai. Gỡ trong Windows Settings rồi cài D vẫn mở được thư viện/chú thích.
- [ ] Updater 1.1.0→1.2.0 trên nguồn thử riêng: version, dữ liệu, default URL, shortcut và đường dẫn cũ được giữ.

Chưa thực thi các ca Windows này trong môi trường đóng gói Linux.

## V1.2.1 — Đường dẫn chưa tồn tại

- Từ Developer Command Prompt có NSIS: `makensis tests\InstallerPathChecks\InstallerPathChecks.nsi`, chạy `tests\InstallerPathChecks\InstallerPathChecks.exe`, kiểm tra exit code 0 và `%TEMP%\Captovix-InstallerPathChecks-result.txt` (12 cases, failures=0). Harness chỉ kiểm tra chuẩn hóa, không cài app/tạo thư mục D.
- Trên máy chưa cài hoặc sau khi gỡ bản managed: xóa/chọn một thư mục kiểm thử trống, nhập D:\Captovix\Captovix khi chưa tồn tại → cài thành công, app/Update.exe/shortcut đúng vị trí.
- Lặp lại với thư mục có sẵn nhưng trống, có dấu/khoảng trắng, gốc ổ, thư mục có dữ liệu, quyền ghi thiếu và chồng lấn vùng dữ liệu.
- Đã cài tại C: chọn D khi chưa gỡ phải hướng dẫn gỡ, không tự di chuyển; sau khi gỡ phải cài được tại D và ảnh cũ còn nguyên.

## V1.3 — Advanced Annotation

1. Tạo chữ tiếng Việt/nhiều dòng, double-click/F2, Ctrl+Enter, Escape, click ra ngoài, Ctrl+S, đổi ảnh, copy/xuất và đóng app khi còn gõ. Xác nhận nội dung cuối đúng, một lần hoàn tất undo một bước, Esc không đổi tài liệu.
2. Callout: kéo theo bốn hướng, sửa chữ dài, đổi màu/cỡ/nét, kéo đuôi sang bốn cạnh/vào hộp, resize/move ở các mức zoom và crop. Không tràn chữ ra ngoài hộp.
3. Freehand: nét ngang/dọc/chéo/chấm, Esc hủy, mất capture hủy, zoom rồi select theo nét, move/resize/duplicate, lưu/mở lại và xuất. Thử nét dài đạt giới hạn; bộ nhớ không tăng không giới hạn.
4. Nhóm nhiều loại: click/Shift chọn/marquee/Ctrl+A, move/resize/align/delete/duplicate/ungroup; undo/redo rồi mở lại. Bản sao phải độc lập với nhóm gốc.
5. Các lệnh trước/sau và Alt+click lớp dưới phải nhất quán với ảnh xuất; thử vùng che cùng callout/nét vẽ.
6. Chia đều ngang/dọc với kích thước khác nhau và nhóm; ít hơn 3 hoặc không đủ chỗ không thay đổi; hai đầu giữ nguyên.
7. Mở file schema 1/2, sửa rồi lưu thành schema 3, đóng/mở lại. So sánh ảnh gốc, JSON và PNG/JPEG. Thử cập nhật từ 1.2.1 và xác nhận nguồn mặc định/dữ liệu/bộ cài ổ D.

Cross-build không thay cho thực thi các thao tác này trên Windows.

## V1.4 — Image Transform & Styling

1. Xoay 0/90/180/270 rồi lật ngang/dọc ở ảnh không đối xứng có chữ, callout, nét và mask. Click/drag/resize/Alt+click phải khớp vị trí sau xoay. Đuôi và điểm nét đi theo hình.
2. Double-click/F2 sửa chữ tiếng Việt nhiều dòng ở 16 tổ hợp xoay/lật; Escape/Ctrl+Enter/click ngoài/đóng app/copy/xuất. Không lạc vị trí hoặc mất nội dung.
3. Resize giữ/bỏ tỷ lệ; kiểm tra số pixel đầu ra PNG/JPEG, 100%, zoom/pan và thumbnail. Xoay sau resize phải đổi rộng/cao; crop số vẫn dùng pixel gốc.
4. Canvas lớn/nhỏ với cả 9 neo; phần tràn bị cắt nhưng khôi phục canvas phải lấy lại đủ ảnh/chú thích. Không tạo annotation trên đệm/viền/khoảng canvas trống.
5. Viền 0/1/128 px, đệm 0/512 px; nền trong suốt/màu; mẫu Sáng/Tối/Bỏ kiểu; bóng các dấu offset và blur 0/80. Bóng không bị cắt ở cạnh đầu ra.
6. PNG alpha đúng, JPEG ghép trắng, clipboard/thumbnail cùng bố cục. Vùng che luôn phủ trên chữ/nét ở mọi tổ hợp biến đổi.
7. Nhập số sai, màu sai, kích thước vượt giới hạn: không thay tài liệu. Reset/undo/redo/mở lại phải khôi phục toàn bộ cấu hình và giữ ảnh gốc.
8. Mở sidecar schema 1/2/3, sửa rồi lưu schema 4; nâng cấp từ V1.3, kiểm tra backup/URL phát hành, thư mục D và khởi động maximized.
