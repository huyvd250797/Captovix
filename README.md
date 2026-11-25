# Captovix V1.68.1 — 1080p Recording Sharpness Fix

- Khôi phục bộ ghi H.264 High, bitrate mode **Quality**, Quality70 như luồng V1.67. V1.68 đã đổi sang CBR với bitrate preset: cùng1080p nhưng bitrate bị giới hạn có thể mất chi tiết chữ. Đây là thay đổi cấu hình xác nhận được qua source; chưa có hai MP4 gốc để đo nguyên nhân trên máy Boss. Dung lượng ghi lại biến thiên theo nội dung, ước lượng bitrate trong preset chỉ mang tính gần đúng.
- Bỏ bước tự gắn nhãn sRGB/range cho video sau khi quay của V1.68. Bỏ ép nhãn màu trong lệnh chuẩn hóa xuất. Màu của preview không được dùng để sửa nội dung/metadata MP4. Không tự retag các video cũ; video đã mất chi tiết không thể phục hồi bằng thay nhãn màu.
- Khung tạm dừng dùng FFmpeg giải mã PNG trực tiếp, giữ1080p gốc, giới hạn tối đa4K thay cho1280×720 rồi phóng lớn. Thumbnail thư viện vẫn dùng kích thước nhỏ riêng. Khi không có FFmpeg, fallback bộ đọc Windows. Xem video đang phát vẫn dùng Windows MediaElement nên khác biệt màu do decoder/ICC/HDR cần kiểm thực tế.
- Nếu xuất toàn bộ nguồn mà không thay đổi nội dung (kể cả Split nhưng giữ liên tục, nguyên thứ tự và không hiệu ứng/điều chỉnh âm thanh), sao chép đúng byte MP4: không tái nén video hay tiếng.
- Timeline cần render vẫn cắt chính xác, nhưng tăng chất lượng từ CRF17 lên14 và bỏ scale/chroma resample cho canvas trùng kích thước nguồn. Crop/rotate/zoom/canvas khác vẫn xử lý đúng. Không dùng sharpen hay tăng saturation để che lỗi.
- Giữ con trỏ video riêng, HUD/icon, nút xuất và mọi dữ liệu/license. Không cần migration mới. Chọn preset1080p rồi quay lại để đánh giá sửa bộ ghi; file đã quay mờ không được tự biến thành bản sắc nét.

## Xác minh

20 kiểm tra media/export qua trên Linux: giữ byte nguồn chính xác, Split liên tục, loại trừ trim/reorder/motion khỏi copy, không bỏ crop, fixture nét UI1920×1080, PNG preview1920×1080, âm thanh, màu và nhiều điểm cắt. Fixture render có MAE luma dưới1.5 mức trên255; không phải đo bản quay thực tế Windows. CaptureWorkflowChecks160 qua. Publish Windows x64 và kiểm gói update trước bàn giao.

Nghiệm thu Windows còn chờ: cùng màn hình/app mẫu/preset1080p, quay V1.67/V1.68/V1.68.1, so frame gốc và file xuất ở100% pixel, kiểm bitrate/detail/âm thanh, paused preview vs playback, HDR/ICC nếu có. Không phóng ảnh720p rồi dùng nó để đánh giá file1080p. Video cũ do CBR mất nét cần quay lại; không áp bộ tăng nét tự động.

Build: chạy Build.cmd; bộ build tải FFmpeg/ffprobe theo checksum. Gói update Windows có engine/runtime: giải nén, thoát app cả khay hệ thống, chạy HuyVo.Captovix-win-Setup.exe. Gói chưa được ký số.


## Các bản trước

# Captovix V1.68.0 — SDR Color & Video Controls

