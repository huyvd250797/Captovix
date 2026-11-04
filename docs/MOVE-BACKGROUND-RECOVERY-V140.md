# V1.40.0 — Move & Background Recovery (M3)

Xây từ source V1.39.0 đã bàn giao, đối chiếu với gói Update-Packages V1.39.0 Boss đính kèm. Giữ M1/M2, sửa capture bounds, ảnh dán/thread, license và runtime hiện có.

## Di chuyển, resize, xóa và nhân bản phần tử

1. Tạo/chọn phần tử Mockup bằng Thêm → Mockup (M1/M2 như trước). Kéo thân để di chuyển, kéo node để resize, Delete để xóa, Ctrl+D để nhân bản; có một bước Undo/Redo cho mỗi thao tác hoàn tất.
2. M3 tách **mặt nạ nền gốc** thành dữ liệu riêng khỏi thân phần tử. Khi di chuyển/resize, mặt nạ giữ nguyên tọa độ/kích thước nguồn; khi xóa phần tử, mặt nạ vẫn tồn tại nên nút/chữ gốc không hiện lại. Bản sao/paste chỉ mang thân phần tử, không sao chép mặt nạ hoặc tham chiếu đến ảnh cũ.
3. Mặt nạ chứa bit chọn pixel, không chứa ảnh gốc. Nền phẳng được ước lượng từ dải bao quanh vùng nguồn; không lấy màu thân nút làm màu trang. Đổi màu/nền thân không tự đổi nền đã phục hồi ở vị trí cũ.
4. Với nút/ô nhập/khối: phân tích thành phần liên thông, lấy vùng thành phần chính rồi giữ các nét chữ/viền bên trong. Thành phần rời ở cạnh ngoài và vùng của Mockup lân cận được bảo vệ. Pixel ngoài khung nguồn không bị phục hồi nền thay đổi. Vị trí đích là một đối tượng overlay; đặt chồng có thể che phần tử khác khi hiển thị nhưng không ghi đè pixel ảnh gốc.
5. Các thao tác trên đối tượng V1.38/1.39 được chuyển sang mặt nạ độc lập khi cần; không buộc tạo lại thư viện. Đối tượng mới có thể tách nền ngay lúc Áp dụng.

## Kiểm tra và chỉnh nền

- **Thêm → Nền & mặt nạ**, hoặc chọn phần tử Mockup → nút **Nền & mặt nạ…** trong thuộc tính. Ô công cụ động nhớ công cụ như các chức năng khác.
- Xem nguồn và nền sau phục hồi; bật Hiện mặt nạ để xem pixel chịu tác động. Chỉnh màu nền bằng HEX/picker, bấm Xem lại.
- Bật **Kéo vùng cần giữ** rồi kéo khung bao chi tiết lân cận cần bảo vệ. Tắt để chọn/kéo/resize các khung giữ; có nút bỏ khung đang chọn.
- **Lưu nền** áp dụng trong một bước Undo. **Đóng** không đổi tài liệu. **Bỏ mặt nạ** chủ động làm hiện lại phần tử nguồn của vùng đó, có Undo. Kể cả phần tử đã bị xóa, vẫn quản lý được mặt nạ ở đây.
- Nhật ký100thao tác Mockup/nền gần nhất lưu cùng sidecar/dự án; xem các mục gần đây trong cửa sổ Nền & mặt nạ. Đây là nhật ký kết quả thao tác. Undo/Redo dùng lịch sử của phiên đang mở; không khẳng định khôi phục toàn bộ stackUndo sau khi đóng app/chuyển ảnh.

## Giới hạn rõ ràng

M3 ưu tiên ảnh giao diện có nền màu phẳng. Chưa có inpainting nền ảnh/gradient/hoa văn. Khi dải nền không đồng màu hoặc không đủ bằng chứng thành phần chính, mặt nạ có cờ **cần kiểm tra** và bảng thuộc tính báo rõ; bù màu có thể chưa khớp. Dùng cửa sổ xem trước, chọn màu/vùng giữ hoặc Undo. Không cam kết tự phục hồi nền phức tạp hay bảo vệ100% chi tiết chưa được nhận diện trong một vùng chọn quá rộng.

Mỗi thao tác tạo mặt nạ tối đa20MP; tổng diện tích khung mặt nạ của một tài liệu tối đa20MP, tối đa500mặt nạ. Nhiều khung chồng nhau vẫn tính diện tích từng khung. Đây là giới hạn bộ nhớ; chọn vùng nhỏ hơn hoặc bỏ mặt nạ không dùng. Mask/bitmap sampling có cache theo thread, xóa khi đổi/đóng ảnh; giữ kiểu bitmap freeze/detach của1.37.3.

