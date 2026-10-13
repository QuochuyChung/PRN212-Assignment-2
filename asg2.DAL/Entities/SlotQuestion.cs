namespace asg2.DAL.Entities
{
    /// <summary>
    /// Câu hỏi được gán cho một slot thi vấn đáp (P5).
    /// </summary>
    public class SlotQuestion
    {
        public int SlotId { get; set; }
        public ExamSlot? Slot { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        /// <summary>Thứ tự câu hỏi trong slot (1, 2, ...).</summary>
        public int OrderNo { get; set; }
    }
}
