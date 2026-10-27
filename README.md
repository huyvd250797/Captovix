# Captovix V1.26.0 — Smart Element Capture

Nâng cấp từ source `Captovix-v1.25.0-Source(1).zip` Boss gửi. Bản này ưu tiên Smart Element Capture thay cho đề xuất Composition trước đó. Source và gói cài/cập nhật đã xây dựng; nghiệm thu tương tác trên Windows thật còn chờ. Không tự đăng GitHub Release.

## Thay đổi

- Chế độ Vùng và Phần tử dùng cùng một lớp phủ. Ảnh vùng cũng bật nhận diện; video giữ kéo vùng thủ công.
- Rê để gợi ý khối. Con lăn lên / phím ↑ hoặc ←: khối cha; xuống / ↓ hoặc →: khối con. Thứ tự dựa trên cây lồng nhau, có chỉ số cấp.
- Click khóa khối, rồi kéo 8 tay nắm chỉnh cạnh/góc; kéo trong khối đã khóa để di chuyển. Góc kéo tự do; Shift giữ tỷ lệ. Enter chụp một lần, không mở bước xác nhận thứ hai.
- Trước khi khóa, giữ chuột kéo luôn tạo vùng thủ công ngay cả trên khối đang gợi ý; không cần thoát chế độ. Tab bỏ khóa/chọn lại; S bật/tắt nhận diện; Esc/chuột phải hủy.
- Sau khóa: phím mũi tên dịch 1 pixel; Shift + mũi tên 10 pixel; Ctrl + mũi tên thay kích thước (Ctrl+Shift: 10 pixel).
- Gợi ý đổi khối phải có hai kết quả xác nhận liên tiếp; giữ cấp cha/con người dùng đã chọn khi còn phù hợp. Không xóa khung chỉ vì bắt đầu truy vấn. UI Automation chạy trong tiến trình phụ có giới hạn thời gian; bổ sung metadata child HWND cho ứng dụng Win32 cũ. Cửa sổ đổi vị trí phải chọn lại trước chụp.
- Bỏ hai bảng hướng dẫn/nút lớn cố định ở góc trên. Thay bằng một thanh nhỏ có camera + Enter, thông tin kích thước/cấp và tooltip hướng dẫn. Ưu tiên ngoài vùng chụp, tự đổi phía khi sát cạnh. Ẩn toàn bộ thanh khi kéo; khung chỉ giữ tay nắm nhỏ. Khi chọn kín màn hình, không có chỗ ngoài vùng: thanh nhỏ đặt sát đáy, đóng trước lấy ảnh.
- Thanh thư viện dọc/ngang chỉ có **Xóa – Quản lý thư viện – Đổi hướng**. Icon đổi hướng thể hiện bố cục đích, tooltip ghi rõ chuyển ngang hay dọc. Tìm, khôi phục, thư mục lưu và xuất vẫn truy cập qua menu Tệp.
- Pixel ảnh vùng/phần tử/toàn màn hình lấy trực tiếp bằng BitBlt, không StretchBlt. Worker đặt PerMonitorV2 và khôi phục DPI context sau chụp. Kiểm tra lại toàn bộ byte BGRA sau lưu PNG; sai kích thước/pixel thì báo lỗi, không âm thầm resize.
- Giảm khoảng trống quanh ảnh trong Editor từ48 xuống24 DIP để ảnh vừa khung lớn hơn. Ảnh nhỏ mặc định không phóng lớn vượt100% pixel; ảnh lớn vẫn thấy toàn bộ ở chế độ vừa khung. Tooltip zoom giải thích cách kiểm tra ở100%. Zoom không tham gia pipeline xuất.

## Giới hạn và kiểm chứng

Lấy đúng pixel không tạo thêm chi tiết chưa tồn tại trên màn hình. Preview vừa khung dưới100% phải thu nhỏ; đánh giá độ nét ở **Rõ nét · 100%** hoặc mở PNG gốc. Không tự sharpen/upscale vì có thể đổi chữ, viền và QR. UltraViewer/Remote Desktop/emulator có thể đã scale/nén ảnh trước khi Captovix chụp; app không khôi phục pixel gốc của máy từ xa. Màu HDR/wide gamut, nội dung bảo vệ, cửa sổ bị che và provider không hỗ trợ là trường hợp đánh giá riêng. Backend cửa sổ native và quay video giữ nguyên.

