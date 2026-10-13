namespace asg2.DAL.Entities
{
    /// <summary>
    /// Câu hỏi trong ngân hàng câu hỏi (P5).
    /// </summary>
    public class Question
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        /// <summary>Nội dung câu hỏi vấn đáp.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Mức độ tư duy theo thang Bloom (1: Nhận biết, 2: Thông hiểu, 3: Vận dụng, 4: Vận dụng cao).
        /// </summary>
        public int BloomLevel { get; set; } = 1;

        /// <summary>Trạng thái hoạt động (khóa thay vì xóa nếu đã từng dùng trong slot thi).</summary>
        public bool IsActive { get; set; } = true;

        public ICollection<SlotQuestion> SlotQuestions { get; set; } = new List<SlotQuestion>();
    }
}
