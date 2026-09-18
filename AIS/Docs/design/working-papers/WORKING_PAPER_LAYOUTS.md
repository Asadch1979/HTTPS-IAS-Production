# Detailed Layouts for the Five Working Papers

## 1. Shared page composition

Every paper uses the workspace shell defined in `UNIFIED_ARCHITECTURE.md` and the same tabs: Plan, Test Sheet, Evidence, Exceptions, Conclusion, Review, and History. The tables below define specialized content. Fields marked **Calculated** are server-derived and read-only. Fields marked **Imported** may be sourced from an approved system extract but remain traceable to the import batch.

Common fields on every sample item are item ID, selection source/row, selected-by/date, procedure result summary, evidence count, exception count, preparer comment, last editor/time and version.

## 2. Loan Case File

**Default Test Sheet grid:** Sample ID | Loan/facility | Product | Currency | Outstanding | Classification | Arrears | Security coverage | Provision variance | Result | Exceptions | Evidence.

Selecting a row opens an item workspace with Overview, Approval, Documentation, Security, Performance, Classification, Procedure Results and Links sections. Financial and classification comparisons remain visible in a compact summary rail while the auditor works through the sections.

### 2.1 Objective, risks and controls

**Objectives:** Determine whether sampled facilities were properly approved, documented, disbursed, secured, classified, monitored and provided for in accordance with policy and applicable regulation; confirm balances and status at the audit cut-off.

**Principal risks:** unauthorized or excess lending; incomplete documentation; incorrect classification/provision; inadequate or expired security; diversion of funds; inaccurate balances; related-party or policy-limit breaches; unrecognized impairment.

**Expected controls:** maker-checker approval, delegated authority limits, pre-disbursement checklist, security perfection/valuation monitoring, core-system controls, periodic classification review, arrears monitoring and provisioning review.

### 2.2 Planning and population

| Field | Rule |
|---|---|
| Portfolio/product scope | Required controlled values plus scope narrative |
| Population source and cut-off | Core/report name, extraction timestamp, owner, reconciliation evidence |
| Population count/value | Required; value includes currency |
| Risk strata | Product, amount band, classification, arrears, related party, exception flags |
| Sampling | Method, size, value coverage and rationale; permit targeted plus representative selections |

### 2.3 Test-sheet fields

| Group | Fields |
|---|---|
| Identity | Loan/facility number; masked customer ID/name; branch; product/facility type; currency |
| Financial | Sanction limit; outstanding principal; accrued markup; disbursement amount/date; maturity date; overdue days |
| Approval | Approval authority/level; approval date; approved limit; deviations; policy/board reference |
| Documentation | Application/agreement; KYC/CDD reliance reference; purpose; pre-disbursement checklist; mandatory documents |
| Security | Security type; required/held value; valuation date/valuer; perfection/charge status; insurance/expiry |
| Performance | Repayment status; days past due; rescheduling/restructuring; account conduct |
| Classification | System classification; auditor-expected classification; system provision; expected provision; variance **Calculated** |
| Related controls | Related-party flag; concentration/limit result; end-use verification; monitoring review |
| Result | Procedure results; item conclusion; evidence; exception/observation links |

Money uses decimal and currency; dates use date/time types; status fields use controlled values. Sensitive customer values are masked in grid views.

### 2.4 Minimum procedures

1. Agree facility identity and balance to the approved population/core record.
2. Verify approval within delegated authority and compliance with sanctioned terms.
3. Inspect executed agreements and mandatory pre-disbursement documents.
4. Verify security existence, perfection, valuation currency, insurance and margin/coverage.
5. Test disbursement authorization, timing, amount and end use.
6. Review repayment conduct, arrears and restructuring history.
7. Reperform classification and provision under approved policy/regulation.
8. Test related-party, concentration and policy exceptions where applicable.

### 2.5 Calculations, evidence and exceptions

- `undrawn/excess = approved limit - relevant exposure`, with excess flagged according to product rules.
- `security shortfall = required security value - eligible held value` where positive.
- `provision variance = expected provision - recorded provision`.
- Coverage shows sample count/value as a percentage of population count/value.
- Required evidence includes sanction/approval, agreement/checklist, security/valuation, account statement, classification/provision support and any deviation approval.
- Exceptions must distinguish documentation, approval, security, performance, classification/provision and policy-limit failures.

### 2.6 Conclusion view

Show sample and value coverage, exception count/value by risk, classification upgrades/downgrades, provision shortfall/excess, security shortfall, unresolved documentation and objective-achievement assessment.

## 3. Voucher Checking

**Default Test Sheet grid:** Sample ID | Voucher | Posting date | Type | Amount/currency | Debit/Credit account | Payee | Approval | Support | Cut-off | Result | Exceptions | Evidence.

