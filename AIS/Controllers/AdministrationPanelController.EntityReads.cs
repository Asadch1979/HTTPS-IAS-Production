using AIS.Exceptions;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;

namespace AIS.Controllers
{
    public partial class AdministrationPanelController
    {
        // This is a temporary, intentionally Super-User-only read boundary.
        // PKG_IAS_ENTITY_MGMT currently has no entity-scope arguments. Do not
        // allow non-Super-Users until the independently approved scope reader
        // validates the entire search result set BEFORE Oracle pagination.
        private IActionResult ReadEntityWorkspaceData(
            SessionUser user, string data, int? entityId, int? typeId,
            string query, string status, int offset, int pageSize)
        {
            Response.Headers["Cache-Control"] = "private, no-store, no-cache";
            Response.Headers["Pragma"] = "no-cache";

            if (!sessionHandler.IsSuperUser() ||
                !CanAccessEntityWorkspace(user))
                return StatusCode(403, new { error = "forbidden", message = "A separately approved entity-read scope is required for this user." });

            data = (data ?? string.Empty).Trim().ToLowerInvariant();
            if (data != "search" && data != "summary" && data != "types" && data != "auditby" &&
                data != "profile" && data != "closure" && data != "history" && data != "archive")
                return BadRequest(new { error = "invalid_request", message = "Unknown read operation." });

            if (data == "search")
            {
                status = (status ?? "ALL").Trim().ToUpperInvariant();
                if (offset < 0 || offset > 1000000 ||
                    pageSize < 1 || pageSize > 200 ||
                    (typeId.HasValue && typeId.Value <= 0) ||
                    (query != null && query.Length > 120) ||
                    (status != "ALL" && status != "ACTIVE" && status != "INACTIVE"))
                    return BadRequest(new { error = "invalid_request", message = "Invalid entity search parameters." });
            }
            else if (data != "summary" && data != "types" && data != "auditby" && (!entityId.HasValue || entityId.Value <= 0))
            {
                return BadRequest(new { error = "invalid_request", message = "Select a valid entity." });
            }

            try
            {
                if (data == "summary")
                    return Json(dBConnection.GetEntityManagementSummary());

                if (data == "types")
                {
                    // Existing approved read-only entity-type lookup.
                    // We do NOT reuse Manage Users page-43 selection endpoints.
                    var typePageId = ResolveWorkflowPageId("/AdministrationPanel/entity_addition");
                    if (typePageId <= 0)
                        return StatusCode(503, new { error = "type_reader_unavailable", message = "Entity Type reader has no approved page mapping." });
                    var rows = dBConnection.GetAuditEntityTypes(typePageId)
                        .Where(t => t.AUTID > 0)
                        .GroupBy(t => t.AUTID)
                        .Select(g => new { id = g.Key, name = g.First().ENTITYTYPEDESC })
                        .OrderBy(t => t.name)
                        .ToList();
                    return Json(new { rows });
                }

                if (data == "auditby")
                {
                    // The existing PKG_AD audit-office reader is used under the
                    // same Super-User dashboard gate, not Manage Users permissions.
                    var auditOffices = dBConnection.GetAuditBy()
                        .Where(e => e.AUTID > 0)
                        .GroupBy(e => e.AUTID)
                        .Select(g => new { id = g.Key, name = g.First().ENTITYTYPEDESC })
                        .OrderBy(e => e.name)
                        .ToList();
                    return Json(new { rows = auditOffices });
                }

                DBConnection.EntityReadResult result;
                switch (data)
                {
                    case "search":
                        result = dBConnection.SearchEntityManagement(typeId, query, status, offset, pageSize);
                        break;
                    case "profile":
                        result = dBConnection.GetEntityManagementProfile(entityId.Value);
                        break;
                    case "closure":
                        result = dBConnection.GetEntityManagementClosurePreview(entityId.Value);
                        break;
                    case "history":
                        result = dBConnection.GetEntityManagementChangeLog(entityId.Value);
                        break;
                    default:
                        result = dBConnection.GetEntityManagementParaArchive(entityId.Value);
                        break;
                }
                if ((data == "profile" || data == "closure") && result.Rows.Count == 0)
                    return NotFound(new { error = "entity_not_found", message = "Entity not found." });

                return Json(new { rows = result.Rows, totalRows = result.TotalRows, truncated = result.Truncated });
            }
            catch (DatabaseUnavailableException ex)
            {
                _logger.LogError(ex, "IAS Entity Management {Operation} database unavailable.", data);
                return StatusCode(503, new { error = "database_unavailable", message = "The Oracle reader is temporarily unavailable." });
            }
            catch (OracleException ex)
            {
                _logger.LogError(ex, "IAS Entity Management {Operation} Oracle execution failed.", data);
                return StatusCode(503, new { error = "oracle_reader_unavailable", message = "The configured Oracle reader could not execute. Check the application database connection and deployed package." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IAS Entity Management {Operation} reader failed.", data);
                return StatusCode(500, new { error = "entity_read_failed", message = "Unable to retrieve entity information." });
            }
        }
    }
}
