param([switch]$SkipInstaller)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location $projectRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install .NET 10 SDK before building: https://dotnet.microsoft.com/download/dotnet/10.0' }
$artifactRoot = Join-Path $projectRoot 'artifacts'
$payload = Join-Path $artifactRoot 'win-x64'
New-Item -ItemType Directory -Force -Path $payload | Out-Null
dotnet run --project tests/GeometryChecks/GeometryChecks.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Geometry checks failed.' }
dotnet publish src/CaptureFoundation/CaptureFoundation.csproj -c Release -r win-x64 --self-contained true -p:Platform=x64 -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o $payload
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
Copy-Item docs (Join-Path $payload 'docs') -Recurse -Force
Copy-Item licenses (Join-Path $payload 'licenses') -Recurse -Force
Copy-Item README.md,THIRD-PARTY-NOTICES.txt -Destination $payload -Force
Copy-Item scripts/Install-Local.ps1,scripts/Install.cmd,scripts/Launch.cmd -Destination $payload -Force
Get-ChildItem $payload -Recurse -Include *.pdb,*.xml | Remove-Item -Force
$portable = Join-Path $artifactRoot 'Capture-Foundation-v0.1.0-Portable-Windows-x64.zip'
if (Test-Path $portable) { Remove-Item $portable }
Compress-Archive -Path "$payload\*" -DestinationPath $portable -CompressionLevel Optimal
if (-not $SkipInstaller) {
    $compiler = Get-Command makensis.exe -ErrorAction SilentlyContinue
    $compilerPath = if ($compiler) { $compiler.Source } else { $null }
    if (-not $compilerPath) {
        $candidate = Join-Path ${env:ProgramFiles(x86)} 'NSIS\makensis.exe'
        if (Test-Path $candidate) { $compilerPath = $candidate }
    }
    if (-not $compilerPath) { throw 'Portable built. Install NSIS 3 to create Setup.exe: https://nsis.sourceforge.io/Download' }
    $prereqRoot = Join-Path $projectRoot 'prerequisites'
    New-Item -ItemType Directory -Force $prereqRoot | Out-Null
    $vc = Join-Path $prereqRoot 'vc_redist.x64.exe'
    if (-not (Test-Path $vc)) { Invoke-WebRequest 'https://aka.ms/vs/17/release/vc_redist.x64.exe' -OutFile $vc }
    if ((Get-AuthenticodeSignature $vc).Status -ne 'Valid') { throw 'Visual C++ Runtime signature is not valid. Download it again from Microsoft.' }
    $setup = Join-Path $artifactRoot 'Capture-Foundation-v0.1.0-Setup.exe'
    & $compilerPath "/DPAYLOAD=$payload" "/DVCREDIST=$vc" "/DOUTPUT=$setup" installer/CaptureFoundation.nsi
    if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
}
Get-ChildItem $artifactRoot -File | ForEach-Object { Get-FileHash $_.FullName -Algorithm SHA256 } | Format-Table
Write-Host 'Build complete. Runtime validation on Windows is still required.'