- Chuẩn hóa thông tin màu của **bản quay SDR mới**: đánh dấu sRGB transfer, BT.709 primaries/matrix và dải video rõ ràng bằng remux H.264, không mã hóa lại ảnh và không tự tăng saturation/gamma. Giữ full range khi nguồn đã đánh dấu pc; dùng video range khi nguồn chưa khai báo. Timeline giữ nhãn sRGB của nguồn bản quay. Bộ ghi H.264 High/CBR dùng đúng bitrate preset.
- Hai ảnh tham chiếu cho thấy khác màu nhưng không đủ xác định toàn bộ nguyên nhân (màn hình HDR, profile ICC, decoder và file MP4 gốc chưa được cung cấp). Đây là sửa luồng metadata cho SDR; không khẳng định HDR hay video cũ đã được phục hồi. Cần so video mới với màn hình trên Windows thực tế.
- Trước khi quay có **Hiển thị con trỏ chuột trong video**, mặc định bật và lưu độc lập qua restart. Cấu hình con trỏ trong ảnh chụp không còn điều khiển con trỏ bản quay. Mouse pointer và từng nguồn ghi đều nhận đúng cấu hình video.
- Fullscreen dùng icon phát tam giác / tạm dừng hai vạch; nút tua10giây và thoát fullscreen là icon có tooltip/nhãn truy cập. Thanh tua dùng cùng kiểu slider của app, HUD giới hạn chiều rộng, tương phản rõ.
- Click trái ngoài HUD/nút thoát sẽ ẩn HUD ngay. Di chuyển chuột thật sự (không phải layout đổi dưới con trỏ) hiện lại; vẫn tự ẩn sau2giây. Click nút/slider không bị chặn. Kéo tua không bị ẩn giữa thao tác.
- Hai nút **Xuất MP4** ở màn chính và Studio dùng cùng icon, nhãn, kích thước và style Primary. Giữ màu thương hiệu Captovix; không thay logo hay sao chép tài sản CapCut.
- Không cần migration mới. Giữ dự án schema8, thư viện và KEY.

## Build / cập nhật

Chạy Build.cmd trên Windows: tự lấy FFmpeg/ffprobe đã pin, build và đóng gói bản cài. Để dựng bằng IDE, chạy scripts/Ensure-MediaEngine.ps1 trước. Bản update Windows đã kèm hai engine; giải nén và chạy HuyVo.Captovix-win-Setup.exe sau khi thoát app. Source ZIP không nhúng executable của media engine.

## Kiểm tra

- VideoExportChecks:16 kiểm tra media thật, gồm metadata sRGB/range, giữ nguyên mẫu YUV qua remux, nhãn màu qua xuất timeline và RGB của ô đen/trắng/xám/xanh UI sai lệch tối đa3/255 trong fixture.
- CaptureWorkflowChecks:160 kiểm tra qua. Publish WPF Windows x64 trên Linux. Test không phải bằng chứng về màu thực tế của Desktop Duplication/MF/WPF trên màn hình Boss.
- Nghiệm thu Windows: quay cùng một bảng màu và app mẫu, so màn hình / MP4 / xuất timeline; thử bật/tắt cursor riêng với ảnh, restart; click ngoài HUD, tua và đổi trạng thái play/pause; so hai nút xuất ở light/dark; kiểm tương tác trong Windows thật. HDR/ICC, webcam và video nhập cần kiểm riêng.

## Tài liệu các bản trước

# Captovix V1.67.0 — Viewer Controls & Retention Catch-up

- Dọn bù khi mở app: nếu Captovix/máy không chạy ở23:59, lần mở tiếp theo xác định mốc23:59 gần nhất chưa xử lý, gom ảnh/video đủ hạn tại mốc đó và chuyển vào Thùng rác Windows khi app rảnh. Không cần chạy app khi máy đã tắt. Sau dọn bù, chờ lịch mới; không quét/xóa liên tục. Cấu hình mới bật lần đầu chờ mốc23:59 đầu tiên.
- Thư viện có lọc nhanh Tất cả / Ảnh / Video, áp dụng trên dữ liệu trước khi lấy30 nội dung gần nhất. Quản lý lịch sử cũng có lọc loại.
- Xem video tại màn chính có phát/dừng, lùi/tiến10 giây, tua, fullscreen và xuất MP4. Fullscreen có cùng thanh tua, thời gian và nút điều khiển. Không di chuột2 giây sẽ ẩn điều khiển và con trỏ; di chuyển/click hiện lại. Kéo tua không bị ẩn giữa thao tác. Esc/F11 thoát; Space phát/dừng; trái/phải tua10 giây trong fullscreen.
- Xuất MP4 từ màn chính và Studio dùng cùng dự án đã chỉnh. Khi có bộ FFmpeg/ffprobe, phần đọc nguồn và xác minh đầu ra không còn phụ thuộc MediaClip/MediaPlayer Windows. Cache thông tin theo nguồn khi nhiều đoạn Split dùng chung tệp. Lỗi FFmpeg hiển thị chi tiết để chẩn đoán. Chưa có log lỗi trên máy Boss nên vẫn cần nghiệm thu thực tế.
- Xuất xong có thể mở thư mục chứa MP4 ngay ở hộp thông báo, hoặc nút Mở thư mục xuất. Kèm FFmpeg và ffprobe trong gói Windows; không cần cài riêng.
- Không cần migration mới. Giữ schema video8 và thư viện/license. Gói chưa được ký số. Kiểm build trên Linux với Windows targeting; nghiệm thu tương tác thực tế Windows còn chờ.

