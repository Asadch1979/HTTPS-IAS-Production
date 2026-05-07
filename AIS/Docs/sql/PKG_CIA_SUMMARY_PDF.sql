CREATE TABLE T_CIA_SUMMARY_PDF_STORE
(
    PDF_ID                NUMBER PRIMARY KEY,
    BATCH_ID              VARCHAR2(100) NOT NULL,
    AUDIT_DEPARTMENT_ID   NUMBER,
    AUDIT_DEPARTMENT_NAME VARCHAR2(500),
    ENTITY_ID             NUMBER,
    ENTITY_NAME           VARCHAR2(500),
    RISK                  VARCHAR2(50),
    PART_NO               NUMBER,
    FILE_NAME             VARCHAR2(1000),
    FILE_MIME_TYPE        VARCHAR2(100) DEFAULT 'application/pdf',
    FILE_SIZE             NUMBER,
    PDF_BLOB              BLOB,
    GENERATED_BY          VARCHAR2(100),
    GENERATED_ON          DATE DEFAULT SYSDATE,
    EXPIRES_ON            DATE,
    STATUS                VARCHAR2(50) DEFAULT 'GENERATED',
    ERROR_MESSAGE         VARCHAR2(4000)
);

CREATE SEQUENCE SEQ_CIA_SUMMARY_PDF_STORE
START WITH 1
INCREMENT BY 1
NOCACHE;


CREATE OR REPLACE PACKAGE PKG_CIA_SUMMARY_PDF AS

    TYPE T_CURSOR IS REF CURSOR;

    PROCEDURE P_SAVE_CIA_SUMMARY_PDF
    (
        P_BATCH_ID              IN  VARCHAR2,
        P_AUDIT_DEPARTMENT_ID   IN  NUMBER,
        P_AUDIT_DEPARTMENT_NAME IN  VARCHAR2,
        P_ENTITY_ID             IN  NUMBER,
        P_ENTITY_NAME           IN  VARCHAR2,
        P_RISK                  IN  VARCHAR2,
        P_PART_NO               IN  NUMBER,
        P_FILE_NAME             IN  VARCHAR2,
        P_FILE_MIME_TYPE        IN  VARCHAR2,
        P_FILE_SIZE             IN  NUMBER,
        P_PDF_BLOB              IN  BLOB,
        P_GENERATED_BY          IN  VARCHAR2,
        P_EXPIRES_ON            IN  DATE,
        P_STATUS                IN  VARCHAR2,
        P_ERROR_MESSAGE         IN  VARCHAR2,
        O_PDF_ID                OUT NUMBER,
        O_STATUS                OUT VARCHAR2,
        O_MESSAGE               OUT VARCHAR2
    );

    PROCEDURE P_GET_CIA_SUMMARY_PDF_LIST
    (
        P_BATCH_ID              IN  VARCHAR2 DEFAULT NULL,
        P_GENERATED_BY          IN  VARCHAR2 DEFAULT NULL,
        P_AUDIT_DEPARTMENT_ID   IN  NUMBER   DEFAULT NULL,
        P_RISK                  IN  VARCHAR2 DEFAULT NULL,
        P_FROM_DATE             IN  DATE     DEFAULT NULL,
        P_TO_DATE               IN  DATE     DEFAULT NULL,
        P_LATEST_BATCH_ONLY     IN  VARCHAR2 DEFAULT 'N',
        O_CURSOR                OUT T_CURSOR
    );

    PROCEDURE P_DOWNLOAD_CIA_SUMMARY_PDF
    (
        P_PDF_ID          IN  NUMBER,
        O_FILE_NAME       OUT VARCHAR2,
        O_FILE_MIME_TYPE  OUT VARCHAR2,
        O_FILE_SIZE       OUT NUMBER,
        O_PDF_BLOB        OUT BLOB,
        O_STATUS          OUT VARCHAR2,
        O_MESSAGE         OUT VARCHAR2
    );

    PROCEDURE P_DELETE_CIA_SUMMARY_PDF
    (
        P_PDF_ID      IN  NUMBER,
        P_DELETED_BY  IN  VARCHAR2,
        O_STATUS      OUT VARCHAR2,
        O_MESSAGE     OUT VARCHAR2
    );

    PROCEDURE P_DELETE_CIA_SUMMARY_BATCH
    (
        P_BATCH_ID    IN  VARCHAR2,
        P_DELETED_BY  IN  VARCHAR2,
        O_ROWS        OUT NUMBER,
        O_STATUS      OUT VARCHAR2,
        O_MESSAGE     OUT VARCHAR2
    );

    PROCEDURE P_CLEANUP_OLD_CIA_SUMMARY_PDFS
    (
        O_ROWS     OUT NUMBER,
        O_STATUS   OUT VARCHAR2,
        O_MESSAGE  OUT VARCHAR2
    );

