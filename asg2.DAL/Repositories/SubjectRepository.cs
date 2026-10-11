using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _db;

        public SubjectRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<Subject> Subjects => _db.Set<Subject>();

        public async Task<IReadOnlyList<Subject>> GetAllAsync() =>
            await Subjects.AsNoTracking().OrderBy(s => s.Code).ToListAsync();

        public Task<Subject?> GetByIdAsync(int id) =>
            Subjects.FirstOrDefaultAsync(s => s.Id == id);

        public Task<bool> CodeExistsAsync(string code, int? excludeId = null) =>
            Subjects.AnyAsync(s => s.Code == code && (excludeId == null || s.Id != excludeId));

        public Task<int> CountAsync() =>
            Subjects.AsNoTracking().CountAsync();

        public async Task AddAsync(Subject subject)
        {
            Subjects.Add(subject);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Subject subject)
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> TryDeleteAsync(Subject subject)
        {
            try
            {
                Subjects.Remove(subject);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _db.Entry(subject).State = EntityState.Unchanged;
                return false;
            }
        }

        public Task<bool> IsInUseAsync(int id)
        {
            // TODO(P3): đếm ExamSession tham chiếu SubjectId khi entity phiên thi đã tồn tại.
            // Service đã gọi TryDeleteAsync (FK) nên chỗ này chỉ cần chặn sớm với thông báo đẹp hơn.
            return Task.FromResult(false);
        }
    }
}
