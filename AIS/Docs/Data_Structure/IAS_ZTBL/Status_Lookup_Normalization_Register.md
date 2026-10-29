# Status and Lookup Normalization Register

Machine-readable register:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\status_lookup_normalization_register.csv`

## Domains Covered

- observation statuses
- compliance stages
- compliance statuses
- IID case statuses
- commercial-audit statuses
- report states
- security/role/user status flags

## Main Findings

1. The current system mixes numeric status IDs, text statuses, stage IDs, and plain active flags across modules.
2. Observation and compliance workflows need separate lookup families. They should not share one overloaded `STATUS` meaning.
3. IID requires a real case-status model rather than only `STATUS`, `STATUS_ID`, and `IS_FINALIZED`.
4. Commercial audit currently uses status-driven progression without a clean normalized status family.
5. Report composition and report issuance should be separate report-state values in IAS_ZTBL.
