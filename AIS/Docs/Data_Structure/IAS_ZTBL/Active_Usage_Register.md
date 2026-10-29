# Active Table, Procedure, and Package Usage Register

This register isolates what the running IAS codebase actually uses today.

Primary evidence sources:

- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/dbconnection_method_register_active.csv`
- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/active_procedure_package_register.csv`
- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/active_object_register.csv`

## Summary

| Area | Count |
| --- | ---: |
| Confirmed active DBConnection methods | 810 |
| Likely active DBConnection methods | 54 |
| Confirmed active procedure/package calls | 748 |
| Likely active procedure/package calls | 39 |
| Confirmed active schema objects | 232 |
| Likely active schema objects | 1 |

## Highest-Impact Live Packages

| Package | Active Procedure Calls | Why It Matters |
| --- | ---: | --- |
| `PKG_AD` | 179 | Security admin, entity maintenance, checklist setup, reversals, compliance-flow configuration. |
| `PKG_AR` | 91 | Field audit execution, observations, DSA, responsibilities, working papers. |
| `PKG_HD` | 67 | Legacy para, concluding, settlement, review, report upload, CAD request utilities. |
| `PKG_RPT` | 65 | Reporting and dashboard access across live operational data. |
| `PKG_INQ` | 63 | IID complaint, investigation, findings, report, and email workflow. |
| `PKG_AE` | 42 | Auditee response and post-compliance submission/evidence flows. |
| `PKG_FAD` | 40 | FAD legacy para monitoring, references, status change, settlement review. |
| `PKG_PG` | 37 | Audit periods, criteria, engagement planning, team/task setup. |
| `PKG_FRPT` | 32 | Field audit and management-audit report composition and finalization. |
| `PKG_DB` | 28 | Dashboard and analytics-style read models still tied to current operations. |

## Highest-Impact Live Tables

These are the most frequently referenced confirmed-active objects from mapped procedure usage. `DUAL` is excluded because it is Oracle system usage.

| Table | Evidence Count | Notes |
| --- | ---: | --- |
| `T_AUDITEE_ENTITIES` | 179 | Shared backbone for security scope, planning, reporting, and entity administration. |
| `T_AU_ACTIVITY_LOG` | 117 | Still used for operational/system activity reporting. |
| `T_AU_PLAN_ENG` | 92 | Central live engagement table. |
| `T_AU_OBSERVATION` | 78 | Core observation workflow table. |
| `T_AUDITEE_ENTITIES_MAPING` | 62 | Entity hierarchy and scope mapping remain active. |
| `T_RISK` | 57 | Reused across setup, execution, and reporting. |
| `AIS_T_AU_POST_COMPLIANCE` | 57 | Post-compliance is still a current workflow. |
| `T_AU_OBSERVATION_TEXT` | 54 | Observation narrative/history dependency. |
| `T_AU_OLD_PARAS_FAD` | 45 | Legacy paras are still operational. |
| `T_AU_PERIOD` | 40 | Planning period control is live. |
| `T_AUDIT_CHECKLIST_DETAILS` | 39 | Execution/checklist subsystem is active. |
| `T_AUDITEE_ENT_TYPES` | 38 | Entity type configuration is live. |
| `T_AUDIT_CHECKLIST_SUB` | 35 | Checklist sub-structure remains live. |
| `T_AUDIT_CHECKLIST_ANNEXURE` | 31 | Annexure linkage is active. |
| `T_AU_AUDIT_TEAM_TASKLIST` | 30 | Engagement task assignment is live. |
| `T_AUDIT_CHECKLIST` | 30 | Checklist header/configuration is live. |
| `T_AU_ERROR_LOGS` | 28 | Logging tables still appear in active package logic. |
| `T_AU_OBSERVATION_STATUS` | 27 | Observation status is controlled through dedicated storage. |
| `T_AU_TEAM_MEMBERS` | 23 | Team membership is active, not derived only at runtime. |
| `T_USER` | 23 | Security/user master remains central. |

## Module-Wise Active Footprint

### Security and Administration

Active packages:

- `PKG_AD`
- `PKG_LG`

Must-migrate tables:

- `T_USER`
- `T_USER_MAPING`
- `T_GROUPS`
- `T_MENU`
- `T_MENU_PAGES`
- `T_MENU_PAGES_GROUPMAP`
- `T_AU_API_MASTER`
- `T_AU_PAGE_API_MAP`
- `T_AU_ROLE_API_PERMISSION`
- `T_USER_SESSION`
- `T_AU_ACTIVITY_LOG`
- `T_IAS_VERSION_HISTORY`

### Planning and Engagement

Active packages:

- `PKG_PG`
- `PKG_AD`
- `PKG_AR`

Must-migrate tables:

- `T_AU_PERIOD`
- `T_AU_PLAN`
- `T_AU_PLAN_ENG`
- `T_AU_PLAN_ENG_LOG`
- `T_AU_AUDIT_TEAMS`
- `T_AU_TEAM_MEMBERS`
- `T_AU_AUDIT_TEAM_TASKLIST`
- `T_AUDIT_CRITERIA`
- `T_AUDIT_CRITERIA_LOG`

### Execution and Observation

Active packages:

- `PKG_AR`
- `PKG_AD`

Must-migrate tables:

- `T_AUDIT_CHECKLIST`
- `T_AUDIT_CHECKLIST_DETAILS`
- `T_AUDIT_CHECKLIST_SUB`
- `T_AUDIT_CHECKLIST_ANNEXURE`
- `T_AU_OBSERVATION`
- `T_AU_OBSERVATION_TEXT`
- `T_AU_OBSERVATION_STATUS`
- `T_AU_OBSERVATIONS_AUDITEE_RESPONSE`
- `T_AU_OBSERVATIONS_AUDITEE_EVIDENCES`
- responsibility tables linked to observations

### Compliance and Legacy Paras

Active packages:

- `PKG_AE`
- `PKG_FAD`
- `PKG_HD`

Must-migrate tables:

- `AIS_T_AU_POST_COMPLIANCE`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY`
- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE`
- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_CAU`
- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_ARCHIVE`
- `T_AU_OLD_PARAS_FAD`
- `T_AU_OLD_PARAS_FAD_TEXT`
- `T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY`
- `T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY`
- `T_AU_OBSERVATION_OLD_CAD_PARAS`
- `T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT`

### IID and Inquiry

Active packages:

- `PKG_INQ`
- `PKG_ISM`

Must-migrate tables:

- `T_AU_IID_COMPLAINT_HDR`
- active complaint child tables under the `T_AU_IID_*` family
- queue/report storage reached through `PKG_INQ`

### Commercial Audit

Active packages:

- `PKG_COMMERCIAL_AUDIT`
- `PKG_CM`

Must-migrate tables:

- `T_CAU_OM`
- `T_CAU_PDP`
- `T_CAU_ARPSE`
- commercial-audit DAC/PAC detail tables
- CAU-linked compliance evidence tables

### Reporting and Final Report

Active packages:

- `PKG_RPT`
- `PKG_DB`
- `PKG_FRPT`
- `PKG_BAC`

Must-migrate tables:

- `T_FRPT_*`
- report text/statistics/snapshot tables under `PKG_FRPT`
- operational source tables already listed above, because reports read them directly

## Active Objects That Look Historical But Are Still Live

These cannot be dropped purely by naming pattern:

- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_ARCHIVE`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY`
- `T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY`
- `T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY`
- `T_IAS_VERSION_HISTORY`

## Direct SQL Still Present in Application Code

The DBConnection layer is package-heavy, but it is not package-only. Examples:

- `DBConnection.IID.cs` directly checks `T_AU_IID_COMPLAINT_HDR.is_finalized`
- notification methods build business messages by composing results from multiple active DBConnection methods
- file-system attachment helpers exist outside package calls and should be considered in document migration strategy

## Use of This Register for IAS_ZTBL

1. Any object in `active_object_register.csv` is part of the live footprint and must be mapped, merged, or intentionally re-homed in IAS_ZTBL.
2. Any package/procedure in `active_procedure_package_register.csv` should be considered active business logic until replaced by a documented IAS_ZTBL module/service.
3. Method-level migration traceability is already preserved in `dbconnection_method_register_active.csv`.
