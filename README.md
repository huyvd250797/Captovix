# Captovix V1.13.0 — Image Composition & Templates

Ứng dụng Windows x64, C#/.NET 10 WPF, một Editor mở phóng to. Bàn giao đúng hai ZIP: Update-Packages (có setup) và Source. Chưa đăng Release/GitHub. Quay màn hình giữ chức năng hiện có; phát triển chuyên sâu về ảnh.

## Nội dung mới

- Menu **Bố cục**: tạo canvas, thêm nhiều ảnh, sắp xếp theo mẫu trực quan, đổi chỗ hai ảnh; tiêu đề, caption và ghi chú cuối là đối tượng chữ sửa được.
- Từng ảnh là lớp riêng: Fit/Fill giữ tỷ lệ, kéo nội dung ảnh trong ô, đổi vị trí bằng thanh trượt, cắt theo pixel nguồn, thay ảnh, viền/bo góc/bóng. Ảnh nguồn nhúng trong tài liệu.
- `.cvtemplate`: lưu bố cục và đối tượng vector, bỏ ảnh công việc và thay chữ bằng placeholder. `.cvproject`: một gói ZIP chứa ảnh gốc và tài liệu với đủ lớp ảnh; mở độc lập file nguồn ngoài máy.
- Callout mới có đệm 30 px, tự mở rộng chiều ngang hợp lý, giới hạn chiều cao theo canvas và clip chữ trong vùng nội dung. Mẫu preview dùng chữ/đệm/cỡ mẫu riêng; dữ liệu yêu thích giữ nguyên style thực tế.
- Màu chữ có palette và hộp chọn màu Windows. Ctrl+B/I/U cho đậm/nghiêng/gạch chân cả đối tượng, cả khi nhập inline; chưa có rich text từng từ.
- Icon vector thống nhất cho công cụ/chụp/quay/copy/xuất/thư viện/layers; chụp teal, quay đỏ dịu. Phím tắt và chụp/quay là hai module riêng. Ẩn cấu hình và địa chỉ nguồn cập nhật; trình bày thuộc tính theo công cụ trước, layers/hình học/toàn ảnh sau.
- Bỏ nút và phím P tạo Pixel hóa; giữ render dữ liệu Pixelate cũ. Vùng pixel chữ nhật/ellipse/lasso vẫn dùng để cắt/copy/tô/xóa pixel ảnh nền.

Hướng dẫn: [IMAGE-COMPOSITION-TEMPLATES.md](docs/IMAGE-COMPOSITION-TEMPLATES.md). Bản này build và kiểm tra logic trên Linux; WPF chỉ cross-build. Chưa chạy giao diện, clipboard, hộp chọn màu native, capture/DPI, cài đặt hoặc update trên Windows thật.

## Bản tiếp theo

**V1.14 — Scrolling Capture Foundation:** chọn vùng rồi cuộn một chiều; ghi các khung có overlap, ghép ảnh dài, xem điểm nối và báo vùng không chắc. Có hủy thao tác/giới hạn bộ nhớ và giữ các khung nguồn nếu ghép thất bại. Chưa triển khai; cần thử riêng theo từng ứng dụng Windows.

## Cài đặt và nâng cấp

1. Giải nén Captovix-v1.13.0-Update-Packages.zip.
2. Máy đang chạy app: đóng app rồi chạy HuyVo.Captovix-win-Setup.exe, hoặc cập nhật từ Trợ giúp → Kiểm tra cập nhật sau khi nhà phát hành đăng các asset.
3. Chọn đường dẫn cài phù hợp, ví dụ D:\Captovix\Captovix. Không chọn thư mục Media/dữ liệu. Installer nhận diện managed install root, không lồng nhiều thư mục current và giữ dữ liệu.
4. Mở Captovix. Giao diện mặc định maximized. Máy chạy Windows x64 cần Visual C++ Runtime; bộ cài có prerequisite. .NET runtime đã tự chứa.

Nguồn cập nhật gắn sẵn trong src/Captovix/update-source.json; V1.13 không cho đổi/hiện địa chỉ trên UI. Khi muốn đổi publisher, sửa file này trong source trước build. Không đưa token/mật khẩu vào file. Không cần Supabase/database hoặc biến môi trường để dùng offline.

## Cách dùng