UIA phụ thuộc ứng dụng đích. Trình duyệt cần cung cấp cây accessibility; app tự vẽ/remote có thể chỉ cung cấp toàn cửa sổ. Khi đó tắt S hoặc kéo chọn thủ công ngay. Không cam kết nhận diện mọi modal/bảng trong mọi ứng dụng.

Đã chạy kiểm tra logic trên Linux và cross-build WPF. Chưa chạy overlay, UIA ứng dụng thật, GDI/PNG thực tế, clipboard, DPI màn hình thật hoặc installer GUI trên Windows trong lần bàn giao này. Xem `docs/VALIDATION.md` và checklist bên dưới.

## Cách cài / cập nhật

1. Sao lưu thư viện và cấu hình. Đóng Captovix.
2. Giải nén `Captovix-v1.26.0-Update-Packages.zip`, chạy `HuyVo.Captovix-win-Setup.exe` để cài trực tiếp; có chọn đường dẫn theo installer hiện có.
3. Để update từ app: đăng đủ5asset trong ZIP lên Release stable/latest tag `v1.26.0` của `huyvd250797/captovix-releases`: full.nupkg, Setup.exe, RELEASES, assets.win.json, releases.win.json. Không chỉ đăng ZIP hoặc source. Chọn Kiểm tra cập nhật trong app.
4. Source dùng phát triển; không dùng Source.zip làm gói update. Bộ cài chưa ký số. Không ghi đè dữ liệu để xử lý lỗi cài.

## Dựng lại

Giải nén Source ZIP, mở thư mục `Captovix-v1.26.0`. Dùng .NET SDK10.0.100, NSIS3 và vpk1.2.161; runtime self-contained10.0.13 đã pin. Không đổi dependency trong bản này.

```powershell
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
dotnet run --project tests/SmartCaptureChecks -c Release
dotnet run --project tests/ConnectorChecks -c Release
dotnet run --project tests/ImageFidelityChecks -c Release
dotnet run --project tests/ShellLayoutChecks -c Release
dotnet run --project tests/InstallerContractChecks -c Release -- .
# Chạy trên Windows để kiểm tra renderer/PNG (không thay cho bộ test desktop):
dotnet run --project tests/FidelityWindowsChecks -c Release -p:Platform=x64
# Fixture tương tác modal/bảng; mở Captovix riêng để thử chụp:
dotnet run --project tests/SmartCaptureWindowsFixture -c Release -p:Platform=x64
```

Các script build/package Windows có trong `scripts/`. Giữ AppID, baked update-source, schema dữ liệu và logo đã duyệt. Xem hướng dẫn nền bên dưới về build/install/publish; chỉ dẫn phiên bản cũ là lịch sử, version hiện tại1.26.0.

## Nghiệm thu Windows cần thực hiện

| Bộ ứng dụng / điều kiện | Thao tác | Kết quả cần đạt | Trạng thái |
| --- | --- | --- | --- |
| Fixture WPF trong source | Modal, DataGrid, child/parent, UIA | Khối đúng vị trí/pixel, không phát sinh click vào ứng dụng đích | Chờ chạy Windows |
| Edge/Chrome trang có modal và bảng | Rê giữa block, gần cạnh modal; wheel cha/con; khóa rồi resize | Chọn đúng cấp đã chủ động chọn, không nhảy cấp khi đứng yên | Chờ chạy Windows |
| Explorer / ứng dụng Win32 | Cửa sổ, bảng, vùng con; fallback thủ công | Không treo provider; kéo vùng được ngay | Chờ chạy Windows |
| UltraViewer / ứng dụng tự vẽ | Provider chỉ có khung ngoài | Không nhận nhầm widget máy từ xa; kéo chọn bằng tay thành công | Chờ chạy Windows |
| DPI100/125/150/175/200%, màn hình tọa độ âm | Chọn/resize/move xuyên màn hình, Enter/Esc | Tọa độ và PNG đúng pixel; không còn toolbar/shade trong ảnh | Chờ chạy Windows |
| Cửa sổ đích di chuyển trong khi khóa | Di chuyển cửa sổ rồi Enter | Từ chối vùng stale, yêu cầu chọn lại | Chờ chạy Windows |
| GDI pattern và ảnh chữ/QR ở100% | Trợ giúp → Kiểm chứng chất lượng ảnh | PNG đúng pixel, crop/rotate/copy/export không resize ngoài ý muốn | Chờ chạy Windows |
| Thư viện hai hướng, filmstrip96–260DIP | Đổi hướng, kéo splitter, xóa đa chọn | Luôn đúng3nút, icon đích đúng và đủ chỗ | Chờ chạy Windows |
| Update từ1.25.0 và Setup ở ổD | Kiểm tra cập nhật/cài tại D:\Captovix\Captovix | Đúng1.26.0, giữ media/style/theme/logo/phím tắt | Chờ chạy Windows |

