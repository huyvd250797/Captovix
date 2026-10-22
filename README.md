# Captovix V1.11.1 — Update Reliability

Ứng dụng Windows x64 chụp ảnh và chỉnh sửa trong một Editor, mở mặc định phóng to. Ưu tiên ảnh; giữ quay màn hình hiện có, chưa phát triển thêm Editor video. © 2026 HuyVo. All rights reserved.

## Mới trong V1.11.1

- Nguồn stable gắn sẵn, đọc feed trực tiếp không bị giới hạn 10 Releases; hiển thị phiên bản nguồn/URL và lỗi rõ ràng.
- Xóa nội dung từ từng thumbnail, chuyển vào Trash và khôi phục lần xóa vừa rồi.
- Ctrl+wheel zoom; wheel cuộn dọc; Shift+wheel cuộn ngang.
- Cấu hình viền cho ảnh chụp mới, màu/dày sửa lại được; PNG gốc không đổi, kích thước xuất tăng theo viền ngoài.
- Thuộc tính ảnh/đối tượng tự áp dụng; bảng màu Viền/Nền có Trong suốt.
- Gallery hình/nét/đầu nối hiển thị màu đang dùng; ☆ lưu mẫu ngay trên tile.
- Bóng riêng cho shape/callout và bóng toàn ảnh có lưới 9 hướng; ô giữa hàng dưới đổ thẳng xuống.

Hướng dẫn đầy đủ: [docs/UPDATE-RELIABILITY.md](docs/UPDATE-RELIABILITY.md). Source và Update-Packages là hai file bàn giao; setup nằm trong Update-Packages. Chưa đăng GitHub, chưa thực thi ứng dụng/cài đặt trên Windows tại nơi build.

## Kế thừa V1.11.0 — Focus & Privacy


- **Kính phóng đại (M):** kéo vùng đặt kính tại chỗ, chỉnh 1.25–8×, chữ nhật hoặc ellipse, màu/dày viền và bật/tắt viền. Di chuyển kính để lấy vùng khác; hình ảnh bên trong giữ hướng của ảnh. Kính lấy ảnh gốc đã áp các vùng che đang bật, không lấy chữ/shape chú thích, không tạo thêm chi tiết thật.
- **Spotlight (O):** giữ sáng vùng chọn, làm tối phần ngoài; chữ nhật/ellipse, màu và độ tối 0–100%. Nhiều vùng sáng được hợp lại; màu/độ tối lấy Spotlight trên cùng. Lớp nền spotlight luôn dưới các chú thích; dùng menu chuột phải sắp xếp các spotlight để chọn style chung.
- **Highlight (H):** chữ nhật/ellipse, màu và độ đậm 0–100%. Dữ liệu highlight cũ giữ nền chữ nhật và alpha 78 như trước.
- **Blur / Pixel / Che kín:** chọn hình vùng chữ nhật/ellipse; xoay, resize, đổi màu che và mức Blur/Pixel 4–64. Chuyển ba kiểu che tại cùng vùng bằng Áp dụng. Che kín luôn đặc 100%, độc lập opacity chung. Lớp che đặc luôn phủ trên chú thích, kính và filter.
- Bộ lọc làm việc theo tile, có halo cho blur và cache giới hạn. Khi có che đặc, mẫu filter được tô che trước khi tính blur/pixel để tránh kéo màu từ phần đã che ra ngoài. Mẫu ngoài crop được thay trắng; kính không lấy ảnh ngoài crop. Cache cập nhật khi vị trí/màu/trạng thái che hoặc crop thay đổi.
- Các hiệu ứng là đối tượng sửa lại được: kéo/resize/xoay, nhóm, khóa/ẩn, Undo/Redo, Ctrl+C/V, Ctrl+D, lưu/mở lại. Bấm trực tiếp để chỉnh từ công cụ vẽ khác; Alt+kéo để tạo mới chồng lên vùng đã có.
- 5 preset mới: Kính rõ 2x, Spotlight nền tối, Che thông tin, Làm mờ mạnh, Highlight vàng. Có thể lưu preset riêng gồm hình vùng, mức zoom/opacity, màu và mức xử lý. Chọn preset → Chỉ dùng preset cho nét mới để chuyển công cụ tương ứng; Áp dụng trên đối tượng giữ loại công cụ hiện tại.
- **Kiểm tra trước khi chia sẻ:** thống kê vùng che đặc/filter/đang ẩn và vùng vượt ảnh/crop; nút kiểm tra ở Actual Pixels ngay trong Editor. Copy/xuất nhắc trạng thái che. PNG/JPEG xuất là ảnh đã ghép; thư viện/dự án vẫn giữ ảnh gốc. Blur/pixel và highlight không thay thế che đặc khi cần giấu dữ liệu.

