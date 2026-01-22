using AIS.Controllers;
using AIS.Filters;
using AIS.Middleware;
using AIS.Security.Cryptography;
using AIS.Security.PasswordPolicy;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.RateLimiting;
using System;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using AIS.Utilities.Json;

namespace AIS
    {
    public class Startup
        {
        public Startup(IConfiguration configuration)
            {
            Configuration = configuration;
            }

        public IConfiguration Configuration { get; }

        private void ValidateRequiredConfigurationValues()
            {
            string[] requiredSettings = new[]
                {
                "ConnectionStrings:DBUserName",
                "ConnectionStrings:DBUserPassword",
                "ConnectionStrings:DBDataSource",
                "Security:SecretKey",
                "Security:CauKey",
                "Email:Host",
                "Email:Port"
                };

            foreach (var setting in requiredSettings)
                {
                var value = Configuration[setting];
                if (string.IsNullOrWhiteSpace(value))
                    {
                    throw new InvalidOperationException($"Missing required configuration setting: {setting}. Configure this value in appsettings for the secure intranet deployment.");
                    }
                }
            }

        public void ConfigureServices(IServiceCollection services)
            {
            ValidateRequiredConfigurationValues();
            var cookieSecurePolicy = CookieSecurePolicy.Always;

            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.Name = "IAS.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = cookieSecurePolicy;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
            });
            services.AddScoped<SessionHandler>();
            services.AddScoped<IDBConnection, DBConnection>();
            services.AddScoped<DBConnection>();
            services.AddScoped<FieldAuditReportPdfBuilder>();
            services.AddScoped<IMenuPagesReader, MenuPagesReader>();
            services.AddScoped<TopMenus>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddSingleton<IPageIdResolver, PageIdResolver>();
            services.AddScoped<PageKeyPermissionGuard>();
            services.AddScoped<PageKeyPermissionFilter>();
            services.AddScoped<SessionAuthorizationFilter>();
            services.AddScoped<PostModelValidationFilter>();
            services.AddScoped<IObjectScopeAuthorizer, ObjectScopeAuthorizer>();
            services.AddScoped<ObjectScopeAuthorizationFilter>();
            services.AddScoped<LoginAttemptTracker>();
            services.AddSingleton<PasswordPolicyValidator>();
            services.AddSingleton<SecurityTokenService>();
            services.AddSingleton<LoginViewResolver>();
            services.AddSingleton<PasswordChangeTokenService>();
            services.AddSingleton<PasswordChangeStateStore>();
            services.AddHostedService<PageIdRouteValidator>();
            var mvcBuilder = services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                    options.JsonSerializerOptions.Converters.Add(new NullableIntJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new NullableLongJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new NullableDecimalJsonConverter());
                });
            mvcBuilder.AddMvcOptions(options =>
            {
                options.Filters.Add<SessionAuthorizationFilter>();
                options.Filters.AddService<PageKeyPermissionFilter>();
                options.Filters.Add<PostModelValidationFilter>();
                options.Filters.AddService<ObjectScopeAuthorizationFilter>();
            });
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                {
                mvcBuilder.AddRazorRuntimeCompilation();
                }
            services.AddHttpContextAccessor();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "IAS.Auth";
                options.Cookie.SecurePolicy = cookieSecurePolicy;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Path = string.IsNullOrWhiteSpace(Configuration["BaseURL"]) ? "/" : Configuration["BaseURL"];
                options.LoginPath = (Configuration["BaseURL"] ?? string.Empty) + "/Login/Index";
                options.LogoutPath = (Configuration["BaseURL"] ?? string.Empty) + "/Login/Logout";
                options.AccessDeniedPath = (Configuration["BaseURL"] ?? string.Empty) + "/Login/Index";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = cookieSecurePolicy;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Path = string.IsNullOrWhiteSpace(Configuration["BaseURL"]) ? "/" : Configuration["BaseURL"];
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("LoginPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                    ResolveClientPartition(context, "login"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

                options.AddPolicy("ForgotPasswordPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                    ResolveClientPartition(context, "forgotpassword"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 8,
                        Window = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

                options.AddPolicy("ChangePasswordPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                    ResolveClientPartition(context, "changepassword"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 8,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

                options.AddPolicy("GeneralApiPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                    ResolveClientPartition(context, "general"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

                options.AddPolicy("FileTransferPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                    ResolveClientPartition(context, "file"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "text/plain";
                    await context.HttpContext.Response.WriteAsync("Too many attempts. Please try again later.", token);
                };
            });


            // Configure form options globally
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100 MB
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 104857600; // 100 MB
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 104857600; // 100 MB
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, LoginViewResolver loginViewResolver)
            {
            ValidateRequiredConfigurationValues();
            _ = loginViewResolver ?? throw new ArgumentNullException(nameof(loginViewResolver));

            var baseUrl = Configuration["BaseURL"] ?? string.Empty;
            app.UsePathBase(baseUrl);

            app.UseMiddleware<ApiExceptionHandlerMiddleware>();

            app.UseForwardedHeaders();

            app.UseHsts();
            logger.LogInformation("HSTS is enabled for this deployment.");
            logger.LogInformation("SMTP configuration loaded. Host={Host}; Port={Port}; From={From}.",
                Configuration["Email:Host"],
                Configuration.GetValue<int?>("Email:Port") ?? 587,
                Configuration["Email:From"]);

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
                {
                if (HttpMethods.IsTrace(context.Request.Method))
                    {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    return;
                    }

                await next();
                });

            app.UseWhen(
                context => !LoginRedirectHelper.IsApiRequest(context.Request),
                builder => builder.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}"));
            app.UseWhen(
                            context => context.Request.Path.StartsWithSegments("/Login", StringComparison.OrdinalIgnoreCase),
                            builder =>
                            {
                                builder.UseMiddleware<SecurityHeadersMiddleware>();
                            });


            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseRateLimiter();
            if (Configuration.GetValue<bool>("Vapt:EnableSoftSessionGate"))
                {
                app.UseMiddleware<SoftSessionGateMiddleware>();
                }
            app.UseMiddleware<AjaxErrorNotificationMiddleware>();
            app.UseMiddleware<SessionExceptionHandlingMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<SessionValidationMiddleware>();
            app.UseAuthorization();
            app.UseMiddleware<ForcePasswordChangeMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            }

        private static string ResolveClientPartition(HttpContext context, string policyName)
            {
            var ip = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            return $"{policyName}:{ip}";
            }

        }
    }
