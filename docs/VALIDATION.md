# V1.26.0 — Smart Element Capture: bằng chứng kiểm tra

Nền: Source(1).zip V1.25.0 do Boss gửi. Môi trường thực hiện: Linux x64, .NET SDK10.0.100; target WPF Windows x64, runtime self-contained10.0.13. Không có phiên Windows desktop trong lần xây dựng này.

| Kiểm tra thực thi | Kết quả |
| --- | --- |
| SmartCaptureChecks |49: hình học8cạnh/góc, move, Shift ratio, click/drag phân biệt, nudge, DPI fit, toolbar, chống jitter/cấp cha-con |
| ConnectorChecks |81: liên kết, hierarchy UIA thuần, tiến trình probe thật với timeout/hủy/khôi phục protocol |
| CaptureWorkflowChecks |53: phím tắt, capture gate, topology/repeat và cấu hình |
| ImageFidelityChecks |122: copy pixel, crop/rotate/flip, DPI/zoom độc lập, vùng không chỉnh |
| ShellLayoutChecks |35: XAML, đúng3action thư viện và cấu trúc menu/editor |
| ScrollQualityChecks |21: match, không bỏ thay đổi1pixel, ghép exact và cancel |
| ScrollAppearanceChecks |78: luồng cuộn, theme và an toàn |
| AdvancedScrollChecks |20: ghép ngang, dải cố định, phục hồi phiên |
| UpdateChecks logic |56: feed/paging, version, scheduling và settings |
| InstallerContractChecks |25: đường dẫn và tham số installer không tái phát EXE_ARGS |
| Tổng logic / cấu trúc |540 trường hợp pass |
| WPF publish app |Pass,0warning/0error |
| Cross-build dự án Windows |FidelityWindowsChecks, ConnectorWindowsChecks, RefinementWindowsChecks, SmartCaptureWindowsFixture: pass,0warning/0error; chưa thực thi |

Bước đóng gói chạy thêm UpdateChecks với full.nupkg thực tế (bao gồm kiểm tra payload/version/hash/integration), Verify-Packages với byte comparison giữa source/payload/ZIP/feed, CRC, logo PE/ICO và wrapper Setup. Đây là kiểm tra artifact; không thay cho chạy installer hoặc update GUI.

Không ghi ứng dụng Edge/Chrome/Explorer/UltraViewer đã được kiểm thử runtime. Chưa xác nhận trên Windows: UIA/provider và khung highlight, cursor/keyboard gesture, GDI byte roundtrip, anti-alias ở màn hình thật, DPI100–200%, màn hình âm, HDR, clipboard, cài ởD, popupupdate và khởi động sau update. Checklist theo từng trường hợp nằm trong `SMART-ELEMENT-CAPTURE.md`; fixture Windows trong source giúp lặp lại modal/bảng theo cùng bố cục.

Bản này giữ backend chụp cửa sổ native và quay video; không tăng độ phân giải thật của remote/emulator. Vừa khung có thể trông mềm do giảm zoom; kiểm tra PNG gốc ở100%. Không dùng sharpen/upscale để che mất pixel nguồn.

---

## Lịch sử kiểm tra

# Captovix V1.25.0 — Scrolling Capture Quality

## Chụp cuộn

- Tìm overlap hai bước: lấy ứng viên nhanh rồi kiểm tra dày hơn, gồm lân cận, để giảm nhận nhầm điểm nối hoặc khung trùng. Giữ copy pixel nguyên bản, không blend/resize tại mối nối.
- Tự cuộn chờ theo cấu hình, rồi so tối đa3cặp khung cách180ms để kiểm tra nội dung đã đứng yên. Nếu còn chuyển động, tạm dừng; Tiếp tục thử lại vị trí hiện tại trước khi cuộn thêm. Có chế độ lấy thủ công cho ứng dụng khó.
- Header/footer khi cuộn dọc, hoặc dải trái/phải khi cuộn ngang: nhập pixel đầu/cuối, bỏ khỏi phép ghép, tùy chọn giữ dải đầu khung đầu và dải cuối khung cuối đúng một lần. Phiên ngang có vùng cố định mới không mở bằng bản cũ; ảnh kết quả PNG vẫn mở bình thường.
- Khung trùng không append;3lần không có nội dung mới dừng tự động, vẫn cảnh báo có thể nguồn không nhận cuộn. Khung không chắc dừng tự động, yêu cầu xác nhận/sửa overlap trước xuất; kích thước thay đổi bị từ chối.
- Xem cặp khung tại điểm nối bằng vùng cuộn pixel; khung trước mở ở cuối hướng cuộn, khung sau ở đầu. Có toàn ảnh Fit/100%, guide opt-in không vào file xuất. Sửa overlap/loại khung giữ nguồn, revision và phiên hash để mở/ghép lại.
- Giới hạn giữ24khung,4MP/khung,64MPnguồn,40MPkết quả,32768px/cạnh theo pipeline hiện có. Không cam kết trang động, video, tải vô hạn hoặc mọi ứng dụng; không phải công cụ tự đọc toàn DOM.