Selecting a row opens Transaction, Authorization, Supporting Documents, Accounting/Tax, Duplicate and Cut-off Tests, Procedure Results and Links sections. The source transaction and auditor test result are displayed side by side where a comparison is required.

### 3.1 Objective, risks and controls

**Objectives:** Confirm sampled transactions are genuine, accurately recorded, properly approved, supported, correctly classified, compliant with policy/tax requirements, and posted in the correct period.

**Principal risks:** fictitious/duplicate payment, unauthorized transaction, wrong account/cost center, inadequate support, split transaction, conflict of interest, tax error, cut-off error and management override.

**Expected controls:** sequential voucher control, role segregation, approval limits, supporting-document checks, duplicate detection, vendor master controls, tax validation, posting controls and period close.

### 3.2 Planning and population

Population metadata includes journal/subledger source, period, voucher count/value, debit/credit reconciliation, extraction criteria and reconciliation to GL. Sampling supports random, monetary-unit, high-value, unusual account, manual journal, weekend/late posting, related-party, round-value and duplicate-risk strata.

### 3.3 Test-sheet fields

| Group | Fields |
|---|---|
| Identity | Voucher number; transaction/posting date; voucher type; source system; batch/journal ID |
| Accounting | Amount/currency; debit account; credit account; cost center/branch; description; accounting period |
| Counterparty | Payee/vendor ID and masked name; bank/payment reference; related-party indicator |
| Preparation/approval | Preparer; authorizer; approval date/time; required/actual authority level; segregation result |
| Support | Invoice/reference/date; purchase order/contract; goods/service receipt; business purpose; document completeness |
| Compliance | Tax treatment; withholding amount; budget availability; policy/procurement compliance; duplicate check |
| Posting tests | Account classification; mathematical accuracy; period cut-off; reversal/adjustment; manual override |
| Result | Procedure results; item conclusion; evidence; exception/observation links |

### 3.4 Minimum procedures

1. Agree voucher to source population and GL posting.
2. Inspect authorization and compare amount to delegated limits.
3. Verify preparer/approver segregation and override history.
4. Inspect invoice, contract/order, receipt and business-purpose support.
5. Reperform arithmetic, tax/withholding and account/cost-center classification.
6. Test posting and service/delivery dates for cut-off.
7. Search for duplicate identifiers/amounts and split transactions.
8. Evaluate related-party, unusual, manual and policy-deviation indicators.

### 3.5 Calculations, evidence and exceptions

- Recalculate voucher/invoice/tax totals and variance.
- Flag duplicate keys and related transactions for auditor disposition; automated flags are not conclusions.
- Compare required and actual authority based on effective approval matrix date.
- Required evidence includes voucher image/export, approval trail, invoice/support, receipt/delivery, relevant contract/order and tax calculation.
- Exceptions use categories: authorization, support, duplicate, classification, cut-off, tax, procurement, segregation, override.

### 3.6 Conclusion view

Show count/value coverage, exception rate/value by category, duplicate/split indicators resolved, authority breaches, unsupported value, cut-off errors, projected error if methodology permits, and objective assessment.

## 4. Account Opening

**Default Test Sheet grid:** Sample ID | Masked account/customer | Open date | Customer type | Account type | Risk rating | Screening | Documents | Approval | KYC review due | Result | Exceptions.

Selecting a row opens Customer Identity, Legal Person/Beneficial Owner, AML/CFT Screening, Risk/EDD, Documents/Mandate, Approval, Funding/Profile, Procedure Results and Links sections. Sensitive identifiers are revealed only through a separately authorized action and are masked again when the item closes.

### 4.1 Objective, risks and controls

**Objectives:** Confirm sampled accounts were opened by authorized personnel for verified customers, with complete KYC/CDD, sanctions/PEP screening, risk assessment, approvals and mandatory documentation appropriate to customer/account type.

**Principal risks:** fictitious/anonymous account, identity theft, money laundering/terrorist financing exposure, sanctions/PEP breach, undisclosed beneficial ownership, missing mandate, incorrect risk rating, unauthorized opening and privacy breach.

**Expected controls:** customer identification, beneficial-owner verification, screening, risk scoring, enhanced due diligence, account-type checklist, independent approval, document validation and periodic KYC review.

### 4.2 Planning and population

Population includes accounts opened/reactivated during scope, count by customer/account type and risk rating, source/extraction date, reconciliation, and excluded closed/test records. Sampling oversamples high-risk, PEP, non-resident, legal-person, remote/digital, reactivated, override and incomplete-document indicators.

### 4.3 Test-sheet fields

