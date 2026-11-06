param(
    [Parameter(Mandatory=$true)][string]$Manifest,
    [switch]$Compare,
    [string]$DataRoot = (Join-Path $env:LOCALAPPDATA 'HuyVo\Captovix')
)
$ErrorActionPreference = 'Stop'
if (Get-Process Captovix -ErrorAction SilentlyContinue) { throw 'Exit Captovix, including the tray, before checking data.' }

function Entry($Kind, $Id, $File) {
    if (!(Test-Path -LiteralPath $File -PathType Leaf)) { throw "Missing $Kind file for $Id" }
    [pscustomobject]@{ Kind=$Kind; Id=$Id; SHA256=(Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash }
}

$library = Join-Path $DataRoot 'library.json'
$items = @(Get-Content -LiteralPath $library -Raw | ConvertFrom-Json | Sort-Object Id)
if (!$items.Count) { throw 'Seed a representative library before running this test.' }
$files = @(
    Entry 'Library' 'library.json' $library
    foreach ($item in $items) { Entry 'Original' $item.Id $item.FilePath }
    foreach ($edit in (Get-ChildItem -LiteralPath (Join-Path $DataRoot 'Edits') -Filter '*.json' -File | Sort-Object Name)) {
        Entry 'Edit' $edit.Name $edit.FullName
    }
    foreach ($name in @('device.bin','certificate.bin')) {
        Entry 'License' $name (Join-Path $DataRoot "licensing\$name")
    }
)
if (!($files | Where-Object Kind -eq 'Edit')) { throw 'Seed and save at least one edited image first.' }
if ($Compare) {
    $before = @(Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json)
    $diff = @(Compare-Object $before $files -Property Kind,Id,SHA256)
    if ($diff.Count) { $diff | Format-Table; throw 'Upgrade data verification failed.' }
    Write-Host "PASS: $($files.Count) library, original, edit and encrypted license hashes match."
} else {
    if (Test-Path -LiteralPath $Manifest) { throw 'Choose a new manifest path to preserve the original baseline.' }
    ConvertTo-Json -InputObject $files -Depth 3 | Set-Content -LiteralPath $Manifest -Encoding UTF8
    Write-Host "Captured $($files.Count) hashes. License contents are never printed."
}