END PKG_CIA_SUMMARY_PDF;
/
CREATE OR REPLACE PACKAGE BODY PKG_CIA_SUMMARY_PDF AS

    ----------------------------------------------------------------------
    -- Save generated CIA Summary PDF BLOB with metadata
    ----------------------------------------------------------------------
    PROCEDURE P_SAVE_CIA_SUMMARY_PDF
    (
        P_BATCH_ID              IN  VARCHAR2,
        P_AUDIT_DEPARTMENT_ID   IN  NUMBER,
        P_AUDIT_DEPARTMENT_NAME IN  VARCHAR2,
        P_ENTITY_ID             IN  NUMBER,
        P_ENTITY_NAME           IN  VARCHAR2,
        P_RISK                  IN  VARCHAR2,
        P_PART_NO               IN  NUMBER,
        P_FILE_NAME             IN  VARCHAR2,
        P_FILE_MIME_TYPE        IN  VARCHAR2,
        P_FILE_SIZE             IN  NUMBER,
        P_PDF_BLOB              IN  BLOB,
        P_GENERATED_BY          IN  VARCHAR2,
        P_EXPIRES_ON            IN  DATE,
        P_STATUS                IN  VARCHAR2,
        P_ERROR_MESSAGE         IN  VARCHAR2,
        O_PDF_ID                OUT NUMBER,
        O_STATUS                OUT VARCHAR2,
        O_MESSAGE               OUT VARCHAR2
    )
    IS
        V_PDF_ID NUMBER;
    BEGIN
        IF P_BATCH_ID IS NULL THEN
            O_STATUS  := 'ERROR';
            O_MESSAGE := 'Batch ID is required.';
            O_PDF_ID  := NULL;
            RETURN;
        END IF;

        IF P_FILE_NAME IS NULL THEN
            O_STATUS  := 'ERROR';
            O_MESSAGE := 'File name is required.';
            O_PDF_ID  := NULL;
            RETURN;
        END IF;

        V_PDF_ID := SEQ_CIA_SUMMARY_PDF_STORE.NEXTVAL;

        INSERT INTO T_CIA_SUMMARY_PDF_STORE
        (
            PDF_ID,
            BATCH_ID,
            AUDIT_DEPARTMENT_ID,
            AUDIT_DEPARTMENT_NAME,
            ENTITY_ID,
            ENTITY_NAME,
            RISK,
            PART_NO,
            FILE_NAME,
            FILE_MIME_TYPE,
            FILE_SIZE,
            PDF_BLOB,
            GENERATED_BY,
            GENERATED_ON,
            EXPIRES_ON,
            STATUS,
            ERROR_MESSAGE
        )
        VALUES
        (
            V_PDF_ID,
            P_BATCH_ID,
            P_AUDIT_DEPARTMENT_ID,
            P_AUDIT_DEPARTMENT_NAME,
            P_ENTITY_ID,
            P_ENTITY_NAME,
            P_RISK,
            NVL(P_PART_NO, 1),
            P_FILE_NAME,
            NVL(P_FILE_MIME_TYPE, 'application/pdf'),
            P_FILE_SIZE,
            P_PDF_BLOB,
            P_GENERATED_BY,
            SYSDATE,
            NVL(P_EXPIRES_ON, SYSDATE + 30),
            NVL(P_STATUS, 'GENERATED'),
            P_ERROR_MESSAGE
        );

        O_PDF_ID  := V_PDF_ID;
        O_STATUS  := 'SUCCESS';
        O_MESSAGE := 'CIA summary PDF saved successfully.';

        COMMIT;

    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            O_PDF_ID  := NULL;
            O_STATUS  := 'ERROR';
            O_MESSAGE := SQLERRM;
    END P_SAVE_CIA_SUMMARY_PDF;


    ----------------------------------------------------------------------
    -- Return previously generated PDFs
    -- Supports:
    -- Batch ID
    -- Generated By
    -- Department
    -- Risk
    -- Date range
    -- Latest batch only
    ----------------------------------------------------------------------
    PROCEDURE P_GET_CIA_SUMMARY_PDF_LIST
    (
        P_BATCH_ID              IN  VARCHAR2 DEFAULT NULL,
        P_GENERATED_BY          IN  VARCHAR2 DEFAULT NULL,
        P_AUDIT_DEPARTMENT_ID   IN  NUMBER   DEFAULT NULL,
        P_RISK                  IN  VARCHAR2 DEFAULT NULL,
        P_FROM_DATE             IN  DATE     DEFAULT NULL,
        P_TO_DATE               IN  DATE     DEFAULT NULL,
        P_LATEST_BATCH_ONLY     IN  VARCHAR2 DEFAULT 'N',
        O_CURSOR                OUT T_CURSOR
    )
    IS
        V_LATEST_BATCH_ID VARCHAR2(100);
    BEGIN
        IF NVL(UPPER(P_LATEST_BATCH_ONLY), 'N') = 'Y' THEN
            SELECT MAX(BATCH_ID)
              INTO V_LATEST_BATCH_ID
              FROM T_CIA_SUMMARY_PDF_STORE
             WHERE STATUS IN ('GENERATED', 'FAILED')
               AND (P_GENERATED_BY IS NULL OR GENERATED_BY = P_GENERATED_BY);
        END IF;

        OPEN O_CURSOR FOR
            SELECT
                   PDF_ID,
                   BATCH_ID,
                   AUDIT_DEPARTMENT_ID,
                   AUDIT_DEPARTMENT_NAME,
                   ENTITY_ID,
                   ENTITY_NAME,
                   RISK,
                   PART_NO,
                   FILE_NAME,
                   FILE_MIME_TYPE,
                   FILE_SIZE,
                   GENERATED_BY,
                   GENERATED_ON,
                   EXPIRES_ON,
                   STATUS,
                   ERROR_MESSAGE
              FROM T_CIA_SUMMARY_PDF_STORE
             WHERE STATUS IN ('GENERATED', 'FAILED')
               AND (P_BATCH_ID IS NULL OR BATCH_ID = P_BATCH_ID)
               AND (
                    NVL(UPPER(P_LATEST_BATCH_ONLY), 'N') <> 'Y'
                    OR BATCH_ID = V_LATEST_BATCH_ID
                   )
               AND (P_GENERATED_BY IS NULL OR GENERATED_BY = P_GENERATED_BY)
               AND (
                    P_AUDIT_DEPARTMENT_ID IS NULL
                    OR P_AUDIT_DEPARTMENT_ID = 0
                    OR AUDIT_DEPARTMENT_ID = P_AUDIT_DEPARTMENT_ID
                   )
               AND (
                    P_RISK IS NULL
                    OR UPPER(P_RISK) = 'ALL'
                    OR UPPER(RISK) = UPPER(P_RISK)
                   )
               AND (
                    P_FROM_DATE IS NULL
                    OR TRUNC(GENERATED_ON) >= TRUNC(P_FROM_DATE)
                   )
               AND (
                    P_TO_DATE IS NULL
                    OR TRUNC(GENERATED_ON) <= TRUNC(P_TO_DATE)
                   )
             ORDER BY
                   GENERATED_ON DESC,
                   BATCH_ID DESC,
                   AUDIT_DEPARTMENT_NAME,
                   ENTITY_NAME,
                   CASE UPPER(RISK)
                        WHEN 'HIGH' THEN 1
                        WHEN 'MEDIUM' THEN 2
                        WHEN 'LOW' THEN 3
                        ELSE 4
                   END,
                   PART_NO,
                   PDF_ID;

    EXCEPTION
        WHEN OTHERS THEN
            OPEN O_CURSOR FOR
                SELECT
                       CAST(NULL AS NUMBER)          AS PDF_ID,
                       CAST(NULL AS VARCHAR2(100))   AS BATCH_ID,
                       CAST(NULL AS NUMBER)          AS AUDIT_DEPARTMENT_ID,
                       CAST(NULL AS VARCHAR2(500))   AS AUDIT_DEPARTMENT_NAME,
                       CAST(NULL AS NUMBER)          AS ENTITY_ID,
                       CAST(NULL AS VARCHAR2(500))   AS ENTITY_NAME,
                       CAST(NULL AS VARCHAR2(50))    AS RISK,
                       CAST(NULL AS NUMBER)          AS PART_NO,
                       CAST(NULL AS VARCHAR2(1000))  AS FILE_NAME,
                       CAST(NULL AS VARCHAR2(100))   AS FILE_MIME_TYPE,
                       CAST(NULL AS NUMBER)          AS FILE_SIZE,
                       CAST(NULL AS VARCHAR2(100))   AS GENERATED_BY,
                       CAST(NULL AS DATE)            AS GENERATED_ON,
                       CAST(NULL AS DATE)            AS EXPIRES_ON,
                       CAST('ERROR' AS VARCHAR2(50)) AS STATUS,
                       CAST(SQLERRM AS VARCHAR2(4000)) AS ERROR_MESSAGE
                  FROM DUAL;
    END P_GET_CIA_SUMMARY_PDF_LIST;


    ----------------------------------------------------------------------
    -- Download PDF by PDF_ID
    -- Returns BLOB, filename, MIME type
    ----------------------------------------------------------------------
    PROCEDURE P_DOWNLOAD_CIA_SUMMARY_PDF
    (
        P_PDF_ID          IN  NUMBER,
        O_FILE_NAME       OUT VARCHAR2,
        O_FILE_MIME_TYPE  OUT VARCHAR2,
        O_FILE_SIZE       OUT NUMBER,
        O_PDF_BLOB        OUT BLOB,
        O_STATUS          OUT VARCHAR2,
        O_MESSAGE         OUT VARCHAR2
    )
    IS
    BEGIN
        SELECT
               FILE_NAME,
               FILE_MIME_TYPE,
               FILE_SIZE,
               PDF_BLOB
          INTO
               O_FILE_NAME,
               O_FILE_MIME_TYPE,
               O_FILE_SIZE,
               O_PDF_BLOB
          FROM T_CIA_SUMMARY_PDF_STORE
         WHERE PDF_ID = P_PDF_ID
           AND STATUS = 'GENERATED';

        O_STATUS  := 'SUCCESS';
        O_MESSAGE := 'PDF fetched successfully.';

    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            O_FILE_NAME      := NULL;
            O_FILE_MIME_TYPE := NULL;
            O_FILE_SIZE      := NULL;
            O_PDF_BLOB       := NULL;
            O_STATUS         := 'ERROR';
            O_MESSAGE        := 'PDF not found or not available for download.';

        WHEN OTHERS THEN
            O_FILE_NAME      := NULL;
            O_FILE_MIME_TYPE := NULL;
            O_FILE_SIZE      := NULL;
            O_PDF_BLOB       := NULL;
            O_STATUS         := 'ERROR';
            O_MESSAGE        := SQLERRM;
    END P_DOWNLOAD_CIA_SUMMARY_PDF;


    ----------------------------------------------------------------------
    -- Delete one selected PDF by PDF_ID
    -- Hard delete version
    ----------------------------------------------------------------------
    PROCEDURE P_DELETE_CIA_SUMMARY_PDF
    (
        P_PDF_ID      IN  NUMBER,
        P_DELETED_BY  IN  VARCHAR2,
        O_STATUS      OUT VARCHAR2,
        O_MESSAGE     OUT VARCHAR2
    )
    IS
        V_COUNT NUMBER;
    BEGIN
        SELECT COUNT(1)
          INTO V_COUNT
          FROM T_CIA_SUMMARY_PDF_STORE
         WHERE PDF_ID = P_PDF_ID;

        IF V_COUNT = 0 THEN
            O_STATUS  := 'ERROR';
            O_MESSAGE := 'PDF not found.';
            RETURN;
        END IF;

        DELETE FROM T_CIA_SUMMARY_PDF_STORE
         WHERE PDF_ID = P_PDF_ID;

        O_STATUS  := 'SUCCESS';
        O_MESSAGE := 'PDF deleted successfully.';

        COMMIT;

    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            O_STATUS  := 'ERROR';
            O_MESSAGE := SQLERRM;
    END P_DELETE_CIA_SUMMARY_PDF;


    ----------------------------------------------------------------------
    -- Delete all PDFs under one BATCH_ID
    -- Hard delete version
    ----------------------------------------------------------------------
    PROCEDURE P_DELETE_CIA_SUMMARY_BATCH
    (
        P_BATCH_ID    IN  VARCHAR2,
        P_DELETED_BY  IN  VARCHAR2,
        O_ROWS        OUT NUMBER,
        O_STATUS      OUT VARCHAR2,
        O_MESSAGE     OUT VARCHAR2
    )
    IS
    BEGIN
        IF P_BATCH_ID IS NULL THEN
            O_ROWS    := 0;
            O_STATUS  := 'ERROR';
            O_MESSAGE := 'Batch ID is required.';
            RETURN;
        END IF;

        DELETE FROM T_CIA_SUMMARY_PDF_STORE
         WHERE BATCH_ID = P_BATCH_ID;

        O_ROWS := SQL%ROWCOUNT;

        O_STATUS  := 'SUCCESS';
        O_MESSAGE := O_ROWS || ' PDF record(s) deleted successfully.';

        COMMIT;

    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            O_ROWS    := 0;
            O_STATUS  := 'ERROR';
            O_MESSAGE := SQLERRM;
    END P_DELETE_CIA_SUMMARY_BATCH;


    ----------------------------------------------------------------------
    -- Cleanup expired PDFs
    -- Deletes records where EXPIRES_ON < SYSDATE
    ----------------------------------------------------------------------
    PROCEDURE P_CLEANUP_OLD_CIA_SUMMARY_PDFS
    (
        O_ROWS     OUT NUMBER,
        O_STATUS   OUT VARCHAR2,
        O_MESSAGE  OUT VARCHAR2
    )
    IS
    BEGIN
        DELETE FROM T_CIA_SUMMARY_PDF_STORE
         WHERE EXPIRES_ON IS NOT NULL
           AND EXPIRES_ON < SYSDATE;

        O_ROWS := SQL%ROWCOUNT;

        O_STATUS  := 'SUCCESS';
        O_MESSAGE := O_ROWS || ' expired PDF record(s) cleaned up successfully.';

        COMMIT;

    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            O_ROWS    := 0;
            O_STATUS  := 'ERROR';
            O_MESSAGE := SQLERRM;
    END P_CLEANUP_OLD_CIA_SUMMARY_PDFS;

END PKG_CIA_SUMMARY_PDF;
/
ALTER TABLE T_CIA_SUMMARY_PDF_STORE ADD
(
    DELETED_BY VARCHAR2(100),
    DELETED_ON DATE
);