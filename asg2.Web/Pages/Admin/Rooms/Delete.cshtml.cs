using asg2.BLL.Dtos;
using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Rooms
{
    public class DeleteModel : PageModel
    {
        private readonly IRoomService _rooms;

        public DeleteModel(IRoomService rooms)
        {
            _rooms = rooms;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public RoomDto? Room { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Room = await _rooms.GetByIdAsync(Id);
            return Room is null ? NotFound() : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _rooms.DeleteAsync(Id);

            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState);
                Room = await _rooms.GetByIdAsync(Id);
                return Room is null ? NotFound() : Page();
            }

            TempData["Success"] = "Đã xóa phòng thi.";
            return RedirectToPage("Index");
        }
    }
}
