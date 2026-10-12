namespace asg2.BLL.Dtos
{
    public class SlotDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string SessionTitle { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentUsername { get; set; } = string.Empty;
        public string StudentFullName { get; set; } = string.Empty;
        public int OrderNo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MainQuestionCount { get; set; }
    }
}