## Kế thừa V1.10 — Text & Callout Studio

- Chọn đối tượng → Ctrl+C → Ctrl+V để dán một bản vẫn sửa được. Giữ nhóm, kiểu chữ và liên kết connector nội bộ; tạo ID mới, tháo liên kết ra ngoài lựa chọn. Dán sang ảnh khác giữ số pixel, không tự co giãn. Mỗi lần dán là một Undo. Ctrl+D tiếp tục nhân bản như cũ.
- Khi không chọn đối tượng, Ctrl+C vẫn copy ảnh đã chỉnh. Ctrl+Shift+C luôn copy ảnh đã chỉnh. Khi đang gõ trong ô chữ, Ctrl+C/V vẫn là copy/paste văn bản. Bản này chưa nhận bitmap ngoài Captovix thành image layer.
- Chọn shape đã tô → bỏ dấu Tô nền hoặc bấm Không tô nền: bỏ nền ngay, giữ màu để bật lại. Nền Text/Callout có checkbox riêng trong Text & Callout Studio. Ctrl+Z phục hồi được.
- Chọn/di chuyển/resize trực tiếp từ mọi công cụ vẽ (ngoại trừ Crop). Bấm đường viền khung trong suốt, hoặc phần nền khung đã tô, rồi kéo; inspector tự theo loại đối tượng được chọn. Kéo vùng trống để vẽ tiếp bằng công cụ đang bật; giữ Alt khi kéo để vẽ đối tượng mới chồng lên đối tượng cũ. Vẫn có Chọn (V), chọn nhiều, Layers và menu chuột phải.
- Text: font cài trong Windows, đậm/nghiêng/gạch chân, màu chữ riêng, căn trái/giữa/phải khi có chiều rộng, padding, wrap theo chiều rộng nhập và giãn dòng. Mỗi đối tượng dùng một kiểu chữ; chưa có rich text theo từng từ.
- Callout: chữ nhật có đuôi, bo góc có đuôi, ellipse có đuôi, chữ nhật không đuôi và bo góc không đuôi. Màu chữ/nền/viền độc lập; căn dọc trên/giữa/dưới, tự tăng chiều cao khi bật Giãn hộp theo chữ. Tay nắm đuôi kế thừa, kéo được khi callout đã chọn.
- Step: tròn, vuông bo góc, hình thoi; tự tăng số khi tạo; đánh lại số các bước được chọn, hoặc các bước hiện và không khóa nếu không chọn. Giới hạn 1–9999; giữ nhóm/khóa và Undo.
- Preset riêng lưu thêm kiểu chữ/callout/step; các preset cũ vẫn đọc được. Thiếu font trên máy khác thì xem/xuất dùng Segoe UI và hiện cảnh báo. Double-click chữ/callout hoặc F2 để sửa trên ảnh; Enter xuống dòng, Ctrl+Enter hoàn tất, Esc hủy. IME cần nghiệm thu trên Windows.

## Kế thừa V1.9 — Arrows & Connectors

