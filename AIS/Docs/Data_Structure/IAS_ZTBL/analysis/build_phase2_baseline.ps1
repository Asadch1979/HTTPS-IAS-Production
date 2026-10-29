Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$analysisDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$baseDir = Split-Path -Parent $analysisDir

$legacyColsPath = Join-Path $analysisDir 'legacy_table_columns.csv'
$targetColsPath = Join-Path $analysisDir 'target_table_columns.csv'
$procPath = Join-Path $analysisDir 'active_procedure_package_register.csv'
$activeObjectsPath = Join-Path $analysisDir 'active_object_register.csv'
$methodRegisterPath = Join-Path $analysisDir 'dbconnection_method_register.csv'

$legacyCols = Import-Csv $legacyColsPath
$targetCols = Import-Csv $targetColsPath
$procs = Import-Csv $procPath
$activeObjects = Import-Csv $activeObjectsPath
$methodRegister = Import-Csv $methodRegisterPath

$existingTables = @{}
$existingCols = @{}
foreach ($row in $targetCols) {
    $table = $row.TableName.ToLower()
    $column = $row.ColumnName.ToLower()
    $existingTables[$table] = $true
    if (-not $existingCols.ContainsKey($table)) {
        $existingCols[$table] = @{}
    }
    $existingCols[$table][$column] = $true
}

function Get-PkColumn {
    param([string]$TargetTable)
    if ([string]::IsNullOrWhiteSpace($TargetTable)) { return '' }
    if ($TargetTable -notmatch '^tbl_') { return '' }
    return ($TargetTable.Substring(4) + '_id').ToLower()
}

function Get-TargetAvailability {
    param(
        [string]$TargetTable,
        [string]$TargetColumn
    )

    if ([string]::IsNullOrWhiteSpace($TargetTable) -or $TargetTable -in @('REVIEW_REQUIRED', 'LEGACY_ARCHIVE_ONLY')) {
        return 'ReviewRequired'
    }

    $tableKey = $TargetTable.ToLower()
    if (-not $existingTables.ContainsKey($tableKey)) {
        return 'RequiredTable'
    }

    if ([string]::IsNullOrWhiteSpace($TargetColumn)) {
        return 'ExistingTable'
    }

    $columnKey = $TargetColumn.ToLower()
    if ($existingCols[$tableKey].ContainsKey($columnKey)) {
        return 'ExistingTableExistingColumn'
    }

    return 'ExistingTableRequiredColumn'
}

function Get-ProcedureAction {
    param($Row)

    $package = [string]$Row.PackageName
    $proc = [string]$Row.ProcedureCall
    $package = $package.ToUpper()
    $proc = $proc.ToUpper()
    $hasWrites = -not [string]::IsNullOrWhiteSpace($Row.WriteObjects)
    $action = 'redesign'
    $target = 'pkg_common'
    $rationale = 'Transactional logic should be reimplemented on IAS_ZTBL.'

    switch ($package) {
        'PKG_CM' {
            $action = 'retire'
            $target = 'pkg_commercial_audit'
            $rationale = 'Legacy commercial-audit package is superseded by the consolidated commercial-audit module.'
        }
        'PKG_AIS_EMAIL' {
            $action = 'retire'
            $target = 'pkg_planning / pkg_notify'
            $rationale = 'Email-side duplication should be absorbed into the notify pipeline and planning module.'
        }
        'PKG_LG' {
            $action = 'wrap'
            $target = 'pkg_sec'
            $rationale = 'Authentication and session calls should be isolated behind a compatibility wrapper during cutover.'
        }
        'PKG_RPT' {
            $action = if ($hasWrites) { 'wrap' } else { 'keep as-is temporarily' }
            $target = 'pkg_report'
            $rationale = 'Read-heavy reporting can be bridged while IAS_ZTBL reporting views and marts are built.'
        }
        'PKG_DB' {
            $action = if ($hasWrites) { 'wrap' } else { 'keep as-is temporarily' }
            $target = 'pkg_report'
            $rationale = 'Dashboard/report composition reads can remain temporarily if compatibility views are supplied.'
        }
        'PKG_BAC' {
            $action = if ($hasWrites) { 'wrap' } else { 'keep as-is temporarily' }
            $target = 'pkg_report'
            $rationale = 'BAC reporting is read-mostly and can be preserved behind compatibility views during transition.'
        }
        'PKG_AI' {
            $action = if ($hasWrites) { 'wrap' } else { 'keep as-is temporarily' }
            $target = 'pkg_sampling'
            $rationale = 'Source extraction procedures can remain temporarily while sampling entities are normalized.'
        }
        'PKG_ISM' {
            $action = 'wrap'
            $target = 'pkg_sampling / pkg_iid'
            $rationale = 'ISM-backed exception retrieval should be isolated behind a wrapper because it spans sampling and IID.'
        }
        'PKG_SM' {
            $action = if ($hasWrites) { 'redesign' } else { 'wrap' }
            $target = 'pkg_sampling'
            $rationale = if ($hasWrites) { 'Sampling writes should move to normalized IAS_ZTBL sampling tables.' } else { 'Read-side exception sampling can be wrapped during migration.' }
        }
        'PKG_FRPT' {
            $action = if ($proc -match 'SAVE_|FINALIZE') { 'redesign' } else { 'wrap' }
            $target = 'pkg_report'
            $rationale = if ($proc -match 'SAVE_|FINALIZE') { 'Field-audit report composition is a live write workflow and should be rebuilt on IAS_ZTBL report tables.' } else { 'Read-side FRPT procedures can be wrapped while the new report model is phased in.' }
        }
        'PKG_COMMERCIAL_AUDIT' {
            $action = if ($proc -match 'SAVE_') { 'redesign' } else { 'wrap' }
            $target = 'pkg_commercial_audit'
            $rationale = if ($proc -match 'SAVE_') { 'Commercial-audit writes must move to normalized IAS_ZTBL commercial tables.' } else { 'Read-side commercial-audit procedures can be wrapped temporarily.' }
        }
        'PKG_INQ' {
            if ($proc -match 'ENQUEUE_EMAIL|GET_EMAIL_QUEUE|MARK_EMAIL') {
                $action = 'split'
                $target = 'pkg_iid + pkg_notify'
                $rationale = 'IID notification responsibilities should be separated from complaint workflow logic.'
            }
            elseif ($hasWrites) {
                $action = 'redesign'
                $target = 'pkg_iid'
                $rationale = 'IID transactional workflow must be reimplemented on normalized complaint/investigation tables.'
            }
            else {
                $action = 'wrap'
                $target = 'pkg_iid'
                $rationale = 'Read-side IID procedures can be wrapped while IAS_ZTBL complaint workflow is introduced.'
            }
        }
        'PKG_AD' {
            if ($hasWrites) {
                $action = 'split'
                if ($proc -match 'USER|GROUP|ROLE|PAGE|API|MENU|PASSWORD') {
                    $target = 'pkg_sec + pkg_admin'
                }
                elseif ($proc -match 'ENTITY|HR_|GM_|RPT_OFFICE|COMPLIANCE_FLOW|HOLIDAY') {
                    $target = 'pkg_entity + pkg_master'
                }
                else {
                    $target = 'pkg_admin + pkg_execution + pkg_planning'
                }
                $rationale = 'PKG_AD mixes security, setup, entity, checklist, and reversal logic and should be split by module.'
            }
            else {
                $action = 'wrap'
                $target = 'pkg_sec / pkg_admin / pkg_entity'
                $rationale = 'Read-only admin/setup procedures can be wrapped while new IAS_ZTBL tables are phased in.'
            }
        }
        'PKG_AR' {
            if ($hasWrites) {
                $action = if ($proc -match 'DSA|RESPONIBILITY|RESPONSIBILITY|JOINING|VOUCHER|LOANCASE|ACCOUNTOPENING|CASHCOUNTER|FIXEDASSETS') { 'split' } else { 'redesign' }
                $target = if ($proc -match 'DSA|RESPONIBILITY|RESPONSIBILITY') { 'pkg_observation + pkg_execution' } else { 'pkg_execution / pkg_observation / pkg_workpaper' }
                $rationale = 'Execution, observation, DSA, and working-paper writes should be separated into clearer module packages.'
            }
            else {
                $action = 'wrap'
                $target = 'pkg_execution / pkg_observation'
                $rationale = 'Read-side execution procedures can be wrapped during IAS_ZTBL cutover.'
            }
        }
        'PKG_AE' {
            $action = if ($hasWrites) { 'redesign' } else { 'wrap' }
            $target = 'pkg_compliance'
            $rationale = if ($hasWrites) { 'Auditee response and post-compliance writes must move to the normalized compliance model.' } else { 'Read-side compliance procedures can be wrapped during migration.' }
        }
        'PKG_FAD' {
            if ($proc -match 'GET_MANUAL|GET_REFERENCE|SEARCHREFERENCES') {
                $action = 'wrap'
                $target = 'pkg_reference'
                $rationale = 'Reference/manual lookups can be wrapped while the new reference model is loaded.'
            }
            elseif ($hasWrites) {
                $action = 'redesign'
                $target = 'pkg_compliance'
                $rationale = 'Legacy para status, settlement, and assignment writes should move to normalized para/compliance tables.'
            }
            else {
                $action = 'wrap'
                $target = 'pkg_compliance'
                $rationale = 'Read-side FAD procedures can be wrapped during migration.'
            }
        }
        'PKG_HD' {
            if ($hasWrites) {
                $action = 'split'
                $target = 'pkg_compliance + pkg_report'
                $rationale = 'PKG_HD mixes para/compliance state changes with report-upload and approval concerns and should be split.'
            }
            else {
                $action = 'wrap'
                $target = 'pkg_compliance / pkg_report'
                $rationale = 'Read-side HD procedures can be wrapped while IAS_ZTBL compliance and report modules come online.'
            }
        }
        'PKG_PG' {
            $action = if ($hasWrites) { 'redesign' } else { 'wrap' }
            $target = 'pkg_planning'
            $rationale = if ($hasWrites) { 'Planning writes should move directly to normalized IAS_ZTBL planning tables.' } else { 'Read-side planning procedures can be wrapped temporarily.' }
        }
        default {
            $action = if ($hasWrites) { 'redesign' } else { 'wrap' }
            $target = 'pkg_common'
            $rationale = if ($hasWrites) { 'Legacy write procedure requires IAS_ZTBL redesign.' } else { 'Legacy read procedure can be wrapped during transition.' }
        }
    }

    [pscustomobject]@{
        Action = $action
        TargetPackage = $target
        Rationale = $rationale
    }
}

