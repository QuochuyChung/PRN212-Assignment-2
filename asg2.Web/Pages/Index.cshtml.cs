using asg2.BLL.Common;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages
{
    public class IndexModel : PageModel
    {
        /// <summary>Điều hướng theo trạng thái đăng nhập và role.</summary>
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Account/Login");

            return User.IsInRole(AppRoles.Admin)
                ? RedirectToPage("/Admin/Accounts/Index")
                : RedirectToPage("/Student/Index");
        }
    }
}
