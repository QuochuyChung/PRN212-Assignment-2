using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Sessions
{
    public class EditModel : PageModel
    {
        private readonly IExamSessionService _sessions;
        private readonly ISubjectService _subjects;
        private readonly IRoomService _rooms;

        public EditModel(IExamSessionService sessions, ISubjectService subjects, IRoomService rooms)
        {
            _sessions = sessions;
            _subjects = subjects;
            _rooms = rooms;
        }

        [FromRoute]
        public int Id { get; set; }

        [BindProperty]
        public SessionInput Input { get; set; } = new();

        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();
        public IReadOnlyList<RoomDto> Rooms { get; private set; } = Array.Empty<RoomDto>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;
            var session = await _sessions.GetByIdAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            Input = SessionInput.From(session);
            await LoadDropdownsAsync(session.RoomId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Id = id;
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(Input.RoomId);
                return Page();
            }

            var result = await _sessions.UpdateAsync(id, Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                await LoadDropdownsAsync(Input.RoomId);
                return Page();
            }

            TempData["Success"] = $"Đã cập nhật phiên thi \"{result.Value!.Title}\".";
            return RedirectToPage("Index");
        }

        private async Task LoadDropdownsAsync(int currentRoomId)
        {
            Subjects = await _subjects.GetAllAsync();
            var allRooms = await _rooms.GetAllAsync();
            // Bao gồm phòng hiện tại của phiên thi kể cả nếu phòng đó đã bị khóa
            Rooms = allRooms.Where(r => r.IsActive || r.Id == currentRoomId).ToList();
        }
    }
}
