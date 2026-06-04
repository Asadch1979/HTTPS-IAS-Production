CREATE OR REPLACE PACKAGE PKG_COMMERCIAL_AUDIT AS
    TYPE T_CURSOR IS REF CURSOR;

    PROCEDURE P_CAU_OM_YEAR(
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_OM(
        P_OM_ID IN NUMBER,
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
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_OMS(
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_PDP(
        P_PDP_ID IN NUMBER,
        P_AUDIT_YEAR_ID IN NUMBER,
        P_PDP_NO IN VARCHAR2,
        P_GIST_OF_PDP IN VARCHAR2,
        P_BODY_OF_PDP IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_DAC_RECOMMENDATIONS IN CLOB,
        P_UPDATE_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_PDPS(
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        P_OM_IDS_CSV IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_ARPSE(
        P_ARPSE_ID IN NUMBER,
        P_ARPSE_YEAR_ID IN NUMBER,
        P_PARA_NO IN VARCHAR2,
        P_GIST_OF_PARA IN VARCHAR2,
        P_BODY_OF_PARA IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_ARPSE_HEADERS(
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_ARPSE_PDP_MAP(
        P_ARPSE_ID IN NUMBER,
        P_PDP_IDS_CSV IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_ARPSE_PDP_MAP(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_ARPSE_DAC(
        P_DAC_ENTRY_ID IN NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_DAC_RECOMMENDATION IN CLOB,
        P_DAC_DATE IN DATE,
        P_UPDATED_STATUS IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_ARPSE_DAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );

    PROCEDURE P_SAVE_ARPSE_PAC(
        P_PAC_ENTRY_ID IN NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_PAC_DIRECTIVE IN CLOB,
        P_PAC_DATE IN DATE,
        P_UPDATED_STATUS IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    );

    PROCEDURE P_GET_ARPSE_PAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    );
END PKG_COMMERCIAL_AUDIT;
/
CREATE OR REPLACE PACKAGE BODY PKG_COMMERCIAL_AUDIT AS
    C_SUCCESS CONSTANT VARCHAR2(10) := 'SUCCESS';
    C_FAILED CONSTANT VARCHAR2(10) := 'FAILED';
    C_ACTIVE CONSTANT CHAR(1) := 'Y';
    C_INACTIVE CONSTANT CHAR(1) := 'N';

    FUNCTION NORMALIZE_FLAG(P_VALUE IN VARCHAR2) RETURN CHAR IS
    BEGIN
        RETURN CASE WHEN UPPER(TRIM(NVL(P_VALUE, 'Y'))) = 'N' THEN C_INACTIVE ELSE C_ACTIVE END;
    END NORMALIZE_FLAG;

    FUNCTION CLOB_TO_VARCHAR(P_VALUE IN CLOB) RETURN VARCHAR2 IS
    BEGIN
        IF P_VALUE IS NULL THEN
            RETURN NULL;
        END IF;

        RETURN DBMS_LOB.SUBSTR(P_VALUE, 32767, 1);
    END CLOB_TO_VARCHAR;

    FUNCTION IS_BLANK(P_VALUE IN VARCHAR2) RETURN BOOLEAN IS
    BEGIN
        IF TRIM(NVL(P_VALUE, '')) IS NULL THEN
            RETURN TRUE;
        END IF;

        RETURN FALSE;
    END IS_BLANK;

    FUNCTION IS_BLANK_CLOB(P_VALUE IN CLOB) RETURN BOOLEAN IS
        V_TEXT VARCHAR2(32767);
    BEGIN
        IF P_VALUE IS NULL THEN
            RETURN TRUE;
        END IF;

        V_TEXT := REGEXP_REPLACE(NVL(CLOB_TO_VARCHAR(P_VALUE), ''), '[[:space:]]', '');
        IF V_TEXT IS NULL THEN
            RETURN TRUE;
        END IF;

        RETURN FALSE;
    END IS_BLANK_CLOB;

    PROCEDURE SET_SUCCESS(
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_MESSAGE_TEXT IN VARCHAR2
    ) IS
    BEGIN
        P_STATUS := C_SUCCESS;
        P_MESSAGE := NVL(P_MESSAGE_TEXT, 'Operation completed successfully.');
    END SET_SUCCESS;

    PROCEDURE SET_FAILURE(
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_MESSAGE_TEXT IN VARCHAR2
    ) IS
    BEGIN
        P_STATUS := C_FAILED;
        P_MESSAGE := SUBSTR(NVL(P_MESSAGE_TEXT, 'Operation failed.'), 1, 4000);
    END SET_FAILURE;

    PROCEDURE P_CAU_OM_YEAR(
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT p.auditperiodid,
                p.description AS PERIOD
            FROM t_cau_period p
            WHERE p.status_id = C_ACTIVE
            ORDER BY START_DATE DESC, AUDITPERIODID DESC;
    END P_CAU_OM_YEAR;

    PROCEDURE SNAPSHOT_OM(
        P_OM_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_OM_HIST (
            HIST_ID, OM_ID, AUDIT_YEAR_ID, OM_NO, GIST_OF_OM, BODY_OF_OM, MANAGEMENT_RESPONSE,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_OM_HIST.NEXTVAL, OM_ID, AUDIT_YEAR_ID, OM_NO, GIST_OF_OM, BODY_OF_OM, MANAGEMENT_RESPONSE,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_OM
        WHERE OM_ID = P_OM_ID;
    END SNAPSHOT_OM;

    PROCEDURE SNAPSHOT_PDP(
        P_PDP_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_PDP_HIST (
            HIST_ID, PDP_ID, AUDIT_YEAR_ID, PDP_NO, GIST_OF_PDP, BODY_OF_PDP, MANAGEMENT_RESPONSE,
            DAC_RECOMMENDATIONS, UPDATE_MANAGEMENT_RESPONSE, IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_PDP_HIST.NEXTVAL, PDP_ID, AUDIT_YEAR_ID, PDP_NO, GIST_OF_PDP, BODY_OF_PDP, MANAGEMENT_RESPONSE,
            DAC_RECOMMENDATIONS, UPDATE_MANAGEMENT_RESPONSE, IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_PDP
        WHERE PDP_ID = P_PDP_ID;
    END SNAPSHOT_PDP;

    PROCEDURE SNAPSHOT_PDP_MAP(
        P_MAPPING_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_PDP_OM_MAP_HIST (
            HIST_ID, MAPPING_ID, PDP_ID, OM_ID, IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_PDP_OM_MAP_HIST.NEXTVAL, MAPPING_ID, PDP_ID, OM_ID, IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_PDP_OM_MAP
        WHERE MAPPING_ID = P_MAPPING_ID;
    END SNAPSHOT_PDP_MAP;

    PROCEDURE SNAPSHOT_ARPSE(
        P_ARPSE_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_ARPSE_HIST (
            HIST_ID, ARPSE_ID, ARPSE_YEAR_ID, PARA_NO, GIST_OF_PARA, BODY_OF_PARA, MANAGEMENT_RESPONSE,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_ARPSE_HIST.NEXTVAL, ARPSE_ID, ARPSE_YEAR_ID, PARA_NO, GIST_OF_PARA, BODY_OF_PARA, MANAGEMENT_RESPONSE,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_ARPSE
        WHERE ARPSE_ID = P_ARPSE_ID;
    END SNAPSHOT_ARPSE;

    PROCEDURE SNAPSHOT_ARPSE_PDP_MAP(
        P_MAPPING_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_ARPSE_PDP_MAP_HIST (
            HIST_ID, MAPPING_ID, ARPSE_ID, PDP_ID, IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_ARPSE_PDP_MAP_HIST.NEXTVAL, MAPPING_ID, ARPSE_ID, PDP_ID, IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_ARPSE_PDP_MAP
        WHERE MAPPING_ID = P_MAPPING_ID;
    END SNAPSHOT_ARPSE_PDP_MAP;

    PROCEDURE SNAPSHOT_DAC(
        P_DAC_ENTRY_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_ARPSE_DAC_HIST (
            HIST_ID, DAC_ENTRY_ID, ARPSE_ID, DAC_RECOMMENDATION, DAC_DATE, UPDATED_STATUS,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_ARPSE_DAC_HIST.NEXTVAL, DAC_ENTRY_ID, ARPSE_ID, DAC_RECOMMENDATION, DAC_DATE, UPDATED_STATUS,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_ARPSE_DAC
        WHERE DAC_ENTRY_ID = P_DAC_ENTRY_ID;
    END SNAPSHOT_DAC;

    PROCEDURE SNAPSHOT_PAC(
        P_PAC_ENTRY_ID IN NUMBER,
        P_ACTION IN VARCHAR2,
        P_SNAPSHOT_BY IN NUMBER
    ) IS
    BEGIN
        INSERT INTO T_COM_AUDIT_ARPSE_PAC_HIST (
            HIST_ID, PAC_ENTRY_ID, ARPSE_ID, PAC_DIRECTIVE, PAC_DATE, UPDATED_STATUS,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            SNAPSHOT_ACTION, SNAPSHOT_BY, SNAPSHOT_ON
        )
        SELECT
            SEQ_T_COM_AUDIT_ARPSE_PAC_HIST.NEXTVAL, PAC_ENTRY_ID, ARPSE_ID, PAC_DIRECTIVE, PAC_DATE, UPDATED_STATUS,
            IS_ACTIVE, CREATED_BY, CREATED_ON, UPDATED_BY, UPDATED_ON,
            P_ACTION, NVL(P_SNAPSHOT_BY, CREATED_BY), SYSDATE
        FROM T_COM_AUDIT_ARPSE_PAC
        WHERE PAC_ENTRY_ID = P_PAC_ENTRY_ID;
    END SNAPSHOT_PAC;

    PROCEDURE P_SAVE_OM(
        P_OM_ID IN NUMBER,
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
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_OM_ID NUMBER;
        V_IS_ACTIVE CHAR(1);
        V_EXISTS NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_AUDIT_YEAR_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Audit Year is required.');
            RETURN;
        END IF;

        IF IS_BLANK(P_OM_NO) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'OM No is required.');
            RETURN;
        END IF;

        IF IS_BLANK(P_GIST_OF_OM) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Gist of OM is required.');
            RETURN;
        END IF;

        IF IS_BLANK_CLOB(P_BODY_OF_OM) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Body of OM is required.');
            RETURN;
        END IF;

        IF NVL(P_OM_ID, 0) = 0 THEN
            V_OM_ID := SEQ_T_COM_AUDIT_OM.NEXTVAL;

            INSERT INTO T_COM_AUDIT_OM (
                OM_ID, AUDIT_YEAR_ID, OM_NO, GIST_OF_OM, BODY_OF_OM, MANAGEMENT_RESPONSE,
                IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                V_OM_ID, P_AUDIT_YEAR_ID, TRIM(P_OM_NO), TRIM(P_GIST_OF_OM), P_BODY_OF_OM, P_MANAGEMENT_RESPONSE,
                V_IS_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_OM(V_OM_ID, 'INSERT', P_USER_PPNO);
            COMMIT;
            P_ID := V_OM_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'OM saved successfully.');
        ELSE
            SELECT COUNT(1)
            INTO V_EXISTS
            FROM T_COM_AUDIT_OM
            WHERE OM_ID = P_OM_ID;

            IF V_EXISTS = 0 THEN
                SET_FAILURE(P_STATUS, P_MESSAGE, 'OM record not found.');
                RETURN;
            END IF;

            SNAPSHOT_OM(P_OM_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_OM
            SET AUDIT_YEAR_ID = P_AUDIT_YEAR_ID,
                OM_NO = TRIM(P_OM_NO),
                GIST_OF_OM = TRIM(P_GIST_OF_OM),
                BODY_OF_OM = P_BODY_OF_OM,
                MANAGEMENT_RESPONSE = P_MANAGEMENT_RESPONSE,
                IS_ACTIVE = V_IS_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE OM_ID = P_OM_ID;

            SNAPSHOT_OM(P_OM_ID, 'UPDATE', P_USER_PPNO);
            COMMIT;
            P_ID := P_OM_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'OM updated successfully.');
        END IF;
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'OM No already exists for the selected Audit Year.');
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save OM. ' || SQLERRM);
    END P_SAVE_OM;

    PROCEDURE P_GET_OMS(
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                OM_ID,
                AUDIT_YEAR_ID,
                TO_CHAR(AUDIT_YEAR_ID) AS AUDIT_YEAR_TEXT,
                OM_NO,
                GIST_OF_OM,
                BODY_OF_OM,
                MANAGEMENT_RESPONSE,
                IS_ACTIVE
            FROM T_COM_AUDIT_OM
            WHERE IS_ACTIVE = C_ACTIVE
            ORDER BY AUDIT_YEAR_ID DESC, OM_NO;
    END P_GET_OMS;

    PROCEDURE P_SAVE_PDP(
        P_PDP_ID IN NUMBER,
        P_AUDIT_YEAR_ID IN NUMBER,
        P_PDP_NO IN VARCHAR2,
        P_GIST_OF_PDP IN VARCHAR2,
        P_BODY_OF_PDP IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_DAC_RECOMMENDATIONS IN CLOB,
        P_UPDATE_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_PDP_ID NUMBER;
        V_IS_ACTIVE CHAR(1);
        V_EXISTS NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_AUDIT_YEAR_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Audit Year is required.');
            RETURN;
        END IF;

        IF IS_BLANK(P_PDP_NO) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'PDP No is required.');
            RETURN;
        END IF;

        IF IS_BLANK(P_GIST_OF_PDP) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Gist of PDP is required.');
            RETURN;
        END IF;

        IF IS_BLANK_CLOB(P_BODY_OF_PDP) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Body of PDP is required.');
            RETURN;
        END IF;

        IF NVL(P_PDP_ID, 0) = 0 THEN
            V_PDP_ID := SEQ_T_COM_AUDIT_PDP.NEXTVAL;

            INSERT INTO T_COM_AUDIT_PDP (
                PDP_ID, AUDIT_YEAR_ID, PDP_NO, GIST_OF_PDP, BODY_OF_PDP, MANAGEMENT_RESPONSE,
                DAC_RECOMMENDATIONS, UPDATE_MANAGEMENT_RESPONSE, IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                V_PDP_ID, P_AUDIT_YEAR_ID, TRIM(P_PDP_NO), TRIM(P_GIST_OF_PDP), P_BODY_OF_PDP, P_MANAGEMENT_RESPONSE,
                P_DAC_RECOMMENDATIONS, P_UPDATE_MANAGEMENT_RESPONSE, V_IS_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_PDP(V_PDP_ID, 'INSERT', P_USER_PPNO);
            COMMIT;
            P_ID := V_PDP_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP saved successfully.');
        ELSE
            SELECT COUNT(1)
            INTO V_EXISTS
            FROM T_COM_AUDIT_PDP
            WHERE PDP_ID = P_PDP_ID;

            IF V_EXISTS = 0 THEN
                SET_FAILURE(P_STATUS, P_MESSAGE, 'PDP record not found.');
                RETURN;
            END IF;

            SNAPSHOT_PDP(P_PDP_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_PDP
            SET AUDIT_YEAR_ID = P_AUDIT_YEAR_ID,
                PDP_NO = TRIM(P_PDP_NO),
                GIST_OF_PDP = TRIM(P_GIST_OF_PDP),
                BODY_OF_PDP = P_BODY_OF_PDP,
                MANAGEMENT_RESPONSE = P_MANAGEMENT_RESPONSE,
                DAC_RECOMMENDATIONS = P_DAC_RECOMMENDATIONS,
                UPDATE_MANAGEMENT_RESPONSE = P_UPDATE_MANAGEMENT_RESPONSE,
                IS_ACTIVE = V_IS_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE PDP_ID = P_PDP_ID;

            SNAPSHOT_PDP(P_PDP_ID, 'UPDATE', P_USER_PPNO);
            COMMIT;
            P_ID := P_PDP_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP updated successfully.');
        END IF;
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'PDP No already exists for the selected Audit Year.');
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save PDP. ' || SQLERRM);
    END P_SAVE_PDP;

    PROCEDURE P_GET_PDPS(
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                P.PDP_ID,
                P.AUDIT_YEAR_ID,
                TO_CHAR(P.AUDIT_YEAR_ID) AS AUDIT_YEAR_TEXT,
                P.PDP_NO,
                P.GIST_OF_PDP,
                P.BODY_OF_PDP,
                P.MANAGEMENT_RESPONSE,
                P.DAC_RECOMMENDATIONS,
                P.UPDATE_MANAGEMENT_RESPONSE,
                (
                    SELECT COUNT(1)
                    FROM T_COM_AUDIT_PDP_OM_MAP M
                    WHERE M.PDP_ID = P.PDP_ID
                      AND M.IS_ACTIVE = C_ACTIVE
                ) AS LINKED_OM_COUNT,
                (
                    SELECT LISTAGG(O.OM_NO, ', ') WITHIN GROUP (ORDER BY O.OM_NO)
                    FROM T_COM_AUDIT_PDP_OM_MAP M
                    JOIN T_COM_AUDIT_OM O
                      ON O.OM_ID = M.OM_ID
                    WHERE M.PDP_ID = P.PDP_ID
                      AND M.IS_ACTIVE = C_ACTIVE
                      AND O.IS_ACTIVE = C_ACTIVE
                ) AS LINKED_OM_NUMBERS,
                P.IS_ACTIVE
            FROM T_COM_AUDIT_PDP P
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
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_IS_ACTIVE CHAR(1);
        V_PDP_EXISTS NUMBER;
        V_OM_IDS VARCHAR2(32767);
        V_INVALID_COUNT NUMBER;
        V_MISSING_OM_COUNT NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_OM_IDS := REPLACE(NVL(CLOB_TO_VARCHAR(P_OM_IDS_CSV), ''), ' ', '');

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_PDP_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Saved PDP is required before mapping OMs.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_PDP_EXISTS
        FROM T_COM_AUDIT_PDP
        WHERE PDP_ID = P_PDP_ID
          AND IS_ACTIVE = C_ACTIVE;

        IF V_PDP_EXISTS = 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Active PDP record not found.');
            RETURN;
        END IF;

        IF V_IS_ACTIVE = C_INACTIVE OR V_OM_IDS IS NULL THEN
            FOR REC IN (
                SELECT MAPPING_ID
                FROM T_COM_AUDIT_PDP_OM_MAP
                WHERE PDP_ID = P_PDP_ID
                  AND IS_ACTIVE = C_ACTIVE
            ) LOOP
                SNAPSHOT_PDP_MAP(REC.MAPPING_ID, 'BEFORE_UPDATE', P_USER_PPNO);

                UPDATE T_COM_AUDIT_PDP_OM_MAP
                SET IS_ACTIVE = C_INACTIVE,
                    UPDATED_BY = P_USER_PPNO,
                    UPDATED_ON = SYSDATE
                WHERE MAPPING_ID = REC.MAPPING_ID;

                SNAPSHOT_PDP_MAP(REC.MAPPING_ID, 'UPDATE', P_USER_PPNO);
            END LOOP;

            COMMIT;
            P_ID := P_PDP_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP OM mappings updated successfully.');
            RETURN;
        END IF;

        IF REGEXP_LIKE(V_OM_IDS, '(^,|,,|,$)') THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'OM mapping contains an invalid OM ID.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_INVALID_COUNT
        FROM (
            SELECT TRIM(REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL)) AS TOKEN
            FROM DUAL
            CONNECT BY REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
        )
        WHERE TOKEN IS NULL
           OR NOT REGEXP_LIKE(TOKEN, '^[0-9]+$');

        IF V_INVALID_COUNT > 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'OM mapping contains an invalid OM ID.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_MISSING_OM_COUNT
        FROM (
            SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL))) AS OM_ID
            FROM DUAL
            CONNECT BY REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
        ) SRC
        WHERE NOT EXISTS (
            SELECT 1
            FROM T_COM_AUDIT_OM O
            WHERE O.OM_ID = SRC.OM_ID
              AND O.IS_ACTIVE = C_ACTIVE
        );

        IF V_MISSING_OM_COUNT > 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'One or more selected OMs are invalid or inactive.');
            RETURN;
        END IF;

        FOR REC IN (
            SELECT MAPPING_ID
            FROM T_COM_AUDIT_PDP_OM_MAP
            WHERE PDP_ID = P_PDP_ID
              AND IS_ACTIVE = C_ACTIVE
              AND OM_ID NOT IN (
                  SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL)))
                  FROM DUAL
                  CONNECT BY REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
              )
        ) LOOP
            SNAPSHOT_PDP_MAP(REC.MAPPING_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_PDP_OM_MAP
            SET IS_ACTIVE = C_INACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE MAPPING_ID = REC.MAPPING_ID;

            SNAPSHOT_PDP_MAP(REC.MAPPING_ID, 'UPDATE', P_USER_PPNO);
        END LOOP;

        FOR REC IN (
            SELECT MAPPING_ID
            FROM T_COM_AUDIT_PDP_OM_MAP
            WHERE PDP_ID = P_PDP_ID
              AND IS_ACTIVE = C_INACTIVE
              AND OM_ID IN (
                  SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL)))
                  FROM DUAL
                  CONNECT BY REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
              )
        ) LOOP
            SNAPSHOT_PDP_MAP(REC.MAPPING_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_PDP_OM_MAP
            SET IS_ACTIVE = C_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE MAPPING_ID = REC.MAPPING_ID;

            SNAPSHOT_PDP_MAP(REC.MAPPING_ID, 'UPDATE', P_USER_PPNO);
        END LOOP;

        FOR REC IN (
            SELECT SRC.OM_ID
            FROM (
                SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL))) AS OM_ID
                FROM DUAL
                CONNECT BY REGEXP_SUBSTR(V_OM_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
            ) SRC
            WHERE NOT EXISTS (
                SELECT 1
                FROM T_COM_AUDIT_PDP_OM_MAP M
                WHERE M.PDP_ID = P_PDP_ID
                  AND M.OM_ID = SRC.OM_ID
            )
        ) LOOP
            INSERT INTO T_COM_AUDIT_PDP_OM_MAP (
                MAPPING_ID, PDP_ID, OM_ID, IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                SEQ_T_COM_AUDIT_PDP_OM_MAP.NEXTVAL, P_PDP_ID, REC.OM_ID, C_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_PDP_MAP(SEQ_T_COM_AUDIT_PDP_OM_MAP.CURRVAL, 'INSERT', P_USER_PPNO);
        END LOOP;

        COMMIT;
        P_ID := P_PDP_ID;
        SET_SUCCESS(P_STATUS, P_MESSAGE, 'PDP OM mappings updated successfully.');
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Duplicate PDP OM mapping detected.');
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save PDP OM mapping. ' || SQLERRM);
    END P_SAVE_PDP_OM_MAP;

    PROCEDURE P_GET_PDP_OM_MAP(
        P_PDP_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                M.MAPPING_ID,
                M.PDP_ID,
                M.OM_ID,
                O.OM_NO,
                O.GIST_OF_OM,
                M.IS_ACTIVE
            FROM T_COM_AUDIT_PDP_OM_MAP M
            JOIN T_COM_AUDIT_OM O
              ON O.OM_ID = M.OM_ID
            WHERE M.PDP_ID = P_PDP_ID
              AND M.IS_ACTIVE = C_ACTIVE
              AND O.IS_ACTIVE = C_ACTIVE
            ORDER BY O.OM_NO;
    END P_GET_PDP_OM_MAP;

    PROCEDURE P_SAVE_ARPSE(
        P_ARPSE_ID IN NUMBER,
        P_ARPSE_YEAR_ID IN NUMBER,
        P_PARA_NO IN VARCHAR2,
        P_GIST_OF_PARA IN VARCHAR2,
        P_BODY_OF_PARA IN CLOB,
        P_MANAGEMENT_RESPONSE IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_ARPSE_ID NUMBER;
        V_IS_ACTIVE CHAR(1);
        V_EXISTS NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_ARPSE_YEAR_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'ARPSE Year is required.');
            RETURN;
        END IF;

        IF IS_BLANK(P_PARA_NO) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Para No is required.');
            RETURN;
        END IF;

        IF IS_BLANK(P_GIST_OF_PARA) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Gist of Para is required.');
            RETURN;
        END IF;

        IF IS_BLANK_CLOB(P_BODY_OF_PARA) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Body of Para is required.');
            RETURN;
        END IF;

        IF NVL(P_ARPSE_ID, 0) = 0 THEN
            V_ARPSE_ID := SEQ_T_COM_AUDIT_ARPSE.NEXTVAL;

            INSERT INTO T_COM_AUDIT_ARPSE (
                ARPSE_ID, ARPSE_YEAR_ID, PARA_NO, GIST_OF_PARA, BODY_OF_PARA, MANAGEMENT_RESPONSE,
                IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                V_ARPSE_ID, P_ARPSE_YEAR_ID, TRIM(P_PARA_NO), TRIM(P_GIST_OF_PARA), P_BODY_OF_PARA, P_MANAGEMENT_RESPONSE,
                V_IS_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_ARPSE(V_ARPSE_ID, 'INSERT', P_USER_PPNO);
            COMMIT;
            P_ID := V_ARPSE_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE header saved successfully.');
        ELSE
            SELECT COUNT(1)
            INTO V_EXISTS
            FROM T_COM_AUDIT_ARPSE
            WHERE ARPSE_ID = P_ARPSE_ID;

            IF V_EXISTS = 0 THEN
                SET_FAILURE(P_STATUS, P_MESSAGE, 'ARPSE record not found.');
                RETURN;
            END IF;

            SNAPSHOT_ARPSE(P_ARPSE_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_ARPSE
            SET ARPSE_YEAR_ID = P_ARPSE_YEAR_ID,
                PARA_NO = TRIM(P_PARA_NO),
                GIST_OF_PARA = TRIM(P_GIST_OF_PARA),
                BODY_OF_PARA = P_BODY_OF_PARA,
                MANAGEMENT_RESPONSE = P_MANAGEMENT_RESPONSE,
                IS_ACTIVE = V_IS_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE ARPSE_ID = P_ARPSE_ID;

            SNAPSHOT_ARPSE(P_ARPSE_ID, 'UPDATE', P_USER_PPNO);
            COMMIT;
            P_ID := P_ARPSE_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE header updated successfully.');
        END IF;
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Para No already exists for the selected ARPSE Year.');
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE header. ' || SQLERRM);
    END P_SAVE_ARPSE;

    PROCEDURE P_GET_ARPSE_HEADERS(
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                A.ARPSE_ID,
                A.ARPSE_YEAR_ID,
                TO_CHAR(A.ARPSE_YEAR_ID) AS ARPSE_YEAR_TEXT,
                A.PARA_NO,
                A.GIST_OF_PARA,
                A.BODY_OF_PARA,
                A.MANAGEMENT_RESPONSE,
                (
                    SELECT COUNT(1)
                    FROM T_COM_AUDIT_ARPSE_PDP_MAP M
                    WHERE M.ARPSE_ID = A.ARPSE_ID
                      AND M.IS_ACTIVE = C_ACTIVE
                ) AS LINKED_PDP_COUNT,
                (
                    SELECT LISTAGG(P.PDP_NO, ', ') WITHIN GROUP (ORDER BY P.PDP_NO)
                    FROM T_COM_AUDIT_ARPSE_PDP_MAP M
                    JOIN T_COM_AUDIT_PDP P
                      ON P.PDP_ID = M.PDP_ID
                    WHERE M.ARPSE_ID = A.ARPSE_ID
                      AND M.IS_ACTIVE = C_ACTIVE
                      AND P.IS_ACTIVE = C_ACTIVE
                ) AS LINKED_PDP_NUMBERS,
                A.IS_ACTIVE
            FROM T_COM_AUDIT_ARPSE A
            WHERE A.IS_ACTIVE = C_ACTIVE
            ORDER BY A.ARPSE_YEAR_ID DESC, A.PARA_NO;
    END P_GET_ARPSE_HEADERS;

    PROCEDURE P_SAVE_ARPSE_PDP_MAP(
        P_ARPSE_ID IN NUMBER,
        P_PDP_IDS_CSV IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_IS_ACTIVE CHAR(1);
        V_ARPSE_EXISTS NUMBER;
        V_PDP_IDS VARCHAR2(32767);
        V_INVALID_COUNT NUMBER;
        V_MISSING_PDP_COUNT NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);
        V_PDP_IDS := REPLACE(NVL(CLOB_TO_VARCHAR(P_PDP_IDS_CSV), ''), ' ', '');

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_ARPSE_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Saved ARPSE record is required before mapping PDPs.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_ARPSE_EXISTS
        FROM T_COM_AUDIT_ARPSE
        WHERE ARPSE_ID = P_ARPSE_ID
          AND IS_ACTIVE = C_ACTIVE;

        IF V_ARPSE_EXISTS = 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Active ARPSE record not found.');
            RETURN;
        END IF;

        IF V_IS_ACTIVE = C_INACTIVE OR V_PDP_IDS IS NULL THEN
            FOR REC IN (
                SELECT MAPPING_ID
                FROM T_COM_AUDIT_ARPSE_PDP_MAP
                WHERE ARPSE_ID = P_ARPSE_ID
                  AND IS_ACTIVE = C_ACTIVE
            ) LOOP
                SNAPSHOT_ARPSE_PDP_MAP(REC.MAPPING_ID, 'BEFORE_UPDATE', P_USER_PPNO);

                UPDATE T_COM_AUDIT_ARPSE_PDP_MAP
                SET IS_ACTIVE = C_INACTIVE,
                    UPDATED_BY = P_USER_PPNO,
                    UPDATED_ON = SYSDATE
                WHERE MAPPING_ID = REC.MAPPING_ID;

                SNAPSHOT_ARPSE_PDP_MAP(REC.MAPPING_ID, 'UPDATE', P_USER_PPNO);
            END LOOP;

            COMMIT;
            P_ID := P_ARPSE_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE PDP mappings updated successfully.');
            RETURN;
        END IF;

        IF REGEXP_LIKE(V_PDP_IDS, '(^,|,,|,$)') THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'PDP mapping contains an invalid PDP ID.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_INVALID_COUNT
        FROM (
            SELECT TRIM(REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL)) AS TOKEN
            FROM DUAL
            CONNECT BY REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
        )
        WHERE TOKEN IS NULL
           OR NOT REGEXP_LIKE(TOKEN, '^[0-9]+$');

        IF V_INVALID_COUNT > 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'PDP mapping contains an invalid PDP ID.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_MISSING_PDP_COUNT
        FROM (
            SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL))) AS PDP_ID
            FROM DUAL
            CONNECT BY REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
        ) SRC
        WHERE NOT EXISTS (
            SELECT 1
            FROM T_COM_AUDIT_PDP P
            WHERE P.PDP_ID = SRC.PDP_ID
              AND P.IS_ACTIVE = C_ACTIVE
        );

        IF V_MISSING_PDP_COUNT > 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'One or more selected PDPs are invalid or inactive.');
            RETURN;
        END IF;

        FOR REC IN (
            SELECT MAPPING_ID
            FROM T_COM_AUDIT_ARPSE_PDP_MAP
            WHERE ARPSE_ID = P_ARPSE_ID
              AND IS_ACTIVE = C_ACTIVE
              AND PDP_ID NOT IN (
                  SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL)))
                  FROM DUAL
                  CONNECT BY REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
              )
        ) LOOP
            SNAPSHOT_ARPSE_PDP_MAP(REC.MAPPING_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_ARPSE_PDP_MAP
            SET IS_ACTIVE = C_INACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE MAPPING_ID = REC.MAPPING_ID;

            SNAPSHOT_ARPSE_PDP_MAP(REC.MAPPING_ID, 'UPDATE', P_USER_PPNO);
        END LOOP;

        FOR REC IN (
            SELECT MAPPING_ID
            FROM T_COM_AUDIT_ARPSE_PDP_MAP
            WHERE ARPSE_ID = P_ARPSE_ID
              AND IS_ACTIVE = C_INACTIVE
              AND PDP_ID IN (
                  SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL)))
                  FROM DUAL
                  CONNECT BY REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
              )
        ) LOOP
            SNAPSHOT_ARPSE_PDP_MAP(REC.MAPPING_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_ARPSE_PDP_MAP
            SET IS_ACTIVE = C_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE MAPPING_ID = REC.MAPPING_ID;

            SNAPSHOT_ARPSE_PDP_MAP(REC.MAPPING_ID, 'UPDATE', P_USER_PPNO);
        END LOOP;

        FOR REC IN (
            SELECT SRC.PDP_ID
            FROM (
                SELECT DISTINCT TO_NUMBER(TRIM(REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL))) AS PDP_ID
                FROM DUAL
                CONNECT BY REGEXP_SUBSTR(V_PDP_IDS, '[^,]+', 1, LEVEL) IS NOT NULL
            ) SRC
            WHERE NOT EXISTS (
                SELECT 1
                FROM T_COM_AUDIT_ARPSE_PDP_MAP M
                WHERE M.ARPSE_ID = P_ARPSE_ID
                  AND M.PDP_ID = SRC.PDP_ID
            )
        ) LOOP
            INSERT INTO T_COM_AUDIT_ARPSE_PDP_MAP (
                MAPPING_ID, ARPSE_ID, PDP_ID, IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                SEQ_T_COM_AUDIT_ARPSE_PDP_MAP.NEXTVAL, P_ARPSE_ID, REC.PDP_ID, C_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_ARPSE_PDP_MAP(SEQ_T_COM_AUDIT_ARPSE_PDP_MAP.CURRVAL, 'INSERT', P_USER_PPNO);
        END LOOP;

        COMMIT;
        P_ID := P_ARPSE_ID;
        SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE PDP mappings updated successfully.');
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Duplicate ARPSE PDP mapping detected.');
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE PDP mapping. ' || SQLERRM);
    END P_SAVE_ARPSE_PDP_MAP;

    PROCEDURE P_GET_ARPSE_PDP_MAP(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                M.MAPPING_ID,
                M.ARPSE_ID,
                M.PDP_ID,
                P.PDP_NO,
                P.GIST_OF_PDP,
                M.IS_ACTIVE
            FROM T_COM_AUDIT_ARPSE_PDP_MAP M
            JOIN T_COM_AUDIT_PDP P
              ON P.PDP_ID = M.PDP_ID
            WHERE M.ARPSE_ID = P_ARPSE_ID
              AND M.IS_ACTIVE = C_ACTIVE
              AND P.IS_ACTIVE = C_ACTIVE
            ORDER BY P.PDP_NO;
    END P_GET_ARPSE_PDP_MAP;

    PROCEDURE P_SAVE_ARPSE_DAC(
        P_DAC_ENTRY_ID IN NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_DAC_RECOMMENDATION IN CLOB,
        P_DAC_DATE IN DATE,
        P_UPDATED_STATUS IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_DAC_ENTRY_ID NUMBER;
        V_IS_ACTIVE CHAR(1);
        V_EXISTS NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_ARPSE_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Saved ARPSE header is required.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_EXISTS
        FROM T_COM_AUDIT_ARPSE
        WHERE ARPSE_ID = P_ARPSE_ID
          AND IS_ACTIVE = C_ACTIVE;

        IF V_EXISTS = 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Active ARPSE header not found.');
            RETURN;
        END IF;

        IF IS_BLANK_CLOB(P_DAC_RECOMMENDATION) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'DAC Recommendation is required.');
            RETURN;
        END IF;

        IF NVL(P_DAC_ENTRY_ID, 0) = 0 THEN
            V_DAC_ENTRY_ID := SEQ_T_COM_AUDIT_ARPSE_DAC.NEXTVAL;

            INSERT INTO T_COM_AUDIT_ARPSE_DAC (
                DAC_ENTRY_ID, ARPSE_ID, DAC_RECOMMENDATION, DAC_DATE, UPDATED_STATUS,
                IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                V_DAC_ENTRY_ID, P_ARPSE_ID, P_DAC_RECOMMENDATION, P_DAC_DATE, TRIM(P_UPDATED_STATUS),
                V_IS_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_DAC(V_DAC_ENTRY_ID, 'INSERT', P_USER_PPNO);
            COMMIT;
            P_ID := V_DAC_ENTRY_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE DAC entry saved successfully.');
        ELSE
            SELECT COUNT(1)
            INTO V_EXISTS
            FROM T_COM_AUDIT_ARPSE_DAC
            WHERE DAC_ENTRY_ID = P_DAC_ENTRY_ID;

            IF V_EXISTS = 0 THEN
                SET_FAILURE(P_STATUS, P_MESSAGE, 'ARPSE DAC entry not found.');
                RETURN;
            END IF;

            SNAPSHOT_DAC(P_DAC_ENTRY_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_ARPSE_DAC
            SET ARPSE_ID = P_ARPSE_ID,
                DAC_RECOMMENDATION = P_DAC_RECOMMENDATION,
                DAC_DATE = P_DAC_DATE,
                UPDATED_STATUS = TRIM(P_UPDATED_STATUS),
                IS_ACTIVE = V_IS_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE DAC_ENTRY_ID = P_DAC_ENTRY_ID;

            SNAPSHOT_DAC(P_DAC_ENTRY_ID, 'UPDATE', P_USER_PPNO);
            COMMIT;
            P_ID := P_DAC_ENTRY_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE DAC entry updated successfully.');
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE DAC entry. ' || SQLERRM);
    END P_SAVE_ARPSE_DAC;

    PROCEDURE P_GET_ARPSE_DAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                DAC_ENTRY_ID,
                ARPSE_ID,
                DAC_RECOMMENDATION,
                DAC_DATE,
                UPDATED_STATUS,
                IS_ACTIVE
            FROM T_COM_AUDIT_ARPSE_DAC
            WHERE ARPSE_ID = P_ARPSE_ID
              AND IS_ACTIVE = C_ACTIVE
            ORDER BY NVL(DAC_DATE, CREATED_ON), DAC_ENTRY_ID;
    END P_GET_ARPSE_DAC_ENTRIES;

    PROCEDURE P_SAVE_ARPSE_PAC(
        P_PAC_ENTRY_ID IN NUMBER,
        P_ARPSE_ID IN NUMBER,
        P_PAC_DIRECTIVE IN CLOB,
        P_PAC_DATE IN DATE,
        P_UPDATED_STATUS IN CLOB,
        P_IS_ACTIVE IN VARCHAR2,
        P_USER_PPNO IN NUMBER,
        P_USER_ROLE_ID IN NUMBER,
        P_USER_ENTITY_ID IN NUMBER,
        P_STATUS OUT VARCHAR2,
        P_MESSAGE OUT VARCHAR2,
        P_ID OUT NUMBER
    ) IS
        V_PAC_ENTRY_ID NUMBER;
        V_IS_ACTIVE CHAR(1);
        V_EXISTS NUMBER;
    BEGIN
        P_ID := 0;
        V_IS_ACTIVE := NORMALIZE_FLAG(P_IS_ACTIVE);

        IF NVL(P_USER_PPNO, 0) <= 0 OR NVL(P_USER_ROLE_ID, 0) <= 0 OR NVL(P_USER_ENTITY_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Invalid user context.');
            RETURN;
        END IF;

        IF NVL(P_ARPSE_ID, 0) <= 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Saved ARPSE header is required.');
            RETURN;
        END IF;

        SELECT COUNT(1)
        INTO V_EXISTS
        FROM T_COM_AUDIT_ARPSE
        WHERE ARPSE_ID = P_ARPSE_ID
          AND IS_ACTIVE = C_ACTIVE;

        IF V_EXISTS = 0 THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Active ARPSE header not found.');
            RETURN;
        END IF;

        IF IS_BLANK_CLOB(P_PAC_DIRECTIVE) THEN
            SET_FAILURE(P_STATUS, P_MESSAGE, 'PAC Directive is required.');
            RETURN;
        END IF;

        IF NVL(P_PAC_ENTRY_ID, 0) = 0 THEN
            V_PAC_ENTRY_ID := SEQ_T_COM_AUDIT_ARPSE_PAC.NEXTVAL;

            INSERT INTO T_COM_AUDIT_ARPSE_PAC (
                PAC_ENTRY_ID, ARPSE_ID, PAC_DIRECTIVE, PAC_DATE, UPDATED_STATUS,
                IS_ACTIVE, CREATED_BY, CREATED_ON
            )
            VALUES (
                V_PAC_ENTRY_ID, P_ARPSE_ID, P_PAC_DIRECTIVE, P_PAC_DATE, TRIM(P_UPDATED_STATUS),
                V_IS_ACTIVE, P_USER_PPNO, SYSDATE
            );

            SNAPSHOT_PAC(V_PAC_ENTRY_ID, 'INSERT', P_USER_PPNO);
            COMMIT;
            P_ID := V_PAC_ENTRY_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE PAC entry saved successfully.');
        ELSE
            SELECT COUNT(1)
            INTO V_EXISTS
            FROM T_COM_AUDIT_ARPSE_PAC
            WHERE PAC_ENTRY_ID = P_PAC_ENTRY_ID;

            IF V_EXISTS = 0 THEN
                SET_FAILURE(P_STATUS, P_MESSAGE, 'ARPSE PAC entry not found.');
                RETURN;
            END IF;

            SNAPSHOT_PAC(P_PAC_ENTRY_ID, 'BEFORE_UPDATE', P_USER_PPNO);

            UPDATE T_COM_AUDIT_ARPSE_PAC
            SET ARPSE_ID = P_ARPSE_ID,
                PAC_DIRECTIVE = P_PAC_DIRECTIVE,
                PAC_DATE = P_PAC_DATE,
                UPDATED_STATUS = TRIM(P_UPDATED_STATUS),
                IS_ACTIVE = V_IS_ACTIVE,
                UPDATED_BY = P_USER_PPNO,
                UPDATED_ON = SYSDATE
            WHERE PAC_ENTRY_ID = P_PAC_ENTRY_ID;

            SNAPSHOT_PAC(P_PAC_ENTRY_ID, 'UPDATE', P_USER_PPNO);
            COMMIT;
            P_ID := P_PAC_ENTRY_ID;
            SET_SUCCESS(P_STATUS, P_MESSAGE, 'ARPSE PAC entry updated successfully.');
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            SET_FAILURE(P_STATUS, P_MESSAGE, 'Unable to save ARPSE PAC entry. ' || SQLERRM);
    END P_SAVE_ARPSE_PAC;

    PROCEDURE P_GET_ARPSE_PAC_ENTRIES(
        P_ARPSE_ID IN NUMBER,
        IO_CURSOR OUT T_CURSOR
    ) IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                PAC_ENTRY_ID,
                ARPSE_ID,
                PAC_DIRECTIVE,
                PAC_DATE,
                UPDATED_STATUS,
                IS_ACTIVE
            FROM T_COM_AUDIT_ARPSE_PAC
            WHERE ARPSE_ID = P_ARPSE_ID
              AND IS_ACTIVE = C_ACTIVE
            ORDER BY NVL(PAC_DATE, CREATED_ON), PAC_ENTRY_ID;
    END P_GET_ARPSE_PAC_ENTRIES;
END PKG_COMMERCIAL_AUDIT;
