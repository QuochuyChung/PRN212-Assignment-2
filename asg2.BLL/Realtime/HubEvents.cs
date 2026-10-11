namespace asg2.BLL.Realtime
{
    /// <summary>
    /// Tên event SignalR (server -> client), chốt theo hợp đồng nhóm. Client JS đăng ký đúng chuỗi này
    /// (ExamEvents.on("SessionCreated", ...)). Thêm event mới thì báo cả nhóm.
    /// </summary>
    public static class HubEvents
    {
        // P1 - Tài khoản (gửi nhóm admins); ForceLogout gửi riêng cho user bị khóa/xóa
        public const string AccountCreated = "AccountCreated";
        public const string AccountUpdated = "AccountUpdated";
        public const string AccountDeleted = "AccountDeleted";
        public const string ForceLogout = "ForceLogout";

        // P2 - Môn học, phòng thi (gửi nhóm admins)
        public const string SubjectCreated = "SubjectCreated";
        public const string SubjectUpdated = "SubjectUpdated";
        public const string SubjectDeleted = "SubjectDeleted";
        public const string RoomCreated = "RoomCreated";
        public const string RoomUpdated = "RoomUpdated";
        public const string RoomDeleted = "RoomDeleted";

        // P3 - Phiên thi (gửi mọi client)
        public const string SessionCreated = "SessionCreated";
        public const string SessionUpdated = "SessionUpdated";
        public const string SessionDeleted = "SessionDeleted";

        // P4 - Slot (gửi sinh viên đó + admins)
        public const string SlotAssigned = "SlotAssigned";
        public const string SlotRemoved = "SlotRemoved";

        // P5 - Câu hỏi (QuestionX gửi admins; SlotQuestionsUpdated gửi sinh viên đó + admins)
        public const string QuestionCreated = "QuestionCreated";
        public const string QuestionUpdated = "QuestionUpdated";
        public const string QuestionDeleted = "QuestionDeleted";
        public const string SlotQuestionsUpdated = "SlotQuestionsUpdated";
    }
}
