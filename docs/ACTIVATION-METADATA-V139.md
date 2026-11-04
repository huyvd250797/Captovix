# V1.39.0 — Trạng thái bản quyền và ngày kích hoạt

## Giao diện

- Máy chưa kích hoạt: giữ form KEY và lời mời dùng thử đã có.
- Máy đã dùng thử: thẻ **Dùng thử30ngày**, ngày giờ kích hoạt, ngày giờ hết hạn và số ngày còn lại; nút **Nhập KEY kích hoạt vĩnh viễn** mở form KEY. Không hiển thị hành động bắt đầu30ngày lần nữa, kể cả khi đã hết hạn. Thời gian bắt đầu lấy từ hạn trial gốc trừ30ngày, không lấy thời điểm xác minh.
- Máy đã kích hoạt vĩnh viễn và quyền hợp lệ: **✓ Đã kích hoạt**, loại bản quyền, ngày giờ kích hoạt; ẩn ô KEY và lời mời trial mặc định. Nếu lease chưa xác minh thì báo cần xác minh, không hiển thị check xanh cho quyền không hợp lệ.
- Trạng thái chi tiết và Xác minh lại/Trả bản quyền/Khôi phục vẫn ở Xem thêm. KEY chỉ mở lại khi chủ động nâng trial hoặc chọn khôi phục yêu cầu KEY, không xuất hiện khi chỉ xem trạng thái đã kích hoạt.
- Ngày giờ hiển thị theo giờ địa phương Windows. Ngày còn lại làm tròn lên; hết hạn là0, không reset.

## Triển khai một lần — dành cho chủ dịch vụ

1. Sao lưu database theo quy trình hiện có. Trong đúng Supabase project đang phục vụ license, mở **SQL Editor**.
2. Chạy **supabase/migrations/202611040001_activation_timestamp.sql**, sau khi các migration nền `202610280001_foundation.sql` và `202610280003_serverless.sql` đã áp dụng. Không chạy lại nền/reset dữ liệu. File TXT rời bàn giao chứa đúng SQL này để copy.
3. Migration chỉ CREATE OR REPLACE hàm `public.cvx_license_operation` hiện có, thêm trường trả về `activatedAt = d.created_at` từ activation đang dùng. Không thêm bảng/cột, không đổi seats/trial/recovery/rate/replay/RLS hay quyền truy cập. Hàm giữ search_path rỗng, security definer và grants hiện có. Có transaction; có thể chạy lại chính migration này.
4. **Không cần deploy lại Edge Function**: hàm ký hiện tại đã ký toàn bộ claims JSON bằng `{...claims, issuer}` nên trường activatedAt đi trong payload đã ký. Giữ nguyên signing key, cấu hình API và public key của app.
5. Cài app1.39.0; bấm trạng thái bản quyền → Xem thêm → **Xác minh lại**, hoặc mở lại app để refresh. Ngày giờ phải khớp `captovix_license.devices.created_at` của `activationId` tương ứng. Refresh tiếp không được đổi ngày.
6. Sau đó publish đủ5releaseassets1.39.0 và đặt Latest để các máy cũ cập nhật. Không chỉ upload sourceZIP/updateZIP.

Trial không cần migration để tính ngày vì chứng nhận cũ đã có trialUntil. App1.39 vẫn dùng được dịch vụ cũ; với chứng nhận vĩnh viễn chưa có activatedAt, popup báo rõ chưa có dữ liệu, không suy đoán từ IssuedAt. Muốn ngày vĩnh viễn đầy đủ thì chạy migration rồi refresh.

Chứng nhận cũ vẫn hợp lệ; trường activatedAt là tùy chọn ở client và được kiểm chứng trong chữ ký. Nếu có trường ngày thì phải nằm từ UnixEpoch đến IssuedAt. Không dùng trường này để mở thêm quyền hay gia hạn trial.

## Xác minh / rollback

Đã kiểm pure presentation, chữ ký có/ngày thiếu/ngày sai, TypeScript ký/giữ ngày và phát hiện sửa payload; đối chiếu migration với hàm trước chỉ khác đúng một trường metadata. Chưa thực thi SQL trên Supabase sản xuất hoặc gọi dịch vụ license thật. Không tự deploy database trong lần bàn giao này.

Nếu rollback client, có thể giữ migration metadata trên server: app cũ bỏ qua trường JSON bổ sung nhưng vẫn kiểm chữ ký. Tài liệu schema19 cần app1.39 trở lên hoặc phục hồi sidecar backup; rollback app không tự chuyển tài liệu19 về18.
