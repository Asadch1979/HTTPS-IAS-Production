-- Read only. Export the returned CLOB to a text file using the result-grid
-- CLOB editor/export facility. Verify the complete CLOB, not a truncated preview.
SELECT DBMS_METADATA.GET_DDL('PACKAGE','PKG_RPT',USER) AS package_spec FROM dual;
