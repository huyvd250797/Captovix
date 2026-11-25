Captovix V1.69.0 — Multi-Pointer Callout & Dynamic Version

Chỉ publish stable/Latest sau khi nghiệm thu Windows theo docs/V1.69.0-VALIDATION.md và các kiểm tra hồi quy hiện có. Build-UpdateCandidate.ps1 tạo bản ứng viên; Build-Windows.ps1 chạy full regression. Nghiệm thu trước khi publish.
Để cài: đóng Captovix cả khay, chạy HuyVo.Captovix-win-Setup.exe vào thư mục hiện có; không gỡ app/xóa dữ liệu/KEY. Không migration mới, không deploy backend.

Release stable v1.69.0 cần đủ 5 tệp rời, không chỉ upload ZIP:
HuyVo.Captovix-1.69.0-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json

Giữ update-source.json hiện có. Source ZIP/Full Setup và Update-Packages là các gói khác nhau; source ZIP không cài được app.