| Group | Fields |
|---|---|
| Identity | Masked account/customer number; branch/channel; open date; account/product type; customer type; status |
| Customer verification | ID type/masked number/expiry; address/contact verification; customer presence/biometric result where applicable |
| Legal persons | Registration/tax reference; legal form; directors/controllers; beneficial owners and verification status |
| AML/CFT | Customer risk rating; expected activity/source of funds; sanctions result/date/reference; PEP/adverse-media result; EDD required/completed |
| Documents | Account form; specimen/mandate; terms acceptance; tax declarations; type-specific mandatory-document checklist |
| Approval | Initiator; approver; approval date; required/actual authority; override/deviation and approval |
| Ongoing controls | Initial deposit/funding source; transaction profile; KYC review due date; restrictions applied pending documents |
| Result | Procedure results; item conclusion; evidence; exception/observation links |

### 4.4 Minimum procedures

1. Agree account to approved population and source-system record.
2. Verify identity/address and, for legal persons, existence/control/beneficial ownership.
3. Inspect sanctions, PEP and required adverse-media screening at opening.
4. Reperform or assess risk rating and EDD trigger treatment.
5. Verify required forms, mandates, declarations and account-type documents.
6. Verify independent approval, authority and override handling.
7. Review source of funds/initial funding and expected activity consistency.
8. Confirm missing documents caused required restrictions and timely remediation.

### 4.5 Calculations, evidence and exceptions

- Document completeness is calculated from the effective account-type checklist.
- KYC review due date derives from opening/review date and risk-based frequency.
- Screening age and expired-document flags derive from relevant dates.
- Required evidence includes account form, identity/registration evidence, beneficial-owner support, screening result, risk assessment/EDD, mandate and approval trail.
- Sensitive evidence requires stricter access, masking and export controls.

### 4.6 Conclusion view

Show sample coverage by account/customer/risk type, incomplete KYC, screening/PEP exceptions, beneficial-owner gaps, approval breaches, expired items, unresolved restrictions and objective assessment. Never display full sensitive identifiers in summary output.

## 5. Fixed Assets

**Default Test Sheet grid:** Sample ID | Asset tag | Description | Class | Register location | Observed location | Exists | Condition | Net book value | Valuation variance | Result | Exceptions.

Selecting a row opens Register Data, Physical Inspection, Ownership/Support, Movement/Disposal, Valuation/Depreciation, Procedure Results and Links sections. A count-reconciliation panel summarizes register-to-floor and floor-to-register selections without mixing the two completeness assertions.

### 5.1 Objective, risks and controls

**Objectives:** Confirm sampled assets exist, belong to the entity, are accurately recorded and located, are safeguarded and appropriately valued/depreciated, and that additions/transfers/disposals are authorized.

**Principal risks:** fictitious/missing asset, unrecorded asset, theft, wrong location/custodian, incorrect capitalization/depreciation, unrecorded impairment/disposal and duplicate asset records.

**Expected controls:** unique tagging, periodic count, custodian records, transfer/disposal approval, register-to-GL reconciliation, capitalization policy, depreciation controls and impairment review.

### 5.2 Planning and population

Define register/GL source, count and gross/net book value, reconciliation status, locations, asset classes and cut-off. Use two-way sampling: register-to-floor for existence and floor-to-register for completeness. Include high-value, portable, fully depreciated-in-use, recent addition/transfer/disposal and prior-exception strata.

### 5.3 Test-sheet fields

| Group | Fields |
|---|---|
| Identity | Asset tag/register ID; description; serial number; class; acquisition date; status |
| Register | Recorded location; recorded custodian; cost; accumulated depreciation; net book value; useful life/method |
| Physical inspection | Inspection date/time; observed location; observed custodian; exists; tag agrees; condition; photo/evidence |
| Ownership/support | Invoice/title; capitalization date; business use; ownership status |
| Movement/disposal | Last transfer/date/approval; disposal status/date/proceeds/approval; register update status |
| Valuation | Auditor depreciation; impairment indicator/test; expected net book value; variance **Calculated** |
| Completeness | Floor selection indicator; register match; unmatched-item resolution |
| Result | Procedure results; item conclusion; evidence; exception/observation links |

### 5.4 Minimum procedures

1. Agree register sample to physical asset, tag, serial, location and custodian.
2. Select physical items and trace to the register for completeness.
3. Inspect condition, use, safeguarding and impairment indicators.
4. Verify acquisition, ownership and capitalization support.
5. Reperform depreciation and assess useful life/method.
6. Inspect transfer and disposal authorization and timely register/GL update.
7. Reconcile count results and register totals to GL/control account.

### 5.5 Calculations, evidence and exceptions

- `location mismatch` and `custodian mismatch` are derived comparisons.
- Auditor depreciation and expected net book value use effective methodology/formula version.
- `valuation variance = expected net book value - recorded net book value`.
- Reconciliation tracks register total, GL balance and difference.
- Required evidence includes register extract, count sheet/photo where policy allows, purchase/title support, transfer/disposal approval and depreciation/GL reconciliation.
- Exceptions distinguish missing, unrecorded, untagged, location/custodian, condition/impairment, valuation and unauthorized movement/disposal.

