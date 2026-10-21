# Captovix V1.6.1 architecture

## Single Editor and window state

MainWindow owns capture configuration, media browser, image surface, video playback and property panels. RegionPicker, CapturePrompts and RecordingBar are transient capture controls. Startup WindowState is Maximized. Capture saves the previous state before hiding the Editor and restores it when opening the Editor afterward; background photo capture retains the active edited document.

## Image document and coordinate system

Original files remain separate. EditorDocument stores schema version, original pixel dimensions/content hash, crop viewport and annotation objects with stable IDs. Every object stays in ORIGINAL image coordinates; cropping changes only the viewport and never repeatedly offsets objects. Restoring the frame or undoing crop reveals hidden content again.

EditorSurface converts pointer positions from local DIP/zoom to source pixels and handles create/select/move/resize/crop. EditorRenderer draws the original and overlays, with clipping to the viewport. Selection borders/handles/crop previews are drawn by the surface only and do not appear in export. PNG/JPEG/clipboard use the same renderer at a 96-DPI render target sized to output pixel dimensions, independent of viewport zoom. The 100%-pixel view compensates for the display DPI. JPEG uses a white background behind transparency.

## History and autosave

EditorSession groups gestures and text editing bursts into transactions. Snapshot-based undo/redo keeps up to 100 committed previous states. Canceled gestures restore the pre-gesture document; no-op commits do not create entries. New edits clear redo. Undo history is in memory for the currently opened image; sidecar files store the final editable document, not undo snapshots.

A 550-ms UI debounce persists committed edits; typing commits after a 700-ms pause or focus loss. Changing media, starting capture and closing the app flush pending edits. Failed save blocks media switching/close and keeps the in-memory document available; a failed thumbnail cache never invalidates a successfully saved sidecar. Ctrl+S retries. A source hash/size/schema mismatch causes an invalid sidecar backup and a visible warning.

## Persistence and migration

Files live in local per-user app data. MediaStore uses JSON metadata/settings, images/video files and cached thumbnails. DocumentStore writes sidecars atomically. New Captovix metadata are imported from V0.1 only when no Captovix library exists. Legacy media remain referenced at their existing paths; they are not moved or copied. The legacy data folder must be retained while referenced.

## Capture and packaging

Desktop photos (region/full display) now use DesktopPhotoCapture with Win32 GDI BitBlt, top-down 32-bit DIB and opaque SDR8 PNG. No scaling or even alignment. Independent window photos and video retain ScreenRecorderLib, Desktop Duplication/WGC, Media Foundation and WASAPI. PerMonitorV2 and physical pixel geometry are retained. Successful native completion precedes committing pending capture files. Installer is a per-user NSIS migration bootstrap around Velopack Setup, with a self-contained .NET payload and offline Microsoft VC++ Runtime prerequisite. Bootstrap installs the new app successfully at LocalAppData/HuyVo.Captovix before invoking the registered, recognized NSIS V1.0 uninstaller. The old generated owned-file uninstaller retains user data. It recreates shortcuts via the stable Update.exe start command. Velopack owns installation, uninstall and future updates; user data remain outside the installation root.

## Validation boundary

Core history/crop/migration/geometry checks can execute on Linux. WPF bitmap export/clipboard, Windows shell/installer, screen/audio capture and real DPI input require Windows. Windows-specific checks are compiled and provided for CI/manual execution; see VALIDATION.md for what was actually run.

## Update lifecycle

Program.Main runs Velopack hooks before WPF or the single-instance mutex; SetAutoApplyOnStartup(false) prevents silent application of cached updates. UpdateSettingsStore loads a saved user override or bundled update-source.json (GitHub repository huyvd250797/captovix-releases by default). Invalid overrides disable automatic checks instead of changing publisher. GitHub source is unauthenticated/public; static sources use HTTPS directories. No credential/token storage in clients.

VelopackUpdateBackend restricts the package to stable, newer HuyVo.Captovix releases on channel win. Installed builds use UpdateManager; portable builds inspect the feed and offer manual download links only. SDK downloads can use deltas/fallback. A second SHA-256 and size check of the final full package gates Ready. Hashes detect damaged downloads, not independent publisher authenticity.

UpdateSession gates checking/download/apply and supports cancellation/retry/defer. UI callbacks marshal progress onto the WPF dispatcher. Applying is synchronous on the UI thread after capture-busy guard, FlushEdits, metadata/settings flush and UpdateBackup.Create. Backups contain JSON and annotations, not media. ApplyUpdatesAndRestart starts the external updater and exits; all required saves happen before it. If preparation fails the app remains open. Cached packages are not blindly restored on the next launch: source must be checked again and package integrity verified before Ready. Defaults do not auto-apply when the user quits or reopens.

A persisted editor-resume selection reopens after restart. Backup creation does not migrate or roll back schemas. Future schema changes must preserve/backup old data and provide explicit migration; automatic data rollback is out of scope.

## Installer recovery (1.0.2)

