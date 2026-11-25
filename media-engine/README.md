# Media engine (download at build time)

Run `scripts/Ensure-MediaEngine.ps1` on Windows, or `Build.cmd` which calls it automatically. The clean source package intentionally excludes FFmpeg executables.

Pinned Gyan FFmpeg 9.0.2 essentials, GPL v3: https://www.gyan.dev/ffmpeg/builds/
Archive: https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-9.0.2-essentials_build.zip
Archive SHA-256: 60f467265b1e312373dbcd92200c2618a74850f98d3d078e94296bb3fa2047ba
Executable SHA-256: 3256173f3f8bffd7df12227c68adf68025edb1832273a9530688a7bb1ed8edec
Upstream commit listed by the build: https://github.com/FFmpeg/FFmpeg/commit/946fcce07b
Build instructions / configuration: https://github.com/GyanD/codexffmpeg

The download script preserves the upstream LICENSE and README with full build configuration and sources. A distributor of a compiled application containing the GPL media engine must also fulfill the corresponding-source obligations for FFmpeg and its enabled GPL dependencies. Do not strip the accompanying license files. This source release downloads the engine directly from its provider on the builder's machine.
