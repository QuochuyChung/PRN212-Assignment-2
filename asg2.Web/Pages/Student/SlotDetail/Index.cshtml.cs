using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Student.SlotDetail
{
    public class IndexModel : PageModel
    {
        private readonly ISlotService _slots;
        private readonly IQuestionPickerService _picker;

        public IndexModel(
            ISlotService slots,
            IQuestionPickerService picker)
        {
            _slots = slots;
            _picker = picker;
        }

        public SlotDto? Slot { get; private set; }
        public IReadOnlyList<SlotQuestionDto> Questions { get; private set; } = Array.Empty<SlotQuestionDto>();

        public async Task<IActionResult> OnGetAsync(int slotId)
        {
            var accountId = User.GetAccountId();
            if (!accountId.HasValue)
            {
                return RedirectToPage("/Account/Login");
            }

            Slot = await _slots.GetByIdAsync(slotId);
            if (Slot == null)
            {
                TempData["Error"] = "Không tìm thấy ca thi.";
                return RedirectToPage("/Student/MySlots/Index");
            }

            var role = User.GetRole();
            if (role != "Admin" && Slot.StudentId != accountId.Value)
            {
                return Forbid();
            }

            Questions = await _picker.GetQuestionsOfSlotAsync(slotId);
            return Page();
        }
    }
}
