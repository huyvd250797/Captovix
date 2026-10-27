# Captovix V1.22.0 — Object Editing Precision

## Chọn đúng đối tượng

- Di chuột lên đối tượng có thể chỉnh để xem viền gợi ý và tooltip. Viền, tay nắm, đường gióng chỉ có trong Editor; không xuất vào ảnh.
- Chọn công cụ Chọn (V), Alt+click để chuyển qua các lớp chồng nhau. Với công cụ vẽ khác, Alt+kéo vẫn có nghĩa vẽ chồng như các bản trước.
- Chuột phải ngay vùng có nhiều đối tượng → **Chọn đối tượng tại đây** → chọn theo tên/loại và nhóm. Danh sách đi từ lớp trên xuống, tôn trọng lớp bảo vệ của công cụ che/mờ. Đối tượng ẩn không được đưa vào; đối tượng khóa có thể được chọn ở menu để mở khóa, nhưng không kéo/sửa khi còn khóa.
- Menu cho tối đa50đối tượng/nhóm tại vị trí; nếu nhiều hơn, có lệnh mở Layers để xem toàn bộ. Layers vẫn hỗ trợ chọn cả các đối tượng ở ngoài vùng crop.

## Căn chỉnh và chia đều

1. Shift/Ctrl+click chọn các đối tượng, hoặc quét chọn bằng công cụ Chọn.
2. Chuột phải → **Căn chỉnh → Tham chiếu căn**:
   - **Lựa chọn**: căn theo khung bao của những đối tượng/nhóm được chọn. Khi chỉ có một đơn vị, dùng toàn canvas.
   - **Vùng ảnh đang thấy**: phần ảnh nền thực sự hiện trong canvas sau xoay/lật/crop, không tính viền/bóng.
   - **Toàn canvas**: toàn kích thước đầu ra hiện tại, kể cả phần mở rộng.
3. Chọn Trái / Giữa ngang / Phải / Trên / Giữa dọc / Dưới.
4. Từ3đối tượng/nhóm trở lên, chọn **Chia đều ngang/dọc** để chia đều khoảng trống giữa chúng, kể cả kích thước khác nhau. Hai đầu giữ nguyên. Nếu không đủ khoảng trống, thông báo và không di chuyển đối tượng.

Tham chiếu căn được ghi nhớ trên máy. Mỗi nhóm được tính là một đơn vị khi căn/chia đều. Công cụ bảo vệ vẫn ở lớp bảo vệ; lệnh sắp lớp không đưa chú thích lên trên vùng che.

## Di chuyển, bắt dính và đường gióng

- Kéo đối tượng để thấy đường gióng mép/tâm theo canvas, vùng ảnh và đối tượng đang hiện khác. Nhóm đang chọn được loại khỏi các đích bắt dính của chính nó.
- Ngưỡng bắt dính6DIP theo giao diện, không là một số pixel nguồn cố định. Vì vậy độ gần theo con trỏ nhất quán ở các mức zoom; hướng căn theo ảnh đã xoay/lật cũng đúng.
- Giữ **Ctrl khi kéo** để tạm bỏ bắt dính; checkbox Bắt dính mép/tâm tắt/bật lâu dài trên máy. Giữ **Shift khi kéo** để khóa hướng ngang/dọc theo màn hình.
- Phím mũi tên dịch1pixel đầu ra; Shift+mũi tên dịch10pixel đầu ra. Mức zoom không đổi bước dịch. Giữ nguyên thao tác Space pan, Ctrl+con lăn zoom, con lăn dọc, Shift+con lăn ngang.

## Nhập vị trí và kích thước

Mở **Vị trí · kích thước · xoay** ở thuộc tính đối tượng:

- X/Y là góc trên trái **khung bao hình học** của lựa chọn, theo pixel canvas đầu ra hiện tại, bao gồm phần mở rộng. Rộng/cao là kích thước khung bao; không tính bóng và phần nét viền nhô ra.
- Nhập số áp dụng tự động theo cơ chế thuộc tính hiện có; Enter để hoàn tất ngay, Esc trả trường về giá trị đang lưu. Đầu vào sai được báo lỗi; không ghi một phần hình học lên tài liệu.
- Góc được ghi rõ là **góc nguồn**, không phải góc màn hình sau biến đổi toàn ảnh. Với một đối tượng thường là góc tuyệt đối. Với nhóm hoặc đường nối, là lượng xoay thêm quanh tâm; sau áp dụng về0. X/Y góc trên trái được giữ; xoay làm thay đổi rộng/cao khung bao và UI cập nhật lại.
- Đường thẳng ngang/dọc có một chiều bằng0: có thể di chuyển và đổi chiều còn lại bằng số; muốn tạo độ nghiêng ở chiều đang bằng0 thì kéo đầu mút hoặc dùng góc xoay. Không tự tạo một hình chữ nhật thay cho đường.
- Nhập số không thay kích thước bitmap gốc hay canvas, không thay zoom, không raster hóa đối tượng. Vị trí nằm ngoài canvas có thể bị cắt khi xuất; dùng Layers để chọn lại và sửa tọa độ.

## Khóa tỷ lệ và nhóm

