using asg2.BLL.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace asg2.Web.Hubs
{
    /// <summary>
    /// Hub realtime tại <c>/hubs/exam</c>. Server chỉ ĐẨY sự kiện xuống client
    /// (qua <c>SignalRNotifier</c> → <c>IHubContext&lt;ExamHub&gt;</c>); client không gọi hàm nào của hub.
    /// Chỉ tài khoản đăng nhập được kết nối; Admin tự vào group <see cref="AdminGroup"/> khi kết nối.
    /// </summary>
    [Authorize]
    public class ExamHub : Hub
    {
        public const string Path = "/hubs/exam";

        /// <summary>Group nhận các event quản trị (Subject*, Room*, Account*, ...).</summary>
        public const string AdminGroup = "admins";

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.IsInRole(AppRoles.Admin) == true)
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);

            await base.OnConnectedAsync();
        }
    }
}
