﻿using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.Resources.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace ASI.Basecode.WebApp
{
    // Authorization configuration
    internal partial class StartupConfigurer
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly TokenProviderOptions _tokenProviderOptions;

        /// <summary>
        /// Configure authorization
        /// </summary>
        private void ConfigureAuthorization()
        {
            var token = Configuration.GetTokenAuthentication();
            var tokenProviderOptionsFactory = this._services.BuildServiceProvider().GetService<TokenProviderOptionsFactory>();
            var tokenValidationParametersFactory = this._services.BuildServiceProvider().GetService<TokenValidationParametersFactory>();
            var tokenValidationParameters = tokenValidationParametersFactory.Create();

            this._services.AddAuthentication(Const.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            })
            .AddCookie(Const.AuthenticationScheme, options =>
            {
                options.Cookie = new CookieBuilder()
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    Name = $"{this._environment.ApplicationName}_{token.CookieName}"
                };
                options.LoginPath = new PathString("/Account/Login");
                //options.AccessDeniedPath = new PathString("/ErrorPage/Forbidden.html");
                options.AccessDeniedPath = new PathString("/Error/Error403");
                options.ReturnUrlParameter = "ReturnUrl";
                options.TicketDataFormat = new CustomJwtDataFormat(SecurityAlgorithms.HmacSha256, _tokenValidationParameters, Configuration, tokenProviderOptionsFactory);
            });

            this._services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdminPolicy", policy =>
                    policy.RequireRole("superadmin"));

                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("administrator"));

                options.AddPolicy("SupportAgentPolicy", policy =>
                    policy.RequireRole("support agent"));

                options.AddPolicy("UserPolicy", policy =>
                    policy.RequireRole("user"));
                options.AddPolicy("SuperAdminAndAdminPolicy", policy =>
                    policy.RequireRole("superadmin", "administrator"));
                options.AddPolicy("AdminAndAgentPolicy", policy =>
                    policy.RequireRole("administrator", "support agent"));
                options.AddPolicy("AdminAgentUserPolicy", policy =>
                    policy.RequireRole("administrator", "support agent", "user"));
                options.AddPolicy("AllRoleTypePolicy", policy =>
                    policy.RequireRole("administrator", "support agent", "superadmin", "user"));
            });

            this._services.AddMvc();
        }
    }
}
