using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Assignments
{
    public class IndexModel : PageModel
    {
        private readonly IExamSessionService _sessions;
        private readonly IAccountService _accounts;
        private readonly ISlotService _slots;

        public IndexModel(
            IExamSessionService sessions,
            IAccountService accounts,
            ISlotService slots)
        {
            _sessions = sessions;
            _accounts = accounts;
            _slots = slots;
        }

        [BindProperty(SupportsGet = true)]
        public int? SessionId { get; set; }

        [BindProperty]
        public List<int> StudentIds { get; set; } = new();

        public IReadOnlyList<SessionDto> Sessions { get; private set; } = Array.Empty<SessionDto>();
        public SessionDto? SelectedSession { get; private set; }
        public IReadOnlyList<AccountDto> Students { get; private set; } = Array.Empty<AccountDto>();
        public IReadOnlyList<SlotDto> Slots { get; private set; } = Array.Empty<SlotDto>();
        public HashSet<int> AssignedStudentIds { get; private set; } = new();

        public async Task OnGetAsync()
        {
            await LoadPageAsync();
        }

        public async Task<IActionResult> OnPostAssignAsync()
        {
            if (!SessionId.HasValue || SessionId.Value <= 0)
            {
                ModelState.AddModelError(nameof(SessionId), "Vui lòng chọn phiên thi cần xếp lịch.");
            }

            if (!ModelState.IsValid)
            {
                await LoadPageAsync();
                return Page();
            }

            var result = await _slots.AssignStudentsAsync(SessionId!.Value, StudentIds);
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState);
                await LoadPageAsync();
                return Page();
            }

            TempData["Success"] = $"Đã xếp lịch cho {result.Value!.Count} sinh viên.";
            return RedirectToPage(new { sessionId = SessionId.Value });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int slotId, int sessionId)
        {
            var result = await _slots.RemoveStudentAsync(slotId);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Errors.Values.SelectMany(messages => messages).FirstOrDefault()
                    ?? "Không thể gỡ sinh viên khỏi phiên thi.";
            }
            else
            {
                TempData["Success"] = "Đã gỡ sinh viên khỏi phiên thi.";
            }

            return RedirectToPage(new { sessionId });
        }

        private async Task LoadPageAsync()
        {
            Sessions = await _sessions.GetAllAsync(status: "Scheduled");
            Students = await _accounts.GetStudentsAsync();

            if (!SessionId.HasValue || SessionId.Value <= 0)
            {
                return;
            }

            SelectedSession = await _sessions.GetByIdAsync(SessionId.Value);
            if (SelectedSession == null)
            {
                ModelState.AddModelError(nameof(SessionId), "Không tìm thấy phiên thi đã chọn.");
                return;
            }

            Slots = await _slots.GetSlotsOfSessionAsync(SessionId.Value);
            AssignedStudentIds = Slots.Select(slot => slot.StudentId).ToHashSet();
        }
    }
}
