using asg2.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace asg2.DAL.Security
{
    /// <summary>Băm / kiểm tra mật khẩu lưu trong Account.PasswordHash (PBKDF2 của ASP.NET Core).</summary>
    public static class AccountPasswordHasher
    {
        private static readonly PasswordHasher<Account> Hasher = new();

        public static string Hash(string password) =>
            Hasher.HashPassword(new Account(), password);

        public static bool Verify(string passwordHash, string password) =>
            Hasher.VerifyHashedPassword(new Account(), passwordHash, password) != PasswordVerificationResult.Failed;
    }
}
