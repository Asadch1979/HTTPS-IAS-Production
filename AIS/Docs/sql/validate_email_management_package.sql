-- Validates standalone email-management Oracle objects after deployment.
-- Run in SQL*Plus, SQLcl, or SQL Developer as the AIS application schema owner.

SET SERVEROUTPUT ON

DECLARE
    v_invalid_count NUMBER;
BEGIN
    SELECT COUNT(*)
      INTO v_invalid_count
      FROM user_objects
     WHERE object_name IN (
           'PKG_EMAIL',
           'EM_EMAIL_EVENT',
           'EM_EMAIL_TEMPLATE',
           'EM_EMAIL_RECIPIENT_RULE',
           'EM_EMAIL_PLACEHOLDER',
           'EM_EMAIL_ATTACHMENT_DEF',
           'EM_EMAIL_LOG')
       AND status <> 'VALID';

    IF v_invalid_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('Invalid standalone email-management objects:');
        FOR item IN (
            SELECT object_name, object_type, status
              FROM user_objects
             WHERE object_name IN (
                   'PKG_EMAIL',
                   'EM_EMAIL_EVENT',
                   'EM_EMAIL_TEMPLATE',
                   'EM_EMAIL_RECIPIENT_RULE',
                   'EM_EMAIL_PLACEHOLDER',
                   'EM_EMAIL_ATTACHMENT_DEF',
                   'EM_EMAIL_LOG')
               AND status <> 'VALID'
             ORDER BY object_name, object_type)
        LOOP
            DBMS_OUTPUT.PUT_LINE(item.object_name || ' ' || item.object_type || ' ' || item.status);
        END LOOP;
        RAISE_APPLICATION_ERROR(-20990, 'Standalone email-management validation failed.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('Standalone email-management objects are valid.');
END;
/

SELECT name, type, line, position, text
  FROM user_errors
 WHERE name = 'PKG_EMAIL'
 ORDER BY sequence;
