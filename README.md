# Captovix V1.60.0 — Audio Editing

## Video Studio và âm thanh

- Mở video từ thư viện để xem hoặc chuyển toàn màn hình. Bấm **Studio** để mở không gian dựng toàn màn hình. **Thoát Studio** hoặc Esc trả về màn xem. Cắt video bằng **Split** rồi **Xóa đoạn**.
- Chọn các hàng **Âm thanh gốc**, **Micro**, **Tiếng máy**, **Nhạc nền**. Track có waveform, Mute/Solo, âm lượng, fade đầu/cuối và danh sách khoảng cắt. Nhạc nền có mốc trong tệp và vị trí riêng trên timeline. Mốc cắt âm bám theo thời gian dự án; audio nguồn đi cùng clip qua Split và sắp xếp.
- Bản quay có WAV riêng dùng các WAV đó khi phối âm. Video nhập hoặc bản quay không có WAV riêng dùng track âm thanh gốc. Nhạc nền tham chiếu tệp trên ổ đĩa. Dùng **Áp dụng âm thanh** để cập nhật bản phối.
- Studio dùng nhãn, icon và renderer chú thích của chỉnh ảnh cho Khung, Callout, Mũi tên, Chữ, Bước, Vẽ tự do, Highlight, Spotlight và các công cụ vùng. Bảng bên phải hiện nhóm thuộc tính theo loại đã chọn. Crop vẫn chỉnh phần hình của clip.
- Trong chỉnh ảnh, **Opacity (%)** chỉnh toàn bộ đối tượng đã chọn. Đậm viền, đậm nền và các trường độ đậm hiện có dùng thanh kéo và ô số đồng bộ. 0% là trong suốt, 100% là giữ độ đậm gốc.
- Waveform, bản phối đã chỉnh và xuất video có bố cục tùy chỉnh hoặc hiệu ứng lấy mẫu cần **FFmpeg**. Đặt `ffmpeg.exe` cạnh `Captovix.exe` hoặc trong `PATH`. Các tác vụ dựng lại hình cần FFmpeg có libx264. Dự án `.cvxvideo` V1–V3 mở được và nâng lên schema 4 khi lưu. Không cần migration cơ sở dữ liệu.


## Dựng video trong V1.59.0

- Mở bản quay hoặc MP4. Bảng **Bố cục video** bên phải cho chọn **Ngang 16:9 (1920×1080)**, **Dọc 9:16 (1080×1920)**, **Vuông 1:1 (1080×1080)** hoặc kích thước của nguồn chính. **Fit** giữ đủ hình với màu nền đã chọn. **Fill** cắt ở giữa để lấp đầy canvas.
- **Crop** trên thanh trên cùng cho kéo vùng giữ lại ngay trên khung xem. Có thể nhập lề crop theo phần trăm trong bảng bên phải. Xoay theo 90°, 180° hoặc 270°. Crop và xoay áp dụng cho đoạn đang chọn, còn tỷ lệ, Fit/Fill và màu nền áp dụng toàn dự án.
- Tua đến vị trí muốn đặt chú thích, chọn **Khung, Callout, Mũi tên, Chữ, Bước** hoặc **Làm mờ** trên thanh trên cùng rồi vẽ/click trong canvas. Chú thích mới mặc định dài 3 giây, giới hạn bởi cuối timeline. Kéo thân thanh trên track để dời cả khoảng. Kéo tay nắm hai đầu để rút ngắn hoặc kéo dài. Có thể nhập mốc chính xác, nội dung, màu, cỡ chữ và độ dày trong bảng bên phải. Kéo chú thích trên canvas để dời vị trí. Nhấp đúp Chữ, Callout hoặc Bước để sửa tại chỗ. Khung, callout, mũi tên và làm mờ có tay nắm trực tiếp để đổi hình học.
- Track chú thích bám vào thời gian của timeline. Đổi thứ tự clip vẫn giữ thời gian của track. Khi timeline ngắn lại, Captovix cắt phần track vượt quá thời lượng. **Ctrl+Z** hoàn tác thay đổi. **Delete** xóa đối tượng đang chọn. Menu chuột phải trên timeline có **Split**.
- Số đoạn được gán khi tạo. Split giữ số cũ cho phần bên trái và cấp số mới cho phần bên phải. Kéo clip giữ tên. Dự án V1.58.0 mở được và gán số theo thứ tự đang lưu. Chú thích cũ được chuyển thành track phủ đúng thời gian của clip trước đó.
- **Xuất đoạn** chỉ lấy phần chú thích giao với đoạn đang chọn. **Xuất timeline MP4** dùng toàn bộ thứ tự và thời gian đã dựng. Xuất có bố cục tùy chỉnh hoặc làm mờ cần **FFmpeg Windows có libx264**. Đặt `ffmpeg.exe` cạnh `Captovix.exe` hoặc thêm FFmpeg vào `PATH` trước khi mở app. Bản xuất theo nguồn không dùng làm mờ tiếp tục dùng bộ mã hóa Windows.
- Bố cục, tên đoạn và track được tự lưu trong dự án `.cvxvideo` phiên bản 3. Không cần migration cơ sở dữ liệu. Khi quay lại V1.58.0, dùng tệp dự án V2 đã sao lưu trước đó.


