# Captovix V1.44.0 — M6 Advanced UI Reconstruction

Nâng trực tiếp từ source V1.43.0 Boss cung cấp. Mục tiêu: quét form → rà soát các phần chưa chắc → tạo đối tượng có cấu trúc, sửa từng phần hoặc di chuyển nhóm.

## Dùng bản này

1. Cài Full Setup để cài mới/cài đè; giữ dữ liệu, license và thiết lập. Chưa publish GitHub Releases. Các file Update-Packages chỉ dùng khi chủ app phát hành đúng hướng dẫn kèm ZIP.
2. Chụp trực tiếp với Hybrid bật, hoặc mở ảnh cũ như trước. Chọn **Thêm → Mockup → Quét vùng/toàn ảnh**.
3. Rà khung/quan hệ/cấu trúc, chọn phần cần chuyển. Font từ ảnh là ước lượng: kiểm tra tên font, cỡ px, đậm/nghiêng/gạch chân, màu, rồi đánh dấu **Đã xác nhận định dạng chữ**. Nếu chưa chắc hoặc nền phức tạp, chọn **Giữ dạng ảnh gốc**. Chọn kết quả rõ không tự chọn chữ chưa xác nhận. Phần giữ ảnh không sinh đối tượng thay thế, pixel nguồn vẫn nằm trong ảnh nền.
4. Xem trước, đối chiếu ảnh gốc và Áp dụng. Chữ của nút/ô nhập được tách khỏi hình control, giữ cùng nhóm. Nhóm hàng/cột/khu vực không tự thay đổi tọa độ; có thể vào nhóm để sửa riêng hoặc di chuyển cả nhóm.
5. Chọn đối tượng Mockup: thuộc tính nằm bên phải. Double-click chữ để gõ ngay trên ảnh. Có đầy đủ font/cỡ/B/I/U, căn chữ/padding/giãn dòng, màu chữ/nền che/nền control/viền, dày viền/bo góc/loại, OCR/Dừng, chọn màu màn hình và bản so sánh nhỏ. Nền & mặt nạ giữ công cụ chuyên biệt hiện có.
6. Đổi text không thay cỡ/font/màu/định dạng. Ví dụ Times New Roman14px, xanh, nghiêng sẽ giữ các thuộc tính đó qua Undo/Redo/lưu lại. Font cần được cài đúng trên máy; không khẳng định có thể suy ra chính xác mọi font chỉ từ bitmap.

## AI/model tùy chọn

Mở **AI cấu trúc** trong cửa sổ rà soát. Chạy dịch vụ Ollama vision riêng, nhập endpoint mặc định `http://127.0.0.1:11434`, bấm **Model đã cài**, chọn model có vision, lưu cấu hình và **Phân tích AI**. Endpoint HTTPS tùy chọn cần tương thích `/api/tags` và `/api/chat`; không có API key UI trong bản này. Chưa có model mặc định đã nghiệm thu: xem `docs/MODEL-EVALUATION-V144.md`.

Chỉ nút phân tích mới gửi bản xem vùng quét (cạnh dài tối đa1536px). Ảnh nguồn/capture/export giữ pixel gốc. Model trả cấu trúc/khung; OCR là nguồn chữ, AI không thay chữ bằng nội dung tự sinh. Gợi ý AI mặc định giữ dạng ảnh và không tự Áp dụng. Hủy/lỗi/không có dịch vụ vẫn dùng ảnh/OCR. Không cài thêm trọng số vào bộ cài. Giới hạn100gợi ý/model, tổng200khung và tối đa120phần tử Áp dụng như nền cũ.

## Bộ nhớ, dung lượng và dữ liệu

- Undo giữ chung PNG/mặt nạ bất biến giữa các bước, nén dữ liệu thuộc tính; tối đa100bước và ngân sách mặc định16MB cho lịch sử, vẫn giữ ít nhất một bước dù bản thân ảnh lớn hơn ngân sách. Bộ nhớ toàn tiến trình còn gồm WPF, ảnh đang mở, OCR/native và quay video: không đồng nghĩa app luôn dưới16MB.
- Raster cache48MiB, thumbnail16MiB. Bản xem so sánh chỉ dựng khi yêu cầu, nhỏ480px và xóa khi đổi đối tượng. Không nén mất dữ liệu/giảm chất lượng capture để giảm RAM.
- Giữ trang Dung lượng & bộ nhớ: xem ảnh nguồn, chỉnh sửa, thumbnails/cache/temp, chọn dọn cache/temp an toàn. Không tự xóa ảnh gốc, project, backup hoặc khóa bản quyền. Model chạy ở dịch vụ riêng cũng tiêu thụ RAM/VRAM; dùng chế độ ảnh/OCR khi máy thiếu tài nguyên.
- Không có migration Supabase mới; không đổi backend/KEY/runtime/OCR. Editor tối đa22 chỉ khi semantic role mới được lưu; trước nâng cấp sidecar có backup `.pre-v1.44.bak`. Bản app cũ không đọc được schema22; giữ bản sao dự án trước khi quay lại bản cũ.
- Hybrid vẫn buộc metadata vào hash/pixel nguồn, chuẩn hóa DPI/crop/màn hình như V1.43. Nguồn không có UIA dùng quét ảnh. Bản nhận diện nâng cao có fallback; chưa cam kết tái dựng hoàn hảo mọi form.

## Build, kiểm thử và phiên bản kế tiếp

Build Windowsx64/.NET10 bằng `scripts/Run-Checks.py`, `scripts/Build-Windows.ps1`, `scripts/Package-Source.py`; xem `docs/VALIDATION.md`, `docs/WINDOWS-ACCEPTANCE-V144.md`. WPF/installer cần chạy trên Windows thật trước phát hành rộng; Linux chỉ cross-build các phần đó.

Tiếp theo: **V1.45.0 — Update, Recovery & Release Quality**: củng cố cập nhật/cài đè/khôi phục, nghiệm thu Windows và đo độ trễ/RAM trên cùng máy. M6 tiếp tục hiệu chỉnh theo bộ form thực tế sau nghiệm thu.

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

Captovix là app desktopWPF, không deploy binaries lên Vercel/Netlify để chạy như web. Backend Supabase/licensing đang dùng giữ nguyên. Sau nghiệm thu Windows, xem `scripts/README-PUBLISH.txt`, upload đúng5assets từ Update-Packages lên GitHub Releases tagv1.44.0. Không chỉ upload ZIP. Kiểm tra source/update/feed URL và thử update máy test trước stableLatest.

Không cần chạy migration mới từ1.43.0. Giữ bản sao thư mục dữ liệu/dự án khi nâng cấp; schema22 có backup trước ghi mới. Nếu quay lại bản cũ, dùng bản sao/backup tương thích, không ép đọc hoặc ghi đè schema22. Gửi lại chính sourceZIP mới này để tiếp tục V1.45.0.
