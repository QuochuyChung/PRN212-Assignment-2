using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboard;

        public IndexModel(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        public DashboardStatsDto Stats { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _dashboard.GetStatsAsync();
        }

        /// <summary>JSON endpoint cho JS: tải lại số liệu mà không cần F5 (P3–P5 gọi lại khi có sự kiện realtime).</summary>
        public async Task<IActionResult> OnGetStats()
        {
            return new JsonResult(await _dashboard.GetStatsAsync());
        }
    }
}
