/*
Phase 1 database recipient contract (deployment action required)
================================================================

The checked-in PKG_SM body does not currently satisfy the application cursor
contract for these two procedures:

  PKG_SM.P_ADD_SAMPLE_DATA
  PKG_SM.P_ADD_EXCEPTION_DATA

Both output cursors must return exactly one outcome row containing:

  REMARKS   VARCHAR2 -- 'N' when generation failed; existing success value otherwise
  EMAIL     VARCHAR2 -- semicolon/comma-separated primary recipients
  EMAIL_CC  VARCHAR2 -- semicolon/comma-separated copy recipients; may be NULL

The recipient values must be selected from the approved database-owned
notification-recipient source for Sampling / Exception Monitoring. They must
not be hardcoded in the application or package.

P_ADD_SAMPLE_DATA currently reassigns IO_CURSOR in P_CREATE_SAMPLE, and
P_ADD_EXCEPTION_DATA currently returns generated exception data rows. The DBA
must preserve those data-return requirements through separate cursors or
procedures if they are still consumed elsewhere, while making the cursor used
by CreateSampleDataAfterEngagementApproval and
CreateExceptionDataAfterEngagementApproval conform to the outcome contract
above.

Until the deployed package returns REMARKS='N' plus EMAIL/EMAIL_CC, the two
failure notification triggers remain blocked. The application reader now
safely consumes these fields when present and does not inject fallback
addresses.
*/
