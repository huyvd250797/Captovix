# V1.43.0 — M5 Hybrid Smart Capture: Validation

Ngày05/11/2026. Host Linuxx64, SDK .NET10.0.100; target Windowsx64/.NET10.0.13 như bản nền.

## Đã xác minh

- 80project kiểm tra đã build:46bộ logic và34bộ WPF/Windows. Hồi quy tổng hoàn tất theo hai phần log; một lượt build bị dừng do publish dùng chung outputPDB đã được build lại và chạy tiếp tuần tự. Không coi lỗi artifactPDB là lỗi runtime app.
- 46bộ logic chạy xong, bao gồm Geometry/Capture bounds/Selection/Editor/Groups/Mockup/ElementScan/Storage/Recovery/Installer/Update/License. HybridCaptureChecks bản mở rộng chạy riêng30PASS; kiểm stable/dynamicpatch, HWND/PID/Z-order, topology, tọa độ âm/crop, DPIpermonitor/legacy, font khi OCR đồng ý, ảnh bị che, stale/corruptsidecar, thay ảnh nguồn, thật sự trao đổi với helper, timeout/kill/cancel và malformedoutput.
- StartupResourceChecks13PASS: tài nguyên không trùng, detach đúng trước parent, ContextMenu/ToolTip root có guard, Hybridworker chạy trước WPF, giữ fallback/log lỗi startup độc lập.
- Check-M4-UI.py11PASS: menu/cây/chuột phải đúng nguồn, theme/viền, footer và hook close.
- Đọc UI đặt busy/Hủy trước await để đóng app hủy đúng phiên; helper có watchdog5giây phòng provider treo khi app cha bị đóng cưỡng bức.
- Main app và34Windows fixtures cross-build thành công, không báo lỗi compile. HybridPopupWindowsChecks có kiểm lặp menu/tooltip/submenu trong light/dark, đọc UIA button thật, tọa độ physical, menuCanvas chọn đối tượng và Send-to-back/Undo.
- Runtime/dependency/backend/licenseconfig/installerarguments giữ nền1.42; thay desktopversion/title và thêm worker branch trước WPF. Editor schema tối đa21, UI sidecar riêng, không có migration mới.

## Gói phát hành

- Publish self-contained Windowsx64; Velopack1.2.161 directive[win]; NSIS3.09 theo installer gốc. Giữ .NET10.0.13, OCRnative và mô hình; bỏ PDB/XML tài liệu và copyresidue theo Prepare-Payload.py.
- Verify-Packages.py kiểm CRC/SHA feed, nupkg, runtime/OCR/icons/installer/sourcebyte và backendlicense giữ nguyên. Báo cáo gói sau đóng gói được bổ sung vào sourceZIP dưới PACKAGE-VALIDATION-V143.txt.
- Source/bộ cài/gói cập nhật chưa push hoặc publish stable/Latest. Không ký số Captovix bằng chứng thư thương mại.

## Giới hạn nghiệm thu

**Chưa chạy ứng dụng/cài đặt/UIA thật trên Windows trong phiên này.** Các fixture WPF mới chỉ compile, không được tính là native PASS. Cần chạy docs/WINDOWS-ACCEPTANCE-V143.md trên Windows10/11, nhiều màn hình/DPI, upgrade từ1.42 trước phát hành rộng.

Không có API Windows chụp UIA+bitmap nguyên tử. Bản này lấy ảnh/khung cửa sổ trước, đọc UIA qua helper, so cửa sổ và patchpixel sau; chỉ dùng phần ổn định, rồi so lại với screenshot/OCR ở Editor. Khung proof20MP/256nodes/helper1.4giây/độ tuổi3giây; ảnh retained có thể chọn crop sau đó vì dữ liệu được buộc với pixel ảnh đã giữ. Native window output khác desktop hoặc đổi kích thước → fallback ảnh/OCR.

UIAName không tự thay chữOCR; font cần OCR đồng ý và font phải có trên máy để render giống. Ảnh cũ/nhập/customcontrol/providerlỗi vẫn dùng scanảnh. Giới hạn mô tả chi tiết trong README; chưa đo RAM/p95UI trên cùng máy Windows trước/sau và không cam kết độ trễ bằng0.
