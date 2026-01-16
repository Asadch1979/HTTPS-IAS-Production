using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
    {
    public abstract class BaseController : Controller
        {

        protected List<Dictionary<string, object>> ConvertDataTable(DataTable dt)
            {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dt.Rows)
                {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    {
                    row[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
                    }
                rows.Add(row);
                }

            return rows;
            }
        protected BaseController(SessionHandler sessionHandler)
            {
            SessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            }

        protected SessionHandler SessionHandler { get; }

        protected (SessionUser user, IActionResult errorResult) GetUserOr401()
            {
            if (SessionHandler.TryGetUser(out var user))
                {
                return (user, null);
                }

            return (null, Unauthorized(new { message = "Session expired" }));
            }



        protected (SessionUser user, IActionResult redirectResult) GetUserOrRedirect(string action = "Index", string controller = "Login")
            {
            if (SessionHandler.TryGetUser(out var user))
                {
                return (user, null);
                }

            return (null, RedirectToAction(action, controller));
            }

        }
    }