- Đường/mũi tên thẳng, gấp khúc và cong; chỉnh nét, màu, độ dày, kiểu đầu và một/hai đầu như trước. Công cụ Mũi tên (A) hoặc Đường (L) → chọn kiểu đường bên phải.
- Bật Bám vào shape, kéo từ shape sang shape. Hai đầu gắn theo ID và vị trí trên biên; cập nhật khi target di chuyển/resize/xoay. V chọn lại; kéo hai đầu để nối lại hoặc tháo đầu đang kéo. Tay nắm thứ ba chỉnh cong hoặc vị trí trục gấp khúc. Đường gấp khúc dùng hai góc và một trục giữa có thể kéo ngang; chưa có thêm/xóa nhiều đỉnh tùy ý hoặc tránh chướng ngại tự động.
- Nhân bản cả mạng shape+connector thì bản sao nối vào các shape bản sao. Chỉ nhân bản connector thì bản sao được tháo liên kết. Xóa shape giữ đường ở vị trí cuối và tháo đầu liên quan. Undo/Redo khôi phục đầy đủ. Kéo riêng cả connector, căn/đổi khung sẽ tháo liên kết ra ngoài lựa chọn; kéo một đầu chỉ thay đầu đó. Ẩn target không tự ẩn connector; khóa target vẫn cho đường nối theo khi target được chỉnh sau mở khóa.
- Sửa Ctrl + ` (phím OEM_3) và hỗ trợ thêm phím dấu. Đăng ký với Windows bằng MOD_NOREPEAT; kiểm tra xung đột và giữ phím cũ nếu đăng ký phím mới thất bại.
- Chụp Phần tử / modal: bám khối dưới chuột qua Windows UI Automation, mặc định ưu tiên container như pane/grid. Con lăn lên mở rộng khối cha, xuống chọn khối con. Click để chụp; giữ chuột trái và kéo để chọn vùng thủ công; chuột phải hoặc Esc hủy. Không gửi click chọn vào app đích.
- Các lệnh nhân bản, nhóm/bỏ nhóm, sắp xếp lớp, căn chỉnh, chia đều, khóa/ẩn và xóa nằm trong menu chuột phải ngay trên canvas. Bảng phải giữ Layers, hình học và thuộc tính nét/chữ.
- Giữ bản vá khởi động V1.8.1, chuyển ảnh V1.5.1 và capture pixel lẻ V1.6.1; không tự thay đổi số pixel theo zoom.

## Cài đặt / nâng cấp ngay

1. Giải nén `Captovix-v1.11.0-Update-Packages.zip`.
2. Thoát Captovix ở khay hệ thống. Nếu app cũ không mở được, kết thúc Captovix.exe trong Task Manager.
3. Chạy `HuyVo.Captovix-win-Setup.exe` bên trong ZIP. Đây là wrapper NSIS kèm bộ cài Velopack và VC++ prerequisite, không phải source.
4. Giữ đường dẫn đang cài, bao gồm `D:\Captovix\Captovix`. Không gỡ app, không xóa thư mục dữ liệu. Bộ cài từ chối đổi root của một bản đã quản lý; muốn di chuyển root cần thực hiện quy trình gỡ/cài riêng và giữ dữ liệu.
5. Mở shortcut, kiểm tra nhãn V1.11.0 và thử mở/chuyển ảnh trước khi kiểm tra chức năng mới.

Máy dùng không cần .NET SDK hoặc Visual Studio: bản phát hành kèm .NET desktop runtime. VC++ Runtime có thể cần quyền quản trị nếu chưa có. Windows x64, ưu tiên Windows 11; Windows 10 22H2 cần nghiệm thu riêng. Windows N/KN cần Media Feature Pack. Chưa có bản x86/ARM64 hoặc chứng thư ký mã thương mại.

## Cấu hình Ctrl + `

1. Mở Thiết lập chụp, preset & phím tắt → Phím tắt & chạy nền.
2. Click ô phím ở dòng Theo cấu hình (hoặc Chọn vùng / Phần tử / modal).
3. Nhấn Ctrl + ` bằng bàn phím; ô phải hiện Ctrl + `, không hiện một số F bất thường.
4. Bật checkbox của dòng, bấm Lưu cấu hình Workflow. Chỉ thông báo thành công sau khi Windows chấp nhận tổ hợp.
5. Đưa focus sang app khác và thử. Captovix phải đang mở hoặc ở khay; phím không chạy khi app đã thoát. Trong lúc focus ô ghi phím, hotkey được tạm ngừng để tránh chụp ngoài ý muốn.

Nếu xung đột, đổi tổ hợp hoặc kiểm tra app khác đang dùng. Captovix phục hồi các tổ hợp cũ khi đổi phím thất bại; không chiếm phím đã đăng ký bởi phần mềm khác. F12 và tổ hợp có phím Windows không được dùng. Nhãn phím dấu theo vị trí bàn phím US; vị trí OEM thực tế phụ thuộc layout Windows. Các phím quay cố định Ctrl+Shift+2/3/4 vẫn dành riêng cho quay/dừng/tạm dừng.

## Chụp theo modal / block / grid

