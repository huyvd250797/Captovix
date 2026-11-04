# V1.41.0 — Groups & Layout Editing validation

Source nền: Captovix V1.40.0 — Move & Background Recovery.zip Boss gửi. Không thay dependency/runtime/backend/license.

## Đã kiểm tra logic

| Suite | PASS |
| --- | ---: |
| GroupLayoutChecks | 30 |
| EditorCoreChecks | 57 |
| CompositionChecks | 167 |
| ObjectControlChecks | 42 |
| PrecisionChecks | 133 |
| AdvancedAnnotationChecks | 49 |
| ShellLayoutChecks | 41 |
| ConnectorChecks | 81 |
| TextClipboardChecks | 54 |
| MockupChecks | 28 |
| ElementScanChecks | 35 |
| MoveRecoveryChecks | 33 |
| QuickCaptureChecks | 20 |
| CaptureWorkflowChecks | 67 |
| PixelStudioChecks | 57 |
| CaptureBoundsChecks | 73 |
| InstallerContractChecks | 25 |

Tổng 992 kiểm tra logic/contract, cộng11UI contracts trong `scripts/Check-M4-UI.py`. Số đếm theo từng dòng PASS thực tế, gồm các phép thử sau dòng tổng kết cũ của một số suite.

M4 xác minh nhóm lồng, chọn scope, giữ khoảng cách/layout, property atomics, copy/duplicate groupID, deep snapshot, Undo/Redo/reopen, validate vòng lặp/nhiều cha; hồi quy nhóm phẳng, text/callout/connector, Mockup/recovery, shortcuts, capturebounds, installer.

Đã cross-build appRelease và GroupLayoutWindowsChecks fixture; hostLinux không thực thi WPF. FixtureWindows kiểm kéo cả panel, kéo riêng label, chọn cha2cấp, render/save/reopen/project/schema21backup và Undo/Redo.

Sau đóng gói phải chạy UpdateChecks trên full.nupkg thật (thêm installed1.40→1.41, giữ nhảy thẳng từ1.31), Verify-Packages đối chiếu source, feed/checksum/CRC/runtime/apphost/SharedDLL và fiveassets. Không dùng cross-build thay nghiệm thu Windows.

## Chưa nghiệm thu native

Windows10/11x64: cài đè/update/restart, click/kéo/keyboard/PrtSc/IME, cây khi10kđối tượng, popup active-owner/outside feedback, theme sáng/tối, scale100/125/150/200%, màn phụ, Explorer/library2hướng. Checklist chi tiết trong GROUPS-LAYOUT-EDITING-V141.md. Phản hồi outline dựa HWNDmodal owner; cần kiểm trên Windows thật.

## Giới hạn

- Nhóm là cây quan hệ và tọa độ lưu chính xác; không tự reflow/constraint responsive hay inpaint nền phức tạp.
- Tối đa16cấp/10kđối tượng. Nhóm có con khóa/ẩn không cho biến đổi toàn nhóm; chọn riêng con khác được.
- Undo/Redo trong phiên; journalM3 vẫn được lưu nhưng không phải persistentUndoStack.
- Popup Captovix thống nhất; hộp chọn tệp/màu do Windows cung cấp vẫn native.
- Nhóm lồng dùngschema21, cóbackuptrướcghi; appcũkhôngđọc21. Không migrationmới.

## Hồ sơ phiên bản trước

# V1.40.0 — Move & Background Recovery validation

Nền sourceV1.39.0 và Update-Packages V1.39.0 Boss gửi. Giữ CaptureEngine/Program/installer arguments/license config/protocol/backend/runtime.

## Đã chạy

