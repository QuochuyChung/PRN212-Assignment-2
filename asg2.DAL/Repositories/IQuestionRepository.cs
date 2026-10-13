using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(int id);
        Task<IReadOnlyList<Question>> GetAllAsync(int? subjectId = null, int? bloomLevel = null, string? search = null, bool includeInactive = true);
        Task<(IReadOnlyList<Question> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, int? subjectId = null, int? bloomLevel = null, string? search = null, bool includeInactive = true);
        Task<IReadOnlyDictionary<int, int>> GetBloomDistributionAsync(int? subjectId = null);
        Task<IReadOnlyList<Question>> GetActiveBySubjectIdAsync(int subjectId);
        Task<int> CountAsync(bool includeInactive = true);
        Task<bool> ExistsContentInSubjectAsync(int subjectId, string content, int? excludeId = null);
        Task<bool> HasBeenUsedInSlotsAsync(int questionId);
        Task AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task<bool> TryDeleteAsync(Question question);
    }
}
