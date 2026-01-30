using AIS.Controllers;
using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

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
                sessionHandler.CacheMenuPages(menus);
                foreach (var item in menus)
                    {
                    menuList.Add(item);

                    }

                }
            var loggedInUser = sessionHandler.GetUserOrThrow();
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
