-- Deploy after recompiling PKG_USER_CONTEXT.sql and before releasing the application.
-- Register the dedicated POST and reuse existing Manage User write permissions.
-- Re-login after deployment to refresh cached API permissions.
SET DEFINE OFF;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
DECLARE
    V_PAGE_ID NUMBER;
    V_API_ID NUMBER;
BEGIN
    SELECT MIN(PAGE_ID) INTO V_PAGE_ID FROM T_AU_API_MASTER
     WHERE LOWER(API_PATH) IN ('/administrationpanel/save_user_contexts', '/administrationpanel/update_user')
       AND UPPER(HTTP_METHOD) = 'POST' AND IS_ACTIVE = 'Y';
    IF V_PAGE_ID IS NULL THEN
        RAISE_APPLICATION_ERROR(-20001, 'Register the existing Manage User Save API/page before deploying Remove.');
    END IF;
    INSERT INTO T_AU_API_MASTER
        (ACTION_NAME, PAGE_ID, CONTROLLER_NAME, API_PATH, HTTP_METHOD, IS_ACTIVE, CREATED_ON)
    SELECT 'delete_user_context_assignment', V_PAGE_ID, 'AdministrationPanel',
           '/AdministrationPanel/delete_user_context_assignment', 'POST', 'Y', SYSDATE
      FROM DUAL WHERE NOT EXISTS
        (SELECT 1 FROM T_AU_API_MASTER
          WHERE LOWER(API_PATH) = '/administrationpanel/delete_user_context_assignment'
            AND UPPER(HTTP_METHOD) = 'POST');
    SELECT API_ID INTO V_API_ID FROM T_AU_API_MASTER
     WHERE LOWER(API_PATH) = '/administrationpanel/delete_user_context_assignment'
       AND UPPER(HTTP_METHOD) = 'POST';
    INSERT INTO T_AU_ROLE_API_PERMISSION (ROLE_ID, API_ID, IS_ACTIVE, CREATED_BY)
    SELECT P.ROLE_ID, V_API_ID, 'Y', MIN(P.CREATED_BY)
      FROM T_AU_ROLE_API_PERMISSION P JOIN T_AU_API_MASTER M ON M.API_ID = P.API_ID
     WHERE LOWER(M.API_PATH) IN ('/administrationpanel/save_user_contexts', '/administrationpanel/update_user')
       AND UPPER(M.HTTP_METHOD) = 'POST' AND M.IS_ACTIVE = 'Y' AND P.IS_ACTIVE = 'Y'
       AND NOT EXISTS (SELECT 1 FROM T_AU_ROLE_API_PERMISSION X
                        WHERE X.ROLE_ID = P.ROLE_ID AND X.API_ID = V_API_ID)
     GROUP BY P.ROLE_ID;
END;
/
COMMIT;
