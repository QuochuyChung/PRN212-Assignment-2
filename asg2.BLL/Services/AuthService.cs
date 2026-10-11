using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.DAL.Repositories;
using asg2.DAL.Security;

namespace asg2.BLL.Services
{
    internal sealed class AuthService : IAuthService
    {
        private readonly IAccountRepository _accounts;

        public AuthService(IAccountRepository accounts)
        {
            _accounts = accounts;
        }

        public async Task<ServiceResult<AccountDto>> LoginAsync(string username, string password)
        {
            username = (username ?? string.Empty).Trim();
            if (username.Length == 0 || string.IsNullOrEmpty(password))
                return ServiceResult<AccountDto>.Failure(string.Empty, "Vui lòng nhập tên đăng nhập và mật khẩu.");

            var account = await _accounts.GetByUsernameAsync(username);

            // Cùng một thông báo cho "không có user" và "sai mật khẩu" để không lộ username nào tồn tại.
            if (account is null || !AccountPasswordHasher.Verify(account.PasswordHash, password))
                return ServiceResult<AccountDto>.Failure(string.Empty, "Sai tên đăng nhập hoặc mật khẩu.");

            if (!account.IsActive)
                return ServiceResult<AccountDto>.Failure(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");

            return ServiceResult<AccountDto>.Success(account.ToDto());
        }

        public Task<bool> IsAccountActiveAsync(int accountId) => _accounts.IsActiveAsync(accountId);
    }
}
