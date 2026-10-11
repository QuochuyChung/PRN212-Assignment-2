using asg2.DAL.Data;
using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Repositories
{
    internal sealed class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;

        public AccountRepository(AppDbContext db)
        {
            _db = db;
        }

        private DbSet<Account> Accounts => _db.Set<Account>();

        public Task<Account?> GetByIdAsync(int id) =>
            Accounts.FirstOrDefaultAsync(a => a.Id == id);

        public Task<Account?> GetByUsernameAsync(string username) =>
            Accounts.FirstOrDefaultAsync(a => a.Username == username);

        public Task<bool> UsernameExistsAsync(string username, int? excludeId = null) =>
            Accounts.AnyAsync(a => a.Username == username && (excludeId == null || a.Id != excludeId));

        public Task<bool> IsActiveAsync(int id) =>
            Accounts.AsNoTracking().AnyAsync(a => a.Id == id && a.IsActive);

        public async Task<IReadOnlyList<Account>> GetByRoleAsync(AccountRole role, string? search, bool includeInactive)
        {
            var query = Accounts.AsNoTracking().Where(a => a.Role == role);

            if (!includeInactive)
                query = query.Where(a => a.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(a => a.Username.Contains(term) || a.FullName.Contains(term));
            }

            return await query.OrderBy(a => a.Username).ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            Accounts.Add(account);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> TryDeleteAsync(Account account)
        {
            try
            {
                Accounts.Remove(account);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                _db.Entry(account).State = EntityState.Unchanged;
                return false;
            }
        }
    }
}
