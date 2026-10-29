# IAS User Context Redesign

## 1. Current design limitation

IAS currently authenticates a user by reading one row that already contains:

- one `ROLE_ID` / `GROUP_ID` from `T_USER_MAPING`
- one `ENTITY_ID` from `T_USER`

That design creates three hard limits:

1. `T_USER` can hold only one current entity.
2. `T_USER_MAPING` can hold only one effective role per login flow.
3. The application stores `UserRoleID` and `UserEntityID` directly into session during login, so there is no place to choose among multiple valid contexts before the home page loads.

The downstream IAS flow is deeply session-bound. The following areas already assume exactly one active role and one active entity per session:

- `AIS/SessionHandler.cs`
- `AIS/Models/SessionUser.cs`
- `AIS/DBConnection.cs` permission and menu calls
- `AIS/Services/PermissionService.cs`
- `AIS/Controllers/HomeController.cs`
- all DB calls that pass `ENT_ID`, `P_NO`, and `R_ID`

Because of that, the safest redesign is not to make downstream pages multi-context aware. The safe redesign is:

- master assignment model: one user can have many allowed contexts
- active runtime model: one selected role + one selected entity remain active for the current session

## 2. Target model

### Chosen data model

The recommended production model is a **combined user-role-entity assignment table**:

- `T_USER_CONTEXT_ASSIGNMENT`

This is preferable to separate `user-role` and `user-entity` tables because IAS does not actually authorize a free cartesian product of every role with every posting. IAS needs **valid role + entity combinations** that reflect postings, additional charges, and reporting relationships. A combined table prevents invalid runtime combinations and keeps login selection clean.

### Core operating principle

One user may have many active records in `T_USER_CONTEXT_ASSIGNMENT`, but at login the user selects only one assignment for the session. After selection:

- session keeps one `UserRoleID`
- session keeps one `UserEntityID`
- session also stores `UserContextAssignmentId`
- the rest of IAS continues to read the same active session fields it uses today

## 3. Impact assessment

### Database impact

- Add `T_USER_CONTEXT_ASSIGNMENT` as the new source of truth.
- Add `USER_CONTEXT_ID` to `T_USER_SESSION`.
- Keep `T_USER.ENTITY_ID` and `T_USER_MAPING` as **legacy compatibility mirrors** of the selected default context.
- Provide migration/backfill from current single-role/single-entity data.

### Authentication and session impact

- Credentials are validated first against base user data only.
- Valid contexts are loaded next.
- If exactly one context exists, it is auto-selected and the current IAS login flow continues.
- If multiple contexts exist, the user is redirected to a lightweight selection page.
- Only after a context is chosen do we create the normal IAS session and auth cookie.

### Authorization impact

No broad permission rewrite is required. Existing permission and menu procedures continue to receive:

- `ENT_ID`
- `P_NO`
- `R_ID`

Those values are still sourced from session after context selection.

### Admin maintenance impact

The current admin screen is single-assignment. It has been revised to support:

- multiple assignments per user
- one default assignment
- enable/disable per assignment
- add/remove assignment rows before save

## 4. Application files changed

### Login and session flow

- `AIS/Controllers/LoginController.cs`
- `AIS/DBConnection.cs`
- `AIS/SessionHandler.cs`
- `AIS/Session/SessionKeys.cs`
- `AIS/Models/UserModel.cs`
- `AIS/Models/SessionUser.cs`
- `AIS/Models/PendingLoginContextState.cs`
- `AIS/Models/ContextSelectionViewModel.cs`
- `AIS/Models/UserContextAssignmentModel.cs`
- `AIS/Models/Requests/PostModels.cs`
- `AIS/Views/Login/SelectContext.cshtml`
- `AIS/wwwroot/js/login.js`

### Admin maintenance

- `AIS/Controllers/AdministrationPanelController.cs`
- `AIS/Controllers/ApiCallsController.cs`
- `AIS/DBConnection.AD.cs`
- `AIS/Views/AdministrationPanel/manage_user.cshtml`
- `AIS/Views/AdministrationPanel/DashboardPartials/_ManageUser.cshtml`
- `AIS/wwwroot/js/csp/Views_AdministrationPanel_manage_user.js`

## 5. Backward compatibility handling

Backward compatibility is preserved by design:

1. `T_USER_CONTEXT_ASSIGNMENT` becomes the master assignment table.
2. `PKG_USER_CONTEXT.P_SYNC_USER_DEFAULT_CONTEXT` mirrors the chosen default assignment back into:
   - `T_USER.ENTITY_ID`
   - `T_USER_MAPING.GROUP_ID / ROLE_ID`
3. Session still stores one active role and one active entity.
4. Existing menu, page, and API permission procedures continue to read the same session values.
5. Existing pages do not need to understand multiple contexts.

This gives IAS multi-role and multi-entity capability without forcing a rewrite of existing business modules.

## 6. Migration notes

### Existing users

- Each existing active user is backfilled into `T_USER_CONTEXT_ASSIGNMENT` using the current `T_USER.ENTITY_ID` + `T_USER_MAPING.GROUP_ID / ROLE_ID`.
- Backfilled rows are marked:
  - active = `Y`
  - default = `Y`

### Legacy code safety

- Old package procedures such as `UPDATE_USERS` and `P_add_new_user` should remain as wrappers that save a single default assignment through the new API.
- Old reports or background jobs that still read `T_USER.ENTITY_ID` or `T_USER_MAPING` continue to work because the compatibility sync keeps those fields current.

### Operational rollout

Recommended rollout sequence:

1. Deploy table and package changes.
2. Run data backfill into `T_USER_CONTEXT_ASSIGNMENT`.
3. Deploy the application changes.
4. Validate login with:
   - single-context user
   - multi-context user
   - inactive-context user
   - password-reset user
5. Validate admin maintenance save and search behavior.

## 7. Verification status

The .NET solution builds successfully after the application-layer changes:

- `dotnet build AIS.sln`

Oracle package deployment was not executed from this workspace, so the SQL artifact should be reviewed and applied in the target schema with DBA change control.
