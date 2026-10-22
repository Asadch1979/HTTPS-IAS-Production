param([string]$AssemblyPath = "$PSScriptRoot/../../AIS/bin/Debug/net8.0/AIS.dll")
$ErrorActionPreference='Stop'
$runtime=Get-ChildItem "$env:ProgramFiles/dotnet/shared/Microsoft.AspNetCore.App" -Directory | Where-Object Name -Like '8.*' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$assemblies=@{}
foreach ($dll in Get-ChildItem $runtime.FullName -Filter 'Microsoft.*.dll') { $assemblies[$dll.Name]=$dll.FullName }
foreach ($dll in Get-ChildItem (Split-Path $AssemblyPath) -Filter 'Microsoft.*.dll') { $assemblies[$dll.Name]=$dll.FullName }
foreach ($dll in $assemblies.Values) { [void][System.Reflection.Assembly]::LoadFrom($dll) }
[void][System.Reflection.Assembly]::LoadFrom((Resolve-Path $AssemblyPath))
$period=[AIS.Models.AuditPeriodModel]::new()
$period.AUDITPERIODID=57
$period.START_DATE=[datetime]'2026-01-01'; $period.END_DATE=[datetime]'2026-12-31'
$today=[datetime]'2026-10-22'
$f=[AIS.Models.ExecutiveDashboardFilterRequest]::new()
$d=[AIS.Models.CiaCommandPolicy]::ResolvePosition($f,$period,$today)
if ($d -ne [datetime]'2026-10-21' -or $f.SnapDivision -ne '1') { throw 'CIA Pakistan yesterday/default division failed' }
$f.PositionAsOn='2026-05-12'
[void][AIS.Models.CiaCommandPolicy]::ResolvePosition($f,$period,$today)
if ($f.PositionAsOn -ne '2026-05-12') { throw 'Apply overwrote selected date' }
foreach ($bad in @('2027-01-01','2026-10-23','2025-12-31','2026-02-30','22/10/2026')) {
    $f.PositionAsOn=$bad; $rejected=$false
    try { [void][AIS.Models.CiaCommandPolicy]::ResolvePosition($f,$period,$today) } catch { $rejected=$true }
    if (!$rejected) { throw "Accepted invalid cutoff $bad" }
}
$period.START_DATE=[datetime]'2025-01-01';$period.END_DATE=[datetime]'2025-12-31'
$f.PositionAsOn=$null
if ([AIS.Models.CiaCommandPolicy]::ResolvePosition($f,$period,$today) -ne [datetime]'2025-12-31') { throw 'Historical period cutoff failed' }
$f.EntityId=42;$rejected=$false
try { [void][AIS.Models.CiaCommandPolicy]::ResolvePosition($f,$period,$today) } catch { $rejected=$true }
if (!$rejected) { throw 'CIA accepted forged auditee organization selector' }
Write-Output '9 CIA cutoff/default/scope policy checks passed.'