- Checkbox **Giữ tỷ lệ khi nhập / kéo** áp dụng cho cả trường số và tay nắm; ghi nhớ trên máy. Giữ Shift khi resize cũng khóa tỷ lệ tạm thời.
- Các tay nắm cạnh và góc đều giữ tỷ lệ, giữ cạnh/góc đối diện; tính đến độ lệch giữa điểm click và tâm tay nắm để tránh nhảy khi vừa kéo.
- Khung và tay nắm được vẽ theo kích thước giao diện, không bị kéo méo theo tỷ lệ resize ảnh. Đối tượng nhỏ giảm tay nắm giữa cạnh; chọn tay nắm gần con trỏ nhất.
- Chữ/số bước và các lựa chọn có đối tượng xoay buộc giữ tỷ lệ. Nhóm có chữ hoặc callout không bị co qua giới hạn chữ8–240 hoặc thân callout36đơn vị nguồn mỗi chiều; resize dừng ở giới hạn tương ứng.
- Ctrl+G nhóm; Ctrl+Shift+G bỏ nhóm. Kéo, resize, xoay, nudge hoặc khóa từ một thành viên áp dụng cho cả nhóm. Nhóm hiện là nhóm phẳng, không lồng nhóm.
- Nhân bản/copy/paste đối tượng ngoài ảnh giữ miền đặt vị trí theo canvas mở rộng, tránh tự đưa trở vào ảnh gốc.

## Ngoài ảnh, crop và dữ liệu cũ

- Kế thừa Canvas/Crop V1.21. Các phép căn, di chuyển và resize không tự tăng canvas. Để thêm chỗ, dùng Canvas… hoặc vẽ tạo mới với tùy chọn mở rộng.
- Khi chủ động đặt một đối tượng vượt vùng ảnh hoặc vào phần khung ngoài, tài liệu bật hiển thị chú thích ngoài ảnh trong cùng lượt Undo; nguồn ảnh và các hiệu ứng cần pixel nguồn vẫn clip đúng trong vùng ảnh. Chú thích có thể hiện trên phần nền/viền đầu ra; nguồn bitmap không tràn vào viền.
- Dữ liệu cũ không bị thay chỉ vì mở/xem hoặc hover. Tọa độ đối tượng vẫn lưu theo ảnh gốc, gồm số âm; quy đổi đầu ra chỉ dùng ở thao tác/UI.
- Không thêm schema dự án: vẫn đọc12/13/14, dùng14 khi có precision của1.21. Preference mới nằm trong settings máy. Không reset thư viện, nguồn cập nhật, logo, hotkey hay media.
- Resize không tự FitText đẩy hộp/chữ trở lại ảnh. Text layout/auto-fit sâu hơn thuộc V1.23; nếu chữ dài vượt hộp đã ép nhỏ, cần tăng khung hoặc sửa chữ.

## Kiểm chứng và giới hạn xác minh

Các kiểm tra logic mới bao gồm: bốn hướng xoay + lật + resize không đều, căn theo canvas/ảnh/lựa chọn, chia khoảng trống, nhóm/khóa, trường số và Undo/Redo, giới hạn tỷ lệ/font/callout, zero-axis line, đối tượng ngoài ảnh, ảnh có viền, và snap/bypass.

Bộ WPF mới kiểm hit stack/ẩn/khóa, render so với trường số, nudge/Undo, tay nắm/giữ tỷ lệ, nhóm, chú thích trên khung và nguồn clip trong viền. Trên môi trường xây dựng Linux: chỉ cross-build bộ WPF. **Chưa chạy nghiệm thu Windows desktop.**

Checklist Windows trước triển khai rộng:

1. Cài mới và update từ1.21: version, logo, media, hotkey, đường dẫn cài, light/dark/system.
2. Chọn chồng nhiều kiểu, outline hover, Alt+click khi Chọn, menu tại con trỏ, khóa/ẩn/nhóm và Layers.
3. Căn theo3tham chiếu; chia đều cả ngang/dọc; ảnh xoay/lật/resize/crop, đối tượng ngoài ảnh, bảo vệ vùng che.
4. Drag/snap/Ctrl-bypass/Shift-axis; zoom25/50/100/200/400%, DPI100/125/150/175/200%, màn hình phụ và move Editor giữa màn hình.
5. Các tay nắm cạnh/góc, click lệch tâm tay nắm, đối tượng rất nhỏ; giữ/tắt tỷ lệ, nhóm có chữ/callout/đối tượng xoay, các giới hạn kích thước.
6. Nhập số hợp lệ/sai, góc nhóm không bị áp lặp, Ctrl+Z/Y, Esc/hủy gesture, save/reopen/project.
7. Đối chiếu pixel và kích thước PNG/clipboard/PDF; zoom/hover/chọn không làm thay file xuất; bitmap không tràn vào khung ngoài.

Bản kế tiếp: **V1.23 — Text & Callout Refinement**: hoàn thiện nhập chữ trực tiếp/IME, căn lề/giãn dòng/khoảng đệm, hộp co theo chữ, đuôi callout tự do và mẫu xem trước nhất quán. Ưu tiên sửa phản hồi Windows của1.22 trước.
