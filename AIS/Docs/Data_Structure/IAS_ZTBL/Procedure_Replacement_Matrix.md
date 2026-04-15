# Procedure Replacement Matrix

Machine-readable register:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\procedure_replacement_matrix.csv`

## Coverage

- confirmed-active legacy procedure/package calls reviewed: `748`

## Replacement Actions

| Action | Count | Meaning |
| --- | ---: | --- |
| `keep as-is temporarily` | 106 | Keep the legacy procedure behind compatibility views only for a limited transition period. |
| `wrap` | 389 | Preserve temporarily behind a new IAS_ZTBL-facing adapter/facade. |
| `split` | 110 | Break the legacy responsibility into more than one future-state package/module. |
| `redesign` | 132 | Rebuild directly on IAS_ZTBL tables; do not carry the legacy implementation. |
| `retire` | 11 | Remove after the IAS_ZTBL replacement is live and validated. |

## Main Findings

1. `PKG_AD`, `PKG_AR`, and `PKG_HD` are too broad and should be treated as split candidates rather than migrated package-for-package.
2. `PKG_RPT`, `PKG_DB`, and `PKG_BAC` are the safest short-term keep/wrap candidates because they are read-heavy.
3. `PKG_INQ`, `PKG_AE`, `PKG_FAD`, `PKG_PG`, and write-side `PKG_FRPT` logic require redesign on normalized IAS_ZTBL structures.
4. `PKG_CM` and `PKG_AIS_EMAIL.P_ADDAUDITCRITERIA` are explicit retire candidates after cutover.
