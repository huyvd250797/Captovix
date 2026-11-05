# Captovix V1.45.0 — Update, Recovery & Release Quality

Nâng từ đúng ZIPV1.44.1 Boss gửi. Bản này sửa cuộn thư viện, ảnh Mockup mờ và lớp quét làm mất chữ; đồng thời bổ sung kiểm gói, backup, khôi phục và cứu hộ. Chưa push/phát hành GitHub Releases.

## Cài đặt

Đóng Captovix cả khay, chạy Captovix-v1.45.0-Full-Setup.exe vào vị trí đang cài. Giữ dữ liệu/license/config như nền cũ. FullSetup không tự lùi phiên bản hay xóa thư viện. Không cần chạy migration mới/Supabase/backend. Nghiệm thu Windows thật vẫn đang chờ: xem docs/WINDOWS-ACCEPTANCE-V145.md trước phát hành rộng.

## Mockup rõ ảnh và giữ nội dung

1. Mở ảnh → Mockup → Quét vùng hoặc toàn ảnh. Mặc định100%pixel thật theoDPI; dùng cuộn/phóng to để đọc chữ, Vừa khung để xem toàn bố cục. Nguồn không bị nén mất dữ liệu hoặc thu xuống1200px; hình vừa khung có thể nhỏ về thị giác nhưng OCR vẫn nhận pixel nguồn đầy đủ.
2. Quét dùng vùng8×8px tới20MP. Config có thể tham chiếu ảnh gốc lớn mà không nhân bản toàn bitmap; ảnh đã chỉnh vượt20MP cần Crop vùng nhỏ trước để tránh dựng một bitmap quá lớn. Crop có thể hoàn tác.
3. Rà soát/chọn tất cả/áp dụng: **giữ hình ảnh gốc**. Một PNG chung giữ toàn nội dung; các khung là phần tử có thể chọn. OCR sai/không nhận ra không còn làm chữ biến mất ngay khi áp dụng. Nội dung chưa nhận diện đi cùng lớp nền/nhóm.
4. Double-click phần tử chữ OCR để sửa riêng, thuộc tính bên phải. Pixel nguồn dùng đến khi thực sự thay nội dung/kiểu; phần được sửa chuyển sang text/control. OCR/font ước lượng có thể cần sửa thủ công một lần; không khẳng định mọi chữ đều tự thành text đúng. Không thể tự sửa nội dung chưa nhận ra bằng text nếu chưa thêm khung/OCR thủ công.
5. Di chuyển cả nhóm giữ cả vùng ảnh/biểu tượng/bảng/nội dung chưa nhận ra. Vào nhóm hoặc double-click để chọn/sửa phần riêng. Chọn lớp nền chỉ để di chuyển nhóm; không đổi font/màu cả nền. Nền vị trí cũ khi di chuyển vẫn có thể cần rà soát ở Nền & mặt nạ, nhất là nền phức tạp.
6. Nếu mở ảnh từng bị quét kiểu cũ1.44.x, config thông báo dùng lại ảnh nguồn bên dưới các lớp quét cũ. Chỉ loại lớp quét cũ khi Áp dụng, Undo phục hồi trạng thái trước. Các vùng che thông tin không bị bỏ. Nếu chỉ còn ảnh xuất phẳng đã mất chữ, app không thể khôi phục nội dung không còn trong file.

Schema23 mới chỉ khi dùng lớp Mockup giữpixel. Lần lưu nâng cấp có `.pre-v1.45.bak`; bản cũ tối đa22 không đọc được23. Giữ bản sao dự án khi muốn quay lại bản cũ. Không có SQL/migration phải chạy.

## Cuộn thư viện

Cả dọc và ngang dùng lệnh cuộn native của WPF, không tự cộng48 đơn vị vào danh sách đang đếm theo phần tử. ScrollUnit đặt trên chính ListBox. Giữ virtualization và chọn nhiều ảnh; Ctrl+C/FileDrop xuất PNG đã chỉnh lần lượt, temp24giờ giữ nềnV1.44.1. Ứng dụng nhận phải hỗ trợ dán/thả danh sách tệp.

## Cập nhật và khôi phục

Luồng: kiểm feed → tải/kiểmSHA256 → kiểm thành phần/version/bản đối chiếu gói → sao lưu nền → kiểm lại cache → bàn giao updater → theo dõi lần mở cửa sổ đầu tiên. Gói bị thay/saihash/thiếuDLL/khácAppId bị chặn, có thể tải lại. `release-quality.json` là bản đối chiếu nhất quán, không phải chữ ký nhà phát hành.

Trong **Trợ giúp → Cập nhật & khôi phục dữ liệu**, chọn bản backup → kiểm → xác nhận → app đóng → helper khôi phục → mở lại. Backup có hash/tệpwhitelist; kiểm trước ghi, staging và bản an toàn trước restore. Restore không xóa tệp mới ngoài danh sách; bản an toàn giữ dữ liệu hiện có để quay lại. Ảnh/video nguồn không nằm trong backup này và không bị thay.

Backup giữ metadata, EditJSON/backup, thiết lập và `licensing/device.bin`/`certificate.bin` đã mã hóaDPAPI. License chỉ dùng trên cùng máy/tài khoản, không đổi quyền/trial/server. Giới hạn512MB/bản,140MB/tệp,10000tệp; nếu vượt hoặc thiếu quyền/dung lượng, cập nhật trongapp dừng để giữ phiên cũ. Có thể sao lưu dự án riêng và dùng FullSetup cài đè. Backup cũ không có manifest vẫn được giữ nhưng không tự restore đè; cần kiểm thủ công.

