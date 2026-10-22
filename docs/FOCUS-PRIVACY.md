# V1.11 — Focus & Privacy: sử dụng và nghiệm thu

## Dùng ngay trong một Editor

- M hoặc Phóng đại: kéo vùng; chỉnh 1.25–8×, chữ nhật/ellipse, viền và màu ở bảng Focus & Privacy → Áp dụng. Đây là kính tại chỗ: nguồn lấy ở tâm vùng và theo khi kéo kính, không có tay nắm nguồn độc lập. Hình trong kính giữ hướng ảnh. Kính lấy ảnh gốc đã áp mask đang hiện, không phóng text/shape annotation, không lấy ảnh ngoài crop, không khôi phục chi tiết chưa có.
- O hoặc Spotlight: kéo vùng cần giữ sáng; màu vùng là màu lớp tối, độ đậm 0–100%. Mặc định dùng màu hiện hành; preset Spotlight nền tối dùng #101828 / 70%. Nhiều spotlight hợp vùng sáng; màu/độ tối theo spotlight trên cùng trong Layers. Lớp tối luôn nằm dưới chú thích, thứ tự spotlight chỉnh qua menu chuột phải.
- H hoặc Highlight: hình chữ nhật/ellipse, màu/opacity tùy chọn. Highlight không phải che thông tin.
- B/P/K: blur/pixel/che đặc. Đổi hình vùng, mức 4–64, màu; chọn kiểu che và Áp dụng để đổi tại cùng vùng. Khi không chọn đối tượng, đổi kiểu sẽ chuyển công cụ tạo mới. Che kín luôn màu đặc 100%, không dùng opacity chung làm trong suốt.
- Bấm đối tượng để kéo/sửa từ công cụ đang dùng; Shift giữ tỷ lệ, tay nắm xoay, X/Y/W/H theo pixel nguồn. Alt+kéo tạo vùng mới chồng lên đối tượng khác; cần Alt nếu bắt đầu trong lens/shape đã có. V+Alt click chọn lớp dưới theo cơ chế cũ. Có Ctrl+C/V, Ctrl+D, nhóm/khóa/ẩn/Undo/Redo.
- Preset có 5 mẫu mới. Chọn mẫu → Chỉ dùng preset cho nét mới để chuyển đúng công cụ. Áp dụng trên đối tượng thay style được hỗ trợ, không tự chuyển Highlight thành Spotlight; đổi Blur/Pixel/Cover tại bảng vùng che. Lưu preset riêng để giữ zoom/opacity/hình vùng/màu/mức lọc.
- Kiểm tra trước khi chia sẻ hiện khi có mask hoặc dùng công cụ che: số che đặc/filter/đang ẩn và phần vượt crop. Kiểm tra ảnh ở 100% bỏ selection và mở Actual Pixels trong Editor; Ctrl+Shift+C copy toàn ảnh đã chỉnh, Xuất tạo PNG/JPEG đã ghép. Ctrl+C khi chọn đối tượng là copy đối tượng sửa lại được.

## Thứ tự vẽ và giới hạn

1. Ảnh gốc giữ số pixel, không dùng thumbnail làm nguồn xuất.
2. Spotlight tạo nền tối ngoài hợp vùng sáng; nằm dưới các annotation.
3. Highlight, lens và annotation theo thứ tự lớp thường. Kính lấy mẫu ảnh nguồn đã áp các mask đang bật. Mọi vùng che đặc được tô vào mẫu trước khi tính blur/pixel; mẫu ngoài crop là trắng. Cache lọc thay khi che/crop đổi. Tính filter theo tile có halo; giữ cache 32 MiB như nền cũ.
4. Blur/pixel vẽ trên chú thích; che đặc vẽ cuối cùng. Sắp xếp lớp không thể hạ che đặc dưới lens/filter. Mask ẩn không tham gia bảo vệ hoặc xuất; review chỉ thông báo để người dùng kiểm tra, không tự tìm dữ liệu nhạy cảm.
5. Crop, xoay/lật/resize trình bày ảnh áp sau compositing theo pipeline cũ; zoom chỉ đổi cách xem.

