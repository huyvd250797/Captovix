# V1.29.1 — Validation và giới hạn

- TypeScript strict check thành công. Edge production source một file, không import dependency runtime; Deno bootstrap đăng ký handler khi module được import.
- 27 Node/WebCrypto/JSDOM checks: RSA proof/salt/signature/timestamp, CORS/quota/errors/body limit, admin Auth/allowlist contract, keygen tạo/nạp lại authority, chặn secret ở public config, session chỉ RAM, text render an toàn, phản hồi muộn sau logout không khôi phục chi tiết key.
- 50 integration checks chạy production handler + PostgreSQL16.15 thật: migration additive/rerun giữ record V1.29.0, revoke quyền anon/authenticated, giao dịch trial/slot/recovery/rate, audit theo UID, tìm kiếm, revoke/restore, kích hoạt đồng thời và chứng nhận interop.
- Interop dùng lớp Protocol.cs sản xuất: .NET ký device proof có Unicode → TypeScript xác minh; TypeScript/SQL ký certificate → .NET xác minh và từ chối payload bị sửa. Khóa test sinh riêng, không cài vào app hoặc đóng gói key sản xuất.
- Auth getUser transport trong test là fixture có kiểm soát; không coi đây là kiểm thử Supabase Auth/Edge hosted thật. RPC fixture dùng kết nối DB trực tiếp để kiểm tra đúng hàm SQL; chưa kiểm chứng PostgREST/Supabase quota trên project của Boss.
- Public-config CLI có kiểm tra URL/key/schema và từ chối private/extra/empty config. Công cụ bootstrap build được. Công cụ trình duyệt kiểm tra bằng DOM/WebCrypto, chưa có kiểm thử hiển thị Chromium/browser native; không khẳng định đã nghiệm thu layout thực tế.
- Lượt hồi quy54 project C#: logic/contract chạy trên Linux, WPF compile-only. App được publish self-contained Windows x64 với runtime10.0.13. Các API capture/render ảnh/installer nền được giữ; package kiểm tra icon, CRC/hash, source tree, full.nupkg và updater SDK trước bàn giao.
- Workflow GitHub Actions, cấu hình Pages/headers và Supabase Dashboard deployment đã bàn giao source/hướng dẫn; chưa chạy/deploy trong tài khoản Boss. Windows Setup/DPAPI/clipboard/multi-monitor/expiry đang thao tác chưa được thực thi trên Windows thật.

**Trạng thái gói:** review build còn URL/public key trống, chỉ chụp kéo vùng. Production cần project/Secrets/Auth allowlist/Pages origin thật, build public config và nghiệm thu Windows. Không có master key, bypass hoặc tự gia hạn khi backend lỗi.

**Bàn giao:** Update-Packages + Source ZIP, không tự push/deploy/Release. Không có private signing key, service-role key hoặc mật khẩu sản xuất trong source/payload. Xem SERVERLESS-LICENSING.md để cấu hình và vận hành.
