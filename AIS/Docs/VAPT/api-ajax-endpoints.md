# API/AJAX endpoints that must never return a view

The following controller actions are triggered via JavaScript or explicitly routed under `/api`. They should always return JSON (or other non-HTML payloads) and must not attempt to render a Razor view on validation, auth, or exception paths.

## ApiCallsController (API/AJAX)
- All POST actions in `ApiCallsController` are consumed by AJAX clients, including the observation management suite and legacy data operations.
- Explicit `/api/hd/observation/...` routes used by SBP UI flows:
  - `api/hd/observation/add`, `update`
  - `api/hd/observation/response/add`, `response/update`
  - `api/hd/observation/delete/request`, `response/delete/request`
  - `api/hd/observation/request/reverse`, `request/approve`, `request/reject`
  - `api/hd/observation/authenticate`, `api/hd/observation/password/update`
- Legacy AJAX methods such as `find_users`, `save_observations*`, `update_observation_*`, and the numerous `get_*` fetchers are expected to return JSON/primitive payloads only.

## AdministrationPanelController (AJAX from manage_user)
- `menu_pages`, `assigned_menu_pages`, `add_group_item_assignment`, `group_add`
- `find_users` and `update_user` invoked by `Views/AdministrationPanel/manage_user.cshtml` via jQuery.

## UploadFileController (file uploads via AJAX)
- All `[HttpPost]` upload actions (e.g., `UploadAttachment`, `UploadGeneral`, `UploadObservationFile`, `UploadObservationDraft`, `UploadFile`, `UploadFileForParas`) are used by front-end scripts to send files asynchronously and expect JSON or plain-text results.

## HMController AJAX helpers
- Although most HM actions render pages, the `SbpObservationRegister` JavaScript uses the `/api/hd/observation/...` endpoints above for all asynchronous operations.
