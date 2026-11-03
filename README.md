# Captovix V1.33.0 — Scrolling Capture Quality

Chụp cuộn ổn định hơn, cảnh báo mối nối/lệch cột, nhận vùng cố định, bỏ khung và chỉnh overlap. Toolbar chuyển lên trên để tăng canvas, chụp được chính app, kéo ảnh đã chỉnh ra ngoài và hoàn thiện popup/trạng thái bản quyền.

- Source: `Captovix V1.33.0 — Scrolling Capture Quality.zip`.
- Update/Setup: `Captovix-v1.33.0-Update-Packages.zip`. Thoát app và chạy Setup trong ZIP vào đường dẫn đang cài; không uninstall.
- Hướng dẫn/checklist: `docs/SCROLLING-CAPTURE-QUALITY-V133.md`.
- Không cần chạy migration mới. Public license config, Supabase backend và admin giữ nguyên. Trial server hiện có không tự reset khi gửi yêu cầu lặp.
- Windows x64, .NET10 SDK, NSIS3: `python scripts/Run-Checks.py` rồi `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`.
- Chưa push GitHub/deploy/publish release. Upload đủ các release assets trong ZIP lên stable GitHub Release v1.33.0 nếu muốn cập nhật trong app.
- Dữ liệu ảnh/schema17 giữ nguyên. Bố cục toolbar V1.33 lưu riêng, không ghi đè bố cục cũ. Trial history được ghi DPAPI theo user Windows; chống tạo trial mới sau xóa/cài lại vẫn dựa vào định danh máy và backend.
- Phiên bản tiếp theo giữ roadmap: V1.34.0 — Capture Workflow & Recovery.

## Hướng dẫn nền và lịch sử

# Captovix V1.32.0 — Smart Element Capture Refinement

Bản Windows chụp ảnh/chỉnh sửa: Chụp mặc định gộp phần tử/modal + kéo vùng; popup KEY tối giản; phím tắt PrtSc/phím đơn; Stamp click để đặt; khung chữ kéo/resize; node xoay xanh lá và quy tắc cuộn thống nhất.

- Chi tiết và hướng dẫn nghiệm thu: `docs/SMART-ELEMENT-CAPTURE-REFINEMENT.md`.
- Source: `Captovix V1.32.0 — Smart Element Capture Refinement.zip`.
- Bộ cài/cập nhật: `Captovix-v1.32.0-Update-Packages.zip`, chứa Setup và các release assets. Không uninstall trước khi nâng cấp.
- Không cần chạy migration mới. Không cần cấu hình/deploy lại Supabase hoặc admin Cloudflare. Public config đã có được giữ nguyên; không đóng gói khóa riêng/server secrets.
- Build Windows x64: .NET10 SDK, NSIS3; `python scripts/Run-Checks.py` rồi `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`. Script kiểm tra logic/native tương ứng môi trường và tạo hai ZIP.
- Nghiệm thu cài đặt/chuột/phím/DPI/clipboard/IME cần Windows thật. Linux chỉ xác minh logic, cross-build và toàn vẹn package; không tự kết luận native QA đạt.
- Không push GitHub/deploy/publish release trong lần bàn giao này. Hướng dẫn build/publish chi tiết bên dưới vẫn áp dụng; tên phiên bản hiện tại là V1.32.0.
- Phiên bản tiếp theo: V1.33.0 — Scrolling Capture Quality: xử lý mất/lặp dòng, sọc ghép, header/footer cố định và cảnh báo mối nối chưa chắc chắn. Chưa triển khai.

## Hướng dẫn nền và lịch sử phiên bản

# Captovix V1.31.0 — Quick Capture Experience

Nâng từ source V1.30 có payload DLL khớp gói update Boss đính kèm. Giữ nguyên public license config, Supabase/Pages và dữ liệu media. **Không cần chạy migration mới hoặc redeploy backend/admin.**