## Tiếp theo đề xuất

**V1.27 — Precision Selection & Magnifier:** kính lúp theo trỏ, nhập W×H bằng pixel, khóa riêng chiều rộng/cao, tỷ lệ thông dụng, bắt cạnh có thể tạm bỏ qua. Chỉ triển khai sau khi Boss chọn và sau nghiệm thu lớp phủ1.26 trên bộ ứng dụng mục tiêu.


---

## Tài liệu các phiên bản trước

# Captovix V1.25.0 — Scrolling Capture Quality

## Chụp cuộn

- Tìm overlap hai bước: lấy ứng viên nhanh rồi kiểm tra dày hơn, gồm lân cận, để giảm nhận nhầm điểm nối hoặc khung trùng. Giữ copy pixel nguyên bản, không blend/resize tại mối nối.
- Tự cuộn chờ theo cấu hình, rồi so tối đa3cặp khung cách180ms để kiểm tra nội dung đã đứng yên. Nếu còn chuyển động, tạm dừng; Tiếp tục thử lại vị trí hiện tại trước khi cuộn thêm. Có chế độ lấy thủ công cho ứng dụng khó.
- Header/footer khi cuộn dọc, hoặc dải trái/phải khi cuộn ngang: nhập pixel đầu/cuối, bỏ khỏi phép ghép, tùy chọn giữ dải đầu khung đầu và dải cuối khung cuối đúng một lần. Phiên ngang có vùng cố định mới không mở bằng bản cũ; ảnh kết quả PNG vẫn mở bình thường.
- Khung trùng không append;3lần không có nội dung mới dừng tự động, vẫn cảnh báo có thể nguồn không nhận cuộn. Khung không chắc dừng tự động, yêu cầu xác nhận/sửa overlap trước xuất; kích thước thay đổi bị từ chối.
- Xem cặp khung tại điểm nối bằng vùng cuộn pixel; khung trước mở ở cuối hướng cuộn, khung sau ở đầu. Có toàn ảnh Fit/100%, guide opt-in không vào file xuất. Sửa overlap/loại khung giữ nguồn, revision và phiên hash để mở/ghép lại.
- Giới hạn giữ24khung,4MP/khung,64MPnguồn,40MPkết quả,32768px/cạnh theo pipeline hiện có. Không cam kết trang động, video, tải vô hạn hoặc mọi ứng dụng; không phải công cụ tự đọc toàn DOM.

## Cập nhật và giao diện

- Khi tự kiểm tra đang bật: kiểm tra nguồn khi mở app, không bị mốc đã kiểm24giờ của phiên trước chặn; trong phiên tối đa mỗi30phút. Popup một lần mỗi phiên bản trong phiên chạy, chờ Editor có thể tương tác; Để sau/đóng thu về nút cập nhật, Cập nhật mở trang kiểm tra/cập nhật.
- Tôn trọng tùy chọn tắt tự kiểm tra. Popup chỉ xuất hiện khi nguồn trả về phiên bản cao hơn. Release phải stable/latest và có đủ5asset, không chỉ SourceZIP/UpdateZIP; lỗi mạng/feed hiện trong trang cập nhật và log, không báo sai là đã mới nhất. Chưa tự publish.
- Màn hình trống chỉ giới thiệu Captovix bằng logo/nội dung, bỏ hai dòng shortcut cố định và tooltip cứng. Phím tắt cấu hình vẫn hoạt động theo cài đặt máy.
- Rê viền vùng chụp hiện con trỏ hai đầu theo cạnh/góc; chỉnh xong Enter/nút máy ảnh chụp. Giữ viền nét đứt từ1.24.
- Dải thư viện ngang: nút icon đầy đủ chuyển sang cụm gọn bên cạnh ảnh; bỏ dấu… trong chế độ này, thư viện dọc vẫn giữ overflow. Thanh phân cách có nền và con trỏ resize; kéo thay chiều cao96–260DIP, lưu trên máy; lần đầu từ cấu hình cũ thu về tối đa112DIP. Thumbnail co theo chiều cao, không dùng hàng riêng cho nút.
- Footer căn giữa theo chiều dọc để copyright, trạng thái và nút update thẳng hàng.

