using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Rooms
{
    public class EditModel : PageModel
    {
        private readonly IRoomService _rooms;

        public EditModel(IRoomService rooms)
        {
            _rooms = rooms;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public RoomInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var room = await _rooms.GetByIdAsync(Id);
            if (room is null)
                return NotFound();

            Input = RoomInput.From(room);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _rooms.UpdateAsync(Id, Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                return Page();
            }

            TempData["Success"] = $"Đã cập nhật phòng {result.Value!.Name}.";
            return RedirectToPage("Index");
        }
    }
}
