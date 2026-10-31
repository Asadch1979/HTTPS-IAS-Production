-- Read only. Export the full returned CLOB as PKG_RPT.production.body.sql.
-- This is extraction only; do not execute the extracted text in production.
SELECT DBMS_METADATA.GET_DDL('PACKAGE_BODY','PKG_RPT',USER) AS package_body FROM dual;
