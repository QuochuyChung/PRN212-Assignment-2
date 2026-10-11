using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Accounts
{
    public class DeleteModel : PageModel
    {
        private readonly IAccountService _accounts;

        public DeleteModel(IAccountService accounts)
        {
            _accounts = accounts;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public AccountDto? Account { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Account = await _accounts.GetByIdAsync(Id);
            return Account is null ? NotFound() : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var actorId = User.GetAccountId() ?? 0;
            var result = await _accounts.DeleteAsync(actorId, Id);

            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState);
                Account = await _accounts.GetByIdAsync(Id);
                return Account is null ? NotFound() : Page();
            }

            TempData["Success"] = "Đã xóa tài khoản.";
            return RedirectToPage("Index");
        }
    }
}