## OCR gọn

Mở OCR → kéo vùng trên ảnh → thả chuột tự nhận chữ → sửa/copy kết quả. Mở đầu không phủ tím toàn ảnh. Chỉ giữ ảnh có zoom, chọn toàn ảnh/nhận lại/hủy, ô kết quả và sao chép/đóng. Mặc định Việt+Anh, tự phân tích bố cục, xử lý trên máy. Bỏ danh sách dòng, thay chữ/nền/font, tách lớp, preview/apply và chọn bố cục/ngôn ngữ khỏi giao diện OCR. Không chỉnh bitmap nguồn. Hủy/đóng vẫn chờ worker kết thúc; lỗi hiện thông báo cụ thể.

## Kiểm chứng và cài đặt

6bộ logic/static đạt232assertions:21mới về overlap/native pixel/transpose/vùng cố định/cancellation; thêm4policy kiểm tra cập nhật theo phiên.3bộ WPF cross-build trên Linux; main Windows x64 self-contained.NET10.0.13 build/publish. Chạy UpdateChecks với nupkg thật và Verify-Packages sau đóng gói. **Chưa chạy Windows desktop hoặc nghiệm thu ảnh cuộn thực tế.** Số liệu bản cũ bên dưới là lịch sử.

Nghiệm thu Windows cần: bộ trang mẫu văn bản/bảng dọc-ngang có header/footer, không mất/lặp dòng/cột; trang tải chậm/animation và retry tại cùng vị trí; cuối trang/Cancel/restore/loại khung; mối nối/PNG đúng pixel; popup khởi động và sau30phút/opt-out; resize con trỏ/DPI100–200%/monitorâm; dải ngang thấp/cao và tất cả nút; OCR chọn vùng/zoom/worker/copy; cài/update/logo/dữ liệu. Chưa có screenshot giao diện mới chạy thật. Không suy diễn kiểm tra tổng hợp thành hỗ trợ mọi app.

Giải nén Update-Packages, đóng app, chạy HuyVo.Captovix-win-Setup.exe. Để phát hành: đăng đủ5asset lên huyvd250797/captovix-releases, tagv1.25.0, stable/latest. Không đổi tên/byte. Backup dữ liệu trước nâng cấp; giữ media/settings/logo/stable source. Setup chưa ký số.

## Tiếp theo V1.26 — Image Composition Studio

Ghép ảnh theo hàng/cột/lưới, chỉnh khoảng cách/nền/viền/bóng, tiêu đề và đánh số bước, mẫu bố cục dùng lại. Ưu tiên patch phản hồi Windows1.25 trước.

## Lịch sử phiên bản trước

# Captovix V1.24.0 — Capture Precision

- Chụp vùng: kéo → chỉnh cạnh/góc nếu cần → Enter hoặc nút máy ảnh là chụp, bỏ bước chọn hành động dư. Nét đứt chuyển động chậm báo vùng chưa chụp. Mũi tên cuộn dọc/ngang nằm ngay vùng chọn, nền trong suốt; video chỉ có xác nhận vùng. Esc/chuột phải hủy.
- Giữ nền capture pixel và kiểm topology/DPI sau hẹn giờ. Chọn phần tử cha/con bằng con lăn, ưu tiên cạnh/góc modal/cửa sổ; vùng cố định/gần đây kế thừa nền trước. Độ tương thích UIA phụ thuộc ứng dụng; không cam kết nhận diện mọi UI.
- Quản lý thư viện: cây năm/tháng/ngày, ảnh dạng ô tự xuống dòng theo chiều rộng; tải100ảnh/lần, nút Xem thêm. Tùy chọn Ẩn ngày tháng không có hình bỏ cả tháng trống. Ctrl/Shift chọn nhiều, mở/xóa/xuất giữ nguyên. Bỏ đường vào màn hình quản lý bộ sưu tập cũ; dữ liệu tag/collection giữ, backup/export còn trong menu.
- Chọn công cụ áp dụng mẫu đã lưu gần nhất trong họ công cụ; chọn mẫu cụ thể vẫn ưu tiên mẫu vừa bấm. Đã lưu mở sẵn; sao trên preview chỉ hiện hover/focus. Sắp xếp công cụ bằng kéo-thả trên thanh công cụ hoặc Hiển thị → Sắp xếp không gian → Sắp xếp công cụ cá nhân; thứ tự giữ trên máy.
- Callout vẽ kéo giữ nguyên thân khi thả, tắt tự co chữ cho đối tượng mới để nhập không đổi thân. Bật Tự co trong thuộc tính nếu muốn. Một click giữ kích thước mặc định. Tay nắm đuôi màu cam, tay nắm thân xanh/trắng dạng tròn. Đuôi vẫn tự do.
- Footer/nút update gọn hơn. Giới thiệu ghi: Ý tưởng, định hướng sản phẩm và thiết kế trải nghiệm: Võ Đức Huy (HuyVo). ©2026HuyVo. All rights reserved.