- Chụp vùng: thanh gọn, ưu tiên ngoài vùng; kéo/chỉnh cạnh rồi Enter một lần chụp. Viền tương phản mảnh theo DPI, resize cursor theo cạnh/góc.
- Đường X/Y theo con trỏ; H bật/tắt. Kính lúp112 ×138 DIP, viền mảnh; M bật/tắt; nhớ lựa chọn. Space + phím mũi tên hỗ trợ chọn vùng bằng bàn phím.
- Nhớ chế độ chụp được chọn; giữ pipeline snapshot/pixel V1.30 để toolbar/kính/tọa độ không vào PNG.
- Crop kéo rồi thả áp dụng ngay, Ctrl+Z hoàn tác; nhập số dùng Áp dụng kích thước.
- Bước chọn màu viền/nền riêng, hỗ trợ Trong suốt và màu số. Sidecar mới dùng schema16 khi có kiểu Bước mới; ảnh cũ giữ hiển thị cũ. Bản cũ không đọc schema16, cần backup trước rollback.
- Stamp mở bằng một nút, có12 hình vector mới gồm check xanh và X đỏ; preview và hình chèn thống nhất.
- Ô công cụ động ngay trái Thêm nhớ công cụ chọn gần nhất, icon riêng; dùng lại không cần mở Thêm.
- Nội dung cập nhật trong Settings/popup dùng chung định dạng dễ đọc, không hiện dấu Markdown thô.

Hướng dẫn thao tác/kiểm thử: [docs/QUICK-CAPTURE-EXPERIENCE.md](docs/QUICK-CAPTURE-EXPERIENCE.md). Đóng app, giải nén Update-Packages và chạy HuyVo.Captovix-win-Setup.exe vào đường dẫn cũ. Để update qua app, đăng năm assets lên stable Release v1.31.0; không chỉ upload ZIP. Không tự publish lần này.

Build .NET10 SDK/Windows x64/NSIS3 như hướng dẫn trước. `python scripts/Run-Checks.py` chạy logic và cross-build WPF trên Linux; WPF/input/DPI/Setup cần nghiệm thu Windows thật. Bàn giao đúng hai ZIP update và source, Setup ở trong ZIP update.

Kế tiếp đề xuất **V1.32 — Capture Presets & Repeat**: mẫu kích thước/tỷ lệ, nhập X/Y/W/H trong selector, chọn vùng gần đây và quản lý chụp lại nhanh; giữ kiểm tra pixel và nghiệm thu đa màn hình. Chưa triển khai.

## Lịch sử và hướng dẫn cấu hình trước đó

# Captovix V1.30.0 — Capture Fidelity & Reliability

Nâng từ source V1.29.1 Boss cung cấp. Giữ nguyên public license config đã gắn; backend Supabase/Pages, authority, dữ liệu bản quyền và media không thay đổi. **Không cần chạy migration mới hoặc redeploy backend/admin.**

## Các thay đổi

- Chụp vùng/phần tử dùng một snapshot desktop giữ trong RAM trước selector, Enter lấy đúng pixel từ snapshot thay vì chụp lại sau khi chọn. Kèm kính lúp15×15pixel, X/Y, phím M ẩn/hiện. Ghi thời điểm giữ ảnh vào metadata.
- PNG gốc đúng pixel vật lý và kích thước vùng chọn; giám sát topology/DPI, hỗ trợ tọa độ âm, không silently downscale. Ngưỡng64MP cho snapshot để giới hạn bộ nhớ.
- Editor phân biệt Vừa khung /100% /Tùy chỉnh; nút chế độ có trạng thái chọn. Zoom không đi vào pipeline file/copy. Save PNG kiểm tra cả kích thước và BGRA đọc lại.
- Giữ các công cụ ảnh, bản quyền/khôi phục/serverless và updater từ V1.29.1; không đụng private key hoặc tạo key/trial trên production.

Hướng dẫn và ma trận nghiệm thu: [docs/CAPTURE-FIDELITY-RELIABILITY.md](docs/CAPTURE-FIDELITY-RELIABILITY.md). Ảnh desktop là SDR8; HDR/WCG đánh giá riêng. Chụp cuộn, video và native-window capture tiếp tục dùng nguồn live riêng.

## Cài/cập nhật

