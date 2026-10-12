namespace asg2.BLL.Dtos
{
    /// <summary>Dữ liệu tạo / sửa phòng thi (bindable bởi form Razor Pages).</summary>
    public class RoomSaveDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Building { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
