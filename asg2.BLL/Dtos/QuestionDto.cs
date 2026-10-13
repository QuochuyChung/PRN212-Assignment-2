namespace asg2.BLL.Dtos
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int BloomLevel { get; set; }

        public string BloomLevelName => BloomLevel switch
        {
            1 => "Nhận biết",
            2 => "Thông hiểu",
            3 => "Vận dụng",
            4 => "Vận dụng cao",
            _ => "Chưa xác định"
        };

        public bool IsActive { get; set; }
    }
}
