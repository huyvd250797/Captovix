# Callout V1.37.0 — click ngoài và tâm di chuyển thân

1. Chọn Callout. Bấm đơn vùng trống không tạo hình. Nhấn giữ rồi kéo vượt4px hiển thị mới tạo Callout; tiếp tục kéo để chỉnh rộng/cao tự do như mô phỏng đã duyệt.
2. Thả để nhập chữ. Click ra ngoài kết thúc và đưa thay đổi vào luồng tự lưu. Lần click canvas này được tiêu thụ, không mở một Callout mới. Công cụ vẫn được giữ; muốn thêm hình phải bắt đầu lượt kéo khác.
3. Click chọn Callout đã có để hiện tâm xanh có dấu cộng. Khi đang nhập, click ra ngoài để hoàn tất rồi chọn lại Callout.
4. Nắm đúng tâm rồi kéo: thân và chữ di chuyển, giữ kích thước/góc xoay và điểm tip đuôi trên ảnh. Không ép căn lưới cho lượt kéo tâm; Shift giữ hướng chính nếu cần.
5. Nắm vùng thân ngoài tâm/node: di chuyển cả Callout, bao gồm đuôi. Node cam chỉ chỉnh đuôi; node viền resize; node xanh lá xoay như cũ. Double-click vẫn sửa chữ. Callout khóa hoặc chọn nhiều không hiện tâm kéo riêng.
6. Undo/Redo khôi phục cả hình và vị trí. Tâm/node chỉ là overlay chỉnh sửa, không có trong ảnh xuất/copy.

## Nghiệm thu Windows còn chờ

- Vẽ→gõViệt/IME→clicktrống10lần: giữ đúng1Callout; nội dung được lưu/mở lại, không thêm popup hay Undo rỗng.
- Clicktrống, giữ không kéo, rung nhẹ dưới ngưỡng và double-clicktrống ở các mứczoom25/100/200%: không tạo mới. Kéo đủ ngưỡng: tạo đúng1 và giữ cách kéo2trục.
- Chọn Callout ở các góc0/35/90°; kéo tâm, đo tip đứng yên; kéo ngoài tâm, cả tip/thân cùng di chuyển. Kiểm10kiểu Callout, font/đệm, callout cũ, Undo/Redo và lưu/mở project.
- Đổi công cụ/menu khi đang nhập vẫn thực hiện lệnh; click canvas đầu tiên chỉ hoàn tất nhập. Không click xuyên xuống canvas khi textbox ẩn.
- Node không lọt vào PNG/clipboard; test100/150/200%DPI và nhiều màn hình. Setup cùngversion và bảnquyền/dữliệu giữ nguyên.

Các fixture Windows trong source chỉ biên dịch tại Linux; không coi build là đã nghiệm thu thao tác này trên Windows thật.
