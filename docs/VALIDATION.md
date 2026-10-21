# Validation — Captovix V1.5.1

Source nền: Captovix-v1.5.0-Source.zip. .NET SDK 10.0.100, Windows runtime 10.0.13, ScreenRecorderLib 7.0.1, Velopack SDK/CLI 1.2.161 giữ nguyên. Không đổi schema ảnh/capture workflow.

## Nguyên nhân và thay đổi

UnloadEditor gọi Surface.Load(null,null) trước khi gán _session=null. Load phát VisualChanged đồng bộ; callback cũ dùng _session để suy đoán Surface.Layout khác null và dereference null. V1.5.1 dọn phiên trước khi clear surface, đồng thời guard cặp session/layout/ảnh trong readout và bảng appearance. Không nuốt exception để che lỗi và không reset dữ liệu.

## Kiểm chứng

Bộ logic/tĩnh giữ 403 ca; SDK trên nupkg thực tế thêm 9 ca, tổng 412. Build app và harness WPF, publish win-x64 self-contained, compile NSIS, kiểm tra package/feed/hash/size/CRC, DLL và URL mặc định. Đây không phải kết luận UI Windows đã chạy thành công.

Bổ sung 11 ca Windows regression trong EditorWindowsChecks: chính trạng thái stale-session/empty-layout của lỗi báo về; readout/property rỗng; reload/unload lặp; chuyển hai ảnh khác kích thước qua ListBox thật; lưu sidecar; detach session cũ; 30 vòng đổi ảnh; bỏ chọn; chọn row mới; giữ kích thước ảnh gốc. Harness chỉ biên dịch trên Linux; chưa thực thi WPF/chuột/clipboard/video/tray hoặc installer/updater Windows thật. Checklist Windows đi kèm để nghiệm thu.

Giới hạn: chưa chạy end-to-end bản vá trên máy Boss. Chưa publish GitHub Release. Chưa ký Authenticode. Không triển khai V1.6 trong hotfix này.
