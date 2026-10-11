using asg2.BLL.Services;
using asg2.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Rooms
{
    public class CreateModel : PageModel
    {
        private readonly IRoomService _rooms;

        public CreateModel(IRoomService rooms)
        {
            _rooms = rooms;
        }

        [BindProperty]
        public RoomInput Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _rooms.CreateAsync(Input.ToDto());
            if (!result.IsSuccess)
            {
                result.AddToModelState(ModelState, nameof(Input));
                return Page();
            }

            TempData["Success"] = $"Đã tạo phòng {result.Value!.Name}.";
            return RedirectToPage("Index");
        }
    }
}
