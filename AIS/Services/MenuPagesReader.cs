using System;
using System.Collections.Generic;
using System.Data;
using AIS.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace AIS.Services
    {
    public class MenuPagesReader : IMenuPagesReader
        {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MenuPagesReader> _logger;

        public MenuPagesReader(IConfiguration configuration, ILogger<MenuPagesReader> logger)
            {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public List<MenuPagesModel> GetActiveMenuPages()
            {
            var menuPages = new List<MenuPagesModel>();

            using var connection = new OracleConnection(BuildConnectionString());
            connection.Open();

            using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandText = "pkg_ais.p_GetAllMenuPage";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Clear();

            // Match your package signature
            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = command.ExecuteReader();

            var hasId = HasColumn(reader, "ID");
            var hasStatus = HasColumn(reader, "STATUS");
            var hasPagePath = HasColumn(reader, "PAGE_PATH");
            var hasPageUrl = HasColumn(reader, "PAGE_URL");

            if (!hasId)
                _logger.LogWarning("p_GetAllMenuPages cursor is missing ID column.");

            if (!hasPagePath && !hasPageUrl)
                {
                _logger.LogWarning("p_GetAllMenuPages cursor is missing PAGE_PATH and PAGE_URL columns.");
                }
            else if (!hasPagePath && hasPageUrl)
                {
                _logger.LogWarning("p_GetAllMenuPages cursor is missing PAGE_PATH; falling back to PAGE_URL.");
                }

            while (reader.Read())
                {
                if (!hasId || reader["ID"] == DBNull.Value)
                    continue;

                // Filter active pages if STATUS is provided by cursor; otherwise keep all and filter by your Excel resolver later
                if (hasStatus && reader["STATUS"] != DBNull.Value)
                    {
                    var status = reader["STATUS"]?.ToString();
                    if (!string.Equals(status, "A", StringComparison.OrdinalIgnoreCase))
                        continue;
                    }

                var pagePath = GetPagePath(reader, hasPagePath, hasPageUrl);
                if (string.IsNullOrWhiteSpace(pagePath))
                    continue;

                menuPages.Add(new MenuPagesModel
                    {
                    PageId = Convert.ToInt32(reader["ID"]),
                    Page_Path = pagePath
                    });
                }

            return menuPages;
            }


        private string BuildConnectionString()
            {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder
                {
                Password = _configuration["ConnectionStrings:DBUserPassword"],
                UserID = _configuration["ConnectionStrings:DBUserName"],
                DataSource = _configuration["ConnectionStrings:DBDataSource"],
                IncrPoolSize = 5,
                MaxPoolSize = 5000,
                MinPoolSize = 1,
                Pooling = true,
                ConnectionTimeout = 3540
                };

            return builder.ConnectionString;
            }

        private static bool HasColumn(IDataRecord reader, string columnName)
            {
            for (var i = 0; i < reader.FieldCount; i++)
                {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    {
                    return true;
                    }
                }

            return false;
            }

        private static string GetPagePath(IDataRecord reader, bool hasPagePath, bool hasPageUrl)
            {
            if (hasPagePath)
                {
                return reader["PAGE_PATH"]?.ToString();
                }

            if (hasPageUrl)
                {
                return reader["PAGE_URL"]?.ToString();
                }

            return null;
            }
        }
    }
