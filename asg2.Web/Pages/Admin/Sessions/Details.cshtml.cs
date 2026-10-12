using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Sessions
{
    public class DetailsModel : PageModel
    {
        private readonly IExamSessionService _sessions;
        private readonly ISlotService _slots;

        public DetailsModel(IExamSessionService sessions, ISlotService slots)
        {
            _sessions = sessions;
            _slots = slots;
        }

        public SessionDto? Session { get; private set; }
        public IReadOnlyList<SlotDto> Slots { get; private set; } = Array.Empty<SlotDto>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Session = await _sessions.GetByIdAsync(id);
            if (Session == null)
            {
                return NotFound();
            }

            Slots = await _slots.GetSlotsOfSessionAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var result = await _sessions.CancelAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = "Không thể hủy phiên thi.";
                return RedirectToPage("Details", new { id });
            }

            TempData["Success"] = $"Đã hủy phiên thi \"{result.Value!.Title}\".";
            return RedirectToPage("Details", new { id });
        }
    }
}
