CREATE OR REPLACE PACKAGE PKG_FAD_RISK AS
  TYPE t_cursor IS REF CURSOR;

  PROCEDURE P_SAVE_KPI_MAIN(p_kpi_main_id   IN OUT NUMBER,
                            p_code          IN VARCHAR2,
                            p_name          IN VARCHAR2,
                            p_display_order IN NUMBER,
                            p_is_active     IN CHAR);

  PROCEDURE P_SAVE_KPI_SUB(p_kpi_sub_id    IN OUT NUMBER,
                           p_kpi_main_id   IN NUMBER,
                           p_code          IN VARCHAR2,
                           p_name          IN VARCHAR2,
                           p_weightage     IN NUMBER,
                           p_display_order IN NUMBER,
                           p_is_active     IN CHAR);

  PROCEDURE P_SAVE_PROCESS(p_process_id    IN OUT NUMBER,
                           p_kpi_sub_id    IN NUMBER,
                           p_code          IN VARCHAR2,
                           p_name          IN VARCHAR2,
                           p_weightage     IN NUMBER,
                           p_display_order IN NUMBER,
                           p_is_active     IN CHAR);

  PROCEDURE P_SAVE_SUB_PROCESS(p_sub_process_id IN OUT NUMBER,
                               p_process_id     IN NUMBER,
                               p_code           IN VARCHAR2,
                               p_name           IN VARCHAR2,
                               p_gravity_id     IN NUMBER,
                               p_is_active      IN CHAR);

  PROCEDURE P_SAVE_ANNEXURE(p_annexure_id IN OUT NUMBER,
                            p_code        IN VARCHAR2,
                            p_name        IN VARCHAR2,
                            p_is_active   IN CHAR);

  PROCEDURE P_SAVE_SUBPROC_ANNEX(p_sub_process_id IN NUMBER,
                                 p_annexure_id    IN NUMBER,
                                 p_is_active      IN CHAR);

  PROCEDURE P_GET_KPI_MAIN(o_cursor OUT t_cursor);
  PROCEDURE P_GET_KPI_SUB(p_kpi_main_id IN NUMBER, o_cursor OUT t_cursor);
  PROCEDURE P_GET_PROCESS(p_kpi_sub_id IN NUMBER, o_cursor OUT t_cursor);
  PROCEDURE P_GET_SUB_PROCESS(p_process_id IN NUMBER,
                              o_cursor     OUT t_cursor);
  PROCEDURE P_GET_ANNEXURE(o_cursor OUT t_cursor);
  PROCEDURE P_GET_SUBPROC_ANNEX(p_sub_process_id IN NUMBER,
                                o_cursor         OUT t_cursor);
  PROCEDURE P_GET_GRAVITY(o_cursor OUT t_cursor);

END PKG_FAD_RISK;

