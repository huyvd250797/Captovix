# Image Layer Thread Safety —1.37.3

## Tình huống

Ảnh1 ← pasteẢnh2 → lưu/chuyển sang ảnh khác trong thanhthưviện → mở lại ảnh1. Screenshot báo thread khác sở hữu object và nhiềuMessageBox xếpchồng. Screenshot thể hiện app1.37.1; bản vá dùng source1.37.2 đínhkèm theo chỉ địnhBoss.

## Đường lỗi trong source

OpenImageAsync chạy DocumentStore.Load/RasterPixels.ValidateDecoded trong Task.Run(MTA). Autosave renderthumbnail trên StaImageWorker(STA). UI vẽ từ cachechungRasterPixels.

Trước vá cache lưu BitmapDecoder.Frames[0] dù Freeze. BitmapFrameDecode vẫn có đường IsDownloading → Decoder.IsDownloading; BitmapDecoder là DispatcherObject. Frozenframe không làm toàn bộ decoder graph dùng được từ luồng khác. Khi dựng/freeze drawing hoặc truy vấn trạng thái ảnh trên UI, decoder worker có thể gây VerifyAccess exception.

Nguồn chính chủ WPF đã đối chiếu: https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Imaging/BitmapFrameDecode.cs . Chưa có errors.log đầyđủ của máyBoss để khẳng định frame chính xác trong stack, nhưng đây là đường chia sẻ object sai luồng cụ thể khớp tình huống mở lại PNG có lớp ảnh.

## Thay đổi

- Trong đúng luồng decode, copy pixel sang BitmapSource.Create BGRA96dpi vàfreeze; chỉ giá trị này vào cache. Không cache BitmapFrame/decoder/metadata.
- Cache vẫn giới hạn256MB/128entry, tính theo PNGstring và BGRApixels. Copy chỉ khi miss; hit dùng lại frozenbitmap. Clearcache và giải phóng khi đóng/chuyển ảnh giữ nguyên.
- Dữ liệu PixelPng/PixelBasePng trong sidecar không đổi; không serialize lại ảnh để sửa lỗi, không xóa layer/ảnhgốc.
- ImageTransfer.Png tách BitmapFrame trước encode để không kéo metadata decoder qua thread.
- App đặt DispatcherUnhandledException.Handled=true trướcMessageBox. ExceptionDialogGate chống nested/concurrentreport, luôn release trongfinally. Không im lặng bỏ lỗi đầu hoặc tắt log toàn app.

## Kiểm tra

- Release app cross-build0warning/0error.
-7 ErrorDialogChecks đã chạy đạt, gồm nested/concurrent/errorcallback/reuse.
- PixelStudio/Composition/TextClipboard/CaptureBounds kiểm tra logic được chạy lại.
- Windows fixture ImageLayerThreadWindowsChecks đã cross-build: workerdecode→UI IsDownloading/CanFreeze/draw, STAencode/thumbnail, sidecarsave/load,25lượt unload/đổiảnh/reopen và đối chiếupixel/PNG/sidecar. Chưa chạy Windows vì hostLinux không có WPF runtime.
- Bộ cài/feed/sourceZIP/update được kiểm riêng; xem docs/VALIDATION.md.

## Windows acceptance bắt buộc trước phát hành rộng

1. Chụp1, chụp2, Copyảnh2/Pasteảnh1, đợi autosave hoặc chuyển ngay, mở lại ảnh1; dán còn nguyên, không lỗi/không hộp thoại chồng.
2. Lặp25lượt với thanh thư viện ngang/dọc, chọn nhanh liên tục, đổi layout/zoom; không mở nhầm kết quả cũ.
3. Dán nhiều ảnh/PNG có alpha, crop/mask/rotate/opacity, có blur/magnifier; save/reopen/PNGexport đúngpixel/nộidung.
4. Đóng/mở app và mở ảnh1 cũ đã cólớpảnh; không cần xóa sidecar, layer/ảnhgốc vẫngiữ. Lưu ý bản lỗi trước đây đã rơi vào recovery thì kiểm tra backup/invalid sidecar trước khi lưuđè; không tự xóa chúng.
5. Copy/drag ảnh đã chú thích ra ứng dụng khác và thumbnails đúng nội dung.
6. Máy1.37.2 nhận1.37.3 sau publishLatest; dữ liệu/key giữ nguyên.

Không cần migration mới. LauncherSetup native không đổi. Chưa tự publish GitHub.