Đóng Captovix → giải nén Update-Packages → chạy `HuyVo.Captovix-win-Setup.exe` → chọn đường dẫn cũ, ví dụ D:\Captovix\Captovix. Giữ thư mục dữ liệu và license; không gỡ app/xóa dữ liệu. Setup nằm trong ZIP update, chỉ bàn giao hai file Source và Update-Packages. Để cập nhật qua app, upload năm release assets trong ZIP lên stable GitHub Release v1.30.0 như hướng dẫn README-PUBLISH.txt.

## Build và kiểm tra

Dùng .NET10 SDK và Windows x64; runtime Windows10.0.13, dependencies pin từV1.29.1. Dùng NSIS3 để dựng bộ cài. Source có GitHub Actions chạy build Windows bằng public Variables đã hướng dẫn ở bản trước, không tự publish Release. Khi chỉ nâng app, không ghi đè admin/config.json mẫu lên Pages đang chạy.

```powershell
python scripts/Run-Checks.py
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả Source: `Captovix V1.30.0 — Capture Fidelity & Reliability.zip`; update: `Captovix-v1.30.0-Update-Packages.zip`. Không đưa private key/service-role/password vào source hoặc Actions Variables. Hướng dẫn serverless cũ bên dưới chỉ dành cấu hình lần đầu; **lần nâng này không cần thực hiện lại**.

Kiểm tra Linux chỉ chạy được logic/geometry và cross-build WPF; chưa thay cho nghiệm thu Windows100–200%, GDI/clipboard/multi-monitor/Setup. Xem docs/VALIDATION.md. Chưa push GitHub/deploy/publish Release trong lần bàn giao.

## Bản kế tiếp đề xuất

**V1.31 — Capture Workflow & Precision:** nhập trực tiếp X/Y/W/H trong selector, mẫu kích thước/tỷ lệ và chọn nhanh vùng gần đây ngay lúc chụp; tiếp tục nghiệm thu Windows theo bộ ảnh kiểm thử. Chưa xây bản này.

## Hướng dẫn hệ thống trước đó

# Captovix V1.29.1 — Serverless Licensing

Ứng dụng chụp/chỉnh ảnh Windows với Editor thống nhất. Bản này chuyển dịch vụ bản quyền V1.29.0 sang **Supabase Edge Functions + PostgreSQL**, trang quản trị **Cloudflare Pages**, giữ các công cụ ảnh/capture và giao thức RSA-PSS hiện có.

**Bản cấu hình cho hệ thống của Boss:** `src/Captovix/license-config.json` đã được thay bằng cấu hình công khai Boss cung cấp và nhúng vào app lúc build. API `/health` và `/ready` đã trả HTTP 200 khi kiểm tra ngày 03/11/2026. Giữ nguyên issuer/public key, không tạo lại private key và không đổi dữ liệu license trên server.

`admin/config.json` trong source vẫn là mẫu trống; trang admin Boss đã deploy đang có cấu hình riêng. Không upload đè thư mục admin mẫu này lên Pages đang hoạt động. Lần bàn giao này chỉ cấu hình/build Windows; không deploy lại Edge Function/admin, không cần chạy migration mới.

Nâng cấp từ V1.27.2: đóng Captovix/kết thúc quay, chạy Setup mới, chọn đường dẫn cài hiện có (ví dụ D:\Captovix\Captovix), không gỡ app hay xóa thư mục dữ liệu. Mở Bản quyền & khôi phục để dùng thử hoặc nhập key đã tạo trên trang quản trị. Chưa kiểm thử WPF/DPAPI/Setup trên Windows thật; việc API sẵn sàng chưa chứng minh toàn bộ luồng kích hoạt hoặc private key server khớp public key đã nhận.

Đọc [docs/CONFIGURED-WINDOWS-BUILD.md](docs/CONFIGURED-WINDOWS-BUILD.md) để cài/cập nhật. Giữ version V1.29.1 cho bản cấu hình đầu tiên này; không phát hành song song review chưa cấu hình cùng version. Nếu review V1.29.1 đã được đăng cho người dùng, cần tăng version trước khi phát hành gói khác.

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
