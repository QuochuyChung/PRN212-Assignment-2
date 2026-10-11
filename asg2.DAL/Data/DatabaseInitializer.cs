using asg2.DAL.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace asg2.DAL.Data
{
    /// <summary>Áp migration rồi chạy các seeder. CHỈ BLL gọi (qua InitializeDatabaseAsync).</summary>
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();

            foreach (var seeder in scope.ServiceProvider.GetServices<ISeeder>().OrderBy(s => s.Order))
                await seeder.SeedAsync(db);
        }
    }
}
