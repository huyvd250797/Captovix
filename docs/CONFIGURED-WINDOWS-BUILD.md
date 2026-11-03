# Cài đặt Captovix V1.29.1 đã cấu hình license

## Đã làm

Nhúng file `license-config.json` công khai Boss gửi vào assembly Windows. Key RSA 3072-bit nhập được; issuer `captovix-production`; API health và ready trả HTTP 200. Không tạo lại authority, không tạo key/trial mới trên production, không thay dữ liệu. Admin/config.json trong source vẫn là mẫu — không upload đè trang admin đã cấu hình.

## Nâng cấp trực tiếp từ V1.27.2

1. Lưu ảnh/công việc đang sửa, kết thúc quay và thoát Captovix (kể cả khay hệ thống).
2. Chạy `Captovix-v1.29.1-Setup.exe` bản mới bàn giao. Chọn đường dẫn bản đang cài, ví dụ `D:\Captovix\Captovix`. Không uninstall và không xóa dữ liệu.
3. Mở app: vào Trợ giúp → Bản quyền & khôi phục. Chọn Dùng thử 30 ngày hoặc nhập key trên trang admin → Kích hoạt key.
4. Kiểm tra footer và các công cụ được mở. Đối chiếu máy đã kích hoạt trên trang quản trị. Không gửi key/recovery code cho hỗ trợ.
5. Nếu lỗi: ghi lại thông báo và version. Trạng thái chưa kích hoạt chỉ mở chụp vùng; dữ liệu ảnh/chú thích được giữ. Không xóa license cache hoặc tạo signing key khác để chữa lỗi.

Không cần chạy migration mới. Không deploy lại admin/Edge Function trong lần cập nhật Windows này.

## Cập nhật trong app V1.27.2

Giải nén Update-Packages.zip, upload đủ năm assets vào GitHub Release mới tag `v1.29.1`, đánh dấu release ổn định/latest. Gồm full.nupkg, canonical `HuyVo.Captovix-win-Setup.exe`, RELEASES, assets.win.json, releases.win.json. Sau đó mở V1.27.2 → Kiểm tra cập nhật → tải/cài theo thông báo. Source ZIP không phải gói update; không upload ZIP thay các assets.

Nếu đã có Release review V1.29.1 được phát hành/cài cho người dùng, không thay bytes cùng version và mong updater phát hiện; cần tăng version. Bản đang dùng V1.27.2 có thể nâng cấp trực tiếp, không cần cài từng bản trung gian.

## Source để nâng cấp sau

Cấu hình công khai đã ở `src/Captovix/license-config.json`; không đưa private key/service role vào source. Build lại bằng scripts/Build-Windows.ps1 hoặc workflow Windows trong GitHub Actions. Giữ issuer và public key trùng authority đang có trên Supabase. Các hướng dẫn cũ về trạng thái review là lịch sử; bản build cấu hình này dùng file Boss cung cấp.

## Giới hạn xác minh

Linux cross-build/publish và kiểm tra dữ liệu nhúng không thay kiểm thử Windows thật. API ready không chứng minh chữ ký server khớp public key này; phải nhận chứng nhận qua luồng dùng thử/kích hoạt trên Windows để xác nhận toàn tuyến. Chưa chạy Setup/DPAPI hoặc dùng key khách hàng tại môi trường phát triển. Không push GitHub hay tự đăng Release.
