namespace asg2.DAL.Entities
{
    /// <summary>Phòng thi (P2). P3 chọn phòng khi tạo phiên thi; chỉ phòng IsActive mới dùng được.</summary>
    public class Room
    {
        public int Id { get; set; }

        /// <summary>Tên phòng, duy nhất, tối đa 50 ký tự (ví dụ: A101).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Tòa nhà (không bắt buộc), tối đa 50 ký tự.</summary>
        public string? Building { get; set; }

        /// <summary>Phòng còn dùng được không. Phòng ngừng dùng thì ẩn khỏi danh sách chọn của P3.</summary>
        public bool IsActive { get; set; } = true;
    }
}
