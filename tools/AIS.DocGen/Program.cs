using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ExcelDataReader;
using Oracle.ManagedDataAccess.Client;

namespace AIS.DocGen;

internal sealed class Program
    {
    private static int Main(string[] args)
        {
        var options = DocGenOptions.Parse(args);
        if (!options.IsValid)
            {
            Console.WriteLine("Usage: dotnet run --project tools/AIS.DocGen -- --connection \"<connection string>\" --out docs/AIS_System_Catalog.md");
            return 1;
            }

        var repoRoot = RepoLocator.FindRepoRoot();
        if (repoRoot == null)
            {
            Console.Error.WriteLine("Unable to locate repository root containing AIS.sln.");
            return 1;
            }

        var aisRoot = Path.Combine(repoRoot, "AIS");
        var outputPath = Path.GetFullPath(Path.Combine(repoRoot, options.OutputPath));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var pageIndex = PageIdIndexLoader.Load(Path.Combine(aisRoot, "wwwroot", "Images", "Page ID.xlsx"));
        var dbProcedureIndex = DbProcedureScanner.Scan(aisRoot);
        var jsIndex = JsEndpointScanner.Scan(Path.Combine(aisRoot, "wwwroot", "js"));
        var viewsIndex = ViewScanner.ScanViews(Path.Combine(aisRoot, "Views"), jsIndex);
        var actions = ControllerScanner.ScanControllers(Path.Combine(aisRoot, "Controllers"), viewsIndex, dbProcedureIndex);
        var securityControls = SecurityControlScanner.Scan(aisRoot);

        var metadataRoot = Path.Combine(repoRoot, "tools", "AIS.DocGen", "metadata");
        var roleDefinitions = RoleMetadataLoader.Load(Path.Combine(metadataRoot, "roles.json"));
        var workflowDefinitions = WorkflowMetadataLoader.Load(Path.Combine(metadataRoot, "workflows.json"));
        var capabilityDefinitions = CapabilityMetadataLoader.Load(Path.Combine(metadataRoot, "capabilities.json"));
        var securityCapabilityDefinitions = SecurityCapabilityMetadataLoader.Load(Path.Combine(metadataRoot, "security_capabilities.json"));

        var menuCatalog = MenuCatalogBuilder.Build(options.ConnectionString, pageIndex, aisRoot);
        var roleDiscovery = RoleDiscoveryBuilder.Build(options.ConnectionString, menuCatalog, pageIndex);
        var roleCoverage = RoleCoverageBuilder.Build(roleDefinitions, roleDiscovery, menuCatalog, pageIndex, actions);
        var workflowCoverage = WorkflowCoverageBuilder.Build(workflowDefinitions, actions);
        var capabilityCoverage = CapabilityCoverageBuilder.Build(capabilityDefinitions, actions, workflowCoverage.Catalog);
        var capabilityScores = CapabilityScoreBuilder.Build(capabilityCoverage, roleCoverage, workflowCoverage.Catalog, menuCatalog);
        var securityCapabilityReport = SecurityCapabilityReportBuilder.Build(securityCapabilityDefinitions, securityControls);
        var deploymentProfile = DeploymentProfileBuilder.Build(aisRoot, actions, dbProcedureIndex, jsIndex);
        var mermaidDiagrams = MermaidDiagramBuilder.Build(menuCatalog, roleCoverage, capabilityCoverage, workflowCoverage.Catalog);

        var markdown = MarkdownCatalogWriter.Write(new CatalogDocument(
            repoRoot,
            menuCatalog,
            pageIndex,
            actions,
            securityControls,
            roleCoverage,
            roleDiscovery,
            workflowCoverage,
            capabilityCoverage,
            capabilityScores,
            securityCapabilityReport,
            deploymentProfile,
            mermaidDiagrams));

        File.WriteAllText(outputPath, markdown, Encoding.UTF8);
        Console.WriteLine($"AIS System Catalog generated at {outputPath}");
        return 0;
        }
    }

internal sealed record DocGenOptions(string? ConnectionString, string OutputPath, bool IsValid)
    {
    public static DocGenOptions Parse(string[] args)
        {
        string? connection = null;
        string? output = null;

        for (var i = 0; i < args.Length; i++)
            {
            var arg = args[i];
            if (string.Equals(arg, "--connection", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                connection = args[++i];
                continue;
                }

            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                output = args[++i];
                continue;
                }
            }

        return new DocGenOptions(connection, output ?? string.Empty, !string.IsNullOrWhiteSpace(output));
        }
    }

internal static class RepoLocator
    {
    public static string? FindRepoRoot()
        {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
            {
            if (File.Exists(Path.Combine(current.FullName, "AIS.sln")))
                {
                return current.FullName;
                }

            current = current.Parent;
            }

        return null;
        }
    }

internal sealed record PageIdEntry(int PageId, string PagePath, string? PageName);

internal sealed record RoleDefinition(
    string Name,
    string Persona,
    string? Description,
    IReadOnlyList<string> FunctionalAreas,
    IReadOnlyList<int> PageIds);

internal sealed record RoleMetadataDocument(IReadOnlyList<RoleDefinition> Roles);

internal sealed record WorkflowDefinition(
    string Name,
    string? Description,
    IReadOnlyList<WorkflowStageDefinition> Stages);

internal sealed record WorkflowStageDefinition(
    string Name,
    string? Description,
    int Order,
    IReadOnlyList<string> Routes,
    IReadOnlyList<string> ApiEndpoints,
    IReadOnlyList<string> DbProcedures);

internal sealed record WorkflowMetadataDocument(IReadOnlyList<WorkflowDefinition> Workflows);

internal sealed record CapabilityDefinition(
    string Name,
    string? Summary,
    IReadOnlyList<FeatureDefinition> Features);

internal sealed record FeatureDefinition(
    string Name,
    string? Description,
    IReadOnlyList<string> Routes,
    IReadOnlyList<string> Controllers,
    IReadOnlyList<string> ApiEndpoints);

internal sealed record CapabilityMetadataDocument(IReadOnlyList<CapabilityDefinition> Capabilities);

internal sealed record SecurityCapabilityDefinition(
    string Name,
    string? Description,
    IReadOnlyList<SecurityControlMetadata> Controls);

internal sealed record SecurityControlMetadata(
    string Control,
    string? Risk,
    string? Purpose);

internal sealed record SecurityCapabilityMetadataDocument(IReadOnlyList<SecurityCapabilityDefinition> Capabilities);

internal sealed record RoleGroupEntry(string RoleId, string RoleName, string? Description, string? Status);

internal sealed record RoleMenuPageEntry(
    string RoleId,
    string RoleName,
    int MenuId,
    int PageId,
    string PageName,
    string PagePath,
    int? PageOrder,
    string? Status);

internal sealed record RoleDiscoveryCatalog(
    IReadOnlyList<RoleGroupEntry> Roles,
    IReadOnlyList<RoleMenuPageEntry> MenuPages,
    string? FailureReason);

internal sealed record WorkflowInferenceEntry(
    WorkflowCatalogEntry Workflow,
    double Confidence,
    IReadOnlyList<string> Evidence);

internal sealed record WorkflowCoverageResult(
    IReadOnlyList<WorkflowCatalogEntry> Catalog,
    IReadOnlyList<WorkflowInferenceEntry> Inferences,
    bool UsingInference,
    string? FallbackReason,
    double AverageConfidence);

internal sealed record CapabilityScoreEntry(
    string Name,
    int Score,
    string Priority,
    IReadOnlyList<string> Evidence);

internal sealed record MermaidDiagramCatalog(
    IReadOnlyList<string> WorkflowDiagrams,
    string RoleCapabilityDiagram,
    string SystemStructureDiagram);

internal static class PageIdIndexLoader
    {
    public static IReadOnlyList<PageIdEntry> Load(string filePath)
        {
        if (!File.Exists(filePath))
            {
            return Array.Empty<PageIdEntry>();
            }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                UseHeaderRow = true
                }
            });

        if (dataSet.Tables.Count == 0)
            {
            return Array.Empty<PageIdEntry>();
            }

        var table = dataSet.Tables[0];
        var columnMap = table.Columns.Cast<DataColumn>()
            .ToDictionary(column => column.ColumnName, column => column.ColumnName, StringComparer.OrdinalIgnoreCase);

        var entries = new List<PageIdEntry>();

        foreach (DataRow row in table.Rows)
            {
            var pageIdRaw = GetColumnValue(row, columnMap, "PAGE_ID", "Page Id", "PageId", "ID");
            if (!int.TryParse(pageIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pageId) || pageId <= 0)
                {
                continue;
                }

            var pagePath = NormalizePath(GetColumnValue(row, columnMap, "PAGE_PATH", "Page Path", "PagePath"));
            var pageName = GetColumnValue(row, columnMap, "PAGE_NAME", "Page Name", "PageName");

            entries.Add(new PageIdEntry(pageId, pagePath, string.IsNullOrWhiteSpace(pageName) ? null : pageName));
            }

        return entries;
        }

    private static string GetColumnValue(DataRow row, IDictionary<string, string> columnMap, params string[] candidates)
        {
        foreach (var candidate in candidates)
            {
            if (columnMap.TryGetValue(candidate, out var columnName))
                {
                var value = row[columnName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    {
                    return value.Trim();
                    }
                }
            }

        return string.Empty;
        }

    public static string NormalizePath(string? path)
        {
        if (string.IsNullOrWhiteSpace(path))
            {
            return string.Empty;
            }

        return path.Trim().Replace("~", string.Empty).Replace("\\", "/").TrimEnd('/');
        }
    }

internal static class RoleMetadataLoader
    {
    public static IReadOnlyList<RoleDefinition> Load(string path)
        {
        var document = MetadataReader.LoadJson<RoleMetadataDocument>(path);
        return document?.Roles?.Select(Normalize).ToList() ?? Array.Empty<RoleDefinition>().ToList();
        }

    private static RoleDefinition Normalize(RoleDefinition role)
        {
        return new RoleDefinition(
            role.Name ?? string.Empty,
            role.Persona ?? string.Empty,
            role.Description,
            role.FunctionalAreas?.ToList() ?? Array.Empty<string>().ToList(),
            role.PageIds?.ToList() ?? Array.Empty<int>().ToList());
        }
    }

internal static class WorkflowMetadataLoader
    {
    public static IReadOnlyList<WorkflowDefinition> Load(string path)
        {
        var document = MetadataReader.LoadJson<WorkflowMetadataDocument>(path);
        return document?.Workflows?.Select(Normalize).ToList() ?? Array.Empty<WorkflowDefinition>().ToList();
        }

    private static WorkflowDefinition Normalize(WorkflowDefinition workflow)
        {
        var stages = workflow.Stages?.Select(stage => new WorkflowStageDefinition(
            stage.Name ?? string.Empty,
            stage.Description,
            stage.Order,
            stage.Routes?.ToList() ?? Array.Empty<string>().ToList(),
            stage.ApiEndpoints?.ToList() ?? Array.Empty<string>().ToList(),
            stage.DbProcedures?.ToList() ?? Array.Empty<string>().ToList())).ToList() ?? new List<WorkflowStageDefinition>();

        return new WorkflowDefinition(
            workflow.Name ?? string.Empty,
            workflow.Description,
            stages);
        }
    }

internal static class CapabilityMetadataLoader
    {
    public static IReadOnlyList<CapabilityDefinition> Load(string path)
        {
        var document = MetadataReader.LoadJson<CapabilityMetadataDocument>(path);
        return document?.Capabilities?.Select(Normalize).ToList() ?? Array.Empty<CapabilityDefinition>().ToList();
        }

    private static CapabilityDefinition Normalize(CapabilityDefinition capability)
        {
        var features = capability.Features?.Select(feature => new FeatureDefinition(
            feature.Name ?? string.Empty,
            feature.Description,
            feature.Routes?.ToList() ?? Array.Empty<string>().ToList(),
            feature.Controllers?.ToList() ?? Array.Empty<string>().ToList(),
            feature.ApiEndpoints?.ToList() ?? Array.Empty<string>().ToList())).ToList() ?? new List<FeatureDefinition>();

        return new CapabilityDefinition(
            capability.Name ?? string.Empty,
            capability.Summary,
            features);
        }
    }

internal static class SecurityCapabilityMetadataLoader
    {
    public static IReadOnlyList<SecurityCapabilityDefinition> Load(string path)
        {
        var document = MetadataReader.LoadJson<SecurityCapabilityMetadataDocument>(path);
        return document?.Capabilities?.Select(Normalize).ToList() ?? Array.Empty<SecurityCapabilityDefinition>().ToList();
        }

    private static SecurityCapabilityDefinition Normalize(SecurityCapabilityDefinition capability)
        {
        var controls = capability.Controls?.Select(control => new SecurityControlMetadata(
            control.Control ?? string.Empty,
            control.Risk,
            control.Purpose)).ToList() ?? new List<SecurityControlMetadata>();

        return new SecurityCapabilityDefinition(
            capability.Name ?? string.Empty,
            capability.Description,
            controls);
        }
    }

