param([string]$AssemblyPath = "$PSScriptRoot/../../AIS/bin/Debug/net8.0/AIS.dll")
$ErrorActionPreference = 'Stop'
# PowerShell is a console host; load the installed ASP.NET shared framework
# so it can inspect the already-built web assembly without another build.
$aspRuntime = Get-ChildItem "$env:ProgramFiles/dotnet/shared/Microsoft.AspNetCore.App" -Directory |
    Where-Object Name -Like '8.*' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (!$aspRuntime) { throw 'ASP.NET Core 8 shared runtime is required.' }
$assemblies = @{}
foreach ($dll in Get-ChildItem $aspRuntime.FullName -Filter 'Microsoft.*.dll') { $assemblies[$dll.Name]=$dll.FullName }
# AIS's existing EF package references Extensions 9; prefer its built
# dependencies to the older shared-framework version when present.
foreach ($dll in Get-ChildItem (Split-Path $AssemblyPath) -Filter 'Microsoft.*.dll') { $assemblies[$dll.Name]=$dll.FullName }
foreach ($assembly in $assemblies.Values) { [void][System.Reflection.Assembly]::LoadFrom($assembly) }
[void][System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))
$script:checks = 0
function Check([bool]$Condition,[string]$Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
function New-TestFilter([string]$Start,[string]$End,[string]$View='glance') {
    $f = [AIS.Models.ExecutiveDashboardFilterRequest]::new()
    $f.StartDate=$Start; $f.EndDate=$End; $f.View=$View
    return $f
}
function Reject($Filter,[string]$Message) {
    $rejected=$false
    try { [void][AIS.Models.AuditParaSnapPolicy]::Validate($Filter) }
    catch { if ($_.Exception.GetBaseException() -is [ArgumentException]) { $rejected=$true } else { throw } }
    Check $rejected $Message
}
$range=[AIS.Models.AuditParaSnapPolicy]::Validate((New-TestFilter '2024-02-29' '2024-02-29'))
Check ($range.StartDate -eq [datetime]'2024-02-29') 'Leap-day start changed'
Check ($range.EndExclusive -eq [datetime]'2024-03-01') 'Inclusive leap day must end at following midnight'
Check (([datetime]'2024-02-29T23:59:59' -lt $range.EndExclusive) -and !([datetime]'2024-03-01' -lt $range.EndExclusive)) 'End boundary failed'
Reject (New-TestFilter '' '') 'Missing dates accepted'
Reject (New-TestFilter '2024-01-01' '') 'One date accepted'
Reject (New-TestFilter '' '2024-01-01') 'End-only accepted'
Reject (New-TestFilter '2024-03-01' '2024-02-29') 'Reversed dates accepted'
Reject (New-TestFilter '2023-02-29' '2024-01-01') 'Invalid calendar date accepted'
Reject (New-TestFilter '01/01/2024' '2024-01-02') 'Non-ISO date accepted'
Reject (New-TestFilter '2024-01-01' ([AIS.Models.AuditParaSnapPolicy]::Today.AddDays(1).ToString('yyyy-MM-dd'))) 'Future date accepted'
Reject (New-TestFilter '1900-01-01' '2000-01-01') 'Unbounded trend range accepted'
Reject (New-TestFilter '2024-01-01' '2024-01-02' 'command') 'CIA period view accepted on Snap'
$invalid=New-TestFilter '2024-01-01' '2024-01-02'
$invalid.EntityId=-1
Reject $invalid 'Negative entity accepted'
$invalid=New-TestFilter '2024-01-01' '2024-01-02'
$invalid.SelectBy='bankWide'
Reject $invalid 'Unknown dimension accepted'
$default=New-TestFilter '' ''
[AIS.Models.AuditParaSnapPolicy]::ApplyDefault($default)
$yesterday=[AIS.Models.AuditParaSnapPolicy]::Today.AddDays(-1)
Check ($default.EndDate -eq $yesterday.ToString('yyyy-MM-dd')) 'Reset end must be last complete Pakistan day'
Check ($default.StartDate -eq ([datetime]::new($yesterday.Year,1,1)).ToString('yyyy-MM-dd')) 'Reset start must be January 1 of last complete day year'
$explicit=New-TestFilter '2024-02-01' '2024-02-29'
[AIS.Models.AuditParaSnapPolicy]::ApplyDefault($explicit)
Check ($explicit.StartDate -eq '2024-02-01' -and $explicit.EndDate -eq '2024-02-29') 'Explicit dates overwritten'
$half=New-TestFilter '2024-02-01' ''
[AIS.Models.AuditParaSnapPolicy]::ApplyDefault($half)
Reject $half 'Default silently filled one missing date'
$definitions=[AIS.Models.AuditParaSnapPolicy]::Definitions()
Check (($definitions.Id -join ',') -eq 'glance,position,settlement,risk,aging,reports') 'Six independent views missing'
Check (@($definitions | Where-Object { $_.Id -ne 'reports' -and $_.Kpis.Count -eq 9 -and $_.UseLifecycleKpis }).Count -eq 5) 'Lifecycle cards missing from a Snap analysis view'
Check (($definitions | Where-Object Id -eq 'reports').Reports -join ',' -eq 'A,B,C,D,E') 'Report A-E altered'
$routes=$explicit.RouteValues('risk',$null)
Check ($routes['startDate'] -eq '2024-02-01' -and $routes['endDate'] -eq '2024-02-29' -and $routes['view'] -eq 'risk') 'Dates lost during navigation'
Check ([AIS.Models.AuditParaSnapPolicy]::PageId -eq 1000304) 'Runtime page ID differs from reserved permission ID'
Write-Output "$script:checks compiled-policy date/default/navigation checks passed."
$frameworkReferences = @(Get-ChildItem "$PSHOME/ref" -Filter '*.dll' | ForEach-Object FullName)
$aspReferences = @($assemblies.Values)
$references = $frameworkReferences + $aspReferences + @((Resolve-Path -LiteralPath $AssemblyPath).Path)
Add-Type -Path "$PSScriptRoot/AuditParaSnapAccessChecks.cs" -ReferencedAssemblies $references -CompilerOptions '/nowarn:1701'
[AuditParaSnapAccessChecks]::Run()
