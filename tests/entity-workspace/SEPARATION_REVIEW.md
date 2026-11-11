# Entity workspace correction review (PR #381)

## Included application corrections
- AdministrationPanelController.cs: removed workspace link construction and both workspace ViewData assignments; the original dashboard uses only its authorized model and visible steps.
- Entity_Dashboard.cshtml: removed the extra administration panel; original stepper, eight step definitions, partials and legacy JavaScript are unchanged.
- AdministrationPanelController.EntityWorkspace.cs: new workspace model permits zero authorized legacy operations; workspace page authorization resolves ID 467 and checks explicit cached grants. Legacy operations still require access to the original dashboard's **effective deployed catalog page ID** plus their own operation grants. No blanket access was added. Optional links use TryResolvePageId, eliminating absent-path warning sources without suppressing real authorization failures. Workspace data permission failures return HTTP 403 JSON; missing authentication/session returns HTTP 401.
- PageIdResolver.cs: retains the existing code fallback for the old dashboard (970202) and adds the new Entity_Workspace code fallback (467).
- Program.cs and route-separation.test.cjs: regression checks for route isolation, separate model without dashboard/operation grants, reader denial, and distinct effective original/workspace page registration.

## Explicitly excluded from PR #381
- **AIS/wwwroot/Images/Page ID.xlsx** is unchanged from main, by request.
- The prior draft of bd16dfa7 edited workbook sheet1.xml: it added workspace 467 and changed the old dashboard's existing Excel entry from 414 to 970202. **Neither workbook edit is included in this PR.**
- The active PageIdResolver loads Page ID.xlsx before code fallback; existing workbook entries win. Consequently, the unchanged workbook may still map the old dashboard to **414**, not fallback 970202. Test output reports the effective legacy ID rather than incorrectly claiming that the workbook was migrated.
- Code fallback 467 applies only when the workbook has no conflicting workspace route. Verify the actual deployed workbook and the exact 467 grants/menu registration before activating the new workspace. Do not silently change the workbook or broaden permissions to make it open.
- Verify the original dashboard retains its intended effective ID and grants under the separately managed page catalog. If a catalog migration to 970202 is required, handle it through a separately approved deployment task.

## Verification and release
- Codex reported a prior .NET build (0 errors, 57 warnings), 23 passing Node tests and C# regression checks on the version **including workbook modifications**. Those results do not establish success after excluding the workbook and changing the catalog-dependent assertion.
- No authenticated Oracle/browser test, screenshot comparison or two-tab isolation test has been executed in this environment. Run the .NET build, Node tests and C# regression harness again after checkout, then confirm both URLs and all eight legacy operations in staging.
- No Oracle packages, legacy operation readers, business partials, engagement reversal/shifting APIs, database data or permission grants have been changed.
- The branch was brought forward from the latest main commit through a content-preserving merge commit, so PR #381 contains only the authorized seven text-file changes and excludes the workbook.

## Production permissions
The new workspace's code expects PAGE_ID 467 with explicit user/role grants. Unscoped Oracle readers are additionally Super User-only. Access to page 467 does not grant legacy operations. The old dashboard's actual page authorization continues to depend on the effective, unchanged deployment catalog; **do not claim PAGE_ID 970202 is effective merely because the code fallback lists it**.
