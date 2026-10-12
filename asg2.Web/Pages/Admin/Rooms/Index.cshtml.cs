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
            var all = await _rooms.GetAllAsync();
            if (string.IsNullOrWhiteSpace(q))
            {
                Rooms = all;
                return;
            }

            var term = q.Trim();
            Rooms = all
                .Where(r => r.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (r.Building?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
    }
}
