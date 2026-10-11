namespace asg2.BLL.Realtime
{
    /// <summary>Bản mặc định không làm gì, để app chạy được khi chưa có SignalR. P2 đăng ký SignalRNotifier thay thế.</summary>
    internal sealed class NullRealtimeNotifier : IRealtimeNotifier
    {
        public Task BroadcastAsync(string eventName, object? payload) => Task.CompletedTask;

        public Task SendToUserAsync(int accountId, string eventName, object? payload) => Task.CompletedTask;

        public Task SendToAdminsAsync(string eventName, object? payload) => Task.CompletedTask;
    }
}
