# Captovix V1.24.0 — Capture Precision

- Chụp vùng: kéo → chỉnh cạnh/góc nếu cần → Enter hoặc nút máy ảnh là chụp, bỏ bước chọn hành động dư. Nét đứt chuyển động chậm báo vùng chưa chụp. Mũi tên cuộn dọc/ngang nằm ngay vùng chọn, nền trong suốt; video chỉ có xác nhận vùng. Esc/chuột phải hủy.
- Giữ nền capture pixel và kiểm topology/DPI sau hẹn giờ. Chọn phần tử cha/con bằng con lăn, ưu tiên cạnh/góc modal/cửa sổ; vùng cố định/gần đây kế thừa nền trước. Độ tương thích UIA phụ thuộc ứng dụng; không cam kết nhận diện mọi UI.
- Quản lý thư viện: cây năm/tháng/ngày, ảnh dạng ô tự xuống dòng theo chiều rộng; tải100ảnh/lần, nút Xem thêm. Tùy chọn Ẩn ngày tháng không có hình bỏ cả tháng trống. Ctrl/Shift chọn nhiều, mở/xóa/xuất giữ nguyên. Bỏ đường vào màn hình quản lý bộ sưu tập cũ; dữ liệu tag/collection giữ, backup/export còn trong menu.
- Chọn công cụ áp dụng mẫu đã lưu gần nhất trong họ công cụ; chọn mẫu cụ thể vẫn ưu tiên mẫu vừa bấm. Đã lưu mở sẵn; sao trên preview chỉ hiện hover/focus. Sắp xếp công cụ bằng kéo-thả trên thanh công cụ hoặc Hiển thị → Sắp xếp không gian → Sắp xếp công cụ cá nhân; thứ tự giữ trên máy.
- Callout vẽ kéo giữ nguyên thân khi thả, tắt tự co chữ cho đối tượng mới để nhập không đổi thân. Bật Tự co trong thuộc tính nếu muốn. Một click giữ kích thước mặc định. Tay nắm đuôi màu cam, tay nắm thân xanh/trắng dạng tròn. Đuôi vẫn tự do.
- Footer/nút update gọn hơn. Giới thiệu ghi: Ý tưởng, định hướng sản phẩm và thiết kế trải nghiệm: Võ Đức Huy (HuyVo). ©2026HuyVo. All rights reserved.

## Cài/update

Giải nén Update-Packages, đóng app và chạy HuyVo.Captovix-win-Setup.exe, hoặc đăng đủ5asset lên GitHub Release stable/latest tagv1.24.0 để app kiểm tra cập nhật. Không đăng chỉ file ZIP. Source ZIP dùng phát triển, không dùng cài. Không tự publish trong lần bàn giao này. Setup chưa ký số.

## Giới hạn và nghiệm thu

Build/publish Windows x64 tự chứa.NET10.0.13; giữ stack/logo/update source/schema và dữ liệu. Đã chạy7bộ logic/static liên quan,620assertions, và cross-build RefinementWindowsChecks; chưa chạy WPF/Windows desktop. Kiểm nupkg và integrity gói riêng sau đóng gói.

Cần Windows nghiệm thu:1Enter/chuột/nút camera, hủy/countdown/quay, arrows cuộn, nhiều DPI100–200%và tọa độ âm; ảnh/clipboard/PNG đúng pixel; thư viện co giãn/lazy/empty month/multi-select/delete; chọn mẫu gần nhất sau restart/chọn mẫu thủ công; callout thân/đuôi/IME/Undo; caption/taskbar/footer/install-update. Không có screenshot UI chạy thật hoặc tuyên bố đã kiểm thị giác. Margins cũ vẫn theo giới hạn1.23.1, chữ không kéo méo glyph hai chiều.

## Tiếp theo V1.25 — Scrolling Capture Quality

Cải thiện mối nối dọc/ngang, vùng header/footer cố định, phát hiện khung lặp/lỗi, bộ trang mẫu để kiểm không mất/lặp dòng/cột. Ưu tiên bản vá phản hồi Windows1.24 trước.

## Lịch sử phiên bản trước

# V1.23.1 — Resize & Capture UX ·27/10/2026

Windows x64 publish thành công; RefinementWindowsChecks cross-build0lỗi/cảnh báo. Chưa chạy WPF trên Windows. RefinementChecks291 kiểm tra đạt; PrecisionChecks có kiểm mới free resize; UpdateChecks với gói thật61 kiểm tra đạt. Verifier byte/hash/feed/ZIP chạy sau đóng gói. Các số liệu1.23 bên dưới là lịch sử, không khẳng định toàn bộ suite đã chạy lại ở1.23.1.

Cần nghiệm thu Windows: kéo cạnh/góc từng công cụ và Shift, callout/text xoay, chọn pixel, chụp nhiều màn hình/scale, điều chỉnh vùng sau thả chuột, xóa đối tượng ngoài ảnh/Undo, popup/fly animation, install/update. Mở rộng tự động mới có metadata riêng; margins cũ chưa phân biệt được thủ công/tự động nên không tự thu. Chữ/số dùng font, không kéo méo glyph như bitmap.

# V1.23.0 — Kiểm tra ngày27/10/2026