## Chụp ngay khi thả chuột

Thiết lập → Chụp & quay → **Nhấn Enter để xác nhận chụp**:

- **Tắt (mặc định):** kéo vùng rồi thả chuột là chụp; click khối gợi ý hợp lệ cũng chụp. Click/rung tạo vùng quá nhỏ không chụp.
- **Bật:** giữ cơ chế chỉnh vùng/cạnh/phím, Enter hoặc nút Chụp để xác nhận.
- Tùy chọn tự lưu. Áp dụng trình chọn vùng/phần tử cho ảnh; không đổi xác nhận quay/chụp cuộn. Toàn màn hình/chụp lại không qua trình kéo vùng giữ luồng cũ. Esc vẫn hủy, PrtSc và snapshot/frozen bounds giữ nguyên.

## Phím tắt và menu

Thiết lập → Phím tắt & chạy nền:

- Bỏ hàng và ngừng đăng ký **Theo cấu hình**, **Chọn cửa sổ**; phím phần tử cũ đã ẩn cũng được tắt. Giữ các mã cũ trong file để đọc được cấu hình lịch sử; không reset các phím Chụp mặc định/Toàn màn hình/Chụp lại còn dùng.
- Xóa hai đoạn mô tả dài về nhập phím/Windows PrtSc và Quay/Dừng/Tạm dừng theo yêu cầu.
- Mục **Phím công cụ** liệt kê công cụ chính, chọn vùng pixel, Nhận chữ và mọi công cụ trong Thêm, gồm Mockup/Nền & mặt nạ. Bấm ô rồi nhấn phím đơn hoặc tổ hợp; bỏ chọn để tắt, bấm Lưu phím tắt để lưu cùng cấu hình chụp.
- Từ chối trùng công cụ/công cụ, trùng phím chụp/quay và phím lõi hệ thống/soạn thảo. Ctrl+S/Z/C/V… và điều hướng được giữ; phím chỉ modifier không hợp lệ. Một số tổ hợp do Windows giữ sẽ không dùng được trong app. PrtSc của công cụ xử lý ở key-up để phù hợp cách Windows phát phím.
- Không chạy phím công cụ khi đang gõ TextBox/IME, chọn ComboBox, mở Thiết lập hoặc đang kéo đối tượng. Giữ phím không lặp mở công cụ. Phím công cụ chỉ hoạt động trong Editor, không đăng ký toàn cục để chặn phím của ứng dụng khác.
- Công cụ Thêm qua cùng RunStudioCommand như click: cập nhật ô động, lưu công cụ gần nhất, mở đúng chức năng. Công cụ mới tiếp tục khai báo trong catalog Thêm.
- Bỏ menu **Vùng pixel** ở thanh menu trên; giữ công cụ/phím pixel và các thao tác đang có.

## Bảng thuộc tính bên phải

Tăng rộng mặc định vừa phải, thu gọn input và khoảng cách, căn các ô hai cột. Nhãn Rộng chữ/Giãn dòng/Tự giãn theo chữ… ngắn hơn; quy tắc0/tự động nằm trong tooltip. Nút màu có swatch + nhãn ngắn một dòng, mãHEX/trong suốt ở tooltip; nút màu cao tương đương combobox. Ẩn nhóm biến thể rỗng để tránh khoảng trống ở công cụ chữ. Giữ giá trị và chức năng chỉnh sửa, theme hệ thống/sáng/tối.

## Dữ liệu / triển khai

Schema20 lưu Recoveries/RecoveryId và nhật ký. Sidecar cũ lần đầu nâng có `.pre-v1.40.bak`; cài1.39 trở xuống không đọc được tài liệu20. Backup trước rollback. Cấu hình Enter/phím công cụ ở capture-workflow.json, có backup.pre-v1.40.bak; không thay KEY, trial hay database.

**Không cần chạy migration mới cho V1.40.0. Không cần deploy lại backend/Edge Function.** Migration ngày kích hoạt từ1.39, nếu chưa áp dụng, vẫn được hướng dẫn riêng trong tài liệu lịch sử; không gửi như migration mới.

Bộ cài đầy đủ self-contained.NET10.0.13 + VC++ như1.39, cùng OCR vie/eng. Publish đủ5releaseassets1.40.0 và đặt Latest để updater nhận; Captovix-Setup.exe online hiện có tiếp tục dùng được. Chưa tự publishGitHub.

Kiểm tra và giới hạn nghiệm thu: docs/VALIDATION.md. Tiếp theo roadmap hiện tại: V1.41.0 — Library & Export Experience, chưa xây trong bản này.
