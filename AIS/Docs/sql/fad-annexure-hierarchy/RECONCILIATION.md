# Annexure reconciliation — pending database evidence

The supplied baseline is 121 staged codes, 114 matched IDs, 114 distinct matched IDs and seven unmatched codes. These are user-provided counts, not locally observed query results. Neither the staging rows nor `FAD_Risk_True_Source_Mapping.csv` is present in this checkout. The seven actual codes and their proposed IAS IDs therefore remain unidentified; no aliases have been invented or approved.

`02_unmatched_diagnostic.sql` produces the reconciliation report with staged code, full violation, procedure code and description, possible IAS code, existing IAS ID and proposal reason. Each unmatched code remains in the report even when no candidate exists. Multiple candidates produce multiple rows, so the report can exceed seven rows. The second query also reports ambiguous exact matches.

Candidate proposals are separate from resolution:

| Difference | Proposed review | Approval state |
| --- | --- | --- |
| Case / surrounding whitespace | Compare exact meaning and operational heading | Unapproved |
| Hyphens / punctuation / spacing | Compare source and operational Annexure | Unapproved |
| Letter O / digit zero | Inspect original source; especially codes shaped like BO7 | Unapproved; BO7 is not asserted to be one of the seven |
| No formatting candidate | Review IAS `CODE` and `HEADING` manually for naming differences | Unresolved |

Record each decision with staged code, existing IAS code and ID, reason, approving person, date and approval reference. Only separately approved decisions may be inserted into `T_RISK_FAD_CODE_ALIAS`; this delivery contains no alias inserts. Aliases must refer to existing operational IDs. An alias on an already exact-matched code is rejected as conflicting, even if it names the same ID. Two source codes cannot silently collapse to one IAS ID.

If no existing IAS Annexure matches, retain the unresolved row and stop the entire import. Do not edit the operational Annexure table or allocate an ID. Run the diagnostic before migration; migration also prints all unresolved/ambiguous codes before rejecting the transaction.
