using System.ComponentModel.DataAnnotations;
using asg2.BLL.Dtos;

namespace asg2.Web.Pages.Admin.Accounts
{
    /// <summary>Dữ liệu form tạo / sửa tài khoản. Validate đầy đủ nằm ở AccountService (BLL); đây chỉ là kiểm tra nhanh phía form.</summary>
    public class AccountInput
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public AccountSaveDto ToDto() => new()
        {
            Username = Username,
            FullName = FullName,
            Password = Password,
            IsActive = IsActive
        };

        public static AccountInput From(AccountDto dto) => new()
        {
            Username = dto.Username,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        };
    }
}
