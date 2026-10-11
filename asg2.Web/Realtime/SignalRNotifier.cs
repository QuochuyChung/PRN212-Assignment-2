using asg2.BLL.Realtime;
using asg2.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace asg2.Web.Realtime
{
    /// <summary>
    /// Bản cài đặt thật của <see cref="IRealtimeNotifier"/>, thay <c>NullRealtimeNotifier</c> khi đăng ký trong
    /// <c>Program.cs</c>. Service (BLL) chỉ biết interface; nơi này nối với hub SignalR ở tầng Web.
    /// Payload được protocol JSON của SignalR serialize dạng camelCase.
    /// </summary>
    internal sealed class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<ExamHub> _hub;

        public SignalRNotifier(IHubContext<ExamHub> hub)
        {
            _hub = hub;
        }

        public Task BroadcastAsync(string eventName, object? payload) =>
            _hub.Clients.All.SendAsync(eventName, payload);

        /// <summary>Gửi theo Account.Id: <c>Clients.User</c> dùng claim NameIdentifier (mặc định của SignalR).</summary>
        public Task SendToUserAsync(int accountId, string eventName, object? payload) =>
            _hub.Clients.User(accountId.ToString()).SendAsync(eventName, payload);

        public Task SendToAdminsAsync(string eventName, object? payload) =>
            _hub.Clients.Group(ExamHub.AdminGroup).SendAsync(eventName, payload);
    }
}
