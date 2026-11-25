param([string]$Destination = (Join-Path (Split-Path $PSScriptRoot -Parent) 'media-engine'))
$ErrorActionPreference = 'Stop'
# Pin both archive and executable: builds do not follow the mutable release alias.
$version = '9.0.2'
$archiveHash = '60f467265b1e312373dbcd92200c2618a74850f98d3d078e94296bb3fa2047ba'
$exeHash = '3256173f3f8bffd7df12227c68adf68025edb1832273a9530688a7bb1ed8edec'
$executable = Join-Path $Destination 'ffmpeg.exe'
if ((Test-Path (Join-Path $Destination 'ffprobe.exe')) -and (Test-Path $executable) -and (Test-Path (Join-Path $Destination 'FFmpeg-LICENSE.txt')) -and (Test-Path (Join-Path $Destination 'FFmpeg-build.txt')) -and (Get-FileHash $executable -Algorithm SHA256).Hash.ToLowerInvariant() -eq $exeHash) { return }
$temp = Join-Path ([IO.Path]::GetTempPath()) ('Captovix-Engine-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $archive = Join-Path $temp 'engine.zip'
    Invoke-WebRequest "https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-$version-essentials_build.zip" -OutFile $archive
    if ((Get-FileHash $archive -Algorithm SHA256).Hash.ToLowerInvariant() -ne $archiveHash) { throw 'Media engine archive checksum mismatch.' }
    Expand-Archive $archive (Join-Path $temp 'expanded')
    $source = Join-Path $temp "expanded/ffmpeg-$version-essentials_build"
    $binary = Join-Path $source 'bin/ffmpeg.exe'
    if ((Get-FileHash $binary -Algorithm SHA256).Hash.ToLowerInvariant() -ne $exeHash) { throw 'Media engine executable checksum mismatch.' }
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Copy-Item $binary $executable -Force
    Copy-Item (Join-Path $source 'bin/ffprobe.exe') (Join-Path $Destination 'ffprobe.exe') -Force
    Copy-Item (Join-Path $source 'LICENSE') (Join-Path $Destination 'FFmpeg-LICENSE.txt') -Force
    Copy-Item (Join-Path $source 'README.txt') (Join-Path $Destination 'FFmpeg-build.txt') -Force
} finally { Remove-Item $temp -Recurse -Force }