## Cài/update

Giải nén Update-Packages, đóng app và chạy HuyVo.Captovix-win-Setup.exe, hoặc đăng đủ5asset lên GitHub Release stable/latest tagv1.24.0 để app kiểm tra cập nhật. Không đăng chỉ file ZIP. Source ZIP dùng phát triển, không dùng cài. Không tự publish trong lần bàn giao này. Setup chưa ký số.

## Giới hạn và nghiệm thu

Build/publish Windows x64 tự chứa.NET10.0.13; giữ stack/logo/update source/schema và dữ liệu. Đã chạy7bộ logic/static liên quan,620assertions, và cross-build RefinementWindowsChecks; chưa chạy WPF/Windows desktop. Kiểm nupkg và integrity gói riêng sau đóng gói.

Cần Windows nghiệm thu:1Enter/chuột/nút camera, hủy/countdown/quay, arrows cuộn, nhiều DPI100–200%và tọa độ âm; ảnh/clipboard/PNG đúng pixel; thư viện co giãn/lazy/empty month/multi-select/delete; chọn mẫu gần nhất sau restart/chọn mẫu thủ công; callout thân/đuôi/IME/Undo; caption/taskbar/footer/install-update. Không có screenshot UI chạy thật hoặc tuyên bố đã kiểm thị giác. Margins cũ vẫn theo giới hạn1.23.1, chữ không kéo méo glyph hai chiều.

## Tiếp theo V1.25 — Scrolling Capture Quality

Cải thiện mối nối dọc/ngang, vùng header/footer cố định, phát hiện khung lặp/lỗi, bộ trang mẫu để kiểm không mất/lặp dòng/cột. Ưu tiên bản vá phản hồi Windows1.24 trước.

## Lịch sử phiên bản trước

# Captovix V1.23.1 — Resize & Capture UX

- Kéo đối tượng mặc định tự do; chỉ Shift giữ tỷ lệ. Bỏ ép tỷ lệ theo loại đối tượng, giữ giới hạn cỡ chữ và thân callout.
- Chọn vùng pixel chữ nhật/ellipse có tay nắm chỉnh kích thước; vùng chụp màn hình giữ sau thả chuột, kéo lại cạnh/góc hoặc chọn lại, Enter/nút icon máy ảnh xác nhận; Esc hủy.
- Fit ảnh sử dụng nhiều diện tích hơn, không thay pixel xuất.
- Theo dõi phần canvas tự nới riêng; kéo vào trong/xóa đối tượng sẽ tính lại biên toàn bộ annotation nhìn thấy, giữ phần mở rộng thủ công cũ. Những margins đã tạo từ bản1.23.0 chưa có dấu tự động được giữ, dùng Crop để bỏ một lần.
- Popup update khi app hoạt động lại; Cập nhật mở trang cập nhật, không tự tải ngay; Để sau/đóng dùng animation về nút cập nhật. Animation tôn trọng thiết lập Windows.
- Ẩn mục Preset cũ và Trình bày toàn ảnh ở thanh phải; dữ liệu mẫu cũ giữ nguyên. Giữ công cụ ngang, thuộc tính bên phải và tùy chọn thư viện nằm dưới; nhận diện tím Captovix.

Chưa nghiệm thu Windows thật: resize/text/IME, capture đa màn hình/DPI, taskbar, animation, install/update. Không tuyên bố giao diện sao chép hoặc tương đương hoàn toàn Snagit. Resize chữ giữ glyph theo font, không kéo méo glyph theo hai trục; không có affine text deformation.