| Suite | PASS | Phạm vi |
| --- | ---: | --- |
| MoveRecoveryChecks | 33 | Mask pixel/bảo vệ lân cận/nguồn bất biến, mask độc lập/Move/Delete/Undo/Redo/JSON/history, snapshot, clipboard/duplicate, validate, shortcut conflict/roundtrip/configEnter |
| CaptureWorkflowChecks | 67 | Cấu hình, giới hạn phím, backup/atomic save, registration rollback; cập nhật kỳ vọng3phím chụp đang dùng |
| EditorCoreChecks | 57 | Session/history/snapshot/autosave |
| CompositionChecks | 167 | Project/template/layers/legacy schema |
| TextClipboardChecks | 54 | Text/clipboard data contracts |
| ElementScanChecks | 35 | M2 scan/review/links/persistence/license presentation |
| PixelStudioChecks | 57 | Pixel/source fidelity |
| MockupChecks | 28 | M1 objects/styles/erase/save/Undo/paste |
| QuickCaptureChecks | 20 | Selector layout, catalog/icon/dynamic command contracts |
| CaptureBoundsChecks | 73 | Source region bounds/randomized regression |

Tổng591 kiểm tra logic đạt. App Release và MoveRecoveryWindowsChecks cross-build0warnings/0errors. Không coi puretests/cross-build là đã chạy thao tác Windows thật.

FixtureWindows mới gồm: actual EditorSurface gesture tách mask, nút cũ biến mất trên nền phẳng, giữ nguyên neighbor nằm trong vùng chọn rộng, nguồn không đổi, Undo/Redo/move/resize/Delete,0annotation export, crop-padding clip, sampling phục hồi, backup sidecar20, background reopen và portable project.

Pipeline bàn giao bắt buộc UpdateChecks dùng Fullnupkg thật, gồm installed1.39 nhận/tải/xác minh1.40 và1.31 nhảy thẳng1.40. Verify-Packages kiểm source/payload/nupkg/feed/SHA256/SHA1/CRC/installer/models/SharedDLL/hostVersion/5assets. Không chạy exeWindows trên hostLinux.

Runtime giữ.NET10.0.13 self-contained và OCR từ gói1.39 Boss gửi; replace app host/DLL và SharedDLL bằng buildRelease mới, tạo lại execution stub. VC++ prerequisite giữ nguyên bytes từ bộ cài1.39. Không đổi dependency/runtime hay dịch vụ license; không migration mới.

## Giới hạn và nghiệm thu còn chờ

- Chưa thực thi WPF Windows fixture, chuột/IME/phím/globalhotkey/PrtSc, sidebar/theme/DPI/đa màn hình, cài đè/updater/restart trên Windows.
- Cần chạy form thật: Move/resize/Delete/duplicate2nút gần nhau; vùng giữ, previewmask/color, Undo/Redo, chuyển thư viện/mở lại; bộ ảnh nền sáng/tối/phẳng, source/crop/alpha; mask gần border/neighbor.
- Nền ảnh/gradient/hoa văn chưa inpaint. Mặt nạ không đồng màu được đánh dấu cần kiểm tra; không khẳng định tự phục hồi mọi nền hay giữ được100% chi tiết chưa nhận diện.
- Kiểm Enter tắt/bật với kéo vùng/clickmodal, Esc,8px threshold, pixelBounds; quay/cuộn vẫn xác nhận. Đổi toàn bộ phím chính/Thêm, phím đơn/tổ hợp, chặn trùng, mở lại cấu hình, không chạy khi gõ/chọn/kéo; công cụThêm phải đổi ô động như click.
- Kiểm sidebar ở100/125/150%DPI, rộng250/278px: nhãn ngắn không rớt dòng, swatch/tooltip và input cân đối; không có menuVùng pixel/hàngTheo cấu hình/Chọn cửa sổ/hai mô tả dài.
- Không tự publishGitHub hoặc deploybackend trong lần bàn giao này. Schema20 và các backup được mô tả trong MOVE-BACKGROUND-RECOVERY-V140.md.

## Lịch sử

# V1.39.0 — Element Scan & Review validation

Nền: ZIP Captovix V1.38.0 — Text & Button Editing Boss đính kèm. Giữ các sửa capture/thread/Callout/M1, không đổi dependency/runtime.

## Đã chạy

