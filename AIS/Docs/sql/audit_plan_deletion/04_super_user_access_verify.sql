-- UAT-only read-only verification for IAS role 1 (Super User).
-- Do not copy session IDs, cookies or personally identifying details into tickets.
-- Review registered approval groups and current DB schema before rollout.
SET SERVEROUTPUT ON;

SELECT G.GROUP_ID, G.ROLE_ID, G.STATUS,
       CASE WHEN EXISTS (
         SELECT 1 FROM T_MENU_PAGES_GROUPMAP MPG JOIN T_MENU_PAGES MP
           ON MP.ID=MPG.PAGE_ID
         WHERE MPG.GROUP_ID=G.GROUP_ID
           AND MP.STATUS='A'
           AND LOWER(TRIM(BOTH '/' FROM MP.PAGE_PATH))=
               'planning/audit_plan_deletion_authorization'
       ) THEN 'GRANTED' ELSE 'MISSING' END AS APPROVAL_PAGE
FROM T_GROUPS G
WHERE G.ROLE_ID=1 AND G.STATUS='Y'
ORDER BY G.GROUP_ID;

SELECT S.ROLE_ID, S.GROUP_ID, S.ENTITY_ID, S.SESSION_ACTIVE,
       COUNT(*) AS SESSION_COUNT
FROM T_USER_SESSION S
WHERE S.ROLE_ID=1 AND S.LOGGED_OUT_DATE IS NULL
GROUP BY S.ROLE_ID,S.GROUP_ID,S.ENTITY_ID,S.SESSION_ACTIVE
ORDER BY S.GROUP_ID,S.ENTITY_ID,S.SESSION_ACTIVE;

SELECT NAME,TYPE,LINE,POSITION,TEXT
FROM USER_ERRORS WHERE NAME='PKG_HD'
ORDER BY TYPE,SEQUENCE;

-- Execute a separate authorized-person UAT test through the application:
-- request made by User A, authorized by User B (Super User or approved officer).
-- User A must remain unable to authorize their own request (ORA-20219).
-- Do not attempt direct destructive PL/SQL without an approved UAT fixture.
