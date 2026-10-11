namespace asg2.BLL.Dtos
{
    /// <summary>Dữ liệu tạo / sửa tài khoản sinh viên.</summary>
    public class AccountSaveDto
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        /// <summary>Bắt buộc khi tạo; khi sửa để trống nghĩa là giữ mật khẩu cũ.</summary>
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