- Nút Chụp phần tử / modal trong phần Thiết lập, hoặc chọn nguồn Phần tử / modal rồi bấm Chụp ảnh. Có một dòng hotkey riêng cho chế độ này.
- Khung teal theo phần tử đang được chọn. Nếu con trỏ ở chữ trong grid, mặc định chọn container gần nhất; cuộn xuống để lấy phần tử nhỏ hơn, cuộn lên để lấy modal/cửa sổ lớn hơn.
- Bộ nhận diện đọc tọa độ pixel vật lý. Highlight chỉ là overlay tạm, được đóng trước khi lấy ảnh. Phần tử được chụp là phần đang hiển thị trên màn hình, không phải full nội dung cuộn bên trong grid. Vùng chọn được chốt lúc click; nếu có đếm ngược mà modal di chuyển thì cần chọn lại.
- UIA chạy trên MTA trong tiến trình phụ có timeout 900 ms; provider chậm không chặn giao diện Editor. Nếu không thấy accessibility tree, dùng khung cửa sổ hoặc kéo vùng thủ công. Không có OCR/nhận diện hình ảnh, không đọc DOM qua extension, không bảo đảm nhận diện từng div/canvas/game/ứng dụng chạy quyền cao. Không tự yêu cầu quyền admin hoặc tắt bảo vệ máy.
- Chế độ Phần tử chỉ dành cho ảnh; quay video chọn vùng, cửa sổ hoặc màn hình như trước. Thử trên app đích của Boss theo checklist `docs/ARROWS-CAPTURE-WORKFLOW.md`.

## Dữ liệu, sao lưu và rollback

Dữ liệu tại `%LOCALAPPDATA%\HuyVo\Captovix`: Media, thư viện, settings, Edits, presets, backup và `errors.log`. Thư mục này độc lập với đường dẫn cài. Ảnh gốc giữ riêng; chú thích là đối tượng sửa lại được, không flatten ảnh gốc. Không có database/server hoặc Supabase; ảnh và cấu hình ở máy.

- Editor schema 9 đọc schema 1–8. Trước lần lưu đầu lên schema 9, giữ byte sidecar cũ ở `Edits/<id>.json.pre-v1.11.bak`; giữ nguyên các backup trước đó. Không ghi đè dữ liệu schema mới hơn.
- Workflow vẫn schema 1, bổ sung action Element. Cấu hình 5 action được nạp thêm action Element mặc định tắt để tránh chiếm tổ hợp cũ; thiết lập cài mới có 6 action. Trước lần ghi workflow đầu ở bản này giữ `capture-workflow.json.pre-v1.9.bak`.
- Trước nâng cấp nên sao lưu toàn bộ thư mục dữ liệu. Updater trong app cũng sao lưu theo cơ chế đang có. Để hạ phiên bản: thoát app, giữ bản sao dữ liệu hiện tại, khôi phục sidecar/workflow phù hợp từ backup trước khi dùng bản cũ. Không mở schema 9 trực tiếp bằng V1.10.0 trở xuống. Không cần xóa Media để nâng cấp.
- Báo lỗi: Win+R → `%LOCALAPPDATA%\HuyVo\Captovix\errors.log`, gửi log mới nhất kèm thao tác gây lỗi. Video có thêm recorder.log.

## Build từ source trên Windows

Cần .NET 10 SDK (pin 10.0.100), Internet NuGet; NSIS 3 để tạo Setup wrapper. CLI Velopack pin 1.2.161 trong `.config/dotnet-tools.json`. Mở PowerShell ở thư mục chứa README này:

```powershell
.\Build.cmd
```

Script chạy các test logic, các bộ WPF trên Windows, publish self-contained win-x64, đóng gói Velopack, biên dịch và chạy harness đường dẫn NSIS, xác minh chữ ký prerequisite Microsoft rồi tạo wrapper. Nếu test hoặc build lỗi, dừng và đọc lỗi đầu tiên; không bỏ qua rồi phát hành artifact cũ.

Kiểm tra riêng:

```powershell
dotnet run --project tests/FocusPrivacyChecks -c Release -p:Platform=x64
dotnet run --project tests/FocusPrivacyWindowsChecks -c Release -p:Platform=x64
dotnet run --project tests/TextClipboardChecks -c Release -p:Platform=x64
dotnet run --project tests/TextStudioWindowsChecks -c Release -p:Platform=x64
dotnet run --project tests/ConnectorChecks -c Release -p:Platform=x64
dotnet run --project tests/ConnectorWindowsChecks -c Release -p:Platform=x64
dotnet run --project tests/EditorWindowsChecks -c Release -p:Platform=x64
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
```

