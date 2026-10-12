namespace asg2.DAL.Entities
{
    /// <summary>
    /// Khung giờ thi của từng sinh viên trong một phiên thi.
    /// </summary>
    public class ExamSlot
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public ExamSession? Session { get; set; }

        public int StudentId { get; set; }
        public Account? Student { get; set; }

        public int OrderNo { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