- Chụp ảnh hoặc Ctrl + ` theo cấu hình của Boss; vào Công cụ → Phím tắt & chạy nền để đăng ký tổ hợp phím. Mở app/khu vực khay để phím toàn cục hoạt động; hệ thống báo xung đột nếu ứng dụng khác đã dùng phím.
- Chụp & quay → Cấu hình chụp & quay: nguồn, delay, mic, âm thanh máy, viền và kết quả sau chụp. Chụp phần tử cần provider UI Automation; con lăn chọn khối cha/con, Esc hủy.
- Ctrl+con lăn zoom, con lăn cuộn dọc, Shift+con lăn cuộn ngang, Space+kéo khung nhìn. Chọn/click đối tượng để di chuyển và sửa; Alt+kéo để vẽ chồng; Ctrl+C/V/D, Ctrl+Z/Y. Nội dung chữ có Ctrl+B/I/U áp dụng cho cả đối tượng.
- Bố cục → Tạo bản ghép mới; chọn kích thước/mẫu/tiêu đề/caption rồi chọn nhiều file. Hướng dẫn chi tiết về Fit/Fill, crop, template/project: docs/IMAGE-COMPOSITION-TEMPLATES.md.
- Delete thumbnail chuyển nội dung vào Trash; nút khôi phục ở đáy thư viện khôi phục lần xóa vừa rồi. Chuột phải canvas mở lệnh nhóm, thứ tự lớp và căn chỉnh.

## Build từ source trên Windows

Cài .NET SDK10, NSIS3 và Git nếu cần quản lý source. ScreenRecorderLib7.0.1/Velopack1.2.161 được restore tự động, runtime10.0.13. Mở PowerShell trong thư mục source đã giải nén:

```powershell
dotnet restore src/Captovix/Captovix.csproj
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
```

Để chạy kiểm tra và tạo nupkg/bộ cài:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Hoặc double-click Build.cmd. Kết quả ở artifacts; Update-Packages là ZIP dùng phân phối. NSIS và Visual C++ prerequisite cần tải/cài theo thông báo script. Chữ ký Microsoft của prerequisite được script kiểm tra trên Windows. Không thêm dấu -- trước --installto ở lệnh CoreSetup vì gây lỗi EXE_ARGS.

Chạy riêng logic/bộ WPF mới trên Windows:

```powershell
dotnet run --project tests/CompositionChecks/CompositionChecks.csproj -c Release
dotnet run --project tests/CompositionWindowsChecks/CompositionWindowsChecks.csproj -c Release -p:Platform=x64
```

Nếu chỉ cross-build ở Linux, các bộ WPF không được xem là đã chạy. Đừng build chỉ định cả thư mục khi có tệp WPF tạm: dùng đúng đường dẫn .csproj.

## Dữ liệu, backup và lỗi

Dữ liệu ở %LOCALAPPDATA%\HuyVo\Captovix, gồm Media, Edits, Thumbnails, cấu hình, library.json. Ảnh/chú thích cũ được giữ; schema12 đọc1–11, backup sidecar .pre-v1.13.bak trước lần ghi đầu. Muốn rollback, sao lưu toàn thư mục dữ liệu trước nâng cấp và chỉ phục hồi backup đồng bộ; app cũ không hiểu schema12.

Lỗi: %LOCALAPPDATA%\HuyVo\Captovix\errors.log. Khi app không mở, gửi đoạn lỗi mới nhất và dùng Setup từ Update-Packages để sửa/cập nhật. Không xóa thư mục dữ liệu để sửa lỗi khởi động. Ghi lại Windows version, scale/màn hình, thao tác gây lỗi và file Source ZIP phiên bản hiện dùng.

Project chứa ảnh gốc và lớp nhúng để chỉnh tiếp. Khi chia sẻ ảnh đã che thông tin, xuất PNG/JPEG và kiểm tra ảnh ghép; không chia sẻ nhầm project giữ source. Pixel editor hỗ trợ40MP; ảnh lớn tốn RAM và ít Undo hơn. Crop lớp ảnh hiện dùng form pixel nguồn, chưa có tay nắm riêng.

## GitHub và phát hành

Bản bàn giao chưa push source hoặc đăng Release. Phần mềm desktop WPF không deploy lên Vercel/Netlify. Source có thể quản lý trong repo riêng:

```powershell
git init
git add .
git commit -m "Captovix v1.13.0 image composition and templates"
git branch -M main
git remote add origin https://github.com/OWNER/CAPTOVIX-SOURCE.git
git push -u origin main
```

Với repo có sẵn: clone/fetch/pull, tích hợp thay đổi rồi commit/push; không force push. Không đưa artifacts hoặc dữ liệu người dùng vào repo source.

Để updater thấy bản mới: tạo GitHub Release công khai stable, tag v1.13.0, Set as latest; tải riêng đúng5 asset trong Update-Packages:

- HuyVo.Captovix-1.13.0-full.nupkg
- HuyVo.Captovix-win-Setup.exe
- releases.win.json
- assets.win.json
- RELEASES

Không chỉ upload ZIP/source. Publish xong thử cập nhật trên một máy Windows trước khi cài nhiều máy. Xem docs/UPDATE-GUIDE.md, docs/VALIDATION.md và docs/WINDOWS-TEST-CHECKLIST.md.

Bàn giao2 ZIP: Update-Packages và Source; Setup ở trong Update-Packages. Lần nâng cấp sau gửi Source ZIP mới nhất này. Roadmap V1.14 và những mốc ảnh tiếp theo ở ROADMAP.md; không tự triển khai video editor trong bản này.
