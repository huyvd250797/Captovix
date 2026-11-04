# Hiệu chỉnh Callout V1.37.0 theo mô phỏng đã duyệt

- CalloutGestureChecks:2.000trường hợp ở4hướng và801bước quét ngang, kiểm đuôi ghim/rộng-cao độc lập/kích thước hợp lệ/Undo-Redo; có xử lý sai số số thực tại ngưỡng36px để không bị hủy hộp nhỏ khi thả.
- EditorCoreChecks cập nhật ví dụ kéo ngang/dọc/chéo, nhấn đơn và giữ đuôi. TextStudioWindowsChecks/PerformanceWindowsChecks cập nhật kiểm kích thước thay đổi trong lúc kéo và giữ sau thả khi chưa nhập chữ; chỉ cross-build tại Linux.
- Không áp dụng tăng bản1.37.1; đóng gói lại DLL và Setup1.37.0. Runtime/dependency và toàn bộ payload không liên quan giữ từ góiV1.37 gốc; đối chiếu từng byte các binary không đổi. Config license/backend giữ nguyên.
- Chưa nghiệm thu chuột/IME/Setup trên Windows. Đang dùng V1.37 cũ cần chạy Setup mới thủ công; không qua kiểm tra cập nhật cùng phiên bản.

## Kiểm tra của bản V1.37 trước hiệu chỉnh

# V1.37.0 — Validation

- Run-Checks.py:61suites build thành công,36logic thực thi thành công,25Windows/WPF chỉ cross-build.2.100assertions PASS; kết thúc ALL_CHECKS_OK.
- EditorCore: snapshot nhanh có nội dung serialize giống bản gốc và không dùng chung mutable fields; save revision cũ không đánh dấu sạch bản sửa mới. Gesture Callout giữ220×120,tip đầu tiên và đi theo cảX/Y khi kéo4hướng.
- BatchChecks:30mục gần đây, pin lịch sử vẫn trong30, giữ toàn bộ metadata/ảnh cũ.
- PerformanceWindowsChecks mới: offscreenrender trên STA worker, kéo Callout chuẩn, scene drawing tái dùng khi zoom, edit vô hiệu cache, giữ tool khi load, decodedcache rỗng sau100vòngload/close. Chỉ compile, chưa chạy Windows. EditorWindowsChecks cập nhật chờ async và kiểm chuyển nhanh không áp dụng kết quả cũ; Refinement/QuickCapture kiểm giữCallout/Crop.
- Sau thay đổi STA/lifecycle cuối: build lại PerformanceWindowsChecks và EditorWindowsChecks,0warning/0error; publish app từ cùng source. Task autosave không chờ UI, final-save chờ worker trước để không bị bản cũ ghi đè. Worker xóa action/cache để không giữ snapshot/bitmap cuối.
- Microbenchmark logic đo trênLinux/.NET10.0.0:5.000chú thích, một đoạn20.000ký tự; warmup3/sample11/median. Begin1000lần:3,818→0,0194ms; snapshot133,9469→6,518ms; allocation20.472.582→3.321.275byte. File JSON thô và script tái lập trong source. Chạy lại script thành công; số thay đổi theo tải host, không là Windows UI benchmark.
- Startup/firstpaint/open/frame/autosave có buffer đoRAM giới hạn128mẫu, không tạo menu chẩn đoán trở lại. Chưa có số FPS/inputlatency/working-setWindows; chưa kết luận hếtlag/khôngleak trên mọi máy.
- Giữ schema17, runtimeWindows10.0.13, dependency và pipelinepixel hiện hữu. Không cần migrationDB; publiclicenseconfig, backend/admin không đổi. Không gọi production tạoKEY/trial.
- Kiểm gói: chạy SDKUpdateChecks với nupkg mới, CRC/hash/feed/nuspec/source/payload/icon/OCR và cấu hình nền qua Verify-Packages. Chưa thực thi Setup hoặc restart update trên Linux.
- Chưa pushGitHub/deploy/publishrelease. Windows acceptance theo docs/PERFORMANCE-RESPONSIVENESS-V137.md:startup,4K/cuộndài,chuột/IME/Callout,zoom,autosave lỗi/thoát,chuyển nhanh,RAM,càiđè.

