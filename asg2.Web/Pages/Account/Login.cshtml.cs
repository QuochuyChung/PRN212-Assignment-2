using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using asg2.BLL.Common;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _auth;

        public LoginModel(IAuthService auth)
        {
            _auth = auth;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public string? Notice { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string? reason, string? returnUrl = null)
        {
            // Bị đá ra (khóa/xóa tài khoản) thì xóa cookie cũ rồi hiện thông báo, tránh tự chuyển ngược lại.
            if (!string.IsNullOrEmpty(reason))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                Notice = reason == "locked"
                    ? "Tài khoản của bạn đã bị khóa hoặc bị xóa. Vui lòng liên hệ quản trị viên."
                    : null;
            }
            else if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLanding(User.GetRole());
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
                return Page();

            var result = await _auth.LoginAsync(Input.Username, Input.Password);
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState);
                return Page();
            }

            var account = result.Value!;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Name, account.Username),
                new(AppClaimTypes.FullName, account.FullName),
                new(ClaimTypes.Role, account.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Cookie phiên (không persistent): đóng trình duyệt là hết đăng nhập.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToLanding(account.Role);
        }

        private IActionResult RedirectToLanding(string role) =>
            role == AppRoles.Admin
                ? RedirectToPage("/Admin/Accounts/Index")
                : RedirectToPage("/Student/Index");
    }
}