function Resolve-LegacyTableTarget {
    param([string]$LegacyTable)

    $table = [string]$LegacyTable
    $table = $table.ToUpper()

    switch -Regex ($table) {
        '^T_USER$' { return @{ TargetTable = 'tbl_user'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Core user account.' } }
        '^T_USER_MAPING$' { return @{ TargetTable = 'tbl_user_scope'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Entity/user scope crosswalk.' } }
        '^T_USER_GROUP_MAP$' { return @{ TargetTable = 'tbl_role_permission'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy page assignment bridge.' } }
        '^T_GROUPS$' { return @{ TargetTable = 'tbl_role'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy role/group catalog.' } }
        '^T_GROUP_RIGHTS$' { return @{ TargetTable = 'tbl_permission'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy role responsibility metadata.' } }
        '^T_MENU$' { return @{ TargetTable = 'tbl_application_page'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy menu catalog.' } }
        '^T_MENU_PAGES$' { return @{ TargetTable = 'tbl_application_page'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy page catalog.' } }
        '^T_AU_API_MASTER$' { return @{ TargetTable = 'tbl_api_endpoint'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy API catalog.' } }
        '^T_AU_PAGE_API_MAP$' { return @{ TargetTable = 'tbl_permission'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Page/API action mapping.' } }
        '^T_AU_ROLE_API_PERMISSION$' { return @{ TargetTable = 'tbl_role_permission'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Role-to-API permission bridge.' } }
        '^T_USER_SESSION$' { return @{ TargetTable = 'tbl_user_session'; Domain = 'Security'; DefaultHistory = 'CurrentState'; Notes = 'Legacy session audit.' } }
        '^T_AU_ACTIVITY_LOG$' { return @{ TargetTable = 'tbl_activity_log'; Domain = 'Security'; DefaultHistory = 'WorkflowEvent'; Notes = 'DB-side activity log; current SQL needs explicit table.' } }
        '^T_IAS_VERSION_HISTORY$' { return @{ TargetTable = 'tbl_release_version_history'; Domain = 'Security'; DefaultHistory = 'History'; Notes = 'Version-history catalog; current SQL needs explicit table.' } }

        '^T_AUDITEE_ENT_TYPES$' { return @{ TargetTable = 'tbl_entity_type'; Domain = 'Entity'; DefaultHistory = 'CurrentState'; Notes = 'Entity-type master.' } }
        '^T_AUDITEE_ENTITIES$' { return @{ TargetTable = 'tbl_entity'; Domain = 'Entity'; DefaultHistory = 'CurrentState'; Notes = 'Core entity master.' } }
        '^T_AUDITEE_ENTITIES_MAPING$' { return @{ TargetTable = 'tbl_entity_relation'; Domain = 'Entity'; DefaultHistory = 'CurrentState'; Notes = 'Entity relationship mapping.' } }
        '^T_AUDITEE_ENTITIES_SIZE' { return @{ TargetTable = 'tbl_entity_size_profile'; Domain = 'Entity'; DefaultHistory = 'History'; Notes = 'Size-band profile requires DDL extension.' } }
        '^T_AUDITEE_ENTITIES_RISK' { return @{ TargetTable = 'tbl_entity_risk_profile'; Domain = 'Entity'; DefaultHistory = 'History'; Notes = 'Risk-profile history requires DDL extension.' } }
        '^T_RISK$' { return @{ TargetTable = 'tbl_lookup_value'; Domain = 'Entity'; DefaultHistory = 'CurrentState'; Notes = 'Risk master normalized as lookup values.' } }
        '^T_MANUAL_MASTER$' { return @{ TargetTable = 'tbl_reference_document'; Domain = 'Reference'; DefaultHistory = 'CurrentState'; Notes = 'Manual master.' } }
        '^T_MANUAL_INDEX$' { return @{ TargetTable = 'tbl_reference_document_version'; Domain = 'Reference'; DefaultHistory = 'CurrentState'; Notes = 'Manual index/detail content.' } }

        '^T_AU_PERIOD$' { return @{ TargetTable = 'tbl_audit_period'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Audit period master.' } }
        '^T_AU_PLAN$' { return @{ TargetTable = 'tbl_audit_plan'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Plan header requires DDL extension.' } }
        '^T_AU_PLAN_ENG$' { return @{ TargetTable = 'tbl_engagement'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Engagement master.' } }
        '^T_AU_PLAN_ENG_LOG$' { return @{ TargetTable = 'tbl_workflow_event'; Domain = 'Planning'; DefaultHistory = 'WorkflowEvent'; Notes = 'Engagement change log.' } }
        '^T_AU_AUDIT_TEAMS$' { return @{ TargetTable = 'tbl_engagement_team'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Team header requires DDL extension.' } }
        '^T_AU_TEAM_MEMBERS$' { return @{ TargetTable = 'tbl_engagement_member'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Engagement team members.' } }
        '^T_AU_AUDIT_TEAM_TASKLIST$' { return @{ TargetTable = 'tbl_engagement_task'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Engagement task list.' } }
        '^T_AUDIT_CRITERIA$' { return @{ TargetTable = 'tbl_plan_criteria'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Criteria matrix.' } }
        '^T_AUDIT_CRITERIA_LOG$' { return @{ TargetTable = 'tbl_engagement_criteria_history'; Domain = 'Planning'; DefaultHistory = 'History'; Notes = 'Criteria history requires DDL extension.' } }
        '^T_AUDIT_BUDGET$' { return @{ TargetTable = 'tbl_engagement_budget'; Domain = 'Planning'; DefaultHistory = 'CurrentState'; Notes = 'Engagement budget requires DDL extension.' } }

        '^T_AUDIT_CHECKLIST$' { return @{ TargetTable = 'tbl_checklist_section'; Domain = 'Execution'; DefaultHistory = 'CurrentState'; Notes = 'Checklist heading.' } }
        '^T_AUDIT_CHECKLIST_SUB$' { return @{ TargetTable = 'tbl_checklist_sub_item'; Domain = 'Execution'; DefaultHistory = 'CurrentState'; Notes = 'Checklist sub-item requires DDL extension.' } }
        '^T_AUDIT_CHECKLIST_DETAILS$' { return @{ TargetTable = 'tbl_checklist_item'; Domain = 'Execution'; DefaultHistory = 'CurrentState'; Notes = 'Checklist detail item.' } }
        '^T_AUDIT_CHECKLIST_ANNEXURE$' { return @{ TargetTable = 'tbl_checklist_annexure'; Domain = 'Execution'; DefaultHistory = 'CurrentState'; Notes = 'Annexure mapping requires DDL extension.' } }
        '^T_AUDIT_CHECKLIST_.*CIRCULAR' { return @{ TargetTable = 'tbl_observation_reference'; Domain = 'Reference'; DefaultHistory = 'CurrentState'; Notes = 'Checklist circular linkage.' } }

        '^T_AU_OBSERVATION$' { return @{ TargetTable = 'tbl_observation'; Domain = 'Observation'; DefaultHistory = 'CurrentState'; Notes = 'Core observation table.' } }
        '^T_AU_OBSERVATION_(TEXT|GIST|MAN)$' { return @{ TargetTable = 'tbl_observation_detail'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Observation text/detail history.' } }
        '^T_AU_OBSERVATION_ASSIGNEDTO$' { return @{ TargetTable = 'tbl_observation_assignment'; Domain = 'Observation'; DefaultHistory = 'CurrentState'; Notes = 'Observation assignee bridge.' } }
        '^T_AU_OBSERVATION_.*RESPON' { return @{ TargetTable = 'tbl_observation_assignment'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Responsibility lifecycle data.' } }
        '^T_AU_OBSERVATIONS_AUDITEE_RESPONSE$' { return @{ TargetTable = 'tbl_observation_response'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Auditee response history.' } }
        '^T_AU_OBSERVATIONS_AUDITEE_EVIDENCES$' { return @{ TargetTable = 'tbl_observation_evidence'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Observation evidence requires DDL extension.' } }
        '^T_AU_OBSERVATIONS_AUDITOR_(RECOMMENDATION|REPLY|RESPONSE)$' { return @{ TargetTable = 'tbl_observation_response'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Auditor response/recommendation stream.' } }
        '^T_AU_OBSERVATION_FINAL_RECCOMENDATION$' { return @{ TargetTable = 'tbl_observation_recommendation'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Final recommendation.' } }
        '^T_AU_OBSERVATION_UPDATED_REFERENCE$' { return @{ TargetTable = 'tbl_observation_reference'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'Observation reference updates.' } }
        '^T_AU_DSA' { return @{ TargetTable = 'tbl_observation_dsa'; Domain = 'Observation'; DefaultHistory = 'History'; Notes = 'DSA workflow requires dedicated DDL extension.' } }
        '^T_AU_AUDIT_JOINING$' { return @{ TargetTable = 'tbl_working_paper'; Domain = 'Execution'; DefaultHistory = 'History'; Notes = 'Joining report as working paper.' } }

        '^AIS_T_AU_POST_COMPLIANCE$' { return @{ TargetTable = 'tbl_compliance_case'; Domain = 'Compliance'; DefaultHistory = 'CurrentState'; Notes = 'Current compliance case.' } }
        '^AIS_T_AU_POST_COMPLIANCE_TEXT' { return @{ TargetTable = 'tbl_compliance_action'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Compliance narrative/action text.' } }
        '^AIS_T_AU_POST_COMPLIANCE_EVIDENCE' { return @{ TargetTable = 'tbl_compliance_evidence'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Compliance evidence requires DDL extension.' } }
        '^AIS_T_AU_POST_COMPLIANCE_HISTORY$' { return @{ TargetTable = 'tbl_compliance_case_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Compliance case history requires DDL extension.' } }
        '^T_AU_OLD_PARAS_FAD$' { return @{ TargetTable = 'tbl_para_case'; Domain = 'Compliance'; DefaultHistory = 'CurrentState'; Notes = 'Legacy para current-state record requires DDL extension.' } }
        '^T_AU_OLD_PARAS_FAD_TEXT$' { return @{ TargetTable = 'tbl_para_case_text'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Legacy para text history requires DDL extension.' } }
        '^T_AU_OLD_PARAS_FAD_RESPONSIBILITY_ASSIGNED$' { return @{ TargetTable = 'tbl_para_assignment'; Domain = 'Compliance'; DefaultHistory = 'CurrentState'; Notes = 'Legacy para assignment requires DDL extension.' } }
        '^T_AU_OLD_PARAS_.*STATUS.*LOG$' { return @{ TargetTable = 'tbl_para_status_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Para status history requires DDL extension.' } }
        '^AIS_T_PARA_STATUS_CHANGE_LOG$' { return @{ TargetTable = 'tbl_para_status_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Para status history requires DDL extension.' } }
        '^T_AU_NEW_PARAS_STATUS_CHANGE_LOG$' { return @{ TargetTable = 'tbl_para_status_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Current para status history requires DDL extension.' } }
        '^T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY$' { return @{ TargetTable = 'tbl_para_settlement_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Legacy settlement history requires DDL extension.' } }
        '^T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY$' { return @{ TargetTable = 'tbl_para_settlement_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'Settlement review history requires DDL extension.' } }
        '^T_AU_OBSERVATION_OLD_CAD_PARAS$' { return @{ TargetTable = 'tbl_para_case'; Domain = 'Compliance'; DefaultHistory = 'CurrentState'; Notes = 'CAD old-para current-state case requires DDL extension.' } }
        '^T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT$' { return @{ TargetTable = 'tbl_para_case_text'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'CAD old-para text history requires DDL extension.' } }
        '^T_AU_OBSERVATION_OLD_CAD_PARAS_SETTLE_LOG$' { return @{ TargetTable = 'tbl_para_settlement_history'; Domain = 'Compliance'; DefaultHistory = 'History'; Notes = 'CAD old-para settlement log requires DDL extension.' } }

        '^T_AU_IID_COMPLAINT_HDR$' { return @{ TargetTable = 'tbl_iid_case'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Complaint header.' } }
        '^T_AU_IID_COMPLAINANT$' { return @{ TargetTable = 'tbl_iid_complainant'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Complainant detail.' } }
        '^T_AU_IID_INV_PLAN$' { return @{ TargetTable = 'tbl_iid_investigation_plan'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Investigation plan.' } }
        '^T_AU_IID_ANALYSIS$' { return @{ TargetTable = 'tbl_iid_analysis'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Analysis requires DDL extension.' } }
        '^T_AU_IID_ASSESSMENT$' { return @{ TargetTable = 'tbl_iid_assessment'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Assessment requires DDL extension.' } }
        '^T_AU_IID_CASE_STUDY$' { return @{ TargetTable = 'tbl_iid_case_study'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Case study requires DDL extension.' } }
        '^T_AU_IID_HEAD_REVIEW$' { return @{ TargetTable = 'tbl_iid_head_review'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Head review requires DDL extension.' } }
        '^T_AU_IID_HEAD_PLAN_APPROVAL$' { return @{ TargetTable = 'tbl_iid_plan_approval'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Plan approval requires DDL extension.' } }
        '^T_AU_IID_INQ_(ACCUSATIONS|ACCUSED_LIST)$' { return @{ TargetTable = 'tbl_iid_subject'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Accused/accusation subject rows.' } }
        '^T_AU_IID_INQ_EVIDENCE_FILES$' { return @{ TargetTable = 'tbl_iid_evidence'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Inquiry evidence.' } }
        '^T_AU_IID_INQ_EVIDENCE_STEP$' { return @{ TargetTable = 'tbl_iid_workflow_history'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Evidence step tracking requires DDL extension.' } }
        '^T_AU_IID_INQ_FIND_RECOMM$' { return @{ TargetTable = 'tbl_iid_finding'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Finding/recommendation split may require additional recommendation table.' } }
        '^T_AU_IID_INQ_PROCEEDINGS$' { return @{ TargetTable = 'tbl_iid_proceeding'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Proceedings require DDL extension.' } }
        '^T_AU_IID_INQ_RECORD_SCRUTINIZED$' { return @{ TargetTable = 'tbl_iid_record'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Record scrutiny requires DDL extension.' } }
        '^T_AU_IID_INQ_STATEMENTS$' { return @{ TargetTable = 'tbl_iid_statement'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Statements.' } }
        '^T_AU_IID_INQ_VIOLATIONS$' { return @{ TargetTable = 'tbl_iid_violation'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Violations require DDL extension.' } }
        '^T_AU_IID_REPORT$' { return @{ TargetTable = 'tbl_iid_report'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'IID report.' } }
        '^T_AU_IID_STATUS_MST$' { return @{ TargetTable = 'tbl_lookup_value'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'IID status master normalized as lookup values.' } }
        '^T_AU_EXCEPTION_REPORTS$' { return @{ TargetTable = 'tbl_iid_exception_report'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Exception report header.' } }
        '^T_AU_EXCEPTION_REPORTS_FORMAT$' { return @{ TargetTable = 'tbl_iid_exception_column'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Exception report column definitions.' } }
        '^T_AU_IID_EXC_(ACCOUNT|LOAN)$' { return @{ TargetTable = 'tbl_iid_exception_item'; Domain = 'IID'; DefaultHistory = 'CurrentState'; Notes = 'Exception item row.' } }
        '^T_AU_IID_EXC_.*(ACCOUNT_TXN|LOAN_TXN)$' { return @{ TargetTable = 'tbl_iid_exception_item_txn'; Domain = 'IID'; DefaultHistory = 'History'; Notes = 'Exception transaction detail.' } }
        '^T_AU_IID_EXC_.*(ACCOUNT_DOC|LOAN_DOC|LOAN_DOC_IMG)$' { return @{ TargetTable = 'tbl_attachment'; Domain = 'Document'; DefaultHistory = 'History'; Notes = 'Exception document attachment.' } }

        '^T_CAU_OM$' { return @{ TargetTable = 'tbl_commercial_om'; Domain = 'Commercial'; DefaultHistory = 'CurrentState'; Notes = 'Commercial OM.' } }
        '^T_CAU_PDP$' { return @{ TargetTable = 'tbl_commercial_pdp'; Domain = 'Commercial'; DefaultHistory = 'CurrentState'; Notes = 'Commercial PDP.' } }
        '^T_CAU_.*(DAC|PAC)' { return @{ TargetTable = 'tbl_commercial_arpse_resolution'; Domain = 'Commercial'; DefaultHistory = 'History'; Notes = 'Commercial DAC/PAC resolution.' } }
        '^T_CAU_ARPSE$' { return @{ TargetTable = 'tbl_commercial_arpse'; Domain = 'Commercial'; DefaultHistory = 'CurrentState'; Notes = 'Commercial ARPSE header.' } }

        '^T_FRPT_REPORT_META$' { return @{ TargetTable = 'tbl_report'; Domain = 'Reporting'; DefaultHistory = 'CurrentState'; Notes = 'Field-audit report header.' } }
        '^T_FRPT_SECTION_MASTER$' { return @{ TargetTable = 'tbl_lookup_value'; Domain = 'Reporting'; DefaultHistory = 'CurrentState'; Notes = 'Report section master normalized as lookup/reference values.' } }
        '^T_FRPT_(TEXT_BLOCKS|OVERALL_CONCLUSION|PARA_NARRATIVE|INCOME_LEAKAGE)$' { return @{ TargetTable = 'tbl_report_section'; Domain = 'Reporting'; DefaultHistory = 'History'; Notes = 'Report narrative section.' } }
        '^T_FRPT_(KPI_SNAPSHOT|NPL_SNAPSHOT|STAFF_SNAPSHOT|PDF_STATISTICS)$' { return @{ TargetTable = 'tbl_report_snapshot'; Domain = 'Reporting'; DefaultHistory = 'History'; Notes = 'Report snapshot payload.' } }

        '^T_AU_EMAIL_QUEUE$' { return @{ TargetTable = 'tbl_notification_queue'; Domain = 'Notify'; DefaultHistory = 'CurrentState'; Notes = 'Notification queue.' } }
        '^EMAILTEMPLATES$' { return @{ TargetTable = 'tbl_notification_template'; Domain = 'Notify'; DefaultHistory = 'CurrentState'; Notes = 'Notification template catalog.' } }
        '^NOTIFICATIONEVENTS$' { return @{ TargetTable = 'tbl_notification_event'; Domain = 'Notify'; DefaultHistory = 'CurrentState'; Notes = 'Notification event catalog.' } }
        '^NOTIFICATIONRULES$' { return @{ TargetTable = 'tbl_notification_rule'; Domain = 'Notify'; DefaultHistory = 'CurrentState'; Notes = 'Notification routing rules.' } }
        '^ATTACHMENTSOURCES$' { return @{ TargetTable = 'tbl_attachment_link'; Domain = 'Document'; DefaultHistory = 'CurrentState'; Notes = 'Attachment source/link definitions.' } }

        '(_LOG|_HISTORY)$' { return @{ TargetTable = 'tbl_workflow_event'; Domain = 'Workflow'; DefaultHistory = 'WorkflowEvent'; Notes = 'Operational history/log normalized into workflow events unless a dedicated history table exists.' } }
        'EVIDENCE|_DOC' { return @{ TargetTable = 'tbl_attachment'; Domain = 'Document'; DefaultHistory = 'History'; Notes = 'Binary/document content normalized to attachment storage.' } }
        default { return @{ TargetTable = 'REVIEW_REQUIRED'; Domain = 'Review'; DefaultHistory = 'Review'; Notes = 'No confident automatic target mapping.' } }
    }
}

function Get-DefaultDependency {
    param(
        [string]$TargetTable,
        [string]$TargetColumn
    )

    $table = ([string]$TargetTable).ToLower()
    $column = ([string]$TargetColumn).ToLower()

    if ($column -match 'user_id|created_by|modified_by|approved_by|uploaded_by|respondent_user_id|assignee_user_id|actor_user_id|submitted_by') {
        return 'Requires tbl_user migration and PPNO-to-user_id crosswalk.'
    }
    if ($column -match 'entity_id' -or $table -match 'entity|engagement|observation|compliance|commercial') {
        return 'Requires tbl_entity migration and entity-code crosswalk.'
    }
    if ($column -match 'audit_period_id') {
        return 'Requires audit-period normalization and period-code/period-name crosswalk.'
    }
    if ($column -match 'status_id|stage_id|type_id|role_id|permission_id|lookup') {
        return 'Requires status/lookup normalization register and lookup load order.'
    }
    if ($table -match 'iid') {
        return 'Requires parent IID case migration before child-row load.'
    }
    if ($table -match 'report') {
        return 'Requires engagement migration and report section/snapshot lookup seed.'
    }

    return 'Requires parent table load order and surrogate key crosswalk.'
}

function Resolve-ColumnMapping {
    param($LegacyRow)

    $tableMeta = Resolve-LegacyTableTarget $LegacyRow.TableName
    $legacyTable = $LegacyRow.TableName
    $legacyColumn = $LegacyRow.ColumnName
    $col = ([string]$legacyColumn).ToUpper()
    $targetTable = $tableMeta.TargetTable
    $targetColumn = ''
    $rule = 'Direct copy after datatype coercion and audit-column normalization.'
    $mandatory = 'Use IAS_ZTBL default if legacy value is null.'
    $history = $tableMeta.DefaultHistory
    $confidence = 'Medium'

    switch ($legacyTable.ToUpper()) {
        'T_USER' {
            switch ($col) {
                'USERID' { $targetColumn = 'user_id'; $rule = 'Preserve legacy user key through surrogate-key crosswalk.'; $confidence = 'High' }
                'LOGIN_NAME' { $targetColumn = 'login_name'; $confidence = 'High' }
                'PASSWORD' { $targetColumn = 'password_hash'; $rule = 'Do not copy raw legacy password text; re-hash or force reset according to security policy.'; $mandatory = 'Force password reset if legacy hash semantics are not compatible.'; $confidence = 'High' }
                'LASTLOGINDATETIME' { $targetColumn = 'last_login_on'; $confidence = 'High' }
                'USERFAILEDLOGINHITS' { $targetColumn = 'failed_login_count'; $confidence = 'High' }
                'PPNO' { $targetColumn = 'pp_no'; $confidence = 'High' }
                'ISACTIVE' { $targetColumn = 'user_status_id'; $rule = 'Map active flag into normalized USER_STATUS lookup; also derive is_active.'; $mandatory = 'Default to ACTIVE if null and validated against current security rules.'; $confidence = 'High' }
                'ENTITY_ID' { $targetColumn = 'home_entity_id'; $confidence = 'High' }
                'DESIGNATION' { $targetColumn = 'designation_title'; $rule = 'Resolve designation code/title through HR lookup during migration.'; $confidence = 'Medium' }
            }
        }
        'T_AU_PLAN_ENG' {
            switch ($col) {
                'ENG_ID' { $targetColumn = 'engagement_id'; $rule = 'Preserve engagement surrogate via crosswalk.'; $confidence = 'High' }
                'PERIOD_ID' { $targetColumn = 'audit_period_id'; $confidence = 'High' }
                'AUDIT_STARTDATE' { $targetColumn = 'audit_start_on'; $confidence = 'High' }
                'AUDIT_ENDDATE' { $targetColumn = 'audit_end_on'; $confidence = 'High' }
                'CREATEDBY' { $targetColumn = 'created_by'; $rule = 'Resolve legacy PP/user identifier to tbl_user.user_id.'; $confidence = 'High' }
                'CREATED_ON' { $targetColumn = 'created_on'; $confidence = 'High' }
                'LASTUPDATEDBY' { $targetColumn = 'modified_by'; $confidence = 'High' }
                'LASTUPDATEDDATE' { $targetColumn = 'modified_on'; $confidence = 'High' }
                'TEAM_NAME' { $targetColumn = 'team_name'; $confidence = 'High' }
                'STATUS' { $targetColumn = 'engagement_status_id'; $rule = 'Map legacy engagement status to normalized lookup.'; $confidence = 'High' }
                'ENTITY_ID' { $targetColumn = 'entity_id'; $confidence = 'High' }
                'PLAN_ID' { $targetColumn = 'plan_criteria_id'; $rule = 'Link to migrated plan header/criteria crosswalk.'; $confidence = 'Medium' }
                'OPERATION_STARTDATE' { $targetColumn = 'field_start_on'; $confidence = 'High' }
                'OPERATION_ENDDATE' { $targetColumn = 'field_end_on'; $confidence = 'High' }
                'TRAVEL_DAY' { $targetColumn = 'travel_days'; $confidence = 'High' }
                'DISCUSSION_DAY' { $targetColumn = 'discussion_days'; $confidence = 'High' }
            }
        }
        'T_AU_OBSERVATION' {
            switch ($col) {
                'ID' { $targetColumn = 'observation_id'; $rule = 'Preserve observation key via crosswalk.'; $confidence = 'High' }
                'ENGPLANID' { $targetColumn = 'engagement_id'; $confidence = 'High' }
                'ENTITY_ID' { $targetColumn = 'entity_id'; $confidence = 'High' }
                'STATUS' { $targetColumn = 'observation_status_id'; $rule = 'Map through observation status normalization register.'; $confidence = 'High' }
                'ENTEREDBY' { $targetColumn = 'created_by'; $rule = 'Resolve legacy PP/user identifier to tbl_user.user_id.'; $confidence = 'High' }
                'ENTEREDDATE' { $targetColumn = 'created_on'; $confidence = 'High' }
                'LASTUPDATEDBY' { $targetColumn = 'modified_by'; $confidence = 'High' }
                'LASTUPDATEDDATE' { $targetColumn = 'modified_on'; $confidence = 'High' }
                'AMOUNT_INVOLVED' { $targetColumn = 'amount_involved'; $rule = 'Numeric cast with invalid-value quarantine.'; $confidence = 'High' }
                'SEVERITY' { $targetColumn = 'severity_id'; $rule = 'Map through severity lookup.'; $confidence = 'High' }
                'MEMO_DATE' { $targetColumn = 'memo_date'; $confidence = 'High' }
                'MEMO_NUMBER' { $targetColumn = 'memo_no'; $confidence = 'High' }
                'RISKMODEL_ID' { $targetColumn = 'risk_rating_id'; $confidence = 'High' }
                'NO_OF_INSTANCES' { $targetColumn = 'instance_count'; $confidence = 'High' }
                'DRAFT_PARA_NO' { $targetColumn = 'draft_para_no'; $confidence = 'High' }
                'FINAL_PARA_NO' { $targetColumn = 'final_para_no'; $confidence = 'High' }
                'STELLED_ON' { $targetColumn = 'settled_on'; $confidence = 'High' }
                'SETTLED_BY' { $targetColumn = 'settled_by'; $confidence = 'High' }
                'REFERENCE_ID' { $targetTable = 'tbl_observation_reference'; $targetColumn = 'reference_document_version_id'; $rule = 'Resolve reference identifier to migrated reference-document version.'; $history = 'History'; $confidence = 'Medium' }
                'ANNEX_REF_ID' { $targetTable = 'tbl_observation_reference'; $targetColumn = 'reference_document_version_id'; $rule = 'Resolve annexure reference to normalized reference-document version.'; $history = 'History'; $confidence = 'Medium' }
                'RESPONSIBILITY_ASSIGNED' { $targetTable = 'tbl_observation_assignment'; $targetColumn = 'assignee_user_id'; $rule = 'Resolve responsible PP/user to tbl_user.user_id.'; $history = 'CurrentState'; $confidence = 'Medium' }
                'REPLYDATE' { $targetTable = 'tbl_observation_response'; $targetColumn = 'responded_on'; $history = 'History'; $confidence = 'Medium' }
                'LASTREPLYBY' { $targetTable = 'tbl_observation_response'; $targetColumn = 'respondent_user_id'; $rule = 'Resolve response actor to tbl_user.user_id.'; $history = 'History'; $confidence = 'Medium' }
                'LASTREPLYDATE' { $targetTable = 'tbl_observation_response'; $targetColumn = 'responded_on'; $history = 'History'; $confidence = 'Medium' }
                'CHECKLISTDETAIL_ID' { $targetColumn = 'checklist_item_id'; $rule = 'Add explicit checklist-item foreign key on tbl_observation and crosswalk legacy checklist detail.'; $confidence = 'Medium' }
                'SUBCHECKLIST_ID' { $targetColumn = 'checklist_sub_item_id'; $rule = 'Add explicit checklist-sub-item foreign key on tbl_observation.'; $confidence = 'Medium' }
            }
        }
        'AIS_T_AU_POST_COMPLIANCE' {
            switch ($col) {
                'COM_ID' { $targetColumn = 'compliance_case_id'; $rule = 'Preserve compliance-case surrogate via crosswalk.'; $confidence = 'High' }
                'NEW_PARA_ID' { $targetColumn = 'observation_id'; $rule = 'Link current para to migrated observation.'; $confidence = 'High' }
                'COM_CYCLE' { $targetColumn = 'compliance_cycle_no'; $confidence = 'High' }
                'COM_STATUS' { $targetColumn = 'compliance_status_id'; $rule = 'Map via compliance status normalization register.'; $confidence = 'High' }
                'COM_STAGE' { $targetColumn = 'compliance_stage_id'; $rule = 'Map via compliance stage normalization register.'; $confidence = 'High' }
                'ENTITY_ID' { $targetColumn = 'responsible_entity_id'; $confidence = 'High' }
                'SETTELED_ON' { $targetColumn = 'closed_on'; $rule = 'Treat settled-on as closure date when case is terminal.'; $confidence = 'Medium' }
                'SETTELED_BY' { $targetColumn = 'approved_by'; $rule = 'Resolve settlement approver to tbl_user.user_id.'; $confidence = 'Medium' }
                'GIST_OF_PARAS' { $targetColumn = 'gist_text'; $rule = 'Add gist_text column to tbl_compliance_case or stage into tbl_compliance_action with action_type=SUMMARY.'; $confidence = 'Medium' }
                'PARA_NO' { $targetColumn = 'legacy_para_no'; $rule = 'Add legacy_para_no on tbl_compliance_case for migration traceability.'; $confidence = 'Medium' }
                'AMOUNT' { $targetColumn = 'amount_involved'; $rule = 'Add numeric amount field to compliance case or action and cast legacy text safely.'; $confidence = 'Medium' }
                'NO_OF_INSTANCES' { $targetColumn = 'instance_count'; $rule = 'Add instance_count to compliance case.'; $confidence = 'Medium' }
            }
        }
        'T_AU_OLD_PARAS_FAD' {
            switch ($col) {
                'ID' { $targetColumn = 'para_case_id'; $rule = 'Preserve legacy para key in para-case crosswalk.'; $confidence = 'High' }
                'REF_P' { $targetColumn = 'legacy_reference_no'; $rule = 'Add legacy_reference_no to para case for traceability.'; $confidence = 'Medium' }
                'ENTITY_ID' { $targetColumn = 'entity_id'; $confidence = 'High' }
                'PARA_NO' { $targetColumn = 'para_no'; $confidence = 'High' }
                'GIST_OF_PARAS' { $targetColumn = 'gist_text'; $confidence = 'High' }
                'STATUS' { $targetColumn = 'case_status_id'; $rule = 'Map through para status normalization register.'; $confidence = 'High' }
                'PARA_STATUS' { $targetColumn = 'para_status_id'; $rule = 'Map through para status normalization register.'; $confidence = 'High' }
                'ENTERED_BY' { $targetColumn = 'created_by'; $confidence = 'High' }
                'ENTERED_ON' { $targetColumn = 'created_on'; $confidence = 'High' }
                'SETTLED_BY' { $targetColumn = 'settled_by'; $confidence = 'High' }
                'PARASETTELEDON' { $targetColumn = 'settled_on'; $confidence = 'High' }
                'NO_OF_INSTANCES' { $targetColumn = 'instance_count'; $confidence = 'High' }
            }
        }
        'T_AU_IID_COMPLAINT_HDR' {
            switch ($col) {
                'COMPLAINT_ID' { $targetColumn = 'iid_case_id'; $rule = 'Preserve complaint key via IID case crosswalk.'; $confidence = 'High' }
                'COMPLAINT_NO' { $targetColumn = 'case_no'; $confidence = 'High' }
                'INTAKE_CHANNEL' { $targetColumn = 'intake_channel_id'; $rule = 'Map text channel to normalized lookup value.'; $confidence = 'High' }
                'STATUS' { $targetColumn = 'case_status_id'; $rule = 'Map text status to normalized IID case status lookup.'; $confidence = 'High' }
                'SUBMITTED_ON' { $targetColumn = 'submitted_on'; $confidence = 'High' }
                'SUBMITTED_BY_PP_NO' { $targetColumn = 'submitted_by'; $rule = 'Resolve PPNO to tbl_user.user_id.'; $confidence = 'High' }
                'ASSIGNED_UNIT_ID' { $targetColumn = 'assigned_entity_id'; $confidence = 'High' }
                'ACTIVE_FLAG' { $targetColumn = 'is_active'; $rule = 'Normalize active flag to Y/N.'; $confidence = 'High' }
                'UPDATED_ON' { $targetColumn = 'modified_on'; $confidence = 'High' }
                'UPDATED_BY_PP_NO' { $targetColumn = 'modified_by'; $confidence = 'High' }
                'STATUS_ID' { $targetColumn = 'case_status_id'; $rule = 'Prefer STATUS_ID when present; STATUS text becomes validation source.'; $confidence = 'High' }
                'IS_FINALIZED' { $targetColumn = 'case_status_id'; $rule = 'Translate finalized flag into terminal IID case status; preserve finalized_on separately.'; $confidence = 'Medium' }
                'FINALIZED_ON' { $targetColumn = 'finalized_on'; $confidence = 'High' }
            }
        }
        'T_CAU_OM' {
            switch ($col) {
                'ID' { $targetColumn = 'commercial_om_id'; $rule = 'Preserve OM key via crosswalk.'; $confidence = 'High' }
                'OM_NO' { $targetColumn = 'om_no'; $confidence = 'High' }
                'CONTENTS_OF_OM' { $targetColumn = 'body_text'; $confidence = 'High' }
                'DIV_ID' { $targetColumn = 'entity_id'; $rule = 'Add explicit division/entity foreign key to tbl_commercial_om if retained separately.'; $confidence = 'Medium' }
                'STATUS' { $targetColumn = 'commercial_status_id'; $rule = 'Map through commercial-audit status lookup.'; $confidence = 'High' }
                'ENTERED_BY' { $targetColumn = 'created_by'; $confidence = 'High' }
                'ENTERED_ON' { $targetColumn = 'created_on'; $confidence = 'High' }
                'YEAR' { $targetColumn = 'audit_year'; $confidence = 'High' }
            }
        }
        'T_AU_API_MASTER' {
            switch ($col) {
                'API_ID' { $targetColumn = 'api_endpoint_id'; $confidence = 'High' }
                'API_PATH' { $targetColumn = 'route_path'; $rule = 'Add route_path to tbl_api_endpoint if not already present.'; $confidence = 'High' }
                'HTTP_METHOD' { $targetColumn = 'http_method'; $rule = 'Add http_method to tbl_api_endpoint if absent.'; $confidence = 'High' }
                'IS_ACTIVE' { $targetColumn = 'is_active'; $confidence = 'High' }
                'CREATED_ON' { $targetColumn = 'created_on'; $confidence = 'High' }
                'CREATED_BY' { $targetColumn = 'created_by'; $confidence = 'High' }
                'UPDATED_ON' { $targetColumn = 'modified_on'; $confidence = 'High' }
                'UPDATE_BY' { $targetColumn = 'modified_by'; $confidence = 'High' }
            }
        }
    }

    if (-not $targetColumn) {
        switch -Regex ($col) {
            '^(ID|.*_ID)$' {
                if ($col -eq 'ENTITY_ID') { $targetColumn = 'entity_id' }
                elseif ($col -eq 'PARENT_ID') { $targetColumn = 'parent_entity_id' }
                elseif ($col -eq 'CHILD_ID') { $targetColumn = 'child_entity_id' }
                elseif ($col -eq 'ROLE_ID') { $targetColumn = 'role_id' }
                elseif ($col -eq 'USERID') { $targetColumn = 'user_id' }
                elseif ($col -eq 'API_ID') { $targetColumn = 'api_endpoint_id' }
                elseif ($col -eq 'PG_ID') { $targetColumn = 'application_page_id' }
                elseif ($col -eq 'PERIOD_ID' -or $col -eq 'AUDITPERIODID') { $targetColumn = 'audit_period_id' }
                elseif ($col -eq 'RISK_ID' -or $col -eq 'RISKMODEL_ID') { $targetColumn = 'risk_rating_id' }
                elseif ($col -eq 'SIZE_ID') { $targetColumn = 'size_band_id' }
                elseif ($col -eq 'STATUS_ID' -or $col -eq 'STATUS') {
                    if ($targetTable -eq 'tbl_engagement') { $targetColumn = 'engagement_status_id' }
                    elseif ($targetTable -eq 'tbl_plan_criteria') { $targetColumn = 'criteria_status_id' }
                    elseif ($targetTable -eq 'tbl_iid_case') { $targetColumn = 'case_status_id' }
                    elseif ($targetTable -eq 'tbl_commercial_om') { $targetColumn = 'commercial_status_id' }
                    elseif ($targetTable -eq 'tbl_report') { $targetColumn = 'report_status_id' }
                    else { $targetColumn = 'status_id' }
                    $rule = 'Map legacy status identifier/text to normalized lookup.'
                }
                elseif ($col -eq 'STAGE' -or $col -eq 'COM_STAGE') {
                    $targetColumn = 'compliance_stage_id'
                    $rule = 'Map legacy stage to normalized compliance stage lookup.'
                }
                elseif ($col -eq 'COM_STATUS' -or $col -eq 'PARA_STATUS') {
                    $targetColumn = 'compliance_status_id'
                    $rule = 'Map legacy compliance/para status to normalized lookup.'
                }
                elseif ($col -eq 'FREQUENCY_ID') { $targetColumn = 'frequency_id' }
                elseif ($col -eq 'TYPE_ID' -or $col -eq 'ENTITY_TYPE' -or $col -eq 'ENTITY_TYPEID') { $targetColumn = 'entity_type_id' }
                elseif ($col -eq 'MAP_ID') { $targetColumn = Get-PkColumn $targetTable }
                else { $targetColumn = Get-PkColumn $targetTable }
            }
            'PPNO|USER_PP_NUMBER' {
                $targetColumn = if ($targetTable -eq 'tbl_user') { 'pp_no' } else { 'user_id' }
                $rule = 'Resolve legacy PPNO to tbl_user.user_id crosswalk.'
            }
            'CREATED_BY|ENTEREDBY|ENTERED_BY' { $targetColumn = 'created_by'; $rule = 'Resolve legacy PP/user identifier to tbl_user.user_id.' }
            'CREATED_ON|ENTEREDDATE|ENTERED_ON' { $targetColumn = 'created_on' }
            'UPDATED_ON|LASTUPDATEDDATE|MODIFIED_ON' { $targetColumn = 'modified_on' }
            'UPDATE_BY|UPDATED_BY|LASTUPDATEDBY' { $targetColumn = 'modified_by'; $rule = 'Resolve legacy PP/user identifier to tbl_user.user_id.' }
            'APPROVED_BY|REVIEWED_BY' { $targetColumn = 'approved_by'; $rule = 'Resolve reviewer/approver to tbl_user.user_id.' }
            'APPROVED_ON|REVIEWED_ON' { $targetColumn = 'approved_on' }
            'ISACTIVE|ACTIVE_FLAG|SESSION_ACTIVE' { $targetColumn = 'is_active'; $rule = 'Normalize active flag to Y/N.' }
            'NAME$' { $targetColumn = 'name'; $confidence = 'Low' }
            'DESCRIPTION$' { $targetColumn = 'description'; $confidence = 'Low' }
            'HEADING|TITLE' { $targetColumn = 'heading_text'; $rule = 'Load descriptive heading into normalized heading/title field.'; $confidence = 'Low' }
            'TEXT|DETAILS|CONTENTS' { $targetColumn = 'detail_text'; $rule = 'Load long text into normalized CLOB detail field.'; $confidence = 'Low' }
        }
    }

    if (-not $targetColumn) {
        $targetColumn = 'REVIEW_REQUIRED'
        $rule = 'Manual mapping required; no safe automatic target column was inferred.'
        $confidence = 'Low'
    }

    $availability = Get-TargetAvailability -TargetTable $targetTable -TargetColumn $targetColumn
    $dependency = Get-DefaultDependency -TargetTable $targetTable -TargetColumn $targetColumn

    [pscustomobject]@{
        OldTable = $legacyTable
        OldColumn = $legacyColumn
        LegacyDataType = $LegacyRow.DataType
        NewTable = $targetTable
        NewColumn = $targetColumn
        TargetAvailability = $availability
        TransformationRule = $rule
        MandatoryDefaultRule = $mandatory
        HistoryStrategy = $history
        MigrationDependency = $dependency
        MappingConfidence = $confidence
        Notes = $tableMeta.Notes
    }
}

$procedureMatrix = foreach ($row in $procs | Where-Object Status -eq 'Confirmed Active') {
    $decision = Get-ProcedureAction $row
    [pscustomobject]@{
        PackageName = $row.PackageName
        ProcedureCall = $row.ProcedureCall
        Modules = $row.Modules
        Methods = $row.Methods
        ReadObjects = $row.ReadObjects
        WriteObjects = $row.WriteObjects
        ReplacementAction = $decision.Action
        TargetPackage = $decision.TargetPackage
        ReplacementRationale = $decision.Rationale
        TransitionRule = switch ($decision.Action) {
            'keep as-is temporarily' { 'Retain behind compatibility views only until IAS_ZTBL reporting cutover is complete.' }
            'wrap' { 'Expose through an adapter/facade and block new direct controller dependencies.' }
            'split' { 'Decompose into module-bound procedures before IAS_ZTBL go-live.' }
            'redesign' { 'Rebuild directly on IAS_ZTBL normalized tables and retire legacy DML path at cutover.' }
            'retire' { 'Remove after equivalent IAS_ZTBL module is live and historical output is validated.' }
            default { '' }
        }
    }
}
$procedureMatrix | Export-Csv (Join-Path $analysisDir 'procedure_replacement_matrix.csv') -NoTypeInformation

$activeBaseObjects = $activeObjects | Where-Object { $_.Status -in @('Confirmed Active', 'Likely Active') -and $_.ObjectName -notlike 'V_*' } | Select-Object -ExpandProperty ObjectName -Unique
$migrationMatrix = foreach ($row in $legacyCols) {
    if ($activeBaseObjects -notcontains $row.TableName) { continue }
    Resolve-ColumnMapping $row
}
$migrationMatrix | Export-Csv (Join-Path $analysisDir 'old_to_new_table_column_migration_matrix.csv') -NoTypeInformation

$statusRows = @(
    [pscustomobject]@{ Domain='Observation'; LegacySource='T_AU_OBSERVATION.STATUS, T_AU_OBSERVATION_STATUS'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='DRAFT'; LookupName='Draft'; SortOrder=10; MigrationRule='Map draft/unsubmitted rows to DRAFT.'; Notes='Initial maker state.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='pkg_ar.P_SubmitAuditObservationToAuditee'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='SUBMITTED_TO_AUDITEE'; LookupName='Submitted to Auditee'; SortOrder=20; MigrationRule='Translate legacy submitted state and string outputs to normalized lookup.'; Notes='Confirmed by Notification.cs.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='T_AU_OBSERVATIONS_AUDITEE_RESPONSE'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='AUDITEE_RESPONDED'; LookupName='Auditee Responded'; SortOrder=30; MigrationRule='Set when auditee response exists and latest state is not closed.'; Notes='Response-driven state.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='pkg_hd.P_get_audit_pre_Concluding, pkg_hd.P_Audit_Concluding'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='PRE_CONCLUDING'; LookupName='Pre-Concluding'; SortOrder=40; MigrationRule='Map pre-concluding workflow states.'; Notes='Used in HD/Execution flows.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='pkg_hd.P_Audit_Concluding'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='CONCLUDED'; LookupName='Concluded'; SortOrder=50; MigrationRule='Map concluded/finalized observation states.'; Notes='Observation ready for compliance or final report.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='T_AU_OBSERVATION.STELLED_ON'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='SETTLED'; LookupName='Settled'; SortOrder=60; MigrationRule='Set where settled_on is present and status is terminal.'; Notes='Terminal observation state.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='pkg_ar.P_DropAuditObservation'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='DROPPED'; LookupName='Dropped'; SortOrder=70; MigrationRule='Map dropped/deleted-by-request cases into DROPPED.'; Notes='Retain workflow event history.' },
    [pscustomobject]@{ Domain='Observation'; LegacySource='pkg_ad.p_audit_observation_reversal'; LookupTypeCode='OBSERVATION_STATUS'; LookupCode='REVERSED'; LookupName='Reversed'; SortOrder=80; MigrationRule='Map reversal outcome into REVERSED plus workflow-event record.'; Notes='Do not overwrite current-state history.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='AIS_T_AU_POST_COMPLIANCE.COM_STAGE'; LookupTypeCode='COMPLIANCE_STAGE'; LookupCode='OPEN'; LookupName='Open'; SortOrder=10; MigrationRule='Initial compliance case state.'; Notes='Current case created.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='pkg_ae.P_SubmitPostAuditCompliance'; LookupTypeCode='COMPLIANCE_STAGE'; LookupCode='SUBMITTED'; LookupName='Submitted'; SortOrder=20; MigrationRule='Branch/auditee submission states normalize to SUBMITTED.'; Notes='Submission timestamp retained.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='pkg_ae.P_SubmitPostAuditCompliance_review'; LookupTypeCode='COMPLIANCE_STAGE'; LookupCode='UNDER_REVIEW'; LookupName='Under Review'; SortOrder=30; MigrationRule='Reviewer workflow states normalize to UNDER_REVIEW.'; Notes='Reviewer actor tracked separately.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='pkg_fad.P_authorizechangestatusrequestforsettledpara'; LookupTypeCode='COMPLIANCE_STAGE'; LookupCode='APPROVED'; LookupName='Approved'; SortOrder=40; MigrationRule='Authorized/approved compliance actions normalize to APPROVED.'; Notes='May coexist with compliance status.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='AIS_T_AU_POST_COMPLIANCE.COM_STATUS'; LookupTypeCode='COMPLIANCE_STATUS'; LookupCode='PENDING'; LookupName='Pending'; SortOrder=10; MigrationRule='Default unresolved status.'; Notes='Live case not yet completed.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='AIS_T_AU_POST_COMPLIANCE.COM_STATUS'; LookupTypeCode='COMPLIANCE_STATUS'; LookupCode='IN_PROGRESS'; LookupName='In Progress'; SortOrder=20; MigrationRule='Map active remediation in progress.'; Notes='Non-terminal.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='AIS_T_AU_POST_COMPLIANCE.COM_STATUS'; LookupTypeCode='COMPLIANCE_STATUS'; LookupCode='PARTIALLY_COMPLIED'; LookupName='Partially Complied'; SortOrder=30; MigrationRule='Map partial compliance outcomes.'; Notes='Needs cycle continuity.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='AIS_T_AU_POST_COMPLIANCE.COM_STATUS'; LookupTypeCode='COMPLIANCE_STATUS'; LookupCode='FULLY_COMPLIED'; LookupName='Fully Complied'; SortOrder=40; MigrationRule='Map full compliance outcomes.'; Notes='May still require approval.' },
    [pscustomobject]@{ Domain='Compliance'; LegacySource='T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY'; LookupTypeCode='COMPLIANCE_STATUS'; LookupCode='SETTLED'; LookupName='Settled'; SortOrder=50; MigrationRule='Map terminal settlement states.'; Notes='Terminal case status.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_COMPLAINT_HDR.STATUS / STATUS_ID'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='RECEIVED'; LookupName='Received'; SortOrder=10; MigrationRule='Initial complaint intake state.'; Notes='Submitted complaint.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_ANALYSIS'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='UNDER_ANALYSIS'; LookupName='Under Analysis'; SortOrder=20; MigrationRule='Set when analysis record exists and case not advanced.'; Notes='Active analysis stage.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_ASSESSMENT'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='UNDER_ASSESSMENT'; LookupName='Under Assessment'; SortOrder=30; MigrationRule='Set when assessment record exists.'; Notes='Assessment stage.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_INV_PLAN'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='INVESTIGATION_PLANNED'; LookupName='Investigation Planned'; SortOrder=40; MigrationRule='Set when investigation plan is saved/approved.'; Notes='Plan approval event may exist separately.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_HEAD_REVIEW'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='UNDER_HEAD_REVIEW'; LookupName='Under Head Review'; SortOrder=50; MigrationRule='Normalize head-review state.'; Notes='Retain review actor/history.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_REPORT'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='REPORT_DRAFTED'; LookupName='Report Drafted'; SortOrder=60; MigrationRule='Set when inquiry report exists but finalization flag is off.'; Notes='Draft report stage.' },
    [pscustomobject]@{ Domain='IID'; LegacySource='T_AU_IID_COMPLAINT_HDR.IS_FINALIZED'; LookupTypeCode='IID_CASE_STATUS'; LookupCode='FINALIZED'; LookupName='Finalized'; SortOrder=70; MigrationRule='Terminal finalized complaint/report state.'; Notes='Confirmed by direct SQL in DBConnection.IID.cs.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='T_CAU_OM.STATUS'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='OM_DRAFT'; LookupName='OM Draft'; SortOrder=10; MigrationRule='Initial OM creation state.'; Notes='Legacy OM record created.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='PKG_CM.P_CAUGetAssignedOMs'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='OM_ASSIGNED'; LookupName='OM Assigned'; SortOrder=20; MigrationRule='Map assigned OM states into normalized status.'; Notes='Assignment visible in live code.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='PKG_COMMERCIAL_AUDIT.P_SAVE_PDP'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='PDP_DRAFTED'; LookupName='PDP Drafted'; SortOrder=30; MigrationRule='Set when PDP exists and is not closed.'; Notes='Commercial remediation stage.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='ARPSE_DRAFTED'; LookupName='ARPSE Drafted'; SortOrder=40; MigrationRule='Set when ARPSE header exists.'; Notes='Commercial ARPSE stage.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE_DAC'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='DAC_REVIEW'; LookupName='DAC Review'; SortOrder=50; MigrationRule='Set when DAC rows are in review cycle.'; Notes='Commercial review stage.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='PKG_COMMERCIAL_AUDIT.P_SAVE_ARPSE_PAC'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='PAC_REVIEW'; LookupName='PAC Review'; SortOrder=60; MigrationRule='Set when PAC rows are in review cycle.'; Notes='Commercial review stage.' },
    [pscustomobject]@{ Domain='Commercial'; LegacySource='Commercial audit closure'; LookupTypeCode='COMMERCIAL_AUDIT_STATUS'; LookupCode='CLOSED'; LookupName='Closed'; SortOrder=70; MigrationRule='Terminal status after final action closure.'; Notes='Terminal commercial case state.' },
    [pscustomobject]@{ Domain='Report'; LegacySource='T_FRPT_REPORT_META.STATUS'; LookupTypeCode='REPORT_STATE'; LookupCode='DRAFT'; LookupName='Draft'; SortOrder=10; MigrationRule='Default report state when report exists but is incomplete.'; Notes='Initial authoring stage.' },
    [pscustomobject]@{ Domain='Report'; LegacySource='PKG_FRPT.P_SAVE_*'; LookupTypeCode='REPORT_STATE'; LookupCode='IN_PREPARATION'; LookupName='In Preparation'; SortOrder=20; MigrationRule='Set while narrative/snapshot sections are being edited.'; Notes='Intermediate state.' },
    [pscustomobject]@{ Domain='Report'; LegacySource='PKG_FRPT.P_FINALIZE_REPORT'; LookupTypeCode='REPORT_STATE'; LookupCode='FINALIZED'; LookupName='Finalized'; SortOrder=30; MigrationRule='Terminal author-side report finalization.'; Notes='Live FRPT transition.' },
    [pscustomobject]@{ Domain='Report'; LegacySource='Approval workflow'; LookupTypeCode='REPORT_STATE'; LookupCode='APPROVED'; LookupName='Approved'; SortOrder=40; MigrationRule='Optional approval state when governance signoff is required.'; Notes='Can be enabled by policy.' },
    [pscustomobject]@{ Domain='Report'; LegacySource='Issued report workflow'; LookupTypeCode='REPORT_STATE'; LookupCode='ISSUED'; LookupName='Issued'; SortOrder=50; MigrationRule='Set when final report is released externally.'; Notes='Notification trigger state.' },
    [pscustomobject]@{ Domain='Security'; LegacySource='T_GROUPS.STATUS, T_AU_ROLE_API_PERMISSION.IS_ACTIVE, T_USER.ISACTIVE'; LookupTypeCode='SECURITY_STATUS'; LookupCode='ACTIVE'; LookupName='Active'; SortOrder=10; MigrationRule='Map enabled/active flags to ACTIVE.'; Notes='Default live state.' },
    [pscustomobject]@{ Domain='Security'; LegacySource='T_GROUPS.STATUS, T_AU_ROLE_API_PERMISSION.IS_ACTIVE, T_USER.ISACTIVE'; LookupTypeCode='SECURITY_STATUS'; LookupCode='INACTIVE'; LookupName='Inactive'; SortOrder=20; MigrationRule='Map disabled/inactive flags to INACTIVE.'; Notes='Soft-disabled state.' },
    [pscustomobject]@{ Domain='Security'; LegacySource='Account lock / failed logins'; LookupTypeCode='SECURITY_STATUS'; LookupCode='LOCKED'; LookupName='Locked'; SortOrder=30; MigrationRule='Derive LOCKED when security policy has locked the account.'; Notes='User-only status.' }
)
$statusRows | Export-Csv (Join-Path $analysisDir 'status_lookup_normalization_register.csv') -NoTypeInformation

$historyRows = @(
    [pscustomobject]@{ Domain='Security'; CurrentStateTables='tbl_user, tbl_role, tbl_permission, tbl_user_session'; HistoryTables='tbl_release_version_history'; WorkflowEventLogging='tbl_activity_log, tbl_workflow_event'; ArchiveOnlyData='Legacy *_BACKUP security tables'; Notes='Keep current security state separate from release/version history and operational activity log.' },
    [pscustomobject]@{ Domain='Entity'; CurrentStateTables='tbl_entity_type, tbl_entity, tbl_entity_relation'; HistoryTables='tbl_entity_size_profile, tbl_entity_risk_profile'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='Entity update staging/backups'; Notes='Entity relationships are current-state; changes in profile attributes should be historized.' },
    [pscustomobject]@{ Domain='Planning'; CurrentStateTables='tbl_audit_period, tbl_audit_plan, tbl_engagement, tbl_engagement_member, tbl_engagement_task, tbl_plan_criteria'; HistoryTables='tbl_engagement_criteria_history'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='T_AU_PLAN_ENG_BACKUP and similar copies'; Notes='Engagement reversals and approvals should become workflow events, not overwrite rows.' },
    [pscustomobject]@{ Domain='Observation'; CurrentStateTables='tbl_observation, tbl_observation_assignment'; HistoryTables='tbl_observation_detail, tbl_observation_response, tbl_observation_recommendation, tbl_observation_reference, tbl_observation_evidence'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='Dropped/duplicate legacy observation helper tables'; Notes='Current observation state stays lean; text/response/evidence changes are historized.' },
    [pscustomobject]@{ Domain='Compliance'; CurrentStateTables='tbl_compliance_case, tbl_compliance_action, tbl_compliance_review, tbl_para_case, tbl_para_assignment'; HistoryTables='tbl_compliance_case_history, tbl_compliance_evidence, tbl_para_case_text, tbl_para_status_history, tbl_para_settlement_history'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='Legacy backup/temp compliance tables only'; Notes='Separate current case state from audit-trace and settlement history.' },
    [pscustomobject]@{ Domain='IID'; CurrentStateTables='tbl_iid_case, tbl_iid_complainant, tbl_iid_investigation_plan, tbl_iid_subject, tbl_iid_statement, tbl_iid_evidence, tbl_iid_report'; HistoryTables='tbl_iid_analysis, tbl_iid_assessment, tbl_iid_head_review, tbl_iid_proceeding, tbl_iid_record, tbl_iid_violation, tbl_iid_finding, tbl_iid_workflow_history'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='None beyond backup/temp exports'; Notes='IID needs explicit workflow history instead of only current header flags.' },
    [pscustomobject]@{ Domain='Commercial'; CurrentStateTables='tbl_commercial_om, tbl_commercial_pdp, tbl_commercial_arpse'; HistoryTables='tbl_commercial_pdp_observation, tbl_commercial_arpse_resolution, tbl_commercial_audit_workflow_history'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='Legacy PKG_CM-only helper structures'; Notes='Commercial review cycles should be modeled as workflow history plus DAC/PAC detail.' },
    [pscustomobject]@{ Domain='Reporting'; CurrentStateTables='tbl_report'; HistoryTables='tbl_report_section, tbl_report_snapshot'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='T_FRPT_STAFF_SNAPSHOT_OLD'; Notes='Report header is current-state; narrative and snapshots are historized/versioned.' },
    [pscustomobject]@{ Domain='Notification'; CurrentStateTables='tbl_notification_event, tbl_notification_template, tbl_notification_rule, tbl_notification_queue'; HistoryTables='Notification delivery audit in tbl_workflow_event'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='Legacy email backup/temp objects only'; Notes='Queue state is current; delivery attempts should be logged separately.' },
    [pscustomobject]@{ Domain='Document'; CurrentStateTables='tbl_attachment, tbl_attachment_link'; HistoryTables='Versioned attachments through record_version and workflow events'; WorkflowEventLogging='tbl_workflow_event'; ArchiveOnlyData='Legacy temp file folders only'; Notes='Do not store repeated file-path columns across business tables.' }
)
$historyRows | Export-Csv (Join-Path $analysisDir 'history_strategy_register.csv') -NoTypeInformation

$triggerSummary = $legacyCols | Where-Object { $_.TriggerName } |
    Group-Object TriggerName,TriggeringEvent,TriggerStatus |
    ForEach-Object {
        [pscustomobject]@{
            DependencyType = 'Trigger'
            DependencyName = $_.Group[0].TriggerName
            SourceObjects = (($_.Group | Select-Object -ExpandProperty TableName -Unique) -join '; ')
            Status = $_.Group[0].TriggerStatus
            Evidence = "Legacy workbook metadata shows $($_.Count) column-level references for trigger $($_.Group[0].TriggerName)."
            Recommendation = 'Replicate only when the trigger enforces a business key or sequencing rule not replaced by standardized IAS_ZTBL sequence/trigger patterns.'
        }
    }

$viewRefs = foreach ($row in $procs) {
    (($row.ReadObjects + '; ' + $row.WriteObjects) -split '; ') | Where-Object { $_ -match '^V_' }
}
$viewSummary = $viewRefs | Group-Object | ForEach-Object {
    [pscustomobject]@{
        DependencyType = 'View'
        DependencyName = $_.Name
        SourceObjects = ''
        Status = 'ReferencedByActiveProcedure'
        Evidence = "Referenced by $($_.Count) confirmed-active package procedures."
        Recommendation = 'Validate the underlying SQL and either recreate as compatibility view or fold logic into IAS_ZTBL reporting/reference tables.'
    }
}

$packageSummary = $procs | Group-Object PackageName | ForEach-Object {
    [pscustomobject]@{
        DependencyType = 'PackageInternal'
        DependencyName = $_.Name
        SourceObjects = ''
        Status = 'ConfirmedActive'
        Evidence = "$($_.Count) confirmed-active procedure calls remain in use."
        Recommendation = 'Use the procedure replacement matrix to split, wrap, redesign, or retire package responsibilities module by module.'
    }
}

$scheduledJobs = @(
    [pscustomobject]@{
        DependencyType = 'ScheduledJob'
        DependencyName = 'DBMS_SCHEDULER / DBMS_JOB search'
        SourceObjects = 'Packages.xlsx shared strings'
        Status = 'NoMatchFound'
        Evidence = 'No DBMS_SCHEDULER, DBMS_JOB, CREATE_JOB, USER_SCHEDULER_JOBS, ALL_SCHEDULER_JOBS, UTL_FILE, or BFILENAME references were found in the available package workbook source.'
        Recommendation = 'Treat scheduler dependency as not proven from available artifacts; confirm against live DBA views before final exclusion of notification/support tables.'
    }
)

$directSql = @(
    [pscustomobject]@{
        DependencyType = 'DirectSQL'
        DependencyName = 'DBConnection.FAD manual/reference queries'
        SourceObjects = 'T_MANUAL_MASTER; T_MANUAL_INDEX'
        Status = 'ConfirmedActive'
        Evidence = 'GetManualMaster, GetManualSections, GetManualChapters, and GetManualIndexByChapter use CommandType.Text against manual tables.'
        Recommendation = 'Migrate manual/reference content to IAS_ZTBL reference tables and keep compatibility views during cutover.'
    },
    [pscustomobject]@{
        DependencyType = 'DirectSQL'
        DependencyName = 'DBConnection.FRPT report row existence check'
        SourceObjects = 'T_FRPT_KPI_SNAPSHOT; T_FRPT_NPL_SNAPSHOT; T_FRPT_STAFF_SNAPSHOT'
        Status = 'ConfirmedActive'
        Evidence = 'HasFieldAuditRows issues SELECT COUNT(1) text queries against FRPT snapshot tables.'
        Recommendation = 'Replace with IAS_ZTBL report-snapshot existence checks after FRPT migration.'
    },
    [pscustomobject]@{
        DependencyType = 'DirectSQL'
        DependencyName = 'DBConnection.IID complaint finalization check'
        SourceObjects = 'T_AU_IID_COMPLAINT_HDR'
        Status = 'ConfirmedActive'
        Evidence = 'IsIidComplaintFinalized directly selects is_finalized from t_au_iid_complaint_hdr.'
        Recommendation = 'Replace with normalized IID case status lookup and helper wrapper.'
    }
)

$filePathRows = @(
    [pscustomobject]@{
        DependencyType = 'FilePath'
        DependencyName = 'Audit report upload folder'
        SourceObjects = 'wwwroot\\Audit_Report\\<subfolder>'
        Status = 'ConfirmedActive'
        Evidence = 'DBConnection.cs GetUploadedAuditReportsFromDirectory reads files directly from Audit_Report subfolders.'
        Recommendation = 'Move file metadata to tbl_attachment/tbl_attachment_link and preserve storage key instead of folder-only references.'
    },
    [pscustomobject]@{
        DependencyType = 'FilePath'
        DependencyName = 'Post-compliance evidence folder'
        SourceObjects = 'wwwroot\\PostCompliance_Evidences\\<subfolder>'
        Status = 'ConfirmedActive'
        Evidence = 'DBConnection.cs GetAttachedFilesFromDirectory and AE compliance submission flows read/delete evidence folders directly.'
        Recommendation = 'Normalize compliance evidence into attachment tables and migrate physical files by storage key.'
    },
    [pscustomobject]@{
        DependencyType = 'FilePath'
        DependencyName = 'Auditee evidence folder'
        SourceObjects = 'wwwroot\\Auditee_Evidences\\<subfolder>'
        Status = 'ConfirmedActive'
        Evidence = 'AE/AR workflows read auditee evidence from filesystem before storing metadata in DB.'
        Recommendation = 'Replace folder-based coupling with attachment rows plus background file ingestion.'
    },
    [pscustomobject]@{
        DependencyType = 'FilePath'
        DependencyName = 'CAU evidence folder'
        SourceObjects = 'wwwroot\\CAU_Evidences\\<subfolder>'
        Status = 'ConfirmedActive'
        Evidence = 'AE branch/CAU compliance flows read and purge CAU evidence folders.'
        Recommendation = 'Unify CAU evidence under attachment model with explicit source-entity linkage.'
    }
)

$dependencyRows = @()
$dependencyRows += $triggerSummary
$dependencyRows += $viewSummary
$dependencyRows += $packageSummary
$dependencyRows += $scheduledJobs
$dependencyRows += $directSql
$dependencyRows += $filePathRows
$dependencyRows | Export-Csv (Join-Path $analysisDir 'db_level_dependency_review_register.csv') -NoTypeInformation

$baselineText = @"
IAS_ZTBL Phase 2 Official Baseline

Generated files:
- procedure_replacement_matrix.csv
- old_to_new_table_column_migration_matrix.csv
- status_lookup_normalization_register.csv
- history_strategy_register.csv
- db_level_dependency_review_register.csv

Source inventories used:
- legacy_table_columns.csv
- target_table_columns.csv
- active_procedure_package_register.csv
- active_object_register.csv
- dbconnection_method_register.csv
"@
$baselineText | Set-Content (Join-Path $baseDir 'Phase2_Official_Baseline.txt')

[pscustomobject]@{
    ProcedureRows = $procedureMatrix.Count
    MigrationRows = $migrationMatrix.Count
    StatusRows = $statusRows.Count
    HistoryRows = $historyRows.Count
    DependencyRows = $dependencyRows.Count
} | Format-Table -AutoSize
