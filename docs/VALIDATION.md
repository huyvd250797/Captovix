# V1.20.0 — Kiểm tra ngày26/10/2026

-27 bộ logic/static:1501 assertions,38 mới về calendar12tháng/ngày/năm nhuận, LRU/concurrency/budget, canvasnegative/rotation/flip/schema13/legacy12/future14/Undo, last-good validation/exact bytes/fallback, metrics64;3contract footer/menu. Các bộ nền trước tiếp tục qua.
-16 bộ WPF compile-only; ReleaseWindowsChecks mới về rectangle/callout ngoài bitmap, xuất native pixel/external alpha/legacy clip/nudge/Undo, caption32×26/height28/name, thread cache và concurrent redaction. Chưa chạy WPF trên Linux.
-Benchmark thật trên Linux .NET10: copy3840×2160/7680×4320,3lượt mỗi cỡ, so toàn bộ byte đều trùng. Chi tiết thời gian/allocation trong PERFORMANCE-BENCHMARK.json; không là UI/GPU/capture/Windows peak-memory test.
-Main publish Windows x64 tự chứa.NET10.0.13 không lỗi/cảnh báo. Runtime/deps/capture foundations/approved logo/installer core arguments giữ từ source1.19 Boss gửi.
-Real nupkg kiểm61 UpdateChecks; verifier kiểm payload/feed/zip/native/model/PE app/updater/Setup, source byte tree và5release assets. Không tự publish GitHub.

Windows desktop acceptance vẫn chờ: caption/theme/DPI/focus/resize, calendar tree/scroll/select/paging/media, tooltip/overflow, outside draw/text/tail/crop/export/copy/source pixels, actual4K/8K zoom/RAM/render/cache, I/O/corrupt recovery/install-update/logo. Checklist IMAGE-RELEASE-PERFORMANCE.md. Chưa tile renderer hoặc cam kết trần RAM/hiệu năng toàn app; legacy documents giữ crop clipping khi chưa bật DrawOutsideImage.