-29 bộ logic/static,1910 assertions đạt; thêm291 kiểm tra exact-edge canvas/work area/text metrics,4shell contracts. Đây là kiểm logic, không phải kết quả tương tác WPF.
-18 bộ WPF cross-build trên Linux,0lỗi/cảnh báo. RefinementWindowsChecks thêm fixture gutter trong suốt, output pixel không đổi, growth/rendered bounds/Undo, live draft/commit/cancel/export và tail target theo rotation. **Chưa thực thi các fixture WPF trên Windows.**
-Main Windows x64 publish self-contained.NET10.0.13; giữ dependency/runtime/logo/source update và installer core arguments. UpdateChecks chạy riêng với nupkg thật; Verify-Packages kiểm feed/hash/payload/models/PE icon/source tree/installer/ZIP.
-Chưa chạy cài/update Windows, capture/hotkey/IME/caret thật, taskbar/auto-hide/multi-monitor/DPI, chuột và nghiệm thu thị giác. Không có screenshot UI của bản đang chạy hoặc cam kết hiệu năng/RAM mới. Checklist docs/TEXT-CALLOUT-REFINEMENT.md.
-Không tự publish GitHub. Setup chưa ký số. Dữ liệu/project cũ giữ margins; canvas không tự thu sau khi xóa/kéo vào trong.

## Lịch sử kiểm chứng trước

# V1.22.0 — Kiểm tra ngày27/10/2026

- 28 bộ logic/static,1615 assertions:90 kiểm tra ObjectPrecision mới; thêm2shell contract. Bao gồm xoay/lật/resize toàn ảnh, vị trí/kích thước theo output, align/image/canvas/selection, group/distribute/lock, font/callout limits, ngoài ảnh và zero-axis line, snapshot Undo/Redo, snap/bypass.
- 17 bộ WPF cross-build trên Linux; PrecisionWindowsChecks thêm hit stack/ẩn/khóa, số so với bounds thực, nudge, giữ tỷ lệ/tay nắm, nhóm, annotation trên khung và clip nguồn. **Không thực thi WPF hoặc kiểm thử Windows desktop tại môi trường này.**
- Main Windows x64 publish tự chứa.NET10.0.13; các dependency được giữ pin, không nâng stack. Capture/hotkey/native pixel-copy/install-path foundations và logo giữ từ ZIP1.21 Boss gửi.
- Gói updater thực tế chạy riêng qua UpdateChecks; kiểm hash/feed/payload/models/PE icons/source tree/installer/ZIP bằng Verify-Packages.py. Không tự publish.
- Nghiệm thu còn chờ: Windows install/update, thao tác chuột/menu/IME, nhiều DPI/màn hình, render/clipboard/PDF và fidelity. Chi tiết docs/OBJECT-EDITING-PRECISION.md. Benchmark cũ1.20 không phải benchmark UI1.22.

## Lịch sử kiểm chứng trước

# V1.21.0 — Kiểm tra ngày26/10/2026

- Logic/static: 27 bộ,1523 assertions; thêm20 kiểm tra margins/crop pixel/rotation/flip/schema14/Undo/Redo và2contract UI. Các phần đã kiểm từ1.20 tiếp tục kế thừa.
- 16 bộ WPF compile-only trên Linux, có fixture mới crop toàn canvas/render annotation ngoài ảnh/callout một click/Undo vùng tăng; cần chạy trên Windows thật. Không tuyên bố đã kiểm tương tác bằng screenshot hay UI tự động.
- Bản build/publish Windows x64 tự chứa; artifact check xác minh feed/nupkg/ZIP/logo/installer/nguồn update và source. UpdateChecks với nupkg thật được chạy riêng sau đóng gói.
- Hướng dẫn thao tác/giới hạn/checklist nghiệm thu: LAYOUT-PRECISION.md. Nền capture, hotkey, pixel copy và install path giữ nguyên từ ZIP1.20 được Boss chỉ định. Benchmark JSON cũ là đo pixel-copy1.20, không là benchmark giao diện1.21.
- Chưa chạy cài/update, tương tác WPF, DPI/multi-monitor, capture/quay/OCR trên Windows desktop. Chưa ký số Setup hay publish GitHub Release.

## Lịch sử kiểm chứng trước

# V1.20.0 — Kiểm tra ngày26/10/2026

-27 bộ logic/static:1501 assertions,38 mới về calendar12tháng/ngày/năm nhuận, LRU/concurrency/budget, canvasnegative/rotation/flip/schema13/legacy12/future14/Undo, last-good validation/exact bytes/fallback, metrics64;3contract footer/menu. Các bộ nền trước tiếp tục qua.
-16 bộ WPF compile-only; ReleaseWindowsChecks mới về rectangle/callout ngoài bitmap, xuất native pixel/external alpha/legacy clip/nudge/Undo, caption32×26/height28/name, thread cache và concurrent redaction. Chưa chạy WPF trên Linux.
-Benchmark thật trên Linux .NET10: copy3840×2160/7680×4320,3lượt mỗi cỡ, so toàn bộ byte đều trùng. Chi tiết thời gian/allocation trong PERFORMANCE-BENCHMARK.json; không là UI/GPU/capture/Windows peak-memory test.
-Main publish Windows x64 tự chứa.NET10.0.13 không lỗi/cảnh báo. Runtime/deps/capture foundations/approved logo/installer core arguments giữ từ source1.19 Boss gửi.
-Real nupkg kiểm61 UpdateChecks; verifier kiểm payload/feed/zip/native/model/PE app/updater/Setup, source byte tree và5release assets. Không tự publish GitHub.

Windows desktop acceptance vẫn chờ: caption/theme/DPI/focus/resize, calendar tree/scroll/select/paging/media, tooltip/overflow, outside draw/text/tail/crop/export/copy/source pixels, actual4K/8K zoom/RAM/render/cache, I/O/corrupt recovery/install-update/logo. Checklist IMAGE-RELEASE-PERFORMANCE.md. Chưa tile renderer hoặc cam kết trần RAM/hiệu năng toàn app; legacy documents giữ crop clipping khi chưa bật DrawOutsideImage.
