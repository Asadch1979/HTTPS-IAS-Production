# AIS End-to-End Cleanup Summary

- Full API-to-DB register rows: 2151
- Missing checked-in package source rows after confirmed legacy move: 115
- Confirmed legacy moves: 2
- Build: dotnet build AIS.sln --no-restore succeeded with 57 warnings and 0 errors

## Missing Source Rows By Package

| Package | Rows |
|---|---:|
| pkg_ad | 13 |
| pkg_ae | 12 |
| pkg_ai | 15 |
| pkg_AIS | 6 |
| pkg_cm | 1 |
| PKG_COMMERCIAL_AUDIT | 31 |
| pkg_hd | 12 |
| pkg_ISM | 23 |
| PKG_LG | 2 |

## Confirmed Legacy Moves

| Moved | Replacement |
|---|---|
| ApiCallsController.update_compliance_flow | add_compliance_flow -> AddComplianceFlow -> pkg_ad.P_ADD_UPDATE_COMPLIANCE_FLOW |
| DBConnection.UpdateComplianceFlow | AddComplianceFlow -> pkg_ad.P_ADD_UPDATE_COMPLIANCE_FLOW |