## Lịch sử kiểm tra

# V1.36.0 — Validation

- 60 suites biên dịch thành công; 36 bộ logic chạy thành công trên Linux, 24 bộ Windows/WPF chỉ biên dịch. scripts/Run-Checks.py kết thúc ALL_CHECKS_OK.
- EditorCoreChecks thêm tạo Callout4hướng, điểm đuôi giữ nguyên và thân căn dọc giữa. PrecisionChecks thêm resize rộng riêng, giữ chiều cao/đệm/font và tail ở góc0/25/90°.
- TextStudioWindowsChecks mở rộng: giữ kích thước hộp với chữ ngắn, chữ dài tự nở và đủ dòng, giữ đuôi xoay, cache dàn chữ khi dịch chuyển phân số, thay chữ/màu không giữ hình cũ, pixel khớp khi kéo về chỗ cũ; có phép đo120lần di chuyển Callout20.000ký tự. Chỉ biên dịch tại Linux; không có số đo hiệu năng Windows để kết luận hết lag trên mọi máy.
- ShellLayoutChecks kiểm logo/tên và loại các lệnh chẩn đoán/hướng dẫn khỏi XAML; mọi menu công cụ/chụp/cập nhật/bản quyền vẫn giữ.
- UpdateChecks dùng SDK thật xác định đích từ1.31 tới1.33/1.36 với feed chỉ có Full mới nhất, không bản trung gian; updater dùng UpdateInfo không chứa chuỗi delta. Sau đóng gói chạy lại với nupkg1.36 thực để kiểm tải/SHA-256/AppId/version. Không chạy Update.exe hoặc Setup trên Linux.
- Pixel capture/copy/PNG giữ pipeline physical snapshot và RenderExact sẵn có; kiểm pixel đầy đủ không phụ thuộc display zoom. Không thay bằng sharpen hoặc giảm độ phân giải. HDR/WCG cần nghiệm thu riêng.
- Sau hiệu chỉnh cache/input cuối, cross-build lại TextStudioWindowsChecks và publish app; Verify-Packages đối chiếu payload/feed/source/ZIP, icon, OCR và public license/backend từ source1.33.
- Không migration DB, không thay endpoint/public key/admin/Supabase; không gọi API production tạo key/trial. Chưa push/deploy/publish release.
- Còn chờ Windows thật: WPF/input tiếng Việt và IME, bố cục/caret/DPI, kéo20ktext và đo frame, native chụp/clipboard/Word/Drive, cài đè và restart cập nhật1.31→1.36. Checklist docs/TEXT-CALLOUT-POLISH-V136.md. Cross-build không thay nghiệm thu này.

## Lịch sử kiểm tra

# V1.33.0 — Validation

- 60 suites biên dịch thành công; 36 bộ logic thực thi trên Linux và 24 bộ Windows/WPF chỉ biên dịch.
- ScrollQualityChecks: pixel rows không mất/lặp, fixed header/footer, phát hiện khung động theo tile, lệch ngang không được chấp nhận tự động, transpose cuộn ngang và hủy phép ghép.
- ShellLayoutChecks: công cụ đúng thứ tự, chuyển khỏi canvas, inspector lên hàng trên, giữ Copy/Export qua menu, không trùng tên WPF.
- CaptureQuality133WindowsChecks: fixture tự chụp cửa sổ cùng process và guard trial đang dùng/đã hết hạn không đổi expiry, không gọi dịch vụ. Chỉ compile tại Linux; chưa thực thi trên Windows.
- Sau tối ưu lề canvas và zoom readout: build app/fixture và chạy lại ShellLayoutChecks thành công.
- Backend trial trong source chỉ tạo trial khi chưa có device và giữ nguyên trial_until khi nhận yêu cầu lặp. Không đổi SQL/API/admin; public license config và endpoint update đối chiếu khớp source đính kèm.
- Chưa nghiệm thu UI/native Windows: Word/Explorer/Drive FileDrop, GDI/UIA, DPI, popup/theme, thao tác chuột và Setup/cài đè/updater thực tế. Xem checklist docs/SCROLLING-CAPTURE-QUALITY-V133.md.
- Chưa push/deploy/publish release. Không tạo key/trial trên production khi build/test.

