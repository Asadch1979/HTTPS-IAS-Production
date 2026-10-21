-- F8: execute this file alone in PL/SQL Developer SQL Window.
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_errors WHERE name IN
    ('PKG_HEAD_AUDIT_MONITOR','V_HEAD_AUDIT_ROLE_DEPT','V_HEAD_AUDIT_ENG_SCOPE','V_HEAD_AUDIT_OFFICE_SCOPE');
  IF n>0 THEN RAISE_APPLICATION_ERROR(-20155,'Head audit monitoring compilation errors; inspect USER_ERRORS.'); END IF;
END;
