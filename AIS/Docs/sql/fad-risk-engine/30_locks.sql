-- DBA maintenance before enabling execution and after adding engagements.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
INSERT INTO T_FAD_ENGINE_LOCK(eng_id)
SELECT e.eng_id FROM T_AU_PLAN_ENG e WHERE NOT EXISTS(SELECT 1 FROM T_FAD_ENGINE_LOCK l WHERE l.eng_id=e.eng_id);
COMMIT;
