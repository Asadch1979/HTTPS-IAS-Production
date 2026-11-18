-- REVIEW DRAFT: formula semantics, not an approved scoring specification.
-- Pure functions only: no access to assessments, no grants, no commits.
-- Keep this version immutable once assessments reference it.
CREATE OR REPLACE PACKAGE PKG_FAD_RISK_SCORE_V1 AS
  FUNCTION instance_marks(gravity NUMBER, instances NUMBER) RETURN NUMBER;
  FUNCTION normalized_marks(raw_marks NUMBER, weight NUMBER, group_weight NUMBER) RETURN NUMBER;
  FUNCTION movement_marks(opening NUMBER, closing NUMBER) RETURN NUMBER;
  FUNCTION shortfall_marks(target NUMBER, achieved NUMBER) RETURN NUMBER;
  FUNCTION profit_marks(profit_rupees NUMBER) RETURN NUMBER;
  FUNCTION final_rating(score NUMBER, branch_size VARCHAR2, fraud_instances NUMBER) RETURN VARCHAR2;
END PKG_FAD_RISK_SCORE_V1;
/
CREATE OR REPLACE PACKAGE BODY PKG_FAD_RISK_SCORE_V1 AS
  PROCEDURE nonnegative(n NUMBER) IS
  BEGIN
    IF n IS NULL OR n < 0 THEN RAISE_APPLICATION_ERROR(-20040,'Required nonnegative input missing or invalid.'); END IF;
  END;
  FUNCTION instance_marks(gravity NUMBER, instances NUMBER) RETURN NUMBER IS
  BEGIN
    nonnegative(instances);
    IF instances <> TRUNC(instances) OR gravity IS NULL OR gravity NOT IN (1,2,3,4,5) THEN
      RAISE_APPLICATION_ERROR(-20040,'Invalid gravity or instance count.');
    END IF;
    RETURN gravity * instances;
  END;
  FUNCTION normalized_marks(raw_marks NUMBER, weight NUMBER, group_weight NUMBER) RETURN NUMBER IS
  BEGIN
    nonnegative(raw_marks); nonnegative(weight); nonnegative(group_weight);
    -- Fraud is excluded from numerical scoring, including its denominator.
    IF weight = 0 THEN RETURN 0; END IF;
    IF group_weight = 0 OR weight > group_weight THEN
      RAISE_APPLICATION_ERROR(-20041,'Invalid normalization denominator.');
    END IF;
    RETURN LEAST(weight,raw_marks*weight/group_weight);
  END;
  FUNCTION movement_marks(opening NUMBER, closing NUMBER) RETURN NUMBER IS
  BEGIN
    nonnegative(opening); nonnegative(closing);
    IF opening = 0 THEN RAISE_APPLICATION_ERROR(-20041,'Zero opening balance requires review.'); END IF;
    RETURN GREATEST(0,(closing-opening)/opening*100);
  END;
  FUNCTION shortfall_marks(target NUMBER, achieved NUMBER) RETURN NUMBER IS
  BEGIN
    nonnegative(target); nonnegative(achieved);
    IF target = 0 THEN RAISE_APPLICATION_ERROR(-20041,'Zero target requires review.'); END IF;
    RETURN GREATEST(0,(target-achieved)/target*100);
  END;
  FUNCTION profit_marks(profit_rupees NUMBER) RETURN NUMBER IS
  BEGIN
    IF profit_rupees IS NULL THEN RAISE_APPLICATION_ERROR(-20040,'Profit in rupees is required.'); END IF;
    RETURN CASE WHEN profit_rupees < -50000 THEN 10 WHEN profit_rupees <= 0 THEN 5
                WHEN profit_rupees <= 500000 THEN 3 ELSE 0 END;
  END;
  FUNCTION final_rating(score NUMBER, branch_size VARCHAR2, fraud_instances NUMBER) RETURN VARCHAR2 IS
    low_limit NUMBER; high_limit NUMBER;
  BEGIN
    nonnegative(score); nonnegative(fraud_instances);
    IF score > 100 OR fraud_instances <> TRUNC(fraud_instances) THEN
      RAISE_APPLICATION_ERROR(-20040,'Invalid score or fraud count.');
    END IF;
    CASE branch_size
      WHEN 'Small' THEN low_limit:=10; high_limit:=40;
      WHEN 'Medium' THEN low_limit:=15; high_limit:=50;
      WHEN 'Large' THEN low_limit:=20; high_limit:=60;
      ELSE RAISE_APPLICATION_ERROR(-20040,'Invalid branch size.');
    END CASE;
    IF fraud_instances > 0 THEN RETURN 'High'; END IF;
    RETURN CASE WHEN score < low_limit THEN 'Low' WHEN score < high_limit THEN 'Medium' ELSE 'High' END;
  END;
END PKG_FAD_RISK_SCORE_V1;
/
