# V1.45.0 — Native Windows acceptance (PENDING)

Không coi cross-build/logic là nativePASS. Windows10/11x64, light/dark,100/125/150/200%DPI; test trên bản sao dữ liệu, không dùng trial/keykhách hàng.

1. Cài mới và cài đè1.44.1, đường dẫn hiện có/D:, reboot, khởi động bằngshortcut/tray; license, thư viện, cấu hình không bịreset. Nguồn ảnh/video hashgiữ nguyên. Chạy script Windows-Sandbox-Smoke.ps1 trong Sandbox cho firstpaint; lưu exitcode/log và runtimefilehash.
2. Source85project:48pure+37WPF. Run-Checks.py trênWindows phải chạy các bộnative, gồm ReleaseQualityWindowsChecks (PNGsnapshot pixel-identical, ApplyAll với OCR/font sai không đổi pixel, save/reopen giữpixel, sửa một label không làm thay đổi bên ngoài bbox). Có thể truyền đường dẫn ảnh thật qua dotnet run --project tests/ReleaseQualityWindowsChecks -- <file.png>. Không đính kèm ảnh riêng tư vào báo cáo công khai.
3. Ảnh gốc Boss cung cấp: so100%trongapp/config/review, màn hìnhDPI100/125/150/200; OCR phải nhận fullwidth/height, không nhận1200px. Vừa khung chỉ đổi hiển thị; zoom/crop/chọn vùng không đổi tọa độSource. Bấm Áp dụng trước khi sửa phải giữ nguyên mọi chữ/giátrị/icon/bảng; thử saiOCR cố ý.
4. Mở sidecar cũđangnátảnh → thôngbáo dùngnguồn → Scan/Apply giữ dữ liệu thật; Undo trởlại trạngthái trước. GiữprivacyCover/Blur, nguồn bị flattenmấtchữ không được bịa lại dữ liệu.
5. Di chuyển nhóm/phần tử, resize/rotate, hide/delete, copy/paste/duplicate, undo/redo/save/reopen; không làm mất phần chưaOCR. Nhóm/canvas có thể mở rộng để mang cả lớp nền. Nền cũphứctạp đánh dấu/ràsoát, chưa có inpaintchínhxácmọinền.
6. Cuộn MediaList thật300+ảnh: wheel±120 từngnấc, delta nhỏ, dọc/ngang, cảCtrl/Shift, scrollbar/trackpad, chuyểnlayout. Không nhảycuối. Đốichiếu ScrollUnittrênListBox; vừa chuyển/scroll không làm chọnảnhkhác.
7. Update: tải thiếu/saiSHA/corrupt/thiếuDLL/saiAppId/version/qualitystamp/cacheđổi sauReady → khônghandoffupdater, có tải lại. Backup lỗi/đóng khi backup → chưaapply, app phục hồi thao tác. Nâng từ1.44.1 tới1.45 actualpackage.
8. Backupmanifest/hashes, traversal/reparse/missingentry/quágiớihạn → từchối trướcghi. Restore sauđóngapp, có bảnan toàn, lỗi giữa chừnghoàntác; tệpnguồn/mới ngoài danh sách khôngxóa. DPAPIcùngmáy còn hoạtđộng. PhiênbảnEditor>23khôngghiđè.
9. Giảlập Theme/DLLlỗi: Recovery.cmd vẫnmởđộc lập; chẩnđoán không kèmảnh/KEY; chọnFullSetupđúng nguồn đểcàilại. Khôngtựlùi binary hoặc tựrestore dữliệu cũ khingười dùng chưa chọn.
10. Cậpnhật bìnhthường: khôngbáolầnnàybịlỗi; Applying→Starting→Healthy saufirstpaint. Tắttrướccửa sổ→lầnmởsau cóchẩnđoán. Healthy khôngđược dùng để kếtluậntoànappPASS.
11. ĐoRAM/idle/peak vàp95UI cùngmáy với4K/20MP,200khung,nhiềuảnh; ảnhgốc/export khôngđổi độphân giải. Temp1ngày/cache cóthểdọn, backup/nguồn/KEYkhôngdọnngầm.

Ghi máy/OS/DPI/log/version cũ/kếtquả từng mục. Sau khi tất cả mục thật sựPASS mới đặt manualAcceptance=PASS trong artifacts/windows-acceptance.json và publishstable. Không tự tạo báo cáoPASS ở Linux.
