CREATE OR REPLACE PACKAGE PKG_IID_EXC AS
    TYPE T_CURSOR IS REF CURSOR;

    PROCEDURE P_GET_IID_EXCEPTION_REPORTS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_PNO          IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_EXCEPTION_REPORT_COLUMNS
    (
        P_REPORT_ID    IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_EXCEPTION_REPORT_DATA
    (
        P_REPORT_ID    IN  NUMBER,
        P_INQUIRY_ID   IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_ACCOUNT_TRANSACTIONS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_ACCOUNT_NO   IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_ACCOUNT_DOCS
    (
        P_ACCOUNT_NO   IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_LOAN_EXCEPTIONS
    (
        P_INDICATOR    IN  VARCHAR2,
        P_STATUS_ID    IN  NUMBER,
        P_INQUIRY_ID   IN  NUMBER,
        P_PNO          IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_LOAN_DOCUMENTS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_LOAN_DISB_ID IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_LOAN_DOCUMENT_IMAGE
    (
        P_IMAGE_ID     IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );

    PROCEDURE P_GET_IID_LOAN_TRANSACTIONS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_LOAN_DISB_ID IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    );
END PKG_IID_EXC;

CREATE OR REPLACE PACKAGE BODY PKG_IID_EXC AS

    PROCEDURE P_GET_IID_EXCEPTION_REPORTS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_PNO          IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                R.REPORT_ID               AS R_ID,
                R.REPORT_TITLE            AS REPORT_TITLE,
                R.DESCRIPTION             AS DISCRIPTION,
                R.REPORT_TYPE             AS REPORT_TYPE,
                R.DRILL_PROC_CODE         AS DRILL_PROC_CODE,
                CASE
                    WHEN R.REPORT_TYPE = 'ACCOUNT' THEN
                        (SELECT MAX(A.REPORTING_PERIOD)
                           FROM T_AU_IID_EXC_ACCOUNT A
                          WHERE A.INQUIRY_ID = P_INQUIRY_ID
                            AND A.REPORT_ID  = R.REPORT_ID
                            AND A.IS_ACTIVE  = 'Y')
                    WHEN R.REPORT_TYPE = 'LOAN' THEN
                        (SELECT MAX(L.REPORTING_PERIOD)
                           FROM T_AU_IID_EXC_LOAN L
                          WHERE L.INQUIRY_ID = P_INQUIRY_ID
                            AND L.REPORT_ID  = R.REPORT_ID
                            AND L.IS_ACTIVE  = 'Y')
                    ELSE NULL
                END AS REPORTING_PERIOD
            FROM T_AU_IID_EXC_REPORT_MST R
            WHERE R.IS_ACTIVE = 'Y'
            ORDER BY R.DISPLAY_ORDER, R.REPORT_ID;
    END P_GET_IID_EXCEPTION_REPORTS;

    PROCEDURE P_GET_IID_EXCEPTION_REPORT_COLUMNS
    (
        P_REPORT_ID    IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                C.COLUMN_ID          AS FORMAT_ID,
                C.REPORT_ID          AS R_ID,
                C.COLUMN_NAME        AS COLUMN_NAME,
                C.COLUMN_HEADER      AS COLUMN_HEADER,
                C.COLUMN_ORDER       AS COLUMN_ORDER,
                C.DATA_TYPE          AS DATA_TYPE,
                C.IS_VISIBLE         AS IS_ACTIVE
            FROM T_AU_IID_EXC_REPORT_COL_MST C
            WHERE C.REPORT_ID   = P_REPORT_ID
              AND C.IS_VISIBLE  = 'Y'
            ORDER BY C.COLUMN_ORDER, C.COLUMN_ID;
    END P_GET_IID_EXCEPTION_REPORT_COLUMNS;

    PROCEDURE P_GET_IID_EXCEPTION_REPORT_DATA
    (
        P_REPORT_ID    IN  NUMBER,
        P_INQUIRY_ID   IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
        V_REPORT_TYPE T_AU_IID_EXC_REPORT_MST.REPORT_TYPE%TYPE;
        V_REPORT_CODE T_AU_IID_EXC_REPORT_MST.REPORT_CODE%TYPE;
    BEGIN
        SELECT REPORT_TYPE, REPORT_CODE
          INTO V_REPORT_TYPE, V_REPORT_CODE
          FROM T_AU_IID_EXC_REPORT_MST
         WHERE REPORT_ID = P_REPORT_ID
           AND IS_ACTIVE = 'Y';

        IF V_REPORT_TYPE = 'ACCOUNT' THEN
            OPEN IO_CURSOR FOR
                SELECT
                    A.ACCOUNT_NO,
                    A.ACCOUNT_TITLE,
                    A.CUSTOMER_NAME,
                    A.CNIC,
                    A.BRANCH_CODE,
                    A.BRANCH_NAME,
                    A.EXCEPTION_DETAIL,
                    A.REPORTING_PERIOD,
                    A.SOURCE_REF_NO
                FROM T_AU_IID_EXC_ACCOUNT A
                WHERE A.INQUIRY_ID = P_INQUIRY_ID
                  AND A.REPORT_ID  = P_REPORT_ID
                  AND A.IS_ACTIVE  = 'Y'
                ORDER BY A.ACCOUNT_NO;

        ELSIF V_REPORT_TYPE = 'LOAN' THEN
            OPEN IO_CURSOR FOR
                SELECT
                    L.LOAN_DISB_ID,
                    L.TYPE,
                    L.SCHEME,
                    L.L_PURPOSE,
                    L.LC_NO,
                    L.CNIC,
                    L.CUSTOMER_NAME,
                    TO_CHAR(L.APP_DATE,  'DD-MON-YYYY') AS APP_DATE_DISP,
                    TO_CHAR(L.DISB_DATE, 'DD-MON-YYYY') AS DISB_DATE_DISP,
                    L.DEV_AMOUNT,
                    L.OUTSTANDING,
                    L.EXCEPTION_DETAIL,
                    L.REPORTING_PERIOD
                FROM T_AU_IID_EXC_LOAN L
                WHERE L.INQUIRY_ID = P_INQUIRY_ID
                  AND L.REPORT_ID  = P_REPORT_ID
                  AND L.IS_ACTIVE  = 'Y'
                ORDER BY L.LOAN_DISB_ID;

        ELSE
            OPEN IO_CURSOR FOR
                SELECT NULL AS MESSAGE
                FROM DUAL
                WHERE 1 = 2;
        END IF;
    END P_GET_IID_EXCEPTION_REPORT_DATA;

    PROCEDURE P_GET_IID_ACCOUNT_TRANSACTIONS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_ACCOUNT_NO   IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                T.TRANSACTION_MASTER_CODE  AS TRANSACTIONMASTERCODE,
                T.DESCRIPTION              AS DESCRIPTION,
                T.REMARKS                  AS REMARKS,
                TO_CHAR(T.TRANSACTION_DATE,   'DD-MON-YYYY') AS TRANSACTIONDATE_DISP,
                TO_CHAR(T.AUTHORIZATION_DATE,'DD-MON-YYYY') AS AUTHORIZATIONDATE_DISP,
                T.TRANSACTION_DATE         AS TRANSACTIONDATE,
                T.AUTHORIZATION_DATE       AS AUTHORIZATIONDATE,
                T.DR_AMOUNT                AS DRAMOUNT,
                T.CR_AMOUNT                AS CRAMOUNT,
                T.TO_ACCOUNT_ID            AS TOACCOUNTID,
                T.TO_ACCOUNT_TITLE         AS TOACCOUNTTITLE,
                T.TO_ACCOUNT_NO            AS TOACCOUNTNO,
                T.TO_ACC_BRANCH_ID         AS TO_ACC_BRANCHID,
                T.INSTRUMENT_NO            AS INSTRUMENTNO
            FROM T_AU_IID_EXC_ACCOUNT_TXN T
            WHERE T.INQUIRY_ID = P_INQUIRY_ID
              AND T.ACCOUNT_NO = P_ACCOUNT_NO
              AND T.IS_ACTIVE  = 'Y'
            ORDER BY T.TRANSACTION_DATE, T.ACCOUNT_TXN_ID;
    END P_GET_IID_ACCOUNT_TRANSACTIONS;

    PROCEDURE P_GET_IID_ACCOUNT_DOCS
    (
        P_ACCOUNT_NO   IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                D.OLD_ACCOUNT_NO   AS OLDACCOUNTNO,
                D.PAGE_NO          AS PAGENO,
                D.DOC_NAME         AS NAME,
                D.DOC_IMAGE        AS DOC_IMAGE,
                D.DOC_REMARKS      AS DOC_REMARKS
            FROM T_AU_IID_EXC_ACCOUNT_DOC D
            WHERE D.ACCOUNT_NO = P_ACCOUNT_NO
              AND D.IS_ACTIVE  = 'Y'
            ORDER BY D.ACCOUNT_DOC_ID;
    END P_GET_IID_ACCOUNT_DOCS;

    PROCEDURE P_GET_IID_LOAN_EXCEPTIONS
    (
        P_INDICATOR    IN  VARCHAR2,
        P_STATUS_ID    IN  NUMBER,
        P_INQUIRY_ID   IN  NUMBER,
        P_PNO          IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                L.LOAN_DISB_ID                         AS LOAN_DISB_ID,
                L.TYPE                                 AS TYPE,
                L.SCHEME                               AS SCHEME,
                L.L_PURPOSE                            AS L_PURPOSE,
                L.LC_NO                                AS LC_NO,
                L.CNIC                                 AS CNIC,
                L.CUSTOMER_NAME                        AS CUSTOMERNAME,
                TO_CHAR(L.APP_DATE,  'DD-MON-YYYY')   AS APP_DATE_DISP,
                TO_CHAR(L.DISB_DATE, 'DD-MON-YYYY')   AS DISB_DATE_DISP,
                L.DEV_AMOUNT                           AS DEV_AMOUNT,
                L.OUTSTANDING                          AS OUTSTANDING
            FROM T_AU_IID_EXC_LOAN L
            WHERE L.INQUIRY_ID = P_INQUIRY_ID
              AND L.IS_ACTIVE  = 'Y'
              AND (P_INDICATOR IS NULL OR L.INDICATOR = P_INDICATOR)
              AND (P_STATUS_ID IS NULL OR L.STATUS_ID = P_STATUS_ID)
            ORDER BY L.LOAN_DISB_ID;
    END P_GET_IID_LOAN_EXCEPTIONS;

    PROCEDURE P_GET_IID_LOAN_DOCUMENTS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_LOAN_DISB_ID IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                D.IMAGE_ID         AS IMAGEID,
                D.BRANCH_CODE      AS BRANCHCODE,
                D.LOAN_APP_ID      AS LOAN_APP_ID,
                D.CNIC             AS CNIC,
                D.CUSTOMER_NAME    AS CUSTOMERNAME,
                D.LOAN_CASE_NO     AS LOAN_CASE_NO,
                D.LOAN_DISB_ID     AS LOAN_DISB_ID,
                D.DOC_NAME         AS DOCNAME
            FROM T_AU_IID_EXC_LOAN_DOC D
            WHERE D.INQUIRY_ID   = P_INQUIRY_ID
              AND D.LOAN_DISB_ID = P_LOAN_DISB_ID
              AND D.IS_ACTIVE    = 'Y'
            ORDER BY D.LOAN_DOC_ID;
    END P_GET_IID_LOAN_DOCUMENTS;

    PROCEDURE P_GET_IID_LOAN_DOCUMENT_IMAGE
    (
        P_IMAGE_ID     IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                I.IMAGE_DATA AS IMAGEDATA
            FROM T_AU_IID_EXC_LOAN_DOC_IMG I
            WHERE I.IMAGE_ID = P_IMAGE_ID;
    END P_GET_IID_LOAN_DOCUMENT_IMAGE;

    PROCEDURE P_GET_IID_LOAN_TRANSACTIONS
    (
        P_INQUIRY_ID   IN  NUMBER,
        P_LOAN_DISB_ID IN  VARCHAR2,
        P_PNO          IN  NUMBER,
        P_ENT_ID       IN  NUMBER,
        P_ROLE_ID      IN  NUMBER,
        IO_CURSOR      OUT T_CURSOR
    )
    AS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT
                T.DESCRIPTION                           AS DESCRIPTION,
                T.MANUAL_VOUCHER_NO                     AS MANUALVOUCHERNO,
                T.TRANSACTION_DATE                      AS TRANSACTIONDATE,
                TO_CHAR(T.TRANSACTION_DATE,'DD-MON-YYYY') AS TRANSACTIONDATE_DISP,
                T.DR_AMOUNT                             AS DRAMOUNT,
                T.CR_AMOUNT                             AS CRAMOUNT,
                T.LN_ACCOUNT_ID                         AS LN_ACCOUNTID,
                T.CREATED_ON_TXN                        AS CREATEDON,
                TO_CHAR(T.CREATED_ON_TXN,'DD-MON-YYYY') AS CREATEDON_DISP,
                T.REMARKS                               AS REMARKS,
                T.REJECTION_DATE                        AS REJECTIONDATE,
                TO_CHAR(T.REJECTION_DATE,'DD-MON-YYYY') AS REJECTIONDATE_DISP,
                T.REVERSAL_DATE                         AS REVERSALDATE,
                TO_CHAR(T.REVERSAL_DATE,'DD-MON-YYYY') AS REVERSALDATE_DISP,
                T.WORKING_DATE                          AS WORKINGDATE,
                TO_CHAR(T.WORKING_DATE,'DD-MON-YYYY')  AS WORKINGDATE_DISP,
                T.AUTHORIZATION_DATE                    AS AUTHORIZATIONDATE,
                TO_CHAR(T.AUTHORIZATION_DATE,'DD-MON-YYYY') AS AUTHORIZATIONDATE_DISP,
                T.MCO_RECEIPT_NO                        AS MCO_RECEIPT_NO,
                T.MCO_BOOK_NO                           AS MCO_BOOK_NO
            FROM T_AU_IID_EXC_LOAN_TXN T
            WHERE T.INQUIRY_ID   = P_INQUIRY_ID
              AND T.LOAN_DISB_ID = P_LOAN_DISB_ID
              AND T.IS_ACTIVE    = 'Y'
            ORDER BY T.TRANSACTION_DATE, T.LOAN_TXN_ID;
    END P_GET_IID_LOAN_TRANSACTIONS;

END PKG_IID_EXC;