## Chỉnh video trên timeline

1. Mở video trong thư viện. Bấm **Thêm clip** để ghép các MP4 theo thứ tự timeline.
2. Bấm hoặc kéo trên thước thời gian để tua. Dùng **Split** trên thanh công cụ hoặc **Split tại vạch phát** trong menu chuột phải.
3. Kéo đoạn qua nửa đầu hoặc nửa sau của đoạn đích để chèn trước hoặc sau. Chọn đoạn rồi nhấn **Delete** để bỏ đoạn và khép khoảng trống. **Ctrl+Z** hoàn tác, **Ctrl+Y** hoặc **Ctrl+Shift+Z** làm lại. Khi con trỏ đang ở thư viện, Delete áp dụng cho thư viện.
4. Dùng **− / +** để zoom timeline, bấm tỷ lệ để về vừa khung. Bấm biểu tượng toàn màn hình hoặc **F11** để xem lớn, **Esc** để trở lại, **Space** để phát/tạm dừng trong toàn màn hình.
5. Chọn clip và chọn **Khung, Callout, Mũi tên, Chữ, Bước** hoặc **Làm mờ**. Vẽ trên khung video. Chú thích áp dụng suốt clip được chọn. Tách clip giữ chú thích ở hai nửa. Dùng **Gỡ chú thích cuối** hoặc Undo để gỡ. Xuất vùng làm mờ cần FFmpeg có bộ mã hóa libx264. Cài ffmpeg.exe trong PATH hoặc đặt cạnh Captovix.exe. Xuất các chú thích còn lại dùng bộ mã hóa Windows.
6. Chọn **Xuất timeline MP4** để kết xuất thứ tự đang dựng, hoặc **Xuất đoạn** để chỉ xuất clip đang chọn. Captovix kiểm tra tệp tạm và thời lượng rồi ghi tệp đích.

Dự án tự lưu thứ tự đoạn, chú thích, các nguồn MP4, đoạn đang chọn và vị trí phát. **Lưu dự án** tạo tệp .cvxvideo mở được từ **Tệp → Mở dự án video**. Để tìm lại nguồn đã di chuyển, chọn đoạn của nguồn đó rồi bấm **Liên kết nguồn**. V1.58 đọc dự án V1.57. Các thay đổi timeline được lưu theo định dạng dự án V2.

Xem hướng dẫn nghiệm thu tại docs/WINDOWS-ACCEPTANCE-V1580.md.

**Bước:** nhập số nguyên hoặc số có một chữ số thập phân bằng dấu chấm hoặc phẩy. Bước tiếp theo lấy số từ Bước mới tạo hoặc sửa gần nhất trên chính ảnh. Ví dụ, 1.9 → 2.0 → 2.1, hoặc 4 → 5. Mở lại ảnh và Undo/Redo giữ trạng thái đánh số. Bản Captovix cũ đọc trường số nguyên dự phòng nếu mở ảnh có Bước thập phân.

## Cấu hình quay

Phiên bản gộp phạm vi V1.53.0 và V1.54.0. Trong **Quay → Chọn nguồn quay**, chọn preset **Nhẹ** (720p/15 FPS), **Bình thường** (1080p/30 FPS), **Chất lượng cao** (theo nguồn/60 FPS) hoặc đổi giới hạn độ phân giải và FPS để dùng **Tùy chỉnh**. Kích thước đầu ra giữ tỷ lệ nguồn và được căn chẵn cho H.264. Ước tính gồm MP4 và WAV của từng nguồn bật. Với cửa sổ/vùng chưa chọn, số hiển thị lấy màn hình tham chiếu. Lỗi encoder hiển thị cấu hình đang dùng. Có thể bỏ chọn ưu tiên phần cứng để dùng encoder phần mềm.

Click khung gợi ý ở chế độ **Vùng chọn** để đếm ngược và quay. Kéo thủ công rồi xác nhận bằng Enter hoặc nút Quay. Thanh quay hiển thị tín hiệu Micro và Âm thanh máy. Phím **Ctrl+Shift+2** mở quay, **Ctrl+Shift+3** dừng, **Ctrl+Shift+4** tạm dừng/tiếp tục và **Ctrl+Shift+5** thêm mốc trong phiên quay nếu tổ hợp còn trống. Nút **Mốc** trong thư viện đưa video đến thời điểm đã đánh dấu. Tệp `.mp4.cues.json` chứa click trái/phải/giữa cùng thời điểm và tọa độ khung. Một phiên giữ tối đa 150 mốc và 20.000 click. Để sửa số **Bước**, nhấp đúp trên ảnh, nhập số 1–9999, cho phép một chữ số thập phân, Enter lưu hoặc Esc hủy.