## Lịch sử kiểm tra

# V1.32.0 — Validation

- 59 bộ kiểm tra trong scripts/Run-Checks.py đã build; 36 bộ logic/chạy được trên Linux đã thực thi thành công.
- 23 bộ Windows/WPF chỉ biên dịch thành công; chưa chạy native tại môi trường Linux.
- Bộ SmartRefinementWindowsChecks mới: khung Text live preview/resize/Undo/Redo/export, vector stamp không bị cắt và click placement, node xoay mũi tên. Có thể chạy trên Windows x64 bằng dotnet run --project tests/SmartRefinementWindowsChecks -c Release -p:Platform=x64.
- Sau hiệu chỉnh cuối: build app và bộ SmartRefinementWindowsChecks, chạy lại CompositionChecks và QuickCaptureChecks thành công.
- Schema17 chỉ khi có fixed TextFrame; legacy manual text không tự thành khung. Reader/writer vẫn bảo vệ schema tương lai.
- Public license config đối chiếu byte với source gốc và tài nguyên nhúng trong Captovix.dll; không gọi API tạo key/trial, không thay server/admin.
- Kiểm thử build/package không thay thế Windows desktop acceptance. DPI100–200%, UIA trình duyệt/modal, PrtSc, IME, clipboard, install và cập nhật native vẫn chờ theo docs/SMART-ELEMENT-CAPTURE-REFINEMENT.md.

## Kết quả các bản trước

# V1.31.0 — Quick Capture Experience: xác minh và giới hạn

- Source nền V1.30 được đối chiếu Captovix.dll và Licensing.Shared.dll với full.nupkg trong Update-Packages Boss gửi: khớp byte. Không thay cấu hình bản quyền/backend/admin/supabase/migrations/dependencies.
- Đã chạy Run-Checks.py: các suite logic/contract chạy trên Linux, suite WPF chỉ cross-build. Có bộ mới QuickCaptureChecks20 tình huống: outside toolbar, fallback góc xa, tọa độ âm, seed keyboard, notes chung, catalog tool/icon/stamp, chọn/chỉnh vùng. CompositionChecks thêm schema16, màu Bước/transparent/copy/invalid color/legacy preservation.
- QuickCaptureWindowsChecks đã cross-build: mouse-up crop/Undo, màu Step qua handler thực, stamp pixels và vị trí ô công cụ động. Chưa thực thi WPF trên Windows. Test authority fixture chỉ trong test assembly, không tạo trial/key trên server sản xuất.
- App Windows x64 được self-contained publish với runtime10.0.13, giữ pin Tesseract/Velopack/ScreenRecorderLib. Không nâng runtime/package trong bản này.
- Verify-Packages đối chiếu sourceZIP, payload, CRC/hash/feed/nuspec, canonical installer wrapper/logo, OCR models và giữ public license/LicenseRuntime/Edge/admin config từ nguồn nền. Updater checks chạy với nupkg vừa tạo. Không mô tả bước này là đã chạy Setup.
- Build/cross-build không chứng minh toàn bộ UI/input/native capture: bắt buộc nghiệm thu Windows100–200%, nhiều màn hình âm, chụp bằng chuột/bàn phím/Enter một lần, full-screen toolbar, contrast/cursor/lens/crosshair không vào PNG, crop thả/Undo, Step màu/copy/reopen, các stamp, recent tool sau restart và reorder, update popup/settings/theme.
- Giữ GDI/snapshot/PNG invariants của V1.30, không resize file do zoom. HDR/WCG, video, window/scroll live vẫn cần ma trận riêng; không hứa toàn bộ ứng dụng UIA đều có cây phần tử hoàn hảo.
- Không cần migration mới, không deploy lại Edge/admin, không push/publish GitHub Release. Hai ZIP bàn giao, Setup trong Update-Packages. Schema16 chỉ xuất khi dùng kiểu Bước mới; app cũ không đọc được sidecar này, cần backup trước rollback.

Hướng dẫn và điều kiện nghiệm thu: docs/QUICK-CAPTURE-EXPERIENCE.md.

---

# V1.30.0 — Validation hiện tại

