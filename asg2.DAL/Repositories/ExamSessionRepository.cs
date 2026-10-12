using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class ExamSessionRepository : IExamSessionRepository
    {
        private readonly AppDbContext _db;

        public ExamSessionRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<ExamSession> Sessions => _db.Set<ExamSession>();

        public async Task<IReadOnlyList<ExamSession>> GetAllAsync(
            string? search = null,
            int? subjectId = null,
            ExamSessionStatus? status = null)
        {
            var query = Sessions
                .AsNoTracking()
                .Include(s => s.Subject)
                .Include(s => s.Room)
                .Include(s => s.Slots)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.Title.Contains(term) ||
                    (s.Subject != null && (s.Subject.Code.Contains(term) || s.Subject.Name.Contains(term))) ||
                    (s.Room != null && s.Room.Name.Contains(term)));
            }

            if (subjectId.HasValue && subjectId.Value > 0)
            {
                query = query.Where(s => s.SubjectId == subjectId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ExamSession>> GetUpcomingAsync(int? subjectId = null)
        {
            var now = DateTime.UtcNow;
            var query = Sessions
                .AsNoTracking()
                .Include(s => s.Subject)
                .Include(s => s.Room)
                .Include(s => s.Slots)
                .Where(s => s.Status == ExamSessionStatus.Scheduled && s.EndTime >= now)
                .AsQueryable();

            if (subjectId.HasValue && subjectId.Value > 0)
            {
                query = query.Where(s => s.SubjectId == subjectId.Value);
            }

            return await query
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public Task<ExamSession?> GetByIdAsync(int id) =>
            Sessions
                .Include(s => s.Subject)
                .Include(s => s.Room)
                .Include(s => s.Slots)
                    .ThenInclude(slot => slot.Student)
                .FirstOrDefaultAsync(s => s.Id == id);

        public Task<bool> HasRoomConflictAsync(int roomId, DateTime start, DateTime end, int? excludeId = null) =>
            Sessions.AnyAsync(s =>
                s.RoomId == roomId &&
                s.Status != ExamSessionStatus.Cancelled &&
                (excludeId == null || s.Id != excludeId.Value) &&
                s.StartTime < end &&
                start < s.EndTime);

        public Task<int> CountAsync() =>
            Sessions.AsNoTracking().CountAsync();

        public Task<int> CountBySubjectAsync(int subjectId) =>
            Sessions.AsNoTracking().CountAsync(s => s.SubjectId == subjectId);

        public Task<int> CountByRoomAsync(int roomId) =>
            Sessions.AsNoTracking().CountAsync(s => s.RoomId == roomId);

        public async Task AddAsync(ExamSession session)
        {
            Sessions.Add(session);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExamSession session)
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> TryDeleteAsync(ExamSession session)
        {
            try
            {
                Sessions.Remove(session);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _db.Entry(session).State = EntityState.Unchanged;
                return false;
            }
        }
    }
}
