namespace asg2.DAL.Entities
{
    /// <summary>Môn học (P2). P3/P5 tham chiếu bằng SubjectId; quản trị viên CRUD ở Admin/Subjects.</summary>
    public class Subject
    {
        public int Id { get; set; }

        /// <summary>Mã môn, duy nhất, tối đa 20 ký tự (ví dụ: PRN222).</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Tên môn học, tối đa 100 ký tự.</summary>
        public string Name { get; set; } = string.Empty;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