Output: `artifacts/releases` có full nupkg/feed/Setup canonical; `artifacts/Captovix-v1.11.0-Update-Packages.zip` dùng phát hành. `.\scripts\Build-Windows.ps1 -SkipInstaller` bỏ wrapper NSIS, không dùng Setup raw đó cho quy trình cài chuyển đổi. Nếu cần delta, truyền `-PreviousReleases` trỏ thư mục chứa nupkg phiên bản trước; không sửa feed/hash bằng tay.

Stack giữ nguyên: C# / .NET 10 WPF, ScreenRecorderLib 7.0.1, Velopack 1.2.161, NSIS 3.08. UIA dùng assembly Windows Desktop, không thêm package trình duyệt. Build Linux chỉ cross-build cho Windows; không thực thi WPF hoặc cài đặt trên Linux.

## GitHub và phát hành cập nhật

Nguồn mặc định đã tích hợp: https://github.com/huyvd250797/captovix-releases . Không phải nhập lại sau mỗi lần cài. Không cần token trong app cho repo công khai.

1. Giải nén Update-Packages.
2. Tạo stable release tag `v1.11.0`, title `Captovix V1.11.0 — Focus & Privacy`.
3. Đính kèm đúng 5 asset: `HuyVo.Captovix-1.11.0-full.nupkg`, `HuyVo.Captovix-win-Setup.exe`, `RELEASES`, `assets.win.json`, `releases.win.json`. Không upload chỉ nguyên ZIP hoặc Source ZIP thay cho các asset này.
4. Publish. Máy đã mở được app: Kiểm tra cập nhật → Tải → Khởi động lại. Máy app lỗi khởi động: dùng Setup trong ZIP.
5. Kiểm tra tải/package hash và nâng cấp trên một máy Windows trước khi cài nhiều máy. Bản bàn giao tại đây chưa được đăng GitHub Release.

Source nên dùng repo phát triển riêng. Repo mới (thay URL đúng repo source của Boss):

```powershell
git init
git add .
git commit -m "Captovix v1.11.0 focus privacy and protected magnifier sampling"
git branch -M main
git remote add origin https://github.com/OWNER/CAPTOVIX-SOURCE.git
git push -u origin main
```

Repo đã có source: clone/fetch/pull trước, đọc khác biệt và tích hợp thay đổi V1.11.0, commit/push bình thường. Không force push để ghi đè lịch sử. Không đưa artifacts lớn hoặc dữ liệu cá nhân vào source repo. Đây là ứng dụng Windows, không deploy WPF lên Vercel/Netlify; GitHub Releases chỉ phân phối bộ cài và update.

## Kiểm chứng và phiên bản tiếp theo

Xem `docs/VALIDATION.md` để phân biệt các test logic đã chạy với các bộ WPF chỉ cross-build. Chưa thực thi Windows thật trong môi trường build này. Đặc biệt cần nghiệm thu chọn/kéo từ mọi công cụ, clipboard đối tượng, tiếng Việt/IME, callout xuất đủ chữ và Ctrl + ` ở app khác/khay, provider UIA của browser/modal/grid, hủy khi provider chậm, input hook, DPI 100/125/150/175/200%, màn hình tọa độ âm và installer/updater. HDR/màu rộng đánh giá riêng.

V1.12 — Selection & Pixel Editing dự kiến bổ sung chọn vùng pixel bằng chữ nhật/ellipse/lasso, copy/cut/paste vùng thành đối tượng ảnh, fill/eraser và cắt bỏ hàng/cột thừa. Tách rõ chọn pixel với chọn chú thích; giữ ảnh gốc, Undo có giới hạn bộ nhớ và lưu/mở đủ dữ liệu vùng đã dán. Video tiếp tục để sau.

Bàn giao 2 ZIP: Update-Packages và Source. Setup nằm trong Update-Packages. Lần nâng cấp sau gửi Source ZIP mới nhất này.

Hướng dẫn/chấp nhận mới: `docs/TEXT-CALLOUT-STUDIO.md`. Nội dung quá dài hoặc đặt ngoài ảnh/crop vẫn bị cắt ở biên file xuất; inspector cảnh báo để di chuyển, tăng hộp hoặc giảm cỡ chữ trước khi xuất. Zoom không tự thay đổi số pixel đầu ra.

Hướng dẫn và checklist V1.11: `docs/FOCUS-PRIVACY.md`. Kiểm chứng chi tiết tại `docs/VALIDATION.md`; bộ WPF chỉ được cross-build tại nơi build Linux, chưa chạy Windows thật.
