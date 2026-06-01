CREATE OR REPLACE PROCEDURE P_add_activity_log (
    v_entity_id  IN NUMBER,
    v_role_id    IN NUMBER,
    v_ppnum      IN NUMBER,
    v_page_id    IN NUMBER,
    v_action     IN VARCHAR2,
    v_unattend   IN VARCHAR2 DEFAULT 'Y'
) AS
    v_last_id      NUMBER := 0;
    v_seq          NUMBER := 1;
    v_last_endtime DATE;
    v_start_time   DATE;
    v_duration     NUMBER := 0; -- in minutes
BEGIN
    -- Get last activity id and its end_time for ppnum
    SELECT NVL(MAX(l.id), 0)
      INTO v_last_id
      FROM t_au_activity_log l
     WHERE l.ppnum = v_ppnum;

    IF v_last_id > 0 THEN
        SELECT end_time
          INTO v_last_endtime
          FROM t_au_activity_log
         WHERE id = v_last_id;

        -- Check if last activity was today and has a non-null end_time
        IF v_last_endtime IS NOT NULL AND TRUNC(v_last_endtime) = TRUNC(SYSDATE) THEN
            v_start_time := v_last_endtime;
        ELSE
            v_start_time := SYSDATE;
        END IF;
    ELSE
        v_start_time := SYSDATE;
    END IF;

    -- Calculate duration (in minutes)
    v_duration := ROUND((SYSDATE - v_start_time) * 24 * 60);

    -- Get next sequence number
    SELECT COALESCE(MAX(l.seq) + 1, 1)
      INTO v_seq
      FROM t_au_activity_log l
     WHERE l.ppnum = v_ppnum;

    -- Insert new log entry
    INSERT INTO t_au_activity_log (
        id, entity_id, role_id, ppnum, page_id, action, start_time, end_time, duration, seq, unattend
    ) VALUES (
        (SELECT COALESCE(MAX(p.id) + 1, 1) FROM t_au_activity_log p),
        v_entity_id,
        v_role_id,
        v_ppnum,
        v_page_id,
        v_action,
        v_start_time,
        SYSDATE,
        v_duration,
        v_seq,
        v_unattend
    );

    COMMIT;
END;

