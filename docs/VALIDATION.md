# V1.45.0 — Validation

05/11/2026. Nền đúng ZIPV1.44.1 Boss cung cấp, SDK.NET10.0.100Linux, Windowsx64/.NET10.0.13 và OCR/Velopack1.2.161 giữ nguyên.

Kiểm hồi quy Run-Checks.py và focusedReleaseQualityChecks/UpdateChecks/Windowscrossbuild. Bộ mới kiểm dữ liệu sourceRaster mộtPNG, khôngerase khiapply, chuyển riêng label khiedit, undo,groupmove,save/reopen,copy/detach, bỏquétcũgiữprivacy; backup/manifest/restore/tamper/journal. ReleaseQualityWindowsChecks mới kiểm pixelđúngnguồn và khôngmấtnộidungkhiOCR/font sai; chỉcompile trênhostLinux.

Chưa có nativeWindows/Setup/WPF/IME/scrollvậtlý hoặc benchmarkWorkingSet/p95Windows trongphiênnày. Không tuyênbố đạt hoànchỉnh cổng nghiệmthu; WINDOWS-ACCEPTANCE-V145.md là yêu cầu trướcstable. Source/bộ cài/góiUpdate là bản bàn giao để kiểm trênWindows, chưapush/publish.

Khôi phục binary là công cụ cứu hộ/cài lại FullSetup, không tựrollback exe khi processkhônglên. Restore dữliệu là thao tácrõràng có checksum/staging/safetybackup và chờappđóng. Ghi nhậnHealthy chỉfirstpaint. Backup512MB,140MB/tệp,10000tệp; bản cũthiếu manifest khôngautorestored. KhôngmigrationSQL, khôngđổiKEY/backend. Schema23 có.pre-v1.45.bak.

Pixelbacking giữ nội dung gốc; OCR chưa ra khôngtự thànhtext cónghĩa nhưng vẫn ở nền/nhóm. Chỉ vùng cụthể sửa mới vẽlạibằng thuộc tính hiệncó. Font từbitmap cònước lượng; nền chỗcũ khidi chuyển còncần ràsoát. Không thểbịa phục hồi chữ từ ảnhphẳng đãmất.

Đóng gói từthưmục cótênriêng mỗi lượt, stamp/đối chiếuDLL trực tiếpvớipayload, runtime/models/hash/feed/sourceZIP. Báocáo cuối và kíchthước được thêm trong sourceZIP sau khi build. Các báo cáo V144/V1441 là lịch sử.
