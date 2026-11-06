# Captovix V1.50.0 — Stability & Release Quality

## Bộ nhớ và chất lượng ảnh

- Đường lưu PNG ghi thẳng vào tệp tạm, kiểm kích thước và đối chiếu từng pixel trước khi thay tệp đích. Ảnh chụp giữ nguyên độ phân giải. JPEG giữ chất lượng 95.
- Cache thumbnail tối đa 12 MiB, cache lớp ảnh 32 MiB và cache hiệu ứng 16 MiB mỗi nguồn trên mỗi luồng. Undo/Redo dùng chung ngân sách mặc định 16.000.000 byte và tối đa 100 bước. Khi một bước riêng vượt ngân sách, bước đó có thể không còn trong lịch sử.
- Dùng `scripts/Measure-Memory.ps1` và `docs/WINDOWS-ACCEPTANCE-V1490.md` để đo trước/sau trên cùng máy. Mức giảm RAM đỉnh 20% là tiêu chí nghiệm thu Windows.

## Hiệu năng trình chỉnh sửa

Captovix có thể tiếp tục chỉnh ảnh trong lúc xuất. Tệp xuất ghi lại nội dung tại thời điểm chọn lưu, kể cả khi chuyển sang ảnh khác. Bấm Ctrl+S để yêu cầu lưu tài liệu ở nền. Bật đo theo phiên bằng biến CAPTOVIX_MEASURE_PERFORMANCE và đọc kết quả trong performance-latest.json theo hướng dẫn nghiệm thu V1.50.0.

## Copy, dán và hợp nhất ảnh

1. Chọn ảnh A trong thư viện và bấm Ctrl+C. Mở ảnh B rồi bấm Ctrl+V.
2. Ảnh A xuất hiện ở kích thước pixel gốc, bên trên ảnh nền. Có thể chỉnh vị trí trước khi hợp nhất. Canvas giữ kích thước cũ.
3. Chọn **Hợp nhất ảnh** trong bảng thuộc tính lớp ảnh hoặc menu chuột phải. Ctrl+Z khôi phục lớp A riêng. Ảnh nguồn và các chú thích khác vẫn giữ được.
4. Khi lớp ảnh đang chồng phía trên đối tượng khác hoặc ảnh hưởng hiệu ứng nền, Captovix yêu cầu chỉnh thứ tự hoặc vị trí để tránh làm sai bố cục.

## Trung tâm khôi phục

- Trung tâm khôi phục đọc bảng màu và chế độ Sáng/Tối/Theo hệ thống giống app chính. Khi lỗi khởi động trong app, Captovix mở Trung tâm khôi phục. Nếu Windows không tải được app, mở **Captovix Recovery Center** trong Start Menu hoặc chạy `Recovery.cmd` trong thư mục `current` của bản cài.
- Bấm **Thử lại**. Công cụ tìm bản mới ở kênh ổn định hiện có. Nếu chưa tìm được bản mới hoặc mạng không truy cập được, công cụ thử mở bản hiện tại. Khi có bản mới, xem ghi chú rồi chọn **Cập nhật & mở lại** hoặc **Mở bản hiện tại**.
- Công cụ kiểm gói và tạo bản sao lưu trước cập nhật. Gói được áp dụng qua trình cập nhật đi kèm bản cài. Nếu cài thất bại hoặc màn hình chính không xác nhận khởi động, xem **Mở báo cáo lỗi** và dùng lại **Thử lại** hoặc Full Setup.
- Cần cài bản V1.46.4 một lần để có công cụ. Khi bản cũ không mở, dùng Full Setup cài đè vào đúng thư mục.

## Sử dụng Mockup