Source nền là ZIP V1.29.1 Boss đính kèm. Đối chiếu byte cho thấy public license config, LicenseRuntime, Licensing.Shared, Supabase/backend và admin không thay đổi. Không gọi API để tạo trial/key, không redeploy hoặc chạy migration. Không cần migration mới.

- Chạy đủ56project qua Run-Checks: 35suite logic/contract thực thi Linux;21suite WPF chỉ cross-build. Không đánh đồng compile với runtime acceptance.
- FrozenCaptureChecks:16 kiểm tra, gồm500crop ngẫu nhiên, negative bounds, edge pixels/magnifier, output không sửa snapshot, guard crop/cap, save snapshot lifecycle. Không có giả lập này nào chứng minh GDI thật đã chạy.
- ImageFidelityChecks122, Geometry/CaptureWorkflow/SmartCapture/Viewport và các suite cũ đã qua. ImageFidelity kiểm tra exact BGRA crop/90°/flip/alpha và toán DPI100–200%/chuyển màn. WPF suite mới chứa20tổ hợp zoom/DPI nhân tạo qua Editor/PNG và clipboard tùy chọn; chỉ cross-build ở đây.
- Build/publish Windows x64 thành công trên Linux, giữ runtime10.0.13. Package kiểm tra CRC, SHA256/SHA1, app/version, icon approved, wrapperSetup, nguồn update, modelOCR, sourceZIP đầy đủ/byte-identical và các file mới có trong payload. Updater SDK kiểm tra chính full.nupkg bàn giao trước upload. Setup chưa được thực thi trênWindows.
- Giữ pipeline PNG đúng kích thước và kiểm tra BGRA decode trước ghi đè. SnapshotGDI, UIA live/frozen overlay, clipboard, scaleWindows thực, taskbar nhiều màn/negative và hiệu năng RAM/latency cần nghiệm thu theo docs/CAPTURE-FIDELITY-RELIABILITY.md.

**Điều kiện nghiệm thu Windows còn chờ:** ảnh PNG đúng W/H vùng đã chọn, không đổi pixel vùng không chỉnh, giữ khoảnh khắc khi đợi/notification, kính lúp đúng pixel, thay zoom không đổi output, nhiều màn100–200%/tọa độ âm, cancel/retry/disconnect không lấy frame cũ. HDR/WCG riêng. Tất cả hàng runtime trong matrix ở trạng thái Chờ Windows, không tự đánh dấu đạt.

Không publish GitHub Release hoặc deploy trong lần bàn giao này. Cài đè Setup từ Update-Packages và giữ dữ liệu/license; SourceZIP có README, roadmap và checklist/harnessWindows để tiếp tục nghiệm thu.

## Lịch sử kiểm tra các bản trước

# V1.29.1 — Configured Windows build validation (03/11/2026)

- Nhúng file public config Boss gửi, không thay code capture/editor/giao thức/backend/SQL.
- Cấu hình HTTPS đúng dạng, issuer captovix-production, SPKI RSA public key 3072-bit; không có private key.
- GET /health và /ready trên API Boss trả HTTP 200; ready:true. Không tạo key/trial/lượt máy mới hoặc sửa database production để kiểm tra.
- App Windows x64 self-contained cross-publish thành công. Công cụ tools/VerifyLicenseBuild đọc ManifestResource từ PE Captovix.dll thực tế, so byte-for-byte với file Boss gửi; SHA256 cấu hình 46b916ca0db15843394a475c82f0b562740a31e232ea67ac4e716656e82cd4e4.
- 18 cryptographic protocol checks và 25 installer contract checks qua. Package được kiểm tra lại CRC/hash/version/icon/source/payload và updater với chính full.nupkg vừa dựng.
- Không cần chạy migration mới. Không push GitHub/deploy lại Cloudflare/Supabase. admin/config.json trong source vẫn là mẫu, không upload đè trang admin đang chạy.
- Chưa chạy WPF/DPAPI/Setup trên Windows, chưa dùng key thật hoặc trial để nhận chứng nhận từ API Boss. Health/ready không xác nhận cặp signing key server khớp public key; luồng kích hoạt trên Windows cần xác minh điều này.

## Lịch sử kiểm tra source nền

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
