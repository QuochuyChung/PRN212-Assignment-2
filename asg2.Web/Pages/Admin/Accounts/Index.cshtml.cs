using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly IAccountService _accounts;

        public IndexModel(IAccountService accounts)
        {
            _accounts = accounts;
        }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public IReadOnlyList<AccountDto> Accounts { get; private set; } = Array.Empty<AccountDto>();

        public async Task OnGetAsync()
        {
            Accounts = await _accounts.GetStudentsAsync(Q, includeInactive: true);
        }
    }
}
