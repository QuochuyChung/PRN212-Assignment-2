using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly IRoomService _rooms;

        public IndexModel(IRoomService rooms)
        {
            _rooms = rooms;
        }

        public string? Q { get; private set; }

        public IReadOnlyList<RoomDto> Rooms { get; private set; } = Array.Empty<RoomDto>();

        public async Task OnGetAsync(string? q)
        {
            Q = q;
            Rooms = await _rooms.SearchAsync(q);
        }
    }
}