MP4 phát âm thanh trộn. Hai nguồn được bật vẫn có WAV riêng để nghe lại. Dùng **Nghe micro** hoặc **Nghe tiếng máy** trong thư viện.


## Quay màn hình

Bấm **Quay**, chọn vùng, cửa sổ hoặc màn hình. Chọn Micro và Âm thanh máy, nói hoặc phát thử để xem hai thanh mức âm. Có thể tắt cả hai để quay im lặng. Vùng chọn có gợi ý phần tử/modal như chụp ảnh. Sau khi xác nhận nguồn, lớp phủ tối hiển thị số 3–2–1 màu trắng. Dùng Esc để hủy.

MP4 phát âm thanh trộn. Các tệp tên kết thúc bằng .mp4.mic.wav và .mp4.system.wav lưu riêng từng nguồn được bật. Trong thư viện, dùng **Nghe micro** hoặc **Nghe tiếng máy**. Sao lưu thư viện chứa sự kiện chuột dùng định dạng 3. V1.54.0 đọc được cả bản sao định dạng 1 và 2.

Menu Chụp ảnh và menu khay có **Chụp mặc định**, **Toàn màn hình** và **Chụp lại vùng**. Dùng phím tắt chụp để giữ menu đang mở trong ảnh. Chụp cuộn nằm trong lớp chọn vùng và mục khôi phục phiên chụp cuộn nằm trong Công cụ.

Xem docs/WINDOWS-ACCEPTANCE-V1540.md để thử quay và xác minh MP4 trên Windows thật.

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

Đổi riêng **Màu nền logo** và **Màu hình logo**. Bật **Gradient nền logo** để chọn thêm màu kết thúc. Hướng chuyển màu dùng chung cho các gradient đang bật. Bỏ chọn **Màu hình logo theo màu nhấn** để chọn màu hình hoặc gradient riêng. Nút **Khôi phục giao diện Captovix** xem trước logo mặc định. Bấm **Lưu** để giữ màu đã chọn. **Hủy** hoặc đóng Thiết lập trả về màu đã lưu. Icon cửa sổ đang chạy, khay và taskbar nhận màu theo lựa chọn. Captovix cập nhật shortcut ghim thuộc bản cài. Explorer có thể lưu đệm icon ghim. Nút Quay dùng nền đỏ #D82038 và chữ trắng trong cả hai chế độ.

Theo yêu cầu hiện tại, bỏ mục **Cập nhật & khôi phục dữ liệu** khỏi menu. Sao lưu trước cập nhật, kiểm hash/gói, Recovery.cmd và dữ liệu backup cũ vẫn giữ để bảo toàn an toàn nâng cấp. Mục bản quyền/khôi phục KEY không thay đổi.

## Nâng cấp và kiểm tra

Đóng app cả khay rồi chạy Captovix-v1.57.0-Full-Setup.exe vào đường dẫn đang cài. Không cần chạy migration mới, không reset database/Supabase. Document schema vẫn tối đa 23, không nâng schema mới; appearance schema 1 có thêm trường tùy chọn tương thích với tệp V1.45. Bộ cài hỗ trợ cài mới và cài đè. Gói Update-Packages.zip chứa tài sản để đưa lên kênh cập nhật hiện có. Xem scripts/README-PUBLISH.txt trước khi phát hành.

Kết quả kiểm thử logic, biên dịch chéo và kiểm gói được ghi trong docs/VALIDATION.md và docs/WINDOWS-ACCEPTANCE-V1490.md. Host hiện tại là Linux: chưa chạy app, bộ cài, phím vật lý hoặc đo RAM trên Windows thật; không coi biên dịch WPF là nghiệm thu Windows. Publish stable vẫn cần báo cáo Windows thật.

**V1.55.0 — Recording Recovery** vẫn nằm trong backlog. Mốc đề xuất tiếp theo là **V1.61.0 — Audio Keyframes & Export Presets**.

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

Không cần chạy migration mới từ V1.45.0. Giữ bản sao thư mục dữ liệu/dự án khi nâng cấp; schema 23 có backup trước ghi mới. Nếu quay lại bản cũ, dùng bản sao/backup tương thích, không ép đọc hoặc ghi đè schema 23. Dùng source ZIP V1.52.0 cho lần nâng cấp tiếp theo.
