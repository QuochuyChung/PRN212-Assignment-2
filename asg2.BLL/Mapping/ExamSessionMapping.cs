using asg2.BLL.Dtos;
using asg2.DAL.Entities;

namespace asg2.BLL.Mapping
{
    internal static class ExamSessionMapping
    {
        public static SessionDto ToDto(this ExamSession s)
        {
            var durationMinutes = (s.EndTime - s.StartTime).TotalMinutes;
            var capacity = s.SlotMinutes > 0 ? (int)Math.Floor(durationMinutes / s.SlotMinutes) : 0;

            return new SessionDto
            {
                Id = s.Id,
                Title = s.Title,
                SubjectId = s.SubjectId,
                SubjectCode = s.Subject?.Code ?? string.Empty,
                SubjectName = s.Subject?.Name ?? string.Empty,
                RoomId = s.RoomId,
                RoomName = s.Room?.Name ?? string.Empty,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotMinutes = s.SlotMinutes,
                MainQuestionCount = s.MainQuestionCount,
                MaxFollowUpCount = s.MaxFollowUpCount,
                Status = s.Status.ToString(),
                Capacity = capacity,
                AssignedCount = s.Slots?.Count ?? 0,
                CreatedAt = s.CreatedAt
            };
        }
    }
}
