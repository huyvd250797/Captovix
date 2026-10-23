# Captovix V1.13.0 — Image Composition & Templates

Ứng dụng Windows x64, C#/.NET 10 WPF, một Editor mở phóng to. Bàn giao đúng hai ZIP: Update-Packages (có setup) và Source. Chưa đăng Release/GitHub. Quay màn hình giữ chức năng hiện có; phát triển chuyên sâu về ảnh.

## Nội dung mới

- Menu **Bố cục**: tạo canvas, thêm nhiều ảnh, sắp xếp theo mẫu trực quan, đổi chỗ hai ảnh; tiêu đề, caption và ghi chú cuối là đối tượng chữ sửa được.
- Từng ảnh là lớp riêng: Fit/Fill giữ tỷ lệ, kéo nội dung ảnh trong ô, đổi vị trí bằng thanh trượt, cắt theo pixel nguồn, thay ảnh, viền/bo góc/bóng. Ảnh nguồn nhúng trong tài liệu.
- `.cvtemplate`: lưu bố cục và đối tượng vector, bỏ ảnh công việc và thay chữ bằng placeholder. `.cvproject`: một gói ZIP chứa ảnh gốc và tài liệu với đủ lớp ảnh; mở độc lập file nguồn ngoài máy.
- Callout mới có đệm 30 px, tự mở rộng chiều ngang hợp lý, giới hạn chiều cao theo canvas và clip chữ trong vùng nội dung. Mẫu preview dùng chữ/đệm/cỡ mẫu riêng; dữ liệu yêu thích giữ nguyên style thực tế.
- Màu chữ có palette và hộp chọn màu Windows. Ctrl+B/I/U cho đậm/nghiêng/gạch chân cả đối tượng, cả khi nhập inline; chưa có rich text từng từ.
- Icon vector thống nhất cho công cụ/chụp/quay/copy/xuất/thư viện/layers; chụp teal, quay đỏ dịu. Phím tắt và chụp/quay là hai module riêng. Ẩn cấu hình và địa chỉ nguồn cập nhật; trình bày thuộc tính theo công cụ trước, layers/hình học/toàn ảnh sau.
- Bỏ nút và phím P tạo Pixel hóa; giữ render dữ liệu Pixelate cũ. Vùng pixel chữ nhật/ellipse/lasso vẫn dùng để cắt/copy/tô/xóa pixel ảnh nền.

Hướng dẫn: [IMAGE-COMPOSITION-TEMPLATES.md](docs/IMAGE-COMPOSITION-TEMPLATES.md). Bản này build và kiểm tra logic trên Linux; WPF chỉ cross-build. Chưa chạy giao diện, clipboard, hộp chọn màu native, capture/DPI, cài đặt hoặc update trên Windows thật.

## Bản tiếp theo

**V1.14 — Scrolling Capture Foundation:** chọn vùng rồi cuộn một chiều; ghi các khung có overlap, ghép ảnh dài, xem điểm nối và báo vùng không chắc. Có hủy thao tác/giới hạn bộ nhớ và giữ các khung nguồn nếu ghép thất bại. Chưa triển khai; cần thử riêng theo từng ứng dụng Windows.

---