## Hướng dẫn bản trước

# Captovix V1.66.0 — Timeline Export & Library Retention

- **Xuất timeline MP4 liền mạch:** chuẩn hóa từng đoạn rồi ghép theo thứ tự đã chỉnh sửa. Trim, Split, đổi thứ tự, tốc độ, dừng hình, canvas, chú thích, chỉ dẫn chuột và camera dùng cùng timeline. Âm thanh được phối theo cùng mốc nguồn. Ranh giới khung hình được làm tròn theo thời gian tích lũy để tránh sai số sau nhiều điểm cắt. Chỉ thay tệp đích sau khi xác minh MP4, kích thước và thời lượng.
- **Chọn vùng quay modal:** click khung tự nhận diện để khóa vùng, điều chỉnh nếu cần, rồi bấm **Quay** hoặc Enter. Chọn vùng luôn ghi một tọa độ màn hình cố định; không chuyển sang quay cửa sổ và không chạy ngay lúc click modal. Cửa sổ di chuyển sau đó không làm vùng quay chạy theo.
- **Chọn cửa sổ có ảnh xem trước:** các ô hiển thị tên và preview DWM của cửa sổ đang mở. Chọn ô rồi xác nhận, nhấp đúp hoặc Enter. Có Làm mới và kiểm lại cửa sổ trước khi nhận. Preview được giải phóng khi đóng hộp chọn.
- **Tự dọn thư viện:** Thiết lập → Dung lượng & bộ nhớ → Tự dọn thư viện. Mặc định tắt. Nhập 1–36500 ngày (mặc định 60), bật và Lưu lịch tự dọn. Tính hạn từ ngày chụp/quay theo giờ Windows, kể cả nội dung cũ. Chuyển tệp hết hạn và bản chỉnh liên quan vào **Thùng rác Windows** thành một thư mục có thông tin phục hồi; không xóa vĩnh viễn bằng lệnh của app.
- Lịch chạy lúc **23:59 hằng ngày khi Captovix đang mở hoặc chạy nền**. Không cài dịch vụ/task tự khởi động. Nếu thoát hẳn hoặc tắt máy, chờ lịch 23:59 tiếp theo khi app hoạt động; timer quá hạn sau sleep được xử lý khi máy thức. Khi đang quay, xuất hoặc thao tác bận, lùi xử lý đến lúc rảnh. Lỗi trong phiên chỉ thử lại ở lịch ngày sau. Không quét/xóa liên tục khi đang sử dụng app.
- Video liên kết gỡ khỏi thư viện nhưng giữ nguồn ngoài app. Nguồn còn dùng bởi dự án chưa hết hạn được giữ; các bản backup cập nhật được giữ. Nếu cần phục hồi một mục đã tự dọn: mở Thùng rác Windows, khôi phục thư mục, xem `Captovix-recovery.json` bên trong để biết đường dẫn nguồn/bản chỉnh ban đầu. Nút Khôi phục của thao tác xóa thủ công vẫn dùng thùng rác Captovix riêng.
- Không cần chạy migration mới. Giữ dữ liệu, license và schema dự án video 8 của V1.65.0.

## Dựng bản cập nhật

