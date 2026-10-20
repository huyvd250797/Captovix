$ErrorActionPreference = 'Stop'
$packageRoot = $PSScriptRoot
$app = Join-Path $packageRoot 'Captovix.exe'
if (-not (Test-Path $app)) { throw 'Extract the complete Portable ZIP first. Do not run inside a ZIP.' }
if (-not [Environment]::Is64BitOperatingSystem) { throw 'Windows x64 is required.' }
$vc = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64' -ErrorAction SilentlyContinue
if (-not $vc -or $vc.Installed -ne 1) {
    Write-Host 'Microsoft Visual C++ Runtime x64 is required. Windows may ask for administrator permission.'
    $download = Join-Path ([IO.Path]::GetTempPath()) ('Captovix-vcredist-' + [guid]::NewGuid().ToString('N') + '.exe')
    try {
        Invoke-WebRequest 'https://aka.ms/vs/17/release/vc_redist.x64.exe' -OutFile $download
        $signature = Get-AuthenticodeSignature $download
        if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Microsoft Corporation') { throw 'Microsoft runtime signature check failed.' }
        $process = Start-Process $download -ArgumentList '/install','/passive','/norestart' -Verb RunAs -Wait -PassThru
        if ($process.ExitCode -notin 0,3010,1638) { throw "Runtime installer failed: $($process.ExitCode)" }
    } finally { Remove-Item $download -Force -ErrorAction SilentlyContinue }
}
# Portable stays portable; install the Setup.exe to enable managed updates.
Write-Host 'Portable prerequisites ready. Use Captovix Setup.exe for installation and in-app updates.'
Start-Process (Join-Path $packageRoot 'Captovix.exe')
