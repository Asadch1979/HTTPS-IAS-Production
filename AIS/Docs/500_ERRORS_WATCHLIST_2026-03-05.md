# 500 Errors Watchlist (Front-End Containment) - 2026-03-05

## Client Containment Added
- Shared AJAX/fetch safety maps server-side failures to meaningful messages (401/403/500/format errors).
- Full HTML error pages are blocked from being injected into modal/page containers.
- Runtime server-error aggregation is available at window.__iasServerErrorWatchlist (endpoint + method + count + lastStatus + timestamp).

## Priority Endpoint Watchlist (Static Hotspots)
- /ApiCalls/GetZoneBranches (usage matches: 26)
- /Planning/audit_periods (usage matches: 21)
- /ApiCalls/GetDivDepartments (usage matches: 20)
- /ApiCalls/AddAuditPlan (usage matches: 19)
- /ApiCalls/GetAuditTeams (usage matches: 19)
- /Setup/get_sub_entities (usage matches: 18)
- /ApiCalls/get_audit_sub_checklist (usage matches: 16)
- /ApiCalls/getparentrel (usage matches: 16)
- /ApiCalls/getpostplace (usage matches: 16)
- /ApiCalls/get_ais_entities_for_admin_panel_entity_addition (usage matches: 11)
- /Setup/process_transactions (usage matches: 10)
- /ApiCalls/get_auditee_evidence_data (usage matches: 9)
- /ApiCalls/sub_checklist (usage matches: 9)
- /ApiCalls/checklist_details (usage matches: 9)
- /ApiCalls/get_employee_name_from_pp (usage matches: 8)
- /Setup/process_details (usage matches: 7)
- /ApiCalls/update_observation_status (usage matches: 7)
- /ApiCalls/get_responded_obs_evidences (usage matches: 6)
- /ApiCalls/close_draft_audit (usage matches: 6)
- /ApiCalls/get_sub_menu_for_admin_panel (usage matches: 6)
- /Execution/sub_voilation (usage matches: 6)
- /ApiCalls/GetComplaint (usage matches: 6)
- /ApiCalls/get_all_para_text (usage matches: 6)
- /Execution/audit_observation_template (usage matches: 6)
- /ApiCalls/get_responsible_by_lc (usage matches: 5)
- /ApiCalls/get_old_para_compliance_cycle_text (usage matches: 5)
- /ApiCalls/save_observations_cau (usage matches: 5)
- /ApiCalls/get_responsible_by_pp (usage matches: 5)
- /Setup/department_add (usage matches: 4)
- /ApiCalls/update_audit_checklist_detail (usage matches: 4)
- /ApiCalls/add_responsible_for_old_paras?IND_Action= (usage matches: 4)
- /ApiCalls/get_responsible_person_list (usage matches: 4)
- /ApiCalls/get_observation_details_for_status_reversal (usage matches: 4)
- /ApiCalls/get_report_paras (usage matches: 4)
- /ApiCalls/get_address (usage matches: 4)
- /ApiCalls/get_observation_text_branches (usage matches: 4)
- /ApiCalls/get_lc_details (usage matches: 4)
- /ApiCalls/UpdateAuditeeEntity (usage matches: 4)
- /ApiCalls/get_zone_Branches (usage matches: 4)
- /ApiCalls/get_hr_entities_for_admin_panel_entity_addition (usage matches: 4)

## How To Capture Real 500 Producers
1. Reproduce failing pages in browser.
2. Check Network tab for requests with HTTP 500.
3. Capture X-Error-Reference-Id if present.
4. Copy corresponding console entry (Client AJAX issue: { endpoint, method, status, ... }).
5. Export window.__iasServerErrorWatchlist from browser console for grouped endpoint counts.

## Source Artifacts
- AIS/Docs/UI_AJAX_SCAN_JS_2026-03-05.csv
- AIS/Docs/UI_AJAX_SCAN_VIEWS_2026-03-05.csv
- AIS/wwwroot/js/site.js
