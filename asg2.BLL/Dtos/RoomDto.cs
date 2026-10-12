namespace asg2.BLL.Dtos
{
    /// <summary>Phòng thi (dùng cho bảng Admin/Rooms và dropdown của P3).</summary>
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Building { get; set; }
        public bool IsActive { get; set; }
    }
}
