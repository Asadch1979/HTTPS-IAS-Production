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

$checks = 0
foreach ($role in @('TOP','CCO','GROUP','RLM','SNAP')) {
    $filter = [AIS.Models.ExecutiveDashboardFilterRequest]::new()
    $filter.StartDate = '2024-02-01'; $filter.EndDate = '2024-02-29'
    $filter.ValidateParentPerspective($role)
    [void]$filter.Validate()
    $checks++
    foreach ($field in @('EntityId','ReportingOfficeId','DimensionId','SelectBy')) {
        $filter = [AIS.Models.ExecutiveDashboardFilterRequest]::new()
        if ($field -eq 'SelectBy') { $filter.$field = 'auditedBy' } else { $filter.$field = 99 }
        $rejected = $false
        try { $filter.ValidateParentPerspective($role) }
        catch { if ($_.Exception.GetBaseException() -is [ArgumentException]) { $rejected = $true } else { throw } }
        if (!$rejected) { throw "$role accepted organizational page parameter $field" }
        # The independent CIA filter contract remains available.
        $filter.ValidateParentPerspective('CIA')
        [void]$filter.Validate()
        $checks += 2
    }
}
Write-Output "$checks compiled parent-filter checks passed."
