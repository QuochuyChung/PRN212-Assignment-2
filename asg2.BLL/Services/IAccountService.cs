using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Danh sách tài khoản sinh viên, sắp theo username. P4 dùng để chọn sinh viên gán vào phiên thi
        /// (mặc định chỉ lấy tài khoản đang hoạt động).
        /// </summary>
        Task<IReadOnlyList<AccountDto>> GetStudentsAsync(string? search = null, bool includeInactive = false);

        Task<AccountDto?> GetByIdAsync(int id);

        Task<ServiceResult<AccountDto>> CreateAsync(AccountSaveDto dto);

        Task<ServiceResult<AccountDto>> UpdateAsync(int actorId, int id, AccountSaveDto dto);

        /// <summary>Xóa tài khoản sinh viên. Bị chặn nếu tài khoản đã được dùng (hãy khóa thay vì xóa).</summary>
        Task<ServiceResult> DeleteAsync(int actorId, int id);
    }
}
