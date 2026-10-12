using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface IExamSessionRepository
    {
        Task<IReadOnlyList<ExamSession>> GetAllAsync(string? search = null, int? subjectId = null, ExamSessionStatus? status = null);
        Task<IReadOnlyList<ExamSession>> GetUpcomingAsync(int? subjectId = null);
        Task<ExamSession?> GetByIdAsync(int id);
        Task<bool> HasRoomConflictAsync(int roomId, DateTime start, DateTime end, int? excludeId = null);
        Task<int> CountAsync();
        Task<int> CountBySubjectAsync(int subjectId);
        Task<int> CountByRoomAsync(int roomId);
        Task AddAsync(ExamSession session);
        Task UpdateAsync(ExamSession session);
        Task<bool> TryDeleteAsync(ExamSession session);
    }
}
