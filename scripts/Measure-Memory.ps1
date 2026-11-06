param(
    [Parameter(Mandatory=$true)][string]$Version,
    [Parameter(Mandatory=$true)][string]$Output,
    [ValidateRange(50,2000)][int]$IntervalMs = 100
)

$ErrorActionPreference = 'Stop'
if ([Console]::IsInputRedirected) { throw 'Run in an interactive Windows terminal.' }
if (Get-Process -Name Captovix -ErrorAction SilentlyContinue) { throw 'Close every Captovix instance, including the tray, before measuring.' }
$csvPath = [IO.Path]::GetFullPath($Output)
$writer = [IO.StreamWriter]::new($csvPath,$false,[Text.UTF8Encoding]::new($false))
$clock = [Diagnostics.Stopwatch]::StartNew()
$phase = 'startup'
$cycle = 0
Write-Host 'Launch Captovix now. Press keys in this terminal to mark each phase.'
Write-Host 'I = idle, C = start next cycle, R = start recovery, Q = finish.'
try {
    $writer.WriteLine('Version,ElapsedMs,Phase,Cycle,ProcessCount,WorkingSetBytes,PrivateBytes')
    $writer.AutoFlush = $true
    $running = $true
    while ($running) {
        while ([Console]::KeyAvailable) {
            switch ([Console]::ReadKey($true).Key) {
                'I' { $phase = 'idle' }
                'C' { $cycle++; $phase = 'work' }
                'R' { $phase = 'recovery' }
                'Q' { $running = $false }
            }
            Write-Host "$($clock.ElapsedMilliseconds) ms · $phase · cycle $cycle"
        }
        if (!$running) { break }
        $count = 0
        $working = 0L
        $private = 0L
        foreach ($proc in (Get-Process -Name Captovix -ErrorAction SilentlyContinue)) {
            try {
                $ws = $proc.WorkingSet64
                $pm = $proc.PrivateMemorySize64
                if (!$proc.HasExited) {
                    $count++
                    $working += $ws
                    $private += $pm
                }
            } catch [InvalidOperationException] {
                # The OCR worker can exit between enumeration and sampling.
            } finally {
                $proc.Dispose()
            }
        }
        if ($count -gt 0) {
            $writer.WriteLine("$Version,$($clock.ElapsedMilliseconds),$phase,$cycle,$count,$working,$private")
        }
        Start-Sleep -Milliseconds $IntervalMs
    }
} finally {
    $writer.Dispose()
}

Import-Csv -LiteralPath $csvPath | Group-Object Phase,Cycle | ForEach-Object {
    $rows = @($_.Group)
    $last = $rows[-1]
    $peak = ($rows | ForEach-Object { [long]$_.WorkingSetBytes } | Measure-Object -Maximum).Maximum
    [pscustomobject]@{
        Phase = $last.Phase
        Cycle = $last.Cycle
        Samples = $rows.Count
        PeakWorkingSetMiB = [Math]::Round($peak / 1MB, 1)
        EndWorkingSetMiB = [Math]::Round([long]$last.WorkingSetBytes / 1MB, 1)
        EndPrivateMiB = [Math]::Round([long]$last.PrivateBytes / 1MB, 1)
    }
} | Format-Table -AutoSize
Write-Host "CSV: $csvPath"