1. Cài .NET 10 SDK x64 và các yêu cầu build ở phần hướng dẫn bên dưới.
2. Giải nén rồi chạy **Build.cmd**. Script tự tải FFmpeg 9.0.2 essentials từ Gyan về `media-engine/`, kiểm SHA-256 cả ZIP và executable, và đưa `ffmpeg.exe` vào payload/bản cài. Cần Internet lần đầu. Không cần đặt FFmpeg thủ công.
3. Source ZIP không nhúng executable FFmpeg. Nếu dựng trực tiếp bằng IDE/dotnet, chạy `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Ensure-MediaEngine.ps1` trước, rồi build/publish. Không phát hành bản thiếu media engine.
4. Build/cài bản Windows theo hướng dẫn hiện có. Giữ cấu hình public licensing và update-source hiện tại; không cần reset thư viện hoặc database.

## Xác minh V1.66.0

- `dotnet run --project tests/VideoExportChecks -c Release`: dùng FFmpeg thật và các hàm dựng lệnh đang dùng ở production; kiểm màu khung hình sau cắt/đổi thứ tự/tốc độ/dừng hình, tần số âm thanh, tổng thời lượng và sai số 1.000 điểm cắt.
- `dotnet run --project tests/LibraryRetentionChecks -c Release`: hạn 60 ngày theo ví dụ, tắt/bật, giờ 23:59, lưu cấu hình, chuyển tệp và phục hồi khi gián đoạn trước/sau commit.
- Checklist Windows cho release: `docs/V1.66.0-WINDOWS-QA.md`. Linux chỉ kiểm build WPF và phần logic/media chạy đa nền tảng; chưa xác nhận tương tác DWM, thu màn hình và Recycle Bin thực tế trên Windows.

## Chức năng đã có từ V1.65.0

- Chọn webcam trong **Nguồn quay**. Captovix lưu màn hình, mic và webcam riêng. Nếu webcam không mở được hoặc bị rút, Captovix tiếp tục lưu màn hình và mic. Tệp webcam và bản đồ đồng bộ nằm cùng MP4.
- Mở **Studio → Camera · hình trong hình**. Chọn video trình bày, bật/tắt camera, chọn góc hoặc nhập vị trí và kích thước. Video nhập có thể chỉnh mốc lệch. Camera không phát tiếng riêng, bản phối dùng track mic/âm thanh của dự án. Xuất MP4 với lớp camera cần FFmpeg có libx264.
- Hợp nhất lớp ảnh chỉ render vùng lớp cần gộp ở kích thước pixel gốc và giữ byte ảnh nền ở các điểm không phủ. Nếu lớp đã thu nhỏ, chi tiết đầu ra vẫn bị giới hạn bởi kích thước canvas.
- Studio tạo waveform khi mở nhóm Âm thanh hoặc cần phối để phát. Khi rời Studio, hiệu ứng GPU, công việc âm thanh và bộ giải mã được giải phóng sau khi giữ khung tĩnh.
- Splash hiển thị lúc mở app. Nếu bật khởi động cùng Windows, Captovix hiện splash rồi mở cửa sổ chính.
- Dự án schema 8 mở được V1–V7. Bản sao thư viện có webcam dùng schema 4. Không cần migration cơ sở dữ liệu.

## Cursor & Click Effects từ V1.64.0

- Mở **Studio → Con trỏ, phím & marker**. Nhập click và marker của bản quay đã có dữ liệu, hoặc thêm thủ công click, vòng sáng, nhãn phím và marker. Với click/vòng sáng, chọn **Thêm tại đầu phát** rồi bấm vào video. V1.54.0 chỉ lưu click và marker.
- Chỉ dẫn neo vào video nguồn, bám theo trim, split, crop, xoay, zoom và tỷ lệ. Khi cắt mất mốc nguồn, chỉ dẫn tại mốc đó cũng biến mất. Xuất video có chỉ dẫn cần FFmpeg có libass và libx264.
- Rê chuột trên ô video ở Thư viện rồi bấm nút ảnh, hoặc dùng **Ảnh Thư viện** dưới video đang mở. Tua đến mốc và lấy khung bằng bộ giải mã Windows, hoặc chọn ảnh từ máy.
- Thoát Studio hủy phối âm nền, tắt bộ hẹn giờ khi rảnh và thay bộ giải mã bằng một ảnh tĩnh. Khi phát hoặc tua, app mở lại video.
- Dự án video schema 7 mở được V1–V6. Không cần migration cơ sở dữ liệu.