## Cập nhật và giao diện

- Khi tự kiểm tra đang bật: kiểm tra nguồn khi mở app, không bị mốc đã kiểm24giờ của phiên trước chặn; trong phiên tối đa mỗi30phút. Popup một lần mỗi phiên bản trong phiên chạy, chờ Editor có thể tương tác; Để sau/đóng thu về nút cập nhật, Cập nhật mở trang kiểm tra/cập nhật.
- Tôn trọng tùy chọn tắt tự kiểm tra. Popup chỉ xuất hiện khi nguồn trả về phiên bản cao hơn. Release phải stable/latest và có đủ5asset, không chỉ SourceZIP/UpdateZIP; lỗi mạng/feed hiện trong trang cập nhật và log, không báo sai là đã mới nhất. Chưa tự publish.
- Màn hình trống chỉ giới thiệu Captovix bằng logo/nội dung, bỏ hai dòng shortcut cố định và tooltip cứng. Phím tắt cấu hình vẫn hoạt động theo cài đặt máy.
- Rê viền vùng chụp hiện con trỏ hai đầu theo cạnh/góc; chỉnh xong Enter/nút máy ảnh chụp. Giữ viền nét đứt từ1.24.
- Dải thư viện ngang: nút icon đầy đủ chuyển sang cụm gọn bên cạnh ảnh; bỏ dấu… trong chế độ này, thư viện dọc vẫn giữ overflow. Thanh phân cách có nền và con trỏ resize; kéo thay chiều cao96–260DIP, lưu trên máy; lần đầu từ cấu hình cũ thu về tối đa112DIP. Thumbnail co theo chiều cao, không dùng hàng riêng cho nút.
- Footer căn giữa theo chiều dọc để copyright, trạng thái và nút update thẳng hàng.

## OCR gọn

Mở OCR → kéo vùng trên ảnh → thả chuột tự nhận chữ → sửa/copy kết quả. Mở đầu không phủ tím toàn ảnh. Chỉ giữ ảnh có zoom, chọn toàn ảnh/nhận lại/hủy, ô kết quả và sao chép/đóng. Mặc định Việt+Anh, tự phân tích bố cục, xử lý trên máy. Bỏ danh sách dòng, thay chữ/nền/font, tách lớp, preview/apply và chọn bố cục/ngôn ngữ khỏi giao diện OCR. Không chỉnh bitmap nguồn. Hủy/đóng vẫn chờ worker kết thúc; lỗi hiện thông báo cụ thể.

## Kiểm chứng và cài đặt

6bộ logic/static đạt232assertions:21mới về overlap/native pixel/transpose/vùng cố định/cancellation; thêm4policy kiểm tra cập nhật theo phiên.3bộ WPF cross-build trên Linux; main Windows x64 self-contained.NET10.0.13 build/publish. Chạy UpdateChecks với nupkg thật và Verify-Packages sau đóng gói. **Chưa chạy Windows desktop hoặc nghiệm thu ảnh cuộn thực tế.** Số liệu bản cũ bên dưới là lịch sử.

Nghiệm thu Windows cần: bộ trang mẫu văn bản/bảng dọc-ngang có header/footer, không mất/lặp dòng/cột; trang tải chậm/animation và retry tại cùng vị trí; cuối trang/Cancel/restore/loại khung; mối nối/PNG đúng pixel; popup khởi động và sau30phút/opt-out; resize con trỏ/DPI100–200%/monitorâm; dải ngang thấp/cao và tất cả nút; OCR chọn vùng/zoom/worker/copy; cài/update/logo/dữ liệu. Chưa có screenshot giao diện mới chạy thật. Không suy diễn kiểm tra tổng hợp thành hỗ trợ mọi app.

Giải nén Update-Packages, đóng app, chạy HuyVo.Captovix-win-Setup.exe. Để phát hành: đăng đủ5asset lên huyvd250797/captovix-releases, tagv1.25.0, stable/latest. Không đổi tên/byte. Backup dữ liệu trước nâng cấp; giữ media/settings/logo/stable source. Setup chưa ký số.

## Tiếp theo V1.26 — Image Composition Studio

Ghép ảnh theo hàng/cột/lưới, chỉnh khoảng cách/nền/viền/bóng, tiêu đề và đánh số bước, mẫu bố cục dùng lại. Ưu tiên patch phản hồi Windows1.25 trước.

## Lịch sử phiên bản trước

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