- Mở ảnh → Mockup → quét vùng hoặc toàn ảnh → rà soát/chọn tất cả → áp dụng. Nguồn vẫn giữ đầy đủ pixel; chế độ 100% và Vừa khung chỉ thay khung nhìn.
- Nút có chữ luôn tạo thân nút và nhãn chữ ở lớp phía trên, cùng một nhóm. Dùng hai nút **Nút / Ô nhập** và **Nhãn chữ** trong thuộc tính để chọn riêng. Sửa nhãn chỉ đổi chữ; đổi nền/viền/bo góc thân nút không sửa nội dung nhãn.
- Thuộc tính bên phải có bản xem trước, mẫu màu hiện tại, bảng màu nhanh, hộp chọn màu, font mẫu và nút B/I/U. Ctrl+B, Ctrl+I, Ctrl+U áp dụng cho toàn đối tượng chữ Mockup đang chọn, kể cả khi nhập trực tiếp; không phải định dạng riêng vài ký tự trong một đối tượng.
- Double-click chữ để nhập tại chỗ. Mở rồi đóng hoặc nhập rồi trả lại đúng nội dung ban đầu sẽ hủy phiên sửa: giữ nguyên font, kích thước, vị trí và lớp pixel gốc, không tạo thay đổi do làm tròn số. Chữ một dòng không tự xuống dòng khi vừa mở; nội dung dài hơn phạm vi vẫn xuống dòng.
- OCR có đối chiếu hình dạng nét ảnh với các font phổ biến đã cài. Font đầy đủ từ UI Automation được ưu tiên nếu khớp screenshot. Ảnh không chứa danh tính font: đây là lựa chọn gần giống, không bảo đảm đoán đúng mọi font. Nếu font lưu trong dự án không có trên máy, renderer chọn font phổ biến gần khung chữ nhất và không ghi đè tên font gốc khi chỉ mở/đóng sửa.
- Khi đổi màu thân nút nền phẳng, nhãn chưa sửa giữ nét chữ ảnh gốc qua mặt nạ alpha thay vì dựng lại bằng font ước lượng. Khi thực sự sửa nội dung/kiểu nhãn, nhãn chuyển sang chữ chỉnh sửa. Nền gradient, icon nằm trong chữ hoặc ảnh nền phức tạp vẫn cần rà soát.
- Undo/Redo, lưu/mở lại, chọn riêng/nhóm và di chuyển tiếp tục dùng nền giữ nội dung V1.45. Phần chưa OCR vẫn là ảnh đi cùng nhóm; không khẳng định mọi pixel đã thành phần tử text. Nền phức tạp tại vị trí cũ sau di chuyển có thể cần sửa trong Nền & mặt nạ.

## Màu giao diện và icon

Bấm nút bánh răng ở góc trên bên phải rồi chọn **Giao diện · màu sắc**. Chọn Sáng/Tối/Theo hệ thống và màu nhấn dạng màu đơn hoặc gradient.

Đổi riêng **Màu nền logo** và **Màu hình logo**. Bỏ chọn **Màu hình logo theo màu nhấn** để chọn màu hình hoặc gradient riêng. Nút **Khôi phục giao diện Captovix** xem trước logo mặc định. Bấm **Lưu** để giữ màu đã chọn. **Hủy** hoặc đóng Thiết lập trả về màu đã lưu. Icon cửa sổ đang chạy, khay và taskbar nhận màu theo lựa chọn. Captovix cập nhật shortcut ghim thuộc bản cài. Explorer có thể lưu đệm icon ghim. Nút Quay dùng nền đỏ #D82038 và chữ trắng trong cả hai chế độ.

Theo yêu cầu hiện tại, bỏ mục **Cập nhật & khôi phục dữ liệu** khỏi menu. Sao lưu trước cập nhật, kiểm hash/gói, Recovery.cmd và dữ liệu backup cũ vẫn giữ để bảo toàn an toàn nâng cấp. Mục bản quyền/khôi phục KEY không thay đổi.

## Nâng cấp và kiểm tra

Đóng app cả khay rồi chạy Captovix-v1.50.0-Full-Setup.exe vào đường dẫn đang cài. Không cần chạy migration mới, không reset database/Supabase. Document schema vẫn tối đa 23, không nâng schema mới; appearance schema 1 có thêm trường tùy chọn tương thích với tệp V1.45. Bộ cài hỗ trợ cài mới và cài đè. Gói Update-Packages.zip chứa tài sản để đưa lên kênh cập nhật hiện có. Xem scripts/README-PUBLISH.txt trước khi phát hành.

