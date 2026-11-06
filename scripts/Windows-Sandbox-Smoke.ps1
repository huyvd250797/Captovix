param([Parameter(Mandatory=$true)][string]$Setup,[Parameter(Mandatory=$true)][string]$Report)
$ErrorActionPreference='Stop'
if ($env:USERNAME -ne 'WDAGUtilityAccount') { throw 'Run this installer smoke only inside Windows Sandbox, not on a working machine.' }
$process=Start-Process -FilePath $Setup -ArgumentList '/S' -Wait -PassThru
if($process.ExitCode -ne 0){throw ('Setup failed: '+$process.ExitCode)}
$root=Join-Path $env:LOCALAPPDATA 'HuyVo.Captovix'
$exe=Get-ChildItem $root -Filter Captovix.exe -Recurse | Select-Object -First 1
if(!$exe){throw 'Installed executable missing.'}
Start-Process $exe.FullName
$deadline=[DateTime]::UtcNow.AddSeconds(45);$visible=$false
while([DateTime]::UtcNow -lt $deadline){$app=Get-Process Captovix -ErrorAction SilentlyContinue | Where-Object MainWindowHandle -ne 0 | Select-Object -First 1;if($app){$visible=$true;break};Start-Sleep -Milliseconds 500}
$result=@{version='1.46.4';platform='Windows';scope='Sandbox Setup and visible main window only; not capture/IME/physical scrolling acceptance';setupExit=$process.ExitCode;windowVisible=$visible;timeUtc=[DateTime]::UtcNow.ToString('o');manualAcceptance='PENDING'}
$result|ConvertTo-Json|Set-Content -Path $Report -Encoding UTF8
if(!$visible){throw 'Main window did not become visible; release is blocked.'}