Nếu app không mở được, chạy **Recovery.cmd** trong thư mục chứa Captovix.exe: công cụ PowerShell/WinForms không phụ thuộc theme/DLL app, mở backup, xuất chẩn đoán và chọn FullSetup tin cậy cài lại. Binary rollback không tự động; dùng bộ cài đã kiểm chứng và backup dữ liệu tương thích. Không ghi đè schema mới bằng app cũ.

Nhật ký mở cửa sổ chỉ chứng minh firstpaint; không thay thế kiểm chụp/quay/OCR/IME/scroll. Không tự gọi update thành công khi mới tải xong.

## Xác minh và giới hạn

HostLinux kiểm logic và cross-build WPF; **chưa chạy Setup/app trên Windows thật**. Script `Windows-Sandbox-Smoke.ps1` chỉ cho chạy trong WindowsSandbox và ghi báo cáo cài/mở cửa sổ. Chạy thêm toàn bộ nativefixtures và checklistV145; smoke cửa sổ không đủ nghiệm thu.

Publish-GitHub.ps1 yêu cầu artifacts/windows-acceptance.json đúng version/platform/windowVisible và manualAcceptance=PASS sau khi kiểm thật. Bản bàn giao chưa có bằng chứng này, chưa publish stable. Không giảm chất lượng ảnh để giảmRAM; lịch sử/cache/bản xem được giới hạn, mỗi vùng chỉ một PNG, không nhân bitmap cho mỗi chữ. Chưa đo peakWindowsRAM/p95UI trên máy Boss.

Tiếp theo đề xuất **V1.46.0 — Mockup Fidelity & Editing Reliability**: hiệu chỉnh trên bộ form thực tế, font/định dạng khi sửa, nền sau di chuyển và hiệu năng dự án nhiều vùng quét. Chưa tự bắt đầu phiên bản này.

## Dựng lại từ source trên Windows

Yêu cầu Windows10/11x64, .NET10SDK, Python3, PowerShell, NSIS3.x (nếu dựng FullSetup), mạng để khôi phục NuGet. Runtime tự chứa10.0.13 đã ghim trong csproj. Thư mục `artifacts`, `bin`, `obj` được tạo lại, không nằm trong sourceZIP.

Giải nén ZIP, mở PowerShell tại thư mục có `README.md`, `scripts` và `src`. Giữ nguyên cấu hình public license ở `src/Captovix/license-config.json` và `update-source.json`. Không thay issuer/public key, không chép private key, service role hay admin token vào desktop.

```powershell
dotnet --info
dotnet tool restore
dotnet restore src/Captovix/Captovix.csproj
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
python scripts/Run-Checks.py
```

Build phải trả `0 Error(s)`; mở thử và làm checklistWindows chứ không chỉ dựa vào build. Khi cài app với license chưa hoạt động, một số công cụ được khóa như phiên bản trước; dùng license/trial hợp lệ, không bypass kiểm tra.

Dựng bộ cài: cài NSIS, đặt Microsoft Visual C++Redistributablex64 chính thức tại `prerequisites/vc_redist.x64.exe` (thư mục/tệp này không kèm trong source). Chạy script; script chạy kiểm tra trước publish, tạo artifacts/win-x64, FullSetup và Update-Packages.

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
python scripts/Package-Source.py
```

Chỉ publish để kiểm binaries, chưa tạo installer:

```powershell
dotnet publish src/Captovix/Captovix.csproj -c Release -r win-x64 --self-contained true -p:Platform=x64 -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o artifacts/win-x64
```

Lỗi restore: kiểm mạng/proxy/NuGet, cài SDK10; không đổi xuống runtime cũ chỉ để bỏ lỗi. Lỗi thiếu VC++: cài redistributable Microsoft đúngx64. Lỗi khởi động: giữ log/report trong thư mục dữ liệu app, gửi version và stacktrace; không xóa thư viện/KEY để thử. Lỗi DLL sau tự build: dùng đầy đủ thư mục publish/installer, không chép riêng Captovix.exe. Lỗi dung lượng: dọn cache/temp qua công cụ app hoặc thư mục build có thể tạo lại; không xóa ảnh nguồn/sidecar/backup.

## GitHub và phát hành

Bản bàn giao chưa push hoặc tạo GitHub Release. Với repo mới: tạo repo GitHub rồi `git init`, `git add .`, `git commit`, `git branch -M main`, thêm remote đúngrepo và push. Với repo có source: clone/fetch/pull nhánh trước, tích hợp source bản này, xem `git diff`, commit/push; không force-push để bỏ lỗi fetchfirst. Kiểm tra .gitignore và không push prerequisites/artifacts/privateconfig.

Captovix là app desktopWPF, không deploy binaries lên Vercel/Netlify để chạy như web. Backend Supabase/licensing đang dùng giữ nguyên. Sau nghiệm thu Windows, xem `scripts/README-PUBLISH.txt`, upload đúng5assets từ Update-Packages lên GitHub Releases tagv1.45.0. Không chỉ upload ZIP. Kiểm tra source/update/feed URL và thử update máy test trước stableLatest.

Không cần chạy migration mới từ1.44.1. Giữ bản sao thư mục dữ liệu/dự án khi nâng cấp; schema22 có backup trước ghi mới. Nếu quay lại bản cũ, dùng bản sao/backup tương thích, không ép đọc hoặc ghi đè schema22. Gửi lại chính sourceZIP mới này để tiếp tục V1.45.0.