internal static class MetadataLoaderDefaults
    {
    public static JsonSerializerOptions CreateOptions()
        {
        return new JsonSerializerOptions
            {
            PropertyNameCaseInsensitive = true
            };
        }
    }

internal static class MetadataReader
    {
    public static T? LoadJson<T>(string path)
        {
        if (!File.Exists(path))
            {
            return default;
            }

        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
            {
            return default;
            }

        return JsonSerializer.Deserialize<T>(json, MetadataLoaderDefaults.CreateOptions());
        }
    }

internal sealed record MenuEntry(int MenuId, string MenuName, int? MenuOrder, string? MenuDescription);
internal sealed record MenuPageEntry(int MenuId, int PageId, string PageName, string PagePath, string? PageKey, int? PageOrder, string? Status);

internal sealed record MenuCatalog(
    IReadOnlyList<MenuEntry> Menus,
    IReadOnlyList<MenuPageEntry> Pages,
    string? FailureReason);

internal static class MenuCatalogBuilder
    {
    public static MenuCatalog Build(string? connectionString, IReadOnlyList<PageIdEntry> pageIndex, string aisRoot)
        {
        if (string.IsNullOrWhiteSpace(connectionString))
            {
            return new MenuCatalog(Array.Empty<MenuEntry>(), Array.Empty<MenuPageEntry>(), "DB menu unavailable (no connection string provided)." );
            }

        try
            {
            using var connection = new OracleConnection(connectionString);
            connection.Open();

            var menus = LoadMenus(connection);
            var pages = LoadMenuPages(connection);

            if (menus.Count == 0 && pages.Count == 0)
                {
                return new MenuCatalog(menus, pages, "DB menu unavailable (no menu data returned)." );
                }

            return new MenuCatalog(menus, pages, null);
            }
        catch (Exception ex)
            {
            return new MenuCatalog(Array.Empty<MenuEntry>(), Array.Empty<MenuPageEntry>(), $"DB menu unavailable ({ex.GetType().Name}).");
            }
        }

    private static List<MenuEntry> LoadMenus(OracleConnection connection)
        {
        var menus = new List<MenuEntry>();
        using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = "pkg_lg.p_GetTopMenus";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Clear();
        command.Parameters.Add("UserRoleID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("P_NO", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("ENT_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("R_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            {
            if (reader["MENU_ID"] == DBNull.Value)
                {
                continue;
                }

            var menuId = Convert.ToInt32(reader["MENU_ID"]);
            var name = reader["MENU_NAME"]?.ToString() ?? string.Empty;
            var description = reader["MENU_DESCRIPTION"]?.ToString();
            var menuOrderRaw = reader["MENU_ORDER"]?.ToString();
            var menuOrder = int.TryParse(menuOrderRaw, out var parsedOrder) ? parsedOrder : (int?)null;
            menus.Add(new MenuEntry(menuId, name, menuOrder, description));
            }

        return menus;
        }

    private static List<MenuPageEntry> LoadMenuPages(OracleConnection connection)
        {
        var pages = new List<MenuPageEntry>();
        using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = "pkg_ad.p_GetAllMenuPages";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Clear();
        command.Parameters.Add("menuId", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("P_NO", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("ENT_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("R_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            {
            if (reader["ID"] == DBNull.Value || reader["MENU_ID"] == DBNull.Value)
                {
                continue;
                }

            var pageId = Convert.ToInt32(reader["ID"]);
            var menuId = Convert.ToInt32(reader["MENU_ID"]);
            var name = reader["PAGE_NAME"]?.ToString() ?? string.Empty;
            var path = reader["PAGE_PATH"]?.ToString() ?? string.Empty;
            var pageKey = reader["PAGE_KEY"]?.ToString();
            var pageOrderRaw = reader["PAGE_ORDER"]?.ToString();
            var pageOrder = int.TryParse(pageOrderRaw, out var parsedOrder) ? parsedOrder : (int?)null;
            var status = reader["STATUS"]?.ToString();
            pages.Add(new MenuPageEntry(menuId, pageId, name, PageIdIndexLoader.NormalizePath(path), pageKey, pageOrder, status));
            }

        return pages;
        }
    }

internal static class RoleDiscoveryBuilder
    {
    public static RoleDiscoveryCatalog Build(string? connectionString, MenuCatalog menuCatalog, IReadOnlyList<PageIdEntry> pageIndex)
        {
        _ = pageIndex;
        if (string.IsNullOrWhiteSpace(connectionString))
            {
            return new RoleDiscoveryCatalog(Array.Empty<RoleGroupEntry>(), Array.Empty<RoleMenuPageEntry>(), "DB role discovery unavailable (no connection string provided).");
            }

        try
            {
            using var connection = new OracleConnection(connectionString);
            connection.Open();

            var roles = LoadRoles(connection);
            var entries = new List<RoleMenuPageEntry>();
            if (menuCatalog.Menus.Count > 0)
                {
                foreach (var role in roles)
                    {
                    foreach (var menu in menuCatalog.Menus)
                        {
                        entries.AddRange(LoadAssignedMenuPages(connection, role, menu));
                        }
                    }
                }

            if (roles.Count == 0)
                {
                return new RoleDiscoveryCatalog(roles, entries, "DB role discovery returned no roles.");
                }

            if (entries.Count == 0 && menuCatalog.Menus.Count > 0)
                {
                return new RoleDiscoveryCatalog(roles, entries, "DB role discovery returned no role/page data.");
                }

            if (entries.Count == 0 && menuCatalog.Menus.Count == 0)
                {
                return new RoleDiscoveryCatalog(roles, entries, "DB role discovery skipped menu mapping (no menu data returned).");
                }

            return new RoleDiscoveryCatalog(roles, entries, null);
            }
        catch (Exception ex)
            {
            return new RoleDiscoveryCatalog(Array.Empty<RoleGroupEntry>(), Array.Empty<RoleMenuPageEntry>(), $"DB role discovery unavailable ({ex.GetType().Name}).");
            }
        }

    private static List<RoleGroupEntry> LoadRoles(OracleConnection connection)
        {
        var roles = new List<RoleGroupEntry>();
        using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = "pkg_ad.P_GetGroups";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Clear();
        command.Parameters.Add("P_NO", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("ENT_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("R_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            {
            var roleId = reader["GROUP_ID"]?.ToString();
            var roleName = reader["GROUP_NAME"]?.ToString();
            var description = reader["DESCRIPTION"]?.ToString();
            var status = reader["STATUS"]?.ToString();
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(roleName))
                {
                continue;
                }

            roles.Add(new RoleGroupEntry(roleId, roleName, description, status));
            }

        return roles;
        }

    private static List<RoleMenuPageEntry> LoadAssignedMenuPages(OracleConnection connection, RoleGroupEntry role, MenuEntry menu)
        {
        var pages = new List<RoleMenuPageEntry>();
        using var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandText = "pkg_ad.P_GetAssignedMenuPages";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Clear();
        command.Parameters.Add("groupId", OracleDbType.Int32, ParameterDirection.Input).Value = int.TryParse(role.RoleId, out var parsedGroup) ? parsedGroup : 0;
        command.Parameters.Add("menuId", OracleDbType.Int32, ParameterDirection.Input).Value = menu.MenuId;
        command.Parameters.Add("P_NO", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("ENT_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("R_ID", OracleDbType.Int32, ParameterDirection.Input).Value = 0;
        command.Parameters.Add("T_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            {
            var pageName = reader["PAGE_NAME"]?.ToString() ?? string.Empty;
            var pagePath = reader["PAGE_PATH"]?.ToString() ?? string.Empty;
            var pageOrderRaw = reader["PAGE_ORDER"]?.ToString();
            var pageOrder = int.TryParse(pageOrderRaw, out var parsedOrder) ? parsedOrder : (int?)null;
            var status = reader["STATUS"]?.ToString();
            var pageId = reader["ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID"]);
            var menuId = reader["MENU_ID"] == DBNull.Value ? menu.MenuId : Convert.ToInt32(reader["MENU_ID"]);
            if (!string.IsNullOrWhiteSpace(pageName) || !string.IsNullOrWhiteSpace(pagePath))
                {
                pages.Add(new RoleMenuPageEntry(
                    role.RoleId,
                    role.RoleName,
                    menuId,
                    pageId,
                    pageName.Trim(),
                    PageIdIndexLoader.NormalizePath(pagePath),
                    pageOrder,
                    status));
                }
            }

        return pages;
        }
    }

internal sealed record JsEndpointIndex(IReadOnlyDictionary<string, IReadOnlyList<string>> EndpointsByFile);

internal static class JsEndpointScanner
    {
    private static readonly Regex EndpointRegex = new Regex("['\"](?<url>/[^'\"]+)['\"]", RegexOptions.Compiled);
    private static readonly HashSet<string> StaticExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
        ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot", ".map"
        };

    public static JsEndpointIndex Scan(string jsRoot)
        {
        var map = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(jsRoot))
            {
            return new JsEndpointIndex(map);
            }

        foreach (var file in Directory.EnumerateFiles(jsRoot, "*.js", SearchOption.AllDirectories))
            {
            var content = File.ReadAllText(file);
            var matches = EndpointRegex.Matches(content);
            var endpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in matches)
                {
                var url = match.Groups["url"].Value;
                if (IsStaticAsset(url))
                    {
                    continue;
                    }

                endpoints.Add(url);
                }

            map[file] = endpoints.OrderBy(x => x).ToList();
            }

        return new JsEndpointIndex(map);
        }

    private static bool IsStaticAsset(string url)
        {
        var extension = Path.GetExtension(url);
        return !string.IsNullOrEmpty(extension) && StaticExtensions.Contains(extension);
        }
    }

internal sealed record ViewScriptInfo(string ViewPath, IReadOnlyList<string> ScriptFiles, IReadOnlyList<string> ApiEndpoints);

internal static class ViewScanner
    {
    private static readonly Regex ScriptRegex = new Regex("src=\"(?<src>[^\"]+)\"|src='(?<src>[^']+)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex InlineEndpointRegex = new Regex("['\"](?<url>/[^'\"]+)['\"]", RegexOptions.Compiled);

    public static IReadOnlyDictionary<string, ViewScriptInfo> ScanViews(string viewsRoot, JsEndpointIndex jsIndex)
        {
        var map = new Dictionary<string, ViewScriptInfo>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(viewsRoot))
            {
            return map;
            }

        foreach (var file in Directory.EnumerateFiles(viewsRoot, "*.cshtml", SearchOption.AllDirectories))
            {
            var content = File.ReadAllText(file);
            var scriptFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in ScriptRegex.Matches(content))
                {
                var src = match.Groups["src"].Value;
                if (string.IsNullOrWhiteSpace(src))
                    {
                    continue;
                    }

                var normalized = src.Replace("~", string.Empty).Trim();
                if (normalized.StartsWith("/"))
                    {
                    normalized = normalized.TrimStart('/');
                    }

                if (normalized.StartsWith("js/", StringComparison.OrdinalIgnoreCase))
                    {
                    var fullPath = Path.Combine(viewsRoot, "..", "wwwroot", normalized.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    scriptFiles.Add(Path.GetFullPath(fullPath));
                    }
                }

            var endpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in InlineEndpointRegex.Matches(content))
                {
                endpoints.Add(match.Groups["url"].Value);
                }

            foreach (var scriptFile in scriptFiles)
                {
                if (jsIndex.EndpointsByFile.TryGetValue(scriptFile, out var jsEndpoints))
                    {
                    foreach (var endpoint in jsEndpoints)
                        {
                        endpoints.Add(endpoint);
                        }
                    }
                }

            map[file] = new ViewScriptInfo(file, scriptFiles.OrderBy(x => x).ToList(), endpoints.OrderBy(x => x).ToList());
            }

        return map;
        }
    }

internal sealed record DbProcedureIndex(IReadOnlyDictionary<string, DbProcedureInfo> MethodMap);
internal sealed record DbProcedureInfo(string FilePath, IReadOnlyList<string> Procedures);

internal static class DbProcedureScanner
    {
    private static readonly Regex MethodRegex = new Regex("public\\s+[\\w<>,\\s]+\\s+(?<name>[A-Za-z0-9_]+)\\s*\\(", RegexOptions.Compiled);
    private static readonly Regex CommandRegex = new Regex("CommandText\\s*=\\s*\"(?<cmd>[^\"]+)\"", RegexOptions.Compiled);

    public static DbProcedureIndex Scan(string aisRoot)
        {
        var map = new Dictionary<string, DbProcedureInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(aisRoot, "DBConnection*.cs", SearchOption.TopDirectoryOnly))
            {
            var lines = File.ReadAllLines(file);
            var index = 0;
            while (index < lines.Length)
                {
                var line = lines[index];
                var methodMatch = MethodRegex.Match(line);
                if (!methodMatch.Success)
                    {
                    index++;
                    continue;
                    }

                var methodName = methodMatch.Groups["name"].Value;
                var blockLines = CaptureBlock(lines, ref index);
                var procedures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var blockLine in blockLines)
                    {
                    var cmdMatch = CommandRegex.Match(blockLine);
                    if (cmdMatch.Success)
                        {
                        procedures.Add(cmdMatch.Groups["cmd"].Value);
                        }
                    }

                if (procedures.Count > 0)
                    {
                    map[methodName] = new DbProcedureInfo(file, procedures.OrderBy(x => x).ToList());
                    }
                }
            }

        return new DbProcedureIndex(map);
        }

    private static List<string> CaptureBlock(string[] lines, ref int index)
        {
        var block = new List<string>();
        var braceCount = 0;
        var started = false;

        for (; index < lines.Length; index++)
            {
            var line = lines[index];
            block.Add(line);
            foreach (var ch in line)
                {
                if (ch == '{')
                    {
                    braceCount++;
                    started = true;
                    }
                else if (ch == '}')
                    {
                    braceCount--;
                    }
                }

            if (started && braceCount <= 0)
                {
                index++;
                break;
                }
            }

        return block;
        }
    }

