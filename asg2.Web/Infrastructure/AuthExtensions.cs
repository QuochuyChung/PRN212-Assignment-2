using asg2.BLL.Common;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace asg2.Web.Infrastructure
{
    public static class AuthPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string StudentOnly = "StudentOnly";
    }

    public static class AuthExtensions
    {
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.Name = "asg2.auth";
                    options.Cookie.HttpOnly = true;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);

                    // Tài khoản bị khóa/xóa thì cookie cũ mất hiệu lực ở request kế tiếp (không phụ thuộc SignalR).
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var accountId = context.Principal?.GetAccountId();
                        var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();

                        if (accountId is null || !await auth.IsAccountActiveAsync(accountId.Value))
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        }
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin))
                .AddPolicy(AuthPolicies.StudentOnly, policy => policy.RequireRole(AppRoles.Student));

            return services;
        }
    }
}
