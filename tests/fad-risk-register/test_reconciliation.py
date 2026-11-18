"""Independent decimal reconciliation, without trusting Excel cached totals."""
import importlib.util, pathlib, unittest
from decimal import Decimal as D
ROOT = pathlib.Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('workbook', ROOT/'tools/fad_risk_workbook.py')
wb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(wb)

def normalized(raw, weight, denominator):
    if weight == 0: return D(0)
    if denominator <= 0: raise ValueError('denominator')
    return min(D(weight), raw*D(weight)/D(denominator))

def ratio(base, actual, movement=False):
    if base <= 0: raise ValueError('denominator')
    return max(D(0), (actual-base if movement else base-actual)/base*100)

def rating(score, size, fraud=0):
    low, high = {'Small':(10,40),'Medium':(15,50),'Large':(20,60)}[size]
    return 'High' if fraud else 'Low' if score < low else 'Medium' if score < high else 'High'

def profit(amount):
    return D(10 if amount < -50000 else 5 if amount <= 0 else 3 if amount <= 500000 else 0)

class Reconciliation(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = wb.extract()
        cls.config = wb.configuration(cls.source)

    def test_all_codes_and_weights(self):
        self.assertEqual(len(self.config['annexures']),121)
        self.assertEqual(len({a['code'] for a in self.config['annexures']}),121)
        self.assertEqual(sum(p['weight'] for p in self.config['processes']),100)
        self.assertTrue(all(a['description'] and a['process_id'] and a['sub_process_id'] for a in self.config['annexures']))

    def test_sample_from_inputs(self):
        m = self.source['sheets']['Rating Matrix']
        def n(c): return D(m[c]['value'])
        total = D(0)
        for p in self.config['processes']:
            raw = D(0)
            for r in p['matrix_rows']:
                if r in (33,34): raw += ratio(n(f'G{r}'),n(f'H{r}'),True)
                elif r in (37,38,39): raw += ratio(n(f'G{r}'),n(f'H{r}'))
                elif r == 64: raw += profit(n('I64'))
                else: raw += n(f'H{r}')*n(f'I{r}')
            marks = normalized(raw,p['weight'],p['normalization_total'])
            if p['weight']:
                cached = D(self.source['sheets']['Final Rating']['H'+p['id'][2:]]['value'])
                self.assertLess(abs(marks-cached),D('0.000000001'))
            total += marks
        self.assertLess(abs(total-D('53.571067821')),D('0.000000001'))
        self.assertEqual(rating(total,'Medium'),'High')

    def test_thresholds(self):
        for size,low,high in [('Small',10,40),('Medium',15,50),('Large',20,60)]:
            for value,expected in [(D(low)-D('.001'),'Low'),(D(low),'Medium'),(D(high)-D('.001'),'Medium'),(D(high),'High')]:
                self.assertEqual(rating(value,size),expected)
            self.assertEqual(rating(0,size,1),'High')

    def test_zero_denominators_caps_and_fraud(self):
        for movement in [True,False]:
            with self.assertRaises(ValueError): ratio(D(0),D(0),movement)
        with self.assertRaises(ValueError): normalized(D(1),1,0)
        self.assertEqual(normalized(D(500),0,0),0)
        self.assertEqual(normalized(D(1000),6,22),6)
        self.assertEqual(ratio(D(100),D(110)),0)
        self.assertEqual(ratio(D(100),D(90),True),0)

    def test_profit_boundaries(self):
        for value,expected in [(-50001,10),(-50000,5),(0,5),(1,3),(500000,3),(500001,0)]:
            self.assertEqual(profit(D(value)),expected)

if __name__ == '__main__': unittest.main()
