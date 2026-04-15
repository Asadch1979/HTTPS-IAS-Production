/*
  IAS_ZTBL Phase 3 sequence and trigger script
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Automated sequence and trigger creation for tbl_% tables
-------------------------------------------------------------------------------

DECLARE
    l_seq_name VARCHAR2(128);
    l_trg_name VARCHAR2(128);
    l_sql      CLOB;
BEGIN
    FOR r IN (
        SELECT uc.table_name, ucc.column_name
        FROM user_constraints uc
        JOIN user_cons_columns ucc
          ON ucc.constraint_name = uc.constraint_name
         AND ucc.table_name = uc.table_name
        WHERE uc.constraint_type = 'P'
          AND uc.table_name LIKE 'TBL\_%' ESCAPE '\'
    ) LOOP
        l_seq_name := 'SEQ_' || SUBSTR(r.table_name, 5);
        l_trg_name := 'TRG_' || SUBSTR(r.table_name, 5) || '_BIR';

        BEGIN
            EXECUTE IMMEDIATE 'CREATE SEQUENCE ' || l_seq_name || ' START WITH 1 INCREMENT BY 1 NOCACHE';
        EXCEPTION
            WHEN OTHERS THEN
                IF SQLCODE != -955 THEN
                    RAISE;
                END IF;
        END;

        l_sql :=
            'CREATE OR REPLACE TRIGGER ' || l_trg_name || CHR(10) ||
            'BEFORE INSERT ON ' || r.table_name || CHR(10) ||
            'FOR EACH ROW ' || CHR(10) ||
            'BEGIN ' || CHR(10) ||
            '    IF :NEW.' || r.column_name || ' IS NULL THEN ' || CHR(10) ||
            '        SELECT ' || l_seq_name || '.NEXTVAL INTO :NEW.' || r.column_name || ' FROM dual; ' || CHR(10) ||
            '    END IF; ' || CHR(10) ||
            'END;';

        EXECUTE IMMEDIATE l_sql;
    END LOOP;
END;
/

-------------------------------------------------------------------------------
