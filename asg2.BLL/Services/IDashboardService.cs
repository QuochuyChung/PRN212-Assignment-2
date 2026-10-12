using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IDashboardService
    {
        /// <summary>
        /// Số liệu dashboard. PageModel chỉ gọi hàm này — không tự đếm bản ghi ở tầng Web.
        /// Không bao giờ bắn sự kiện realtime.
        /// </summary>
        Task<DashboardStatsDto> GetStatsAsync();
    }
}