# Captovix V1.23.1 — Text & Callout Refinement

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161 · Tesseract5.2.0 x64 + model Việt/Anh kèm app.

Captovix = Capture + Tools + Visual + eXperience. Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.

## Chức năng mới

- Ảnh mới/mở lại mặc định Vừa khung. Vùng thao tác quanh ảnh có nền workspace, không hiển thị checkerboard như một khung ảnh lớn; kích thước xuất vẫn theo pixel ảnh/canvas thực.
- Vẽ hoặc kéo chú thích ra ngoài sẽ nới đúng cạnh bị vượt theo biên render, gồm viền, đuôi và bóng. Một callout ở trên không tự thêm viền trái/phải/dưới; nới và đối tượng chung một Undo.
- Sửa chữ trực tiếp trên ảnh bằng ô nhập trong suốt, không nền đen/viền ô nhập, không chồng hai lớp chữ. Ctrl+Enter hoàn tất, Enter xuống dòng, Esc hủy; bấm ngoài hoàn tất và về Chọn. Ctrl+B/I/U đổi kiểu toàn đối tượng.
- Callout co chiều cao theo chữ và đệm, giữ rộng người dùng chọn khi đủ chỗ; đuôi tự do giữ đích khi đổi nội dung. Mẫu và ảnh dùng chung hình học/đệm/giãn dòng/căn lề; đuôi bo tròn nối theo biên thật.
- Thanh phải chia thẻ Màu & kích thước → Hình dạng & kiểu nét → Chữ & bố trí → Hiệu ứng; cài đặt sâu gấp gọn. Font gõ/tìm được, B/I/U dạng nút, mẫu đã lưu theo công cụ.
- Tên ảnh/thời gian gọn, ngày đầy đủ trong tooltip; icon có khoảng đệm. Menu nhóm theo thao tác thực tế. Caption cả ba nút có hover; cửa sổ tối đa theo work area Windows để footer không nằm dưới taskbar.

Hướng dẫn/giới hạn: docs/TEXT-CALLOUT-REFINEMENT.md.

## Cài / cập nhật

Giải nén Captovix-v1.23.1-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.23.1, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

## Build từ source

Cài .NET10 SDK và NSIS3 trên Windows. Mở PowerShell tại thư mục source:

```powershell
dotnet restore src/Captovix/Captovix.csproj
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả ở artifacts. scripts/Run-Checks.py chạy các bộ logic và thực thi WPF trên Windows; trên Linux chỉ cross-build WPF. Đóng app trước cài/update. Không thêm dấu -- trước --installto trong installer vì gây lỗi EXE_ARGS.

## Dữ liệu

%LOCALAPPDATA%\\HuyVo\\Captovix: Media, Edits, library.json, appearance-settings.json, recent-capture-regions.json, ScrollSessions, OcrJobs, media-location.json, Brand, branding.log, capture.log và errors.log. Thư mục media có thể chọn riêng cho file mới; file cũ giữ đường dẫn. Metadata/Edits không chuyển theo media. Giữ dữ liệu khi update; sao lưu toàn thư mục trước nâng cấp/rollback. Tài liệu Editor đọc schema12/13/14; schema phụ thuộc chức năng đã dùng. Phiên cuộn mới schema2 đọc phiên1; app1.14 không mở phiên2. Khi loại khung, phiên gốc vẫn giữ và revision là thư mục mới. Không tự xóa frame nguồn; dọn phiên đã hết nhu cầu bằng Explorer.

## Kiểm tra và tiếp tục

29 bộ logic/static đạt1910 assertions;18 bộ WPF chỉ cross-build trên Linux. Main Windows x64 build/publish và nupkg/feed/installer/ZIP có kiểm riêng. Chưa chạy Windows desktop: UI/IME/taskbar/DPI/capture/install-update cần nghiệm thu theo docs/TEXT-CALLOUT-REFINEMENT.md và docs/VALIDATION.md.

Tiếp theo **V1.24 — Capture Precision**: làm rõ chọn phần tử cha/con, vùng cố định/gần đây, preview tọa độ/kích thước pixel, kiểm capture nhiều màn hình và các mức scale. Chỉ cam kết loại app đã nghiệm thu; UIA/native fallback cần đánh giá theo ứng dụng. Ưu tiên patch phản hồi Windows1.23 trước. Gửi SourceZIP1.23 này để tiếp tục.
