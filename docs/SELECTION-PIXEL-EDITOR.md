# Captovix V1.12.0 — Selection & Pixel Editing

## Editor rộng và nhóm chức năng

Một Editor Windows duy nhất, mở maximized. Thanh menu ngang có Tệp, Chỉnh sửa, Chụp & quay, Hiển thị, Công cụ, Trợ giúp; các nhóm có menu con. Cấu hình chụp/quay, preset capture, phím tắt/chạy nền, cập nhật, thông tin nội dung và máy/màn hình được chuyển vào hộp thoại có cây nhóm bên trái. Chọn module để xem nội dung; đóng hộp thoại giữ các cấu hình đã lưu. Workflow/hotkey và nguồn update vẫn có nút Lưu vì phải xác minh hoặc đăng ký với Windows; thuộc tính chỉnh ảnh vẫn tự áp dụng. Các control được dùng lại, không tạo một bộ cấu hình thứ hai.

Thanh phải chỉ giữ công cụ/đối tượng, Layers, hình học, viền/nền/bóng, callout và pixel. Layers mặc định thu gọn; có thể ẩn/hiện thanh thuộc tính từ Hiển thị. Thanh chụp/quay và thanh công cụ dùng nút gọn; gợi ý dài chuyển thành tooltip. Không gian dành cho ảnh tăng theo chiều cao; khi dùng thư viện dưới, chiều rộng ảnh không phải chia với cột thư viện.

Thư viện: Hiển thị → Thư viện → Dọc bên trái / Ngang phía dưới, hoặc icon ⇄ ở đáy thư viện. Thumbnails dùng danh sách cuộn theo hướng tương ứng, giữ thư viện và ảnh đang chọn. Bố cục được nhớ trong editor-layout.json. Các thao tác xóa, khôi phục lần xóa vừa rồi và mở thư mục là icon ở đáy; có tooltip. Xóa tiếp tục chuyển vào Trash, không xóa vĩnh viễn.

## Gộp công cụ

- Mũi tên gồm nét thẳng/gấp khúc/cong và các mẫu đầu mũi. Chọn Không đầu · đường để vẽ đường, kể cả hai đầu đều không có mũi. Không có nút Đường riêng. L vào nhóm Mũi tên với đầu None; A vào nhóm Mũi tên.
- Khung gồm chữ nhật, bo góc, ellipse và các hình kín hiện có. Không còn nút Hình dạng / Ellipse / Khung trùng nhau. R chọn chữ nhật, E chọn ellipse, S tiếp tục hình đang dùng trong nhóm Khung.
- Bỏ nút Che kín, phím K và lựa chọn tạo Cover mới từ inspector. Preset mặc định thay bằng Khung nền đặc. Các đối tượng Cover đã lưu và preset riêng cũ vẫn đọc, hiển thị và bảo vệ như trước; không tự đổi sang một hình thường khi mở ảnh cũ.
- Khung tô nền là đối tượng thường: màu/độ đậm/thứ tự lớp ảnh hưởng phần nhìn thấy. Xem ảnh đã xuất trước khi chia sẻ nội dung đã che. Ảnh gốc và source dự án vẫn chứa nội dung gốc.

## Callout trực quan

10 mẫu xem trước bằng màu đang dùng: hộp có đuôi, bo góc có đuôi, bầu dục có đuôi, hộp, hộp bo góc, viên thuốc có đuôi, viên thuốc, bầu dục, thoi có đuôi và lục giác có đuôi. Bấm mẫu để dùng cho nét mới hoặc đổi callout đang chọn; không cần chọn combobox. ☆ lưu nhanh mẫu callout như mẫu hình/nét.

Callout mới dùng khung thân ổn định để chọn/resize/xoay. Với mẫu có đuôi, chọn callout rồi kéo tay nắm tại đầu đuôi sang bất kỳ hướng nào, kể cả ngoài ảnh; thân không bị kéo theo chỉ vì đổi đuôi. Điểm nối với thân tự bám biên; mẫu không đuôi không hiện tay nắm đuôi. Đưa đầu đuôi vào thân thì đuôi không hiện; kéo ra lại để hiện. Đuôi vượt ảnh/crop bị cắt khi xuất. Chữ vẫn sửa tại ảnh, có font/viền/nền/căn chỉnh và Undo.

Callout cũ giữ quy ước pivot cũ để không đổi vị trí hiển thị khi mở lại. Flag CalloutBodyPivot chỉ bật mặc định cho callout mới. Phiên bản này có một đuôi và độ rộng đuôi tự tính; chưa có nhiều đuôi, đuôi cong hoặc tay nắm độ rộng gốc đuôi.

Tham khảo nguyên lý điều khiển qua tay nắm trong tài liệu TechSmith: https://assets.techsmith.com/Docs/pdf-snagit/Snagit_12.4.1_Help.pdf . Captovix tự viết geometry, gallery, icon và mã nguồn; không nhập asset/giao diện/định dạng dự án của Snagit.

## Chọn và chỉnh pixel

Chọn Vùng pixel trên toolbar hoặc Chỉnh sửa → Vùng pixel. Đổi chữ nhật / ellipse / lasso trong inspector. Kéo để chọn; lasso kéo đường bao rồi thả để đóng vùng. Kéo trong vùng đã chọn để di chuyển vùng chọn, không di chuyển pixel ngay. Vùng tối thiểu 2 × 2 px, lasso từ 3 điểm, tối đa 8192 điểm. Pixel thuộc vùng theo tâm pixel; mép cứng, chưa feather/antialias selection. Đổi loại vùng hoặc đổi ảnh sẽ bỏ vùng cũ. Chọn V để trở lại chế độ đối tượng.

