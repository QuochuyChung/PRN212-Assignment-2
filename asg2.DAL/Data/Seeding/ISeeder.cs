namespace asg2.DAL.Data.Seeding
{
    /// <summary>
    /// Mỗi người viết seeder cho entity của mình (class implement ISeeder, đặt trong Data/Seeding/).
    /// Tự được đăng ký (quét assembly) và chạy theo thứ tự Order khi InitializeDatabaseAsync.
    /// Seeder phải idempotent: chạy nhiều lần không sinh dữ liệu trùng.
    /// </summary>
    public interface ISeeder
    {
        /// <summary>Thứ tự chạy, nhỏ chạy trước (dữ liệu được tham chiếu phải có Order nhỏ hơn).</summary>
        int Order { get; }

        Task SeedAsync(AppDbContext db);
    }
}
