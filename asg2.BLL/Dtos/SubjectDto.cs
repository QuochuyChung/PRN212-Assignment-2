namespace asg2.BLL.Dtos
{
    /// <summary>Môn học (dùng cho bảng Admin/Subjects và dropdown của P3/P5).</summary>
    public class SubjectDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