| Suite | PASS | Nội dung |
| --- | ---: | --- |
| ElementScanChecks | 35 | OCR/vision synthetic pixels, loại trùng, ghép label-control, offset/crop, kiểm tra tường minh, cap/cancel, sửa/Apply atomically, Undo/Redo/JSON, copy liên kết, trạng thái trial/perpetual/ngày kích hoạt |
| MockupChecks | 28 | M1: màu, vùng che, source, save/Undo/Redo/paste/template/resize |
| CompositionChecks | 167 | Lớp ảnh/dự án/mẫu/schema và nguồn |
| TextClipboardChecks | 54 | Text/clipboard data contracts |
| PixelStudioChecks | 57 | Pixel/source fidelity |
| CaptureBoundsChecks | 73 | Biên chụp và randomized cases |
| ErrorDialogChecks | 7 | Non-reentrant error dialog gate |
| QuickCaptureChecks | 20 | Catalog/công cụ động/icon contracts |
| LicensingChecks | 22 | Chữ ký, binding, legacy certificate không có ngày, ngày hợp lệ/sai |

Tổng463 kiểm tra logic/crypto đạt. Ngoài ra22 handler tests TypeScript đạt, gồm giữ activatedAt trong payload đã ký và từ chối payload bị sửa. Đây là unit/mocked HTTP/RPC, không gọi license production.

App Release và ElementScanWindowsChecks cross-build:0warnings/0errors. FixtureWindows chứa che hết glyph cũ, unchecked không tạo, nguyên vẹn nguồn, một bước Undo/Redo, privacy-rendered/frozen snapshot, project reload, sidecar backup19/background load và popup actualWindow cho trial/perpetual. Chưa thực thi fixtureWPF do hostLinux không cóWindowsDesktop.

Migration SQL được đối chiếu với hàm đã có: chỉ thêm đúng activatedAt từ devices.created_at; policy, security definer/search_path/grants và các SQL cũ giữ nguyên. Chưa chạy migration lên Supabase thực. FileTXT rời phải khớp bytesSQL trong source.

Pipeline bàn giao bắt buộc UpdateChecks với Fullnupkg thật: máy1.38.0 nhận/tải/kiểm hash1.39.0,1.31 nhảy thẳng1.39.0, chặn bản cũ/hash sai. Verify-Packages.py đối chiếu source/payload/nupkg/feed/SHA256/SHA1/CRC/installer/models/SharedDLL/manifest và5releaseassets trước bàn giao.

Payload dùng lại self-contained1.38 (.NET10.0.13 + native/OCR), thay Captovix.exe/Captovix.dll và Licensing.Shared.dll bằng buildRelease mới, tạo lại execution stub, cập nhật app dependency1.39.0. VC++ prerequisite lấy nguyên bytes từ fullSetup1.38, không tải bản runtime khác. Không dùng framework-dependent output làm bộ cài.

## Còn chờ nghiệm thu

- Windows: chạy tests/ElementScanWindowsChecks cùng fixture hồi quy M1/ảnh dán/capturebounds; form thật tiếng Việt/Anh, viền/nút/input/khối, sửa khung/kéo/cancel/preview,200khung, lựa chọn tường minh; Ctrl/Shift+Scroll; Undo/Redo; chuyển thư viện/mở lại; DPI100/125/150%; IME và Setup/updater restart.
- Quét synthetic dùng OCR lines chuẩn bị sẵn để kiểm thuật toán, chưa đo precision/recall OCR/vision trên tập form thật; không cam kết tách chính xác100% hay không lag trên mọi máy. Nền gradient/ảnh chưa được phục hồi.
- Supabase: chủ dịch vụ chạy202611040001 SQL rồi refresh license, kiểm ngày activation/lease/trial cũ không đổi. Không tự deploy DB hoặc publishGitHub trong lần bàn giao.

Hướng dẫn: ELEMENT-SCAN-REVIEW-V139.md và ACTIVATION-METADATA-V139.md.

