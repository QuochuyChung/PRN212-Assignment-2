namespace asg2.DAL.Entities
{
    /// <summary>
    /// Phiên thi vấn đáp (P3).
    /// </summary>
    public class ExamSession
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public int RoomId { get; set; }
        public Room? Room { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        /// <summary>Thời lượng thi cho mỗi thí sinh (phút).</summary>
        public int SlotMinutes { get; set; } = 15;

        /// <summary>Số câu hỏi chính bốc cho mỗi slot.</summary>
        public int MainQuestionCount { get; set; } = 2;

        /// <summary>Số câu hỏi phụ tối đa.</summary>
        public int MaxFollowUpCount { get; set; } = 1;

        public ExamSessionStatus Status { get; set; } = ExamSessionStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ExamSlot> Slots { get; set; } = new List<ExamSlot>();
    }
}