CREATE OR REPLACE PACKAGE BODY PKG_FAD_RISK AS

  /* ===== Helpers: resolve actual column names safely ===== */

  FUNCTION F_HAS_COL(p_table IN VARCHAR2, p_col IN VARCHAR2) RETURN NUMBER IS
    v_cnt NUMBER;
  BEGIN
    SELECT COUNT(*)
      INTO v_cnt
      FROM USER_TAB_COLS
     WHERE TABLE_NAME = UPPER(p_table)
       AND COLUMN_NAME = UPPER(p_col);
  
    RETURN CASE WHEN v_cnt > 0 THEN 1 ELSE 0 END;
  END;

  FUNCTION F_PICK_COL(p_table IN VARCHAR2,
                      p_c1    IN VARCHAR2,
                      p_c2    IN VARCHAR2,
                      p_c3    IN VARCHAR2,
                      p_c4    IN VARCHAR2) RETURN VARCHAR2 IS
  BEGIN
    IF p_c1 IS NOT NULL AND F_HAS_COL(p_table, p_c1) = 1 THEN
      RETURN UPPER(p_c1);
    END IF;
    IF p_c2 IS NOT NULL AND F_HAS_COL(p_table, p_c2) = 1 THEN
      RETURN UPPER(p_c2);
    END IF;
    IF p_c3 IS NOT NULL AND F_HAS_COL(p_table, p_c3) = 1 THEN
      RETURN UPPER(p_c3);
    END IF;
    IF p_c4 IS NOT NULL AND F_HAS_COL(p_table, p_c4) = 1 THEN
      RETURN UPPER(p_c4);
    END IF;
  
    RETURN NULL; -- caller must handle
  END;

  /* ============================================================
  SAVE – MAIN KPI
  ============================================================ */
  PROCEDURE P_SAVE_KPI_MAIN(p_kpi_main_id   IN OUT NUMBER,
                            p_code          IN VARCHAR2,
                            p_name          IN VARCHAR2,
                            p_display_order IN NUMBER,
                            p_is_active     IN CHAR) IS
    v_id NUMBER;
  BEGIN
    IF p_kpi_main_id IS NULL OR p_kpi_main_id = 0 THEN
      v_id := SEQ_RISK_FAD_KPI_MAIN.NEXTVAL;
    
      INSERT INTO T_RISK_FAD_KPI_MAIN
        (KPI_MAIN_ID, KPI_CODE, KPI_NAME, DISPLAY_ORDER, IS_ACTIVE)
      VALUES
        (v_id,
         UPPER(TRIM(p_code)),
         TRIM(p_name),
         NVL(p_display_order, 0),
         NVL(p_is_active, 'Y'));
    
      p_kpi_main_id := v_id;
    ELSE
      UPDATE T_RISK_FAD_KPI_MAIN
         SET KPI_CODE      = UPPER(TRIM(p_code)),
             KPI_NAME      = TRIM(p_name),
             DISPLAY_ORDER = NVL(p_display_order, 0),
             IS_ACTIVE     = NVL(p_is_active, 'Y')
       WHERE KPI_MAIN_ID = p_kpi_main_id;
    END IF;
  
    COMMIT;
  END P_SAVE_KPI_MAIN;

  /* ============================================================
  SAVE – SUB KPI
  ============================================================ */
  PROCEDURE P_SAVE_KPI_SUB(p_kpi_sub_id    IN OUT NUMBER,
                           p_kpi_main_id   IN NUMBER,
                           p_code          IN VARCHAR2,
                           p_name          IN VARCHAR2,
                           p_weightage     IN NUMBER,
                           p_display_order IN NUMBER,
                           p_is_active     IN CHAR) IS
    v_id         NUMBER;
    v_name_col   VARCHAR2(30);
    v_active_col VARCHAR2(30);
    v_sql        CLOB;
  BEGIN
    v_name_col := F_PICK_COL('T_RISK_FAD_KPI_SUB',
                             'KPI_NAME',
                             'KPI_SUB_NAME',
                             'SUB_KPI_NAME',
                             'NAME');
    IF v_name_col IS NULL THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'Name column not found in T_RISK_FAD_KPI_SUB (expected KPI_NAME/KPI_SUB_NAME/SUB_KPI_NAME/NAME).');
    END IF;
  
    v_active_col := F_PICK_COL('T_RISK_FAD_KPI_SUB',
                               'IS_ACTIVE',
                               'STATUS',
                               'ACTIVE',
                               'ISACTIVE');
    IF v_active_col IS NULL THEN
      -- if table has no active flag, fall back to not using it
      v_active_col := NULL;
    END IF;
  
    IF p_kpi_sub_id IS NULL OR p_kpi_sub_id = 0 THEN
      v_id := SEQ_RISK_FAD_KPI_SUB.NEXTVAL;
    
      v_sql := 'INSERT INTO T_RISK_FAD_KPI_SUB (KPI_SUB_ID, KPI_MAIN_ID, KPI_CODE, ' ||
               v_name_col || ', WEIGHTAGE, DISPLAY_ORDER' || CASE
                 WHEN v_active_col IS NOT NULL THEN
                  ', ' || v_active_col
                 ELSE
                  ''
               END || ') VALUES (:1, :2, :3, :4, :5, :6' || CASE
                 WHEN v_active_col IS NOT NULL THEN
                  ', :7'
                 ELSE
                  ''
               END || ')';
    
      IF v_active_col IS NOT NULL THEN
        EXECUTE IMMEDIATE v_sql
          USING v_id, p_kpi_main_id, UPPER(TRIM(p_code)), TRIM(p_name), NVL(p_weightage, 0), NVL(p_display_order, 0), NVL(p_is_active, 'Y');
      ELSE
        EXECUTE IMMEDIATE v_sql
          USING v_id, p_kpi_main_id, UPPER(TRIM(p_code)), TRIM(p_name), NVL(p_weightage, 0), NVL(p_display_order, 0);
      END IF;
    
      p_kpi_sub_id := v_id;
    
    ELSE
      v_sql := 'UPDATE T_RISK_FAD_KPI_SUB SET KPI_MAIN_ID=:1, KPI_CODE=:2, ' ||
               v_name_col || '=:3, WEIGHTAGE=:4, DISPLAY_ORDER=:5' || CASE
                 WHEN v_active_col IS NOT NULL THEN
                  ', ' || v_active_col || '=:6'
                 ELSE
                  ''
               END || ' WHERE KPI_SUB_ID=:7';
    
      IF v_active_col IS NOT NULL THEN
        EXECUTE IMMEDIATE v_sql
          USING p_kpi_main_id, UPPER(TRIM(p_code)), TRIM(p_name), NVL(p_weightage, 0), NVL(p_display_order, 0), NVL(p_is_active, 'Y'), p_kpi_sub_id;
      ELSE
        EXECUTE IMMEDIATE v_sql
          USING p_kpi_main_id, UPPER(TRIM(p_code)), TRIM(p_name), NVL(p_weightage, 0), NVL(p_display_order, 0), p_kpi_sub_id;
      END IF;
    END IF;
  
    COMMIT;
  END P_SAVE_KPI_SUB;

  /* ============================================================
  SAVE – PROCESS
  ============================================================ */
  PROCEDURE P_SAVE_PROCESS(p_process_id    IN OUT NUMBER,
                           p_kpi_sub_id    IN NUMBER,
                           p_code          IN VARCHAR2,
                           p_name          IN VARCHAR2,
                           p_weightage     IN NUMBER,
                           p_display_order IN NUMBER,
                           p_is_active     IN CHAR) IS
    v_id NUMBER;
  BEGIN
    IF p_process_id IS NULL OR p_process_id = 0 THEN
      v_id := SEQ_RISK_FAD_PROCESS.NEXTVAL;
    
      INSERT INTO T_RISK_FAD_PROCESS
        (PROCESS_ID,
         KPI_SUB_ID,
         PROCESS_CODE,
         PROCESS_NAME,
         WEIGHTAGE,
         DISPLAY_ORDER,
         IS_ACTIVE)
      VALUES
        (v_id,
         p_kpi_sub_id,
         UPPER(TRIM(p_code)),
         TRIM(p_name),
         NVL(p_weightage, 0),
         NVL(p_display_order, 0),
         NVL(p_is_active, 'Y'));
    
      p_process_id := v_id;
    ELSE
      UPDATE T_RISK_FAD_PROCESS
         SET KPI_SUB_ID    = p_kpi_sub_id,
             PROCESS_CODE  = UPPER(TRIM(p_code)),
             PROCESS_NAME  = TRIM(p_name),
             WEIGHTAGE     = NVL(p_weightage, 0),
             DISPLAY_ORDER = NVL(p_display_order, 0),
             IS_ACTIVE     = NVL(p_is_active, 'Y')
       WHERE PROCESS_ID = p_process_id;
    END IF;
  
    COMMIT;
  END P_SAVE_PROCESS;

  /* ============================================================
  SAVE – SUB PROCESS
  ============================================================ */
  PROCEDURE P_SAVE_SUB_PROCESS(p_sub_process_id IN OUT NUMBER,
                               p_process_id     IN NUMBER,
                               p_code           IN VARCHAR2,
                               p_name           IN VARCHAR2,
                               p_gravity_id     IN NUMBER,
                               p_is_active      IN CHAR) IS
    v_id NUMBER;
  BEGIN
    IF p_sub_process_id IS NULL OR p_sub_process_id = 0 THEN
      v_id := SEQ_RISK_FAD_SUB_PROCESS.NEXTVAL;
    
      INSERT INTO T_RISK_FAD_SUB_PROCESS
        (SUB_PROCESS_ID,
         PROCESS_ID,
         SUB_PROCESS_CODE,
         SUB_PROCESS_NAME,
         GRAVITY_ID,
         IS_ACTIVE)
      VALUES
        (v_id,
         p_process_id,
         UPPER(TRIM(p_code)),
         TRIM(p_name),
         p_gravity_id,
         NVL(p_is_active, 'Y'));
    
      p_sub_process_id := v_id;
    ELSE
      UPDATE T_RISK_FAD_SUB_PROCESS
         SET PROCESS_ID       = p_process_id,
             SUB_PROCESS_CODE = UPPER(TRIM(p_code)),
             SUB_PROCESS_NAME = TRIM(p_name),
             GRAVITY_ID       = p_gravity_id,
             IS_ACTIVE        = NVL(p_is_active, 'Y')
       WHERE SUB_PROCESS_ID = p_sub_process_id;
    END IF;
  
    COMMIT;
  END P_SAVE_SUB_PROCESS;

  /* ============================================================
  SAVE – ANNEXURE
  ============================================================ */
  PROCEDURE P_SAVE_ANNEXURE(p_annexure_id IN OUT NUMBER,
                            p_code        IN VARCHAR2,
                            p_name        IN VARCHAR2,
                            p_is_active   IN CHAR) IS
    v_id NUMBER;
  BEGIN
    IF p_annexure_id IS NULL OR p_annexure_id = 0 THEN
      v_id := SEQ_RISK_FAD_ANNEXURE.NEXTVAL;
    
      INSERT INTO T_RISK_FAD_ANNEXURE
        (ANNEXURE_ID, ANNEXURE_CODE, ANNEXURE_NAME, IS_ACTIVE)
      VALUES
        (v_id, UPPER(TRIM(p_code)), TRIM(p_name), NVL(p_is_active, 'Y'));
    
      p_annexure_id := v_id;
    ELSE
      UPDATE T_RISK_FAD_ANNEXURE
         SET ANNEXURE_CODE = UPPER(TRIM(p_code)),
             ANNEXURE_NAME = TRIM(p_name),
             IS_ACTIVE     = NVL(p_is_active, 'Y')
       WHERE ANNEXURE_ID = p_annexure_id;
    END IF;
  
    COMMIT;
  END P_SAVE_ANNEXURE;

  /* ============================================================
  SAVE – SUB PROCESS ? ANNEXURE MAP
  ============================================================ */
  PROCEDURE P_SAVE_SUBPROC_ANNEX(p_sub_process_id IN NUMBER,
                                 p_annexure_id    IN NUMBER,
                                 p_is_active      IN CHAR) IS
  BEGIN
    MERGE INTO T_RISK_FAD_SUBPROC_ANNEX m
    USING (SELECT p_sub_process_id AS sub_process_id,
                  p_annexure_id    AS annexure_id
             FROM dual) s
    ON (m.SUB_PROCESS_ID = s.sub_process_id AND m.ANNEXURE_ID = s.annexure_id)
    WHEN MATCHED THEN
      UPDATE SET m.IS_ACTIVE = NVL(p_is_active, 'Y')
    WHEN NOT MATCHED THEN
      INSERT
        (SUB_PROCESS_ID, ANNEXURE_ID, IS_ACTIVE)
      VALUES
        (p_sub_process_id, p_annexure_id, NVL(p_is_active, 'Y'));
  
    COMMIT;
  END P_SAVE_SUBPROC_ANNEX;

  /* ============================================================
  GET – MAIN KPI
  ============================================================ */
  PROCEDURE P_GET_KPI_MAIN(o_cursor OUT t_cursor) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT KPI_MAIN_ID, KPI_CODE, KPI_NAME, DISPLAY_ORDER, IS_ACTIVE
        FROM T_RISK_FAD_KPI_MAIN
       ORDER BY DISPLAY_ORDER, KPI_MAIN_ID;
  END P_GET_KPI_MAIN;

  /* ============================================================
  GET – SUB KPI (BY MAIN)
  ============================================================ */
  PROCEDURE P_GET_KPI_SUB(p_kpi_main_id IN NUMBER, o_cursor OUT t_cursor) IS
    v_name_col   VARCHAR2(30);
    v_active_col VARCHAR2(30);
    v_sql        CLOB;
  BEGIN
    v_name_col := F_PICK_COL('T_RISK_FAD_KPI_SUB',
                             'KPI_NAME',
                             'KPI_SUB_NAME',
                             'SUB_KPI_NAME',
                             'NAME');
    IF v_name_col IS NULL THEN
      RAISE_APPLICATION_ERROR(-20002,
                              'Name column not found in T_RISK_FAD_KPI_SUB.');
    END IF;
  
    v_active_col := F_PICK_COL('T_RISK_FAD_KPI_SUB',
                               'IS_ACTIVE',
                               'STATUS',
                               'ACTIVE',
                               'ISACTIVE');
  
    v_sql := 'SELECT KPI_SUB_ID,
                   KPI_MAIN_ID,
                   KPI_CODE,
                   ' || v_name_col ||
             ' AS KPI_NAME,
                   WEIGHTAGE,
                   DISPLAY_ORDER' || CASE
               WHEN v_active_col IS NOT NULL THEN
                ', ' || v_active_col || ' AS IS_ACTIVE'
               ELSE
                ', ''Y'' AS IS_ACTIVE'
             END || '  FROM T_RISK_FAD_KPI_SUB
              WHERE KPI_MAIN_ID = :1
              ORDER BY DISPLAY_ORDER, KPI_SUB_ID';
  
    OPEN o_cursor FOR v_sql
      USING p_kpi_main_id;
  END P_GET_KPI_SUB;

  /* ============================================================
  GET – PROCESS (BY SUB KPI)
  ============================================================ */
  PROCEDURE P_GET_PROCESS(p_kpi_sub_id IN NUMBER, o_cursor OUT t_cursor) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT PROCESS_ID,
             KPI_SUB_ID,
             PROCESS_CODE,
             PROCESS_NAME,
             WEIGHTAGE,
             DISPLAY_ORDER,
             IS_ACTIVE
        FROM T_RISK_FAD_PROCESS
       WHERE KPI_SUB_ID = p_kpi_sub_id
       ORDER BY DISPLAY_ORDER, PROCESS_ID;
  END P_GET_PROCESS;

  /* ============================================================
  GET – SUB PROCESS (BY PROCESS)
  ============================================================ */
  PROCEDURE P_GET_SUB_PROCESS(p_process_id IN NUMBER,
                              o_cursor     OUT t_cursor) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT SUB_PROCESS_ID,
             PROCESS_ID,
             SUB_PROCESS_CODE,
             SUB_PROCESS_NAME,
             GRAVITY_ID,
             IS_ACTIVE
        FROM T_RISK_FAD_SUB_PROCESS
       WHERE PROCESS_ID = p_process_id
       ORDER BY SUB_PROCESS_ID;
  END P_GET_SUB_PROCESS;

  /* ============================================================
  GET – ANNEXURE MASTER
  ============================================================ */
  PROCEDURE P_GET_ANNEXURE(o_cursor OUT t_cursor) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT ANNEXURE_ID, ANNEXURE_CODE, ANNEXURE_NAME, IS_ACTIVE
        FROM T_RISK_FAD_ANNEXURE
       ORDER BY ANNEXURE_CODE, ANNEXURE_ID;
  END P_GET_ANNEXURE;

  /* ============================================================
  GET – SUB PROCESS ? ANNEXURE MAP
  ============================================================ */
  PROCEDURE P_GET_SUBPROC_ANNEX(p_sub_process_id IN NUMBER,
                                o_cursor         OUT t_cursor) IS
  BEGIN
    OPEN o_cursor FOR
      SELECT SUB_PROCESS_ID, ANNEXURE_ID, IS_ACTIVE
        FROM T_RISK_FAD_SUBPROC_ANNEX
       WHERE SUB_PROCESS_ID = p_sub_process_id
       ORDER BY ANNEXURE_ID;
  END P_GET_SUBPROC_ANNEX;

  /* ============================================================
  GET – GRAVITY MASTER
  ============================================================ */
  PROCEDURE P_GET_GRAVITY(o_cursor OUT t_cursor) IS
    v_active_col VARCHAR2(30);
    v_sql        CLOB;
  BEGIN
    v_active_col := F_PICK_COL('T_RISK_FAD_GRAVITY',
                               'IS_ACTIVE',
                               'STATUS',
                               'ACTIVE',
                               'ISACTIVE');
  
    v_sql := 'SELECT GRAVITY_ID,
                   GRAVITY_NAME,
                   DISPLAY_ORDER' || CASE
               WHEN v_active_col IS NOT NULL THEN
                ', ' || v_active_col || ' AS IS_ACTIVE'
               ELSE
                ', ''Y'' AS IS_ACTIVE'
             END || '  FROM T_RISK_FAD_GRAVITY
              ORDER BY DISPLAY_ORDER, GRAVITY_ID';
  
    OPEN o_cursor FOR v_sql;
  END P_GET_GRAVITY;

END PKG_FAD_RISK;

