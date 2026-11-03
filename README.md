# Captovix V1.29.1 — Serverless Licensing

Ứng dụng chụp/chỉnh ảnh Windows với Editor thống nhất. Bản này chuyển dịch vụ bản quyền V1.29.0 sang **Supabase Edge Functions + PostgreSQL**, trang quản trị **Cloudflare Pages**, giữ các công cụ ảnh/capture và giao thức RSA-PSS hiện có.

**Source/build bàn giao chưa kết nối Supabase/Cloudflare production.** `src/Captovix/license-config.json` và `admin/config.json` còn trống. Gói Setup review chỉ mở chụp vùng thủ công lưu PNG gốc; trial/key cần hoàn tất cấu hình và build lại. Không đăng review package này lên Release dùng chung.

## Triển khai không cần Docker/VPS

Hướng dẫn từng bước: **[docs/SERVERLESS-LICENSING.md](docs/SERVERLESS-LICENSING.md)**.

1. Tạo/chọn project Supabase; SQL Editor chạy `supabase/SETUP-ALL.sql` (additive, không reset).
2. Tạo user quản trị trong Auth, đưa đúng UID vào `captovix_license.admins`.
3. Mở `tools/LicenseKeygen.html` bằng Edge/Chrome để tạo/nạp khóa ngoại tuyến. Lưu private key/edge-secrets.env ngoài Source. Chỉ public config được copy vào app/admin.
4. Supabase Secrets: khai báo signing key B64, issuer, lease/cooldown, origin trang quản trị.
5. Deploy **một file** `supabase/functions/licensing/index.ts`, tên function `licensing`, tắt gateway Verify JWT (code vẫn xác minh device proof và admin Auth/allowlist). Kiểm tra `/health` và `/ready`.
6. Điền `admin/config.json`, upload **chỉ `admin/`** lên Cloudflare Pages Direct Upload. Đặt `ADMIN_ORIGINS` đúng URL Pages.
7. Dùng GitHub Actions workflow đã bàn giao để nhúng public URL/public key và build hai ZIP trên runner Windows. Chi tiết Variables/Actions trong guide; không cần SDK trên máy Boss. Chưa chạy workflow trong tài khoản Boss.
8. Nghiệm thu Windows/kích hoạt/chuyển máy rồi mới phát hành update assets.

## Công nghệ và bảo mật

- WPF .NET 10 x64, self-contained runtime Windows 10.0.13; Tesseract/ScreenRecorderLib/Velopack giữ bản pin từ source nền.
- Edge Function TypeScript dùng WebCrypto + fetch, không dependency runtime. Giao dịch PostgreSQL/RPC chống vượt slot và replay.
- Quản trị Supabase Auth + allowlist riêng, audit theo UID; session chỉ RAM của tab. Không nhúng service-role/signing private key vào Pages hoặc app.
- Trial 30 × 24h; lease trial tối đa48h, key vĩnh viễn tối đa30d offline mặc định. Vĩnh viễn không có hạn trả phí nhưng cần xác minh định kỳ.
- Khi hết quyền khóa chỉnh/copy/paste/OCR/Studio/export/video/capture nâng cao; chỉ chụp kéo vùng. Giữ media/chú thích và checkpoint công việc đang có.
- Chuyển máy, recovery code một lần/rotation/cooldown72h, quản trị slot/trial/repair/thu hồi/audit. Thu hồi không tức thì trên máy offline còn lease.

## Phát triển và kiểm tra

Node24+ để chạy kiểm tra backend/DOM; .NET10 SDK cho core/WPF; Windows/NSIS3 để dựng installer và nghiệm thu. NPM chỉ phục vụ test, không cần cho Dashboard deployment.

```powershell
npm ci
npm run check
npm test
python scripts/Run-Checks.py
```

`npm run test:integration` chỉ dùng database KIỂM THỬ PostgreSQL đã migration; xem guide. Auth transport trong kiểm tra là fixture, chưa chứng minh Supabase Auth production. Linux chỉ cross-build WPF, không thực thi Windows.

Build local Windows sau copy public config:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Build chặn cấu hình thiếu hoặc private key trong public field. `-AllowUnconfiguredLicensing` chỉ để review hạn chế. `scripts/Package-Source.py` tạo Source ZIP sạch, từ chối file secret có tên chuẩn; không bỏ kiểm tra để đóng gói secrets.

## Bàn giao và bước kế tiếp

Hai file: `Captovix-v1.29.1-Update-Packages.zip` (có canonical Setup.exe) và `Captovix-v1.29.1-Source.zip`. Không push GitHub, deploy hoặc publish Release tự động. Khi tiếp tục, gửi Source ZIP mới nhất, giữ public config đã dùng và mô tả yêu cầu; không gửi private key/service-role/password.

Đề xuất tiếp **V1.30 — Licensing Hardening & Release**: key rotation/key ring, MFA/quyền quản trị chi tiết, giám sát quota/backup và nghiệm thu Windows/staging đầy đủ. Chưa xây phiên bản đó.