## Motion & Zoom từ V1.63.0

- Mở video và kéo thanh tua. Chọn **Ảnh đại diện** để dùng khung hiện tại hoặc ảnh từ máy.
- Chọn **Vào Studio → Chuyển động & tốc độ**, chọn đoạn rồi nhập tốc độ 0,25–4×, số giây dừng cuối đoạn và mờ dần vào/ra. Để giữ một khung giữa đoạn, Split tại mốc đó rồi đặt dừng hình cho đoạn bên trái.
- Tua trong đoạn, nhập Zoom và tâm ngang/dọc, rồi **Lưu keyframe**. Có thể chọn một keyframe để tua tới mốc, cập nhật hoặc xóa.
- Tốc độ và dừng hình dùng cùng thời gian cho timeline, phối âm và MP4. Âm thanh nguồn giữ cao độ bằng FFmpeg, phần dừng hình không có âm thanh nguồn. Nhạc nền vẫn theo timeline. Cần FFmpeg cho MP4 chuyển động, phối âm và lấy khung làm ảnh đại diện.
- Chú thích không giao nhau tự xếp chung một hàng. Khi kéo, hình và tay nắm đi cùng nhau. **Quay lại** ở góc trái đóng công việc tạo waveform của Studio.
- Dự án video dùng schema 6. V1–V5 vẫn mở được. Không cần migration cơ sở dữ liệu.

## Editor Responsiveness từ V1.61.1

- Dùng **Đậm nền** và **Đậm viền** để chỉnh chú thích. Callout có thanh rộng và ô phần trăm đồng bộ. Kéo để xem thay đổi trực tiếp, thả chuột để ghi một bước hoàn tác. Có thể nhập số rồi nhấn Enter hoặc rời ô.
- Chọn ảnh chèn, mở **Lớp ảnh → Opacity (%)** để chỉnh độ trong suốt của ảnh. Các chú thích cũ giữ giá trị độ đậm đã lưu.
- Trình chỉnh sửa tái sử dụng hình học Callout, mẫu công cụ và bitmap đang vẽ để giảm công việc khi rê chuột và kéo.

## Chú thích và mẫu đã lưu

- Trong **Studio**, tua tới điểm bắt đầu và chọn **Tiêu đề**, **Chữ**, **Callout**, **Mũi tên**, **Khung** hoặc **Bước**. Nhấp đúp chú thích chữ để sửa trực tiếp trên canvas. Chọn đối tượng để chỉnh nội dung, màu, font, căn lề và opacity trong bảng bên phải.
- Kéo Callout từ vị trí đuôi tới vị trí đặt hộp như trình chỉnh ảnh. Tay nắm cam di chuyển đuôi, tay nắm giữa hộp di chuyển thân, các tay nắm biên đổi kích thước. Giữ Ctrl khi vẽ đè lên chú thích có sẵn để tạo đối tượng mới.
- Kéo mỗi lớp trên timeline hoặc chỉnh hai đầu, hoặc dùng **Hiện tại đầu phát** và **Ẩn tại đầu phát** để lấy thời điểm chính xác. Chú thích xuất hiện từ mốc đầu và ẩn ở mốc cuối. Preview và xuất dùng cùng renderer, tọa độ được lưu theo tỷ lệ canvas.
- Mở **Đã lưu**, dùng **Lưu công cụ hiện tại** để thêm kiểu đang chỉnh vào danh sách chung với chỉnh ảnh. Mẫu từ phiên bản trước khi có Opacity được đọc lại nếu còn trong tệp cài đặt. Áp dụng mẫu lên chú thích đang chọn giữ vị trí, thời gian và nội dung của lớp đó.
- Dự án video schema 5 mở được V1–V4. Các dự án đã lưu bằng schema 5 cần V1.61.0 hoặc mới hơn để mở.

## Video Studio và âm thanh từ V1.60.0

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

**V1.55.0 — Recording Recovery** vẫn nằm trong backlog. Mốc đề xuất tiếp theo là **V1.66.0 — Audio Keyframes & Export Presets**.

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