## Lịch sử

# V1.38.0 — Text & Button Editing validation

Source nền: Captovix V1.37.3 — Image Layer Thread Safety.zip Boss gửi. Không đổi CaptureEngine/InstallPath/license/backend; giữ bản sửa1.37.2/1.37.3.

## Đã chạy trên host phát triển

| Suite | PASS | Phạm vi |
| --- | ---: | --- |
| MockupChecks | 28 | Metadata, chọn vùng, màu, lưu/đọc, Undo/Redo, paste/duplicate/template, validate và resize |
| EditorCoreChecks | 57 | Document/session/history/snapshot |
| CompositionChecks | 167 | Lớp ảnh, dự án, mẫu và schema cũ |
| TextClipboardChecks | 54 | Text/clipboard data contracts |
| PixelStudioChecks | 57 | Pixel operations / source fidelity |
| ErrorDialogChecks | 7 | Gate hộp thoại và reentrancy |
| CaptureBoundsChecks | 73 | Biên ảnh / vùng chọn và randomized regression |
| StudioReliabilityChecks | 52 | Style/load/restore safeguards |
| PrecisionChecks | 133 | Object resize/move/Callout pivot hồi quy |

Tổng628 kiểm tra logic; không phải628 thao tác nghiệm thu Windows. App Release và fixture MockupWindowsChecks cross-build0warnings/0errors.

- Fixture MockupWindowsChecks chứa render che glyph cũ, pixels ngoài vùng và ảnh gốc không đổi, Undo/Redo pixels, Việt/border/radius, snapshot liên luồng, preview giới hạn, sidecar backup/reload, portable project và click/drag. Chưa thực thi vì hostLinux không cóWindowsDesktop.
- UpdateChecks dùng góiFull thật, gồm máy1.37.3 nhận/tải/xác minh1.38.0 và máy1.31 nhảy thẳng1.38.0; package pipeline yêu cầu đạt trước bàn giao.
- Verify-Packages.py đối chiếu source/payload/nupkg/feed/SHA256/SHA1/CRC/installer/OCRmodels/license/backend và đủ5releaseassets. Hướng dẫn publish nằm trongupdateZIP. Chưa publishGitHub.
- Đóng gói tái sử dụng payload self-contained1.37.3 (.NET10.0.13 + binary native/runtime đã có), thay Captovix.dll bằng bản Release1.38.0 và cập nhật application dependency1.38.0. Không thay runtime/dependency; không dùng framework-dependent output làm bộ cài.
- Mockup dùng schema18 chỉ khi cần; backup sidecar cũ ở .pre-v1.38.bak. Không migrationDB/deploybackend. Bản cũ không đọc schema18.

## Nghiệm thu Windows còn chờ

Chạy `dotnet run --project tests/MockupWindowsChecks -c Release -p:Platform=x64` và các fixture hồi quy. Nghiệm thu form thực với3label/title/nút; OCRvie/eng, IME, sửa/Cancel, màu nền phẳng sáng/tối, kéo/resize, chuyển thư viện rồi mở lại, Undo/Redo, lưu dự án/PNG/copy. Kiểm DPI100/125/150% và cài đè/updater/restart. Không khẳng định đã chạy native OCR/WPF/Setup trên Windows hoặc đo chất lượng nhận diện trên form thật. Chi tiết docs/MOCKUP-M1-V138.md.

## Lịch sử

# V1.37.3 — Image Layer Thread Safety validation

