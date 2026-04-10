/*
    Commercial Audit Module - finalized first-stage schema and package
    -----------------------------------------------------------------
    Workflow:
    1. OM
    2. PDP with many-to-many OM mapping
    3. ARPSE with separate DAC and PAC child blocks

    Design decisions:
    - ARPSE is intentionally kept independent from PDP in v1.
      The current CAU flow and app contract do not carry a mandatory PDP reference,
      so PDP lineage is not enforced until business confirms it is required.
    - Every major table uses IS_ACTIVE for soft delete.
    - Every major table has a full-row history table.
    - Inserted rows are snapshotted immediately after insert to establish a baseline.
    - Updated or soft-deleted rows are snapshotted before the main-row change.
    - GET procedures return active rows only by default.
*/

CREATE TABLE CM_COMM_AUDIT_OM (
    OM_ID NUMBER PRIMARY KEY,
    AUDIT_YEAR_ID NUMBER NOT NULL,
    OM_NO VARCHAR2(100) NOT NULL,
    GIST_OF_OM VARCHAR2(500) NOT NULL,
    BODY_OF_OM CLOB NOT NULL,
    MANAGEMENT_RESPONSE CLOB,
    IS_ACTIVE CHAR(1) DEFAULT 'Y' NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE DEFAULT SYSDATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    CONSTRAINT UK_CM_COMM_AUDIT_OM UNIQUE (AUDIT_YEAR_ID, OM_NO),
    CONSTRAINT CK_CM_COMM_AUDIT_OM_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_OM_HIST (
    HIST_ID NUMBER PRIMARY KEY,
    OM_ID NUMBER NOT NULL,
    AUDIT_YEAR_ID NUMBER NOT NULL,
    OM_NO VARCHAR2(100) NOT NULL,
    GIST_OF_OM VARCHAR2(500) NOT NULL,
    BODY_OF_OM CLOB NOT NULL,
    MANAGEMENT_RESPONSE CLOB,
    IS_ACTIVE CHAR(1) NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    SNAPSHOT_ACTION VARCHAR2(30) NOT NULL,
    SNAPSHOT_BY NUMBER NOT NULL,
    SNAPSHOT_ON DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT CK_CM_COMM_AUDIT_OM_H_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_PDP (
    PDP_ID NUMBER PRIMARY KEY,
    AUDIT_YEAR_ID NUMBER NOT NULL,
    PDP_NO VARCHAR2(100) NOT NULL,
    GIST_OF_PDP VARCHAR2(500) NOT NULL,
    BODY_OF_PDP CLOB NOT NULL,
    MANAGEMENT_RESPONSE CLOB,
    DAC_RECOMMENDATIONS CLOB,
    UPDATED_STATUS VARCHAR2(200),
    IS_ACTIVE CHAR(1) DEFAULT 'Y' NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE DEFAULT SYSDATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    CONSTRAINT UK_CM_COMM_AUDIT_PDP UNIQUE (AUDIT_YEAR_ID, PDP_NO),
    CONSTRAINT CK_CM_COMM_AUDIT_PDP_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_PDP_HIST (
    HIST_ID NUMBER PRIMARY KEY,
    PDP_ID NUMBER NOT NULL,
    AUDIT_YEAR_ID NUMBER NOT NULL,
    PDP_NO VARCHAR2(100) NOT NULL,
    GIST_OF_PDP VARCHAR2(500) NOT NULL,
    BODY_OF_PDP CLOB NOT NULL,
    MANAGEMENT_RESPONSE CLOB,
    DAC_RECOMMENDATIONS CLOB,
    UPDATED_STATUS VARCHAR2(200),
    IS_ACTIVE CHAR(1) NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    SNAPSHOT_ACTION VARCHAR2(30) NOT NULL,
    SNAPSHOT_BY NUMBER NOT NULL,
    SNAPSHOT_ON DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT CK_CM_COMM_AUDIT_PDP_H_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_PDP_OM_MAP (
    MAPPING_ID NUMBER PRIMARY KEY,
    PDP_ID NUMBER NOT NULL,
    OM_ID NUMBER NOT NULL,
    IS_ACTIVE CHAR(1) DEFAULT 'Y' NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE DEFAULT SYSDATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    CONSTRAINT UK_CM_COMM_AUDIT_PDP_OM UNIQUE (PDP_ID, OM_ID),
    CONSTRAINT FK_CM_COMM_AUDIT_MAP_PDP FOREIGN KEY (PDP_ID)
        REFERENCES CM_COMM_AUDIT_PDP (PDP_ID),
    CONSTRAINT FK_CM_COMM_AUDIT_MAP_OM FOREIGN KEY (OM_ID)
        REFERENCES CM_COMM_AUDIT_OM (OM_ID),
    CONSTRAINT CK_CM_COMM_AUDIT_MAP_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_PDP_OM_MAP_HIST (
    HIST_ID NUMBER PRIMARY KEY,
    MAPPING_ID NUMBER NOT NULL,
    PDP_ID NUMBER NOT NULL,
    OM_ID NUMBER NOT NULL,
    IS_ACTIVE CHAR(1) NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    SNAPSHOT_ACTION VARCHAR2(30) NOT NULL,
    SNAPSHOT_BY NUMBER NOT NULL,
    SNAPSHOT_ON DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT CK_CM_COMM_AUDIT_MAP_H_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_ARPSE (
    ARPSE_ID NUMBER PRIMARY KEY,
    ARPSE_YEAR_ID NUMBER NOT NULL,
    PARA_NO VARCHAR2(100) NOT NULL,
    GIST_OF_PARA VARCHAR2(500) NOT NULL,
    MANAGEMENT_RESPONSE CLOB,
    IS_ACTIVE CHAR(1) DEFAULT 'Y' NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE DEFAULT SYSDATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    CONSTRAINT UK_CM_COMM_AUDIT_ARPSE UNIQUE (ARPSE_YEAR_ID, PARA_NO),
    CONSTRAINT CK_CM_COMM_AUDIT_ARPSE_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_ARPSE_HIST (
    HIST_ID NUMBER PRIMARY KEY,
    ARPSE_ID NUMBER NOT NULL,
    ARPSE_YEAR_ID NUMBER NOT NULL,
    PARA_NO VARCHAR2(100) NOT NULL,
    GIST_OF_PARA VARCHAR2(500) NOT NULL,
    MANAGEMENT_RESPONSE CLOB,
    IS_ACTIVE CHAR(1) NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    SNAPSHOT_ACTION VARCHAR2(30) NOT NULL,
    SNAPSHOT_BY NUMBER NOT NULL,
    SNAPSHOT_ON DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT CK_CM_COMM_AUDIT_ARPSE_H_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_ARPSE_DAC (
    DAC_ENTRY_ID NUMBER PRIMARY KEY,
    ARPSE_ID NUMBER NOT NULL,
    DAC_RECOMMENDATION CLOB NOT NULL,
    DAC_DATE DATE,
    UPDATED_STATUS VARCHAR2(200),
    IS_ACTIVE CHAR(1) DEFAULT 'Y' NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE DEFAULT SYSDATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    CONSTRAINT FK_CM_COMM_AUDIT_DAC_ARPSE FOREIGN KEY (ARPSE_ID)
        REFERENCES CM_COMM_AUDIT_ARPSE (ARPSE_ID),
    CONSTRAINT CK_CM_COMM_AUDIT_DAC_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_ARPSE_DAC_HIST (
    HIST_ID NUMBER PRIMARY KEY,
    DAC_ENTRY_ID NUMBER NOT NULL,
    ARPSE_ID NUMBER NOT NULL,
    DAC_RECOMMENDATION CLOB NOT NULL,
    DAC_DATE DATE,
    UPDATED_STATUS VARCHAR2(200),
    IS_ACTIVE CHAR(1) NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    SNAPSHOT_ACTION VARCHAR2(30) NOT NULL,
    SNAPSHOT_BY NUMBER NOT NULL,
    SNAPSHOT_ON DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT CK_CM_COMM_AUDIT_DAC_H_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_ARPSE_PAC (
    PAC_ENTRY_ID NUMBER PRIMARY KEY,
    ARPSE_ID NUMBER NOT NULL,
    PAC_DIRECTIVE CLOB NOT NULL,
    PAC_DATE DATE,
    UPDATED_STATUS VARCHAR2(200),
    IS_ACTIVE CHAR(1) DEFAULT 'Y' NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE DEFAULT SYSDATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    CONSTRAINT FK_CM_COMM_AUDIT_PAC_ARPSE FOREIGN KEY (ARPSE_ID)
        REFERENCES CM_COMM_AUDIT_ARPSE (ARPSE_ID),
    CONSTRAINT CK_CM_COMM_AUDIT_PAC_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE TABLE CM_COMM_AUDIT_ARPSE_PAC_HIST (
    HIST_ID NUMBER PRIMARY KEY,
    PAC_ENTRY_ID NUMBER NOT NULL,
    ARPSE_ID NUMBER NOT NULL,
    PAC_DIRECTIVE CLOB NOT NULL,
    PAC_DATE DATE,
    UPDATED_STATUS VARCHAR2(200),
    IS_ACTIVE CHAR(1) NOT NULL,
    CREATED_BY NUMBER NOT NULL,
    CREATED_ON DATE NOT NULL,
    UPDATED_BY NUMBER,
    UPDATED_ON DATE,
    SNAPSHOT_ACTION VARCHAR2(30) NOT NULL,
    SNAPSHOT_BY NUMBER NOT NULL,
    SNAPSHOT_ON DATE DEFAULT SYSDATE NOT NULL,
    CONSTRAINT CK_CM_COMM_AUDIT_PAC_H_ACT CHECK (IS_ACTIVE IN ('Y', 'N'))
);

CREATE SEQUENCE SEQ_CM_COMM_AUDIT_OM START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_OM_HIST START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_PDP START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_PDP_HIST START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_PDP_OM_MAP START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_PDP_OM_MAP_HIST START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_ARPSE START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_ARPSE_HIST START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_ARPSE_DAC START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_ARPSE_DAC_HIST START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_ARPSE_PAC START WITH 1;
CREATE SEQUENCE SEQ_CM_COMM_AUDIT_ARPSE_PAC_HIST START WITH 1;

CREATE OR REPLACE PACKAGE PKG_COMMERCIAL_AUDIT AS
    TYPE T_CURSOR IS REF CURSOR;

    PROCEDURE P_SAVE_OM(
        P_OM_ID IN OUT NUMBER,
        P_AUDIT_YEAR_ID IN NUMBER,
        P_OM_NO IN VARCHAR2,
        P_GIST_OF_OM IN VARCHAR2,
        P_BODY_OF_OM IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    );

    PROCEDURE P_GET_OMS(IO_CURSOR OUT T_CURSOR);

    PROCEDURE P_SAVE_PDP(
        P_PDP_ID IN OUT NUMBER,
        P_AUDIT_YEAR_ID IN NUMBER,
        P_PDP_NO IN VARCHAR2,
        P_GIST_OF_PDP IN VARCHAR2,
        P_BODY_OF_PDP IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_DAC_RECOMMENDATIONS IN CLOB,
        P_UPDATED_STATUS IN VARCHAR2,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    );

    PROCEDURE P_GET_PDPS(IO_CURSOR OUT T_CURSOR);

    PROCEDURE P_SAVE_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        P_OM_IDS_CSV IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    );

    PROCEDURE P_GET_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_ARPSE(
        P_ARPSE_ID IN OUT NUMBER,
        P_ARPSE_YEAR_ID IN NUMBER,
        P_PARA_NO IN VARCHAR2,
        P_GIST_OF_PARA IN VARCHAR2,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    );

    PROCEDURE P_GET_ARPSE_HEADERS(IO_CURSOR OUT T_CURSOR);

    PROCEDURE P_SAVE_ARPSE_DAC(
        P_DAC_ENTRY_ID IN OUT NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_DAC_RECOMMENDATION IN CLOB,
        P_DAC_DATE IN DATE,
        P_UPDATED_STATUS IN VARCHAR2,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    );

    PROCEDURE P_GET_ARPSE_DAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_ARPSE_PAC(
        P_PAC_ENTRY_ID IN OUT NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_PAC_DIRECTIVE IN CLOB,
        P_PAC_DATE IN DATE,
        P_UPDATED_STATUS IN VARCHAR2,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    );

    PROCEDURE P_GET_ARPSE_PAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );
END PKG_COMMERCIAL_AUDIT;
/

CREATE OR REPLACE PACKAGE BODY PKG_COMMERCIAL_AUDIT AS
    C_STATUS_SUCCESS CONSTANT VARCHAR2(10) := 'SUCCESS';
    C_STATUS_FAILED  CONSTANT VARCHAR2(10) := 'FAILED';
    C_ACTIVE         CONSTANT CHAR(1) := 'Y';
    C_INACTIVE       CONSTANT CHAR(1) := 'N';

    FUNCTION NORMALIZE_FLAG(P_VALUE IN VARCHAR2) RETURN CHAR IS
    BEGIN
        RETURN CASE
                   WHEN UPPER(SUBSTR(TRIM(NVL(P_VALUE, C_ACTIVE)), 1, 1)) = C_INACTIVE THEN C_INACTIVE
                   ELSE C_ACTIVE
               END;
    END NORMALIZE_FLAG;

    PROCEDURE SET_SUCCESS(
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_MESSAGE_TEXT IN VARCHAR2
    ) IS
    BEGIN
        P_STATUS := C_STATUS_SUCCESS;
        P_MESSAGE := SUBSTR(P_MESSAGE_TEXT, 1, 4000);
    END SET_SUCCESS;

    PROCEDURE SET_FAILURE(
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_MESSAGE_TEXT IN VARCHAR2
    ) IS
    BEGIN
        P_STATUS := C_STATUS_FAILED;
        P_MESSAGE := SUBSTR(P_MESSAGE_TEXT, 1, 4000);
    END SET_FAILURE;

    PROCEDURE SNAPSHOT_OM(
        P_OM_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_USER_PPNO IN NUMBER
    ) IS
    BEGIN
        INSERT INTO CM_COMM_AUDIT_OM_HIST (
            HIST_ID,
            OM_ID,
            AUDIT_YEAR_ID,
            OM_NO,
            GIST_OF_OM,
            BODY_OF_OM,
            MANAGEMENT_RESPONSE,
            IS_ACTIVE,
            CREATED_BY,
            CREATED_ON,
            UPDATED_BY,
            UPDATED_ON,
            SNAPSHOT_ACTION,
            SNAPSHOT_BY,
            SNAPSHOT_ON
        )
        SELECT SEQ_CM_COMM_AUDIT_OM_HIST.NEXTVAL,
               T.OM_ID,
               T.AUDIT_YEAR_ID,
               T.OM_NO,
               T.GIST_OF_OM,
               T.BODY_OF_OM,
               T.MANAGEMENT_RESPONSE,
               T.IS_ACTIVE,
               T.CREATED_BY,
               T.CREATED_ON,
               T.UPDATED_BY,
               T.UPDATED_ON,
               P_ACTION,
               P_USER_PPNO,
               SYSDATE
          FROM CM_COMM_AUDIT_OM T
         WHERE T.OM_ID = P_OM_ID;
    END SNAPSHOT_OM;

    PROCEDURE SNAPSHOT_PDP(
        P_PDP_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_USER_PPNO IN NUMBER
    ) IS
    BEGIN
        INSERT INTO CM_COMM_AUDIT_PDP_HIST (
            HIST_ID,
            PDP_ID,
            AUDIT_YEAR_ID,
            PDP_NO,
            GIST_OF_PDP,
            BODY_OF_PDP,
            MANAGEMENT_RESPONSE,
            DAC_RECOMMENDATIONS,
            UPDATED_STATUS,
            IS_ACTIVE,
            CREATED_BY,
            CREATED_ON,
            UPDATED_BY,
            UPDATED_ON,
            SNAPSHOT_ACTION,
            SNAPSHOT_BY,
            SNAPSHOT_ON
        )
        SELECT SEQ_CM_COMM_AUDIT_PDP_HIST.NEXTVAL,
               T.PDP_ID,
               T.AUDIT_YEAR_ID,
               T.PDP_NO,
               T.GIST_OF_PDP,
               T.BODY_OF_PDP,
               T.MANAGEMENT_RESPONSE,
               T.DAC_RECOMMENDATIONS,
               T.UPDATED_STATUS,
               T.IS_ACTIVE,
               T.CREATED_BY,
               T.CREATED_ON,
               T.UPDATED_BY,
               T.UPDATED_ON,
               P_ACTION,
               P_USER_PPNO,
               SYSDATE
          FROM CM_COMM_AUDIT_PDP T
         WHERE T.PDP_ID = P_PDP_ID;
    END SNAPSHOT_PDP;

    PROCEDURE SNAPSHOT_PDP_OM_MAP(
        P_MAPPING_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_USER_PPNO IN NUMBER
    ) IS
    BEGIN
        INSERT INTO CM_COMM_AUDIT_PDP_OM_MAP_HIST (
            HIST_ID,
            MAPPING_ID,
            PDP_ID,
            OM_ID,
            IS_ACTIVE,
            CREATED_BY,
            CREATED_ON,
            UPDATED_BY,
            UPDATED_ON,
            SNAPSHOT_ACTION,
            SNAPSHOT_BY,
            SNAPSHOT_ON
        )
        SELECT SEQ_CM_COMM_AUDIT_PDP_OM_MAP_HIST.NEXTVAL,
               T.MAPPING_ID,
               T.PDP_ID,
               T.OM_ID,
               T.IS_ACTIVE,
               T.CREATED_BY,
               T.CREATED_ON,
               T.UPDATED_BY,
               T.UPDATED_ON,
               P_ACTION,
               P_USER_PPNO,
               SYSDATE
          FROM CM_COMM_AUDIT_PDP_OM_MAP T
         WHERE T.MAPPING_ID = P_MAPPING_ID;
    END SNAPSHOT_PDP_OM_MAP;

    PROCEDURE SNAPSHOT_ARPSE(
        P_ARPSE_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_USER_PPNO IN NUMBER
    ) IS
    BEGIN
        INSERT INTO CM_COMM_AUDIT_ARPSE_HIST (
            HIST_ID,
            ARPSE_ID,
            ARPSE_YEAR_ID,
            PARA_NO,
            GIST_OF_PARA,
            MANAGEMENT_RESPONSE,
            IS_ACTIVE,
            CREATED_BY,
            CREATED_ON,
            UPDATED_BY,
            UPDATED_ON,
            SNAPSHOT_ACTION,
            SNAPSHOT_BY,
            SNAPSHOT_ON
        )
        SELECT SEQ_CM_COMM_AUDIT_ARPSE_HIST.NEXTVAL,
               T.ARPSE_ID,
               T.ARPSE_YEAR_ID,
               T.PARA_NO,
               T.GIST_OF_PARA,
               T.MANAGEMENT_RESPONSE,
               T.IS_ACTIVE,
               T.CREATED_BY,
               T.CREATED_ON,
               T.UPDATED_BY,
               T.UPDATED_ON,
               P_ACTION,
               P_USER_PPNO,
               SYSDATE
          FROM CM_COMM_AUDIT_ARPSE T
         WHERE T.ARPSE_ID = P_ARPSE_ID;
    END SNAPSHOT_ARPSE;

    PROCEDURE SNAPSHOT_ARPSE_DAC(
        P_DAC_ENTRY_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_USER_PPNO IN NUMBER
    ) IS
    BEGIN
        INSERT INTO CM_COMM_AUDIT_ARPSE_DAC_HIST (
            HIST_ID,
            DAC_ENTRY_ID,
            ARPSE_ID,
            DAC_RECOMMENDATION,
            DAC_DATE,
            UPDATED_STATUS,
            IS_ACTIVE,
            CREATED_BY,
            CREATED_ON,
            UPDATED_BY,
            UPDATED_ON,
            SNAPSHOT_ACTION,
            SNAPSHOT_BY,
            SNAPSHOT_ON
        )
        SELECT SEQ_CM_COMM_AUDIT_ARPSE_DAC_HIST.NEXTVAL,
               T.DAC_ENTRY_ID,
               T.ARPSE_ID,
               T.DAC_RECOMMENDATION,
               T.DAC_DATE,
               T.UPDATED_STATUS,
               T.IS_ACTIVE,
               T.CREATED_BY,
               T.CREATED_ON,
               T.UPDATED_BY,
               T.UPDATED_ON,
               P_ACTION,
               P_USER_PPNO,
               SYSDATE
          FROM CM_COMM_AUDIT_ARPSE_DAC T
         WHERE T.DAC_ENTRY_ID = P_DAC_ENTRY_ID;
    END SNAPSHOT_ARPSE_DAC;

    PROCEDURE SNAPSHOT_ARPSE_PAC(
        P_PAC_ENTRY_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_USER_PPNO IN NUMBER
    ) IS
    BEGIN
        INSERT INTO CM_COMM_AUDIT_ARPSE_PAC_HIST (
            HIST_ID,
            PAC_ENTRY_ID,
            ARPSE_ID,
            PAC_DIRECTIVE,
            PAC_DATE,
            UPDATED_STATUS,
            IS_ACTIVE,
            CREATED_BY,
            CREATED_ON,
            UPDATED_BY,
            UPDATED_ON,
            SNAPSHOT_ACTION,
            SNAPSHOT_BY,
            SNAPSHOT_ON
        )
        SELECT SEQ_CM_COMM_AUDIT_ARPSE_PAC_HIST.NEXTVAL,
               T.PAC_ENTRY_ID,
               T.ARPSE_ID,
               T.PAC_DIRECTIVE,
               T.PAC_DATE,
               T.UPDATED_STATUS,
               T.IS_ACTIVE,
               T.CREATED_BY,
               T.CREATED_ON,
               T.UPDATED_BY,
               T.UPDATED_ON,
               P_ACTION,
               P_USER_PPNO,
               SYSDATE
          FROM CM_COMM_AUDIT_ARPSE_PAC T
         WHERE T.PAC_ENTRY_ID = P_PAC_ENTRY_ID;
    END SNAPSHOT_ARPSE_PAC;

    PROCEDURE P_SAVE_OM(
        P_OM_ID IN OUT NUMBER,
        P_AUDIT_YEAR_ID IN NUMBER,
        P_OM_NO IN VARCHAR2,
        P_GIST_OF_OM IN VARCHAR2,
        P_BODY_OF_OM IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    ) IS
        V_FLAG CHAR(1) := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_EXISTS NUMBER;
    BEGIN
        IF NVL(P_AUDIT_YEAR_ID, 0) <= 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Audit Year is required.');
        ELSIF TRIM(P_OM_NO) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20002, 'OM No is required.');
        ELSIF TRIM(P_GIST_OF_OM) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20003, 'Gist of OM is required.');
        ELSIF P_BODY_OF_OM IS NULL THEN
            RAISE_APPLICATION_ERROR(-20004, 'Body of OM is required.');
        END IF;

        IF NVL(P_OM_ID, 0) = 0 THEN
            P_OM_ID := SEQ_CM_COMM_AUDIT_OM.NEXTVAL;

            INSERT INTO CM_COMM_AUDIT_OM (
                OM_ID,
                AUDIT_YEAR_ID,
                OM_NO,
                GIST_OF_OM,
                BODY_OF_OM,
                MANAGEMENT_RESPONSE,
                IS_ACTIVE,
                CREATED_BY,
                CREATED_ON
            ) VALUES (
                P_OM_ID,
                P_AUDIT_YEAR_ID,
                TRIM(P_OM_NO),
                TRIM(P_GIST_OF_OM),
                P_BODY_OF_OM,
                P_MANAGEMENT_RESPONSE,
                V_FLAG,
                P_USER_PPNO,
                SYSDATE
            );

            SNAPSHOT_OM(P_OM_ID, 'INSERT', P_USER_PPNO);
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'OM saved successfully.');
        ELSE
            SELECT COUNT(*)
              INTO V_EXISTS
              FROM CM_COMM_AUDIT_OM
             WHERE OM_ID = P_OM_ID;

            IF V_EXISTS = 0 THEN
                RAISE_APPLICATION_ERROR(-20005, 'OM record not found.');
            END IF;

            SNAPSHOT_OM(P_OM_ID, 'UPDATE_BEFORE', P_USER_PPNO);

            UPDATE CM_COMM_AUDIT_OM
               SET AUDIT_YEAR_ID = P_AUDIT_YEAR_ID,
                   OM_NO = TRIM(P_OM_NO),
                   GIST_OF_OM = TRIM(P_GIST_OF_OM),
                   BODY_OF_OM = P_BODY_OF_OM,
                   MANAGEMENT_RESPONSE = P_MANAGEMENT_RESPONSE,
                   IS_ACTIVE = V_FLAG,
                   UPDATED_BY = P_USER_PPNO,
                   UPDATED_ON = SYSDATE
             WHERE OM_ID = P_OM_ID;

            SET_SUCCESS(P_STATUS, P_MESSAGE, 'OM updated successfully.');
        END IF;
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'OM No already exists for the selected Audit Year.');
        WHEN OTHERS THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save OM. ' || SUBSTR(SQLERRM, 1, 3500));
    END P_SAVE_OM;

    PROCEDURE P_GET_OMS(IO_CURSOR OUT T_CURSOR) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT O.OM_ID,
                   O.AUDIT_YEAR_ID,
                   TO_CHAR(O.AUDIT_YEAR_ID) AS AUDIT_YEAR_TEXT,
                   O.OM_NO,
                   O.GIST_OF_OM,
                   O.BODY_OF_OM,
                   O.MANAGEMENT_RESPONSE,
                   O.IS_ACTIVE
              FROM CM_COMM_AUDIT_OM O
             WHERE O.IS_ACTIVE = C_ACTIVE
             ORDER BY O.AUDIT_YEAR_ID DESC, O.OM_NO;
    END P_GET_OMS;

    PROCEDURE P_SAVE_PDP(
        P_PDP_ID IN OUT NUMBER,
        P_AUDIT_YEAR_ID IN NUMBER,
        P_PDP_NO IN VARCHAR2,
        P_GIST_OF_PDP IN VARCHAR2,
        P_BODY_OF_PDP IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_DAC_RECOMMENDATIONS IN CLOB,
        P_UPDATED_STATUS IN VARCHAR2,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    ) IS
        V_FLAG CHAR(1) := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_EXISTS NUMBER;
    BEGIN
        IF NVL(P_AUDIT_YEAR_ID, 0) <= 0 THEN
            RAISE_APPLICATION_ERROR(-20011, 'Audit Year is required.');
        ELSIF TRIM(P_PDP_NO) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20012, 'PDP No is required.');
        ELSIF TRIM(P_GIST_OF_PDP) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20013, 'Gist of PDP is required.');
        ELSIF P_BODY_OF_PDP IS NULL THEN
            RAISE_APPLICATION_ERROR(-20014, 'Body of PDP is required.');
        END IF;

        IF NVL(P_PDP_ID, 0) = 0 THEN
            P_PDP_ID := SEQ_CM_COMM_AUDIT_PDP.NEXTVAL;

            INSERT INTO CM_COMM_AUDIT_PDP (
                PDP_ID,
                AUDIT_YEAR_ID,
                PDP_NO,
                GIST_OF_PDP,
                BODY_OF_PDP,
                MANAGEMENT_RESPONSE,
                DAC_RECOMMENDATIONS,
                UPDATED_STATUS,
                IS_ACTIVE,
                CREATED_BY,
                CREATED_ON
            ) VALUES (
                P_PDP_ID,
                P_AUDIT_YEAR_ID,
                TRIM(P_PDP_NO),
                TRIM(P_GIST_OF_PDP),
                P_BODY_OF_PDP,
                P_MANAGEMENT_RESPONSE,
                P_DAC_RECOMMENDATIONS,
                TRIM(P_UPDATED_STATUS),
                V_FLAG,
                P_USER_PPNO,
                SYSDATE
            );

            SNAPSHOT_PDP(P_PDP_ID, 'INSERT', P_USER_PPNO);
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP saved successfully.');
        ELSE
            SELECT COUNT(*)
              INTO V_EXISTS
              FROM CM_COMM_AUDIT_PDP
             WHERE PDP_ID = P_PDP_ID;

            IF V_EXISTS = 0 THEN
                RAISE_APPLICATION_ERROR(-20015, 'PDP record not found.');
            END IF;

            SNAPSHOT_PDP(P_PDP_ID, 'UPDATE_BEFORE', P_USER_PPNO);

            UPDATE CM_COMM_AUDIT_PDP
               SET AUDIT_YEAR_ID = P_AUDIT_YEAR_ID,
                   PDP_NO = TRIM(P_PDP_NO),
                   GIST_OF_PDP = TRIM(P_GIST_OF_PDP),
                   BODY_OF_PDP = P_BODY_OF_PDP,
                   MANAGEMENT_RESPONSE = P_MANAGEMENT_RESPONSE,
                   DAC_RECOMMENDATIONS = P_DAC_RECOMMENDATIONS,
                   UPDATED_STATUS = TRIM(P_UPDATED_STATUS),
                   IS_ACTIVE = V_FLAG,
                   UPDATED_BY = P_USER_PPNO,
                   UPDATED_ON = SYSDATE
             WHERE PDP_ID = P_PDP_ID;

            SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP updated successfully.');
        END IF;
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'PDP No already exists for the selected Audit Year.');
        WHEN OTHERS THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save PDP. ' || SUBSTR(SQLERRM, 1, 3500));
    END P_SAVE_PDP;

    PROCEDURE P_GET_PDPS(IO_CURSOR OUT T_CURSOR) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT P.PDP_ID,
                   P.AUDIT_YEAR_ID,
                   TO_CHAR(P.AUDIT_YEAR_ID) AS AUDIT_YEAR_TEXT,
                   P.PDP_NO,
                   P.GIST_OF_PDP,
                   P.BODY_OF_PDP,
                   P.MANAGEMENT_RESPONSE,
                   P.DAC_RECOMMENDATIONS,
                   P.UPDATED_STATUS,
                   P.IS_ACTIVE,
                   NVL(M.LINKED_OM_COUNT, 0) AS LINKED_OM_COUNT,
                   M.LINKED_OM_NUMBERS
              FROM CM_COMM_AUDIT_PDP P
              LEFT JOIN (
                    SELECT PM.PDP_ID,
                           COUNT(*) AS LINKED_OM_COUNT,
                           LISTAGG(O.OM_NO, ', ') WITHIN GROUP (ORDER BY O.OM_NO) AS LINKED_OM_NUMBERS
                      FROM CM_COMM_AUDIT_PDP_OM_MAP PM
                      JOIN CM_COMM_AUDIT_OM O
                        ON O.OM_ID = PM.OM_ID
                       AND O.IS_ACTIVE = C_ACTIVE
                     WHERE PM.IS_ACTIVE = C_ACTIVE
                     GROUP BY PM.PDP_ID
                ) M
                ON M.PDP_ID = P.PDP_ID
             WHERE P.IS_ACTIVE = C_ACTIVE
             ORDER BY P.AUDIT_YEAR_ID DESC, P.PDP_NO;
    END P_GET_PDPS;

    PROCEDURE P_SAVE_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        P_OM_IDS_CSV IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    ) IS
        V_FLAG CHAR(1) := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_PDP_EXISTS NUMBER;
        V_CSV VARCHAR2(32767);
        V_INVALID_COUNT NUMBER;
        V_MAPPING_ID NUMBER;
        V_IS_ACTIVE CHAR(1);
    BEGIN
        IF NVL(P_PDP_ID, 0) <= 0 THEN
            RAISE_APPLICATION_ERROR(-20021, 'Saved PDP is required before mapping OMs.');
        END IF;

        SELECT COUNT(*)
          INTO V_PDP_EXISTS
          FROM CM_COMM_AUDIT_PDP
         WHERE PDP_ID = P_PDP_ID
           AND IS_ACTIVE = C_ACTIVE;

        IF V_PDP_EXISTS = 0 THEN
            RAISE_APPLICATION_ERROR(-20022, 'Selected PDP does not exist or is inactive.');
        END IF;

        V_CSV := TRIM(DBMS_LOB.SUBSTR(P_OM_IDS_CSV, 32767, 1));

        IF V_CSV IS NOT NULL THEN
            SELECT COUNT(*)
              INTO V_INVALID_COUNT
              FROM (
                    SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_CSV, '[^,]+', 1, LEVEL))) AS OM_ID
                      FROM DUAL
                    CONNECT BY REGEXP_SUBSTR(V_CSV, '[^,]+', 1, LEVEL) IS NOT NULL
                   ) SRC
             WHERE NOT EXISTS (
                       SELECT 1
                         FROM CM_COMM_AUDIT_OM O
                        WHERE O.OM_ID = SRC.OM_ID
                          AND O.IS_ACTIVE = C_ACTIVE
                   );

            IF V_INVALID_COUNT > 0 THEN
                RAISE_APPLICATION_ERROR(-20023, 'One or more selected OMs are invalid or inactive.');
            END IF;
        END IF;

        IF V_FLAG = C_INACTIVE THEN
            FOR REC IN (
                SELECT MAPPING_ID
                  FROM CM_COMM_AUDIT_PDP_OM_MAP
                 WHERE PDP_ID = P_PDP_ID
                   AND IS_ACTIVE = C_ACTIVE
            ) LOOP
                SNAPSHOT_PDP_OM_MAP(REC.MAPPING_ID, 'DEACTIVATE_BEFORE', P_USER_PPNO);

                UPDATE CM_COMM_AUDIT_PDP_OM_MAP
                   SET IS_ACTIVE = C_INACTIVE,
                       UPDATED_BY = P_USER_PPNO,
                       UPDATED_ON = SYSDATE
                 WHERE MAPPING_ID = REC.MAPPING_ID;
            END LOOP;

            SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP OM mappings deactivated successfully.');
            RETURN;
        END IF;

        FOR REC IN (
            SELECT MAPPING_ID
              FROM CM_COMM_AUDIT_PDP_OM_MAP
             WHERE PDP_ID = P_PDP_ID
               AND IS_ACTIVE = C_ACTIVE
               AND OM_ID NOT IN (
                    SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_CSV, '[^,]+', 1, LEVEL)))
                      FROM DUAL
                    CONNECT BY REGEXP_SUBSTR(V_CSV, '[^,]+', 1, LEVEL) IS NOT NULL
               )
        ) LOOP
            SNAPSHOT_PDP_OM_MAP(REC.MAPPING_ID, 'DEACTIVATE_BEFORE', P_USER_PPNO);

            UPDATE CM_COMM_AUDIT_PDP_OM_MAP
               SET IS_ACTIVE = C_INACTIVE,
                   UPDATED_BY = P_USER_PPNO,
                   UPDATED_ON = SYSDATE
             WHERE MAPPING_ID = REC.MAPPING_ID;
        END LOOP;

        IF V_CSV IS NOT NULL THEN
            FOR REC IN (
                SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_CSV, '[^,]+', 1, LEVEL))) AS OM_ID
                  FROM DUAL
                CONNECT BY REGEXP_SUBSTR(V_CSV, '[^,]+', 1, LEVEL) IS NOT NULL
            ) LOOP
                BEGIN
                    SELECT MAPPING_ID, IS_ACTIVE
                      INTO V_MAPPING_ID, V_IS_ACTIVE
                      FROM CM_COMM_AUDIT_PDP_OM_MAP
                     WHERE PDP_ID = P_PDP_ID
                       AND OM_ID = REC.OM_ID;

                    IF V_IS_ACTIVE = C_INACTIVE THEN
                        SNAPSHOT_PDP_OM_MAP(V_MAPPING_ID, 'REACTIVATE_BEFORE', P_USER_PPNO);

                        UPDATE CM_COMM_AUDIT_PDP_OM_MAP
                           SET IS_ACTIVE = C_ACTIVE,
                               UPDATED_BY = P_USER_PPNO,
                               UPDATED_ON = SYSDATE
                         WHERE MAPPING_ID = V_MAPPING_ID;
                    END IF;
                EXCEPTION
                    WHEN NO_DATA_FOUND THEN
                        V_MAPPING_ID := SEQ_CM_COMM_AUDIT_PDP_OM_MAP.NEXTVAL;

                        INSERT INTO CM_COMM_AUDIT_PDP_OM_MAP (
                            MAPPING_ID,
                            PDP_ID,
                            OM_ID,
                            IS_ACTIVE,
                            CREATED_BY,
                            CREATED_ON
                        ) VALUES (
                            V_MAPPING_ID,
                            P_PDP_ID,
                            REC.OM_ID,
                            C_ACTIVE,
                            P_USER_PPNO,
                            SYSDATE
                        );

                        SNAPSHOT_PDP_OM_MAP(V_MAPPING_ID, 'INSERT', P_USER_PPNO);
                END;
            END LOOP;
        END IF;

        SET_SUCCESS(P_STATUS, P_MESSAGE, 'Linked OMs saved successfully.');
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Duplicate PDP OM mapping detected.');
        WHEN OTHERS THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save PDP OM mappings. ' || SUBSTR(SQLERRM, 1, 3500));
    END P_SAVE_PDP_OM_MAP;

    PROCEDURE P_GET_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT M.MAPPING_ID,
                   M.PDP_ID,
                   M.OM_ID,
                   O.OM_NO,
                   O.GIST_OF_OM,
                   M.IS_ACTIVE
              FROM CM_COMM_AUDIT_PDP_OM_MAP M
              JOIN CM_COMM_AUDIT_OM O
                ON O.OM_ID = M.OM_ID
               AND O.IS_ACTIVE = C_ACTIVE
             WHERE M.PDP_ID = P_PDP_ID
               AND M.IS_ACTIVE = C_ACTIVE
             ORDER BY O.OM_NO;
    END P_GET_PDP_OM_MAP;

    PROCEDURE P_SAVE_ARPSE(
        P_ARPSE_ID IN OUT NUMBER,
        P_ARPSE_YEAR_ID IN NUMBER,
        P_PARA_NO IN VARCHAR2,
        P_GIST_OF_PARA IN VARCHAR2,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    ) IS
        V_FLAG CHAR(1) := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_EXISTS NUMBER;
    BEGIN
        IF NVL(P_ARPSE_YEAR_ID, 0) <= 0 THEN
            RAISE_APPLICATION_ERROR(-20031, 'ARPSE Year is required.');
        ELSIF TRIM(P_PARA_NO) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20032, 'Para No is required.');
        ELSIF TRIM(P_GIST_OF_PARA) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20033, 'Gist of Para is required.');
        END IF;

        IF NVL(P_ARPSE_ID, 0) = 0 THEN
            P_ARPSE_ID := SEQ_CM_COMM_AUDIT_ARPSE.NEXTVAL;

            INSERT INTO CM_COMM_AUDIT_ARPSE (
                ARPSE_ID,
                ARPSE_YEAR_ID,
                PARA_NO,
                GIST_OF_PARA,
                MANAGEMENT_RESPONSE,
                IS_ACTIVE,
                CREATED_BY,
                CREATED_ON
            ) VALUES (
                P_ARPSE_ID,
                P_ARPSE_YEAR_ID,
                TRIM(P_PARA_NO),
                TRIM(P_GIST_OF_PARA),
                P_MANAGEMENT_RESPONSE,
                V_FLAG,
                P_USER_PPNO,
                SYSDATE
            );

            SNAPSHOT_ARPSE(P_ARPSE_ID, 'INSERT', P_USER_PPNO);
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE header saved successfully.');
        ELSE
            SELECT COUNT(*)
              INTO V_EXISTS
              FROM CM_COMM_AUDIT_ARPSE
             WHERE ARPSE_ID = P_ARPSE_ID;

            IF V_EXISTS = 0 THEN
                RAISE_APPLICATION_ERROR(-20034, 'ARPSE header not found.');
            END IF;

            SNAPSHOT_ARPSE(P_ARPSE_ID, 'UPDATE_BEFORE', P_USER_PPNO);

            UPDATE CM_COMM_AUDIT_ARPSE
               SET ARPSE_YEAR_ID = P_ARPSE_YEAR_ID,
                   PARA_NO = TRIM(P_PARA_NO),
                   GIST_OF_PARA = TRIM(P_GIST_OF_PARA),
                   MANAGEMENT_RESPONSE = P_MANAGEMENT_RESPONSE,
                   IS_ACTIVE = V_FLAG,
                   UPDATED_BY = P_USER_PPNO,
                   UPDATED_ON = SYSDATE
             WHERE ARPSE_ID = P_ARPSE_ID;

            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE header updated successfully.');
        END IF;
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Para No already exists for the selected ARPSE Year.');
        WHEN OTHERS THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE header. ' || SUBSTR(SQLERRM, 1, 3500));
    END P_SAVE_ARPSE;

    PROCEDURE P_GET_ARPSE_HEADERS(IO_CURSOR OUT T_CURSOR) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT A.ARPSE_ID,
                   A.ARPSE_YEAR_ID,
                   TO_CHAR(A.ARPSE_YEAR_ID) AS ARPSE_YEAR_TEXT,
                   A.PARA_NO,
                   A.GIST_OF_PARA,
                   A.MANAGEMENT_RESPONSE,
                   A.IS_ACTIVE
              FROM CM_COMM_AUDIT_ARPSE A
             WHERE A.IS_ACTIVE = C_ACTIVE
             ORDER BY A.ARPSE_YEAR_ID DESC, A.PARA_NO;
    END P_GET_ARPSE_HEADERS;

    PROCEDURE P_SAVE_ARPSE_DAC(
        P_DAC_ENTRY_ID IN OUT NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_DAC_RECOMMENDATION IN CLOB,
        P_DAC_DATE IN DATE,
        P_UPDATED_STATUS IN VARCHAR2,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    ) IS
        V_FLAG CHAR(1) := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_EXISTS NUMBER;
    BEGIN
        IF NVL(P_ARPSE_ID, 0) <= 0 THEN
            RAISE_APPLICATION_ERROR(-20041, 'Saved ARPSE header is required.');
        ELSIF P_DAC_RECOMMENDATION IS NULL THEN
            RAISE_APPLICATION_ERROR(-20042, 'DAC Recommendation is required.');
        END IF;

        SELECT COUNT(*)
          INTO V_EXISTS
          FROM CM_COMM_AUDIT_ARPSE
         WHERE ARPSE_ID = P_ARPSE_ID
           AND IS_ACTIVE = C_ACTIVE;

        IF V_EXISTS = 0 THEN
            RAISE_APPLICATION_ERROR(-20043, 'Selected ARPSE header does not exist or is inactive.');
        END IF;

        IF NVL(P_DAC_ENTRY_ID, 0) = 0 THEN
            P_DAC_ENTRY_ID := SEQ_CM_COMM_AUDIT_ARPSE_DAC.NEXTVAL;

            INSERT INTO CM_COMM_AUDIT_ARPSE_DAC (
                DAC_ENTRY_ID,
                ARPSE_ID,
                DAC_RECOMMENDATION,
                DAC_DATE,
                UPDATED_STATUS,
                IS_ACTIVE,
                CREATED_BY,
                CREATED_ON
            ) VALUES (
                P_DAC_ENTRY_ID,
                P_ARPSE_ID,
                P_DAC_RECOMMENDATION,
                P_DAC_DATE,
                TRIM(P_UPDATED_STATUS),
                V_FLAG,
                P_USER_PPNO,
                SYSDATE
            );

            SNAPSHOT_ARPSE_DAC(P_DAC_ENTRY_ID, 'INSERT', P_USER_PPNO);
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE DAC entry saved successfully.');
        ELSE
            SELECT COUNT(*)
              INTO V_EXISTS
              FROM CM_COMM_AUDIT_ARPSE_DAC
             WHERE DAC_ENTRY_ID = P_DAC_ENTRY_ID;

            IF V_EXISTS = 0 THEN
                RAISE_APPLICATION_ERROR(-20044, 'ARPSE DAC entry not found.');
            END IF;

            SNAPSHOT_ARPSE_DAC(P_DAC_ENTRY_ID, 'UPDATE_BEFORE', P_USER_PPNO);

            UPDATE CM_COMM_AUDIT_ARPSE_DAC
               SET ARPSE_ID = P_ARPSE_ID,
                   DAC_RECOMMENDATION = P_DAC_RECOMMENDATION,
                   DAC_DATE = P_DAC_DATE,
                   UPDATED_STATUS = TRIM(P_UPDATED_STATUS),
                   IS_ACTIVE = V_FLAG,
                   UPDATED_BY = P_USER_PPNO,
                   UPDATED_ON = SYSDATE
             WHERE DAC_ENTRY_ID = P_DAC_ENTRY_ID;

            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE DAC entry updated successfully.');
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE DAC entry. ' || SUBSTR(SQLERRM, 1, 3500));
    END P_SAVE_ARPSE_DAC;

    PROCEDURE P_GET_ARPSE_DAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT D.DAC_ENTRY_ID,
                   D.ARPSE_ID,
                   D.DAC_RECOMMENDATION,
                   D.DAC_DATE,
                   D.UPDATED_STATUS,
                   D.IS_ACTIVE
              FROM CM_COMM_AUDIT_ARPSE_DAC D
             WHERE D.ARPSE_ID = P_ARPSE_ID
               AND D.IS_ACTIVE = C_ACTIVE
             ORDER BY NVL(D.DAC_DATE, D.CREATED_ON) DESC, D.DAC_ENTRY_ID DESC;
    END P_GET_ARPSE_DAC_ENTRIES;

    PROCEDURE P_SAVE_ARPSE_PAC(
        P_PAC_ENTRY_ID IN OUT NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_PAC_DIRECTIVE IN CLOB,
        P_PAC_DATE IN DATE,
        P_UPDATED_STATUS IN VARCHAR2,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2
    ) IS
        V_FLAG CHAR(1) := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_EXISTS NUMBER;
    BEGIN
        IF NVL(P_ARPSE_ID, 0) <= 0 THEN
            RAISE_APPLICATION_ERROR(-20051, 'Saved ARPSE header is required.');
        ELSIF P_PAC_DIRECTIVE IS NULL THEN
            RAISE_APPLICATION_ERROR(-20052, 'PAC Directive is required.');
        END IF;

        SELECT COUNT(*)
          INTO V_EXISTS
          FROM CM_COMM_AUDIT_ARPSE
         WHERE ARPSE_ID = P_ARPSE_ID
           AND IS_ACTIVE = C_ACTIVE;

        IF V_EXISTS = 0 THEN
            RAISE_APPLICATION_ERROR(-20053, 'Selected ARPSE header does not exist or is inactive.');
        END IF;

        IF NVL(P_PAC_ENTRY_ID, 0) = 0 THEN
            P_PAC_ENTRY_ID := SEQ_CM_COMM_AUDIT_ARPSE_PAC.NEXTVAL;

            INSERT INTO CM_COMM_AUDIT_ARPSE_PAC (
                PAC_ENTRY_ID,
                ARPSE_ID,
                PAC_DIRECTIVE,
                PAC_DATE,
                UPDATED_STATUS,
                IS_ACTIVE,
                CREATED_BY,
                CREATED_ON
            ) VALUES (
                P_PAC_ENTRY_ID,
                P_ARPSE_ID,
                P_PAC_DIRECTIVE,
                P_PAC_DATE,
                TRIM(P_UPDATED_STATUS),
                V_FLAG,
                P_USER_PPNO,
                SYSDATE
            );

            SNAPSHOT_ARPSE_PAC(P_PAC_ENTRY_ID, 'INSERT', P_USER_PPNO);
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE PAC entry saved successfully.');
        ELSE
            SELECT COUNT(*)
              INTO V_EXISTS
              FROM CM_COMM_AUDIT_ARPSE_PAC
             WHERE PAC_ENTRY_ID = P_PAC_ENTRY_ID;

            IF V_EXISTS = 0 THEN
                RAISE_APPLICATION_ERROR(-20054, 'ARPSE PAC entry not found.');
            END IF;

            SNAPSHOT_ARPSE_PAC(P_PAC_ENTRY_ID, 'UPDATE_BEFORE', P_USER_PPNO);

            UPDATE CM_COMM_AUDIT_ARPSE_PAC
               SET ARPSE_ID = P_ARPSE_ID,
                   PAC_DIRECTIVE = P_PAC_DIRECTIVE,
                   PAC_DATE = P_PAC_DATE,
                   UPDATED_STATUS = TRIM(P_UPDATED_STATUS),
                   IS_ACTIVE = V_FLAG,
                   UPDATED_BY = P_USER_PPNO,
                   UPDATED_ON = SYSDATE
             WHERE PAC_ENTRY_ID = P_PAC_ENTRY_ID;

            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE PAC entry updated successfully.');
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE PAC entry. ' || SUBSTR(SQLERRM, 1, 3500));
    END P_SAVE_ARPSE_PAC;

    PROCEDURE P_GET_ARPSE_PAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT P.PAC_ENTRY_ID,
                   P.ARPSE_ID,
                   P.PAC_DIRECTIVE,
                   P.PAC_DATE,
                   P.UPDATED_STATUS,
                   P.IS_ACTIVE
              FROM CM_COMM_AUDIT_ARPSE_PAC P
             WHERE P.ARPSE_ID = P_ARPSE_ID
               AND P.IS_ACTIVE = C_ACTIVE
             ORDER BY NVL(P.PAC_DATE, P.CREATED_ON) DESC, P.PAC_ENTRY_ID DESC;
    END P_GET_ARPSE_PAC_ENTRIES;
END PKG_COMMERCIAL_AUDIT;
/
