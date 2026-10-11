using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Accounts
{
    public class EditModel : PageModel
    {
        private readonly IAccountService _accounts;

        public EditModel(IAccountService accounts)
        {
            _accounts = accounts;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public AccountInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var account = await _accounts.GetByIdAsync(Id);
            if (account is null)
                return NotFound();

            Input = AccountInput.From(account);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var actorId = User.GetAccountId() ?? 0;
            var result = await _accounts.UpdateAsync(actorId, Id, Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                return Page();
            }

            TempData["Success"] = $"Đã cập nhật tài khoản {result.Value!.Username}.";
            return RedirectToPage("Index");
        }
    }
}
