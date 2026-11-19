"""Application source contract guards; live Oracle/MVC tests are a staging gate."""
import pathlib, unittest
ROOT=pathlib.Path(__file__).resolve().parents[2]
class ApplicationContract(unittest.TestCase):
 def test_session_only_parameters_and_transactions(self):
  source=(ROOT/'AIS/DBConnection.FAD.Assessment.cs').read_text()
  for name in ['p_user','p_role','p_entity','p_group','p_context','p_session']:
   line=next(s for s in source.splitlines() if '"'+name+'"' in s)
   self.assertIn('user.',line)
   self.assertNotIn('request.',line)
  self.assertEqual(source.count('using var tx=con.BeginTransaction()'),2)
  self.assertEqual(source.count('tx.Commit()'),2)
  self.assertIn('OracleDbType.Clob',source)
  self.assertIn('ParameterDirection.InputOutput',source)
 def test_anti_forgery_and_api_checks(self):
  source=(ROOT/'AIS/Controllers/FADRiskControlPanelController.Assessment.cs').read_text()
  self.assertEqual(source.count('ValidateAntiForgeryToken'),2)
  self.assertIn('HasApiPermissionForPath',source)
  self.assertIn('UserHasPagePermissionForCurrentAction',source)
  self.assertNotIn('AllowAnonymous',source)
  self.assertNotIn('ex.Message',source)
 def test_frontend_database_matrix_and_safe_dom(self):
  source=(ROOT/'AIS/wwwroot/js/fad-risk-assessment.js').read_text()
  self.assertIn('rows = data.details',source)
  self.assertNotIn('innerHTML',source)
  self.assertIn('RequestVerificationToken',source)
  self.assertIn('mustReload = true',source)
  self.assertIn('permissions.canReview',source)
