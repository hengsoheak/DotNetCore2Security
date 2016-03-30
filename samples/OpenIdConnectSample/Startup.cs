using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OpenIdConnectSample
{
    public class Startup
    {
        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets()
                .Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
                sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            // Simple error page
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync(ex.ToString());
                    }
                    else
                    {
                        throw;
                    }
                }
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["oidc:clientid"],
                ClientSecret = Configuration["oidc:clientsecret"], // for code flow
                Authority = Configuration["oidc:authority"],
                /*
                // https://developers.google.com/identity/protocols/OpenIDConnect
                ClientId = Configuration["google:clientid"],
                ClientSecret = Configuration["google:clientsecret"], // for code flow
                Authority = Configuration["google:authority"],
                // Google only appears to work with OpenIdConnectResponseTypes.Code
                */
                ResponseType = OpenIdConnectResponseTypes.Code,
                GetClaimsFromUserInfoEndpoint = true
            });

            app.Run(async context =>
            {
                if (context.Request.Path.Equals("/signout"))
                {
                    await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync($"<html><body>Signed out {context.User.Identity.Name}<br>{Environment.NewLine}");
                    await context.Response.WriteAsync("<a href=\"/\">Sign In</a>");
                    await context.Response.WriteAsync($"</body></html>");
                    return;
                }

                if (context.Request.Path.Equals("/remotesignout"))
                {
                    await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = "/signedout" }); // Redirects
                    return;
                }

                if (context.Request.Path.Equals("/signedout"))
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync($"<html><body>Signed out<br>");
                    await context.Response.WriteAsync("<a href=\"/\">Sign In</a>");
                    await context.Response.WriteAsync($"</body></html>");
                    return;
                }

                if (!context.User.Identities.Any(identity => identity.IsAuthenticated))
                {
                    await context.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
                    return;
                }

                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync($"<html><body>Hello Authenticated User {context.User.Identity.Name}<br>{Environment.NewLine}");
                foreach (var claim in context.User.Claims)
                {
                    await context.Response.WriteAsync($"{claim.Type}: {claim.Value}<br>{Environment.NewLine}");
                }
                await context.Response.WriteAsync("<a href=\"/signout\">Sign Out</a><br>");
                await context.Response.WriteAsync("<a href=\"/remotesignout\">Remote Sign Out</a><br>");
                await context.Response.WriteAsync($"</body></html>");
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultHostingConfiguration(args)
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

