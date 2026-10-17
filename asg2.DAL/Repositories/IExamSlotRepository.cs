using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface IExamSlotRepository
    {
        Task<IReadOnlyList<ExamSlot>> GetBySessionIdAsync(int sessionId);
        Task<IReadOnlyList<ExamSlot>> GetByStudentIdAsync(int studentId);
        Task<ExamSlot?> GetByIdAsync(int id);
        Task<int> CountAssignedInSessionAsync(int sessionId);
        Task<bool> HasStudentConflictAsync(int studentId, DateTime start, DateTime end, int? excludeSlotId = null);
        Task<IReadOnlyList<ExamSlot>> GetStudentConflictsAsync(
            IEnumerable<int> studentIds,
            DateTime start,
            DateTime end,
            int? excludeSessionId = null);
        Task AddAsync(ExamSlot slot);
        Task AddRangeAsync(IEnumerable<ExamSlot> slots);
        Task UpdateRangeAsync(IEnumerable<ExamSlot> slots);
        Task RemoveRangeAsync(IEnumerable<ExamSlot> slots);
    }
}
