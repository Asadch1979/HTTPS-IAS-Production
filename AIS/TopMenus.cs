using AIS.Controllers;
using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS
    {

    public class TopMenus
        {
        private DBConnection dBConnection;
        private readonly SessionHandler sessionHandler;
        public IConfiguration _configuration;
        public ISession _session;
        public IHttpContextAccessor _httpCon;
        private readonly AIS.Security.Cryptography.SecurityTokenService _tokenService;

        public TopMenus(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, SessionHandler sessionHandler, AIS.Security.Cryptography.SecurityTokenService tokenService)
            {
            if (httpContextAccessor == null)
                throw new ArgumentNullException(nameof(httpContextAccessor));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (sessionHandler == null)
                throw new ArgumentNullException(nameof(sessionHandler));
            if (tokenService == null)
                throw new ArgumentNullException(nameof(tokenService));

            _session = httpContextAccessor.HttpContext.Session;
            _httpCon = httpContextAccessor;
            _configuration = configuration;
            this.sessionHandler = sessionHandler;
            _tokenService = tokenService;
            }
        private DBConnection CreateDbConnection()
            {
            if (_httpCon == null)
                throw new InvalidOperationException("HTTP context accessor has not been provided to the top menu helper.");
            if (_configuration == null)
                throw new InvalidOperationException("Configuration has not been provided to the top menu helper.");

            return DBConnection.CreateFromHttpContext(_httpCon, _configuration, sessionHandler, _tokenService);
            }

        public List<Object> GetTopMenus()
            {
            dBConnection = CreateDbConnection();

            List<object> menuList = new List<object>();
            if (sessionHandler.IsUserLoggedIn())
                {
                var menus = dBConnection.GetTopMenus();
                foreach (var item in menus)
                    {
                    menuList.Add(item);
                    }
                }
            return menuList;
            }

        public List<Object> GetTopMenusPages()
            {
            dBConnection = CreateDbConnection();

            List<object> menuList = new List<object>();
            List<object> submenuList = new List<object>();
            if (sessionHandler.IsUserLoggedIn())
                {
                var menus = dBConnection.GetTopMenuPages();
                var iidMenuId = menus
                    .Where(x => !string.IsNullOrWhiteSpace(x.Page_Path) && x.Page_Path.StartsWith("IID/", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Menu_Id)
                    .FirstOrDefault();

                var dashboardExists = menus.Any(x => string.Equals(x.Page_Path, "IID/MonitoringDashboard", StringComparison.OrdinalIgnoreCase));
                if (iidMenuId > 0 && !dashboardExists)
                    {
                    var maxOrder = menus.Where(x => x.Menu_Id == iidMenuId).Select(x => x.Page_Order).DefaultIfEmpty(0).Max();
                    menus.Add(new MenuPagesModel
                        {
                        Id = 0,
                        Menu_Id = iidMenuId,
                        PageId = 0,
                        Page_Name = "Monitoring Dashboard",
                        Page_Key = "IID_MONITORING_DASHBOARD",
                        Page_URL = "",
                        Page_Path = "IID/MonitoringDashboard",
                        Page_Order = maxOrder + 1,
                        Status = "ACTIVE",
                        Sub_Menu = "IID",
                        Sub_Menu_Id = "IID",
                        Sub_Menu_Name = "IID",
                        Hide_Menu = 0
                        });
                    }

                sessionHandler.CacheMenuPages(menus);
                foreach (var item in menus)
                    {
                    menuList.Add(item);

                    }

                }
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null
                || loggedInUser.UserEntityID.GetValueOrDefault() <= 0
                || string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                || loggedInUser.UserRoleID <= 0)
                {
                return new List<Object>();
                }
            AvatarNameDisplayModel av = new AvatarNameDisplayModel();
            av.Menu_Id = 1020304050;
            av.Id = 11223344;
            av.PPNO = loggedInUser.PPNumber;
            av.User_Entity_Name = loggedInUser.UserEntityName;
            av.User_Role_Name = loggedInUser.UserRoleName;
            av.Name = loggedInUser.Name;
            av.Sub_Menu = "";
            av.Sub_Menu_Id = "";
            av.Sub_Menu_Name = "";

            menuList.Add(av);
            return menuList;
            }

        }
    }
