# IAS-ERR-01264 investigation

Reviewed local commit 09335c2 against local main. The exact runtime cause remains unconfirmed: no live Oracle metadata or protected incident exception was available. The HTTP 500 is not claimed resolved.

Index renders the independent partial; Url.Action generates the API URL with configured PathBase. JavaScript calls RegisterHierarchy, RiskAction checks permissions, and GetFadApprovedRegister binds one OUT RefCursor to PKG_FAD_RISK.P_GET_REGISTER. OracleDataAdapter.Fill executes and fetches; ConvertDataTable preserves nulls, then MVC serializes. Authorization and serialization exceptions outside the delegate reach the outer middleware.

Both package source units declare P_GET_REGISTER. Its fingerprint guard can raise ORA-20101. Its cursor reads V_FAD_RISK_REGISTER without weights. The register view retains pending rows. No available evidence confirms deployed package drift, invalid views, schema mismatch, source drift or incompatible cursor types. The prior README's categorical ORA-20101 statement is not proof of this incident's cause.

Confirmed corrections: RiskAction logs full exceptions, inner details, Oracle number, action, path, trace and existing reference. RegisterHierarchy returns a register-specific safe failure message. Caught 500 exceptions pass through request Items to the existing outer monitor instead of becoming synthetic status-only errors. Only that monitor persists the incident. AJAX logs reuse an existing reference or trace; the outer middleware logs trace alongside IAS reference. Existing classifications and security controls remain.

DBA action (not executed): retrieve protected details for IAS-ERR-01264, correlate a new request trace after application deployment, inspect actual session user/current schema/database/service and synonym resolution. Inspect ALL_PROCEDURES and ALL_ARGUMENTS for the OUT REF CURSOR signature; ALL_OBJECTS, ALL_ERRORS and ALL_DEPENDENCIES for package and source/register/fingerprint views. Compare deployed package source with the repository. Use the protected exception stack to distinguish execution, validation, fetching and serialization.

If drift is confirmed, deploy only affected definitions from 01_source_contract.sql and, if missing/stale, 03_source_fingerprints.sql. Merge the P_GET_REGISTER declaration/body from ../PKG_FAD_RISK.sql into the reviewed deployed package units, preserving live-only members. Oracle requires enclosing package replacement and DDL commits implicitly. Do not blindly replace live packages or reload staging. No mismatch is currently confirmed, so speculative replacement SQL was not introduced.

For ORA-20101 compare all 121 staging fingerprints and the approved source SHA; report genuine drift without bypassing validation. Confirm 4 Main KPIs, 9 Packages, 14 Processes, 37 Procedures and 121 Annexures, retaining descriptions and classifications.

BO25/BO26/BO35/BO36/BO37/BO38/GO16 have identified IAS records BO-25/BO-26/BO-35/BO-36/BO-37/BO-38/GO-16, IDs 116 through 122 respectively. They remain pending under the current exact normalized-code comparison until approved reconciliation. No aliases or operational IDs were changed.

Modified the two controller files, two error middlewares, SystemErrorMonitor and this report. No automated tests, builds, test-code changes or database operations performed. Unrelated working-tree changes excluded. Runtime resolution still requires evidence and controlled deployment.
