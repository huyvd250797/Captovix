# V1.44.0 — M6 Advanced UI Reconstruction: validation

Ngày05/11/2026. Host Linuxx64, SDK.NET10.0.100. Windowsx64/.NET10.0.13, Velopack1.2.161, NSIS3.09 và OCR/backend/license giữ nền1.43.

## Phạm vi kiểm tra

82project:47logic chạy trên Linux và35WPF/Windows chỉ cross-build. Hồi quy chạy tuần tự theo2phần log; lỗi nhóm lồng nhau tìm được ở M6 đã sửa và chạy lại các bộ liên quan. Không dùng kết quả compile để coi như đã cài/mở app trên Windows.

M6ReconstructionChecks22PASS: TimesNewRoman14px xanh nghiêng/gạch chân giữ qua sửa/undo/redo/serialize, schema22/nhóm panel–row–control không nhiều cha, màu nền control đồng bộ text, historylossless, dedup, bảng grid, fallback nền phức tạp, normalizedcrop và giao thức/hủy AI. M6WindowsChecks có renderer/sidebar/doubleclick/backup22/reopenpixel; chưa chạy native.

PERFORMANCE-M6-V144.json: controlled fixture40bước sửa cùng PNG1MB, lịch sử JSON lặp115159580byte so với dữ liệu chia sẻ/nén2713262byte. Đây là retained payload theo mô hình lịch sử, không phải working set toàn tiến trình hoặc đo cùng máy Windows. Không giảm chất lượng source/export. Một ảnh TaskManager thật854×749 do Boss cung cấp đã chạy smoke phân tích pixel, không sửa ảnh nguồn; chưa có groundtruth nên không dùng để khẳng định độ chính xác form. Không đính kèm ảnh riêng tư trong source.

Giữ Check-M4-UI.py11PASS (menu/Explorer/chrome/themes). Verify-Packages.py kiểm byte/runtime/OCR/installericons/feedhash/sourceZIP sau đóng gói; kết quả trong PACKAGE-VALIDATION-V144.txt. StartupResourceChecks giữ guardparent ContextMenu/ToolTip và fallback fatal độc lập.

## Các giới hạn chưa được nghiệm thu

- Chưa chạy Setup, WPF, UIA, IME và khởi động thực trên Windows10/11 trong phiên này. Bắt buộc chạy WINDOWS-ACCEPTANCE-V144.md trước phát hành rộng, đặc biệt tránh lặp lỗi không mở app.
- Chưa có model vision/trọng số thật được đo trên bộ>=30form; HTTP giả lập chỉ xác minh adapter. MODEL-EVALUATION-V144.md nêu lựa chọn, tiêu chí và phần chưa đạt. Không vector hóa mọi icon hoặc tái dựng mọi table widget; pixel gốc/giữ ảnh là fallback có chủ đích.
- Font từ bitmap chỉ là ước lượng; thiếu stylecomplete UI phải rà soát/xác nhận hoặc giữ ảnh. Sau khi xác nhận, sửa chữ không tự fit lại cỡ. Font phải có trên máy.
- Chưa đo p95UI hoặc RAM của toàn tiến trình trên máy Boss trước/sau. Cache48+16MiB, history có ngân sách nhưng giữ tối thiểu1bước; ảnh8K, OCR, recorder và dịch vụAI có working set riêng.
- Chưa push/publish GitHub Releases và chưa ký Captovix bằng chứng thư thương mại. Không migration/deploy backend/đổi KEY. Semantic role mới nâng sidecar22 có backup.pre-v1.44.bak.

## Phiên bản kế tiếp

V1.45.0 — Update, Recovery & Release Quality: kiểm update/cài đè/khôi phục trên Windows thật, bảo vệ dữ liệu và cải thiện theo kết quả corpus/đo cùng máy.
