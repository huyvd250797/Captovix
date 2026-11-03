# Captovix V1.29.1 — Triển khai bản quyền không cần Docker/VPS

## 1. Trạng thái bàn giao

Source V1.29.1 chuyển backend ASP.NET Core của V1.29.0 sang **Supabase Edge Functions + PostgreSQL**, trang quản trị tĩnh chạy trên **Cloudflare Pages**. Không cần cài Docker, không thuê VPS, không mua tên miền riêng để bắt đầu. App chụp/chỉnh ảnh vẫn là WPF .NET trên Windows.

**Chưa kết nối project Supabase, tài khoản Auth hoặc Cloudflare của Boss.** Source để trống cấu hình production. Setup trong Update-Packages bàn giao là bản review, chỉ mở chụp vùng thủ công lưu PNG gốc. Hoàn tất các bước dưới, build bản đã cấu hình và nghiệm thu Windows trước khi phát hành cho khách. Không có demo key/master key hay tự cấp quyền khi dịch vụ lỗi.

Chỉ hai file bàn giao: Update-Packages ZIP (chứa Setup.exe và update assets) và Source ZIP (gồm app/API/admin/migrations/công cụ/hướng dẫn). Không tự publish GitHub Release, deploy Supabase hoặc Cloudflare trong lần bàn giao này.

## 2. Các thành phần

| Thành phần | Chạy ở đâu | Vai trò |
| --- | --- | --- |
| Captovix WPF | Windows của khách | Chụp, chỉnh ảnh, kiểm tra chữ ký chứng nhận, lưu dữ liệu cục bộ |
| `supabase/functions/licensing/index.ts` | Supabase Edge Functions | Kiểm tra RSA proof, xác minh quản trị, gọi RPC và ký chứng nhận |
| `supabase/SETUP-ALL.sql` | Supabase PostgreSQL | Trial/key/slot/recovery/nonce/rate limit và audit trong giao dịch |
| `admin/` | Cloudflare Pages | Đăng nhập quản trị, cấp key, quản lý máy, trial, xem audit |
| Supabase Auth + allowlist riêng | Supabase | Xác thực chủ app; khách dùng Captovix không cần tạo tài khoản |
| `tools/LicenseKeygen.html` | Trình duyệt máy quản trị | Tạo/nạp khóa và cấu hình ngoại tuyến; chặn kết nối mạng |
| GitHub Actions | Runner Windows do GitHub cung cấp | Build gói Windows đã nhúng cấu hình công khai; không cần SDK trên máy Boss |

API không có dependency runtime bên thứ ba: một file TypeScript dùng WebCrypto và HTTPS REST. NPM/TypeScript/Postgres.js/JSDOM chỉ phục vụ kiểm tra phát triển, có `package-lock.json`. Backend không sử dụng database password trong app hoặc trang quản trị.

## 3. Quy tắc bản quyền được giữ

- Dùng thử đúng **30 × 24 giờ**, bắt đầu khi máy chủ xác nhận. Cài lại không tự đặt lại hạn.
- Trial được chứng nhận ngoại tuyến tối đa 48 giờ, không vượt thời điểm hết trial.
- Key vĩnh viễn không có hạn trả phí; mặc định một máy đang kích hoạt. Có thể cấp 1–100 slot.
- Chứng nhận vĩnh viễn mặc định cần làm mới trong 30 ngày; app thử refresh lúc mở và mỗi giờ. Có thể cấu hình lease 1–30 ngày trên server.
- Hết quyền/chưa kích hoạt: chỉ kéo vùng, chỉnh biên và Enter để tự lưu PNG gốc. Khóa chỉnh ảnh, copy/paste ảnh/đối tượng, OCR/Studio, xuất, nhập, video và các chế độ capture khác. Không ghi đè thiết lập viền/auto-copy đã lưu chỉ vì đang ở chế độ hạn chế.
- Hộp thoại bản quyền, cập nhật, giới thiệu, thoát còn dùng được. Nhập/dán key, sao chép mã hỗ trợ/khôi phục trong hộp thoại bản quyền là ngoại lệ vận hành.
- Ảnh/chú thích không bị xóa. App cố checkpoint công việc đang có trước khi hạn chế; lỗi lưu vẫn cần xử lý trước khi đóng app để tránh mất nội dung chỉ còn trong RAM.
- Trả slot trên máy cũ để chuyển máy. Mất máy: key + recovery code trả mọi slot cũ, kích hoạt máy mới, đổi mã khôi phục và áp dụng cooldown mặc định 72 giờ.

