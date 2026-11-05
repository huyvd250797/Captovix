# V1.42.0 — Element Recognition & Fidelity

- M5: chữ/control riêng theo nhóm, liên kết label–input và khối–con; giữ màu, ước lượng viền/bo góc/font, chỉnh trong rà soát trước Apply.
- Một viền popup/menu/modal mảnh theo sáng/tối; bỏ viền và bóng dư của Thêm, không dùng màu nhấn cho khung.
- Trang Dung lượng & bộ nhớ, dọn có chọn lọc cache/thumbnail/temp cũ; giữ nguồn, chỉnh sửa, backup, KEY; thumbnail dựng lại từ bản chỉnh.
- Giới hạn cache raster96MiB, thumbnail16MiB; giảm cấp phát khi lấy màu và tái sử dụng phân tích hình học. Không giảm chất lượng ảnh gốc.
- Không migration/backend mới. Giữ sửa lỗi startup1.41.1. Native Windows chưa nghiệm thu trong môi trường Linux; xem VALIDATION.
