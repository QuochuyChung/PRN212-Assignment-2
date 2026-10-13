using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _db;

        public QuestionRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<Question> Questions => _db.Set<Question>();

        public Task<Question?> GetByIdAsync(int id) =>
            Questions.Include(q => q.Subject).FirstOrDefaultAsync(q => q.Id == id);

        public async Task<IReadOnlyList<Question>> GetAllAsync(
            int? subjectId = null,
            int? bloomLevel = null,
            string? search = null,
            bool includeInactive = true)
        {
            var query = Questions.AsNoTracking().Include(q => q.Subject).AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(q => q.IsActive);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(q => q.SubjectId == subjectId.Value);
            }

            if (bloomLevel.HasValue)
            {
                query = query.Where(q => q.BloomLevel == bloomLevel.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(q => q.Content.Contains(term));
            }

            return await query
                .OrderBy(q => q.Subject!.Code)
                .ThenBy(q => q.BloomLevel)
                .ThenBy(q => q.Id)
                .ToListAsync();
        }

        public async Task<(IReadOnlyList<Question> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            int? subjectId = null,
            int? bloomLevel = null,
            string? search = null,
            bool includeInactive = true)
        {
            var query = Questions.AsNoTracking().Include(q => q.Subject).AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(q => q.IsActive);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(q => q.SubjectId == subjectId.Value);
            }

            if (bloomLevel.HasValue)
            {
                query = query.Where(q => q.BloomLevel == bloomLevel.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(q => q.Content.Contains(term));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(q => q.Subject!.Code)
                .ThenBy(q => q.BloomLevel)
                .ThenBy(q => q.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IReadOnlyDictionary<int, int>> GetBloomDistributionAsync(int? subjectId = null)
        {
            var query = Questions.AsNoTracking().AsQueryable();
            if (subjectId.HasValue)
            {
                query = query.Where(q => q.SubjectId == subjectId.Value);
            }

            var grouped = await query
                .GroupBy(q => q.BloomLevel)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            return grouped.ToDictionary(x => x.Level, x => x.Count);
        }

        public async Task<IReadOnlyList<Question>> GetActiveBySubjectIdAsync(int subjectId) =>
            await Questions.AsNoTracking()
                .Where(q => q.SubjectId == subjectId && q.IsActive)
                .OrderBy(q => q.BloomLevel)
                .ThenBy(q => q.Id)
                .ToListAsync();

        public Task<int> CountAsync(bool includeInactive = true) =>
            includeInactive
                ? Questions.AsNoTracking().CountAsync()
                : Questions.AsNoTracking().CountAsync(q => q.IsActive);

        public Task<bool> ExistsContentInSubjectAsync(int subjectId, string content, int? excludeId = null)
        {
            var trimmed = content.Trim();
            return Questions.AnyAsync(q =>
                q.SubjectId == subjectId &&
                q.Content.Trim() == trimmed &&
                (!excludeId.HasValue || q.Id != excludeId.Value));
        }

        public Task<bool> HasBeenUsedInSlotsAsync(int questionId) =>
            _db.Set<SlotQuestion>().AnyAsync(sq => sq.QuestionId == questionId);

        public async Task AddAsync(Question question)
        {
            Questions.Add(question);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Question question)
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> TryDeleteAsync(Question question)
        {
            try
            {
                Questions.Remove(question);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _db.Entry(question).State = EntityState.Unchanged;
                return false;
            }
        }
    }
}
