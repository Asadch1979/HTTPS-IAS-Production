-- Run only in a disposable validation schema after installing SCORE_V1.
SET SERVEROUTPUT ON
WHENEVER SQLERROR EXIT FAILURE ROLLBACK
DECLARE
  total NUMBER := 0;
  PROCEDURE eq(actual NUMBER, expected NUMBER) IS
  BEGIN
    IF actual IS NULL OR ABS(actual-expected)>0.000000001 THEN
      RAISE_APPLICATION_ERROR(-20990,'Calculation mismatch');
    END IF;
  END;
  PROCEDURE band(score NUMBER, branch_size VARCHAR2, expected VARCHAR2, fraud NUMBER DEFAULT 0) IS
  BEGIN
    IF PKG_FAD_RISK_SCORE_V1.final_rating(score,branch_size,fraud) <> expected THEN
      RAISE_APPLICATION_ERROR(-20991,'Rating mismatch');
    END IF;
  END;
  PROCEDURE add_marks(raw NUMBER, weight NUMBER, denominator NUMBER) IS
  BEGIN total := total + PKG_FAD_RISK_SCORE_V1.normalized_marks(raw,weight,denominator); END;
  PROCEDURE zero_denominator(kind NUMBER) IS
    n NUMBER;
  BEGIN
    BEGIN
      IF kind=1 THEN n:=PKG_FAD_RISK_SCORE_V1.movement_marks(0,1);
      ELSIF kind=2 THEN n:=PKG_FAD_RISK_SCORE_V1.shortfall_marks(0,0);
      ELSE n:=PKG_FAD_RISK_SCORE_V1.normalized_marks(1,1,0); END IF;
    EXCEPTION WHEN OTHERS THEN
      IF SQLCODE=-20041 THEN RETURN; ELSE RAISE; END IF;
    END;
    RAISE_APPLICATION_ERROR(-20992,'Expected denominator rejection');
  END;
BEGIN
  eq(PKG_FAD_RISK_SCORE_V1.instance_marks(3,9),27);
  add_marks(0,6,22); add_marks(27,6,22); add_marks(0,3,22); add_marks(0,3,22); add_marks(0,4,22);
  add_marks(5,6,12); add_marks(2,6,12);
  add_marks(PKG_FAD_RISK_SCORE_V1.movement_marks(279.05,487.021),5,20);
  add_marks(PKG_FAD_RISK_SCORE_V1.movement_marks(118.84,150),5,20);
  add_marks(PKG_FAD_RISK_SCORE_V1.shortfall_marks(23.1,23),2,20);
  add_marks(PKG_FAD_RISK_SCORE_V1.shortfall_marks(605,358.54),3,20);
  add_marks(PKG_FAD_RISK_SCORE_V1.shortfall_marks(812.595,363.334),5,20);
  add_marks(0,5,36); add_marks(156,5,36); add_marks(405,6,36);
  add_marks(3,3,36); add_marks(10,10,36); add_marks(187,7,36);
  add_marks(PKG_FAD_RISK_SCORE_V1.profit_marks(-22.69),10,10);
  add_marks(999,0,0);
  eq(total,53.571067821);
  band(total,'Medium','High');
  FOR i IN 1..3 LOOP zero_denominator(i); END LOOP;
  band(9.999,'Small','Low'); band(10,'Small','Medium'); band(39.999,'Small','Medium'); band(40,'Small','High');
  band(14.999,'Medium','Low'); band(15,'Medium','Medium'); band(49.999,'Medium','Medium'); band(50,'Medium','High');
  band(19.999,'Large','Low'); band(20,'Large','Medium'); band(59.999,'Large','Medium'); band(60,'Large','High');
  band(0,'Small','High',1); band(0,'Medium','High',1); band(0,'Large','High',1);
  eq(PKG_FAD_RISK_SCORE_V1.profit_marks(-50000.01),10);
  eq(PKG_FAD_RISK_SCORE_V1.profit_marks(-50000),5);
  eq(PKG_FAD_RISK_SCORE_V1.profit_marks(0),5);
  eq(PKG_FAD_RISK_SCORE_V1.profit_marks(0.01),3);
  eq(PKG_FAD_RISK_SCORE_V1.profit_marks(500000),3);
  eq(PKG_FAD_RISK_SCORE_V1.profit_marks(500000.01),0);
  DBMS_OUTPUT.PUT_LINE('PASS: sample, thresholds, caps, fraud and zero denominators');
END;
/
