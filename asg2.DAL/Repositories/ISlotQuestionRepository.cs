using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface ISlotQuestionRepository
    {
        Task<IReadOnlyList<SlotQuestion>> GetBySlotIdAsync(int slotId);
        Task<IReadOnlyList<SlotQuestion>> GetBySessionIdAsync(int sessionId);
        Task AddRangeAsync(IEnumerable<SlotQuestion> slotQuestions);
        Task RemoveBySlotIdAsync(int slotId);
        Task RemoveBySessionIdAsync(int sessionId);
    }
}
