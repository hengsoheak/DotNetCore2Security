// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;

namespace Interop.Microsoft.Owin.TestApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IAppBuilder app)
        {
            app.UseErrorPage();

            var dataProtection = DataProtectionProvider.Create(new DirectoryInfo("..\\..\\artifacts"));
            var dataProtector = dataProtection.CreateProtector(
                "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", // full name of the ASP.NET Core type
                "Cookies", "v2");

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(dataProtector)),
                CookieName = ".AspNetCore.Cookies",
            });

            app.Run(async context =>
            {
                if (context.Request.Path.Equals("/echo"))
                {
                    await context.Response.WriteAsync(context.Authentication.User.Identity.Name ?? "No User");
                }
                else if (context.Request.Path.Equals("/create"))
                {
                    var user = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }, "Cookies");
                    context.Authentication.SignIn(user);
                    await context.Response.WriteAsync("Created");
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            });
        }
    }
}
