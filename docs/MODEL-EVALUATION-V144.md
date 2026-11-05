# M6: đánh giá model và giới hạn nhận diện

## Triển khai

Adapter thật cho Ollama `/api/chat` vision, format JSON, ảnh base64, kiểm hình học normalized0..1000, timeout5–120giây, cancel, response512KB, tối đa100gợi ý. `keep_alive=0` yêu cầu unload model; việc thu hồi RAM/VRAM thực tế tùy dịch vụ. Chọn/list/save cấu hình, không tải/khởi động model cùng Captovix. Endpoint nhận ảnh vùng quét chỉ khi người dùng bấm.

## Đánh giá lựa chọn

| Lựa chọn | Điểm phù hợp | Quyết định V1.44 |
| --- | --- | --- |
| UIA + OCR + hình học cục bộ | Không cần model vision lớn; font/khung UI khi nguồn cung cấp đủ và khớp ảnh | Mặc định; vẫn phải nghiệm thu nhiều app và DPI Windows |
| Ollama vision có sẵn trên máy/người dùng cấu hình | API hỗ trợ ảnh, JSON và unload; hỗ trợ phân loại icon/table/tab/menu/panel | Adapter đã tích hợp và kiểm protocol; chưa chọn model chất lượng đạt chuẩn |
| Microsoft OmniParser v2 | Chuyên UI/icon, có detection+caption riêng; không cùng giao thức Ollama mặc định | Chưa tích hợp trọng số; cần pipeline, license từng model và benchmark riêng |

Nguồn kỹ thuật chính thức: https://docs.ollama.com/api/chat ; https://docs.ollama.com/capabilities/structured-outputs ; https://huggingface.co/microsoft/OmniParser-v2.0 . Đây là đánh giá khả năng tích hợp, không phải đo độ chính xác model.

## Bằng chứng và cổng nghiệm thu

M6ReconstructionChecks kiểm parser, tọa độ/crop, protocol HTTP, hủy, fallback, dedup, nhóm và định dạng; HTTP giả lập không chứng minh chất lượng model thật. Chưa chạy trọng số vision, chưa có model được chứng nhận hoặc bộ form thực tế đủ đa dạng. Không thông báo confidence model như xác suất đúng đã hiệu chuẩn.

Trước bật model làm mặc định: tối thiểu30form thực tế (WinForms/WPF/Web, light/dark, serif/italic, bảng/tab/menu/icon, nền phức tạp); có nhãn loại/khung/parent/font. Đo precision/recall tại IoU0.5 và0.75, tỷ lệ trùng, đúng quan hệ, sai khác font/cỡ/style/màu và số bước sửa; đo p50/p95 thời gian và peak RAM cùng máy. Mục tiêu đề xuất: precision>=95%, quan hệ>=95%, trùng<=1%, không tự áp dụng phần chưa chắc; đối tượng xác nhận style phải giữ100% các thuộc tính qua sửa/lưu. Chưa đo: không coi các ngưỡng này đã đạt.

Icon và bảng: nhận biết khung/cấu trúc có thể dùng UIA/model/đường lưới. Pixel của icon/grid phức tạp được giữ ở nền; bản này không vector hóa mọi icon hay tạo spreadsheet/table widget đầy đủ. Nếu bỏ Giữ ảnh cho Block, cần kiểm trước vì phần nền đó sẽ được thay bằng khối phẳng. Cell text có thể chọn sửa riêng.