- Source gốc: ZIP V1.37.2 Boss đínhkèm; giữ CaptureBounds và mọi sửa trước đó.
- App Release cross-build0warning/0error; Windowsthreadfixture cross-build0warning/0error, chưa thực thiWPFWindows.
- ErrorDialogChecks7kiểm tra đã chạy đạt; các kiểm logic PixelStudio/Composition/TextClipboard/CaptureBounds được chạy lại.
- FixtureWindows bao gồm bitmap workerđãfreeze nhưng không còn decoder, PNGalpha/pixel, cachehit liênluồng, STAthumbnail, sidecar save/load và25lần UI-scene-freeze/render.
- UpdateChecks dùng góiFull thật, gồm máy1.37.2 nhận/tải bản vá; SourceZIP/CRC/payload/feed/installer được kiểm toànvẹn.
- Không đổi runtime/dependency/license/backend/schema; không cần migration mới. Chưa publish GitHub.
- Chưa nghiệm thu thực tế Copy/Paste/quicklibrary/render/WindowsDesktop; không coi cross-build hoặc puretest là đã chạy tình huống WPF máyBoss. Chi tiết docs/IMAGE-LAYER-THREAD-V1373.md.

## Lịch sử1.37.2

# V1.37.2 — Capture Bounds Fix validation

-73 kiểm tra giới hạn tọa độ đạt, gồm20.000 trường hợp ngẫu nhiên.16FrozenCaptureChecks,49SmartCaptureChecks,20QuickCaptureChecks đạt.
- Release app buildWindows từLinux đạt0warnings/0errors. Selector-session Windowsfixture cross-build với appRelease; chưa chạy Windows.
- Kiểm bằng góiFull thật: installed1.37.1 nhận/tải/xác minh bản1.37.2; feed/hash/sourceZIP/installer/payload được kiểm riêng.
- Runtime/dependency và binary ngoài app giữ từ gói1.37.1; license/backend/schema không đổi. Không migration mới.
- Chưa publish GitHub. Chưa nghiệm thu chụp/DPI/đa màn hình/Setup trực tiếp trên Windows. Chi tiết docs/CAPTURE-BOUNDS-V1372.md.

## Lịch sử kiểm tra1.37.1

# V1.37.1 — Validation

- Build Release Windows từ Linux; kiểm tra phiên bản assembly/UI/installer/nuspec/feed cùng1.37.1.
- UpdateChecks kiểm tra bằng gói Full thực: máy1.37.0 nhận1.37.1, tải và xác minh SHA256; vẫn chặn hash sai/bản cũ/cùng phiên bản.
- Kiểm toàn vẹn source ZIP, payload, feed, installer và update ZIP.
- Bản vá chỉ đổi version/tài liệu và kiểm thử updater; logic Callout giữ nguyên bản sửa đã kiểm tra bên dưới.
- Chưa publish lên stable latest. Chưa chạy cài đè/chuột/IME/DPI trên Windows. Không cần migration mới.

## Lịch sử kiểm tra V1.37.0

# V1.37.0 — sửa click ngoài và tâm Callout

- PrecisionChecks bổ sung kiểm tâm kéo thân ở0/35/90/-120°, cả pivot thân hiện tại và Callout cũ: tip world-space không đổi, thân dời đúng delta, không resize/đổi chữ; Undo/Redo khớp byte.
- EditorCoreChecks và CalloutGestureChecks chạy lại để giữ cách kéo giãn2trục đã chốt.
- CalloutInteractionWindowsChecks mới: mouse-down/rung/click không tạo; drag tạo1; click nhiều lần không sinh bản sao; tâm dời thân giữ đuôi ở Callout xoay, kéo ngoài tâm dời cả cụm, node không xuất vào ảnh; gọi luồng click ngoài thật để commit chữ Việt và consume click canvas. Chỉ cross-build, chưa chạy Windows.
- ReleaseWindowsChecks đã bỏ kiểm thử bấm đơn tạo Callout cũ, thay bằng kiểm chỉ kéo mới tạo.
- Giữ version1.37.0 và cấu hình license/backend. Runtime và DLL khác giữ từ góiV1.37 trước; đóng gói DLL app mới và Setup, kiểm source/payload/feed/ZIP.
- Chưa nghiệm thu chuột thật, IME, DPI/zoom và Setup trên Windows. Đang dùng1.37 cũ cần thoát app/khay và chạy Setup mới thủ công; updater không báo bản cùng version.

## Lịch sử kiểm tra

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