internal sealed record ControllerActionInfo(
    string Controller,
    string Action,
    string? HttpMethod,
    string Route,
    string? ViewPath,
    IReadOnlyList<string> PermissionGates,
    IReadOnlyList<string> DbTouchpoints,
    IReadOnlyList<string> DbProcedures,
    IReadOnlyList<string> ApiEndpoints);

internal static class ControllerScanner
    {
    private static readonly Regex ClassRegex = new Regex("class\\s+(?<name>[A-Za-z0-9_]+)Controller", RegexOptions.Compiled);
    private static readonly Regex MethodRegex = new Regex("public\\s+(?:async\\s+)?(?:Task<\\s*)?(?<return>IActionResult|ActionResult|JsonResult|PartialViewResult|ViewResult|FileResult|ContentResult)(?:\\s*>)?\\s+(?<name>[A-Za-z0-9_]+)\\s*\\(", RegexOptions.Compiled);
    private static readonly Regex ViewRegex = new Regex("return\\s+(?<kind>PartialView|View)\\s*\\((?<args>[^)]*)\\)", RegexOptions.Compiled);
    private static readonly Regex SimpleViewRegex = new Regex("return\\s+(?<kind>PartialView|View)\\s*\\(\\s*\\)", RegexOptions.Compiled);
    private static readonly Regex AttributeRegex = new Regex("^\\s*\\[(?<attr>[^\\]]+)\\]", RegexOptions.Compiled);

    public static IReadOnlyList<ControllerActionInfo> ScanControllers(
        string controllersRoot,
        IReadOnlyDictionary<string, ViewScriptInfo> viewIndex,
        DbProcedureIndex dbProcedures)
        {
        var actions = new List<ControllerActionInfo>();
        if (!Directory.Exists(controllersRoot))
            {
            return actions;
            }

        foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
            {
            var lines = File.ReadAllLines(file);
            var controllerName = string.Empty;
            var classRoute = string.Empty;
            var attributeBuffer = new List<string>();

            for (var i = 0; i < lines.Length; i++)
                {
                var line = lines[i];
                var classMatch = ClassRegex.Match(line);
                if (classMatch.Success)
                    {
                    controllerName = classMatch.Groups["name"].Value;
                    classRoute = ExtractRoute(attributeBuffer);
                    attributeBuffer.Clear();
                    continue;
                    }

                var attrMatch = AttributeRegex.Match(line);
                if (attrMatch.Success)
                    {
                    attributeBuffer.Add(attrMatch.Groups["attr"].Value);
                    continue;
                    }

                var methodMatch = MethodRegex.Match(line);
                if (!methodMatch.Success)
                    {
                    attributeBuffer.Clear();
                    continue;
                    }

                var actionName = methodMatch.Groups["name"].Value;
                var methodAttributes = attributeBuffer.ToList();
                attributeBuffer.Clear();

                var blockLines = CaptureBlock(lines, ref i);
                var viewPath = ResolveViewPath(controllerName, actionName, blockLines);
                var viewEndpoints = ResolveViewEndpoints(viewPath, viewIndex);
                var permissionGates = ExtractPermissionGates(methodAttributes);
                var dbTouchpoints = ExtractDbCalls(blockLines, file);
                var dbProceduresList = MapDbProcedures(dbTouchpoints, dbProcedures);

                var httpMethod = ExtractHttpMethod(methodAttributes);
                var route = BuildRoute(controllerName, actionName, classRoute, methodAttributes);

                actions.Add(new ControllerActionInfo(
                    controllerName,
                    actionName,
                    httpMethod,
                    route,
                    viewPath,
                    permissionGates,
                    dbTouchpoints,
                    dbProceduresList,
                    viewEndpoints));
                }
            }

        return actions;
        }

    private static List<string> CaptureBlock(string[] lines, ref int index)
        {
        var block = new List<string>();
        var braceCount = 0;
        var started = false;

        for (; index < lines.Length; index++)
            {
            var line = lines[index];
            block.Add(line);
            foreach (var ch in line)
                {
                if (ch == '{')
                    {
                    braceCount++;
                    started = true;
                    }
                else if (ch == '}')
                    {
                    braceCount--;
                    }
                }

            if (started && braceCount <= 0)
                {
                index++;
                break;
                }
            }

        return block;
        }

    private static string ResolveViewPath(string controllerName, string actionName, List<string> blockLines)
        {
        var blockText = string.Join(Environment.NewLine, blockLines);
        var match = ViewRegex.Match(blockText);
        if (match.Success)
            {
            var args = match.Groups["args"].Value.Trim();
            var viewName = ExtractViewName(args);
            return ResolveViewPath(controllerName, actionName, viewName);
            }

        if (SimpleViewRegex.IsMatch(blockText))
            {
            return ResolveViewPath(controllerName, actionName, (string?)null);
            }

        return string.Empty;
        }

    private static string ResolveViewPath(string controllerName, string actionName, string? viewName)
        {
        if (!string.IsNullOrWhiteSpace(viewName))
            {
            if (viewName.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
                {
                return viewName.Replace("~/", "");
                }

            if (viewName.Contains("Views/", StringComparison.OrdinalIgnoreCase))
                {
                return viewName.TrimStart('/');
                }

            if (viewName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                return $"Views/{controllerName}/{viewName}";
                }

            return $"Views/{controllerName}/{viewName}.cshtml";
            }

        return $"Views/{controllerName}/{actionName}.cshtml";
        }

    private static string ExtractViewName(string args)
        {
        if (string.IsNullOrWhiteSpace(args))
            {
            return string.Empty;
            }

        var match = Regex.Match(args, "\"(?<name>[^\"]+)\"");
        if (match.Success)
            {
            return match.Groups["name"].Value;
            }

        match = Regex.Match(args, "'(?<name>[^']+)'" );
        if (match.Success)
            {
            return match.Groups["name"].Value;
            }

        return string.Empty;
        }

    private static IReadOnlyList<string> ResolveViewEndpoints(string viewPath, IReadOnlyDictionary<string, ViewScriptInfo> viewIndex)
        {
        if (string.IsNullOrWhiteSpace(viewPath))
            {
            return Array.Empty<string>();
            }

        var normalized = viewPath.Replace("/", Path.DirectorySeparatorChar.ToString());
        foreach (var entry in viewIndex)
            {
            if (entry.Key.EndsWith(normalized, StringComparison.OrdinalIgnoreCase))
                {
                return entry.Value.ApiEndpoints;
                }
            }

        return Array.Empty<string>();
        }

    private static IReadOnlyList<string> ExtractPermissionGates(IEnumerable<string> attributes)
        {
        var gates = new List<string>();
        foreach (var attribute in attributes)
            {
            if (attribute.Contains("Authorize", StringComparison.OrdinalIgnoreCase)
                || attribute.Contains("AllowAnonymous", StringComparison.OrdinalIgnoreCase)
                || attribute.Contains("ValidateAntiForgeryToken", StringComparison.OrdinalIgnoreCase)
                || attribute.Contains("ServiceFilter", StringComparison.OrdinalIgnoreCase)
                || attribute.Contains("TypeFilter", StringComparison.OrdinalIgnoreCase))
                {
                gates.Add(attribute.Trim());
                }
            }

        return gates.OrderBy(x => x).ToList();
        }

    private static string ExtractRoute(IEnumerable<string> attributes)
        {
        foreach (var attribute in attributes)
            {
            var match = Regex.Match(attribute, "Route\\(\\\"(?<route>[^\\\"]+)\\\"\\)");
            if (match.Success)
                {
                return match.Groups["route"].Value.Trim('/');
                }
            }

        return string.Empty;
        }

    private static string ExtractHttpMethod(IEnumerable<string> attributes)
        {
        foreach (var attribute in attributes)
            {
            if (attribute.StartsWith("HttpGet", StringComparison.OrdinalIgnoreCase))
                {
                return "GET";
                }
            if (attribute.StartsWith("HttpPost", StringComparison.OrdinalIgnoreCase))
                {
                return "POST";
                }
            if (attribute.StartsWith("HttpPut", StringComparison.OrdinalIgnoreCase))
                {
                return "PUT";
                }
            if (attribute.StartsWith("HttpDelete", StringComparison.OrdinalIgnoreCase))
                {
                return "DELETE";
                }
            if (attribute.StartsWith("HttpPatch", StringComparison.OrdinalIgnoreCase))
                {
                return "PATCH";
                }
            }

        return string.Empty;
        }

    private static string BuildRoute(string controller, string action, string classRoute, IReadOnlyList<string> attributes)
        {
        var methodRoute = ExtractRoute(attributes);
        if (string.IsNullOrWhiteSpace(methodRoute) && !string.IsNullOrWhiteSpace(classRoute))
            {
            return $"/{classRoute}/{action}".Replace("//", "/");
            }

        if (!string.IsNullOrWhiteSpace(methodRoute))
            {
            if (methodRoute.StartsWith("/"))
                {
                return methodRoute;
                }

            if (!string.IsNullOrWhiteSpace(classRoute))
                {
                return $"/{classRoute}/{methodRoute}".Replace("//", "/");
                }

            return $"/{methodRoute}";
            }

        return $"/{controller}/{action}";
        }

    private static IReadOnlyList<string> ExtractDbCalls(IEnumerable<string> blockLines, string filePath)
        {
        var dbIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fieldRegex = new Regex("DBConnection\\s+(?<name>[A-Za-z0-9_]+)", RegexOptions.Compiled);
        var callRegex = new Regex("(?<name>[A-Za-z0-9_]+)\\.(?<method>[A-Za-z0-9_]+)\\s*\\(", RegexOptions.Compiled);

        foreach (var line in blockLines)
            {
            var match = fieldRegex.Match(line);
            if (match.Success)
                {
                dbIdentifiers.Add(match.Groups["name"].Value);
                }
            }

        var calls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in blockLines)
            {
            var match = callRegex.Match(line);
            if (!match.Success)
                {
                continue;
                }

            var name = match.Groups["name"].Value;
            if (!dbIdentifiers.Contains(name))
                {
                if (!name.Contains("db", StringComparison.OrdinalIgnoreCase))
                    {
                    continue;
                    }
                }

            calls.Add(match.Groups["method"].Value);
            }

        return calls.OrderBy(x => x).ToList();
        }

    private static IReadOnlyList<string> MapDbProcedures(IEnumerable<string> dbMethods, DbProcedureIndex dbProcedureIndex)
        {
        var procedures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var method in dbMethods)
            {
            if (dbProcedureIndex.MethodMap.TryGetValue(method, out var info))
                {
                foreach (var procedure in info.Procedures)
                    {
                    procedures.Add(procedure);
                    }
                }
            }

        return procedures.OrderBy(x => x).ToList();
        }
    }

internal sealed record SecurityControl(string Control, string Location);