- Ctrl+C hoặc Copy: copy vùng hiển thị gồm các chú thích/vùng che đang hiện, ở số pixel nguồn; không lấy zoom/viền toàn ảnh vào tọa độ vùng. Ngoài ellipse/lasso được trong suốt.
- Ctrl+X hoặc Cắt: copy vùng hiển thị, sau đó xóa pixel ảnh nền trong vùng. Chú thích và lớp ảnh vẫn là đối tượng riêng và không bị xóa bởi Cắt. Nếu muốn xóa chúng, dùng chế độ Chọn và Delete.
- Tô: dùng màu tô vùng trong inspector, tác động ảnh nền.
- Delete hoặc Xóa pixel: xóa ảnh nền trong vùng thành trong suốt, không tô trắng. PNG giữ alpha; JPEG ghép trên trắng theo quy tắc xuất đã có.
- Ctrl+V: dán ảnh clipboard thành lớp ảnh riêng, giữ số pixel, chọn/kéo/resize/xoay, nhóm/khóa/ẩn, sắp xếp và xóa như đối tượng. PNG clipboard được ưu tiên giữ alpha; ứng dụng nguồn chỉ cung cấp Bitmap thì dùng Bitmap. Nếu clipboard là đối tượng Captovix, tiếp tục dán đối tượng; đang gõ trong ô chữ vẫn dùng copy/paste văn bản.
- Mỗi lần tô/cắt/xóa/dán là một bước Undo/Redo. Ctrl+Shift+C vẫn copy toàn ảnh đã chỉnh. Các thao tác ảnh nền không ghi đè PNG gốc; phần nền đã chỉnh và lớp dán lưu trong sidecar để mở lại được.

Vùng chọn chỉ là trạng thái Editor; không in vào ảnh xuất. Paste có thể vượt crop/canvas, phần vượt bị cắt khi xuất; không tự resize hoặc phóng lớn canvas. Ảnh dán được di chuyển như đối tượng, vùng chọn được di chuyển như đường bao. Chưa chỉnh pixel bên trong từng lớp ảnh riêng; tô/cắt/xóa hiện chỉ tác động nền. Chưa cut-out hàng/cột, magic wand, flood-fill theo màu, feather, content-aware, nhiều vùng cộng/trừ hoặc xóa bằng cọ tự do.

Kính/Blur/Pixel lấy mẫu nền đã chỉnh và các lớp ảnh đang hiện; các Cover cũ tiếp tục được tô vào mẫu trước khi lọc và phủ trên cùng. Chữ/mũi tên không được đưa vào nguồn mẫu của kính như quy tắc cũ.

## Dữ liệu và giới hạn

Schema 11 đọc 1–10. Trước lần lưu sidecar schema cũ, giữ nguyên byte .pre-v1.12.bak; không ghi đè schema tương lai. Mẫu sao V1.11.1 được chuyển hash trong bộ nhớ, kiểm tra bằng hash từ bản V1.11.1 thực; trước khi ghi lại giữ .pre-v1.12.bak. Ảnh gốc, URL stable, AppId/kênh win, hotkey/UIA, DPI và root cài đặt được giữ.

Chỉnh/dán pixel giới hạn 40 triệu pixel/canvas, PNG mã hóa tối đa 96 triệu ký tự/lớp và 128 triệu ký tự/tài liệu. Kiểm tra kích thước trước decode lớp, sidecar được kiểm tra decode khi mở. Lịch sử dùng ngân sách JSON hiện có, ảnh lớn có thể chỉ giữ ít bước Undo. Clipboard đối tượng Captovix giữ giới hạn 4 triệu ký tự; với lớp lớn có thể dùng copy ảnh/vùng qua clipboard PNG. Sidecar ảnh nền/lớp lớn tốn thêm RAM và dung lượng; không phải database hoặc cloud upload. Các thao tác PNG lớn có thể tốn thời gian trên UI. Điều chỉnh hiển thị/zoom không thay đổi dữ liệu pixel.

## Nâng cấp và build

Bản cài trong Update-Packages: HuyVo.Captovix-win-Setup.exe. Đóng app trước khi cài thủ công; chọn đúng root hiện có, ví dụ D:\Captovix\Captovix, không chọn thư mục current. Không cần cài .NET riêng vì gói self-contained; bộ cài có VC++ runtime. Luồng installer/đường dẫn/EXE_ARGS giữ nguyên bản trước.

Để phát hành: giải nén Update-Packages, upload 5 assets vào cùng Release public stable v1.12.0, Set as latest. Không upload nguyên ZIP thay từng file, không dùng Source ZIP làm gói update. Nguồn gắn sẵn: https://github.com/huyvd250797/captovix-releases/releases/latest/download . Bản app cũ v1.9 có thể cần chọn nguồn Thư mục HTTPS URL này hoặc cài Setup thủ công. Lần bàn giao này chưa push/publish GitHub.

Build trên Windows: .NET10 SDK, NSIS3, chạy Build.cmd; scripts/Build-Windows.ps1 chạy cả logic và harness WPF rồi publish win-x64 runtime10.0.13/Velopack1.2.161. Không tự nâng stack trong V1.12. Xem VALIDATION.md và WINDOWS-TEST-CHECKLIST.md để phân biệt các kiểm tra đã chạy với phần cần Windows thật.

## Tiếp theo

V1.13 — Image Composition & Templates: mở/kéo thả thêm nhiều ảnh thành lớp, mẫu bố cục 2/3/4 ô, căn/giãn khoảng cách các ảnh, header/caption, xuất ảnh ghép và lưu bộ tài nguyên dự án. Kế thừa lớp ảnh đã có trong V1.12; video tiếp tục để sau. Chưa triển khai phiên bản này.
