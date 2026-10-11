using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IAuthService
    {
        /// <summary>Đăng nhập bằng username + mật khẩu. Sai thông tin / tài khoản bị khóa trả Failure.</summary>
        Task<ServiceResult<AccountDto>> LoginAsync(string username, string password);

        /// <summary>Tài khoản còn tồn tại và đang hoạt động? Web dùng để vô hiệu hóa cookie của tài khoản đã bị khóa/xóa.</summary>
        Task<bool> IsAccountActiveAsync(int accountId);
    }
}
