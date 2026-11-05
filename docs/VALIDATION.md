# V1.46.0 — Validation

Ngày 05/11/2026. Nền: source V1.45.0 Boss đính kèm. SDK .NET 10.0.100 trên Linux, target Windows x64, runtime tự chứa 10.0.13. Giữ phiên bản dependencies, licensing và backend.

Source gồm 87 dự án kiểm tra: 49 thuần .NET và 38 Windows/WPF. Bộ mới MockupReliabilityChecks kiểm tách nút có tên nhưng không OCR runs, thứ tự lớp/nhóm, font giữ nguyên, sửa thân/nhãn độc lập, nhãn nhiều dòng, cấu hình gradient và tương thích appearance cũ. MockupReliabilityWindowsChecks có fixture pixel/glyph khi đổi nền, no-op inspector và gradient/icon; chỉ biên dịch trên Linux.

Tệp CHECKS-V146.txt và PACKAGE-VALIDATION-V146.txt ghi kết quả cuối sau khi kiểm/đóng gói. Báo cáo V145 và trước đó là lịch sử. Hai assertion cũ được cập nhật theo hành vi mới: nút UI-only có hai lớp; caption không còn bị thay thuộc tính nền khi sửa thân.

Chưa chạy bộ cài, app hoặc WPF fixtures trên Windows thật. Không khẳng định phím vật lý/DPI/IME/scroll native đạt, không có số đo RAM hoặc p95 Windows mới. Checklist bắt buộc trước stable: WINDOWS-ACCEPTANCE-V146.md. First-paint smoke vẫn không thay thế nghiệm thu toàn app.

Không có SQL/migration mới. Schema tài liệu vẫn tối đa 23; appearance schema 1 thêm trường tùy chọn. Menu khôi phục dữ liệu được bỏ theo yêu cầu, nhưng backup trước update, hash/audit và Recovery.cmd vẫn giữ. KEY/backend/ảnh nguồn không bị thay.

Font ảnh raster chỉ được đối chiếu gần giống, không chứng minh danh tính font. Khi không sửa, lớp gốc và định dạng lưu giữ nguyên. Sau khi đổi màu thân phẳng, caption chưa sửa dùng nét raster/mặt nạ alpha; nhãn có nhiều màu hoặc nền phức tạp vẫn cần rà soát. Nền vị trí cũ sau di chuyển chưa có inpaint tổng quát. Icon cá nhân áp dụng trong app/cửa sổ/tray; EXE/installer/shortcut giữ icon tĩnh đã duyệt.
