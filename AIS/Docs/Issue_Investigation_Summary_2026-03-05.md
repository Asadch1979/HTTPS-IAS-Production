# Investigation Summary: HM + PostCompliance Production Issues

Date: 2026-03-05

## 1) `HM/old_paras_monitoring_ppno` shows `_Layout.cshtml` HTML in alert

### Root cause identified
`POST /ApiCalls/get_user_name` was being denied by centralized API permission guard for non-super users, which returns HTTP 403 for denied API paths. This is enforced in `PageKeyPermissionGuard.TryAuthorize()`.

In affected sessions, the frontend still executed dependent flow and surfaced error content in legacy alert handling, which made HTML/error responses visible to users.

### Fix implemented
Added `/ApiCalls/get_user_name` to API-permission exemption list (GET/POST), same pattern used by existing global utility APIs.

This prevents false 403s for the employee-name lookup used by `old_paras_monitoring_ppno`.

---

## 2) `PostCompliance/monitoring_of_para_settlement` "View Para Text" button not working

### Root cause identified
The page relied on `data-onclick` / `data-onchange` delegated inline handler execution, which uses `new Function(...)` in `delegated-inline-handlers.js`.

That pattern is CSP-fragile and will fail under enforced CSP (`unsafe-eval` blocked), causing click actions like **View Para Text** to no-op.

### Fix implemented
Reworked this page to avoid string-evaluated handlers for key interactions:

- Replaced generated `data-onclick` links with class-based links + data attributes.
- Bound handlers via jQuery delegated `$(document).on(...)` listeners.
- Removed `data-onchange` from entity dropdown and bound regular change event in script.
- Replaced save-comments `data-onclick` with ID-based click binding.

This makes the page behavior independent of eval-based delegated inline handler execution.

---

## 3) `HM/ManageSbpPassword` redirects to app root

### Root cause identified
Action `HMController.ManageSbpPassword` redirected to `Home/Index` whenever SBP access session flag was missing (`sessionHandler.HasSbpAccess()`), even for authenticated users with page permission.

That appeared as immediate redirect to `/ZTBLAIS` in production.

### Fix implemented
Removed the SBP-access gate from `ManageSbpPassword` action while keeping authentication and page-permission checks intact.

This allows authorized users to open the password management page directly.

---

## Notes
- CSP report endpoint `404` is still a configuration/deployment concern and not required for feature correctness.
- Existing app-wide delegated inline handler mechanism still contains `new Function`; only the targeted broken flow was migrated in this change.
