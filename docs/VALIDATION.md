# V1.15.0 — Kiểm tra ngày25/10/2026

Linux x64: main app build0 lỗi/0 cảnh báo;21 bộ logic/static,1278 assertions đạt;11 bộ WPF chỉ cross-build. Bộ AdvancedScrollChecks mới gồm20 assertions ghép ngang, không mất/lặp cột, transpose roundtrip và cắt header/footer đúng từng byte. Các bộ ảnh/editor/shapes/text/clipboard/geometry/update cũ chạy lại. WPF runtime không thực thi trên Linux.

Gói nupkg thật:61 assertions UpdateChecks (52 baseline đã tính trong1278 và9 kiểm tra tích hợp thêm). Verify-Packages xác minh42 điều kiện gồm hash/size/feed, ZIPCRC, source đầy đủ, payload so byte, stable URL, installer wrapper, nền capture/native và câu lệnh installer giữ nguyên từ1.14. Kết quả đóng gói được xác nhận khi bàn giao.

Chưa kiểm trên Windows thật: tự cuộn quaWM_MOUSEWHEEL/HWHEEL, pause/resume/focus, dừng cuối trang, tương thích app đích, client/menu, khôi phục và loại frame, DPI/màn phụ, cài/update. Chưa có app nào được công bố nghiệm thu. Các tiêu chí không mất/lặp dòng/cột trên trang mẫu thực tế còn chờ. Xem docs/ADVANCED-CAPTURE.md và mẫuHTML dọc/ngang. Không suy diễn kiểm tra pixel tổng hợp thành nghiệm thu Windows.
