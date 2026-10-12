using asg2.BLL.Dtos;
using asg2.DAL.Entities;

namespace asg2.BLL.Mapping
{
    internal static class ExamSlotMapping
    {
        public static SlotDto ToDto(this ExamSlot s) => new()
        {
            Id = s.Id,
            SessionId = s.SessionId,
            SessionTitle = s.Session?.Title ?? string.Empty,
            SubjectCode = s.Session?.Subject?.Code ?? string.Empty,
            SubjectName = s.Session?.Subject?.Name ?? string.Empty,
            RoomName = s.Session?.Room?.Name ?? string.Empty,
            StudentId = s.StudentId,
            StudentUsername = s.Student?.Username ?? string.Empty,
            StudentFullName = s.Student?.FullName ?? string.Empty,
            OrderNo = s.OrderNo,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            MainQuestionCount = s.Session?.MainQuestionCount ?? 0
        };
    }
}
