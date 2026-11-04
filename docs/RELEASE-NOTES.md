# Captovix V1.37.0 — Performance & Responsiveness

Nâng từ source V1.36.0 Boss gửi; tập trung hiệu năng, giữ công cụ và sửa lại Callout. Không thay dependency/runtime hay cấu hình license/backend.

Đã triển khai:

- Giảm công việc trên đường khởi động: danh sách thumbnail từ50 còn30, nhận diện media đã có bằng HashSet thay quét toàn bộ metadata cho từng file; danh sách font chỉ nạp khi mở picker, sửa shortcut sau lần vẽ giao diện đầu tiên.
- Mở/chuyển ảnh ở nền, chỉ một worker giải mã cùng lúc. Hủy yêu cầu đang đợi; generation/cancellation loại kết quả cũ khi chuyển nhanh hoặc đóng cửa sổ. Giải mã native đang chạy sẽ hoàn tất rồi bị bỏ nếu không còn cần; không hứa ngắt WIC giữa chừng.
- Lưu tự động ở nền bằng snapshot độc lập, không serialize/deserialize vòng trên UI. Gộp thay đổi bằng timer; không chạy nhiều autosave cùng lúc. Revision ngăn báo Đã lưu cho nội dung mới chưa ghi. Khi chuyển ảnh/đóng/cập nhật, đợi worker cũ rồi lưu cuối, tránh bản cũ ghi đè bản mới.
- Kéo/resize: giữ cache chữ/hình từ1.36, tránh tìm lại đối tượng chính trong toàn bộ danh sách ở mỗi node; bỏ quét connector lặp khi đã ở trong phiên nhập. Zoom/hover dùng lại drawing của cảnh khi nội dung không đổi; chỉnh sửa, redaction, chữ inline và tương tác làm hết hạn cache.
- Đóng/thay ảnh giải phóng drawing, cache chữ/đuôi và bitmap sampling từng giữ ảnh cũ, cùng cache PNG giải mã. Worker autosave cũng giải phóng cache theo thread sau khi xong; không ép GC trên UI. Undo vẫn giữ ngân sách sẵn có.
- Giữ công cụ đang chọn khi hoàn tất nhập chữ/Callout, chụp/chuyển ảnh, crop và hủy thao tác bằng Esc. Chọn công cụ khác mới thay đổi công cụ; khi bấm lệnh chuyên biệt yêu cầu chế độ Chọn (ví dụ sửa lớp ảnh), giữ hành vi riêng của lệnh đó.
- Danh sách gần đây tối đa30 nội dung, kể cả ảnh lịch sử được ghim tạm; không xóa ảnh cũ, xem đầy đủ trong Quản lý thư viện.
- Sửa Callout theo mô phỏng Boss duyệt: điểm nhấn đầu ghim đuôi; kéo ngang thay đổi rộng, kéo dọc thay đổi cao, kéo chéo thay đổi cả hai độc lập, không khóa tỷ lệ. Gốc đuôi tự bám cạnh theo hướng kéo. Thả khi hộp trống giữ kích thước vừa vẽ; chỉ tự nở khi nhập chữ. Node thân và node đuôi vẫn độc lập. Kích thước tối thiểu36px; nhấn đơn tạo hộp220×120px.
- Bổ sung đo có giới hạn trong RAM cho dựng cửa sổ/first paint, mở ảnh, autosave và frame editor; không đưa lại menu chẩn đoán đã bỏ.

Đo trước/sau logic trên cùng host Linux/.NET10, 5.000 chú thích có một Callout20.000ký tự,3lượt làm nóng +11lượt đo lấy trung vị:

| Phép đo | V1.36 | V1.37 |
| --- | ---: | ---: |
| 1.000 lần Begin trong cùng phiên sửa | 3,818ms | 0,0194ms |
| Tạo snapshot5.000chú thích | 133,9469ms | 6,518ms |
| Cấp phát mỗi snapshot | 20.472.582byte | 3.321.275byte |

Đây là microbenchmark logic, không phải đo startup/chuột/FPS/RAM Windows toàn app. JSON thô ở docs/PERFORMANCE-V137-before.json và after.json. Có script chạy lại và fixture Windows; chưa có số đo Windows để khẳng định không lag hoặc hết rò rỉ RAM trên mọi máy.

## Cài đặt và build

1. Bộ cài: `Captovix-v1.37.0-Update-Packages.zip`. Thoát cả app/khay, giải nén và chạy `HuyVo.Captovix-win-Setup.exe` vào thư mục hiện tại. Không uninstall.
2. Source: `Captovix V1.37.0 — Performance & Responsiveness.zip`. Windows x64, .NET10 SDK và NSIS3. Mở thư mục gốc, chạy `python scripts/Run-Checks.py`, sau đó `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`. Các hướng dẫn nền phía dưới tiếp tục áp dụng.
3. Kiểm đo: `python scripts/Benchmark-Performance.py`; Windows chạy thêm `dotnet run --project tests/PerformanceWindowsChecks -c Release -p:Platform=x64`. Xem `docs/PERFORMANCE-RESPONSIVENESS-V137.md`.
4. Không cần chạy migration mới, không deploy lại backend/admin; license public config và dữ liệu hiện hữu giữ nguyên. Schema17 giữ nguyên; backup trước rollback.
5. Chưa push GitHub/deploy/publish. Để bản cũ thấy1.37, upload đủ5releaseassets trong ZIP lên stable release1.37 (latest), không chỉ upload ZIP/source. Updater dùng Full nên không cần cài trung gian.
6. Tiếp theo roadmap đã điều chỉnh: **V1.38.0 — Capture & Image Reliability**: nhiều màn hình/DPI, chọn vùng/phần tử/chụp app/chụp cuộn, đối chiếu ảnh và phục hồi khi hủy/lỗi lưu. Chưa triển khai bản1.38.

