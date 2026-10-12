using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class ExamSlotRepository : IExamSlotRepository
    {
        private readonly AppDbContext _db;

        public ExamSlotRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<ExamSlot> Slots => _db.Set<ExamSlot>();

        public async Task<IReadOnlyList<ExamSlot>> GetBySessionIdAsync(int sessionId) =>
            await Slots
                .AsNoTracking()
                .Include(s => s.Student)
                .Where(s => s.SessionId == sessionId)
                .OrderBy(s => s.OrderNo)
                .ToListAsync();

        public async Task<IReadOnlyList<ExamSlot>> GetByStudentIdAsync(int studentId) =>
            await Slots
                .AsNoTracking()
                .Include(s => s.Session)
                    .ThenInclude(session => session!.Subject)
                .Include(s => s.Session)
                    .ThenInclude(session => session!.Room)
                .Where(s => s.StudentId == studentId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

        public Task<ExamSlot?> GetByIdAsync(int id) =>
            Slots
                .Include(s => s.Session)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == id);

        public Task<int> CountAssignedInSessionAsync(int sessionId) =>
            Slots.AsNoTracking().CountAsync(s => s.SessionId == sessionId);

        public Task<bool> HasStudentConflictAsync(int studentId, DateTime start, DateTime end, int? excludeSlotId = null) =>
            Slots.AnyAsync(s =>
                s.StudentId == studentId &&
                (excludeSlotId == null || s.Id != excludeSlotId.Value) &&
                s.StartTime < end &&
                start < s.EndTime);

        public async Task AddAsync(ExamSlot slot)
        {
            Slots.Add(slot);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<ExamSlot> slots)
        {
            Slots.UpdateRange(slots);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<ExamSlot> slots)
        {
            Slots.RemoveRange(slots);
            await _db.SaveChangesAsync();
        }
    }
}
