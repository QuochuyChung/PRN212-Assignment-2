namespace asg2.BLL.Dtos
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        /// <summary>"Admin" hoặc "Student" (xem AppRoles).</summary>
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
