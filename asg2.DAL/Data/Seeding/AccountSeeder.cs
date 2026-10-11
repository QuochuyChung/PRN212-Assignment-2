using asg2.DAL.Entities;
using asg2.DAL.Security;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Data.Seeding
{
    /// <summary>
    /// Tài khoản mẫu để dev/test (KHÔNG dùng cho môi trường thật). Mật khẩu mẫu ghi trong README.md.
    /// </summary>
    internal sealed class AccountSeeder : ISeeder
    {
        private const string AdminPassword = "Admin@123";
        private const string StudentPassword = "Student@123";

        private static readonly string[] StudentNames =
        {
            "Nguyễn Văn An", "Trần Thị Bình", "Lê Hoàng Cường", "Phạm Minh Đức", "Võ Thị Hà",
            "Đặng Quốc Huy", "Bùi Thị Lan", "Hoàng Gia Minh", "Ngô Thanh Nam", "Đỗ Thị Phương"
        };

        public int Order => 10;

        public async Task SeedAsync(AppDbContext db)
        {
            var accounts = db.Set<Account>();
            var existing = (await accounts.Select(a => a.Username).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existing.Contains("admin"))
            {
                accounts.Add(new Account
                {
                    Username = "admin",
                    FullName = "Quản trị viên",
                    PasswordHash = AccountPasswordHasher.Hash(AdminPassword),
                    Role = AccountRole.Admin,
                    IsActive = true
                });
            }

            for (var i = 0; i < StudentNames.Length; i++)
            {
                var username = $"student{i + 1}";
                if (existing.Contains(username)) continue;

                accounts.Add(new Account
                {
                    Username = username,
                    FullName = StudentNames[i],
                    PasswordHash = AccountPasswordHasher.Hash(StudentPassword),
                    Role = AccountRole.Student,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