internal static class SecurityControlScanner
    {
    public static IReadOnlyList<SecurityControl> Scan(string aisRoot)
        {
        var controls = new List<SecurityControl>();
        var targets = new[]
            {
            Path.Combine(aisRoot, "Filters"),
            Path.Combine(aisRoot, "Middleware"),
            Path.Combine(aisRoot, "Security")
            };

        foreach (var target in targets)
            {
            if (!Directory.Exists(target))
                {
                continue;
                }

            foreach (var file in Directory.EnumerateFiles(target, "*.cs", SearchOption.AllDirectories))
                {
                var content = File.ReadAllText(file);
                var classMatch = Regex.Match(content, "class\\s+(?<name>[A-Za-z0-9_]+)");
                if (!classMatch.Success)
                    {
                    continue;
                    }

                var className = classMatch.Groups["name"].Value;
                var method = Regex.Match(content, "void\\s+(?<method>OnActionExecuting|InvokeAsync|Invoke)\\s*\\(");
                var location = method.Success
                    ? $"{file}: {className}.{method.Groups["method"].Value}"
                    : $"{file}: {className}";
                controls.Add(new SecurityControl(className, location));
                }
            }

        var startupPath = Path.Combine(aisRoot, "Startup.cs");
        if (File.Exists(startupPath))
            {
            controls.Add(new SecurityControl("MVC Filters", $"{startupPath}: AddMvcOptions"));
            controls.Add(new SecurityControl("Cookie Authentication", $"{startupPath}: AddAuthentication"));
            controls.Add(new SecurityControl("Application Cookies", $"{startupPath}: ConfigureApplicationCookie"));
            }

        return controls.OrderBy(c => c.Control).ToList();
        }
    }

internal sealed class RolePersonaConfig
    {
    public List<RolePersonaDefinition> Roles { get; init; } = new();
    }

