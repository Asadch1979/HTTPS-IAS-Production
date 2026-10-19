# Uses the already-built AIS.dll and existing test runtime setup, no extra build.
. "$PSScriptRoot/audit-para-snap-policy.ps1"
$checks=0
$select=[AIS.Controllers.ExecutiveOverviewController].GetMethod('SelectCiaAuditPeriod',[System.Reflection.BindingFlags]'NonPublic,Static')
function Period([int]$Id,[string]$Start,[string]$End) {
    $p=[AIS.Models.AuditPeriodModel]::new()
    $p.AUDITPERIODID=$Id; $p.START_DATE=[datetime]$Start; $p.END_DATE=[datetime]$End
    return $p
}
function ExpectPeriod($Periods,$Requested,$Expected) {
    $chosen=$select.Invoke($null,@([AIS.Models.AuditPeriodModel[]]$Periods,$Requested,[datetime]'2026-10-18'))
    if ($chosen -ne $Expected) { throw "Expected period '$Expected'; got '$chosen'." }
    $script:checks++
}
$current=Period 12 '2026-01-01' '2026-12-31'
$future=Period 13 '2027-01-01' '2027-12-31'
$past=Period 11 '2025-01-01' '2025-12-31'
ExpectPeriod @($future,$current,$past) $null 12
ExpectPeriod @($future,$current,$past) 13 13
ExpectPeriod @($future,$current,$past) 11 11
ExpectPeriod @($future,$current,$past) 999 12
ExpectPeriod @($future,$past) $null 11
ExpectPeriod @($future) $null $null
ExpectPeriod @() $null $null
ExpectPeriod @((Period 14 '2026-10-18' '2026-11-30'),$current) $null 14
ExpectPeriod @((Period 14 '2026-10-01' '2026-10-17'),$current) $null 12
ExpectPeriod @((Period 14 '2026-10-18' '2026-10-18'),$current) $null 14
ExpectPeriod @((Period 14 '2026-12-31' '2026-01-01'),$current) 14 12
$repo=Resolve-Path "$PSScriptRoot/../.."
$dev=Get-Content "$repo/AIS/appsettings.Development.json" -Raw | ConvertFrom-Json
$production=Get-Content "$repo/AIS/appsettings.Production.json" -Raw | ConvertFrom-Json
foreach ($obsolete in @('CiaCommandCenterEnabled','CiaWorkflowDefinitionsVerified')) {
    if ($dev.RoleDashboards -and
        $dev.RoleDashboards.PSObject.Properties.Name -contains $obsolete) {
        throw "Obsolete CIA environment flag remains in Development settings: $obsolete"
    }
    if ($production.RoleDashboards -and
        $production.RoleDashboards.PSObject.Properties.Name -contains $obsolete) {
        throw "Obsolete CIA environment flag remains in Production settings: $obsolete"
    }
    $checks+=2
}
Write-Output "$checks CIA period/configuration checks passed."
foreach ($dll in Get-ChildItem (Split-Path $AssemblyPath) -Filter 'Oracle.*.dll') {
    [void][System.Reflection.Assembly]::LoadFrom($dll.FullName)
}
Add-Type -Path "$PSScriptRoot/CiaCommandAccessChecks.cs" -ReferencedAssemblies $references -CompilerOptions '/nowarn:1701'
[CiaCommandAccessChecks]::Run()