Thu hồi/chuyển máy không vô hiệu tức thì một bản sao chứng nhận đang offline còn hạn: máy đó bị hạn chế khi refresh hoặc lease hết. DPAPI, mốc giờ và chữ ký không phải cam kết chống được binary bị sửa, snapshot VM hay MachineGuid bị giả. V1.29.1 dùng DPAPI CurrentUser: đổi tài khoản Windows/cài lại hệ điều hành có thể cần quy trình hỗ trợ định danh; không mặc định dùng chung kích hoạt cho mọi tài khoản Windows.

## 4. Chuẩn bị Supabase

1. Đăng nhập [Supabase Dashboard](https://supabase.com/dashboard), tạo/chọn project dành cho bản quyền.
2. Ghi **Project URL**, dạng `https://PROJECT_REF.supabase.co`.
3. Trong phần **Settings → API Keys**, lấy **publishable key** (`sb_publishable_...`) hoặc legacy **anon key**. Đây là khóa công khai cho trang quản trị. **Không dùng `service_role` hoặc `sb_secret_...` trong trang quản trị/app Windows.**
4. Nếu tái sử dụng project đang phục vụ app khác, giữ nguyên Auth, bảng và chính sách của app đó. Các bảng license nằm trong schema riêng. Không tắt đăng ký Auth toàn project hoặc reset database chỉ để cài Captovix.

Hạn mức miễn phí không phải cam kết hoạt động liên tục. Supabase Free có hạn mức database/lượt function và có thể pause project khi không hoạt động; khi dịch vụ bản quyền cần ổn định cho khách trả phí, đánh giá gói phù hợp và backup. Không thêm cron giả hoạt động để né chính sách. Khi project pause, vào Dashboard để resume rồi kiểm tra `/ready`.

## 5. Tạo/nâng cấp database bằng SQL Editor

1. Mở file **`supabase/SETUP-ALL.sql`** trong Source ZIP bằng trình soạn thảo văn bản.
2. Vào Supabase **SQL Editor → New query**, dán toàn bộ nội dung, bấm **Run**.
3. File gồm migration nền tảng `202610280001_foundation.sql` và migration cộng thêm `202610280003_serverless.sql`. Không drop/reset dữ liệu. Không cần `002_api_role.sql` của backend Docker cũ.
4. Trong Database xem schema **`captovix_license`**: có `licenses`, `devices`, `nonces`, `audit`, `admins`, `rate_buckets`.
5. Không thêm schema riêng này vào **Exposed schemas** của Data API. Chỉ các RPC `cvx_license_*` trong `public` được gọi qua server, đã revoke EXECUTE từ PUBLIC/anon/authenticated và chỉ grant `service_role`.

Không chỉnh RPC thành mọi người gọi được để chữa lỗi 403. `SECURITY DEFINER` chỉ dùng với search_path rỗng, tên bảng đầy đủ và quyền gọi giới hạn. Giao dịch/advisory lock nằm trong PostgreSQL nên nhiều instance Edge Function không vượt slot, kể cả khi kích hoạt đồng thời.

## 6. Tạo tài khoản quản trị

1. Supabase **Authentication → Users → Add user / Create user**.
2. Tạo email và mật khẩu riêng cho chủ app. Đảm bảo email được xác nhận bằng tùy chọn của Dashboard/quy trình xác nhận phù hợp.
3. Sao chép **User UID** của tài khoản vừa tạo.
4. SQL Editor chạy, thay giá trị bằng UUID thật:

```sql
insert into captovix_license.admins(user_id)
values ('YOUR_AUTH_USER_UUID'::uuid)
on conflict do nothing;
```

Tài khoản có trong Auth nhưng không có trong bảng riêng này **không được quản trị**. Khách dùng trial/key không cần tài khoản Auth. Để thu hồi quyền quản trị, xóa đúng UID khỏi `captovix_license.admins`; Edge/RPC kiểm tra lại membership, không chỉ nhìn email từ trình duyệt.

Mật khẩu quản trị không đưa vào GitHub, app hoặc file config công khai. Bản này có audit theo UID nhưng chưa có MFA/nhóm quyền chi tiết hay màn hình tự thêm quản trị viên; quản lý allowlist trong SQL Editor.

## 7. Tạo hoặc nạp khóa ngay trong trình duyệt

1. Giải nén Source, mở **`tools/LicenseKeygen.html`** trực tiếp bằng Edge/Chrome. Công cụ không gọi mạng và không lưu localStorage.
2. Nhập Project URL, publishable/anon key, issuer mặc định `captovix-production`. Địa chỉ Cloudflare có thể bổ sung sau.
3. Nếu đã phát hành license trước đây: **nạp `captovix-signing.pem` cũ (PKCS#8), giữ issuer cũ**. Không tạo lại authority ở mỗi bản cập nhật.
4. Nếu hệ thống mới chưa có khóa: để trống phần nạp file và bấm Tạo cấu hình.
5. Lưu các file công cụ tạo:

| File | Đưa vào đâu |
| --- | --- |
| `captovix-signing.pem` | Backup RIÊNG, mã hóa/giới hạn quyền; tuyệt đối không vào Source/Release/admin/GitHub |
| `edge-secrets.env` | Chỉ đưa các giá trị vào Supabase Edge Secrets; giữ riêng |
| `license-config.json` | Cấu hình CÔNG KHAI để nhúng vào app Windows |
| `config.json` | Thay file `admin/config.json` trước upload trang quản trị |
| `captovix-public.pem` | Public key nhiều dòng để đưa vào GitHub Actions Variables |

Tên file tải xuống có thể thêm `(1)` khi trùng: đổi lại đúng tên khi copy. Không upload toàn thư mục Source lên Pages; chỉ upload bộ file trong `admin/`. Không đưa công cụ tạo khóa riêng lên trang quản trị công khai.

Nếu dùng công cụ .NET thay thế, tại thư mục Source chạy:

```powershell
dotnet run --project tools/LicenseBootstrap -- https://PROJECT_REF.supabase.co/functions/v1/licensing D:\Captovix-Secrets
```

Thư mục đích phải trống và nằm ngoài Source. Công cụ không in private key ra console. Muốn giữ khóa cũ, dùng chức năng nạp file của công cụ trình duyệt. Mất private key thì không thể ký tiếp bằng public key đã phát hành; V1.29.1 chưa có key ring/rotation tự động.

## 8. Khai báo Supabase Secrets

Vào **Edge Functions → Secrets**, thêm các giá trị trong file RIÊNG `edge-secrets.env`:

| Tên | Giá trị |
| --- | --- |
| `LICENSE_SIGNING_KEY_B64` | Base64 PKCS#8, một dòng do công cụ tạo |
| `LICENSE_ISSUER` | Phải trùng `issuer` trong app config |
| `PERPETUAL_LEASE_DAYS` | `30` mặc định; 1–30 |
| `RECOVERY_COOLDOWN_HOURS` | `72` mặc định; 1–720 |
| `ADMIN_ORIGINS` | Origin trang quản trị chính xác, ví dụ `https://captovix-admin.pages.dev`; có thể điền sau bước 10 |

`SUPABASE_URL` và `SUPABASE_SERVICE_ROLE_KEY` được môi trường Edge cung cấp; code sử dụng chúng ở server. Không tạo file chứa service role trong `admin/`, không đưa nó vào Variables của build Windows. Không cần database password hoặc admin token dùng chung của V1.29.0.

Không paste private key PEM nhiều dòng thay cho B64 vào `LICENSE_SIGNING_KEY_B64`. Công cụ đã tạo đúng dạng B64 trong file Secrets. Không gửi file này cho người dùng Captovix.

## 9. Deploy Edge Function bằng Dashboard

1. **Edge Functions → Deploy/Create new function → Editor** (tên menu có thể khác nhẹ).
2. Tên function: **`licensing`**.
3. Mở **`supabase/functions/licensing/index.ts`**, copy toàn bộ vào editor rồi Deploy. Đây là một file, không cần npm install/import dependency.
4. Trong phần cấu hình function, **tắt Verify JWT / Verify JWT with legacy secret**. Nếu dùng CLI, `supabase/config.toml` đã có `verify_jwt=false`. Điều này cho desktop dùng RSA proof; admin vẫn được code xác minh qua Supabase Auth + allowlist. Không bỏ các bước kiểm tra trong source.
5. Mở:

```text
https://PROJECT_REF.supabase.co/functions/v1/licensing/health
https://PROJECT_REF.supabase.co/functions/v1/licensing/ready
```

`/health` trả version `1.29.1`; `/ready` phải trả `ready: true`. Health chỉ chứng minh handler được tải. Ready kiểm tra khóa ký và RPC/migration; chưa chứng minh đã nghiệm thu luồng trên Windows.

Nếu nhận 401 trước khi có JSON của Captovix, kiểm tra JWT verification của gateway. Nếu 503 CONFIG_INVALID: kiểm tra Secrets/issuer/khóa PKCS#8. Nếu SERVICE_UNAVAILABLE: kiểm tra project có pause, migration và log function; không in key/mật khẩu vào log để chẩn đoán.

CLI là lựa chọn thêm, không bắt buộc; triển khai Dashboard không cần Docker trên máy Boss. Chạy/test toàn bộ Supabase local bằng CLI là việc khác và không thuộc quy trình cài đặt này.

## 10. Upload trang quản trị lên Cloudflare Pages

1. Thay **`admin/config.json`** bằng file công khai được tạo ở bước 7. Nội dung chỉ có `supabaseUrl` và `publishableKey`.
2. Đăng nhập Cloudflare, mở **Workers & Pages → Create application → Pages → Direct Upload / Drag and drop**.
3. Chọn tên project, upload các file **bên trong `admin/`**: `index.html`, `admin.js`, `admin.css`, `brand.png`, `config.json`, `_headers`.
4. Deploy, lấy URL mặc định `https://TEN_PROJECT.pages.dev`. Không cần tên miền riêng. Không cấu hình build command hay API function trên Pages.
5. Quay lại Supabase Secrets, đặt **`ADMIN_ORIGINS`** bằng đúng origin này, không thêm đường dẫn hoặc dấu `/` cuối. Nếu có nhiều origin thì phân cách bằng dấu phẩy; không dùng `*`. Redeploy function sau đổi Secrets để bảo đảm worker nhận cấu hình mới.
6. Mở trang quản trị, đăng nhập email/mật khẩu bước 6. Kiểm tra tải danh sách key.

Trang giữ session trong RAM của tab; reload/đóng tab cần đăng nhập lại. Supabase refresh token cũng chỉ giữ trong RAM. Trang không chứa khóa ký, service-role key hoặc mật khẩu hardcode. Nội dung khách hàng được render bằng textContent; `_headers` đặt CSP/no-store/noindex. URL tùy chỉnh ngoài `*.supabase.co` cần sửa `connect-src` trong `_headers` cho đúng origin; cấu hình mặc định dùng Supabase URL được cung cấp miễn phí.

## 11. Dựng app Windows bằng GitHub Actions — không cần SDK trên máy Boss

1. Đưa Source lên repository của Boss, giữ đúng cấu trúc gốc `src/`, `scripts/`, `.github/`, `supabase/`, `admin/`, `tools/`, `tests/`.
2. Với repo đã có source, clone/fetch và tích hợp trước; không force push ghi đè lịch sử. Có thể dùng công cụ upload GitHub quen thuộc của Boss. File ZIP source không phải nội dung dùng làm update asset.
Ví dụ repo mới (chạy tại thư mục Source, thay URL thật):

```powershell
git init
git add .
git commit -m "Captovix V1.29.1 Serverless Licensing"
git branch -M main
git remote add origin https://github.com/OWNER/REPO.git
git push -u origin main
```

Repo đã có source: clone/fetch trước, tạo branch nâng cấp rồi tích hợp ZIP và xem diff:

```powershell
git clone https://github.com/OWNER/REPO.git CaptovixRepo
cd CaptovixRepo
git fetch origin
git switch -c update/captovix-v1.29.1
```

Đưa nội dung Source mới vào working tree, đối chiếu `git diff` và `git status`. Bản mới loại bỏ thư mục backend cũ **`server/`**; overlay ZIP không tự xóa file tracked cũ, cần phản ánh việc loại bỏ đó khi tích hợp. Không đưa private Secrets vào repo, không thay `.git` bằng dữ liệu ZIP.

```powershell
git diff
git status
git add .
git commit -m "Upgrade Captovix to V1.29.1"
git push -u origin update/captovix-v1.29.1
```

Nếu push báo fetch-first, fetch và tích hợp thay đổi remote rồi push lại, không dùng force push để ghi đè. Review/merge theo quy trình repo của Boss; Actions có thể chạy trên branch nâng cấp đã có workflow.

3. Repository **Settings → Secrets and variables → Actions → Variables**, tạo ba biến CÔNG KHAI:

| Variable | Giá trị |
| --- | --- |
| `CAPTOVIX_LICENSE_API_URL` | `https://PROJECT_REF.supabase.co/functions/v1/licensing` |
| `CAPTOVIX_LICENSE_PUBLIC_KEY_PEM` | Toàn bộ nội dung `captovix-public.pem`, gồm BEGIN/END và xuống dòng thật |
| `CAPTOVIX_LICENSE_ISSUER` | `captovix-production` hoặc issuer đang dùng |

**Không đưa private key/edge-secrets.env/service-role vào ba biến này.** Không copy chuỗi JSON có ký tự `\n` nguyên văn thay cho nhiều dòng của PEM; mở file public.pem để copy.

4. Mở tab **Actions → Build configured Captovix Windows packages → Run workflow**.
5. Workflow chạy NPM checks, .NET checks/build trên runner Windows, cài NSIS và đóng gói. Nó kiểm tra public config trước khi nhúng; từ chối cấu hình private key ở trường public key. GitHub có hạn mức runner theo tài khoản; kiểm tra Usage trước khi chạy nhiều lượt.
6. Khi thành công, tải artifact **`Captovix-Windows-configured`**. Bên trong có Update-Packages ZIP và Source ZIP đã chứa public config dùng build. Workflow không tự tạo GitHub Release.
7. Đây là cấu hình workflow được bàn giao, **chưa được chạy trong tài khoản GitHub của Boss**. Nếu CI lỗi một kiểm tra Windows, đọc log và xử lý; không bỏ kiểm tra rồi coi đã nghiệm thu.

Thay thế bằng build trên máy Windows có .NET 10 SDK/NSIS 3: copy `license-config.json` vào `src/Captovix/license-config.json`, rồi chạy:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Build-Windows.ps1
```

Build mặc định từ chối thiếu URL/public key. `-AllowUnconfiguredLicensing` chỉ để tạo review build hạn chế; không phải cờ cấp quyền. App trust config được nhúng vào DLL, không phải file cạnh EXE mà người dùng thay để tự cấp quyền.

## 12. Cấp key, dùng thử và chuyển máy

**Cấp key:** đăng nhập trang quản trị → nhập khách hàng/số máy → Tạo key → lưu key + recovery code riêng → giao cho khách. Key chỉ hiển thị khi tạo; database giữ hash, không có nút đọc lại key gốc. Khách mở Captovix → Trợ giúp → Bản quyền & khôi phục → nhập key → Kích hoạt.

**Trial:** khách bấm Dùng thử 30 ngày, cần mạng lần đầu. Bấm lại/cài lại không cộng thêm 30 ngày. Hạn cuối được hiển thị trong hộp thoại; không tính từ ngày cài app cũ V1.27.

**Chuyển máy:** máy cũ online → Trả bản quyền máy này → máy mới nhập cùng key. Máy cũ giữ ảnh/chú thích và khóa công cụ. Key nhiều slot chỉ trả đúng slot máy đã chọn.

**Mất máy:** máy mới nhập key + recovery code → Khôi phục → xác nhận trả TẤT CẢ các slot cũ → lưu mã MỚI. Mã cũ mất hiệu lực ngay, mặc định chờ 72 giờ giữa hai lần recovery. Nếu mất mã/cần hỗ trợ, quản trị xác minh khách trước khi trả máy hoặc đổi recovery. Đổi recovery không tự trả máy.

Nếu mạng ngắt sau khi gửi lệnh, máy chủ có thể đã xử lý dù client chưa nhận được trả lời. Kiểm tra danh sách máy/audit trước khi thử tiếp. Mã recovery mới bị thất lạc sau tình huống này cần quản trị đổi lại; không tự khôi phục key từ hash.

## 13. Hỗ trợ trial, quản trị và nhật ký

- Tìm key theo khách/license ID/5 ký tự cuối; phân trang 50 key. Các máy đang kích hoạt được ưu tiên hiển thị, tối đa 100 lượt trên mỗi key.
- Tìm trial theo mã máy/ID/tên máy. Có gia hạn 1–30 ngày, khóa/mở, sửa dấu khóa thiết bị sau khi cài lại nhưng giữ hạn cũ. Khách lấy Mã máy/Dấu khóa trong màn hình bản quyền; không gửi device.bin/certificate.bin/private key.
- Không giảm slot dưới số máy đang kích hoạt: trả các máy thừa trước. Thu hồi/mở lại license có hiệu lực khi máy refresh hoặc hết lease.
- Mọi thay đổi quản trị cần lý do; audit lưu UID tài khoản đã được xác minh. Không nhập key/mã khôi phục vào trường lý do.
- Audit phân trang 100 dòng; nút tải JSON chỉ tải trang đang xem. Không coi là backup toàn bộ database.
- Rate limit dùng PostgreSQL, không dựa RAM của một worker: global 600 yêu cầu/phút, mỗi khóa công khai thiết bị 30/phút, mỗi quản trị 30/phút. 429 có Retry-After 60 giây. Đây là mức mặc định kiểm chứng; lưu lượng lớn cần đánh giá quota và khả năng chống DoS.

## 14. Nâng cấp từ V1.29.0 và backup

Nếu V1.29.0 chỉ là review chưa deploy: làm quy trình mới từ đầu, không cần dựng backend cũ.

Nếu đã dùng database V1.29.0 trên cùng Supabase: backup trước, giữ nguyên private signing key/issuer, chạy migration cộng thêm, tạo Auth allowlist, deploy Edge/staging và build client gắn URL mới. Không tạo lại key khách hàng hoặc hạn trial. Source đã kiểm tra giữ dữ liệu legacy khi migration chạy lại.

Nếu database cũ ở nơi khác, cần chuyển schema/dữ liệu có kiểm tra sang Supabase trước; không dùng export chỉ các dòng đang thấy trên giao diện làm backup. Thử trên staging, đối chiếu số key/máy/hạn và hash. Không xóa/reset production.

Sau khi chuyển xong, tắt endpoint backend cũ để admin token cũ không còn đường truy cập; xử lý role DB riêng `captovix_api` của bản cũ theo kế hoạch cutover. Không tự đổi quyền role đang dùng cho app khác. Client cũ có URL cũ sẽ không tự chuyển endpoint nếu chưa cập nhật.

Sao lưu riêng: database/schema/audit/admin allowlist; private key; Secrets; public config + source/version đã phát hành. Giới hạn quyền và mã hóa backup. Đổi khóa ký đòi hỏi kế hoạch client/key ring, không tự chạy tạo khóa mới để chữa lỗi dịch vụ. Rotation/MFA là mục tiếp theo, chưa có trong V1.29.1.

## 15. Phát hành và kiểm tra

Giải nén Update-Packages của bản ĐÃ CẤU HÌNH/NGHIỆM THU, đăng năm asset lên GitHub Releases như trước:

```text
HuyVo.Captovix-1.29.1-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json
```

Source ZIP dùng phát triển, không phải gói app tải để cập nhật. Canonical Setup vẫn là NSIS wrapper cho phép chọn đường dẫn và giữ logo; không thay bằng raw Velopack Setup. Nếu đã phát hành review cùng số version, phải tăng version mới và đồng bộ source/installer/notes trước phát hành nội dung khác; không ghi đè cùng version để ép updater.

Kiểm tra bắt buộc sau deploy: `/ready`; đăng nhập đúng/sai/không có quyền; tạo key kiểm thử riêng; trial; Windows active; hết lease khi đang nhập chữ; offline/clock rollback; Ctrl+C/V/context menu; key đủ slot; chuyển máy; recovery/old code; revoke/restore; cập nhật từ bản cũ và bảo toàn media/theme. Kiểm tra giao diện sáng/tối và nhiều màn hình/scale trên Windows thật.

Kiểm tra phát triển:

```powershell
npm ci
npm run check
npm test
python scripts\Run-Checks.py
```

`npm run test:integration` cần PostgreSQL KIỂM THỬ riêng đã migration, các role test và `LICENSE_TEST_DB`/`LICENSE_TEST_OWNER_DB`. Không chạy trên production: test tạo và thay đổi license/trial. Có thể cung cấp `DOTNET_EXE` + `INTEROP_DLL` để kiểm tra chữ ký hai chiều; build harness `tests/ServerlessInteropChecks` trước.

Đã kiểm tra TypeScript, Node WebCrypto/DOM, PostgreSQL thực với quyền role và transaction, interop .NET. Auth transport trong test là fixture, không phải Supabase Auth thật. WPF trên Linux chỉ cross-build; chưa deploy Supabase/Cloudflare/GitHub Actions hoặc chạy Setup/DPAPI trên Windows thật. Xem `docs/VALIDATION.md` để biết đúng phạm vi bằng chứng.

Tài liệu chính thức: [Dashboard Edge Functions](https://supabase.com/docs/guides/functions/quickstart-dashboard), [Function auth/configuration](https://supabase.com/docs/guides/functions/auth), [SQL function privileges](https://supabase.com/docs/guides/database/functions), [Pages Direct Upload](https://developers.cloudflare.com/pages/get-started/direct-upload/), [Pages headers](https://developers.cloudflare.com/pages/configuration/headers/), [Supabase pricing/limits](https://supabase.com/pricing).
