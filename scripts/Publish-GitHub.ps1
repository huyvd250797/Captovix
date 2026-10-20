param([Parameter(Mandatory=$true)][string]$RepoUrl,[switch]$Publish)
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
$uri = [uri]$RepoUrl
if (-not $uri.IsAbsoluteUri -or $uri.Scheme -ne 'https' -or $uri.Host -ne 'github.com' -or -not $uri.IsDefaultPort -or $uri.UserInfo -or $uri.Query -or $uri.Fragment -or $uri.AbsolutePath.Trim('/').Split('/').Count -ne 2) { throw 'RepoUrl must be https://github.com/OWNER/REPO.' }
if (-not $env:VPK_TOKEN) { throw 'Set VPK_TOKEN in the publishing shell only. Never put it in app config, source, or screenshots.' }
if (-not (Test-Path artifacts/releases/releases.win.json)) { throw 'Build-Windows.ps1 must complete first.' }
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'Tool restore failed.' }
# Token comes from the CLI documented VPK_TOKEN environment variable.
$publishArgs = @('--skip-updates','upload','github','--repoUrl',$RepoUrl.TrimEnd('/'),'--outputDir','artifacts/releases','--channel','win')
if ($Publish) { $publishArgs += '--publish' }
dotnet tool run vpk -- @publishArgs
if ($LASTEXITCODE -ne 0) { throw 'Release upload failed.' }
if (-not $Publish) { Write-Host 'Draft created. Review assets and release notes in GitHub, then publish when ready.' }
