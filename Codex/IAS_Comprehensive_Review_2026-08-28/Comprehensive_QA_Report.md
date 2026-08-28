# Comprehensive IAS QA Report (In Progress)

## Scope completed

- Live login/context/home verification for all 35 supplied roles.
- Top-level module visibility captured for all roles.
- Super User quick-link baseline captured.
- Post Compliance > Monitoring of Settled Paras tested for role 45 using month/year filters.
- Global authentication, session, page/API permission, security-header and object-scope code reviewed initially.

## Confirmed results

- 34 accounts reach the IAS home page.
- Employee Para Check authenticates but is denied because no valid role/entity context exists.
- Module sets vary by role and are recorded in `Role_Permission_Matrix.csv`.
- Clicking sidebar destinations and Logout failed to navigate during repeated browser tests; direct navigation works.
- Monitoring of Settled Paras returned one August 2026 record, proving the filter/data path works, but the record lacked a Para No.
- The monitoring table has serious responsive/readability problems at the tested browser width.

## Coverage warning

Rows marked `BROWSER-BASELINE` prove only login and top-level visibility. They do not yet prove page access, CRUD authority, field validation, workflow authority, entity scope, API scope, or Oracle business-rule enforcement.

Detailed defects are maintained in `Defect_Register.csv`; enhancements are kept separately in `Improvement_Register.csv`.

