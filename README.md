# Captovix V1.14.0 — Scrolling Capture Foundation

Ứng dụng Windows x64, C#/.NET10 WPF, một Editor mở maximized. Bản nâng cấp từ source V1.13.0, giữ bộ công cụ ảnh và chức năng quay hiện có. Bàn giao đúng hai ZIP: Update-Packages (có Setup.exe) và Source. Chưa tự đăng GitHub/Release.

## Nội dung V1.14

- **Chụp cuộn dọc:** chọn vùng, tự cuộn trang bằng tay, lấy từng khung bằng nút hoặc Ctrl+Shift+F10; Ctrl+Shift+F11 mở ghép/xem trước. Tìm overlap, loại khung trùng, đánh dấu điểm nối không chắc; chỉnh overlap theo hàng pixel và xác nhận trước lưu. Giới hạn24 khung,4MP/khung,64MP nguồn/phiên,40MP ảnh ghép và cao32768px.
- **Phục hồi:** giữ PNG gốc + manifest + SHA256 trong ScrollSessions. Có menu khôi phục phiên để xem/ghép lại; không cần giữ cửa sổ nguồn cũ. Có hủy thao tác; lỗi không xóa nguồn đã thu. Chưa có auto-scroll hoặc ghép ngang.
- **Logo chính thức theo mẫu Boss chốt:** biểu tượng hai góc/chấm tròn gọn, tím #7C3AED, dùng trong header, exe/taskbar/tray và bộ cài. Có SVG và ICO16–256px.
- **Giao diện riêng trên từng máy:** Sáng/Tối/Theo hệ thống, palette và chọn màu nhấn tùy ý; áp dụng/lưu ngay. Lưu riêng tài khoản Windows trên máy, giữ qua cập nhật, không đồng bộ cloud. Không đổi màu ảnh/chú thích/file xuất.

Hướng dẫn thao tác, giới hạn và đường dẫn dữ liệu: [SCROLLING-CAPTURE-APPEARANCE.md](docs/SCROLLING-CAPTURE-APPEARANCE.md). Kết quả kiểm tra: docs/VALIDATION.md. Nghiệm thu Windows thật còn chờ: docs/WINDOWS-TEST-V1.14.md. Không coi build thành công là đã kiểm thử chụp/quay/cài đặt/UI trên Windows.

## Cài đặt hoặc cập nhật

1. Giải nén **Captovix-v1.14.0-Update-Packages.zip**.
2. Đóng Captovix cả ở tray; chạy **HuyVo.Captovix-win-Setup.exe** trong ZIP để cài/nâng cấp thủ công. Máy mới có thể chọn thư mục riêng như `D:\\Captovix\\Captovix`; máy đã cài managed thì giữ installroot hiện có, không lồng thêm current.
3. Hoặc dùng Trợ giúp → Kiểm tra cập nhật **sau khi** nhà phát hành đăng đủ5 assets lên stable latest Release. Không nhập link, không cần cài source. Không cần nâng lần lượt qua từng minor version khi dùng full package hợp lệ.
4. Ứng dụng tự chứa .NET10.0.13 x64, bộ cài có Visual C++ Runtime. App/Setup chưa được ký bằng chứng thư nhà phát hành. Không xóa thư mục dữ liệu để sửa lỗi.

Nguồn ổn định: `src/Captovix/update-source.json`, gắn sẵn và không hiển thị địa chỉ trên UI. Giao diện: **Hiển thị → Giao diện · Sáng / Tối / Theo hệ thống…**. Chụp cuộn: **Chụp & quay → Chụp cuộn dọc…**. Khôi phục phiên cũng trong menu này.

## Công cụ ảnh trước đây vẫn giữ

Ctrl+wheel zoom; wheel cuộn dọc; Shift+wheel ngang; Space+kéo pan. Click đối tượng để chọn/kéo/sửa, Alt+kéo vẽ chồng, Ctrl+C/V/D, Ctrl+Z/Y; Ctrl+B/I/U cả đối tượng chữ. Shapes/mũi tên/callout/layers/blur/focus/pixel selection/composition/template/project giữ hành vi V1.13. Hướng dẫn cũ: docs/IMAGE-COMPOSITION-TEMPLATES.md; roadmap tập trung ảnh, video phát triển sau.

## Bản tiếp theo — V1.15: Advanced Capture & Stitching

Mở rộng **sau nghiệm thu V1.14**: auto-scroll cho từng app được hỗ trợ, thử ghép ngang, vùng cố định nhập số và các chế độ window/client/menu; nhận diện viewport đổi hoặc animation, công bố rõ trường hợp hỗ trợ/fallback. Không hứa auto-scroll mọi app. Có thể tách các nhóm này thành patch nhỏ để ổn định trước khi thêm rộng.

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
dotnet run --project tests/ScrollAppearanceChecks/ScrollAppearanceChecks.csproj -c Release
dotnet run --project tests/ScrollAppearanceWindowsChecks/ScrollAppearanceWindowsChecks.csproj -c Release -p:Platform=x64
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
git commit -m "Captovix v1.14.0 scrolling capture and personal appearance"
git branch -M main
git remote add origin https://github.com/OWNER/CAPTOVIX-SOURCE.git
git push -u origin main
```

Với repo có sẵn: clone/fetch/pull, tích hợp thay đổi rồi commit/push; không force push. Không đưa artifacts hoặc dữ liệu người dùng vào repo source.

Để updater thấy bản mới: tạo GitHub Release công khai stable, tag v1.14.0, Set as latest; tải riêng đúng5 asset trong Update-Packages:

- HuyVo.Captovix-1.14.0-full.nupkg
- HuyVo.Captovix-win-Setup.exe
- releases.win.json
- assets.win.json
- RELEASES

Không chỉ upload ZIP/source. Publish xong thử cập nhật trên một máy Windows trước khi cài nhiều máy. Xem docs/UPDATE-GUIDE.md, docs/VALIDATION.md và docs/WINDOWS-TEST-CHECKLIST.md.

Bàn giao2 ZIP: Update-Packages và Source; Setup ở trong Update-Packages. Lần nâng cấp sau gửi Source ZIP mới nhất này. Roadmap V1.15 và những mốc ảnh tiếp theo ở ROADMAP.md; không tự triển khai video editor trong bản này.
