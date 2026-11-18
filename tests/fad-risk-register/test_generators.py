import pathlib, subprocess, sys, unittest
ROOT=pathlib.Path(__file__).resolve().parents[2]
class Generators(unittest.TestCase):
 def test_reproducible_sql(self):
  folder=ROOT/'AIS/Docs/sql/fad-risk-register'
  files=[folder/n for n in ['11_immutability.sql','20_workbook_seed.sql','24_exact_configuration.sql','30_assessment_package.sql']]
  before=[p.read_bytes() for p in files]
  for script in ['build_fad_risk_seed.py','build_fad_risk_package.py']:
   subprocess.run([sys.executable,str(ROOT/'tools'/script)],check=True,cwd=ROOT)
  self.assertEqual(before,[p.read_bytes() for p in files])
