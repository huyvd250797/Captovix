# Captovix V1.6.1 — Exact Desktop Capture Hotfix

Sửa lỗi chụp vùng rộng/cao lẻ bị thiếu 1 pixel: 1467×386 phải giữ nguyên 1467×386. Ảnh vùng/toàn màn hình lấy pixel desktop bằng GDI BitBlt thay vì engine ép chẵn. Không scale nội suy hay thêm cột giả để khớp kích thước. Hỗ trợ tọa độ màn hình âm và tùy chọn con trỏ, giữ pipeline SDR8 PNG.

Giữ quay video, chụp cửa sổ, thư viện, dữ liệu, sửa chuyển ảnh V1.5.1 và URL cập nhật. Thông báo DLL/runtime chỉ hiện đúng loại lỗi. Cài qua update hoặc setup trong Update-Packages; source không phải gói update.

Build/test logic thực hiện trên Linux, chưa xác minh GDI, clipboard, installer/update và ma trận DPI trên Windows thật. HDR/màu rộng chưa đánh giá. Bản tiếp theo V1.7 sẽ thêm hình dạng và thuộc tính viền/nền/nét với đối tượng sửa được sau khi vẽ.
