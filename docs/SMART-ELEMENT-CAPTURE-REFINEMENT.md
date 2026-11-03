# Captovix V1.32.0 — Smart Element Capture Refinement

- Chụp mặc định gộp gợi ý phần tử/modal và kéo vùng. Click khóa phần tử → chỉnh cạnh → Enter chụp; kéo trực tiếp tạo vùng thủ công. ↑/↓ chuyển cha/con; Tab chọn lại; S bật/tắt gợi ý.
- Nhận diện có bộ nhớ hierarchy ngắn hạn, hủy truy vấn khi chuột rời vị trí, bỏ phản hồi cũ và giữ ngưỡng ổn định. Provider không phản hồi vẫn kéo chọn được.
- Đường X/Y vàng cam có viền đen mảnh; H bật/tắt, M bật/tắt kính lúp. Overlay và toolbar không đi vào ảnh nền đã giữ.
- Popup KEY gọn theo theme: Nhập KEY → Kích hoạt. Tôi không có KEY mở lời mời dùng thử 30 ngày; chỉ gọi trial khi bấm xác nhận. Xác minh/trả máy/khôi phục đặt trong Xem thêm; bỏ mã máy và dấu khóa khỏi UI.
- Thiết lập chụp/quay chia nhóm nguồn, sau chụp, preset và video; giữ cấu hình cũ và phím cũ.
- Phím tắt cho phép phím đơn và tổ hợp Ctrl/Alt/Shift/Win; nhận PrtSc cả key-up. Windows kiểm tra xung đột và phím dành riêng; không chiếm phím bằng hook ngầm.
- Thư viện 39 stamp vector tự thiết kế, chia nhóm. Hình xem trước căn theo giới hạn nét vẽ, không bị cắt. Chọn mẫu rồi click lên ảnh để đặt; click tiếp thêm stamp; chọn lại đối tượng/tay nắm để sửa, Esc về Chọn.
- Chữ: kéo tạo khung với Text xem trước, nhập trực tiếp sau thả; resize theo cạnh/góc, reflow giữ cỡ chữ. Khung cố định giới hạn phần chữ hiển thị; tăng chiều cao nếu chữ tràn. Chữ cũ giữ nguyên hành vi.
- Mũi tên bỏ khung chữ nhật khi chọn/rê; mũi tên thẳng có hai đầu và node xoay xanh lá. Mũi tên cong vẫn giữ đường cong; node điều khiển cong của công cụ Đường được giữ riêng.
- Node xoay của các đối tượng/nhóm đổi xanh lá.
- Cuộn thường: dọc; Shift+cuộn: ngang; Ctrl+cuộn: zoom. Trong Editor zoom ảnh; trong panel/popup zoom nội dung UI 65–200%, không đổi dữ liệu xuất. Selector không dùng con lăn đổi phần tử; dùng ↑/↓.

Không cần chạy migration mới. Không cần deploy lại backend/admin bản quyền. Giữ nguyên public license config và cặp khóa server; không gọi tạo key/trial trong kiểm thử tự động.

## Nâng cấp từ V1.31.0

Đóng Captovix (cả khay hệ thống), giải nén Update-Packages và chạy `HuyVo.Captovix-win-Setup.exe` vào đường dẫn hiện có. Không gỡ cài đặt/xóa ảnh hoặc cấu hình bản quyền. Đăng đủ các release assets lên GitHub stable Release v1.32.0 nếu muốn cập nhật qua app.

Preset nguồn mode3 và hotkey phần tử cũ được chuyển về luồng Chụp mặc định; định danh cũ vẫn được đọc để không mất cấu hình. Video ở mode0 dùng kéo vùng và không bật nhận diện phần tử.

Ảnh mới có khung chữ dùng schema17 để bản cũ không đọc nhầm. Tài liệu cũ không tự chuyển thành khung. Sao lưu thư mục dữ liệu trước khi thử quay lại app cũ; V1.31 không chỉnh được tài liệu schema17. Không có thay đổi database.

## Phím PrtSc và giới hạn Windows

Bấm ô phím và nhấn PrtSc rồi Lưu phím tắt. Nếu Windows mở Snipping Tool, vào Cài đặt → Trợ năng → Bàn phím và tắt dùng Print Screen để chụp màn hình. Nếu có công cụ khác giữ phím, tắt/đổi phím ở công cụ đó. Fn là phím phần cứng: chỉ nhận được phím mà firmware gửi cho Windows. Ctrl+Alt+Del và các phím dành riêng không thể bảo đảm đăng ký được; app báo lỗi và phục hồi cấu hình trước đó.

## Ma trận nghiệm thu trên Windows thật (chưa thực hiện tại môi trường Linux)

- Windows10/11, scale100/125/150/175/200%, màn hình phụ tọa độ âm: mở selector thấy nền đứng yên, chụp đúng pixel.
- Chrome/Edge web modal/table và cửa sổ Win32/WPF: rê gợi ý, ↑/↓ đổi cha/con, click khóa, kéo biên, Enter chỉ chụp một lần. UIA không hỗ trợ/chậm: kéo vùng vẫn dùng được.
- Chuyển nhanh chuột qua nhiều vùng và nhấn Tab/S: kết quả cũ không đè lên vùng khóa. Hủy khi provider đang truy vấn không treo editor.
- PrtSc độc lập, A, số, phím điều hướng, numpad, tổ hợp Ctrl/Alt/Shift/Win; kiểm tra báo trùng, đăng ký lỗi phục hồi, restart nhớ phím. Bấm Tab/Esc trong ô phím là ghi nhận phím đó; dùng chuột để chuyển ô.
- KEY popup light/dark/system: ban đầu chỉ input/activate, link không có key và Xem thêm. Mở trial invitation không tạo trial; Để sau không gọi server; xác nhận mới gọi trial. Xác minh/trả máy/recover vẫn hoạt động.
- Kiểm tra mọi stamp: thumbnail full, chọn mẫu chưa thêm ảnh, click canvas thêm tại vị trí chuột, click tiếp thêm, chọn/resize/Undo/Redo đúng, Esc kết thúc.
- Chữ kéo theo 4 hướng, Text live preview, resize 8 cạnh/góc không đổi font, nhập tiếng Việt/IME, Ctrl+Enter kết thúc, lưu/mở đúng khung, clip đúng phần tràn, export/copy không thay vì zoom. Chữ cũ và Step vẫn giữ hành vi.
- Mũi tên: không khung chữ nhật, 2 đầu và node xoay xanh lá; thao tác tại zoom25/100/200%. Nhóm/hình/callout có node xoay xanh lá; đuôi callout màu cam.
- Mọi vùng scroll Editor, thư viện, thanh thuộc tính, Settings, popup Stamp, popup KEY: thường dọc, Shift ngang, Ctrl zoom phù hợp. Nested scroll không cuộn hai lần; Ctrl+scroll không đổi giá trị ô số/ComboBox.
- Cài đè từ V1.31, giữ ảnh/license, mở lại và kiểm tra updater qua release thử nghiệm riêng. Không tự đăng release production trong lần xây này.
