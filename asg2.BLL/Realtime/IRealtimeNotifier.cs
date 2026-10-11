namespace asg2.BLL.Realtime
{
    /// <summary>
    /// Cổng đẩy sự kiện realtime xuống client. Interface nằm ở BLL (BLL không phụ thuộc SignalR);
    /// bản cài đặt thật (SignalRNotifier dùng IHubContext) do P2 viết ở tầng Web và đăng ký thay cho NullRealtimeNotifier.
    /// Service gọi notifier SAU KHI đã lưu DB thành công. Tên event lấy từ HubEvents.
    /// </summary>
    public interface IRealtimeNotifier
    {
        /// <summary>Gửi cho mọi client đang đăng nhập.</summary>
        Task BroadcastAsync(string eventName, object? payload);

        /// <summary>Gửi cho mọi kết nối của một tài khoản (theo Account.Id).</summary>
        Task SendToUserAsync(int accountId, string eventName, object? payload);

        /// <summary>Gửi cho nhóm "admins".</summary>
        Task SendToAdminsAsync(string eventName, object? payload);
    }
}
