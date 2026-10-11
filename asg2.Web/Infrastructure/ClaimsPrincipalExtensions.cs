using System.Security.Claims;

namespace asg2.Web.Infrastructure
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Id tài khoản đang đăng nhập (claim NameIdentifier). SignalR cũng dùng claim này để gửi theo user.</summary>
        public static int? GetAccountId(this ClaimsPrincipal user) =>
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public static string GetFullName(this ClaimsPrincipal user) =>
            user.FindFirstValue(AppClaimTypes.FullName) ?? user.Identity?.Name ?? string.Empty;

        public static string GetRole(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }

    public static class AppClaimTypes
    {
        public const string FullName = "FullName";
    }
}
