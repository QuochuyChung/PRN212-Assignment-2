using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Student.MySlots
{
    public class IndexModel : PageModel
    {
        private readonly ISlotService _slots;

        public IndexModel(ISlotService slots)
        {
            _slots = slots;
        }

        public IReadOnlyList<SlotDto> MySlots { get; private set; } = Array.Empty<SlotDto>();

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = User.GetAccountId();
            if (!studentId.HasValue)
            {
                return RedirectToPage("/Account/Login");
            }

            MySlots = await _slots.GetMySlotsAsync(studentId.Value);
            return Page();
        }
    }
}
