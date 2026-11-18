"""Source regression guards, NOT substitutes for executing denial tests in Oracle."""
import pathlib, unittest
ROOT=pathlib.Path(__file__).resolve().parents[2]
DIR=ROOT/'AIS/Docs/sql/fad-risk-register'
class SecurityContract(unittest.TestCase):
 def setUp(self): self.sql=(DIR/'assessment-body.sql.inc').read_text()
 def test_unauthorized_context_and_unknown_operation(self):
  for token in ["p_operation IS NULL OR p_operation NOT IN",'p_context IS NULL AND s.user_context_id IS NULL','s.user_context_id=p_context',"s.session_active='Y'",'c.user_id=u.userid']:
   self.assertIn(token,self.sql)
 def test_closed_engagement_and_entity_changes(self):
  for token in ['SELECT status,entity_id INTO n,v_entity','SELECT entity_id,status INTO v_entity,v_status',"v_status NOT BETWEEN 10 AND 13",'a.entity_id<>v_entity']:
   self.assertIn(token,self.sql)
  self.assertGreaterEqual(self.sql.count('FOR UPDATE'),4)
 def test_incomplete_submission_and_approval(self):
  self.assertIn("IF p_action IN ('SUBMIT','APPROVE') THEN require_complete(p_id)",self.sql)
  for token in ['expected<>127','Invalid detail mapping','n<>20','a.rating IS NULL','total<>a.score']:
   self.assertIn(token,self.sql)
 def test_evidence_ownership(self):
  for token in ['f.memoid=o.id','o.entity_id=e.entity_id','h.entity_id=e.entity_id','e.auditby_id=:office',"^WP:[0-9]{1,18}$",'validate_evidence(a.eng_id,office']:
   self.assertIn(token,self.sql)
 def test_invalid_publication(self):
  sql=(DIR/'25_publication.sql').read_text()
  self.assertIn("SYS_CONTEXT('USERENV','SESSION_USER')",sql)
  self.assertIn('P_CHECK_FAD_RISK_V1(p_version)',sql)
  contract=(DIR/'24_exact_configuration.sql').read_text()
  self.assertEqual(contract.count('Incorrect Annexure mapping'),121)
  self.assertEqual(contract.count('Incorrect process distribution'),20)
  self.assertIn('weight=0 AND denominator=0',contract)
