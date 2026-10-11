namespace asg2.DAL.Entities
{
    /// <summary>Tài khoản đăng nhập (P1). Admin quản lý, Student xem lịch thi.</summary>
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public AccountRole Role { get; set; } = AccountRole.Student;
        public bool IsActive { get; set; } = true;
    }
}
