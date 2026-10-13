using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class SlotQuestionRepository : ISlotQuestionRepository
    {
        private readonly AppDbContext _db;

        public SlotQuestionRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<SlotQuestion> SlotQuestions => _db.Set<SlotQuestion>();

        public async Task<IReadOnlyList<SlotQuestion>> GetBySlotIdAsync(int slotId) =>
            await SlotQuestions.AsNoTracking()
                .Include(sq => sq.Question)
                    .ThenInclude(q => q!.Subject)
                .Where(sq => sq.SlotId == slotId)
                .OrderBy(sq => sq.OrderNo)
                .ToListAsync();

        public async Task<IReadOnlyList<SlotQuestion>> GetBySessionIdAsync(int sessionId) =>
            await SlotQuestions.AsNoTracking()
                .Include(sq => sq.Question)
                .Where(sq => sq.Slot!.SessionId == sessionId)
                .OrderBy(sq => sq.Slot!.OrderNo)
                .ThenBy(sq => sq.OrderNo)
                .ToListAsync();

        public async Task AddRangeAsync(IEnumerable<SlotQuestion> slotQuestions)
        {
            SlotQuestions.AddRange(slotQuestions);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveBySlotIdAsync(int slotId)
        {
            var existing = await SlotQuestions.Where(sq => sq.SlotId == slotId).ToListAsync();
            if (existing.Count > 0)
            {
                SlotQuestions.RemoveRange(existing);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveBySessionIdAsync(int sessionId)
        {
            var existing = await SlotQuestions.Where(sq => sq.Slot!.SessionId == sessionId).ToListAsync();
            if (existing.Count > 0)
            {
                SlotQuestions.RemoveRange(existing);
                await _db.SaveChangesAsync();
            }
        }
    }
}
