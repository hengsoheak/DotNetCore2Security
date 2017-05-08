// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Interop.Microsoft.AspNetCore.TestApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCookieAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.Run(async context =>
            {
                if (context.Request.Path.Equals("/echo"))
                {
                    await context.Response.WriteAsync(context.User.Identity.Name ?? "No User");
                }
                else if (context.Request.Path.Equals("/create"))
                {
                    var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }, CookieAuthenticationDefaults.AuthenticationScheme);
                    var user = new ClaimsPrincipal(identity);
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);
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
