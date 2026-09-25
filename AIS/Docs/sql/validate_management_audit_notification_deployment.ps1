[CmdletBinding()]
param(
    [switch]$Deploy,
    [string]$OracleConnection
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$solution = Join-Path $repoRoot 'AIS.sln'

& dotnet build $solution --no-restore
if ($LASTEXITCODE -ne 0) {
    throw 'The .NET build failed; Oracle deployment was not started.'
}

if (-not $Deploy) {
    Write-Host 'Build passed. Use -Deploy -OracleConnection <user/password@service> to run Oracle deployment checks.'
    return
}

if ([string]::IsNullOrWhiteSpace($OracleConnection)) {
    throw '-OracleConnection is required with -Deploy.'
}

$sqlPlus = (Get-Command sqlplus -ErrorAction Stop).Source
Push-Location $PSScriptRoot
try {
    & $sqlPlus -L $OracleConnection '@management_audit_para_notifications_20260925.sql'
    if ($LASTEXITCODE -ne 0) {
        throw 'Oracle deployment or its fail-fast validation checks failed.'
    }

    & $sqlPlus -L $OracleConnection '@management_audit_notification_verification_20260925.sql'
    if ($LASTEXITCODE -ne 0) {
        throw 'Post-deployment evidence query failed.'
    }
}
finally {
    Pop-Location
}
