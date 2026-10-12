namespace asg2.BLL.Dtos
{
    public class SessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SlotMinutes { get; set; }
        public int MainQuestionCount { get; set; }
        public int MaxFollowUpCount { get; set; }
        public string Status { get; set; } = "Scheduled";
        public int Capacity { get; set; }
        public int AssignedCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
