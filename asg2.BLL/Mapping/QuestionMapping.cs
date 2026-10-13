using asg2.BLL.Dtos;
using asg2.DAL.Entities;

namespace asg2.BLL.Mapping
{
    internal static class QuestionMapping
    {
        public static QuestionDto ToDto(this Question q) => new()
        {
            Id = q.Id,
            SubjectId = q.SubjectId,
            SubjectCode = q.Subject?.Code ?? string.Empty,
            SubjectName = q.Subject?.Name ?? string.Empty,
            Content = q.Content,
            BloomLevel = q.BloomLevel,
            IsActive = q.IsActive
        };

        public static SlotQuestionDto ToDto(this SlotQuestion sq) => new()
        {
            SlotId = sq.SlotId,
            QuestionId = sq.QuestionId,
            OrderNo = sq.OrderNo,
            Content = sq.Question?.Content ?? string.Empty,
            BloomLevel = sq.Question?.BloomLevel ?? 1,
            SubjectCode = sq.Question?.Subject?.Code ?? string.Empty,
            SubjectName = sq.Question?.Subject?.Name ?? string.Empty
        };
    }
}
