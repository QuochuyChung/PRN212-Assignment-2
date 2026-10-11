using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
        Task<bool> IsActiveAsync(int id);
        Task<IReadOnlyList<Account>> GetByRoleAsync(AccountRole role, string? search, bool includeInactive);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);

        /// <summary>Xóa tài khoản. Trả về false nếu bị ràng buộc khóa ngoại (đang được bảng khác tham chiếu).</summary>
        Task<bool> TryDeleteAsync(Account account);
    }
}
