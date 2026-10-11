using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Accounts
{
    public class CreateModel : PageModel
    {
        private readonly IAccountService _accounts;

        public CreateModel(IAccountService accounts)
        {
            _accounts = accounts;
        }

        [BindProperty]
        public AccountInput Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _accounts.CreateAsync(Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                return Page();
            }

            TempData["Success"] = $"Đã tạo tài khoản {result.Value!.Username}.";
            return RedirectToPage("Index");
        }
    }
}
