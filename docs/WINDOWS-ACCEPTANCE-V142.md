# Nghiệm thu Windows V1.42.0 — chưa thực hiện trên host Linux

Không đánh dấu đã đạt bằng kết quả cross-build. Thử Windows10/11x64, scale100/125/150/200%, cả hồ sơ mới và dữ liệu1.41.1.

1. Chạy Run-Checks.py: StartupResourceChecks và StartupWindowsChecks phải qua; BAML load, TreeView, modal mở/đóng, popup mở nhiều lần không lỗi logical parent.
2. Thoát app/khay; Full Setup cài đè đúng thư mục cũ và D:\Captovix\Captovix; mở từ shortcut, restart app3lần. Giữ ảnh, nhóm, KEY/trial, cấu hình và update source.
3. Thêm và ComboBox/menu chuột phải/modal KEY/settings/quét: chỉ một viền mảnh, màu bạc dịu, đổi light/dark/accent; ngoài modal chỉ nháy viền, không ẩn cửa sổ.
4. Quét form có chữ Việt/Anh, nút tối/chữ sáng, ô nhập nền trắng có viền, góc vuông/bo, label trái/trên, panel chứa control. Rà soát khung/loại, sửa font/viền/bo góc; xem trước. Không nhận font đúng tuyệt đối từ raster.
5. Apply có chữ/control riêng trong nhóm, Alt+click/cây chọn chữ; di chuyển nhóm, sửa chữ, xóa/nhân bản/copy. Undo/Redo một bước, lưu/mở lại, xuất PNG giữ bố cục. So ảnh gốc ở100%; không để lặp chữ hoặc mất phần tử. Thử sửa text trong rà soát: nội dung sửa dùng đối tượng control có text tích hợp nếu không còn khớp các dòng OCR.
6. Hủy quét/đóng dialog khi OCR đang chạy, đổi ảnh, quét vùng20MP/200khung; phần cũ không áp dụng nhầm. Giới hạn120đối tượng tính cả chữ tách.
7. Trang dung lượng: thư viện có media ngoài dataRoot, file thiếu/quyền hạn, cache trống; số liệu riêng. Dọn chọn một mục/cả nhóm; file mới dưới24giờ/đang dùng/đã đổi/symlink giữ nguyên. Đối chiếu ảnh gốc/edits/KEY/backup trước/sau.
8. Xóa thumbnail, mở lại thư viện: thumbnail có bản chỉnh phải khớp ảnh đã chỉnh, không chỉ ảnh gốc. Giải phóng RAM khi đang mở ảnh; ảnh/công cụ vẫn hoạt động, thumbnail gần đây nạp lại. Cuộn thư viện lớn, chuyển ảnh, zoom/drag, lưu nền, mở/copy ảnh có lớp ảnh.
9. Đo working set/private bytes và p95 thời gian mở/chuyển/drag/scan trước/sau trên cùng máy/dữ liệu. Không kết luận RAM toàn app đã giảm chỉ từ trần cache.
10. Chụp vùng/cửa sổ/cuộn, OCR Việt/Anh, quay mic/system, paste ảnh, export/PDF; cập nhật1.41.1→1.42 và nhảy bản cũ; gói lỗi checksum bị từ chối; thử mất mạng/retry/rollback.

Chỉ publish stable/Latest sau khi checklist đạt. Có lỗi mở app: giữ errors.log và quay lại1.41.1 bằng bộ cài đúng thư mục, không xóa dữ liệu/KEY.
