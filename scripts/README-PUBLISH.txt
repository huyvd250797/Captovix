Captovix V1.68.3 — GitHub Actions Build & Packaging Fix

Chỉ publish stable/Latest sau khi nghiệm thu Windows theo docs/V1.68.3-VALIDATION.md và các kiểm tra hồi quy hiện có. Gói chỉ được phát hành khi Build-Windows.ps1 hoàn thành và package-verification.json là PASS. CI không thay thế nghiệm thu Desktop.
Để cài: đóng Captovix cả khay, chạy HuyVo.Captovix-win-Setup.exe vào thư mục hiện có; không gỡ app/xóa dữ liệu/KEY. Không migration mới, không deploy backend.

Release stable v1.68.3 cần đủ 5 tệp rời, không chỉ upload ZIP:
HuyVo.Captovix-1.68.3-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json

Giữ update-source.json hiện có. Source ZIP/Full Setup và Update-Packages là các gói khác nhau; source ZIP không cài được app.
