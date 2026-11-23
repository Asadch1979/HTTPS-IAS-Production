# Pending operational Annexure reconciliation

The user-supplied baseline is 121 staged Annexures, 114 unique matched IAS IDs and seven unmatched codes. The approved source CSV is in source/FAD_Risk_True_Source_Mapping.csv. Live operational match results are not available in this checkout, so this correction does not invent the seven codes or their target IDs.

V_FAD_RISK_REGISTER preserves all staged rows. Normalized matching means UPPER(TRIM(code)) on both sides; case and surrounding whitespace are already covered. Punctuation removal and letter O/digit zero substitutions are diagnostic proposals only and never resolve a production mapping.

02_unmatched_diagnostic.sql lists actual pending rows with full violation/procedure text and reports possible formatting candidates separately. Missing candidates remain listed. The application shows every pending code across the catalog, as well as the status of each row in the selected process. Missing or ambiguous operational IDs are NULL in the read result and displayed as unresolved; no NULL is inserted into a scoring column or master.

| Display status | Meaning |
| --- | --- |
| MATCHED | One normalized operational code row with a globally unique non-null IAS ID |
| PENDING_RECONCILIATION | No normalized operational code match |
| CONFLICTING_IAS_CODE | Multiple operational rows match the normalized code |
| INVALID_IAS_ID | Matching operational row has no ID |
| CONFLICTING_IAS_ID | Candidate ID occurs on multiple operational rows |

Pending mappings remain blocked from operational use. They do not prevent browsing the hierarchy and are not silently discarded. The view deliberately does not consume the previous alias table, which may depend on the absent physical ID key. Any future alias support requires a separately approved mapping design with approval evidence and ambiguity controls. Record staged code, proposed existing IAS code/ID, rationale, approver, date and approval reference before such work. Do not edit the staging codes to evade source fingerprints or modify the operational Annexure table through this correction.
