using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _db;

        public RoomRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<Room> Rooms => _db.Set<Room>();

        public async Task<IReadOnlyList<Room>> GetAllAsync() =>
            await Rooms.AsNoTracking().OrderBy(r => r.Name).ToListAsync();

        public async Task<IReadOnlyList<Room>> GetActiveAsync() =>
            await Rooms.AsNoTracking().Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();

        public Task<Room?> GetByIdAsync(int id) =>
            Rooms.FirstOrDefaultAsync(r => r.Id == id);

        public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
            Rooms.AnyAsync(r => r.Name == name && (excludeId == null || r.Id != excludeId));

        public Task<int> CountAsync() =>
            Rooms.AsNoTracking().CountAsync();

        public Task<int> CountActiveAsync() =>
            Rooms.AsNoTracking().CountAsync(r => r.IsActive);

        public async Task AddAsync(Room room)
        {
            Rooms.Add(room);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> TryDeleteAsync(Room room)
        {
            try
            {
                Rooms.Remove(room);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _db.Entry(room).State = EntityState.Unchanged;
                return false;
            }
        }

        public Task<bool> IsInUseAsync(int id) =>
            _db.Set<ExamSession>().AnyAsync(s => s.RoomId == id);
    }
}
