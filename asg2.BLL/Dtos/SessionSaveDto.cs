namespace asg2.BLL.Dtos
{
    public class SessionSaveDto
    {
        public string Title { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SlotMinutes { get; set; } = 15;
        public int MainQuestionCount { get; set; } = 2;
        public int MaxFollowUpCount { get; set; } = 1;
        public string Status { get; set; } = "Scheduled";
    }
}