### 5.6 Conclusion view

Show existence/completeness sample coverage, missing/unrecorded counts and values, location/custodian mismatches, valuation/depreciation variance, impaired assets, register-to-GL difference and objective assessment.

## 6. Cash Count

**Default Count Sheet grid:** Currency | Denomination | Physical quantity | Physical amount | Register quantity | Register amount | Quantity variance | Amount variance | Recount | Exception.

The Cash Count page places the count context and attestations above the denomination grid, with Other Accountable Items and Timing Items as adjacent sub-tabs. A persistent reconciliation panel shows physical, register and ledger totals by currency/count point; the bottom action area holds witness attestation and workflow actions.

### 6.1 Objective, risks and controls

**Objectives:** Confirm cash and cash equivalents physically held at the specified location and cut-off agree with authorized register and ledger balances, are properly safeguarded, and were counted under witnessed dual control.

**Principal risks:** misappropriation, concealed shortage/overage, counterfeit or damaged notes, unrecorded transaction, cut-off manipulation, unauthorized access and ineffective dual control.

**Expected controls:** surprise independent count, dual custody, controlled keys/combinations, denomination records, register/ledger reconciliation, teller/vault limits, sealed packet control, counterfeit detection and prompt variance escalation.

### 6.2 Planning and count header

| Field | Rule |
|---|---|
| Location | Entity, branch, vault/till/night safe, count point; server-authorized |
| Timing | Count date/time, register cut-off, ledger cut-off, surprise/planned indicator |
| People | Custodian(s), auditor(s), independent witness(es), dual-control confirmation |
| Scope | Currencies, cash points, sealed packets, stamps/other accountable items, exclusions |
| Sources | Register/report ID and balance; GL account/report and balance; extraction timestamps |
| Safeguarding | Key/combination control, seals, access log, CCTV/alarm status where in scope |

### 6.3 Denomination sheet

| Field | Rule |
|---|---|
| Currency/denomination | Approved active master value; unique per count point and sheet |
| Physical quantity | Non-negative whole number; independently recounted quantity if triggered |
| Physical amount | `denomination x final physical quantity` **Calculated** |
| Register quantity | Imported or entered whole number with source reference |
| Register amount | `denomination x register quantity` **Calculated** |
| Quantity variance | `final physical quantity - register quantity` **Calculated** |
| Amount variance | `physical amount - register amount` **Calculated** |
| Note condition | Fit/unfit/damaged/suspected counterfeit quantities where applicable |
| Explanation | Required for any variance or recount; exception link required above tolerance |

Other accountable items use separate schedules so they are included exactly once. Sealed packets record seal number, stated amount, verification method and whether opened/recounted.

### 6.4 Minimum procedures

1. Secure the count area and capture the register/ledger cut-off before counting.
2. Confirm custodians, witnesses, dual control and key/seal status.
3. Count/recount each denomination and accountable item; document count method.
4. Reconcile physical totals by count point/currency to register.
5. Reconcile register total to GL and investigate timing items.
6. Inspect unfit/damaged/counterfeit notes and sealed items.
7. Test transactions immediately before/after cut-off where risk requires.
8. Obtain signed custodian/witness attestation and disposition every variance.

### 6.5 Reconciliation and rules

| Measure | Formula/control |
|---|---|
| Physical total | Sum denomination physical amounts plus included other-item schedules |
| Register total | Sum denomination register amounts plus included register schedules |
| Physical-to-register variance | Physical total minus register total |
| Register-to-ledger variance | Register total minus ledger balance, adjusted only by documented timing items |
| Physical-to-ledger variance | Physical total minus ledger balance |
| Recount trigger | Configured quantity/amount variance or auditor judgment; retain original and recount |
| Exception trigger | Any non-zero unexplained variance or configured control failure; zero-tolerance items always excepted |

Required evidence includes signed count sheet/attestation, register extract, GL extract, approved timing-item support, seal/key/access evidence where tested, and variance resolution. The conclusion cannot be signed while a variance lacks explanation/disposition.

### 6.6 Conclusion view

Show totals by currency/count point, all three reconciliations, shortages/overages, timing items, counterfeit/damaged/unfit items, safeguarding failures, unresolved exceptions, witness attestation and objective assessment.

## 7. Cross-paper completeness rules

Before preparer sign-off, every paper must have approved scope/population/sample metadata, at least one objective, results for every applicable procedure, required evidence or an explained limitation, an exception for each failed result, a disposition for each exception, a completed conclusion and no calculation/validation errors. Before reviewer sign-off, all required review notes must be closed and changes since preparer sign-off must be explicitly acknowledged through the controlled return/resubmission cycle.
