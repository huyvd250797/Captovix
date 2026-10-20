# Validation — Captovix V1.1.0

Source đầu vào: Captovix-v1.0.3-Source(1).zip do người dùng cung cấp. Ngày đóng gói: 20/10/2026.

## Kiểm tra logic

- 12 geometry checks và 38 editor/history/migration checks.
- 76 AnnotationChecks: migration schema, serialize/copy loại hiệu ứng, giới hạn dữ liệu, 6 cách căn một/nhóm, undo/redo, tay nắm/tỷ lệ/font/snap/clamp, blur so với hàm tham chiếu độc lập, pixel average/alpha, output tile so với cả vùng, preset riêng/validation/atomic persistence.
- 41 UpdateChecks core/state/config/backup, gồm preset trong backup. Sau khi tạo full.nupkg, chạy thêm 9 kiểm tra SDK thực tế với bản cài mô phỏng 1.0.3 để phát hiện/tải/xác minh 1.1.0 và từ chối gói không hợp lệ.
- 10 installer contract checks tĩnh giữ lệnh cài không có EXE_ARGS passthrough.
- EditorWindowsChecks bổ sung ảnh xuất mask/crop/zoom/layering/hit-test; chỉ cross-build tại đây, chưa chạy.

## Giới hạn môi trường

Không có Windows desktop trong môi trường đóng gói: chưa thực thi WPF/renderer/clipboard, UI click/drag thực tế, NSIS/CoreSetup.exe/Update.exe hoặc capture/âm thanh/DPI. Cross-build và core tests không thay thế kiểm thử Windows. Xem WINDOWS-TEST-CHECKLIST.md trước khi phát hành rộng. Không tự Publish Release lên GitHub.

Blur/pixel là hiệu ứng làm giảm chi tiết, không bảo đảm giấu hết thông tin. Che kín cho pixel màu đặc. Ảnh gốc/sidecar vẫn giữ dữ liệu gốc để sửa lại; xuất ảnh tổng hợp để chia sẻ.
