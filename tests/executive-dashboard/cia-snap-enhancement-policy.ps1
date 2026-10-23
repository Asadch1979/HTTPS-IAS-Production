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
$rows=[System.Collections.Generic.List[AIS.Models.ExecutiveFilterEntity]]::new()
$i=0
foreach ($name in @('Group','Division','Department','GM Office','Branch')) {
    $i++
    $row=[AIS.Models.ExecutiveFilterEntity]::new()
    $row.EntityId=1000+$i;$row.EntityTypeId=500+$i;$row.EntityTypeName=$name
    $row.EntityName="Entity $i";$row.ReportingOfficeId=2000+$i;$row.ReportingOfficeName="Office $i"
    $row.AuditedById=3000+$i;$row.AuditedByName="Audit office $i"
    $rows.Add($row)
}
$f=[AIS.Models.ExecutiveDashboardFilterRequest]::new()
$f.ValidateParentPerspective('SNAP')
$options=[AIS.Models.ExecutiveDashboardFilterOptions]::FromScopedEntities($rows,$f,$null,$true)
if ($options.Dimensions.Name -join ',' -ne 'Group,Division,Department,GM Office') { throw 'Snap entity type presentation restriction/order failed' }
if ($options.Entities.Count -ne 5) { throw 'Presentation restriction removed reporting entities' }
$f.DimensionId=503;$f.ReportingOfficeId=2003;$f.EntityId=1003
$f.ValidateParentPerspective('SNAP')
$options=[AIS.Models.ExecutiveDashboardFilterOptions]::FromScopedEntities($rows,$f,$null,$true)
if ($options.Entities.Count -ne 1 -or $options.Entities[0].Id -ne 1003) { throw 'Scoped cascade failed' }
foreach ($field in @('DimensionId','ReportingOfficeId','EntityId')) {
    $f=[AIS.Models.ExecutiveDashboardFilterRequest]::new();$f.$field=999999;$rejected=$false
    try { [void][AIS.Models.ExecutiveDashboardFilterOptions]::FromScopedEntities($rows,$f,$null,$true) } catch { $rejected=$true }
    if (!$rejected) { throw "Forged $field accepted" }
}
$f=[AIS.Models.ExecutiveDashboardFilterRequest]::new();$f.DimensionId=505;$rejected=$false
try { [void][AIS.Models.ExecutiveDashboardFilterOptions]::FromScopedEntities($rows,$f,$null,$true) } catch { $rejected=$true }
if (!$rejected) { throw 'Restricted type ID accepted' }
$f.SelectBy='auditedBy';$f.DimensionId=3005
$options=[AIS.Models.ExecutiveDashboardFilterOptions]::FromScopedEntities($rows,$f,$null,$true)
if ($options.Entities[0].Id -ne 1005) { throw 'Audited By incorrectly uses entity-type restriction' }
$f=[AIS.Models.ExecutiveDashboardFilterRequest]::new();$f.EntityId=1003;$rejected=$false
try { $f.ValidateParentPerspective('TOP') } catch { $rejected=$true }
if (!$rejected) { throw 'Unrelated TOP parent contract weakened' }
Write-Output '9 Snap presentation/cascade/forged-scope policy checks passed.'
$row=[AIS.Models.AuditParaSnapPerformanceRow]::new()
$row.Position=[AIS.Models.ExecutiveKpiLifecycle]::new()
$row.Position.OpeningEstimated=$true
$row.Position.OpeningOpenParas=80;$row.Position.ParaAdded=20;$row.Position.ParasSettled=30;$row.Position.ClosingOpenParas=72
if ($row.SettlementPercentage -ne 30 -or $row.NetChange -ne -8 -or $row.ArithmeticDifference -ne 2) { throw 'Comparison rate/net change/reconciliation changed' }
$row.Position.ParasSettled=101
if ($null -ne $row.SettlementPercentage) { throw 'Incomparable settlement denominator accepted' }
$row.Position.OpeningOpenParas=0;$row.Position.ParaAdded=0;$row.Position.ParasSettled=0
if ($null -ne $row.SettlementPercentage) { throw 'Zero denominator presented as verified percentage' }
$row.Position.OpeningOpenParas=80;$row.Position.OpeningEstimated=$false
if ($null -ne $row.SettlementPercentage) { throw 'Unavailable historical baseline presented as verified rate' }
Write-Output '4 performance denominator/reconciliation checks passed.'
$references=@(Get-ChildItem "$PSHOME/ref" -Filter '*.dll' | ForEach-Object FullName)+@($assemblies.Values)+@((Resolve-Path $AssemblyPath).Path)
Add-Type -Path "$PSScriptRoot/AuditParaSnapAccessChecks.cs" -ReferencedAssemblies $references -CompilerOptions '/nowarn:1701'
[AuditParaSnapAccessChecks]::Run()
foreach ($dll in Get-ChildItem (Split-Path $AssemblyPath) -Filter 'itext*.dll') { [void][System.Reflection.Assembly]::LoadFrom($dll.FullName) }
$report=[AIS.Models.ExecutiveReportResult]::new()
$report.DashboardType='CIA';$report.Filters=[AIS.Models.ExecutiveDashboardFilterRequest]::new()
$report.Rows=[System.Collections.Generic.List[AIS.Models.ExecutiveReportRow]]::new()
$report.Filters.StartDate='2026-01-01';$report.Filters.EndDate='2026-10-21'
$record=[AIS.Models.ExecutiveReportRow]::new();$record.RowKind='DETAIL';$record.ItemName='<script>unsafe</script>'
$record.GroupName='GM Field Offices';$record.OutstandingParas=12
$report.Rows.Add($record)
$htmlMethod=[AIS.Services.ExecutiveReportsPdfGenerator].GetMethod('BuildHtml',[System.Reflection.BindingFlags]'NonPublic,Static')
$snapHtml=$htmlMethod.Invoke($null,@('B',$report,$true))
$otherHtml=$htmlMethod.Invoke($null,@('B',$report,$false))
if ($snapHtml -match '<th>(Compliance|Audit|Unknown)</th>') { throw 'Snap PDF still displays settlement source columns' }
if ($otherHtml -notmatch '<th>Compliance</th>' -or $otherHtml -notmatch '<th>Audit</th>' -or $otherHtml -notmatch '<th>Unknown</th>') { throw 'Unrelated PDF presentation changed' }
if ($snapHtml -match '<script>unsafe</script>' -or $snapHtml -notmatch '&lt;script&gt;unsafe&lt;/script&gt;') { throw 'PDF stored labels are not encoded' }
$body=[regex]::Match($snapHtml,'<tbody>([\s\S]*?)</tbody>').Groups[1].Value
if ([regex]::Matches($body,'<td\b').Count -ne 11) { throw 'Snap PDF row/header columns differ' }
Write-Output '4 PDF scope/column/encoding checks passed.'