The wrapper no longer forwards application arguments through Setup.exe's trailing EXE_ARGS positional. Velopack 1.2.161's parser panics on this path (upstream #1039). Supported named options only are used: installto, log, verbose. Core Setup is visible, so the user sees its actual progress, errors and prerequisite UI. Bootstrap exposes three phases and rotates a dedicated setup-core.log in the user's data/Logs directory. The app-only install suppression argument has been removed from Program.Main. A visible setup may launch the app before the outer Finish page; the Finish action checks the app mutex to avoid duplicate-launch warnings. Old NSIS removal remains gated on core success and installed file checks, with no recursive deletion of the data root.

## Image Annotation (1.1)

EditorDocument accepts schema 1 and 2, promotes loaded schema 1 in memory and writes schema 2 on the next edit save. Blur/Pixelate/Cover append enum kinds, with bounded EffectSize. RedactionPixels is a platform-independent, separable CPU blur/block average with opaque outputs. RedactionRenderer converts source to BGRA32, processes 512-pixel tiles with blur halos/block-aligned pixel tiles, and retains at most 32 MiB/64 cached masks per source bitmap. Large masks process in tiles without an additional full-mask working buffer. During interactive mask transforms the canvas shows opaque drafts; flatten/export always compute final pixels. Redactions are composited above normal annotations and solid Cover patches above all filters.

AnnotationEditing provides multi-object layout/resize/snap/clamp transforms without WPF dependencies. EditorSurface maintains selection IDs separate from document state and snapshots geometry per gesture; selection is not undo history, geometry changes are. Presets include six immutable built-ins and up to twenty custom styles saved atomically in annotation-presets.json. UpdateBackup includes that file from V1.1.

## Image Editor UX (1.2)

EditorViewportMath centralizes clamped wheel scaling, cursor-anchored zoom offsets and DIP-based pan. MainWindow.Viewport maps canvas/ScrollViewer coordinates after layout, handles Space capture/release/deactivation and keeps pan out of document history. CropGeometry provides integer source-pixel creation, clamp/resize/move and overflow-safe validation. EditorSurface keeps CropDraft outside EditorDocument until an explicit apply; moving/resizing a draft never writes sidecar state. Crop applies through one EditorSession transaction and leaves annotation coordinates unchanged. Schema remains 2.

NSIS bootstrap adds a directory page and validates paths before any install work (also in silent mode). It passes chosen INSTDIR via the supported --installto option, rejects nonempty fresh targets/data-folder overlap and requires a fixed local writable volume. Existing managed roots are detected through Velopack's HKCU uninstall InstallLocation, a bootstrap setting and the legacy default path, verifying Update.exe/current/Captovix.exe. Automatic relocation is deliberately not implemented; uninstall-then-install preserves external user data and avoids conflicting registry/shortcut registrations.

## Capture Workflow (1.5)

CaptureWorkflow.cs is platform independent: schema-1 configuration/validation and atomic storage, capture presets, physical-region topology fingerprints, hotkey transaction adapter and single-owner gate. It does not change image schema 4. MainWindow.CaptureWorkflow.cs integrates controls into the existing Editor and owns lifecycle, preset application, capture dispatch and keyboard bindings. CaptureWorkflowNative.cs provides Shell_NotifyIconW, HKCU Run registration and temporary window/countdown prompts.

The gate is acquired before flushing edits to prevent reentrant capture from a modal save prompt. A successful engine completion precedes moving pending media, saving library history, storing last-region, then copying/notifying. Background capture does not select the new item or unload the active image. Failure/cancellation restores prior visibility; absence of a usable tray forces the Editor visible. HWND is never stored in presets. A window is selected fresh and validated with its process ID before engine start. Screen layout, monitor interface identity, bounds and DPI are fingerprinted and rechecked after countdown.

Five photo hotkeys have editable chords and fixed action IDs; recording IDs 2/3/4 remain reserved. Duplicate/unsupported input is rejected before registration. Startup permits best-effort registration with warnings; edited bindings use rollback on external conflicts. While a hotkey textbox has focus, global bindings are released so the key can be entered locally. Committing workflow preferences handles hotkey registration, owned HKCU Run value and atomic JSON; file failures attempt to restore previous live keys and Run value and report recovery failures.

Close-to-tray and autorun default off. Manual launch always opens the maximized Editor. Only the fixed autorun argument permits startup hidden when opted in; there is no capture-on-start argument. The second-instance broadcast restores a hidden Editor. Explicit tray Exit bypasses close-to-tray and waits for active video completion. Apply update saves and backs up state before suspending tray/hotkeys; failed apply recreates them. The early Velopack uninstall hook removes only the owned autorun registration and leaves user media/settings.

## Image Fidelity (1.6)

ImageFidelity contains platform-independent BGRA transforms, alpha composition, DPI/zoom math and capture size guards. EditorRenderer uses exact pixel copy for integral unit-scale orthogonal layouts; frame and annotation layers are rasterized separately, then composited. Resized previews/thumbnails and user-requested resize retain WPF high-quality rendering. Odd centered canvas offsets are floored to physical integer pixels to avoid accidental half-pixel sampling.

ImageTransfer owns BGRA8 conversion, PNG encoding, clipboard PNG/Bitmap and atomic export with decoded dimension verification. Output metadata is 96 DPI, file pixel dimensions come from document layout. Screen capture completion validates decoded size before commit; window capture records the native extent without forcing GetWindowRect border dimensions. MainWindow readouts distinguish original/output dimensions and physical zoom; monitor DPI changes preserve chosen physical magnification or recalculate fit.

FidelityWindowsProbe is a separate explicit diagnostic process entered after Velopack hooks but before normal Editor/mutex initialization. It requires user SDR confirmation, uses a real pattern/window/full-display capture for each observed monitor, checks clipboard and renderer, and writes local JSON. It never changes Windows display scaling or claims a missing scale has passed. Linux tests simulate coordinate math only. The diagnostic is not automatic capture on app startup.