internal sealed class RolePersonaDefinition
    {
    public string Name { get; init; } = string.Empty;
    public string Persona { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<int> PageIds { get; init; } = new();
    public List<string> FunctionalAreas { get; init; } = new();
    }

internal sealed record RolePageReference(int PageId, string PageName, string PagePath, int? MenuId);

internal sealed record RolePersonaCoverage(
    string Name,
    string Persona,
    string? Description,
    string? Status,
    IReadOnlyList<string> FunctionalAreas,
    IReadOnlyList<RolePageReference> Pages,
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> ApiEndpoints,
    IReadOnlyList<string> DbTouchpoints,
    IReadOnlyList<string> DbProcedures);

internal static class RolePersonaConfigLoader
    {
    public static RolePersonaConfig Load(string path)
        {
        return JsonConfigLoader.Load(path, new RolePersonaConfig());
        }
    }

internal static class RolePersonaMapper
    {
    public static IReadOnlyList<RolePersonaCoverage> Map(
        RolePersonaConfig config,
        IReadOnlyList<PageIdEntry> pageIndex,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        if (config.Roles.Count == 0)
            {
            return Array.Empty<RolePersonaCoverage>();
            }

        var pageIndexMap = pageIndex.ToDictionary(p => p.PageId);
        var results = new List<RolePersonaCoverage>();

        foreach (var role in config.Roles)
            {
            var pages = new List<RolePageReference>();
            var matchedActions = new HashSet<ControllerActionInfo>();
            foreach (var pageId in role.PageIds.Distinct())
                {
                if (!pageIndexMap.TryGetValue(pageId, out var pageEntry))
                    {
                    continue;
                    }

                pages.Add(new RolePageReference(pageEntry.PageId, pageEntry.PageName ?? pageEntry.PagePath, pageEntry.PagePath, null));
                foreach (var action in actions)
                    {
                    if (PageActionMatcher.Matches(pageEntry.PagePath, action))
                        {
                        matchedActions.Add(action);
                        }
                    }
                }

            var actionNames = matchedActions
                .OrderBy(a => a.Controller)
                .ThenBy(a => a.Action)
                .Select(a => $"{a.Controller}.{a.Action}")
                .ToList();

            var apiEndpoints = matchedActions
                .SelectMany(a => a.ApiEndpoints)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            var dbTouchpoints = matchedActions
                .SelectMany(a => a.DbTouchpoints)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            var dbProcedures = matchedActions
                .SelectMany(a => a.DbProcedures)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            results.Add(new RolePersonaCoverage(
                role.Name,
                role.Persona,
                role.Description,
                null,
                role.FunctionalAreas.Distinct().OrderBy(x => x).ToList(),
                pages.OrderBy(p => p.PageId).ToList(),
                actionNames,
                apiEndpoints,
                dbTouchpoints,
                dbProcedures));
            }

        return results;
        }
    }

internal static class RoleCoverageBuilder
    {
    public static IReadOnlyList<RolePersonaCoverage> Build(
        IReadOnlyList<RoleDefinition> roleDefinitions,
        RoleDiscoveryCatalog roleDiscovery,
        MenuCatalog menuCatalog,
        IReadOnlyList<PageIdEntry> pageIndex,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        var roleMap = roleDefinitions.ToDictionary(r => r.Name, StringComparer.OrdinalIgnoreCase);
        var roleNames = new HashSet<string>(roleMap.Keys, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in roleDiscovery.Roles)
            {
            roleNames.Add(entry.RoleName);
            }

        if (roleNames.Count == 0)
            {
            return Array.Empty<RolePersonaCoverage>();
            }

        var pageIndexById = pageIndex.ToDictionary(p => p.PageId);
        var pageIndexByName = pageIndex
            .Where(p => !string.IsNullOrWhiteSpace(p.PageName))
            .GroupBy(p => p.PageName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var pageIndexByPath = pageIndex
            .Where(p => !string.IsNullOrWhiteSpace(p.PagePath))
            .GroupBy(p => PageIdIndexLoader.NormalizePath(p.PagePath), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var menuPagesByName = menuCatalog.Pages
            .Where(p => !string.IsNullOrWhiteSpace(p.PageName))
            .GroupBy(p => p.PageName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var menuPagesByPath = menuCatalog.Pages
            .Where(p => !string.IsNullOrWhiteSpace(p.PagePath))
            .GroupBy(p => p.PagePath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var menuIdByPageId = menuCatalog.Pages
            .GroupBy(p => p.PageId)
            .ToDictionary(g => g.Key, g => g.First().MenuId);

        var results = new List<RolePersonaCoverage>();

        foreach (var roleName in roleNames.OrderBy(x => x))
            {
            roleMap.TryGetValue(roleName, out var metadata);
            var discoveredRole = roleDiscovery.Roles.FirstOrDefault(r => string.Equals(r.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
            var persona = metadata?.Persona ?? roleName;
            var description = metadata?.Description ?? discoveredRole?.Description;
            var status = discoveredRole?.Status;
            var functionalAreas = metadata?.FunctionalAreas?.Distinct().OrderBy(x => x).ToList() ?? new List<string>();

            var pages = new List<RolePageReference>();
            var pageKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (metadata?.PageIds != null)
                {
                foreach (var pageId in metadata.PageIds.Distinct())
                    {
                    if (pageIndexById.TryGetValue(pageId, out var pageEntry))
                        {
                        var menuId = menuIdByPageId.TryGetValue(pageEntry.PageId, out var resolvedMenuId) ? resolvedMenuId : (int?)null;
                        AddPage(pages, pageKeys, new RolePageReference(pageEntry.PageId, pageEntry.PageName ?? pageEntry.PagePath, pageEntry.PagePath, menuId));
                        }
                    else
                        {
                        AddPage(pages, pageKeys, new RolePageReference(pageId, $"Page {pageId}", string.Empty, null));
                        }
                    }
                }

            foreach (var entry in roleDiscovery.MenuPages.Where(e => string.Equals(e.RoleName, roleName, StringComparison.OrdinalIgnoreCase)))
                {
                var resolvedPage = ResolvePage(entry, menuPagesByPath, menuPagesByName, pageIndexByPath, pageIndexByName, pageIndexById);
                AddPage(pages, pageKeys, resolvedPage);
                }

            var matchedActions = new HashSet<ControllerActionInfo>();
            foreach (var page in pages)
                {
                if (string.IsNullOrWhiteSpace(page.PagePath))
                    {
                    continue;
                    }

                foreach (var action in actions)
                    {
                    if (PageActionMatcher.Matches(page.PagePath, action))
                        {
                        matchedActions.Add(action);
                        }
                    }
                }

            var actionNames = matchedActions
                .OrderBy(a => a.Controller)
                .ThenBy(a => a.Action)
                .Select(a => $"{a.Controller}.{a.Action}")
                .ToList();

            var apiEndpoints = matchedActions
                .SelectMany(a => a.ApiEndpoints)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            var dbTouchpoints = matchedActions
                .SelectMany(a => a.DbTouchpoints)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            var dbProcedures = matchedActions
                .SelectMany(a => a.DbProcedures)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            results.Add(new RolePersonaCoverage(
                roleName,
                persona,
                description,
                status,
                functionalAreas,
                pages.OrderBy(p => p.PageId).ThenBy(p => p.PageName).ToList(),
                actionNames,
                apiEndpoints,
                dbTouchpoints,
                dbProcedures));
            }

        return results;
        }

    private static void AddPage(List<RolePageReference> pages, HashSet<string> pageKeys, RolePageReference page)
        {
        var key = $"{page.PageId}:{page.PageName}:{page.PagePath}:{page.MenuId}";
        if (pageKeys.Add(key))
            {
            pages.Add(page);
            }
        }

    private static RolePageReference ResolvePage(
        RoleMenuPageEntry entry,
        IReadOnlyDictionary<string, MenuPageEntry> menuPagesByPath,
        IReadOnlyDictionary<string, MenuPageEntry> menuPagesByName,
        IReadOnlyDictionary<string, PageIdEntry> pageIndexByPath,
        IReadOnlyDictionary<string, PageIdEntry> pageIndexByName,
        IReadOnlyDictionary<int, PageIdEntry> pageIndexById)
        {
        if (entry.PageId > 0 && pageIndexById.TryGetValue(entry.PageId, out var pageById))
            {
            return new RolePageReference(pageById.PageId, pageById.PageName ?? pageById.PagePath, pageById.PagePath, entry.MenuId);
            }

        if (!string.IsNullOrWhiteSpace(entry.PagePath) && menuPagesByPath.TryGetValue(entry.PagePath, out var menuPageByPath))
            {
            return new RolePageReference(menuPageByPath.PageId, menuPageByPath.PageName, menuPageByPath.PagePath, menuPageByPath.MenuId);
            }

        if (!string.IsNullOrWhiteSpace(entry.PagePath) && pageIndexByPath.TryGetValue(entry.PagePath, out var pageByPath))
            {
            return new RolePageReference(pageByPath.PageId, pageByPath.PageName ?? pageByPath.PagePath, pageByPath.PagePath, entry.MenuId);
            }

        if (!string.IsNullOrWhiteSpace(entry.PageName) && menuPagesByName.TryGetValue(entry.PageName, out var menuPage))
            {
            return new RolePageReference(menuPage.PageId, menuPage.PageName, menuPage.PagePath, menuPage.MenuId);
            }

        if (!string.IsNullOrWhiteSpace(entry.PageName) && pageIndexByName.TryGetValue(entry.PageName, out var pageEntry))
            {
            return new RolePageReference(pageEntry.PageId, pageEntry.PageName ?? pageEntry.PagePath, pageEntry.PagePath, entry.MenuId);
            }

        return new RolePageReference(entry.PageId, entry.PageName, entry.PagePath, entry.MenuId);
        }
    }

internal sealed class WorkflowConfig
    {
    public List<WorkflowConfigDefinition> Workflows { get; init; } = new();
    }

internal sealed class WorkflowConfigDefinition
    {
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<WorkflowStageConfigDefinition> Stages { get; init; } = new();
    }

internal sealed class WorkflowStageConfigDefinition
    {
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public List<WorkflowTransitionConfigDefinition> Transitions { get; init; } = new();
    }

internal sealed class WorkflowTransitionConfigDefinition
    {
    public string To { get; init; } = string.Empty;
    public List<string> Routes { get; init; } = new();
    public List<string> Controllers { get; init; } = new();
    public List<string> Actions { get; init; } = new();
    public List<string> Apis { get; init; } = new();
    public List<string> DbProcedures { get; init; } = new();
    }

internal sealed record WorkflowTransitionCatalogEntry(
    string ToStage,
    IReadOnlyList<string> Routes,
    IReadOnlyList<string> Apis,
    IReadOnlyList<string> DbProcedures,
    IReadOnlyList<string> Actions);

internal sealed record WorkflowStageCatalogEntry(
    string Name,
    int Order,
    IReadOnlyList<WorkflowTransitionCatalogEntry> Transitions);

internal sealed record WorkflowCatalogEntry(
    string Name,
    string? Description,
    IReadOnlyList<WorkflowStageCatalogEntry> Stages);

internal static class WorkflowConfigLoader
    {
    public static WorkflowConfig Load(string path)
        {
        return JsonConfigLoader.Load(path, new WorkflowConfig());
        }
    }

internal static class WorkflowMapper
    {
    public static IReadOnlyList<WorkflowCatalogEntry> Map(WorkflowConfig config, IReadOnlyList<ControllerActionInfo> actions)
        {
        if (config.Workflows.Count == 0)
            {
            return Array.Empty<WorkflowCatalogEntry>();
            }

        var results = new List<WorkflowCatalogEntry>();
        foreach (var workflow in config.Workflows)
            {
            var stages = new List<WorkflowStageCatalogEntry>();
            foreach (var stage in workflow.Stages)
                {
                var transitions = new List<WorkflowTransitionCatalogEntry>();
                foreach (var transition in stage.Transitions)
                    {
                    transitions.Add(ResolveTransition(transition, actions));
                    }

                stages.Add(new WorkflowStageCatalogEntry(stage.Name, stage.Order, transitions));
                }

            results.Add(new WorkflowCatalogEntry(workflow.Name, workflow.Description, stages.OrderBy(s => s.Order).ToList()));
            }

        return results;
        }

    private static WorkflowTransitionCatalogEntry ResolveTransition(
        WorkflowTransitionConfigDefinition transition,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        var resolvedRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var route in transition.Routes)
            {
            var matches = actions.Where(a => string.Equals(a.Route, route, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
                {
                resolvedRoutes.Add($"{route} (no controller match)");
                continue;
                }

            foreach (var match in matches)
                {
                resolvedRoutes.Add($"{match.Route} → {match.Controller}.{match.Action}");
                }
            }

        var resolvedApis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var api in transition.Apis)
            {
            var match = actions.Any(a => a.ApiEndpoints.Any(endpoint => PathMatcher.Matches(endpoint, api)));
            resolvedApis.Add(match ? api : $"{api} (unmatched)");
            }

        var resolvedProcedures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var procedure in transition.DbProcedures)
            {
            var match = actions.Any(a => a.DbProcedures.Any(p => PathMatcher.Matches(p, procedure)));
            resolvedProcedures.Add(match ? procedure : $"{procedure} (unmatched)");
            }

        var resolvedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var controller in transition.Controllers)
            {
            foreach (var match in actions.Where(a => string.Equals(a.Controller, controller, StringComparison.OrdinalIgnoreCase)))
                {
                resolvedActions.Add($"{match.Controller}.{match.Action}");
                }
            }

        foreach (var actionName in transition.Actions)
            {
            foreach (var match in actions.Where(a => string.Equals(a.Action, actionName, StringComparison.OrdinalIgnoreCase)))
                {
                resolvedActions.Add($"{match.Controller}.{match.Action}");
                }
            }

        return new WorkflowTransitionCatalogEntry(
            transition.To,
            resolvedRoutes.OrderBy(r => r).ToList(),
            resolvedApis.OrderBy(a => a).ToList(),
            resolvedProcedures.OrderBy(p => p).ToList(),
            resolvedActions.OrderBy(a => a).ToList());
        }
    }

internal static class WorkflowCoverageBuilder
    {
    private const double InferenceConfidenceThreshold = 0.45;

    public static WorkflowCoverageResult Build(
        IReadOnlyList<WorkflowDefinition> workflowDefinitions,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        var declaredCatalog = MapDeclaredWorkflows(workflowDefinitions, actions);
        var inferredWorkflows = WorkflowInferenceEngine.Infer(actions);
        var averageConfidence = inferredWorkflows.Count == 0 ? 0 : inferredWorkflows.Average(entry => entry.Confidence);

        var useInference = inferredWorkflows.Count > 0 && (averageConfidence >= InferenceConfidenceThreshold || declaredCatalog.Count == 0);
        var fallbackReason = useInference
            ? null
            : inferredWorkflows.Count == 0
                ? "Workflow inference produced no candidate workflows."
                : $"Workflow inference confidence {averageConfidence:F2} below threshold; using declared workflows.";

        var catalog = useInference
            ? inferredWorkflows.Select(entry => entry.Workflow).ToList()
            : declaredCatalog;

        return new WorkflowCoverageResult(catalog, inferredWorkflows, useInference, fallbackReason, averageConfidence);
        }

    private static IReadOnlyList<WorkflowCatalogEntry> MapDeclaredWorkflows(
        IReadOnlyList<WorkflowDefinition> workflowDefinitions,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        if (workflowDefinitions.Count == 0)
            {
            return Array.Empty<WorkflowCatalogEntry>();
            }

        var results = new List<WorkflowCatalogEntry>();
        foreach (var workflow in workflowDefinitions)
            {
            var stages = new List<WorkflowStageCatalogEntry>();
            var orderedStages = workflow.Stages.OrderBy(stage => stage.Order).ToList();
            for (var i = 0; i < orderedStages.Count; i++)
                {
                var stage = orderedStages[i];
                var nextStageName = i + 1 < orderedStages.Count ? orderedStages[i + 1].Name : "Complete";
                var transition = BuildDeclaredTransition(stage, nextStageName, actions);
                stages.Add(new WorkflowStageCatalogEntry(stage.Name, stage.Order, new List<WorkflowTransitionCatalogEntry> { transition }));
                }

            results.Add(new WorkflowCatalogEntry(workflow.Name, workflow.Description, stages));
            }

        return results;
        }

    private static WorkflowTransitionCatalogEntry BuildDeclaredTransition(
        WorkflowStageDefinition stage,
        string nextStage,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        var matchedActions = new HashSet<ControllerActionInfo>();
        foreach (var route in stage.Routes)
            {
            foreach (var action in actions.Where(a => PathMatcher.Matches(a.Route, route)))
                {
                matchedActions.Add(action);
                }
            }

        foreach (var api in stage.ApiEndpoints)
            {
            foreach (var action in actions.Where(a => a.ApiEndpoints.Any(endpoint => PathMatcher.Matches(endpoint, api))))
                {
                matchedActions.Add(action);
                }
            }

        foreach (var dbProcedure in stage.DbProcedures)
            {
            foreach (var action in actions.Where(a => a.DbProcedures.Any(proc => PathMatcher.Matches(proc, dbProcedure))))
                {
                matchedActions.Add(action);
                }
            }

        return BuildTransition(nextStage, matchedActions);
        }

    private static WorkflowTransitionCatalogEntry BuildTransition(string toStage, IEnumerable<ControllerActionInfo> matchedActions)
        {
        var actionList = matchedActions.ToList();

        var routes = actionList
            .Select(a => $"{a.Route} → {a.Controller}.{a.Action}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        var apis = actionList
            .SelectMany(a => a.ApiEndpoints)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        var dbProcedures = actionList
            .SelectMany(a => a.DbProcedures)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        var actions = actionList
            .Select(a => $"{a.Controller}.{a.Action}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        return new WorkflowTransitionCatalogEntry(toStage, routes, apis, dbProcedures, actions);
        }
    }

internal static class WorkflowInferenceEngine
    {
    private static readonly string[] SubmitKeywords =
        {
        "submit", "create", "save", "request", "apply", "initiate"
        };

    private static readonly string[] ApproveKeywords =
        {
        "approve", "review", "authorize", "verify", "endorse"
        };

    private static readonly string[] CloseKeywords =
        {
        "close", "complete", "finalize", "settle", "archive"
        };

    private static readonly string[] RejectKeywords =
        {
        "reject", "return", "rework", "revise", "cancel"
        };

    private static readonly string[] StatusKeywords =
        {
        "status", "state"
        };

    public static IReadOnlyList<WorkflowInferenceEntry> Infer(IReadOnlyList<ControllerActionInfo> actions)
        {
        var results = new List<WorkflowInferenceEntry>();
        foreach (var group in actions.GroupBy(a => a.Controller))
            {
            var submitActions = FilterByKeywords(group, SubmitKeywords);
            var approveActions = FilterByKeywords(group, ApproveKeywords);
            var closeActions = FilterByKeywords(group, CloseKeywords);
            var rejectActions = FilterByKeywords(group, RejectKeywords);
            var statusSignals = HasStatusSignals(group);

            var signalCount = 0;
            if (submitActions.Count > 0) signalCount++;
            if (approveActions.Count > 0) signalCount++;
            if (closeActions.Count > 0) signalCount++;
            if (rejectActions.Count > 0) signalCount++;
            if (statusSignals) signalCount++;

            if (signalCount < 2)
                {
                continue;
                }

            var stages = new List<WorkflowStageCatalogEntry>();
            var evidence = new List<string>();

            if (submitActions.Count > 0)
                {
                stages.Add(new WorkflowStageCatalogEntry(
                    "Initiate",
                    stages.Count + 1,
                    new List<WorkflowTransitionCatalogEntry>
                        {
                        BuildInferredTransition(approveActions.Count > 0 ? "Review" : closeActions.Count > 0 ? "Close" : "Complete", submitActions)
                        }));
                evidence.Add($"Submit actions: {FormatActionEvidence(submitActions)}");
                }

            if (approveActions.Count > 0)
                {
                var transitions = new List<WorkflowTransitionCatalogEntry>
                    {
                    BuildInferredTransition(closeActions.Count > 0 ? "Close" : "Complete", approveActions)
                    };
                if (rejectActions.Count > 0)
                    {
                    transitions.Add(BuildInferredTransition("Rework", rejectActions));
                    }

                stages.Add(new WorkflowStageCatalogEntry("Review", stages.Count + 1, transitions));
                evidence.Add($"Approval actions: {FormatActionEvidence(approveActions)}");
                }

            if (rejectActions.Count > 0 && stages.All(stage => stage.Name != "Rework"))
                {
                stages.Add(new WorkflowStageCatalogEntry(
                    "Rework",
                    stages.Count + 1,
                    new List<WorkflowTransitionCatalogEntry>
                        {
                        BuildInferredTransition("Review", rejectActions)
                        }));
                evidence.Add($"Rework actions: {FormatActionEvidence(rejectActions)}");
                }

            if (closeActions.Count > 0)
                {
                stages.Add(new WorkflowStageCatalogEntry(
                    "Close",
                    stages.Count + 1,
                    new List<WorkflowTransitionCatalogEntry>()));
                evidence.Add($"Close actions: {FormatActionEvidence(closeActions)}");
                }

            if (statusSignals)
                {
                evidence.Add("Status/state signals detected in DB procedures.");
                }

            var confidence = Math.Min(1.0, signalCount / 5.0);
            var workflow = new WorkflowCatalogEntry(
                $"{group.Key} Lifecycle",
                $"Inferred from {group.Key} controller patterns.",
                stages);

            results.Add(new WorkflowInferenceEntry(workflow, confidence, evidence));
            }

        return results.OrderBy(r => r.Workflow.Name).ToList();
        }

    private static List<ControllerActionInfo> FilterByKeywords(IEnumerable<ControllerActionInfo> actions, IEnumerable<string> keywords)
        {
        return actions.Where(action => MatchesAnyKeyword(action, keywords)).ToList();
        }

    private static bool MatchesAnyKeyword(ControllerActionInfo action, IEnumerable<string> keywords)
        {
        return keywords.Any(keyword =>
            action.Action.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || action.Route.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || action.DbProcedures.Any(proc => proc.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

    private static bool HasStatusSignals(IEnumerable<ControllerActionInfo> actions)
        {
        return actions.Any(action =>
            action.DbProcedures.Any(proc => StatusKeywords.Any(keyword => proc.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            || action.DbTouchpoints.Any(touchpoint => StatusKeywords.Any(keyword => touchpoint.Contains(keyword, StringComparison.OrdinalIgnoreCase))));
        }

    private static WorkflowTransitionCatalogEntry BuildInferredTransition(string toStage, IReadOnlyList<ControllerActionInfo> actions)
        {
        return new WorkflowTransitionCatalogEntry(
            toStage,
            actions.Select(a => $"{a.Route} → {a.Controller}.{a.Action}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList(),
            actions.SelectMany(a => a.ApiEndpoints)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList(),
            actions.SelectMany(a => a.DbProcedures)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList(),
            actions.Select(a => $"{a.Controller}.{a.Action}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList());
        }

    private static string FormatActionEvidence(IEnumerable<ControllerActionInfo> actions)
        {
        return string.Join(", ", actions.Select(a => $"{a.Controller}.{a.Action}").Distinct().OrderBy(a => a).Take(6));
        }
    }

internal sealed class CapabilityConfig
    {
    public List<CapabilityConfigDefinition> Capabilities { get; init; } = new();
    }

internal sealed class CapabilityConfigDefinition
    {
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<string> Routes { get; init; } = new();
    public List<string> Controllers { get; init; } = new();
    public List<string> Apis { get; init; } = new();
    }

internal sealed record CapabilityCatalogEntry(
    string Name,
    string? Description,
    IReadOnlyList<string> Routes,
    IReadOnlyList<string> Controllers,
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> Apis,
    IReadOnlyList<string> Views,
    IReadOnlyList<string> Workflows,
    IReadOnlyList<string> DbProcedures);

internal static class CapabilityConfigLoader
    {
    public static CapabilityConfig Load(string path)
        {
        return JsonConfigLoader.Load(path, new CapabilityConfig());
        }
    }

internal static class CapabilityMapper
    {
    public static IReadOnlyList<CapabilityCatalogEntry> Map(
        CapabilityConfig config,
        IReadOnlyList<ControllerActionInfo> actions,
        IReadOnlyList<WorkflowCatalogEntry> workflows)
        {
        if (config.Capabilities.Count == 0)
            {
            return Array.Empty<CapabilityCatalogEntry>();
            }

        var results = new List<CapabilityCatalogEntry>();
        foreach (var capability in config.Capabilities)
            {
            var matchedActions = ResolveActions(capability, actions);

            var routes = matchedActions.Select(a => a.Route).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var controllers = matchedActions.Select(a => a.Controller).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var apis = matchedActions.SelectMany(a => a.ApiEndpoints).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var rawViews = matchedActions.Select(a => a.ViewPath);
            var views = rawViews
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();
            var dbProcedures = matchedActions.SelectMany(a => a.DbProcedures).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var workflowMatches = ResolveWorkflowMatches(capability, workflows);

            results.Add(new CapabilityCatalogEntry(
                capability.Name,
                capability.Description,
                routes,
                controllers,
                matchedActions.Select(a => $"{a.Controller}.{a.Action}").Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList(),
                apis,
                views,
                workflowMatches,
                dbProcedures));
            }

        return results;
        }

    private static IReadOnlyList<ControllerActionInfo> ResolveActions(CapabilityConfigDefinition capability, IReadOnlyList<ControllerActionInfo> actions)
        {
        var matches = new HashSet<ControllerActionInfo>();

        foreach (var route in capability.Routes)
            {
            foreach (var action in actions.Where(a => PathMatcher.Matches(a.Route, route)))
                {
                matches.Add(action);
                }
            }

        foreach (var controller in capability.Controllers)
            {
            foreach (var action in actions.Where(a => string.Equals(a.Controller, controller, StringComparison.OrdinalIgnoreCase)))
                {
                matches.Add(action);
                }
            }

        foreach (var api in capability.Apis)
            {
            foreach (var action in actions.Where(a => a.ApiEndpoints.Any(endpoint => PathMatcher.Matches(endpoint, api))))
                {
                matches.Add(action);
                }
            }

        return matches.ToList();
        }

    private static IReadOnlyList<string> ResolveWorkflowMatches(
        CapabilityConfigDefinition capability,
        IReadOnlyList<WorkflowCatalogEntry> workflows)
        {
        var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var workflow in workflows)
            {
            foreach (var stage in workflow.Stages)
                {
                foreach (var transition in stage.Transitions)
                    {
                    if (transition.Routes.Any(route => capability.Routes.Any(r => PathMatcher.Matches(route, r)))
                        || transition.Apis.Any(api => capability.Apis.Any(a => PathMatcher.Matches(api, a))))
                        {
                        matches.Add($"{workflow.Name} → {stage.Name}");
                        }
                    }
                }
            }

        return matches.OrderBy(x => x).ToList();
        }
    }

internal static class CapabilityCoverageBuilder
    {
    public static IReadOnlyList<CapabilityCatalogEntry> Build(
        IReadOnlyList<CapabilityDefinition> capabilityDefinitions,
        IReadOnlyList<ControllerActionInfo> actions,
        IReadOnlyList<WorkflowCatalogEntry> workflows)
        {
        if (capabilityDefinitions.Count == 0)
            {
            return Array.Empty<CapabilityCatalogEntry>();
            }

        var results = new List<CapabilityCatalogEntry>();
        foreach (var capability in capabilityDefinitions)
            {
            var matchedActions = ResolveActions(capability, actions);

            var routes = matchedActions.Select(a => a.Route).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var controllers = matchedActions.Select(a => a.Controller).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var actionNames = matchedActions.Select(a => $"{a.Controller}.{a.Action}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
            var apis = matchedActions.SelectMany(a => a.ApiEndpoints).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var rawViews = matchedActions.Select(a => a.ViewPath);
            var views = rawViews
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList();
            var dbProcedures = matchedActions.SelectMany(a => a.DbProcedures).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            var workflowMatches = ResolveWorkflowMatches(capability, workflows);

            results.Add(new CapabilityCatalogEntry(
                capability.Name,
                capability.Summary,
                routes,
                controllers,
                actionNames,
                apis,
                views,
                workflowMatches,
                dbProcedures));
            }

        return results;
        }

    private static IReadOnlyList<ControllerActionInfo> ResolveActions(
        CapabilityDefinition capability,
        IReadOnlyList<ControllerActionInfo> actions)
        {
        var matches = new HashSet<ControllerActionInfo>();

        foreach (var feature in capability.Features)
            {
            foreach (var route in feature.Routes)
                {
                foreach (var action in actions.Where(a => PathMatcher.Matches(a.Route, route)))
                    {
                    matches.Add(action);
                    }
                }

            foreach (var controller in feature.Controllers)
                {
                foreach (var action in actions.Where(a => string.Equals(a.Controller, controller, StringComparison.OrdinalIgnoreCase)))
                    {
                    matches.Add(action);
                    }
                }

            foreach (var api in feature.ApiEndpoints)
                {
                foreach (var action in actions.Where(a => a.ApiEndpoints.Any(endpoint => PathMatcher.Matches(endpoint, api))))
                    {
                    matches.Add(action);
                    }
                }
            }

        return matches.ToList();
        }

    private static IReadOnlyList<string> ResolveWorkflowMatches(
        CapabilityDefinition capability,
        IReadOnlyList<WorkflowCatalogEntry> workflows)
        {
        var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var workflow in workflows)
            {
            foreach (var stage in workflow.Stages)
                {
                foreach (var transition in stage.Transitions)
                    {
                    if (transition.Routes.Any(route => capability.Features.Any(feature => feature.Routes.Any(r => PathMatcher.Matches(route, r))))
                        || transition.Apis.Any(api => capability.Features.Any(feature => feature.ApiEndpoints.Any(a => PathMatcher.Matches(api, a)))))
                        {
                        matches.Add($"{workflow.Name} → {stage.Name}");
                        }
                    }
                }
            }

        return matches.OrderBy(x => x).ToList();
        }
    }

internal sealed class SecurityMetadataConfig
    {
    public List<SecurityCapabilityMetadata> Capabilities { get; init; } = new();
    }

internal sealed class SecurityCapabilityMetadata
    {
    public string Name { get; init; } = string.Empty;
    public string? Purpose { get; init; }
    public List<string> Risks { get; init; } = new();
    public List<SecurityControlConfigMetadata> Controls { get; init; } = new();
    }

internal sealed class SecurityControlConfigMetadata
    {
    public string Control { get; init; } = string.Empty;
    public string? Purpose { get; init; }
    public string? Risk { get; init; }
    }

internal sealed record SecurityControlDetail(string Control, string Location, string? Purpose, string? Risk);

internal sealed record SecurityCapabilityEntry(
    string Name,
    string? Purpose,
    IReadOnlyList<string> Risks,
    IReadOnlyList<SecurityControlDetail> Controls);

internal sealed record SecurityCapabilityCatalog(
    IReadOnlyList<SecurityCapabilityEntry> Capabilities,
    IReadOnlyList<SecurityControl> UnmappedControls);

internal static class SecurityMetadataLoader
    {
    public static SecurityMetadataConfig Load(string path)
        {
        return JsonConfigLoader.Load(path, new SecurityMetadataConfig());
        }
    }

internal static class SecurityCapabilityMapper
    {
    public static SecurityCapabilityCatalog Map(
        IReadOnlyList<SecurityControl> controls,
        SecurityMetadataConfig metadata)
        {
        var mappedControls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var capabilities = new List<SecurityCapabilityEntry>();

        foreach (var capability in metadata.Capabilities)
            {
            var controlDetails = new List<SecurityControlDetail>();
            foreach (var controlMeta in capability.Controls)
                {
                var matched = controls.FirstOrDefault(c => string.Equals(c.Control, controlMeta.Control, StringComparison.OrdinalIgnoreCase));
                if (matched == null)
                    {
                    continue;
                    }

                mappedControls.Add(matched.Control);
                controlDetails.Add(new SecurityControlDetail(
                    matched.Control,
                    matched.Location,
                    controlMeta.Purpose,
                    controlMeta.Risk));
                }

            capabilities.Add(new SecurityCapabilityEntry(
                capability.Name,
                capability.Purpose,
                capability.Risks.Distinct().OrderBy(x => x).ToList(),
                controlDetails.OrderBy(c => c.Control).ToList()));
            }

        var unmapped = controls
            .Where(c => !mappedControls.Contains(c.Control))
            .OrderBy(c => c.Control)
            .ToList();

        return new SecurityCapabilityCatalog(capabilities, unmapped);
        }
    }

internal static class SecurityCapabilityReportBuilder
    {
    public static SecurityCapabilityCatalog Build(
        IReadOnlyList<SecurityCapabilityDefinition> definitions,
        IReadOnlyList<SecurityControl> controls)
        {
        if (definitions.Count == 0)
            {
            return new SecurityCapabilityCatalog(Array.Empty<SecurityCapabilityEntry>(), controls.OrderBy(c => c.Control).ToList());
            }

        var mappedControls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var capabilities = new List<SecurityCapabilityEntry>();

        foreach (var capability in definitions)
            {
            var controlDetails = new List<SecurityControlDetail>();
            var risks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var controlMeta in capability.Controls)
                {
                if (!string.IsNullOrWhiteSpace(controlMeta.Risk))
                    {
                    risks.Add(controlMeta.Risk);
                    }

                var matched = controls.FirstOrDefault(c => string.Equals(c.Control, controlMeta.Control, StringComparison.OrdinalIgnoreCase));
                if (matched == null)
                    {
                    continue;
                    }

                mappedControls.Add(matched.Control);
                controlDetails.Add(new SecurityControlDetail(
                    matched.Control,
                    matched.Location,
                    controlMeta.Purpose,
                    controlMeta.Risk));
                }

            capabilities.Add(new SecurityCapabilityEntry(
                capability.Name,
                capability.Description,
                risks.OrderBy(x => x).ToList(),
                controlDetails.OrderBy(c => c.Control).ToList()));
            }

        var unmapped = controls
            .Where(c => !mappedControls.Contains(c.Control))
            .OrderBy(c => c.Control)
            .ToList();

        return new SecurityCapabilityCatalog(capabilities, unmapped);
        }
    }

internal sealed record DeploymentProfile(IReadOnlyList<string> Statements);

internal static class DeploymentProfileBuilder
    {
    public static DeploymentProfile Build(
        string aisRoot,
        IReadOnlyList<ControllerActionInfo> actions,
        DbProcedureIndex dbProcedures,
        JsEndpointIndex jsIndex)
        {
        var statements = new List<string>();

        var moduleFolders = new[]
            {
            "Controllers",
            "Views",
            "Models",
            "Services",
            "Filters",
            "Middleware",
            "Security",
            "Utilities",
            "Session",
            "Validation"
            };

        var existingModules = moduleFolders
            .Where(folder => Directory.Exists(Path.Combine(aisRoot, folder)))
            .OrderBy(folder => folder)
            .ToList();

        if (existingModules.Count > 0)
            {
            statements.Add($"Modular structure present ({string.Join(", ", existingModules)}).");
            }

        var apiRoutes = actions.Count(a => a.Route.StartsWith("/api", StringComparison.OrdinalIgnoreCase));
        var apiEndpoints = actions.SelectMany(a => a.ApiEndpoints).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (apiRoutes > 0 || apiEndpoints > 0)
            {
            statements.Add($"API-driven design signaled by {apiRoutes} API routes and {apiEndpoints} client-referenced endpoints.");
            }

        var dbProceduresCount = dbProcedures.MethodMap.SelectMany(m => m.Value.Procedures).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (dbProceduresCount > 0)
            {
            statements.Add($"Database encapsulation via DBConnection layer (procedures referenced: {dbProceduresCount}).");
            }

        var configurationFiles = Directory.EnumerateFiles(aisRoot, "appsettings*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .OrderBy(name => name)
            .ToList();
        if (configurationFiles.Count > 0)
            {
            statements.Add($"Environment-aware configuration using {string.Join(", ", configurationFiles)}.");
            }

        var hasWebConfig = File.Exists(Path.Combine(aisRoot, "web.config"));
        if (hasWebConfig)
            {
            statements.Add("Deployment configuration includes web.config for hosting settings.");
            }

        var jsFiles = jsIndex.EndpointsByFile.Count;
        if (jsFiles > 0)
            {
            statements.Add($"Client extensibility via {jsFiles} JavaScript modules with routed endpoints.");
            }

        if (statements.Count == 0)
            {
            statements.Add("No deployment or extensibility signals detected.");
            }

        return new DeploymentProfile(statements);
        }
    }

internal static class CapabilityScoreBuilder
    {
    public static IReadOnlyList<CapabilityScoreEntry> Build(
        IReadOnlyList<CapabilityCatalogEntry> capabilities,
        IReadOnlyList<RolePersonaCoverage> roles,
        IReadOnlyList<WorkflowCatalogEntry> workflows,
        MenuCatalog menuCatalog)
        {
        if (capabilities.Count == 0)
            {
            return Array.Empty<CapabilityScoreEntry>();
            }

        var results = new List<CapabilityScoreEntry>();
        var totalWorkflows = workflows.Count;
        foreach (var capability in capabilities)
            {
            var roleCount = roles.Count(role => role.Actions.Any(action => capability.Actions.Contains(action, StringComparer.OrdinalIgnoreCase)));
            var workflowCount = capability.Workflows.Count;
            var apiCount = capability.Apis.Count;
            var menuProminence = menuCatalog.Pages
                .Where(page => capability.Views.Any(view => PathMatcher.Matches(page.PagePath, view))
                    || capability.Routes.Any(route => PathMatcher.Matches(page.PagePath, route)))
                .Select(page => page.MenuId)
                .Distinct()
                .Count();
            var dbDepth = capability.DbProcedures.Count;

            var score = (roleCount * 3) + (workflowCount * 2) + apiCount + menuProminence + dbDepth;
            var priority = score >= 15 ? "High" : score >= 8 ? "Medium" : "Low";

            var evidence = new List<string>
                {
                $"{roleCount} roles mapped",
                $"{workflowCount} workflows supported (of {totalWorkflows})",
                $"{apiCount} APIs referenced",
                $"{menuProminence} menu touchpoints",
                $"{dbDepth} DB procedures"
                };

            results.Add(new CapabilityScoreEntry(capability.Name, score, priority, evidence));
            }

        return results.OrderByDescending(result => result.Score).ThenBy(result => result.Name).ToList();
        }
    }

internal static class MermaidDiagramBuilder
    {
    public static MermaidDiagramCatalog Build(
        MenuCatalog menuCatalog,
        IReadOnlyList<RolePersonaCoverage> roles,
        IReadOnlyList<CapabilityCatalogEntry> capabilities,
        IReadOnlyList<WorkflowCatalogEntry> workflows)
        {
        var workflowDiagrams = workflows.Select(BuildWorkflowDiagram).ToList();
        var roleCapabilityDiagram = BuildRoleCapabilityDiagram(roles, capabilities);
        var systemDiagram = BuildSystemStructureDiagram(menuCatalog, roles, capabilities, workflows);

        return new MermaidDiagramCatalog(workflowDiagrams, roleCapabilityDiagram, systemDiagram);
        }

    private static string BuildWorkflowDiagram(WorkflowCatalogEntry workflow)
        {
        var builder = new StringBuilder();
        builder.AppendLine("flowchart TD");

        var stageIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var stageIndex = 0;
        foreach (var stage in workflow.Stages)
            {
            var id = $"stage{stageIndex++}";
            stageIds[stage.Name] = id;
            builder.AppendLine($"{id}[\"{stage.Name}\"]");
            }

        foreach (var stage in workflow.Stages)
            {
            if (!stageIds.TryGetValue(stage.Name, out var fromId))
                {
                continue;
                }

            foreach (var transition in stage.Transitions)
                {
                if (!stageIds.TryGetValue(transition.ToStage, out var toId))
                    {
                    toId = $"stage{stageIndex++}";
                    stageIds[transition.ToStage] = toId;
                    builder.AppendLine($"{toId}[\"{transition.ToStage}\"]");
                    }

                builder.AppendLine($"{fromId} --> {toId}");
                }
            }

        return builder.ToString().TrimEnd();
        }

    private static string BuildRoleCapabilityDiagram(
        IReadOnlyList<RolePersonaCoverage> roles,
        IReadOnlyList<CapabilityCatalogEntry> capabilities)
        {
        var builder = new StringBuilder();
        builder.AppendLine("graph LR");

        var roleIds = roles.Select((role, index) => new { role, id = $"R{index}" }).ToList();
        var capIds = capabilities.Select((capability, index) => new { capability, id = $"C{index}" }).ToList();

        foreach (var entry in roleIds)
            {
            builder.AppendLine($"{entry.id}[\"{entry.role.Name}\"]");
            }

        foreach (var entry in capIds)
            {
            builder.AppendLine($"{entry.id}[\"{entry.capability.Name}\"]");
            }

        foreach (var roleEntry in roleIds)
            {
            foreach (var capEntry in capIds)
                {
                if (roleEntry.role.Actions.Any(action => capEntry.capability.Actions.Contains(action, StringComparer.OrdinalIgnoreCase)))
                    {
                    builder.AppendLine($"{roleEntry.id} --> {capEntry.id}");
                    }
                }
            }

        return builder.ToString().TrimEnd();
        }

    private static string BuildSystemStructureDiagram(
        MenuCatalog menuCatalog,
        IReadOnlyList<RolePersonaCoverage> roles,
        IReadOnlyList<CapabilityCatalogEntry> capabilities,
        IReadOnlyList<WorkflowCatalogEntry> workflows)
        {
        var builder = new StringBuilder();
        builder.AppendLine("graph LR");

        var menuCount = menuCatalog.Menus.Count;
        var pageCount = menuCatalog.Pages.Count;
        var roleCount = roles.Count;
        var capabilityCount = capabilities.Count;
        var workflowCount = workflows.Count;

        builder.AppendLine($"Menus[\"Menus ({menuCount})\"]");
        builder.AppendLine($"Pages[\"Pages ({pageCount})\"]");
        builder.AppendLine($"Roles[\"Roles ({roleCount})\"]");
        builder.AppendLine($"Capabilities[\"Capabilities ({capabilityCount})\"]");
        builder.AppendLine($"Workflows[\"Workflows ({workflowCount})\"]");

        builder.AppendLine("Menus --> Pages");
        builder.AppendLine("Pages --> Capabilities");
        builder.AppendLine("Roles --> Capabilities");
        builder.AppendLine("Capabilities --> Workflows");

        return builder.ToString().TrimEnd();
        }
    }

internal static class JsonConfigLoader
    {
    private static readonly JsonSerializerOptions Options = new()
        {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
        };

    public static T Load<T>(string path, T fallback) where T : class
        {
        if (!File.Exists(path))
            {
            return fallback;
            }

        var content = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(content))
            {
            return fallback;
            }

        var parsed = JsonSerializer.Deserialize<T>(content, Options);
        return parsed ?? fallback;
        }
    }

internal static class PageActionMatcher
    {
    public static bool Matches(string pagePath, ControllerActionInfo action)
        {
        if (string.IsNullOrWhiteSpace(pagePath))
            {
            return false;
            }

        var normalizedPage = PageIdIndexLoader.NormalizePath(pagePath).TrimStart('/');
        var normalizedRoute = action.Route.TrimStart('/');
        if (string.Equals(normalizedPage, normalizedRoute, StringComparison.OrdinalIgnoreCase))
            {
            return true;
            }

        if (!string.IsNullOrWhiteSpace(action.ViewPath))
            {
            var normalizedView = action.ViewPath.Replace("\\", "/").TrimStart('/');
            if (normalizedView.EndsWith(normalizedPage, StringComparison.OrdinalIgnoreCase))
                {
                return true;
                }

            if (!normalizedPage.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                var combined = $"{normalizedPage}.cshtml";
                if (normalizedView.EndsWith(combined, StringComparison.OrdinalIgnoreCase))
                    {
                    return true;
                    }
                }
            }

        return false;
        }
    }

internal static class PathMatcher
    {
    public static bool Matches(string target, string pattern)
        {
        if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(pattern))
            {
            return false;
            }

        if (pattern.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
            var prefix = pattern.TrimEnd('*');
            return target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

        return string.Equals(target, pattern, StringComparison.OrdinalIgnoreCase);
        }
    }

internal sealed record CatalogDocument(
    string RepoRoot,
    MenuCatalog MenuCatalog,
    IReadOnlyList<PageIdEntry> PageIndex,
    IReadOnlyList<ControllerActionInfo> Actions,
    IReadOnlyList<SecurityControl> SecurityControls,
    IReadOnlyList<RolePersonaCoverage> RoleCoverage,
    RoleDiscoveryCatalog RoleDiscovery,
    WorkflowCoverageResult WorkflowCoverage,
    IReadOnlyList<CapabilityCatalogEntry> CapabilityCatalog,
    IReadOnlyList<CapabilityScoreEntry> CapabilityScores,
    SecurityCapabilityCatalog SecurityCapabilityCatalog,
    DeploymentProfile DeploymentProfile,
    MermaidDiagramCatalog MermaidDiagrams);

internal static class MarkdownCatalogWriter
    {
    public static string Write(CatalogDocument document)
        {
        var builder = new StringBuilder();
        builder.AppendLine("# AIS System Catalog");
        builder.AppendLine();
        builder.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        builder.AppendLine();

        WriteSystemCatalogSummary(builder, document);
        WriteMenuCatalog(builder, document);
        WritePageDetails(builder, document);
        WriteRoles(builder, document);
        WriteInferredWorkflows(builder, document);
        WriteWorkflows(builder, document);
        WriteCapabilities(builder, document);
        WriteCapabilityScores(builder, document);
        WriteSecurityCapabilities(builder, document);
        WriteMermaidDiagrams(builder, document);
        WriteDeploymentProfile(builder, document);
        WriteSecurityControls(builder, document);

        return builder.ToString();
        }

    private static void WriteMenuCatalog(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Menu Catalog");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(document.MenuCatalog.FailureReason))
            {
            builder.AppendLine($"> {document.MenuCatalog.FailureReason}");
            builder.AppendLine();
            }

        if (document.MenuCatalog.Menus.Count == 0 || document.MenuCatalog.Pages.Count == 0)
            {
            builder.AppendLine("No menu data available.");
            builder.AppendLine();
            return;
            }

        var pagesByMenu = document.MenuCatalog.Pages
            .GroupBy(p => p.MenuId)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.PageOrder ?? int.MaxValue).ThenBy(p => p.PageName).ToList());

        foreach (var menu in document.MenuCatalog.Menus.OrderBy(m => m.MenuOrder ?? int.MaxValue).ThenBy(m => m.MenuName))
            {
            builder.AppendLine($"### {menu.MenuName}");
            if (menu.MenuOrder.HasValue)
                {
                builder.AppendLine($"- Order: {menu.MenuOrder.Value}");
                }
            if (!string.IsNullOrWhiteSpace(menu.MenuDescription))
                {
                builder.AppendLine($"> {menu.MenuDescription}");
                }

            builder.AppendLine();
            if (!pagesByMenu.TryGetValue(menu.MenuId, out var pages))
                {
                builder.AppendLine("No pages returned for this menu.");
                builder.AppendLine();
                continue;
                }

            foreach (var page in pages)
                {
                builder.AppendLine($"- **{page.PageName}** (Page ID: {page.PageId})");
                builder.AppendLine($"  - Path: `{page.PagePath}`");
                if (!string.IsNullOrWhiteSpace(page.PageKey))
                    {
                    builder.AppendLine($"  - Page Key: `{page.PageKey}`");
                    }
                if (page.PageOrder.HasValue)
                    {
                    builder.AppendLine($"  - Order: {page.PageOrder.Value}");
                    }
                if (!string.IsNullOrWhiteSpace(page.Status))
                    {
                    builder.AppendLine($"  - Status: {page.Status}");
                    }
                }

            builder.AppendLine();
            }
        }

    private static void WriteSystemCatalogSummary(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Technical System Catalog");
        builder.AppendLine();

        var controllers = document.Actions.Select(a => a.Controller).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var routes = document.Actions.Select(a => a.Route).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var views = document.Actions.Select(a => a.ViewPath).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var apis = document.Actions.SelectMany(a => a.ApiEndpoints).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var dbProcedures = document.Actions.SelectMany(a => a.DbProcedures).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        builder.AppendLine($"- Controllers: {controllers}");
        builder.AppendLine($"- Actions/routes: {document.Actions.Count} actions across {routes} routes");
        builder.AppendLine($"- Views: {views}");
        builder.AppendLine($"- Client APIs: {apis}");
        builder.AppendLine($"- DB procedures/packages: {dbProcedures}");
        builder.AppendLine($"- Menu pages: {document.MenuCatalog.Pages.Count}");
        builder.AppendLine($"- Roles discovered: {document.RoleDiscovery.Roles.Count}");
        builder.AppendLine($"- Capabilities cataloged: {document.CapabilityCatalog.Count}");
        builder.AppendLine($"- Workflows cataloged: {document.WorkflowCoverage.Catalog.Count}");
        builder.AppendLine();
        }

    private static void WritePageDetails(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Page Details");
        builder.AppendLine();
        builder.AppendLine("Global permission gates: SessionAuthorizationFilter, PageKeyPermissionFilter, PostModelValidationFilter, ObjectScopeAuthorizationFilter.");
        builder.AppendLine();

        foreach (var action in document.Actions.OrderBy(a => a.Controller).ThenBy(a => a.Action))
            {
            builder.AppendLine($"### {action.Controller}.{action.Action}");
            builder.AppendLine($"- Route: `{action.Route}`{FormatHttpMethod(action.HttpMethod)}");
            builder.AppendLine($"- Controller/Action: `{action.Controller}Controller.{action.Action}`");
            builder.AppendLine($"- View Path: `{(string.IsNullOrWhiteSpace(action.ViewPath) ? "(no view returned)" : action.ViewPath)}`");

            builder.AppendLine("- APIs Referenced:");
            WriteList(builder, action.ApiEndpoints);

            builder.AppendLine("- DB Touchpoints:");
            WriteList(builder, action.DbTouchpoints);

            builder.AppendLine("- DB Procedures/Packages:");
            WriteList(builder, action.DbProcedures);

            builder.AppendLine("- Permission Gates:");
            WriteList(builder, action.PermissionGates);

            builder.AppendLine();
            }
        }

    private static void WriteSecurityControls(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Security Controls Register");
        builder.AppendLine();
        foreach (var control in document.SecurityControls)
            {
            builder.AppendLine($"- **{control.Control}** → `{control.Location}`");
            }

        builder.AppendLine();
        }

    private static void WriteRoles(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## DB-derived Role Coverage");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(document.RoleDiscovery.FailureReason))
            {
            builder.AppendLine($"> {document.RoleDiscovery.FailureReason}");
            builder.AppendLine();
            }

        if (document.RoleCoverage.Count == 0)
            {
            builder.AppendLine("No role coverage discovered.");
            builder.AppendLine();
            return;
            }

        foreach (var role in document.RoleCoverage.OrderBy(r => r.Name))
            {
            builder.AppendLine($"### {role.Name}");
            if (!string.IsNullOrWhiteSpace(role.Persona))
                {
                builder.AppendLine($"- Persona: {role.Persona}");
                }
            if (!string.IsNullOrWhiteSpace(role.Description))
                {
                builder.AppendLine($"- Description: {role.Description}");
                }
            if (!string.IsNullOrWhiteSpace(role.Status))
                {
                builder.AppendLine($"- Status: {role.Status}");
                }

            builder.AppendLine($"- System Coverage: {role.Pages.Count} pages, {role.Actions.Count} actions, {role.ApiEndpoints.Count} APIs, {role.DbTouchpoints.Count} DB touchpoints.");
            builder.AppendLine("- Functional Areas:");
            WriteList(builder, role.FunctionalAreas);

            builder.AppendLine("- Accessible Pages:");
            if (role.Pages.Count == 0)
                {
                builder.AppendLine("  - (none mapped)");
                }
            else
                {
                var menuById = document.MenuCatalog.Menus.ToDictionary(m => m.MenuId);
                var menuByPageId = document.MenuCatalog.Pages
                    .GroupBy(p => p.PageId)
                    .ToDictionary(g => g.Key, g => g.First().MenuId);

                var pagesByMenu = role.Pages
                    .GroupBy(page =>
                        {
                        var resolvedMenuId = page.MenuId;
                        if (!resolvedMenuId.HasValue && menuByPageId.TryGetValue(page.PageId, out var menuId))
                            {
                            resolvedMenuId = menuId;
                            }

                        if (resolvedMenuId.HasValue && menuById.TryGetValue(resolvedMenuId.Value, out var menu))
                            {
                            return menu.MenuName;
                            }

                        return "Unmapped Pages";
                        })
                    .OrderBy(group => group.Key);

                foreach (var group in pagesByMenu)
                    {
                    builder.AppendLine($"  - Menu: {group.Key}");
                    foreach (var page in group.OrderBy(p => p.PageId))
                        {
                        builder.AppendLine($"    - {page.PageName} (Page ID: {page.PageId}, Path: `{page.PagePath}`)");
                        }
                    }
                }

            builder.AppendLine("- Actions:");
            WriteList(builder, role.Actions);

            builder.AppendLine("- APIs:");
            WriteList(builder, role.ApiEndpoints);

            builder.AppendLine("- DB Touchpoints:");
            WriteList(builder, role.DbTouchpoints);

            builder.AppendLine("- DB Procedures/Packages:");
            WriteList(builder, role.DbProcedures);

            builder.AppendLine();
            }
        }

    private static void WriteInferredWorkflows(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Inferred Workflow Narratives");
        builder.AppendLine();

        if (document.WorkflowCoverage.Inferences.Count == 0)
            {
            builder.AppendLine("No inferred workflow narratives were generated.");
            builder.AppendLine();
            return;
            }

        foreach (var entry in document.WorkflowCoverage.Inferences.OrderBy(e => e.Workflow.Name))
            {
            builder.AppendLine($"### {entry.Workflow.Name}");
            builder.AppendLine($"- Confidence: {entry.Confidence:F2}");
            if (entry.Evidence.Count > 0)
                {
                builder.AppendLine("- Evidence:");
                foreach (var item in entry.Evidence)
                    {
                    builder.AppendLine($"  - {item}");
                    }
                }
            builder.AppendLine();
            }
        }

    private static void WriteWorkflows(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Workflows & Lifecycles");
        builder.AppendLine();

        if (document.WorkflowCoverage.UsingInference)
            {
            builder.AppendLine($"> Workflow catalog generated from inferred lifecycle patterns (average confidence {document.WorkflowCoverage.AverageConfidence:F2}).");
            builder.AppendLine();
            }
        else if (!string.IsNullOrWhiteSpace(document.WorkflowCoverage.FallbackReason))
            {
            builder.AppendLine($"> {document.WorkflowCoverage.FallbackReason}");
            builder.AppendLine();
            }

        if (document.WorkflowCoverage.Catalog.Count == 0)
            {
            builder.AppendLine("No workflow catalog available.");
            builder.AppendLine();
            return;
            }

        foreach (var workflow in document.WorkflowCoverage.Catalog.OrderBy(w => w.Name))
            {
            builder.AppendLine($"### {workflow.Name}");
            if (!string.IsNullOrWhiteSpace(workflow.Description))
                {
                builder.AppendLine($"> {workflow.Description}");
                }

            builder.AppendLine();
            builder.AppendLine("Stages:");
            foreach (var stage in workflow.Stages.OrderBy(s => s.Order))
                {
                builder.AppendLine($"- {stage.Order}. {stage.Name}");
                foreach (var transition in stage.Transitions)
                    {
                    builder.AppendLine($"  - Transition to: {transition.ToStage}");
                    builder.AppendLine("    - Routes:");
                    WriteIndentedList(builder, transition.Routes, 6);
                    builder.AppendLine("    - APIs:");
                    WriteIndentedList(builder, transition.Apis, 6);
                    builder.AppendLine("    - DB Procedures:");
                    WriteIndentedList(builder, transition.DbProcedures, 6);
                    builder.AppendLine("    - Controllers/Actions:");
                    WriteIndentedList(builder, transition.Actions, 6);
                    }
                }

            builder.AppendLine();
            }
        }

    private static void WriteCapabilities(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Product Capabilities");
        builder.AppendLine();

        if (document.CapabilityCatalog.Count == 0)
            {
            builder.AppendLine("No capability metadata provided.");
            builder.AppendLine();
            return;
            }

        foreach (var capability in document.CapabilityCatalog.OrderBy(c => c.Name))
            {
            builder.AppendLine($"### {capability.Name}");
            if (!string.IsNullOrWhiteSpace(capability.Description))
                {
                builder.AppendLine($"> {capability.Description}");
                }

            builder.AppendLine("- Traceable Components:");
            builder.AppendLine("  - Routes:");
            WriteIndentedList(builder, capability.Routes, 4);
            builder.AppendLine("  - Controllers:");
            WriteIndentedList(builder, capability.Controllers, 4);
            builder.AppendLine("  - Actions:");
            WriteIndentedList(builder, capability.Actions, 4);
            builder.AppendLine("  - APIs:");
            WriteIndentedList(builder, capability.Apis, 4);
            builder.AppendLine("  - Views:");
            WriteIndentedList(builder, capability.Views, 4);
            builder.AppendLine("  - Workflows:");
            WriteIndentedList(builder, capability.Workflows, 4);
            builder.AppendLine("  - DB Procedures/Packages:");
            WriteIndentedList(builder, capability.DbProcedures, 4);

            builder.AppendLine();
            }
        }

    private static void WriteCapabilityScores(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Scored Capability Matrix");
        builder.AppendLine();

        if (document.CapabilityScores.Count == 0)
            {
            builder.AppendLine("No capability scoring data available.");
            builder.AppendLine();
            return;
            }

        foreach (var score in document.CapabilityScores)
            {
            builder.AppendLine($"### {score.Name}");
            builder.AppendLine($"- Score: {score.Score}");
            builder.AppendLine($"- Priority: {score.Priority}");
            builder.AppendLine("- Evidence:");
            foreach (var item in score.Evidence)
                {
                builder.AppendLine($"  - {item}");
                }
            builder.AppendLine();
            }
        }

    private static void WriteSecurityCapabilities(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Security & Compliance Capabilities");
        builder.AppendLine();

        if (document.SecurityCapabilityCatalog.Capabilities.Count == 0)
            {
            builder.AppendLine("No security capability metadata provided.");
            builder.AppendLine();
            return;
            }

        foreach (var capability in document.SecurityCapabilityCatalog.Capabilities.OrderBy(c => c.Name))
            {
            builder.AppendLine($"### {capability.Name}");
            if (!string.IsNullOrWhiteSpace(capability.Purpose))
                {
                builder.AppendLine($"- Purpose: {capability.Purpose}");
                }
            if (capability.Risks.Count > 0)
                {
                builder.AppendLine($"- Risks Addressed: {string.Join(", ", capability.Risks)}");
                }

            builder.AppendLine("- Enforcement Locations:");
            if (capability.Controls.Count == 0)
                {
                builder.AppendLine("  - (none mapped)");
                }
            else
                {
                foreach (var control in capability.Controls.OrderBy(c => c.Control))
                    {
                    var details = new List<string> { $"`{control.Location}`" };
                    if (!string.IsNullOrWhiteSpace(control.Purpose))
                        {
                        details.Add($"purpose: {control.Purpose}");
                        }
                    if (!string.IsNullOrWhiteSpace(control.Risk))
                        {
                        details.Add($"risk: {control.Risk}");
                        }
                    builder.AppendLine($"  - {control.Control} → {string.Join("; ", details)}");
                    }
                }

            builder.AppendLine();
            }

        if (document.SecurityCapabilityCatalog.UnmappedControls.Count > 0)
            {
            builder.AppendLine("### Unmapped Security Controls");
            foreach (var control in document.SecurityCapabilityCatalog.UnmappedControls.OrderBy(c => c.Control))
                {
                builder.AppendLine($"- {control.Control} → `{control.Location}`");
                }
            builder.AppendLine();
            }
        }

    private static void WriteMermaidDiagrams(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Mermaid Diagrams");
        builder.AppendLine();

        if (document.MermaidDiagrams.WorkflowDiagrams.Count > 0)
            {
            builder.AppendLine("### Workflow Diagrams");
            builder.AppendLine();
            foreach (var diagram in document.MermaidDiagrams.WorkflowDiagrams)
                {
                builder.AppendLine("```mermaid");
                builder.AppendLine(diagram);
                builder.AppendLine("```");
                builder.AppendLine();
                }
            }

        builder.AppendLine("### Role → Capability Mapping");
        builder.AppendLine();
        builder.AppendLine("```mermaid");
        builder.AppendLine(document.MermaidDiagrams.RoleCapabilityDiagram);
        builder.AppendLine("```");
        builder.AppendLine();

        builder.AppendLine("### System Structure Overview");
        builder.AppendLine();
        builder.AppendLine("```mermaid");
        builder.AppendLine(document.MermaidDiagrams.SystemStructureDiagram);
        builder.AppendLine("```");
        builder.AppendLine();
        }

    private static void WriteDeploymentProfile(StringBuilder builder, CatalogDocument document)
        {
        builder.AppendLine("## Deployment & Extensibility Profile");
        builder.AppendLine();
        foreach (var statement in document.DeploymentProfile.Statements)
            {
            builder.AppendLine($"- {statement}");
            }

        builder.AppendLine();
        }

    private static void WriteList(StringBuilder builder, IReadOnlyList<string> items)
        {
        if (items.Count == 0)
            {
            builder.AppendLine("  - (none detected)");
            return;
            }

        foreach (var item in items)
            {
            builder.AppendLine($"  - {item}");
            }
        }

    private static void WriteIndentedList(StringBuilder builder, IReadOnlyList<string> items, int spaces)
        {
        var padding = new string(' ', spaces);
        if (items.Count == 0)
            {
            builder.AppendLine($"{padding}- (none mapped)");
            return;
            }

        foreach (var item in items)
            {
            builder.AppendLine($"{padding}- {item}");
            }
        }

    private static string FormatHttpMethod(string? method)
        {
        if (string.IsNullOrWhiteSpace(method))
            {
            return string.Empty;
            }

        return $" ({method})";
        }
    }
