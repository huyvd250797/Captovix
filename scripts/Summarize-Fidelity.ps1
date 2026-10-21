param([string]$Root = (Join-Path $env:LOCALAPPDATA 'HuyVo/Captovix/FidelityChecks'))
$ErrorActionPreference = 'Stop'
$reports = @()
if (Test-Path $Root) {
    foreach ($file in Get-ChildItem $Root -Filter report.json -Recurse -File) {
        try { $r = Get-Content $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json; if ($r.Version -eq '1.6.1' -and $r.PassedThisRun) { $reports += $r } } catch { Write-Warning "Unreadable report: $($file.FullName)" }
    }
}
$observed = @($reports | ForEach-Object { $_.ObservedDisplays } | Where-Object { $_.Passed })
$scales = @($observed | ForEach-Object { [int][Math]::Round($_.WindowDpiX / 96 * 100) } | Sort-Object -Unique)
$missing = @(100,125,150,175,200 | Where-Object { $_ -notin $scales })
$negative = @($observed | Where-Object { $_.Bounds.X -lt 0 -or $_.Bounds.Y -lt 0 }).Count -gt 0
Write-Host "Passing runs for 1.6.1: $($reports.Count)"
Write-Host "Observed passing scales (%): $($scales -join ', ')"
Write-Host "Missing required scales (%): $($missing -join ', ')"
Write-Host "Passing negative-origin monitor observed: $negative"
Write-Host 'Editor drag between monitors, clipboard in destination apps, HDR state and install/update still need the manual checklist.'
if ($reports.Count -eq 0 -or $missing.Count -gt 0 -or -not $negative) { Write-Host 'Acceptance matrix: INCOMPLETE'; exit 2 }
Write-Host 'Recorded capture matrix covered. Manual acceptance is still pending.'
