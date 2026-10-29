# History Strategy Register

Machine-readable register:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\history_strategy_register.csv`

## Main Separation Rules

1. Current-state tables hold only the latest operational row.
2. History tables preserve text versions, approvals, settlements, evidence changes, and long-running workflow transitions.
3. `tbl_workflow_event` should become the shared event log for approvals, reversals, state changes, and cross-module actions.
4. Backup/temp tables stay outside IAS_ZTBL operational design.

## High-Risk Domains

- engagement reversals and criteria approvals
- observation text/response/recommendation lifecycle
- para/compliance settlement history
- IID stage progression
- commercial-audit review cycles
- FRPT report sections and snapshots

## Main Finding

The current legacy system mixes current-state rows, logs, and historical copies inconsistently. IAS_ZTBL needs explicit current-state tables, explicit history tables, and a separate workflow-event stream.
