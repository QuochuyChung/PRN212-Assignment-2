namespace asg2.BLL.Dtos
{
    /// <summary>Dữ liệu tạo / sửa môn học (bindable bởi form Razor Pages).</summary>
    public class SubjectSaveDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
