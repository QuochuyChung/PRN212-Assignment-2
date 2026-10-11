namespace asg2.BLL.Dtos
{
    /// <summary>
    /// Số liệu Dashboard. Cấu trúc "đóng băng" từ P2 để JS của Dashboard
    /// và các phase sau (P3–P5) tin được — thêm field mới thì báo cả nhóm.
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalSubjects { get; set; }
        public int TotalRooms { get; set; }
        public int ActiveRooms { get; set; }

        /// <summary>Tổng số tài khoản sinh viên (kể cả đã khóa).</summary>
        public int TotalStudents { get; set; }

        /// <summary>Số phiên thi — TODO(P3): đếm khi entity ExamSession đã tồn tại.</summary>
        public int TotalSessions { get; set; }

        /// <summary>Số câu hỏi — TODO(P5): đếm khi entity Question đã tồn tại.</summary>
        public int TotalQuestions { get; set; }
    }
}
