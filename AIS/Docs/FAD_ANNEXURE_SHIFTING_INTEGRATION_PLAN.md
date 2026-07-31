# Field Audit Annexure shifting integration plan

The Annexure master is `T_AUDIT_CHECKLIST_ANNEXURE`: `ID` is the value stored in
the observation tables, `CODE` is the display code, `HEADING` is the description,
and `STATUS` is the master status. The configuration therefore stores no duplicate
code or description.

`P_Add_Entity_Shifting` was not modified. A later reviewed change must address each
condition independently in `PKG_AD.sql`:

1. Line near 6503, `T_AU_OLD_PARAS_FAD F`: replace the applicable-ID part of the
   compound `OR` only after preserving its associated `ENGPLANID` exception.
2. Line near 6564, `T_AU_OBSERVATION O`: replace the applicable-ID part of the
   compound `OR` only after preserving its associated `ENGPLANID` exception.
3. Line near 6576, `T_AU_OBSERVATION O`: replace `NVL(O.ANNEX,-1) NOT IN (...)`
   with `NOT EXISTS`; this preserves null Annexures as non-applicable.
4. Line near 6588, `T_AU_OBSERVATION_FAD F`: replace the applicable condition
   with `EXISTS`.
5. Line near 6597, `T_AU_OBSERVATION_FAD F`: replace the non-applicable condition
   with `NOT EXISTS`; this preserves null Annexures as non-applicable.

Applicable form:

```sql
EXISTS (
  SELECT 1
    FROM T_AU_FAD_ANNEXURE_CONFIG C
   WHERE C.ANNEXURE_ID = O.ANNEX
     AND C.SHIFT_APPLICABLE = 'Y'
     AND C.ACTIVE = 'Y'
)
```

For aliases named `F`, use `C.ANNEXURE_ID = F.ANNEX`. Non-applicable branches use
the identical correlated query prefixed with `NOT EXISTS`. Regression testing must
cover null Annexures and every distinct `ENGPLANID` branch before deployment.