Xuất PNG/JPEG tạo bitmap phẳng, không gửi sidecar/Media theo file ảnh. Thư viện/dự án và backup vẫn giữ ảnh gốc để sửa lại: không chia sẻ những phần này khi muốn giấu nguồn. Blur/pixel có thể còn nhận ra nội dung; muốn che dùng màu đặc, phủ dư biên chữ và kiểm tra ảnh xuất ở 100%. Cạnh ellipse/xoay có antialias; không dùng một pixel sát biên làm ranh giới bảo vệ. Chưa có tự nhận diện bí mật/OCR hoặc chứng nhận không phục hồi.

## Dữ liệu và nâng cấp

Editor schema 9 đọc 1–8, giữ highlight alpha 78; không thay style callout schema 8. Lần lưu đầu sidecar cũ giữ byte chính xác tại Edits/<id>.json.pre-v1.11.bak; giữ mọi backup trước. Preset thêm Focus tùy chọn; preset cũ vẫn đọc. Workflow giữ cơ chế .pre-v1.9.bak. Không reset thư viện hoặc đổi URL update/root cài.

Cài: giải nén Update-Packages, thoát Captovix ở khay, chạy HuyVo.Captovix-win-Setup.exe, giữ D:\Captovix\Captovix nếu đang dùng. Muốn update trong app, tạo release stable v1.11.0 và đăng đúng 5 asset theo README-PUBLISH.txt trong ZIP. Nơi build chưa đăng release. Muốn hạ bản cần khôi phục sidecar tương thích từ backup; V1.10 không đọc schema 9.

## Checklist Windows — chưa chạy ở nơi build Linux

- Chạy Build.cmd để chạy toàn bộ logic/WPF và dựng gói. FocusPrivacyWindowsChecks phải đạt mới coi kiểm tra render tự động đã đạt.
- Lens các mức 1.25/2/3/8, chữ nhật/ellipse, xoay 0/33/90, di chuyển/resize sát biên crop. Kính rõ hơn do phóng nguồn, không làm thay kích thước file. Thử 4K/8K, nhiều mask, RAM và độ trễ trước triển khai rộng.
- Spotlight một/nhiều vùng giao nhau không tối trong vùng hợp; đổi topmost style, ẩn/khóa. Highlight opacity 0/30/100, ellipse góc ngoài giữ nguồn. Lưu/mở và Undo đúng.
- Che một chuỗi chữ có màu khác nhau. Lens chồng lên, lens phủ lớn hơn mask, blur/pixel chồng gần biên che; đổi z-order. Xuất PNG, JPEG và clipboard, kiểm tra không thấy chữ đã che. Thử thay ảnh nguồn chỉ ở bí mật đã che trong fixture và so bản xuất. Giữ mask phủ dư biên.
- Đổi màu/di chuyển/ẩn cover rồi Undo để kiểm cache không giữ ảnh lọc cũ. Crop có nội dung nhạy cảm ở ngoài, lens/filter không kéo nội dung ấy vào ảnh xuất.
- Chọn vùng che từ Layers, chuyển Blur→Cover→Pixel; 1 Apply=1 Undo. Tạo mới cùng style; preset/save; Ctrl+C/V sang ảnh khác giữ cỡ và dữ liệu. Khi đang gõ text vẫn copy/paste văn bản.
- Mở/chuyển nhiều ảnh, Editor maximized. Scale 100/125/150/175/200%, monitor âm và đổi màn hình; capture/hotkey/UIA/IME theo checklist cũ. Cài D và update restart, ảnh gốc/sidecar/preset không mất.
- File PNG/JPEG chia sẻ chỉ là kết quả ghép. Đối chiếu pixel/màu trên Windows SDR; HDR/màu rộng đánh giá riêng. Không đánh dấu đạt chỉ nhờ build.

```powershell
dotnet run --project tests/FocusPrivacyChecks -c Release -p:Platform=x64
dotnet run --project tests/FocusPrivacyWindowsChecks -c Release -p:Platform=x64
.\Build.cmd
```

Bản tiếp theo V1.12 — Selection & Pixel Editing: chọn pixel chữ nhật/ellipse/lasso; copy/cut/paste thành đối tượng ảnh; fill/eraser; bỏ hàng/cột thừa. Giữ ảnh gốc và Undo theo ngân sách bộ nhớ; tách rõ chọn pixel với chọn annotation. Chưa triển khai trong V1.11.
