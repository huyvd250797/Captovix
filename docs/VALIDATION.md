# V1.42.0 — Element Recognition & Fidelity: kiểm tra ngày04/11/2026

## Kết quả đã chạy

- Host Linuxx64, SDK .NET10.0.100; target net10.0-windows/win-x64 self-contained .NET10.0.13 như source nền. Không thay dependency/runtime.
- Run-Checks.py: 77 bộ đã build thành công; 33 bộ WPF/Windows chỉ cross-build. Các bộ logic còn lại chạy xong,2311PASS tại lần hồi quy tổng; trong đó ElementScanChecks lúc đó35PASS.
- ElementScanChecks mở rộng chạy riêng42PASS: tách text/control/nhóm, nguồn không đổi, liên kết input, source offset, Apply/Undo/Redo/lưu/mở lại/copy/duplicate, giới hạn/cancel, style/font/viền/bo góc, nhóm khối–con, tái sử dụng phân tích hình học.
- StorageMaintenanceChecks chạy riêng8PASS: đếm nguồn không trùng, dọn đúng bytes, giữ originals/edits/license ngay cả category giả, giữ tệp mới/đổi/chưa scan, không đi qua symlink, hủy scan, thư mục thiếu. Hai ca symlink chạy Linux; trên Windows bỏ qua ca tạo symlink vì quyền hệ điều hành.
- StartupResourceChecks chạy11PASS: không resource key trùng, detach trước parent, updater/OCR worker trướcWPF, fallback/log lỗi startup độc lập. StartupWindowsChecks cross-build thành công; thêm kiểm một viền1DIP và nội dung không còn viền thứ hai.
- Check-M4-UI.py:11PASS sau đổi tài nguyên viền. Main app build/publish không lỗi. ElementScanWindowsChecks cross-build với kiểm canh ink OCR để tránh cắt chữ do line-height; chưa thực thi WPF.
- Lấy màu cho khung1500×900 trên ảnh1600×1000: cấp phát 1,216 bytes trong fixture đơn sắc, thay vì tối thiểu5,477,056bytes cho bản sao vùng như source nền. Không đồng nghĩa mọi ảnh hay RAM toàn app giảm cùng tỷ lệ.
- Trần cache raster256→96MiB; thumbnail16MiB. Không hạ chất lượng ảnh gốc và không ép GC trên UI.

## Gói và dung lượng

- Dùng publish Windows, Velopack1.2.161 directive [win] và NSIS3.09; bộ cài giữ nguyên logic thư mục/đối số của1.41.1, chỉ tăng version.
- Prepare-Payload.py bỏ PDB/XML tài liệu và copy residue có bản chính; giữ WPF/native DLL/OCR/license. STORAGE-AUDIT-V142.json ghi kích thước, nhóm trùng chỉ để review; không tự gộp DLL khác đường dẫn.
- PAYLOAD-CLEANUP-V142.json ghi raw publish trước/sau dọn. Đây là so sánh cùng bản1.42 trước/sau đóng gói, **không phải số đo bộ cài1.41.1 hoặc máy Boss** vì đầu vào chỉ có source ZIP.
- Kiểm CRC/SHA feed/nupkg/payload/source/backend giữ nguyên và bộ cài thực hiện bởi Verify-Packages.py. Chi tiết kết quả cuối được bàn giao trong báo cáo kiểm gói.
- Không ký số Captovix bằng chứng thư thương mại. Chưa push/publish stable/Latest; không đổi dữ liệu backend và không có migration mới.

## Còn phải xác nhận trên Windows thật

**Chưa chạy app/cài đè/UIA/capture/record/OCR native trên Windows trong lần này.** Không khẳng định đã hết mọi lỗi mở app. Các fixture startup/render có trong source, Build-Windows.ps1 yêu cầu chạy chúng trên Windows trước phát hành; checklist docs/WINDOWS-ACCEPTANCE-V142.md.

Font chỉ là ước lượng các font UI có sẵn theo khung ink OCR; raster không chứng minh được font gốc. Nhận diện ưu tiên nền phẳng, còn giới hạn200khung/500dòng OCR/120đối tượng tạo mỗi lần. Ảnh/gradient/control phức tạp và font đặc biệt cần rà soát.

Chưa có số đo working set/private bytes hay p95thao tác trên cùng máy Windows trước/sau. Không cam kết mọi thao tác không độ trễ; đã giảm cấp phát/lặp phân tích và đưa scan/dọn nền trong phạm vi thay đổi.
