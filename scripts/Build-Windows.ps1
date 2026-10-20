param(
    [switch]$SkipInstaller,
    [string]$UpdateSource = '',
    [ValidateSet('GitHub','Https')][string]$UpdateKind = 'GitHub',
    [string]$PreviousReleases = ''
)
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location $projectRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0' }
[xml]$project = Get-Content src/Captovix/Captovix.csproj
$version = [string]$project.Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Use stable three-component version in Captovix.csproj.' }
$artifactRoot = Join-Path $projectRoot 'artifacts'
$payload = Join-Path $artifactRoot 'win-x64'
$releases = Join-Path $artifactRoot 'releases'
foreach ($folder in @($payload,$releases)) {
    if (Test-Path $folder) { Remove-Item $folder -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
}
# Supply the previous release's nupkg files to enable delta generation. Never
# put unreleased/test packages in this directory.
if ($PreviousReleases) { Copy-Item (Join-Path (Resolve-Path $PreviousReleases).Path '*.nupkg') $releases -Force }
dotnet run --project tests/InstallerContractChecks/InstallerContractChecks.csproj -c Release -- $projectRoot
if ($LASTEXITCODE -ne 0) { throw 'Installer argument regression checks failed.' }
foreach ($check in @('GeometryChecks','EditorCoreChecks','UpdateChecks','EditorWindowsChecks')) {
    dotnet run --project "tests/$check/$check.csproj" -c Release -p:Platform=x64
    if ($LASTEXITCODE -ne 0) { throw "$check failed." }
}
dotnet publish src/Captovix/Captovix.csproj -c Release -r win-x64 --self-contained true -p:Platform=x64 -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o $payload
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
Copy-Item docs,licenses -Destination $payload -Recurse -Force
Copy-Item README.md,CHANGELOG.md,THIRD-PARTY-NOTICES.txt -Destination $payload -Force
Copy-Item scripts/Install-Local.ps1,scripts/Install.cmd,scripts/Launch.cmd -Destination $payload -Force
if ($UpdateSource) {
    $uri = [uri]$UpdateSource
    if (-not $uri.IsAbsoluteUri -or $uri.Scheme -ne 'https' -or $uri.UserInfo -or $uri.Query -or $uri.Fragment) { throw 'UpdateSource must be HTTPS without credentials/query/fragment.' }
    if ($UpdateKind -eq 'GitHub' -and ($uri.Host -ne 'github.com' -or -not $uri.IsDefaultPort -or $uri.AbsolutePath.Trim('/').Split('/').Count -ne 2)) { throw 'Use https://github.com/OWNER/REPO for GitHub updates.' }
    @{SourceKind=$UpdateKind;SourceUrl=$UpdateSource.TrimEnd('/');AutoCheck=$true} | ConvertTo-Json | Set-Content (Join-Path $payload 'update-source.json') -Encoding utf8
}
Get-ChildItem $payload -Recurse -Include *.pdb,*.xml | Remove-Item -Force
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'Unable to restore the pinned Velopack CLI.' }
$vpkArgs = @('--skip-updates','pack','--packId','HuyVo.Captovix','--packVersion',$version,'--packDir',$payload,'--mainExe','Captovix.exe','--packTitle','Captovix','--packAuthors','HuyVo','--runtime','win-x64','--channel','win','--icon','src/Captovix/Assets/Captovix.ico','--releaseNotes','docs/RELEASE-NOTES.md','--framework','vcredist143-x64','--noPortable','--outputDir',$releases)
dotnet tool run vpk -- @vpkArgs
if ($LASTEXITCODE -ne 0) { throw 'Velopack packaging failed.' }
# A deliberately plain portable bundle: in-app checks and manual downloads only.
$portable = Join-Path $artifactRoot "Captovix-v$version-Portable-Windows-x64.zip"
if (Test-Path $portable) { Remove-Item $portable }
Compress-Archive -Path "$payload\*" -DestinationPath $portable -CompressionLevel Optimal
if (-not $SkipInstaller) {
    $compiler = Get-Command makensis.exe -ErrorAction SilentlyContinue
    $compilerPath = if ($compiler) { $compiler.Source } else { Join-Path ${env:ProgramFiles(x86)} 'NSIS\makensis.exe' }
    if (-not (Test-Path $compilerPath)) { throw 'Install NSIS 3 to build the migration Setup.exe. Raw Velopack output is available in artifacts/releases.' }
    $prereqRoot = Join-Path $projectRoot 'prerequisites'; New-Item -ItemType Directory -Force $prereqRoot | Out-Null
    $vc = Join-Path $prereqRoot 'vc_redist.x64.exe'
    if (-not (Test-Path $vc)) { Invoke-WebRequest 'https://aka.ms/vs/17/release/vc_redist.x64.exe' -OutFile $vc }
    $sig = Get-AuthenticodeSignature $vc
    if ($sig.Status -ne 'Valid' -or $sig.SignerCertificate.Subject -notmatch 'Microsoft Corporation') { throw 'Visual C++ Runtime signature is not valid.' }
    $core = Join-Path $artifactRoot 'CoreSetup.exe'
    $canonicalSetup = Join-Path $releases 'HuyVo.Captovix-win-Setup.exe'
    Move-Item $canonicalSetup $core -Force
    $setup = Join-Path $artifactRoot "Captovix-v$version-Setup.exe"
    & $compilerPath "/DAPPVERSION=$version" "/DCORESETUP=$core" "/DVCREDIST=$vc" "/DOUTPUT=$setup" installer/Captovix.nsi
    if ($LASTEXITCODE -ne 0) { throw 'Migration installer compilation failed.' }
    Copy-Item $setup $canonicalSetup -Force
}
# Verify a real full package through the production SDK, including bad hash rejection.
$full = Get-Item (Join-Path $releases "HuyVo.Captovix-$version-full.nupkg")
dotnet run --project tests/UpdateChecks/UpdateChecks.csproj -c Release -p:Platform=x64 -- $full.FullName
if ($LASTEXITCODE -ne 0) { throw 'Real package integration checks failed.' }
$releaseZip = Join-Path $artifactRoot "Captovix-v$version-Update-Packages.zip"
if (Test-Path $releaseZip) { Remove-Item $releaseZip }
$releaseAssets = @(
    Get-ChildItem $releases -File | Where-Object { $_.Name -like '*.nupkg' -or $_.Name -in @('HuyVo.Captovix-win-Setup.exe','RELEASES','assets.win.json','releases.win.json') } | ForEach-Object { $_.FullName }
)
Compress-Archive -Path $releaseAssets -DestinationPath $releaseZip -CompressionLevel Optimal
Get-ChildItem $artifactRoot -File | Where-Object Name -ne 'CoreSetup.exe' | ForEach-Object { Get-FileHash $_.FullName -Algorithm SHA256 } | Format-Table
Write-Host 'Build complete. Windows desktop install/update/capture validation is required before broad rollout.'