Kết quả kiểm thử logic, biên dịch chéo và kiểm gói được ghi trong docs/VALIDATION.md và docs/WINDOWS-ACCEPTANCE-V1490.md. Host hiện tại là Linux: chưa chạy app, bộ cài, phím vật lý hoặc đo RAM trên Windows thật; không coi biên dịch WPF là nghiệm thu Windows. Publish stable vẫn cần báo cáo Windows thật.

Mốc đề xuất sau nghiệm thu là **V1.50.1 — Windows Acceptance Fixes** nếu phép thử thực tế phát hiện hồi quy.

## Dựng lại từ source trên Windows

Yêu cầu Windows10/11x64, .NET10SDK, Python3, PowerShell, NSIS3.x (nếu dựng FullSetup), mạng để khôi phục NuGet. Runtime tự chứa10.0.13 đã ghim trong csproj. Thư mục `artifacts`, `bin`, `obj` được tạo lại, không nằm trong sourceZIP.

Giải nén ZIP, mở PowerShell tại thư mục có `README.md`, `scripts` và `src`. Giữ nguyên cấu hình public license ở `src/Captovix/license-config.json` và `update-source.json`. Không thay issuer/public key, không chép private key, service role hay admin token vào desktop.

```powershell
dotnet --info
dotnet tool restore
dotnet restore src/Captovix/Captovix.csproj
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
python scripts/Run-Checks.py
```

Build phải trả `0 Error(s)`; mở thử và làm checklistWindows chứ không chỉ dựa vào build. Khi cài app với license chưa hoạt động, một số công cụ được khóa như phiên bản trước; dùng license/trial hợp lệ, không bypass kiểm tra.

Dựng bộ cài: cài NSIS, đặt Microsoft Visual C++Redistributablex64 chính thức tại `prerequisites/vc_redist.x64.exe` (thư mục/tệp này không kèm trong source). Chạy script; script chạy kiểm tra trước publish, tạo artifacts/win-x64, FullSetup và Update-Packages.

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
python scripts/Package-Source.py
```

Chỉ publish để kiểm binaries, chưa tạo installer:

```powershell
dotnet publish src/Captovix/Captovix.csproj -c Release -r win-x64 --self-contained true -p:Platform=x64 -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o artifacts/win-x64
```

Lỗi restore: kiểm mạng/proxy/NuGet, cài SDK10; không đổi xuống runtime cũ chỉ để bỏ lỗi. Lỗi thiếu VC++: cài redistributable Microsoft đúngx64. Lỗi khởi động: giữ log/report trong thư mục dữ liệu app, gửi version và stacktrace; không xóa thư viện/KEY để thử. Lỗi DLL sau tự build: dùng đầy đủ thư mục publish/installer, không chép riêng Captovix.exe. Lỗi dung lượng: dọn cache/temp qua công cụ app hoặc thư mục build có thể tạo lại; không xóa ảnh nguồn/sidecar/backup.

## GitHub và phát hành

Bản bàn giao chưa push hoặc tạo GitHub Release. Với repo mới: tạo repo GitHub rồi `git init`, `git add .`, `git commit`, `git branch -M main`, thêm remote đúngrepo và push. Với repo có source: clone/fetch/pull nhánh trước, tích hợp source bản này, xem `git diff`, commit/push; không force-push để bỏ lỗi fetchfirst. Kiểm tra .gitignore và không push prerequisites/artifacts/privateconfig.

Captovix là app desktopWPF, không deploy binaries lên Vercel/Netlify để chạy như web. Backend Supabase/licensing đang dùng giữ nguyên. Sau nghiệm thu Windows, xem `scripts/README-PUBLISH.txt`, upload đúng5assets từ Update-Packages lên GitHub Releases tag v1.48.0. Không chỉ upload ZIP. Kiểm tra source/update/feed URL và thử update máy test trước stableLatest.

Không cần chạy migration mới từ V1.45.0. Giữ bản sao thư mục dữ liệu/dự án khi nâng cấp; schema 23 có backup trước ghi mới. Nếu quay lại bản cũ, dùng bản sao/backup tương thích, không ép đọc hoặc ghi đè schema 23. Dùng source ZIP V1.50.0 cho lần nâng cấp tiếp theo.
