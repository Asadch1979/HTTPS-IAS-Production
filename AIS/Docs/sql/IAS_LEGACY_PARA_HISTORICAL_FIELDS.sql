-- SOURCE ONLY. Alternative upgrade for an existing request table; do not run
-- after the revised fresh-install DDL. No data backfill or feature activation.
ALTER TABLE T_IAS_LEGACY_PARA_REQUEST ADD (GIST CLOB, ANNEXURE_DETAILS CLOB);
-- Existing requests retain their original payload/history. Missing gist requires
-- return to maker and explicit correction; never infer gist from the heading.
