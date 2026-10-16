using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Sessions
{
    public class CreateModel : PageModel
    {
        private readonly IExamSessionService _sessions;
        private readonly ISubjectService _subjects;
        private readonly IRoomService _rooms;

        public CreateModel(IExamSessionService sessions, ISubjectService subjects, IRoomService rooms)
        {
            _sessions = sessions;
            _subjects = subjects;
            _rooms = rooms;
        }

        [BindProperty]
        public SessionInput Input { get; set; } = new();

        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();
        public IReadOnlyList<RoomDto> Rooms { get; private set; } = Array.Empty<RoomDto>();

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            var result = await _sessions.CreateAsync(Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                await LoadDropdownsAsync();
                return Page();
            }

            TempData["Success"] = $"Đã tạo phiên thi \"{result.Value!.Title}\".";
            return RedirectToPage("Index");
        }

        private async Task LoadDropdownsAsync()
        {
            Subjects = await _subjects.GetAllAsync();
            Rooms = await _rooms.GetActiveAsync();
        }
    }
}
